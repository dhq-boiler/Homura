
using Homura.ORM;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using System.Threading.Tasks;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class BetaChangePlan_Version_1 : BetaChangePlan_Abstract<Version_1>
    {
        public BetaChangePlan_Version_1(VersioningMode mode) : base("Beta_1", PostMigrationVerification.TableExists, mode)
        {
        }

        public override async Task UpgradeToTargetVersion(IConnection connection)
        {
            await CreateTable(connection);
        }
    }
}
