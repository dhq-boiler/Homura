
using System;

namespace Homura.ORM
{
    [Obsolete]
    public static class DaoConst
    {
        public abstract class Is
        {
            public static readonly Is Null = new IsNull();
            public static readonly Is NotNull = new IsNotNull();

            public abstract Is GetOperand();
        }

        internal class IsNull : Is
        {
            public override Is GetOperand()
            {
                return this;
            }
        }

        internal class IsNotNull : Is
        {
            public override Is GetOperand()
            {
                return this;
            }
        }
    }
}
