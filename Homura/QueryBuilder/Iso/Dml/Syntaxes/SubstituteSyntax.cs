

using Homura.QueryBuilder.Core;
using System.Collections.Generic;
using System.Linq;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class SubstituteSyntax : SyntaxBase, IValueExpressionSyntax
    {
        public IWhereSyntax<ISinkStateSyntax, IOperatorSyntax<ISinkStateSyntax>, IIsSyntax<ISinkStateSyntax>> Where { get { return new WhereSyntax<ISinkStateSyntax, IOperatorSyntax<ISinkStateSyntax>, IIsSyntax<ISinkStateSyntax>>(this); } }

        internal SubstituteSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        internal SubstituteSyntax(SyntaxBase syntaxBase, object value)
            : this(syntaxBase)
        {
            AddParameter(value);
        }

        public ISetClauseSyntax Column(string name)
        {
            return new UpdateColumnSyntax(this, name, Delimiter.Comma);
        }

        public ISetClauseSyntax Column(string tableAlias, string name)
        {
            return new UpdateColumnSyntax(this, tableAlias, name, Delimiter.Comma);
        }

        public ISetClauseSyntax Columns(IEnumerable<string> names)
        {
            return Columns(names.ToArray());
        }

        public ISetClauseSyntax Columns(params string[] names)
        {
            ISetClauseSyntax ret = null;
            foreach (var name in names)
            {
                if (ret == null)
                {
                    ret = new UpdateColumnSyntax(this, name);
                }
                else
                {
                    ret = new UpdateColumnSyntax(ret as SyntaxBase, name);
                }
            }
            return ret;
        }

        public override string Represent()
        {
            return $"= {LocalParameters.First()}";
        }

        public string ToSql()
        {
            return Relay.RelayQuery(this);
        }

        public IValueExpressionSyntax KeyEqualToValue(Dictionary<string, object> columnValues)
        {
            IValueExpressionSyntax ret = null;
            foreach (var columnValue in columnValues)
            {
                if (ret == null)
                {
                    var column = new UpdateColumnSyntax(this, columnValue.Key);
                    ret = new SubstituteSyntax(column, columnValue.Value);
                }
                else
                {
                    var column = new UpdateColumnSyntax(ret as SyntaxBase, columnValue.Key);
                    ret = new SubstituteSyntax(column, columnValue.Value);
                }
            }
            return ret;
        }
    }
}
