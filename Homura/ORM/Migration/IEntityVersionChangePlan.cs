

using Homura.Core;
using Homura.ORM.Mapping;
using Homura.ORM.Setup;
using System;
using System.Threading.Tasks;

namespace Homura.ORM.Migration
{
    public interface IEntityVersionChangePlan : IModifiedCounter
    {
        VersionOrigin TargetVersion { get; set; }

        VersioningMode Mode { get; set; }

        void UpgradeToTargetVersion(IConnection connection);

        void DowngradeToTargetVersion(IConnection connection);

        Type TargetEntityType { get; set; }

        void CreateTable(IConnection connection);

        void DropTable(IConnection connection);
    }
}
