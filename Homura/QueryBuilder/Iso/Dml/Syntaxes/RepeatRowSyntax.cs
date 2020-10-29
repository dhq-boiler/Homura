using Homura.QueryBuilder.Core;
using System.Collections.Generic;
using System.Linq;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class RepeatRowSyntax : RepeatableSyntax, IValuesSyntax
    {
        internal RepeatRowSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        internal RepeatRowSyntax(SyntaxBase syntaxBase, Delimiter prefix)
            : base(syntaxBase, prefix)
        { }

        public IRowSyntax Row(IEnumerable<object> values)
        {
            return Row(values.ToArray());
        }

        public IRowSyntax Row(object[] values)
        {
            return new RowSyntax(this, values, Delimiter.None);
        }

        public ISetClauseValueSyntax Value(IEnumerable<object> values)
        {
            return Value(values.ToArray());
        }

        public ISetClauseValueSyntax Value(params object[] values)
        {
            return new SetClauseValueSyntax(this, values, Delimiter.OpenedParenthesis);
        }

        public ISetClauseValueSyntax Value(object value)
        {
            return new SetClauseValueSyntax(this, value, Delimiter.OpenedParenthesis);
        }

        public IRowSyntax SubQuery(ICloseSyntax<IConditionValueSyntax> subquery)
        {
            var begin = new BeginSubquerySyntax(this);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end;
        }

        public IRowSyntax SubQuery(IConditionValueSyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end;
        }

        public IRowSyntax SubQuery(IJoinConditionSyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end;
        }

        public IRowSyntax SubQuery(IOrderBySyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end;
        }

        public IRowSyntax SubQuery(IOrderByColumnSyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end;
        }

        public override string Represent()
        {
            return $"{Delimiter.ToString()}";
        }

        public IRowSyntax Rows(IEnumerable<IEnumerable<object>> rows)
        {
            return Rows(rows.Cast<object[]>().ToArray());
        }

        public IRowSyntax Rows(params object[][] rows)
        {
            IRowSyntax ret = null;
            foreach (var row in rows)
            {
                if (ret == null)
                {
                    ret = new RowSyntax(this, row);
                }
                else
                {
                    ret = new RowSyntax(ret as SyntaxBase, row, Delimiter.Comma);
                }
            }
            return ret;
        }
    }
}
