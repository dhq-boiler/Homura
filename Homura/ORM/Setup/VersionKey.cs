

using Homura.Core;
using Homura.ORM.Mapping;

namespace Homura.ORM.Setup
{
    public class VersionKey : BaseObject
    {
        public VersionOrigin TargetVersion { get; set; }

        public VersionKey(VersionOrigin targetVersion)
        {
            TargetVersion = targetVersion;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is VersionKey)) return false;

            var o = obj as VersionKey;
            return TargetVersion.GetType().FullName.Equals(o.TargetVersion.GetType().FullName);
        }

        public override int GetHashCode()
        {
            return TargetVersion.GetType().FullName.GetHashCode();
        }
    }
}
