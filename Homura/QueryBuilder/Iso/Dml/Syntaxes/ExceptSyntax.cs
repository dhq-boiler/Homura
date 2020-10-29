using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Transitions;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class ExceptSyntax : SyntaxBase, IExceptSyntax
    {
        internal ExceptSyntax(SyntaxBase syntax)
            : base(syntax)
        { }

        public ICorrespondingTransition All { get { return new AllSyntax(this); } }

        public ICorrespondingSyntax Corresponding { get { return new CorrespondingSyntax(this); } }

        public ICrossSyntax Cross { get { return new CrossSyntax(this); } }

        public IOuterJoinTypeSyntax Full { get { return new FullSyntax(this); } }

        public IJoinTypeSyntax Inner { get { return new InnerSyntax(this); } }

        public IOuterJoinTypeSyntax Left { get { return new LeftSyntax(this); } }

        public INaturalSyntax Natural { get { return new NaturalSyntax(this); } }

        public IOuterJoinTypeSyntax Right { get { return new RightSyntax(this); } }

        public ISelectSyntax Select { get { return new SelectSyntax(this); } }

        public IUnionSyntax Union { get { return new UnionSyntax(this); } }

        public override string Represent()
        {
            return "EXCEPT";
        }
    }
}
