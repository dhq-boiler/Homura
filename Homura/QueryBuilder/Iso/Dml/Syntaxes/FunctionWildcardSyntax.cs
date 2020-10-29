using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class FunctionAsteriskSyntax : SyntaxBase, INoMarginLeftSyntax
    {
        internal FunctionAsteriskSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        public override string Represent()
        {
            return "*";
        }
    }
}
