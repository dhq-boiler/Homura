using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Migration;
using Homura.Test.TestFixture.Migration.Plan;
using NUnit.Framework;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace Homura.Test.UnitTest
{
    [TestFixture]
    public class DeleteTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var _filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "DeleteTest.db");
            ConnectionManager.SetDefaultConnection($"Data Source={_filePath}", typeof(SQLiteConnection));

            var svManager = new DataVersionManager();
            svManager.CurrentConnection = ConnectionManager.DefaultConnection;
            svManager.Mode = VersioningStrategy.ByTick;
            var registeringPlan = new ChangePlanByVersion<VersionOrigin>();
            registeringPlan.AddVersionChangePlan(new AlphaChangePlan_VersionOrigin());
            registeringPlan.AddVersionChangePlan(new BetaChangePlan_VersionOrigin());
            registeringPlan.AddVersionChangePlan(new GammaChangePlan_VersionOrigin());
            svManager.RegisterChangePlan(registeringPlan);
            svManager.SetDefault();
            var registeringPlan1 = new ChangePlanByVersion<Version_1>();
            registeringPlan1.AddVersionChangePlan(new AlphaChangePlan_Version_1());
            svManager.RegisterChangePlan(registeringPlan1);
            var registeringPlan2 = new ChangePlanByVersion<Version_2>();
            registeringPlan2.AddVersionChangePlan(new AlphaChangePlan_Version_2());
            registeringPlan2.AddVersionChangePlan(new BetaChangePlan_Version_1());
            svManager.RegisterChangePlan(registeringPlan2);
            var registeringPlan3 = new ChangePlanByVersion<Version_3>();
            registeringPlan3.AddVersionChangePlan(new BetaChangePlan_Version_2());
            registeringPlan3.AddVersionChangePlan(new GammaChangePlan_Version_1());
            svManager.RegisterChangePlan(registeringPlan3);
            var registeringPlan4 = new ChangePlanByVersion<Version_4>();
            registeringPlan4.AddVersionChangePlan(new AlphaChangePlan_Version_3());
            svManager.RegisterChangePlan(registeringPlan4);
            svManager.UpgradeToTargetVersion();
        }

        [Test]
        public void Image_3テーブルを全削除()
        {
            var dao = new AlphaDao(typeof(Version_3));
            dao.Delete(new Dictionary<string, object>());
        }
    }
}
