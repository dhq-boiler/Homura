
using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    public interface IIsSyntax<R> : ISyntaxBase where R : class
    {
        R Null { get; }

        R NotNull { get; }
    }
}
