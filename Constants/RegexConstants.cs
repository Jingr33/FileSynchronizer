using System.Text.RegularExpressions;

namespace FileSynchronizer.Constants;

public static class RegexConstants
{
    public static readonly Regex IntervalRegex = new(@"^(?:(?<d>\d+)d)?(?:(?<h>\d+)h)?(?:(?<m>\d+)m)?$", RegexOptions.Compiled);
}
