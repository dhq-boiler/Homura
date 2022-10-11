

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

        Task UpgradeToTargetVersion(IConnection connection);

        Task DowngradeToTargetVersion(IConnection connection);

        Type TargetEntityType { get; set; }

        Task CreateTable(IConnection connection);

        Task DropTable(IConnection connection);
    }
}
