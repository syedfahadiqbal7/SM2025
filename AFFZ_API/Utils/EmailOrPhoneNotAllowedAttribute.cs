using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AFFZ_API.Utils
{
    public class EmailOrPhoneNotAllowedAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var content = value as string;

            if (string.IsNullOrWhiteSpace(content))
            {
                return ValidationResult.Success;
            }

            // Regular expression to match email addresses and phone numbers
            var emailPattern = @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}";
            var phonePattern = @"(\d{3}-\d{3}-\d{4})|(\(?\d{3}\)?\s*-?\s*\d{3}\s*-?\s*\d{4})";
            var wordsPattern = @"\b(one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve)\b";

            if (Regex.IsMatch(content, emailPattern, RegexOptions.IgnoreCase) ||
                Regex.IsMatch(content, phonePattern, RegexOptions.IgnoreCase) ||
                Regex.IsMatch(content, wordsPattern, RegexOptions.IgnoreCase))
            {
                return new ValidationResult("Content cannot contain email addresses or phone numbers.");
            }

            return ValidationResult.Success;
        }
    }
}
