
using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Transitions;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    public interface IFromSyntax<Return> : ISyntaxBase, ITableTransition<Return>, ISubQueryTransition<Return> where Return : class
    {
    }
}
