
using System;

namespace Homura.ORM.Mapping
{
    /// <summary>
    /// エンティティクラスに付与すると、Source Generator が対応する DAO クラスを自動生成する。
    /// 生成されるクラス名は {EntityName}Dao。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class GenerateDaoAttribute : Attribute
    {
        /// <summary>
        /// 生成する DAO クラスの名前。省略時は {EntityName}Dao。
        /// </summary>
        public string DaoName { get; set; }
    }
}
