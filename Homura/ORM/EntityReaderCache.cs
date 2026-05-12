using Homura.Extensions;
using Homura.ORM.Mapping;
using Reactive.Bindings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace Homura.ORM
{
    /// <summary>
    /// Per-entity-type cache of a compiled reader delegate that materializes an entity
    /// from an IDataRecord. Replaces the reflection-based path in
    /// <c>Dao&lt;E&gt;.ToEntityInDefaultWay</c> for the common "no excluded columns" case.
    ///
    /// The compiled delegate is built once per (E, versionType) pair using
    /// <see cref="Expression"/> trees, then reused for every row, so reflection cost
    /// only happens at first call. Handles plain properties and ReactiveProperty/
    /// ReactivePropertySlim wrappers (assigns through <c>.Value</c>).
    /// </summary>
    internal static class EntityReaderCache<E> where E : EntityBaseObject, new()
    {
        private static readonly ConcurrentDictionary<Type, Func<IDataRecord, ITable, E>> s_cache = new();
        private static readonly Type s_reactiveInterface = typeof(IReactiveProperty);
        private static readonly Type s_extensionsType = typeof(Homura.Extensions.Extensions);

        public static Func<IDataRecord, ITable, E> Get(Type versionKey, IReadOnlyList<IColumn> columns)
        {
            return s_cache.GetOrAdd(versionKey ?? typeof(VersionOrigin), _ => Build(columns));
        }

        private static Func<IDataRecord, ITable, E> Build(IReadOnlyList<IColumn> columns)
        {
            var readerParam = Expression.Parameter(typeof(IDataRecord), "reader");
            var tableParam = Expression.Parameter(typeof(ITable), "table");
            var entityVar = Expression.Variable(typeof(E), "entity");

            var body = new List<Expression>(columns.Count + 2);
            body.Add(Expression.Assign(entityVar, Expression.New(typeof(E))));

            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                var propInfo = column.PropertyInfo;
                if (propInfo == null || !propInfo.CanWrite && !IsReactiveProperty(propInfo.PropertyType))
                {
                    continue;
                }

                var assignExpr = BuildColumnAssignment(entityVar, propInfo, column.ColumnName, readerParam, tableParam);
                if (assignExpr != null)
                {
                    body.Add(assignExpr);
                }
            }

            body.Add(entityVar);

            var block = Expression.Block(new[] { entityVar }, body);
            return Expression.Lambda<Func<IDataRecord, ITable, E>>(block, readerParam, tableParam).Compile();
        }

        private static Expression BuildColumnAssignment(
            ParameterExpression entityVar,
            PropertyInfo propInfo,
            string columnName,
            ParameterExpression readerParam,
            ParameterExpression tableParam)
        {
            var entityProp = Expression.Property(entityVar, propInfo);
            Expression assignTarget;
            Type valueType;

            if (IsReactiveProperty(propInfo.PropertyType))
            {
                var valueProp = propInfo.PropertyType.GetProperty("Value");
                if (valueProp == null) return null;
                assignTarget = Expression.Property(entityProp, valueProp);
                valueType = valueProp.PropertyType;
            }
            else
            {
                assignTarget = entityProp;
                valueType = propInfo.PropertyType;
            }

            var readExpr = BuildReadExpression(readerParam, tableParam, columnName, valueType);
            if (readExpr == null) return null;

            return Expression.Assign(assignTarget, readExpr);
        }

        private static Expression BuildReadExpression(
            ParameterExpression reader,
            ParameterExpression table,
            string columnName,
            Type valueType)
        {
            var methodName = GetSafeGetMethodName(valueType);
            if (methodName == null) return null;

            var method = s_extensionsType.GetMethod(
                methodName,
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: new[] { typeof(IDataRecord), typeof(string), typeof(ITable) },
                modifiers: null);
            if (method == null) return null;

            return Expression.Call(method, reader, Expression.Constant(columnName, typeof(string)), table);
        }

        private static bool IsReactiveProperty(Type t)
            => s_reactiveInterface.IsAssignableFrom(t);

        private static string GetSafeGetMethodName(Type t)
        {
            if (t == typeof(bool)) return nameof(Homura.Extensions.Extensions.SafeGetBoolean);
            if (t == typeof(bool?)) return nameof(Homura.Extensions.Extensions.SafeGetNullableBoolean);
            if (t == typeof(char)) return nameof(Homura.Extensions.Extensions.SafeGetChar);
            if (t == typeof(char?)) return nameof(Homura.Extensions.Extensions.SafeGetNullableChar);
            if (t == typeof(string)) return nameof(Homura.Extensions.Extensions.SafeGetString);
            if (t == typeof(int)) return nameof(Homura.Extensions.Extensions.SafeGetInt);
            if (t == typeof(int?)) return nameof(Homura.Extensions.Extensions.SafeGetNullableInt);
            if (t == typeof(long)) return nameof(Homura.Extensions.Extensions.SafeGetLong);
            if (t == typeof(long?)) return nameof(Homura.Extensions.Extensions.SafeNullableGetLong);
            if (t == typeof(float)) return nameof(Homura.Extensions.Extensions.SafeGetFloat);
            if (t == typeof(float?)) return nameof(Homura.Extensions.Extensions.SafeGetNullableFloat);
            if (t == typeof(double)) return nameof(Homura.Extensions.Extensions.SafeGetDouble);
            if (t == typeof(double?)) return nameof(Homura.Extensions.Extensions.SafeGetNullableDouble);
            if (t == typeof(DateTime)) return nameof(Homura.Extensions.Extensions.SafeGetDateTime);
            if (t == typeof(DateTime?)) return nameof(Homura.Extensions.Extensions.SafeGetNullableDateTime);
            if (t == typeof(Guid)) return nameof(Homura.Extensions.Extensions.SafeGetGuid);
            if (t == typeof(Guid?)) return nameof(Homura.Extensions.Extensions.SafeGetNullableGuid);
            if (t == typeof(Type)) return nameof(Homura.Extensions.Extensions.SafeGetType);
            if (t == typeof(object)) return nameof(Homura.Extensions.Extensions.SafeGetObject);
            return null;
        }
    }
}
