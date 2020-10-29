

using System;

namespace Homura.Core
{
    public class ModifiedEventArgs : EventArgs, IModifiedCounter
    {
        private readonly int _modifiedCount;

        public ModifiedEventArgs(int modifiedCount)
        {
            _modifiedCount = modifiedCount;
        }

        public int ModifiedCount { get { return _modifiedCount; } }
    }
}