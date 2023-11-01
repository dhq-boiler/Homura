

using Homura.ORM.Migration;
using Homura.ORM.Setup;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace Homura.ORM
{
    public interface IDao
    {
        string TableName { get; }

        ITable Table { get; }

        IConnection CurrentConnection { get; set; }

        DbConnection GetConnection();

        Task<DbConnection> GetConnectionAsync();

        void VerifyTableDefinition(DbConnection conn);

        void CreateTableIfNotExists(TimeSpan? timeout = null);

        int CountAll(DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null);

        int CountBy(Dictionary<string, object> idDic, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null);

        void DeleteWhereIDIs(Guid id, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null);

        void UpgradeTable(VersionChangeUnit upgradePath, VersioningMode mode, DbConnection conn = null, TimeSpan? timeout = null);

        void DeleteAll(DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null);

        void DropTableIfExists(TimeSpan? timeout = null);
    }

    public interface IDao<E> : IDao where E : EntityBaseObject
    {
        void Insert(E entity, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null);

        IEnumerable<E> FindAll(DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null);

        IEnumerable<E> FindBy(Dictionary<string, object> idDic, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null);

        void Update(E entity, DbConnection conn = null, string anotherDatabaseAliasName = null, TimeSpan? timeout = null);
        
        void AdjustColumns(Type versionFrom, Type versionTo, DbConnection conn = null, TimeSpan? timeout = null);
    }
}
