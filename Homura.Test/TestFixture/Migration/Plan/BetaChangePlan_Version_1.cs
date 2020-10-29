
using Homura.ORM;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class BetaChangePlan_Version_1 : BetaChangePlan_Abstract<Version_1>
    {
        public override void UpgradeToTargetVersion(IConnection connection)
        {
            CreateTable(connection);
        }
    }
}
