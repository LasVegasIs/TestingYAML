using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Crey.Contracts.Bank
{
    public enum Currency
    {
        RealMoney_EUR,
        Gold,
        KarmaPoint,
        KarmaCoin,
    }

    public class GiftParams : IValidatableObject
    {
        [Required]
        public int? GiftedUserId { get; set; }

        [Required]
        public decimal? Count { get; set; }

        [Required]

        public Currency? Currency { get; set; }

        [Required]
        public string Description { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            const decimal LIMIT = 20_000;

            if (!Count.HasValue || Count.Value < 0 || Count.Value > LIMIT)
            {
                yield return new ValidationResult($"Count must be set to a positive number less than {LIMIT}");
            }
        }
    }

    public class Transaction
    {
        public int Id { get; set; }
        public DateTimeOffset Date { get; set; }
        public int Issuer { get; set; }
        public string Description { get; set; }
    }
}
