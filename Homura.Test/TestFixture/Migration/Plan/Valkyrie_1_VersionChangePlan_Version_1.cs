using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Migration;
using System.Collections.Generic;

namespace Sunctum.Domain.Data.Dao.Migration.Plan
{
    internal class Valkyrie_1_VersionChangePlan_Version_1 : ChangePlan<Version_1>
    {
        public Valkyrie_1_VersionChangePlan_Version_1(VersioningMode mode) : base(mode)
        {
        }

        public override IEnumerable<IEntityVersionChangePlan> VersionChangePlanList
        {
            get
            {
                yield return new Valkyrie_1_ChangePlan_VersionOrigin(Mode);
            }
        }
    }
}
