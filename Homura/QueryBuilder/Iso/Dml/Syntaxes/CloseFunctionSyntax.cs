﻿using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Transitions;
using System.Collections.Generic;
using System.Linq;
using static Homura.QueryBuilder.Iso.Dml.Syntaxes.ReplaceSyntax;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class CloseFunctionSyntax : SyntaxBase, ICountSyntax, INoMarginLeftSyntax, IComparisonPredicateTransition<IOperatorSyntax<IConditionValueSyntax>>
    {
        internal CloseFunctionSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        public IFromSyntax<ICloseSyntax<IConditionValueSyntax>> From { get { return new FromSyntax<ICloseSyntax<IConditionValueSyntax>, IConditionValueSyntax>(this); } }

        public IOperatorSyntax<IConditionValueSyntax> NotEqualTo => new NotEqualToSyntax<IConditionValueSyntax>(this, false);

        public IOperatorSyntax<IConditionValueSyntax> GreaterThan => new GreaterThanSyntax<IConditionValueSyntax>(this, false);

        public IOperatorSyntax<IConditionValueSyntax> LessThan => new LessThanSyntax<IConditionValueSyntax>(this, false);

        public IOperatorSyntax<IConditionValueSyntax> GreaterThanOrEqualTo => new GreaterThanOrEqualToSyntax<IConditionValueSyntax>(this, false);

        public IOperatorSyntax<IConditionValueSyntax> LessThanOrEqualTo => new LessThanOrEqualToSyntax<IConditionValueSyntax>(this, false);

        public IOperatorSyntax<IConditionValueSyntax> EqualTo => new EqualToSyntax<IConditionValueSyntax>(this, false);

        public IAsSyntax As(string columnAlias)
        {
            return new AsSyntax(this, columnAlias);
        }

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

        public IColumnSyntax SubQuery(IJoinConditionSyntax subquery)
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
            return ")";
        }

        public IColumnSyntax Replace(string expression, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Expression, expression, pattern, replacement);
        }

        public IColumnSyntax ReplaceColumn(string columnName, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Column, columnName, pattern, replacement);
        }

        public IColumnSyntax ReplaceColumn(string tableAlias, string columnName, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Column, tableAlias, columnName, pattern, replacement);
        }
    }
}
