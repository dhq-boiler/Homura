using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using System.Collections.Generic;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class Frey_VersionChangePlan_VersionOrigin : ChangePlan<VersionOrigin>
    {
        public override IEnumerable<IEntityVersionChangePlan> VersionChangePlanList
        {
            get
            {
                yield return new Frey_ChangePlan_VersionOrigin();
            }
        }
    }
}