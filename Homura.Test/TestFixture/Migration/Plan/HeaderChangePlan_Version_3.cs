
using Homura.ORM.Setup;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class HeaderChangePlan_Version_3 : HeaderChangePlan_Abstract<Version_3>
    {
        public HeaderChangePlan_Version_3(VersioningMode mode) : base("Header_3", ORM.Migration.PostMigrationVerification.TableExists, mode)
        {
        }
    }
}
