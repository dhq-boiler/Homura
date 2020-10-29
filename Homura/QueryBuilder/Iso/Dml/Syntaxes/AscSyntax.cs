using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class AscSyntax : SyntaxBase, IOrderTypeSyntax, ICloseSyntax<IConditionValueSyntax>
    {
        internal AscSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        public IWhereSyntax<IConditionValueSyntax, IOperatorSyntax<IConditionValueSyntax>, IIsSyntax<IConditionValueSyntax>> Where { get { return new WhereSyntax<IConditionValueSyntax, IOperatorSyntax<IConditionValueSyntax>, IIsSyntax<IConditionValueSyntax>>(this); } }

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

        public IOrderByColumnSyntax Column(string name)
        {
            return new OrderByColumnSyntax(this, name, Delimiter.Comma);
        }

        public IOrderByColumnSyntax Column(string tableAlias, string name)
        {
            return new OrderByColumnSyntax(this, tableAlias, name, Delimiter.Comma);
        }

        public override string Represent()
        {
            return "ASC";
        }

        public string ToSql()
        {
            return Relay.RelayQuery(this);
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
