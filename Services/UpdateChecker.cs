using System;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ValorVDC_FamilyBrowser.Services;

public static class UpdateChecker
{
    private const string ReleasesApiUrl =
        "https://api.github.com/repos/cabowker/ValorVDC_FamilyBrowser/releases/latest";

    public const string ReleasesPageUrl =
        "https://github.com/cabowker/ValorVDC_FamilyBrowser/releases/latest";

    // Returns the latest tag (e.g. "v1.0.2") if newer than the running assembly,
    // or null if up-to-date, unreachable, or on parse error.
    public static async Task<string?> GetLatestTagIfNewerAsync()
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            client.DefaultRequestHeaders.Add("User-Agent", "ValorVDC-FamilyBrowser");

            var json = await client.GetStringAsync(ReleasesApiUrl).ConfigureAwait(false);
            var tag = JObject.Parse(json)["tag_name"]?.ToString();

            if (string.IsNullOrWhiteSpace(tag)) return null;

            var latestVersion  = ParseVersion(tag);
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

            if (latestVersion != null && currentVersion != null &&
                latestVersion > currentVersion)
                return tag;

            return null;
        }
        catch
        {
            // No internet, GitHub down, or rate-limited — fail silently.
            return null;
        }
    }

    private static Version? ParseVersion(string tag)
    {
        // Accepts "v1.0.2" or "1.0.2"
        var match = Regex.Match(tag, @"(\d+\.\d+[\.\d]*)");
        return match.Success && Version.TryParse(match.Groups[1].Value, out var v) ? v : null;
    }
}
