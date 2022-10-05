﻿

using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Entity;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class BetaChangePlan_Abstract<V> : ChangePlan<Beta, V> where V : VersionOrigin
    {
        public override void CreateTable(IConnection connection)
        {
            var dao = new BetaDao(TargetVersion.GetType());
            dao.CurrentConnection = connection;
            dao.CreateTableIfNotExists();
            ++ModifiedCount;
        }
    }
}
