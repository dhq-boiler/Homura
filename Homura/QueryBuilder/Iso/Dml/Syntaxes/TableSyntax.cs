

using Homura.QueryBuilder.Core;
using System.Collections.Generic;
using System.Linq;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class TableSyntax<R> : SyntaxBase, ICloseSyntax<R>, IDeleteTableSyntax<R>, IInsertColumnSyntax, IUpdateTableSyntax where R : class
    {
        private string _catalogName;
        private string _schemaName;
        private string _tableName;
        private string _alias;

        public IWhereSyntax<R, IOperatorSyntax<R>, IIsSyntax<R>> Where { get { return new WhereSyntax<R, IOperatorSyntax<R>, IIsSyntax<R>>(this); } }

        public INaturalSyntax Natural { get { return new NaturalSyntax(this); } }

        public IJoinTypeSyntax Inner { get { return new InnerSyntax(this); } }

        public IOuterJoinTypeSyntax Left { get { return new LeftSyntax(this); } }

        public IOuterJoinTypeSyntax Right { get { return new RightSyntax(this); } }

        public IOuterJoinTypeSyntax Full { get { return new FullSyntax(this); } }

        public IOrderBySyntax OrderBy { get { return new OrderBySyntax(this); } }

        public IGroupBySyntax GroupBy { get { return new GroupBySyntax(this); } }

        public ISelectSyntax Select { get { return new SelectSyntax(this); } }

        public IValuesSyntax Values { get { return new ValuesSyntax(this); } }

        public ISetSyntax Set { get { return new SetSyntax(this); } }

        public IUnionSyntax Union { get { return new UnionSyntax(this); } }

        public ICrossSyntax Cross { get { return new CrossSyntax(this); } }

        internal TableSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        internal TableSyntax(string tableName, SyntaxBase syntaxBase, string alias = null)
            : this(syntaxBase)
        {
            _tableName = tableName;
            _alias = alias;
        }

        internal TableSyntax(string schemaName, string tableName, SyntaxBase syntaxBase, string alias = null)
            : this(tableName, syntaxBase, alias)
        {
            _schemaName = schemaName;
        }

        internal TableSyntax(string catalogName, string schemaName, string tableName, SyntaxBase syntaxBase, string alias = null)
            : this(schemaName, tableName, syntaxBase, alias)
        {
            _catalogName = catalogName;
        }

        public override string Represent()
        {
            string ret = $"{_tableName}{(!string.IsNullOrEmpty(_alias) ? " " : "")}{_alias}";
            if (!string.IsNullOrWhiteSpace(_schemaName))
            {
                ret = $"{_schemaName}." + ret;
                if (!string.IsNullOrWhiteSpace(_catalogName))
                {
                    ret = $"{_catalogName}." + ret;
                }
            }
            return ret;
        }

        public string ToSql()
        {
            return Relay.RelayQuery(this);
        }

        public IInsertColumnSyntax Column(string name)
        {
            return new InsertColumnSyntax(this, name, Delimiter.OpenedParenthesis);
        }

        public IInsertColumnSyntax Column(string tableAlias, string name)
        {
            return new InsertColumnSyntax(this, tableAlias, name, Delimiter.OpenedParenthesis);
        }

        public IInsertColumnSyntax Columns(IEnumerable<string> names)
        {
            return Columns(names.ToArray());
        }

        public IInsertColumnSyntax Columns(params string[] names)
        {
            IInsertColumnSyntax ret = null;
            foreach (var name in names)
            {
                if (ret == null)
                {
                    ret = new InsertColumnSyntax(this, name, Delimiter.OpenedParenthesis);
                }
                else
                {
                    ret = new InsertColumnSyntax(ret as SyntaxBase, name, Delimiter.Comma);
                }
            }
            return ret;
        }

        public IJoinTableSyntax Join(string tableName)
        {
            return new JoinTableSyntax(this, tableName);
        }

        public IJoinTableSyntax Join(string tableName, string tableAlias)
        {
            return new JoinTableSyntax(this, tableName, tableAlias);
        }
    }
}
