using Homura.ORM;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Entity;
using NUnit.Framework;
using Sunctum.Domain.Data.Dao.Migration.Plan;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homura.Test.UnitTest
{
    [TestFixture]
    internal class ReactivePropertyTest
    {
        private string _filePath;

        [SetUp]
        public void Initialize()
        {
            _filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "ReactivePropertyTest.db");
            ConnectionManager.SetDefaultConnection(Guid.Parse("C10F1D49-1C4B-437E-B293-21599C516ABC"), $"Data Source={_filePath}", typeof(SQLiteConnection));
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }

        [Test]
        public async Task Test_RP()
        {
            var dao = new Valkyrie_3_Dao();
            dao.CurrentConnection = ConnectionManager.DefaultConnection;

            dao.CreateTableIfNotExists();

            var record = new Valkyrie_3();
            record.Id.Value = Guid.Parse("C10F1D49-1C4B-437E-B293-21599C516ABC");
            await dao.InsertAsync(record);

            var records = await dao.FindAllAsync().ToListAsync();
            Assert.That(records, Has.Count.EqualTo(1));
            Assert.That(records[0], Has.Property("Id").Property("Value").EqualTo(Guid.Parse("C10F1D49-1C4B-437E-B293-21599C516ABC")));
        }
    }
}
