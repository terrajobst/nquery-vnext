using System;
using System.Windows;

using NQuery;
using NQuery.Authoring.Document;
using NQuery.Text;

namespace NQueryViewer.Editor
{
    public interface IEditorView
    {
        UIElement Element { get; }
        int CaretPosition { get; set; }
        TextSpan Selection { get; set; }
        NQueryDocument Document { get; }

        void Focus();

        event EventHandler CaretPositionChanged;
        event EventHandler SelectionChanged;
    }
}