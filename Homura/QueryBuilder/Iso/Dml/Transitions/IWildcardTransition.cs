namespace Homura.QueryBuilder.Iso.Dml.Transitions
{
    public interface IAsteriskTransition<Return> where Return : class
    {

        Return Asterisk();

        Return Asterisk(string tableAlias);
    }
}
