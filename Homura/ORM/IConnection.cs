

using System.Data.Common;

namespace Homura.ORM
{
    public interface IConnection
    {
        string ConnectionString { get; }

        DbConnection OpenConnection();

        bool TableExists(string tableName);
    }
}
