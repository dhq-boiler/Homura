using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class OpenFunctionSyntax : SyntaxBase, INoMarginLeftSyntax
    {
        internal OpenFunctionSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        public override string Represent()
        {
            return "(";
        }
    }
}
