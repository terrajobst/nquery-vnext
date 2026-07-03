#if NETFRAMEWORK

namespace System;

internal static class CharExtensions
{
    extension(char)
    {
        public static bool IsAsciiDigit(char c)
        {
            return c is >= '0' and <= '9';
        }
    }
}

#endif
