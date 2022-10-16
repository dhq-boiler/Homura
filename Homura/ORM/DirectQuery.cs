using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace Homura.ORM
{
    public static class DirectQuery
    {
        public static void RunQuery(DbConnection conn, Action<DbConnection> body)
        {
            QueryHelper.KeepTryingUntilProcessSucceed(() => body(conn));
        }

        public static R RunQuery<R>(DbConnection conn, Func<DbConnection, R> body)
        {
            return QueryHelper.KeepTryingUntilProcessSucceedAndReturn(() => body(conn));
        }

        public static async Task RunQueryAsync(DbConnection conn, Func<DbConnection, Task> body)
        {
            await QueryHelper.KeepTryingUntilProcessSucceedAsync<Task>(async () => await body(conn));
        }

        public static async Task<R> RunQueryAsync<R>(DbConnection conn, Func<DbConnection, Task<R>> body)
        {
            return await QueryHelper.KeepTryingUntilProcessSucceedAndReturnAsync(async () => await body(conn));
        }
    }
}
