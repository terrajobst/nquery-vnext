using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Completion;
using NQuery.Authoring.Completion.Providers;

namespace NQuery.Authoring.UnitTests.Completion.Providers
{
    [TestClass]
    public class KeywordCompletionProviderTests
    {
        private static CompletionModel CreateCompletionModel(SemanticModel semanticModel, int position)
        {
            var provider = new KeywordCompletionProvider();
            var providers = new[] {provider};
            return semanticModel.GetCompletionModel(position, providers);
        }

        private static CompletionModel GetCompletionModel(string query)
        {
            int position;
            var actualQuery = query.ParseSinglePosition(out position);

            var compilation = CompilationFactory.CreateQuery(actualQuery);
            var semanticModel = compilation.GetSemanticModel();

            return CreateCompletionModel(semanticModel, position);
        }

        private static CompletionModel GetCompletionModelWithFirstChar(string query, string keyword)
        {
            int position;
            var actualQuery = query.ParseSinglePosition(out position);

            var modifiedQuery = actualQuery.Insert(position, keyword.Substring(0, 1));
            position++;

            var compilation = CompilationFactory.CreateQuery(modifiedQuery);
            var semanticModel = compilation.GetSemanticModel();

            return CreateCompletionModel(semanticModel, position);
        }

        private static void AssertIsMatch(string query, string keyword)
        {
            var completionModel = GetCompletionModel(query);
            AsserIsMatch(completionModel, keyword);

            var completionModelWithFirstChar = GetCompletionModelWithFirstChar(query, keyword);
            AsserIsMatch(completionModelWithFirstChar, keyword);
        }

        private static void AsserIsMatch(CompletionModel completionModel, string keyword)
        {
            var item = completionModel.Items.Single(i => i.InsertionText == keyword);

            Assert.AreEqual(NQueryGlyph.Keyword, item.Glyph);
            Assert.AreEqual(keyword, item.DisplayText);
            Assert.AreEqual(null, item.Description);
            Assert.AreEqual(null, item.Symbol);
        }

        private static void AssertIsEmpty(string query)
        {
            var completionModel = GetCompletionModel(query);
            Assert.AreEqual(0, completionModel.Items.Length);
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsNothing_InMultiLineComment()
        {
            var query = @"
                /*
                 * SELECT |
                 */
                SELECT  *
                FROM    Employees e
            ";

            AssertIsEmpty(query);
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsNothing_InSingleLineComment()
        {
            var query = @"
                // SELECT |
                SELECT  *
                FROM    Employees e
            ";

            AssertIsEmpty(query);
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsNothing_InString()
        {
            var query = @"
                SELECT  'T|'
                FROM    Employees e
            ";

            AssertIsEmpty(query);
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsNothing_InPropertyAccess()
        {
            var query = @"
                SELECT  e.|
                FROM    Employees e
            ";

            AssertIsEmpty(query);
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsNot_IfExpressionStart()
        {
            var query = @"
                SELECT |
            ";

            AssertIsMatch(query, "NOT");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsCast_IfExpressionStart()
        {
            var query = @"
                SELECT |
            ";

            AssertIsMatch(query, "CAST");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsCoalesce_IfExpressionStart()
        {
            var query = @"
                SELECT |
            ";

            AssertIsMatch(query, "COALESCE");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsNullIf_IfExpressionStart()
        {
            var query = @"
                SELECT |
            ";

            AssertIsMatch(query, "NULLIF");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsNull_IfExpressionStart()
        {
            var query = @"
                SELECT |
            ";

            AssertIsMatch(query, "NULL");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsNull_IfAfterIs()
        {
            var query = @"
                SELECT 1 IS |
            ";

            AssertIsMatch(query, "NULL");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsTrue_IfExpressionStart()
        {
            var query = @"
                SELECT |
            ";

            AssertIsMatch(query, "TRUE");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsFalse_IfExpressionStart()
        {
            var query = @"
                SELECT |
            ";

            AssertIsMatch(query, "FALSE");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsExists_IfExpressionStart()
        {
            var query = @"
                SELECT |
            ";

            AssertIsMatch(query, "EXISTS");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsCase_IfExpressionStart()
        {
            var query = @"
                SELECT |
            ";

            AssertIsMatch(query, "CASE");
        }
        
        [TestMethod]
        public void KeywordCompletionProvider_ReturnsAnd_IfAfterExpression()
        {
            var query = @"
                SELECT 1 |
            ";

            AssertIsMatch(query, "AND");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsOr_IfAfterExpression()
        {
            var query = @"
                SELECT 1 |
            ";

            AssertIsMatch(query, "OR");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsIs_IfAfterExpression()
        {
            var query = @"
                SELECT 1 |
            ";

            AssertIsMatch(query, "IS");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsLike_IfAfterExpression()
        {
            var query = @"
                SELECT 1 |
            ";

            AssertIsMatch(query, "LIKE");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsLike_IfAfterSounds()
        {
            var query = @"
                SELECT 'test' SOUNDS |
            ";

            AssertIsMatch(query, "LIKE");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsSounds_IfAfterExpression()
        {
            var query = @"
                SELECT 1 |
            ";

            AssertIsMatch(query, "SOUNDS");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsSimilar_IfAfterExpression()
        {
            var query = @"
                SELECT 1 |
            ";

            AssertIsMatch(query, "SIMILAR");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsBetween_IfAfterExpression()
        {
            var query = @"
                SELECT 1 |
            ";

            AssertIsMatch(query, "BETWEEN");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsIn_IfAfterExpression()
        {
            var query = @"
                SELECT 1 |
            ";

            AssertIsMatch(query, "IN");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsTo_IfAfterSimilar()
        {
            var query = @"
                SELECT 'test' SIMILAR |
            ";

            AssertIsMatch(query, "TO");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsDistinct_IfAfterSelect()
        {
            var query = @"
                SELECT |
            ";

            AssertIsMatch(query, "DISTINCT");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsTop_IfAfterSelect()
        {
            var query = @"
                SELECT |
            ";

            AssertIsMatch(query, "TOP");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsTop_IfAfterAll()
        {
            var query = @"
                SELECT ALL |
            ";

            AssertIsMatch(query, "TOP");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsTop_IfAfterDistinct()
        {
            var query = @"
                SELECT DISTINCT |
            ";

            AssertIsMatch(query, "TOP");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsWith_IfBeforeQuery()
        {
            var query = @"
                |
            ";

            AssertIsMatch(query, "WITH");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsWith_IfAfterTop()
        {
            var query = @"
                SELECT TOP 10 |
            ";

            AssertIsMatch(query, "WITH");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsTies_IfAfterWith()
        {
            var query = @"
                SELECT TOP 10 WITH |
            ";

            AssertIsMatch(query, "TIES");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsAs_IfInColumnAlias()
        {
            var query = @"
                SELECT  1 |
            ";

            AssertIsMatch(query, "AS");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsAs_IfInTableAlias()
        {
            var query = @"
                SELECT  *
                FROM    Employees |
            ";

            AssertIsMatch(query, "AS");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsAs_IfInCast()
        {
            var query = @"
                SELECT CAST(1 |
            ";

            AssertIsMatch(query, "AS");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsAs_IfCommontTableExpressionName()
        {
            var query = @"
                WITH MyCte |
            ";

            AssertIsMatch(query, "AS");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsWhen_IfInCase()
        {
            var query = @"
                SELECT CASE e.ReportsTo
                          |
            ";

            AssertIsMatch(query, "WHEN");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsThen_IfAfterThen()
        {
            var query = @"
                SELECT CASE e.ReportsTo
                          WHEN 2 |
            ";

            AssertIsMatch(query, "THEN");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsElse_IfAfterThen()
        {
            var query = @"
                SELECT CASE e.ReportsTo
                          WHEN 2 THEN 2
                          |
            ";

            AssertIsMatch(query, "ELSE");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsEnd_IfAfterThen()
        {
            var query = @"
                SELECT CASE e.ReportsTo
                          WHEN 2 THEN 2
                       |
            ";

            AssertIsMatch(query, "END");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsBy_IfAfterOrder()
        {
            var query = @"
                SELECT  *
                FROM    Employees
                ORDER   |
            ";

            AssertIsMatch(query, "BY");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsBy_IfAfterGroup()
        {
            var query = @"
                SELECT  *
                FROM    Employees
                GROUP   |
            ";

            AssertIsMatch(query, "BY");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsAsc_IfAfterExpressionInOrderBy()
        {
            var query = @"
                SELECT  *
                FROM    Employees
                ORDER   BY 1 |
            ";

            AssertIsMatch(query, "ASC");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsDesc_IfAfterExpressionInOrderBy()
        {
            var query = @"
                SELECT  *
                FROM    Employees
                ORDER   BY 1, 2 |
            ";

            AssertIsMatch(query, "DESC");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsSelect_IfEmpty()
        {
            var query = @"|";

            AssertIsMatch(query, "SELECT");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsSelect_IfBeforeQuery()
        {
            var query = @"
                |
            ";

            AssertIsMatch(query, "SELECT");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsSelect_IfParenthesizedQuery()
        {
            var query = @"
                (|
            ";

            AssertIsMatch(query, "SELECT");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsSelect_IfParenthesizedExpression()
        {
            var query = @"
                SELECT (|
            ";

            AssertIsMatch(query, "SELECT");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsSelect_IfInInExpression()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                WHERE   e.ReportsTo IN (|
            ";

            AssertIsMatch(query, "SELECT");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsFrom_IfInQuery()
        {
            var query = @"
                SELECT  *
                |
            ";

            AssertIsMatch(query, "FROM");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsWhere_IfInQuery()
        {
            var query = @"
                SELECT  *
                FROM    Employees
                |
            ";

            AssertIsMatch(query, "WHERE");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsGroup_IfInQuery()
        {
            var query = @"
                SELECT  *
                FROM    Employees
                |
            ";

            AssertIsMatch(query, "GROUP");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsHaving_IfInQuery()
        {
            var query = @"
                SELECT  *
                FROM    Employees
                |
            ";

            AssertIsMatch(query, "HAVING");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsOrder_IfAfterQuery()
        {
            var query = @"
                (
                    SELECT  *
                    FROM    Employees
                )
                |
            ";

            AssertIsMatch(query, "ORDER");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsUnion_IfAfterQuery()
        {
            var query = @"
                SELECT  *
                FROM    Employees
                |
            ";

            AssertIsMatch(query, "UNION");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsIntersect_IfAfterQuery()
        {
            var query = @"
                SELECT  *
                FROM    Employees

                UNION

                SELECT  *
                FROM    Employees

                |
            ";

            AssertIsMatch(query, "INTERSECT");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsExcept_IfAfterQuery()
        {
            var query = @"
                (
                    SELECT  *
                    FROM    Employees
                )
                |
            ";

            AssertIsMatch(query, "EXCEPT");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsAll_IfAfterSelect()
        {
            var query = @"
                SELECT  |
            ";

            AssertIsMatch(query, "ALL");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsAll_IfAfterUnion()
        {
            var query = @"
                SELECT  *
                FROM    Employees

                UNION   |
            ";

            AssertIsMatch(query, "ALL");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsAll_IfInAllAny()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                WHERE   e.EmployeeId >= |
            ";

            AssertIsMatch(query, "ALL");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsAny_IfInAllAny()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                WHERE   e.EmployeeId >= |
            ";

            AssertIsMatch(query, "ANY");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsSome_IfInAllAny()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                WHERE   e.EmployeeId >= |
            ";

            AssertIsMatch(query, "SOME");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsJoin_IfAfterTableReference()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            |
            ";

            AssertIsMatch(query, "JOIN");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsJoin_IfAfterInner()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            INNER |
            ";

            AssertIsMatch(query, "JOIN");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsJoin_IfAfterOuter()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            LEFT OUTER |
            ";

            AssertIsMatch(query, "JOIN");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsJoin_IfAfterLeft()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            LEFT |
            ";

            AssertIsMatch(query, "JOIN");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsJoin_IfAfterRight()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            RIGHT |
            ";

            AssertIsMatch(query, "JOIN");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsJoin_IfAfterFull()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            FULL |
            ";

            AssertIsMatch(query, "JOIN");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsInner_IfAfterTableReference()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            |
            ";

            AssertIsMatch(query, "INNER");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsCross_IfAfterTableReference()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            |
            ";

            AssertIsMatch(query, "CROSS");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsLeft_IfAfterTableReference()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            |
            ";

            AssertIsMatch(query, "LEFT");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsRight_IfAfterTableReference()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            |
            ";

            AssertIsMatch(query, "RIGHT");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsFull_IfAfterTableReference()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            |
            ";

            AssertIsMatch(query, "FULL");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsOuter_IfAfterLeft()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            LEFT |
            ";

            AssertIsMatch(query, "OUTER");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsOuter_IfAfterRight()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            RIGHT |
            ";

            AssertIsMatch(query, "OUTER");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsOuter_IfAfterFull()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            FULL |
            ";

            AssertIsMatch(query, "OUTER");
        }

        [TestMethod]
        public void KeywordCompletionProvider_ReturnsOn_IfBeforeJoinCondition()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            INNER JOIN EmployeeTerritories et |
            ";

            AssertIsMatch(query, "ON");
        }
    }
}