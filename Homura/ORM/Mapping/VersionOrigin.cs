

using Homura.ORM.Migration;
using System.Collections.Generic;

namespace Homura.ORM.Mapping
{
    /// <summary>
    /// 最初のバージョンを表すクラス。次のバージョンを作成するにはこのクラスを継承します。継承するクラスの名前は任意です。
    /// 例）[VersionOrigin] <--inherit-- [Version_1] <--inherit-- [Version_2] <--...-- [Version_N]
    /// テーブルマッピングでTargetVersion属性、Since属性、Until属性でこのクラスを指定することができます。
    /// </summary>
    public class VersionOrigin : ISchemaVersion, IEqualityComparer<VersionOrigin>
    {
        public bool Equals(VersionOrigin x, VersionOrigin y)
        {
            return x.GetType().FullName.Equals(y.GetType().FullName);
        }

        public int GetHashCode(VersionOrigin obj)
        {
            return obj.GetType().FullName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj.GetType().FullName.Equals(GetType().FullName);
        }

        public override int GetHashCode()
        {
            return GetType().FullName.GetHashCode();
        }

        public int GetIndex()
        {
            return VersionHelper.GetInternalVersionNumberFromVersionType(this);
        }
    }
}
