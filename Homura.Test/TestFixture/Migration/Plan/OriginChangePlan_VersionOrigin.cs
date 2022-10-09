


using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Setup;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class OriginChangePlan_VersionOrigin : OriginChangePlan_Abstract<VersionOrigin>
    {
        public OriginChangePlan_VersionOrigin(VersioningMode mode) : base("Origin", ORM.Migration.PostMigrationVerification.TableExists, mode)
        {
        }

        public override void UpgradeToTargetVersion(IConnection connection)
        {
            CreateTable(connection);
        }

        public override void DowngradeToTargetVersion(IConnection connection)
        {
            base.DowngradeToTargetVersion(connection);
        }
    }
}
