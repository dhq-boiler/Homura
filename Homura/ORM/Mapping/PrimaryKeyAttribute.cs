

using System;

namespace Homura.ORM.Mapping
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class PrimaryKeyAttribute : Attribute, IDdlConstraintAttribute
    {
        /// <summary>
        /// for properties
        /// </summary>
        public PrimaryKeyAttribute()
        { }

        /// <summary>
        /// for classes
        /// </summary>
        /// <param name="columnName"></param>
        public PrimaryKeyAttribute(params string[] columnNames)
        {
            ColumnNames = columnNames;
        }

        public string[] ColumnNames { get; set; }

        public IDdlConstraint ToConstraint()
        {
            return new PrimaryKey(ColumnNames);
        }
    }
}
