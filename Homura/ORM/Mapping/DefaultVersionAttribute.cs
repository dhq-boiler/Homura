

using System;

namespace Homura.ORM.Mapping
{
    /// <summary>
    /// Dao抽象クラスのデフォルトコンストラクタによってインスタンス化したサブクラスがアクセス対象とする，エンティティのバージョンを指定する
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DefaultVersionAttribute : Attribute
    {
        public DefaultVersionAttribute(Type version)
        {
            Version = version;
        }

        public Type Version { get; set; }
    }
}
