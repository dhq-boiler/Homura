

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Homura.Core;

namespace Homura.ORM
{
    public abstract class BaseColumn : BaseObject, IColumn
    {
        public abstract string ColumnName { get; protected set; }

        public abstract string DataType { get; protected set; }

        public abstract IEnumerable<IDdlConstraint> Constraints { get; protected set; }

        public abstract int Order { get; protected set; }

        public abstract PropertyInfo PropInfo { get; protected set; }

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
            return new PlaceholderRightValue($"@{ColumnName.ToLower()}", PropInfo.GetValue(entity));
        }
    }
}
