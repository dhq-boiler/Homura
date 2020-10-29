
using System;

namespace Homura.ORM
{
    public static class ConnectionManager
    {
        public static IConnection DefaultConnection { get; set; }

        public static void SetDefaultConnection(string v, Type dbConnectionType)
        {
            DefaultConnection = new Connection(v, dbConnectionType);
        }
    }
}
