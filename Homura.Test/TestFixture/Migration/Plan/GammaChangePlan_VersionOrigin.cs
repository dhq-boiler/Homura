

using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Setup;
using System.Threading.Tasks;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class GammaChangePlan_VersionOrigin : GammaChangePlan_Abstract<VersionOrigin>
    {
        public GammaChangePlan_VersionOrigin(VersioningMode mode) : base("Gamma", ORM.Migration.PostMigrationVerification.TableExists, mode)
        {
        }

        public override async Task UpgradeToTargetVersion(IConnection connection)
        {
            await CreateTable(connection);
        }
    }
}
