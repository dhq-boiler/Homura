

using System;
using System.Runtime.Serialization;

namespace Homura.ORM
{
    [Serializable]
    public class NotMatchColumnException : Exception
    {
        public NotMatchColumnException()
        {
        }

        public NotMatchColumnException(string message) : base(message)
        {
        }

        public NotMatchColumnException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NotMatchColumnException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}