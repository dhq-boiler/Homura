

using Homura.ORM;
using Homura.Test.TestFixture.Entity;
using System;
using System.Data;
using static Homura.Extensions.Extensions;

namespace Homura.Test.TestFixture.Dao
{
    internal class AlphaDao : Dao<Alpha>
    {
        public AlphaDao()
            : base()
        { }

        public AlphaDao(Type entityVersionType)
            : base(entityVersionType)
        { }

        protected override Alpha ToEntity(IDataRecord reader)
        {
            return new Alpha()
            {
                Id = CatchThrow(() => reader.SafeGetGuid("Id", Table)),
                Item1 = CatchThrow(() => reader.SafeGetString("Item1", Table)),
                Item2 = CatchThrow(() => reader.SafeGetString("Item2", Table)),
                Item3 = CatchThrow(() => reader.SafeGetGuid("Item3", Table)),
                Item4 = CatchThrow(() => reader.SafeGetString("Item4", Table)),
                Item5 = CatchThrow(() => reader.SafeGetInt("Item5", Table)),
                Item6 = CatchThrow(() => reader.SafeGetLong("Item6", Table)),
                Item7 = CatchThrow(() => reader.SafeGetString("Item7", Table)),
                Item8 = CatchThrow(() => reader.SafeGetBoolean("Item8", Table)),
                Item9 = CatchThrow(() => reader.SafeGetBoolean("Item9", Table)),
            };
        }
    }
}
