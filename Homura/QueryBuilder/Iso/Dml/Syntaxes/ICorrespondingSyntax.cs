using Homura.QueryBuilder.Iso.Dml.Transitions;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    public interface ICorrespondingSyntax : IQueryTermTransition
    {
        IBySyntax By { get; }
    }
}
