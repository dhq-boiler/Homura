

using Homura.Core;
using Homura.ORM.Mapping;
using System;

namespace Homura.ORM.Migration
{
    public interface IEntityVersionChangePlan : IModifiedCounter
    {
        VersionOrigin TargetVersion { get; set; }

        void UpgradeToTargetVersion(IConnection connection);

        void DowngradeToTargetVersion(IConnection connection);

        Type TargetEntityType { get; set; }

        void CreateTable(IConnection connection);

        void DropTable(IConnection connection);
    }
}
