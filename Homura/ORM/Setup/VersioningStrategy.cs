

using Homura.Core;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using System;
using System.Diagnostics;

namespace Homura.ORM.Setup
{
    public abstract class VersioningStrategy : IModifiedCounter
    {
        public static readonly VersioningStrategy ByTable = new VersioningStrategyByTable();
        public static readonly VersioningStrategy ByTick = new VersioningStrategyByTick();

        public int ModifiedCount {[DebuggerStepThrough] get; set; }

        internal abstract void RegisterChangePlan(IVersionChangePlan plan);

        internal abstract void RegisterChangePlan(IEntityVersionChangePlan plan);

        internal abstract void UnregisterChangePlan(VersionOrigin targetVersion);

        internal abstract void UnregisterChangePlan(Type targetEntityType, VersionOrigin targetVersion);

        internal abstract IVersionChangePlan GetPlan(VersionOrigin targetVersion);

        internal abstract IEntityVersionChangePlan GetPlan(Type targetEntityType, VersionOrigin targetVersion);

        internal abstract void Reset();

        internal abstract void UpgradeToTargetVersion(IConnection connection);
    }
}
