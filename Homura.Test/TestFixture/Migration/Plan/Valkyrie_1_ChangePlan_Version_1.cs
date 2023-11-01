using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Entity;
using Homura.Test.TestFixture.Migration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sunctum.Domain.Data.Dao.Migration.Plan
{
    internal class Valkyrie_1_ChangePlan_Version_1 : ChangePlan<Valkyrie_1, Version_1>
    {
        public Valkyrie_1_ChangePlan_Version_1(VersioningMode mode) : base("Valkyrie_1_1", PostMigrationVerification.TableExists, mode)
        {
        }

        public override async Task CreateTable(IConnection connection)
        {
            var dao = new Valkyrie_1_Dao(typeof(Version_1));
            dao.CurrentConnection = connection;
            await dao.CreateTableIfNotExistsAsync();
            ++ModifiedCount;
            await dao.CreateIndexIfNotExistsAsync();
            ++ModifiedCount;
        }

        public override async Task DropTable(IConnection connection)
        {
            var dao = new Valkyrie_1_Dao(typeof(Version_1));
            dao.CurrentConnection = connection;
            await dao.DropTableIfExistsAsync();
            ++ModifiedCount;
        }

        public override async Task UpgradeToTargetVersion(IConnection connection)
        {
            await CreateTable(connection);

            var dao = new Valkyrie_1_Dao(TargetVersion.GetType());
            if (await dao.CountAllAsync() > 0)
            {
                await dao.DeleteAsync(new Dictionary<string, object>());
            }

            await dao.UpgradeTableAsync(new VersionChangeUnit(typeof(VersionOrigin), TargetVersion.GetType()), Mode);
        }
    }
}
