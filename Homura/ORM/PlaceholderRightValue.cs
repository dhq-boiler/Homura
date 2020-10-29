

using System.Collections.Generic;

namespace Homura.ORM
{
    public class PlaceholderRightValue : RightValueImpl
    {
        public override string Name { get; set; }

        public override object[] Values { get; set; }

        public PlaceholderRightValue(string placeholderName, object value)
        {
            Name = placeholderName;
            Values = new object[] { value };
        }

        public override string ToSql()
        {
            return ToSql(new Dictionary<string, int>());
        }

        public override string ToSql(Dictionary<string, int> placeholderNameDictionary)
        {
            int count = 0;
            if (placeholderNameDictionary.ContainsKey(EscapedPlaceholderName))
            {
                count = ++placeholderNameDictionary[EscapedPlaceholderName];
            }
            else
            {
                count = 1;
                placeholderNameDictionary.Add(EscapedPlaceholderName, count);
            }
            Name = $"{EscapedPlaceholderName}_{count}";
            return $"@{Name}";
        }
    }
}
