

using Homura.Extensions;
using Homura.ORM;
using Homura.QueryBuilder.Iso.Dml;
using Homura.QueryBuilder.Vendor.SQLite.Dml;
using Homura.Test.TestFixture.Entity;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace Homura.Test.TestFixture.Dao
{
    public class PageDao : SQLiteBaseDao<Page>
    {
        private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

        public PageDao() : base()
        { }

        public PageDao(Type entityVersionType) : base(entityVersionType)
        { }

        protected override void VerifyColumnDefinitions(IDbConnection conn)
        {
            var columnDefinitions = GetColumnDefinitions(conn);
            foreach (var column in Columns)
            {
                if (!columnDefinitions.Contains(column))
                {
                    var targetColumn = columnDefinitions.SingleOrDefault(c => c.ColumnName == column.ColumnName);
                    if (targetColumn is null)
                    {
                        throw new NotMatchColumnException($"{TableName}.{column.ColumnName} DataType client:{column.DBDataType}, but database didn't have {TableName}.{column.ColumnName}");
                    }
                    if (targetColumn.Order != column.Order)
                    {
                        var overridedColumn = new OverridedColumn((Column)column, newOrder: targetColumn.Order);
                        OverridedColumns.Add(overridedColumn);
                        s_logger.Debug($"{TableName}.{column.ColumnName} overrided:{overridedColumn}");
                    }
                }
            }
        }

        public IEnumerable<Page> FindByBookIdTop1(Guid bookID, IDbConnection conn = null)
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
                    using (var query = new Select().Asterisk()
                                                   .From.Table(new Table<Page>())
                                                   .Where.Column("BookID").EqualTo.Value(bookID)
                                                   .OrderBy.Column("PageIndex")
                                                   .Limit(1))
                    {
                        string sql = query.ToSql();
                        command.CommandText = sql;
                        query.SetParameters(command);

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

        public IEnumerable<Page> FindAll(string anotherDatabaseAliasName, IDbConnection conn = null)
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
                    using (var query = new Select().Asterisk()
                                                   .From.Table(new Table<Page>() { Schema = anotherDatabaseAliasName }))
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

        public IEnumerable<Page> FindBy(string anotherDatabaseAliasName, Dictionary<string, object> idDic, DbConnection conn = null)
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
                    using (var query = new Select().Asterisk()
                                                   .From.Table(new Table<Page>() { Schema = anotherDatabaseAliasName })
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

        public void IncrementPageIndex(Guid bookID, IDbConnection conn = null)
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
                    using (var query = new Update().Table(new Table<Page>().Name)
                                                   .Set.Column("PageIndex").EqualTo.Expression("PageIndex + 1")
                                                   .Where.Column("BookID").EqualTo.Value(bookID))
                    {
                        string sql = query.ToSql();
                        command.CommandText = sql;
                        query.SetParameters(command);

                        command.ExecuteNonQuery();
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

        protected override Page ToEntity(IDataRecord reader)
        {
            Guid id = reader.SafeGetGuid("ID", Table);
            string title = reader.SafeGetString("Title", Table);
            Guid bookid = reader.SafeGetGuid("BookID", Table);
            Guid imageid = reader.SafeGetGuid("ImageID", Table);
            int pageindex = reader.SafeGetInt("PageIndex", Table);

            return new Page()
            {
                ID = id,
                Title = title,
                BookID = bookid,
                ImageID = imageid,
                PageIndex = pageindex,
            };
        }
    }
}
