using Homura.QueryBuilder.Core;
using System.Collections.Generic;
using System.Linq;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class DefaultSyntax : SyntaxBase, IValueExpressionSyntax
    {
        internal DefaultSyntax(SyntaxBase syntax)
            : base(syntax)
        { }

        public IWhereSyntax<ISinkStateSyntax, IOperatorSyntax<ISinkStateSyntax>, IIsSyntax<ISinkStateSyntax>> Where { get { return new WhereSyntax<ISinkStateSyntax, IOperatorSyntax<ISinkStateSyntax>, IIsSyntax<ISinkStateSyntax>>(this); } }

        public ISetClauseSyntax Column(string name)
        {
            return new UpdateColumnSyntax(this, name);
        }

        public ISetClauseSyntax Column(string tableAlias, string name)
        {
            return new UpdateColumnSyntax(this, tableAlias, name);
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
                    ret = new UpdateColumnSyntax(this, name, Delimiter.Comma);
                }
                else
                {
                    ret = new UpdateColumnSyntax(ret as SyntaxBase, name, Delimiter.Comma);
                }
            }
            return ret;
        }

        public IValueExpressionSyntax KeyEqualToValue(Dictionary<string, object> columnValues)
        {
            IValueExpressionSyntax ret = null;
            foreach (var columnValue in columnValues)
            {
                if (ret == null)
                {
                    var column = new UpdateColumnSyntax(this, columnValue.Key, Delimiter.Comma);
                    var equalTo = new EqualToSyntax<IValueExpressionSyntax>(column);
                    ret = new ParameterizedValueExpressionSyntax(equalTo, columnValue.Value);
                }
                else
                {
                    var column = new UpdateColumnSyntax(ret as SyntaxBase, columnValue.Key, Delimiter.Comma);
                    var equalTo = new EqualToSyntax<IValueExpressionSyntax>(column);
                    ret = new ParameterizedValueExpressionSyntax(equalTo, columnValue.Value);
                }
            }
            return ret;
        }

        public override string Represent()
        {
            return "DEFAULT";
        }

        public string ToSql()
        {
            return RelayQuery(this);
        }
    }
}