using Homura.ORM;
using Homura.ORM.Setup;
using Homura.Test.TestFixture.Entity;
using System;

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

        public RokiDao(DataVersionManager dataVersionManager) : base(dataVersionManager)
        {
        }
    }
}
