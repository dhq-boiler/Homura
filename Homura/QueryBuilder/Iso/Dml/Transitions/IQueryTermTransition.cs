using Homura.QueryBuilder.Iso.Dml.Syntaxes;

namespace Homura.QueryBuilder.Iso.Dml.Transitions
{
    public interface IQueryTermTransition : ICrossJoinTransition, IJoinTypeTransition, IOuterJoinTypeTransition, INaturalTransition
    {
        ISelectSyntax Select { get; }
    }
}
