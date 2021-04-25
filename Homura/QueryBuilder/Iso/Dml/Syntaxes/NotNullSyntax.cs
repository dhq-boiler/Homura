

using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class NotNullSyntax : SyntaxBase, IConditionValueSyntax, ISinkStateSyntax, IJoinConditionSyntax
    {
        internal NotNullSyntax(SyntaxBase syntaxBase)
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

        public IWhereSyntax<IJoinConditionSyntax, IOperatorSyntax<IJoinConditionSyntax>, IIsSyntax<IJoinConditionSyntax>> Where => throw new System.NotImplementedException();

        public ICrossSyntax Cross => throw new System.NotImplementedException();

        public IExceptSyntax Except => throw new System.NotImplementedException();

        public IIntersectSyntax Intersect => throw new System.NotImplementedException();

        public string ToSql()
        {
            return Relay.RelayQuery(this);
        }

        public override string Represent()
        {
            return "NOT NULL";
        }

        public IJoinTableSyntax Join(string tableName)
        {
            throw new System.NotImplementedException();
        }

        public IJoinTableSyntax Join(string tableName, string tableAlias)
        {
            throw new System.NotImplementedException();
        }
    }
}
