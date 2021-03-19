using System;

namespace Crey.Contracts
{
    // note: moved to standard
    public class CreyException : Exception
    {
        public readonly ErrorCodes Error;

        public CreyException(ErrorCodes error)
        {
            this.Error = error;
        }

        public CreyException(ErrorCodes error, string message)
            : base(message)
        {
            this.Error = error;
        }

        public static CreyException Create(ErrorCodes error)
        {
            return new CreyException(error);
        }

        public static CreyException Create(ErrorCodes error, string message)
        {
            return new CreyException(error, message);
        }

        public override string ToString()
        {
            return $"ERROR ({Error.ToString()}), {base.ToString()}";
        }
    }
}