
using Homura.ORM;
using Homura.ORM.Mapping;
using Homura.Test.TestFixture.Migration;
using System;

namespace Homura.Test.TestFixture.Entity
{
    [GenerateDao]
    [GenerateChangePlan(typeof(VersionOrigin))]
    [GenerateChangePlan(typeof(Version_1), typeof(VersionOrigin))]
    [DefaultVersion(typeof(Version_1))]
    internal class GeneratedSample : EntityBaseObject
    {
        [Column("Id", "NUMERIC", 0), PrimaryKey, Index]
        public Guid Id { get; set; }

        [Column("Name", "TEXT", 1)]
        [Since(typeof(VersionOrigin))]
        public string Name { get; set; }

        [Column("Value", "INTEGER", 2)]
        [Since(typeof(VersionOrigin))]
        public int Value { get; set; }

        [Column("IsActive", "INTEGER", 3, HandlingDefaultValue.AsValue, false)]
        [Since(typeof(Version_1))]
        public bool IsActive { get; set; }
    }
}
