using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using System.Collections.Generic;

namespace Sunctum.Domain.Data.Dao.Migration.Plan
{
    internal class Valkyrie_0_VersionChangePlan_VersionOrigin : ChangePlan<VersionOrigin>
    {
        public Valkyrie_0_VersionChangePlan_VersionOrigin(VersioningMode mode) : base(mode)
        {
        }

        public override IEnumerable<IEntityVersionChangePlan> VersionChangePlanList
        {
            get
            {
                yield return new Valkyrie_0_ChangePlan_VersionOrigin(Mode);
            }
        }
    }
}
