using Homura.QueryBuilder.Iso.Dml.Syntaxes;

namespace Homura.QueryBuilder.Iso.Dml.Transitions
{
    public interface ISubQueryTransition<Return> where Return : class
    {

        Return SubQuery(ICloseSyntax<IConditionValueSyntax> subquery);

        Return SubQuery(IConditionValueSyntax subquery);

        Return SubQuery(IJoinConditionSyntax subquery);

        Return SubQuery(IOrderBySyntax subquery);

        Return SubQuery(IOrderByColumnSyntax subquery);
    }
}
