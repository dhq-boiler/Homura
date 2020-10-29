using Homura.QueryBuilder.Iso.Dml.Syntaxes;

namespace Homura.QueryBuilder.Iso.Dml.Transitions
{
    public interface ISetOperatorTransition
    {
        IUnionSyntax Union { get; }

        IExceptSyntax Except { get; }

        IIntersectSyntax Intersect { get; }
    }
}
