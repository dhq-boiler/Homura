using Homura.QueryBuilder.Iso.Dml.Transitions;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    public interface IOrderTypeSyntax : IOrderBySyntax, IWhereTransition<IConditionValueSyntax>, INaturalTransition, IJoinTypeTransition, IOuterJoinTypeTransition
    {
    }
}
