

using Homura.QueryBuilder.Core;
using System.Collections.Generic;
using System.Linq;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class RowSyntax : RepeatableSyntax, IRowSyntax
    {
        internal RowSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        internal RowSyntax(SyntaxBase syntaxBase, object[] values)
            : base(syntaxBase)
        {
            AddParameters(values);
        }

        internal RowSyntax(SyntaxBase syntaxBase, object[] values, Delimiter prefix)
            : base(syntaxBase, prefix)
        {
            AddParameters(values);
        }

        public IRowSyntax Row(IEnumerable<object> values)
        {
            return Row(values.ToArray());
        }

        public IRowSyntax Row(params object[] values)
        {
            return new RowSyntax(this, values, Delimiter.Comma);
        }

        public override string Represent()
        {
            string ret = $"{Delimiter.ToString()}(";
            ret += ValueLoop(LocalParameters.Cast<object>().ToList());
            ret += ")";
            return ret;
        }

        public string ToSql()
        {
            return Relay.RelayQuery(this);
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
