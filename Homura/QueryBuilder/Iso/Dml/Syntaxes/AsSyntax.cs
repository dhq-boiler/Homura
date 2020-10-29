using Homura.QueryBuilder.Core;
using System.Collections.Generic;
using System.Linq;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class AsSyntax : SyntaxBase, IAsSyntax
    {
        private string _columnAlias;

        internal AsSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        internal AsSyntax(SyntaxBase syntaxBase, string columnAlias)
            : this(syntaxBase)
        {
            _columnAlias = columnAlias;
        }

        public IFromSyntax<ICloseSyntax<IConditionValueSyntax>> From { get { return new FromSyntax<ICloseSyntax<IConditionValueSyntax>, IConditionValueSyntax>(this); } }

        public IColumnSyntax Column(string name)
        {
            return new ColumnSyntax(this, name, Delimiter.Comma);
        }

        public IColumnSyntax Column(string tableAlias, string name)
        {
            return new ColumnSyntax(this, tableAlias, name, Delimiter.Comma);
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
                    ret = new ColumnSyntax(this, name, Delimiter.Comma);
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

        public IColumnSyntax SubQuery(IJoinConditionSyntax subquery)
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

        public IColumnSyntax SubQuery(IOrderBySyntax subquery)
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

        public IColumnSyntax Asterisk()
        {
            return new AsteriskSyntax(this, Delimiter.Comma);
        }

        public IColumnSyntax Asterisk(string tableAlias)
        {
            return new AsteriskSyntax(this, tableAlias, Delimiter.Comma);
        }

        public override string Represent()
        {
            return $"AS '{_columnAlias}'";
        }
    }
}
