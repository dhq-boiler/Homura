namespace Homura.QueryBuilder.Iso.Dml.Transitions
{
    public interface IUpdateSourceTransition<Return>
    {
        Return Value(object value);

        Return Expression(string expression);

        Return Null { get; }

        Return Default { get; }
    }
}
