

using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class NotSyntax : SyntaxBase, INotSyntax
    {
        internal NotSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        public IInSyntax In { get { return new InSyntax(this); } }

        public override string Represent()
        {
            return "NOT";
        }
    }
}
