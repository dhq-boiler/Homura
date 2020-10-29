using Homura.QueryBuilder.Vendor.SQLite.Dcl.Syntaxes;

namespace Homura.QueryBuilder.Vendor.SQLite.Dcl
{
    public class Vacuum : IVacuumSyntax
    {
        private VacuumSyntax syntax = new VacuumSyntax();

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    syntax.Dispose();
                    syntax = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public string ToSql()
        {
            return syntax.ToSql();
        }
        #endregion
    }
}
