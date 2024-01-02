

using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Homura.ORM.Setup
{
    internal class VersioningStrategyNotSupported : VersioningStrategy
    {
        public override IEnumerable<ChangePlanHasTable> ChangePlans => throw new NotSupportedException();

        internal override bool ExistsPlan(VersionOrigin targetVersion)
        {
            throw new NotSupportedException();
        }

        internal override bool ExistsPlan(Type entityType, VersionOrigin targetVersion)
        {
            throw new NotSupportedException();
        }

        internal override IVersionChangePlan GetPlan(VersionOrigin targetVersion)
        {
            throw new NotSupportedException();
        }

        internal override IEntityVersionChangePlan GetPlan(Type targetEntityType, VersionOrigin targetVersion)
        {
            throw new NotSupportedException();
        }

        internal override void RegisterChangePlan(IEntityVersionChangePlan plan)
        {
            throw new NotSupportedException();
        }

        internal override void RegisterChangePlan(IVersionChangePlan plan)
        {
            throw new NotSupportedException();
        }

        internal override void Reset()
        {
            //Do nothing
        }

        internal override void UnregisterChangePlan(VersionOrigin targetVersion)
        {
            throw new NotSupportedException();
        }

        internal override void UnregisterChangePlan(Type targetEntityType, VersionOrigin targetVersion)
        {
            throw new NotSupportedException();
        }

        internal override void UpgradeToTargetVersion(IConnection connection)
        {
            throw new NotSupportedException();
        }
    }
}
