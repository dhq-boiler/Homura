using Homura.QueryBuilder.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class HavingSyntax<Return> : SyntaxBase, IHavingSyntax<Return> where Return : class
    {
        internal HavingSyntax(SyntaxBase syntax)
               : base(syntax)
        { }

        public Return Count()
        {
            var count = new CountSyntax(this);
            var open = new OpenFunctionSyntax(count);
            var asterisk = new FunctionAsteriskSyntax(open);
            return new CloseFunctionSyntax(asterisk) as Return;
        }

        public Return Count(string name)
        {
            var count = new CountSyntax(this);
            var open = new OpenFunctionSyntax(count);
            var column = new FunctionColumnSyntax(open, name);
            return new CloseFunctionSyntax(column) as Return;
        }

        public Return Count(ICountParameterSyntax column)
        {
            var count = new CountSyntax(this);
            var open = new OpenFunctionSyntax(count);
            var syntax = column as SyntaxBase;
            open.RelaySyntax(syntax);
            return new CloseFunctionSyntax(syntax) as Return;
        }

        public override string Represent()
        {
            return "HAVING";
        }
    }
}
