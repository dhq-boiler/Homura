﻿using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.Test.TestFixture.Migration;
using System.Collections.Generic;

namespace Sunctum.Domain.Data.Dao.Migration.Plan
{
    internal class Valkyrie_1_VersionChangePlan_Version_2 : ChangePlan<Version_2>
    {
        public override IEnumerable<IEntityVersionChangePlan> VersionChangePlanList
        {
            get
            {
                yield return new Valkyrie_1_ChangePlan_Version_1();
            }
        }
    }
}
