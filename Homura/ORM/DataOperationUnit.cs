

using System;
using System.Data.Common;

namespace Homura.ORM
{
    public class DataOperationUnit : IDisposable
    {
        public DbConnection CurrentConnection { get; private set; }

        public DbTransaction CurrentTransaction { get; private set; }

        public DataOperationUnit()
        { }

        public void Open(IConnection connection)
        {
            CurrentConnection = connection.OpenConnection();
        }

        public void BeginTransaction()
        {
            if (CurrentConnection == null)
            {
                throw new InvalidOperationException("Connection is not opened");
            }
            CurrentTransaction = CurrentConnection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
        }

        public void Commit()
        {
            CurrentTransaction.Commit();
        }

        public void Rollback()
        {
            CurrentTransaction.Rollback();
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
        #endregion
    }
}
