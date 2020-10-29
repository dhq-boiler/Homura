using Homura.QueryBuilder.Iso.Dml.Syntaxes;
using System;

namespace Homura.QueryBuilder.Iso.Dml
{
    public class Insert : IInsertSyntax, IDisposable
    {
        internal InsertSyntax _syntax = new InsertSyntax();

        public Insert()
            : base()
        { }

        #region IDisposable Support
        private bool disposedValue = false;

        public IIntoSyntax Into { get { return _syntax.Into; } }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _syntax.Dispose();
                    _syntax = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
