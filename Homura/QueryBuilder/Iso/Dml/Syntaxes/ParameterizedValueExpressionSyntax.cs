

using Homura.QueryBuilder.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class ParameterizedValueExpressionSyntax : SyntaxBase, IConditionValueSyntax, IJoinConditionSyntax, ISinkStateSyntax, IValueExpressionSyntax
    {
        internal ParameterizedValueExpressionSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        internal ParameterizedValueExpressionSyntax(SyntaxBase syntaxBase, object value)
            : this(syntaxBase)
        {
            AddParameter(value);
        }

        public IWhereSyntax<IJoinConditionSyntax, IOperatorSyntax<IJoinConditionSyntax>, IIsSyntax<IJoinConditionSyntax>> Where { get { return new WhereSyntax<IJoinConditionSyntax, IOperatorSyntax<IJoinConditionSyntax>, IIsSyntax<IJoinConditionSyntax>>(this); } }

        public INaturalSyntax Natural { get { return new NaturalSyntax(this); } }

        public IJoinTypeSyntax Inner { get { return new InnerSyntax(this); } }

        public IOuterJoinTypeSyntax Left { get { return new LeftSyntax(this); } }

        public IOuterJoinTypeSyntax Right { get { return new RightSyntax(this); } }

        public IOuterJoinTypeSyntax Full { get { return new FullSyntax(this); } }

        public IOrderBySyntax OrderBy { get { return new OrderBySyntax(this); } }

        public IGroupBySyntax GroupBy { get { return new GroupBySyntax(this); } }

        public IUnionSyntax Union { get { return new UnionSyntax(this); } }

        public ICrossSyntax Cross { get { return new CrossSyntax(this); } }

        public IExceptSyntax Except { get { return new ExceptSyntax(this); } }

        public IIntersectSyntax Intersect { get { return new IntersectSyntax(this); } }

        IWhereSyntax<ISinkStateSyntax, IOperatorSyntax<ISinkStateSyntax>, IIsSyntax<ISinkStateSyntax>> IValueExpressionSyntax.Where { get { return new WhereSyntax<ISinkStateSyntax, IOperatorSyntax<ISinkStateSyntax>, IIsSyntax<ISinkStateSyntax>>(this); } }

        public string ToSql()
        {
            return Relay.RelayQuery(this);
        }

        public override string Represent()
        {
            return $"{LocalParameters.First()}";
        }

        public IJoinTableSyntax Join(string tableName)
        {
            return new JoinTableSyntax(this, tableName);
        }

        public IJoinTableSyntax Join(string tableName, string tableAlias)
        {
            return new JoinTableSyntax(this, tableName, tableAlias);
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

        public ISetClauseSyntax Column(string name)
        {
            return new UpdateColumnSyntax(this, name, Delimiter.Comma);
        }

        public ISetClauseSyntax Column(string tableAlias, string name)
        {
            return new UpdateColumnSyntax(this, tableAlias, name);
        }

        public ISetClauseSyntax Columns(IEnumerable<string> names)
        {
            throw new NotSupportedException();
        }

        public ISetClauseSyntax Columns(params string[] names)
        {
            throw new NotSupportedException();
        }
    }
}
