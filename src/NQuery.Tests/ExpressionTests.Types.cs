using System;
using System.Collections.Generic;

namespace NQuery.Tests
{
    public partial class ExpressionTests
    {
        public static IEnumerable<object[]> GetBuiltInNumericTypes()
        {
            return new[]
            {
                new object[] {typeof (int)},
                new object[] {typeof (uint)},
                new object[] {typeof (long)},
                new object[] {typeof (ulong)},
                new object[] {typeof (float)},
                new object[] {typeof (double)},
                new object[] {typeof (decimal)}
            };
        }

        public static IEnumerable<object[]> GetBuiltInSignedNumericTypes()
        {
            return new[]
            {
                new object[] {typeof (int)},
                new object[] {typeof (long)},
                new object[] {typeof (float)},
                new object[] {typeof (double)},
                new object[] {typeof (decimal)}
            };
        }

        public static IEnumerable<object[]> GetBuiltInIntegralTypes()
        {
            return new[]
            {
                new object[] {typeof (int)},
                new object[] {typeof (uint)},
                new object[] {typeof (long)},
                new object[] {typeof (ulong)}
            };
        }
    }
}