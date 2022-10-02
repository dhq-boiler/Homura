

using Homura.ORM.Mapping;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Entity;
using Homura.Test.TestFixture.Migration.Plan;
using NUnit.Framework;
using System.Collections.Generic;

namespace Sunctum.Infrastructure.Test.UnitTest.Data.Setup
{
    [Category("Infrastructure")]
    [Category("UnitTest")]
    [TestFixture]
    public class DataVersionManagerTest
    {
        [SetUp]
        public void Initialize()
        {
            DataVersionManager.DefaultSchemaVersion = new DataVersionManager();
            NotSetDefaultExplicitly();
        }

        [Test]
        public void NotSetDefaultExplicitly()
        {
            var defMng = DataVersionManager.DefaultSchemaVersion;
            Assert.That(defMng, Is.Not.Null);
        }

        [Test]
        public void SetDefault()
        {
            var svManager = new DataVersionManager();
            svManager.SetDefault();

            var defMng = DataVersionManager.DefaultSchemaVersion;
            Assert.That(defMng, Is.Not.Null);
        }

        [Test]
        public void RegisterChangePlan_GetPlan_ByTick()
        {
            var defMng = DataVersionManager.DefaultSchemaVersion;
            defMng.Mode = VersioningMode.ByTick;
            defMng.RegisterChangePlan(new VersionChangePlan_VersionOrigin());

            var plan = defMng.GetPlan(new VersionOrigin());
            Assert.That(plan, Is.TypeOf<VersionChangePlan_VersionOrigin>());
        }

        [Test]
        public void GetPlan_NotRegistered_ByTick()
        {
            var defMng = DataVersionManager.DefaultSchemaVersion;
            defMng.Mode = VersioningMode.ByTick;
            Assert.Throws<KeyNotFoundException>(() => defMng.GetPlan(new VersionOrigin()));

            defMng.RegisterChangePlan(new VersionChangePlan_Version_1());
            Assert.Throws<KeyNotFoundException>(() => defMng.GetPlan(new VersionOrigin()));
        }

        [TearDown]
        public void TearDown()
        {
            DataVersionManager.DefaultSchemaVersion = new DataVersionManager();
        }
    }
}
