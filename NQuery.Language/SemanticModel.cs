﻿using System;
using System.Collections.Generic;
using System.Linq;
using NQuery.Language.Binding;
using NQuery.Language.BoundNodes;
using NQuery.Language.Symbols;

namespace NQuery.Language
{
    public sealed class SemanticModel
    {
        private readonly Compilation _compilation;
        private readonly BindingResult _bindingResult;

        internal SemanticModel(Compilation compilation, BindingResult bindingResult)
        {
            _compilation = compilation;
            _bindingResult = bindingResult;
        }

        public Compilation Compilation
        {
            get { return _compilation; }
        }

        public Symbol GetSymbol(ExpressionSyntax expression)
        {
            var boundExpression = GetBoundExpression(expression);
            return boundExpression == null ? null : boundExpression.Symbol;
        }

        public Type GetExpressionType(ExpressionSyntax expression)
        {
            var boundExpression = GetBoundExpression(expression);
            return boundExpression == null ? null : boundExpression.Type;
        }

        private BoundExpression GetBoundExpression(ExpressionSyntax expression)
        {
            return _bindingResult.GetBoundNode(expression) as BoundExpression;
        }

        public TableInstanceSymbol GetDeclaredSymbol(TableReferenceSyntax tableReference)
        {
            var result = _bindingResult.GetBoundNode(tableReference) as BoundNamedTableReference;
            return result == null ? null : result.TableInstance;
        }

        public Symbol GetDeclaredSymbol(DerivedTableReferenceSyntax tableReference)
        {
            var result = _bindingResult.GetBoundNode(tableReference) as BoundDerivedTableReference;
            return result == null ? null : result.TableInstance;
        }

        public IEnumerable<Diagnostic> GetDiagnostics()
        {
            return _bindingResult.Diagnostics;
        }

        public IEnumerable<Symbol> LookupSymbols(int position)
        {
            var node = FindClosestNodeWithBindingContext(_bindingResult.Root, position, 0);
            var bindingContext = node == null ? null : _bindingResult.GetBindingContext(node);
            return bindingContext != null ? bindingContext.LookupSymbols() : Enumerable.Empty<Symbol>();
        }

        private SyntaxNode FindClosestNodeWithBindingContext(SyntaxNode root, int position, int lastPosition)
        {
            foreach (var nodeOrToken in root.ChildNodesAndTokens())
            {
                if (lastPosition <= position && position < nodeOrToken.Span.End)
                {
                    if (nodeOrToken.IsToken)
                        return null;

                    var node = nodeOrToken.AsNode();
                    var result = FindClosestNodeWithBindingContext(node, position, lastPosition);
                    if (result != null)
                        return result;

                    if (_bindingResult.GetBindingContext(node) != null)
                        return node;
                }

                lastPosition = nodeOrToken.Span.End;
            }

            return null;
        }
    }
}