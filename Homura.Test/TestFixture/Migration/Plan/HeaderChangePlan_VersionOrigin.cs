

using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Setup;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class HeaderChangePlan_VersionOrigin : HeaderChangePlan_Abstract<VersionOrigin>
    {
        public HeaderChangePlan_VersionOrigin(VersioningMode mode) : base(mode)
        {
        }

        public override void UpgradeToTargetVersion(IConnection connection)
        {
            CreateTable(connection);
        }
    }
}
