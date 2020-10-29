using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Transitions;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class DeleteSyntax : SyntaxBase, IDeleteSyntax
    {
        internal DeleteSyntax()
            : base()
        { }

        internal DeleteSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        public ITableTransition<IDeleteTableSyntax<ISinkStateSyntax>> From { get { return new FromSyntax<IDeleteTableSyntax<ISinkStateSyntax>, ISinkStateSyntax>(this); } }

        public override string Represent()
        {
            return "DELETE";
        }
    }
}
