﻿

using Homura.ORM;
using System;
using System.Collections.Generic;

namespace Homura.Test.TestFixture
{
    internal class DummyImageTagTable : DummyAbstractTable
    {
        public override IEnumerable<IColumn> Columns
        {
            get
            {
                List<IColumn> list = new List<IColumn>();
                list.Add(new Column("ImageID", typeof(Guid), "datatype", null, 0, null));
                list.Add(new Column("TagID", typeof(Guid), "datatype", null, 1, null));
                return list;
            }
        }

        public override string Name
        {
            get
            {
                return "ImageTag";
            }
        }
    }
}
