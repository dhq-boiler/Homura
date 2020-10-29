
using Homura.QueryBuilder.Core;
using System.Collections.Generic;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    public interface IInValueSyntax : ISyntaxBase, ICloseSyntax<IConditionValueSyntax>
    {
        IInValueSyntax Value(object value);

        IInValueSyntax Value(params object[] values);

        IInValueSyntax Value(IEnumerable<object> values);
    }
}
