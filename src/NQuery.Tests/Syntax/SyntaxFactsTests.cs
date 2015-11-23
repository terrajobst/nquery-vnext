using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xunit;

namespace NQuery.Tests.Syntax
{
    public class SyntaxFactsTests
    {
        [Fact]
        public void SyntaxFacts_Keyword_GetKeywords_ReturnsAllKindsNamedKeyword()
        {
            var expectedKeywords = GetAllKindsNamedKeyword().OrderBy(t => t);
            var acutalKeywords = SyntaxFacts.GetKeywordKinds().OrderBy(t => t);

            Assert.Equal(expectedKeywords, acutalKeywords);
        }

        [Fact]
        public void SyntaxFacts_Keyword_GetKeywords_EqualsReservedPlusContextual()
        {
            var getKeywords = SyntaxFacts.GetKeywordKinds().OrderBy(t => t);
            var reserved = SyntaxFacts.GetReservedKeywordKinds();
            var contextual = SyntaxFacts.GetContextualKeywordKinds();
            var reservedPlusContextual = reserved.Concat(contextual).OrderBy(t => t);

            Assert.Equal(getKeywords, reservedPlusContextual);
        }

        [Fact]
        public void SyntaxFacts_Keyword_ReservedAndContextual_AreMutuallyExclusive()
        {
            var reservedKinds = new HashSet<SyntaxKind>(SyntaxFacts.GetReservedKeywordKinds());
            var contextualKinds = new HashSet<SyntaxKind>(SyntaxFacts.GetContextualKeywordKinds());

            foreach (var kind in SyntaxFacts.GetKeywordKinds())
            {
                Assert.True(reservedKinds.Contains(kind) ^ contextualKinds.Contains(kind));
            }
        }

        [Fact]
        public void SyntaxFacts_Keyword_IsKeyword_IsTrueForAllKeywords()
        {
            foreach (var kind in SyntaxFacts.GetKeywordKinds())
            {
                Assert.True(kind.IsKeyword());
            }
        }

        [Fact]
        public void SyntaxFacts_Keyword_IsIdentifierOrKeyword_IsTrueForAllKeywords()
        {
            foreach (var kind in SyntaxFacts.GetKeywordKinds())
            {
                Assert.True(kind.IsIdentifierOrKeyword());
            }
        }

        [Fact]
        public void SyntaxFacts_Identifier_IsIdentifierOrKeyword_IsTrueForIdentifier()
        {
            Assert.True(SyntaxKind.IdentifierToken.IsIdentifierOrKeyword());
        }

        [Fact]
        public void SyntaxFacts_Keyword_GetKeywordKind_ThrowsArgumentNullException_IfTextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => SyntaxFacts.GetKeywordKind(null));
        }

        [Fact]
        public void SyntaxFacts_Keyword_GetKeywordKind_DetectsAllReservedKeywords()
        {
            foreach (var kind in SyntaxFacts.GetReservedKeywordKinds())
            {
                var text = kind.GetText();
                var keywordKind = SyntaxFacts.GetKeywordKind(text);

                Assert.Equal(kind, keywordKind);
            }
        }

        [Fact]
        public void SyntaxFacts_Keyword_GetKeywordKind_DetectsAllReservedKeywords_RegardlessOfCase()
        {
            foreach (var kind in SyntaxFacts.GetReservedKeywordKinds())
            {
                var text = kind.GetText();
                var textWithMixedCast = GetMixedCase(text);
                var keywordKind = SyntaxFacts.GetKeywordKind(textWithMixedCast);

                Assert.Equal(kind, keywordKind);
            }
        }

        [Fact]
        public void SyntaxFacts_Keyword_GetKeywordKind_ReturnsIdentifier_IfNotKeyword()
        {
            var keywordKind = SyntaxFacts.GetKeywordKind("ipsum");

            Assert.Equal(SyntaxKind.IdentifierToken, keywordKind);
        }

        [Fact]
        public void SyntaxFacts_Keyword_GetContextualKeywordKind_ThrowsArgumentNullException_IfTextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => SyntaxFacts.GetContextualKeywordKind(null));
        }

        [Fact]
        public void SyntaxFacts_Keyword_GetContextualKeywordKind_DetectsAllContextualKeywords()
        {
            foreach (var kind in SyntaxFacts.GetContextualKeywordKinds())
            {
                var text = kind.GetText();
                var keywordKind = SyntaxFacts.GetContextualKeywordKind(text);

                Assert.Equal(kind, keywordKind);
            }
        }

        [Fact]
        public void SyntaxFacts_Keyword_GetContextualKeywordKind_DetectsAllContextualKeywords_RegardlessOfCase()
        {
            foreach (var kind in SyntaxFacts.GetContextualKeywordKinds())
            {
                var text = kind.GetText();
                var textWithMixedCast = GetMixedCase(text);
                var keywordKind = SyntaxFacts.GetContextualKeywordKind(textWithMixedCast);

                Assert.Equal(kind, keywordKind);
            }
        }

        [Fact]
        public void SyntaxFacts_Keyword_GetContextualKeywordKind_ReturnsIdentifier_IfNotKeyword()
        {
            var keywordKind = SyntaxFacts.GetContextualKeywordKind("ipsum");

            Assert.Equal(SyntaxKind.IdentifierToken, keywordKind);
        }

        [Fact]
        public void SyntaxFacts_Keyword_GetDisplayText_EqualsGetText()
        {
            foreach (var kind in SyntaxFacts.GetKeywordKinds())
            {
                var displayText = kind.GetDisplayText();
                var text = kind.GetText();

                Assert.Equal(displayText, text);
            }
        }

        [Fact]
        public void SyntaxFacts_Token_GetText_ReturnsValidText_IfTokenIsNotDynamic()
        {
            foreach (var tokenKind in GetAllKindsNamedToken().Where(t => !IsDynamicToken(t)))
            {
                var text = tokenKind.GetText();
                var token = SyntaxFacts.ParseToken(text);

                Assert.Equal(tokenKind, token.Kind);
            }
        }

        [Fact]
        public void SyntaxFacts_Token_GetText_ReturnsEmpty_IfTokenIsDynamic()
        {
            foreach (var tokenKind in GetAllKindsNamedToken().Where(IsDynamicToken))
            {
                var text = tokenKind.GetText();

                Assert.Equal(string.Empty, text);
            }
        }

        [Fact]
        public void SyntaxFacts_Token_GetDisplayText_IsNotEmpty_IfTokenIsDynamic()
        {
            foreach (var tokenKind in GetAllKindsNamedToken().Where(t => t != SyntaxKind.BadToken && IsDynamicToken(t)))
            {
                var text = tokenKind.GetDisplayText();

                Assert.NotEqual(string.Empty, text);
            }
        }

        [Fact]
        public void SyntaxFacts_Token_SwapBinaryExpressionTokenKind_Cycles()
        {
            var validOperators = GetAllKinds().Where(SyntaxFacts.CanSwapBinaryExpressionTokenKind);

            foreach (var tokenKind in validOperators)
            {
                var outputKind1 = SyntaxFacts.SwapBinaryExpressionTokenKind(tokenKind);
                var outputKind2 = SyntaxFacts.SwapBinaryExpressionTokenKind(outputKind1);

                Assert.Equal(tokenKind, outputKind2);
            }
        }

        [Fact]
        public void SyntaxFacts_Token_SwapBinaryExpressionTokenKind_Throws()
        {
            var invalidOperators = GetAllKinds().Where(k => !SyntaxFacts.CanSwapBinaryExpressionTokenKind(k));

            foreach (var tokenKind in invalidOperators)
            {
                Assert.Throws<InvalidOperationException>(() => SyntaxFacts.SwapBinaryExpressionTokenKind(tokenKind));
            }
        }

        [Fact]
        public void SyntaxFacts_Literal_IsLiteral_IsTrueForAllKindsNamedLiteralToken()
        {
            foreach (var kind in GetAllKindsNamedLiteralToken())
            {
                Assert.True(kind.IsLiteral());
            }
        }

        [Fact]
        public void SyntaxFacts_Trivia_IsComment_IsTrueForAllKindsNamedCommentTrivia()
        {
            foreach (var kind in GetAllKindsNamedCommentTrivia())
            {
                Assert.True(kind.IsComment());
            }
        }

        [Fact]
        public void SyntaxFacts_Identifier_GetValidIdentifier_ReturnsSame_ForValidIdentifier()
        {
            const string text = "lorem";
            var identifier = SyntaxFacts.GetValidIdentifier(text);

            Assert.Equal(text, identifier);
        }

        [Fact]
        public void SyntaxFacts_Identifier_GetValidIdentifier_ReturnsParenthesized_ForReservedKeywords()
        {
            foreach (var keywordKind in SyntaxFacts.GetReservedKeywordKinds())
            {
                var text = keywordKind.GetText();
                var expected = "[" + text + "]";
                var identifier = SyntaxFacts.GetValidIdentifier(text);

                Assert.Equal(expected, identifier);
            }
        }

        [Fact]
        public void SyntaxFacts_Identifier_GetValidIdentifier_ReturnsSame_ForContextualKeywords()
        {
            foreach (var keywordKind in SyntaxFacts.GetContextualKeywordKinds())
            {
                var text = keywordKind.GetText();
                var identifier = SyntaxFacts.GetValidIdentifier(text);

                Assert.Equal(text, identifier);
            }
        }

        [Fact]
        public void SyntaxFacts_Identifier_GetValidIdentifier_ReturnsParenthesized_ForQuotes()
        {
            const string text = "\"lorem\"";
            const string expected = "[" + text + "]";
            var identifier = SyntaxFacts.GetValidIdentifier(text);

            Assert.Equal(expected, identifier);
        }

        [Fact]
        public void SyntaxFacts_Identifier_GetValidIdentifier_ReturnsParenthesized_ForBrackets()
        {
            const string text = "[lorem]";
            const string expected = "[" + text + "]]";
            var identifier = SyntaxFacts.GetValidIdentifier(text);

            Assert.Equal(expected, identifier);
        }

        [Fact]
        public void SyntaxFacts_Identifier_GetValidIdentifier_ReturnsParenthesized_ForSpaces()
        {
            const string text = "lorem ipsum";
            const string expected = "[" + text + "]";
            var identifier = SyntaxFacts.GetValidIdentifier(text);

            Assert.Equal(expected, identifier);
        }

        [Fact]
        public void SyntaxFacts_Identifier_IsValidIdentifier_ReturnsTrue_ForValidIdentifier()
        {
            const string text = "lorem";
            var isValid = SyntaxFacts.IsValidIdentifier(text);

            Assert.True(isValid);
        }

        [Fact]
        public void SyntaxFacts_Identifier_IsValidIdentifier_ReturnsFalse_ForEmpty()
        {
            var isValid = SyntaxFacts.IsValidIdentifier(string.Empty);

            Assert.False(isValid);
        }

        [Fact]
        public void SyntaxFacts_Identifier_IsValidIdentifier_ReturnsFalse_ForQuotes()
        {
            const string text = "\"lorem\"";
            var isValid = SyntaxFacts.IsValidIdentifier(text);

            Assert.False(isValid);
        }

        [Fact]
        public void SyntaxFacts_Identifier_IsValidIdentifier_ReturnsFalse_ForBrackets()
        {
            const string text = "[lorem]";
            var isValid = SyntaxFacts.IsValidIdentifier(text);

            Assert.False(isValid);
        }

        [Fact]
        public void SyntaxFacts_Identifier_IsValidIdentifier_ReturnsFalse_ForSpaces()
        {
            const string text = "lorem ipsum";
            var isValid = SyntaxFacts.IsValidIdentifier(text);

            Assert.False(isValid);
        }

        [Fact]
        public void SyntaxFacts_Identifier_IsValidIdentifier_ReturnsFalse_ForReservedKeywords()
        {
            foreach (var keywordKind in SyntaxFacts.GetReservedKeywordKinds())
            {
                var text = keywordKind.GetText();
                var isValid = SyntaxFacts.IsValidIdentifier(text);

                Assert.False(isValid);
            }
        }

        [Fact]
        public void SyntaxFacts_Identifier_IsValidIdentifier_ReturnsTrue_ForContextualKeywords()
        {
            foreach (var keywordKind in SyntaxFacts.GetContextualKeywordKinds())
            {
                var text = keywordKind.GetText();
                var isValid = SyntaxFacts.IsValidIdentifier(text);

                Assert.True(isValid);
            }
        }

        [Fact]
        public void SyntaxFacts_Identifier_GetParenthesizedIdentifier_ReturnsIdentifier()
        {
            const string text = "lorem ipsum";
            var quoted = SyntaxFacts.GetParenthesizedIdentifier(text);
            var expected = "[" + text + "]";

            Assert.Equal(expected, quoted);
        }

        [Fact]
        public void SyntaxFacts_Identifier_GetParenthesizedIdentifier_ReturnsIdentifier_AndEscapesClosingBrackets()
        {
            const string text = "lorem [ipsum]";
            var quoted = SyntaxFacts.GetParenthesizedIdentifier(text);
            var expected = "[" + text.Replace("]", "]]") + "]";

            Assert.Equal(expected, quoted);
        }

        [Fact]
        public void SyntaxFacts_Identifier_GetQuotedIdentifier_ReturnsIdentifier()
        {
            const string text = "lorem ipsum";
            var quoted = SyntaxFacts.GetQuotedIdentifier(text);
            var expected = "\"" + text + "\"";

            Assert.Equal(expected, quoted);
        }

        [Fact]
        public void SyntaxFacts_Identifier_GetQuotedIdentifier_ReturnsIdentifier_AndEscapesQuotes()
        {
            const string text = "lorem \"ipsum\"";
            var quoted = SyntaxFacts.GetQuotedIdentifier(text);
            var expected = "\"" + text.Replace("\"", "\"\"") + "\"";

            Assert.Equal(expected, quoted);
        }

        private static IEnumerable<SyntaxKind> GetAllKinds()
        {
            return Enum.GetValues(typeof(SyntaxKind)).Cast<SyntaxKind>();
        }

        private static IEnumerable<SyntaxKind> GetAllKindsNamedKeyword()
        {
            return GetAllKinds().Where(k => k.ToString().EndsWith("Keyword"));
        }

        private static IEnumerable<SyntaxKind> GetAllKindsNamedToken()
        {
            return GetAllKinds().Where(k => k.ToString().EndsWith("Token"));
        }

        private static IEnumerable<SyntaxKind> GetAllKindsNamedLiteralToken()
        {
            return GetAllKinds().Where(k => k.ToString().EndsWith("LiteralToken"));
        }

        private static IEnumerable<SyntaxKind> GetAllKindsNamedCommentTrivia()
        {
            return GetAllKinds().Where(k => k.ToString().EndsWith("CommentTrivia"));
        }

        private static bool IsDynamicToken(SyntaxKind kind)
        {
            return kind == SyntaxKind.BadToken ||
                   kind == SyntaxKind.IdentifierToken ||
                   kind.ToString().EndsWith("LiteralToken");
        }

        private static string GetMixedCase(string text)
        {
            var sb = new StringBuilder(text.Length);
            foreach (var c in text)
            {
                var isEven = sb.Length % 2 == 0;
                var modifiedChar = isEven ? char.ToLower(c) : char.ToUpper(c);
                sb.Append(modifiedChar);
            }
            return sb.ToString();
        }
    }
}