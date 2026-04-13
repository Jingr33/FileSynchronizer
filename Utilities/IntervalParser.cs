using FileSynchronizer.Constants;

namespace FileSynchronizer.Utilities;

public static class IntervalParser
{
    public static TimeSpan ParseToTimeSpan(string interval)
    {
        var clean = interval.ToLowerInvariant().Replace(" ", "");

        if (int.TryParse(clean, out int rawMins))
        {
            clean = rawMins + "m";
        }

        var match = RegexConstants.IntervalRegex.Match(clean);

        if (!match.Success)
        {
            return TimeSpan.Zero;
        }

        int d = match.Groups["d"].Success ? int.Parse(match.Groups["d"].Value) : 0;
        int h = match.Groups["h"].Success ? int.Parse(match.Groups["h"].Value) : 0;
        int m = match.Groups["m"].Success ? int.Parse(match.Groups["m"].Value) : 0;

        return TimeSpan.FromDays(d) + TimeSpan.FromHours(h) + TimeSpan.FromMinutes(m);
    }

    public static string GetReadableInterval(string rawInterval)
    {
        if (int.TryParse(rawInterval.Trim(), out int minutes))
        {
            return minutes == 1 ? $"{minutes} minute" : $"{minutes} minutes";
        }

        return rawInterval;
    }
}