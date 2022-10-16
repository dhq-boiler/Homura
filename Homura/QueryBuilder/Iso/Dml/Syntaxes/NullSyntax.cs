using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Transitions;
using System.Collections.Generic;
using System.Linq;
using static Homura.QueryBuilder.Iso.Dml.Syntaxes.ReplaceSyntax;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class NullSyntax : SyntaxBase, IConditionValueSyntax, ISinkStateSyntax, IValueExpressionSyntax, IJoinConditionSyntax
    {
        internal NullSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        public IJoinTypeSyntax Inner { get { return new InnerSyntax(this); } }

        public IOuterJoinTypeSyntax Left { get { return new LeftSyntax(this); } }

        public INaturalSyntax Natural { get { return new NaturalSyntax(this); } }

        public IOrderBySyntax OrderBy { get { return new OrderBySyntax(this); } }

        public IOuterJoinTypeSyntax Right { get { return new RightSyntax(this); } }

        public IOuterJoinTypeSyntax Full { get { return new FullSyntax(this); } }

        public IGroupBySyntax GroupBy { get { return new GroupBySyntax(this); } }

        public IUnionSyntax Union { get { return new UnionSyntax(this); } }

        public IWhereSyntax<ISinkStateSyntax, IOperatorSyntax<ISinkStateSyntax>, IIsSyntax<ISinkStateSyntax>> Where { get { return new WhereSyntax<ISinkStateSyntax, IOperatorSyntax<ISinkStateSyntax>, IIsSyntax<ISinkStateSyntax>>(this); } }

        IWhereSyntax<IJoinConditionSyntax, IOperatorSyntax<IJoinConditionSyntax>, IIsSyntax<IJoinConditionSyntax>> IWhereTransition<IJoinConditionSyntax>.Where => throw new System.NotImplementedException();

        public ICrossSyntax Cross => throw new System.NotImplementedException();

        public IExceptSyntax Except => throw new System.NotImplementedException();

        public IIntersectSyntax Intersect => throw new System.NotImplementedException();

        public string ToSql()
        {
            return Relay.RelayQuery(this);
        }

        public override string Represent()
        {
            return "NULL";
        }

        public IValueExpressionSyntax KeyEqualToValue(Dictionary<string, object> columnValues)
        {
            IValueExpressionSyntax ret = null;
            foreach (var columnValue in columnValues)
            {
                if (ret == null)
                {
                    var column = new UpdateColumnSyntax(this, columnValue.Key, Delimiter.Comma);
                    var equalTo = new EqualToSyntax<IValueExpressionSyntax>(column);
                    ret = new ParameterizedValueExpressionSyntax(equalTo, columnValue.Value);
                }
                else
                {
                    var column = new UpdateColumnSyntax(ret as SyntaxBase, columnValue.Key, Delimiter.Comma);
                    var equalTo = new EqualToSyntax<IValueExpressionSyntax>(column);
                    ret = new ParameterizedValueExpressionSyntax(equalTo, columnValue.Value);
                }
            }
            return ret;
        }

        public ISetClauseSyntax Column(string name)
        {
            return new UpdateColumnSyntax(this, name);
        }

        public ISetClauseSyntax Column(string tableAlias, string name)
        {
            return new UpdateColumnSyntax(this, tableAlias, name);
        }

        public ISetClauseSyntax Columns(IEnumerable<string> names)
        {
            return Columns(names.ToArray());
        }

        public ISetClauseSyntax Columns(params string[] names)
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

        public IJoinTableSyntax Join(string tableName)
        {
            throw new System.NotImplementedException();
        }

        public IJoinTableSyntax Join(string tableName, string tableAlias)
        {
            throw new System.NotImplementedException();
        }

        public ISetClauseSyntax Replace(string expression, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Expression, expression, pattern, replacement);
        }

        public ISetClauseSyntax ReplaceColumn(string columnName, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Column, columnName, pattern, replacement);
        }

        public ISetClauseSyntax ReplaceColumn(string tableAlias, string columnName, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Column, tableAlias, columnName, pattern, replacement);
        }
    }
}
