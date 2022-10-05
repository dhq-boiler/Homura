
using Homura.ORM;
using Homura.ORM.Setup;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class AlphaChangePlan_Version_1 : AlphaChangePlan_Abstract<Version_1>
    {
        public AlphaChangePlan_Version_1(VersioningMode mode) : base(mode)
        {
        }

        public override void UpgradeToTargetVersion(IConnection connection)
        {
            CreateTable(connection);
        }
    }
}
