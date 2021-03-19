namespace Crey.Exceptions
{
    public class PasswordException : InvalidArgumentException
    {
        public PasswordException(string message)
            : base(message)
        {
        }
    }
}
