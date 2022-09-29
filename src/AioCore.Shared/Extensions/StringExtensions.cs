using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace AioCore.Shared.Extensions;

public static class StringExtensions
{
    public static Guid ToGuid(this string str)
    {
        return Guid.TryParse(str, out var guid) ? guid : Guid.Empty;
    }

    public static bool ParseGuid(this string? str)
    {
        return Guid.TryParse(str, out _);
    }

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

    public static string CreateMd5(this string? input, string? prefix = "")
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        using var md5 = MD5.Create();
        var inputBytes = Encoding.ASCII.GetBytes(input);
        var hashBytes = md5.ComputeHash(inputBytes);

        var sb = new StringBuilder();
        foreach (var hashByte in hashBytes)
            sb.Append(hashByte.ToString("X2"));
        var prefixMd5 = !string.IsNullOrEmpty(prefix) ? $"{prefix}_" : string.Empty;
        return $"{prefixMd5}{sb}";
    }
}