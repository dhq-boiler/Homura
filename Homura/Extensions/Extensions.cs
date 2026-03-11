using Homura.ORM;
using System;
using System.Data;

namespace Homura.Extensions
{
    public static class Extensions
    {
        public static Guid SafeGetGuid(this IDataRecord rdr, string columnName, ITable table)
        {
            if (!rdr.TryGetColumnIndex(columnName, out int index))
                return Guid.Empty;

            bool isNull = rdr.IsDBNull(index);

            return isNull ? Guid.Empty : rdr.GetGuid(index);
        }

        public static Guid? SafeGetNullableGuid(this IDataRecord rdr, string columnName, ITable table)
        {
            if (!rdr.TryGetColumnIndex(columnName, out int index))
                return null;

            bool isNull = rdr.IsDBNull(index);

            return isNull ? null : rdr.GetGuid(index);
        }

        public static char? SafeGetNullableChar(this IDataRecord rdr, string columnName, ITable table)
        {
            if (!rdr.TryGetColumnIndex(columnName, out int index))
                return null;

            bool isNull = rdr.IsDBNull(index);

            return isNull ? null : rdr.GetChar(index);
        }

        public static char SafeGetChar(this IDataRecord rdr, string columnName, ITable table)
        {
            if (!rdr.TryGetColumnIndex(columnName, out int index))
                return char.MinValue;

            bool isNull = rdr.IsDBNull(index);

            return isNull ? char.MinValue : rdr.GetChar(index);
        }

        public static string SafeGetString(this IDataRecord rdr, string columnName, ITable table)
        {
            if (!rdr.TryGetColumnIndex(columnName, out int index))
                return null;

            bool isNull = rdr.IsDBNull(index);

            return isNull ? null : rdr.GetString(index);
        }

        public static int SafeGetInt(this IDataRecord rdr, string columnName, ITable table)
        {
            if (!rdr.TryGetColumnIndex(columnName, out int index))
                return int.MinValue;

            bool isNull = rdr.IsDBNull(index);

            return isNull ? int.MinValue : rdr.GetInt32(index);
        }

        public static int? SafeGetNullableInt(this IDataRecord rdr, string columnName, ITable table)
        {
            if (!rdr.TryGetColumnIndex(columnName, out int index))
                return null;

            bool isNull = rdr.IsDBNull(index);

            return isNull ? null : (int?)rdr.GetInt32(index);
        }

        public static long SafeGetLong(this IDataRecord rdr, string columnName, ITable table)
        {
            if (!rdr.TryGetColumnIndex(columnName, out int index))
                return 0L;

            return rdr.GetInt64(index);
        }

        public static long? SafeNullableGetLong(this IDataRecord rdr, string columnName, ITable table)
        {
            if (!rdr.TryGetColumnIndex(columnName, out int index))
                return null;

            bool isNull = rdr.IsDBNull(index);

            return isNull ? null : (long?)rdr.GetInt64(index);
        }

        public static float SafeGetFloat(this IDataRecord rdr, string columnName, ITable table)
        {
            if (!rdr.TryGetColumnIndex(columnName, out int index))
                return 0f;

            return rdr.GetFloat(index);
        }

        public static float? SafeGetNullableFloat(this IDataRecord rdr, string columnName, ITable table)
        {
            if (!rdr.TryGetColumnIndex(columnName, out int index))
                return null;

            bool isNull = rdr.IsDBNull(index);

            return isNull ? null : (float?)rdr.GetFloat(index);
        }

        public static double SafeGetDouble(this IDataRecord rdr, string columnName, ITable table)
        {
            if (!rdr.TryGetColumnIndex(columnName, out int index))
                return 0d;

            return rdr.GetDouble(index);
        }

        public static double? SafeGetNullableDouble(this IDataRecord rdr, string columnName, ITable table)
        {
            if (!rdr.TryGetColumnIndex(columnName, out int index))
                return null;

            bool isNull = rdr.IsDBNull(index);

            return isNull ? null : (double?)rdr.GetDouble(index);
        }

        public static DateTime SafeGetDateTime(this IDataRecord rdr, string columnName, ITable table)
        {
            if (!rdr.TryGetColumnIndex(columnName, out int index))
                return default;

            return rdr.GetDateTime(index);
        }

        public static DateTime? SafeGetNullableDateTime(this IDataRecord rdr, string columnName, ITable table)
        {
            if (!rdr.TryGetColumnIndex(columnName, out int index))
                return null;

            bool isNull = rdr.IsDBNull(index);

            return isNull ? null : (DateTime?)rdr.GetDateTime(index);
        }

        public static bool SafeGetBoolean(this IDataRecord rdr, string columnName, ITable table)
        {
            if (!rdr.TryGetColumnIndex(columnName, out int index))
                return false;

            return rdr.GetBoolean(index);
        }

        public static bool? SafeGetNullableBoolean(this IDataRecord rdr, string columnName, ITable table)
        {
            if (!rdr.TryGetColumnIndex(columnName, out int index))
                return null;

            bool isNull = rdr.IsDBNull(index);

            return isNull ? null : rdr.GetBoolean(index);
        }

        public static Type SafeGetType(this IDataRecord rdr, string columnName, ITable table)
        {
            if (!rdr.TryGetColumnIndex(columnName, out int index))
                return null;

            bool isNull = rdr.IsDBNull(index);

            var FQDN = rdr.GetString(index);

            return isNull ? null : Type.GetType(FQDN);
        }

        public static object SafeGetObject(this IDataRecord rdr, string columnName, ITable table)
        {
            if (!rdr.TryGetColumnIndex(columnName, out int index))
                return null;

            bool isNull = rdr.IsDBNull(index);

            return isNull ? null : rdr.GetValue(index);
        }

        /// <summary>
        /// カラムのインデックスを取得する。カラムが存在しない場合は false を返す。
        /// </summary>
        public static bool TryGetColumnIndex(this IDataRecord rdr, string columnName, out int index)
        {
            try
            {
                index = rdr.GetOrdinal(columnName);
                return index != -1;
            }
            catch (IndexOutOfRangeException)
            {
                index = -1;
                return false;
            }
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
