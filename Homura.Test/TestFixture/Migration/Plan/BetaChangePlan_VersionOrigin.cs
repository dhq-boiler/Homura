

using Homura.ORM;
using Homura.ORM.Mapping;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class BetaChangePlan_VersionOrigin : BetaChangePlan_Abstract<VersionOrigin>
    {
        public override void UpgradeToTargetVersion(IConnection connection)
        {
            CreateTable(connection);
        }
    }
}
