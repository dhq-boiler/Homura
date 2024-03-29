﻿

using Homura.ORM;
using System;
using System.Collections.Generic;

namespace Homura.Test.TestFixture
{
    internal class DummyThumbnailTable : DummyAbstractTable
    {
        public DummyThumbnailTable()
            : base()
        { }

        public DummyThumbnailTable(string aliasName)
            : base(aliasName)
        { }

        public override IEnumerable<IColumn> Columns
        {
            get
            {
                List<IColumn> list = new List<IColumn>();
                list.Add(new Column("ID", typeof(Guid), "datatype", null, 0, null));
                list.Add(new Column("ImageID", typeof(Guid), "datatype", null, 1, null));
                list.Add(new Column("Path", typeof(string), "datatype", null, 2, null));
                return list;
            }
        }

        public override string Name
        {
            get
            {
                return "Thumbnail";
            }
        }
    }
}