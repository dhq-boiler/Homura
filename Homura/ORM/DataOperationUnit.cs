﻿

using NLog;
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Homura.ORM
{
    public class DataOperationUnit : IDisposable, IAsyncDisposable
    {
        private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();
        public DbConnection CurrentConnection { get; private set; }

        public DbTransaction CurrentTransaction { get; private set; }

        private Guid CurrentTransactionId { get; set; }

        public virtual Guid InstanceId { get; set; } 

        public DataOperationUnit()
        { }

        public DataOperationUnit(Guid instanceId)
        {
            InstanceId = instanceId;
        }

        public void Open(IConnection connection)
        {
            CurrentConnection = connection.OpenConnection();
        }

        public async Task OpenAsync(IConnection connection)
        {
            CurrentConnection = await connection.OpenConnectionAsync().ConfigureAwait(false);
        }

        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (CurrentConnection == null)
            {
                throw new InvalidOperationException("Connection is not opened");
            }
            CurrentTransactionId = Guid.NewGuid();
            s_logger.Debug($"BeginTransaction Id={CurrentTransactionId} \n{Environment.StackTrace}");
            CurrentTransaction = CurrentConnection.BeginTransaction(isolationLevel);
            ConnectionManager.PutAttendance(Guid.NewGuid(), new ConnectionManager.Attendance(InstanceId, CurrentTransaction, Environment.StackTrace));
        }

        public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (CurrentConnection == null)
            {
                throw new InvalidOperationException("Connection is not opened");
            }
            CurrentTransactionId = Guid.NewGuid();
            s_logger.Debug($"BeginTransaction Id={CurrentTransactionId} \n{Environment.StackTrace}");
            CurrentTransaction = await CurrentConnection.BeginTransactionAsync(isolationLevel).ConfigureAwait(false);
            ConnectionManager.PutAttendance(Guid.NewGuid(), new ConnectionManager.Attendance(InstanceId, CurrentTransaction, Environment.StackTrace));
        }

        public void Commit()
        {
            CurrentTransaction.Commit();
            s_logger.Debug($"Commit Id={CurrentTransactionId}");
            ConnectionManager.RemoveAttendance(CurrentTransaction);
            CurrentTransaction.Dispose();
            CurrentTransaction = null;
        }

        public async Task CommitAsync()
        {
            await CurrentTransaction.CommitAsync().ConfigureAwait(false);
            s_logger.Debug($"Commit Id={CurrentTransactionId}");
            ConnectionManager.RemoveAttendance(CurrentTransaction);
            CurrentTransaction.Dispose();
            CurrentTransaction = null;
        }

        public void Rollback()
        {
            CurrentTransaction.Rollback();
            s_logger.Debug($"Rollback Id={CurrentTransactionId}");
            ConnectionManager.RemoveAttendance(CurrentTransaction);
            CurrentTransaction.Dispose();
            CurrentTransaction = null;
        }

        public async Task RollbackAsync()
        {
            await CurrentTransaction.RollbackAsync().ConfigureAwait(false);
            s_logger.Debug($"Rollback Id={CurrentTransactionId}");
            ConnectionManager.RemoveAttendance(CurrentTransaction);
            CurrentTransaction.Dispose();
            CurrentTransaction = null;
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (CurrentTransaction != null)
                    {
                        CurrentTransaction.Dispose();
                    }
                    if (CurrentConnection != null)
                    {
                        CurrentConnection.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }
        protected virtual async Task DisposeAsync(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (CurrentTransaction != null)
                    {
                        await CurrentTransaction.DisposeAsync().ConfigureAwait(false);
                    }
                    if (CurrentConnection != null)
                    {
                        await CurrentConnection.DisposeAsync().ConfigureAwait(false);
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~DataOperationUnit()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(true).ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
