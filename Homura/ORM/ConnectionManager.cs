
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Homura.ORM
{
    public static class ConnectionManager
    {
        private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();
        public static IConnection DefaultConnection { get; set; }

        public static void SetDefaultConnection(Guid instanceId, string v, Type dbConnectionType)
        {
            DefaultConnection = new Connection(instanceId, v, dbConnectionType);
        }

        private static ConcurrentDictionary<Guid, Attendance> Attendances = new ConcurrentDictionary<Guid, Attendance>();

        /// <summary>
        /// 接続中またはトランザクション中のオブジェクトを登録します。
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="attendance"></param>
        internal static void PutAttendance(Guid guid, Attendance attendance)
        {
            Attendances.TryAdd(guid, attendance);
        }

        /// <summary>
        /// 接続中のDbConnectionオブジェクトを抹消します。
        /// Dispose()はされませんのでご注意ください。
        /// </summary>
        /// <param name="connection"></param>
        internal static void RemoveAttendance(DbConnection connection)
        {
            var remove = Attendances.SingleOrDefault(x => x.Value.Connection is not null && x.Value.Connection.Equals(connection));
            if (remove.Key != Guid.Empty)
            {
                Attendances.TryRemove(remove);
            }
        }

        /// <summary>
        /// トランザクション中のDbTransactionオブジェクトを抹消します。
        /// Dispose()はされませんのでご注意ください。
        /// </summary>
        /// <param name="transaction"></param>
        internal static void RemoveAttendance(DbTransaction transaction)
        {
            var remove = Attendances.SingleOrDefault(x => x.Value.Transaction is not null && x.Value.Transaction.Equals(transaction));
            if (remove.Key != Guid.Empty)
            {
                Attendances.TryRemove(remove);
            }
        }

        /// <summary>
        /// すべての接続とすべてのトランザクションを廃棄します。
        /// </summary>
        public static void DisposeAllDebris()
        {
            Attendances.ToList().ForEach(x =>
            {
                if (x.Value.Transaction is not null)
                {
                    x.Value.Transaction.Dispose();
                    s_logger.Debug($"Dispose Transaction Guid={x.Key}");
                }
                if (x.Value.Connection is not null)
                {
                    x.Value.Connection.Dispose();
                    s_logger.Debug($"Dispose Connection Guid={x.Key}");
                }
            });
            Attendances.Clear();
        }

        /// <summary>
        /// すべての接続とすべてのトランザクションを廃棄します。
        /// </summary>
        public static void DisposeDebris(Guid instanceId)
        {
            var list = Attendances.Where(x => x.Value.InstanceId.Equals(instanceId)).ToList();

            list.ForEach(x =>
            {
                if (x.Value.Transaction is not null)
                {
                    x.Value.Transaction.Dispose();
                    s_logger.Debug($"Dispose Transaction Guid={x.Key}");
                }
                if (x.Value.Connection is not null)
                {
                    x.Value.Connection.Dispose();
                    s_logger.Debug($"Dispose Connection Guid={x.Key}");
                }
                Attendances.TryRemove(x);
            });
        }

        /// <summary>
        /// すべての接続とすべてのトランザクションのスタックトレースを取得します。
        /// </summary>
        /// <returns>スタックトレースのコレクション</returns>
        public static IEnumerable<string> EnumerateAllDebris()
        {
            return Attendances.Select(x => x.Value.StackTrace);
        }

        public class Attendance
        {
            public Attendance(Guid instanceId, DbConnection connection, string stackTrace)
            {
                InstanceId = instanceId;
                Connection = connection;
                StackTrace = stackTrace;
            }

            public Attendance(Guid instanceId, DbTransaction transaction, string stackTrace)
            {
                InstanceId = instanceId;
                Transaction = transaction;
                StackTrace = stackTrace;
            }

            public Guid InstanceId { get; }
            public DbConnection Connection { get; }
            public DbTransaction Transaction { get; }
            public string StackTrace { get; }
        }
    }
}
