using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using System.Collections.Generic;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class Frey_VersionChangePlan_VersionOrigin : ChangePlan<VersionOrigin>
    {
        public Frey_VersionChangePlan_VersionOrigin(VersioningMode mode) : base(mode)
        {
        }

        public override IEnumerable<IEntityVersionChangePlan> VersionChangePlanList
        {
            get
            {
                yield return new Frey_ChangePlan_VersionOrigin(Mode);
            }
        }
    }
}