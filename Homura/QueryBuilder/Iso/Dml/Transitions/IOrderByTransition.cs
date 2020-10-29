using Homura.QueryBuilder.Iso.Dml.Syntaxes;

namespace Homura.QueryBuilder.Iso.Dml.Transitions
{
    public interface IOrderByTransition
    {
        IOrderBySyntax OrderBy { get; }
    }
}
