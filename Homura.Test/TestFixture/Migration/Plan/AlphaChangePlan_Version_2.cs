
using Homura.ORM;
using Homura.ORM.Setup;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class AlphaChangePlan_Version_2 : AlphaChangePlan_Abstract<Version_2>
    {
        public AlphaChangePlan_Version_2(VersioningMode mode) : base(mode)
        {
        }

        public override void UpgradeToTargetVersion(IConnection connection)
        {
            CreateTable(connection);
        }
    }
}
