

using NUnit.Framework;
using Homura.QueryBuilder.Iso.Dml;
using System.Collections.Generic;

namespace Homura.QueryBuilder.Test.Iso.Dml
{
    [TestFixture]
    public class DeleteTest
    {
        [Category("Homura.QueryBuilder QueryBuilder")]
        public class BasicTest
        {
            [Test]
            public void Delete_From_Table()
            {
                using (var query = new Delete().From.Table("Table"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("DELETE FROM Table"));
                }
            }

            [Test]
            public void Delete_From_Table_a()
            {
                using (var query = new Delete().From.Table("Table", "a"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("DELETE FROM Table a"));
                }
            }
        }

        [Category("Homura.QueryBuilder QueryBuilder")]
        public class WhereTest
        {
            [Test]
            public void Delete_From_Table_Where_Column1_EqualTo_1()
            {
                using (var query = new Delete().From.Table("Table").Where.Column("Column1").EqualTo.Value(1))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("DELETE FROM Table WHERE Column1 = @val_0"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", 1)));
                }
            }

            [Test]
            public void Delete_Table_Where_Column1_EqualTo_1_And_Column2_NotEqualTo_2()
            {
                using (var query = new Delete().From.Table("Table").Where.Column("Column1").EqualTo.Value(1)
                                               .And().Column("Column2").NotEqualTo.Value(2))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("DELETE FROM Table WHERE Column1 = @val_0 AND Column2 <> @val_1"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", 1)));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_1", 2)));
                }
            }

            [Test]
            public void Delete_Table_Where_Column1_EqualTo_String1()
            {
                using (var query = new Delete().From.Table("Table").Where.Column("Column1").EqualTo.Value("String1"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("DELETE FROM Table WHERE Column1 = @val_0"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "String1")));
                }
            }

            [Test]
            public void Delete_Table_Where_Column1_GreaterThan_1()
            {
                using (var query = new Delete().From.Table("Table").Where.Column("Column1").GreaterThan.Value(1))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("DELETE FROM Table WHERE Column1 > @val_0"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", 1)));
                }
            }

            [Test]
            public void Delete_Table_Where_Column1_LessThan_1()
            {
                using (var query = new Delete().From.Table("Table").Where.Column("Column1").LessThan.Value(1))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("DELETE FROM Table WHERE Column1 < @val_0"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", 1)));
                }
            }

            [Test]
            public void Delete_Table_Where_Column1_GreaterThanOrEqualTo_1()
            {
                using (var query = new Delete().From.Table("Table").Where.Column("Column1").GreaterThanOrEqualTo.Value(1))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("DELETE FROM Table WHERE Column1 >= @val_0"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", 1)));
                }
            }

            [Test]
            public void Delete_Table_Where_Column1_LessThanOrEqualTo_1()
            {
                using (var query = new Delete().From.Table("Table").Where.Column("Column1").LessThanOrEqualTo.Value(1))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("DELETE FROM Table WHERE Column1 <= @val_0"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", 1)));
                }
            }

            [Test]
            public void Delete_Table_Where_Column1_NotEqualTo_1()
            {
                using (var query = new Delete().From.Table("Table").Where.Column("Column1").NotEqualTo.Value(1))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("DELETE FROM Table WHERE Column1 <> @val_0"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", 1)));
                }
            }

            [Test]
            public void Delete_Table_Where_Column1_IsNULL()
            {
                using (var query = new Delete().From.Table("Table").Where.Column("Column1").Is.Null)
                {
                    Assert.That(query.ToSql(), Is.EqualTo("DELETE FROM Table WHERE Column1 IS NULL"));
                }
            }

            [Test]
            public void Delete_Table_Where_Column1_IsNotNULL()
            {
                using (var query = new Delete().From.Table("Table").Where.Column("Column1").Is.NotNull)
                {
                    Assert.That(query.ToSql(), Is.EqualTo("DELETE FROM Table WHERE Column1 IS NOT NULL"));
                }
            }

            [Test]
            public void Delete_Table_Where_Column1_Like_String1()
            {
                using (var query = new Delete().From.Table("Table").Where.Column("Column1").Like.Value("%String1%"))
                {
                    Assert.That(query.ToSql(), Is.EqualTo("DELETE FROM Table WHERE Column1 LIKE @val_0"));
                    Assert.That(query, Has.Property("Parameters").One.EqualTo(new KeyValuePair<string, object>("@val_0", "%String1%")));
                }
            }
        }
    }
}
