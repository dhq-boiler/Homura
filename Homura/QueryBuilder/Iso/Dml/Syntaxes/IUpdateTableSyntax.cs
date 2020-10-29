using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    public interface IUpdateTableSyntax : ISyntaxBase
    {
        ISetSyntax Set { get; }
    }
}
