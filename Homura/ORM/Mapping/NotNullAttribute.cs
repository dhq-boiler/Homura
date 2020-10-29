

using System;

namespace Homura.ORM.Mapping
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NotNullAttribute : Attribute, IDdlConstraintAttribute
    {
        public NotNullAttribute()
        { }

        public IDdlConstraint ToConstraint()
        {
            return new NotNull();
        }
    }
}
