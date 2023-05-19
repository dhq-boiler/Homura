using Homura.ORM;
using Homura.Test.TestFixture.Entity;
using System;

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
    }
}
