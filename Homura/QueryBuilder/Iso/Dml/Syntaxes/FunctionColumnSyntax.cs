using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class FunctionColumnSyntax : SyntaxBase, INoMarginLeftSyntax
    {
        private string _name;

        internal FunctionColumnSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        internal FunctionColumnSyntax(SyntaxBase syntaxBase, string name)
            : base(syntaxBase)
        {
            _name = name;
        }

        public override string Represent()
        {
            return $"{_name}";
        }
    }
}
