using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Refactorings;

using Xunit;

namespace NQuery.Authoring.Tests.CodeActions.Refactorings
{
    public class FlipBinaryOperatorSidesTests : CodeRefactoringTests
    {
        protected override ICodeRefactoringProvider CreateProvider()
        {
            return new FlipBinaryOperatorSidesCodeRefactoringProvider();
        }

        [Theory]
        [MemberData(nameof(GetBinaryOperatorTokensThatCanBeSwapped), true)]
        public void FlipBinaryOperatorSides_SwapsOperator_ToSelf(SyntaxKind kind)
        {
            var operatorText = kind.GetText();

            var query = $@"
                SELECT  *
                FROM    Employees e
                WHERE   /* prefix */ e.FirstName /* before */ {operatorText}| /* after */ 'Andrew' /* suffix */
            ";

            var fixedQuery = $@"
                SELECT  *
                FROM    Employees e
                WHERE   /* prefix */ 'Andrew' /* before */ {operatorText} /* after */ e.FirstName /* suffix */
            ";

            var description = $"Flip '{operatorText}' operands";

            AssertFixes(query, fixedQuery, description);
        }

        [Theory]
        [MemberData(nameof(GetBinaryOperatorTokensThatCanBeSwapped), false)]
        public void FlipBinaryOperatorSides_SwapsOperator_ToOther(SyntaxKind kind)
        {
            var operatorText = kind.GetText();
            var otherOperatorText = SyntaxFacts.SwapBinaryExpressionTokenKind(kind).GetText();

            var query = $@"
                SELECT  *
                FROM    Employees e
                WHERE   /* prefix */ e.FirstName /* before */ {operatorText}| /* after */ 'Andrew' /* suffix */
            ";

            var fixedQuery = $@"
                SELECT  *
                FROM    Employees e
                WHERE   /* prefix */ 'Andrew' /* before */ {otherOperatorText} /* after */ e.FirstName /* suffix */
            ";

            var description = $"Flip '{operatorText}' operator to '{otherOperatorText}'";

            AssertFixes(query, fixedQuery, description);
        }

        [Theory]
        [MemberData(nameof(GetBinaryOperatorTokensThatCannotBeSwapped))]
        public void FlipBinaryOperatorSides_DoesNoTrigger_ForInvalidOperators(SyntaxKind kind)
        {
            var operatorText = kind.GetText();

            var query = $@"
                SELECT  *
                FROM    Employees e
                WHERE   /* prefix */ e.FirstName /* before */ {operatorText}| /* after */ 'Andrew' /* suffix */
            ";

            AssertDoesNotTrigger(query);
        }

        public static IEnumerable<object[]> GetBinaryOperatorTokensThatCanBeSwapped(bool self)
        {
            return SyntaxFacts.GetBinaryExpressionTokenKinds()
                              .Where(k => k != SyntaxKind.BarToken) // Causes issues with AnnotatedText
                              .Where(SyntaxFacts.CanSwapBinaryExpressionTokenKind)
                              .Where(k => self && SyntaxFacts.SwapBinaryExpressionTokenKind(k) == k ||
                                          !self && SyntaxFacts.SwapBinaryExpressionTokenKind(k) != k)
                              .Select(k => new object[] {k});
        }

        public static IEnumerable<object[]> GetBinaryOperatorTokensThatCannotBeSwapped()
        {
            return SyntaxFacts.GetBinaryExpressionTokenKinds()
                              .Where(k => !SyntaxFacts.CanSwapBinaryExpressionTokenKind(k))
                              .Select(k => new object[] { k });
        }
    }
}