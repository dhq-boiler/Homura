

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
        public void RegisterChangePlan_GetPlan_ByTable()
        {
            var defMng = DataVersionManager.DefaultSchemaVersion;
            defMng.Mode = VersioningStrategy.ByTable;
            defMng.RegisterChangePlan(new OriginChangePlan_VersionOrigin());

            var plan = defMng.GetPlan(typeof(Origin), new VersionOrigin());
            Assert.That(plan, Is.TypeOf<OriginChangePlan_VersionOrigin>());
        }

        [Test]
        public void RegisterChangePlan_GetPlan_ByTick()
        {
            var defMng = DataVersionManager.DefaultSchemaVersion;
            defMng.Mode = VersioningStrategy.ByTick;
            defMng.RegisterChangePlan(new VersionChangePlan_VersionOrigin());

            var plan = defMng.GetPlan(new VersionOrigin());
            Assert.That(plan, Is.TypeOf<VersionChangePlan_VersionOrigin>());
        }

        [Test]
        public void GetPlan_NotRegistered_ByTable()
        {
            var defMng = DataVersionManager.DefaultSchemaVersion;
            defMng.Mode = VersioningStrategy.ByTable;
            Assert.Throws<KeyNotFoundException>(() => defMng.GetPlan(typeof(Header), new VersionOrigin()));

            defMng.RegisterChangePlan(new OriginChangePlan_Version_1());
            Assert.Throws<KeyNotFoundException>(() => defMng.GetPlan(typeof(Header), new VersionOrigin()));
        }

        [Test]
        public void GetPlan_NotRegistered_ByTick()
        {
            var defMng = DataVersionManager.DefaultSchemaVersion;
            defMng.Mode = VersioningStrategy.ByTick;
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
