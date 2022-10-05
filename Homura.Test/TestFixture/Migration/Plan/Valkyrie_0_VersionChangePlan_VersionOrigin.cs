using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using System.Collections.Generic;

namespace Sunctum.Domain.Data.Dao.Migration.Plan
{
    internal class Valkyrie_0_VersionChangePlan_VersionOrigin : ChangePlan<VersionOrigin>
    {
        public override IEnumerable<IEntityVersionChangePlan> VersionChangePlanList
        {
            get
            {
                yield return new Valkyrie_0_ChangePlan_VersionOrigin();
            }
        }
    }
}
