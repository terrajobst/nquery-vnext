using NQuery.Iterators;

namespace NQuery.Tests.Refactor.Iterators.RowBuffers
{
    public class CombinedRowBufferTests : RowBufferTests
    {
        private static ArrayRowBuffer Buffer(params object[] values)
        {
            var buffer = new ArrayRowBuffer(values.Length);
            values.CopyTo(buffer.Array, 0);
            return buffer;
        }

        [Fact]
        public void RowBuffers_Combined_ConcatenatesAcrossTheBoundary()
        {
            var combined = new CombinedRowBuffer(Buffer(1, 2), Buffer("a"));
            AssertContract(combined, 1, 2, "a");
        }

        [Fact]
        public void RowBuffers_Combined_HandlesEmptyLeft()
        {
            var combined = new CombinedRowBuffer(Buffer(), Buffer(1, 2));
            AssertContract(combined, 1, 2);
        }

        [Fact]
        public void RowBuffers_Combined_HandlesEmptyRight()
        {
            var combined = new CombinedRowBuffer(Buffer(1, 2), Buffer());
            AssertContract(combined, 1, 2);
        }

        [Fact]
        public void RowBuffers_Combined_ReflectsUnderlyingWrites()
        {
            var left = Buffer(1);
            var right = Buffer(2);
            var combined = new CombinedRowBuffer(left, right);

            left.Array[0] = 10;
            right.Array[0] = 20;

            AssertContract(combined, 10, 20);
        }
    }
}
