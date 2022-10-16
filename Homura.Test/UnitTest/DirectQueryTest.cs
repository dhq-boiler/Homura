using Dapper;
using Homura.ORM;
using NUnit.Framework;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

namespace Homura.Test.UnitTest
{
    [TestFixture]
    public class DirectQueryTest
    {
        private string _filePath = "DirectQueryTest.db";

        [Test]
        public void RunQuery()
        {
            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();
                DirectQuery.RunQuery(conn, (conn) =>
                {
                    string sql = $"create table if not exists Alpha(ID NUMERIC PRIMARY KEY, Title TEXT NOT NULL, AuthorID NUMERIC, PublishDate NUMERIC, ByteSize INTEGER)";
                    conn.Execute(sql);
                });
            }
        }

        [Test]
        public void RunQuery_long()
        {
            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();
                var ret = DirectQuery.RunQuery<long>(conn, (conn) =>
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "select 1";
                        return (long)cmd.ExecuteScalar();
                    }
                });
                Assert.That(ret, Is.EqualTo(1));
            }
        }

        [Test]
        public async Task RunQueryAsync()
        {
            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();
                await DirectQuery.RunQueryAsync(conn, async (conn) =>
                {
                    string sql = $"create table if not exists Alpha(ID NUMERIC PRIMARY KEY, Title TEXT NOT NULL, AuthorID NUMERIC, PublishDate NUMERIC, ByteSize INTEGER)";
                    await conn.ExecuteAsync(sql);
                });
            }
        }

        [Test]
        public async Task RunQueryAsync_long()
        {
            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                await conn.OpenAsync();
                var ret = await DirectQuery.RunQueryAsync<long>(conn, async (conn) =>
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "select 1";
                        return (long) await cmd.ExecuteScalarAsync();
                    }
                });
                Assert.That(ret, Is.EqualTo(1));
            }
        }

        [TearDown]
        public void _TearDown()
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }
    }
}
