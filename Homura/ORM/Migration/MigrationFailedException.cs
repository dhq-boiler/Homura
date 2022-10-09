using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Homura.ORM.Migration
{
    /// <summary>
    /// データベースマイグレーション時に予期しない事象が発生して失敗した場合にスローされます。
    /// </summary>
    public class MigrationFailedException : Exception
    {
        public MigrationFailedException()
        {
        }

        public MigrationFailedException(string message) : base(message)
        {
        }

        public MigrationFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MigrationFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
