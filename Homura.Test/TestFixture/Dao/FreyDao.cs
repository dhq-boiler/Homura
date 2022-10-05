using Homura.Extensions;
using Homura.ORM;
using Homura.Test.TestFixture.Entity;
using System;
using System.Data;

namespace Homura.Test.TestFixture.Dao
{
    public class FreyDao : Dao<Frey>
    {
        public FreyDao()
            : base()
        { }

        public FreyDao(Type entityVersionType)
            : base(entityVersionType)
        { }

        protected override Frey ToEntity(IDataRecord reader)
        {
            return new Frey()
            {
                Id = reader.SafeGetGuid("ID", Table),
                Item1 = reader.SafeGetString("Item1", Table),
                Item2 = reader.SafeGetString("Item2", Table),
                Item3 = CatchThrow(() => reader.SafeGetString("Item3", Table)),
                Item4 = CatchThrow(() => reader.SafeGetString("Item4", Table)),
            };
        }
    }
}
