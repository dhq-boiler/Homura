

using Homura.ORM;
using Homura.Test.TestFixture.Entity;
using System;
using System.Data;

namespace Homura.Test.TestFixture.Dao
{
    public class DetailDao : Dao<Detail>
    {
        public DetailDao()
            : base()
        { }

        public DetailDao(Type entityVersionType)
            : base(entityVersionType)
        { }

        private static Guid SafeGetGuid(IDataRecord rdr, string columnName)
        {
            var index = rdr.GetOrdinal(columnName);
            var isNull = rdr.IsDBNull(index);

            return isNull ? Guid.Empty : rdr.GetGuid(index);
        }

        private static string SafeGetString(IDataRecord rdr, string columnName)
        {
            var index = rdr.GetOrdinal(columnName);
            var isNull = rdr.IsDBNull(index);

            return isNull ? null : rdr.GetString(index);
        }
    }
}
