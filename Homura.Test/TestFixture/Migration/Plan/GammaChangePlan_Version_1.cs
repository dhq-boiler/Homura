﻿
using Homura.ORM;
using Homura.ORM.Setup;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class GammaChangePlan_Version_1 : GammaChangePlan_Abstract<Version_1>
    {
        public GammaChangePlan_Version_1(VersioningMode mode) : base(mode)
        {
        }

        public override void UpgradeToTargetVersion(IConnection connection)
        {
            CreateTable(connection);
        }
    }
}
