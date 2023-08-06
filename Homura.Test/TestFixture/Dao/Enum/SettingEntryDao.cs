using Homura.ORM;
using Homura.Test.UnitTest.Enum;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Homura.Test.TestFixture.Dao.Enum
{
    public class SettingEntryDao : Dao<SettingEntry>
    {
        public SettingEntryDao() : base()
        {
        }

        public SettingEntryDao(Type entityVersionType) : base(entityVersionType)
        {
        }

        public new IEnumerable<SettingEntry> FindAll(
            DbConnection conn = null,
            string anotherDatabaseAliasName = null,
            TimeSpan? timeout = null)
        {
            var entries = base.FindAll(conn, anotherDatabaseAliasName, timeout);

            foreach (var entry in entries)
            {
                if (entry.Type.Value.IsEnum)
                {
                    yield return entry.SugarCoatingEnum();
                }
            }
        }

        public new async IAsyncEnumerable<SettingEntry> FindAllAsync(
            DbConnection conn = null,
            string anotherDatabaseAliasName = null,
            TimeSpan? timeout = null)
        {
            var entries = base.FindAllAsync(conn, anotherDatabaseAliasName, timeout);

            await foreach (var entry in entries)
            {
                if (entry.Type.Value.IsEnum)
                {
                    yield return entry.SugarCoatingEnum();
                }
            }
        }
    }
}
