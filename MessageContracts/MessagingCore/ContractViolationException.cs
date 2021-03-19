using System;
using System.Runtime.Serialization;

namespace MessagingCore
{
    [Serializable]
    public class ContractViolationException : Exception
    {
        public ContractViolationException()
        {
        }

        public ContractViolationException(string message) : base(message)
        {
        }

        public ContractViolationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ContractViolationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}