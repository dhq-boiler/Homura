

using System;
using System.Runtime.Serialization;

namespace Homura.ORM.Mapping
{
    [Serializable]
    internal class DiscontinuousVersionClassException : Exception
    {
        public DiscontinuousVersionClassException()
        {
        }

        public DiscontinuousVersionClassException(string message) : base(message)
        {
        }

        public DiscontinuousVersionClassException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DiscontinuousVersionClassException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}