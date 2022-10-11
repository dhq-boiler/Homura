
using Homura.ORM;
using Homura.ORM.Setup;
using System.Threading.Tasks;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class GammaChangePlan_Version_1 : GammaChangePlan_Abstract<Version_1>
    {
        public GammaChangePlan_Version_1(VersioningMode mode) : base("Gamma_1", ORM.Migration.PostMigrationVerification.TableExists, mode)
        {
        }

        public override async Task UpgradeToTargetVersion(IConnection connection)
        {
            await CreateTable(connection);
        }
    }
}
