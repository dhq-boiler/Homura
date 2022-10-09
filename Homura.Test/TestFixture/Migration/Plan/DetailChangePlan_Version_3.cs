
using Homura.ORM.Setup;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class DetailChangePlan_Version_3 : DetailChangePlan_Abstract<Version_3>
    {
        public DetailChangePlan_Version_3(VersioningMode mode) : base("Detail_3", ORM.Migration.PostMigrationVerification.TableExists, mode)
        {
        }
    }
}
