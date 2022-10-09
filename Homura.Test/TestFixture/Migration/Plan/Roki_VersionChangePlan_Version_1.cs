using Homura.ORM.Migration;
using Homura.ORM.Setup;
using System.Collections.Generic;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class Roki_VersionChangePlan_Version_1 : ChangePlan<Version_1>
    {
        public Roki_VersionChangePlan_Version_1(VersioningMode mode) : base(mode)
        {
        }

        public override IEnumerable<IEntityVersionChangePlan> VersionChangePlanList
        {
            get
            {
                yield return new Roki_ChangePlan_Version_1(Mode);
            }
        }
    }
}