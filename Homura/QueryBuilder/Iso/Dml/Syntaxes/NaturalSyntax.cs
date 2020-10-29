

using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class NaturalSyntax : SyntaxBase, INaturalSyntax
    {
        internal NaturalSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        public IJoinTypeSyntax Inner { get { return new InnerSyntax(this); } }

        public IJoinTypeSyntax Left { get { return new LeftSyntax(this); } }

        public IJoinTypeSyntax Right { get { return new RightSyntax(this); } }

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
            return "NATURAL";
        }
    }
}
