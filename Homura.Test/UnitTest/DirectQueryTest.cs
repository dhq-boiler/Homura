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
        private string _filePath = "DirectQueryTest";

        [Test]
        public void RunQuery()
        {
            DirectQuery.RunQuery(new SQLiteConnection($"Data Source={_filePath}"), (conn) =>
            {
                string sql = $"create table if not exists Alpha(ID NUMERIC PRIMARY KEY, Title TEXT NOT NULL, AuthorID NUMERIC, PublishDate NUMERIC, ByteSize INTEGER)";
                conn.Execute(sql);
            });
        }

        [Test]
        public async Task RunQueryAsync()
        {
            await DirectQuery.RunQueryAsync(new SQLiteConnection($"Data Source={_filePath}"), async (conn) =>
            {
                string sql = $"create table if not exists Alpha(ID NUMERIC PRIMARY KEY, Title TEXT NOT NULL, AuthorID NUMERIC, PublishDate NUMERIC, ByteSize INTEGER)";
                await conn.ExecuteAsync(sql);
            });
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
