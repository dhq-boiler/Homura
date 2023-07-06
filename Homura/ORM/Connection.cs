

using NLog;
using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
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
                ConnectionManager.PutAttendance(Guid.NewGuid(), new ConnectionManager.Attendance(InstanceId, connection, GetStackTrace()));
                s_logger.Trace($"Connection Opened. ConnectionString={ConnectionString}\n{GetStackTrace()}");
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
                ConnectionManager.PutAttendance(Guid.NewGuid(), new ConnectionManager.Attendance(InstanceId, connection, GetStackTrace()));
                s_logger.Trace($"Connection Opened. ConnectionString={ConnectionString}\n{GetStackTrace()}");
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
            s_logger.Trace($"Connection Disposed. ConnectionString={ConnectionString}\n{GetStackTrace()}");
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
                using (var connection = await OpenConnectionAsync().ConfigureAwait(false))
                using (var command = connection.CreateCommand())
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

        private string GetStackTrace()
        {
            var builder = new StringBuilder();
            var stackTrace = new StackTrace();
            var frames = stackTrace.GetFrames();

            if (frames != null)
            {
                foreach (var frame in frames)
                {
                    builder.Append(frame.ToString());
                }
            }
            return builder.ToString();
        }
    }
}
