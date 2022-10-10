
using Dapper;
using Homura.Core;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Homura.ORM
{
    public abstract class Dao<E> : MustInitialize<Type>, IDao<E> where E : EntityBaseObject
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

        protected DbConnection GetConnection()
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

        protected async Task<DbConnection> GetConnectionAsync()
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

        protected IEnumerable<IColumn> GetColumnDefinitions(DbConnection conn = null)
        {
            bool isTransaction = conn != null;

            try
            {
                if (!isTransaction)
                {
                    conn = GetConnection();
                }

                var objSchemaInfo = (conn as DbConnection).GetSchema(OleDbMetaDataCollectionNames.Columns, new string[] { null, null, TableName, null });
                foreach (DataRow objRow in objSchemaInfo.Rows)
                {
                    bool isNullable = objRow.Field<bool>(s_IS_NULLABLE);
                    bool isPrimaryKey = objRow.Field<bool>(s_PRIMARY_KEY);

                    var constraints = ToIConstraintList(isNullable, isPrimaryKey);

                    yield return new Column(
                        objRow.Field<string>(s_COLUMN_NAME),
                        typeof(E).GetProperty(objRow.Field<string>(s_COLUMN_NAME)).PropertyType,
                        objRow.Field<string>(s_DATA_TYPE).ToUpper(),
                        constraints,
                        objSchemaInfo.Rows.IndexOf(objRow),
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
                yield return new NotNull();
            if (isPrimaryKey)
                yield return new PrimaryKey();
        }

        protected abstract E ToEntity(IDataRecord reader);

        public void CreateTableIfNotExists(TimeSpan? timeout = null)
        {
            KeepTryingUntilProcessSucceed(() =>
            {
                using (var conn = GetConnection())
                {
                    string sql = $"create table if not exists {TableName}";

                    DefineColumns(ref sql, Columns);

                    LogManager.GetCurrentClassLogger().Debug(sql);
                    conn.Execute(sql);
                }
            }, timeout);
        }

        public async Task CreateTableIfNotExistsAsync(TimeSpan? timeout = null)
        {
            await KeepTryingUntilProcessSucceedAsync<Task>(new Func<Task>(async () =>
            {
                using (var conn = await GetConnectionAsync().ConfigureAwait(false))
                {
                    string sql = $"create table if not exists {TableName}";

                    DefineColumns(ref sql, Columns);

                    LogManager.GetCurrentClassLogger().Debug(sql);
                    await conn.ExecuteAsync(sql).ConfigureAwait(false);
                }
            }), timeout);
        }

        public void DropTable(TimeSpan? timeout = null)
        {
            KeepTryingUntilProcessSucceed(() =>
            {
                using (var conn = GetConnection())
                {
                    string sql = $"drop table {TableName}";

                    LogManager.GetCurrentClassLogger().Debug(sql);
                    conn.Execute(sql);
                }
            }, timeout);
        }

        public async Task DropTableAsync(TimeSpan? timeout = null)
        {
            await KeepTryingUntilProcessSucceedAsync<Task>(new Func<Task>(async () =>
            {
                using (var conn = await GetConnectionAsync().ConfigureAwait(false))
                {
                    string sql = $"drop table {TableName}";

                    LogManager.GetCurrentClassLogger().Debug(sql);
                    await conn.ExecuteAsync(sql).ConfigureAwait(false);
                }
            }), timeout).ConfigureAwait(false);
        }

        public int CreateIndexIfNotExists(TimeSpan? timeout = null)
        {
            int created = 0;
            KeepTryingUntilProcessSucceed(() =>
            {
                created = CreateIndexClass(created);
                created = CreateIndexProperties(created);
            }, timeout);
            return created;
        }

        public async Task<int> CreateIndexIfNotExistsAsync(TimeSpan? timeout = null)
        {
            int created = 0;
            await KeepTryingUntilProcessSucceedAsync<Task>(new Func<Task>(async () =>
            {
                created = await CreateIndexClassAsync(created);
                created = await CreateIndexPropertiesAsync(created);
            }), timeout).ConfigureAwait(false);
            return created;
        }

        private int CreateIndexProperties(int created)
        {
            HashSet<string> indexPropertyNames = SearchIndexProperties();
            foreach (var indexPropertyName in indexPropertyNames)
            {
                string indexName = $"index_{TableName}_{indexPropertyName}";

                using (var conn = GetConnection())
                {
                    string sql = $"create index if not exists {indexName} on {TableName}({indexPropertyName})";

                    LogManager.GetCurrentClassLogger().Debug(sql);
                    int result = conn.Execute(sql);
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
            HashSet<string> indexPropertyNames = SearchIndexProperties();
            foreach (var indexPropertyName in indexPropertyNames)
            {
                string indexName = $"index_{TableName}_{indexPropertyName}";

                using (var conn = await GetConnectionAsync().ConfigureAwait(false))
                {
                    string sql = $"create index if not exists {indexName} on {TableName}({indexPropertyName})";

                    LogManager.GetCurrentClassLogger().Debug(sql);
                    int result = await conn.ExecuteAsync(sql).ConfigureAwait(false);
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
            HashSet<string> indexColumnNames = SearchIndexClass();
            if (indexColumnNames.Count() > 0)
            {
                string indexName = $"index_{TableName}_";
                var queue = new Queue<string>(indexColumnNames);
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
                    string sql = $"create index if not exists {indexName} on {TableName}(";
                    var queue2 = new Queue<string>(indexColumnNames);
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
                    int result = conn.Execute(sql);
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
            HashSet<string> indexColumnNames = SearchIndexClass();
            if (indexColumnNames.Count() > 0)
            {
                string indexName = $"index_{TableName}_";
                var queue = new Queue<string>(indexColumnNames);
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
                    string sql = $"create index if not exists {indexName} on {TableName}(";
                    var queue2 = new Queue<string>(indexColumnNames);
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
                    int result = await conn.ExecuteAsync(sql).ConfigureAwait(false);
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
            HashSet<string> indexColumnNames = new HashSet<string>();
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
            HashSet<string> indexColumnNames = new HashSet<string>();
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

        /// <summary>
        /// _IsTransaction フラグによって局所的に DbConnection を使用するかどうか選択できるクエリ実行用内部メソッド
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="body"></param>
        /// <returns></returns>
        protected R ConnectionInternal<R>(Func<DbConnection, R> body, DbConnection conn = null)
        {
            bool isTransaction = conn != null;

            try
            {
                if (!isTransaction)
                {
                    conn = GetConnection();
                }

                return body.Invoke(conn);
            }
            finally
            {
                if (!isTransaction)
                {
                    conn.Dispose();
                }
            }
        }

        /// <summary>
        /// _IsTransaction フラグによって局所的に DbConnection を使用するかどうか選択できるクエリ実行用内部メソッド
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="body"></param>
        /// <returns></returns>
        protected async Task<R> ConnectionInternalAndReturnAsync<R>(Func<DbConnection, R> body, DbConnection conn = null)
        {
            bool isTransaction = conn != null;

            try
            {
                if (!isTransaction)
                {
                    conn = await GetConnectionAsync().ConfigureAwait(false);
                }

                return body(conn);
            }
            finally
            {
                if (!isTransaction)
                {
                    conn.Dispose();
                }
            }
        }

        protected IEnumerable<R> ConnectionInternalYield<R>(Func<DbConnection, IEnumerable<R>> body, DbConnection conn = null)
        {
            bool isTransaction = conn != null;

            try
            {
                if (!isTransaction)
                {
                    conn = GetConnection();
                }

                return body.Invoke(conn);
            }
            finally
            {
                if (!isTransaction)
                {
                    conn.Dispose();
                }
            }
        }

        protected async IAsyncEnumerable<R> ConnectionInternalYieldAsync<R>(Func<DbConnection, IAsyncEnumerable<R>> body, DbConnection conn = null)
        {
            bool isTransaction = conn != null;

            try
            {
                if (!isTransaction)
                {
                    conn = await GetConnectionAsync().ConfigureAwait(false);
                }

                yield return (R)body(conn);
            }
            finally
            {
                if (!isTransaction)
                {
                    conn.Dispose();
                }
            }
        }

        /// <summary>
        /// _IsTransaction フラグによって局所的に DbConnection を使用するかどうか選択できるクエリ実行用内部メソッド
        /// </summary>
        /// <param name="body"></param>
        protected void ConnectionInternal(Action<DbConnection> body, DbConnection conn = null)
        {
            bool isTransaction = conn != null;

            try
            {
                if (!isTransaction)
                {
                    conn = GetConnection();
                }

                body.Invoke(conn);
            }
            finally
            {
                if (!isTransaction)
                {
                    conn.Dispose();
                }
            }
        }

        /// <summary>
        /// _IsTransaction フラグによって局所的に DbConnection を使用するかどうか選択できるクエリ実行用内部メソッド
        /// </summary>
        /// <param name="body"></param>
        protected async Task ConnectionInternalAsync(Action<DbConnection> body, DbConnection conn = null)
        {
            bool isTransaction = conn != null;

            try
            {
                if (!isTransaction)
                {
                    conn = await GetConnectionAsync().ConfigureAwait(false);
                }

                body(conn);
            }
            finally
            {
                if (!isTransaction)
                {
                    conn.Dispose();
                }
            }
        }

        public readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Actionを成功するかタイムアウトするまで試行し続けます。
        /// </summary>
        /// <param name="body">試行し続ける対象のAction</param>
        /// <param name="timeout">タイムアウト</param>
        protected void KeepTryingUntilProcessSucceed(Action body, TimeSpan? timeout = null)
        {
            if (timeout == null)
            {
                timeout = TimeSpan.FromMinutes(5);
            }

            var beginTime = DateTime.Now;

            while ((DateTime.Now - beginTime) <= timeout)
            {
                try
                {
                    LogManager.GetCurrentClassLogger().Trace("try body()");
                    body();
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

        /// <summary>
        /// Funcを成功するかタイムアウトするまで試行し続けます。
        /// </summary>
        /// <param name="body">試行し続ける対象のFunc</param>
        /// <param name="timeout">タイムアウト</param>
        protected T KeepTryingUntilProcessSucceed<T>(Func<T> body, TimeSpan? timeout = null)
        {
            if (timeout == null)
            {
                timeout = TimeSpan.FromMinutes(5);
            }

            var beginTime = DateTime.Now;

            while ((DateTime.Now - beginTime) <= timeout)
            {
                try
                {
                    LogManager.GetCurrentClassLogger().Trace("try body()");
                    return body();
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

        /// <summary>
        /// Funcを成功するかタイムアウトするまで試行し続けます。
        /// </summary>
        /// <param name="body">試行し続ける対象のFunc</param>
        /// <param name="timeout">タイムアウト</param>
        protected async Task KeepTryingUntilProcessSucceedAsync<T>(Func<Task> body, TimeSpan? timeout = null)
        {
            if (timeout == null)
            {
                timeout = TimeSpan.FromMinutes(5);
            }

            var beginTime = DateTime.Now;

            while ((DateTime.Now - beginTime) <= timeout)
            {
                try
                {
                    LogManager.GetCurrentClassLogger().Trace("try body()");
                    await body().ConfigureAwait(false);
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

        /// <summary>
        /// Funcを成功するかタイムアウトするまで試行し続けます。
        /// </summary>
        /// <param name="body">試行し続ける対象のFunc</param>
        /// <param name="timeout">タイムアウト</param>
        protected async Task<T> KeepTryingUntilProcessSucceedAndReturnAsync<T>(Func<Task<T>> body, TimeSpan? timeout = null)
        {
            if (timeout == null)
            {
                timeout = TimeSpan.FromMinutes(5);
            }

            var beginTime = DateTime.Now;

            while ((DateTime.Now - beginTime) <= timeout)
            {
                try
                {
                    LogManager.GetCurrentClassLogger().Trace("try body()");
                    return await body().ConfigureAwait(false);
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

        public int CountAll(DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            return KeepTryingUntilProcessSucceed<int>(() =>
                ConnectionInternal(new Func<DbConnection, int>((connection) =>
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
                            string sql = query.ToSql();

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
            return await KeepTryingUntilProcessSucceedAndReturnAsync<int>(async () =>
                await await ConnectionInternalAndReturnAsync(new Func<DbConnection, Task<int>>(async (connection) =>
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
                            string sql = query.ToSql();

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
            return KeepTryingUntilProcessSucceed<int>(() =>
                ConnectionInternal(new Func<DbConnection, int>((connection) =>
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
                            string sql = query.ToSql();

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
            return await KeepTryingUntilProcessSucceedAndReturnAsync(async () =>
                await await ConnectionInternalAndReturnAsync(new Func<DbConnection, Task<int>>(async (connection) =>
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
                            string sql = query.ToSql();

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
            KeepTryingUntilProcessSucceed(() =>
            {
                ConnectionInternal(new Action<DbConnection>((connection) =>
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
                            string sql = query.ToSql();
                            command.CommandText = sql;

                            query.SetParameters(command);

                            LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                            command.ExecuteNonQuery();
                        }
                    }
                }), conn);

            }, timeout);
        }

        public async Task DeleteWhereIDIsAsync(Guid id, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            await KeepTryingUntilProcessSucceedAsync<Task>(async() =>
            {
                await ConnectionInternalAsync(new Action<DbConnection>(async (connection) =>
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
                            string sql = query.ToSql();
                            command.CommandText = sql;

                            query.SetParameters(command);

                            LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    }
                }), conn).ConfigureAwait(false);

            }, timeout).ConfigureAwait(false);
        }

        public void DeleteAll(DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            KeepTryingUntilProcessSucceed(() =>
            {
                ConnectionInternal(new Action<DbConnection>((connection) =>
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
                            string sql = query.ToSql();
                            command.CommandText = sql;
                            command.CommandType = CommandType.Text;

                            query.SetParameters(command);

                            LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                            int deleted = command.ExecuteNonQuery();
                        }
                    }
                }), conn);
            }, timeout);
        }

        public async Task DeleteAllAsync(DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            await KeepTryingUntilProcessSucceedAsync<Task>(async () =>
            {
                await ConnectionInternalAsync(new Action<DbConnection>(async (connection) =>
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
                            string sql = query.ToSql();
                            command.CommandText = sql;
                            command.CommandType = CommandType.Text;

                            query.SetParameters(command);

                            LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                            int deleted = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    }
                }), conn).ConfigureAwait(false);
            }, timeout).ConfigureAwait(false);
        }

        public void Delete(Dictionary<string, object> idDic, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            KeepTryingUntilProcessSucceed(() =>
            {
                ConnectionInternal(new Action<DbConnection>((connection) =>
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
                            string sql = query.ToSql();
                            command.CommandText = sql;
                            command.CommandType = CommandType.Text;

                            query.SetParameters(command);

                            LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                            int deleted = command.ExecuteNonQuery();
                        }
                    }
                }), conn);
            }, timeout);
        }

        public async Task DeleteAsync(Dictionary<string, object> idDic, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            await KeepTryingUntilProcessSucceedAsync<Task>(async () =>
            {
                await ConnectionInternalAsync(new Action<DbConnection>(async (connection) =>
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
                            string sql = query.ToSql();
                            command.CommandText = sql;
                            command.CommandType = CommandType.Text;

                            query.SetParameters(command);

                            LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                            int deleted = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    }
                }), conn).ConfigureAwait(false);
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

            KeepTryingUntilProcessSucceed(() =>
            {
                ConnectionInternal(new Action<DbConnection>((connection) =>
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
                                                                                          .Values.Value(overrideColumns.Select(c => c.PropInfo.GetValue(entity))))
                        {
                            string sql = query.ToSql();
                            command.CommandText = sql;
                            query.SetParameters(command);

                            LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                            int inserted = command.ExecuteNonQuery();
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

            await KeepTryingUntilProcessSucceedAsync<Task>(async () =>
            {
                await ConnectionInternalAsync(new Action<DbConnection>(async (connection) =>
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
                                                                                          .Values.Value(overrideColumns.Select(c => c.PropInfo.GetValue(entity))))
                        {
                            string sql = query.ToSql();
                            command.CommandText = sql;
                            query.SetParameters(command);

                            LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                            int inserted = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                            if (inserted == 0)
                            {
                                throw new NoEntityInsertedException($"Failed:{sql} {query.GetParameters().ToStringKeyIsValue()}");
                            }
                        }
                    }
                }), conn).ConfigureAwait(false);
            }, timeout).ConfigureAwait(false);
        }

        protected IEnumerable<IColumn> SwapIfOverrided(IEnumerable<IColumn> columns)
        {
            List<IColumn> ret = new List<IColumn>();

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

            List<E> ret = new List<E>();

            while ((DateTime.Now - beginTime) <= timeout)
            {
                try
                {
                    bool isTransaction = conn != null;

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
                                string sql = query.ToSql();
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

            List<E> ret = new List<E>();

            while ((DateTime.Now - beginTime) <= timeout)
            {
                try
                {
                    bool isTransaction = conn != null;

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
                                string sql = query.ToSql();
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

            List<E> ret = new List<E>();

            while ((DateTime.Now - beginTime) <= timeout)
            {
                try
                {
                    bool isTransaction = conn != null;

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
                                string sql = query.ToSql();
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

            List<E> ret = new List<E>();

            while ((DateTime.Now - beginTime) <= timeout)
            {
                try
                {
                    bool isTransaction = conn != null;

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
                                string sql = query.ToSql();
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
            KeepTryingUntilProcessSucceed(() =>
            {
                ConnectionInternal(new Action<DbConnection>((connection) =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        var table = (Table<E>)Table.Clone();
                        if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                        {
                            table.Schema = anotherDatabaseAliasName;
                        }

                        using (var query = new Update().Table(table).Set.KeyEqualToValue(table.ColumnsWithoutPrimaryKeys.ToDictionary(c => c.ColumnName, c => c.PropInfo.GetValue(entity)))
                                                                                     .Where.KeyEqualToValue(table.PrimaryKeyColumns.ToDictionary(c => c.ColumnName, c => c.PropInfo.GetValue(entity))))
                        {
                            string sql = query.ToSql();
                            command.CommandText = sql;
                            query.SetParameters(command);

                            LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                            command.ExecuteNonQuery();
                        }
                    }
                }), conn);
            }, timeout);
        }

        public async Task UpdateAsync(E entity, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null)
        {
            await KeepTryingUntilProcessSucceedAsync<Task>(async () =>
            {
                await ConnectionInternalAsync(new Action<DbConnection>(async (connection) =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        var table = (Table<E>)Table.Clone();
                        if (!string.IsNullOrWhiteSpace(anotherDatabaseAliasName))
                        {
                            table.Schema = anotherDatabaseAliasName;
                        }

                        using (var query = new Update().Table(table).Set.KeyEqualToValue(table.ColumnsWithoutPrimaryKeys.ToDictionary(c => c.ColumnName, c => c.PropInfo.GetValue(entity)))
                                                                                     .Where.KeyEqualToValue(table.PrimaryKeyColumns.ToDictionary(c => c.ColumnName, c => c.PropInfo.GetValue(entity))))
                        {
                            string sql = query.ToSql();
                            command.CommandText = sql;
                            query.SetParameters(command);

                            LogManager.GetCurrentClassLogger().Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    }
                }), conn).ConfigureAwait(false);
            }, timeout).ConfigureAwait(false);
        }

        private static string sqlToDefineColumns(IColumn c)
        {
            string r = $"{c.ColumnName} {c.DBDataType}";
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
            var queue = new Queue<IColumn>(columns);
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
            if (OverridedColumns != null)
            {
                OverridedColumns.Clear();
            }
        }

        public void UpgradeTable(VersionChangeUnit upgradePath, VersioningMode mode, DbConnection conn = null, TimeSpan? timeout = null)
        {
            KeepTryingUntilProcessSucceed(() =>
            {
                ConnectionInternal(new Action<DbConnection>((connection) =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        var newTable = new Table<E>(upgradePath.To);
                        var oldTable = new Table<E>(upgradePath.From);

                        using (var query = new Insert().Into.Table(newTable)
                                                       .Columns(newTable.Columns.Select(c => c.ColumnName))
                                                       .Select.Columns(oldTable.Columns.Select(c => c.ColumnName).Union(newTable.NewColumns(oldTable, newTable).Select(v => v.WrapOutput()))).From.Table(oldTable))
                        {
                            string sql = query.ToSql();
                            command.CommandText = sql;

                            LogManager.GetCurrentClassLogger().Debug($"{sql}");
                            command.ExecuteNonQuery();
                        }
                    }

                    if ((mode & VersioningMode.DeleteAllRecordInTableCastedOff) == VersioningMode.DeleteAllRecordInTableCastedOff)
                    {
                        using (var command = connection.CreateCommand())
                        {
                            string sql = $"delete from {new Table<E>(upgradePath.From).Name}";
                            command.CommandText = sql;
                            LogManager.GetCurrentClassLogger().Debug($"{sql}");
                            command.ExecuteNonQuery();
                        }
                    }

                    if ((mode & VersioningMode.DropTableCastedOff) == VersioningMode.DropTableCastedOff)
                    {
                        using (var command = connection.CreateCommand())
                        {
                            string sql = $"drop table {new Table<E>(upgradePath.From).Name}";
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
            await KeepTryingUntilProcessSucceedAsync<Task>(async () =>
            {
                await ConnectionInternalAsync(new Action<DbConnection>(async (connection) =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        var newTable = new Table<E>(upgradePath.To);
                        var oldTable = new Table<E>(upgradePath.From);

                        using (var query = new Insert().Into.Table(newTable)
                                                       .Columns(newTable.Columns.Select(c => c.ColumnName))
                                                       .Select.Columns(oldTable.Columns.Select(c => c.ColumnName).Union(newTable.NewColumns(oldTable, newTable).Select(v => v.WrapOutput()))).From.Table(oldTable))
                        {
                            string sql = query.ToSql();
                            command.CommandText = sql;

                            LogManager.GetCurrentClassLogger().Debug($"{sql}");
                            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    }

                    if ((mode & VersioningMode.DeleteAllRecordInTableCastedOff) == VersioningMode.DeleteAllRecordInTableCastedOff)
                    {
                        using (var command = connection.CreateCommand())
                        {
                            string sql = $"delete from {new Table<E>(upgradePath.From).Name}";
                            command.CommandText = sql;
                            LogManager.GetCurrentClassLogger().Debug($"{sql}");
                            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    }

                    if ((mode & VersioningMode.DropTableCastedOff) == VersioningMode.DropTableCastedOff)
                    {
                        using (var command = connection.CreateCommand())
                        {
                            string sql = $"drop table {new Table<E>(upgradePath.From).Name}";
                            command.CommandText = sql;
                            LogManager.GetCurrentClassLogger().Debug($"{sql}");
                            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                        }
                    }

                }), conn).ConfigureAwait(false);
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
            KeepTryingUntilProcessSucceed(() =>
            {
                ConnectionInternal(new Action<DbConnection>((connection) =>
                {
                    var newTable = new Table<E>(versionTo);
                    var oldTable = new Table<E>(versionFrom);
                    using (var command = connection.CreateCommand())
                    {
                        //Toテーブル作成
                        string sql = $"create table {newTable.Name}_To(";
                        foreach (var c in newTable.Columns)
                        {
                            sql += $"{c.ColumnName} {c.DBDataType}";
                            if (!c.Equals(newTable.Columns.Last()))
                                sql += ", ";
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
            await KeepTryingUntilProcessSucceedAsync<Task>(async () =>
            {
                await ConnectionInternalAsync(new Action<DbConnection>(async (connection) =>
                {
                    var newTable = new Table<E>(versionTo);
                    var oldTable = new Table<E>(versionFrom);
                    using (var command = connection.CreateCommand())
                    {
                        //Toテーブル作成
                        string sql = $"create table {newTable.Name}_To(";
                        foreach (var c in newTable.Columns)
                        {
                            sql += $"{c.ColumnName} {c.DBDataType}";
                            if (!c.Equals(newTable.Columns.Last()))
                                sql += ", ";
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
                }), conn).ConfigureAwait(false);
            }, timeout).ConfigureAwait(false);
        }
    }
}
