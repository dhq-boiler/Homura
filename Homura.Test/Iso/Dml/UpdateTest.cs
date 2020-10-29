

using NUnit.Framework;
using Homura.QueryBuilder.Iso.Dml;
using System.Collections.Generic;

namespace Homura.QueryBuilder.Test.Iso.Dml
{
    [TestFixture]
    public class UpdateTest
    {
        [Category("Homura.QueryBuilder QueryBuilder")]
        public class BasicTest
        {
            [Test]
            public void Update_Table_Set_Column1_Equal_Value1()
            {
                using (var query = new Update().Table("Table").Set.Column("Column1").EqualTo.Value("Value1"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("UPDATE Table SET Column1 = @val_0"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "Value1")));
                }
            }

            [Test]
            public void Update_aTable_Set_Column1_Equal_Value1()
            {
                using (var query = new Update().Table("Table", "a").Set.Column("a", "Column1").EqualTo.Value("Value1"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("UPDATE Table a SET a.Column1 = @val_0"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "Value1")));
                }
            }

            [Test]
            public void Update_Table_Set_Column1_Equal_Value1_Column2_Equal_Value2_Column3_Equal_Value3()
            {
                using (var query = new Update().Table("Table").Set
                                               .Column("Column1").EqualTo.Value("Value1")
                                               .Column("Column2").EqualTo.Value("Value2")
                                               .Column("Column3").EqualTo.Value("Value3"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("UPDATE Table SET Column1 = @val_0, Column2 = @val_1, Column3 = @val_2"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "Value1")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", "Value2")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_2", "Value3")));
                }
            }

            [Test]
            public void Update_Table_Set_KeyEqualValue()
            {
                using (var query = new Update().Table("Table").Set.KeyEqualToValue(new Dictionary<string, object>() { { "Column1", "Value1" }, { "Column2", "Value2" }, { "Column3", "Value3" } }))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("UPDATE Table SET Column1 = @val_0, Column2 = @val_1, Column3 = @val_2"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "Value1")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", "Value2")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_2", "Value3")));
                }
            }

            [Test]
            public void Update_Table_Set_Column1_Equal_Value1_KeyEqualValue()
            {
                using (var query = new Update().Table("Table").Set.Column("Column1").EqualTo.Value("Value1")
                                               .KeyEqualToValue(new Dictionary<string, object>() { { "Column2", "Value2" }, { "Column3", "Value3" } }))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("UPDATE Table SET Column1 = @val_0, Column2 = @val_1, Column3 = @val_2"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "Value1")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", "Value2")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_2", "Value3")));
                }
            }

            [Test]
            public void Update_Test_Set_Column1_EqualTo_Null()
            {
                using (var query = new Update().Table("Table").Set.Column("Column1").EqualTo.Null)
                {
                    Assert.That(query.ToSql(), Is.EqualTo("UPDATE Table SET Column1 = NULL"));
                    Assert.That(query, Has.Property("Parameters").Empty);
                }
            }

            [Test]
            public void Update_Test_Set_Column1_EqualTo_Default()
            {
                using (var query = new Update().Table("Table").Set.Column("Column1").EqualTo.Default)
                {
                    Assert.That(query.ToSql(), Is.EqualTo("UPDATE Table SET Column1 = DEFAULT"));
                    Assert.That(query, Has.Property("Parameters").Empty);
                }
            }

            [Test]
            public void Update_Table_Set_Column1_EqualTo_Column1_Plus_1()
            {
                using (var query = new Update().Table("Table").Set.Column("Column1").EqualTo.Expression("Column1 + 1"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("UPDATE Table SET Column1 = Column1 + 1"));
                    Assert.That(query, Has.Property("Parameters").Empty);
                }
            }
        }

        [Category("Homura.QueryBuilder QueryBuilder")]
        public class WhereTest
        {
            [Test]
            public void Update_Table_Set_Column1_Equal_Value1_Where_Column1_EqualTo_1()
            {
                using (var query = new Update().Table("Table").Set.Column("Column1").EqualTo.Value("Value1").Where.Column("Column1").EqualTo.Value(1))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("UPDATE Table SET Column1 = @val_0 WHERE Column1 = @val_1"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "Value1")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", 1)));
                }
            }

            [Test]
            public void Update_Table_Set_Column1_Equal_Value1_Where_Column1_EqualTo_1_And_Column2_NotEqualTo_2()
            {
                using (var query = new Update().Table("Table").Set.Column("Column1").EqualTo.Value("Value1").Where.Column("Column1").EqualTo.Value(1)
                                               .And().Column("Column2").NotEqualTo.Value(2))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("UPDATE Table SET Column1 = @val_0 WHERE Column1 = @val_1 AND Column2 <> @val_2"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "Value1")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", 1)));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_2", 2)));
                }
            }

            [Test]
            public void Update_Table_Set_Column1_Equal_Value1_Where_Column1_EqualTo_String1()
            {
                using (var query = new Update().Table("Table").Set.Column("Column1").EqualTo.Value("Value1").Where.Column("Column1").EqualTo.Value("String1"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("UPDATE Table SET Column1 = @val_0 WHERE Column1 = @val_1"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "Value1")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", "String1")));
                }
            }

            [Test]
            public void Update_Table_Set_Column1_Equal_Value1_Where_Column1_GreaterThan_1()
            {
                using (var query = new Update().Table("Table").Set.Column("Column1").EqualTo.Value("Value1").Where.Column("Column1").GreaterThan.Value(1))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("UPDATE Table SET Column1 = @val_0 WHERE Column1 > @val_1"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "Value1")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", 1)));
                }
            }

            [Test]
            public void Update_Table_Set_Column1_Equal_Value1_Where_Column1_LessThan_1()
            {
                using (var query = new Update().Table("Table").Set.Column("Column1").EqualTo.Value("Value1").Where.Column("Column1").LessThan.Value(1))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("UPDATE Table SET Column1 = @val_0 WHERE Column1 < @val_1"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "Value1")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", 1)));
                }
            }

            [Test]
            public void Update_Table_Set_Column1_Equal_Value1_Where_Column1_GreaterThanOrEqualTo_1()
            {
                using (var query = new Update().Table("Table").Set.Column("Column1").EqualTo.Value("Value1").Where.Column("Column1").GreaterThanOrEqualTo.Value(1))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("UPDATE Table SET Column1 = @val_0 WHERE Column1 >= @val_1"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "Value1")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", 1)));
                }
            }

            [Test]
            public void Update_Table_Set_Column1_Equal_Value1_Where_Column1_LessThanOrEqualTo_1()
            {
                using (var query = new Update().Table("Table").Set.Column("Column1").EqualTo.Value("Value1").Where.Column("Column1").LessThanOrEqualTo.Value(1))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("UPDATE Table SET Column1 = @val_0 WHERE Column1 <= @val_1"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "Value1")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", 1)));
                }
            }

            [Test]
            public void Update_Table_Set_Column1_Equal_Value1_Where_Column1_NotEqualTo_1()
            {
                using (var query = new Update().Table("Table").Set.Column("Column1").EqualTo.Value("Value1").Where.Column("Column1").NotEqualTo.Value(1))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("UPDATE Table SET Column1 = @val_0 WHERE Column1 <> @val_1"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "Value1")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", 1)));
                }
            }

            [Test]
            public void Update_Table_Set_Column1_Equal_Value1_Where_Column1_IsNULL()
            {
                using (var query = new Update().Table("Table").Set.Column("Column1").EqualTo.Value("Value1").Where.Column("Column1").Is.Null)
                {
                    Assert.That(query.ToSql(), Is.EqualTo("UPDATE Table SET Column1 = @val_0 WHERE Column1 IS NULL"));
                }
            }

            [Test]
            public void Update_Table_Set_Column1_Equal_Value1_Where_Column1_IsNotNULL()
            {
                using (var query = new Update().Table("Table").Set.Column("Column1").EqualTo.Value("Value1").Where.Column("Column1").Is.NotNull)
                {
                    Assert.That(query.ToSql(), Is.EqualTo("UPDATE Table SET Column1 = @val_0 WHERE Column1 IS NOT NULL"));
                }
            }

            [Test]
            public void Update_Table_Set_Column1_Equal_Value1_Where_Column1_Like_String1()
            {
                using (var query = new Update().Table("Table").Set.Column("Column1").EqualTo.Value("Value1").Where.Column("Column1").Like.Value("%String1%"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("UPDATE Table SET Column1 = @val_0 WHERE Column1 LIKE @val_1"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "Value1")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", "%String1%")));
                }
            }

            [Test]
            public void Update_Table_Set_KeyEqualValue_Where_KeyEqualValue()
            {
                using (var query = new Update().Table("Table").Set.KeyEqualToValue(new Dictionary<string, object>() { { "Column1", "Value1" }, { "Column2", "Value2" }, { "Column3", "Value3" } })
                                               .Where.KeyEqualToValue(new Dictionary<string, object>() { { "Column4", "Value4" }, { "Column5", "Value5" }, { "Column6", "Value6" } }))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("UPDATE Table SET Column1 = @val_0, Column2 = @val_1, Column3 = @val_2 WHERE Column4 = @val_3 AND Column5 = @val_4 AND Column6 = @val_5"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "Value1")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", "Value2")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_2", "Value3")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_3", "Value4")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_4", "Value5")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_5", "Value6")));
                }
            }
        }
    }
}
