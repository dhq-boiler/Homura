using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class ConditionColumnSyntax : SyntaxBase, IJoinConditionSyntax
    {
        private string _name;
        private string _tableAlias;

        internal ConditionColumnSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        internal ConditionColumnSyntax(SyntaxBase syntaxBase, string name)
            : this(syntaxBase)
        {
            _name = name;
        }

        internal ConditionColumnSyntax(SyntaxBase syntaxBase, string tableAlias, string name)
            : this(syntaxBase, name)
        {
            _tableAlias = tableAlias;
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

        public string ToSql()
        {
            return Relay.RelayQuery(this);
        }

        public override string Represent()
        {
            return $"{_tableAlias}{(!string.IsNullOrEmpty(_tableAlias) ? "." : "")}{_name}";
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
