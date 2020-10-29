using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class OpenParenthesisSyntax : SyntaxBase, INoMarginRightSyntax
    {
        internal OpenParenthesisSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        public override string Represent()
        {
            return "(";
        }
    }
}
