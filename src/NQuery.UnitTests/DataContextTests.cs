using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.UnitTests
{
    [TestClass]
    public class DataContextTests
    {
        [TestMethod]
        public void DataContext_AddingNoTablesYieldsSameInstance()
        {
            var before = DataContext.Empty;
            var after1 = before.AddTables();
            var after2 = before.AddTables(null);
            Assert.AreSame(before, after1);
            Assert.AreSame(before, after2);
        }

        [TestMethod]
        public void DataContext_ReplacingWithSameTablesYieldsSameInstance()
        {
            var before = DataContext.Empty;
            var after1 = before.WithTables(before.Tables);
            Assert.AreSame(before, after1);
        }

        [TestMethod]
        public void DataContext_AddingNoRelationsYieldsSameInstance()
        {
            var before = DataContext.Empty;
            var after1 = before.AddRelations();
            var after2 = before.AddRelations(null);
            Assert.AreSame(before, after1);
            Assert.AreSame(before, after2);
        }

        [TestMethod]
        public void DataContext_ReplacingWithSameRelationsYieldsSameInstance()
        {
            var before = DataContext.Empty;
            var after = before.WithRelations(before.Relations);
            Assert.AreSame(before, after);
        }

        [TestMethod]
        public void DataContext_AddingNoFunctionsYieldsSameInstance()
        {
            var before = DataContext.Empty;
            var after1 = before.AddFunctions();
            var after2 = before.AddFunctions(null);
            Assert.AreSame(before, after1);
            Assert.AreSame(before, after2);
        }

        [TestMethod]
        public void DataContext_ReplacingWithSameFunctionsYieldsSameInstance()
        {
            var before = DataContext.Empty;
            var after = before.WithFunctions(before.Functions);
            Assert.AreSame(before, after);
        }

        [TestMethod]
        public void DataContext_AddingNoAggregatesYieldsSameInstance()
        {
            var before = DataContext.Empty;
            var after1 = before.AddAggregates();
            var after2 = before.AddAggregates(null);
            Assert.AreSame(before, after1);
            Assert.AreSame(before, after2);
        }

        [TestMethod]
        public void DataContext_ReplacingWithSameAggregatesYieldsSameInstance()
        {
            var before = DataContext.Empty;
            var after = before.WithAggregates(before.Aggregates);
            Assert.AreSame(before, after);
        }

        [TestMethod]
        public void DataContext_AddingNoVariablesYieldsSameInstance()
        {
            var before = DataContext.Empty;
            var after1 = before.AddVariables();
            var after2 = before.AddVariables(null);
            Assert.AreSame(before, after1);
            Assert.AreSame(before, after2);
        }

        [TestMethod]
        public void DataContext_ReplacingWithSameVariablesYieldsSameInstance()
        {
            var before = DataContext.Empty;
            var after = before.WithVariables(before.Variables);
            Assert.AreSame(before, after);
        }

        [TestMethod]
        public void DataContext_ReplacingWithSamePropertyProvidersYieldsSameInstance()
        {
            var before = DataContext.Empty;
            var after = before.WithPropertyProviders(before.PropertyProviders);
            Assert.AreSame(before, after);
        }

        [TestMethod]
        public void DataContext_ReplacingWithSameMethodProvidersYieldsSameInstance()
        {
            var before = DataContext.Empty;
            var after = before.WithMethodProviders(before.MethodProviders);
            Assert.AreSame(before, after);
        }

        [TestMethod]
        public void DataContext_ReplacingWithSameComparersYieldsSameInstance()
        {
            var before = DataContext.Empty;
            var after = before.WithComparers(before.Comparers);
            Assert.AreSame(before, after);
        }
    }
}