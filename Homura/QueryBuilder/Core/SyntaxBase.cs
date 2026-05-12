using Homura.QueryBuilder.Iso.Dml;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Homura.QueryBuilder.Core
{
    public abstract class SyntaxBase : ISyntaxBase, IDisposable
    {
        internal SyntaxBase Parent { get; private set; }
        private List<SyntaxBase> _relayCache;

        /// <summary>
        /// Returns the rendering chain for this node (everything that should precede it).
        /// Built lazily on first access; subsequent reads (including subquery splices that
        /// mutate the list via AddRange) hit the cache. Construction no longer copies.
        /// </summary>
        internal List<SyntaxBase> Relay
        {
            get
            {
                if (_relayCache == null)
                {
                    _relayCache = new List<SyntaxBase>();
                    if (Parent != null)
                    {
                        _relayCache.AddRange(Parent.Relay);
                        _relayCache.Add(Parent);
                    }
                }
                return _relayCache;
            }
            set
            {
                _relayCache = value;
            }
        }

        internal Dictionary<string, object> Parameters { get; set; }

        protected List<string> LocalParameters { get; set; }

        protected int ParameterCount { get; set; }

        protected SyntaxBase()
        {
            Parameters = new Dictionary<string, object>();
            LocalParameters = new List<string>();
            ParameterCount = 0;
        }

        protected SyntaxBase(SyntaxBase syntaxBase)
        {
            Parent = syntaxBase;
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
            var relay = new List<SyntaxBase>(Relay);
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
                    if (_relayCache != null)
                    {
                        foreach (var relay in _relayCache)
                        {
                            relay.Dispose();
                        }
                        _relayCache.Clear();
                        _relayCache = null;
                    }
                    else
                    {
                        Parent?.Dispose();
                    }
                    Parent = null;
                    Parameters?.Clear();
                    Parameters = null;
                    LocalParameters?.Clear();
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
