using System;
using System.Runtime.Serialization;

namespace Homura.Test.UnitTest.Enum
{
    [Serializable]
    internal class TypeMismatchException : Exception
    {
        public TypeMismatchException()
        {
        }

        public TypeMismatchException(string message) : base(message)
        {
        }

        public TypeMismatchException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TypeMismatchException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}