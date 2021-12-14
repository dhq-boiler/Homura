using Homura.ORM;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Entity;
using Homura.Test.TestFixture.Migration.Plan;
using Moq;
using NUnit.Framework;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace Homura.Test.UnitTest
{
    [TestFixture]
    public class DatabaseIsLockTest
    {
        private string _filePath;

        [SetUp]
        public void Initialize()
        {
            _filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TableNameTest.db");
            ConnectionManager.SetDefaultConnection($"Data Source={_filePath}", typeof(SQLiteConnection));
        }

        [Test]
        public void タイムアウトするはず()
        {
            var dvManager = new DataVersionManager();
            dvManager.CurrentConnection = ConnectionManager.DefaultConnection;
            dvManager.Mode = VersioningStrategy.ByTable;
            dvManager.RegisterChangePlan(new OriginChangePlan_VersionOrigin());
            dvManager.SetDefault();

            var dao = new OriginDao();
            dao.CurrentConnection = ConnectionManager.DefaultConnection;
            dao.CreateTableIfNotExists();

            var mock = new Mock<IDbConnection>();
            var mock2 = new Mock<IDbCommand>();
            var mock3 = new Mock<IDbDataParameter>();
            var mock4 = new Mock<IDataParameterCollection>();
            mock.Setup(x => x.CreateCommand()).Returns(mock2.Object);
            mock2.Setup(x => x.ExecuteNonQuery()).Throws(new SQLiteException("database is lock."));
            mock2.Setup(x => x.CreateParameter()).Returns(mock3.Object);
            mock2.SetupGet(x => x.Parameters).Returns(mock4.Object);

            Assert.Throws<TimeoutException>(() =>
                dao.Insert(new Origin()
                {
                    Id = Guid.Empty,
                    Item1 = "org_item1",
                    Item2 = "org_item2",
                }, mock.Object, timeout: TimeSpan.FromSeconds(10)));
        }

        [Test]
        public void タイムアウトしないはず()
        {
            var dvManager = new DataVersionManager();
            dvManager.CurrentConnection = ConnectionManager.DefaultConnection;
            dvManager.Mode = VersioningStrategy.ByTable;
            dvManager.RegisterChangePlan(new OriginChangePlan_VersionOrigin());
            dvManager.SetDefault();

            var dao = new OriginDao();
            dao.CurrentConnection = ConnectionManager.DefaultConnection;
            dao.CreateTableIfNotExists();

            var mock = new Mock<IDbConnection>();
            var mock2 = new Mock<IDbCommand>();
            var mock3 = new Mock<IDbDataParameter>();
            var mock4 = new Mock<IDataParameterCollection>();
            mock.Setup(x => x.CreateCommand()).Returns(mock2.Object);
            mock2.Setup(x => x.ExecuteNonQuery()).Returns(1);
            mock2.Setup(x => x.CreateParameter()).Returns(mock3.Object);
            mock2.SetupGet(x => x.Parameters).Returns(mock4.Object);

            Assert.DoesNotThrow(() =>
                dao.Insert(new Origin()
                {
                    Id = Guid.Empty,
                    Item1 = "org_item1",
                    Item2 = "org_item2",
                }, mock.Object, timeout: TimeSpan.FromSeconds(10)));
        }
    }
}
