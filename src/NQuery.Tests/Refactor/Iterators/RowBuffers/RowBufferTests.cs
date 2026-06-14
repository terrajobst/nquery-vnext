using NQuery.Iterators;

namespace NQuery.Tests.Refactor.Iterators.RowBuffers
{
    // Shared contract for every RowBuffer: Count, the indexer, and CopyTo must agree.
    // A concrete test configures a buffer (and any mutator state) then calls AssertContract
    // with the values it should currently expose; buffer-specific behavior (swapping,
    // probing, ...) is tested on top.
    public abstract class RowBufferTests
    {
        private protected static void AssertContract(RowBuffer buffer, params object[] expected)
        {
            // Count
            Assert.Equal(expected.Length, buffer.Count);

            // Indexer
            for (var i = 0; i < expected.Length; i++)
                Assert.Equal(expected[i], buffer[i]);

            // CopyTo into an exactly-sized array.
            var exact = new object[expected.Length];
            buffer.CopyTo(exact, 0);
            Assert.Equal(expected, exact);

            // CopyTo at an offset must fill exactly [offset, offset + Count) and leave the
            // surrounding slots untouched.
            const int prefix = 2;
            const int suffix = 2;
            var sentinel = new object();
            var padded = new object[prefix + expected.Length + suffix];
            Array.Fill(padded, sentinel);

            buffer.CopyTo(padded, prefix);

            for (var i = 0; i < prefix; i++)
                Assert.Same(sentinel, padded[i]);

            for (var i = 0; i < expected.Length; i++)
                Assert.Equal(expected[i], padded[prefix + i]);

            for (var i = 0; i < suffix; i++)
                Assert.Same(sentinel, padded[prefix + expected.Length + i]);
        }
    }
}
