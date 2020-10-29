
using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Transitions;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    public interface ICloseSyntax<R> : ISyntaxBase, IWhereTransition<R>, INaturalTransition, IJoinTypeTransition, IOuterJoinTypeTransition, ICrossJoinTransition, IJoinTableTransition<IJoinTableSyntax>, IOrderByTransition, IGroupByTransition, ISql where R : class
    { }
}
