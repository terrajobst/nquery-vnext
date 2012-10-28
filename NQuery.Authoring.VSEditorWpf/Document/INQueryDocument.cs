using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Text;

using NQuery.Language.Services;

namespace NQuery.Language.VSEditor.Document
{
    public interface INQueryDocument
    {
        NQueryDocumentType DocumentType { get; set; }
        DataContext DataContext { get; set; }

        Task<SyntaxTree> GetSyntaxTreeAsync();
        Task<SemanticModel> GetSemanticModelAsync();
        ITextSnapshot GetTextSnapshot(SyntaxTree syntaxTree);

        event EventHandler<EventArgs> SyntaxTreeInvalidated;
        event EventHandler<EventArgs> SemanticModelInvalidated;
    }
}