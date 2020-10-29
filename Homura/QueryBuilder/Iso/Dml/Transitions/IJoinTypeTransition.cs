using Homura.QueryBuilder.Iso.Dml.Syntaxes;

namespace Homura.QueryBuilder.Iso.Dml.Transitions
{
    public interface IJoinTypeTransition
    {
        IJoinTypeSyntax Inner { get; }

        IUnionSyntax Union { get; }
    }
}
