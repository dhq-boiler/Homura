

using Homura.ORM.Migration;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Homura.ORM
{
    public interface IDao
    {
        string TableName { get; }

        ITable Table { get; }

        IConnection CurrentConnection { get; set; }

        void VerifyTableDefinition(DbConnection conn);

        void CreateTableIfNotExists();

        int CountAll(DbConnection conn = null, string anotherDatabaseAliasName = null);

        int CountBy(Dictionary<string, object> idDic, DbConnection conn = null, string anotherDatabaseAliasName = null);

        void DeleteWhereIDIs(Guid id, DbConnection conn = null, string anotherDatabaseAliasName = null);

        void UpgradeTable(VersionChangeUnit upgradePath, DbConnection conn = null);
    }

    public interface IDao<E> : IDao where E : EntityBaseObject
    {
        void Insert(E entity, DbConnection conn = null, string anotherDatabaseAliasName = null);

        IEnumerable<E> FindAll(DbConnection conn = null, string anotherDatabaseAliasName = null);

        IEnumerable<E> FindBy(Dictionary<string, object> idDic, DbConnection conn = null, string anotherDatabaseAliasName = null);

        void Update(E entity, DbConnection conn = null, string anotherDatabaseAliasName = null);
    }
}
