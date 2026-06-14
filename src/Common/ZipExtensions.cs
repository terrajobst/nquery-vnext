#if NETFRAMEWORK

using System.Collections.Generic;

#nullable enable

namespace System.Linq;

internal static class ZipExtensions
{
    extension<TFirst, TSecond>(IEnumerable<TFirst> first)
    {
        public IEnumerable<(TFirst First, TSecond Second)> Zip(IEnumerable<TSecond> second)
        {
            return first.Zip(second, static (f, s) => (f, s));
        }
    }
}

#endif
