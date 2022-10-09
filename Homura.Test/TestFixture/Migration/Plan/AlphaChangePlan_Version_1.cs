
using Homura.ORM;
using Homura.ORM.Migration;
using Homura.ORM.Setup;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class AlphaChangePlan_Version_1 : AlphaChangePlan_Abstract<Version_1>
    {
        public AlphaChangePlan_Version_1(VersioningMode mode) : base("Alpha_1", PostMigrationVerification.TableExists, mode)
        {
        }

        public override void UpgradeToTargetVersion(IConnection connection)
        {
            CreateTable(connection);
        }
    }
}
