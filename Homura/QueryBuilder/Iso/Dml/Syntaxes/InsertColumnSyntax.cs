

using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Transitions;
using System;
using System.Collections.Generic;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class InsertColumnSyntax : RepeatableSyntax, IInsertColumnSyntax
    {
        private string _name;
        private string _tableAlias;

        internal InsertColumnSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        internal InsertColumnSyntax(SyntaxBase syntaxBase, string name, Delimiter prefix)
            : base(syntaxBase, prefix)
        {
            _name = name;
        }

        internal InsertColumnSyntax(SyntaxBase syntaxBase, string tableAlias, string name, Delimiter prefix)
            : this(syntaxBase, name, prefix)
        {
            _tableAlias = tableAlias;
        }

        public ISelectSyntax Select
        {
            get
            {
                var close = new CloseParenthesisSyntax(this);
                return new SelectSyntax(close);
            }
        }

        public IValuesSyntax Values
        {
            get
            {
                var close = new CloseParenthesisSyntax(this);
                return new ValuesSyntax(close);
            }
        }

        public IInsertColumnSyntax Column(string name)
        {
            return new InsertColumnSyntax(this, name, Delimiter.Comma);
        }

        public IInsertColumnSyntax Column(string tableAlias, string name)
        {
            return new InsertColumnSyntax(this, tableAlias, name, Delimiter.Comma);
        }

        public IInsertColumnSyntax Columns(params string[] names)
        {
            IInsertColumnSyntax ret = null;
            foreach (var name in names)
            {
                if (ret == null)
                {
                    ret = new InsertColumnSyntax(this, name, Delimiter.Comma);
                }
                else
                {
                    ret = new InsertColumnSyntax(ret as SyntaxBase, name, Delimiter.Comma);
                }
            }
            return ret;
        }

        public override string Represent()
        {
            return $"{Delimiter.ToString()}{_tableAlias}{(!string.IsNullOrEmpty(_tableAlias) ? "." : "")}{_name}";
        }

        IInsertColumnSyntax IColumnTransition<IInsertColumnSyntax>.Columns(IEnumerable<string> names)
        {
            throw new NotImplementedException();
        }
    }
}
