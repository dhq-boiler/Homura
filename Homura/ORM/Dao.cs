
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
                    conn.Dispose();
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

        private DelegateCache _dcache = new DelegateCache();

        protected E ToEntityInDefaultWay(IDataRecord reader, params IColumn[] columns)
        {
            var ret = Dao<E>.CreateInstance();
            const string VALUE_STR = "Value";

            foreach (var column in Columns.Except(columns))
            {
                if (column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty)))
                {
                    _dcache.TryGet<E>(ret.GetType(), column.ColumnName, ret, out var rp);
                    _dcache.TrySet(column.EntityDataType, VALUE_STR, column.EntityDataType, DelegateCache.ConvertTo(column.EntityDataType, rp), CatchThrow(() => GetColumnValue(reader, column, Table)));
                }
                else
                {
                    _dcache.TrySet<E>(ret.GetType(), column.ColumnName, column.EntityDataType, ret, CatchThrow(() => GetColumnValue(reader, column, Table)));
                }
            }

            return ret;
        }

        internal class DelegateCache
        {
            private readonly Dictionary<Type, Dictionary<string, System.Delegate>> _dictionary = new();

            public bool TryGet<TObj>(Type type, string name, TObj? parameter, out object value)
            {
                try
                {
                    value = ((Func<TObj, object>)_dictionary[type][name])(parameter);
                    return true;
                }
                catch (KeyNotFoundException e)
                {
                    if (!_dictionary.ContainsKey(type))
                    {
                        _dictionary[type] = new Dictionary<string, System.Delegate>();
                    }

                    if (!_dictionary[type].ContainsKey(name))
                    {
                        _dictionary[type][name] = GetGetter<TObj, object>(name);
                        value = ((Func<TObj, object>)_dictionary[type][name])(parameter);
                        return true;
                    }
                }

                value = default(object);
                return false;
            }

            public bool TrySet<TObj>(Type type, string name, Type objType, TObj obj, object? parameter)
            {
                try
                {
                    if (objType == typeof(bool))
                    {
                        ((Action<TObj, bool>)_dictionary[type][name])(obj, (bool)parameter);
                    }
                    else if (objType == typeof(bool?))
                    {
                        ((Action<TObj, bool?>)_dictionary[type][name])(obj, (bool?)parameter);
                    }
                    else if (objType == typeof(short))
                    {
                        ((Action<TObj, short>)_dictionary[type][name])(obj, (short)parameter);
                    }
                    else if (objType == typeof(short?))
                    {
                        ((Action<TObj, short?>)_dictionary[type][name])(obj, (short?)parameter);
                    }
                    else if (objType == typeof(int))
                    {
                        ((Action<TObj, int>)_dictionary[type][name])(obj, (int)parameter);
                    }
                    else if (objType == typeof(int?))
                    {
                        ((Action<TObj, int?>)_dictionary[type][name])(obj, (int?)parameter);
                    }
                    else if (objType == typeof(long))
                    {
                        ((Action<TObj, long>)_dictionary[type][name])(obj, (long)parameter);
                    }
                    else if (objType == typeof(long?))
                    {
                        ((Action<TObj, long?>)_dictionary[type][name])(obj, (long?)parameter);
                    }
                    else if (objType == typeof(ushort))
                    {
                        ((Action<TObj, ushort>)_dictionary[type][name])(obj, (ushort)parameter);
                    }
                    else if (objType == typeof(ushort?))
                    {
                        ((Action<TObj, ushort?>)_dictionary[type][name])(obj, (ushort?)parameter);
                    }
                    else if (objType == typeof(uint))
                    {
                        ((Action<TObj, uint>)_dictionary[type][name])(obj, (uint)parameter);
                    }
                    else if (objType == typeof(uint?))
                    {
                        ((Action<TObj, uint?>)_dictionary[type][name])(obj, (uint?)parameter);
                    }
                    else if (objType == typeof(ulong))
                    {
                        ((Action<TObj, ulong>)_dictionary[type][name])(obj, (ulong)parameter);
                    }
                    else if (objType == typeof(ulong?))
                    {
                        ((Action<TObj, ulong?>)_dictionary[type][name])(obj, (ulong?)parameter);
                    }
                    else if (objType == typeof(float))
                    {
                        ((Action<TObj, float>)_dictionary[type][name])(obj, (float)parameter);
                    }
                    else if (objType == typeof(float?))
                    {
                        ((Action<TObj, float?>)_dictionary[type][name])(obj, (float?)parameter);
                    }
                    else if (objType == typeof(double))
                    {
                        ((Action<TObj, double>)_dictionary[type][name])(obj, (double)parameter);
                    }
                    else if (objType == typeof(double?))
                    {
                        ((Action<TObj, double?>)_dictionary[type][name])(obj, (double?)parameter);
                    }
                    else if (objType == typeof(string))
                    {
                        ((Action<TObj, string>)_dictionary[type][name])(obj, (string)parameter);
                    }
                    else if (objType == typeof(DateTime))
                    {
                        ((Action<TObj, DateTime>)_dictionary[type][name])(obj, (DateTime)parameter);
                    }
                    else if (objType == typeof(DateTime?))
                    {
                        ((Action<TObj, DateTime?>)_dictionary[type][name])(obj, (DateTime?)parameter);
                    }
                    else if (objType == typeof(Guid))
                    {
                        ((Action<TObj, Guid>)_dictionary[type][name])(obj, (Guid)parameter);
                    }
                    else if (objType == typeof(Guid?))
                    {
                        ((Action<TObj, Guid?>)_dictionary[type][name])(obj, (Guid?)parameter);
                    }
                    else if (objType == typeof(Type))
                    {
                        ((Action<TObj, Type>)_dictionary[type][name])(obj, (Type)parameter);
                    }
                    else if (objType == typeof(object))
                    {
                        ((Action<TObj, object>)_dictionary[type][name])(obj, (object)parameter);
                    }

                    else if (objType == typeof(ReactivePropertySlim<bool>))
                    {
                        ((Action<ReactivePropertySlim<bool>, bool>)_dictionary[type][name])(obj as ReactivePropertySlim<bool>, (bool)parameter);
                    }
                    else if (objType == typeof(ReactivePropertySlim<bool?>))
                    {
                        ((Action<ReactivePropertySlim<bool?>, bool?>)_dictionary[type][name])(obj as ReactivePropertySlim<bool?>, (bool?)parameter);
                    }
                    else if (objType == typeof(ReactivePropertySlim<short>))
                    {
                        ((Action<ReactivePropertySlim<short>, short>)_dictionary[type][name])(obj as ReactivePropertySlim<short>, (short)parameter);
                    }
                    else if (objType == typeof(ReactivePropertySlim<short?>))
                    {
                        ((Action<ReactivePropertySlim<short?>, short?>)_dictionary[type][name])(obj as ReactivePropertySlim<short?>, (short?)parameter);
                    }
                    else if (objType == typeof(ReactivePropertySlim<int>))
                    {
                        ((Action<ReactivePropertySlim<int>, int>)_dictionary[type][name])(obj as ReactivePropertySlim<int>, (int)parameter);
                    }
                    else if (objType == typeof(ReactivePropertySlim<int?>))
                    {
                        ((Action<ReactivePropertySlim<int?>, int?>)_dictionary[type][name])(obj as ReactivePropertySlim<int?>, (int?)parameter);
                    }
                    else if (objType == typeof(ReactivePropertySlim<long>))
                    {
                        ((Action<ReactivePropertySlim<long>, long>)_dictionary[type][name])(obj as ReactivePropertySlim<long>, (long)parameter);
                    }
                    else if (objType == typeof(ReactivePropertySlim<long?>))
                    {
                        ((Action<ReactivePropertySlim<long?>, long?>)_dictionary[type][name])(obj as ReactivePropertySlim<long?>, (long?)parameter);
                    }
                    else if (objType == typeof(ReactivePropertySlim<ushort>))
                    {
                        ((Action<ReactivePropertySlim<ushort>, ushort>)_dictionary[type][name])(obj as ReactivePropertySlim<ushort>, (ushort)parameter);
                    }
                    else if (objType == typeof(ReactivePropertySlim<ushort?>))
                    {
                        ((Action<ReactivePropertySlim<ushort?>, ushort?>)_dictionary[type][name])(obj as ReactivePropertySlim<ushort?>, (ushort?)parameter);
                    }
                    else if (objType == typeof(ReactivePropertySlim<uint>))
                    {
                        ((Action<ReactivePropertySlim<uint>, uint>)_dictionary[type][name])(obj as ReactivePropertySlim<uint>, (uint)parameter);
                    }
                    else if (objType == typeof(ReactivePropertySlim<uint?>))
                    {
                        ((Action<ReactivePropertySlim<uint?>, uint?>)_dictionary[type][name])(obj as ReactivePropertySlim<uint?>, (uint?)parameter);
                    }
                    else if (objType == typeof(ReactivePropertySlim<ulong>))
                    {
                        ((Action<ReactivePropertySlim<ulong>, ulong>)_dictionary[type][name])(obj as ReactivePropertySlim<ulong>, (ulong)parameter);
                    }
                    else if (objType == typeof(ReactivePropertySlim<ulong?>))
                    {
                        ((Action<ReactivePropertySlim<ulong?>, ulong?>)_dictionary[type][name])(obj as ReactivePropertySlim<ulong?>, (ulong?)parameter);
                    }
                    else if (objType == typeof(ReactivePropertySlim<float>))
                    {
                        ((Action<ReactivePropertySlim<float>, float>)_dictionary[type][name])(obj as ReactivePropertySlim<float>, (float)parameter);
                    }
                    else if (objType == typeof(ReactivePropertySlim<float?>))
                    {
                        ((Action<ReactivePropertySlim<float?>, float?>)_dictionary[type][name])(obj as ReactivePropertySlim<float?>, (float?)parameter);
                    }
                    else if (objType == typeof(ReactivePropertySlim<double>))
                    {
                        ((Action<ReactivePropertySlim<double>, double>)_dictionary[type][name])(obj as ReactivePropertySlim<double>, (double)parameter);
                    }
                    else if (objType == typeof(ReactivePropertySlim<double?>))
                    {
                        ((Action<ReactivePropertySlim<double?>, double?>)_dictionary[type][name])(obj as ReactivePropertySlim<double?>, (double?)parameter);
                    }
                    else if (objType == typeof(ReactivePropertySlim<string>))
                    {
                        ((Action<ReactivePropertySlim<string>, string>)_dictionary[type][name])(obj as ReactivePropertySlim<string>, (string)parameter);
                    }
                    else if (objType == typeof(ReactivePropertySlim<DateTime>))
                    {
                        ((Action<ReactivePropertySlim<DateTime>, DateTime>)_dictionary[type][name])(obj as ReactivePropertySlim<DateTime>, (DateTime)parameter);
                    }
                    else if (objType == typeof(ReactivePropertySlim<DateTime?>))
                    {
                        ((Action<ReactivePropertySlim<DateTime?>, DateTime?>)_dictionary[type][name])(obj as ReactivePropertySlim<DateTime?>, (DateTime?)parameter);
                    }
                    else if (objType == typeof(ReactivePropertySlim<Guid>))
                    {
                        ((Action<ReactivePropertySlim<Guid>, Guid>)_dictionary[type][name])(obj as ReactivePropertySlim<Guid>, (Guid)parameter);
                    }
                    else if (objType == typeof(ReactivePropertySlim<Guid?>))
                    {
                        ((Action<ReactivePropertySlim<Guid?>, Guid?>)_dictionary[type][name])(obj as ReactivePropertySlim<Guid?>, (Guid?)parameter);
                    }
                    else if (objType == typeof(ReactivePropertySlim<Type>))
                    {
                        ((Action<ReactivePropertySlim<Type>, Type>)_dictionary[type][name])(obj as ReactivePropertySlim<Type>, (Type)parameter);
                    }
                    else if (objType == typeof(ReactivePropertySlim<object>))
                    {
                        ((Action<ReactivePropertySlim<object>, object>)_dictionary[type][name])(obj as ReactivePropertySlim<object>, (object)parameter);
                    }

                    else if (objType == typeof(ReactiveProperty<short>))
                    {
                        ((Action<ReactiveProperty<short>, short>)_dictionary[type][name])(obj as ReactiveProperty<short>, (short)parameter);
                    }
                    else if (objType == typeof(ReactiveProperty<short?>))
                    {
                        ((Action<ReactiveProperty<short?>, short?>)_dictionary[type][name])(obj as ReactiveProperty<short?>, (short?)parameter);
                    }
                    else if (objType == typeof(ReactiveProperty<int>))
                    {
                        ((Action<ReactiveProperty<int>, int>)_dictionary[type][name])(obj as ReactiveProperty<int>, (int)parameter);
                    }
                    else if (objType == typeof(ReactiveProperty<int?>))
                    {
                        ((Action<ReactiveProperty<int?>, int?>)_dictionary[type][name])(obj as ReactiveProperty<int?>, (int?)parameter);
                    }
                    else if (objType == typeof(ReactiveProperty<long>))
                    {
                        ((Action<ReactiveProperty<long>, long>)_dictionary[type][name])(obj as ReactiveProperty<long>, (long)parameter);
                    }
                    else if (objType == typeof(ReactiveProperty<long?>))
                    {
                        ((Action<ReactiveProperty<long?>, long?>)_dictionary[type][name])(obj as ReactiveProperty<long?>, (long?)parameter);
                    }
                    else if (objType == typeof(ReactiveProperty<ushort>))
                    {
                        ((Action<ReactiveProperty<ushort>, ushort>)_dictionary[type][name])(obj as ReactiveProperty<ushort>, (ushort)parameter);
                    }
                    else if (objType == typeof(ReactiveProperty<ushort?>))
                    {
                        ((Action<ReactiveProperty<ushort?>, ushort?>)_dictionary[type][name])(obj as ReactiveProperty<ushort?>, (ushort?)parameter);
                    }
                    else if (objType == typeof(ReactiveProperty<uint>))
                    {
                        ((Action<ReactiveProperty<uint>, uint>)_dictionary[type][name])(obj as ReactiveProperty<uint>, (uint)parameter);
                    }
                    else if (objType == typeof(ReactiveProperty<uint?>))
                    {
                        ((Action<ReactiveProperty<uint?>, uint?>)_dictionary[type][name])(obj as ReactiveProperty<uint?>, (uint?)parameter);
                    }
                    else if (objType == typeof(ReactiveProperty<ulong>))
                    {
                        ((Action<ReactiveProperty<ulong>, ulong>)_dictionary[type][name])(obj as ReactiveProperty<ulong>, (ulong)parameter);
                    }
                    else if (objType == typeof(ReactiveProperty<ulong?>))
                    {
                        ((Action<ReactiveProperty<ulong?>, ulong?>)_dictionary[type][name])(obj as ReactiveProperty<ulong?>, (ulong?)parameter);
                    }
                    else if (objType == typeof(ReactiveProperty<float>))
                    {
                        ((Action<ReactiveProperty<float>, float>)_dictionary[type][name])(obj as ReactiveProperty<float>, (float)parameter);
                    }
                    else if (objType == typeof(ReactiveProperty<float?>))
                    {
                        ((Action<ReactiveProperty<float?>, float?>)_dictionary[type][name])(obj as ReactiveProperty<float?>, (float?)parameter);
                    }
                    else if (objType == typeof(ReactiveProperty<double>))
                    {
                        ((Action<ReactiveProperty<double>, double>)_dictionary[type][name])(obj as ReactiveProperty<double>, (double)parameter);
                    }
                    else if (objType == typeof(ReactiveProperty<double?>))
                    {
                        ((Action<ReactiveProperty<double?>, double?>)_dictionary[type][name])(obj as ReactiveProperty<double?>, (double?)parameter);
                    }
                    else if (objType == typeof(ReactiveProperty<string>))
                    {
                        ((Action<ReactiveProperty<string>, string>)_dictionary[type][name])(obj as ReactiveProperty<string>, (string)parameter);
                    }
                    else if (objType == typeof(ReactiveProperty<DateTime>))
                    {
                        ((Action<ReactiveProperty<DateTime>, DateTime>)_dictionary[type][name])(obj as ReactiveProperty<DateTime>, (DateTime)parameter);
                    }
                    else if (objType == typeof(ReactiveProperty<DateTime?>))
                    {
                        ((Action<ReactiveProperty<DateTime?>, DateTime?>)_dictionary[type][name])(obj as ReactiveProperty<DateTime?>, (DateTime?)parameter);
                    }
                    else if (objType == typeof(ReactiveProperty<Guid>))
                    {
                        ((Action<ReactiveProperty<Guid>, Guid>)_dictionary[type][name])(obj as ReactiveProperty<Guid>, (Guid)parameter);
                    }
                    else if (objType == typeof(ReactiveProperty<Guid?>))
                    {
                        ((Action<ReactiveProperty<Guid?>, Guid?>)_dictionary[type][name])(obj as ReactiveProperty<Guid?>, (Guid?)parameter);
                    }
                    else if (objType == typeof(ReactiveProperty<Type>))
                    {
                        ((Action<ReactiveProperty<Type>, Type>)_dictionary[type][name])(obj as ReactiveProperty<Type>, (Type)parameter);
                    }
                    else if (objType == typeof(ReactiveProperty<object>))
                    {
                        ((Action<ReactiveProperty<object>, object>)_dictionary[type][name])(obj as ReactiveProperty<object>, (object)parameter);
                    }

                    return true;
                }
                catch (KeyNotFoundException e)
                {
                    if (!_dictionary.ContainsKey(type))
                    {
                        _dictionary[type] = new Dictionary<string, System.Delegate>();
                    }

                    if (!_dictionary[type].ContainsKey(name))
                    {
                        if (objType == typeof(bool))
                        {
                            _dictionary[type][name] = GetSetter<TObj, bool>(obj.GetType(), name);
                            ((Action<TObj, bool>)_dictionary[type][name])(obj, (bool)parameter);
                        }
                        else if (objType == typeof(bool?))
                        {
                            _dictionary[type][name] = GetSetter<TObj, bool?>(obj.GetType(), name);
                            ((Action<TObj, bool?>)_dictionary[type][name])(obj, (bool?)parameter);
                        }
                        else if (objType == typeof(short))
                        {
                            _dictionary[type][name] = GetSetter<TObj, short>(obj.GetType(), name);
                            ((Action<TObj, short>)_dictionary[type][name])(obj, (short)parameter);
                        }
                        else if (objType == typeof(short?))
                        {
                            _dictionary[type][name] = GetSetter<TObj, short?>(obj.GetType(), name);
                            ((Action<TObj, short?>)_dictionary[type][name])(obj, (short?)parameter);
                        }
                        else if (objType == typeof(int))
                        {
                            _dictionary[type][name] = GetSetter<TObj, int>(obj.GetType(), name);
                            ((Action<TObj, int>)_dictionary[type][name])(obj, (int)parameter);
                        }
                        else if (objType == typeof(int?))
                        {
                            _dictionary[type][name] = GetSetter<TObj, int?>(obj.GetType(), name);
                            ((Action<TObj, int?>)_dictionary[type][name])(obj, (int?)parameter);
                        }
                        else if (objType == typeof(long))
                        {
                            _dictionary[type][name] = GetSetter<TObj, long>(obj.GetType(), name);
                            ((Action<TObj, long>)_dictionary[type][name])(obj, (long)parameter);
                        }
                        else if (objType == typeof(long?))
                        {
                            _dictionary[type][name] = GetSetter<TObj, long?>(obj.GetType(), name);
                            ((Action<TObj, long?>)_dictionary[type][name])(obj, (long?)parameter);
                        }
                        else if (objType == typeof(ushort))
                        {
                            _dictionary[type][name] = GetSetter<TObj, ushort>(obj.GetType(), name);
                            ((Action<TObj, ushort>)_dictionary[type][name])(obj, (ushort)parameter);
                        }
                        else if (objType == typeof(ushort?))
                        {
                            _dictionary[type][name] = GetSetter<TObj, ushort?>(obj.GetType(), name);
                            ((Action<TObj, ushort?>)_dictionary[type][name])(obj, (ushort?)parameter);
                        }
                        else if (objType == typeof(uint))
                        {
                            _dictionary[type][name] = GetSetter<TObj, uint>(obj.GetType(), name);
                            ((Action<TObj, uint>)_dictionary[type][name])(obj, (uint)parameter);
                        }
                        else if (objType == typeof(uint?))
                        {
                            _dictionary[type][name] = GetSetter<TObj, uint?>(obj.GetType(), name);
                            ((Action<TObj, uint?>)_dictionary[type][name])(obj, (uint?)parameter);
                        }
                        else if (objType == typeof(ulong))
                        {
                            _dictionary[type][name] = GetSetter<TObj, ulong>(obj.GetType(), name);
                            ((Action<TObj, ulong>)_dictionary[type][name])(obj, (ulong)parameter);
                        }
                        else if (objType == typeof(ulong?))
                        {
                            _dictionary[type][name] = GetSetter<TObj, ulong?>(obj.GetType(), name);
                            ((Action<TObj, ulong?>)_dictionary[type][name])(obj, (ulong?)parameter);
                        }
                        else if (objType == typeof(float))
                        {
                            _dictionary[type][name] = GetSetter<TObj, float>(obj.GetType(), name);
                            ((Action<TObj, float>)_dictionary[type][name])(obj, (float)parameter);
                        }
                        else if (objType == typeof(float?))
                        {
                            _dictionary[type][name] = GetSetter<TObj, float?>(obj.GetType(), name);
                            ((Action<TObj, float?>)_dictionary[type][name])(obj, (float?)parameter);
                        }
                        else if (objType == typeof(double))
                        {
                            _dictionary[type][name] = GetSetter<TObj, double>(obj.GetType(), name);
                            ((Action<TObj, double>)_dictionary[type][name])(obj, (double)parameter);
                        }
                        else if (objType == typeof(double?))
                        {
                            _dictionary[type][name] = GetSetter<TObj, double?>(obj.GetType(), name);
                            ((Action<TObj, double?>)_dictionary[type][name])(obj, (double?)parameter);
                        }
                        else if (objType == typeof(string))
                        {
                            _dictionary[type][name] = GetSetter<TObj, string>(obj.GetType(), name);
                            ((Action<TObj, string>)_dictionary[type][name])(obj, (string)parameter);
                        }
                        else if (objType == typeof(DateTime))
                        {
                            _dictionary[type][name] = GetSetter<TObj, DateTime>(obj.GetType(), name);
                            ((Action<TObj, DateTime>)_dictionary[type][name])(obj, (DateTime)parameter);
                        }
                        else if (objType == typeof(DateTime?))
                        {
                            _dictionary[type][name] = GetSetter<TObj, DateTime?>(obj.GetType(), name);
                            ((Action<TObj, DateTime?>)_dictionary[type][name])(obj, (DateTime?)parameter);
                        }
                        else if (objType == typeof(Guid))
                        {
                            _dictionary[type][name] = GetSetter<TObj, Guid>(obj.GetType(), name);
                            ((Action<TObj, Guid>)_dictionary[type][name])(obj, (Guid)parameter);
                        }
                        else if (objType == typeof(Guid?))
                        {
                            _dictionary[type][name] = GetSetter<TObj, Guid?>(obj.GetType(), name);
                            ((Action<TObj, Guid?>)_dictionary[type][name])(obj, (Guid?)parameter);
                        }
                        else if (objType == typeof(Type))
                        {
                            _dictionary[type][name] = GetSetter<TObj, Type>(obj.GetType(), name);
                            ((Action<TObj, Type>)_dictionary[type][name])(obj, (Type)parameter);
                        }
                        else if (objType == typeof(object))
                        {
                            _dictionary[type][name] = GetSetter<TObj, object>(obj.GetType(), name);
                            ((Action<TObj, object>)_dictionary[type][name])(obj, (object)parameter);
                        }
                        else if (objType == typeof(ReactivePropertySlim<bool>))
                        {
                            _dictionary[type][name] = GetSetter<ReactivePropertySlim<bool>, bool>(obj.GetType(), name);
                            ((Action<ReactivePropertySlim<bool>, bool>)_dictionary[type][name])(obj as ReactivePropertySlim<bool>, (bool)parameter);
                        }
                        else if (objType == typeof(ReactivePropertySlim<bool?>))
                        {
                            _dictionary[type][name] = GetSetter<ReactivePropertySlim<bool?>, bool?>(obj.GetType(), name);
                            ((Action<ReactivePropertySlim<bool?>, bool?>)_dictionary[type][name])(obj as ReactivePropertySlim<bool?>, (bool?)parameter);
                        }
                        else if (objType == typeof(ReactivePropertySlim<short>))
                        {
                            _dictionary[type][name] = GetSetter<ReactivePropertySlim<short>, short>(obj.GetType(), name);
                            ((Action<ReactivePropertySlim<short>, short>)_dictionary[type][name])(obj as ReactivePropertySlim<short>, (short)parameter);
                        }
                        else if (objType == typeof(ReactivePropertySlim<short?>))
                        {
                            _dictionary[type][name] = GetSetter<ReactivePropertySlim<short?>, short?>(obj.GetType(), name);
                            ((Action<ReactivePropertySlim<short?>, short?>)_dictionary[type][name])(obj as ReactivePropertySlim<short?>, (short?)parameter);
                        }
                        else if (objType == typeof(ReactivePropertySlim<int>))
                        {
                            _dictionary[type][name] = GetSetter<ReactivePropertySlim<int>, int>(obj.GetType(), name);
                            ((Action<ReactivePropertySlim<int>, int>)_dictionary[type][name])(obj as ReactivePropertySlim<int>, (int)parameter);
                        }
                        else if (objType == typeof(ReactivePropertySlim<int?>))
                        {
                            _dictionary[type][name] = GetSetter<ReactivePropertySlim<int?>, int?>(obj.GetType(), name);
                            ((Action<ReactivePropertySlim<int?>, int?>)_dictionary[type][name])(obj as ReactivePropertySlim<int?>, (int?)parameter);
                        }
                        else if (objType == typeof(ReactivePropertySlim<long>))
                        {
                            _dictionary[type][name] = GetSetter<ReactivePropertySlim<long>, long>(obj.GetType(), name);
                            ((Action<ReactivePropertySlim<long>, long>)_dictionary[type][name])(obj as ReactivePropertySlim<long>, (long)parameter);
                        }
                        else if (objType == typeof(ReactivePropertySlim<long?>))
                        {
                            _dictionary[type][name] = GetSetter<ReactivePropertySlim<long?>, long?>(obj.GetType(), name);
                            ((Action<ReactivePropertySlim<long?>, long?>)_dictionary[type][name])(obj as ReactivePropertySlim<long?>, (long?)parameter);
                        }
                        else if (objType == typeof(ReactivePropertySlim<ushort>))
                        {
                            _dictionary[type][name] = GetSetter<ReactivePropertySlim<ushort>, ushort>(obj.GetType(), name);
                            ((Action<ReactivePropertySlim<ushort>, ushort>)_dictionary[type][name])(obj as ReactivePropertySlim<ushort>, (ushort)parameter);
                        }
                        else if (objType == typeof(ReactivePropertySlim<ushort?>))
                        {
                            _dictionary[type][name] = GetSetter<ReactivePropertySlim<ushort?>, ushort?>(obj.GetType(), name);
                            ((Action<ReactivePropertySlim<ushort?>, ushort?>)_dictionary[type][name])(obj as ReactivePropertySlim<ushort?>, (ushort?)parameter);
                        }
                        else if (objType == typeof(ReactivePropertySlim<uint>))
                        {
                            _dictionary[type][name] = GetSetter<ReactivePropertySlim<uint>, uint>(obj.GetType(), name);
                            ((Action<ReactivePropertySlim<uint>, uint>)_dictionary[type][name])(obj as ReactivePropertySlim<uint>, (uint)parameter);
                        }
                        else if (objType == typeof(ReactivePropertySlim<uint?>))
                        {
                            _dictionary[type][name] = GetSetter<ReactivePropertySlim<uint?>, uint?>(obj.GetType(), name);
                            ((Action<ReactivePropertySlim<uint?>, uint?>)_dictionary[type][name])(obj as ReactivePropertySlim<uint?>, (uint?)parameter);
                        }
                        else if (objType == typeof(ReactivePropertySlim<ulong>))
                        {
                            _dictionary[type][name] = GetSetter<ReactivePropertySlim<ulong>, ulong>(obj.GetType(), name);
                            ((Action<ReactivePropertySlim<ulong>, ulong>)_dictionary[type][name])(obj as ReactivePropertySlim<ulong>, (ulong)parameter);
                        }
                        else if (objType == typeof(ReactivePropertySlim<ulong?>))
                        {
                            _dictionary[type][name] = GetSetter<ReactivePropertySlim<ulong?>, ulong?>(obj.GetType(), name);
                            ((Action<ReactivePropertySlim<ulong?>, ulong?>)_dictionary[type][name])(obj as ReactivePropertySlim<ulong?>, (ulong?)parameter);
                        }
                        else if (objType == typeof(ReactivePropertySlim<float>))
                        {
                            _dictionary[type][name] = GetSetter<ReactivePropertySlim<float>, float>(obj.GetType(), name);
                            ((Action<ReactivePropertySlim<float>, float>)_dictionary[type][name])(obj as ReactivePropertySlim<float>, (float)parameter);
                        }
                        else if (objType == typeof(ReactivePropertySlim<float?>))
                        {
                            _dictionary[type][name] = GetSetter<ReactivePropertySlim<float?>, float?>(obj.GetType(), name);
                            ((Action<ReactivePropertySlim<float?>, float?>)_dictionary[type][name])(obj as ReactivePropertySlim<float?>, (float?)parameter);
                        }
                        else if (objType == typeof(ReactivePropertySlim<double>))
                        {
                            _dictionary[type][name] = GetSetter<ReactivePropertySlim<double>, double>(obj.GetType(), name);
                            ((Action<ReactivePropertySlim<double>, double>)_dictionary[type][name])(obj as ReactivePropertySlim<double>, (double)parameter);
                        }
                        else if (objType == typeof(ReactivePropertySlim<double?>))
                        {
                            _dictionary[type][name] = GetSetter<ReactivePropertySlim<double?>, double?>(obj.GetType(), name);
                            ((Action<ReactivePropertySlim<double?>, double?>)_dictionary[type][name])(obj as ReactivePropertySlim<double?>, (double?)parameter);
                        }
                        else if (objType == typeof(ReactivePropertySlim<string>))
                        {
                            _dictionary[type][name] = GetSetter<ReactivePropertySlim<string>, string>(obj.GetType(), name);
                            ((Action<ReactivePropertySlim<string>, string>)_dictionary[type][name])(obj as ReactivePropertySlim<string>, (string)parameter);
                        }
                        else if (objType == typeof(ReactivePropertySlim<DateTime>))
                        {
                            _dictionary[type][name] = GetSetter<ReactivePropertySlim<DateTime>, DateTime>(obj.GetType(), name);
                            ((Action<ReactivePropertySlim<DateTime>, DateTime>)_dictionary[type][name])(obj as ReactivePropertySlim<DateTime>, (DateTime)parameter);
                        }
                        else if (objType == typeof(ReactivePropertySlim<DateTime?>))
                        {
                            _dictionary[type][name] = GetSetter<ReactivePropertySlim<DateTime?>, DateTime?>(obj.GetType(), name);
                            ((Action<ReactivePropertySlim<DateTime?>, DateTime?>)_dictionary[type][name])(obj as ReactivePropertySlim<DateTime?>, (DateTime?)parameter);
                        }
                        else if (objType == typeof(ReactivePropertySlim<Guid>))
                        {
                            _dictionary[type][name] = GetSetter<ReactivePropertySlim<Guid>, Guid>(obj.GetType(), name);
                            ((Action<ReactivePropertySlim<Guid>, Guid>)_dictionary[type][name])(obj as ReactivePropertySlim<Guid>, (Guid)parameter);
                        }
                        else if (objType == typeof(ReactivePropertySlim<Guid?>))
                        {
                            _dictionary[type][name] = GetSetter<ReactivePropertySlim<Guid?>, Guid?>(obj.GetType(), name);
                            ((Action<ReactivePropertySlim<Guid?>, Guid?>)_dictionary[type][name])(obj as ReactivePropertySlim<Guid?>, (Guid?)parameter);
                        }
                        else if (objType == typeof(ReactivePropertySlim<Type>))
                        {
                            _dictionary[type][name] = GetSetter<ReactivePropertySlim<Type>, Type>(obj.GetType(), name);
                            ((Action<ReactivePropertySlim<Type>, Type>)_dictionary[type][name])(obj as ReactivePropertySlim<Type>, (Type)parameter);
                        }
                        else if (objType == typeof(ReactivePropertySlim<object>))
                        {
                            _dictionary[type][name] = GetSetter<ReactivePropertySlim<object>, object>(obj.GetType(), name);
                            ((Action<ReactivePropertySlim<object>, object>)_dictionary[type][name])(obj as ReactivePropertySlim<object>, (object)parameter);
                        }
                        else if (objType == typeof(ReactiveProperty<bool>))
                        {
                            _dictionary[type][name] = GetSetter<ReactiveProperty<bool>, bool>(obj.GetType(), name);
                            ((Action<ReactiveProperty<bool>, bool>)_dictionary[type][name])(obj as ReactiveProperty<bool>, (bool)parameter);
                        }
                        else if (objType == typeof(ReactiveProperty<bool?>))
                        {
                            _dictionary[type][name] = GetSetter<ReactiveProperty<bool?>, bool?>(obj.GetType(), name);
                            ((Action<ReactiveProperty<bool?>, bool?>)_dictionary[type][name])(obj as ReactiveProperty<bool?>, (bool?)parameter);
                        }
                        else if (objType == typeof(ReactiveProperty<short>))
                        {
                            _dictionary[type][name] = GetSetter<ReactiveProperty<short>, short>(obj.GetType(), name);
                            ((Action<ReactiveProperty<short>, short>)_dictionary[type][name])(obj as ReactiveProperty<short>, (short)parameter);
                        }
                        else if (objType == typeof(ReactiveProperty<short?>))
                        {
                            _dictionary[type][name] = GetSetter<ReactiveProperty<short?>, short?>(obj.GetType(), name);
                            ((Action<ReactiveProperty<short?>, short?>)_dictionary[type][name])(obj as ReactiveProperty<short?>, (short?)parameter);
                        }
                        else if (objType == typeof(ReactiveProperty<int>))
                        {
                            _dictionary[type][name] = GetSetter<ReactiveProperty<int>, int>(obj.GetType(), name);
                            ((Action<ReactiveProperty<int>, int>)_dictionary[type][name])(obj as ReactiveProperty<int>, (int)parameter);
                        }
                        else if (objType == typeof(ReactiveProperty<int?>))
                        {
                            _dictionary[type][name] = GetSetter<ReactiveProperty<int?>, int?>(obj.GetType(), name);
                            ((Action<ReactiveProperty<int?>, int?>)_dictionary[type][name])(obj as ReactiveProperty<int?>, (int?)parameter);
                        }
                        else if (objType == typeof(ReactiveProperty<long>))
                        {
                            _dictionary[type][name] = GetSetter<ReactiveProperty<long>, long>(obj.GetType(), name);
                            ((Action<ReactiveProperty<long>, long>)_dictionary[type][name])(obj as ReactiveProperty<long>, (long)parameter);
                        }
                        else if (objType == typeof(ReactiveProperty<long?>))
                        {
                            _dictionary[type][name] = GetSetter<ReactiveProperty<long?>, long?>(obj.GetType(), name);
                            ((Action<ReactiveProperty<long?>, long?>)_dictionary[type][name])(obj as ReactiveProperty<long?>, (long?)parameter);
                        }
                        else if (objType == typeof(ReactiveProperty<ushort>))
                        {
                            _dictionary[type][name] = GetSetter<ReactiveProperty<ushort>, ushort>(obj.GetType(), name);
                            ((Action<ReactiveProperty<ushort>, ushort>)_dictionary[type][name])(obj as ReactiveProperty<ushort>, (ushort)parameter);
                        }
                        else if (objType == typeof(ReactiveProperty<ushort?>))
                        {
                            _dictionary[type][name] = GetSetter<ReactiveProperty<ushort?>, ushort?>(obj.GetType(), name);
                            ((Action<ReactiveProperty<ushort?>, ushort?>)_dictionary[type][name])(obj as ReactiveProperty<ushort?>, (ushort?)parameter);
                        }
                        else if (objType == typeof(ReactiveProperty<uint>))
                        {
                            _dictionary[type][name] = GetSetter<ReactiveProperty<uint>, uint>(obj.GetType(), name);
                            ((Action<ReactiveProperty<uint>, uint>)_dictionary[type][name])(obj as ReactiveProperty<uint>, (uint)parameter);
                        }
                        else if (objType == typeof(ReactiveProperty<uint?>))
                        {
                            _dictionary[type][name] = GetSetter<ReactiveProperty<uint?>, uint?>(obj.GetType(), name);
                            ((Action<ReactiveProperty<uint?>, uint?>)_dictionary[type][name])(obj as ReactiveProperty<uint?>, (uint?)parameter);
                        }
                        else if (objType == typeof(ReactiveProperty<ulong>))
                        {
                            _dictionary[type][name] = GetSetter<ReactiveProperty<ulong>, ulong>(obj.GetType(), name);
                            ((Action<ReactiveProperty<ulong>, ulong>)_dictionary[type][name])(obj as ReactiveProperty<ulong>, (ulong)parameter);
                        }
                        else if (objType == typeof(ReactiveProperty<ulong?>))
                        {
                            _dictionary[type][name] = GetSetter<ReactiveProperty<ulong?>, ulong?>(obj.GetType(), name);
                            ((Action<ReactiveProperty<ulong?>, ulong?>)_dictionary[type][name])(obj as ReactiveProperty<ulong?>, (ulong?)parameter);
                        }
                        else if (objType == typeof(ReactiveProperty<float>))
                        {
                            _dictionary[type][name] = GetSetter<ReactiveProperty<float>, float>(obj.GetType(), name);
                            ((Action<ReactiveProperty<float>, float>)_dictionary[type][name])(obj as ReactiveProperty<float>, (float)parameter);
                        }
                        else if (objType == typeof(ReactiveProperty<float?>))
                        {
                            _dictionary[type][name] = GetSetter<ReactiveProperty<float?>, float?>(obj.GetType(), name);
                            ((Action<ReactiveProperty<float?>, float?>)_dictionary[type][name])(obj as ReactiveProperty<float?>, (float?)parameter);
                        }
                        else if (objType == typeof(ReactiveProperty<double>))
                        {
                            _dictionary[type][name] = GetSetter<ReactiveProperty<double>, double>(obj.GetType(), name);
                            ((Action<ReactiveProperty<double>, double>)_dictionary[type][name])(obj as ReactiveProperty<double>, (double)parameter);
                        }
                        else if (objType == typeof(ReactiveProperty<double?>))
                        {
                            _dictionary[type][name] = GetSetter<ReactiveProperty<double?>, double?>(obj.GetType(), name);
                            ((Action<ReactiveProperty<double?>, double?>)_dictionary[type][name])(obj as ReactiveProperty<double?>, (double?)parameter);
                        }
                        else if (objType == typeof(ReactiveProperty<string>))
                        {
                            _dictionary[type][name] = GetSetter<ReactiveProperty<string>, string>(obj.GetType(), name);
                            ((Action<ReactiveProperty<string>, string>)_dictionary[type][name])(obj as ReactiveProperty<string>, (string)parameter);
                        }
                        else if (objType == typeof(ReactiveProperty<DateTime>))
                        {
                            _dictionary[type][name] = GetSetter<ReactiveProperty<DateTime>, DateTime>(obj.GetType(), name);
                            ((Action<ReactiveProperty<DateTime>, DateTime>)_dictionary[type][name])(obj as ReactiveProperty<DateTime>, (DateTime)parameter);
                        }
                        else if (objType == typeof(ReactiveProperty<DateTime?>))
                        {
                            _dictionary[type][name] = GetSetter<ReactiveProperty<DateTime?>, DateTime?>(obj.GetType(), name);
                            ((Action<ReactiveProperty<DateTime?>, DateTime?>)_dictionary[type][name])(obj as ReactiveProperty<DateTime?>, (DateTime?)parameter);
                        }
                        else if (objType == typeof(ReactiveProperty<Guid>))
                        {
                            _dictionary[type][name] = GetSetter<ReactiveProperty<Guid>, Guid>(obj.GetType(), name);
                            ((Action<ReactiveProperty<Guid>, Guid>)_dictionary[type][name])(obj as ReactiveProperty<Guid>, (Guid)parameter);
                        }
                        else if (objType == typeof(ReactiveProperty<Guid?>))
                        {
                            _dictionary[type][name] = GetSetter<ReactiveProperty<Guid?>, Guid?>(obj.GetType(), name);
                            ((Action<ReactiveProperty<Guid?>, Guid?>)_dictionary[type][name])(obj as ReactiveProperty<Guid?>, (Guid?)parameter);
                        }
                        else if (objType == typeof(ReactiveProperty<Type>))
                        {
                            _dictionary[type][name] = GetSetter<ReactiveProperty<Type>, Type>(obj.GetType(), name);
                            ((Action<ReactiveProperty<Type>, Type>)_dictionary[type][name])(obj as ReactiveProperty<Type>, (Type)parameter);
                        }
                        else if (objType == typeof(ReactiveProperty<object>))
                        {
                            _dictionary[type][name] = GetSetter<ReactiveProperty<object>, object>(obj.GetType(), name);
                            ((Action<ReactiveProperty<object>, object>)_dictionary[type][name])(obj as ReactiveProperty<object>, (object)parameter);
                        }

                        return true;
                    }
                }

                return false;
            }

            public static object ConvertTo(Type targetType, object value)
            {
                // ターゲット型がvalueの型と同じであれば、変換不要
                if (value.GetType() == targetType)
                {
                    return value;
                }

                // ここで特定の型へのカスタム変換ロジックを実装
                if (targetType == typeof(ReactivePropertySlim<bool>))
                {
                    return (ReactivePropertySlim<bool>)value;
                }
                else if (targetType == typeof(ReactivePropertySlim<bool?>))
                {
                    return (ReactivePropertySlim<bool?>)value;
                }
                else if (targetType == typeof(ReactivePropertySlim<short>))
                {
                    return (ReactivePropertySlim<short>)value;
                }
                else if (targetType == typeof(ReactivePropertySlim<short?>))
                {
                    return (ReactivePropertySlim<short?>)value;
                }
                else if (targetType == typeof(ReactivePropertySlim<int>))
                {
                    return (ReactivePropertySlim<int>)value;
                }
                else if (targetType == typeof(ReactivePropertySlim<int?>))
                {
                    return (ReactivePropertySlim<int?>)value;
                }
                else if (targetType == typeof(ReactivePropertySlim<long>))
                {
                    return (ReactivePropertySlim<long>)value;
                }
                else if (targetType == typeof(ReactivePropertySlim<long?>))
                {
                    return (ReactivePropertySlim<long?>)value;
                }
                else if (targetType == typeof(ReactivePropertySlim<ushort>))
                {
                    return (ReactivePropertySlim<ushort>)value;
                }
                else if (targetType == typeof(ReactivePropertySlim<ushort?>))
                {
                    return (ReactivePropertySlim<ushort?>)value;
                }
                else if (targetType == typeof(ReactivePropertySlim<uint>))
                {
                    return (ReactivePropertySlim<uint>)value;
                }
                else if (targetType == typeof(ReactivePropertySlim<uint?>))
                {
                    return (ReactivePropertySlim<uint?>)value;
                }
                else if (targetType == typeof(ReactivePropertySlim<ulong>))
                {
                    return (ReactivePropertySlim<ulong>)value;
                }
                else if (targetType == typeof(ReactivePropertySlim<ulong?>))
                {
                    return (ReactivePropertySlim<ulong?>)value;
                }
                else if (targetType == typeof(ReactivePropertySlim<float>))
                {
                    return (ReactivePropertySlim<float>)value;
                }
                else if (targetType == typeof(ReactivePropertySlim<float?>))
                {
                    return (ReactivePropertySlim<float?>)value;
                }
                else if (targetType == typeof(ReactivePropertySlim<double>))
                {
                    return (ReactivePropertySlim<double>)value;
                }
                else if (targetType == typeof(ReactivePropertySlim<double?>))
                {
                    return (ReactivePropertySlim<double?>)value;
                }
                else if (targetType == typeof(ReactivePropertySlim<DateTime>))
                {
                    return (ReactivePropertySlim<DateTime>)value;
                }
                else if (targetType == typeof(ReactivePropertySlim<DateTime?>))
                {
                    return (ReactivePropertySlim<DateTime?>)value;
                }
                else if (targetType == typeof(ReactivePropertySlim<Guid>))
                {
                    return (ReactivePropertySlim<Guid>)value;
                }
                else if (targetType == typeof(ReactivePropertySlim<Guid?>))
                {
                    return (ReactivePropertySlim<Guid?>)value;
                }
                else if (targetType == typeof(ReactivePropertySlim<Type>))
                {
                    return (ReactivePropertySlim<Type>)value;
                }
                else if (targetType == typeof(ReactivePropertySlim<object>))
                {
                    return (ReactivePropertySlim<object>)value;
                }
                else if (targetType == typeof(ReactiveProperty<bool>))
                {
                    return (ReactiveProperty<bool>)value;
                }
                else if (targetType == typeof(ReactiveProperty<bool?>))
                {
                    return (ReactiveProperty<bool?>)value;
                }
                else if (targetType == typeof(ReactiveProperty<short>))
                {
                    return (ReactiveProperty<short>)value;
                }
                else if (targetType == typeof(ReactiveProperty<short?>))
                {
                    return (ReactiveProperty<short?>)value;
                }
                else if (targetType == typeof(ReactiveProperty<int>))
                {
                    return (ReactiveProperty<int>)value;
                }
                else if (targetType == typeof(ReactiveProperty<int?>))
                {
                    return (ReactiveProperty<int?>)value;
                }
                else if (targetType == typeof(ReactiveProperty<long>))
                {
                    return (ReactiveProperty<long>)value;
                }
                else if (targetType == typeof(ReactiveProperty<long?>))
                {
                    return (ReactiveProperty<long?>)value;
                }
                else if (targetType == typeof(ReactiveProperty<ushort>))
                {
                    return (ReactiveProperty<ushort>)value;
                }
                else if (targetType == typeof(ReactiveProperty<ushort?>))
                {
                    return (ReactiveProperty<ushort?>)value;
                }
                else if (targetType == typeof(ReactiveProperty<uint>))
                {
                    return (ReactiveProperty<uint>)value;
                }
                else if (targetType == typeof(ReactiveProperty<uint?>))
                {
                    return (ReactiveProperty<uint?>)value;
                }
                else if (targetType == typeof(ReactiveProperty<ulong>))
                {
                    return (ReactiveProperty<ulong>)value;
                }
                else if (targetType == typeof(ReactiveProperty<ulong?>))
                {
                    return (ReactiveProperty<ulong?>)value;
                }
                else if (targetType == typeof(ReactiveProperty<float>))
                {
                    return (ReactiveProperty<float>)value;
                }
                else if (targetType == typeof(ReactiveProperty<float?>))
                {
                    return (ReactiveProperty<float?>)value;
                }
                else if (targetType == typeof(ReactiveProperty<double>))
                {
                    return (ReactiveProperty<double>)value;
                }
                else if (targetType == typeof(ReactiveProperty<double?>))
                {
                    return (ReactiveProperty<double?>)value;
                }
                else if (targetType == typeof(ReactiveProperty<DateTime>))
                {
                    return (ReactiveProperty<DateTime>)value;
                }
                else if (targetType == typeof(ReactiveProperty<DateTime?>))
                {
                    return (ReactiveProperty<DateTime?>)value;
                }
                else if (targetType == typeof(ReactiveProperty<Guid>))
                {
                    return (ReactiveProperty<Guid>)value;
                }
                else if (targetType == typeof(ReactiveProperty<Guid?>))
                {
                    return (ReactiveProperty<Guid?>)value;
                }
                else if (targetType == typeof(ReactiveProperty<Type>))
                {
                    return (ReactiveProperty<Type>)value;
                }
                else if (targetType == typeof(ReactiveProperty<object>))
                {
                    return (ReactiveProperty<object>)value;
                }

                throw new InvalidOperationException("サポートされていない型への変換");
            }

            public static Func<TObj, TProp> GetGetter<TObj, TProp>(string propName)
                => (Func<TObj, TProp>)
                    System.Delegate.CreateDelegate(typeof(Func<TObj, TProp>),
                        typeof(TObj).GetProperty(propName).GetGetMethod(nonPublic: true));

            public static Action<TObj, TProp> GetSetter<TObj, TProp>(Type type, string propName)
                => (Action<TObj, TProp>)
                    System.Delegate.CreateDelegate(typeof(Action<,>).MakeGenericType(type, typeof(TProp)),
                        type.GetProperty(propName).GetSetMethod(nonPublic: true));
        }

        protected object GetColumnValue(IDataRecord reader, IColumn column, ITable table)
        {
            if (column.EntityDataType == typeof(Guid) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<Guid>)))
            {
                return reader.SafeGetGuid(column.ColumnName, table);
            }
            else if (column.EntityDataType == typeof(Guid?) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<Guid?>)))
            {
                return reader.SafeGetNullableGuid(column.ColumnName, table);
            }
            else if (column.EntityDataType == typeof(string) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<string>)))
            {
                return reader.SafeGetString(column.ColumnName, table);
            }
            else if (column.EntityDataType == typeof(int) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<int>)))
            {
                return reader.SafeGetInt(column.ColumnName, table);
            }
            else if (column.EntityDataType == typeof(int?) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<int?>)))
            {
                return reader.SafeGetNullableInt(column.ColumnName, table);
            }
            else if (column.EntityDataType == typeof(long) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<long>)))
            {
                return reader.SafeGetLong(column.ColumnName, table);
            }
            else if (column.EntityDataType == typeof(long?) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<long?>)))
            {
                return reader.SafeNullableGetLong(column.ColumnName, table);
            }
            else if (column.EntityDataType == typeof(float) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<float>)))
            {
                return reader.SafeGetFloat(column.ColumnName, table);
            }
            else if (column.EntityDataType == typeof(float?) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<float?>)))
            {
                return reader.SafeGetNullableFloat(column.ColumnName, table);
            }
            else if (column.EntityDataType == typeof(double) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<double>)))
            {
                return reader.SafeGetDouble(column.ColumnName, table);
            }
            else if (column.EntityDataType == typeof(double?) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<double?>)))
            {
                return reader.SafeGetNullableDouble(column.ColumnName, table);
            }
            else if (column.EntityDataType == typeof(DateTime) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<DateTime>)))
            {
                return reader.SafeGetDateTime(column.ColumnName, table);
            }
            else if (column.EntityDataType == typeof(DateTime?) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<DateTime?>)))
            {
                return reader.SafeGetNullableDateTime(column.ColumnName, table);
            }
            else if (column.EntityDataType == typeof(bool) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<bool>)))
            {
                return reader.SafeGetBoolean(column.ColumnName, table);
            }
            else if (column.EntityDataType == typeof(bool?) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<bool?>)))
            {
                return reader.SafeGetNullableBoolean(column.ColumnName, table);
            }
            else if (column.EntityDataType == typeof(Type) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<Type>)))
            {
                return reader.SafeGetType(column.ColumnName, table);
            }
            else if (column.EntityDataType == typeof(object) || column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty<object>)))
            {
                return reader.SafeGetObject(column.ColumnName, table);
            }
            else if (column.DBDataType == "INTEGER")
            {
                return reader.SafeGetInt(column.ColumnName, table);
            }
            else
            {
                throw new NotSupportedException($"{column.EntityDataType.FullName} is not supported.");
            }
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

        public void DropTable(TimeSpan? timeout = null)
        {
            QueryHelper.KeepTryingUntilProcessSucceed(() =>
            {
                using var conn = GetConnection();
                var sql = $"drop table {TableName}";
                LogManager.GetCurrentClassLogger().Debug(sql);
                conn.Execute(sql);
            }, timeout);
        }

        public async Task DropTableAsync(TimeSpan? timeout = null)
        {
            await QueryHelper.KeepTryingUntilProcessSucceedAsync(new Func<Task>(async () =>
            {
                await using var conn = await GetConnectionAsync().ConfigureAwait(false);
                var sql = $"drop table {TableName}";
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
                            conn.Dispose();
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
                    finally
                    {
                        if (!isTransaction)
                        {
                            conn.Dispose();
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

        public async IAsyncEnumerable<E> FindByAsync(Dictionary<string, object> idDic, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
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
