using Homura.ORM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homura.Extensions
{
    public static class Extensions
    {
        public static Guid SafeGetGuid(this IDataRecord rdr, string columnName, ITable table)
        {
            int index = rdr.CheckColumnExists(columnName, table);

            bool isNull = rdr.IsDBNull(index);

            return isNull ? Guid.Empty : rdr.GetGuid(index);
        }

        public static Guid? SafeGetNullableGuid(this IDataRecord rdr, string columnName, ITable table)
        {
            int index = rdr.CheckColumnExists(columnName, table);

            bool isNull = rdr.IsDBNull(index);

            return isNull ? null : rdr.GetGuid(index);
        }

        public static string SafeGetString(this IDataRecord rdr, string columnName, ITable table)
        {
            int index = CheckColumnExists(rdr, columnName, table);

            bool isNull = rdr.IsDBNull(index);

            return isNull ? null : rdr.GetString(index);
        }

        public static int SafeGetInt(this IDataRecord rdr, string columnName, ITable table)
        {
            int index = CheckColumnExists(rdr, columnName, table);

            bool isNull = rdr.IsDBNull(index);

            return isNull ? int.MinValue : rdr.GetInt32(index);
        }

        public static int? SafeGetNullableInt(this IDataRecord rdr, string columnName, ITable table)
        {
            int index = CheckColumnExists(rdr, columnName, table);

            bool isNull = rdr.IsDBNull(index);

            return isNull ? null : (int?)rdr.GetInt32(index);
        }

        public static long SafeGetLong(this IDataRecord rdr, string columnName, ITable table)
        {
            int index = CheckColumnExists(rdr, columnName, table);
            return rdr.GetInt64(index);
        }

        public static long? SafeNullableGetLong(this IDataRecord rdr, string columnName, ITable table)
        {
            int index = CheckColumnExists(rdr, columnName, table);

            bool isNull = rdr.IsDBNull(index);

            return isNull ? null : (long?)rdr.GetInt64(index);
        }

        public static float SafeGetFloat(this IDataRecord rdr, string columnName, ITable table)
        {
            int index = CheckColumnExists(rdr, columnName, table);
            return rdr.GetFloat(index);
        }

        public static float? SafeGetNullableFloat(this IDataRecord rdr, string columnName, ITable table)
        {
            int index = CheckColumnExists(rdr, columnName, table);

            bool isNull = rdr.IsDBNull(index);

            return isNull ? null : (float?)rdr.GetFloat(index);
        }

        public static double SafeGetDouble(this IDataRecord rdr, string columnName, ITable table)
        {
            int index = CheckColumnExists(rdr, columnName, table);
            return rdr.GetDouble(index);
        }

        public static double? SafeGetNullableDouble(this IDataRecord rdr, string columnName, ITable table)
        {
            int index = CheckColumnExists(rdr, columnName, table);

            bool isNull = rdr.IsDBNull(index);

            return isNull ? null : (double?)rdr.GetDouble(index);
        }

        public static DateTime SafeGetDateTime(this IDataRecord rdr, string columnName, ITable table)
        {
            int index = CheckColumnExists(rdr, columnName, table);

            return rdr.GetDateTime(index);
        }

        public static DateTime? SafeGetNullableDateTime(this IDataRecord rdr, string columnName, ITable table)
        {
            int index = CheckColumnExists(rdr, columnName, table);

            bool isNull = rdr.IsDBNull(index);

            return isNull ? null : (DateTime?)rdr.GetDateTime(index);
        }

        public static bool SafeGetBoolean(this IDataRecord rdr, string columnName, ITable table)
        {
            int index = CheckColumnExists(rdr, columnName, table);
            return rdr.GetBoolean(index);
        }

        public static bool? SafeGetNullableBoolean(this IDataRecord rdr, string columnName, ITable table)
        {
            int index = CheckColumnExists(rdr, columnName, table);

            bool isNull = rdr.IsDBNull(index);

            return isNull ? null : rdr.GetBoolean(index);
        }

        public static int CheckColumnExists(this IDataRecord rdr, string columnName, ITable table)
        {
            int index = rdr.GetOrdinal(columnName);
            if (index == -1)
            {
                var adding = "";
                if (!(table is null))
                {
                    adding = $" in {table.Name} {table.SpecifiedVersion} {table.DefaultVersion} {table.EntityClassType}";
                }
                throw new NotExistColumnException($"{columnName} ordinal is {index}" + adding);
            }

            return index;
        }

        public static bool IsDBNull(this IDataRecord rdr, string columnName)
        {
            return rdr.IsDBNull(rdr.GetOrdinal(columnName));
        }
    }
}
