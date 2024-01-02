

using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Homura.ORM
{
    public static class Extensions
    {
        public static string ParameterListToString(this IEnumerable<PlaceholderRightValue> parameters)
        {
            string ret = "[";

            var queue = new Queue<PlaceholderRightValue>(parameters);

            while (queue.Count > 0)
            {
                var parameter = queue.Dequeue();
                ret += $"{parameter.Name}={parameter.Values.First()}";
                if (queue.Count > 0)
                {
                    ret += ", ";
                }
            }

            ret += "]";

            return ret;
        }

        public static void SetParameter(this IDbCommand command, PlaceholderRightValue parameter)
        {
            var p = command.CreateParameter();
            p.ParameterName = parameter.Name;
            p.Value = parameter.Values.First();
            command.Parameters.Add(p);
        }

        public static void SetParameters(this IDbCommand command, IEnumerable<PlaceholderRightValue> parameters)
        {
            foreach (var parameter in parameters)
            {
                command.SetParameter(parameter);
            }
        }

        public static IEnumerable<string> GetTableNames(this DbConnection conn)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            var tables = conn.GetSchemaAsync("Tables").Result;
            return tables.Rows.OfType<DataRow>().Select(r => r[2] as string);
        }

        public static IEnumerable<string> GetColumnNames(this DbConnection conn, string tableName)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            var tables = conn.GetSchemaAsync("Columns").Result;
            return tables.Rows.OfType<DataRow>().Where(r => r[2].ToString() == tableName).Select(r => r[3].ToString());
        }
    }
}
