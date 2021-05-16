using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class CrossSyntax : SyntaxBase, ICrossSyntax
    {
        internal CrossSyntax(SyntaxBase syntax)
            : base(syntax)
        { }

        public ICrossJoinSyntax Join(string tableName)
        {
            return new JoinTableSyntax(this, tableName);
        }

        public ICrossJoinSyntax Join(string tableName, string tableAlias)
        {
            return new JoinTableSyntax(this, tableName, tableAlias);
        }

        public override string Represent()
        {
            return "CROSS";
        }
    }
}
