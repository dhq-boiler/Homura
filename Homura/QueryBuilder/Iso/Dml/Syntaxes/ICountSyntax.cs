using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Transitions;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    public interface ICountSyntax : ISyntaxBase, IColumnTransition<IColumnSyntax>, IFromTransition<IFromSyntax<ICloseSyntax<IConditionValueSyntax>>>, IAsTransition
    { }
}
