

using Homura.ORM;
using Homura.Test.TestFixture.Entity;
using System;
using System.Data;

namespace Homura.Test.TestFixture.Dao
{
    internal class BetaDao : Dao<Beta>
    {
        public BetaDao()
            : base()
        { }

        public BetaDao(Type entityVersionType)
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
