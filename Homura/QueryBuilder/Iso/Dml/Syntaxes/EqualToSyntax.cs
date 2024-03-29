﻿using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Transitions;
using static Homura.QueryBuilder.Iso.Dml.Syntaxes.ReplaceSyntax;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class EqualToSyntax<R> : SyntaxBase, IOperatorSyntax<R>, IOperatorColumnSyntax<R>, IUpdateSourceTransition<R> where R : class
    {
        public R Null { get { return new NullSyntax(this) as R; } }

        public R Default { get { return new DefaultSyntax(this) as R; } }

        private bool isParametarize;

        internal EqualToSyntax(SyntaxBase syntaxBase, bool isParametarize = true)
            : base(syntaxBase)
        {
            this.isParametarize = isParametarize;
        }

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
            if (isParametarize)
                return new ParameterizedValueExpressionSyntax(this, value) as R;
            else
                return new NonParameterizedValueExpressionSyntax(this, value) as R;
        }

        public override string Represent()
        {
            return "=";
        }

        public R Expression(string expression)
        {
            return new ValueExpressionSyntax(this, expression) as R;
        }

        public R Replace(string expression, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Expression, expression, pattern, replacement) as R;
        }

        public R ReplaceColumn(string columnName, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Column, columnName, pattern, replacement) as R;
        }

        public R ReplaceColumn(string tableAlias, string columnName, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Column, tableAlias, columnName, pattern, replacement) as R;
        }
    }
}
