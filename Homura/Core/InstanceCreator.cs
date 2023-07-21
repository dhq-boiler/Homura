using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Homura.Core
{
    internal static class InstanceCreator<T>
    {
        public static Func<T> CreateInstance()
        {
            return Expression.Lambda<Func<T>>(Expression.New(typeof(T))).Compile();
        }
    }
}
