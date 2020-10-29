

using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class OrderByColumnSyntax : RepeatableSyntax, IOrderByColumnSyntax
    {
        private string _name;
        private string _tableAlias;

        internal OrderByColumnSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        internal OrderByColumnSyntax(SyntaxBase syntaxBase, string name)
            : this(syntaxBase)
        {
            _name = name;
        }

        internal OrderByColumnSyntax(SyntaxBase syntaxBase, string tableAlias, string name)
            : this(syntaxBase, name)
        {
            _tableAlias = tableAlias;
        }

        public OrderByColumnSyntax(SyntaxBase syntaxBase, string name, Delimiter prefix)
            : base(syntaxBase, prefix)
        {
            _name = name;
        }

        public OrderByColumnSyntax(SyntaxBase syntaxBase, string tableAlias, string name, Delimiter prefix)
            : this(syntaxBase, name, prefix)
        {
            _tableAlias = tableAlias;
        }

        public IWhereSyntax<IConditionValueSyntax, IOperatorSyntax<IConditionValueSyntax>, IIsSyntax<IConditionValueSyntax>> Where { get { return new WhereSyntax<IConditionValueSyntax, IOperatorSyntax<IConditionValueSyntax>, IIsSyntax<IConditionValueSyntax>>(this); } }

        public INaturalSyntax Natural { get { return new NaturalSyntax(this); } }

        public IJoinTypeSyntax Inner { get { return new InnerSyntax(this); } }

        public IOuterJoinTypeSyntax Left { get { return new LeftSyntax(this); } }

        public IOuterJoinTypeSyntax Right { get { return new RightSyntax(this); } }

        public IOuterJoinTypeSyntax Full { get { return new FullSyntax(this); } }

        public IOrderTypeSyntax Asc { get { return new AscSyntax(this); } }

        public IUnionSyntax Union { get { return new UnionSyntax(this); } }

        public IOrderByColumnSyntax Column(string name)
        {
            return new OrderByColumnSyntax(this, name, Delimiter.Comma);
        }

        public IOrderByColumnSyntax Column(string tableAlias, string name)
        {
            return new OrderByColumnSyntax(this, tableAlias, name, Delimiter.Comma);
        }

        public IOrderTypeSyntax Desc { get { return new DescSyntax(this); } }

        public string ToSql()
        {
            return Relay.RelayQuery(this);
        }

        public override string Represent()
        {
            return $"{Delimiter.ToString()}{_tableAlias}{(!string.IsNullOrEmpty(_tableAlias) ? "." : "")}{_name}";
        }
    }
}
