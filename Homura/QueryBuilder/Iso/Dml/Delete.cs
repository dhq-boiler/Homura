using Homura.QueryBuilder.Iso.Dml.Syntaxes;
using Homura.QueryBuilder.Iso.Dml.Transitions;
using System;

namespace Homura.QueryBuilder.Iso.Dml
{
    public class Delete : IDeleteSyntax, IDisposable
    {
        internal DeleteSyntax _syntax = new DeleteSyntax();

        public Delete()
        { }

        #region IDisposable Support
        private bool disposedValue = false;

        public ITableTransition<IDeleteTableSyntax<ISinkStateSyntax>> From { get { return _syntax.From; } }

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
