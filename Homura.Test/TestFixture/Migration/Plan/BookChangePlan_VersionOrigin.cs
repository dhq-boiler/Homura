
using Homura.ORM;
using Homura.ORM.Mapping;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class BookChangePlan_VersionOrigin : BookChangePlan_Abstract<VersionOrigin>
    {
        public override void UpgradeToTargetVersion(IConnection connection)
        {
            CreateTable(connection);
        }
    }
}
