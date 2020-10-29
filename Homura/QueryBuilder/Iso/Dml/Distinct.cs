using Homura.QueryBuilder.Iso.Dml.Syntaxes;

namespace Homura.QueryBuilder.Iso.Dml
{
    public static class Distinct
    {
        public static ICountParameterSyntax Column(string name)
        {
            return new DistinctColumnSyntax(name);
        }

        public static ICountParameterSyntax Column(string tableAlias, string name)
        {
            return new DistinctColumnSyntax(tableAlias, name);
        }
    }
}
