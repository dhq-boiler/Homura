

using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.Test.TestFixture.Migration;
using System;

namespace Homura.Test.TestFixture.Entity
{
    internal class Alpha : EntityBaseObject
    {
        [Column("Id", "NUMERIC", 0), PrimaryKey, Index]
        public Guid Id { get; set; }

        [Column("Item1", "TEXT", 1)]
        public string Item1 { get; set; }

        [Column("Item2", "TEXT", 2)]
        [Since(typeof(VersionOrigin))]
        public string Item2 { get; set; }

        [Column("Item3", "NUMERIC", 3)]
        [Since(typeof(Version_1))]
        public Guid Item3 { get; set; }

        [Column("Item4", "TEXT", 4)]
        [Since(typeof(Version_1))]
        public string Item4 { get; set; }

        [Column("Item5", "INTEGER", 5)]
        [Since(typeof(Version_1))]
        public int Item5 { get; set; }

        [Column("Item6", "INTEGER", 6)]
        [Since(typeof(Version_1))]
        public long Item6 { get; set; }

        [Column("Item7", "TEXT", 7)]
        [Since(typeof(Version_2))]
        public string Item7 { get; set; }

        [Column("Item8", "INTEGER", 8, PassAsColumnOrValue.AsValue, false)]
        [Since(typeof(Version_3))]
        public bool Item8 { get; set; }

        [Column("Item9", "INTEGER", 9, PassAsColumnOrValue.AsValue, true)]
        [Since(typeof(Version_3))]
        public bool Item9 { get; set; }
    }
}
