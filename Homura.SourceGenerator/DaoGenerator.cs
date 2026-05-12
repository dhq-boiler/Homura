using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Homura.SourceGenerator
{
    [Generator(LanguageNames.CSharp)]
    public class DaoGenerator : IIncrementalGenerator
    {
        private const string GenerateDaoAttributeName = "Homura.ORM.Mapping.GenerateDaoAttribute";
        private const string ColumnAttributeName = "Homura.ORM.Mapping.ColumnAttribute";
        private const string PrimaryKeyAttributeName = "Homura.ORM.Mapping.PrimaryKeyAttribute";
        private const string SinceAttributeName = "Homura.ORM.Mapping.SinceAttribute";
        private const string UntilAttributeName = "Homura.ORM.Mapping.UntilAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var provider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    GenerateDaoAttributeName,
                    predicate: static (node, _) => node is ClassDeclarationSyntax,
                    transform: static (ctx, _) => GetEntityInfo(ctx))
                .Where(static x => x != null);

            context.RegisterSourceOutput(provider, static (spc, entityInfo) =>
            {
                var source = GenerateDaoSource(entityInfo);
                spc.AddSource($"{entityInfo.DaoName}.g.cs", source);
            });
        }

        private static EntityInfo GetEntityInfo(GeneratorAttributeSyntaxContext context)
        {
            var symbol = (INamedTypeSymbol)context.TargetSymbol;
            var attr = context.Attributes.First();

            string daoName = null;
            foreach (var namedArg in attr.NamedArguments)
            {
                if (namedArg.Key == "DaoName" && namedArg.Value.Value is string name)
                    daoName = name;
            }
            daoName = daoName ?? $"{symbol.Name}Dao";

            var entityNamespace = symbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : symbol.ContainingNamespace.ToDisplayString();

            var properties = new List<PropertyColumnInfo>();
            bool hasVersionDependentColumns = false;
            CollectColumnProperties(symbol, properties, ref hasVersionDependentColumns);

            return new EntityInfo
            {
                EntityName = symbol.Name,
                EntityFullName = symbol.ToDisplayString(),
                Namespace = entityNamespace,
                DaoName = daoName,
                Properties = properties,
                Accessibility = symbol.DeclaredAccessibility,
                HasVersionDependentColumns = hasVersionDependentColumns,
            };
        }

        private static void CollectColumnProperties(INamedTypeSymbol symbol, List<PropertyColumnInfo> properties, ref bool hasVersionDependentColumns)
        {
            // Collect class-level [PrimaryKey("Col1", "Col2")] column names
            var classPkColumnNames = new HashSet<string>();
            var type = symbol;
            while (type != null)
            {
                foreach (var classAttr in type.GetAttributes())
                {
                    if (classAttr.AttributeClass?.ToDisplayString() == PrimaryKeyAttributeName)
                    {
                        foreach (var arg in classAttr.ConstructorArguments)
                        {
                            if (arg.Kind == TypedConstantKind.Array)
                            {
                                foreach (var item in arg.Values)
                                {
                                    if (item.Value is string colName)
                                        classPkColumnNames.Add(colName);
                                }
                            }
                            else if (arg.Value is string colName)
                            {
                                classPkColumnNames.Add(colName);
                            }
                        }
                    }
                }
                type = type.BaseType;
            }

            // Walk inheritance chain to collect all [Column] properties
            type = symbol;
            while (type != null)
            {
                foreach (var member in type.GetMembers())
                {
                    if (!(member is IPropertySymbol prop)) continue;
                    if (prop.IsStatic || prop.IsIndexer) continue;
                    if (prop.SetMethod == null) continue;

                    var columnAttr = prop.GetAttributes()
                        .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == ColumnAttributeName);
                    if (columnAttr == null) continue;

                    // Avoid duplicates from overrides
                    if (properties.Any(p => p.PropertyName == prop.Name)) continue;

                    var columnName = columnAttr.ConstructorArguments.Length > 0
                        ? columnAttr.ConstructorArguments[0].Value as string
                        : prop.Name;

                    var order = columnAttr.ConstructorArguments.Length > 2
                        ? (int)columnAttr.ConstructorArguments[2].Value
                        : 0;

                    var safeGetMethod = GetSafeGetMethodName(prop.Type);

                    // Check for [PrimaryKey] on property
                    bool isPrimaryKey = prop.GetAttributes()
                        .Any(a => a.AttributeClass?.ToDisplayString() == PrimaryKeyAttributeName);

                    // Also check class-level PrimaryKey attribute
                    if (!isPrimaryKey && classPkColumnNames.Contains(columnName))
                        isPrimaryKey = true;

                    // Check for [Since] or [Until]
                    bool hasSinceOrUntil = prop.GetAttributes()
                        .Any(a =>
                        {
                            var name = a.AttributeClass?.ToDisplayString();
                            return name == SinceAttributeName || name == UntilAttributeName;
                        });

                    if (hasSinceOrUntil)
                        hasVersionDependentColumns = true;

                    properties.Add(new PropertyColumnInfo
                    {
                        PropertyName = prop.Name,
                        ColumnName = columnName,
                        Order = order,
                        SafeGetMethod = safeGetMethod,
                        PropertyTypeName = prop.Type.ToDisplayString(),
                        IsReactiveProperty = IsReactivePropertyType(prop.Type),
                        ReactiveInnerTypeName = GetReactiveInnerType(prop.Type),
                        IsPrimaryKey = isPrimaryKey,
                    });
                }

                type = type.BaseType;
            }

            properties.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        private static bool IsReactivePropertyType(ITypeSymbol type)
        {
            if (type is INamedTypeSymbol named && named.IsGenericType)
            {
                var name = named.OriginalDefinition.ToDisplayString();
                return name.StartsWith("Reactive.Bindings.ReactiveProperty")
                    || name.StartsWith("Reactive.Bindings.ReactivePropertySlim");
            }
            return false;
        }

        private static string GetReactiveInnerType(ITypeSymbol type)
        {
            if (type is INamedTypeSymbol named && named.IsGenericType && named.TypeArguments.Length > 0)
            {
                if (IsReactivePropertyType(type))
                    return named.TypeArguments[0].ToDisplayString();
            }
            return null;
        }

        private static string GetSafeGetMethodName(ITypeSymbol type)
        {
            // Handle ReactiveProperty<T> - use inner type
            if (type is INamedTypeSymbol named && named.IsGenericType)
            {
                var origName = named.OriginalDefinition.ToDisplayString();
                if (origName.StartsWith("Reactive.Bindings.ReactiveProperty")
                    || origName.StartsWith("Reactive.Bindings.ReactivePropertySlim"))
                {
                    if (named.TypeArguments.Length > 0)
                        return GetSafeGetMethodForSimpleType(named.TypeArguments[0]);
                }
            }

            return GetSafeGetMethodForSimpleType(type);
        }

        private static string GetSafeGetMethodForSimpleType(ITypeSymbol type)
        {
            var display = type.ToDisplayString();

            switch (display)
            {
                case "bool": return "SafeGetBoolean";
                case "bool?": return "SafeGetNullableBoolean";
                case "char": return "SafeGetChar";
                case "char?": return "SafeGetNullableChar";
                case "string": return "SafeGetString";
                case "int": return "SafeGetInt";
                case "int?": return "SafeGetNullableInt";
                case "long": return "SafeGetLong";
                case "long?": return "SafeNullableGetLong";
                case "float": return "SafeGetFloat";
                case "float?": return "SafeGetNullableFloat";
                case "double": return "SafeGetDouble";
                case "double?": return "SafeGetNullableDouble";
                case "System.DateTime": return "SafeGetDateTime";
                case "System.DateTime?": return "SafeGetNullableDateTime";
                case "System.Guid": return "SafeGetGuid";
                case "System.Guid?": return "SafeGetNullableGuid";
                case "System.Type": return "SafeGetType";
                case "object": return "SafeGetObject";
                default: return "SafeGetObject";
            }
        }

        private static bool IsTypeProperty(PropertyColumnInfo prop)
        {
            return prop.PropertyTypeName == "System.Type";
        }

        private static string GenerateGetValueExpression(PropertyColumnInfo prop, string entityVar)
        {
            string rawValue;
            if (prop.IsReactiveProperty)
                rawValue = $"{entityVar}.{prop.PropertyName}.Value";
            else
                rawValue = $"{entityVar}.{prop.PropertyName}";

            if (IsTypeProperty(prop))
                return $"(object){rawValue}?.AssemblyQualifiedName ?? DBNull.Value";

            return $"(object){rawValue} ?? DBNull.Value";
        }

        private static string GenerateDaoSource(EntityInfo info)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("#nullable disable");
            sb.AppendLine();
            sb.AppendLine("using Homura.Extensions;");
            sb.AppendLine("using Homura.ORM;");
            sb.AppendLine("using Homura.ORM.Setup;");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Data.Common;");
            sb.AppendLine();

            if (info.Namespace != null)
            {
                sb.AppendLine($"namespace {info.Namespace}");
                sb.AppendLine("{");
            }

            var accessibility = info.Accessibility == Accessibility.Public ? "public" : "internal";

            sb.AppendLine($"    {accessibility} partial class {info.DaoName} : Dao<{info.EntityFullName}>");
            sb.AppendLine("    {");

            // Constructors
            sb.AppendLine($"        public {info.DaoName}() : base() {{ }}");
            sb.AppendLine();
            sb.AppendLine($"        public {info.DaoName}(Type entityVersionType) : base(entityVersionType) {{ }}");
            sb.AppendLine();
            sb.AppendLine($"        public {info.DaoName}(DataVersionManager dataVersionManager) : base(dataVersionManager) {{ }}");
            sb.AppendLine();

            // ToEntity override
            sb.AppendLine($"        protected override {info.EntityFullName} ToEntity(IDataRecord reader, params IColumn[] columns)");
            sb.AppendLine("        {");
            sb.AppendLine($"            var entity = new {info.EntityFullName}();");

            foreach (var prop in info.Properties)
            {
                if (prop.IsReactiveProperty)
                {
                    sb.AppendLine($"            entity.{prop.PropertyName}.Value = reader.{prop.SafeGetMethod}(\"{prop.ColumnName}\", Table);");
                }
                else
                {
                    sb.AppendLine($"            entity.{prop.PropertyName} = reader.{prop.SafeGetMethod}(\"{prop.ColumnName}\", Table);");
                }
            }

            sb.AppendLine("            return entity;");
            sb.AppendLine("        }");

            // Generate Insert/Update/Select/Delete fast path only if no version-dependent columns
            if (!info.HasVersionDependentColumns)
            {
                GenerateInsertMethods(sb, info);
                GenerateUpdateMethods(sb, info);
                GenerateSelectMethods(sb, info);
                GenerateFindByMethods(sb, info);
                GenerateDeleteMethods(sb, info);
                GeneratePrimaryKeyMethods(sb, info);
            }

            sb.AppendLine("    }");

            if (info.Namespace != null)
                sb.AppendLine("}");

            return sb.ToString();
        }

        private static void GenerateInsertMethods(StringBuilder sb, EntityInfo info)
        {
            var allColumns = info.Properties;
            var columnNames = string.Join(", ", allColumns.Select(p => p.ColumnName));
            var paramNames = string.Join(", ", allColumns.Select(p => $"@{p.ColumnName.ToLower()}"));

            // SQL template constant
            sb.AppendLine();
            sb.AppendLine($"        private const string _insertSqlTemplate = \"INSERT INTO {{0}} ({columnNames}) VALUES ({paramNames})\";");

            // TryBuildInsertCommand
            sb.AppendLine();
            sb.AppendLine($"        protected override bool TryBuildInsertCommand(DbCommand command, {info.EntityFullName} entity, string tableName)");
            sb.AppendLine("        {");
            sb.AppendLine("            command.CommandText = string.Format(_insertSqlTemplate, tableName);");

            for (int i = 0; i < allColumns.Count; i++)
            {
                var prop = allColumns[i];
                sb.AppendLine($"            var p{i} = command.CreateParameter();");
                sb.AppendLine($"            p{i}.ParameterName = \"@{prop.ColumnName.ToLower()}\";");
                sb.AppendLine($"            p{i}.Value = {GenerateGetValueExpression(prop, "entity")};");
                sb.AppendLine($"            command.Parameters.Add(p{i});");
            }

            sb.AppendLine("            return true;");
            sb.AppendLine("        }");

            // TryUpdateInsertParameters
            sb.AppendLine();
            sb.AppendLine($"        protected override bool TryUpdateInsertParameters(DbCommand command, {info.EntityFullName} entity)");
            sb.AppendLine("        {");

            for (int i = 0; i < allColumns.Count; i++)
            {
                var prop = allColumns[i];
                sb.AppendLine($"            command.Parameters[{i}].Value = {GenerateGetValueExpression(prop, "entity")};");
            }

            sb.AppendLine("            return true;");
            sb.AppendLine("        }");
        }

        private static void GeneratePrimaryKeyMethods(StringBuilder sb, EntityInfo info)
        {
            var pkColumns = info.Properties.Where(p => p.IsPrimaryKey).ToList();
            if (pkColumns.Count == 0) return;

            var allColumns = info.Properties;
            var columnList = string.Join(", ", allColumns.Select(p => p.ColumnName));
            var whereClause = string.Join(" AND ", pkColumns.Select(p => $"{p.ColumnName} = @{p.ColumnName.ToLowerInvariant()}"));

            // SQL template constants
            sb.AppendLine();
            sb.AppendLine($"        private const string _findByPkSqlTemplate = \"SELECT {columnList} FROM {{0}} WHERE {whereClause}\";");
            sb.AppendLine();
            sb.AppendLine($"        private const string _deleteByPkSqlTemplate = \"DELETE FROM {{0}} WHERE {whereClause}\";");

            // Build C# parameter list: e.g. "System.Guid id, int otherKey"
            var csharpParams = string.Join(", ", pkColumns.Select(p => $"{PkParamType(p)} {PkParamName(p)}"));

            GenerateFindByPkBody(sb, info, pkColumns, csharpParams);
            GenerateDeleteByPkBody(sb, info, pkColumns, csharpParams);
        }

        private static string PkParamType(PropertyColumnInfo prop)
            => prop.IsReactiveProperty ? prop.ReactiveInnerTypeName : prop.PropertyTypeName;

        private static string PkParamName(PropertyColumnInfo prop)
        {
            // camelCase: lower-case first char
            var n = prop.PropertyName;
            if (string.IsNullOrEmpty(n)) return n;
            var lower = char.ToLowerInvariant(n[0]) + (n.Length > 1 ? n.Substring(1) : "");
            return IsCSharpKeyword(lower) ? "@" + lower : lower;
        }

        private static bool IsCSharpKeyword(string s)
        {
            switch (s)
            {
                case "abstract": case "as": case "base": case "bool": case "break":
                case "byte": case "case": case "catch": case "char": case "checked":
                case "class": case "const": case "continue": case "decimal": case "default":
                case "delegate": case "do": case "double": case "else": case "enum":
                case "event": case "explicit": case "extern": case "false": case "finally":
                case "fixed": case "float": case "for": case "foreach": case "goto":
                case "if": case "implicit": case "in": case "int": case "interface":
                case "internal": case "is": case "lock": case "long": case "namespace":
                case "new": case "null": case "object": case "operator": case "out":
                case "override": case "params": case "private": case "protected": case "public":
                case "readonly": case "ref": case "return": case "sbyte": case "sealed":
                case "short": case "sizeof": case "stackalloc": case "static": case "string":
                case "struct": case "switch": case "this": case "throw": case "true":
                case "try": case "typeof": case "uint": case "ulong": case "unchecked":
                case "unsafe": case "ushort": case "using": case "virtual": case "void":
                case "volatile": case "while":
                    return true;
                default: return false;
            }
        }

        private static string PkBindValueExpression(PropertyColumnInfo prop, string paramName)
        {
            var t = PkParamType(prop);
            if (t == "System.Type") return $"(object){paramName}?.AssemblyQualifiedName ?? DBNull.Value";
            return $"(object){paramName} ?? DBNull.Value";
        }

        private static void GenerateFindByPkBody(StringBuilder sb, EntityInfo info, List<PropertyColumnInfo> pkColumns, string csharpParams)
        {
            sb.AppendLine();
            sb.AppendLine($"        public async System.Threading.Tasks.Task<{info.EntityFullName}> FindByPrimaryKeyAsync({csharpParams}, System.Data.Common.DbConnection conn = null, System.TimeSpan? timeout = null)");
            sb.AppendLine("        {");
            sb.AppendLine("            timeout ??= System.TimeSpan.FromMinutes(5);");
            sb.AppendLine("            var beginTime = System.DateTime.Now;");
            sb.AppendLine("            var isTransaction = conn != null;");
            sb.AppendLine("            System.Data.Common.DbConnection localConn = conn;");
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine("                if (!isTransaction) localConn = await GetConnectionAsync().ConfigureAwait(false);");
            sb.AppendLine("                while ((System.DateTime.Now - beginTime) <= timeout)");
            sb.AppendLine("                {");
            sb.AppendLine("                    try");
            sb.AppendLine("                    {");
            sb.AppendLine("                        await using var command = localConn.CreateCommand();");
            sb.AppendLine("                        command.CommandText = string.Format(_findByPkSqlTemplate, TableName);");
            sb.AppendLine("                        command.CommandType = System.Data.CommandType.Text;");

            for (int i = 0; i < pkColumns.Count; i++)
            {
                var pk = pkColumns[i];
                var paramName = PkParamName(pk);
                sb.AppendLine($"                        var p{i} = command.CreateParameter();");
                sb.AppendLine($"                        p{i}.ParameterName = \"@{pk.ColumnName.ToLowerInvariant()}\";");
                sb.AppendLine($"                        p{i}.Value = {PkBindValueExpression(pk, paramName)};");
                sb.AppendLine($"                        command.Parameters.Add(p{i});");
            }

            sb.AppendLine("                        if (s_logger.IsDebugEnabled) s_logger.Debug(command.CommandText);");
            sb.AppendLine("                        await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);");
            sb.AppendLine("                        if (!await reader.ReadAsync().ConfigureAwait(false)) return null;");
            sb.AppendLine("                        return ToEntityFast(reader, null);");
            sb.AppendLine("                    }");
            sb.AppendLine("                    catch (System.Exception ex)");
            sb.AppendLine("                    {");
            sb.AppendLine("                        if (ex.Message.Contains(\"database is lock\")) { s_logger.Warn(\"database is lock\"); continue; }");
            sb.AppendLine("                        s_logger.Error(ex);");
            sb.AppendLine("                        throw;");
            sb.AppendLine("                    }");
            sb.AppendLine("                }");
            sb.AppendLine("                throw new System.TimeoutException();");
            sb.AppendLine("            }");
            sb.AppendLine("            finally");
            sb.AppendLine("            {");
            sb.AppendLine("                if (!isTransaction && localConn != null) await localConn.DisposeAsync().ConfigureAwait(false);");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
        }

        private static void GenerateDeleteByPkBody(StringBuilder sb, EntityInfo info, List<PropertyColumnInfo> pkColumns, string csharpParams)
        {
            sb.AppendLine();
            sb.AppendLine($"        public async System.Threading.Tasks.Task<int> DeleteByPrimaryKeyAsync({csharpParams}, System.Data.Common.DbConnection conn = null, System.TimeSpan? timeout = null)");
            sb.AppendLine("        {");
            sb.AppendLine("            timeout ??= System.TimeSpan.FromMinutes(5);");
            sb.AppendLine("            var beginTime = System.DateTime.Now;");
            sb.AppendLine("            var isTransaction = conn != null;");
            sb.AppendLine("            System.Data.Common.DbConnection localConn = conn;");
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine("                if (!isTransaction) localConn = await GetConnectionAsync().ConfigureAwait(false);");
            sb.AppendLine("                while ((System.DateTime.Now - beginTime) <= timeout)");
            sb.AppendLine("                {");
            sb.AppendLine("                    try");
            sb.AppendLine("                    {");
            sb.AppendLine("                        await using var command = localConn.CreateCommand();");
            sb.AppendLine("                        command.CommandText = string.Format(_deleteByPkSqlTemplate, TableName);");
            sb.AppendLine("                        command.CommandType = System.Data.CommandType.Text;");

            for (int i = 0; i < pkColumns.Count; i++)
            {
                var pk = pkColumns[i];
                var paramName = PkParamName(pk);
                sb.AppendLine($"                        var p{i} = command.CreateParameter();");
                sb.AppendLine($"                        p{i}.ParameterName = \"@{pk.ColumnName.ToLowerInvariant()}\";");
                sb.AppendLine($"                        p{i}.Value = {PkBindValueExpression(pk, paramName)};");
                sb.AppendLine($"                        command.Parameters.Add(p{i});");
            }

            sb.AppendLine("                        if (s_logger.IsDebugEnabled) s_logger.Debug(command.CommandText);");
            sb.AppendLine("                        return await command.ExecuteNonQueryAsync().ConfigureAwait(false);");
            sb.AppendLine("                    }");
            sb.AppendLine("                    catch (System.Exception ex)");
            sb.AppendLine("                    {");
            sb.AppendLine("                        if (ex.Message.Contains(\"database is lock\")) { s_logger.Warn(\"database is lock\"); continue; }");
            sb.AppendLine("                        s_logger.Error(ex);");
            sb.AppendLine("                        throw;");
            sb.AppendLine("                    }");
            sb.AppendLine("                }");
            sb.AppendLine("                throw new System.TimeoutException();");
            sb.AppendLine("            }");
            sb.AppendLine("            finally");
            sb.AppendLine("            {");
            sb.AppendLine("                if (!isTransaction && localConn != null) await localConn.DisposeAsync().ConfigureAwait(false);");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
        }

        private static void GenerateFindByMethods(StringBuilder sb, EntityInfo info)
        {
            var allColumns = info.Properties;
            var columnList = string.Join(", ", allColumns.Select(p => p.ColumnName));

            sb.AppendLine();
            sb.AppendLine("        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> s_findBySqlCache = new();");

            sb.AppendLine();
            sb.AppendLine($"        private const string _findBySelectPrefix = \"SELECT {columnList} FROM \";");

            sb.AppendLine();
            sb.AppendLine("        protected override string BuildFindByFastSql(string tableName, System.Collections.Generic.IReadOnlyList<string> orderedColumnNames)");
            sb.AppendLine("        {");
            sb.AppendLine("            var cacheKey = tableName + \"|\" + string.Join(\",\", orderedColumnNames);");
            sb.AppendLine("            return s_findBySqlCache.GetOrAdd(cacheKey, _ =>");
            sb.AppendLine("            {");
            sb.AppendLine("                var sb = new System.Text.StringBuilder();");
            sb.AppendLine("                sb.Append(_findBySelectPrefix).Append(tableName).Append(\" WHERE \");");
            sb.AppendLine("                for (int i = 0; i < orderedColumnNames.Count; i++)");
            sb.AppendLine("                {");
            sb.AppendLine("                    if (i > 0) sb.Append(\" AND \");");
            sb.AppendLine("                    var col = orderedColumnNames[i];");
            sb.AppendLine("                    sb.Append(col).Append(\" = @\").Append(col.ToLowerInvariant());");
            sb.AppendLine("                }");
            sb.AppendLine("                return sb.ToString();");
            sb.AppendLine("            });");
            sb.AppendLine("        }");
        }

        private static void GenerateDeleteMethods(StringBuilder sb, EntityInfo info)
        {
            sb.AppendLine();
            sb.AppendLine("        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> s_deleteBySqlCache = new();");

            sb.AppendLine();
            sb.AppendLine("        private const string _deleteAllSqlTemplate = \"DELETE FROM {0}\";");

            sb.AppendLine();
            sb.AppendLine("        protected override string GetDeleteAllFastSql(string tableName)");
            sb.AppendLine("            => string.Format(_deleteAllSqlTemplate, tableName);");

            sb.AppendLine();
            sb.AppendLine("        protected override string BuildDeleteFastSql(string tableName, System.Collections.Generic.IReadOnlyList<string> orderedColumnNames)");
            sb.AppendLine("        {");
            sb.AppendLine("            var cacheKey = tableName + \"|\" + string.Join(\",\", orderedColumnNames);");
            sb.AppendLine("            return s_deleteBySqlCache.GetOrAdd(cacheKey, _ =>");
            sb.AppendLine("            {");
            sb.AppendLine("                var sb = new System.Text.StringBuilder();");
            sb.AppendLine("                sb.Append(\"DELETE FROM \").Append(tableName).Append(\" WHERE \");");
            sb.AppendLine("                for (int i = 0; i < orderedColumnNames.Count; i++)");
            sb.AppendLine("                {");
            sb.AppendLine("                    if (i > 0) sb.Append(\" AND \");");
            sb.AppendLine("                    var col = orderedColumnNames[i];");
            sb.AppendLine("                    sb.Append(col).Append(\" = @\").Append(col.ToLowerInvariant());");
            sb.AppendLine("                }");
            sb.AppendLine("                return sb.ToString();");
            sb.AppendLine("            });");
            sb.AppendLine("        }");
        }

        private static void GenerateSelectMethods(StringBuilder sb, EntityInfo info)
        {
            var allColumns = info.Properties;
            var columnList = string.Join(", ", allColumns.Select(p => p.ColumnName));

            // SQL template
            sb.AppendLine();
            sb.AppendLine($"        private const string _findAllSqlTemplate = \"SELECT {columnList} FROM {{0}}\";");

            // GetFindAllFastSql
            sb.AppendLine();
            sb.AppendLine("        protected override string GetFindAllFastSql(string tableName)");
            sb.AppendLine("            => string.Format(_findAllSqlTemplate, tableName);");

            // PrecomputeOrdinals (returns empty array — ToEntityFast uses literal indices from SELECT order)
            sb.AppendLine();
            sb.AppendLine("        protected override int[] PrecomputeOrdinals(IDataRecord reader)");
            sb.AppendLine("            => System.Array.Empty<int>();");

            // ToEntityFast
            sb.AppendLine();
            sb.AppendLine($"        protected override {info.EntityFullName} ToEntityFast(IDataRecord reader, int[] ordinals)");
            sb.AppendLine("        {");
            sb.AppendLine($"            var entity = new {info.EntityFullName}();");

            for (int i = 0; i < allColumns.Count; i++)
            {
                var prop = allColumns[i];
                var readExpr = GenerateReaderReadExpression(prop, i);
                if (prop.IsReactiveProperty)
                    sb.AppendLine($"            entity.{prop.PropertyName}.Value = {readExpr};");
                else
                    sb.AppendLine($"            entity.{prop.PropertyName} = {readExpr};");
            }

            sb.AppendLine("            return entity;");
            sb.AppendLine("        }");
        }

        private static string GenerateReaderReadExpression(PropertyColumnInfo prop, int index)
        {
            string typeName = prop.IsReactiveProperty ? prop.ReactiveInnerTypeName : prop.PropertyTypeName;
            switch (typeName)
            {
                case "bool":
                    return $"reader.GetBoolean({index})";
                case "bool?":
                    return $"reader.IsDBNull({index}) ? (bool?)null : reader.GetBoolean({index})";
                case "char":
                    return $"reader.IsDBNull({index}) ? char.MinValue : reader.GetChar({index})";
                case "char?":
                    return $"reader.IsDBNull({index}) ? (char?)null : reader.GetChar({index})";
                case "string":
                    return $"reader.IsDBNull({index}) ? null : reader.GetString({index})";
                case "int":
                    return $"reader.IsDBNull({index}) ? int.MinValue : reader.GetInt32({index})";
                case "int?":
                    return $"reader.IsDBNull({index}) ? (int?)null : reader.GetInt32({index})";
                case "long":
                    return $"reader.GetInt64({index})";
                case "long?":
                    return $"reader.IsDBNull({index}) ? (long?)null : reader.GetInt64({index})";
                case "float":
                    return $"reader.GetFloat({index})";
                case "float?":
                    return $"reader.IsDBNull({index}) ? (float?)null : reader.GetFloat({index})";
                case "double":
                    return $"reader.GetDouble({index})";
                case "double?":
                    return $"reader.IsDBNull({index}) ? (double?)null : reader.GetDouble({index})";
                case "System.DateTime":
                    return $"reader.GetDateTime({index})";
                case "System.DateTime?":
                    return $"reader.IsDBNull({index}) ? (System.DateTime?)null : reader.GetDateTime({index})";
                case "System.Guid":
                    return $"reader.IsDBNull({index}) ? System.Guid.Empty : reader.GetGuid({index})";
                case "System.Guid?":
                    return $"reader.IsDBNull({index}) ? (System.Guid?)null : reader.GetGuid({index})";
                case "System.Type":
                    return $"reader.IsDBNull({index}) ? null : Homura.Extensions.Extensions.GetCachedType(reader.GetString({index}))";
                case "object":
                    return $"reader.IsDBNull({index}) ? null : reader.GetValue({index})";
                default:
                    // Fallback: cast GetValue
                    return $"reader.IsDBNull({index}) ? default({typeName}) : ({typeName})reader.GetValue({index})";
            }
        }

        private static void GenerateUpdateMethods(StringBuilder sb, EntityInfo info)
        {
            var nonPkColumns = info.Properties.Where(p => !p.IsPrimaryKey).ToList();
            var pkColumns = info.Properties.Where(p => p.IsPrimaryKey).ToList();

            if (!pkColumns.Any()) return; // No PK, can't generate update

            var setClauses = string.Join(", ", nonPkColumns.Select(p => $"{p.ColumnName} = @{p.ColumnName.ToLower()}"));
            var whereClauses = string.Join(" AND ", pkColumns.Select(p => $"{p.ColumnName} = @{p.ColumnName.ToLower()}"));

            // SQL template constant
            sb.AppendLine();
            sb.AppendLine($"        private const string _updateSqlTemplate = \"UPDATE {{0}} SET {setClauses} WHERE {whereClauses}\";");

            // TryBuildUpdateCommand - parameters order: non-PK then PK (matching query builder behavior)
            sb.AppendLine();
            sb.AppendLine($"        protected override bool TryBuildUpdateCommand(DbCommand command, {info.EntityFullName} entity, string tableName)");
            sb.AppendLine("        {");
            sb.AppendLine("            command.CommandText = string.Format(_updateSqlTemplate, tableName);");

            var orderedColumns = nonPkColumns.Concat(pkColumns).ToList();
            for (int i = 0; i < orderedColumns.Count; i++)
            {
                var prop = orderedColumns[i];
                sb.AppendLine($"            var p{i} = command.CreateParameter();");
                sb.AppendLine($"            p{i}.ParameterName = \"@{prop.ColumnName.ToLower()}\";");
                sb.AppendLine($"            p{i}.Value = {GenerateGetValueExpression(prop, "entity")};");
                sb.AppendLine($"            command.Parameters.Add(p{i});");
            }

            sb.AppendLine("            return true;");
            sb.AppendLine("        }");

            // TryUpdateUpdateParameters
            sb.AppendLine();
            sb.AppendLine($"        protected override bool TryUpdateUpdateParameters(DbCommand command, {info.EntityFullName} entity)");
            sb.AppendLine("        {");

            for (int i = 0; i < orderedColumns.Count; i++)
            {
                var prop = orderedColumns[i];
                sb.AppendLine($"            command.Parameters[{i}].Value = {GenerateGetValueExpression(prop, "entity")};");
            }

            sb.AppendLine("            return true;");
            sb.AppendLine("        }");
        }

        private class EntityInfo
        {
            public string EntityName { get; set; }
            public string EntityFullName { get; set; }
            public string Namespace { get; set; }
            public string DaoName { get; set; }
            public List<PropertyColumnInfo> Properties { get; set; }
            public Accessibility Accessibility { get; set; }
            public bool HasVersionDependentColumns { get; set; }
        }

        private class PropertyColumnInfo
        {
            public string PropertyName { get; set; }
            public string ColumnName { get; set; }
            public int Order { get; set; }
            public string SafeGetMethod { get; set; }
            public string PropertyTypeName { get; set; }
            public bool IsReactiveProperty { get; set; }
            public string ReactiveInnerTypeName { get; set; }
            public bool IsPrimaryKey { get; set; }
        }
    }
}
