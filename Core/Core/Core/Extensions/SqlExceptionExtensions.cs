using Microsoft.EntityFrameworkCore;

namespace Core.Extensions
{
    // note: moved to net50 
    public static class SqlExceptionExtensions
    {
        public static bool IsConflictException(this DbUpdateException dbUpdateException)
        {
            if (dbUpdateException is DbUpdateConcurrencyException)
                return true;
            switch (dbUpdateException.InnerException)
            {
                case System.Data.SqlClient.SqlException ex: return IsConflictCode(ex.Number);
                case Microsoft.Data.SqlClient.SqlException ex: return IsConflictCode(ex.Number);
                default: return false;
            }
        }

        private static bool IsConflictCode(int code)
        {
            switch (code)
            {
                case 2627:  // Unique constraint error
                case 547:   // Constraint check violation
                case 2601:  // Duplicated key row error                            
                    return true;

                default: return false;
            }
        }
    }
}