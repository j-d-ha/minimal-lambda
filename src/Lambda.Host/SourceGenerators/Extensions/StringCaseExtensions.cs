namespace Lambda.Host.SourceGenerators.Extensions;

internal static class StringCaseExtensions
{
    internal static string ToCamelCase(this string str)
    {
        if (string.IsNullOrEmpty(str) || str.Length == 0)
            return str;

        str = str.RemoveInterfaceI();

        return char.ToLowerInvariant(str[0]) + str[1..];
    }

    internal static string ToPrivateCamelCase(this string str) => "_" + str.ToCamelCase();

    private static string RemoveInterfaceI(this string str) =>
        string.IsNullOrEmpty(str) || str.Length < 2 || str[0] != 'I' || !char.IsUpper(str[1])
            ? str
            : str[1..];
}
