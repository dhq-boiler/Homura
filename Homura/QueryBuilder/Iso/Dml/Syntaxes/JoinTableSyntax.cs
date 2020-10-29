

using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class JoinTableSyntax : SyntaxBase, IJoinTableSyntax, IJoinConditionSyntax
    {
        private string _tableAlias;
        private string _tableName;

        internal JoinTableSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        internal JoinTableSyntax(SyntaxBase syntaxBase, string tableName)
            : this(syntaxBase)
        {
            _tableName = tableName;
        }

        internal JoinTableSyntax(SyntaxBase syntaxBase, string tableName, string tableAlias)
            : this(syntaxBase, tableName)
        {
            _tableAlias = tableAlias;
        }

        public IOnSyntax On { get { return new OnSyntax(this); } }

        public IWhereSyntax<IJoinConditionSyntax, IOperatorSyntax<IJoinConditionSyntax>, IIsSyntax<IJoinConditionSyntax>> Where { get { return new WhereSyntax<IJoinConditionSyntax, IOperatorSyntax<IJoinConditionSyntax>, IIsSyntax<IJoinConditionSyntax>>(this); } }

        public INaturalSyntax Natural { get { return new NaturalSyntax(this); } }

        public IJoinTypeSyntax Inner { get { return new InnerSyntax(this); } }

        public IUnionSyntax Union { get { return new UnionSyntax(this); } }

        public IOuterJoinTypeSyntax Left { get { return new LeftSyntax(this); } }

        public IOuterJoinTypeSyntax Right { get { return new RightSyntax(this); } }

        public IOuterJoinTypeSyntax Full { get { return new FullSyntax(this); } }

        public ICrossSyntax Cross { get { return new CrossSyntax(this); } }

        public IOrderBySyntax OrderBy { get { return new OrderBySyntax(this); } }

        public IGroupBySyntax GroupBy { get { return new GroupBySyntax(this); } }

        public IExceptSyntax Except { get { return new ExceptSyntax(this); } }

        public IIntersectSyntax Intersect { get { return new IntersectSyntax(this); } }

        public IJoinConditionSyntax Using(string[] columnNames)
        {
            return new UsingSyntax(this, columnNames);
        }

        public override string Represent()
        {
            return $"JOIN {_tableName}{(!string.IsNullOrEmpty(_tableAlias) ? " " : "")}{_tableAlias}";
        }

        public IJoinTableSyntax Join(string tableName)
        {
            return new JoinTableSyntax(this, tableName);
        }

        public IJoinTableSyntax Join(string tableName, string tableAlias)
        {
            return new JoinTableSyntax(this, tableName, tableAlias);
        }

        public string ToSql()
        {
            return Relay.RelayQuery(this);
        }
    }
}
