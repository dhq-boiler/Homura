
using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using System.Collections.Generic;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class BookChangePlan_Version_1 : BookChangePlan_Abstract<Version_1>
    {
        public BookChangePlan_Version_1(VersioningMode mode) : base("Book_1", PostMigrationVerification.TableExists, mode)
        {
        }

        public override void UpgradeToTargetVersion(IConnection connection)
        {
            var dao = new BookDao(TargetVersion.GetType());
            dao.CurrentConnection = connection;

            dao.CreateTableIfNotExists();

            if (dao.CountAll() > 0)
            {
                dao.Delete(new Dictionary<string, object>());
            }

            dao.UpgradeTable(new VersionChangeUnit(typeof(VersionOrigin), TargetVersion.GetType()), Mode);
        }
    }
}
