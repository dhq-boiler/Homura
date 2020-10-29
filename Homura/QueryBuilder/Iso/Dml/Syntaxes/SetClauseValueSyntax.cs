

using Homura.QueryBuilder.Core;
using System.Collections.Generic;
using System.Linq;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class SetClauseValueSyntax : RepeatableSyntax, ISetClauseValueSyntax
    {
        internal SetClauseValueSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        {
        }

        internal SetClauseValueSyntax(SyntaxBase syntaxBase, Delimiter prefix)
            : base(syntaxBase, prefix)
        {
        }

        internal SetClauseValueSyntax(SyntaxBase syntaxBase, object value)
            : this(syntaxBase)
        {
            AddParameter(value);
        }

        internal SetClauseValueSyntax(SyntaxBase syntaxBase, object value, Delimiter prefix)
            : this(syntaxBase, prefix)
        {
            AddParameter(value);
        }

        internal SetClauseValueSyntax(SyntaxBase syntaxBase, object[] values)
            : this(syntaxBase)
        {
            AddParameters(values);
        }

        internal SetClauseValueSyntax(SyntaxBase syntaxBase, object[] values, Delimiter prefix)
            : this(syntaxBase, prefix)
        {
            AddParameters(values);
        }

        public IValuesSyntax NextRow { get { return new RepeatRowSyntax(this, Delimiter.ClosedParenthesisAndComma); } }

        public ISetClauseValueSyntax Value(object value)
        {
            return new SetClauseValueSyntax(this, value, Delimiter.Comma);
        }

        public ISetClauseValueSyntax Value(IEnumerable<object> values)
        {
            return Value(values.ToArray());
        }

        public ISetClauseValueSyntax Value(params object[] values)
        {
            ISetClauseValueSyntax ret = null;
            foreach (var value in values)
            {
                if (ret == null)
                {
                    ret = new SetClauseValueSyntax(this, value, Delimiter.Comma);
                }
                else
                {
                    ret = new SetClauseValueSyntax(ret as SyntaxBase, value, Delimiter.Comma);
                }
            }
            return ret;
        }

        public override string Represent()
        {
            string ret = $"{Delimiter.ToString()}";
            ret += ValueLoop(LocalParameters.Cast<object>().ToList());
            return ret;
        }

        public string ToSql()
        {
            var close = new CloseParenthesisSyntax(this);
            return close.ToSql();
        }
    }
}
