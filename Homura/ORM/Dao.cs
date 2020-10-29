
using Dapper;
using Homura.Core;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.QueryBuilder.Iso.Dml;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Linq;
using System.Reflection;

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

        private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

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

                var objSchemaInfo = conn.GetSchema(OleDbMetaDataCollectionNames.Columns, new string[] { null, null, TableName, null });
                foreach (DataRow objRow in objSchemaInfo.Rows)
                {
                    bool isNullable = objRow.Field<bool>(s_IS_NULLABLE);
                    bool isPrimaryKey = objRow.Field<bool>(s_PRIMARY_KEY);

                    var constraints = ToIConstraintList(isNullable, isPrimaryKey);

                    yield return new Column(
                        objRow.Field<string>(s_COLUMN_NAME),
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

        public void CreateTableIfNotExists()
        {
            using (var conn = GetConnection())
            {
                string sql = $"create table if not exists {TableName}";

                DefineColumns(ref sql, Columns);

                s_logger.Debug(sql);
                conn.Execute(sql);
            }
        }

        public void DropTable()
        {
            using (var conn = GetConnection())
            {
                string sql = $"drop table {TableName}";

                s_logger.Debug(sql);
                conn.Execute(sql);
            }
        }

        public int CreateIndexIfNotExists()
        {
            int created = 0;
            created = CreateIndexClass(created);
            created = CreateIndexProperties(created);
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

                    s_logger.Debug(sql);
                    int result = conn.Execute(sql);
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

                    s_logger.Debug(sql);
                    int result = conn.Execute(sql);
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

        public int CountAll(DbConnection conn = null, string anotherDatabaseAliasName = null)
        {
            return ConnectionInternal(new Func<DbConnection, int>((connection) =>
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

                        s_logger.Debug(sql);
                        using (var reader = command.ExecuteReader())
                        {
                            reader.Read();
                            return reader.GetInt32(reader.GetOrdinal("Count"));
                        }
                    }
                }
            }), conn);
        }

        public int CountBy(Dictionary<string, object> idDic, DbConnection conn = null, string anotherDatabaseAliasName = null)
        {
            return ConnectionInternal(new Func<IDbConnection, int>((connection) =>
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

                        s_logger.Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                        using (var reader = command.ExecuteReader())
                        {
                            reader.Read();
                            return reader.GetInt32(reader.GetOrdinal("Count"));
                        }
                    }
                }
            }), conn);
        }

        public void DeleteWhereIDIs(Guid id, DbConnection conn = null, string anotherDatabaseAliasName = null)
        {
            ConnectionInternal(new Action<IDbConnection>((connection) =>
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

                        s_logger.Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                        command.ExecuteNonQuery();
                    }
                }
            }), conn);
        }

        public void DeleteAll(DbConnection conn = null, string anotherDatabaseAliasName = null)
        {
            ConnectionInternal(new Action<IDbConnection>((connection) =>
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

                        s_logger.Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                        int deleted = command.ExecuteNonQuery();
                    }
                }
            }), conn);
        }

        public void Delete(Dictionary<string, object> idDic, DbConnection conn = null, string anotherDatabaseAliasName = null)
        {
            ConnectionInternal(new Action<IDbConnection>((connection) =>
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

                        s_logger.Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                        int deleted = command.ExecuteNonQuery();
                    }
                }
            }), conn);
        }

        public void Insert(E entity, DbConnection conn = null, string anotherDatabaseAliasName = null)
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

            ConnectionInternal(new Action<IDbConnection>((connection) =>
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

                        s_logger.Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                        int inserted = command.ExecuteNonQuery();
                        if (inserted == 0)
                        {
                            throw new NoEntityInsertedException($"Failed:{sql} {query.GetParameters().ToStringKeyIsValue()}");
                        }
                    }
                }
            }), conn);
        }

        //protected void SetWhere(IQueryBuilder query, Dictionary<string, object> idDic)
        //{
        //    foreach (var x in idDic)
        //    {
        //        if (x.Value != null && x.Value.GetType().IsArray)
        //        {
        //            IEnumerable list = (IEnumerable)x.Value;
        //            var col = new List<object>();
        //            foreach (object o in list)
        //            {
        //                col.Add(o);
        //            }
        //            query.AddWhere(new In(x.Key, col.ToArray()));
        //        }
        //        else if (x.Value is DaoConst.IsNull)
        //        {
        //            query.AddWhere(new IsNull(x.Key));
        //        }
        //        else if (x.Value is DaoConst.IsNotNull)
        //        {
        //            query.AddWhere(new IsNotNull(x.Key));
        //        }
        //        else
        //        {
        //            query.AddWhere(x.Key, x.Value);
        //        }
        //    }
        //}

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

        public IEnumerable<E> FindAll(DbConnection conn = null, string anotherDatabaseAliasName = null)
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

                        s_logger.Debug(sql);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                yield return ToEntity(reader);
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

        public IEnumerable<E> FindBy(Dictionary<string, object> idDic, DbConnection conn = null, string anotherDatabaseAliasName = null)
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

                        s_logger.Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                yield return ToEntity(reader);
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

        public void Update(E entity, DbConnection conn = null, string anotherDatabaseAliasName = null)
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

                        s_logger.Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                        command.ExecuteNonQuery();
                    }
                }
            }), conn);
        }

        private static string sqlToDefineColumns(IColumn c)
        {
            string r = $"{c.ColumnName} {c.DataType}";
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

        public void UpgradeTable(VersionChangeUnit upgradePath, DbConnection conn = null)
        {
            ConnectionInternal(new Action<DbConnection>((connection) =>
            {
                using (var command = connection.CreateCommand())
                {
                    var newTable = new Table<E>(upgradePath.To);
                    var oldTable = new Table<E>(upgradePath.From);

                    using (var query = new Insert().Into.Table(newTable)
                                                                    .Columns(oldTable.Columns.Select(c => c.ColumnName))
                                                                    .Select.Columns(oldTable.Columns.Select(c => c.ColumnName)).From.Table(oldTable))
                    {
                        string sql = query.ToSql();
                        command.CommandText = sql;

                        s_logger.Debug($"{sql}");
                        command.ExecuteNonQuery();
                    }
                }
            }), conn);
        }
    }
}
