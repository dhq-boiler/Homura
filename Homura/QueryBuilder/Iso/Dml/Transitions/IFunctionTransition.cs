using Homura.QueryBuilder.Iso.Dml.Syntaxes;

namespace Homura.QueryBuilder.Iso.Dml.Transitions
{
    public interface IFunctionTransition<Return> where Return : class
    {
        Return Count();

        Return Count(string name);

        Return Count(ICountParameterSyntax column);
    }
}
