

using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Linq;

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

            var objSchemaInfo = conn.GetSchema(OleDbMetaDataCollectionNames.Tables, new string[] { null, null, null, null });
            return objSchemaInfo.AsEnumerable().Select(r => r.Field<string>("TABLE_NAME"));
        }

        public static IEnumerable<string> GetColumnNames(this DbConnection conn, string tableName)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            var objSchemaInfo = conn.GetSchema(OleDbMetaDataCollectionNames.Columns, new string[] { null, null, tableName, null });
            return objSchemaInfo.AsEnumerable().Select(r => r.Field<string>("COLUMN_NAME"));
        }
    }
}
