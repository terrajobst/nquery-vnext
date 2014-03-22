using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Refactorings;

namespace NQuery.Authoring.UnitTests.CodeActions.Refactorings
{
    [TestClass]
    public class AddMissingKeywordTests : RefactoringTests
    {
        protected override ICodeRefactoringProvider CreateProvider()
        {
            return new AddMissingKeywordCodeRefactoringProvider();
        }

        [TestMethod]
        public void AddMissingKeyword_DetectedForOrderWithoutBy()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                ORDER   /* before */ |e.FirstName
            ";

            var fixedQuery = @"
                SELECT  *
                FROM    Employees e
                ORDER   /* before */ BY e.FirstName
            ";

            var description = "Add missing 'BY' keyword";

            AssertFixes(query, fixedQuery, description);
        }
    }
}