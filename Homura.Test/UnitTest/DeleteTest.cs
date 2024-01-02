using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Migration;
using Homura.Test.TestFixture.Migration.Plan;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

namespace Homura.Test.UnitTest
{
    [TestFixture]
    public class DeleteTest
    {
        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            var _filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "DeleteTest.db");
            ConnectionManager.SetDefaultConnection(Guid.Parse("B9464890-845E-4295-B3BA-20BC50C7136F"), $"Data Source={_filePath}", typeof(SQLiteConnection));

            var svManager = new DataVersionManager();
            svManager.CurrentConnection = ConnectionManager.DefaultConnection;
            //svManager.Mode = VersioningMode.ByTick;
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
            registeringPlan2.AddVersionChangePlan(new BetaChangePlan_Version_1( VersioningMode.ByTick));
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
        public async Task Image_3テーブルを全削除()
        {
            var dao = new AlphaDao(typeof(Version_3));
            dao.Delete(new Dictionary<string, object>());
        }

        [TearDown]
        public void TearDown()
        {
            ConnectionManager.DisposeDebris(Guid.Parse("B9464890-845E-4295-B3BA-20BC50C7136F"));
        }
    }
}
