using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Transitions
{
    public interface ITableTransition<Return> where Return : class
    {
        Return Table(string name);

        Return Table(string name, string alias);

        Return Table(string schemaName, string name, string alias);

        Return Table(string catalogName, string schemaName, string name, string alias);

        Return Table(Table table);
    }
}
