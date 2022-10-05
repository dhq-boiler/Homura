

using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Entity;
using Homura.Test.TestFixture.Migration;
using Homura.Test.TestFixture.Migration.Plan;
using NUnit.Framework;
using Sunctum.Domain.Data.Dao.Migration.Plan;
using System;
using System.Collections.Generic;
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
            ConnectionManager.SetDefaultConnection(Guid.Parse("7F39B3C1-1DDE-4B81-933F-FEAF4336F1B5"), $"Data Source={_filePath}", typeof(SQLiteConnection));
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }

        [Test]
        public void ByTick_Origin_VersinOrigin_To_Version_1()
        {
            var svManager = new DataVersionManager();
            svManager.CurrentConnection = ConnectionManager.DefaultConnection;
            svManager.Mode = VersioningMode.ByTick;
            var registeringPlan = new ChangePlan<VersionOrigin>();
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

            var registeringPlan1 = new ChangePlan<Version_1>();
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
            svManager.Mode = VersioningMode.ByTick;
            var registeringPlan = new ChangePlan<VersionOrigin>();
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

            var registeringPlan1 = new ChangePlan<Version_1>();
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

            var registeringPlan2 = new ChangePlan<Version_2>();
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

            var registeringPlan3 = new ChangePlan<Version_3>();
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

            var registeringPlan4 = new ChangePlan<Version_4>();
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
            svManager.Mode = VersioningMode.ByTick;
            var registeringPlan = new ChangePlan<VersionOrigin>();
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

            var registeringPlan1 = new ChangePlan<Version_1>();
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

        [Test]
        public void ByVersion_Page_SimpleCase()
        {
            var svManager = new DataVersionManager();
            svManager.CurrentConnection = ConnectionManager.DefaultConnection;
            svManager.Mode = VersioningMode.ByTick;
            var registeringPlan = new ChangePlan<VersionOrigin>();
            registeringPlan.AddVersionChangePlan(new PageChangePlan_VersionOrigin());
            svManager.RegisterChangePlan(registeringPlan);
            svManager.SetDefault();
            svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                conn.Open();

                var dao = new PageDao(typeof(VersionOrigin));

                dao.Insert(new Page()
                {
                    ID = Guid.Parse("89B5FA63-7D91-4622-8DB1-61F7BE80B416"),
                    BookID = Guid.Parse("75813CCD-CC48-4894-ACC6-3CE8C5333422"),
                    ImageID = Guid.Parse("655CAD5B-423D-4531-B474-68983FFFE385"),
                    PageIndex = 3,
                    Title = "E9C012E2-938A-484F-8F01-04169B920781",
                });

                var record = dao.FindBy(new Dictionary<string, object>() { { "ID", Guid.Parse("89B5FA63-7D91-4622-8DB1-61F7BE80B416") } }).SingleOrDefault();
                Assert.That(record, Is.Not.Null);
                Assert.That(record, Has.Property("BookID").EqualTo(Guid.Parse("75813CCD-CC48-4894-ACC6-3CE8C5333422")));
                Assert.That(record, Has.Property("ImageID").EqualTo(Guid.Parse("655CAD5B-423D-4531-B474-68983FFFE385")));
                Assert.That(record, Has.Property("PageIndex").EqualTo(3));
                Assert.That(record, Has.Property("Title").EqualTo("E9C012E2-938A-484F-8F01-04169B920781"));
            }
        }


        [TearDown]
        public void TearDown()
        {
            ConnectionManager.DisposeDebris(Guid.Parse("7F39B3C1-1DDE-4B81-933F-FEAF4336F1B5"));
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }
    }
}
