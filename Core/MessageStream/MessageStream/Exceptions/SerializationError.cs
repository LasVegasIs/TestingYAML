using System;
using System.Runtime.Serialization;

namespace Core.MessageStream
{
    [Serializable]
    internal class SerializationError : Exception
    {
        public SerializationError()
        {
        }

        public SerializationError(string message) : base(message)
        {
        }

        public SerializationError(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SerializationError(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}