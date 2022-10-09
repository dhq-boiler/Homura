using Homura.ORM.Migration;
using Homura.ORM.Setup;
using System.Collections.Generic;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class Roki_VersionChangePlan_Version_2 : ChangePlan<Version_2>
    {
        public Roki_VersionChangePlan_Version_2(VersioningMode mode) : base(mode)
        {
        }

        public override IEnumerable<IEntityVersionChangePlan> VersionChangePlanList
        {
            get
            {
                yield return new Roki_ChangePlan_Version_2(Mode);
            }
        }
    }
}