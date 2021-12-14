

using Homura.ORM.Migration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Homura.ORM
{
    public interface IDao
    {
        string TableName { get; }

        ITable Table { get; }

        IConnection CurrentConnection { get; set; }

        void VerifyTableDefinition(IDbConnection conn);

        void CreateTableIfNotExists(TimeSpan? timeout = null);

        int CountAll(IDbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null);

        int CountBy(Dictionary<string, object> idDic, IDbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null);

        void DeleteWhereIDIs(Guid id, IDbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null);

        void UpgradeTable(VersionChangeUnit upgradePath, IDbConnection conn = null, TimeSpan? timeout = null);
    }

    public interface IDao<E> : IDao where E : EntityBaseObject
    {
        void Insert(E entity, IDbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null);

        IEnumerable<E> FindAll(IDbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null);

        IEnumerable<E> FindBy(Dictionary<string, object> idDic, IDbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null);

        void Update(E entity, IDbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null);
    }
}
