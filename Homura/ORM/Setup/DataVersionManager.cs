

using Homura.Core;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static Homura.Core.Delegate;

namespace Homura.ORM.Setup
{
    public class DataVersionManager
    {
        public List<VersioningStrategy> Strategies { get; } = new List<VersioningStrategy>();

        public IConnection CurrentConnection { get; set; }

        public static DataVersionManager DefaultSchemaVersion { get; set; } = new DataVersionManager();

        public DataVersionManager()
        {
        }

        public DataVersionManager(IConnection connection)
        {
            CurrentConnection = connection;
        }
        private static bool HasFlag(VersioningMode value, VersioningMode target)
        {
            return (value & target) == target;
        }

        public void RegisterChangePlan(IVersionChangePlan plan)
        {
            VersioningStrategy strategy = new VersioningStrategyNotSupported();
            if (HasFlag(plan.Mode, VersioningMode.ByTick))
            {
                strategy = new VersioningStrategyByTick();
            }
            if (HasFlag(plan.Mode, VersioningMode.ByAlterTable))
            {
                strategy = new VersioningStrategyByAlterTable();
            }
            Strategies.Add(strategy);
            strategy.RegisterChangePlan(plan);
        }

        public void UnregisterChangePlan(VersionOrigin targetVersion)
        {
            foreach (var strategy in Strategies)
            {
                strategy.UnregisterChangePlan(targetVersion);
            }
        }

        public void RegisterChangePlan(IEntityVersionChangePlan plan)
        {
            VersioningStrategy strategy = new VersioningStrategyNotSupported();
            if (HasFlag(plan.Mode, VersioningMode.ByTick))
            {
                strategy = new VersioningStrategyByTick();
            }
            if (HasFlag(plan.Mode, VersioningMode.ByAlterTable))
            {
                strategy = new VersioningStrategyByAlterTable();
            }
            Strategies.Add(strategy);
            strategy.RegisterChangePlan(plan);
        }

        public void UnregisterChangePlan(Type targetEntityType, VersionOrigin targetVersion)
        {
            foreach (var strategy in Strategies)
            {
                strategy.UnregisterChangePlan(targetEntityType, targetVersion);
            }
        }

        public void SetDefault()
        {
            DefaultSchemaVersion = this;
        }

        public IVersionChangePlan GetPlan(VersionOrigin targetVersion)
        {
            var a = Strategies.FirstOrDefault(x => x.ExistsPlan(targetVersion));
            if (a is null)
                throw new KeyNotFoundException();
            return a.GetPlan(targetVersion);
        }

        public IEntityVersionChangePlan GetPlan(Type entityType, VersionOrigin targetVersion)
        {
            var a = Strategies.FirstOrDefault(x => x.ExistsPlan(entityType, targetVersion));
            if (a is null)
                throw new KeyNotFoundException();
            return a.GetPlan(entityType, targetVersion);
        }

        public async Task UpgradeToTargetVersion()
        {
            var processed = false;
            foreach (var strategy in Strategies.Where(x => x.State == VersionStrategyState.Ready))
            {
                await strategy.UpgradeToTargetVersion(CurrentConnection);
                strategy.State = VersionStrategyState.Processed;
                processed = true;
            }

            if (processed)
            {
                OnFinishedToUpgradeTo(new ModifiedEventArgs(Strategies.Sum(x => x.ModifiedCount)));
            }
        }

        public event ModifiedEventHandler FinishedToUpgradeTo;

        protected virtual void OnFinishedToUpgradeTo(ModifiedEventArgs e)
        {
            FinishedToUpgradeTo?.Invoke(this, e);
        }
    }
}
