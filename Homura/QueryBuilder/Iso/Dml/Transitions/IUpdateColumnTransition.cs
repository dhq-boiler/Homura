using Homura.QueryBuilder.Iso.Dml.Syntaxes;
using System.Collections.Generic;

namespace Homura.QueryBuilder.Iso.Dml.Transitions
{
    public interface IUpdateColumnTransition : IColumnTransition<ISetClauseSyntax>
    {
        IValueExpressionSyntax KeyEqualToValue(Dictionary<string, object> columnValues);
    }
}
