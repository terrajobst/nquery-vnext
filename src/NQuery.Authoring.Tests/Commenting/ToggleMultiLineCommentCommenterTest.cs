using System;

using NQuery.Authoring.Commenting;
using NQuery.Text;

using Xunit;

namespace NQuery.Authoring.Tests.Commenting
{
    public class ToggleMultiLineCommentCommenterTest : CommenterTests
    {
        protected override SyntaxTree ToggleComment(SyntaxTree syntaxTree, TextSpan span)
        {
            return syntaxTree.ToggleMultiLineComment(span);
        }

        [Fact]
        public void ToggleMultiLineComment_InsertsEmptyComment_WhenNoSelection()
        {
            var query = @"
                SELECT  e.FirstName,
                        e.City|,
                        e.LastName
                FROM    Employees e
            ";

            var expectedQuery = @"
                SELECT  e.FirstName,
                        e.City/**/,
                        e.LastName
                FROM    Employees e
            ";

            AssertIsMatch(query, expectedQuery);
        }

        [Fact]
        public void ToggleMultiLineComment_UncommentsEmptyComment_WhenNoSelection()
        {
            var query = @"
                SELECT  e.FirstName,
                        e.City/*|*/,
                        e.LastName
                FROM    Employees e
            ";

            var expectedQuery = @"
                SELECT  e.FirstName,
                        e.City,
                        e.LastName
                FROM    Employees e
            ";

            AssertIsMatch(query, expectedQuery);
        }

        [Fact]
        public void ToggleMultiLineComment_UncommentsEmptyComment_WhenSelected()
        {
            var query = @"
                SELECT  e.FirstName,
                        e.City/*{*/},
                        e.LastName
                FROM    Employees e
            ";

            var expectedQuery = @"
                SELECT  e.FirstName,
                        e.City,
                        e.LastName
                FROM    Employees e
            ";

            AssertIsMatch(query, expectedQuery);
        }

        [Fact]
        public void ToggleMultiLineComment_Comments_WhenSelectionOnSingleLine()
        {
            var query = @"
                SELECT  e.FirstName,
                        {e.City,}
                        e.LastName
                FROM    Employees e
            ";

            var expectedQuery = @"
                SELECT  e.FirstName,
                        /*e.City,*/
                        e.LastName
                FROM    Employees e
            ";

            AssertIsMatch(query, expectedQuery);
        }

        [Fact]
        public void ToggleMultiLineComment_Uncomments_WhenSelectionOnSingleLine()
        {
            var query = @"
                SELECT  e.FirstName,
                        /*e.{City,}*/
                        e.LastName
                FROM    Employees e
            ";

            var expectedQuery = @"
                SELECT  e.FirstName,
                        e.City,
                        e.LastName
                FROM    Employees e
            ";

            AssertIsMatch(query, expectedQuery);
        }

        [Fact]
        public void ToggleMultiLineComment_Comments_WhenSelectionOnMultipleLine()
        {
            var query = @"
                SELECT  {e.FirstName,
                        e.City,}
                        e.LastName
                FROM    Employees e
            ";

            var expectedQuery = @"
                SELECT  /*e.FirstName,
                        e.City,*/
                        e.LastName
                FROM    Employees e
            ";

            AssertIsMatch(query, expectedQuery);
        }

        [Fact]
        public void ToggleMultiLineComment_Uncomments_WhenSelectionOnMultipleLine()
        {
            var query = @"
                SELECT  {/*e.FirstName,
                        e.City},*/
                        e.LastName
                FROM    Employees e
            ";

            var expectedQuery = @"
                SELECT  e.FirstName,
                        e.City,
                        e.LastName
                FROM    Employees e
            ";

            AssertIsMatch(query, expectedQuery);
        }

        [Fact]
        public void ToggleMultiLineComment_Comments_AroundSingleLineComments()
        {
            var query = @"
                SELECT  {e.FirstName,
                        -- First
                        -- Second
                        e.City,}
                        e.LastName
                FROM    Employees e
            ";

            var expectedQuery = @"
                SELECT  /*e.FirstName,
                        -- First
                        -- Second
                        e.City,*/
                        e.LastName
                FROM    Employees e
            ";

            AssertIsMatch(query, expectedQuery);
        }

        [Fact]
        public void ToggleMultiLineComment_Uncomments_WhenNotTerminated()
        {
            var query = @"
                SELECT  /*e.FirstName,
                        -- First
                        -- Second
                        e.City,|
                        e.LastName
                FROM    Employees e
            ";

            var expectedQuery = @"
                SELECT  e.FirstName,
                        -- First
                        -- Second
                        e.City,
                        e.LastName
                FROM    Employees e
            ";

            AssertIsMatch(query, expectedQuery);
        }
    }
}