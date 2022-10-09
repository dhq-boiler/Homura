
using Homura.ORM;
using Homura.ORM.Migration;
using Homura.ORM.Setup;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class AlphaChangePlan_Version_2 : AlphaChangePlan_Abstract<Version_2>
    {
        public AlphaChangePlan_Version_2(VersioningMode mode) : base("Alpha_2", PostMigrationVerification.TableExists, mode)
        {
        }

        public override void UpgradeToTargetVersion(IConnection connection)
        {
            CreateTable(connection);
        }
    }
}
