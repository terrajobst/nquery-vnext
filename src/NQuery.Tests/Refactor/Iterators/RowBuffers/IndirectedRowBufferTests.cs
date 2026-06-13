using NQuery.Refactor.Iterators;

namespace NQuery.Tests.Refactor.Iterators.RowBuffers
{
    public class IndirectedRowBufferTests : RowBufferTests
    {
        private static ArrayRowBuffer Buffer(params object[] values)
        {
            var buffer = new ArrayRowBuffer(values.Length);
            values.CopyTo(buffer.Array, 0);
            return buffer;
        }

        [Fact]
        public void RowBuffers_Indirected_ExposesActiveBuffer()
        {
            var buffer = new IndirectedRowBuffer(2, Buffer(1, 2));
            AssertContract(buffer, 1, 2);
        }

        [Fact]
        public void RowBuffers_Indirected_SwapsActiveBuffer()
        {
            var first = Buffer(1, 2);
            var second = Buffer(3, 4);
            var buffer = new IndirectedRowBuffer(2, first);

            AssertContract(buffer, 1, 2);

            buffer.ActiveRowBuffer = second;
            AssertContract(buffer, 3, 4);
        }

        [Fact]
        public void RowBuffers_Indirected_CountIsFixedAtConstruction()
        {
            var buffer = new IndirectedRowBuffer(2, Buffer(1, 2));
            Assert.Equal(2, buffer.Count);

            buffer.ActiveRowBuffer = Buffer(9, 9);
            Assert.Equal(2, buffer.Count);
        }
    }
}
