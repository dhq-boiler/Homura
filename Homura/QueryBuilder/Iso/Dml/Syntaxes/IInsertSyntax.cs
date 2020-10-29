
using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    public interface IInsertSyntax : ISyntaxBase
    {
        IIntoSyntax Into { get; }
    }
}
