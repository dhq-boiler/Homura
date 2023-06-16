using Homura.ORM;
using Homura.Test.TestFixture.Dao;
using Homura.Test.TestFixture.Entity;
using NUnit.Framework;
using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
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
            record.Item1.Value = Guid.Parse("D10F1D49-1C4B-437E-B293-21599C516ABC");
            record.Item2.Value = false;
            record.Item3.Value = false;
            await dao.InsertAsync(record);

            var records = await dao.FindAllAsync().ToListAsync();
            Assert.That(records, Has.Count.EqualTo(1));
            Assert.That(records[0], Has.Property("Id").Property("Value").EqualTo(Guid.Parse("C10F1D49-1C4B-437E-B293-21599C516ABC")));
            Assert.That(records[0], Has.Property("Item1").Property("Value").EqualTo(Guid.Parse("D10F1D49-1C4B-437E-B293-21599C516ABC")));
            Assert.That(records[0], Has.Property("Item2").Property("Value").EqualTo(false));
            Assert.That(records[0], Has.Property("Item3").Property("Value").EqualTo(false));

            record.Id.Value = Guid.Parse("C10F1D49-1C4B-437E-B293-21599C516ABC");
            record.Item1.Value = Guid.Parse("E10F1D49-1C4B-437E-B293-21599C516ABC");
            record.Item2.Value = true;
            record.Item3.Value = true;
            var updateCount = await dao.UpdateAsync(record);

            records = await dao.FindAllAsync().ToListAsync();
            Assert.That(records, Has.Count.EqualTo(1));
            Assert.That(records[0], Has.Property("Id").Property("Value").EqualTo(Guid.Parse("C10F1D49-1C4B-437E-B293-21599C516ABC")));
            Assert.That(records[0], Has.Property("Item1").Property("Value").EqualTo(Guid.Parse("E10F1D49-1C4B-437E-B293-21599C516ABC")));
            Assert.That(records[0], Has.Property("Item2").Property("Value").EqualTo(true));
            Assert.That(records[0], Has.Property("Item3").Property("Value").EqualTo(true));
        }

        [TearDown]
        public void TearDown()
        {
            ConnectionManager.DisposeDebris(Guid.Parse("C10F1D49-1C4B-437E-B293-21599C516ABC"));
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }
    }
}
