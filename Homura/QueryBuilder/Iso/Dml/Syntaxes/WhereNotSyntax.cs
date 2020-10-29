

using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class WhereNotSyntax<R> : SyntaxBase, IWhereNotSyntax<R> where R : class
    {
        internal WhereNotSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        public IExistsSyntax<R> Exists { get { return new ExistsSyntax<R>(this); } }

        public override string Represent()
        {
            return "NOT";
        }
    }
}
