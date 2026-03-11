

using Reactive.Bindings;
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

        Func<object, object> PropertyGetter { get; }

        string ConstraintsToSql();

        PlaceholderRightValue ToParameter(EntityBaseObject entity);

        PlaceholderRightValue ToParameter(Dictionary<string, object> idDic);
        string WrapOutput();

        /// <summary>
        /// 新カラムのデフォルト値を SQL 式として返す。
        /// UpgradeTable でソーステーブルに存在しない新カラムに使用する。
        /// WrapOutput と異なり、PassType に関係なく常にデフォルト値を返す。
        /// </summary>
        string WrapOutputAsDefault();

        HandlingDefaultValue PassType { get; }
    }
}
