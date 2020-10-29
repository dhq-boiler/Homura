

using Homura.Core;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using NLog;
using System;
using System.Diagnostics;
using static Homura.Core.Delegate;

namespace Homura.ORM.Setup
{
    public class DataVersionManager
    {
        private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

        private VersioningStrategy _Mode;

        public VersioningStrategy Mode
        {
            [DebuggerStepThrough]
            get
            { return _Mode; }
            set
            {
                value.Reset();
                _Mode = value;
            }
        }

        public IConnection CurrentConnection { get; set; }

        public static DataVersionManager DefaultSchemaVersion { get; set; } = new DataVersionManager();

        public DataVersionManager()
        {
            Mode = new VersioningStrategyNotSupported();
        }

        public DataVersionManager(IConnection connection)
        {
            CurrentConnection = connection;
        }

        public void RegisterChangePlan(IVersionChangePlan plan)
        {
            Mode.RegisterChangePlan(plan);
        }

        public void UnregisterChangePlan(VersionOrigin targetVersion)
        {
            Mode.UnregisterChangePlan(targetVersion);
        }

        public void RegisterChangePlan(IEntityVersionChangePlan plan)
        {
            Mode.RegisterChangePlan(plan);
        }

        public void UnregisterChangePlan(Type targetEntityType, VersionOrigin targetVersion)
        {
            Mode.UnregisterChangePlan(targetEntityType, targetVersion);
        }

        public void SetDefault()
        {
            DefaultSchemaVersion = this;
        }

        public IVersionChangePlan GetPlan(VersionOrigin targetVersion)
        {
            return Mode.GetPlan(targetVersion);
        }

        public IEntityVersionChangePlan GetPlan(Type entityType, VersionOrigin targetVersion)
        {
            return Mode.GetPlan(entityType, targetVersion);
        }

        public void UpgradeToTargetVersion()
        {
            Mode.UpgradeToTargetVersion(CurrentConnection);

            OnFinishedToUpgradeTo(new ModifiedEventArgs(Mode.ModifiedCount));
        }

        public event ModifiedEventHandler FinishedToUpgradeTo;

        protected virtual void OnFinishedToUpgradeTo(ModifiedEventArgs e)
        {
            FinishedToUpgradeTo?.Invoke(this, e);
        }
    }
}
