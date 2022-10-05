﻿
using Homura.ORM;
using Homura.ORM.Setup;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class BetaChangePlan_Version_1 : BetaChangePlan_Abstract<Version_1>
    {
        public BetaChangePlan_Version_1(VersioningMode mode) : base(mode)
        {
        }

        public override void UpgradeToTargetVersion(IConnection connection)
        {
            CreateTable(connection);
        }
    }
}
