using Homura.QueryBuilder.Core;
using System.Collections.Generic;
using System.Linq;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class GroupByColumnSyntax : RepeatableSyntax, IGroupByColumnSyntax
    {
        private string name;
        private string tableAlias;

        internal GroupByColumnSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        internal GroupByColumnSyntax(SyntaxBase syntaxBase, string name)
            : this(syntaxBase)
        {
            this.name = name;
        }

        internal GroupByColumnSyntax(SyntaxBase syntaxBase, string name, Delimiter prefix)
            : base(syntaxBase, prefix)
        {
            this.name = name;
        }

        internal GroupByColumnSyntax(SyntaxBase syntaxBase, string tableAlias, string name)
            : this(syntaxBase, name)
        {
            this.tableAlias = tableAlias;
        }

        public GroupByColumnSyntax(SyntaxBase syntaxBase, string tableAlias, string name, Delimiter prefix)
            : base(syntaxBase, prefix)
        {
            this.name = name;
            this.tableAlias = tableAlias;
        }

        public IJoinTypeSyntax Inner { get { return new InnerSyntax(this); } }

        public IOuterJoinTypeSyntax Left { get { return new LeftSyntax(this); } }

        public INaturalSyntax Natural { get { return new NaturalSyntax(this); } }

        public IOrderBySyntax OrderBy { get { return new OrderBySyntax(this); } }

        public IOuterJoinTypeSyntax Right { get { return new RightSyntax(this); } }

        public IOuterJoinTypeSyntax Full { get { return new FullSyntax(this); } }

        public IUnionSyntax Union { get { return new UnionSyntax(this); } }

        public IExceptSyntax Except { get { return new ExceptSyntax(this); } }

        public IIntersectSyntax Intersect { get { return new IntersectSyntax(this); } }

        public IWhereSyntax<IConditionValueSyntax, IOperatorSyntax<IConditionValueSyntax>, IIsSyntax<IConditionValueSyntax>> Where { get { return new WhereSyntax<IConditionValueSyntax, IOperatorSyntax<IConditionValueSyntax>, IIsSyntax<IConditionValueSyntax>>(this); } }

        public IGroupByColumnSyntax Column(string name)
        {
            return new GroupByColumnSyntax(this, name, Delimiter.Comma);
        }

        public IGroupByColumnSyntax Column(string tableAlias, string name)
        {
            return new GroupByColumnSyntax(this, tableAlias, name, Delimiter.Comma);
        }

        public IGroupByColumnSyntax Columns(params string[] names)
        {
            IGroupByColumnSyntax ret = null;
            foreach (var name in names)
            {
                if (ret == null)
                {
                    ret = new GroupByColumnSyntax(this, name, Delimiter.Comma);
                }
                else
                {
                    ret = new GroupByColumnSyntax(ret as SyntaxBase, name, Delimiter.Comma);
                }
            }
            return ret;
        }

        public IGroupByColumnSyntax Columns(IEnumerable<string> names)
        {
            return Columns(names.ToArray());
        }

        public string ToSql()
        {
            return Relay.RelayQuery(this);
        }

        public override string Represent()
        {
            return $"{Delimiter.ToString()}{tableAlias}{(!string.IsNullOrEmpty(tableAlias) ? "." : "")}{name}";
        }
    }
}
