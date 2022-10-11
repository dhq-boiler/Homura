
using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using System.Threading.Tasks;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class BookChangePlan_VersionOrigin : BookChangePlan_Abstract<VersionOrigin>
    {
        public BookChangePlan_VersionOrigin(VersioningMode mode) : base("Book", PostMigrationVerification.TableExists, mode)
        {
        }

        public override async Task UpgradeToTargetVersion(IConnection connection)
        {
            await CreateTable(connection);
        }
    }
}
