using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace NQuery.Language.VSEditor
{
    internal sealed class NQueryBraceTagger : ITagger<ITextMarkerTag>
    {
        private readonly ITextView _textView;
        private readonly ITextBuffer _textBuffer;
        private readonly INQuerySyntaxTreeManager _syntaxTreeManager;

        public NQueryBraceTagger(ITextView textView, ITextBuffer textBuffer, INQuerySyntaxTreeManager syntaxTreeManager)
        {
            _textView = textView;
            _textBuffer = textBuffer;
            _textView.Caret.PositionChanged += CaretOnPositionChanged;
            _syntaxTreeManager = syntaxTreeManager;
            _syntaxTreeManager.SyntaxTreeChanged += SyntaxTreeManagerOnSyntaxTreeChanged;
        }

        private void CaretOnPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            InvalidateTags();
        }

        private void SyntaxTreeManagerOnSyntaxTreeChanged(object sender, EventArgs e)
        {
            InvalidateTags();
        }

        private void InvalidateTags()
        {
            var snapshot = _textBuffer.CurrentSnapshot;
            var snapshotSpan = new SnapshotSpan(snapshot, 0, snapshot.Length);
            OnTagsChanged(new SnapshotSpanEventArgs(snapshotSpan));
        }

        public IEnumerable<ITagSpan<ITextMarkerTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            var position = _textView.Caret.Position.BufferPosition.Position;
            TextSpan left;
            TextSpan right;
            if (!FindBraces(position, out left, out right))
                return Enumerable.Empty<ITagSpan<ITextMarkerTag>>();

            return new[]
            {
                CreateBraceTag(left),
                CreateBraceTag(right)
            };
        }

        private ITagSpan<ITextMarkerTag> CreateBraceTag(TextSpan textSpan)
        {
            var snapshot = _textBuffer.CurrentSnapshot;
            var snapshotSpan = new SnapshotSpan(snapshot, textSpan.Start, textSpan.Length);
            var tag = new TextMarkerTag("bracehighlight");
            var tagSpan = new TagSpan<ITextMarkerTag>(snapshotSpan, tag);
            return tagSpan;
        }

        private void OnTagsChanged(SnapshotSpanEventArgs e)
        {
            var handler = TagsChanged;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        private bool IsBrace(SyntaxKind syntaxKind)
        {
            switch (syntaxKind)
            {
                case SyntaxKind.LeftParenthesisToken:
                case SyntaxKind.RightParenthesisToken:
                case SyntaxKind.CaseKeyword:
                case SyntaxKind.EndKeyword:
                case SyntaxKind.StringLiteralToken:
                case SyntaxKind.DateLiteralToken:
                    return true;

                default:
                    return false;
            }
        }

        private SyntaxNode FindParent(SyntaxNode root, bool isLeft, int position)
        {
            if (!root.FullSpan.Contains(position))
                return null;

            SyntaxToken token;
            if (FindBrace(root, position, isLeft, out token))
                return root;

            var nodes = from n in root.ChildNodesAndTokens()
                        where n.IsNode
                        select n.AsNode();

            return (from n in nodes
                    let p = FindParent(n, isLeft, position)
                    where p != null
                    select p).FirstOrDefault();               
        }

        private bool FindBrace(SyntaxNode parent, int position, bool isLeft, out SyntaxToken token)
        {
            var tokens = from t in parent.ChildNodesAndTokens()
                         where t.IsToken
                         select t.AsToken();

            foreach (var t in tokens)
            {
                var isMatch = isLeft
                                  ? t.Span.Start == position
                                  : t.Span.End == position + 1;

                if (isMatch && IsBrace(t.Kind) && !t.IsMissing)
                {
                    token = t;
                    return true;
                }
            }
            
            token = new SyntaxToken();
            return false;
        }

        private bool FindBraces(int position, out TextSpan left, out TextSpan right)
        {
            if (FindBraces(position, true, out left, out right))
                return true;

            if (FindBraces(position - 1, false, out left, out right))
                return true;

            return false;
        }

        private bool FindBraces(int position, bool isLeft, out TextSpan left, out TextSpan right)
        {
            left = new TextSpan();
            right = new TextSpan();

            var syntaxTree = _syntaxTreeManager.SyntaxTree;
            if (syntaxTree == null)
                return false;

            var parent = FindParent(syntaxTree.Root, isLeft, position);
            if (parent == null)
                return false;

            SyntaxToken token;
            if (!FindBrace(parent, position, isLeft, out token))
                return false;

            switch (token.Kind)
            {
                case SyntaxKind.LeftParenthesisToken:
                    if (!isLeft)
                        return false;
                    left = token.Span;
                    return FindMatchingBrace(position, 1, parent, SyntaxKind.RightParenthesisToken, out right);
                case SyntaxKind.CaseKeyword:
                    if (!isLeft)
                        return false;
                    left = token.Span;
                    return FindMatchingBrace(position, 1, parent, SyntaxKind.EndKeyword, out right);
                case SyntaxKind.RightParenthesisToken:
                    if (isLeft)
                        return false;
                    right = token.Span;
                    return FindMatchingBrace(position, -1, parent, SyntaxKind.LeftParenthesisToken, out left);
                case SyntaxKind.EndKeyword:
                    if (isLeft)
                        return false;
                    right = token.Span;
                    return FindMatchingBrace(position, -1, parent, SyntaxKind.CaseKeyword, out left);

                case SyntaxKind.StringLiteralToken:
                case SyntaxKind.DateLiteralToken:
                    left = new TextSpan(token.Span.Start, 1);
                    right = new TextSpan(token.Span.End - 1, 1);
                    return true;

                default:
                    return false;
            }
        }


        private bool FindMatchingBrace(int position, int direction, SyntaxNode parent, SyntaxKind syntaxKind, out TextSpan right)
        {
            var tokens = from t in parent.ChildNodesAndTokens()
                         where t.Kind == syntaxKind && t.IsToken
                         select t.AsToken();

            var relevantTokens = direction < 0
                                     ? from t in tokens
                                       where t.Span.End <= position
                                       select t
                                     : from t in tokens
                                       where position < t.Span.Start
                                       select t;

            right = new TextSpan();
            var found = false;

            foreach (var token in relevantTokens)
            {
                if (!found)
                {
                    right = token.Span;
                    found = true;
                }
                else
                    return false;
            }

            return found;
        }
    }
}