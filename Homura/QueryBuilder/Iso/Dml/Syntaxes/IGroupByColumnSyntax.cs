using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Transitions;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    public interface IGroupByColumnSyntax : ISyntaxBase, IColumnTransition<IGroupByColumnSyntax>, IWhereTransition<IConditionValueSyntax>, IOrderByTransition, INaturalTransition, IJoinTypeTransition, IOuterJoinTypeTransition, ISql
    {
    }
}
