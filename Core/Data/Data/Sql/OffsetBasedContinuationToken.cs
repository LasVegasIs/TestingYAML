#nullable enable
namespace Crey.Data.Sql
{
    public class OffsetBasedContinuationToken
    {
        public OffsetBasedContinuationToken(string? token)
        {
            if (token != null)
            {
                int offset;
                int.TryParse(token, out offset);
                Offset = offset;
            }
            else
            {
                Offset = 0;
            }
        }

        public OffsetBasedContinuationToken()
        {
            // Default constructor
        }

        public int Offset { get; set; }

        public bool IsContinuation()
        {
            return Offset > 0;
        }

        public override string ToString()
        {
            return Offset.ToString();
        }

        public string IntoToken()
        {
            return ToString();
        }
    }
}