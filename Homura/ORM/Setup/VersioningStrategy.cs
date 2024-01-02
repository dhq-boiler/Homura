

using Homura.Core;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Homura.ORM.Setup
{
    public abstract class VersioningStrategy : IModifiedCounter
    {
        internal static readonly VersioningStrategy ByTable = new VersioningStrategyByTable();
        internal static readonly VersioningStrategy ByTick = new VersioningStrategyByTick();
        internal static readonly VersioningStrategy ByAlterTable = new VersioningStrategyByAlterTable();

        public int ModifiedCount {[DebuggerStepThrough] get; set; }

        public VersioningMode VersioningMode { get; set; }

        public VersionStrategyState State { get; internal set; }

        public abstract IEnumerable<ChangePlanBase> ChangePlans { get; }

        internal abstract void RegisterChangePlan(IVersionChangePlan plan);

        internal abstract void RegisterChangePlan(IEntityVersionChangePlan plan);

        internal abstract void UnregisterChangePlan(VersionOrigin targetVersion);

        internal abstract void UnregisterChangePlan(Type targetEntityType, VersionOrigin targetVersion);

        internal abstract IVersionChangePlan GetPlan(VersionOrigin targetVersion);

        internal abstract IEntityVersionChangePlan GetPlan(Type targetEntityType, VersionOrigin targetVersion);

        internal abstract void Reset();

        internal abstract void UpgradeToTargetVersion(IConnection connection);

        internal abstract bool ExistsPlan(VersionOrigin targetVersion);

        internal abstract bool ExistsPlan(Type entityType, VersionOrigin targetVersion);

        internal void SetOption(VersioningMode options)
        {
            VersioningMode = VersioningMode & options;
        }
    }
}
