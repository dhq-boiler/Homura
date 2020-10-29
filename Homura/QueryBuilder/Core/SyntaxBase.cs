using Homura.QueryBuilder.Iso.Dml;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Homura.QueryBuilder.Core
{
    public abstract class SyntaxBase : ISyntaxBase, IDisposable
    {
        internal List<SyntaxBase> Relay { get; set; }

        internal Dictionary<string, object> Parameters { get; set; }

        protected List<string> LocalParameters { get; set; }

        protected int ParameterCount { get; set; }

        protected SyntaxBase()
        {
            Relay = new List<SyntaxBase>();
            Parameters = new Dictionary<string, object>();
            LocalParameters = new List<string>();
            ParameterCount = 0;
        }

        protected SyntaxBase(SyntaxBase syntaxBase)
        {
            Relay = syntaxBase.Relay.ToList();
            Relay.Add(syntaxBase);
            Parameters = syntaxBase.Parameters;
            LocalParameters = new List<string>();
            ParameterCount = syntaxBase.ParameterCount;
        }

        protected void AddParameter(object value)
        {
            string parameter = $"@val_{ParameterCount++}";
            Parameters.Add(parameter, value);
            LocalParameters.Add(parameter);
        }

        protected void AddParameters(object[] values)
        {
            foreach (var value in values)
            {
                AddParameter(value);
            }
        }

        internal void AddRelay(SyntaxBase syntax)
        {
            Relay.Add(syntax);
        }

        internal void AddRelayRange(IEnumerable<SyntaxBase> syntaxList)
        {
            Relay.AddRange(syntaxList);
        }

        public abstract string Represent();

        internal List<SyntaxBase> PassRelay()
        {
            var relay = Relay.ToList();
            relay.Add(this);
            return relay;
        }

        internal void RelaySyntax(SyntaxBase syntax)
        {
            var relay = PassRelay();
            syntax.AddRelayRange(relay);
        }

        public string RelayQuery(SyntaxBase syntax)
        {
            return Relay.RelayQuery(syntax);
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach (var relay in Relay)
                    {
                        relay.Dispose();
                    }
                    Relay.Clear();
                    Relay = null;
                    Parameters.Clear();
                    Parameters = null;
                    LocalParameters.Clear();
                    LocalParameters = null;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
