using NQuery.Binding;

namespace NQuery.Tests.Binding
{
    public class SoundexTests
    {
        [Theory]
        [InlineData("0000", "")]
        [InlineData("R163", "Robert")]
        [InlineData("R163", "Rupert")]
        [InlineData("R150", "Rubin")]
        [InlineData("A261", "Ashcraft")]
        [InlineData("A261", "Ashcroft")]
        [InlineData("T522", "Tymczak")]
        [InlineData("P236", "Pfister")]
        [InlineData("P400", "Paella")]
        [InlineData("E200", "Ex Machina")]
        [InlineData("E200", "Ex;Machina")]
        [InlineData("E200", "Ex_Machina")]
        public void Soundex_ProducesCorrectResults(string expectedCode, string text)
        {
            Assert.Equal(expectedCode, Soundex.GetCode(text));
        }
    }
}
