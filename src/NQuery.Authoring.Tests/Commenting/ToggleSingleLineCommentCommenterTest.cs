using NQuery.Authoring.Commenting;
using NQuery.Text;

using Xunit;

namespace NQuery.Authoring.Tests.Commenting
{
    public class ToggleSingleLineCommentCommenterTest : CommenterTests
    {
        protected override SyntaxTree ToggleComment(SyntaxTree syntaxTree, TextSpan span)
        {
            return syntaxTree.ToggleSingleLineComment(span);
        }

        [Fact]
        public void ToggleSingleLineComment_CommentsSingleLine_WhenNoSelection()
        {
            var query = @"
                SELECT  e.FirstName,
                        e.City|,
                        e.LastName
                FROM    Employees e
            ";

            var expectedQuery = @"
                SELECT  e.FirstName,
                --        e.City,
                        e.LastName
                FROM    Employees e
            ";

            AssertIsMatch(query, expectedQuery);
        }

        [Fact]
        public void ToggleSingleLineComment_UncommentsSingleLine_WhenNoSelection()
        {
            var query = @"
                SELECT  e.FirstName,
                --      Unrelated
                --        e.City|,
                        e.LastName
                FROM    Employees e
            ";

            var expectedQuery = @"
                SELECT  e.FirstName,
                --      Unrelated
                        e.City,
                        e.LastName
                FROM    Employees e
            ";

            AssertIsMatch(query, expectedQuery);
        }

        [Fact]
        public void ToggleSingleLineComment_CommentsMultipleLines_WhenSelected()
        {
            var query = @"
                SELECT  e.FirstName,
                        e.{City,
                        e.Country,}
                        e.LastName
                FROM    Employees e
            ";

            var expectedQuery = @"
                SELECT  e.FirstName,
                --        e.City,
                --        e.Country,
                        e.LastName
                FROM    Employees e
            ";

            AssertIsMatch(query, expectedQuery);
        }

        [Fact]
        public void ToggleSingleLineComment_CommentsMultipleLines_WhenLeadingAndTrailingIsMixed()
        {
            var query = @"
                SELECT  e.FirstName,
                        e.City, -- {City
                        -- Country}
                        e.Country,
                        e.LastName
                FROM    Employees e
            ";

            var expectedQuery = @"
                SELECT  e.FirstName,
                --        e.City, -- City
                --        -- Country
                        e.Country,
                        e.LastName
                FROM    Employees e
            ";

            AssertIsMatch(query, expectedQuery);
        }

        [Fact]
        public void ToggleSingleLineComment_UncommentsMultipleLines_WhenSelected()
        {
            var query = @"
                SELECT  e.FirstName,
                --      Unrelated
                --        e.{City,
                --        e.LastName}
                FROM    Employees e
            ";

            var expectedQuery = @"
                SELECT  e.FirstName,
                --      Unrelated
                        e.City,
                        e.LastName
                FROM    Employees e
            ";

            AssertIsMatch(query, expectedQuery);
        }

        [Fact]
        public void ToggleSingleLineComment_CommentsOverMultiLineComment()
        {
            var query = @"
                SELECT  e.FirstName,
                        /*e.{LastName,
                        e.C}ity,
                        e.Country*/
                FROM    Employees e
            ";

            var expectedQuery = @"
                SELECT  e.FirstName,
                --        /*e.LastName,
                --        e.City,
                        e.Country*/
                FROM    Employees e
            ";

            AssertIsMatch(query, expectedQuery);
        }

        [Fact]
        public void ToggleSingleLineComment_UncommentsFromMultiLineComment()
        {
            var query = @"
                SELECT  e.FirstName,
                --        /*e.LastN{ame,
                --}        e.City,
                        e.Country*/
                FROM    Employees e
            ";

            var expectedQuery = @"
                SELECT  e.FirstName,
                        /*e.LastName,
                        e.City,
                        e.Country*/
                FROM    Employees e
            ";

            AssertIsMatch(query, expectedQuery);
        }
    }
}