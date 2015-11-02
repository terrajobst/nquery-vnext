using System;
using System.Collections.Generic;

using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Binding
{
    internal sealed class BoundQueryState
    {
        private readonly BoundQueryState _parent;
        private readonly Dictionary<TableInstanceSymbol, SyntaxToken> _introducedTables = new Dictionary<TableInstanceSymbol, SyntaxToken>();
        private readonly List<BoundComputedValueWithSyntax> _accessibleComputedValues = new List<BoundComputedValueWithSyntax>();
        private readonly List<BoundComputedValueWithSyntax> _computedGroupings = new List<BoundComputedValueWithSyntax>();
        private readonly List<BoundComputedValueWithSyntax> _computedAggregates = new List<BoundComputedValueWithSyntax>(); 
        private readonly List<BoundComputedValueWithSyntax> _computedProjections = new List<BoundComputedValueWithSyntax>(); 
        private readonly Dictionary<ExpressionSyntax, ValueSlot> _replacedExpression = new Dictionary<ExpressionSyntax, ValueSlot>();

        public BoundQueryState(BoundQueryState parent)
        {
            _parent = parent;
        }

        public BoundQueryState Parent
        {
            get { return _parent; }
        }

        public Dictionary<TableInstanceSymbol, SyntaxToken> IntroducedTables
        {
            get { return _introducedTables; }
        }

        public List<BoundComputedValueWithSyntax> AccessibleComputedValues
        {
            get { return _accessibleComputedValues; }
        }

        public List<BoundComputedValueWithSyntax> ComputedGroupings
        {
            get { return _computedGroupings; }
        }

        public List<BoundComputedValueWithSyntax> ComputedAggregates
        {
            get { return _computedAggregates; }
        }

        public List<BoundComputedValueWithSyntax> ComputedProjections
        {
            get { return _computedProjections; }
        }

        public Dictionary<ExpressionSyntax, ValueSlot> ReplacedExpression
        {
            get { return _replacedExpression; }
        }
    }
}