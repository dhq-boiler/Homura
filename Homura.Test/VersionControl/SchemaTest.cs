

using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Migration;
using NUnit.Framework;
using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace Homura.Test.VersionControl
{
    [Category("Infrastructure")]
    [Category("IntegrationTest")]
    [TestFixture]
    public class SchemaTest
    {
        private string _filePath;

        [SetUp]
        public void Initialize()
        {
            _filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TableNameTest.db");
            ConnectionManager.SetDefaultConnection(Guid.Parse("95D40F7F-63BA-49D8-BD74-1B63F5531CB5"), $"Data Source={_filePath}", typeof(SQLiteConnection));
        }

        [Test]
        public void CreateTable_Specified_VersionOrigin()
        {
            var svManager = new DataVersionManager();
            svManager.SetDefault();

            //Create VersionOrigin
            var dao = new OriginDao(typeof(VersionOrigin));
            dao.CurrentConnection = ConnectionManager.DefaultConnection;
            dao.CreateTableIfNotExists();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                Assert.That(conn.GetTableNames(), Has.Exactly(1).EqualTo(dao.TableName));

                var columnNames = conn.GetColumnNames(dao.TableName);
                Assert.That(columnNames.Count(), Is.EqualTo(3));
                Assert.That(columnNames, Has.Exactly(1).EqualTo("Id"));
                Assert.That(columnNames, Has.Exactly(1).EqualTo("Item1"));
                Assert.That(columnNames, Has.Exactly(1).EqualTo("Item2"));
            }
        }

        [Test]
        public void CreateTable_DaoUseDefaultConstructor_VersionOrigin()
        {
            var svManager = new DataVersionManager();
            svManager.SetDefault();

            //Create VersionOrigin
            var dao = new OriginDao();
            dao.CurrentConnection = ConnectionManager.DefaultConnection;
            dao.CreateTableIfNotExists();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                Assert.That(conn.GetTableNames(), Has.Exactly(1).EqualTo(dao.TableName));

                var columnNames = conn.GetColumnNames(dao.TableName);
                Assert.That(columnNames.Count(), Is.EqualTo(3));
                Assert.That(columnNames, Has.Exactly(1).EqualTo("Id"));
                Assert.That(columnNames, Has.Exactly(1).EqualTo("Item1"));
                Assert.That(columnNames, Has.Exactly(1).EqualTo("Item2"));
            }
        }

        [Test]
        public void CreateTable_Specified_Version_1()
        {
            var svManager = new DataVersionManager();
            svManager.SetDefault();

            //Create Version_1
            var dao = new OriginDao(typeof(Version_1));
            dao.CurrentConnection = ConnectionManager.DefaultConnection;
            dao.CreateTableIfNotExists();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                Assert.That(conn.GetTableNames(), Has.Exactly(1).EqualTo(dao.TableName));

                var columnNames = conn.GetColumnNames(dao.TableName);
                Assert.That(columnNames.Count(), Is.EqualTo(4));
                Assert.That(columnNames, Has.Exactly(1).EqualTo("Id"));
                Assert.That(columnNames, Has.Exactly(1).EqualTo("Item1"));
                Assert.That(columnNames, Has.Exactly(1).EqualTo("Item2"));
                Assert.That(columnNames, Has.Exactly(1).EqualTo("Item3"));
            }
        }

        [Test]
        public void CreateTable_Specified_VersionOrigin_Version_1()
        {
            var svManager = new DataVersionManager();
            svManager.SetDefault();

            //1. Create VersionOrigin
            var dao = new OriginDao(typeof(VersionOrigin));
            dao.CurrentConnection = ConnectionManager.DefaultConnection;
            dao.CreateTableIfNotExists();

            //check Header(VersionOrigin)
            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                Assert.That(conn.GetTableNames(), Has.Exactly(1).EqualTo(dao.TableName));

                var columnNames = conn.GetColumnNames(dao.TableName);
                Assert.That(columnNames.Count(), Is.EqualTo(3));
                Assert.That(columnNames, Has.Exactly(1).EqualTo("Id"));
                Assert.That(columnNames, Has.Exactly(1).EqualTo("Item1"));
                Assert.That(columnNames, Has.Exactly(1).EqualTo("Item2"));
            }

            //Create Version_1
            dao = new OriginDao(typeof(Version_1));
            dao.CurrentConnection = ConnectionManager.DefaultConnection;
            dao.CreateTableIfNotExists();

            //check Header(VersionOrigin)
            dao = new OriginDao(typeof(VersionOrigin));
            dao.CurrentConnection = ConnectionManager.DefaultConnection;
            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                Assert.That(conn.GetTableNames(), Has.Exactly(1).EqualTo(dao.TableName));

                var columnNames = conn.GetColumnNames(dao.TableName);
                Assert.That(columnNames.Count(), Is.EqualTo(3));
                Assert.That(columnNames, Has.Exactly(1).EqualTo("Id"));
                Assert.That(columnNames, Has.Exactly(1).EqualTo("Item1"));
                Assert.That(columnNames, Has.Exactly(1).EqualTo("Item2"));
            }

            //check Header(Version_1)
            dao = new OriginDao(typeof(Version_1));
            dao.CurrentConnection = ConnectionManager.DefaultConnection;
            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                Assert.That(conn.GetTableNames(), Has.Exactly(1).EqualTo(dao.TableName));

                var columnNames = conn.GetColumnNames(dao.TableName);
                Assert.That(columnNames.Count(), Is.EqualTo(4));
                Assert.That(columnNames, Has.Exactly(1).EqualTo("Id"));
                Assert.That(columnNames, Has.Exactly(1).EqualTo("Item1"));
                Assert.That(columnNames, Has.Exactly(1).EqualTo("Item2"));
                Assert.That(columnNames, Has.Exactly(1).EqualTo("Item3"));
            }
        }

        [TearDown]
        public void TearDown()
        {
            ConnectionManager.DisposeDebris(Guid.Parse("95D40F7F-63BA-49D8-BD74-1B63F5531CB5"));
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }
    }
}
