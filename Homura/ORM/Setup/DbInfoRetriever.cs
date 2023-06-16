

using System.Collections.Generic;

namespace Homura.ORM.Setup
{
    internal class DbInfoRetriever
    {
        public static async IAsyncEnumerable<string> GetTableNames(IConnection connection)
        {
            using (var conn = connection.OpenConnection())
            {
                foreach (var item in await conn.GetTableNames())
                {
                    yield return item;
                }
            }
        }
    }
}
