

using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Setup;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class BetaChangePlan_VersionOrigin : BetaChangePlan_Abstract<VersionOrigin>
    {
        public BetaChangePlan_VersionOrigin(VersioningMode mode) : base(mode)
        {
        }

        public override void UpgradeToTargetVersion(IConnection connection)
        {
            CreateTable(connection);
        }
    }
}
