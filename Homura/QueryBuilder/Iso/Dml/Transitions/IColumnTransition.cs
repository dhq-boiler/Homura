using System.Collections.Generic;

namespace Homura.QueryBuilder.Iso.Dml.Transitions
{
    public interface IColumnTransition<Return> where Return : class
    {
        Return Column(string name);

        Return Column(string tableAlias, string name);

        Return Columns(IEnumerable<string> names);

        Return Columns(params string[] names);

        Return Replace(string expression, string pattern, string replacement);

        Return ReplaceColumn(string columnName, string pattern, string replacement);

        Return ReplaceColumn(string tableAlias, string columnName, string pattern, string replacement);
    }
}
