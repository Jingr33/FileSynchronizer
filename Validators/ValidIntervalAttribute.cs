using FileSynchronizer.Constants;
using System.ComponentModel.DataAnnotations;

namespace FileSynchronizer.Validators;

public class ValidIntervalAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var interval = value as string;

        if (string.IsNullOrWhiteSpace(interval))
        {
            return ValidationResult.Success;
        }

        var clean = interval.ToLowerInvariant().Replace(" ", "");

        if (int.TryParse(clean, out int rawMins))
        {
            clean = rawMins + "m";
        }

        var match = RegexConstants.IntervalRegex.Match(clean);

        if (!match.Success || string.IsNullOrEmpty(match.Value))
        {
            return new ValidationResult($"Invalid interval format: '{interval}'. (Valid example: '1d12h30m').");
        }

        int d = match.Groups["d"].Success ? int.Parse(match.Groups["d"].Value) : 0;
        int h = match.Groups["h"].Success ? int.Parse(match.Groups["h"].Value) : 0;
        int m = match.Groups["m"].Success ? int.Parse(match.Groups["m"].Value) : 0;

        int totalMinutes = d * 1440 + h * 60 + m;

        if (totalMinutes <= 0)
        {
            return new ValidationResult("Interval cannot be 0 minutes and less.");
        }

        int normDays = totalMinutes / 1440;

        if (normDays > 31)
        {
            return new ValidationResult("Interval cannot be longer than 31 days.");
        }

        return ValidationResult.Success;
    }
}