

using NLog;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Homura.ORM
{
    public class Connection : IConnection
    {
        private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

        public Guid InstanceId { get; }
        public string ConnectionString { get; private set; }

        private DbSelector _selector;

        public Connection(Guid instanceId, string connectionString, Type dbConnectionType)
        {
            InstanceId = instanceId;
            ConnectionString = connectionString;
            _selector = new DbSelector(dbConnectionType);
        }

        public DbConnection OpenConnection()
        {
            var connection = _selector.CreateConnection();
            connection.ConnectionString = ConnectionString;
            try
            {
                connection.Open();
                ConnectionManager.PutAttendance(Guid.NewGuid(), new ConnectionManager.Attendance(InstanceId, connection, Environment.StackTrace));
                s_logger.Debug($"Connection Opened. ConnectionString={ConnectionString}\n{Environment.StackTrace}");
            }
            catch (ArgumentException e)
            {
                Console.Error.WriteLine(e.ToString());
                throw new FailedOpeningDatabaseException("", e);
            }
            connection.Disposed += Connection_Disposed;
            return connection;
        }

        public async Task<DbConnection> OpenConnectionAsync()
        {
            var connection = _selector.CreateConnection();
            connection.ConnectionString = ConnectionString;
            try
            {
                await connection.OpenAsync().ConfigureAwait(false);
                ConnectionManager.PutAttendance(Guid.NewGuid(), new ConnectionManager.Attendance(InstanceId, connection, Environment.StackTrace));
                s_logger.Debug($"Connection Opened. ConnectionString={ConnectionString}\n{Environment.StackTrace}");
            }
            catch (ArgumentException e)
            {
                Console.Error.WriteLine(e.ToString());
                throw new FailedOpeningDatabaseException("", e);
            }
            connection.Disposed += Connection_Disposed;
            return connection;
        }

        private void Connection_Disposed(object sender, EventArgs e)
        {
            ConnectionManager.RemoveAttendance(sender as DbConnection);
            s_logger.Debug($"Connection Disposed. ConnectionString={ConnectionString}\n{Environment.StackTrace}");
        }

        public bool TableExists(string tableName)
        {
            try
            {
                using (var conn = OpenConnection())
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = "select count(*) from sqlite_master where type='table' and name=@tablename;";
                    command.CommandType = CommandType.Text;
                    command.SetParameter(new PlaceholderRightValue("@tablename", tableName));

                    using (var reader = command.ExecuteReader())
                    {
                        reader.Read();
                        int count = reader.GetInt32(0);
                        return count == 1;
                    }
                }
            }
            catch (FailedOpeningDatabaseException)
            {
                throw;
            }
        }

        public async Task<bool> TableExistsAsync(string tableName)
        {
            try
            {
                using (var conn = await OpenConnectionAsync().ConfigureAwait(false))
                using (var command = (SqlCommand)conn.CreateCommand())
                {
                    command.CommandText = "select count(*) from sqlite_master where type='table' and name=@tablename;";
                    command.CommandType = CommandType.Text;
                    command.SetParameter(new PlaceholderRightValue("@tablename", tableName));

                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        await reader.ReadAsync().ConfigureAwait(false);
                        int count = reader.GetInt32(0);
                        return count == 1;
                    }
                }
            }
            catch (FailedOpeningDatabaseException)
            {
                throw;
            }
        }
    }
}
