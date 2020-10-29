

using System;
using System.Runtime.Serialization;

namespace Homura.ORM
{
    [Serializable]
    public class DatabaseSchemaException : Exception
    {
        public DatabaseSchemaException()
        {
        }

        public DatabaseSchemaException(string message) : base(message)
        {
        }

        public DatabaseSchemaException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DatabaseSchemaException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}