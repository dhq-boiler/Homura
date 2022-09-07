

using Homura.ORM;
using System;
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
                list.Add(new Column("ID", typeof(Guid), "datatype", null, 0, null));
                list.Add(new Column("Title", typeof(string), "datatype", null, 1, null));
                list.Add(new Column("MasterPath", typeof(string), "datatype", null, 2, null));
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