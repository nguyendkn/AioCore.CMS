using System.Text.RegularExpressions;

namespace AioCore.Shared.Extensions;

public static class StringExtensions
{
    public static string RemoveDiacritics(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return string.Empty;

        try
        {
            str = str.ToLower().Trim();
            str = Regex.Replace(str, @"[\r|\n]", " ");
            str = Regex.Replace(str, @"\s+", " ");
            str = Regex.Replace(str, "[áàảãạâấầẩẫậăắằẳẵặ]", "a");
            str = Regex.Replace(str, "[éèẻẽẹêếềểễệ]", "e");
            str = Regex.Replace(str, "[iíìỉĩị]", "i");
            str = Regex.Replace(str, "[óòỏõọơớờởỡợôốồổỗộ]", "o");
            str = Regex.Replace(str, "[úùủũụưứừửữự]", "u");
            str = Regex.Replace(str, "[yýỳỷỹỵ]", "y");
            str = Regex.Replace(str, "[đ]", "d");

            str = Regex.Replace(str, "[\"`~!@#$%^&*()\\-+=?/>.<,{}[]|]\\]", "");
            str = str.Trim();
            return str;
        }
        catch (Exception)
        {
            return str;
        }
    }

    public static string JoinString(this IEnumerable<string>? arr, string character)
    {
        if (arr is null) return string.Empty;
        var enumerable = arr as string[] ?? arr.ToArray();
        return string.Join(character, enumerable);
    }

    public static string Slice(this string input, int start, string to)
    {
        var index = input.IndexOf(to, StringComparison.Ordinal);
        return input.Substring(start, index - 1);
    }
}