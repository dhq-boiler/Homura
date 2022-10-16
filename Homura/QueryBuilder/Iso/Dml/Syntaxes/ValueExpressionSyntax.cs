using Homura.QueryBuilder.Core;
using System;
using System.Collections.Generic;
using static Homura.QueryBuilder.Iso.Dml.Syntaxes.ReplaceSyntax;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class ValueExpressionSyntax : SyntaxBase, IValueExpressionSyntax
    {
        private string _expression;

        internal ValueExpressionSyntax(SyntaxBase syntax, string expression)
            : base(syntax)
        {
            _expression = expression;
        }

        public IWhereSyntax<ISinkStateSyntax, IOperatorSyntax<ISinkStateSyntax>, IIsSyntax<ISinkStateSyntax>> Where { get { return new WhereSyntax<ISinkStateSyntax, IOperatorSyntax<ISinkStateSyntax>, IIsSyntax<ISinkStateSyntax>>(this); } }

        public ISetClauseSyntax Column(string name)
        {
            return new UpdateColumnSyntax(this, name, Delimiter.Comma);
        }

        public ISetClauseSyntax Column(string tableAlias, string name)
        {
            return new UpdateColumnSyntax(this, tableAlias, name);
        }

        public ISetClauseSyntax Columns(IEnumerable<string> names)
        {
            throw new NotSupportedException();
        }

        public ISetClauseSyntax Columns(params string[] names)
        {
            throw new NotSupportedException();
        }

        public IValueExpressionSyntax KeyEqualToValue(Dictionary<string, object> columnValues)
        {
            IValueExpressionSyntax ret = null;
            foreach (var columnValue in columnValues)
            {
                if (ret == null)
                {
                    var column = new UpdateColumnSyntax(this, columnValue.Key, Delimiter.Comma);
                    ret = new SubstituteSyntax(column, columnValue.Value);
                }
                else
                {
                    var column = new UpdateColumnSyntax(ret as SyntaxBase, columnValue.Key, Delimiter.Comma);
                    ret = new SubstituteSyntax(column, columnValue.Value);
                }
            }
            return ret;
        }

        public ISetClauseSyntax Replace(string expression, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Expression, expression, pattern, replacement);
        }

        public ISetClauseSyntax ReplaceColumn(string columnName, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Column, columnName, pattern, replacement);
        }

        public ISetClauseSyntax ReplaceColumn(string tableAlias, string columnName, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Column, tableAlias, columnName, pattern, replacement);
        }

        public override string Represent()
        {
            return _expression;
        }

        public string ToSql()
        {
            return Relay.RelayQuery(this);
        }
    }
}
