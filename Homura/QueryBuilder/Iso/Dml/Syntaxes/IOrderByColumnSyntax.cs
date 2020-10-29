
using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Transitions;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    public interface IOrderByColumnSyntax : IOrderBySyntax, IWhereTransition<IConditionValueSyntax>, INaturalTransition, IJoinTypeTransition, IOuterJoinTypeTransition, ISql
    {
        IOrderTypeSyntax Asc { get; }

        IOrderTypeSyntax Desc { get; }
    }
}
