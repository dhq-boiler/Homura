

using Homura.ORM.Mapping;
using System;
using System.Diagnostics;

namespace Homura.Test.TestFixture.Entity
{
    [DefaultVersion(typeof(VersionOrigin))]
    public class Page : Entry
    {
        private Guid _BookID;
        private Guid _ImageID;
        private int _PageIndex;

        [Column("BookID", "NUMERIC", 2), NotNull, Index]
        [Since(typeof(VersionOrigin))]
        public Guid BookID
        {
            [DebuggerStepThrough]
            get
            { return _BookID; }
            set { SetProperty(ref _BookID, value); }
        }

        [Column("ImageID", "NUMERIC", 3), NotNull]
        [Since(typeof(VersionOrigin))]
        public Guid ImageID
        {
            [DebuggerStepThrough]
            get
            { return _ImageID; }
            set { SetProperty(ref _ImageID, value); }
        }

        [Column("PageIndex", "INTEGER", 4), NotNull]
        [Since(typeof(VersionOrigin))]
        public int PageIndex
        {
            [DebuggerStepThrough]
            get
            { return _PageIndex; }
            set { SetProperty(ref _PageIndex, value); }
        }
    }
}
