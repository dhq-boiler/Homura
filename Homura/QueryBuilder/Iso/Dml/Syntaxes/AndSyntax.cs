

using Homura.QueryBuilder.Core;
using System.Collections.Generic;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class AndSyntax<R, R1, R2> : SyntaxBase, IWhereSyntax<R, R1, R2> where R : class
                                                                              where R1 : class
                                                                              where R2 : class
    {
        internal AndSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        public IExistsSyntax<R> Exists { get { return new ExistsSyntax<R>(this); } }

        public IWhereNotSyntax<R> Not { get { return new WhereNotSyntax<R>(this); } }

        public ISearchCondition<R, R1, R2> Column(string name)
        {
            return new SearchConditionSyntax<R, R1, R2>(this, name);
        }

        public ISearchCondition<R, R1, R2> Column(string tableAlias, string name)
        {
            return new SearchConditionSyntax<R, R1, R2>(this, tableAlias, name);
        }

        public R KeyEqualToValue(Dictionary<string, object> conditions)
        {
            R ret = null;
            foreach (var condition in conditions)
            {
                if (ret == null)
                {
                    var column = new SearchConditionSyntax<R, R1, R2>(this, condition.Key);
                    var equal = new EqualToSyntax<R>(column);
                    ret = equal.Value(condition.Value);
                }
                else
                {
                    var and = new AndSyntax<R, R1, R2>(ret as SyntaxBase);
                    var column = new SearchConditionSyntax<R, R1, R2>(and, condition.Key);
                    var equal = new EqualToSyntax<R>(column);
                    ret = equal.Value(condition.Value);
                }
            }
            return ret;
        }

        public override string Represent()
        {
            return "AND";
        }
    }
}
