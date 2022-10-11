


using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Setup;
using System.Threading.Tasks;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class OriginChangePlan_VersionOrigin : OriginChangePlan_Abstract<VersionOrigin>
    {
        public OriginChangePlan_VersionOrigin(VersioningMode mode) : base("Origin", ORM.Migration.PostMigrationVerification.TableExists, mode)
        {
        }

        public override async Task UpgradeToTargetVersion(IConnection connection)
        {
            await CreateTable(connection);
        }

        public override async Task DowngradeToTargetVersion(IConnection connection)
        {
            await base.DowngradeToTargetVersion(connection);
        }
    }
}
