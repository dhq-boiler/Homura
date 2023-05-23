

using Homura.ORM;
using Homura.QueryBuilder.Iso.Dml;
using Homura.QueryBuilder.Vendor.SQLite.Dml;
using Homura.Test.TestFixture.Entity;
using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

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

            await QueryHelper.KeepTryingUntilProcessSucceedAsync(async () =>
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
                                                                    .Values.Row(overrideColumns.Select(c => c.PropertyGetter(entity))))
                            {
                                var sql = query.ToSql();
                                command.CommandText = sql;
                                query.SetParameters(command);

                                //s_logger.Debug($"{sql} {query.GetParameters().ToStringKeyIsValue()}");
                                var inserted = await command.ExecuteNonQueryAsync();
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
