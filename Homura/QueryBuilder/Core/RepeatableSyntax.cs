
using System.Collections.Generic;

namespace Homura.QueryBuilder.Core
{
    internal abstract class RepeatableSyntax : SyntaxBase, IRepeatable
    {
        private Delimiter _prefix;

        internal RepeatableSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        {
            _prefix = Delimiter.None;
        }

        internal RepeatableSyntax(SyntaxBase syntaxBase, Delimiter prefix)
            : base(syntaxBase)
        {
            _prefix = prefix;
        }

        public Delimiter Delimiter { get { return _prefix; } }

        protected string ValueLoop(List<object> _values)
        {
            int i = 0;
            string ret = "";
            foreach (var val in _values)
            {
                ret += val.ToString();

                if (i + 1 < _values.Count)
                {
                    ret += ", ";
                }
                ++i;
            }
            return ret;
        }
    }
}
