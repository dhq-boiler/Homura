

using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace Homura.ORM
{
    public class Column : BaseColumn
    {
        public override string ColumnName { get; protected set; }
        public override Type EntityDataType { get; protected set; }

        public override string DBDataType { get; protected set; }

        public override IEnumerable<IDdlConstraint> Constraints { get; protected set; }

        public override int Order { get; protected set; }

        public override object DefaultValue { get; protected set; }


        public override HandlingDefaultValue PassType { get; protected set; }

        protected Column()
        { }

        public Column(string columnName, Type entityDataType, string dbDataType, IEnumerable<IDdlConstraint> constraints, int order, PropertyInfo propertyInfo, HandlingDefaultValue passType = ORM.HandlingDefaultValue.AsColumn, object defaultValue = null)
        {
            ColumnName = columnName;
            EntityDataType = entityDataType;
            DBDataType = dbDataType;
            Constraints = constraints?.ToList();
            Order = order;
            PropertyGetter = (obj) =>
            {
                var getter = obj.GetType().GetProperty(columnName);
                var columnValue = getter.GetValue(obj);
                if (columnValue is null)
                {
                    return null;
                }
                else if (columnValue.GetType().GetInterfaces().Contains(typeof(IReactiveProperty)))
                {
                    var rp = getter.GetValue(obj) as IReactiveProperty;
                    var ret = rp.GetType().GetProperty("Value").GetValue(rp);
                    return ret;
                }
                else
                {
                    return obj.GetType().GetProperty(columnName).GetValue(obj);
                }
            };
            PassType = passType;
            DefaultValue = defaultValue;
        }

        public override bool Equals(object obj)
        {
            if (GetType() != obj.GetType())
                return false;
            Column c = obj as Column;
            return ColumnName == c.ColumnName
                && DBDataType == c.DBDataType
                && Order == c.Order;
        }

        public override int GetHashCode()
        {
            return ColumnName.GetHashCode()
                 ^ DBDataType.GetHashCode()
                 ^ Order.GetHashCode();
        }
    }
}
