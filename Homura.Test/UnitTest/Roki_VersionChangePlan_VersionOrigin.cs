using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Migration.Plan;
using System.Collections.Generic;

namespace Homura.Test.UnitTest
{
    internal class Roki_VersionChangePlan_VersionOrigin : ChangePlan<VersionOrigin>
    {
        public Roki_VersionChangePlan_VersionOrigin(VersioningMode mode) : base(mode)
        {
        }

        public override IEnumerable<IEntityVersionChangePlan> VersionChangePlanList
        {
            get
            {
                yield return new Roki_ChangePlan_VersionOrigin(Mode);
            }
        }
    }
}