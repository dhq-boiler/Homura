namespace Homura.QueryBuilder.Iso.Dml.Transitions
{
    public interface IJoinTableTransition<Return> where Return : class
    {
        Return Join(string tableName);

        Return Join(string tableName, string tableAlias);
    }
}
