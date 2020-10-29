

using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class LessThanSyntax<R> : SyntaxBase, IOperatorSyntax<R>, IOperatorColumnSyntax<R> where R : class
    {
        internal LessThanSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        public R Column(string name)
        {
            return new ConditionColumnSyntax(this, name) as R;
        }

        public R Column(string tableAlias, string name)
        {
            return new ConditionColumnSyntax(this, tableAlias, name) as R;
        }

        public R SubQuery(IOrderBySyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end as R;
        }

        public R SubQuery(IOrderByColumnSyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end as R;
        }

        public R SubQuery(IJoinConditionSyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end as R;
        }

        public R SubQuery(IConditionValueSyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end as R;
        }

        public R SubQuery(ICloseSyntax<IConditionValueSyntax> subquery)
        {
            var begin = new BeginSubquerySyntax(this);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end as R;
        }

        public R Value(object value)
        {
            return new ParameterizedValueExpressionSyntax(this, value) as R;
        }

        public override string Represent()
        {
            return "<";
        }
    }
}
