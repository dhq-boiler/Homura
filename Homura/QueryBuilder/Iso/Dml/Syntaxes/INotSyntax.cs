
using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    public interface INotSyntax : ISyntaxBase
    {
        IInSyntax In { get; }
    }
}
