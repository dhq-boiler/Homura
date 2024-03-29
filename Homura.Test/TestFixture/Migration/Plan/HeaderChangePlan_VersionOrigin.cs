﻿

using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Setup;
using System.Threading.Tasks;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class HeaderChangePlan_VersionOrigin : HeaderChangePlan_Abstract<VersionOrigin>
    {
        public HeaderChangePlan_VersionOrigin(VersioningMode mode) : base("Header", ORM.Migration.PostMigrationVerification.TableExists, mode)
        {
        }

        public override void UpgradeToTargetVersion(IConnection connection)
        {
            CreateTable(connection);
        }
    }
}
