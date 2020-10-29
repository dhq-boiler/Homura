using Homura.QueryBuilder.Core;
using System.Collections.Generic;
using System.Linq;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class NullSyntax : SyntaxBase, IConditionValueSyntax, ISinkStateSyntax, IValueExpressionSyntax
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
    }
}
