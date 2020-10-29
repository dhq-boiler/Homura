

using Homura.QueryBuilder.Core;
using System.Collections.Generic;
using System.Linq;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class InValueSyntax : RepeatableSyntax, IInValueSyntax
    {
        internal InValueSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        internal InValueSyntax(SyntaxBase syntaxBase, object value)
            : this(syntaxBase)
        {
            AddParameter(value);
        }

        internal InValueSyntax(SyntaxBase syntaxBase, object value, Delimiter prefix)
            : base(syntaxBase, prefix)
        {
            AddParameter(value);
        }

        internal InValueSyntax(SyntaxBase syntax, object[] values)
            : base(syntax)
        {
            AddParameters(values);
        }

        internal InValueSyntax(SyntaxBase syntax, object[] values, Delimiter prefix)
            : base(syntax, prefix)
        {
            AddParameters(values);
        }

        public IWhereSyntax<IConditionValueSyntax, IOperatorSyntax<IConditionValueSyntax>, IIsSyntax<IConditionValueSyntax>> Where
        {
            get
            {
                var close = new CloseParenthesisSyntax(this);
                return new WhereSyntax<IConditionValueSyntax, IOperatorSyntax<IConditionValueSyntax>, IIsSyntax<IConditionValueSyntax>>(close);
            }
        }

        public INaturalSyntax Natural
        {
            get
            {
                var close = new CloseParenthesisSyntax(this);
                return new NaturalSyntax(close);
            }
        }

        public IJoinTypeSyntax Inner
        {
            get
            {
                var close = new CloseParenthesisSyntax(this);
                return new InnerSyntax(close);
            }
        }

        public IOuterJoinTypeSyntax Left
        {
            get
            {
                var close = new CloseParenthesisSyntax(this);
                return new LeftSyntax(close);
            }
        }

        public IOuterJoinTypeSyntax Right
        {
            get
            {
                var close = new CloseParenthesisSyntax(this);
                return new RightSyntax(close);
            }
        }

        public IOuterJoinTypeSyntax Full
        {
            get
            {
                var close = new CloseParenthesisSyntax(this);
                return new FullSyntax(close);
            }
        }

        public IOrderBySyntax OrderBy
        {
            get
            {
                var close = new CloseParenthesisSyntax(this);
                return new OrderBySyntax(close);
            }
        }

        public IGroupBySyntax GroupBy
        {
            get
            {
                var close = new CloseParenthesisSyntax(this);
                return new GroupBySyntax(close);
            }
        }

        public IUnionSyntax Union
        {
            get
            {
                var close = new CloseParenthesisSyntax(this);
                return new UnionSyntax(close);
            }
        }

        public ICrossSyntax Cross
        {
            get
            {
                var close = new CloseParenthesisSyntax(this);
                return new CrossSyntax(close);
            }
        }

        public IExceptSyntax Except
        {
            get
            {
                var close = new CloseParenthesisSyntax(this);
                return new ExceptSyntax(close);
            }
        }

        public IIntersectSyntax Intersect
        {
            get
            {
                var close = new CloseParenthesisSyntax(this);
                return new IntersectSyntax(close);
            }
        }

        public string ToSql()
        {
            var close = new CloseParenthesisSyntax(this);
            return close.ToSql();
        }

        public IInValueSyntax Value(object value)
        {
            return new InValueSyntax(this, value, Delimiter.Comma);
        }

        public IJoinTableSyntax Join(string tableName)
        {
            return new JoinTableSyntax(this, tableName);
        }

        public IJoinTableSyntax Join(string tableName, string tableAlias)
        {
            return new JoinTableSyntax(this, tableName, tableAlias);
        }

        public override string Represent()
        {
            var ret = $"{Delimiter.ToString()}";
            foreach (var parameter in LocalParameters)
            {
                ret += $"{parameter.ToString()}";
                if (!LocalParameters.Last().Equals(parameter))
                {
                    ret += ", ";
                }
            }
            return ret;
        }

        public IInValueSyntax Value(params object[] values)
        {
            return new InValueSyntax(this, values);
        }

        public IInValueSyntax Value(IEnumerable<object> values)
        {
            return Value(values.ToArray());
        }
    }
}
