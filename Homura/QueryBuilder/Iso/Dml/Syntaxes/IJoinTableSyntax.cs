
using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    public interface IJoinTableSyntax : ISyntaxBase
    {
        IJoinConditionSyntax Using(string[] columnNames);

        IOnSyntax On { get; }
    }
}
