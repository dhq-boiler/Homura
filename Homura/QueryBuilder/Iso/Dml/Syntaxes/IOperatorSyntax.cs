
using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Transitions;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    public interface IOperatorSyntax<R> : ISyntaxBase, ISubQueryTransition<R> where R : class
    {
        R Value(object value);
    }
}
