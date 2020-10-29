
using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    public interface IWhereNotSyntax<R> : ISyntaxBase where R : class
    {
        IExistsSyntax<R> Exists { get; }
    }
}
