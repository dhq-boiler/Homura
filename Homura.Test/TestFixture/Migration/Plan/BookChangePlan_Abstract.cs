

using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Entity;
using System.Threading.Tasks;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class BookChangePlan_Abstract<V> : ChangePlan<Book, V> where V : VersionOrigin
    {
        public BookChangePlan_Abstract(string targetTableName, PostMigrationVerification postMigrationVerification, VersioningMode mode) : base(targetTableName, postMigrationVerification, mode)
        {
        }

        public override async Task CreateTable(IConnection connection)
        {
            var dao = new BookDao(TargetVersion.GetType());
            dao.CurrentConnection = connection;
            await dao.CreateTableIfNotExistsAsync();
            ++ModifiedCount;
        }
    }
}
