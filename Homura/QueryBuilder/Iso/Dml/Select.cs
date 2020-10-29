
using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Syntaxes;
using Homura.QueryBuilder.Iso.Dml.Transitions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Homura.QueryBuilder.Iso.Dml
{
    public class Select : ISelectSyntax, IDisposable
    {
        internal SelectSyntax _syntax = new SelectSyntax();

        public Select()
            : base()
        { }

        public IColumnTransition<IColumnSyntax> All { get { return new AllSyntax(_syntax); } }

        public ISetQuantifierSyntax Distinct { get { return new DistinctSyntax(_syntax); } }

        public IColumnSyntax Column(string name)
        {
            return new ColumnSyntax(_syntax, name);
        }

        public IColumnSyntax Column(string tableAlias, string name)
        {
            return new ColumnSyntax(_syntax, tableAlias, name);
        }

        public IColumnSyntax Columns(IEnumerable<string> names)
        {
            return Columns(names.ToArray());
        }

        public IColumnSyntax Columns(params string[] names)
        {
            IColumnSyntax ret = null;
            foreach (var name in names)
            {
                if (ret == null)
                {
                    ret = new ColumnSyntax(_syntax, name);
                }
                else
                {
                    ret = new ColumnSyntax(ret as SyntaxBase, name, Delimiter.Comma);
                }
            }
            return ret;
        }

        public ICountSyntax Count()
        {
            return _syntax.Count();
        }

        public ICountSyntax Count(string name)
        {
            return _syntax.Count(name);
        }

        public ICountSyntax Count(ICountParameterSyntax column)
        {
            return _syntax.Count(column);
        }

        public IColumnSyntax SubQuery(IConditionValueSyntax subquery)
        {
            return _syntax.SubQuery(subquery);
        }

        public IColumnSyntax SubQuery(IJoinConditionSyntax subquery)
        {
            return _syntax.SubQuery(subquery);
        }

        public IColumnSyntax SubQuery(IOrderByColumnSyntax subquery)
        {
            return _syntax.SubQuery(subquery);
        }

        public IColumnSyntax SubQuery(IOrderBySyntax subquery)
        {
            return _syntax.SubQuery(subquery);
        }

        public IColumnSyntax SubQuery(ICloseSyntax<IConditionValueSyntax> subquery)
        {
            return _syntax.SubQuery(subquery);
        }

        public IColumnSyntax Asterisk()
        {
            return new AsteriskSyntax(_syntax);
        }

        public IColumnSyntax Asterisk(string tableAlias)
        {
            return new AsteriskSyntax(_syntax, tableAlias);
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
