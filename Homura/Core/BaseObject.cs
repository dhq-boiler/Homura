

using System;
using System.Reflection;

namespace Homura.Core
{
    public class BaseObject : NotifyPropertyChangedImpl
    {
        public override string ToString()
        {
            return ShowPropertiesAndFields();
        }

        protected void VerifyArg(object arg, string information = null)
        {
            if (arg == null)
            {
                if (information != null)
                {
                    throw new ArgumentNullException(information);
                }
                else
                {
                    throw new ArgumentNullException();
                }
            }
        }

        private string ShowPropertiesAndFields()
        {
            string ret = $"{GetType().Name}{{";

            PropertyInfo[] properties = GetType().GetProperties(
                BindingFlags.Public
                | BindingFlags.Instance);

            foreach (var property in properties)
            {
                ret += $"{property.Name}={property.GetValue(this)},";
            }

            FieldInfo[] fields = GetType().GetFields(
                BindingFlags.Public
                | BindingFlags.Instance);

            foreach (var field in fields)
            {
                ret += $"{field.Name}={field.GetValue(this)},";
            }
            ret = ret.Remove(ret.Length - 1, 1);
            ret += $"}}";
            return ret;
        }
    }
}
