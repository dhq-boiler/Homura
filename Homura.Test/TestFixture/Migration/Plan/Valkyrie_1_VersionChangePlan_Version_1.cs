using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.Test.TestFixture.Migration;
using System.Collections.Generic;

namespace Sunctum.Domain.Data.Dao.Migration.Plan
{
    internal class Valkyrie_1_VersionChangePlan_Version_1 : ChangePlanByVersion<Version_1>
    {
        public override IEnumerable<IEntityVersionChangePlan> VersionChangePlanList
        {
            get
            {
                yield return new Valkyrie_1_ChangePlan_VersionOrigin();
            }
        }
    }
}
