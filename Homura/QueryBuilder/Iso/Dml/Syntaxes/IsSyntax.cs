

using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class IsSyntax<R> : SyntaxBase, IIsSyntax<R> where R : class
    {
        internal IsSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        public R NotNull { get { return new NotNullSyntax(this) as R; } }

        public R Null { get { return new NullSyntax(this) as R; } }

        public override string Represent()
        {
            return "IS";
        }
    }
}
