

using Homura.ORM;
using System.Collections.Generic;

namespace Homura.Test.TestFixture
{
    internal class DummyImageTable : DummyAbstractTable
    {
        public DummyImageTable()
            : base()
        { }

        public DummyImageTable(string aliasName)
            : base(aliasName)
        { }

        public override IEnumerable<IColumn> Columns
        {
            get
            {
                List<IColumn> list = new List<IColumn>();
                list.Add(new Column("ID", "datatype", null, 0, null));
                list.Add(new Column("Title", "datatype", null, 1, null));
                list.Add(new Column("MasterPath", "datatype", null, 2, null));
                return list;
            }
        }

        public override string Name
        {
            get
            {
                return "Image";
            }
        }
    }
}