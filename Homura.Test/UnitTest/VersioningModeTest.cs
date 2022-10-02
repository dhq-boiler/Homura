using Homura.ORM;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Entity;
using NUnit.Framework;
using Sunctum.Domain.Data.Dao.Migration.Plan;
using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace Homura.Test.UnitTest
{
    [TestFixture]
    public class VersioningModeTest
    {
        private string _filePath;

        [SetUp]
        public void Initialize()
        {
            _filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "VersioningModeTest.db");
            ConnectionManager.SetDefaultConnection(Guid.Parse("7940DC36-2113-4C32-B7FB-9C969B1DBA02"), $"Data Source={_filePath}", typeof(SQLiteConnection));
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }

        [Test]
        public void ByTick()
        {
            var svManager = new DataVersionManager();
            svManager.CurrentConnection = ConnectionManager.DefaultConnection;
            svManager.Mode = VersioningMode.ByTick;
            svManager.RegisterChangePlan(new Valkyrie_0_VersionChangePlan_VersionOrigin());
            svManager.SetDefault();

            svManager.UpgradeToTargetVersion();

            var dao = new Valkyrie_0_Dao();
            dao.CurrentConnection = ConnectionManager.DefaultConnection;

            dao.Insert(new Valkyrie_0()
            {
                Id = Guid.Empty,
                Item1 = "org_item1",
                Item2 = "org_item2",
            });

            Assert.That(dao.CountAll(), Is.EqualTo(1));

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_0"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_0_1"));
            }

            svManager.RegisterChangePlan(new Valkyrie_1_VersionChangePlan_Version_1());

            svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_1"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_1_1"));

                var items = dao.FindAll();
                Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_1
                Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items.First().Item3, Is.Null);
            }

            svManager.RegisterChangePlan(new Valkyrie_1_VersionChangePlan_Version_2());

            svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_1"));
                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_1_1"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_1_2"));

                var items = dao.FindAll();
                Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_2
                Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items.First().Item3, Is.Null);
            }
        }

        [Ignore("not implemented")]
        [Test]
        public void ByAlterTable()
        {

        }

        [Test]
        public void DropTableCastedOff()
        {
            var svManager = new DataVersionManager();
            svManager.CurrentConnection = ConnectionManager.DefaultConnection;
            svManager.Mode = VersioningMode.ByTick | VersioningMode.DropTableCastedOff;
            svManager.RegisterChangePlan(new Valkyrie_0_VersionChangePlan_VersionOrigin());
            svManager.SetDefault();

            svManager.UpgradeToTargetVersion();

            var dao = new Valkyrie_0_Dao();
            dao.CurrentConnection = ConnectionManager.DefaultConnection;

            dao.Insert(new Valkyrie_0()
            {
                Id = Guid.Empty,
                Item1 = "org_item1",
                Item2 = "org_item2",
            });

            Assert.That(dao.CountAll(), Is.EqualTo(1));

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_0"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_0_1"));
            }

            svManager.RegisterChangePlan(new Valkyrie_1_VersionChangePlan_Version_1());

            svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_0"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_0_1"));
                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_1"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_1_1"));

                var items = dao.FindAll();
                Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_1
                Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items.First().Item3, Is.Null);
            }

            svManager.RegisterChangePlan(new Valkyrie_1_VersionChangePlan_Version_2());

            svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_0"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_0_1"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_1"));
                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_1_1"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_1_2"));

                var items = dao.FindAll();
                Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_2
                Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items.First().Item3, Is.Null);
            }
        }

        [Ignore("not implemented")]
        [Test]
        public void DeleteAllRecordInTableCastedOff()
        {

        }
    }
}
