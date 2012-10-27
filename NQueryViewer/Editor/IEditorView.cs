using System;
using System.Windows;

using NQuery.Language;
using NQuery.Language.Services;

namespace NQueryViewer.Editor
{
    public interface IEditorView
    {
        UIElement Element { get; }
        int CaretPosition { get; set; }
        TextSpan Selection { get; set; }
        NQueryDocumentType DocumentType { get; set; }
        DataContext DataContext { get; set; }
        SyntaxTree SyntaxTree { get; }
        SemanticModel SemanticModel { get; }

        void Focus();

        event EventHandler CaretPositionChanged;
        event EventHandler SelectionChanged;
        event EventHandler DocumentTypeChanged;
        event EventHandler DataContextChanged;
        event EventHandler SyntaxTreeChanged;
        event EventHandler SemanticModelChanged;
    }
}