using System;
using System.Linq;

namespace IAM.Areas.Authentication
{
    public class DateOfBirth
    {
        public const int MinYearOfBirth = 1900;

        public static DateOfBirthValidationResult IsValid(int year, int month, int day)
        {
            if (year == 0 || month == 0 || day == 0)
            {
                return new DateOfBirthValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Required field."
                };
            }

            try
            {
                var dateTime = new DateTime(year, month, day).ToUniversalTime();
                int maxYearOfBirth = DateTime.UtcNow.Year;
                if (dateTime.Year >= maxYearOfBirth || dateTime.Year < MinYearOfBirth)
                {
                    return new DateOfBirthValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = $"Year must be between {MinYearOfBirth} and {maxYearOfBirth}"
                    };
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
                string errorMessage;
                if (e.ParamName == null)
                {
                    errorMessage = "Year, Month, and Day parameters describe an un-representable date.";
                }
                else
                {
                    var paramName = e.ParamName.First().ToString().ToUpper() + e.ParamName.Substring(1);
                    errorMessage = $"{paramName} is out of range.";
                }

                return new DateOfBirthValidationResult
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }

            return new DateOfBirthValidationResult
            {
                IsValid = true,
                ErrorMessage = ""
            };
        }
    }

    public struct DateOfBirthValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class DateOfBirthInvalidException : Exception
    {
        public DateOfBirthInvalidException(string message)
            : base(message)
        {
        }
    }
}
