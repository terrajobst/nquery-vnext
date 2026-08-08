using System.Globalization;

namespace NQuery.Authoring.LanguageServer.Mapping;

// Result cells cross the wire as display strings rather than JSON values. Sending typed JSON
// would round decimals through doubles and turn a byte[] column -- Northwind's Categories.Picture
// carries real image data -- into megabytes of base64 the grid could not show anyway.
internal static class ValueFormatter
{
    // Long enough for any realistic text column, short enough that one pathological row cannot
    // wedge the client.
    private const int MaxLength = 4096;

    private const string NullDisplay = null!;

    public static string? Format(object? value)
    {
        // Distinguished from the string "NULL" by being JSON null on the wire.
        if (value is null || value is DBNull)
            return NullDisplay;

        switch (value)
        {
            case string text:
                return Truncate(text);

            case byte[] bytes:
                return $"byte[{bytes.Length}]";

            case DateTime dateTime:
                return dateTime.TimeOfDay == TimeSpan.Zero
                    ? dateTime.ToString(@"yyyy-MM-dd", CultureInfo.InvariantCulture)
                    : dateTime.ToString(@"yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            case DateTimeOffset dateTimeOffset:
                return dateTimeOffset.ToString(@"yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture);

            case bool flag:
                return flag ? @"true" : @"false";

            case IFormattable formattable:
                return Truncate(formattable.ToString(null, CultureInfo.InvariantCulture));

            default:
                return Truncate(value.ToString() ?? string.Empty);
        }
    }

    public static string FormatTypeName(Type type)
    {
        ThrowIfNull(type);

        var underlying = Nullable.GetUnderlyingType(type) ?? type;

        return underlying switch
        {
            _ when underlying == typeof(bool) => @"bool",
            _ when underlying == typeof(byte) => @"byte",
            _ when underlying == typeof(short) => @"short",
            _ when underlying == typeof(int) => @"int",
            _ when underlying == typeof(long) => @"long",
            _ when underlying == typeof(float) => @"float",
            _ when underlying == typeof(double) => @"double",
            _ when underlying == typeof(decimal) => @"decimal",
            _ when underlying == typeof(string) => @"string",
            _ when underlying == typeof(DateTime) => @"datetime",
            _ when underlying == typeof(byte[]) => @"byte[]",
            _ => underlying.Name
        };
    }

    private static string Truncate(string text)
    {
        return text.Length <= MaxLength
            ? text
            : string.Concat(text.AsSpan(0, MaxLength), @"…");
    }
}
