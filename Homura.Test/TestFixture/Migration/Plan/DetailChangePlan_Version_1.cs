

using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class DetailChangePlan_Version_1 : DetailChangePlan_Abstract<Version_1>
    {
        public DetailChangePlan_Version_1(VersioningMode mode) : base("Detail_1", PostMigrationVerification.TableExists, mode)
        {
        }

        public override async Task UpgradeToTargetVersion(IConnection connection)
        {
            var dao = new DetailDao(TargetVersion.GetType());
            dao.CurrentConnection = connection;

            await dao.CreateTableIfNotExistsAsync();

            if (await dao.CountAllAsync() > 0)
            {
                await dao.DeleteAsync(new Dictionary<string, object>());
            }

            await dao.UpgradeTableAsync(new VersionChangeUnit(typeof(VersionOrigin), TargetVersion.GetType()), Mode);
        }
    }
}
