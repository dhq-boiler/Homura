

using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Syntaxes;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Homura.QueryBuilder.Iso.Dml
{
    public static class Extensions
    {
        public static IWhereSyntax<IConditionValueSyntax, IOperatorSyntax<IConditionValueSyntax>, IIsSyntax<IConditionValueSyntax>> And(this IConditionValueSyntax syntax)
        {
            return new AndSyntax<IConditionValueSyntax, IOperatorSyntax<IConditionValueSyntax>, IIsSyntax<IConditionValueSyntax>>((SyntaxBase)syntax);
        }

        public static IWhereSyntax<IConditionValueSyntax, IOperatorSyntax<IConditionValueSyntax>, IIsSyntax<IConditionValueSyntax>> Or(this IConditionValueSyntax syntax)
        {
            return new OrSyntax<IConditionValueSyntax, IOperatorSyntax<IConditionValueSyntax>, IIsSyntax<IConditionValueSyntax>>((SyntaxBase)syntax);
        }
        public static IWhereSyntax<ISinkStateSyntax, IOperatorSyntax<ISinkStateSyntax>, IIsSyntax<ISinkStateSyntax>> And(this ISinkStateSyntax syntax)
        {
            return new AndSyntax<ISinkStateSyntax, IOperatorSyntax<ISinkStateSyntax>, IIsSyntax<ISinkStateSyntax>>((SyntaxBase)syntax);
        }

        public static IWhereSyntax<ISinkStateSyntax, IOperatorSyntax<ISinkStateSyntax>, IIsSyntax<ISinkStateSyntax>> Or(this ISinkStateSyntax syntax)
        {
            return new OrSyntax<ISinkStateSyntax, IOperatorSyntax<ISinkStateSyntax>, IIsSyntax<ISinkStateSyntax>>((SyntaxBase)syntax);
        }

        public static IWhereSyntax<IJoinConditionSyntax, IOperatorColumnSyntax<IJoinConditionSyntax>, IIsSyntax<IJoinConditionSyntax>> And(this IJoinConditionSyntax syntax)
        {
            return new AndSyntax<IJoinConditionSyntax, IOperatorColumnSyntax<IJoinConditionSyntax>, IIsSyntax<IJoinConditionSyntax>>((SyntaxBase)syntax);
        }

        public static void SetParameters(this ISyntaxBase query, IDbCommand command)
        {
            (query as SyntaxBase).SetParameters(command);
        }

        public static void SetParameters(this SyntaxBase syntax, IDbCommand command)
        {
            var parameters = syntax.Parameters;
            foreach (var parameter in parameters)
            {
                var p = command.CreateParameter();
                p.ParameterName = parameter.Key.ToString();
                p.Value = parameter.Value;
                command.Parameters.Add(p);
            }
        }

        public static Dictionary<string, object> GetParameters(this ISyntaxBase syntax)
        {
            return (syntax as SyntaxBase).Parameters;
        }

        public static string ToStringKeyIsValue(this Dictionary<string, object> dictionary)
        {
            string ret = "[";

            int i = 0;
            foreach (var entry in dictionary)
            {
                if (i + 1 == dictionary.Count)
                {
                    ret += ", ";
                }
                ret += $"{entry.Key}={entry.Value}";
            }

            ret += "]";

            return ret;
        }

        private static string RelayQuery(this List<SyntaxBase> list)
        {
            SyntaxBase previous = null;
            StringBuilder builder = new StringBuilder();
            foreach (var syntax in list)
            {
                Merge(builder, previous, syntax);
                previous = syntax;
            }
            return builder.ToString();
        }

        public static string RelayQuery(this List<SyntaxBase> list, SyntaxBase syntaxBase)
        {
            var relay = list.ToList();
            relay.Add(syntaxBase);
            return relay.RelayQuery();
        }

        #region private

        private static readonly string s_DELIMITER_SPACE = " ";

        private static void Merge(StringBuilder builder, SyntaxBase previous, SyntaxBase current)
        {
            var building = builder.ToString();
            bool bothNotEmpty = !string.IsNullOrWhiteSpace(building) && !string.IsNullOrWhiteSpace(current.Represent());
            bool lastIsNotSpace = !building.EndsWith(s_DELIMITER_SPACE);
            bool IsNotNoMargin = !(previous is INoMarginRightSyntax) && !(current is INoMarginLeftSyntax);

            if (bothNotEmpty && lastIsNotSpace && IsNotNoMargin)
            {
                if (current is IRepeatable)
                {
                    var repeatable = current as IRepeatable;
                    if (repeatable.Delimiter != Delimiter.Comma && repeatable.Delimiter != Delimiter.ClosedParenthesisAndComma)
                    {
                        builder.Append(s_DELIMITER_SPACE);
                    }
                }
                else
                {
                    builder.Append(s_DELIMITER_SPACE);
                }
            }

            builder.Append(current.Represent());
        }

        #endregion //private
    }
}
