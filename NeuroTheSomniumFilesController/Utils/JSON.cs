namespace NeuroTheSomniumFiles;

public static class JSON
{
    public static string ExtractJsonValue(string json, string key)
    {
        string pattern = $"\"{key}\":\"(.*?)\"";
        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(json, pattern);
        if (match.Success) return match.Groups[1].Value;
        return "";
    }
}
