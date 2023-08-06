using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Homura.Test.UnitTest.Enum
{
    public static class Extensions
    {
        public static void AddTo<T>(this IEnumerable<T> array, ObservableCollection<T> target)
        {
            array.ToList().ForEach(target.Add);
        }
    }
}
