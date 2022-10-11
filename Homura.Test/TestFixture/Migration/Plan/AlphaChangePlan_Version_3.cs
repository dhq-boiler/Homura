
using Homura.ORM;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class AlphaChangePlan_Version_3 : AlphaChangePlan_Abstract<Version_3>
    {
        public AlphaChangePlan_Version_3(VersioningMode mode) : base("Alpha_3", PostMigrationVerification.TableExists, mode)
        {
        }

        public override async Task UpgradeToTargetVersion(IConnection connection)
        {
            var dao = new AlphaDao(TargetVersion.GetType());
            dao.CurrentConnection = connection;

            await dao.CreateTableIfNotExistsAsync();

            if (await dao.CountAllAsync() > 0)
            {
                await dao.DeleteAsync(new Dictionary<string, object>());
            }

            await dao.UpgradeTableAsync(new VersionChangeUnit(typeof(Version_2), TargetVersion.GetType()), Mode);
        }
    }
}
