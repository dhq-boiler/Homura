

using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Entity;
using Homura.Test.TestFixture.Migration;
using Homura.Test.TestFixture.Migration.Plan;
using NUnit.Framework;
using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Sunctum.Infrastructure.Test.IntegrationTest.Data.Rdbms.VersionControl
{
    [Category("Infrastructure")]
    [Category("IntegrationTest")]
    [TestFixture]
    public class TableMigrationTest
    {
        private string _filePath;

        [SetUp]
        public void Initialize()
        {
            _filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TableMigrationTest.db");
            ConnectionManager.SetDefaultConnection($"Data Source={_filePath}", typeof(SQLiteConnection));
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }

        [Test]
        public void ByTable_Origin_VersionOrigin_To_Version_1()
        {
            var dvManager = new DataVersionManager();
            dvManager.CurrentConnection = ConnectionManager.DefaultConnection;
            dvManager.Mode = VersioningStrategy.ByTable;
            dvManager.RegisterChangePlan(new OriginChangePlan_VersionOrigin());
            dvManager.SetDefault();

            var dao = new OriginDao();
            dao.CurrentConnection = ConnectionManager.DefaultConnection;
            dao.CreateTableIfNotExists();

            dao.Insert(new Origin()
            {
                Id = Guid.Empty,
                Item1 = "org_item1",
                Item2 = "org_item2",
            });

            Assert.That(dao.CountAll(), Is.EqualTo(1));

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Origin"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Origin_1"));
            }

            dvManager.RegisterChangePlan(new OriginChangePlan_Version_1());
            var plan = DataVersionManager.DefaultSchemaVersion.GetPlan(typeof(Origin), new Version_1());
            plan.UpgradeToTargetVersion(ConnectionManager.DefaultConnection);

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Origin"));
                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Origin_1"));

                var items = dao.FindAll();
                Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_1
                Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items.First().Item3, Is.Null);
            }
        }

        [Test]
        public void ByTable_Header_Detail_ComplexCase()
        {
            var dvManager = new DataVersionManager();
            dvManager.CurrentConnection = ConnectionManager.DefaultConnection;
            dvManager.Mode = VersioningStrategy.ByTable;
            //before: Header, Detailの変更プランを登録
            dvManager.RegisterChangePlan(new HeaderChangePlan_VersionOrigin());
            dvManager.RegisterChangePlan(new DetailChangePlan_VersionOrigin());
            dvManager.SetDefault();

            //before:テーブルは1つも存在しない
            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Header"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Header_1"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Header_2"));

                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Detail"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Detail_1"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Detail_2"));
            }

            dvManager.UpgradeToTargetVersion();

            //after: Header, Detailテーブルが存在する
            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                var tablenames = conn.GetTableNames();

                Assert.That(tablenames, Has.One.EqualTo("Header"));
                Assert.That(tablenames, Has.None.EqualTo("Header_1"));
                Assert.That(tablenames, Has.None.EqualTo("Header_2"));

                Assert.That(tablenames, Has.One.EqualTo("Detail"));
                Assert.That(tablenames, Has.None.EqualTo("Detail_1"));
                Assert.That(tablenames, Has.None.EqualTo("Detail_2"));
            }

            //before: Headerテーブルに1レコード追加
            var headerDao = new HeaderDao(typeof(VersionOrigin));
            headerDao.CurrentConnection = ConnectionManager.DefaultConnection;
            headerDao.Insert(new Header()
            {
                Id = Guid.Empty,
                Item1 = "org_item1",
                Item2 = "org_item2",
            });

            //before: Detailテーブルに1レコード追加
            var detailDao = new DetailDao(typeof(VersionOrigin));
            detailDao.CurrentConnection = ConnectionManager.DefaultConnection;
            detailDao.Insert(new Detail()
            {
                Id = Guid.Empty,
                Item1 = "org_item1",
                Item2 = "org_item2",
            });

            Assert.That(headerDao.CountAll(), Is.EqualTo(1));
            Assert.That(detailDao.CountAll(), Is.EqualTo(1));

            //before: Header, Detailテーブルが存在する
            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                var tablenames = conn.GetTableNames();

                Assert.That(tablenames, Has.One.EqualTo("Header"));
                Assert.That(tablenames, Has.None.EqualTo("Header_1"));
                Assert.That(tablenames, Has.None.EqualTo("Header_2"));

                Assert.That(tablenames, Has.One.EqualTo("Detail"));
                Assert.That(tablenames, Has.None.EqualTo("Detail_1"));
                Assert.That(tablenames, Has.None.EqualTo("Detail_2"));
            }

            //before: Header_1, Detail_1, Header_2の変更プランを登録
            dvManager.RegisterChangePlan(new HeaderChangePlan_Version_1());
            dvManager.RegisterChangePlan(new DetailChangePlan_Version_1());
            dvManager.RegisterChangePlan(new HeaderChangePlan_Version_2());
            dvManager.UpgradeToTargetVersion();

            headerDao = new HeaderDao(typeof(Version_2));
            headerDao.CurrentConnection = ConnectionManager.DefaultConnection;
            detailDao = new DetailDao(typeof(Version_1));
            detailDao.CurrentConnection = ConnectionManager.DefaultConnection;

            //after: Header, Header_1, Header_2のテーブルが存在する
            //after: Detail, Detail_1のテーブルが存在する
            //after: Header_3, Detail_2のテーブルは存在しない
            //after: Header2, Detail_1のレコード数は1件である
            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                var tablenames = conn.GetTableNames();

                Assert.That(tablenames, Has.One.EqualTo("Header"));
                Assert.That(tablenames, Has.One.EqualTo("Header_1"));
                Assert.That(tablenames, Has.One.EqualTo("Header_2"));
                Assert.That(tablenames, Has.None.EqualTo("Header_3"));

                Assert.That(tablenames, Has.One.EqualTo("Detail"));
                Assert.That(tablenames, Has.One.EqualTo("Detail_1"));
                Assert.That(tablenames, Has.None.EqualTo("Detail_2"));

                var items_h = headerDao.FindAll();
                Assert.That(items_h.Count(), Is.EqualTo(1));
                Assert.That(items_h.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items_h.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items_h.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items_h.First().Item3, Is.Null);

                var items_d = detailDao.FindAll();
                Assert.That(items_d.Count(), Is.EqualTo(1));
                Assert.That(items_d.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items_d.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items_d.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items_d.First().Item3, Is.Null);
                Assert.That(items_d.First().Item4, Is.Null);
            }
        }

        [Test]
        public void ByTick_Origin_VersinOrigin_To_Version_1()
        {
            var svManager = new DataVersionManager();
            svManager.CurrentConnection = ConnectionManager.DefaultConnection;
            svManager.Mode = VersioningStrategy.ByTick;
            var registeringPlan = new ChangePlanByVersion<VersionOrigin>();
            registeringPlan.AddVersionChangePlan(new OriginChangePlan_VersionOrigin());
            svManager.RegisterChangePlan(registeringPlan);
            svManager.SetDefault();

            svManager.UpgradeToTargetVersion();

            var dao = new OriginDao();
            dao.CurrentConnection = ConnectionManager.DefaultConnection;

            dao.Insert(new Origin()
            {
                Id = Guid.Empty,
                Item1 = "org_item1",
                Item2 = "org_item2",
            });

            Assert.That(dao.CountAll(), Is.EqualTo(1));

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Origin"));
                Assert.That(conn.GetTableNames(), Has.None.EqualTo("Origin_1"));
            }

            var registeringPlan1 = new ChangePlanByVersion<Version_1>();
            registeringPlan1.AddVersionChangePlan(new OriginChangePlan_Version_1());
            svManager.RegisterChangePlan(registeringPlan1);

            svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Origin"));
                Assert.That(conn.GetTableNames(), Has.One.EqualTo("Origin_1"));

                var items = dao.FindAll();
                Assert.That(items.Count(), Is.EqualTo(1)); //default version:Version_1
                Assert.That(items.First().Id, Is.EqualTo(Guid.Empty));
                Assert.That(items.First().Item1, Is.EqualTo("org_item1"));
                Assert.That(items.First().Item2, Is.EqualTo("org_item2"));
                Assert.That(items.First().Item3, Is.Null);
            }
        }

        [Test]
        public void ByVersion_Alpha_Beta_Gamma_ComplexCase()
        {
            var svManager = new DataVersionManager();
            svManager.CurrentConnection = ConnectionManager.DefaultConnection;
            svManager.Mode = VersioningStrategy.ByTick;
            var registeringPlan = new ChangePlanByVersion<VersionOrigin>();
            registeringPlan.AddVersionChangePlan(new AlphaChangePlan_VersionOrigin());
            registeringPlan.AddVersionChangePlan(new BetaChangePlan_VersionOrigin());
            registeringPlan.AddVersionChangePlan(new GammaChangePlan_VersionOrigin());
            svManager.RegisterChangePlan(registeringPlan);
            svManager.SetDefault();
            svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                var tablenames = conn.GetTableNames();

                Assert.That(tablenames, Has.One.EqualTo("Alpha"));
                Assert.That(tablenames, Has.None.EqualTo("Alpha_1"));
                Assert.That(tablenames, Has.None.EqualTo("Alpha_2"));

                Assert.That(tablenames, Has.One.EqualTo("Beta"));
                Assert.That(tablenames, Has.None.EqualTo("Beta_1"));
                Assert.That(tablenames, Has.None.EqualTo("Beta_2"));

                Assert.That(tablenames, Has.One.EqualTo("Gamma"));
                Assert.That(tablenames, Has.None.EqualTo("Gamma_1"));
                Assert.That(tablenames, Has.None.EqualTo("Gamma_2"));
            }

            var registeringPlan1 = new ChangePlanByVersion<Version_1>();
            registeringPlan1.AddVersionChangePlan(new AlphaChangePlan_Version_1());
            svManager.RegisterChangePlan(registeringPlan1);
            svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                var tablenames = conn.GetTableNames();

                Assert.That(tablenames, Has.One.EqualTo("Alpha"));
                Assert.That(tablenames, Has.One.EqualTo("Alpha_1"));
                Assert.That(tablenames, Has.None.EqualTo("Alpha_2"));

                Assert.That(tablenames, Has.One.EqualTo("Beta"));
                Assert.That(tablenames, Has.None.EqualTo("Beta_1"));
                Assert.That(tablenames, Has.None.EqualTo("Beta_2"));

                Assert.That(tablenames, Has.One.EqualTo("Gamma"));
                Assert.That(tablenames, Has.None.EqualTo("Gamma_1"));
                Assert.That(tablenames, Has.None.EqualTo("Gamma_2"));
            }

            var registeringPlan2 = new ChangePlanByVersion<Version_2>();
            registeringPlan2.AddVersionChangePlan(new AlphaChangePlan_Version_2());
            registeringPlan2.AddVersionChangePlan(new BetaChangePlan_Version_1());
            svManager.RegisterChangePlan(registeringPlan2);
            svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                var tablenames = conn.GetTableNames();

                Assert.That(tablenames, Has.One.EqualTo("Alpha"));
                Assert.That(tablenames, Has.One.EqualTo("Alpha_1"));
                Assert.That(tablenames, Has.One.EqualTo("Alpha_2"));

                Assert.That(tablenames, Has.One.EqualTo("Beta"));
                Assert.That(tablenames, Has.One.EqualTo("Beta_1"));
                Assert.That(tablenames, Has.None.EqualTo("Beta_2"));

                Assert.That(tablenames, Has.One.EqualTo("Gamma"));
                Assert.That(tablenames, Has.None.EqualTo("Gamma_1"));
                Assert.That(tablenames, Has.None.EqualTo("Gamma_2"));
            }

            var registeringPlan3 = new ChangePlanByVersion<Version_3>();
            registeringPlan3.AddVersionChangePlan(new BetaChangePlan_Version_2());
            registeringPlan3.AddVersionChangePlan(new GammaChangePlan_Version_1());
            svManager.RegisterChangePlan(registeringPlan3);
            svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                var tablenames = conn.GetTableNames();

                Assert.That(tablenames, Has.One.EqualTo("Alpha"));
                Assert.That(tablenames, Has.One.EqualTo("Alpha_1"));
                Assert.That(tablenames, Has.One.EqualTo("Alpha_2"));

                Assert.That(tablenames, Has.One.EqualTo("Beta"));
                Assert.That(tablenames, Has.One.EqualTo("Beta_1"));
                Assert.That(tablenames, Has.One.EqualTo("Beta_2"));

                Assert.That(tablenames, Has.One.EqualTo("Gamma"));
                Assert.That(tablenames, Has.One.EqualTo("Gamma_1"));
                Assert.That(tablenames, Has.None.EqualTo("Gamma_2"));
            }

            var registeringPlan4 = new ChangePlanByVersion<Version_4>();
            registeringPlan4.AddVersionChangePlan(new AlphaChangePlan_Version_3());
            svManager.RegisterChangePlan(registeringPlan4);

            var dao = new AlphaDao(typeof(Version_2));
            dao.CurrentConnection = ConnectionManager.DefaultConnection;

            dao.Insert(new Alpha()
            {
                Id = Guid.Parse("26D55FA7-1662-4E74-9143-290940A8E8D4"),
                Item1 = "org_item1",
                Item2 = "org_item2",
                Item3 = Guid.Empty,
                Item4 = "org_item4",
                Item5 = 1,
                Item6 = 2,
                Item7 = "org_item7",
                Item8 = true,
                Item9 = true,
            });

            svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                var tablenames = conn.GetTableNames();

                Assert.That(tablenames, Has.One.EqualTo("Alpha"));
                Assert.That(tablenames, Has.One.EqualTo("Alpha_1"));
                Assert.That(tablenames, Has.One.EqualTo("Alpha_2"));

                dao = new AlphaDao(typeof(Version_2));
                dao.CurrentConnection = ConnectionManager.DefaultConnection;

                var all = dao.FindAll();
                var arecord = all.Take(1).Single();
                Assert.That(arecord, Has.Property("Id").EqualTo(Guid.Parse("26D55FA7-1662-4E74-9143-290940A8E8D4")));
                Assert.That(arecord, Has.Property("Item1").EqualTo("org_item1"));
                Assert.That(arecord, Has.Property("Item2").EqualTo("org_item2"));
                Assert.That(arecord, Has.Property("Item3").EqualTo(Guid.Empty));
                Assert.That(arecord, Has.Property("Item4").EqualTo("org_item4"));
                Assert.That(arecord, Has.Property("Item5").EqualTo(1));
                Assert.That(arecord, Has.Property("Item6").EqualTo(2));
                Assert.That(arecord, Has.Property("Item7").EqualTo("org_item7"));
                Assert.That(arecord, Has.Property("Item8").False);
                Assert.That(arecord, Has.Property("Item9").False);

                Assert.That(tablenames, Has.One.EqualTo("Alpha_3"));

                dao =  new AlphaDao(typeof(Version_3));
                dao.CurrentConnection = ConnectionManager.DefaultConnection;

                dao.Insert(new Alpha()
                {
                    Id = Guid.Parse("8069BD2D-2BC9-4C89-966D-8E966FB87546"),
                    Item1 = "org_item1",
                    Item2 = "org_item2",
                    Item3 = Guid.Empty,
                    Item4 = "org_item4",
                    Item5 = 1,
                    Item6 = 2,
                    Item7 = "org_item7",
                    Item8 = true,
                    Item9 = true,
                });
                
                all = dao.FindAll();
                arecord = all.Skip(1).Take(1).Single();
                Assert.That(arecord, Has.Property("Id").EqualTo(Guid.Parse("8069BD2D-2BC9-4C89-966D-8E966FB87546")));
                Assert.That(arecord, Has.Property("Item1").EqualTo("org_item1"));
                Assert.That(arecord, Has.Property("Item2").EqualTo("org_item2"));
                Assert.That(arecord, Has.Property("Item3").EqualTo(Guid.Empty));
                Assert.That(arecord, Has.Property("Item4").EqualTo("org_item4"));
                Assert.That(arecord, Has.Property("Item5").EqualTo(1));
                Assert.That(arecord, Has.Property("Item6").EqualTo(2));
                Assert.That(arecord, Has.Property("Item7").EqualTo("org_item7"));
                Assert.That(arecord, Has.Property("Item8").True);
                Assert.That(arecord, Has.Property("Item9").True);

                Assert.That(tablenames, Has.One.EqualTo("Beta"));
                Assert.That(tablenames, Has.One.EqualTo("Beta_1"));
                Assert.That(tablenames, Has.One.EqualTo("Beta_2"));

                Assert.That(tablenames, Has.One.EqualTo("Gamma"));
                Assert.That(tablenames, Has.One.EqualTo("Gamma_1"));
                Assert.That(tablenames, Has.None.EqualTo("Gamma_2"));
            }
        }

        [Test]
        public void ByVersion_Book_ComplexCase()
        {
            var svManager = new DataVersionManager();
            svManager.CurrentConnection = ConnectionManager.DefaultConnection;
            svManager.Mode = VersioningStrategy.ByTick;
            var registeringPlan = new ChangePlanByVersion<VersionOrigin>();
            registeringPlan.AddVersionChangePlan(new BookChangePlan_VersionOrigin());
            svManager.RegisterChangePlan(registeringPlan);
            svManager.SetDefault();
            svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                var dao = new BookDao(typeof(VersionOrigin));
                dao.Insert(new Book()
                {
                    ID = Guid.Parse("8069BD2D-2BC9-4C89-966D-8E966FB87546"),
                    Title = "kintama",
                    AuthorID = Guid.Parse("3320FF3E-B7F0-42CC-A994-C6DF57B2067D"),
                    PublishDate = DateTime.Parse("2022/09/08 00:00:00"),
                    ByteSize = 1024,
                    FingerPrint = "9834876dcfb05cb167a5c24953eba58c4ac89b1adf57f28f2f9d09af107ee8f0",
                });
            }

            var registeringPlan1 = new ChangePlanByVersion<Version_1>();
            registeringPlan1.AddVersionChangePlan(new BookChangePlan_Version_1());
            svManager.RegisterChangePlan(registeringPlan1);
            svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                var dao = new BookDao(typeof(Version_1));

                var records = dao.FindAll().ToList();
                Assert.That(records.Count(), Is.EqualTo(1));
                Assert.That(records[0], Has.Property("Title").EqualTo("kintama"));
                Assert.That(records[0], Has.Property("AuthorID").EqualTo(Guid.Parse("3320FF3E-B7F0-42CC-A994-C6DF57B2067D")));
                Assert.That(records[0], Has.Property("PublishDate").EqualTo(DateTime.Parse("2022/09/08 00:00:00")));
                Assert.That(records[0], Has.Property("ByteSize").Null);
                Assert.That(records[0], Has.Property("FingerPrint").Null);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }
    }
}
