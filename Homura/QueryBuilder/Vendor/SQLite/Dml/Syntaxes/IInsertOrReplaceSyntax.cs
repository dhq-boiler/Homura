using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Syntaxes;

namespace Homura.QueryBuilder.Vendor.SQLite.Dml.Syntaxes
{
    public interface IInsertOrReplaceSyntax : ISyntaxBase
    {
        IIntoSyntax Into { get; }
    }
}