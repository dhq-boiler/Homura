

using System.Collections.Generic;
using System.Linq;

namespace Homura.ORM
{
    public class MultiplePlaceholderRightValue : RightValueImpl
    {
        public override string Name { get; set; }

        public override object[] Values { get; set; }

        private List<IRightValue> _overwritedPlaceholderNameList = new List<IRightValue>();

        public List<IRightValue> OverwritedPlaceholderNames { get { return _overwritedPlaceholderNameList; } }

        public IEnumerable<string> PlaceholderNameList
        {
            get
            {
                if (Values == null || Values.Length == 0)
                {
                    return new string[] { };
                }
                List<string> ret = new List<string>();
                for (int i = 0; i < Values.Count(); ++i)
                {
                    ret.Add($"@{EscapedPlaceholderName}_{i}");
                }
                return ret;
            }
        }

        public MultiplePlaceholderRightValue(string placeholderName, object[] values)
        {
            Name = placeholderName;
            Values = values;
        }

        public override string ToSql()
        {
            return ToSql(new Dictionary<string, int>());
        }

        public override string ToSql(Dictionary<string, int> placeholderNameDictionary)
        {
            _overwritedPlaceholderNameList.Clear();

            string sql = "(";
            for (int i = 0; i < Values.Count(); ++i)
            {
                var value = Values[i];
                sql += ReferencePlaceholderNameAndOverwrite(placeholderNameDictionary, value);
                if (!ReferenceEquals(Values.Last(), value))
                {
                    sql += ", ";
                }
            }
            sql += ")";

            return sql;
        }

        private string ReferencePlaceholderNameAndOverwrite(Dictionary<string, int> placeholderNameDictionary, object value)
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
            var newPlaceholderName = $"{EscapedPlaceholderName}_{count}";
            _overwritedPlaceholderNameList.Add(new PlaceholderRightValue(newPlaceholderName, value));
            return $"@{newPlaceholderName}";
        }
    }
}
