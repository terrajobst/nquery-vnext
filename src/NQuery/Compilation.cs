using System;

using NQuery.Algebra;
using NQuery.Binding;
using NQuery.BoundNodes;

namespace NQuery
{
    public sealed class Compilation
    {
        private readonly SyntaxTree _syntaxTree;
        private readonly DataContext _dataContext;

        public Compilation(SyntaxTree syntaxTree, DataContext dataContext)
        {
            _syntaxTree = syntaxTree;
            _dataContext = dataContext;
        }

        public SemanticModel GetSemanticModel()
        {
            var bindingResult = Binder.Bind(_syntaxTree.Root, _dataContext);
            return new SemanticModel(this, bindingResult);
        }

        public ShowPlanNode GetShowPlan()
        {
            var bindingResult = Binder.Bind(_syntaxTree.Root, _dataContext);
            var boundRoot = bindingResult.BoundRoot as BoundQuery;
            if (boundRoot == null)
                return null;

            var algebraNode = Algebrizer.Algebrize(boundRoot);
            return ShowPlanBuilder.Build(algebraNode);
        }

        public Compilation WithSyntaxTree(SyntaxTree syntaxTree)
        {
            return _syntaxTree == syntaxTree ? this : new Compilation(syntaxTree, _dataContext);
        }

        public Compilation WithDataContext(DataContext dataContext)
        {
            return _dataContext == dataContext ? this : new Compilation(_syntaxTree, dataContext);
        }

        public static readonly Compilation Empty = new Compilation(SyntaxTree.Empty, DataContext.Empty);

        public SyntaxTree SyntaxTree
        {
            get { return _syntaxTree; }
        }

        public DataContext DataContext
        {
            get { return _dataContext; }
        }
    }
}