using Homura.QueryBuilder.Core;
using System.Collections.Generic;
using System.Linq;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class EndSubquerySyntax : SyntaxBase, IColumnSyntax, ICloseSyntax<IConditionValueSyntax>, IConditionValueSyntax, IRowSyntax, INoMarginLeftSyntax
    {
        internal EndSubquerySyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        public IFromSyntax<ICloseSyntax<IConditionValueSyntax>> From { get { return new FromSyntax<ICloseSyntax<IConditionValueSyntax>, IConditionValueSyntax>(this); } }

        public IWhereSyntax<IConditionValueSyntax, IOperatorSyntax<IConditionValueSyntax>, IIsSyntax<IConditionValueSyntax>> Where { get { return new WhereSyntax<IConditionValueSyntax, IOperatorSyntax<IConditionValueSyntax>, IIsSyntax<IConditionValueSyntax>>(this); } }

        public INaturalSyntax Natural { get { return new NaturalSyntax(this); } }

        public IJoinTypeSyntax Inner { get { return new InnerSyntax(this); } }

        public IOuterJoinTypeSyntax Left { get { return new LeftSyntax(this); } }

        public IOuterJoinTypeSyntax Right { get { return new RightSyntax(this); } }

        public IOuterJoinTypeSyntax Full { get { return new FullSyntax(this); } }

        public IOrderBySyntax OrderBy { get { return new OrderBySyntax(this); } }

        public IGroupBySyntax GroupBy { get { return new GroupBySyntax(this); } }

        public IUnionSyntax Union { get { return new UnionSyntax(this); } }

        public ICrossSyntax Cross { get { return new CrossSyntax(this); } }

        public IExceptSyntax Except { get { return new ExceptSyntax(this); } }

        public IIntersectSyntax Intersect { get { return new IntersectSyntax(this); } }

        public IAsSyntax As(string columnAlias)
        {
            return new AsSyntax(this, columnAlias);
        }

        public IColumnSyntax Column(string name)
        {
            return new ColumnSyntax(this, name, Delimiter.Comma);
        }

        public IColumnSyntax Column(string tableAlias, string name)
        {
            return new ColumnSyntax(this, tableAlias, name, Delimiter.Comma);
        }

        public IColumnSyntax Columns(IEnumerable<string> names)
        {
            return Columns(names.ToArray());
        }

        public IColumnSyntax Columns(params string[] names)
        {
            IColumnSyntax ret = null;
            foreach (var name in names)
            {
                if (ret == null)
                {
                    ret = new ColumnSyntax(this, name, Delimiter.Comma);
                }
                else
                {
                    ret = new ColumnSyntax(ret as SyntaxBase, name, Delimiter.Comma);
                }
            }
            return ret;
        }

        public IColumnSyntax SubQuery(IOrderByColumnSyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this);
            Relay.AddRange((subquery as SyntaxBase).Relay);
            return new EndSubquerySyntax(begin);
        }

        public IColumnSyntax SubQuery(IOrderBySyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this);
            Relay.AddRange((subquery as SyntaxBase).Relay);
            return new EndSubquerySyntax(begin);
        }

        public IColumnSyntax SubQuery(IJoinConditionSyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this);
            Relay.AddRange((subquery as SyntaxBase).Relay);
            return new EndSubquerySyntax(begin);
        }

        public IColumnSyntax SubQuery(IConditionValueSyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this);
            Relay.AddRange((subquery as SyntaxBase).Relay);
            return new EndSubquerySyntax(begin);
        }

        public IColumnSyntax SubQuery(ICloseSyntax<IConditionValueSyntax> subquery)
        {
            var begin = new BeginSubquerySyntax(this);
            Relay.AddRange((subquery as SyntaxBase).Relay);
            return new EndSubquerySyntax(begin);
        }

        public string ToSql()
        {
            return Relay.RelayQuery(this);
        }

        public IColumnSyntax Asterisk()
        {
            return new AsteriskSyntax(this, Delimiter.Comma);
        }

        public IColumnSyntax Asterisk(string tableAlias)
        {
            return new AsteriskSyntax(this, tableAlias, Delimiter.Comma);
        }

        public override string Represent()
        {
            return ")";
        }

        public IRowSyntax Row(IEnumerable<object> values)
        {
            return Row(values.ToArray());
        }

        public IRowSyntax Row(params object[] values)
        {
            return new RowSyntax(this, values, Delimiter.Comma);
        }

        public IJoinTableSyntax Join(string tableName)
        {
            return new JoinTableSyntax(this, tableName);
        }

        public IJoinTableSyntax Join(string tableName, string tableAlias)
        {
            return new JoinTableSyntax(this, tableName, tableAlias);
        }

        public IRowSyntax Rows(IEnumerable<IEnumerable<object>> rows)
        {
            return Rows(rows.Cast<object[]>().ToArray());
        }

        public IRowSyntax Rows(params object[][] rows)
        {
            IRowSyntax ret = null;
            foreach (var row in rows)
            {
                if (ret == null)
                {
                    ret = new RowSyntax(this, row);
                }
                else
                {
                    ret = new RowSyntax(ret as SyntaxBase, row, Delimiter.Comma);
                }
            }
            return ret;
        }
    }
}
