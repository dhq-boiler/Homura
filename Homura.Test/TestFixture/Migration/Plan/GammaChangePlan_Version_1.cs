
using Homura.ORM;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class GammaChangePlan_Version_1 : GammaChangePlan_Abstract<Version_1>
    {
        public override void UpgradeToTargetVersion(IConnection connection)
        {
            CreateTable(connection);
        }
    }
}
