using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Transitions;
using System.Collections.Generic;
using System.Linq;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class SelectSyntax : SyntaxBase, ISelectSyntax
    {
        public SelectSyntax()
            : base()
        { }

        internal SelectSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        public IColumnTransition<IColumnSyntax> All { get { return new AllSyntax(this); } }

        public ISetQuantifierSyntax Distinct { get { return new DistinctSyntax(this); } }

        public ICountSyntax Count()
        {
            var count = new CountSyntax(this);
            var open = new OpenFunctionSyntax(count);
            var asterisk = new FunctionAsteriskSyntax(open);
            return new CloseFunctionSyntax(asterisk);
        }

        public ICountSyntax Count(string name)
        {
            var count = new CountSyntax(this);
            var open = new OpenFunctionSyntax(count);
            var column = new FunctionColumnSyntax(open, name);
            return new CloseFunctionSyntax(column);
        }

        public ICountSyntax Count(ICountParameterSyntax column)
        {
            var count = new CountSyntax(this);
            var open = new OpenFunctionSyntax(count);
            var syntax = column as SyntaxBase;
            open.RelaySyntax(syntax);
            return new CloseFunctionSyntax(syntax);
        }

        public IColumnSyntax Column(string name)
        {
            return new ColumnSyntax(this, name);
        }

        public IColumnSyntax Column(string tableAlias, string name)
        {
            return new ColumnSyntax(this, tableAlias, name);
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
                    ret = new ColumnSyntax(this, name);
                }
                else
                {
                    ret = new ColumnSyntax(ret as SyntaxBase, name, Delimiter.Comma);
                }
            }
            return ret;
        }

        public IColumnSyntax SubQuery(IOrderByColumnSyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this, Delimiter.Comma);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end;
        }

        public IColumnSyntax SubQuery(IOrderBySyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this, Delimiter.Comma);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end;
        }

        public IColumnSyntax SubQuery(IConditionValueSyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this, Delimiter.Comma);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end;
        }

        public IColumnSyntax SubQuery(ICloseSyntax<IConditionValueSyntax> subquery)
        {
            var begin = new BeginSubquerySyntax(this, Delimiter.Comma);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end;
        }

        public IColumnSyntax SubQuery(IJoinConditionSyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this, Delimiter.Comma);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end;
        }

        public IColumnSyntax Asterisk()
        {
            return new AsteriskSyntax(this);
        }

        public IColumnSyntax Asterisk(string tableAlias)
        {
            return new AsteriskSyntax(this, tableAlias);
        }

        public override string Represent()
        {
            return "SELECT";
        }
    }
}
