

using System.Collections.Generic;

namespace Homura.ORM.Setup
{
    internal class DbInfoRetriever
    {
        public static IEnumerable<string> GetTableNames(IConnection connection)
        {
            using (var conn = connection.OpenConnection())
            {
                foreach (var item in conn.GetTableNames())
                {
                    yield return item;
                }
            }
        }
    }
}
