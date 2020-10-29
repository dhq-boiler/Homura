
using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Transitions;
using System.Collections.Generic;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    public interface IInSyntax : ISyntaxBase, ISubQueryTransition<ICloseSyntax<IConditionValueSyntax>>
    {
        IInValueSyntax Value(object value);

        IInValueSyntax Value(params object[] values);

        IInValueSyntax Array(IEnumerable<object> values);
    }
}
