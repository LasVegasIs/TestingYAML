#nullable enable
using Microsoft.EntityFrameworkCore;

namespace Crey.Data.Sql
{
    public static class SqlExceptionExtensions
    {
        public static bool IsConflictException(this DbUpdateException dbUpdateException)
        {
            if (dbUpdateException is DbUpdateConcurrencyException)
                return true;
            switch (dbUpdateException.InnerException)
            {
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