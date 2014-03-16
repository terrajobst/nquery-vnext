using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.UnitTests.Highlighting.Highlighters
{
    [TestClass]
    public class CaseKeywordHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighligher()
        {
            return new CaseKeywordHighlighter();
        }

        protected override string[] GetExpectedHighlights()
        {
            var expectedHighlights = new[]
                                     {
                                         "CASE", "WHEN", "THEN", "WHEN", "THEN", "ELSE", "END"
                                     };
            return expectedHighlights;
        }

        // CASE

        [TestMethod]
        public void CaseKeywordHighlighter_MatchesAtBeginningOfCase()
        {
            var query = @"
                SELECT  |CASE
                            WHEN e.EmployeeID = 1 THEN 'One'
                            WHEN e.EmployeeID = 2 THEN 'Then'
                            ELSE 'Other'
                        END
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void CaseKeywordHighlighter_MatchesInMiddleOfCase()
        {
            var query = @"
                SELECT  CA|SE
                            WHEN e.EmployeeID = 1 THEN 'One'
                            WHEN e.EmployeeID = 2 THEN 'Then'
                            ELSE 'Other'
                        END
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void CaseKeywordHighlighter_MatchesAtEndOfCase()
        {
            var query = @"
                SELECT  CASE|
                            WHEN e.EmployeeID = 1 THEN 'One'
                            WHEN e.EmployeeID = 2 THEN 'Then'
                            ELSE 'Other'
                        END
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }

        // WHEN

        [TestMethod]
        public void CaseKeywordHighlighter_MatchesAtBeginningOfWhen()
        {
            var query = @"
                SELECT  CASE
                            |WHEN e.EmployeeID = 1 THEN 'One'
                            WHEN e.EmployeeID = 2 THEN 'Then'
                            ELSE 'Other'
                        END
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void CaseKeywordHighlighter_MatchesInMiddleOfWhen()
        {
            var query = @"
                SELECT  CASE
                            WH|EN e.EmployeeID = 1 THEN 'One'
                            WHEN e.EmployeeID = 2 THEN 'Then'
                            ELSE 'Other'
                        END
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void CaseKeywordHighlighter_MatchesAtEndOfWhen()
        {
            var query = @"
                SELECT  CASE
                            WHEN| e.EmployeeID = 1 THEN 'One'
                            WHEN e.EmployeeID = 2 THEN 'Then'
                            ELSE 'Other'
                        END
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }

        // THEN

        [TestMethod]
        public void CaseKeywordHighlighter_MatchesAtBeginningOfThen()
        {
            var query = @"
                SELECT  CASE
                            WHEN e.EmployeeID = 1 |THEN 'One'
                            WHEN e.EmployeeID = 2 THEN 'Then'
                            ELSE 'Other'
                        END
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void CaseKeywordHighlighter_MatchesInMiddleOfThen()
        {
            var query = @"
                SELECT  CASE
                            WHEN e.EmployeeID = 1 TH|EN 'One'
                            WHEN e.EmployeeID = 2 THEN 'Then'
                            ELSE 'Other'
                        END
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void CaseKeywordHighlighter_MatchesAtEndOfThen()
        {
            var query = @"
                SELECT  CASE
                            WHEN e.EmployeeID = 1 THEN| 'One'
                            WHEN e.EmployeeID = 2 THEN 'Then'
                            ELSE 'Other'
                        END
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }

        // ELSE

        [TestMethod]
        public void CaseKeywordHighlighter_MatchesAtBeginningOfElse()
        {
            var query = @"
                SELECT  CASE
                            WHEN e.EmployeeID = 1 THEN 'One'
                            WHEN e.EmployeeID = 2 THEN 'Then'
                            |ELSE 'Other'
                        END
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void CaseKeywordHighlighter_MatchesInMiddleOfElse()
        {
            var query = @"
                SELECT  CASE
                            WHEN e.EmployeeID = 1 THEN 'One'
                            WHEN e.EmployeeID = 2 THEN 'Then'
                            EL|SE 'Other'
                        END
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void CaseKeywordHighlighter_MatchesAtEndOfElse()
        {
            var query = @"
                SELECT  CASE
                            WHEN e.EmployeeID = 1 THEN 'One'
                            WHEN e.EmployeeID = 2 THEN 'Then'
                            ELSE| 'Other'
                        END
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }

        // END

        [TestMethod]
        public void CaseKeywordHighlighter_MatchesAtBeginningOfEnd()
        {
            var query = @"
                SELECT  CASE
                            WHEN e.EmployeeID = 1 THEN 'One'
                            WHEN e.EmployeeID = 2 THEN 'Then'
                            ELSE 'Other'
                        |END
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void CaseKeywordHighlighter_MatchesInMiddleOfEnd()
        {
            var query = @"
                SELECT  CASE
                            WHEN e.EmployeeID = 1 THEN 'One'
                            WHEN e.EmployeeID = 2 THEN 'Then'
                            ELSE 'Other'
                        EN|D
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void CaseKeywordHighlighter_MatchesAtEndOfEnd()
        {
            var query = @"
                SELECT  CASE
                            WHEN e.EmployeeID = 1 THEN 'One'
                            WHEN e.EmployeeID = 2 THEN 'Then'
                            ELSE 'Other'
                        END|
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }
    }
}