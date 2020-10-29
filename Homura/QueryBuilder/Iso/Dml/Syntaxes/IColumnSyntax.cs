using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Transitions;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    public interface IColumnSyntax : ISyntaxBase, IColumnTransition<IColumnSyntax>, IAsteriskTransition<IColumnSyntax>, ISubQueryTransition<IColumnSyntax>, IFromTransition<IFromSyntax<ICloseSyntax<IConditionValueSyntax>>>, IAsTransition
    {
    }
}
