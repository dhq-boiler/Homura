
using Homura.Test.TestFixture.Entity;
using NUnit.Framework;
using System;

namespace Homura.Test.UnitTest.SourceGenerator
{
    [TestFixture]
    public class SourceGeneratorTest
    {
        [Test]
        public void GeneratedDaoExists()
        {
            // Source Generator によって GeneratedSampleDao が生成されていることを確認
            var dao = new GeneratedSampleDao();
            Assert.That(dao, Is.Not.Null);
            Assert.That(dao.TableName, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void GeneratedDaoWithVersionExists()
        {
            var dao = new GeneratedSampleDao(typeof(Homura.ORM.Mapping.VersionOrigin));
            Assert.That(dao, Is.Not.Null);
        }

        [Test]
        public void GeneratedChangePlanExists()
        {
            // Source Generator によって ChangePlan が生成されていることを確認
            var plan = new ChangePlan_GeneratedSample_VersionOrigin(Homura.ORM.Setup.VersioningMode.ByTick);
            Assert.That(plan, Is.Not.Null);

            var planV1 = new ChangePlan_GeneratedSample_Version_1(Homura.ORM.Setup.VersioningMode.ByTick);
            Assert.That(planV1, Is.Not.Null);
        }
    }
}
