using Homura.QueryBuilder.Iso.Dml.Syntaxes;
using System.Collections.Generic;

namespace Homura.QueryBuilder.Iso.Dml.Transitions
{
    public interface IValueTransition
    {
        ISetClauseValueSyntax Value(object value);

        ISetClauseValueSyntax Value(IEnumerable<object> values);

        ISetClauseValueSyntax Value(params object[] values);
    }
}
