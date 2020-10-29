

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Homura.ORM
{
    public class Column : BaseColumn
    {
        public override string ColumnName { get; protected set; }

        public override string DataType { get; protected set; }

        public override IEnumerable<IDdlConstraint> Constraints { get; protected set; }

        public override int Order { get; protected set; }

        public override PropertyInfo PropInfo { get; protected set; }

        protected Column()
        { }

        public Column(string columnName, string dataType, IEnumerable<IDdlConstraint> constraints, int order, PropertyInfo propertyInfo)
        {
            ColumnName = columnName;
            DataType = dataType;
            Constraints = constraints?.ToList();
            Order = order;
            PropInfo = propertyInfo;
        }

        public override bool Equals(object obj)
        {
            if (GetType() != obj.GetType())
                return false;
            Column c = obj as Column;
            return ColumnName == c.ColumnName
                && DataType == c.DataType
                && Order == c.Order;
        }

        public override int GetHashCode()
        {
            return ColumnName.GetHashCode()
                 ^ DataType.GetHashCode()
                 ^ Order.GetHashCode();
        }
    }
}
