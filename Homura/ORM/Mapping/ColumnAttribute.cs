

using System;

namespace Homura.ORM.Mapping
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute(string columnName, string columnType, int order, PassAsColumnOrValue passType = PassAsColumnOrValue.AsColumn, object defaultValue = null)
        {
            ColumnName = columnName;
            ColumnType = columnType;
            Order = order;
            PassType = passType;
            DefaultValue = defaultValue;
        }

        public string ColumnName { get; set; }
        public string ColumnType { get; set; }
        public int Order { get; set; }
        public PassAsColumnOrValue PassType { get; set; }
        public object DefaultValue { get; set; }
    }
}
