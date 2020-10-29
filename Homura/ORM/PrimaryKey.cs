

using System.Collections.Generic;

namespace Homura.ORM
{
    public class PrimaryKey : IDdlConstraint
    {
        public PrimaryKey(params string[] columnNames)
        {
            ColumnNames = columnNames;
        }

        public string[] ColumnNames { get; set; }

        public string ToSql()
        {
            string sql = "PRIMARY KEY";

            if (ColumnNames == null)
            {
                return sql;
            }

            var queue = new Queue<string>(ColumnNames);
            sql += "(";

            while (queue.Count > 0)
            {
                var columnName = queue.Dequeue();
                sql += columnName;
                if (queue.Count > 0)
                {
                    sql += ", ";
                }
            }
            sql += ")";

            return sql;
        }
    }
}
