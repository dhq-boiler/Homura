using Homura.Extensions;
using Homura.ORM;
using Homura.Test.TestFixture.Entity;
using System;
using System.Data;

namespace Homura.Test.TestFixture.Dao
{
    public class Valkyrie_0_Dao : Dao<Valkyrie_0>
    {
        public Valkyrie_0_Dao()
            : base()
        { }

        public Valkyrie_0_Dao(Type entityVersionType)
            : base(entityVersionType)
        { }
        protected override Valkyrie_0 ToEntity(IDataRecord reader)
        {
            return new Valkyrie_0()
            {
                Id = reader.SafeGetGuid("ID", Table),
                Item1 = reader.SafeGetString("Item1", Table),
                Item2 = reader.SafeGetString("Item2", Table),
                Item3 = CatchThrow(() => reader.SafeGetString("Item3", Table)),
                Item4 = CatchThrow(() => reader.SafeGetString("Item4", Table)),
            };
        }
    }

    public class Valkyrie_1_Dao : Dao<Valkyrie_1>
    {
        public Valkyrie_1_Dao()
            : base()
        { }

        public Valkyrie_1_Dao(Type entityVersionType)
            : base(entityVersionType)
        { }
        protected override Valkyrie_1 ToEntity(IDataRecord reader)
        {
            return new Valkyrie_1()
            {
                Id = reader.SafeGetGuid("ID", Table),
                Item1 = reader.SafeGetString("Item1", Table),
                Item2 = reader.SafeGetString("Item2", Table),
                Item3 = CatchThrow(() => reader.SafeGetString("Item3", Table)),
                Item4 = CatchThrow(() => reader.SafeGetString("Item4", Table)),
            };
        }
    }

    public class Valkyrie_2_Dao : Dao<Valkyrie_2>
    {
        public Valkyrie_2_Dao()
            : base()
        { }

        public Valkyrie_2_Dao(Type entityVersionType)
            : base(entityVersionType)
        { }
        protected override Valkyrie_2 ToEntity(IDataRecord reader)
        {
            return new Valkyrie_2()
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
