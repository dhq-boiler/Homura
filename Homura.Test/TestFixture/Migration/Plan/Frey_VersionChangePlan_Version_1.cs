using Homura.ORM.Migration;
using System.Collections.Generic;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class Frey_VersionChangePlan_Version_1 : ChangePlan<Version_1>
    {
        public override IEnumerable<IEntityVersionChangePlan> VersionChangePlanList
        {
            get
            {
                yield return new Frey_ChangePlan_Version_1();
            }
        }
    }
}