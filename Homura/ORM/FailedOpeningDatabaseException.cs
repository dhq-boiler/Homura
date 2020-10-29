

using System;

namespace Homura.ORM
{
    [Serializable]
    public class FailedOpeningDatabaseException : Exception
    {
        internal FailedOpeningDatabaseException()
            : base()
        { }

        internal FailedOpeningDatabaseException(string message)
            : base(message)
        { }

        internal FailedOpeningDatabaseException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
