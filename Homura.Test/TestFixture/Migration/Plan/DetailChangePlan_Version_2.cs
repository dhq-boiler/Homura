﻿

using Homura.ORM;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Dao;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class DetailChangePlan_Version_2 : DetailChangePlan_Abstract<Version_2>
    {
        public DetailChangePlan_Version_2(VersioningMode mode) : base("Detail_2", PostMigrationVerification.TableExists, mode)
        {
        }

        public override void UpgradeToTargetVersion(IConnection connection)
        {
            var dao = new DetailDao(TargetVersion.GetType());
            dao.CurrentConnection = connection;

            dao.CreateTableIfNotExists();

            if (dao.CountAll() > 0)
            {
                dao.Delete(new Dictionary<string, object>());
            }

            dao.UpgradeTable(new VersionChangeUnit(typeof(Version_1), TargetVersion.GetType()), Mode);
        }
    }
}
