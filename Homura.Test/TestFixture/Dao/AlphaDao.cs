

using Homura.ORM;
using Homura.QueryBuilder.Iso.Dml;
using Homura.QueryBuilder.Vendor.SQLite.Dml;
using Homura.Test.TestFixture.Entity;
using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using static Homura.Extensions.Extensions;

namespace Homura.Test.TestFixture.Dao
{
    internal class AlphaDao : Dao<Alpha>
    {
        public AlphaDao()
            : base()
        { }

        public AlphaDao(Type entityVersionType)
            : base(entityVersionType)
        { }

        protected override Alpha ToEntity(IDataRecord reader)
        {
            return new Alpha()
            {
                Id = CatchThrow(() => reader.SafeGetGuid("Id", Table)),
                Item1 = CatchThrow(() => reader.SafeGetString("Item1", Table)),
                Item2 = CatchThrow(() => reader.SafeGetString("Item2", Table)),
                Item3 = CatchThrow(() => reader.SafeGetGuid("Item3", Table)),
                Item4 = CatchThrow(() => reader.SafeGetString("Item4", Table)),
                Item5 = CatchThrow(() => reader.SafeGetInt("Item5", Table)),
                Item6 = CatchThrow(() => reader.SafeGetLong("Item6", Table)),
                Item7 = CatchThrow(() => reader.SafeGetString("Item7", Table)),
                Item8 = CatchThrow(() => reader.SafeGetBoolean("Item8", Table)),
                Item9 = CatchThrow(() => reader.SafeGetBoolean("Item9", Table)),
            };
        }
        public async Task InsertOrReplaceAsync(Alpha entity, DbConnection conn = null)
        {
            InitializeColumnDefinitions();
            try
            {
                VerifyColumnDefinitions(conn);
            }
            catch (NotMatchColumnException e)
            {
                throw new DatabaseSchemaException($"Didn't insert because mismatch definition of table:{TableName}", e);
            }

            await QueryHelper.KeepTryingUntilProcessSucceedAsync<Task>(async () =>
            {
                await QueryHelper.ForDao.ConnectionInternalAsync(this, async (connection) =>
                {
                    try
                    {
                        using (var command = connection.CreateCommand())
                        {
                            var overrideColumns = SwapIfOverrided(Columns);

                            using (var query = new InsertOrReplace().Into.Table(new Table<Alpha>().Name)
                                                                    .Columns(overrideColumns.Select(c => c.ColumnName))
                                                                    .Values.Row(overrideColumns.Select(c => c.PropInfo.GetValue(entity))))
                            {
                                string sql = query.ToSql();
                                command.CommandText = sql;
                                query.SetParameters(command);

                                //s_logger.Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                                int inserted = await command.ExecuteNonQueryAsync();
                                if (inserted == 0)
                                {
                                    throw new NoEntityInsertedException($"Failed:{sql} {query.GetParameters().ToStringKeyIsValue()}");
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }, conn);
            });
        }
    }
}
