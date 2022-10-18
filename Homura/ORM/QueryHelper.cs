﻿using NLog;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace Homura.ORM
{
    public static class QueryHelper
    {
        public static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Actionを成功するかタイムアウトするまで試行し続けます。
        /// </summary>
        /// <param name="body">試行し続ける対象のAction</param>
        /// <param name="timeout">タイムアウト</param>
        public static void KeepTryingUntilProcessSucceed(Action body, TimeSpan? timeout = null)
        {
            if (timeout == null)
            {
                timeout = TimeSpan.FromMinutes(5);
            }

            var beginTime = DateTime.Now;

            while ((DateTime.Now - beginTime) <= timeout)
            {
                try
                {
                    LogManager.GetCurrentClassLogger().Trace("try body()");
                    body();
                    return;
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("database is lock"))
                    {
                        LogManager.GetCurrentClassLogger().Warn("database is lock");
                        continue;
                    }
                    else
                    {
                        LogManager.GetCurrentClassLogger().Error(ex);
                        throw;
                    }
                }
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Funcを成功するかタイムアウトするまで試行し続けます。
        /// </summary>
        /// <param name="body">試行し続ける対象のFunc</param>
        /// <param name="timeout">タイムアウト</param>
        public static T KeepTryingUntilProcessSucceed<T>(Func<T> body, TimeSpan? timeout = null)
        {
            if (timeout == null)
            {
                timeout = TimeSpan.FromMinutes(5);
            }

            var beginTime = DateTime.Now;

            while ((DateTime.Now - beginTime) <= timeout)
            {
                try
                {
                    LogManager.GetCurrentClassLogger().Trace("try body()");
                    return body();
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("database is lock"))
                    {
                        LogManager.GetCurrentClassLogger().Warn("database is lock");
                        continue;
                    }
                    else
                    {
                        LogManager.GetCurrentClassLogger().Error(ex);
                        throw;
                    }
                }
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Funcを成功するかタイムアウトするまで非同期で試行し続けます。
        /// </summary>
        /// <param name="body">試行し続ける対象のFunc</param>
        /// <param name="timeout">タイムアウト</param>
        public static async Task KeepTryingUntilProcessSucceedAsync<T>(Func<Task> body, TimeSpan? timeout = null)
        {
            if (timeout == null)
            {
                timeout = TimeSpan.FromMinutes(5);
            }

            var beginTime = DateTime.Now;

            while ((DateTime.Now - beginTime) <= timeout)
            {
                try
                {
                    LogManager.GetCurrentClassLogger().Trace("try body()");
                    await body().ConfigureAwait(false);
                    return;
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("database is lock"))
                    {
                        LogManager.GetCurrentClassLogger().Warn("database is lock");
                        continue;
                    }
                    else
                    {
                        LogManager.GetCurrentClassLogger().Error(ex);
                        throw;
                    }
                }
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Funcを成功するかタイムアウトするまで試行し続けます。
        /// </summary>
        /// <param name="body">試行し続ける対象のFunc</param>
        /// <param name="timeout">タイムアウト</param>
        public static T KeepTryingUntilProcessSucceedAndReturn<T>(Func<T> body, TimeSpan? timeout = null)
        {
            if (timeout == null)
            {
                timeout = TimeSpan.FromMinutes(5);
            }

            var beginTime = DateTime.Now;

            while ((DateTime.Now - beginTime) <= timeout)
            {
                try
                {
                    LogManager.GetCurrentClassLogger().Trace("try body()");
                    return body();
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("database is lock"))
                    {
                        LogManager.GetCurrentClassLogger().Warn("database is lock");
                        continue;
                    }
                    else
                    {
                        LogManager.GetCurrentClassLogger().Error(ex);
                        throw;
                    }
                }
            }

            throw new TimeoutException();
        }

        /// <summary>
        /// Funcを成功するかタイムアウトするまで非同期で試行し続けます。
        /// </summary>
        /// <param name="body">試行し続ける対象のFunc</param>
        /// <param name="timeout">タイムアウト</param>
        public static async Task<T> KeepTryingUntilProcessSucceedAndReturnAsync<T>(Func<Task<T>> body, TimeSpan? timeout = null)
        {
            if (timeout == null)
            {
                timeout = TimeSpan.FromMinutes(5);
            }

            var beginTime = DateTime.Now;

            while ((DateTime.Now - beginTime) <= timeout)
            {
                try
                {
                    LogManager.GetCurrentClassLogger().Trace("try body()");
                    return await body().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("database is lock"))
                    {
                        LogManager.GetCurrentClassLogger().Warn("database is lock");
                        continue;
                    }
                    else
                    {
                        LogManager.GetCurrentClassLogger().Error(ex);
                        throw;
                    }
                }
            }

            throw new TimeoutException();
        }

        public static class ForDao
        {
            /// <summary>
            /// _IsTransaction フラグによって局所的に DbConnection を使用するかどうか選択できるクエリ実行用内部メソッド
            /// </summary>
            /// <typeparam name="R"></typeparam>
            /// <param name="body"></param>
            /// <returns></returns>
            public static R ConnectionInternalAndReturn<R>(IDao dao, Func<DbConnection, R> body, DbConnection conn = null)
            {
                if (dao is null)
                {
                    throw new ArgumentNullException(nameof(dao));
                }

                if (body is null)
                {
                    throw new ArgumentNullException(nameof(body));
                }

                bool isTransaction = conn != null;

                try
                {
                    if (!isTransaction)
                    {
                        conn = dao.GetConnection();
                    }

                    return body(conn);
                }
                catch (ObjectDisposedException)
                {
                    using (var tempConn = dao.GetConnection())
                    {
                        return body(tempConn);
                    }
                }
                finally
                {
                    if (!isTransaction)
                    {
                        conn.Dispose();
                    }
                }
            }

            /// <summary>
            /// _IsTransaction フラグによって局所的に DbConnection を使用するかどうか選択できるクエリ実行用内部メソッド
            /// </summary>
            /// <typeparam name="R"></typeparam>
            /// <param name="body"></param>
            /// <returns></returns>
            public static async Task<R> ConnectionInternalAndReturnAsync<R>(IDao dao, Func<DbConnection, R> body, DbConnection conn = null)
            {
                if (dao is null)
                {
                    throw new ArgumentNullException(nameof(dao));
                }

                if (body is null)
                {
                    throw new ArgumentNullException(nameof(body));
                }

                bool isTransaction = conn != null;

                try
                {
                    if (!isTransaction)
                    {
                        conn = await dao.GetConnectionAsync().ConfigureAwait(false);
                    }

                    return await Task.Run(() => body(conn)).ConfigureAwait(false);
                }
                catch (ObjectDisposedException)
                {
                    using (var tempConn = await dao.GetConnectionAsync().ConfigureAwait(false))
                    {
                        return await Task.Run(() => body(tempConn)).ConfigureAwait(false);
                    }
                }
                finally
                {
                    if (!isTransaction)
                    {
                        await conn.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }

            public static IEnumerable<R> ConnectionInternalYield<R>(IDao dao, Func<DbConnection, IEnumerable<R>> body, DbConnection conn = null)
            {
                if (dao is null)
                {
                    throw new ArgumentNullException(nameof(dao));
                }

                if (body is null)
                {
                    throw new ArgumentNullException(nameof(body));
                }

                bool isTransaction = conn != null;

                try
                {
                    if (!isTransaction)
                    {
                        conn = dao.GetConnection();
                    }

                    return body(conn);
                }
                catch (ObjectDisposedException)
                {
                    using (var tempConn = dao.GetConnection())
                    {
                        return body(tempConn);
                    }
                }
                finally
                {
                    if (!isTransaction)
                    {
                        conn.Dispose();
                    }
                }
            }

            public static async IAsyncEnumerable<R> ConnectionInternalYieldAsync<R>(IDao dao, Func<DbConnection, IAsyncEnumerable<R>> body, DbConnection conn = null)
            {
                if (dao is null)
                {
                    throw new ArgumentNullException(nameof(dao));
                }

                if (body is null)
                {
                    throw new ArgumentNullException(nameof(body));
                }

                IAsyncEnumerator<R> enumerator = null;
                DbConnection tempConn = null;

                try
                {
                    enumerator = body(conn).GetAsyncEnumerator();
                }
                catch (ObjectDisposedException)
                {
                    tempConn = await dao.GetConnectionAsync().ConfigureAwait(false);
                    enumerator = body(tempConn).GetAsyncEnumerator();
                }

                while (true)
                {
                    R ret = default(R);
                    try
                    {
                        if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                        {
                            break;
                        }
                        ret = enumerator.Current;
                    }
                    catch (Exception)
                    {
                        break;
                    }
                    yield return ret;
                }

                if (tempConn is not null)
                {
                    await tempConn.DisposeAsync().ConfigureAwait(false);
                }
            }

            /// <summary>
            /// _IsTransaction フラグによって局所的に DbConnection を使用するかどうか選択できるクエリ実行用内部メソッド
            /// </summary>
            /// <param name="body"></param>
            public static void ConnectionInternal(IDao dao, Action<DbConnection> body, DbConnection conn = null)
            {
                if (dao is null)
                {
                    throw new ArgumentNullException(nameof(dao));
                }

                if (body is null)
                {
                    throw new ArgumentNullException(nameof(body));
                }

                bool isTransaction = conn != null;

                try
                {
                    if (!isTransaction)
                    {
                        conn = dao.GetConnection();
                    }

                    body(conn);
                }
                catch (ObjectDisposedException)
                {
                    ConnectionInternal(dao, body, dao.GetConnection());
                }
                finally
                {
                    if (!isTransaction)
                    {
                        conn.Dispose();
                    }
                }
            }

            /// <summary>
            /// _IsTransaction フラグによって局所的に DbConnection を使用するかどうか選択できるクエリ実行用内部メソッド
            /// </summary>
            /// <param name="body"></param>
            public static async Task ConnectionInternalAsync(IDao dao, Action<DbConnection> body, DbConnection conn = null)
            {
                if (dao is null)
                {
                    throw new ArgumentNullException(nameof(dao));
                }

                if (body is null)
                {
                    throw new ArgumentNullException(nameof(body));
                }

                bool isTransaction = conn != null;

                try
                {
                    if (!isTransaction)
                    {
                        conn = await dao.GetConnectionAsync().ConfigureAwait(false);
                    }

                    await Task.Run(() => body(conn));
                }
                catch(ObjectDisposedException)
                {
                    await ConnectionInternalAsync(dao, body, await dao.GetConnectionAsync()).ConfigureAwait(false);
                }
                finally
                {
                    if (!isTransaction)
                    {
                        await conn.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
