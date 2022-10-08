using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Entity;
using Homura.Test.TestFixture.Migration;
using Homura.Test.TestFixture.Migration.Plan;
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
            svManager.RegisterChangePlan(new Valkyrie_0_VersionChangePlan_VersionOrigin(VersioningMode.ByTick));
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

            svManager.RegisterChangePlan(new Valkyrie_1_VersionChangePlan_Version_1(VersioningMode.ByTick));

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

            svManager.RegisterChangePlan(new Valkyrie_1_VersionChangePlan_Version_2(VersioningMode.ByTick));

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

        [Test]
        public void ByAlterTable()
        {
            var svManager = new DataVersionManager();
            svManager.CurrentConnection = ConnectionManager.DefaultConnection;
            svManager.RegisterChangePlan(new Frey_VersionChangePlan_VersionOrigin(VersioningMode.ByAlterTable));
            svManager.SetDefault();

            svManager.UpgradeToTargetVersion();

            var dao = new FreyDao();
            dao.CurrentConnection = ConnectionManager.DefaultConnection;

            dao.Insert(new Frey()
            {
                Id = Guid.Empty,
                Item1 = "org_item1",
                Item2 = "org_item2",
            });

            Assert.That(dao.CountAll(), Is.EqualTo(1));

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Frey"));
                var items = dao.FindAll();
                Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_1
                Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items.First().Item3, Is.Null);

                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_0_1"));
            }
            svManager.RegisterChangePlan(new Frey_VersionChangePlan_Version_1(VersioningMode.ByAlterTable));
            svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Frey"));
                {
                    var items = dao.FindAll();
                    Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_1
                    Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                    Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                    Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                    Assert.That(items.First().Item3, Is.Null);
                }

                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Frey_0_1"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Frey_1"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Frey_1_1"));
            }
        }

        [Test]
        public void ByTick_then_ByAlterTable()
        {
            var svManager = new DataVersionManager();
            svManager.CurrentConnection = ConnectionManager.DefaultConnection;
            svManager.RegisterChangePlan(new Roki_VersionChangePlan_VersionOrigin(VersioningMode.ByTick));
            svManager.SetDefault();

            svManager.UpgradeToTargetVersion();

            var dao = new RokiDao(typeof(VersionOrigin));
            dao.CurrentConnection = ConnectionManager.DefaultConnection;

            dao.Insert(new Roki()
            {
                Id = Guid.Empty,
                Item1 = "org_item1",
                Item2 = "org_item2",
            });

            Assert.That(dao.CountAll(), Is.EqualTo(1));

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Roki"));
                var items = dao.FindAll();
                Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_1
                Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items.First().Item3, Is.Null);
            }
            svManager.RegisterChangePlan(new Roki_VersionChangePlan_Version_1(VersioningMode.ByTick | VersioningMode.DropTableCastedOff));
            svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Roki"));
                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Roki_1"));
                {
                    dao = new RokiDao(typeof(Version_1));
                    dao.CurrentConnection = ConnectionManager.DefaultConnection;
                    var items = dao.FindAll();
                    Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_1
                    Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                    Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                    Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                    Assert.That(items.First().Item3, Is.Null);
                }

                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Roki"));
                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Roki_1"));
            }

            svManager.RegisterChangePlan(new Roki_VersionChangePlan_Version_2(VersioningMode.ByAlterTable));
            svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Roki"));
                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Roki_1"));
                {
                    dao = new RokiDao(typeof(Version_2));
                    dao.CurrentConnection = ConnectionManager.DefaultConnection;
                    var items = dao.FindAll();
                    Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_1
                    Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                    Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                    Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                    Assert.That(items.First().Item3, Is.Null);
                }
            }
        }

        [Test]
        public void ByAlterTable_DropTableCastedOff()
        {
            var svManager = new DataVersionManager();
            svManager.CurrentConnection = ConnectionManager.DefaultConnection;
            svManager.RegisterChangePlan(new Frey_VersionChangePlan_VersionOrigin(VersioningMode.ByAlterTable | VersioningMode.DropTableCastedOff));
            svManager.SetDefault();

            svManager.UpgradeToTargetVersion();

            var dao = new FreyDao();
            dao.CurrentConnection = ConnectionManager.DefaultConnection;

            dao.Insert(new Frey()
            {
                Id = Guid.Empty,
                Item1 = "org_item1",
                Item2 = "org_item2",
            });

            Assert.That(dao.CountAll(), Is.EqualTo(1));

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Frey"));
                var items = dao.FindAll();
                Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_1
                Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items.First().Item3, Is.Null);

                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_0_1"));
            }
            svManager.RegisterChangePlan(new Frey_VersionChangePlan_Version_1(VersioningMode.ByAlterTable));
            svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Frey"));
                {
                    var items = dao.FindAll();
                    Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_1
                    Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                    Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                    Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                    Assert.That(items.First().Item3, Is.Null);
                }

                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Frey_0_1"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Frey_1"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Frey_1_1"));
            }
        }

        [Test]
        public void ByAlterTable_DeleteAllRecordInTableCastedOff()
        {
            var svManager = new DataVersionManager();
            svManager.CurrentConnection = ConnectionManager.DefaultConnection;
            svManager.RegisterChangePlan(new Frey_VersionChangePlan_VersionOrigin(VersioningMode.ByAlterTable | VersioningMode.DeleteAllRecordInTableCastedOff));
            svManager.SetDefault();

            svManager.UpgradeToTargetVersion();

            var dao = new FreyDao();
            dao.CurrentConnection = ConnectionManager.DefaultConnection;

            dao.Insert(new Frey()
            {
                Id = Guid.Empty,
                Item1 = "org_item1",
                Item2 = "org_item2",
            });

            Assert.That(dao.CountAll(), Is.EqualTo(1));

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Frey"));
                var items = dao.FindAll();
                Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_1
                Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items.First().Item3, Is.Null);

                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_0_1"));
            }
            svManager.RegisterChangePlan(new Frey_VersionChangePlan_Version_1(VersioningMode.ByAlterTable | VersioningMode.DeleteAllRecordInTableCastedOff));
            svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Frey"));
                {
                    var items = dao.FindAll();
                    Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_1
                    Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                    Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                    Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                    Assert.That(items.First().Item3, Is.Null);
                }

                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Frey_0_1"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Frey_1"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Frey_1_1"));
            }
        }

        [Test]
        public void DropTableCastedOff()
        {
            var svManager = new DataVersionManager();
            svManager.CurrentConnection = ConnectionManager.DefaultConnection;
            svManager.RegisterChangePlan(new Valkyrie_0_VersionChangePlan_VersionOrigin(VersioningMode.ByTick | VersioningMode.DropTableCastedOff));
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

            svManager.RegisterChangePlan(new Valkyrie_1_VersionChangePlan_Version_1(VersioningMode.ByTick | VersioningMode.DropTableCastedOff));

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

            svManager.RegisterChangePlan(new Valkyrie_1_VersionChangePlan_Version_2(VersioningMode.ByTick | VersioningMode.DropTableCastedOff));

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

        [Test]
        public void DeleteAllRecordInTableCastedOff()
        {
            var svManager = new DataVersionManager();
            svManager.CurrentConnection = ConnectionManager.DefaultConnection;
            svManager.RegisterChangePlan(new Valkyrie_0_VersionChangePlan_VersionOrigin(VersioningMode.ByTick | VersioningMode.DeleteAllRecordInTableCastedOff));
            svManager.SetDefault();

            svManager.UpgradeToTargetVersion();

            var dao = new Valkyrie_0_Dao(typeof(VersionOrigin));
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
                var items = dao.FindAll();
                Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_1
                Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items.First().Item3, Is.Null);

                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_0_1"));
            }

            svManager.RegisterChangePlan(new Valkyrie_1_VersionChangePlan_Version_1(VersioningMode.ByTick | VersioningMode.DeleteAllRecordInTableCastedOff));
            svManager.UpgradeToTargetVersion();

            var dao1 = new Valkyrie_1_Dao(typeof(VersionOrigin));
            dao1.CurrentConnection = ConnectionManager.DefaultConnection;
            dao1.Insert(new Valkyrie_1()
            {
                Id = Guid.Empty,
                Item1 = "org_item1",
                Item2 = "org_item2",
            });

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_0"));
                dao = new Valkyrie_0_Dao();
                var items = dao.FindAll();
                Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_1
                Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items.First().Item3, Is.Null);

                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_0_1"));

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_1"));
                dao1 = new Valkyrie_1_Dao(typeof(VersionOrigin));
                var items1 = dao1.FindAll();
                Assert.That(items1.Count(), Is.EqualTo(1)); //default version:Version_1
                Assert.That(items1.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items1.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items1.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items1.First().Item3, Is.Null);

                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_1_1"));
            }

            svManager.RegisterChangePlan(new Valkyrie_1_VersionChangePlan_Version_2(VersioningMode.ByTick | VersioningMode.DeleteAllRecordInTableCastedOff));
            svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_0"));
                var dao0 = new Valkyrie_0_Dao();
                var items0 = dao0.FindAll();
                Assert.That(items0.Count(), Is.EqualTo(1)); //default version:Version_1
                Assert.That(items0.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items0.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items0.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items0.First().Item3, Is.Null);

                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_0_1"));

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_1"));
                dao1 = new Valkyrie_1_Dao(typeof(VersionOrigin));
                var items1 = dao1.FindAll();
                Assert.That(items1.Count(), Is.EqualTo(0)); //default version:Version_2

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_1_1"));
                var dao2 = new Valkyrie_1_Dao(typeof(Version_1));
                var items2 = dao2.FindAll();
                Assert.That(items2.Count(), Is.EqualTo(1)); //default version:Version_2
                Assert.That(items2.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items2.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items2.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items2.First().Item3, Is.Null);

                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_1_2"));
            }
        }
    }
}
