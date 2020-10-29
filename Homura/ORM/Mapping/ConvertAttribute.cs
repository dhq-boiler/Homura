

using System;

namespace Homura.ORM.Mapping
{
    /// <summary>
    /// カラムを変換する
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ConvertAttribute : Attribute
    {
        /// <summary>
        /// 同テーブルのカラムへ変換する
        /// </summary>
        /// <param name="convertionVersionTiming">変換を実施するバージョン</param>
        /// <param name="destPropertyName">変換先カラムプロパティ名</param>
        public ConvertAttribute(Type convertionVersionTiming, string destPropertyName)
        {
            DestinationProeprtyName = destPropertyName;
        }

        /// <summary>
        /// 別のテーブルのカラムへ変換する
        /// </summary>
        /// <param name="convertionVersionTiming">変換を実施するバージョン</param>
        /// <param name="destClassName">変換先テーブルクラス名</param>
        /// <param name="destPropertyName">変換先カラムプロパティ名</param>
        public ConvertAttribute(Type convertionVersionTiming, string destClassName, string destPropertyName)
            : this(convertionVersionTiming, destPropertyName)
        {
            DestinationClassName = destClassName;
        }

        public Type ConvertionVersionTiming { get; set; }

        public string DestinationClassName { get; set; }

        public string DestinationProeprtyName { get; set; }
    }
}
