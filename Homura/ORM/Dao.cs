
using Dapper;
using Homura.Core;
using Homura.Extensions;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml;
using NLog;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using System.Collections;
using System.Collections.Concurrent;

namespace Homura.ORM
{
    public abstract class Dao<E> : MustInitialize<Type>, IDao<E> where E : EntityBaseObject, new()
    {
        public static readonly string s_COLUMN_NAME = "COLUMN_NAME";
        public static readonly string s_IS_NULLABLE = "IS_NULLABLE";
        public static readonly string s_DATA_TYPE = "DATA_TYPE";
        public static readonly string s_PRIMARY_KEY = "PRIMARY_KEY";

        public static readonly string DELIMITER_SPACE = " ";
        public static readonly string DELIMITER_PARENTHESIS = "(";
        public static readonly string DELIMITER_COMMA = ",";
        public static readonly string CONDITION_AND = "and";

        public IConnection CurrentConnection { get; set; }

        /// <summary>
        /// デフォルトコンストラクタ．
        /// アクセス対象となるエンティティのバージョンは，DefaultVersion属性で指定されたバージョンになる．
        /// </summary>
        protected Dao()
            : base(null)
        {
            OverridedColumns = new List<IColumn>();
            EntityVersionType = VersionHelper.GetDefaultVersion<E>();
        }

        /// <summary>
        /// コンストラクタ．
        /// アクセス対象となるエンティティのバージョンは，コンストラクタ引数に渡したバージョンになる．
        /// </summary>
        /// <param name="entityVersionType">バージョン値．VersionOriginクラスまたはそのサブクラスのTypeオブジェクト．</param>
        protected Dao(Type entityVersionType) : base(entityVersionType)
        {
            OverridedColumns = new List<IColumn>();
            EntityVersionType = entityVersionType;
        }

        protected Dao(DataVersionManager dataVersionManager) : base(VersionHelper.GetVersionTypeFromDataVersionManager<E>(dataVersionManager))
        {
            OverridedColumns = new List<IColumn>();
            EntityVersionType = VersionHelper.GetVersionTypeFromDataVersionManager<E>(dataVersionManager);
        }

        public Type EntityVersionType { get; set; }

        public ITable Table
        {
            get
            {
                if (EntityVersionType != null)
                {
                    return new Table<E>(EntityVersionType);
                }
                else
                {
                    return new Table<E>();
                }
            }
        }

        protected List<IColumn> OverridedColumns { get; set; }

        public string TableName => Table.Name;

        protected IEnumerable<IColumn> Columns => Table.Columns;

        public DbConnection GetConnection()
        {
            if (CurrentConnection != null)
            {
                return CurrentConnection.OpenConnection();
            }
            else
            {
                return ConnectionManager.DefaultConnection.OpenConnection();
            }
        }

        public async Task<DbConnection> GetConnectionAsync()
        {
            if (CurrentConnection != null)
            {
                return await CurrentConnection.OpenConnectionAsync().ConfigureAwait(false);
            }
            else
            {
                return await ConnectionManager.DefaultConnection.OpenConnectionAsync().ConfigureAwait(false);
            }
        }

        public void VerifyTableDefinition(DbConnection conn)
        {
            InitializeColumnDefinitions();
            try
            {
                VerifyColumnDefinitions(conn);
            }
            catch (Exception e)
            {
                throw new DatabaseSchemaException($"Didn't insert because mismatch definition of table:{TableName}", e);
            }
        }

        protected virtual void VerifyColumnDefinitions(DbConnection conn)
        { }

        protected async IAsyncEnumerable<IColumn> GetColumnDefinitions(DbConnection conn = null)
        {
            var isTransaction = conn != null;

            try
            {
                if (!isTransaction)
                {
                    conn = await GetConnectionAsync();
                }
                var tableName = TableName;
                var tables = await conn.GetSchemaAsync("Columns");
                var targetTableRows = tables.Rows.OfType<DataRow>().Where(r => r.ItemArray[2].ToString() == tableName);
                foreach (var objRow in targetTableRows)
                {
                    var isNullable = (bool)objRow.ItemArray[10];
                    var isPrimaryKey = (bool)objRow.ItemArray[27];

                    var constraints = ToIConstraintList(isNullable, isPrimaryKey);

                    yield return new Column(
                        objRow.Field<string>(s_COLUMN_NAME),
                        typeof(E).GetProperty(objRow.Field<string>(s_COLUMN_NAME)).PropertyType,
                        objRow.Field<string>(s_DATA_TYPE).ToUpper(),
                        constraints,
                        targetTableRows.ToList().IndexOf(objRow),
                        null);
                }
            }
            finally
            {
                if (!isTransaction)
                {
                    conn?.Dispose();
                }
            }
        }

        private static IEnumerable<IDdlConstraint> ToIConstraintList(bool isNullable, bool isPrimaryKey)
        {
            if (!isNullable)
            {
                yield return new NotNull();
            }

            if (isPrimaryKey)
            {
                yield return new PrimaryKey();
            }
        }

        protected virtual E ToEntity(IDataRecord reader, params IColumn[] columns)
        {
            return ToEntityInDefaultWay(reader, columns);
        }

        private readonly DelegateCache _dcache = new();

        protected E ToEntityInDefaultWay(IDataRecord reader, params IColumn[] columns)
        {
            var ret = CreateInstance();
            const string VALUE_STR = "Value";

            Columns.Except(columns).AsParallel().ToList().ForEach(column =>
            {
                if (column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty)))
                {
                    _dcache.TryGet<E>(ret.GetType(), column.ColumnName, ret, out var rp);
                    _dcache.TrySet(column.EntityDataType, VALUE_STR, column.EntityDataType,
                        DelegateCache.ConvertTo(column.EntityDataType, rp),
                        CatchThrow(() => GetColumnValue(reader, column, Table)));
                    return;
                }

                _dcache.TrySet<E>(ret.GetType(), column.ColumnName, column.EntityDataType, ret,
                    CatchThrow(() => GetColumnValue(reader, column, Table)));
            });

            return ret;
        }

        internal class DelegateCache
        {
            private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, System.Delegate>> _dictionary = new();
            private static readonly Type _typeOfBool             = typeof(bool);
            private static readonly Type _typeOfNullableBool     = typeof(bool?);
            private static readonly Type _typeOfShort            = typeof(short);
            private static readonly Type _typeOfNullableShort    = typeof(short?);
            private static readonly Type _typeOfInt              = typeof(int);
            private static readonly Type _typeOfNullableInt      = typeof(int?);
            private static readonly Type _typeOfLong             = typeof(long);
            private static readonly Type _typeOfNullableLong     = typeof(long?);
            private static readonly Type _typeOfUshort           = typeof(ushort);
            private static readonly Type _typeOfNullableUshort   = typeof(ushort?);
            private static readonly Type _typeOfUint             = typeof(uint);
            private static readonly Type _typeOfNullableUint     = typeof(uint?);
            private static readonly Type _typeOfUlong            = typeof(ulong);
            private static readonly Type _typeNullableUlong      = typeof(ulong?);
            private static readonly Type _typeOfFloat            = typeof(float);
            private static readonly Type _typeOfNullableFloat    = typeof(float?);
            private static readonly Type _typeOfDouble           = typeof(double);
            private static readonly Type _typeOfNullableDouble   = typeof(double?);
            private static readonly Type _typeOfString           = typeof(string);
            private static readonly Type _typeOfDateTime         = typeof(DateTime);
            private static readonly Type _typeOfNullableDateTime = typeof(DateTime?);
            private static readonly Type _typeOfGuid             = typeof(Guid);
            private static readonly Type _typeOfNullableGuid     = typeof(Guid?);
            private static readonly Type _typeOfType             = typeof(Type);
            private static readonly Type _typeOfObject           = typeof(object);
            private static readonly Type _typeOfRSBool             = typeof(ReactivePropertySlim<bool>);
            private static readonly Type _typeOfRSNullableBool     = typeof(ReactivePropertySlim<bool?>);
            private static readonly Type _typeOfRSShort            = typeof(ReactivePropertySlim<short>);
            private static readonly Type _typeOfRSNullableShort    = typeof(ReactivePropertySlim<short?>);
            private static readonly Type _typeOfRSInt              = typeof(ReactivePropertySlim<int>);
            private static readonly Type _typeOfRSNullableInt      = typeof(ReactivePropertySlim<int?>);
            private static readonly Type _typeOfRSLong             = typeof(ReactivePropertySlim<long>);
            private static readonly Type _typeOfRSNullableLong     = typeof(ReactivePropertySlim<long?>);
            private static readonly Type _typeOfRSUshort           = typeof(ReactivePropertySlim<ushort>);
            private static readonly Type _typeOfRSNullableUshort   = typeof(ReactivePropertySlim<ushort?>);
            private static readonly Type _typeOfRSUint             = typeof(ReactivePropertySlim<uint>);
            private static readonly Type _typeOfRSNullableUint     = typeof(ReactivePropertySlim<uint?>);
            private static readonly Type _typeOfRSUlong            = typeof(ReactivePropertySlim<ulong>);
            private static readonly Type _typeOfRSNullableUlong    = typeof(ReactivePropertySlim<ulong?>);
            private static readonly Type _typeOfRSFloat            = typeof(ReactivePropertySlim<float>);
            private static readonly Type _typeOfRSNullableFloat    = typeof(ReactivePropertySlim<float?>);
            private static readonly Type _typeOfRSDouble           = typeof(ReactivePropertySlim<double>);
            private static readonly Type _typeOfRSNullableDouble   = typeof(ReactivePropertySlim<double?>);
            private static readonly Type _typeOfRSString           = typeof(ReactivePropertySlim<string>);
            private static readonly Type _typeOfRSDateTime         = typeof(ReactivePropertySlim<DateTime>);
            private static readonly Type _typeOfRSNullableDateTime = typeof(ReactivePropertySlim<DateTime?>);
            private static readonly Type _typeOfRSGuid             = typeof(ReactivePropertySlim<Guid>);
            private static readonly Type _typeOfRSNullableGuid     = typeof(ReactivePropertySlim<Guid?>);
            private static readonly Type _typeOfRSType             = typeof(ReactivePropertySlim<Type>);
            private static readonly Type _typeOfRSObject           = typeof(ReactivePropertySlim<object>);
            private static readonly Type _typeOfRBool             = typeof(ReactiveProperty<bool>);
            private static readonly Type _typeOfRNullableBool     = typeof(ReactiveProperty<bool?>);
            private static readonly Type _typeOfRShort            = typeof(ReactiveProperty<short>);
            private static readonly Type _typeOfRNullableShort    = typeof(ReactiveProperty<short?>);
            private static readonly Type _typeOfRInt              = typeof(ReactiveProperty<int>);
            private static readonly Type _typeOfRNullableInt      = typeof(ReactiveProperty<int?>);
            private static readonly Type _typeOfRLong             = typeof(ReactiveProperty<long>);
            private static readonly Type _typeOfRNullableLong     = typeof(ReactiveProperty<long?>);
            private static readonly Type _typeOfRUshort           = typeof(ReactiveProperty<ushort>);
            private static readonly Type _typeOfRNullableUshort   = typeof(ReactiveProperty<ushort?>);
            private static readonly Type _typeOfRUint             = typeof(ReactiveProperty<uint>);
            private static readonly Type _typeOfRNullableUint     = typeof(ReactiveProperty<uint?>);
            private static readonly Type _typeOfRUlong            = typeof(ReactiveProperty<ulong>);
            private static readonly Type _typeOfRNullableUlong    = typeof(ReactiveProperty<ulong?>);
            private static readonly Type _typeOfRFloat            = typeof(ReactiveProperty<float>);
            private static readonly Type _typeOfRNullableFloat    = typeof(ReactiveProperty<float?>);
            private static readonly Type _typeOfRDouble           = typeof(ReactiveProperty<double>);
            private static readonly Type _typeOfRNullableDouble   = typeof(ReactiveProperty<double?>);
            private static readonly Type _typeOfRString           = typeof(ReactiveProperty<string>);
            private static readonly Type _typeOfRDateTime         = typeof(ReactiveProperty<DateTime>);
            private static readonly Type _typeOfRNullableDateTime = typeof(ReactiveProperty<DateTime?>);
            private static readonly Type _typeOfRGuid             = typeof(ReactiveProperty<Guid>);
            private static readonly Type _typeOfRNullableGuid     = typeof(ReactiveProperty<Guid?>);
            private static readonly Type _typeOfRType             = typeof(ReactiveProperty<Type>);
            private static readonly Type _typeOfRObject           = typeof(ReactiveProperty<object>);

            public bool TryGet<TObj>(Type type, string name, TObj? parameter, out object value)
            {
                if (_dictionary.ContainsKey(type) && _dictionary[type] is not null && _dictionary[type].ContainsKey(name))
                {
                    value = ((Func<TObj, object>)_dictionary[type][name])(parameter);
                    return true;
                }

                if (!_dictionary.ContainsKey(type))
                {
                    _dictionary[type] = new ConcurrentDictionary<string, System.Delegate>();
                }

                if (!_dictionary[type].ContainsKey(name))
                {
                    var getter = _dictionary[type][name] = GetGetter<TObj, object>(name);
                    value = ((Func<TObj, object>)getter)(parameter);
                    return true;
                }

                value = default;
                return false;
            }

            public bool TrySet<TObj>(Type type, string name, Type objType, TObj obj, object? parameter)
            {
                if (_dictionary.ContainsKey(type) && _dictionary[type] is not null && _dictionary[type].ContainsKey(name))
                {
                    switch (objType)
                    {
                        case not null when objType == _typeOfBool:
                            ((Action<TObj, bool>)_dictionary[type][name])(obj, parameter is not null ? (bool)parameter : default(bool));
                            return true;
                        case not null when objType == _typeOfNullableBool:
                            ((Action<TObj, bool?>)_dictionary[type][name])(obj, parameter is not null ? (bool?)parameter : default(bool?));
                            return true;
                        case not null when objType == _typeOfShort:
                            ((Action<TObj, short>)_dictionary[type][name])(obj, parameter is not null ? (short)parameter : default(short));
                            return true;
                        case not null when objType == _typeOfNullableShort:
                            ((Action<TObj, short?>)_dictionary[type][name])(obj, parameter is not null ? (short?)parameter : default(short?));
                            return true;
                        case not null when objType == _typeOfInt:
                            ((Action<TObj, int>)_dictionary[type][name])(obj, parameter is not null ? (int)parameter : default(int));
                            return true;
                        case not null when objType == _typeOfNullableInt:
                            ((Action<TObj, int?>)_dictionary[type][name])(obj, parameter is not null ? (int?)parameter : default(int?));
                            return true;
                        case not null when objType == _typeOfLong:
                            ((Action<TObj, long>)_dictionary[type][name])(obj, parameter is not null ? (long)parameter : default(long));
                            return true;
                        case not null when objType == _typeOfNullableLong:
                            ((Action<TObj, long?>)_dictionary[type][name])(obj, parameter is not null ? (long?)parameter : default(long?));
                            return true;
                        case not null when objType == _typeOfUshort:
                            ((Action<TObj, ushort>)_dictionary[type][name])(obj, parameter is not null ? (ushort)parameter : default(ushort));
                            return true;
                        case not null when objType == _typeOfNullableUshort:
                            ((Action<TObj, ushort?>)_dictionary[type][name])(obj, parameter is not null ? (ushort?)parameter : default(ushort?));
                            return true;
                        case not null when objType == _typeOfUint:
                            ((Action<TObj, uint>)_dictionary[type][name])(obj, parameter is not null ? (uint)parameter : default(uint));
                            return true;
                        case not null when objType == _typeOfNullableUint:
                            ((Action<TObj, uint?>)_dictionary[type][name])(obj, parameter is not null ? (uint?)parameter : default(uint?));
                            return true;
                        case not null when objType == _typeOfUlong:
                            ((Action<TObj, ulong>)_dictionary[type][name])(obj, parameter is not null ? (ulong)parameter : default(ulong));
                            return true;
                        case not null when objType == _typeNullableUlong:
                            ((Action<TObj, ulong?>)_dictionary[type][name])(obj, parameter is not null ? (ulong?)parameter : default(ulong?));
                            return true;
                        case not null when objType == _typeOfFloat:
                            ((Action<TObj, float>)_dictionary[type][name])(obj, parameter is not null ? (float)parameter : default(float));
                            return true;
                        case not null when objType == _typeOfNullableFloat:
                            ((Action<TObj, float?>)_dictionary[type][name])(obj, parameter is not null ? (float?)parameter : default(float?));
                            return true;
                        case not null when objType == _typeOfDouble:
                            ((Action<TObj, double>)_dictionary[type][name])(obj, parameter is not null ? (double)parameter : default(double));
                            return true;
                        case not null when objType == _typeOfNullableDouble:
                            ((Action<TObj, double?>)_dictionary[type][name])(obj, parameter is not null ? (double?)parameter : default(double?));
                            return true;
                        case not null when objType == _typeOfString:
                            ((Action<TObj, string>)_dictionary[type][name])(obj, parameter is not null ? (string)parameter : default(string));
                            return true;
                        case not null when objType == _typeOfDateTime:
                            ((Action<TObj, DateTime>)_dictionary[type][name])(obj, parameter is not null ? (DateTime)parameter : default(DateTime));
                            return true;
                        case not null when objType == _typeOfNullableDateTime:
                            ((Action<TObj, DateTime?>)_dictionary[type][name])(obj, parameter is not null ? (DateTime?)parameter : default(DateTime?));
                            return true;
                        case not null when objType == _typeOfGuid:
                            ((Action<TObj, Guid>)_dictionary[type][name])(obj, parameter is not null ? (Guid)parameter : default(Guid));
                            return true;
                        case not null when objType == _typeOfNullableGuid:
                            ((Action<TObj, Guid?>)_dictionary[type][name])(obj, parameter is not null ? (Guid?)parameter : default(Guid?));
                            return true;
                        case not null when objType == _typeOfType:
                            ((Action<TObj, Type>)_dictionary[type][name])(obj, parameter is not null ? (Type)parameter : default(Type));
                            return true;
                        case not null when objType == _typeOfObject:
                            ((Action<TObj, object>)_dictionary[type][name])(obj, parameter is not null ? (object)parameter : default(object));
                            return true;
                        case not null when objType == _typeOfRSBool:
                            ((Action<ReactivePropertySlim<bool>, bool>)_dictionary[type][name])(
                                obj as ReactivePropertySlim<bool>, parameter is not null ? (bool)parameter : default(bool));
                            return true;
                        case not null when objType == _typeOfRSNullableBool:
                            ((Action<ReactivePropertySlim<bool?>, bool?>)_dictionary[type][name])(
                                obj as ReactivePropertySlim<bool?>, parameter is not null ? (bool?)parameter : default(bool?));
                            return true;
                        case not null when objType == _typeOfRSShort:
                            ((Action<ReactivePropertySlim<short>, short>)_dictionary[type][name])(
                                obj as ReactivePropertySlim<short>, parameter is not null ? (short)parameter : default(short));
                            return true;
                        case not null when objType == _typeOfRSNullableShort:
                            ((Action<ReactivePropertySlim<short?>, short?>)_dictionary[type][name])(
                                obj as ReactivePropertySlim<short?>, parameter is not null ? (short?)parameter : default(short?));
                            return true;
                        case not null when objType == _typeOfRSInt:
                            ((Action<ReactivePropertySlim<int>, int>)_dictionary[type][name])(
                                obj as ReactivePropertySlim<int>, parameter is not null ? (int)parameter : default(int));
                            return true;
                        case not null when objType == _typeOfRSNullableInt:
                            ((Action<ReactivePropertySlim<int?>, int?>)_dictionary[type][name])(
                                obj as ReactivePropertySlim<int?>, parameter is not null ? (int?)parameter : default(int?));
                            return true;
                        case not null when objType == _typeOfRSLong:
                            ((Action<ReactivePropertySlim<long>, long>)_dictionary[type][name])(
                                obj as ReactivePropertySlim<long>, parameter is not null ? (long)parameter : default(long));
                            return true;
                        case not null when objType == _typeOfRSNullableLong:
                            ((Action<ReactivePropertySlim<long?>, long?>)_dictionary[type][name])(
                                obj as ReactivePropertySlim<long?>, parameter is not null ? (long?)parameter : default(long?));
                            return true;
                        case not null when objType == _typeOfRSUshort:
                            ((Action<ReactivePropertySlim<ushort>, ushort>)_dictionary[type][name])(
                                obj as ReactivePropertySlim<ushort>, parameter is not null ? (ushort)parameter : default(ushort));
                            return true;
                        case not null when objType == _typeOfRSNullableUshort:
                            ((Action<ReactivePropertySlim<ushort?>, ushort?>)_dictionary[type][name])(
                                obj as ReactivePropertySlim<ushort?>, parameter is not null ? (ushort?)parameter : default(ushort?));
                            return true;
                        case not null when objType == _typeOfRSUint:
                            ((Action<ReactivePropertySlim<uint>, uint>)_dictionary[type][name])(
                                obj as ReactivePropertySlim<uint>, parameter is not null ? (uint)parameter : default(uint));
                            return true;
                        case not null when objType == _typeOfRSNullableUint:
                            ((Action<ReactivePropertySlim<uint?>, uint?>)_dictionary[type][name])(
                                obj as ReactivePropertySlim<uint?>, parameter is not null ? (uint?)parameter : default(uint?));
                            return true;
                        case not null when objType == _typeOfRSUlong:
                            ((Action<ReactivePropertySlim<ulong>, ulong>)_dictionary[type][name])(
                                obj as ReactivePropertySlim<ulong>, parameter is not null ? (ulong)parameter : default(ulong));
                            return true;
                        case not null when objType == _typeOfRSNullableUlong:
                            ((Action<ReactivePropertySlim<ulong?>, ulong?>)_dictionary[type][name])(
                                obj as ReactivePropertySlim<ulong?>, parameter is not null ? (ulong?)parameter : default(ulong?));
                            return true;
                        case not null when objType == _typeOfRSFloat:
                            ((Action<ReactivePropertySlim<float>, float>)_dictionary[type][name])(
                                obj as ReactivePropertySlim<float>, parameter is not null ? (float)parameter : default(float));
                            return true;
                        case not null when objType == _typeOfRSNullableFloat:
                            ((Action<ReactivePropertySlim<float?>, float?>)_dictionary[type][name])(
                                obj as ReactivePropertySlim<float?>, parameter is not null ? (float?)parameter : default(float?));
                            return true;
                        case not null when objType == _typeOfRSDouble:
                            ((Action<ReactivePropertySlim<double>, double>)_dictionary[type][name])(
                                obj as ReactivePropertySlim<double>, parameter is not null ? (double)parameter : default(double));
                            return true;
                        case not null when objType == _typeOfRSNullableDouble:
                            ((Action<ReactivePropertySlim<double?>, double?>)_dictionary[type][name])(
                                obj as ReactivePropertySlim<double?>, parameter is not null ? (double?)parameter : default(double?));
                            return true;
                        case not null when objType == _typeOfRSString:
                            ((Action<ReactivePropertySlim<string>, string>)_dictionary[type][name])(
                                obj as ReactivePropertySlim<string>, parameter is not null ? (string)parameter : default(string));
                            return true;
                        case not null when objType == _typeOfRSDateTime:
                            ((Action<ReactivePropertySlim<DateTime>, DateTime>)_dictionary[type][name])(
                                obj as ReactivePropertySlim<DateTime>, parameter is not null ? (DateTime)parameter : default(DateTime));
                            return true;
                        case not null when objType == _typeOfRSNullableDateTime:
                            ((Action<ReactivePropertySlim<DateTime?>, DateTime?>)_dictionary[type][name])(
                                obj as ReactivePropertySlim<DateTime?>, parameter is not null ? (DateTime?)parameter : default(DateTime?));
                            return true;
                        case not null when objType == _typeOfRSGuid:
                            ((Action<ReactivePropertySlim<Guid>, Guid>)_dictionary[type][name])(
                                obj as ReactivePropertySlim<Guid>, parameter is not null ? (Guid)parameter : default(Guid));
                            return true;
                        case not null when objType == _typeOfRSNullableGuid:
                            ((Action<ReactivePropertySlim<Guid?>, Guid?>)_dictionary[type][name])(
                                obj as ReactivePropertySlim<Guid?>, parameter is not null ? (Guid?)parameter : default(Guid?));
                            return true;
                        case not null when objType == _typeOfRSType:
                            ((Action<ReactivePropertySlim<Type>, Type>)_dictionary[type][name])(
                                obj as ReactivePropertySlim<Type>, parameter is not null ? (Type)parameter : default(Type));
                            return true;
                        case not null when objType == _typeOfRSObject:
                            ((Action<ReactivePropertySlim<object>, object>)_dictionary[type][name])(
                                obj as ReactivePropertySlim<object>, parameter is not null ? (object)parameter : default(object));
                            return true;
                        case not null when objType == _typeOfRBool:
                            ((Action<ReactiveProperty<bool>, bool>)_dictionary[type][name])(
                                obj as ReactiveProperty<bool>, parameter is not null ? (bool)parameter : default(bool));
                            return true;
                        case not null when objType == _typeOfRNullableBool:
                            ((Action<ReactiveProperty<bool?>, bool?>)_dictionary[type][name])(
                                obj as ReactiveProperty<bool?>, parameter is not null ? (bool?)parameter : default(bool?));
                            return true;
                        case not null when objType == _typeOfRShort:
                            ((Action<ReactiveProperty<short>, short>)_dictionary[type][name])(
                                obj as ReactiveProperty<short>, parameter is not null ? (short)parameter : default(short));
                            return true;
                        case not null when objType == _typeOfRNullableShort:
                            ((Action<ReactiveProperty<short?>, short?>)_dictionary[type][name])(
                                obj as ReactiveProperty<short?>, parameter is not null ? (short?)parameter : default(short?));
                            return true;
                        case not null when objType == _typeOfRInt:
                            ((Action<ReactiveProperty<int>, int>)_dictionary[type][name])(
                                obj as ReactiveProperty<int>, parameter is not null ? (int)parameter : default(int));
                            return true;
                        case not null when objType == _typeOfRNullableInt:
                            ((Action<ReactiveProperty<int?>, int?>)_dictionary[type][name])(
                                obj as ReactiveProperty<int?>, parameter is not null ? (int?)parameter : default(int?));
                            return true;
                        case not null when objType == _typeOfRLong:
                            ((Action<ReactiveProperty<long>, long>)_dictionary[type][name])(
                                obj as ReactiveProperty<long>, parameter is not null ? (long)parameter : default(long));
                            return true;
                        case not null when objType == _typeOfRNullableLong:
                            ((Action<ReactiveProperty<long?>, long?>)_dictionary[type][name])(
                                obj as ReactiveProperty<long?>, parameter is not null ? (long?)parameter : default(long?));
                            return true;
                        case not null when objType == _typeOfRUshort:
                            ((Action<ReactiveProperty<ushort>, ushort>)_dictionary[type][name])(
                                obj as ReactiveProperty<ushort>, parameter is not null ? (ushort)parameter : default(ushort));
                            return true;
                        case not null when objType == _typeOfRNullableUshort:
                            ((Action<ReactiveProperty<ushort?>, ushort?>)_dictionary[type][name])(
                                obj as ReactiveProperty<ushort?>, parameter is not null ? (ushort?)parameter : default(ushort?));
                            return true;
                        case not null when objType == _typeOfRUint:
                            ((Action<ReactiveProperty<uint>, uint>)_dictionary[type][name])(
                                obj as ReactiveProperty<uint>, parameter is not null ? (uint)parameter : default(uint));
                            return true;
                        case not null when objType == _typeOfRNullableUint:
                            ((Action<ReactiveProperty<uint?>, uint?>)_dictionary[type][name])(
                                obj as ReactiveProperty<uint?>, parameter is not null ? (uint?)parameter : default(uint?));
                            return true;
                        case not null when objType == _typeOfRUlong:
                            ((Action<ReactiveProperty<ulong>, ulong>)_dictionary[type][name])(
                                obj as ReactiveProperty<ulong>, parameter is not null ? (ulong)parameter : default(ulong));
                            return true;
                        case not null when objType == _typeOfRNullableUlong:
                            ((Action<ReactiveProperty<ulong?>, ulong?>)_dictionary[type][name])(
                                obj as ReactiveProperty<ulong?>, parameter is not null ? (ulong?)parameter : default(ulong?));
                            return true;
                        case not null when objType == _typeOfRFloat:
                            ((Action<ReactiveProperty<float>, float>)_dictionary[type][name])(
                                obj as ReactiveProperty<float>, parameter is not null ? (float)parameter : default(float));
                            return true;
                        case not null when objType == _typeOfRNullableFloat:
                            ((Action<ReactiveProperty<float?>, float?>)_dictionary[type][name])(
                                obj as ReactiveProperty<float?>, parameter is not null ? (float?)parameter : default(float?));
                            return true;
                        case not null when objType == _typeOfRDouble:
                            ((Action<ReactiveProperty<double>, double>)_dictionary[type][name])(
                                obj as ReactiveProperty<double>, parameter is not null ? (double)parameter : default(double));
                            return true;
                        case not null when objType == _typeOfRNullableDouble:
                            ((Action<ReactiveProperty<double?>, double?>)_dictionary[type][name])(
                                obj as ReactiveProperty<double?>, parameter is not null ? (double?)parameter : default(double?));
                            return true;
                        case not null when objType == _typeOfRString:
                            ((Action<ReactiveProperty<string>, string>)_dictionary[type][name])(
                                obj as ReactiveProperty<string>, parameter is not null ? (string)parameter : default(string));
                            return true;
                        case not null when objType == _typeOfRDateTime:
                            ((Action<ReactiveProperty<DateTime>, DateTime>)_dictionary[type][name])(
                                obj as ReactiveProperty<DateTime>, parameter is not null ? (DateTime)parameter : default(DateTime));
                            return true;
                        case not null when objType == _typeOfRNullableDateTime:
                            ((Action<ReactiveProperty<DateTime?>, DateTime?>)_dictionary[type][name])(
                                obj as ReactiveProperty<DateTime?>, parameter is not null ? (DateTime?)parameter : default(DateTime?));
                            return true;
                        case not null when objType == _typeOfRGuid:
                            ((Action<ReactiveProperty<Guid>, Guid>)_dictionary[type][name])(
                                obj as ReactiveProperty<Guid>, parameter is not null ? (Guid)parameter : default(Guid));
                            return true;
                        case not null when objType == _typeOfRNullableGuid:
                            ((Action<ReactiveProperty<Guid?>, Guid?>)_dictionary[type][name])(
                                obj as ReactiveProperty<Guid?>, parameter is not null ? (Guid?)parameter : default(Guid?));
                            return true;
                        case not null when objType == _typeOfRType:
                            ((Action<ReactiveProperty<Type>, Type>)_dictionary[type][name])(
                                obj as ReactiveProperty<Type>, parameter is not null ? (Type)parameter : default(Type));
                            return true;
                        case not null when objType == _typeOfRObject:
                            ((Action<ReactiveProperty<object>, object>)_dictionary[type][name])(
                                obj as ReactiveProperty<object>, parameter is not null ? (object)parameter : default(object));
                            return true;
                        default:
                            return false;
                    }
                }

                if (!_dictionary.ContainsKey(type))
                {
                    _dictionary[type] = new ConcurrentDictionary<string, System.Delegate>();
                }

                switch (objType)
                {
                    case not null when objType == _typeOfBool:
                        _dictionary[type][name] = GetSetter<TObj, bool>(obj.GetType(), name);
                        ((Action<TObj, bool>)_dictionary[type][name])(obj, parameter is not null ? (bool)parameter : default(bool));
                        return true;
                    case not null when objType == _typeOfNullableBool:
                        _dictionary[type][name] = GetSetter<TObj, bool?>(obj.GetType(), name);
                        ((Action<TObj, bool?>)_dictionary[type][name])(obj, parameter is not null ? (bool?)parameter : default(bool?));
                        return true;
                    case not null when objType == _typeOfShort:
                        _dictionary[type][name] = GetSetter<TObj, short>(obj.GetType(), name);
                        ((Action<TObj, short>)_dictionary[type][name])(obj, parameter is not null ? (short)parameter : default(short));
                        return true;
                    case not null when objType == _typeOfNullableShort:
                        _dictionary[type][name] = GetSetter<TObj, short?>(obj.GetType(), name);
                        ((Action<TObj, short?>)_dictionary[type][name])(obj, parameter is not null ? (short?)parameter : default(short?));
                        return true;
                    case not null when objType == _typeOfInt:
                        _dictionary[type][name] = GetSetter<TObj, int>(obj.GetType(), name);
                        ((Action<TObj, int>)_dictionary[type][name])(obj, parameter is not null ? (int)parameter : default(int));
                        return true;
                    case not null when objType == _typeOfNullableInt:
                        _dictionary[type][name] = GetSetter<TObj, int?>(obj.GetType(), name);
                        ((Action<TObj, int?>)_dictionary[type][name])(obj, parameter is not null ? (int?)parameter : default(int?));
                        return true;
                    case not null when objType == _typeOfLong:
                        _dictionary[type][name] = GetSetter<TObj, long>(obj.GetType(), name);
                        ((Action<TObj, long>)_dictionary[type][name])(obj, parameter is not null ? (long)parameter : default(long));
                        return true;
                    case not null when objType == _typeOfNullableLong:
                        _dictionary[type][name] = GetSetter<TObj, long?>(obj.GetType(), name);
                        ((Action<TObj, long?>)_dictionary[type][name])(obj, parameter is not null ? (long?)parameter : default(long?));
                        return true;
                    case not null when objType == _typeOfUshort:
                        _dictionary[type][name] = GetSetter<TObj, ushort>(obj.GetType(), name);
                        ((Action<TObj, ushort>)_dictionary[type][name])(obj, parameter is not null ? (ushort)parameter : default(ushort));
                        return true;
                    case not null when objType == _typeOfNullableUshort:
                        _dictionary[type][name] = GetSetter<TObj, ushort?>(obj.GetType(), name);
                        ((Action<TObj, ushort?>)_dictionary[type][name])(obj, parameter is not null ? (ushort?)parameter : default(ushort?));
                        return true;
                    case not null when objType == _typeOfUint:
                        _dictionary[type][name] = GetSetter<TObj, uint>(obj.GetType(), name);
                        ((Action<TObj, uint>)_dictionary[type][name])(obj, parameter is not null ? (uint)parameter : default(uint));
                        return true;
                    case not null when objType == _typeOfNullableUint:
                        _dictionary[type][name] = GetSetter<TObj, uint?>(obj.GetType(), name);
                        ((Action<TObj, uint?>)_dictionary[type][name])(obj, parameter is not null ? (uint?)parameter : default(uint?));
                        return true;
                    case not null when objType == _typeOfUlong:
                        _dictionary[type][name] = GetSetter<TObj, ulong>(obj.GetType(), name);
                        ((Action<TObj, ulong>)_dictionary[type][name])(obj, parameter is not null ? (ulong)parameter : default(ulong));
                        return true;
                    case not null when objType == _typeNullableUlong:
                        _dictionary[type][name] = GetSetter<TObj, ulong?>(obj.GetType(), name);
                        ((Action<TObj, ulong?>)_dictionary[type][name])(obj, parameter is not null ? (ulong?)parameter : default(ulong?));
                        return true;
                    case not null when objType == _typeOfFloat:
                        _dictionary[type][name] = GetSetter<TObj, float>(obj.GetType(), name);
                        ((Action<TObj, float>)_dictionary[type][name])(obj, parameter is not null ? (float)parameter : default(float));
                        return true;
                    case not null when objType == _typeOfNullableFloat:
                        _dictionary[type][name] = GetSetter<TObj, float?>(obj.GetType(), name);
                        ((Action<TObj, float?>)_dictionary[type][name])(obj, parameter is not null ? (float?)parameter : default(float?));
                        return true;
                    case not null when objType == _typeOfDouble:
                        _dictionary[type][name] = GetSetter<TObj, double>(obj.GetType(), name);
                        ((Action<TObj, double>)_dictionary[type][name])(obj, parameter is not null ? (double)parameter : default(double));
                        return true;
                    case not null when objType == _typeOfNullableDouble:
                        _dictionary[type][name] = GetSetter<TObj, double?>(obj.GetType(), name);
                        ((Action<TObj, double?>)_dictionary[type][name])(obj, parameter is not null ? (double?)parameter : default(double?));
                        return true;
                    case not null when objType == _typeOfString:
                        _dictionary[type][name] = GetSetter<TObj, string>(obj.GetType(), name);
                        ((Action<TObj, string>)_dictionary[type][name])(obj, parameter is not null ? (string)parameter : default(string));
                        return true;
                    case not null when objType == _typeOfDateTime:
                        _dictionary[type][name] = GetSetter<TObj, DateTime>(obj.GetType(), name);
                        ((Action<TObj, DateTime>)_dictionary[type][name])(obj, parameter is not null ? (DateTime)parameter : default(DateTime));
                        return true;
                    case not null when objType == _typeOfNullableDateTime:
                        _dictionary[type][name] = GetSetter<TObj, DateTime?>(obj.GetType(), name);
                        ((Action<TObj, DateTime?>)_dictionary[type][name])(obj, parameter is not null ? (DateTime?)parameter : default(DateTime?));
                        return true;
                    case not null when objType == _typeOfGuid:
                        _dictionary[type][name] = GetSetter<TObj, Guid>(obj.GetType(), name);
                        ((Action<TObj, Guid>)_dictionary[type][name])(obj, parameter is not null ? (Guid)parameter : default(Guid));
                        return true;
                    case not null when objType == _typeOfNullableGuid:
                        _dictionary[type][name] = GetSetter<TObj, Guid?>(obj.GetType(), name);
                        ((Action<TObj, Guid?>)_dictionary[type][name])(obj, parameter is not null ? (Guid?)parameter : default(Guid?));
                        return true;
                    case not null when objType == _typeOfType:
                        _dictionary[type][name] = GetSetter<TObj, Type>(obj.GetType(), name);
                        ((Action<TObj, Type>)_dictionary[type][name])(obj, parameter is not null ? (Type)parameter : default(Type));
                        return true;
                    case not null when objType == _typeOfObject:
                        _dictionary[type][name] = GetSetter<TObj, object>(obj.GetType(), name);
                        ((Action<TObj, object>)_dictionary[type][name])(obj, parameter is not null ? (object)parameter : default(object));
                        return true;
                    case not null when objType == _typeOfRSBool:
                        _dictionary[type][name] = GetSetter<ReactivePropertySlim<bool>, bool>(obj.GetType(), name);
                        ((Action<ReactivePropertySlim<bool>, bool>)_dictionary[type][name])(obj as ReactivePropertySlim<bool>, parameter is not null ? (bool)parameter : default(bool));
                        return true;
                    case not null when objType == _typeOfRSNullableBool:
                        _dictionary[type][name] = GetSetter<ReactivePropertySlim<bool?>, bool?>(obj.GetType(), name);
                        ((Action<ReactivePropertySlim<bool?>, bool?>)_dictionary[type][name])(obj as ReactivePropertySlim<bool?>, parameter is not null ? (bool?)parameter : default(bool?));
                        return true;
                    case not null when objType == _typeOfRSShort:
                        _dictionary[type][name] = GetSetter<ReactivePropertySlim<short>, short>(obj.GetType(), name);
                        ((Action<ReactivePropertySlim<short>, short>)_dictionary[type][name])(obj as ReactivePropertySlim<short>, parameter is not null ? (short)parameter : default(short));
                        return true;
                    case not null when objType == _typeOfRSNullableShort:
                        _dictionary[type][name] = GetSetter<ReactivePropertySlim<short?>, short?>(obj.GetType(), name);
                        ((Action<ReactivePropertySlim<short?>, short?>)_dictionary[type][name])(obj as ReactivePropertySlim<short?>, parameter is not null ? (short?)parameter : default(short?));
                        return true;
                    case not null when objType == _typeOfRSInt:
                        _dictionary[type][name] = GetSetter<ReactivePropertySlim<int>, int>(obj.GetType(), name);
                        ((Action<ReactivePropertySlim<int>, int>)_dictionary[type][name])(obj as ReactivePropertySlim<int>, parameter is not null ? (int)parameter : default(int));
                        return true;
                    case not null when objType == _typeOfRSNullableInt:
                        _dictionary[type][name] = GetSetter<ReactivePropertySlim<int?>, int?>(obj.GetType(), name);
                        ((Action<ReactivePropertySlim<int?>, int?>)_dictionary[type][name])(obj as ReactivePropertySlim<int?>, parameter is not null ? (int?)parameter : default(int?));
                        return true;
                    case not null when objType == _typeOfRSLong:
                        _dictionary[type][name] = GetSetter<ReactivePropertySlim<long>, long>(obj.GetType(), name);
                        ((Action<ReactivePropertySlim<long>, long>)_dictionary[type][name])(obj as ReactivePropertySlim<long>, parameter is not null ? (long)parameter : default(long));
                        return true;
                    case not null when objType == _typeOfRSNullableLong:
                        _dictionary[type][name] = GetSetter<ReactivePropertySlim<long?>, long?>(obj.GetType(), name);
                        ((Action<ReactivePropertySlim<long?>, long?>)_dictionary[type][name])(obj as ReactivePropertySlim<long?>, parameter is not null ? (long?)parameter : default(long?));
                        return true;
                    case not null when objType == _typeOfRSUshort:
                        _dictionary[type][name] = GetSetter<ReactivePropertySlim<ushort>, ushort>(obj.GetType(), name);
                        ((Action<ReactivePropertySlim<ushort>, ushort>)_dictionary[type][name])(obj as ReactivePropertySlim<ushort>, parameter is not null ? (ushort)parameter : default(ushort));
                        return true;
                    case not null when objType == _typeOfRSNullableUshort:
                        _dictionary[type][name] = GetSetter<ReactivePropertySlim<ushort?>, ushort?>(obj.GetType(), name);
                        ((Action<ReactivePropertySlim<ushort?>, ushort?>)_dictionary[type][name])(obj as ReactivePropertySlim<ushort?>, parameter is not null ? (ushort?)parameter : default(ushort?));
                        return true;
                    case not null when objType == _typeOfRSUint:
                        _dictionary[type][name] = GetSetter<ReactivePropertySlim<uint>, uint>(obj.GetType(), name);
                        ((Action<ReactivePropertySlim<uint>, uint>)_dictionary[type][name])(obj as ReactivePropertySlim<uint>, parameter is not null ? (uint)parameter : default(uint));
                        return true;
                    case not null when objType == _typeOfRSNullableUint:
                        _dictionary[type][name] = GetSetter<ReactivePropertySlim<uint?>, uint?>(obj.GetType(), name);
                        ((Action<ReactivePropertySlim<uint?>, uint?>)_dictionary[type][name])(obj as ReactivePropertySlim<uint?>, parameter is not null ? (uint?)parameter : default(uint?));
                        return true;
                    case not null when objType == _typeOfRSUlong:
                        _dictionary[type][name] = GetSetter<ReactivePropertySlim<ulong>, ulong>(obj.GetType(), name);
                        ((Action<ReactivePropertySlim<ulong>, ulong>)_dictionary[type][name])(obj as ReactivePropertySlim<ulong>, parameter is not null ? (ulong)parameter : default(ulong));
                        return true;
                    case not null when objType == _typeOfRSNullableUlong:
                        _dictionary[type][name] = GetSetter<ReactivePropertySlim<ulong?>, ulong?>(obj.GetType(), name);
                        ((Action<ReactivePropertySlim<ulong?>, ulong?>)_dictionary[type][name])(obj as ReactivePropertySlim<ulong?>, parameter is not null ? (ulong?)parameter : default(ulong?));
                        return true;
                    case not null when objType == _typeOfRSFloat:
                        _dictionary[type][name] = GetSetter<ReactivePropertySlim<float>, float>(obj.GetType(), name);
                        ((Action<ReactivePropertySlim<float>, float>)_dictionary[type][name])(obj as ReactivePropertySlim<float>, parameter is not null ? (float)parameter : default(float));
                        return true;
                    case not null when objType == _typeOfRSNullableFloat:
                        _dictionary[type][name] = GetSetter<ReactivePropertySlim<float?>, float?>(obj.GetType(), name);
                        ((Action<ReactivePropertySlim<float?>, float?>)_dictionary[type][name])(obj as ReactivePropertySlim<float?>, parameter is not null ? (float?)parameter : default(float?));
                        return true;
                    case not null when objType == _typeOfRSDouble:
                        _dictionary[type][name] = GetSetter<ReactivePropertySlim<double>, double>(obj.GetType(), name);
                        ((Action<ReactivePropertySlim<double>, double>)_dictionary[type][name])(obj as ReactivePropertySlim<double>, parameter is not null ? (double)parameter : default(double));
                        return true;
                    case not null when objType == _typeOfRSNullableDouble:
                        _dictionary[type][name] = GetSetter<ReactivePropertySlim<double?>, double?>(obj.GetType(), name);
                        ((Action<ReactivePropertySlim<double?>, double?>)_dictionary[type][name])(obj as ReactivePropertySlim<double?>, parameter is not null ? (double?)parameter : default(double?));
                        return true;
                    case not null when objType == _typeOfRSString:
                        _dictionary[type][name] = GetSetter<ReactivePropertySlim<string>, string>(obj.GetType(), name);
                        ((Action<ReactivePropertySlim<string>, string>)_dictionary[type][name])(obj as ReactivePropertySlim<string>, parameter is not null ? (string)parameter : default(string));
                        return true;
                    case not null when objType == _typeOfRSDateTime:
                        _dictionary[type][name] = GetSetter<ReactivePropertySlim<DateTime>, DateTime>(obj.GetType(), name);
                        ((Action<ReactivePropertySlim<DateTime>, DateTime>)_dictionary[type][name])(obj as ReactivePropertySlim<DateTime>, parameter is not null ? (DateTime)parameter : default(DateTime));
                        return true;
                    case not null when objType == _typeOfRSNullableDateTime:
                        _dictionary[type][name] = GetSetter<ReactivePropertySlim<DateTime?>, DateTime?>(obj.GetType(), name);
                        ((Action<ReactivePropertySlim<DateTime?>, DateTime?>)_dictionary[type][name])(obj as ReactivePropertySlim<DateTime?>, parameter is not null ? (DateTime?)parameter : default(DateTime?));
                        return true;
                    case not null when objType == _typeOfRSGuid:
                        _dictionary[type][name] = GetSetter<ReactivePropertySlim<Guid>, Guid>(obj.GetType(), name);
                        ((Action<ReactivePropertySlim<Guid>, Guid>)_dictionary[type][name])(obj as ReactivePropertySlim<Guid>, parameter is not null ? (Guid)parameter : default(Guid));
                        return true;
                    case not null when objType == _typeOfRSNullableGuid:
                        _dictionary[type][name] = GetSetter<ReactivePropertySlim<Guid?>, Guid?>(obj.GetType(), name);
                        ((Action<ReactivePropertySlim<Guid?>, Guid?>)_dictionary[type][name])(obj as ReactivePropertySlim<Guid?>, parameter is not null ? (Guid?)parameter : default(Guid?));
                        return true;
                    case not null when objType == _typeOfRSType:
                        _dictionary[type][name] = GetSetter<ReactivePropertySlim<Type>, Type>(obj.GetType(), name);
                        ((Action<ReactivePropertySlim<Type>, Type>)_dictionary[type][name])(obj as ReactivePropertySlim<Type>, parameter is not null ? (Type)parameter : default(Type));
                        return true;
                    case not null when objType == _typeOfRSObject:
                        _dictionary[type][name] = GetSetter<ReactivePropertySlim<object>, object>(obj.GetType(), name);
                        ((Action<ReactivePropertySlim<object>, object>)_dictionary[type][name])(obj as ReactivePropertySlim<object>, parameter is not null ? (object)parameter : default(object));
                        return true;
                    case not null when objType == _typeOfRBool:
                        _dictionary[type][name] = GetSetter<ReactiveProperty<bool>, bool>(obj.GetType(), name);
                        ((Action<ReactiveProperty<bool>, bool>)_dictionary[type][name])(obj as ReactiveProperty<bool>, parameter is not null ? (bool)parameter : default(bool));
                        return true;
                    case not null when objType == _typeOfRNullableBool:
                        _dictionary[type][name] = GetSetter<ReactiveProperty<bool?>, bool?>(obj.GetType(), name);
                        ((Action<ReactiveProperty<bool?>, bool?>)_dictionary[type][name])(obj as ReactiveProperty<bool?>, parameter is not null ? (bool?)parameter : default(bool?));
                        return true;
                    case not null when objType == _typeOfRShort:
                        _dictionary[type][name] = GetSetter<ReactiveProperty<short>, short>(obj.GetType(), name);
                        ((Action<ReactiveProperty<short>, short>)_dictionary[type][name])(obj as ReactiveProperty<short>, parameter is not null ? (short)parameter : default(short));
                        return true;
                    case not null when objType == _typeOfRNullableShort:
                        _dictionary[type][name] = GetSetter<ReactiveProperty<short?>, short?>(obj.GetType(), name);
                        ((Action<ReactiveProperty<short?>, short?>)_dictionary[type][name])(obj as ReactiveProperty<short?>, parameter is not null ? (short?)parameter : default(short?));
                        return true;
                    case not null when objType == _typeOfRInt:
                        _dictionary[type][name] = GetSetter<ReactiveProperty<int>, int>(obj.GetType(), name);
                        ((Action<ReactiveProperty<int>, int>)_dictionary[type][name])(obj as ReactiveProperty<int>, parameter is not null ? (int)parameter : default(int));
                        return true;
                    case not null when objType == _typeOfRNullableInt:
                        _dictionary[type][name] = GetSetter<ReactiveProperty<int?>, int?>(obj.GetType(), name);
                        ((Action<ReactiveProperty<int?>, int?>)_dictionary[type][name])(obj as ReactiveProperty<int?>, parameter is not null ? (int?)parameter : default(int?));
                        return true;
                    case not null when objType == _typeOfRLong:
                        _dictionary[type][name] = GetSetter<ReactiveProperty<long>, long>(obj.GetType(), name);
                        ((Action<ReactiveProperty<long>, long>)_dictionary[type][name])(obj as ReactiveProperty<long>, parameter is not null ? (long)parameter : default(long));
                        return true;
                    case not null when objType == _typeOfRNullableLong:
                        _dictionary[type][name] = GetSetter<ReactiveProperty<long?>, long?>(obj.GetType(), name);
                        ((Action<ReactiveProperty<long?>, long?>)_dictionary[type][name])(obj as ReactiveProperty<long?>, parameter is not null ? (long?)parameter : default(long?));
                        return true;
                    case not null when objType == _typeOfRUshort:
                        _dictionary[type][name] = GetSetter<ReactiveProperty<ushort>, ushort>(obj.GetType(), name);
                        ((Action<ReactiveProperty<ushort>, ushort>)_dictionary[type][name])(obj as ReactiveProperty<ushort>, parameter is not null ? (ushort)parameter : default(ushort));
                        return true;
                    case not null when objType == _typeOfRNullableUshort:
                        _dictionary[type][name] = GetSetter<ReactiveProperty<ushort?>, ushort?>(obj.GetType(), name);
                        ((Action<ReactiveProperty<ushort?>, ushort?>)_dictionary[type][name])(obj as ReactiveProperty<ushort?>, parameter is not null ? (ushort?)parameter : default(ushort?));
                        return true;
                    case not null when objType == _typeOfRUint:
                        _dictionary[type][name] = GetSetter<ReactiveProperty<uint>, uint>(obj.GetType(), name);
                        ((Action<ReactiveProperty<uint>, uint>)_dictionary[type][name])(obj as ReactiveProperty<uint>, parameter is not null ? (uint)parameter : default(uint));
                        return true;
                    case not null when objType == _typeOfRNullableUint:
                        _dictionary[type][name] = GetSetter<ReactiveProperty<uint?>, uint?>(obj.GetType(), name);
                        ((Action<ReactiveProperty<uint?>, uint?>)_dictionary[type][name])(obj as ReactiveProperty<uint?>, parameter is not null ? (uint?)parameter : default(uint?));
                        return true;
                    case not null when objType == _typeOfRUlong:
                        _dictionary[type][name] = GetSetter<ReactiveProperty<ulong>, ulong>(obj.GetType(), name);
                        ((Action<ReactiveProperty<ulong>, ulong>)_dictionary[type][name])(obj as ReactiveProperty<ulong>, parameter is not null ? (ulong)parameter : default(ulong));
                        return true;
                    case not null when objType == _typeOfRNullableUlong:
                        _dictionary[type][name] = GetSetter<ReactiveProperty<ulong?>, ulong?>(obj.GetType(), name);
                        ((Action<ReactiveProperty<ulong?>, ulong?>)_dictionary[type][name])(obj as ReactiveProperty<ulong?>, parameter is not null ? (ulong?)parameter : default(ulong?));
                        return true;
                    case not null when objType == _typeOfRFloat:
                        _dictionary[type][name] = GetSetter<ReactiveProperty<float>, float>(obj.GetType(), name);
                        ((Action<ReactiveProperty<float>, float>)_dictionary[type][name])(obj as ReactiveProperty<float>, parameter is not null ? (float)parameter : default(float));
                        return true;
                    case not null when objType == _typeOfRNullableFloat:
                        _dictionary[type][name] = GetSetter<ReactiveProperty<float?>, float?>(obj.GetType(), name);
                        ((Action<ReactiveProperty<float?>, float?>)_dictionary[type][name])(obj as ReactiveProperty<float?>, parameter is not null ? (float?)parameter : default(float?));
                        return true;
                    case not null when objType == _typeOfRDouble:
                        _dictionary[type][name] = GetSetter<ReactiveProperty<double>, double>(obj.GetType(), name);
                        ((Action<ReactiveProperty<double>, double>)_dictionary[type][name])(obj as ReactiveProperty<double>, parameter is not null ? (double)parameter : default(double));
                        return true;
                    case not null when objType == _typeOfRNullableDouble:
                        _dictionary[type][name] = GetSetter<ReactiveProperty<double?>, double?>(obj.GetType(), name);
                        ((Action<ReactiveProperty<double?>, double?>)_dictionary[type][name])(obj as ReactiveProperty<double?>, parameter is not null ? (double?)parameter : default(double?));
                        return true;
                    case not null when objType == _typeOfRString:
                        _dictionary[type][name] = GetSetter<ReactiveProperty<string>, string>(obj.GetType(), name);
                        ((Action<ReactiveProperty<string>, string>)_dictionary[type][name])(obj as ReactiveProperty<string>, parameter is not null ? (string)parameter : default(string));
                        return true;
                    case not null when objType == _typeOfRDateTime:
                        _dictionary[type][name] = GetSetter<ReactiveProperty<DateTime>, DateTime>(obj.GetType(), name);
                        ((Action<ReactiveProperty<DateTime>, DateTime>)_dictionary[type][name])(obj as ReactiveProperty<DateTime>, parameter is not null ? (DateTime)parameter : default(DateTime));
                        return true;
                    case not null when objType == _typeOfRNullableDateTime:
                        _dictionary[type][name] = GetSetter<ReactiveProperty<DateTime?>, DateTime?>(obj.GetType(), name);
                        ((Action<ReactiveProperty<DateTime?>, DateTime?>)_dictionary[type][name])(obj as ReactiveProperty<DateTime?>, parameter is not null ? (DateTime?)parameter : default(DateTime?));
                        return true;
                    case not null when objType == _typeOfRGuid:
                        _dictionary[type][name] = GetSetter<ReactiveProperty<Guid>, Guid>(obj.GetType(), name);
                        ((Action<ReactiveProperty<Guid>, Guid>)_dictionary[type][name])(obj as ReactiveProperty<Guid>, parameter is not null ? (Guid)parameter : default(Guid));
                        return true;
                    case not null when objType == _typeOfRNullableGuid:
                        _dictionary[type][name] = GetSetter<ReactiveProperty<Guid?>, Guid?>(obj.GetType(), name);
                        ((Action<ReactiveProperty<Guid?>, Guid?>)_dictionary[type][name])(obj as ReactiveProperty<Guid?>, parameter is not null ? (Guid?)parameter : default(Guid?));
                        return true;
                    case not null when objType == _typeOfRType:
                        _dictionary[type][name] = GetSetter<ReactiveProperty<Type>, Type>(obj.GetType(), name);
                        ((Action<ReactiveProperty<Type>, Type>)_dictionary[type][name])(obj as ReactiveProperty<Type>, parameter is not null ? (Type)parameter : default(Type));
                        return true;
                    case not null when objType == _typeOfRObject:
                        _dictionary[type][name] = GetSetter<ReactiveProperty<object>, object>(obj.GetType(), name);
                        ((Action<ReactiveProperty<object>, object>)_dictionary[type][name])(obj as ReactiveProperty<object>, parameter is not null ? (object)parameter : default(object));
                        return true;
                    default:
                        return false;
                }
            }

            public static object ConvertTo(Type targetType, object value)
            {
                // ターゲット型がvalueの型と同じであれば、変換不要
                if (value.GetType() == targetType)
                {
                    return value;
                }

                // ここで特定の型へのカスタム変換ロジックを実装
                return targetType switch
                {
                    not null when targetType == _typeOfRSBool => (ReactivePropertySlim<bool>)value,
                    not null when targetType == _typeOfRSNullableBool => (ReactivePropertySlim<bool?>)value,
                    not null when targetType == _typeOfRSShort => (ReactivePropertySlim<short>)value,
                    not null when targetType == _typeOfRSNullableShort => (ReactivePropertySlim<short?>)value,
                    not null when targetType == _typeOfRSInt => (ReactivePropertySlim<int>)value,
                    not null when targetType == _typeOfRSNullableInt => (ReactivePropertySlim<int?>)value,
                    not null when targetType == _typeOfRSLong => (ReactivePropertySlim<long>)value,
                    not null when targetType == _typeOfRSNullableLong => (ReactivePropertySlim<long?>)value,
                    not null when targetType == _typeOfRSUshort => (ReactivePropertySlim<ushort>)value,
                    not null when targetType == _typeOfRSNullableUshort => (ReactivePropertySlim<ushort?>)value,
                    not null when targetType == _typeOfRSUint => (ReactivePropertySlim<uint>)value,
                    not null when targetType == _typeOfRSNullableUint => (ReactivePropertySlim<uint?>)value,
                    not null when targetType == _typeOfRSUlong => (ReactivePropertySlim<ulong>)value,
                    not null when targetType == _typeOfRSNullableUlong => (ReactivePropertySlim<ulong?>)value,
                    not null when targetType == _typeOfRSFloat => (ReactivePropertySlim<float>)value,
                    not null when targetType == _typeOfRSNullableFloat => (ReactivePropertySlim<float?>)value,
                    not null when targetType == _typeOfRSDouble => (ReactivePropertySlim<double>)value,
                    not null when targetType == _typeOfRSNullableDouble => (ReactivePropertySlim<double?>)value,
                    not null when targetType == _typeOfRSString => (ReactivePropertySlim<string>)value,
                    not null when targetType == _typeOfRSDateTime => (ReactivePropertySlim<DateTime>)value,
                    not null when targetType == _typeOfRSNullableDateTime => (ReactivePropertySlim<DateTime?>)value,
                    not null when targetType == _typeOfRSGuid => (ReactivePropertySlim<Guid>)value,
                    not null when targetType == _typeOfRSNullableGuid => (ReactivePropertySlim<Guid?>)value,
                    not null when targetType == _typeOfRSType => (ReactivePropertySlim<Type>)value,
                    not null when targetType == _typeOfRSObject => (ReactivePropertySlim<object>)value,
                    not null when targetType == _typeOfRBool => (ReactiveProperty<bool>)value,
                    not null when targetType == _typeOfRNullableBool => (ReactiveProperty<bool?>)value,
                    not null when targetType == _typeOfRShort => (ReactiveProperty<short>)value,
                    not null when targetType == _typeOfRNullableShort => (ReactiveProperty<short?>)value,
                    not null when targetType == _typeOfRInt => (ReactiveProperty<int>)value,
                    not null when targetType == _typeOfRNullableInt => (ReactiveProperty<int?>)value,
                    not null when targetType == _typeOfRLong => (ReactiveProperty<long>)value,
                    not null when targetType == _typeOfRNullableLong => (ReactiveProperty<long?>)value,
                    not null when targetType == _typeOfRUshort => (ReactiveProperty<ushort>)value,
                    not null when targetType == _typeOfRNullableUshort => (ReactiveProperty<ushort?>)value,
                    not null when targetType == _typeOfRUint => (ReactiveProperty<uint>)value,
                    not null when targetType == _typeOfRNullableUint => (ReactiveProperty<uint?>)value,
                    not null when targetType == _typeOfRUlong => (ReactiveProperty<ulong>)value,
                    not null when targetType == _typeOfRNullableUlong => (ReactiveProperty<ulong?>)value,
                    not null when targetType == _typeOfRFloat => (ReactiveProperty<float>)value,
                    not null when targetType == _typeOfRNullableFloat => (ReactiveProperty<float?>)value,
                    not null when targetType == _typeOfRDouble => (ReactiveProperty<double>)value,
                    not null when targetType == _typeOfRNullableDouble => (ReactiveProperty<double?>)value,
                    not null when targetType == _typeOfRString => (ReactiveProperty<string>)value,
                    not null when targetType == _typeOfRDateTime => (ReactiveProperty<DateTime>)value,
                    not null when targetType == _typeOfRNullableDateTime => (ReactiveProperty<DateTime?>)value,
                    not null when targetType == _typeOfRGuid => (ReactiveProperty<Guid>)value,
                    not null when targetType == _typeOfRNullableGuid => (ReactiveProperty<Guid?>)value,
                    not null when targetType == _typeOfRType => (ReactiveProperty<Type>)value,
                    not null when targetType == _typeOfRObject => (ReactiveProperty<object>)value,
                    _ => throw new InvalidOperationException("サポートされていない型への変換"),
                };
            }

            public static Func<TObj, TProp> GetGetter<TObj, TProp>(string propName)
                => (Func<TObj, TProp>)
                    System.Delegate.CreateDelegate(typeof(Func<TObj, TProp>),
                        typeof(TObj).GetProperty(propName).GetGetMethod(nonPublic: true));

            public static Action<TObj, TProp> GetSetter<TObj, TProp>(Type type, string propName)
            {
                var ret = (Action<TObj, TProp>)
                    System.Delegate.CreateDelegate(typeof(Action<,>).MakeGenericType(type, typeof(TProp)),
                        type.GetProperty(propName).GetSetMethod(nonPublic: true));
                if (ret is null)
                {

                }
                return ret;
            }
        }

        protected object GetColumnValue(IDataRecord reader, IColumn column, ITable table)
        {
            return column.EntityDataType switch
            {
                not null when column.EntityDataType == typeof(bool) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<bool>)) => reader.SafeGetBoolean(column.ColumnName, table),
                not null when column.EntityDataType == typeof(bool?) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<bool?>)) => reader.SafeGetNullableBoolean(column.ColumnName, table),
                not null when column.EntityDataType == typeof(char) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<char>)) => reader.SafeGetChar(column.ColumnName, table),
                not null when column.EntityDataType == typeof(char?) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<char?>)) => reader.SafeGetNullableChar(column.ColumnName, table),
                not null when column.EntityDataType == typeof(string) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<string>)) => reader.SafeGetString(column.ColumnName, table),
                not null when column.EntityDataType == typeof(int) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<int>)) => reader.SafeGetInt(column.ColumnName, table),
                not null when column.EntityDataType == typeof(int?) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<int?>)) => reader.SafeGetNullableInt(column.ColumnName, table),
                not null when column.EntityDataType == typeof(long) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<long>)) => reader.SafeGetLong(column.ColumnName, table),
                not null when column.EntityDataType == typeof(long?) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<long?>)) => reader.SafeNullableGetLong(column.ColumnName, table),
                not null when column.EntityDataType == typeof(float) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<float>)) => reader.SafeGetFloat(column.ColumnName, table),
                not null when column.EntityDataType == typeof(float?) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<float?>)) => reader.SafeGetNullableFloat(column.ColumnName, table),
                not null when column.EntityDataType == typeof(double) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<double>)) => reader.SafeGetDouble(column.ColumnName, table),
                not null when column.EntityDataType == typeof(double?) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<double?>)) => reader.SafeGetNullableDouble(column.ColumnName, table),
                not null when column.EntityDataType == typeof(DateTime) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<DateTime>)) => reader.SafeGetDateTime(column.ColumnName, table),
                not null when column.EntityDataType == typeof(DateTime?) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<DateTime?>)) => reader.SafeGetNullableDateTime(column.ColumnName, table),
                not null when column.EntityDataType == typeof(Guid) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<Guid>)) => reader.SafeGetGuid(column.ColumnName, table),
                not null when column.EntityDataType == typeof(Guid?) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<Guid?>)) => reader.SafeGetNullableGuid(column.ColumnName, table),
                not null when column.EntityDataType == typeof(Type) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<Type>)) => reader.SafeGetType(column.ColumnName, table),
                not null when column.EntityDataType == typeof(object) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<object>)) => reader.SafeGetObject(column.ColumnName, table),
                not null when column.DBDataType == "INTEGER" => reader.SafeGetInt(column.ColumnName, table),
                _ => throw new NotSupportedException($"{column.EntityDataType.FullName} is not supported."),
            };
        }

        private static E CreateInstance()
        {
            return InstanceCreator<E>.CreateInstance();
        }

        public void CreateTableIfNotExists(TimeSpan? timeout = null)
        {
            QueryHelper.KeepTryingUntilProcessSucceed(() =>
            {
                using var conn = GetConnection();
                var sql = $"create table if not exists {TableName}";
                DefineColumns(ref sql, Columns);
                LogManager.GetCurrentClassLogger().Debug(sql);
                conn.Execute(sql);
            }, timeout);
        }

        public async Task CreateTableIfNotExistsAsync(TimeSpan? timeout = null)
        {
            await QueryHelper.KeepTryingUntilProcessSucceedAsync(new Func<Task>(async () =>
            {
                await using var conn = await GetConnectionAsync().ConfigureAwait(false);
                var sql = $"create table if not exists {TableName}";
                DefineColumns(ref sql, Columns);
                LogManager.GetCurrentClassLogger().Debug(sql);
                await conn.ExecuteAsync(sql).ConfigureAwait(false);
            }), timeout);
        }

        public void DropTableIfExists(TimeSpan? timeout = null)
        {
            QueryHelper.KeepTryingUntilProcessSucceed(() =>
            {
                using var conn = GetConnection();
                var sql = $"drop table if exists {TableName}";
                LogManager.GetCurrentClassLogger().Debug(sql);
                conn.Execute(sql);
            }, timeout);
        }

        public async Task DropTableIfExistsAsync(TimeSpan? timeout = null)
        {
            await QueryHelper.KeepTryingUntilProcessSucceedAsync(new Func<Task>(async () =>
            {
                await using var conn = await GetConnectionAsync().ConfigureAwait(false);
                var sql = $"drop table if exists {TableName}";
                LogManager.GetCurrentClassLogger().Debug(sql);
                await conn.ExecuteAsync(sql).ConfigureAwait(false);
            }), timeout).ConfigureAwait(false);
        }

        public int CreateIndexIfNotExists(TimeSpan? timeout = null)
        {
            var created = 0;
            QueryHelper.KeepTryingUntilProcessSucceed(() =>
            {
                created = CreateIndexClass(created);
                created = CreateIndexProperties(created);
            }, timeout);
            return created;
        }

        public async Task<int> CreateIndexIfNotExistsAsync(TimeSpan? timeout = null)
        {
            var created = 0;
            await QueryHelper.KeepTryingUntilProcessSucceedAsync(new Func<Task>(async () =>
            {
                created = await CreateIndexClassAsync(created);
                created = await CreateIndexPropertiesAsync(created);
            }), timeout).ConfigureAwait(false);
            return created;
        }

        private int CreateIndexProperties(int created)
        {
            var indexPropertyNames = SearchIndexProperties();
            foreach (var indexPropertyName in indexPropertyNames)
            {
                var indexName = $"index_{TableName}_{indexPropertyName}";

                using var conn = GetConnection();
                var sql = $"create index if not exists {indexName} on {TableName}({indexPropertyName})";
                LogManager.GetCurrentClassLogger().Debug(sql);
                var result = conn.Execute(sql);
                if (result != -1)
                {
                    created += 1;
                }
            }

            return created;
        }
        private async Task<int> CreateIndexPropertiesAsync(int created)
        {
            var indexPropertyNames = SearchIndexProperties();
            foreach (var indexPropertyName in indexPropertyNames)
            {
                var indexName = $"index_{TableName}_{indexPropertyName}";
                await using var conn = await GetConnectionAsync().ConfigureAwait(false);
                var sql = $"create index if not exists {indexName} on {TableName}({indexPropertyName})";
                LogManager.GetCurrentClassLogger().Debug(sql);
                var result = await conn.ExecuteAsync(sql).ConfigureAwait(false);
                if (result != -1)
                {
                    created += 1;
                }
            }

            return created;
        }

        private int CreateIndexClass(int created)
        {
            var indexColumnNames = SearchIndexClass();
            if (!indexColumnNames.Any()) return created;
            var indexName = $"index_{TableName}_";
            Queue<string> queue = new(indexColumnNames);
            while (queue.Any())
            {
                indexName += queue.Dequeue();
                if (queue.Any())
                {
                    indexName += "_";
                }
            }

            using var conn = GetConnection();
            var sql = $"create index if not exists {indexName} on {TableName}(";
            Queue<string> queue2 = new(indexColumnNames);
            while (queue2.Any())
            {
                sql += queue2.Dequeue();
                if (queue2.Any())
                {
                    sql += ", ";
                }
            }
            sql += ")";

            LogManager.GetCurrentClassLogger().Debug(sql);
            var result = conn.Execute(sql);
            if (result != -1)
            {
                created += 1;
            }

            return created;
        }

        private async Task<int> CreateIndexClassAsync(int created)
        {
            var indexColumnNames = SearchIndexClass();
            if (!indexColumnNames.Any()) return created;
            var indexName = $"index_{TableName}_";
            Queue<string> queue = new(indexColumnNames);
            while (queue.Any())
            {
                indexName += queue.Dequeue();
                if (queue.Any())
                {
                    indexName += "_";
                }
            }

            await using var conn = await GetConnectionAsync().ConfigureAwait(false);
            var sql = $"create index if not exists {indexName} on {TableName}(";
            Queue<string> queue2 = new(indexColumnNames);
            while (queue2.Any())
            {
                sql += queue2.Dequeue();
                if (queue2.Any())
                {
                    sql += ", ";
                }
            }
            sql += ")";

            LogManager.GetCurrentClassLogger().Debug(sql);
            var result = await conn.ExecuteAsync(sql).ConfigureAwait(false);
            if (result != -1)
            {
                created += 1;
            }

            return created;
        }

        private static HashSet<string> SearchIndexProperties()
        {
            HashSet<string> indexColumnNames = new();
            var pInfoList = typeof(E).GetProperties();

            foreach (var pInfo in pInfoList)
            {
                var indexAttr = pInfo.GetCustomAttribute<IndexAttribute>();
                var columnAttr = pInfo.GetCustomAttribute<ColumnAttribute>();

                if (indexAttr != null && columnAttr != null)
                {
                    indexColumnNames.Add(pInfo.Name);
                }
            }

            return indexColumnNames;
        }

        private static HashSet<string> SearchIndexClass()
        {
            HashSet<string> indexColumnNames = new();
            var cInfo = typeof(E);
            var indexAttr = cInfo.GetCustomAttribute<IndexAttribute>();
            if (indexAttr == null) return indexColumnNames;
            foreach (var name in indexAttr.PropertyNames)
            {
                indexColumnNames.Add(name);
            }
            return indexColumnNames;
        }

        public int CountAll(DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            return QueryHelper.KeepTryingUntilProcessSucceed<int>(() =>
                QueryHelper.ForDao.ConnectionInternalAndReturn(this, new Func<DbConnection, int>((connection) =>
                {
                    using var command = connection.CreateCommand();
                    var table = (Homura.ORM.Table<E>)Table.Clone();
                    if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                    {
                        table.Schema = anotherDatabaseAliasName;
                    }

                    using var query = new Select().Count("1").As("Count")
                        .From.Table(table);
                    var sql = query.ToSql();

                    command.CommandText = sql;

                    LogManager.GetCurrentClassLogger().Debug(sql);
                    using var reader = command.ExecuteReader();
                    reader.Read();
                    return reader.GetInt32(reader.GetOrdinal("Count"));
                }), conn)
            , timeout);
        }

        public async Task<int> CountAllAsync(DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            return await QueryHelper.KeepTryingUntilProcessSucceedAndReturnAsync<int>(async () =>
                await await QueryHelper.ForDao.ConnectionInternalAndReturnAsync(this, new Func<DbConnection, Task<int>>(async (connection) =>
                {
                    await using var command = connection.CreateCommand();
                    var table = (Homura.ORM.Table<E>)Table.Clone();
                    if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                    {
                        table.Schema = anotherDatabaseAliasName;
                    }

                    using var query = new Select().Count("1").As("Count")
                        .From.Table(table);
                    var sql = query.ToSql();

                    command.CommandText = sql;

                    LogManager.GetCurrentClassLogger().Debug(sql);
                    await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                    await reader.ReadAsync().ConfigureAwait(false);
                    return reader.GetInt32(reader.GetOrdinal("Count"));
                }), conn).ConfigureAwait(false)
            , timeout).ConfigureAwait(false);
        }

        public int CountBy(Dictionary<string, object> idDic, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            return QueryHelper.KeepTryingUntilProcessSucceed<int>(() =>
                QueryHelper.ForDao.ConnectionInternalAndReturn(this, new Func<DbConnection, int>((connection) =>
                {
                    using var command = connection.CreateCommand();
                    var table = (Table<E>)Table.Clone();
                    if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                    {
                        table.Schema = anotherDatabaseAliasName;
                    }

                    using var query = new Select().Count("1").As("Count")
                        .From.Table(table)
                        .Where.KeyEqualToValue(idDic);
                    var sql = query.ToSql();

                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    query.SetParameters(command);

                    LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                    using var reader = command.ExecuteReader();
                    reader.Read();
                    return reader.GetInt32(reader.GetOrdinal("Count"));
                }), conn)
            , timeout);
        }

        public async Task<int> CountByAsync(Dictionary<string, object> idDic, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            return await QueryHelper.KeepTryingUntilProcessSucceedAndReturnAsync(async () =>
                await await QueryHelper.ForDao.ConnectionInternalAndReturnAsync(this, new Func<DbConnection, Task<int>>(async (connection) =>
                {
                    await using var command = connection.CreateCommand();
                    var table = (Table<E>)Table.Clone();
                    if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                    {
                        table.Schema = anotherDatabaseAliasName;
                    }

                    using var query = new Select().Count("1").As("Count")
                        .From.Table(table)
                        .Where.KeyEqualToValue(idDic);
                    var sql = query.ToSql();

                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    query.SetParameters(command);

                    LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                    await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                    await reader.ReadAsync().ConfigureAwait(false);
                    return reader.GetInt32(reader.GetOrdinal("Count"));
                }), conn).ConfigureAwait(false)
            , timeout).ConfigureAwait(false);
        }

        public void DeleteWhereIDIs(Guid id, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            QueryHelper.KeepTryingUntilProcessSucceed(() =>
            {
                QueryHelper.ForDao.ConnectionInternal(this, (connection) =>
                {
                    using var command = connection.CreateCommand();
                    var table = (Table<E>)Table.Clone();
                    if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                    {
                        table.Schema = anotherDatabaseAliasName;
                    }

                    using var query = new Delete().From.Table(table)
                        .Where.Column("ID").EqualTo.Value(id);
                    var sql = query.ToSql();
                    command.CommandText = sql;

                    query.SetParameters(command);

                    LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                    command.ExecuteNonQuery();
                }, conn);
            }, timeout);
        }

        public async Task DeleteWhereIDIsAsync(Guid id, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            await QueryHelper.KeepTryingUntilProcessSucceedAsync(async () =>
            {
                await QueryHelper.ForDao.ConnectionInternalAsync(this, async (connection) =>
                {
                    await using var command = connection.CreateCommand();
                    var table = (Table<E>)Table.Clone();
                    if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                    {
                        table.Schema = anotherDatabaseAliasName;
                    }

                    using var query = new Delete().From.Table(table)
                        .Where.Column("ID").EqualTo.Value(id);
                    var sql = query.ToSql();
                    command.CommandText = sql;

                    query.SetParameters(command);

                    LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }, conn).ConfigureAwait(false);

            }, timeout).ConfigureAwait(false);
        }

        public void DeleteAll(DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            QueryHelper.KeepTryingUntilProcessSucceed(() =>
            {
                QueryHelper.ForDao.ConnectionInternal(this, new Action<DbConnection>((connection) =>
                {
                    using var command = connection.CreateCommand();
                    var table = (Table<E>)Table.Clone();
                    if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                    {
                        table.Schema = anotherDatabaseAliasName;
                    }

                    using var query = new Delete().From.Table(table);
                    var sql = query.ToSql();
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    query.SetParameters(command);

                    LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                    var deleted = command.ExecuteNonQuery();
                }), conn);
            }, timeout);
        }

        public async Task DeleteAllAsync(DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            await QueryHelper.KeepTryingUntilProcessSucceedAsync(async () =>
            {
                await QueryHelper.ForDao.ConnectionInternalAsync(this, async (connection) =>
                {
                    await using var command = connection.CreateCommand();
                    var table = (Table<E>)Table.Clone();
                    if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                    {
                        table.Schema = anotherDatabaseAliasName;
                    }

                    using var query = new Delete().From.Table(table);
                    var sql = query.ToSql();
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    query.SetParameters(command);

                    LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                    var deleted = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }, conn).ConfigureAwait(false);
            }, timeout).ConfigureAwait(false);
        }

        public void Delete(Dictionary<string, object> idDic, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            QueryHelper.KeepTryingUntilProcessSucceed(() =>
            {
                QueryHelper.ForDao.ConnectionInternal(this, new Action<DbConnection>((connection) =>
                {
                    using var command = connection.CreateCommand();
                    var table = (Table<E>)Table.Clone();
                    if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                    {
                        table.Schema = anotherDatabaseAliasName;
                    }

                    using var query = new Delete().From.Table(table)
                        .Where.KeyEqualToValue(idDic);
                    var sql = query.ToSql();
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    query.SetParameters(command);

                    LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                    var deleted = command.ExecuteNonQuery();
                }), conn);
            }, timeout);
        }

        public async Task DeleteAsync(Dictionary<string, object> idDic, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            await QueryHelper.KeepTryingUntilProcessSucceedAsync(async () =>
            {
                await QueryHelper.ForDao.ConnectionInternalAsync(this, async (connection) =>
                {
                    await using var command = connection.CreateCommand();
                    var table = (Table<E>)Table.Clone();
                    if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                    {
                        table.Schema = anotherDatabaseAliasName;
                    }

                    using var query = new Delete().From.Table(table)
                        .Where.KeyEqualToValue(idDic);
                    var sql = query.ToSql();
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    query.SetParameters(command);

                    LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                    var deleted = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }, conn).ConfigureAwait(false);
            }, timeout).ConfigureAwait(false);
        }

        public void Insert(E entity, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            InitializeColumnDefinitions();
            try
            {
                VerifyColumnDefinitions(conn);
            }
            catch (NotMatchColumnException e)
            {
                throw new DatabaseSchemaException($"Didn't insert because mismatch definition of table:{TableName}", e);
            }

            QueryHelper.KeepTryingUntilProcessSucceed(() =>
            {
                QueryHelper.ForDao.ConnectionInternal(this, new Action<DbConnection>((connection) =>
                {
                    using var command = connection.CreateCommand();
                    var overrideColumns = SwapIfOverrided(Columns);

                    var table = (Table<E>)Table.Clone();
                    if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                    {
                        table.Schema = anotherDatabaseAliasName;
                    }

                    using var query = new Insert().Into.Table(table).Columns(overrideColumns.Select(c => c.ColumnName))
                        .Values.Value(overrideColumns.Select(c => c.PropertyGetter(entity)));
                    var sql = query.ToSql();
                    command.CommandText = sql;
                    query.SetParameters(command);

                    LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                    var inserted = command.ExecuteNonQuery();
                    if (inserted == 0)
                    {
                        throw new NoEntityInsertedException($"Failed:{sql} {query.GetParameters().ToStringKeyIsValue()}");
                    }
                }), conn);
            }, timeout);
        }

        public async Task InsertAsync(E entity, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            InitializeColumnDefinitions();
            try
            {
                VerifyColumnDefinitions(conn);
            }
            catch (NotMatchColumnException e)
            {
                throw new DatabaseSchemaException($"Didn't insert because mismatch definition of table:{TableName}", e);
            }

            await QueryHelper.KeepTryingUntilProcessSucceedAsync(async () =>
            {
                await QueryHelper.ForDao.ConnectionInternalAsync(this, async (connection) =>
                {
                    await using var command = connection.CreateCommand();
                    var overrideColumns = SwapIfOverrided(Columns);

                    var table = (Table<E>)Table.Clone();
                    if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                    {
                        table.Schema = anotherDatabaseAliasName;
                    }

                    using var query = new Insert().Into.Table(table).Columns(overrideColumns.Select(c => c.ColumnName))
                        .Values.Value(overrideColumns.Select(c => c.PropertyGetter(entity)));
                    var sql = query.ToSql();
                    command.CommandText = sql;
                    query.SetParameters(command);

                    LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                    var inserted = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    if (inserted == 0)
                    {
                        throw new NoEntityInsertedException($"Failed:{sql} {query.GetParameters().ToStringKeyIsValue()}");
                    }
                }, conn).ConfigureAwait(false);
            }, timeout).ConfigureAwait(false);
        }

        protected IEnumerable<IColumn> SwapIfOverrided(IEnumerable<IColumn> columns)
        {
            List<IColumn> ret = new();

            foreach (var column in columns)
            {
                if (OverridedColumns != null && OverridedColumns.Count(oc => oc.ColumnName == column.ColumnName) == 1)
                {
                    ret.Add(OverridedColumns.Single(oc => oc.ColumnName == column.ColumnName));
                }
                else
                {
                    ret.Add(column);
                }
            }

            return ret.OrderBy(c => c.Order);
        }

        public IEnumerable<E> FindAll(DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            timeout ??= TimeSpan.FromMinutes(5);

            var beginTime = DateTime.Now;

            List<E> ret = new();

            while ((DateTime.Now - beginTime) <= timeout)
            {
                try
                {
                    var isTransaction = conn != null;

                    try
                    {
                        if (!isTransaction)
                        {
                            conn = GetConnection();
                        }

                        using var command = conn.CreateCommand();
                        var table = (Table<E>)Table.Clone();
                        if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                        {
                            table.Schema = anotherDatabaseAliasName;
                        }

                        using var query = new Select().Asterisk().From.Table(table);
                        var sql = query.ToSql();
                        command.CommandText = sql;
                        command.CommandType = CommandType.Text;

                        LogManager.GetCurrentClassLogger().Debug(sql);
                        using var reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            ret.Add(ToEntity(reader));
                        }

                        return ret;
                    }
                    finally
                    {
                        if (!isTransaction)
                        {
                            conn?.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("database is lock"))
                    {
                        LogManager.GetCurrentClassLogger().Warn("database is lock");
                        continue;
                    }
                    else
                    {
                        LogManager.GetCurrentClassLogger().Error(ex);
                        throw;
                    }
                }
            }

            throw new TimeoutException();
        }

        public async IAsyncEnumerable<E> FindAllAsync(DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            timeout ??= TimeSpan.FromMinutes(5);

            var beginTime = DateTime.Now;

            List<E> ret = new();

            while ((DateTime.Now - beginTime) <= timeout)
            {
                try
                {
                    var isTransaction = conn != null;

                    try
                    {
                        if (!isTransaction)
                        {
                            conn = await GetConnectionAsync().ConfigureAwait(false);
                        }

                        await using var command = conn.CreateCommand();
                        var table = (Table<E>)Table.Clone();
                        if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                        {
                            table.Schema = anotherDatabaseAliasName;
                        }

                        using var query = new Select().Asterisk().From.Table(table);
                        var sql = query.ToSql();
                        command.CommandText = sql;
                        command.CommandType = CommandType.Text;

                        LogManager.GetCurrentClassLogger().Debug(sql);
                        await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            ret.Add(ToEntity(reader));
                        }
                    }
                    finally
                    {
                        if (!isTransaction)
                        {
                            await conn.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("database is lock"))
                    {
                        LogManager.GetCurrentClassLogger().Warn("database is lock");
                        continue;
                    }
                    else
                    {
                        LogManager.GetCurrentClassLogger().Error(ex);
                        throw;
                    }
                }

                foreach (var item in ret)
                {
                    yield return item;
                }

                yield break;
            }

            throw new TimeoutException();
        }

        public IEnumerable<E> FindBy(Dictionary<string, object> idDic, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            timeout ??= TimeSpan.FromMinutes(5);

            var beginTime = DateTime.Now;

            List<E> ret = new();
            var isTransaction = conn != null;

            try
            {
                if (!isTransaction)
                {
                    conn = GetConnection();
                }

                while ((DateTime.Now - beginTime) <= timeout)
                {

                    try
                    {

                        using var command = conn.CreateCommand();
                        var table = (Table<E>)Table.Clone();
                        if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                        {
                            table.Schema = anotherDatabaseAliasName;
                        }

                        using var query = new Select().Asterisk().From.Table(table)
                            .Where.KeyEqualToValue(idDic);
                        var sql = query.ToSql();
                        command.CommandText = sql;
                        command.CommandType = CommandType.Text;
                        query.SetParameters(command);

                        LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                        using var reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            ret.Add(ToEntity(reader));
                        }

                        return ret;
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("database is lock"))
                        {
                            LogManager.GetCurrentClassLogger().Warn("database is lock");
                            continue;
                        }
                        else
                        {
                            LogManager.GetCurrentClassLogger().Error(ex);
                            throw;
                        }
                    }
                }
            }
            finally
            {
                if (!isTransaction)
                {
                    conn?.Dispose();
                }
            }

            throw new TimeoutException();
        }

        public async IAsyncEnumerable<E> FindByAsync(Dictionary<string, object> idDic, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            timeout ??= TimeSpan.FromMinutes(5);

            var beginTime = DateTime.Now;

            List<E> ret = new();
            var isTransaction = conn != null;
            try
            {
                if (!isTransaction)
                {
                    conn = await GetConnectionAsync().ConfigureAwait(false);
                }

                async Task LocalFunction()
                {
                    while ((DateTime.Now - beginTime) <= timeout)
                    {
                        ret.Clear();
                        try
                        {
                            await using var command = conn.CreateCommand();
                            var table = (Table<E>)Table.Clone();
                            if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                            {
                                table.Schema = anotherDatabaseAliasName;
                            }

                            using var query = new Select().Asterisk().From.Table(table)
                                .Where.KeyEqualToValue(idDic);
                            var sql = query.ToSql();
                            command.CommandText = sql;
                            command.CommandType = CommandType.Text;
                            query.SetParameters(command);

                            LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                            while (await reader.ReadAsync().ConfigureAwait(false))
                            {
                                ret.Add(ToEntity(reader));
                            }

                            return;
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.Contains("database is lock"))
                            {
                                LogManager.GetCurrentClassLogger().Warn("database is lock");
                                continue;
                            }
                            else
                            {
                                LogManager.GetCurrentClassLogger().Error(ex);
                                throw;
                            }
                        }
                    }

                    throw new TimeoutException();
                }

                await LocalFunction();
            }
            finally
            {
                if (!isTransaction)
                {
                    await conn.DisposeAsync().ConfigureAwait(false);
                }
            }

            foreach (var item in ret)
            {
                yield return item;
            }
        }

        public void Update(E entity, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            QueryHelper.KeepTryingUntilProcessSucceed(() =>
            {
                QueryHelper.ForDao.ConnectionInternal(this, new Action<DbConnection>((connection) =>
                {
                    using var command = connection.CreateCommand();
                    var table = (Table<E>)Table.Clone();
                    if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                    {
                        table.Schema = anotherDatabaseAliasName;
                    }

                    using var query = new Update().Table(table).Set.KeyEqualToValue(table.ColumnsWithoutPrimaryKeys.ToDictionary(c => c.ColumnName, c => c.PropertyGetter(entity)))
                        .Where.KeyEqualToValue(table.PrimaryKeyColumns.ToDictionary(c => c.ColumnName, c => c.PropertyGetter(entity)));
                    var sql = query.ToSql();
                    command.CommandText = sql;
                    query.SetParameters(command);

                    LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                    command.ExecuteNonQuery();
                }), conn);
            }, timeout);
        }

        public async Task<int> UpdateAsync(E entity, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            return await QueryHelper.KeepTryingUntilProcessSucceedAsync(async () =>
            {
                return await QueryHelper.ForDao.ConnectionInternalAsync(this, async (connection) =>
                {
                    await using var command = connection.CreateCommand();
                    var table = (Table<E>)Table.Clone();
                    if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                    {
                        table.Schema = anotherDatabaseAliasName;
                    }

                    var a = table.ColumnsWithoutPrimaryKeys.ToDictionary(c => c.ColumnName, c => c.PropertyGetter(entity));
                    var b = table.PrimaryKeyColumns.ToDictionary(c => c.ColumnName, c => c.PropertyGetter(entity));

                    using var query = new Update().Table(table).Set.KeyEqualToValue(table.ColumnsWithoutPrimaryKeys.ToDictionary(c => c.ColumnName, c => c.PropertyGetter(entity)))
                        .Where.KeyEqualToValue(table.PrimaryKeyColumns.ToDictionary(c => c.ColumnName, c => c.PropertyGetter(entity)));
                    var sql = query.ToSql();
                    command.CommandText = sql;
                    query.SetParameters(command);

                    LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                    return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }, conn).ConfigureAwait(false);
            }, timeout).ConfigureAwait(false);
        }

        private static string SqlToDefineColumns(IColumn c)
        {
            var r = $"{c.ColumnName} {c.DBDataType}";
            if (c.Constraints != null && c.Constraints.Any())
            {
                r += $" {c.ConstraintsToSql()}";
            }
            return r;
        }

        private void DefineColumns(ref string sql, IEnumerable<IColumn> columns)
        {
            CheckDelimiter(ref sql);
            sql += "(";
            EnumerateColumnsIntoSql(ref sql, SqlToDefineColumns, ", ", columns);

            //複合主キー
            var primaryKeyConstraintAttributes = typeof(E).GetCustomAttribute<PrimaryKeyAttribute>();
            if (primaryKeyConstraintAttributes != null)
            {
                var primaryKeyConstraint = primaryKeyConstraintAttributes.ToConstraint();
                sql += $", {primaryKeyConstraint.ToSql()}";
            }

            sql += ")";
        }

        private static void CheckDelimiter(ref string sql)
        {
            if (!char.IsWhiteSpace(sql.Last()) && sql.Last().ToString() != DELIMITER_PARENTHESIS)
            {
                sql += DELIMITER_SPACE;
            }
        }

        private static void EnumerateColumnsIntoSql(ref string sql, Func<IColumn, string> content, string connection, IEnumerable<IColumn> columns)
        {
            CheckDelimiter(ref sql);
            Queue<IColumn> queue = new(columns);
            while (queue.Count > 0)
            {
                var column = queue.Dequeue();

                sql += content.Invoke(column);

                if (queue.Count > 0)
                {
                    sql += connection;
                }
            }
        }

        protected void InitializeColumnDefinitions()
        {
            OverridedColumns?.Clear();
        }

        public void UpgradeTable(VersionChangeUnit upgradePath, VersioningMode mode, DbConnection conn = null, TimeSpan? timeout = null)
        {
            QueryHelper.KeepTryingUntilProcessSucceed(() =>
            {
                QueryHelper.ForDao.ConnectionInternal(this, new Action<DbConnection>((connection) =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        Table<E> newTable = new(upgradePath.To);
                        Table<E> oldTable = new(upgradePath.From);

                        using var query = new Insert().Into.Table(newTable)
                            .Columns(newTable.Columns.Select(c => c.ColumnName))
                            .Select.Columns(oldTable.Columns.Select(c => c.ColumnName).Concat(newTable.NewColumns(oldTable, newTable).Select(v => v.WrapOutput()))).From.Table(oldTable);
                        var sql = query.ToSql();
                        command.CommandText = sql;

                        LogManager.GetCurrentClassLogger().Debug($"{sql}");
                        command.ExecuteNonQuery();
                    }

                    if ((mode & VersioningMode.DeleteAllRecordInTableCastedOff) == VersioningMode.DeleteAllRecordInTableCastedOff)
                    {
                        using var command = connection.CreateCommand();
                        var sql = $"delete from {new Table<E>(upgradePath.From).Name}";
                        command.CommandText = sql;
                        LogManager.GetCurrentClassLogger().Debug($"{sql}");
                        command.ExecuteNonQuery();
                    }

                    if ((mode & VersioningMode.DropTableCastedOff) != VersioningMode.DropTableCastedOff) return;

                    {
                        using var command = connection.CreateCommand();
                        var sql = $"drop table {new Table<E>(upgradePath.From).Name}";
                        command.CommandText = sql;
                        LogManager.GetCurrentClassLogger().Debug($"{sql}");
                        command.ExecuteNonQuery();
                    }

                }), conn);
            }, timeout);
        }

        public async Task UpgradeTableAsync(VersionChangeUnit upgradePath, VersioningMode mode, DbConnection conn = null, TimeSpan? timeout = null)
        {
            await QueryHelper.KeepTryingUntilProcessSucceedAsync(async () =>
            {
                await QueryHelper.ForDao.ConnectionInternalAsync(this, async (connection) =>
                {
                    await using (var command = connection.CreateCommand())
                    {
                        Table<E> newTable = new(upgradePath.To);
                        Table<E> oldTable = new(upgradePath.From);

                        using var query = new Insert().Into.Table(newTable)
                            .Columns(newTable.Columns.Select(c => c.ColumnName))
                            .Select.Columns(oldTable.Columns.Select(c => c.ColumnName).Concat(newTable.NewColumns(oldTable, newTable).Select(v => v.WrapOutput()))).From.Table(oldTable);
                        var sql = query.ToSql();
                        command.CommandText = sql;

                        LogManager.GetCurrentClassLogger().Debug($"{sql}");
                        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }

                    if ((mode & VersioningMode.DeleteAllRecordInTableCastedOff) == VersioningMode.DeleteAllRecordInTableCastedOff)
                    {
                        await using var command = connection.CreateCommand();
                        var sql = $"delete from {new Table<E>(upgradePath.From).Name}";
                        command.CommandText = sql;
                        LogManager.GetCurrentClassLogger().Debug($"{sql}");
                        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }

                    if ((mode & VersioningMode.DropTableCastedOff) == VersioningMode.DropTableCastedOff)
                    {
                        await using var command = connection.CreateCommand();
                        var sql = $"drop table {new Table<E>(upgradePath.From).Name}";
                        command.CommandText = sql;
                        LogManager.GetCurrentClassLogger().Debug($"{sql}");
                        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }

                }, conn).ConfigureAwait(false);
            }, timeout).ConfigureAwait(false);
        }

        public T CatchThrow<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch (Exception)
            {
                return default;
            }
        }

        public void AdjustColumns(Type versionFrom, Type versionTo, DbConnection conn = null, TimeSpan? timeout = null)
        {
            QueryHelper.KeepTryingUntilProcessSucceed(() =>
            {
                QueryHelper.ForDao.ConnectionInternal(this, new Action<DbConnection>((connection) =>
                {
                    Table<E> newTable = new(versionTo);
                    Table<E> oldTable = new(versionFrom);
                    using (var command = connection.CreateCommand())
                    {
                        //Toテーブル作成
                        var sql = $"create table {newTable.Name}_To(";
                        foreach (var c in newTable.Columns)
                        {
                            sql += $"{c.ColumnName} {c.DBDataType}";
                            if (!c.Equals(newTable.Columns.Last()))
                            {
                                sql += ", ";
                            }
                        }
                        sql += ")";
                        command.CommandText = sql;
                        LogManager.GetCurrentClassLogger().Debug($"{sql}");
                        command.ExecuteNonQuery();
                    }

                    using (var command = connection.CreateCommand())
                    {
                        //fromテーブルからToテーブルへコピー
                        using (var query = new Insert().Into.Table(new NeutralTable($"{newTable.Name}_To"))
                                                        .Columns(newTable.Columns.Select(c => c.ColumnName))
                                                        .Select.Columns(oldTable.Columns.Select(c => c.ColumnName).Concat(newTable.NewColumns(oldTable, newTable).Select(v => v.WrapOutput()))).From.Table(oldTable))
                        {
                            command.CommandText = query.ToSql();
                            LogManager.GetCurrentClassLogger().Debug($"{query.ToSql()}");
                            command.ExecuteNonQuery();
                        }
                    }

                    using (var command = connection.CreateCommand())
                    {
                        //Drop fromテーブル
                        var sql = $"drop table {oldTable.Name}";
                        command.CommandText = sql;
                        LogManager.GetCurrentClassLogger().Debug($"{sql}");
                        command.ExecuteNonQuery();
                    }

                    using (var command = connection.CreateCommand())
                    {
                        //ToテーブルをリネームしてFromテーブルに
                        var sql = $"alter table {newTable.Name}_To rename to {oldTable.Name}";
                        command.CommandText = sql;
                        LogManager.GetCurrentClassLogger().Debug($"{sql}");
                        command.ExecuteNonQuery();
                    }
                }), conn);
            }, timeout);
        }

        public async Task AdjustColumnsAsync(Type versionFrom, Type versionTo, DbConnection conn = null, TimeSpan? timeout = null)
        {
            await QueryHelper.KeepTryingUntilProcessSucceedAsync(async () =>
            {
                await QueryHelper.ForDao.ConnectionInternalAsync(this, async (connection) =>
                {
                    Table<E> newTable = new(versionTo);
                    Table<E> oldTable = new(versionFrom);
                    await using (var command = connection.CreateCommand())
                    {
                        //Toテーブル作成
                        var sql = $"create table {newTable.Name}_To(";
                        foreach (var c in newTable.Columns)
                        {
                            sql += $"{c.ColumnName} {c.DBDataType}";
                            if (!c.Equals(newTable.Columns.Last()))
                            {
                                sql += ", ";
                            }
                        }
                        sql += ")";
                        command.CommandText = sql;
                        LogManager.GetCurrentClassLogger().Debug($"{sql}");
                        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }

                    await using (var command = connection.CreateCommand())
                    {
                        //fromテーブルからToテーブルへコピー
                        using var query = new Insert().Into.Table(new NeutralTable($"{newTable.Name}_To"))
                            .Columns(newTable.Columns.Select(c => c.ColumnName))
                            .Select.Columns(oldTable.Columns.Select(c => c.ColumnName).Concat(newTable.NewColumns(oldTable, newTable).Select(v => v.WrapOutput()))).From.Table(oldTable);
                        command.CommandText = query.ToSql();
                        LogManager.GetCurrentClassLogger().Debug($"{query.ToSql()}");
                        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }

                    await using (var command = connection.CreateCommand())
                    {
                        //Drop fromテーブル
                        var sql = $"drop table {oldTable.Name}";
                        command.CommandText = sql;
                        LogManager.GetCurrentClassLogger().Debug($"{sql}");
                        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }

                    await using (var command = connection.CreateCommand())
                    {
                        //ToテーブルをリネームしてFromテーブルに
                        var sql = $"alter table {newTable.Name}_To rename to {oldTable.Name}";
                        command.CommandText = sql;
                        LogManager.GetCurrentClassLogger().Debug($"{sql}");
                        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }
                }, conn).ConfigureAwait(false);
            }, timeout).ConfigureAwait(false);
        }
    }
}
