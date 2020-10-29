using Homura.QueryBuilder.Iso.Dml.Syntaxes;

namespace Homura.QueryBuilder.Iso.Dml.Transitions
{
    public interface IQuantityTransition
    {
        ISetQuantifierSyntax Distinct { get; }

        IColumnTransition<IColumnSyntax> All { get; }
    }
}
