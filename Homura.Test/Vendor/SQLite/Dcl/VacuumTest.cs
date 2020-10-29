using NUnit.Framework;
using Homura.QueryBuilder.Vendor.SQLite.Dcl;

namespace Homura.QueryBuilder.Test.Vendor.SQLite.Dcl
{
    [TestFixture]
    [Category("Homura.QueryBuilder QueryBuilder")]
    public class VacuumTest
    {
        [Test]
        public void Vacuum()
        {
            using (var query = new Vacuum())
            {
                Assert.That(query.ToSql(), Is.EqualTo("VACUUM"));
            }
        }
    }
}
