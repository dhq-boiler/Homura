

using Homura.QueryBuilder.Core;
using System.Collections.Generic;
using System.Linq;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class UsingSyntax : SyntaxBase, IJoinConditionSyntax
    {
        private List<string> _columnNames;

        internal UsingSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        internal UsingSyntax(SyntaxBase syntaxBase, string[] columnNames)
            : this(syntaxBase)
        {
            _columnNames = new List<string>(columnNames);
        }

        public IJoinTypeSyntax Inner { get { return new InnerSyntax(this); } }

        public IOuterJoinTypeSyntax Left { get { return new LeftSyntax(this); } }

        public INaturalSyntax Natural { get { return new NaturalSyntax(this); } }

        public IOuterJoinTypeSyntax Right { get { return new RightSyntax(this); } }

        public IOuterJoinTypeSyntax Full { get { return new FullSyntax(this); } }

        public IUnionSyntax Union { get { return new UnionSyntax(this); } }

        public ICrossSyntax Cross { get { return new CrossSyntax(this); } }

        public IOrderBySyntax OrderBy { get { return new OrderBySyntax(this); } }

        public IWhereSyntax<IJoinConditionSyntax, IOperatorSyntax<IJoinConditionSyntax>, IIsSyntax<IJoinConditionSyntax>> Where { get { return new WhereSyntax<IJoinConditionSyntax, IOperatorSyntax<IJoinConditionSyntax>, IIsSyntax<IJoinConditionSyntax>>(this); } }

        public IGroupBySyntax GroupBy { get { return new GroupBySyntax(this); } }

        public IExceptSyntax Except { get { return new ExceptSyntax(this); } }

        public IIntersectSyntax Intersect { get { return new IntersectSyntax(this); } }

        public string ToSql()
        {
            return Relay.RelayQuery(this);
        }

        public override string Represent()
        {
            string ret = "USING(";
            int i = 0;
            foreach (var columnName in _columnNames)
            {
                ret += columnName;
                if (i + 1 < _columnNames.Count())
                {
                    ret += ", ";
                }
                ++i;
            }
            ret += ")";
            return ret;
        }

        public IJoinTableSyntax Join(string tableName)
        {
            return new JoinTableSyntax(this, tableName);
        }

        public IJoinTableSyntax Join(string tableName, string tableAlias)
        {
            return new JoinTableSyntax(this, tableName, tableAlias);
        }
    }
}
