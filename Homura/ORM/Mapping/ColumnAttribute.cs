

using System;

namespace Homura.ORM.Mapping
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute(string columnName, string columnType, int order)
        {
            ColumnName = columnName;
            ColumnType = columnType;
            Order = order;
        }

        public string ColumnName { get; set; }
        public string ColumnType { get; set; }
        public int Order { get; set; }
    }
}
