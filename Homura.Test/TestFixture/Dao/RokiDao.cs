using Homura.Extensions;
using Homura.ORM;
using Homura.Test.TestFixture.Entity;
using System;
using System.Data;

namespace Homura.Test.TestFixture.Dao
{
    public class RokiDao : Dao<Roki>
    {
        public RokiDao()
            : base()
        { }

        public RokiDao(Type entityVersionType)
            : base(entityVersionType)
        { }

        protected override Roki ToEntity(IDataRecord reader)
        {
            return new Roki()
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
