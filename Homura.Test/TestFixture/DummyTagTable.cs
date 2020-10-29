

using Homura.ORM;
using System.Collections.Generic;

namespace Homura.Test.TestFixture
{
    internal class DummyTagTable : DummyAbstractTable
    {
        public override IEnumerable<IColumn> Columns
        {
            get
            {
                List<IColumn> list = new List<IColumn>();
                list.Add(new Column("ID", "datatype", null, 0, null));
                list.Add(new Column("Name", "datatype", null, 1, null));
                return list;
            }
        }

        public override string Name
        {
            get
            {
                return "Tag";
            }
        }
    }
}
