

using System;
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
                if (parameter.Value is Type t)
                {
                    p.Value = t.AssemblyQualifiedName;
                }
                else
                {
                    p.Value = parameter.Value;
                }
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

        public static string RelayQuery(this List<SyntaxBase> list, SyntaxBase syntaxBase)
        {
            // Render list then syntaxBase, in one pass with no intermediate list copy.
            var builder = new StringBuilder();
            var state = new MergeState();
            SyntaxBase previous = null;
            foreach (var syntax in list)
            {
                Merge(builder, ref state, previous, syntax);
                previous = syntax;
            }
            Merge(builder, ref state, previous, syntaxBase);
            return builder.ToString();
        }

        #region private

        private const char s_DELIMITER_SPACE_CHAR = ' ';

        private struct MergeState
        {
            public bool HasContent;     // any non-whitespace already appended
            public bool LastCharIsSpace; // last char written was ' '
        }

        private static void Merge(StringBuilder builder, ref MergeState state, SyntaxBase previous, SyntaxBase current)
        {
            // Cache Represent() — it can be non-trivial.
            var repr = current.Represent();
            bool currentIsNotBlank = !string.IsNullOrWhiteSpace(repr);

            bool bothNotEmpty = state.HasContent && currentIsNotBlank;
            bool lastIsNotSpace = !state.LastCharIsSpace;
            bool isNotNoMargin = !(previous is INoMarginRightSyntax) && !(current is INoMarginLeftSyntax);

            if (bothNotEmpty && lastIsNotSpace && isNotNoMargin)
            {
                bool addSpace = true;
                if (current is IRepeatable repeatable)
                {
                    if (repeatable.Delimiter == Delimiter.Comma || repeatable.Delimiter == Delimiter.ClosedParenthesisAndComma)
                    {
                        addSpace = false;
                    }
                }

                if (addSpace)
                {
                    builder.Append(s_DELIMITER_SPACE_CHAR);
                    state.LastCharIsSpace = true;
                    state.HasContent = true;
                }
            }

            if (!string.IsNullOrEmpty(repr))
            {
                builder.Append(repr);
                char last = repr[repr.Length - 1];
                state.LastCharIsSpace = last == s_DELIMITER_SPACE_CHAR;
                if (currentIsNotBlank)
                {
                    state.HasContent = true;
                }
            }
        }

        #endregion //private
    }
}
