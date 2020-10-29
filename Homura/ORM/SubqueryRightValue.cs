

using Homura.QueryBuilder.Iso.Dml;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Homura.ORM
{
    //public class SubqueryRightValue : RightValueImpl
    //{
    //    public override string Name { get; set; }

    //    public override object[] Values { get; set; }

    //    public Select Subquery { get; set; }

    //    public SubqueryRightValue(Select subquery)
    //    {
    //        Contract.Assert(subquery != null);
    //        Subquery = subquery;
    //    }

    //    public override string ToSql()
    //    {
    //        return ToSql(new Dictionary<string, int>());
    //    }

    //    public override string ToSql(Dictionary<string, int> placeholderNameDictionary)
    //    {
    //        return $"({Subquery.ToSql(placeholderNameDictionary)})";
    //    }
    //}
}
