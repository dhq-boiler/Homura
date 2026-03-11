

using System;

namespace Homura.ORM.Mapping
{
    /// <summary>
    /// エンティティクラスに付与すると、Source Generator が対応する ChangePlan クラスを自動生成する。
    /// 複数回付与可能（バージョンごとに1つ）。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class GenerateChangePlanAttribute : Attribute
    {
        /// <summary>
        /// ターゲットバージョンの型。
        /// </summary>
        public Type TargetVersion { get; }

        /// <summary>
        /// アップグレード元のバージョン型。null の場合はテーブル新規作成。
        /// </summary>
        public Type FromVersion { get; }

        /// <summary>
        /// テーブル新規作成用（VersionOrigin 等の初期バージョン向け）。
        /// </summary>
        public GenerateChangePlanAttribute(Type targetVersion)
        {
            TargetVersion = targetVersion;
        }

        /// <summary>
        /// バージョンアップグレード用。
        /// </summary>
        public GenerateChangePlanAttribute(Type targetVersion, Type fromVersion)
        {
            TargetVersion = targetVersion;
            FromVersion = fromVersion;
        }
    }
}
