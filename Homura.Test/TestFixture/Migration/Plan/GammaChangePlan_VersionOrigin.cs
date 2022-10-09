

using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Setup;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class GammaChangePlan_VersionOrigin : GammaChangePlan_Abstract<VersionOrigin>
    {
        public GammaChangePlan_VersionOrigin(VersioningMode mode) : base("Gamma", ORM.Migration.PostMigrationVerification.TableExists, mode)
        {
        }

        public override void UpgradeToTargetVersion(IConnection connection)
        {
            CreateTable(connection);
        }
    }
}
