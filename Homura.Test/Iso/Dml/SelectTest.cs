

using NUnit.Framework;
using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml;
using System.Collections.Generic;

namespace Homura.QueryBuilder.Test.Iso.Dml
{
    [TestFixture]
    public class SelectTest
    {
        [Category("Homura.QueryBuilder QueryBuilder")]
        public class BasicTest
        {
            [Test]
            public void Select_Asterisk_From_Table()
            {
                using (var query = new Select().Asterisk().From.Table("Table"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT * FROM Table"));
                }
            }

            [Test]
            public void Select_Column1_From_Table()
            {
                using (var query = new Select().Column("Column1").From.Table("Table"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Table"));
                }
            }

            [Test]
            public void Select_Column1_Column2_Column3_From_Table()
            {
                using (var query = new Select().Column("Column1").Column("Column2").Column("Column3").From.Table("Table"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1, Column2, Column3 FROM Table"));
                }
            }

            [Test]
            public void Select_Distinct_Column1_From_Table()
            {
                using (var query = new Select().Distinct.Column("Column1").From.Table("Table"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT DISTINCT Column1 FROM Table"));
                }
            }

            [Test]
            public void Select_All_Column1_From_Table()
            {
                using (var query = new Select().All.Column("Column1").From.Table("Table"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT ALL Column1 FROM Table"));
                }
            }

            [Test]
            public void Select_aColumn1_From_Table_a()
            {
                using (var query = new Select().Column("a", "Column1").From.Table("Table", "a"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT a.Column1 FROM Table a"));
                }
            }

            [Test]
            public void Select_Column1_As_Alpha_From_Table()
            {
                using (var query = new Select().Column("Column1").As("Alpha").From.Table("Table"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 AS 'Alpha' FROM Table"));
                }
            }

            [Test]
            public void Select_Column1_aAsterisk_From_Table_a()
            {
                using (var query = new Select().Column("Column1").Asterisk("a").From.Table("Table", "a"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1, a.* FROM Table a"));
                }
            }

            [Test]
            public void Select_COUNT__Asterisk__From_Table()
            {
                using (var query = new Select().Count().From.Table("Table"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT COUNT(*) FROM Table"));
                }
            }

            [Test]
            public void Select_COUNT__Column1__From_Table()
            {
                using (var query = new Select().Count("Column1").From.Table("Table"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT COUNT(Column1) FROM Table"));
                }
            }

            [Test]
            public void Select_COUNT__Distinct_Column1__From_Table()
            {
                using (var query = new Select().Count(Distinct.Column("Column1")).From.Table("Table"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT COUNT(DISTINCT Column1) FROM Table"));
                }
            }

            [Test]
            public void Select_COUNT__All_Column1__From_Table()
            {
                using (var query = new Select().Count(All.Column("Column1")).From.Table("Table"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT COUNT(ALL Column1) FROM Table"));
                }
            }
        }

        [Category("Homura.QueryBuilder QueryBuilder")]
        public class WhereTest
        {
            [Test]
            public void Select_Column1_From_Table_Where_Column1_EqualTo_1()
            {
                using (var query = new Select().Column("Column1").From.Table("Table").Where.Column("Column1").EqualTo.Value(1))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Table WHERE Column1 = @val_0"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", 1)));
                }
            }

            [Test]
            public void Select_Column1_Column2_From_Table_Where_Column1_EqualTo_1_And_Column2_NotEqualTo_2()
            {
                using (var query = new Select().Column("Column1").Column("Column2").From.Table("Table")
                                               .Where.Column("Column1").EqualTo.Value(1)
                                               .And().Column("Column2").NotEqualTo.Value(2))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1, Column2 FROM Table WHERE Column1 = @val_0 AND Column2 <> @val_1"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", 1)));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", 2)));
                }
            }

            [Test]
            public void Select_Column1_Column2_From_Table_Where_Column1_EqualTo_1_Or_Column2_NotEqualTo_2()
            {
                using (var query = new Select().Column("Column1").Column("Column2").From.Table("Table")
                                               .Where.Column("Column1").EqualTo.Value(1)
                                               .Or().Column("Column2").NotEqualTo.Value(2))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1, Column2 FROM Table WHERE Column1 = @val_0 OR Column2 <> @val_1"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", 1)));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", 2)));
                }
            }

            [Test]
            public void Select_Column1_From_Table_Where_Column1_EqualTo_String1()
            {
                using (var query = new Select().Column("Column1").From.Table("Table").Where.Column("Column1").EqualTo.Value("String1"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Table WHERE Column1 = @val_0"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "String1")));
                }
            }

            [Test]
            public void Select_Column1_From_Table_Where_Column1_GreaterThan_String1()
            {
                using (var query = new Select().Column("Column1").From.Table("Table").Where.Column("Column1").GreaterThan.Value(1))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Table WHERE Column1 > @val_0"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", 1)));
                }
            }

            [Test]
            public void Select_Column1_From_Table_Where_Column1_LessThan_String1()
            {
                using (var query = new Select().Column("Column1").From.Table("Table").Where.Column("Column1").LessThan.Value(1))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Table WHERE Column1 < @val_0"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", 1)));
                }
            }

            [Test]
            public void Select_Column1_From_Table_Where_Column1_GreaterThanOrEqualTo_String1()
            {
                using (var query = new Select().Column("Column1").From.Table("Table").Where.Column("Column1").GreaterThanOrEqualTo.Value(1))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Table WHERE Column1 >= @val_0"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", 1)));
                }
            }

            [Test]
            public void Select_Column1_From_Table_Where_Column1_LessThanOrEqualTo_String1()
            {
                using (var query = new Select().Column("Column1").From.Table("Table").Where.Column("Column1").LessThanOrEqualTo.Value(1))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Table WHERE Column1 <= @val_0"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", 1)));
                }
            }

            [Test]
            public void Select_Column1_From_Table_Where_Column1_NotEqualTo_String1()
            {
                using (var query = new Select().Column("Column1").From.Table("Table").Where.Column("Column1").NotEqualTo.Value(1))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Table WHERE Column1 <> @val_0"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", 1)));
                }
            }

            [Test]
            public void Select_Column1_From_Table_Where_Column1_Is_NULL()
            {
                using (var query = new Select().Column("Column1").From.Table("Table").Where.Column("Column1").Is.Null)
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Table WHERE Column1 IS NULL"));
                }
            }

            [Test]
            public void Select_Column1_From_Table_Where_Column1_Is_NotNULL()
            {
                using (var query = new Select().Column("Column1").From.Table("Table").Where.Column("Column1").Is.NotNull)
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Table WHERE Column1 IS NOT NULL"));
                }
            }

            [Test]
            public void Select_Column1_From_Table_Where_Column1_Like_String1()
            {
                using (var query = new Select().Column("Column1").From.Table("Table").Where.Column("Column1").Like.Value("%String1%"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Table WHERE Column1 LIKE @val_0"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "%String1%")));
                }
            }

            [Test]
            public void Select_Column1_From_Table_Where_Column1_In_Value1_Value2_Value3()
            {
                using (var query = new Select().Column("Column1").From.Table("Table").Where.Column("Column1").In.Value("Value1").Value("Value2").Value("Value3"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Table WHERE Column1 IN (@val_0, @val_1, @val_2)"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "Value1")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", "Value2")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_2", "Value3")));
                }
            }

            [Test]
            public void Select_Column1_From_Table_Where_Column1_In_Array()
            {
                var array = new object[] { 1, "2" };
                using (var query = new Select().Column("Column1").From.Table("Table").Where.Column("Column1").In.Array(array))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Table WHERE Column1 IN (@val_0, @val_1)"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", 1)));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", "2")));
                }
            }

            [Test]
            public void Select_Column1_From_Table_Where_Column1_In_Value1_Value2_Value3_OrderBy_Column1_Desc()
            {
                using (var query = new Select().Column("Column1").From.Table("Table").Where.Column("Column1").In.Value("Value1").Value("Value2").Value("Value3")
                                               .OrderBy.Column("Column1").Desc)
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Table WHERE Column1 IN (@val_0, @val_1, @val_2) ORDER BY Column1 DESC"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "Value1")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", "Value2")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_2", "Value3")));
                }
            }

            [Test]
            public void Select_Column1_From_Table_Where_Column1_In_Value1_Value2_Value3_Inner_Join_Table_Using_Column1()
            {
                using (var query = new Select().Column("Column1").From.Table("Table").Where.Column("Column1").In.Value("Value1").Value("Value2").Value("Value3")
                                               .Inner.Join("Table").Using(new string[] { "Column1" }))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Table WHERE Column1 IN (@val_0, @val_1, @val_2) INNER JOIN Table USING(Column1)"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "Value1")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", "Value2")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_2", "Value3")));
                }
            }

            [Test]
            public void Select_Column1_From_Table_Where_Column1_In_Value1_Value2_Value3_Left_Join_Table_Using_Column1()
            {
                using (var query = new Select().Column("Column1").From.Table("Table").Where.Column("Column1").In.Value("Value1").Value("Value2").Value("Value3")
                                               .Left.Join("Table").Using(new string[] { "Column1" }))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Table WHERE Column1 IN (@val_0, @val_1, @val_2) LEFT JOIN Table USING(Column1)"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "Value1")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", "Value2")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_2", "Value3")));
                }
            }

            [Test]
            public void Select_Column1_From_Table_Where_Column1_In_Value1_Value2_Value3_Right_Join_Table_Using_Column1()
            {
                using (var query = new Select().Column("Column1").From.Table("Table").Where.Column("Column1").In.Value("Value1").Value("Value2").Value("Value3")
                                               .Right.Join("Table").Using(new string[] { "Column1" }))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Table WHERE Column1 IN (@val_0, @val_1, @val_2) RIGHT JOIN Table USING(Column1)"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "Value1")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", "Value2")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_2", "Value3")));
                }
            }

            [Test]
            public void Select_Column1_From_Table_Where_Column1_Not_In_Value1_Value2_Value3()
            {
                using (var query = new Select().Column("Column1").From.Table("Table").Where.Column("Column1").Not.In.Value("Value1").Value("Value2").Value("Value3"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Table WHERE Column1 NOT IN (@val_0, @val_1, @val_2)"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "Value1")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", "Value2")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_2", "Value3")));
                }
            }

            [Test]
            public void Select_Column1_From_Table_Where_Exists__Select_Column1_From_Table()
            {
                using (var query = new Select().Column("Column1").From.Table("Table").Where.Exists.SubQuery(new Select().Column("Column1").From.Table("Table")))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Table WHERE EXISTS (SELECT Column1 FROM Table)"));
                }
            }

            [Test]
            public void Select_Column1_From_Table_Where_Not_Exists__Select_Column1_From_Table()
            {
                using (var query = new Select().Column("Column1").From.Table("Table").Where.Not.Exists.SubQuery(new Select().Column("Column1").From.Table("Table")))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Table WHERE NOT EXISTS (SELECT Column1 FROM Table)"));
                }
            }

            [Test]
            public void Select_Column1_From_Table_Where_KeyEqualValue()
            {
                var dictionary = new Dictionary<string, object>() { { "Column1", "Value1" } };
                using (var query = new Select().Column("Column1").From.Table("Table").Where.KeyEqualToValue(dictionary))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Table WHERE Column1 = @val_0"));
                }
            }

            [Test]
            public void Select_Column1_From_Table_Where_KeyEqualValue_And_KeyEqualValue()
            {
                var dictionary = new Dictionary<string, object>() { { "Column1", "Value1" }, { "Column2", "Value1" } };
                using (var query = new Select().Column("Column1").Column("Column2").From.Table("Table").Where.KeyEqualToValue(dictionary))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1, Column2 FROM Table WHERE Column1 = @val_0 AND Column2 = @val_1"));
                }
            }
        }

        [Category("Homura.QueryBuilder QueryBuilder")]
        public class TableTest
        {
            [Test]
            public void Select_Asterisk_From_Table()
            {
                using (var query = new Select().Asterisk().From.Table("Table"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT * FROM Table"));
                }
            }

            [Test]
            public void Select_Asterisk_From_Table_alias()
            {
                using (var query = new Select().Asterisk().From.Table("Table", "alias"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT * FROM Table alias"));
                }
            }

            [Test]
            public void Select_Asterisk_From_Schema_Table()
            {
                using (var query = new Select().Asterisk().From.Table("Schema", "Table", null))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT * FROM Schema.Table"));
                }
            }

            [Test]
            public void Select_Asterisk_From_Schema_Table_alias()
            {
                using (var query = new Select().Asterisk().From.Table("Schema", "Table", "alias"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT * FROM Schema.Table alias"));
                }
            }

            [Test]
            public void Select_Asterisk_From_Catalog_Schema_Table()
            {
                using (var query = new Select().Asterisk().From.Table("Catalog", "Schema", "Table", null))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT * FROM Catalog.Schema.Table"));
                }
            }

            [Test]
            public void Select_Asterisk_From_Catalog_Schema_Table_alias()
            {
                using (var query = new Select().Asterisk().From.Table("Catalog", "Schema", "Table", "alias"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT * FROM Catalog.Schema.Table alias"));
                }
            }

            [Test]
            public void Select_Asterisk_From_TableObject__Name()
            {
                var table = new Table()
                {
                    Name = "Table"
                };
                using (var query = new Select().Asterisk().From.Table(table))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT * FROM Table"));
                }
            }

            [Test]
            public void Select_Asterisk_From_TableObject__Name_alias()
            {
                var table = new Table()
                {
                    Name = "Table",
                    Alias = "alias"
                };
                using (var query = new Select().Asterisk().From.Table(table))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT * FROM Table alias"));
                }
            }

            [Test]
            public void Select_Asterisk_From_TableObject__Schema_Name()
            {
                var table = new Table()
                {
                    Schema = "Schema",
                    Name = "Table"
                };
                using (var query = new Select().Asterisk().From.Table(table))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT * FROM Schema.Table"));
                }
            }

            [Test]
            public void Select_Asterisk_From_TableObject__Catalog_Schema_Name()
            {
                var table = new Table()
                {
                    Catalog = "Catalog",
                    Schema = "Schema",
                    Name = "Table"
                };
                using (var query = new Select().Asterisk().From.Table(table))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT * FROM Catalog.Schema.Table"));
                }
            }

            [Test]
            public void Select_Asterisk_From_TableObject__Catalog_null_Name()
            {
                var table = new Table()
                {
                    Catalog = "Catalog",
                    Schema = null,
                    Name = "Table"
                };
                using (var query = new Select().Asterisk().From.Table(table))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT * FROM Table"));
                }
            }
        }

        [Category("Homura.QueryBuilder QueryBuilder")]
        public class JoinTest
        {
            [Test]
            public void Select_Column1_From_Alpha_Join_Beta_Using_Column1()
            {
                using (var query = new Select().Column("Column1").From.Table("Alpha").Join("Beta").Using(new string[] { "Column1" }))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Alpha JOIN Beta USING(Column1)"));
                }
            }

            [Test]
            public void Select_Column1_From_Alpha_Inner_Join_Beta_Using_Column1()
            {
                using (var query = new Select().Column("Column1").From.Table("Alpha").Inner.Join("Beta").Using(new string[] { "Column1" }))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Alpha INNER JOIN Beta USING(Column1)"));
                }
            }

            [Test]
            public void Select_Column1_From_Alpha_Union_Join_Beta_Using_Column1()
            {
                using (var query = new Select().Column("Column1").From.Table("Alpha").Union.Join("Beta").Using(new string[] { "Column1" }))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Alpha UNION JOIN Beta USING(Column1)"));
                }
            }

            [Test]
            public void Select_Column1_From_Alpha_Left_Join_Beta_Using_Column1()
            {
                using (var query = new Select().Column("Column1").From.Table("Alpha").Left.Join("Beta").Using(new string[] { "Column1" }))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Alpha LEFT JOIN Beta USING(Column1)"));
                }
            }

            [Test]
            public void Select_Column1_From_Alpha_Left_Outer_Join_Beta_Using_Column1()
            {
                using (var query = new Select().Column("Column1").From.Table("Alpha").Left.Outer.Join("Beta").Using(new string[] { "Column1" }))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Alpha LEFT OUTER JOIN Beta USING(Column1)"));
                }
            }

            [Test]
            public void Select_Column1_From_Alpha_Right_Join_Beta_Using_Column1()
            {
                using (var query = new Select().Column("Column1").From.Table("Alpha").Right.Join("Beta").Using(new string[] { "Column1" }))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Alpha RIGHT JOIN Beta USING(Column1)"));
                }
            }

            [Test]
            public void Select_Column1_From_Alpha_Right_Outer_Join_Beta_Using_Column1()
            {
                using (var query = new Select().Column("Column1").From.Table("Alpha").Right.Outer.Join("Beta").Using(new string[] { "Column1" }))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Alpha RIGHT OUTER JOIN Beta USING(Column1)"));
                }
            }

            [Test]
            public void Select_Column1_From_Alpha_Full_Join_Beta_Using_Column1()
            {
                using (var query = new Select().Column("Column1").From.Table("Alpha").Full.Join("Beta").Using(new string[] { "Column1" }))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Alpha FULL JOIN Beta USING(Column1)"));
                }
            }

            [Test]
            public void Select_Column1_From_Alpha_Full_Outer_Join_Beta_Using_Column1()
            {
                using (var query = new Select().Column("Column1").From.Table("Alpha").Full.Outer.Join("Beta").Using(new string[] { "Column1" }))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Alpha FULL OUTER JOIN Beta USING(Column1)"));
                }
            }

            [Test]
            public void Select_Column1_From_Alpha_Cross_Join_Beta()
            {
                using (var query = new Select().Column("Column1").From.Table("Alpha").Cross.Join("Beta"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Alpha CROSS JOIN Beta"));
                }
            }

            [Test]
            public void Select_Column1_From_Alpha_Inner_Join_Beta_On_AlphaColumn1_Equal_BetaColumn1()
            {
                using (var query = new Select().Column("Column1").From.Table("Alpha").Inner.Join("Beta").On.Column("Alpha", "Column1").EqualTo.Column("Beta", "Column1"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Alpha INNER JOIN Beta ON Alpha.Column1 = Beta.Column1"));
                }
            }

            [Test]
            public void Select_Column1_From_Alpha_Inner_Join_Beta_On_AlphaColumn1_Equal_BetaColumn1_And_BetaColumn1_Equal_String1()
            {
                using (var query = new Select().Column("Column1").From.Table("Alpha").Inner.Join("Beta").On.Column("Alpha", "Column1").EqualTo.Column("Beta", "Column1")
                                               .And().Column("Beta", "Column1").EqualTo.Value("String1"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Alpha INNER JOIN Beta ON Alpha.Column1 = Beta.Column1 AND Beta.Column1 = @val_0"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "String1")));
                }
            }

            [Test]
            public void Select_Column1_Column2_From_Alpha_a_Join_Beta_b_On_aColumn1_Equal_bColumn1_Join_Gamma_g_On_aColumn2_Equal_gColumn2()
            {
                using (var query = new Select().Column("Column1").Column("Column2").From.Table("Alpha", "a")
                                               .Join("Beta", "b").On.Column("a", "Column1").EqualTo.Column("b", "Column1")
                                               .Join("Gamma", "g").On.Column("a", "Column2").EqualTo.Column("g", "Column2"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1, Column2 FROM Alpha a JOIN Beta b ON a.Column1 = b.Column1 JOIN Gamma g ON a.Column2 = g.Column2"));
                }
            }

            [Test]
            public void Select_Column1_Column2_From_Alpha_a_Inner_Join_Beta_b_On_aColumn1_Equal_bColumn1_Inner_Join_Gamma_g_On_aColumn2_Equal_gColumn2()
            {
                using (var query = new Select().Column("Column1").Column("Column2").From.Table("Alpha", "a")
                                               .Inner.Join("Beta", "b").On.Column("a", "Column1").EqualTo.Column("b", "Column1")
                                               .Inner.Join("Gamma", "g").On.Column("a", "Column2").EqualTo.Column("g", "Column2"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1, Column2 FROM Alpha a INNER JOIN Beta b ON a.Column1 = b.Column1 INNER JOIN Gamma g ON a.Column2 = g.Column2"));
                }
            }
        }

        [Category("Homura.QueryBuilder QueryBuilder")]
        public class OrderTest
        {
            [Test]
            public void Select_Column1_From_Table_OrderBy_Column1()
            {
                using (var query = new Select().Column("Column1").From.Table("Table").OrderBy.Column("Column1"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Table ORDER BY Column1"));
                }
            }

            [Test]
            public void Select_Column1_From_Table_OrderBy_Column1_Asc()
            {
                using (var query = new Select().Column("Column1").From.Table("Table").OrderBy.Column("Column1").Asc)
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Table ORDER BY Column1 ASC"));
                }
            }

            [Test]
            public void Select_Column1_From_Table_OrderBy_Column1_Desc()
            {
                using (var query = new Select().Column("Column1").From.Table("Table").OrderBy.Column("Column1").Desc)
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Table ORDER BY Column1 DESC"));
                }
            }

            [Test]
            public void Select_Column1_Column2_From_Table_OrderBy_Column1_Desc_Column2_Desc()
            {
                using (var query = new Select().Column("Column1").From.Table("Table").OrderBy.Column("Column1").Desc.Column("Column2").Desc)
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Table ORDER BY Column1 DESC, Column2 DESC"));
                }
            }
        }

        [Category("Homura.QueryBuilder QueryBuilder")]
        public class GroupByTest
        {
            [Test]
            public void Select_Column1_From_Table_GroupBy_Column1()
            {
                using (var query = new Select().Column("Column1").From.Table("Table").GroupBy.Column("Column1"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Table GROUP BY Column1"));
                }
            }

            [Test]
            public void Select_Column1_Column2_Column3_From_Table_GroupBy_Column1_Column2_Column3()
            {
                using (var query = new Select().Column("Column1").Column("Column2").Column("Column3").From.Table("Table")
                                               .GroupBy.Column("Column1").Column("Column2").Column("Column3"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1, Column2, Column3 FROM Table GROUP BY Column1, Column2, Column3"));
                }
            }

            [Test]
            public void Select_Columns__c1_c2_c3__From_Table_GroupBy_Columns__c1_c2_c3()
            {
                using (var query = new Select().Columns("c1", "c2", "c3").From.Table("Table")
                                               .GroupBy.Columns("c1", "c2", "c3"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT c1, c2, c3 FROM Table GROUP BY c1, c2, c3"));
                }
            }
        }

        [Category("Homura.QueryBuilder QueryBuilder")]
        public class SubqueryTest
        {
            [Test]
            public void Select_Column1_Column2__Select_Column3_From_Alpha__From_Alpha()
            {
                using (var query = new Select().Column("Column1").Column("Column2").SubQuery(new Select().Column("Column3").From.Table("Alpha")).From.Table("Alpha"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1, Column2, (SELECT Column3 FROM Alpha) FROM Alpha"));
                }
            }

            [Test]
            public void Select_Column1_From__Select_Column1_From_Alpha()
            {
                using (var query = new Select().Column("Column1").From.SubQuery(new Select().Column("Column1").From.Table("Alpha")))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM (SELECT Column1 FROM Alpha)"));
                }
            }

            [Test]
            public void Select_Column1_From_Alpha_Where_Column1_Equal__Select_Column1_From_Beta()
            {
                using (var query = new Select().Column("Column1").From.Table("Alpha").Where.Column("Column1").EqualTo.SubQuery(new Select().Column("Column1").From.Table("Beta")))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Alpha WHERE Column1 = (SELECT Column1 FROM Beta)"));
                }
            }
        }

        [Category("Homura.QueryBuilder QueryBuilder")]
        public class CombinationTest
        {
            /// <summary>
            /// Select -> Where -> OrderBy
            /// </summary>
            [Test]
            public void Select_Column1_From_Alpha_Where_Column1_EqualTo_Value1_OrderBy_Column1_Desc()
            {
                using (var query = new Select().Column("Column1").From.Table("Alpha").Where.Column("Column1").EqualTo.Value("Value1")
                                                                                     .OrderBy.Column("Column1").Desc)
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Alpha WHERE Column1 = @val_0 ORDER BY Column1 DESC"));
                }
            }


            /// <summary>
            /// Select -> Where -> Join
            /// </summary>
            [Test]
            public void Select_Column1_From_Alpha_a_Where_Column1_EqualTo_Value1_Inner_Join_Beta_b_On_aColumn1_Equal_bColumn1()
            {
                using (var query = new Select().Column("Column1").From.Table("Alpha", "a").Where.Column("Column1").EqualTo.Value("Value1")
                                                                                          .Inner.Join("Beta", "b").On.Column("a", "Column1").EqualTo.Column("b", "Column1"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Alpha a WHERE Column1 = @val_0 INNER JOIN Beta b ON a.Column1 = b.Column1"));
                }
            }

            // <summary>
            // Select -> OrderBy -> Where
            // </summary>
            [Test]
            public void Select_Column1_From_Alpha_OrderBy_Column1_Desc_Where_Column1_EqualTo_Value1()
            {
                using (var query = new Select().Column("Column1").From.Table("Alpha").OrderBy.Column("Column1").Desc
                                                                                     .Where.Column("Column1").EqualTo.Value("Value1"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Alpha ORDER BY Column1 DESC WHERE Column1 = @val_0"));
                }
            }

            /// <summary>
            /// Select -> OrderBy -> Join
            /// </summary>
            [Test]
            public void Select_Column1_From_Alpha_a_OrderBy_Column1_Desc_Inner_Join_Beta_b_On_aColumn1_Equal_bColumn1()
            {
                using (var query = new Select().Column("Column1").From.Table("Alpha", "a").OrderBy.Column("Column1").Desc
                                                                                          .Inner.Join("Beta", "b").On.Column("a", "Column1").EqualTo.Column("b", "Column1"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Alpha a ORDER BY Column1 DESC INNER JOIN Beta b ON a.Column1 = b.Column1"));
                }
            }


            /// <summary>
            /// Select -> Join -> Where
            /// </summary>
            [Test]
            public void Select_Column1_From_Alpha_a_Inner_Join_Beta_b_On_aColumn1_Equal_bColumn1_Where_Column1_EqualTo_Value1()
            {
                using (var query = new Select().Column("Column1").From.Table("Alpha", "a").Inner.Join("Beta", "b").On.Column("a", "Column1").EqualTo.Column("b", "Column1")
                                                                                          .Where.Column("Column1").EqualTo.Value("Value1"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Alpha a INNER JOIN Beta b ON a.Column1 = b.Column1 WHERE Column1 = @val_0"));
                }
            }

            /// <summary>
            /// Select -> Join -> OrderBy
            /// </summary>
            [Test]
            public void Select_Column1_From_Alpha_a_Inner_Join_Beta_b_On_aColumn1_Equal_bColumn1_OrderBy_Column1_Desc()
            {
                using (var query = new Select().Column("Column1").From.Table("Alpha", "a").Inner.Join("Beta", "b").On.Column("a", "Column1").EqualTo.Column("b", "Column1")
                                                                                          .OrderBy.Column("Column1").Desc)
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Alpha a INNER JOIN Beta b ON a.Column1 = b.Column1 ORDER BY Column1 DESC"));
                }
            }

            /// <summary>
            /// Select -> Join -> GroupBy -> OrderBy
            /// </summary>
            [Test]
            public void Select_Count_Asterisk_As_Count_aColumn1_aColumn2_From_Alpha_a_Inner_Join_Beta_b_On_aColumn1_EqualTo_bColumn3_GroupBy_aColumn1_OrderBy_Count_Desc()
            {
                using (var query = new Select().Count("*").As("Count").Column("a", "Column1").Column("a", "Column2")
                                               .From.Table("Alpha", "a")
                                               .Inner.Join("Beta", "b").On.Column("a", "Column1").EqualTo.Column("b", "Column3")
                                               .GroupBy.Column("a", "Column1")
                                               .OrderBy.Column("Count").Desc)
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT COUNT(*) AS 'Count', a.Column1, a.Column2 FROM Alpha a INNER JOIN Beta b ON a.Column1 = b.Column3 GROUP BY a.Column1 ORDER BY Count DESC"));
                }
            }
        }

        [Category("Homura.QueryBuilder QueryBuilder")]
        public class FunctionTest
        {
            [Test]
            public void Select_Count_1_As_Count_From_Table()
            {
                using (var query = new Select().Count("1").As("Count").From.Table("Table"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT COUNT(1) AS 'Count' FROM Table"));
                }
            }
        }

        [Category("Homura.QueryBuilder QueryBuilder")]
        public class SetOperationTest
        {
            [Test]
            public void Select_Column1_From_Alpha_Union_Select_Column1_From_Beta()
            {
                using (var query = new Select().Column("Column1").From.Table("Alpha")
                                               .Union
                                               .Select.Column("Column1").From.Table("Beta"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Alpha UNION SELECT Column1 FROM Beta"));
                }
            }

            [Test]
            public void Select_Column1_From_Alpha_Union_ALL_Select_Column1_From_Beta()
            {
                using (var query = new Select().Column("Column1").From.Table("Alpha")
                                               .Union.All
                                               .Select.Column("Column1").From.Table("Beta"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Alpha UNION ALL SELECT Column1 FROM Beta"));
                }
            }

            [Test]
            public void Select_Column1_From_Alpha_Union_Corresponding_Select_Column1_From_Beta()
            {
                using (var query = new Select().Column("Column1").From.Table("Alpha")
                                               .Union.Corresponding
                                               .Select.Column("Column1").From.Table("Beta"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Alpha UNION CORRESPONDING SELECT Column1 FROM Beta"));
                }
            }

            [Test]
            public void Select_Column1_From_Alpha_Union_ALL_Corresponding_Select_Column1_From_Beta()
            {
                using (var query = new Select().Column("Column1").From.Table("Alpha")
                                               .Union.All.Corresponding
                                               .Select.Column("Column1").From.Table("Beta"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Alpha UNION ALL CORRESPONDING SELECT Column1 FROM Beta"));
                }
            }

            [Test]
            public void Select_Column1_From_Alpha_Union_ALL_Corresponding_Column1_Select_Column1_From_Beta()
            {
                using (var query = new Select().Column("Column1").From.Table("Alpha")
                                               .Union.All.Corresponding.By.Column("Column1")
                                               .Select.Column("Column1").From.Table("Beta"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1 FROM Alpha UNION ALL CORRESPONDING BY (Column1) SELECT Column1 FROM Beta"));
                }
            }

            [Test]
            public void Select_Column1_Column2_Column3_From_Alpha_Union_ALL_Corresponding_Column1_Column2_Column3_Select_Column1_Column2_Column3_From_Beta()
            {
                using (var query = new Select().Column("Column1").Column("Column2").Column("Column3").From.Table("Alpha")
                                               .Union.All.Corresponding.By.Column("Column1").Column("Column2").Column("Column3")
                                               .Select.Column("Column1").Column("Column2").Column("Column3").From.Table("Beta"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("SELECT Column1, Column2, Column3 FROM Alpha UNION ALL CORRESPONDING BY (Column1, Column2, Column3) SELECT Column1, Column2, Column3 FROM Beta"));
                }
            }
        }
    }
}
