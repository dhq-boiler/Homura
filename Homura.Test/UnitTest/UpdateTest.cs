using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Entity;
using NUnit.Framework;
using Sunctum.Domain.Data.Dao.Migration.Plan;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homura.Test.UnitTest
{
    [TestFixture]
    public class UpdateTest
    {
        private string _filePath;

        [SetUp]
        public void Initialize()
        {
            _filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Update.db");
            ConnectionManager.SetDefaultConnection(Guid.Parse("3EC3B843-847A-42D0-B0CA-C8228062CACB"), $"Data Source={_filePath}", typeof(SQLiteConnection));
        }

        [Test]
        public async Task Update_DataOpUnit_is_null()
        {

            var svManager = new DataVersionManager();
            svManager.CurrentConnection = ConnectionManager.DefaultConnection;
            //svManager.Mode = VersioningMode.ByTick;
            var registeringPlan = new ChangePlan<VersionOrigin>(VersioningMode.ByTick);
            registeringPlan.AddVersionChangePlan(new PageChangePlan_VersionOrigin(VersioningMode.ByTick));
            svManager.RegisterChangePlan(registeringPlan);
            svManager.SetDefault();
            await svManager.UpgradeToTargetVersion();

            using (var conn = new SQLiteConnection($"Data Source={_filePath}"))
            {
                await conn.OpenAsync();

                var dao = new PageDao(typeof(VersionOrigin));

                await dao.InsertAsync(new Page()
                {
                    ID = Guid.Parse("89B5FA63-7D91-4622-8DB1-61F7BE80B416"),
                    BookID = Guid.Parse("75813CCD-CC48-4894-ACC6-3CE8C5333422"),
                    ImageID = Guid.Parse("655CAD5B-423D-4531-B474-68983FFFE385"),
                    PageIndex = 3,
                    Title = "E9C012E2-938A-484F-8F01-04169B920781",
                }, conn);

                var record = (await dao.FindByAsync(new Dictionary<string, object>() { { "ID", Guid.Parse("89B5FA63-7D91-4622-8DB1-61F7BE80B416") } }, conn).ToListAsync()).SingleOrDefault();
                Assert.That(record, Is.Not.Null);
                Assert.That(record, Has.Property("BookID").EqualTo(Guid.Parse("75813CCD-CC48-4894-ACC6-3CE8C5333422")));
                Assert.That(record, Has.Property("ImageID").EqualTo(Guid.Parse("655CAD5B-423D-4531-B474-68983FFFE385")));
                Assert.That(record, Has.Property("PageIndex").EqualTo(3));
                Assert.That(record, Has.Property("Title").EqualTo("E9C012E2-938A-484F-8F01-04169B920781"));

                record.PageIndex = 4;

                DataOperationUnit dataOpUnit = null;

                await dao.UpdateAsync(record, dataOpUnit?.CurrentConnection);
                
                record = (await dao.FindByAsync(new Dictionary<string, object>() { { "ID", Guid.Parse("89B5FA63-7D91-4622-8DB1-61F7BE80B416") } }).ToListAsync()).SingleOrDefault();
                Assert.That(record, Is.Not.Null);
                Assert.That(record, Has.Property("BookID").EqualTo(Guid.Parse("75813CCD-CC48-4894-ACC6-3CE8C5333422")));
                Assert.That(record, Has.Property("ImageID").EqualTo(Guid.Parse("655CAD5B-423D-4531-B474-68983FFFE385")));
                Assert.That(record, Has.Property("PageIndex").EqualTo(4));
                Assert.That(record, Has.Property("Title").EqualTo("E9C012E2-938A-484F-8F01-04169B920781"));
            }
        }

        [TearDown]
        public void TearDown()
        {
            ConnectionManager.DisposeDebris(Guid.Parse("3EC3B843-847A-42D0-B0CA-C8228062CACB"));
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }
    }
}
