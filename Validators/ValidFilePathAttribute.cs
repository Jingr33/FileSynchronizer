using System.ComponentModel.DataAnnotations;

namespace FileSynchronizer.Validators;

public class ValidFilePathAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var path = value as string;

        if (string.IsNullOrWhiteSpace(path))
        {
            return new ValidationResult("File path argument cannot be empty");
        }

        if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            return new ValidationResult($"File path '{path}' contains forbidden signs.");
        }

        if (!path.EndsWith(".log", StringComparison.OrdinalIgnoreCase))
        {
            return new ValidationResult($"File path '{path}' must end with '.log'.");
        }

        try
        {
            Path.GetFullPath(path);

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Path.GetFullPath(directory);
            }
        }
        catch (Exception ex)
        {
            return new ValidationResult($"Provided log file path '{path}' is invalid: {ex.Message}");
        }

        return ValidationResult.Success;
    }
}
