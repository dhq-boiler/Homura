using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Migration;
using Homura.Test.TestFixture.Migration.Plan;
using NUnit.Framework;
using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Homura.Test.UnitTest
{
    [TestFixture]
    public class SelectTest
    {
        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            var _filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "SelectTest.db");
            ConnectionManager.SetDefaultConnection(Guid.Parse("D88B3E8E-B46C-41E3-AAEA-87FEA352C9F6"), $"Data Source={_filePath}", typeof(SQLiteConnection));

            var svManager = new DataVersionManager();
            svManager.CurrentConnection = ConnectionManager.DefaultConnection;
            var registeringPlan = new ChangePlan<VersionOrigin>(VersioningMode.ByTick);
            registeringPlan.AddVersionChangePlan(new AlphaChangePlan_VersionOrigin(VersioningMode.ByTick));
            registeringPlan.AddVersionChangePlan(new BetaChangePlan_VersionOrigin(VersioningMode.ByTick));
            registeringPlan.AddVersionChangePlan(new GammaChangePlan_VersionOrigin(VersioningMode.ByTick));
            svManager.RegisterChangePlan(registeringPlan);
            svManager.SetDefault();
            var registeringPlan1 = new ChangePlan<Version_1>(VersioningMode.ByTick);
            registeringPlan1.AddVersionChangePlan(new AlphaChangePlan_Version_1(VersioningMode.ByTick));
            svManager.RegisterChangePlan(registeringPlan1);
            var registeringPlan2 = new ChangePlan<Version_2>(VersioningMode.ByTick);
            registeringPlan2.AddVersionChangePlan(new AlphaChangePlan_Version_2(VersioningMode.ByTick));
            registeringPlan2.AddVersionChangePlan(new BetaChangePlan_Version_1(VersioningMode.ByTick));
            svManager.RegisterChangePlan(registeringPlan2);
            var registeringPlan3 = new ChangePlan<Version_3>(VersioningMode.ByTick);
            registeringPlan3.AddVersionChangePlan(new BetaChangePlan_Version_2(VersioningMode.ByTick));
            registeringPlan3.AddVersionChangePlan(new GammaChangePlan_Version_1(VersioningMode.ByTick));
            svManager.RegisterChangePlan(registeringPlan3);
            var registeringPlan4 = new ChangePlan<Version_4>(VersioningMode.ByTick);
            registeringPlan4.AddVersionChangePlan(new AlphaChangePlan_Version_3(VersioningMode.ByTick));
            svManager.RegisterChangePlan(registeringPlan4);
            svManager.UpgradeToTargetVersion();
        }

        [Test]
        public async Task FindAllAsync()
        {
            var dao = new AlphaDao(typeof(Version_3));
            var items = await dao.FindAllAsync().ToListAsync();
        }

        [TearDown]
        public void TearDown()
        {
            ConnectionManager.DisposeDebris(Guid.Parse("D88B3E8E-B46C-41E3-AAEA-87FEA352C9F6"));
        }
    }
}
