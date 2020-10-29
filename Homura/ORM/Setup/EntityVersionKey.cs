

using Homura.Core;
using Homura.ORM.Mapping;
using System;

namespace Homura.ORM.Setup
{
    public class EntityVersionKey : BaseObject
    {
        public Type TargetEntityType { get; set; }

        public VersionOrigin TargetVersion { get; set; }

        public EntityVersionKey(Type targetEntityType, VersionOrigin targetVersion)
        {
            TargetEntityType = targetEntityType;
            TargetVersion = targetVersion;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is EntityVersionKey)) return false;

            var o = obj as EntityVersionKey;
            return TargetEntityType.FullName.Equals(o.TargetEntityType.FullName)
                && TargetVersion.GetType().FullName.Equals(o.TargetVersion.GetType().FullName);
        }

        public override int GetHashCode()
        {
            return TargetEntityType.FullName.GetHashCode()
                 ^ TargetVersion.GetType().FullName.GetHashCode();
        }
    }
}
