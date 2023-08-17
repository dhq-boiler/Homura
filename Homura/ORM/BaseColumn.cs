

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Homura.Core;
using Reactive.Bindings;

namespace Homura.ORM
{
    public abstract class BaseColumn : BaseObject, IColumn
    {
        public abstract string ColumnName { get; protected set; }

        public abstract Type EntityDataType { get; protected set; }

        public abstract string DBDataType { get; protected set; }

        public abstract IEnumerable<IDdlConstraint> Constraints { get; protected set; }

        public abstract int Order { get; protected set; }

        public abstract object DefaultValue { get; protected set; }


        public Func<object, object> PropertyGetter { get; protected set; }

        private object GetValue(object obj)
        {
            return PropertyGetter(obj);
        }

        public abstract HandlingDefaultValue PassType { get; protected set; }

        public string ConstraintsToSql()
        {
            string sql = "";
            var queue = new Queue<IDdlConstraint>(Constraints);
            while (queue.Count() > 0)
            {
                var constraint = queue.Dequeue();
                sql += constraint.ToSql();
                if (queue.Count() > 0)
                {
                    sql += " ";
                }
            }
            return sql;
        }

        public PlaceholderRightValue ToParameter(Dictionary<string, object> idDic)
        {
            return new PlaceholderRightValue($"@{ColumnName.ToLower()}", idDic[ColumnName]);
        }

        public PlaceholderRightValue ToParameter(EntityBaseObject entity)
        {
            return new PlaceholderRightValue($"@{ColumnName.ToLower()}", PropertyGetter(entity));
        }

        public string WrapOutput()
        {
            if (PassType == ORM.HandlingDefaultValue.AsColumn)
            {
                return ColumnName.ToString();
            }
            else if (PassType == ORM.HandlingDefaultValue.AsValue)
            {
                if (DBDataType == "TEXT")
                {
                    if (DefaultValue is null)
                    {
                        return "null";
                    }
                    return $"\'{DefaultValue}'";
                }
                else
                {
                    if (DefaultValue is null)
                    {
                        return "null";
                    }
                    switch (EntityDataType.Name)
                    {
                        case "String":
                            return $"\'{DefaultValue}\'";
                        case "int":
                            return DefaultValue.ToString();
                        case "Boolean":
                            return bool.Parse(DefaultValue.ToString()) ? "1" : "0";
                        case "Guid":
                            return Guid.Parse(DefaultValue.ToString()).ToString();
                    }
                    return DefaultValue.ToString();
                }
            }
            throw new System.Exception("No match");
        }
    }
}
