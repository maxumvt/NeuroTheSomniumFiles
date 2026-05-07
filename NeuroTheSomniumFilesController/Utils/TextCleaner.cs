namespace NeuroTheSomniumFiles;

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public static class TextCleaner
{
    static Dictionary<string, string> nameMap = new Dictionary<string, string>()
    {
        {"ui_main_name_c00", "Unknown"},
        {"ui_main_name_c01", "Kaname Date"},
        {"ui_main_name_c02", "Aiba"},
        {"ui_main_name_c04", "Hitomi Sagan"},
        {"ui_main_name_c05", "Iris Sagan"},
        {"ui_main_name_c06", "Ota Matsushita"},
        {"ui_main_name_c09", "Boss"},
        {"ui_main_name_c11", "Mayumi Matsushita"},
        {"ui_main_name_c51", "Inspector"},
        {"ui_main_name_c52", "Policeman"},
        {"ui_main_name_c102", "A-set"},
    };
    public static float TimeConvert(string input)
    {
        string final_time = "";
        string pattern = "<.*?>";
        string[] time_elements = Regex.Split(input, pattern);
        final_time += time_elements.Length > 1 ? time_elements[0] + "." + time_elements[1] : time_elements[0];
        return float.Parse(final_time);
    }
    public static string Clean(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        string text = input;

        text = RemoveRichText(text);
        text = NormalizeWhitespace(text);
        text = ReplaceQuotes(text);
        text = ResolveCharacterNames(text);

        return text;
    }

    public static string ReplaceQuotes(string text)
    {
        return text.Replace("\"","\'");
    }
    public static string RemoveRichText(string text)
    {
        return Regex.Replace(text, "<.*?>", "");
    }
    public static string ResolveCharacterNames(string text)
    {
        foreach (var kv in nameMap)
        {
            text = text.Replace(kv.Key, kv.Value);
        }
        return text;
    }
    public static string NormalizeWhitespace(string text)
    {
        return text.Replace("\r\n", " ")
               .Replace("\n", " ")
               .Replace("\r", " ");
    }
}
