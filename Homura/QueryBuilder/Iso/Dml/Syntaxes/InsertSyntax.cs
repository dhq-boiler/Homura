using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class InsertSyntax : SyntaxBase, IInsertSyntax
    {
        internal InsertSyntax()
            : base()
        { }

        internal InsertSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        public IIntoSyntax Into { get { return new IntoSyntax(this); } }

        public override string Represent()
        {
            return "INSERT";
        }
    }
}
