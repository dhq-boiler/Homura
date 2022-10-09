

using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace Homura.ORM
{
    public interface IConnection
    {
        Guid InstanceId { get; }

        string ConnectionString { get; }

        DbConnection OpenConnection();

        Task<DbConnection> OpenConnectionAsync();

        bool TableExists(string tableName);
    }
}
