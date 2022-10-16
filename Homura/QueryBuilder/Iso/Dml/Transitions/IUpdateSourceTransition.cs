namespace Homura.QueryBuilder.Iso.Dml.Transitions
{
    public interface IUpdateSourceTransition<Return>
    {
        Return Value(object value);

        Return Expression(string expression);

        Return Replace(string expression, string pattern, string replacement);

        Return ReplaceColumn(string columnName, string pattern, string replacement);

        Return ReplaceColumn(string tableAlias, string columnName, string pattern, string replacement);

        Return Null { get; }

        Return Default { get; }
    }
}
