

using Homura.ORM.Mapping;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Homura.Core.Delegate;

namespace Homura.ORM.Migration
{
    public class ChangePlanByVersion<V> : IVersionChangePlan
                                where V : VersionOrigin
    {
        private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

        public VersionOrigin TargetVersion { get { return Activator.CreateInstance<V>(); } }

        public virtual IEnumerable<IEntityVersionChangePlan> VersionChangePlanList { get; private set; }

        public int ModifiedCount {[DebuggerStepThrough] get; set; }

        public ChangePlanByVersion()
        {
            VersionChangePlanList = new List<IEntityVersionChangePlan>();
        }

        public void AddVersionChangePlan(IEntityVersionChangePlan plan)
        {
            var list = VersionChangePlanList.ToList();
            list.Add(plan);
            VersionChangePlanList = list;
        }

        public void RemoveVersionChangePlan(IEntityVersionChangePlan plan)
        {
            var list = VersionChangePlanList.ToList();
            list.Remove(plan);
            VersionChangePlanList = list;
        }

        public void DowngradeToTargetVersion(IConnection connection)
        {
            OnBeginToDowngradeTo(new VersionChangeEventArgs(TargetVersion));

            s_logger.Info($"Begin to downgrade to {TargetVersion.GetType().Name}.");

            foreach (var vcp in VersionChangePlanList)
            {
                vcp.DowngradeToTargetVersion(connection);
                ModifiedCount += vcp.ModifiedCount;
            }

            s_logger.Info($"Finish to downgrade to {TargetVersion.GetType().Name}.");

            OnFinishedToDowngradeTo(new VersionChangeEventArgs(TargetVersion));
        }

        public void UpgradeToTargetVersion(IConnection connection)
        {
            OnBeginToUpgradeTo(new VersionChangeEventArgs(TargetVersion));

            s_logger.Info($"Begin to upgrade to {TargetVersion.GetType().Name}.");

            foreach (var vcp in VersionChangePlanList)
            {
                vcp.UpgradeToTargetVersion(connection);
                ModifiedCount += vcp.ModifiedCount;
            }

            s_logger.Info($"Finish to upgrade to {TargetVersion.GetType().Name}.");

            OnFinishedToUpgradeTo(new VersionChangeEventArgs(TargetVersion));
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ChangePlanByVersion<V>)) return false;
            var operand = obj as ChangePlanByVersion<V>;
            return TargetVersion.GetType().FullName.Equals(operand.TargetVersion.GetType().FullName)
                && VersionChangePlanList.Equals(operand.VersionChangePlanList);
        }

        public override int GetHashCode()
        {
            return TargetVersion.GetHashCode()
                ^ VersionChangePlanList.GetHashCode();
        }

        public event BeginToUpgradeToEventHandler BeginToUpgradeTo;
        public event FinishedToUpgradeToEventHandler FinishedToUpgradeTo;
        public event BeginToDowngradeToEventHandler BeginToDowngradeTo;
        public event FinishedToDowngradeToEventHandler FinishedToDowngradeTo;

        protected virtual void OnBeginToUpgradeTo(VersionChangeEventArgs e)
        {
            BeginToUpgradeTo?.Invoke(this, e);
        }

        protected virtual void OnFinishedToUpgradeTo(VersionChangeEventArgs e)
        {
            FinishedToUpgradeTo?.Invoke(this, e);
        }

        protected virtual void OnBeginToDowngradeTo(VersionChangeEventArgs e)
        {
            BeginToDowngradeTo?.Invoke(this, e);
        }

        protected virtual void OnFinishedToDowngradeTo(VersionChangeEventArgs e)
        {
            FinishedToDowngradeTo?.Invoke(this, e);
        }
    }
}
