using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homura.ORM
{
    public class DBValue
    {
        //public static readonly 
        
        public bool IsSpecified { get; internal set; }
        public object Value { get; internal set; }
        public object DefaultValue { get; internal set; }

        internal DBValue(object value, bool isSpecified, object defaultValue)
        {
            Value = value;
            IsSpecified = isSpecified;
            DefaultValue = defaultValue;
        }

        public static DBValue Constant(object value)
        {
            return new DBValue(value, true, value);
        }
    }
}
