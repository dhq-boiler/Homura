

using Homura.ORM;
using Homura.Test.TestFixture.Entity;
using System;
using System.Data;

namespace Homura.Test.TestFixture.Dao
{
    public class OriginDao : Dao<Origin>
    {
        public OriginDao()
            : base()
        { }

        public OriginDao(Type entityVersionType)
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
