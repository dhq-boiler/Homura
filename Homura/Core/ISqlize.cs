
using System.Collections.Generic;

namespace Homura.Core
{
    public interface ISqlize
    {
        string ToSql();

        string ToSql(Dictionary<string, int> placeholderNameDictionary);
    }
}
