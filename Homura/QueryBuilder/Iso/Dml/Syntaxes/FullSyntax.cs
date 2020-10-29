using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class FullSyntax : SyntaxBase, IOuterJoinTypeSyntax
    {
        internal FullSyntax(SyntaxBase syntax)
            : base(syntax)
        { }

        public IJoinTypeSyntax Outer { get { return new OuterSyntax(this); } }

        public IJoinTableSyntax Join(string tableName)
        {
            return new JoinTableSyntax(this, tableName);
        }

        public IJoinTableSyntax Join(string tableName, string tableAlias)
        {
            return new JoinTableSyntax(this, tableName, tableAlias);
        }

        public override string Represent()
        {
            return "FULL";
        }
    }
}
