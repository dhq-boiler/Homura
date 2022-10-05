using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.Test.TestFixture.Migration;
using System;

namespace Homura.Test.TestFixture.Entity
{
    [DefaultVersion(typeof(VersionOrigin))]
    public class Valkyrie_0 : EntityBaseObject
    {
        [Column("Id", "NUMERIC", 0), PrimaryKey, Index]
        public Guid Id { get; set; }

        [Column("Item1", "TEXT", 1)]
        public string Item1 { get; set; }

        [Column("Item2", "TEXT", 2)]
        [Since(typeof(VersionOrigin))]
        public string Item2 { get; set; }

        [Column("Item3", "TEXT", 3, HandlingDefaultValue.AsValue)]
        [Since(typeof(Version_1))]
        public string Item3 { get; set; }

        [Column("Item4", "TEXT", 4, HandlingDefaultValue.AsValue)]
        [Since(typeof(Version_2))]
        public string Item4 { get; set; }
    }

    [DefaultVersion(typeof(Version_1))]
    public class Valkyrie_1 : EntityBaseObject
    {
        [Column("Id", "NUMERIC", 0), PrimaryKey, Index]
        public Guid Id { get; set; }

        [Column("Item1", "TEXT", 1)]
        public string Item1 { get; set; }

        [Column("Item2", "TEXT", 2)]
        [Since(typeof(VersionOrigin))]
        public string Item2 { get; set; }

        [Column("Item3", "TEXT", 3, HandlingDefaultValue.AsValue)]
        [Since(typeof(Version_1))]
        public string Item3 { get; set; }

        [Column("Item4", "TEXT", 4, HandlingDefaultValue.AsValue)]
        [Since(typeof(Version_2))]
        public string Item4 { get; set; }
    }

    [DefaultVersion(typeof(Version_2))]
    public class Valkyrie_2 : EntityBaseObject
    {
        [Column("Id", "NUMERIC", 0), PrimaryKey, Index]
        public Guid Id { get; set; }

        [Column("Item1", "TEXT", 1)]
        public string Item1 { get; set; }

        [Column("Item2", "TEXT", 2)]
        [Since(typeof(VersionOrigin))]
        public string Item2 { get; set; }

        [Column("Item3", "TEXT", 3, HandlingDefaultValue.AsValue)]
        [Since(typeof(Version_1))]
        public string Item3 { get; set; }

        [Column("Item4", "TEXT", 4, HandlingDefaultValue.AsValue)]
        [Since(typeof(Version_2))]
        public string Item4 { get; set; }
    }
}
