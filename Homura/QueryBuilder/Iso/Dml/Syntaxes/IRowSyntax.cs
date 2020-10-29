

using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Transitions;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    public interface IRowSyntax : ISyntaxBase, IRowTransition, ISql
    { }
}
