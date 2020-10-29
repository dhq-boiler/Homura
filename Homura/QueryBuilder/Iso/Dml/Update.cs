using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Syntaxes;
using System;

namespace Homura.QueryBuilder.Iso.Dml
{
    public class Update : IUpdateSyntax, IDisposable
    {
        internal UpdateSyntax _syntax = new UpdateSyntax();

        public Update()
        { }

        public IUpdateTableSyntax Table(string name)
        {
            return _syntax.Table(name);
        }

        public IUpdateTableSyntax Table(string name, string alias)
        {
            return _syntax.Table(name, alias);
        }

        public IUpdateTableSyntax Table(string schemaName, string name, string alias)
        {
            return _syntax.Table(schemaName, name, alias);
        }

        public IUpdateTableSyntax Table(string catalogName, string schemaName, string name, string alias)
        {
            return _syntax.Table(catalogName, schemaName, name, alias);
        }

        public IUpdateTableSyntax Table(Table table)
        {
            return _syntax.Table(table);
        }

        #region IDisposable Support
        private bool disposedValue = false;

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
