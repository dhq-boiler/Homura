

using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.Test.TestFixture.Migration;
using NUnit.Framework;

namespace Sunctum.Infrastructure.Test.UnitTest.Data.Rdbms
{
    public class TableTest
    {
        [Category("Infrastructure")]
        [Category("UnitTest")]
        [TestFixture]
        public class NotSpecified
        {
            [Test]
            public void PhysicalTableName_NotSpecified()
            {
                Assert.That(new Table<DummyTable>().Name, Is.EqualTo("DummyTable"));
            }

            public class DummyTable : EntityBaseObject
            { }
        }

        [Category("Infrastructure")]
        [Category("UnitTest")]
        [TestFixture]
        public class Specified_VersionOrigin
        {
            [Test]
            public void PhysicalTableName_Specified_VersionOrigin()
            {
                Assert.That(new Table<DummyTable>().Name, Is.EqualTo("DummyTable"));
            }

            [DefaultVersion(typeof(VersionOrigin))]
            public class DummyTable : EntityBaseObject
            { }
        }

        [Category("Infrastructure")]
        [Category("UnitTest")]
        [TestFixture]
        public class Specified_Version_1
        {
            [Test]
            public void PhysicalTableName_Specified_Version_1()
            {
                Assert.That(new Table<DummyTable>().Name, Is.EqualTo("DummyTable_1"));
            }

            [DefaultVersion(typeof(Version_1))]
            public class DummyTable : EntityBaseObject
            { }
        }

        [Category("Infrastructure")]
        [Category("UnitTest")]
        [TestFixture]
        public class Specified_Version_3
        {
            [Test]
            public void PhysicalTableName_Specified_Version_3()
            {
                Assert.That(new Table<DummyTable>().Name, Is.EqualTo("DummyTable_3"));
            }

            [DefaultVersion(typeof(Version_3))]
            public class DummyTable : EntityBaseObject
            { }
        }
    }
}
