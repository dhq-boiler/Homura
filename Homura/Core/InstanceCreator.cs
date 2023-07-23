using System;
using System.Linq.Expressions;

namespace Homura.Core
{
    internal static class InstanceCreator<T>
    {
        private static readonly Func<T> _CreateInstanceFunc = Expression.Lambda<Func<T>>(Expression.New(typeof(T))).Compile();

        public static Func<T> CreateInstance()
        {
            return _CreateInstanceFunc;
        }
    }
}
