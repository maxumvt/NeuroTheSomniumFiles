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

        if (obj is System.Collections.IDictionary dict)
        {
            var parts = new List<string>();
            foreach (System.Collections.DictionaryEntry kv in dict)
            {
                parts.Add($"\"{Escape(kv.Key.ToString())}\":{ToJson(kv.Value)}");
            }
            return "{" + string.Join(",", parts.ToArray()) + "}";
        }

        if (obj is System.Collections.IEnumerable list && !(obj is string))
        {
            var parts = new List<string>();
            foreach (var item in list)
                parts.Add(ToJson(item));
            return "[" + string.Join(",", parts.ToArray()) + "]";
        }

        var type = obj.GetType();

        var objDict = new Dictionary<string, object>();

        foreach (var field in type.GetFields())
        {
            objDict[field.Name] = field.GetValue(obj);
        }

        return JSON.ToJson(objDict);
    }

    static string Escape(string s)
    {
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
