


using Homura.ORM;
using Homura.ORM.Mapping;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class OriginChangePlan_VersionOrigin : OriginChangePlan_Abstract<VersionOrigin>
    {
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
