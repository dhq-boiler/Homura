

using Homura.Core;
using Homura.ORM.Mapping;
using Homura.ORM.Setup;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Homura.Core.Delegate;

namespace Homura.ORM.Migration
{
    public interface IVersionChangePlan : IModifiedCounter
    {
        VersionOrigin TargetVersion { get; }

        VersioningMode Mode { get; set; }

        IEnumerable<IEntityVersionChangePlan> VersionChangePlanList { get; }

        void AddVersionChangePlan(IEntityVersionChangePlan plan);

        void RemoveVersionChangePlan(IEntityVersionChangePlan plan);

        void UpgradeToTargetVersion(IConnection connection);

        void DowngradeToTargetVersion(IConnection connection);

        event BeginToUpgradeToEventHandler BeginToUpgradeTo;
        event FinishedToUpgradeToEventHandler FinishedToUpgradeTo;
        event BeginToDowngradeToEventHandler BeginToDowngradeTo;
        event FinishedToDowngradeToEventHandler FinishedToDowngradeTo;
    }
}
