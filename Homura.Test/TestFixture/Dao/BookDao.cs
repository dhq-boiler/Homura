using Homura.ORM;
using Homura.QueryBuilder.Iso.Dml;
using Homura.Test.TestFixture.Entity;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Homura.Test.TestFixture.Dao
{
    internal class BookDao : SQLiteBaseDao<Book>
    {
        private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

        public BookDao() : base()
        { }

        public BookDao(Type entityVersionType) : base(entityVersionType)
        { }

        public IEnumerable<Book> FindAll(string anotherDatabaseAliasName, DbConnection conn = null)
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
                    using (var query = new Select().Asterisk().From.Table(new Table<Book>() { Schema = anotherDatabaseAliasName }))
                    {
                        var sql = query.ToSql();
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
    }
}
