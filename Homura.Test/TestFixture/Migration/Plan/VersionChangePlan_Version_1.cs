

using Homura.ORM.Migration;
using Homura.ORM.Setup;

namespace Homura.Test.TestFixture.Migration.Plan
{
    internal class VersionChangePlan_Version_1 : ChangePlan<Version_1>
    {
        public VersionChangePlan_Version_1(VersioningMode mode) : base(mode)
        {
        }
    }
}
