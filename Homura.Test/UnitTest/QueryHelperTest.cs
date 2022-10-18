using Dapper;
using Homura.ORM;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Entity;
using NUnit.Framework;
using System;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

namespace Homura.Test.UnitTest
{
    [TestFixture]
    public class QueryHelperTest
    {
        private string _filePath = "QueryHelperTest.db";
        private Guid _guid = Guid.NewGuid();

        [Test]
        public async Task ConnectionInternalAsync_AfterDisposed()
        {
            ConnectionManager.SetDefaultConnection(_guid, $"Data Source={_filePath}", typeof(SQLiteConnection));

            var dao = new AlphaDao();
            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                DirectQuery.RunQuery(conn, (conn) =>
                {
                    string sql = $"create table if not exists Alpha(ID NUMERIC PRIMARY KEY, Item1 TEXT, Item2 TEXT, Item3 NUMERIC, Item4 TEXT, Item5 INTEGER, Item6 INTEGER, Item7 TEXT, Item8 INTEGER, Item9 INTEGER)";
                    conn.Execute(sql);
                });
                await conn.DisposeAsync();
                var alpha = new Alpha();
                await dao.InsertOrReplaceAsync(alpha, conn);
            }
        }

        [TearDown]
        public void _TearDown()
        {
            ConnectionManager.DisposeDebris(_guid);

            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }
    }
}
