using Homura.QueryBuilder.Iso.Dml.Syntaxes;

namespace Homura.QueryBuilder.Iso.Dml.Transitions
{
    public interface IOuterJoinTypeTransition
    {
        IOuterJoinTypeSyntax Left { get; }

        IOuterJoinTypeSyntax Right { get; }

        IOuterJoinTypeSyntax Full { get; }
    }
}
