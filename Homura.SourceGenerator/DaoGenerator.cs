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
            CollectColumnProperties(symbol, properties);

            return new EntityInfo
            {
                EntityName = symbol.Name,
                EntityFullName = symbol.ToDisplayString(),
                Namespace = entityNamespace,
                DaoName = daoName,
                Properties = properties,
                Accessibility = symbol.DeclaredAccessibility,
            };
        }

        private static void CollectColumnProperties(INamedTypeSymbol symbol, List<PropertyColumnInfo> properties)
        {
            // Walk inheritance chain to collect all [Column] properties
            var type = symbol;
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

                    properties.Add(new PropertyColumnInfo
                    {
                        PropertyName = prop.Name,
                        ColumnName = columnName,
                        Order = order,
                        SafeGetMethod = safeGetMethod,
                        PropertyTypeName = prop.Type.ToDisplayString(),
                        IsReactiveProperty = IsReactivePropertyType(prop.Type),
                        ReactiveInnerTypeName = GetReactiveInnerType(prop.Type),
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

            sb.AppendLine("    }");

            if (info.Namespace != null)
                sb.AppendLine("}");

            return sb.ToString();
        }

        private class EntityInfo
        {
            public string EntityName { get; set; }
            public string EntityFullName { get; set; }
            public string Namespace { get; set; }
            public string DaoName { get; set; }
            public List<PropertyColumnInfo> Properties { get; set; }
            public Accessibility Accessibility { get; set; }
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
        }
    }
}
