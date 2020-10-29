using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Transitions;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    public interface INaturalSyntax : ISyntaxBase, IJoinTableTransition<IJoinTableSyntax>
    { }
}
