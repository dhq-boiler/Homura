

using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.ORM.Setup;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class VersionChangePlan_VersionOrigin : ChangePlan<VersionOrigin>
    {
        public VersionChangePlan_VersionOrigin(VersioningMode mode) : base(mode)
        {
        }
    }
}
