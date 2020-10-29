

using System;

namespace Homura.ORM.Mapping
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class UntilAttribute : Attribute
    {
        public UntilAttribute(Type versionUntil)
        {
            VersionUntil = versionUntil;
        }

        public Type VersionUntil { get; set; }
    }
}
