using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class OuterSyntax : SyntaxBase, IJoinTypeSyntax
    {
        internal OuterSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        public IJoinTableSyntax Join(string tableName)
        {
            return new JoinTableSyntax(this, tableName);
        }

        public IJoinTableSyntax Join(string tableName, string tableAlias)
        {
            return new JoinTableSyntax(this, tableName);
        }

        public override string Represent()
        {
            return "OUTER";
        }
    }
}
