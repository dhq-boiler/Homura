using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Transitions;
using System.Collections.Generic;
using System.Linq;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class ColumnSyntax : RepeatableSyntax, IColumnSyntax, ICorrespondingColumnSyntax
    {
        private string _name;
        private string _tableAlias;

        internal ColumnSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        internal ColumnSyntax(SyntaxBase syntaxBase, string name)
            : this(syntaxBase)
        {
            _name = name;
        }

        internal ColumnSyntax(SyntaxBase syntaxBase, string name, Delimiter prefix)
            : base(syntaxBase, prefix)
        {
            _name = name;
        }

        internal ColumnSyntax(SyntaxBase syntaxBase, string tableAlias, string name)
            : this(syntaxBase, name)
        {
            _tableAlias = tableAlias;
        }

        internal ColumnSyntax(SyntaxBase syntaxBase, string tableAlias, string name, Delimiter prefix)
            : this(syntaxBase, name, prefix)
        {
            _tableAlias = tableAlias;
        }

        public IFromSyntax<ICloseSyntax<IConditionValueSyntax>> From { get { return new FromSyntax<ICloseSyntax<IConditionValueSyntax>, IConditionValueSyntax>(this); } }

        public ICrossSyntax Cross
        {
            get
            {
                var close = new CloseParenthesisSyntax(this);
                return new CrossSyntax(close);
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

        public INaturalSyntax Natural
        {
            get
            {
                var close = new CloseParenthesisSyntax(this);
                return new NaturalSyntax(close);
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

        public ISelectSyntax Select
        {
            get
            {
                var close = new CloseParenthesisSyntax(this);
                return new SelectSyntax(close);
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

        public IColumnSyntax SubQuery(IJoinConditionSyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this, Delimiter.Comma);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end;
        }

        public IColumnSyntax SubQuery(IOrderBySyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this, Delimiter.Comma);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end;
        }

        public IColumnSyntax SubQuery(IOrderByColumnSyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this, Delimiter.Comma);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end;
        }

        public IColumnSyntax SubQuery(IConditionValueSyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this, Delimiter.Comma);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end;
        }

        public IColumnSyntax SubQuery(ICloseSyntax<IConditionValueSyntax> subquery)
        {
            var begin = new BeginSubquerySyntax(this, Delimiter.Comma);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end;
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
            return $"{Delimiter.ToString()}{_tableAlias}{(!string.IsNullOrEmpty(_tableAlias) ? "." : "")}{_name}";
        }

        ICorrespondingColumnSyntax IColumnTransition<ICorrespondingColumnSyntax>.Column(string name)
        {
            return new ColumnSyntax(this, name, Delimiter.Comma);
        }

        ICorrespondingColumnSyntax IColumnTransition<ICorrespondingColumnSyntax>.Column(string tableAlias, string name)
        {
            return new ColumnSyntax(this, tableAlias, name, Delimiter.Comma);
        }

        ICorrespondingColumnSyntax IColumnTransition<ICorrespondingColumnSyntax>.Columns(IEnumerable<string> names)
        {
            return (this as IColumnTransition<ICorrespondingColumnSyntax>).Columns(names.ToArray());
        }

        ICorrespondingColumnSyntax IColumnTransition<ICorrespondingColumnSyntax>.Columns(params string[] names)
        {
            ICorrespondingColumnSyntax ret = null;
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
    }
}
