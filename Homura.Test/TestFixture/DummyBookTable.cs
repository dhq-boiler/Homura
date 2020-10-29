

using Homura.ORM;
using System.Collections.Generic;

namespace Homura.Test.TestFixture
{
    internal class DummyBookTable : DummyAbstractTable
    {
        public DummyBookTable()
            : base()
        { }

        public DummyBookTable(string aliasName)
            : base(aliasName)
        { }

        public override IEnumerable<IColumn> Columns
        {
            get
            {
                List<IColumn> list = new List<IColumn>();
                list.Add(new Column("ID", "datatype", null, 0, null));
                list.Add(new Column("Title", "datatype", null, 1, null));
                list.Add(new Column("AuthorID", "datatype", null, 2, null));
                list.Add(new Column("PublishDate", "datatype", null, 3, null));
                return list;
            }
        }

        public override string Name
        {
            get
            {
                return "Book";
            }
        }
    }
}