

using Homura.Core;
using Homura.ORM.Mapping;
using System;
using System.Diagnostics;

namespace Homura.ORM.Migration
{
    public abstract class ChangePlanByTable<E, V> : IEntityVersionChangePlan, IModifiedCounter
                                          where E : EntityBaseObject
                                          where V : VersionOrigin
    {
        public Type TargetEntityType { get; set; }

        public VersionOrigin TargetVersion { get; set; }

        public int ModifiedCount {[DebuggerStepThrough] get; set; }

        protected ChangePlanByTable()
        {
            TargetEntityType = typeof(E);
            TargetVersion = Activator.CreateInstance<V>();
        }

        public virtual void CreateTable(IConnection connection)
        {
            throw new NotSupportedException();
        }

        public virtual void DropTable(IConnection connection)
        {
            throw new NotSupportedException();
        }

        public virtual void UpgradeToTargetVersion(IConnection connection)
        {
            throw new NotSupportedException();
        }

        public virtual void DowngradeToTargetVersion(IConnection connection)
        {
            throw new NotSupportedException();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ChangePlanByTable<E, V>)) return false;
            var operand = obj as ChangePlanByTable<E, V>;
            return TargetEntityType.FullName.Equals(operand.TargetEntityType.FullName)
                && TargetVersion.GetType().FullName.Equals(operand.TargetVersion.GetType().FullName);
        }

        public override int GetHashCode()
        {
            return TargetEntityType.GetHashCode() ^ TargetVersion.GetHashCode();
        }
    }
}
