using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Outlining;

namespace NQuery.Authoring.UnitTests.Outlining
{
    [TestClass]
    public class OutliningCommentTests
    {
        private static OutliningRegionSpan[] GetCommentRegions(string query)
        {
            var syntaxTree = SyntaxTree.ParseQuery(query);
            var commentRegions = syntaxTree.Root.FindRegions().Where(r => r.Text != "SELECT").ToArray();
            return commentRegions;
        }

        [TestMethod]
        public void OutliningComment_DoesNotTriggerForSinglineSlashStarComments()
        {
            var query = @"
                /* The Query */
                SELECT  FirstName, /* First Column */
                        LastName   /* Second Column */
                FROM    Employees
            ";

            var regions = GetCommentRegions(query);
            Assert.AreEqual(0, regions.Length);
        }

        [TestMethod]
        public void OutliningComment_DoesNotTriggerForSinglineSlashSlashComments()
        {
            var query = @"
                // The Query
                SELECT  FirstName, // First Column
                        LastName   // Second Column
                FROM    Employees
            ";

            var regions = GetCommentRegions(query);
            Assert.AreEqual(0, regions.Length);
        }

        [TestMethod]
        public void OutliningComment_DoesNotTriggerForSinglineMinusMinusComments()
        {
            var query = @"
                -- The Query
                SELECT  FirstName, -- First Column
                        LastName   -- Second Column
                FROM    Employees
            ";

            var regions = GetCommentRegions(query);
            Assert.AreEqual(0, regions.Length);
        }

        [TestMethod]
        public void OutliningComment_FindsConsecutiveSlashStarComments()
        {
            var query = @"
                /*
                || This is a query. 
                */
                SELECT  FirstName, /* First
                                      Column */
                        LastName   /* Second
                                      Column */
                FROM    Employees
            ";

            var comment0Span = @"/*
                || This is a query. 
                */";
            var comment0Text = "/* ...";

            var comment1Span = @"/* First
                                      Column */";
            var comment1Text = "/* First ...";

            var comment2Span = @"/* Second
                                      Column */";
            var comment2Text = "/* Second ...";

            var regions = GetCommentRegions(query);
            Assert.AreEqual(3, regions.Length);

            Assert.AreEqual(comment0Span, query.Substring(regions[0].Span));
            Assert.AreEqual(comment0Text, regions[0].Text);
            
            Assert.AreEqual(comment1Span, query.Substring(regions[1].Span));
            Assert.AreEqual(comment1Text, regions[1].Text);
            
            Assert.AreEqual(comment2Span, query.Substring(regions[2].Span));
            Assert.AreEqual(comment2Text, regions[2].Text);
        }

        [TestMethod]
        public void OutliningComment_FindsConsecutiveSlashSlashComments()
        {
            var query = @"
                //---------------------
                // This is a query. 
                //---------------------
                SELECT  FirstName, // First column
                        // Second
                        // Column
                        LastName   
                FROM    Employees
            ";

            var comment0Span = @"//---------------------
                // This is a query. 
                //---------------------";

            var comment0Text = "//--------------------- ...";

            var comment1Span = @"// Second
                        // Column";
            var comment1Text = "// Second ...";

            var regions = GetCommentRegions(query);
            Assert.AreEqual(2, regions.Length);

            Assert.AreEqual(comment0Span, query.Substring(regions[0].Span));
            Assert.AreEqual(comment0Text, regions[0].Text);

            Assert.AreEqual(comment1Span, query.Substring(regions[1].Span));
            Assert.AreEqual(comment1Text, regions[1].Text);

        }

        [TestMethod]
        public void OutliningComment_FindsConsecutiveMinusMinusComments()
        {
            var query = @"
                -----------------------
                -- This is a query. 
                -----------------------
                SELECT  FirstName, -- First Column
                        -- Second
                        -- Column
                        LastName   
                FROM    Employees
            ";

            var comment0 = @"-----------------------
                -- This is a query. 
                -----------------------";
            var comment0Text = "----------------------- ...";

            var comment1 = @"-- Second
                        -- Column";
            var comment1Text = "-- Second ...";

            var regions = GetCommentRegions(query);
            Assert.AreEqual(2, regions.Length);

            Assert.AreEqual(comment0, query.Substring(regions[0].Span));
            Assert.AreEqual(comment0Text, regions[0].Text);

            Assert.AreEqual(comment1, query.Substring(regions[1].Span));
            Assert.AreEqual(comment1Text, regions[1].Text);
        }
    }
}