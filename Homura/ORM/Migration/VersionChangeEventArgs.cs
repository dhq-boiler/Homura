

using Homura.ORM.Mapping;
using System;

namespace Homura.ORM.Migration
{
    public class VersionChangeEventArgs : EventArgs
    {
        private readonly VersionOrigin _version;

        public VersionChangeEventArgs(VersionOrigin version)
        {
            _version = version;
        }

        public VersionOrigin Version { get { return _version; } }
    }
}
