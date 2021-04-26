using Homura.QueryBuilder.Iso.Dml.Transitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    public interface IHavingSyntax<Return> : IFunctionTransition<Return> where Return : class
    {
    }
}
