namespace PKOC.Net.Tests;

public static class Utilities
{
    /// <summary>
    /// Removes white space characters from a hexadecimal string.
    /// </summary>
    /// <param name="hexadecimalString">The input hexadecimal string.</param>
    /// <returns>The hexadecimal string with all white space characters removed.</returns>
    public static string RemoveWhiteSpaceFromHexadecimalString(this string hexadecimalString)
    {
        return hexadecimalString.Replace(" ", string.Empty);
    }
}