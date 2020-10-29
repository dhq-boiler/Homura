
namespace Homura.QueryBuilder.Core
{
    internal abstract class Delimiter
    {
        public static readonly Delimiter Comma = new Delimiter_Comma();
        public static readonly Delimiter OpenedParenthesis = new Delimiter_OpenedParenthesis();
        public static readonly Delimiter ClosedParenthesis = new Delimiter_ClosedParenthesis();
        public static readonly Delimiter ClosedParenthesisAndComma = new Delimiter_ClosedParenthesisAndComma();
        public static readonly Delimiter None = new Delimiter_None();

        private class Delimiter_Comma : Delimiter
        {
            public override string ToString()
            {
                return ", ";
            }
        }

        private class Delimiter_OpenedParenthesis : Delimiter
        {
            public override string ToString()
            {
                return "(";
            }
        }

        private class Delimiter_ClosedParenthesis : Delimiter
        {
            public override string ToString()
            {
                return ")";
            }
        }

        private class Delimiter_ClosedParenthesisAndComma : Delimiter
        {
            public override string ToString()
            {
                return "), ";
            }
        }

        private class Delimiter_None : Delimiter
        {
            public override string ToString()
            {
                return "";
            }
        }
    }
}
