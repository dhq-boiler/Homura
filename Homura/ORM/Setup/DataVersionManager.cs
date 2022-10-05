

using Homura.Core;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using System;
using static Homura.Core.Delegate;

namespace Homura.ORM.Setup
{
    public class DataVersionManager
    {
        private VersioningStrategy _Strategy;
        private VersioningMode _Mode;

        public VersioningMode Mode
        {
            get { return _Mode; }
            set
            {
                if (HasFlag(value, VersioningMode.ByTick))
                {
                    _Strategy = VersioningStrategy.ByTick;
                }
                if (HasFlag(value, VersioningMode.ByAlterTable))
                {
                    _Strategy = VersioningStrategy.ByAlterTable;
                }
                if (HasFlag(value, VersioningMode.DropTableCastedOff))
                {
                    _Strategy.SetOption(VersioningMode.DropTableCastedOff);
                }
                if (HasFlag(value, VersioningMode.DeleteAllRecordInTableCastedOff))
                {
                    _Strategy.SetOption(VersioningMode.DeleteAllRecordInTableCastedOff);
                }
                _Strategy.Reset();
                _Mode = value;
            }
        }

        private static bool HasFlag(VersioningMode value, VersioningMode target)
        {
            return (value & target) == target;
        }

        public IConnection CurrentConnection { get; set; }

        public static DataVersionManager DefaultSchemaVersion { get; set; } = new DataVersionManager();

        public DataVersionManager()
        {
            _Strategy = new VersioningStrategyNotSupported();
        }

        public DataVersionManager(IConnection connection)
        {
            CurrentConnection = connection;
        }

        public void RegisterChangePlan(IVersionChangePlan plan)
        {
            _Strategy.VersioningMode = Mode;
            _Strategy.RegisterChangePlan(plan);
        }

        public void UnregisterChangePlan(VersionOrigin targetVersion)
        {
            _Strategy.UnregisterChangePlan(targetVersion);
        }

        public void RegisterChangePlan(IEntityVersionChangePlan plan)
        {
            _Strategy.VersioningMode = Mode;
            _Strategy.RegisterChangePlan(plan);
        }

        public void UnregisterChangePlan(Type targetEntityType, VersionOrigin targetVersion)
        {
            _Strategy.UnregisterChangePlan(targetEntityType, targetVersion);
        }

        public void SetDefault()
        {
            DefaultSchemaVersion = this;
        }

        public IVersionChangePlan GetPlan(VersionOrigin targetVersion)
        {
            return _Strategy.GetPlan(targetVersion);
        }

        public IEntityVersionChangePlan GetPlan(Type entityType, VersionOrigin targetVersion)
        {
            return _Strategy.GetPlan(entityType, targetVersion);
        }

        public void UpgradeToTargetVersion()
        {
            _Strategy.UpgradeToTargetVersion(CurrentConnection);

            OnFinishedToUpgradeTo(new ModifiedEventArgs(_Strategy.ModifiedCount));
        }

        public event ModifiedEventHandler FinishedToUpgradeTo;

        protected virtual void OnFinishedToUpgradeTo(ModifiedEventArgs e)
        {
            FinishedToUpgradeTo?.Invoke(this, e);
        }
    }
}
