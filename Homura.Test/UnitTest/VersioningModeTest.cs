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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

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
        public async Task ByTick()
        {
            var svManager = new DataVersionManager();
            svManager.CurrentConnection = ConnectionManager.DefaultConnection;
            svManager.RegisterChangePlan(new Valkyrie_0_VersionChangePlan_VersionOrigin(VersioningMode.ByTick));
            svManager.SetDefault();

            await svManager.UpgradeToTargetVersion();

            var dao = new Valkyrie_0_Dao();
            dao.CurrentConnection = ConnectionManager.DefaultConnection;

            await dao.InsertAsync(new Valkyrie_0()
            {
                Id = Guid.Empty,
                Item1 = "org_item1",
                Item2 = "org_item2",
            });

            Assert.That(await dao.CountAllAsync(), Is.EqualTo(1));

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                await conn.OpenAsync();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_0"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_0_1"));
            }

            svManager.RegisterChangePlan(new Valkyrie_1_VersionChangePlan_Version_1(VersioningMode.ByTick));

            await svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                await conn.OpenAsync();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_1"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_1_1"));

                var items = await dao.FindAllAsync().ToListAsync();
                Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_1
                Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items.First().Item3, Is.Null);
            }

            svManager.RegisterChangePlan(new Valkyrie_1_VersionChangePlan_Version_2(VersioningMode.ByTick));

            await svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                await conn.OpenAsync();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_1"));
                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_1_1"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_1_2"));

                var items = await dao.FindAllAsync().ToListAsync();
                Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_2
                Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items.First().Item3, Is.Null);
            }
        }

        [Test]
        public async Task ByAlterTable()
        {
            var svManager = new DataVersionManager();
            svManager.CurrentConnection = ConnectionManager.DefaultConnection;
            svManager.RegisterChangePlan(new Frey_VersionChangePlan_VersionOrigin(VersioningMode.ByAlterTable));
            svManager.SetDefault();

            await svManager.UpgradeToTargetVersion();

            var dao = new FreyDao();
            dao.CurrentConnection = ConnectionManager.DefaultConnection;

            await dao.InsertAsync(new Frey()
            {
                Id = Guid.Empty,
                Item1 = "org_item1",
                Item2 = "org_item2",
            });

            Assert.That(await dao.CountAllAsync(), Is.EqualTo(1));

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                await conn.OpenAsync();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Frey"));
                var items = await dao.FindAllAsync().ToListAsync();
                Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_1
                Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items.First().Item3, Is.Null);

                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_0_1"));
            }
            svManager.RegisterChangePlan(new Frey_VersionChangePlan_Version_1(VersioningMode.ByAlterTable));
            await svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                await conn.OpenAsync();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Frey"));
                {
                    var items = await dao.FindAllAsync().ToListAsync();
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
        public async Task ByAlterTable_UpgradeToTargetVersion_2回実行()
        {
            var svManager = new DataVersionManager();
            svManager.CurrentConnection = ConnectionManager.DefaultConnection;
            svManager.RegisterChangePlan(new Frey_VersionChangePlan_VersionOrigin(VersioningMode.ByAlterTable));
            svManager.SetDefault();

            await svManager.UpgradeToTargetVersion();

            var dao = new FreyDao();
            dao.CurrentConnection = ConnectionManager.DefaultConnection;

            await dao.InsertAsync(new Frey()
            {
                Id = Guid.Empty,
                Item1 = "org_item1",
                Item2 = "org_item2",
            });

            Assert.That(await dao.CountAllAsync(), Is.EqualTo(1));

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                await conn.OpenAsync();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Frey"));
                var items = await dao.FindAllAsync().ToListAsync();
                Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_1
                Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items.First().Item3, Is.Null);

                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_0_1"));
            }
            svManager.RegisterChangePlan(new Frey_VersionChangePlan_Version_1(VersioningMode.ByAlterTable));
            await svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                await conn.OpenAsync();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Frey"));
                {
                    var items = await dao.FindAllAsync().ToListAsync();
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

            svManager = new DataVersionManager();
            svManager.CurrentConnection = ConnectionManager.DefaultConnection;
            svManager.RegisterChangePlan(new Frey_VersionChangePlan_VersionOrigin(VersioningMode.ByAlterTable));
            svManager.SetDefault();
            svManager.RegisterChangePlan(new Frey_VersionChangePlan_Version_1(VersioningMode.ByAlterTable));
            await svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                await conn.OpenAsync();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Frey"));
                {
                    var items = await dao.FindAllAsync().ToListAsync();
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
        public async Task ByTick_then_ByAlterTable()
        {
            var svManager = new DataVersionManager();
            svManager.CurrentConnection = ConnectionManager.DefaultConnection;
            svManager.RegisterChangePlan(new Roki_VersionChangePlan_VersionOrigin(VersioningMode.ByTick));
            svManager.SetDefault();

            await svManager.UpgradeToTargetVersion();

            var dao = new RokiDao(typeof(VersionOrigin));
            dao.CurrentConnection = ConnectionManager.DefaultConnection;

            await dao.InsertAsync(new Roki()
            {
                Id = Guid.Empty,
                Item1 = "org_item1",
                Item2 = "org_item2",
            });

            Assert.That(await dao.CountAllAsync(), Is.EqualTo(1));

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                await conn.OpenAsync();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Roki"));
                var items = await dao.FindAllAsync().ToListAsync();
                Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_1
                Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items.First().Item3, Is.Null);
            }
            svManager.RegisterChangePlan(new Roki_VersionChangePlan_Version_1(VersioningMode.ByTick | VersioningMode.DropTableCastedOff));
            await svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                await conn.OpenAsync();

                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Roki"));
                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Roki_1"));
                {
                    dao = new RokiDao(typeof(Version_1));
                    dao.CurrentConnection = ConnectionManager.DefaultConnection;
                    var items = await dao.FindAllAsync().ToListAsync();
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
            await svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                await conn.OpenAsync();

                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Roki"));
                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Roki_1"));
                {
                    dao = new RokiDao(DataVersionManager.DefaultSchemaVersion);
                    dao.CurrentConnection = ConnectionManager.DefaultConnection;
                    var items = await dao.FindAllAsync().ToListAsync();
                    Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_1
                    Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                    Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                    Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                    Assert.That(items.First().Item3, Is.Null);
                }
            }
        }

        [Test]
        public async Task ByAlterTable_DropTableCastedOff()
        {
            var svManager = new DataVersionManager();
            svManager.CurrentConnection = ConnectionManager.DefaultConnection;
            svManager.RegisterChangePlan(new Frey_VersionChangePlan_VersionOrigin(VersioningMode.ByAlterTable | VersioningMode.DropTableCastedOff));
            svManager.SetDefault();

            await svManager.UpgradeToTargetVersion();

            var dao = new FreyDao();
            dao.CurrentConnection = ConnectionManager.DefaultConnection;

            await dao.InsertAsync(new Frey()
            {
                Id = Guid.Empty,
                Item1 = "org_item1",
                Item2 = "org_item2",
            });

            Assert.That(await dao.CountAllAsync(), Is.EqualTo(1));

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                await conn.OpenAsync();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Frey"));
                var items = await dao.FindAllAsync().ToListAsync();
                Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_1
                Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items.First().Item3, Is.Null);

                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_0_1"));
            }
            svManager.RegisterChangePlan(new Frey_VersionChangePlan_Version_1(VersioningMode.ByAlterTable));
            await svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                await conn.OpenAsync();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Frey"));
                {
                    var items = await dao.FindAllAsync().ToListAsync();
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
        public async Task ByAlterTable_DeleteAllRecordInTableCastedOff()
        {
            var svManager = new DataVersionManager();
            svManager.CurrentConnection = ConnectionManager.DefaultConnection;
            svManager.RegisterChangePlan(new Frey_VersionChangePlan_VersionOrigin(VersioningMode.ByAlterTable | VersioningMode.DeleteAllRecordInTableCastedOff));
            svManager.SetDefault();

            await svManager.UpgradeToTargetVersion();

            var dao = new FreyDao();
            dao.CurrentConnection = ConnectionManager.DefaultConnection;

            await dao.InsertAsync(new Frey()
            {
                Id = Guid.Empty,
                Item1 = "org_item1",
                Item2 = "org_item2",
            });

            Assert.That(await dao.CountAllAsync(), Is.EqualTo(1));

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                await conn.OpenAsync();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Frey"));
                var items = await dao.FindAllAsync().ToListAsync();
                Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_1
                Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items.First().Item3, Is.Null);

                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_0_1"));
            }
            svManager.RegisterChangePlan(new Frey_VersionChangePlan_Version_1(VersioningMode.ByAlterTable | VersioningMode.DeleteAllRecordInTableCastedOff));
            await svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                await conn.OpenAsync();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Frey"));
                {
                    var items = await dao.FindAllAsync().ToListAsync();
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
        public async Task DropTableCastedOff()
        {
            var svManager = new DataVersionManager();
            svManager.CurrentConnection = ConnectionManager.DefaultConnection;
            svManager.RegisterChangePlan(new Valkyrie_0_VersionChangePlan_VersionOrigin(VersioningMode.ByTick | VersioningMode.DropTableCastedOff));
            svManager.SetDefault();

            await svManager.UpgradeToTargetVersion();

            var dao = new Valkyrie_0_Dao();
            dao.CurrentConnection = ConnectionManager.DefaultConnection;

            await dao.InsertAsync(new Valkyrie_0()
            {
                Id = Guid.Empty,
                Item1 = "org_item1",
                Item2 = "org_item2",
            });

            Assert.That(await dao.CountAllAsync(), Is.EqualTo(1));

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                await conn.OpenAsync();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_0"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_0_1"));
            }

            svManager.RegisterChangePlan(new Valkyrie_1_VersionChangePlan_Version_1(VersioningMode.ByTick | VersioningMode.DropTableCastedOff));

            await svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                await conn.OpenAsync();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_0"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_0_1"));
                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_1"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_1_1"));

                var items = await dao.FindAllAsync().ToListAsync();
                Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_1
                Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items.First().Item3, Is.Null);
            }

            svManager.RegisterChangePlan(new Valkyrie_1_VersionChangePlan_Version_2(VersioningMode.ByTick | VersioningMode.DropTableCastedOff));

            await svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                await conn.OpenAsync();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_0"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_0_1"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_1"));
                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_1_1"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_1_2"));

                var items = await dao.FindAllAsync().ToListAsync();
                Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_2
                Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items.First().Item3, Is.Null);
            }
        }

        [Test]
        public async Task DeleteAllRecordInTableCastedOff()
        {
            var svManager = new DataVersionManager();
            svManager.CurrentConnection = ConnectionManager.DefaultConnection;
            svManager.RegisterChangePlan(new Valkyrie_0_VersionChangePlan_VersionOrigin(VersioningMode.ByTick | VersioningMode.DeleteAllRecordInTableCastedOff));
            svManager.SetDefault();

            await svManager.UpgradeToTargetVersion();

            var dao = new Valkyrie_0_Dao(typeof(VersionOrigin));
            dao.CurrentConnection = ConnectionManager.DefaultConnection;

            await dao.InsertAsync(new Valkyrie_0()
            {
                Id = Guid.Empty,
                Item1 = "org_item1",
                Item2 = "org_item2",
            });

            Assert.That(await dao.CountAllAsync(), Is.EqualTo(1));

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                await conn.OpenAsync();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_0"));
                var items = await dao.FindAllAsync().ToListAsync();
                Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_1
                Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items.First().Item3, Is.Null);

                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_0_1"));
            }

            svManager.RegisterChangePlan(new Valkyrie_1_VersionChangePlan_Version_1(VersioningMode.ByTick | VersioningMode.DeleteAllRecordInTableCastedOff));
            await svManager.UpgradeToTargetVersion();

            var dao1 = new Valkyrie_1_Dao(typeof(VersionOrigin));
            dao1.CurrentConnection = ConnectionManager.DefaultConnection;
            await dao1.InsertAsync(new Valkyrie_1()
            {
                Id = Guid.Empty,
                Item1 = "org_item1",
                Item2 = "org_item2",
            });

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                await conn.OpenAsync();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_0"));
                dao = new Valkyrie_0_Dao();
                var items = await dao.FindAllAsync().ToListAsync();
                Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_1
                Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items.First().Item3, Is.Null);

                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_0_1"));

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_1"));
                dao1 = new Valkyrie_1_Dao(typeof(VersionOrigin)); 
                var items1 = await dao1.FindAllAsync().ToListAsync();
                Assert.That(items1.Count(), Is.EqualTo(1)); //default version:Version_1
                Assert.That(items1.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items1.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items1.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items1.First().Item3, Is.Null);

                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_1_1"));
            }

            svManager.RegisterChangePlan(new Valkyrie_1_VersionChangePlan_Version_2(VersioningMode.ByTick | VersioningMode.DeleteAllRecordInTableCastedOff));
            await svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                await conn.OpenAsync();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_0"));
                var dao0 = new Valkyrie_0_Dao();
                var items0 = await dao.FindAllAsync().ToListAsync();
                Assert.That(items0.Count(), Is.EqualTo(1)); //default version:Version_1
                Assert.That(items0.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items0.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items0.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items0.First().Item3, Is.Null);

                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Valkyrie_0_1"));

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_1"));
                dao1 = new Valkyrie_1_Dao(typeof(VersionOrigin));
                var items1 = await dao1.FindAllAsync().ToListAsync();
                Assert.That(items1.Count(), Is.EqualTo(0)); //default version:Version_2

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Valkyrie_1_1"));
                var dao2 = new Valkyrie_1_Dao(typeof(Version_1));
                var items2 = await dao2.FindAllAsync().ToListAsync();
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
