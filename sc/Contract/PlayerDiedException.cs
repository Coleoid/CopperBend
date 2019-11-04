using System;
using System.Runtime.Serialization;

namespace CopperBend.Contract
{
    [Serializable]
    public class PlayerDiedException : Exception
    {
        public PlayerDiedException()
        {
        }

        public PlayerDiedException(string message) : base(message)
        {
        }

        public PlayerDiedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PlayerDiedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
