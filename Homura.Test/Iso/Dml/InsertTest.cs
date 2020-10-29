

using NUnit.Framework;
using Homura.QueryBuilder.Iso.Dml;
using System.Collections.Generic;

namespace Homura.QueryBuilder.Test.Iso.Dml
{
    [TestFixture]
    public class InsertTest
    {
        [Category("Homura.QueryBuilder QueryBuilder")]
        public class BasicTest
        {
            [Test]
            public void Insert_Into_Table_Values_Value1()
            {
                using (var query = new Insert().Into.Table("Table").Values.Value("Value1"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("INSERT INTO Table VALUES (@val_0)"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "Value1")));
                }
            }

            [Test]
            public void Insert_Into_Table_Column1_Values_Value1()
            {
                using (var query = new Insert().Into.Table("Table").Column("Column1").Values.Value("Value1"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("INSERT INTO Table (Column1) VALUES (@val_0)"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "Value1")));
                }
            }

            [Test]
            public void Insert_Into_Table_Column1_Values_1()
            {
                using (var query = new Insert().Into.Table("Table").Column("Column1").Values.Value(1))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("INSERT INTO Table (Column1) VALUES (@val_0)"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", 1)));
                }
            }

            [Test]
            public void Insert_Into_Table_Column1_Column2_Column3_Values_Value1_Value2_Value3()
            {
                using (var query = new Insert().Into.Table("Table").Column("Column1").Column("Column2").Column("Column3")
                                               .Values.Value("Value1").Value("Value2").Value("Value3"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("INSERT INTO Table (Column1, Column2, Column3) VALUES (@val_0, @val_1, @val_2)"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "Value1")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", "Value2")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_2", "Value3")));
                }
            }

            [Test]
            public void Insert_Into_Table_Column1_Columns__Column2_Column3__Values_Value1_Value__Value2_Value3()
            {
                using (var query = new Insert().Into.Table("Table").Column("Column1").Columns("Column2", "Column3")
                                               .Values.Value("Value1").Value("Value2", "Value3"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("INSERT INTO Table (Column1, Column2, Column3) VALUES (@val_0, @val_1, @val_2)"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "Value1")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", "Value2")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_2", "Value3")));
                }
            }

            [Test]
            public void Insert_Into_Table_Values_Row1()
            {
                using (var query = new Insert().Into.Table("Table").Values.Row(new object[] { "RowValue1", "RowValue1" }))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("INSERT INTO Table VALUES (@val_0, @val_1)"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "RowValue1")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", "RowValue1")));
                }
            }

            [Test]
            public void Insert_Into_Table_Values_Row1_Row2()
            {
                using (var query = new Insert().Into.Table("Table").Values.Row(new object[] { "RowValue1", "RowValue1" }).Row(new object[] { "RowValue2", "RowValue2" }))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("INSERT INTO Table VALUES (@val_0, @val_1), (@val_2, @val_3)"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "RowValue1")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", "RowValue1")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_2", "RowValue2")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_3", "RowValue2")));
                }
            }

            [Test]
            public void Insert_Into_Table_Values_Value1_Value2_Value3_NextRow_Value4_Value5_Value6()
            {
                using (var query = new Insert().Into.Table("Table").Values.Value("Value1").Value("Value2").Value("Value3").NextRow
                                               .Value("Value4").Value("Value5").Value("Value6"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("INSERT INTO Table VALUES (@val_0, @val_1, @val_2), (@val_3, @val_4, @val_5)"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "Value1")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", "Value2")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_2", "Value3")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_3", "Value4")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_4", "Value5")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_5", "Value6")));
                }
            }

            [Test]
            public void Insert_Into_Table_Values_Value1_NextRow_Row1()
            {
                using (var query = new Insert().Into.Table("Table").Values.Value("Value1").NextRow.Row(new object[] { "Row1" }))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("INSERT INTO Table VALUES (@val_0), (@val_1)"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "Value1")));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", "Row1")));
                }
            }

            [Test]
            public void Insert_Into_Table_Columns__c1_c2_c3__Values_Value__v1_v2_v3()
            {
                using (var query = new Insert().Into.Table("Table").Columns("c1", "c2", "c3").Values.Value("v1", "v2", "v3"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("INSERT INTO Table (c1, c2, c3) VALUES (@val_0, @val_1, @val_2)"));
                }
            }
        }

        [Category("Homura.QueryBuilder QueryBuilder")]
        public class InsertSelectTest
        {
            [Test]
            public void Insert_Into_Alpha_Select_Column1_From_Beta()
            {
                using (var query = new Insert().Into.Table("Alpha").Select.Column("Column1").From.Table("Beta"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("INSERT INTO Alpha SELECT Column1 FROM Beta"));
                }
            }
            [Test]
            public void Insert_Into_Alpha_Column1_Select_Column1_From_Beta()
            {
                using (var query = new Insert().Into.Table("Alpha").Column("Column1").Select.Column("Column1").From.Table("Beta"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("INSERT INTO Alpha (Column1) SELECT Column1 FROM Beta"));
                }
            }

            [Test]
            public void Insert_Into_Alpha_Columns__c1_c2_c3__Select_Column1_Column2_Column3_From_Beta()
            {
                using (var query = new Insert().Into.Table("Alpha").Columns("c1", "c2", "c3").Select.Columns("Column1", "Column2", "Column3").From.Table("Beta"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("INSERT INTO Alpha (c1, c2, c3) SELECT Column1, Column2, Column3 FROM Beta"));
                }
            }

            [Test]
            public void Insert_Into_Alpha_Column1_Columns__Column2_Column3__Select_Column1_Column2_Column3_From_Beta()
            {
                using (var query = new Insert().Into.Table("Alpha").Column("Column1").Columns("Column2", "Column3")
                                               .Select.Column("Column1").Column("Column2").Column("Column3").From.Table("Beta"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("INSERT INTO Alpha (Column1, Column2, Column3) SELECT Column1, Column2, Column3 FROM Beta"));
                }
            }

            [Test]
            public void Insert_Into_Alpha_Columns__Column1_Column2__Column3__Select_Column1_Column2_Column3_From_Beta()
            {
                using (var query = new Insert().Into.Table("Alpha").Columns("Column1", "Column2").Column("Column3")
                                               .Select.Column("Column1").Column("Column2").Column("Column3").From.Table("Beta"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("INSERT INTO Alpha (Column1, Column2, Column3) SELECT Column1, Column2, Column3 FROM Beta"));
                }
            }
        }
    }
}
