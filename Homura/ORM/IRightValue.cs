


using Homura.Core;

namespace Homura.ORM
{
    public interface IRightValue : ISqlize
    {
        string Name { get; set; }

        object[] Values { get; set; }
    }
}
