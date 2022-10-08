

using Homura.Core;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using static Homura.Core.Delegate;

namespace Homura.ORM.Setup
{
    public class DataVersionManager
    {
        private List<VersioningStrategy> versioningStrategies = new List<VersioningStrategy>();
        //private VersioningStrategy _Strategy;
        //private VersioningMode _Mode;

        //public VersioningMode Mode
        //{
        //    get { return _Mode; }
        //    set
        //    {
        //        if (HasFlag(value, VersioningMode.ByTick))
        //        {
        //            _Strategy = VersioningStrategy.ByTick;
        //        }
        //        if (HasFlag(value, VersioningMode.ByAlterTable))
        //        {
        //            _Strategy = VersioningStrategy.ByAlterTable;
        //        }
        //        if (HasFlag(value, VersioningMode.DropTableCastedOff))
        //        {
        //            _Strategy.SetOption(VersioningMode.DropTableCastedOff);
        //        }
        //        if (HasFlag(value, VersioningMode.DeleteAllRecordInTableCastedOff))
        //        {
        //            _Strategy.SetOption(VersioningMode.DeleteAllRecordInTableCastedOff);
        //        }
        //        _Strategy.Reset();
        //        _Mode = value;
        //    }
        //}

        //private static bool HasFlag(VersioningMode value, VersioningMode target)
        //{
        //    return (value & target) == target;
        //}

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
            versioningStrategies.Add(strategy);
            strategy.RegisterChangePlan(plan);
        }

        public void UnregisterChangePlan(VersionOrigin targetVersion)
        {
            foreach (var strategy in versioningStrategies)
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
            versioningStrategies.Add(strategy);
            strategy.RegisterChangePlan(plan);
        }

        public void UnregisterChangePlan(Type targetEntityType, VersionOrigin targetVersion)
        {
            foreach (var strategy in versioningStrategies)
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
            var a = versioningStrategies.FirstOrDefault(x => x.ExistsPlan(targetVersion));
            if (a is null)
                throw new KeyNotFoundException();
            return a.GetPlan(targetVersion);
        }

        public IEntityVersionChangePlan GetPlan(Type entityType, VersionOrigin targetVersion)
        {
            var a = versioningStrategies.FirstOrDefault(x => x.ExistsPlan(entityType, targetVersion));
            if (a is null)
                throw new KeyNotFoundException();
            return a.GetPlan(entityType, targetVersion);
        }

        public void UpgradeToTargetVersion()
        {
            foreach (var strategy in versioningStrategies.Where(x => x.State == VersionStrategyState.Ready))
            {
                strategy.UpgradeToTargetVersion(CurrentConnection);
                strategy.State = VersionStrategyState.Processed;
            }

            OnFinishedToUpgradeTo(new ModifiedEventArgs(versioningStrategies.Sum(x => x.ModifiedCount)));
        }

        public event ModifiedEventHandler FinishedToUpgradeTo;

        protected virtual void OnFinishedToUpgradeTo(ModifiedEventArgs e)
        {
            FinishedToUpgradeTo?.Invoke(this, e);
        }
    }
}
