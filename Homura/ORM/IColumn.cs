

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Homura.ORM
{
    public interface IColumn
    {
        string ColumnName { get; }

        Type EntityDataType { get; }

        string DBDataType { get; }

        IEnumerable<IDdlConstraint> Constraints { get; }

        int Order { get; }

        object DefaultValue { get; }

        PropertyInfo PropInfo { get; }

        string ConstraintsToSql();

        PlaceholderRightValue ToParameter(EntityBaseObject entity);

        PlaceholderRightValue ToParameter(Dictionary<string, object> idDic);
        string WrapOutput();

        HandlingDefaultValue PassType { get; }
    }
}
