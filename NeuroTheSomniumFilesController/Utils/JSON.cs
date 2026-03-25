namespace NeuroTheSomniumFiles;

using System.Collections.Generic;

public static class JSON
{
    public static string ExtractJsonValue(string json, string key)
    {
        string pattern = $"\"{key}\":\"(.*?)\"";
        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(json, pattern);
        if (match.Success) return match.Groups[1].Value;
        return "";
    }

    public static string ToJson(object obj)
    {
        if (obj == null) return "null";

        if (obj is string s)
            return "\"" + Escape(s) + "\"";

        if (obj is bool b)
            return b ? "true" : "false";

        if (obj is IDictionary<string, object> dict)
        {
            var parts = new List<string>();
            foreach (var kv in dict)
            {
                parts.Add($"\"{Escape(kv.Key)}\":{ToJson(kv.Value)}");
            }
            return "{" + string.Join(",", parts.ToArray()) + "}";
        }

        if (obj is IList<object> list)
        {
            var parts = new List<string>();
            foreach (var item in list)
                parts.Add(ToJson(item));
            return "[" + string.Join(",", parts.ToArray()) + "]";
        }

        return obj.ToString();
    }

    static string Escape(string s)
    {
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
