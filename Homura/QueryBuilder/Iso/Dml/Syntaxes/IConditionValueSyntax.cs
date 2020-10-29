

using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Transitions;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    public interface IConditionValueSyntax : ISyntaxBase, IOrderByTransition, INaturalTransition, IJoinTypeTransition, IOuterJoinTypeTransition, IGroupByTransition, ISql
    { }
}
