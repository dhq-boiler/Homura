

using Homura.ORM.Migration;

namespace Homura.Core
{
    public class Delegate
    {
        public delegate void BeginToUpgradeToEventHandler(object sender, VersionChangeEventArgs e);
        public delegate void FinishedToUpgradeToEventHandler(object sender, VersionChangeEventArgs e);
        public delegate void BeginToDowngradeToEventHandler(object sender, VersionChangeEventArgs e);
        public delegate void FinishedToDowngradeToEventHandler(object sender, VersionChangeEventArgs e);

        public delegate void ModifiedEventHandler(object sender, ModifiedEventArgs e);
    }
}
