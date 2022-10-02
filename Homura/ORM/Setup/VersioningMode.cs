using System;

namespace Homura.ORM.Setup
{
    [Flags]
    public enum VersioningMode
    {
        /// <summary>
        /// テーブル名の末尾にバージョンナンバーを付けて、バージョンコントロールします。
        /// </summary>
        ByTick = 0x1,

        /// <summary>
        /// テーブル名はそのままに、一時的なTempテーブルを作成してデータを退避し、
        /// テーブル定義を修正した後、再度Tempテーブルのデータを投入して、バージョンコントロールします。
        /// </summary>
        ByAlterTable = 0x2,

        /// <summary>
        /// ByTickで有効です。データの入れ物として使い終わったテーブルはDropします。
        /// </summary>
        DropTableCastedOff = 0x10,

        /// <summary>
        /// ByTickで有効です。データの入れ物として使い終わったテーブルのレコードはすべてDeleteします。
        /// </summary>
        DeleteAllRecordInTableCastedOff = 0x20,
    }
}
