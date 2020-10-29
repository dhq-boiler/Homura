

using System;

namespace Homura.ORM.Migration
{
    public class VersionChangeUnit
    {
        public Type From { get; set; }

        public Type To { get; set; }

        public VersionChangeUnit()
        { }

        public VersionChangeUnit(Type from, Type to)
        {
            From = from;
            To = to;
        }

        public VersionChangeUnit Reverse()
        {
            return new VersionChangeUnit(To, From);
        }
    }
}
