using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Transitions;
using System.Collections.Generic;
using System.Linq;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class ReplaceSyntax : SyntaxBase, IColumnSyntax, ICorrespondingColumnSyntax, ISetClauseSyntax, IGroupByColumnSyntax, IInsertColumnSyntax, IValueExpressionSyntax
    {
        public ReplaceSyntax(SyntaxBase syntaxBase, EExpression columnOrExpr, string columnNameOrExpr, string pattern, string replacement) : base(syntaxBase)
        {
            ColumnOrExpr = columnOrExpr;
            Expression = columnNameOrExpr;
            Pattern = pattern;
            Replacement = replacement;
        }

        public ReplaceSyntax(SyntaxBase syntaxBase, EExpression columnOrExpr, string tableAlias, string columnName, string pattern, string replacement) : this(syntaxBase, columnOrExpr, columnName, pattern, replacement)
        {
            TableAlias = tableAlias;
        }

        public ISelectSyntax Select
        {
            get
            {
                var close = new CloseParenthesisSyntax(this);
                return new SelectSyntax(close);
            }
        }

        public ICrossSyntax Cross
        {
            get
            {
                var close = new CloseParenthesisSyntax(this);
                return new CrossSyntax(close);
            }
        }

        public IJoinTypeSyntax Inner
        {
            get
            {
                var close = new CloseParenthesisSyntax(this);
                return new InnerSyntax(close);
            }
        }

        public IUnionSyntax Union
        {
            get
            {
                var close = new CloseParenthesisSyntax(this);
                return new UnionSyntax(close);
            }
        }

        public IOuterJoinTypeSyntax Left
        {
            get
            {
                var close = new CloseParenthesisSyntax(this);
                return new LeftSyntax(close);
            }
        }

        public IOuterJoinTypeSyntax Right
        {
            get
            {
                var close = new CloseParenthesisSyntax(this);
                return new RightSyntax(close);
            }
        }

        public IOuterJoinTypeSyntax Full
        {
            get
            {
                var close = new CloseParenthesisSyntax(this);
                return new FullSyntax(close);
            }
        }

        public INaturalSyntax Natural
        {
            get
            {
                var close = new CloseParenthesisSyntax(this);
                return new NaturalSyntax(close);
            }
        }

        public EExpression ColumnOrExpr { get; }
        public string Expression { get; }
        public string Pattern { get; }
        public string Replacement { get; }
        public string TableAlias { get; }

        public IFromSyntax<ICloseSyntax<IConditionValueSyntax>> From => new FromSyntax<ICloseSyntax<IConditionValueSyntax>, IConditionValueSyntax>(this);

        public IUpdateSourceTransition<IValueExpressionSyntax> EqualTo => new EqualToSyntax<IValueExpressionSyntax>(this, false);

        public IWhereSyntax<IConditionValueSyntax, IOperatorSyntax<IConditionValueSyntax>, IIsSyntax<IConditionValueSyntax>> Where => new WhereSyntax<IConditionValueSyntax, IOperatorSyntax<IConditionValueSyntax>, IIsSyntax<IConditionValueSyntax>>(this);

        public IOrderBySyntax OrderBy => new OrderBySyntax(this);

        public IHavingSyntax<IComparisonPredicateTransition<IOperatorSyntax<IConditionValueSyntax>>> Having => new HavingSyntax<IComparisonPredicateTransition<IOperatorSyntax<IConditionValueSyntax>>>(this);

        public IValuesSyntax Values
        {
            get
            {
                var close = new CloseParenthesisSyntax(this);
                return new ValuesSyntax(close);
            }
        }

        IWhereSyntax<ISinkStateSyntax, IOperatorSyntax<ISinkStateSyntax>, IIsSyntax<ISinkStateSyntax>> IValueExpressionSyntax.Where => new WhereSyntax<ISinkStateSyntax, IOperatorSyntax<ISinkStateSyntax>, IIsSyntax<ISinkStateSyntax>>(this);

        public IAsSyntax As(string columnAlias)
        {
            return new AsSyntax(this, columnAlias);
        }

        public IColumnSyntax Asterisk()
        {
            return new AsteriskSyntax(this, Delimiter.Comma);
        }

        public IColumnSyntax Asterisk(string tableAlias)
        {
            return new AsteriskSyntax(this, tableAlias, Delimiter.Comma);
        }

        public ICorrespondingColumnSyntax Column(string name)
        {
            return new ColumnSyntax(this, name, Delimiter.Comma);
        }

        public ICorrespondingColumnSyntax Column(string tableAlias, string name)
        {
            return new ColumnSyntax(this, tableAlias, name, Delimiter.Comma);
        }

        public ICorrespondingColumnSyntax Columns(IEnumerable<string> names)
        {
            return Columns(names.ToArray());
        }

        public ICorrespondingColumnSyntax Columns(params string[] names)
        {
            ICorrespondingColumnSyntax ret = null;
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

        public IValueExpressionSyntax KeyEqualToValue(Dictionary<string, object> columnValues)
        {
            IValueExpressionSyntax ret = null;
            foreach (var columnValue in columnValues)
            {
                if (ret == null)
                {
                    var column = new UpdateColumnSyntax(this, columnValue.Key, Delimiter.Comma);
                    ret = new SubstituteSyntax(column, columnValue.Value);
                }
                else
                {
                    var column = new UpdateColumnSyntax(ret as SyntaxBase, columnValue.Key, Delimiter.Comma);
                    ret = new SubstituteSyntax(column, columnValue.Value);
                }
            }
            return ret;
        }

        public ICorrespondingColumnSyntax Replace(string expression, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Expression, expression, pattern, replacement);
        }

        public ICorrespondingColumnSyntax ReplaceColumn(string columnName, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Column, columnName, pattern, replacement);
        }

        public ICorrespondingColumnSyntax ReplaceColumn(string tableAlias, string columnName, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Column, tableAlias, columnName, pattern, replacement);
        }

        public override string Represent()
        {
            return $"REPLACE("
                + $"{(ColumnOrExpr == EExpression.Column ? (!string.IsNullOrWhiteSpace(TableAlias) ? TableAlias + "." + Expression : Expression) : "'" + Expression + "'")}"
                + $", '{Pattern}'"
                + $", '{Replacement}')";
        }

        public IColumnSyntax SubQuery(ICloseSyntax<IConditionValueSyntax> subquery)
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

        public IColumnSyntax SubQuery(IJoinConditionSyntax subquery)
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

        public IColumnSyntax SubQuery(IOrderByColumnSyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this, Delimiter.Comma);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end;
        }

        public string ToSql()
        {
            return Relay.RelayQuery(this);
        }

        IColumnSyntax IColumnTransition<IColumnSyntax>.Column(string name)
        {
            return new ColumnSyntax(this, name, Delimiter.Comma);
        }

        IColumnSyntax IColumnTransition<IColumnSyntax>.Column(string tableAlias, string name)
        {
            return new ColumnSyntax(this, tableAlias, name, Delimiter.Comma);
        }

        IGroupByColumnSyntax IColumnTransition<IGroupByColumnSyntax>.Column(string name)
        {
            return new GroupByColumnSyntax(this, name, Delimiter.Comma);
        }

        IGroupByColumnSyntax IColumnTransition<IGroupByColumnSyntax>.Column(string tableAlias, string name)
        {
            return new GroupByColumnSyntax(this, tableAlias, name, Delimiter.Comma);
        }

        IInsertColumnSyntax IColumnTransition<IInsertColumnSyntax>.Column(string name)
        {
            return new InsertColumnSyntax(this, name, Delimiter.Comma);
        }

        IInsertColumnSyntax IColumnTransition<IInsertColumnSyntax>.Column(string tableAlias, string name)
        {
            return new InsertColumnSyntax(this, tableAlias, name, Delimiter.Comma);
        }

        ISetClauseSyntax IColumnTransition<ISetClauseSyntax>.Column(string name)
        {
            return new UpdateColumnSyntax(this, name, Delimiter.Comma);
        }

        ISetClauseSyntax IColumnTransition<ISetClauseSyntax>.Column(string tableAlias, string name)
        {
            return new UpdateColumnSyntax(this, tableAlias, name);
        }

        IColumnSyntax IColumnTransition<IColumnSyntax>.Columns(IEnumerable<string> names)
        {
            return (IColumnSyntax)Columns(names.ToArray());
        }

        IColumnSyntax IColumnTransition<IColumnSyntax>.Columns(params string[] names)
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

        IGroupByColumnSyntax IColumnTransition<IGroupByColumnSyntax>.Columns(IEnumerable<string> names)
        {
            return (IGroupByColumnSyntax)Columns(names.ToArray());
        }

        IGroupByColumnSyntax IColumnTransition<IGroupByColumnSyntax>.Columns(params string[] names)
        {
            IGroupByColumnSyntax ret = null;
            foreach (var name in names)
            {
                if (ret == null)
                {
                    ret = new GroupByColumnSyntax(this, name, Delimiter.Comma);
                }
                else
                {
                    ret = new GroupByColumnSyntax(ret as SyntaxBase, name, Delimiter.Comma);
                }
            }
            return ret;
        }

        IInsertColumnSyntax IColumnTransition<IInsertColumnSyntax>.Columns(IEnumerable<string> names)
        {
            return (IInsertColumnSyntax)Columns(names.ToArray());
        }

        IInsertColumnSyntax IColumnTransition<IInsertColumnSyntax>.Columns(params string[] names)
        {
            IInsertColumnSyntax ret = null;
            foreach (var name in names)
            {
                if (ret == null)
                {
                    ret = new InsertColumnSyntax(this, name, Delimiter.Comma);
                }
                else
                {
                    ret = new InsertColumnSyntax(ret as SyntaxBase, name, Delimiter.Comma);
                }
            }
            return ret;
        }

        ISetClauseSyntax IColumnTransition<ISetClauseSyntax>.Columns(IEnumerable<string> names)
        {
            return (ISetClauseSyntax)Columns(names.ToArray());
        }

        ISetClauseSyntax IColumnTransition<ISetClauseSyntax>.Columns(params string[] names)
        {
            ISetClauseSyntax ret = null;
            foreach (var name in names)
            {
                if (ret == null)
                {
                    ret = new UpdateColumnSyntax(this, name, Delimiter.Comma);
                }
                else
                {
                    ret = new UpdateColumnSyntax(ret as SyntaxBase, name, Delimiter.Comma);
                }
            }
            return ret;
        }

        IColumnSyntax IColumnTransition<IColumnSyntax>.Replace(string expression, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Expression, expression, pattern, replacement);
        }

        IGroupByColumnSyntax IColumnTransition<IGroupByColumnSyntax>.Replace(string expression, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Expression, expression, pattern, replacement);
        }

        IInsertColumnSyntax IColumnTransition<IInsertColumnSyntax>.Replace(string expression, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Expression, expression, pattern, replacement);
        }

        ISetClauseSyntax IColumnTransition<ISetClauseSyntax>.Replace(string expression, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Expression, expression, pattern, replacement);
        }

        IColumnSyntax IColumnTransition<IColumnSyntax>.ReplaceColumn(string columnName, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Column, columnName, pattern, replacement);
        }

        IColumnSyntax IColumnTransition<IColumnSyntax>.ReplaceColumn(string tableAlias, string columnName, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Column, tableAlias, columnName, pattern, replacement);
        }

        IGroupByColumnSyntax IColumnTransition<IGroupByColumnSyntax>.ReplaceColumn(string columnName, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Column, columnName, pattern, replacement);
        }

        IGroupByColumnSyntax IColumnTransition<IGroupByColumnSyntax>.ReplaceColumn(string tableAlias, string columnName, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Column, tableAlias, columnName, pattern, replacement);
        }

        IInsertColumnSyntax IColumnTransition<IInsertColumnSyntax>.ReplaceColumn(string columnName, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Column, columnName, pattern, replacement);
        }

        IInsertColumnSyntax IColumnTransition<IInsertColumnSyntax>.ReplaceColumn(string tableAlias, string columnName, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Column, tableAlias, columnName, pattern, replacement);
        }

        ISetClauseSyntax IColumnTransition<ISetClauseSyntax>.ReplaceColumn(string columnName, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Column, columnName, pattern, replacement);
        }

        ISetClauseSyntax IColumnTransition<ISetClauseSyntax>.ReplaceColumn(string tableAlias, string columnName, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Column, tableAlias, columnName, pattern, replacement);
        }

        public enum EExpression
        {
            Column,
            Expression,
        }
    }
}