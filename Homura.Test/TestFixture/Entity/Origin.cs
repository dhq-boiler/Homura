﻿

using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.Test.TestFixture.Migration;
using System;

namespace Homura.Test.TestFixture.Entity
{
    [DefaultVersion(typeof(VersionOrigin))]
    public class Origin : EntityBaseObject
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
    }
}
