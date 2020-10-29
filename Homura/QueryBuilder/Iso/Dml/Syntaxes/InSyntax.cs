using Homura.QueryBuilder.Core;
using System.Collections.Generic;
using System.Linq;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class InSyntax : SyntaxBase, IInSyntax
    {
        internal InSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        public IInValueSyntax Value(object value)
        {
            return new InValueSyntax(this, value, Delimiter.OpenedParenthesis);
        }

        public IInValueSyntax Value(params object[] values)
        {
            return new InValueSyntax(this, values, Delimiter.OpenedParenthesis);
        }

        public IInValueSyntax Array(IEnumerable<object> values)
        {
            return Value(values.ToArray());
        }

        public override string Represent()
        {
            return "IN";
        }

        public ICloseSyntax<IConditionValueSyntax> SubQuery(ICloseSyntax<IConditionValueSyntax> subquery)
        {
            var begin = new BeginSubquerySyntax(this);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end as ICloseSyntax<IConditionValueSyntax>;
        }

        public ICloseSyntax<IConditionValueSyntax> SubQuery(IConditionValueSyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end as ICloseSyntax<IConditionValueSyntax>;
        }

        public ICloseSyntax<IConditionValueSyntax> SubQuery(IJoinConditionSyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end as ICloseSyntax<IConditionValueSyntax>;
        }

        public ICloseSyntax<IConditionValueSyntax> SubQuery(IOrderBySyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end as ICloseSyntax<IConditionValueSyntax>;
        }

        public ICloseSyntax<IConditionValueSyntax> SubQuery(IOrderByColumnSyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end as ICloseSyntax<IConditionValueSyntax>;
        }
    }
}
