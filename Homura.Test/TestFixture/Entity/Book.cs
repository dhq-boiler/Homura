

using Homura.ORM.Mapping;
using Homura.Test.TestFixture.Migration;
using System;
using System.Diagnostics;

namespace Homura.Test.TestFixture.Entity
{
    [DefaultVersion(typeof(Version_2))]
    public class Book : Entry
    {
        private Guid _AuthorID;
        private long? _ByteSize;
        private string _FingerPrint;

        [Column("AuthorID", "NUMERIC", 2)]
        [Since(typeof(VersionOrigin))]
        public Guid AuthorID
        {
            [DebuggerStepThrough]
            get
            { return _AuthorID; }
            set { SetProperty(ref _AuthorID, value); }
        }

        [Column("PublishDate", "NUMERIC", 3)]
        [Since(typeof(VersionOrigin))]
        public DateTime? PublishDate { get; internal set; }

        [Column("ByteSize", "INTEGER", 4, ORM.HandlingDefaultValue.AsValue)]
        [Since(typeof(Version_1))]
        public long? ByteSize
        {
            [DebuggerStepThrough]
            get
            { return _ByteSize; }
            set { SetProperty(ref _ByteSize, value); }
        }

        [Column("FingerPrint", "TEXT", 5, ORM.HandlingDefaultValue.AsValue)]
        [Since(typeof(Version_2))]
        public string FingerPrint
        {
            [DebuggerStepThrough]
            get
            { return _FingerPrint; }
            set { SetProperty(ref _FingerPrint, value); }
        }
    }
}
