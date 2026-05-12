using NUnit.Framework;
using System;
using HomuraExt = Homura.Extensions.Extensions;

namespace Homura.Test.UnitTest
{
    [Category("Infrastructure")]
    [Category("UnitTest")]
    [TestFixture]
    public class TypeCacheTest
    {
        [Test]
        public void GetCachedType_ReturnsSameInstance_ForRepeatedCalls()
        {
            var fqn = typeof(string).AssemblyQualifiedName;

            var first = HomuraExt.GetCachedType(fqn);
            var second = HomuraExt.GetCachedType(fqn);

            Assert.That(first, Is.SameAs(typeof(string)));
            Assert.That(second, Is.SameAs(first));
        }

        [Test]
        public void GetCachedType_ReturnsNull_ForNullInput()
        {
            Assert.That(HomuraExt.GetCachedType(null), Is.Null);
        }

        [Test]
        public void GetCachedType_ResolvesDifferentTypes_Independently()
        {
            var t1 = HomuraExt.GetCachedType(typeof(int).AssemblyQualifiedName);
            var t2 = HomuraExt.GetCachedType(typeof(Guid).AssemblyQualifiedName);

            Assert.That(t1, Is.EqualTo(typeof(int)));
            Assert.That(t2, Is.EqualTo(typeof(Guid)));
        }
    }
}
