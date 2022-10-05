

using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Setup;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class AlphaChangePlan_VersionOrigin : AlphaChangePlan_Abstract<VersionOrigin>
    {
        public AlphaChangePlan_VersionOrigin(VersioningMode mode) : base(mode)
        {
        }

        public override void UpgradeToTargetVersion(IConnection connection)
        {
            CreateTable(connection);
        }
    }
}
