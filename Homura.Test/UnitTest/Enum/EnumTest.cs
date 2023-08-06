using Homura.ORM;
using Homura.Test.TestFixture.Dao.Enum;
using NUnit.Framework;
using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Homura.Test.UnitTest.Enum
{
    [TestFixture]
    internal class EnumTest
    {
        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            var _filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "EnumTest.db");
            ConnectionManager.SetDefaultConnection(Guid.Parse("A9690DBC-95AA-4AB6-986D-778695FE35DC"),
                $"Data Source={_filePath}", typeof(SQLiteConnection));

            var dao = new SettingEntryDao();
            await dao.DropTableAsync();
            await dao.CreateTableIfNotExistsAsync();
        }

        [Test]
        public async Task TestEnum()
        {
            var dao = new SettingEntryDao();
            await dao.InsertAsync(new EnumSettingEntry<DayOfWeek>("8F42B0D5-F7EF-4BA6-99D8-F2F0575ED34B", "週の始まり", DayOfWeek.Sunday));
            await dao.InsertAsync(new EnumSettingEntry<Month>("3F60B869-F9AA-4EDF-BD63-7269549D72F4", "年度の始まり", Month.April));

            var entries = (await dao.FindAllAsync().ToListAsync()).ToArray();
            Assert.That(entries, Has.Length.EqualTo(2));
            Assert.That(entries[0].Title.Value, Is.EqualTo("週の始まり"));
            Assert.That(entries[1].Title.Value, Is.EqualTo("年度の始まり"));
        }
    }

    public enum Month
    {
        /// <summary>
        /// 1月
        /// </summary>
        January = 1,

        /// <summary>
        /// 2月
        /// </summary>
        February,

        /// <summary>
        /// 3月
        /// </summary>
        March,

        /// <summary>
        /// 4月
        /// </summary>
        April,

        /// <summary>
        /// 5月
        /// </summary>
        May,

        /// <summary>
        /// 6月
        /// </summary>
        June,

        /// <summary>
        /// 7月
        /// </summary>
        July,

        /// <summary>
        /// 8月
        /// </summary>
        August,

        /// <summary>
        /// 9月
        /// </summary>
        September,

        /// <summary>
        /// 10月
        /// </summary>
        October,

        /// <summary>
        /// 11月
        /// </summary>
        November,

        /// <summary>
        /// 12月
        /// </summary>
        December
    }
}
