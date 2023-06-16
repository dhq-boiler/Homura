
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
        public Dao()
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
        public Dao(Type entityVersionType) : base(entityVersionType)
        {
            OverridedColumns = new List<IColumn>();
            EntityVersionType = entityVersionType;
        }

        public Dao(DataVersionManager dataVersionManager) : base(VersionHelper.GetVersionTypeFromDataVersionManager<E>(dataVersionManager))
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

        public string TableName
        {
            get
            {
                return Table.Name;
            }
        }

        protected IEnumerable<IColumn> Columns
        {
            get
            {
                return Table.Columns;
            }
        }

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
                    conn = GetConnection();
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

        private IEnumerable<IDdlConstraint> ToIConstraintList(bool isNullable, bool isPrimaryKey)
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

        protected virtual E ToEntity(IDataRecord reader)
        {
            return ToEntityInDefaultWay(reader);
        }

        protected E ToEntityInDefaultWay(IDataRecord reader)
        {
            var ret = CreateInstance();
            const string VALUE_STR = "Value";

            foreach (var column in Columns)
            {
                if (column.EntityDataType.GetInterfaces().Contains(typeof(IReactiveProperty)))
                {
                    var getter = ret.GetType().GetProperty(column.ColumnName);
                    var rp = getter.GetValue(ret);
                    var setter = rp.GetType().GetProperty(VALUE_STR);
                    setter.SetValue(rp, CatchThrow(() => GetColumnValue(reader, column, Table)));
                }
                else
                {
                    var setter = ret.GetType().GetProperty(column.ColumnName);
                    setter.SetValue(ret, CatchThrow(() => GetColumnValue(reader, column, Table)));
                }
            }

            return ret;
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
            else
            {
                throw new NotSupportedException($"{column.EntityDataType.FullName} is not supported.");
            }
        }

        private E CreateInstance()
        {
            return new E();
        }

        public void CreateTableIfNotExists(TimeSpan? timeout = null)
        {
            QueryHelper.KeepTryingUntilProcessSucceed(() =>
            {
                using (var conn = GetConnection())
                {
                    var sql = $"create table if not exists {TableName}";

                    DefineColumns(ref sql, Columns);

                    LogManager.GetCurrentClassLogger().Debug(sql);
                    conn.Execute(sql);
                }
            }, timeout);
        }

        public async Task CreateTableIfNotExistsAsync(TimeSpan? timeout = null)
        {
            await QueryHelper.KeepTryingUntilProcessSucceedAsync(new Func<Task>(async () =>
            {
                using (var conn = await GetConnectionAsync().ConfigureAwait(false))
                {
                    var sql = $"create table if not exists {TableName}";

                    DefineColumns(ref sql, Columns);

                    LogManager.GetCurrentClassLogger().Debug(sql);
                    await conn.ExecuteAsync(sql).ConfigureAwait(false);
                }
            }), timeout);
        }

        public void DropTable(TimeSpan? timeout = null)
        {
            QueryHelper.KeepTryingUntilProcessSucceed(() =>
            {
                using (var conn = GetConnection())
                {
                    var sql = $"drop table {TableName}";

                    LogManager.GetCurrentClassLogger().Debug(sql);
                    conn.Execute(sql);
                }
            }, timeout);
        }

        public async Task DropTableAsync(TimeSpan? timeout = null)
        {
            await QueryHelper.KeepTryingUntilProcessSucceedAsync(new Func<Task>(async () =>
            {
                using (var conn = await GetConnectionAsync().ConfigureAwait(false))
                {
                    var sql = $"drop table {TableName}";

                    LogManager.GetCurrentClassLogger().Debug(sql);
                    await conn.ExecuteAsync(sql).ConfigureAwait(false);
                }
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

                using (var conn = GetConnection())
                {
                    var sql = $"create index if not exists {indexName} on {TableName}({indexPropertyName})";

                    LogManager.GetCurrentClassLogger().Debug(sql);
                    var result = conn.Execute(sql);
                    if (result != -1)
                    {
                        created += 1;
                    }
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

                using (var conn = await GetConnectionAsync().ConfigureAwait(false))
                {
                    var sql = $"create index if not exists {indexName} on {TableName}({indexPropertyName})";

                    LogManager.GetCurrentClassLogger().Debug(sql);
                    var result = await conn.ExecuteAsync(sql).ConfigureAwait(false);
                    if (result != -1)
                    {
                        created += 1;
                    }
                }
            }

            return created;
        }

        private int CreateIndexClass(int created)
        {
            var indexColumnNames = SearchIndexClass();
            if (indexColumnNames.Count() > 0)
            {
                var indexName = $"index_{TableName}_";
                Queue<string> queue = new(indexColumnNames);
                while (queue.Count() > 0)
                {
                    indexName += queue.Dequeue();
                    if (queue.Count() > 0)
                    {
                        indexName += "_";
                    }
                }
                using (var conn = GetConnection())
                {
                    var sql = $"create index if not exists {indexName} on {TableName}(";
                    Queue<string> queue2 = new(indexColumnNames);
                    while (queue2.Count() > 0)
                    {
                        sql += queue2.Dequeue();
                        if (queue2.Count() > 0)
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
                }
            }

            return created;
        }

        private async Task<int> CreateIndexClassAsync(int created)
        {
            var indexColumnNames = SearchIndexClass();
            if (indexColumnNames.Count() > 0)
            {
                var indexName = $"index_{TableName}_";
                Queue<string> queue = new(indexColumnNames);
                while (queue.Count() > 0)
                {
                    indexName += queue.Dequeue();
                    if (queue.Count() > 0)
                    {
                        indexName += "_";
                    }
                }
                using (var conn = await GetConnectionAsync().ConfigureAwait(false))
                {
                    var sql = $"create index if not exists {indexName} on {TableName}(";
                    Queue<string> queue2 = new(indexColumnNames);
                    while (queue2.Count() > 0)
                    {
                        sql += queue2.Dequeue();
                        if (queue2.Count() > 0)
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
                }
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
            if (indexAttr != null)
            {
                foreach (var name in indexAttr.PropertyNames)
                {
                    indexColumnNames.Add(name);
                }
            }
            return indexColumnNames;
        }

        public int CountAll(DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            return QueryHelper.KeepTryingUntilProcessSucceed<int>(() =>
                QueryHelper.ForDao.ConnectionInternalAndReturn(this, new Func<DbConnection, int>((connection) =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        var table = (Homura.ORM.Table<E>)Table.Clone();
                        if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                        {
                            table.Schema = anotherDatabaseAliasName;
                        }

                        using (var query = new Select().Count("1").As("Count")
                                                                        .From.Table(table))
                        {
                            var sql = query.ToSql();

                            command.CommandText = sql;

                            LogManager.GetCurrentClassLogger().Debug(sql);
                            using (var reader = command.ExecuteReader())
                            {
                                reader.Read();
                                return reader.GetInt32(reader.GetOrdinal("Count"));
                            }
                        }
                    }
                }), conn)
            , timeout);
        }

        public async Task<int> CountAllAsync(DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            return await QueryHelper.KeepTryingUntilProcessSucceedAndReturnAsync<int>(async () =>
                await await QueryHelper.ForDao.ConnectionInternalAndReturnAsync(this, new Func<DbConnection, Task<int>>(async (connection) =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        var table = (Homura.ORM.Table<E>)Table.Clone();
                        if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                        {
                            table.Schema = anotherDatabaseAliasName;
                        }

                        using (var query = new Select().Count("1").As("Count")
                                                                        .From.Table(table))
                        {
                            var sql = query.ToSql();

                            command.CommandText = sql;

                            LogManager.GetCurrentClassLogger().Debug(sql);
                            using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                            {
                                await reader.ReadAsync().ConfigureAwait(false);
                                return reader.GetInt32(reader.GetOrdinal("Count"));
                            }
                        }
                    }
                }), conn).ConfigureAwait(false)
            , timeout).ConfigureAwait(false);
        }

        public int CountBy(Dictionary<string, object> idDic, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            return QueryHelper.KeepTryingUntilProcessSucceed<int>(() =>
                QueryHelper.ForDao.ConnectionInternalAndReturn(this, new Func<DbConnection, int>((connection) =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        var table = (Table<E>)Table.Clone();
                        if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                        {
                            table.Schema = anotherDatabaseAliasName;
                        }

                        using (var query = new Select().Count("1").As("Count")
                                                                        .From.Table(table)
                                                                        .Where.KeyEqualToValue(idDic))
                        {
                            var sql = query.ToSql();

                            command.CommandText = sql;
                            command.CommandType = CommandType.Text;
                            query.SetParameters(command);

                            LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                            using (var reader = command.ExecuteReader())
                            {
                                reader.Read();
                                return reader.GetInt32(reader.GetOrdinal("Count"));
                            }
                        }
                    }
                }), conn)
            , timeout);
        }

        public async Task<int> CountByAsync(Dictionary<string, object> idDic, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            return await QueryHelper.KeepTryingUntilProcessSucceedAndReturnAsync(async () =>
                await await QueryHelper.ForDao.ConnectionInternalAndReturnAsync(this, new Func<DbConnection, Task<int>>(async (connection) =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        var table = (Table<E>)Table.Clone();
                        if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                        {
                            table.Schema = anotherDatabaseAliasName;
                        }

                        using (var query = new Select().Count("1").As("Count")
                                                                        .From.Table(table)
                                                                        .Where.KeyEqualToValue(idDic))
                        {
                            var sql = query.ToSql();

                            command.CommandText = sql;
                            command.CommandType = CommandType.Text;
                            query.SetParameters(command);

                            LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                            using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                            {
                                await reader.ReadAsync().ConfigureAwait(false);
                                return reader.GetInt32(reader.GetOrdinal("Count"));
                            }
                        }
                    }
                }), conn).ConfigureAwait(false)
            , timeout).ConfigureAwait(false);
        }

        public void DeleteWhereIDIs(Guid id, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            QueryHelper.KeepTryingUntilProcessSucceed(() =>
            {
                QueryHelper.ForDao.ConnectionInternal(this, (connection) =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        var table = (Table<E>)Table.Clone();
                        if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                        {
                            table.Schema = anotherDatabaseAliasName;
                        }

                        using (var query = new Delete().From.Table(table)
                                                                        .Where.Column("ID").EqualTo.Value(id))
                        {
                            var sql = query.ToSql();
                            command.CommandText = sql;

                            query.SetParameters(command);

                            LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                            command.ExecuteNonQuery();
                        }
                    }
                }, conn);
            }, timeout);
        }

        public async Task DeleteWhereIDIsAsync(Guid id, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            await QueryHelper.KeepTryingUntilProcessSucceedAsync(async () =>
            {
                await QueryHelper.ForDao.ConnectionInternalAsync(this, async (connection) =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        var table = (Table<E>)Table.Clone();
                        if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                        {
                            table.Schema = anotherDatabaseAliasName;
                        }

                        using (var query = new Delete().From.Table(table)
                                                                        .Where.Column("ID").EqualTo.Value(id))
                        {
                            var sql = query.ToSql();
                            command.CommandText = sql;

                            query.SetParameters(command);

                            LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    }
                }, conn).ConfigureAwait(false);

            }, timeout).ConfigureAwait(false);
        }

        public void DeleteAll(DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            QueryHelper.KeepTryingUntilProcessSucceed(() =>
            {
                QueryHelper.ForDao.ConnectionInternal(this, new Action<DbConnection>((connection) =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        var table = (Table<E>)Table.Clone();
                        if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                        {
                            table.Schema = anotherDatabaseAliasName;
                        }

                        using (var query = new Delete().From.Table(table))
                        {
                            var sql = query.ToSql();
                            command.CommandText = sql;
                            command.CommandType = CommandType.Text;

                            query.SetParameters(command);

                            LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                            var deleted = command.ExecuteNonQuery();
                        }
                    }
                }), conn);
            }, timeout);
        }

        public async Task DeleteAllAsync(DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            await QueryHelper.KeepTryingUntilProcessSucceedAsync(async () =>
            {
                await QueryHelper.ForDao.ConnectionInternalAsync(this, async (connection) =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        var table = (Table<E>)Table.Clone();
                        if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                        {
                            table.Schema = anotherDatabaseAliasName;
                        }

                        using (var query = new Delete().From.Table(table))
                        {
                            var sql = query.ToSql();
                            command.CommandText = sql;
                            command.CommandType = CommandType.Text;

                            query.SetParameters(command);

                            LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                            var deleted = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    }
                }, conn).ConfigureAwait(false);
            }, timeout).ConfigureAwait(false);
        }

        public void Delete(Dictionary<string, object> idDic, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            QueryHelper.KeepTryingUntilProcessSucceed(() =>
            {
                QueryHelper.ForDao.ConnectionInternal(this, new Action<DbConnection>((connection) =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        var table = (Table<E>)Table.Clone();
                        if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                        {
                            table.Schema = anotherDatabaseAliasName;
                        }

                        using (var query = new Delete().From.Table(table)
                                                                        .Where.KeyEqualToValue(idDic))
                        {
                            var sql = query.ToSql();
                            command.CommandText = sql;
                            command.CommandType = CommandType.Text;

                            query.SetParameters(command);

                            LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                            var deleted = command.ExecuteNonQuery();
                        }
                    }
                }), conn);
            }, timeout);
        }

        public async Task DeleteAsync(Dictionary<string, object> idDic, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            await QueryHelper.KeepTryingUntilProcessSucceedAsync(async () =>
            {
                await QueryHelper.ForDao.ConnectionInternalAsync(this, async (connection) =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        var table = (Table<E>)Table.Clone();
                        if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                        {
                            table.Schema = anotherDatabaseAliasName;
                        }

                        using (var query = new Delete().From.Table(table)
                                                                        .Where.KeyEqualToValue(idDic))
                        {
                            var sql = query.ToSql();
                            command.CommandText = sql;
                            command.CommandType = CommandType.Text;

                            query.SetParameters(command);

                            LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                            var deleted = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    }
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
                    using (var command = connection.CreateCommand())
                    {
                        var overrideColumns = SwapIfOverrided(Columns);

                        var table = (Table<E>)Table.Clone();
                        if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                        {
                            table.Schema = anotherDatabaseAliasName;
                        }

                        using (var query = new Insert().Into.Table(table).Columns(overrideColumns.Select(c => c.ColumnName))
                                                                                          .Values.Value(overrideColumns.Select(c => c.PropertyGetter(entity))))
                        {
                            var sql = query.ToSql();
                            command.CommandText = sql;
                            query.SetParameters(command);

                            LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                            var inserted = command.ExecuteNonQuery();
                            if (inserted == 0)
                            {
                                throw new NoEntityInsertedException($"Failed:{sql} {query.GetParameters().ToStringKeyIsValue()}");
                            }
                        }
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
                    using (var command = connection.CreateCommand())
                    {
                        var overrideColumns = SwapIfOverrided(Columns);

                        var table = (Table<E>)Table.Clone();
                        if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                        {
                            table.Schema = anotherDatabaseAliasName;
                        }

                        using (var query = new Insert().Into.Table(table).Columns(overrideColumns.Select(c => c.ColumnName))
                                                                                          .Values.Value(overrideColumns.Select(c => c.PropertyGetter(entity))))
                        {
                            var sql = query.ToSql();
                            command.CommandText = sql;
                            query.SetParameters(command);

                            LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                            var inserted = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                            if (inserted == 0)
                            {
                                throw new NoEntityInsertedException($"Failed:{sql} {query.GetParameters().ToStringKeyIsValue()}");
                            }
                        }
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
            if (timeout == null)
            {
                timeout = TimeSpan.FromMinutes(5);
            }

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

                        using (var command = conn.CreateCommand())
                        {
                            var table = (Table<E>)Table.Clone();
                            if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                            {
                                table.Schema = anotherDatabaseAliasName;
                            }

                            using (var query = new Select().Asterisk().From.Table(table))
                            {
                                var sql = query.ToSql();
                                command.CommandText = sql;
                                command.CommandType = CommandType.Text;

                                LogManager.GetCurrentClassLogger().Debug(sql);
                                using (var reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        ret.Add(ToEntity(reader));
                                    }
                                }
                            }
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
            if (timeout == null)
            {
                timeout = TimeSpan.FromMinutes(5);
            }

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

                        using (var command = conn.CreateCommand())
                        {
                            var table = (Table<E>)Table.Clone();
                            if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                            {
                                table.Schema = anotherDatabaseAliasName;
                            }

                            using (var query = new Select().Asterisk().From.Table(table))
                            {
                                var sql = query.ToSql();
                                command.CommandText = sql;
                                command.CommandType = CommandType.Text;

                                LogManager.GetCurrentClassLogger().Debug(sql);
                                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                                {
                                    while (await reader.ReadAsync().ConfigureAwait(false))
                                    {
                                        ret.Add(ToEntity(reader));
                                    }
                                }
                            }
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
            if (timeout == null)
            {
                timeout = TimeSpan.FromMinutes(5);
            }

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

                        using (var command = conn.CreateCommand())
                        {
                            var table = (Table<E>)Table.Clone();
                            if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                            {
                                table.Schema = anotherDatabaseAliasName;
                            }

                            using (var query = new Select().Asterisk().From.Table(table)
                                                                                       .Where.KeyEqualToValue(idDic))
                            {
                                var sql = query.ToSql();
                                command.CommandText = sql;
                                command.CommandType = CommandType.Text;
                                query.SetParameters(command);

                                LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                                using (var reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        ret.Add(ToEntity(reader));
                                    }
                                }
                            }
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
            if (timeout == null)
            {
                timeout = TimeSpan.FromMinutes(5);
            }

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

                        using (var command = conn.CreateCommand())
                        {
                            var table = (Table<E>)Table.Clone();
                            if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                            {
                                table.Schema = anotherDatabaseAliasName;
                            }

                            using (var query = new Select().Asterisk().From.Table(table)
                                                                                       .Where.KeyEqualToValue(idDic))
                            {
                                var sql = query.ToSql();
                                command.CommandText = sql;
                                command.CommandType = CommandType.Text;
                                query.SetParameters(command);

                                LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                                {
                                    while (await reader.ReadAsync().ConfigureAwait(false))
                                    {
                                        ret.Add(ToEntity(reader));
                                    }
                                }
                            }
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
                    using (var command = connection.CreateCommand())
                    {
                        var table = (Table<E>)Table.Clone();
                        if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                        {
                            table.Schema = anotherDatabaseAliasName;
                        }

                        using (var query = new Update().Table(table).Set.KeyEqualToValue(table.ColumnsWithoutPrimaryKeys.ToDictionary(c => c.ColumnName, c => c.PropertyGetter(entity)))
                                                                                     .Where.KeyEqualToValue(table.PrimaryKeyColumns.ToDictionary(c => c.ColumnName, c => c.PropertyGetter(entity))))
                        {
                            var sql = query.ToSql();
                            command.CommandText = sql;
                            query.SetParameters(command);

                            LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                            command.ExecuteNonQuery();
                        }
                    }
                }), conn);
            }, timeout);
        }

        public async Task<int> UpdateAsync(E entity, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            return await QueryHelper.KeepTryingUntilProcessSucceedAsync(async () =>
            {
                return await QueryHelper.ForDao.ConnectionInternalAsync(this, async (connection) =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        var table = (Table<E>)Table.Clone();
                        if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                        {
                            table.Schema = anotherDatabaseAliasName;
                        }

                        var a = table.ColumnsWithoutPrimaryKeys.ToDictionary(c => c.ColumnName, c => c.PropertyGetter(entity));
                        var b = table.PrimaryKeyColumns.ToDictionary(c => c.ColumnName, c => c.PropertyGetter(entity));

                        using (var query = new Update().Table(table).Set.KeyEqualToValue(table.ColumnsWithoutPrimaryKeys.ToDictionary(c => c.ColumnName, c => c.PropertyGetter(entity)))
                                                                                     .Where.KeyEqualToValue(table.PrimaryKeyColumns.ToDictionary(c => c.ColumnName, c => c.PropertyGetter(entity))))
                        {
                            var sql = query.ToSql();
                            command.CommandText = sql;
                            query.SetParameters(command);

                            LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                            return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    }
                }, conn).ConfigureAwait(false);
            }, timeout).ConfigureAwait(false);
        }

        private static string sqlToDefineColumns(IColumn c)
        {
            var r = $"{c.ColumnName} {c.DBDataType}";
            if (c.Constraints != null && c.Constraints.Count() > 0)
            {
                r += $" {c.ConstraintsToSql()}";
            }
            return r;
        }

        private void DefineColumns(ref string sql, IEnumerable<IColumn> columns)
        {
            CheckDelimiter(ref sql);
            sql += "(";
            EnumerateColumnsIntoSQL(ref sql, (c) => sqlToDefineColumns(c), ", ", columns);

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

        private void EnumerateColumnsIntoSQL(ref string sql, Func<IColumn, string> content, string connection, IEnumerable<IColumn> columns)
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

                        using (var query = new Insert().Into.Table(newTable)
                                                       .Columns(newTable.Columns.Select(c => c.ColumnName))
                                                       .Select.Columns(oldTable.Columns.Select(c => c.ColumnName).Union(newTable.NewColumns(oldTable, newTable).Select(v => v.WrapOutput()))).From.Table(oldTable))
                        {
                            var sql = query.ToSql();
                            command.CommandText = sql;

                            LogManager.GetCurrentClassLogger().Debug($"{sql}");
                            command.ExecuteNonQuery();
                        }
                    }

                    if ((mode & VersioningMode.DeleteAllRecordInTableCastedOff) == VersioningMode.DeleteAllRecordInTableCastedOff)
                    {
                        using (var command = connection.CreateCommand())
                        {
                            var sql = $"delete from {new Table<E>(upgradePath.From).Name}";
                            command.CommandText = sql;
                            LogManager.GetCurrentClassLogger().Debug($"{sql}");
                            command.ExecuteNonQuery();
                        }
                    }

                    if ((mode & VersioningMode.DropTableCastedOff) == VersioningMode.DropTableCastedOff)
                    {
                        using (var command = connection.CreateCommand())
                        {
                            var sql = $"drop table {new Table<E>(upgradePath.From).Name}";
                            command.CommandText = sql;
                            LogManager.GetCurrentClassLogger().Debug($"{sql}");
                            command.ExecuteNonQuery();
                        }
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
                    using (var command = connection.CreateCommand())
                    {
                        Table<E> newTable = new(upgradePath.To);
                        Table<E> oldTable = new(upgradePath.From);

                        using (var query = new Insert().Into.Table(newTable)
                                                       .Columns(newTable.Columns.Select(c => c.ColumnName))
                                                       .Select.Columns(oldTable.Columns.Select(c => c.ColumnName).Union(newTable.NewColumns(oldTable, newTable).Select(v => v.WrapOutput()))).From.Table(oldTable))
                        {
                            var sql = query.ToSql();
                            command.CommandText = sql;

                            LogManager.GetCurrentClassLogger().Debug($"{sql}");
                            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    }

                    if ((mode & VersioningMode.DeleteAllRecordInTableCastedOff) == VersioningMode.DeleteAllRecordInTableCastedOff)
                    {
                        using (var command = connection.CreateCommand())
                        {
                            var sql = $"delete from {new Table<E>(upgradePath.From).Name}";
                            command.CommandText = sql;
                            LogManager.GetCurrentClassLogger().Debug($"{sql}");
                            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    }

                    if ((mode & VersioningMode.DropTableCastedOff) == VersioningMode.DropTableCastedOff)
                    {
                        using (var command = connection.CreateCommand())
                        {
                            var sql = $"drop table {new Table<E>(upgradePath.From).Name}";
                            command.CommandText = sql;
                            LogManager.GetCurrentClassLogger().Debug($"{sql}");
                            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
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
                return default(T);
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
                                                        .Select.Columns(oldTable.Columns.Select(c => c.ColumnName).Union(newTable.NewColumns(oldTable, newTable).Select(v => v.WrapOutput()))).From.Table(oldTable))
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
                        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }

                    using (var command = connection.CreateCommand())
                    {
                        //fromテーブルからToテーブルへコピー
                        using (var query = new Insert().Into.Table(new NeutralTable($"{newTable.Name}_To"))
                                                        .Columns(newTable.Columns.Select(c => c.ColumnName))
                                                        .Select.Columns(oldTable.Columns.Select(c => c.ColumnName).Union(newTable.NewColumns(oldTable, newTable).Select(v => v.WrapOutput()))).From.Table(oldTable))
                        {
                            command.CommandText = query.ToSql();
                            LogManager.GetCurrentClassLogger().Debug($"{query.ToSql()}");
                            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    }

                    using (var command = connection.CreateCommand())
                    {
                        //Drop fromテーブル
                        var sql = $"drop table {oldTable.Name}";
                        command.CommandText = sql;
                        LogManager.GetCurrentClassLogger().Debug($"{sql}");
                        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }

                    using (var command = connection.CreateCommand())
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
