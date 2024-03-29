﻿using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.Test.TestFixture.Migration;
using Reactive.Bindings;
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

    [DefaultVersion(typeof(VersionOrigin))]
    public class Valkyrie_3 : EntityBaseObject
    {
        [Column("Id", "NUMERIC", 0), PrimaryKey, Index]
        public ReactivePropertySlim<Guid> Id { get; } = new();

        [Column("Item1", "NUMERIC", 1)]
        public ReactivePropertySlim<Guid> Item1 { get; } = new();

        [Column("Item2", "INTEGER", 2, HandlingDefaultValue.AsValue, false)]
        public ReactivePropertySlim<bool> Item2 { get; } = new();

        [Column("Item3", "INTEGER", 3, HandlingDefaultValue.AsValue, false)]
        public ReactivePropertySlim<bool?> Item3 { get; } = new();
    }
}
