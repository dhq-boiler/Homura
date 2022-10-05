﻿

using Homura.Core;
using Homura.ORM.Mapping;
using Homura.ORM.Setup;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Homura.Core.Delegate;

namespace Homura.ORM.Migration
{
    public class ChangePlan<E, V> : IEntityVersionChangePlan, IModifiedCounter where E : EntityBaseObject
                                                                               where V : VersionOrigin
    {
        private VersioningStrategy _Strategy;
        private VersioningMode _Mode;

        public ChangePlan(VersioningMode mode)
        {
            Mode = mode;
            TargetEntityType = typeof(E);
            TargetVersion = Activator.CreateInstance<V>();
        }

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

        public Type TargetEntityType { get; set; }

        public VersionOrigin TargetVersion { get; set; }

        public int ModifiedCount {[DebuggerStepThrough] get; set; }

        public virtual void CreateTable(IConnection connection)
        {
            throw new NotImplementedException();
        }

        public virtual void DropTable(IConnection connection)
        {
            throw new NotImplementedException();
        }

        public virtual void UpgradeToTargetVersion(IConnection connection)
        {
            throw new NotImplementedException();
        }

        public virtual void DowngradeToTargetVersion(IConnection connection)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ChangePlan<E, V>)) return false;
            var operand = obj as ChangePlan<E, V>;
            return TargetEntityType.FullName.Equals(operand.TargetEntityType.FullName)
                && TargetVersion.GetType().FullName.Equals(operand.TargetVersion.GetType().FullName);
        }

        public override int GetHashCode()
        {
            return TargetEntityType.GetHashCode() ^ TargetVersion.GetHashCode();
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


    public class ChangePlan<V> : IVersionChangePlan where V : VersionOrigin
    {
        private VersioningStrategy _Strategy;
        private VersioningMode _Mode;

        public ChangePlan(VersioningMode mode)
        {
            Mode = mode;
            VersionChangePlanList = new List<IEntityVersionChangePlan>();
        }

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

        public VersionOrigin TargetVersion { get { return Activator.CreateInstance<V>(); } }

        public virtual IEnumerable<IEntityVersionChangePlan> VersionChangePlanList { get; private set; }

        public int ModifiedCount { [DebuggerStepThrough] get; set; }


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

            LogManager.GetCurrentClassLogger().Info($"Begin to downgrade to {TargetVersion.GetType().Name}.");

            foreach (var vcp in VersionChangePlanList)
            {
                vcp.Mode = Mode;
                vcp.DowngradeToTargetVersion(connection);
                ModifiedCount += vcp.ModifiedCount;
            }

            LogManager.GetCurrentClassLogger().Info($"Finish to downgrade to {TargetVersion.GetType().Name}.");

            OnFinishedToDowngradeTo(new VersionChangeEventArgs(TargetVersion));
        }

        public void UpgradeToTargetVersion(IConnection connection)
        {
            OnBeginToUpgradeTo(new VersionChangeEventArgs(TargetVersion));

            LogManager.GetCurrentClassLogger().Info($"Begin to upgrade to {TargetVersion.GetType().Name}.");

            foreach (var vcp in VersionChangePlanList)
            {
                vcp.Mode = Mode;
                vcp.UpgradeToTargetVersion(connection);
                ModifiedCount += vcp.ModifiedCount;
            }

            LogManager.GetCurrentClassLogger().Info($"Finish to upgrade to {TargetVersion.GetType().Name}.");

            OnFinishedToUpgradeTo(new VersionChangeEventArgs(TargetVersion));
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ChangePlan<V>)) return false;
            var operand = obj as ChangePlan<V>;
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
