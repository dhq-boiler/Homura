

using System;
using System.Data.Common;

namespace Homura.ORM
{
    public interface IConnection
    {
        Guid InstanceId { get; }

        string ConnectionString { get; }

        DbConnection OpenConnection();

        bool TableExists(string tableName);
    }
}
