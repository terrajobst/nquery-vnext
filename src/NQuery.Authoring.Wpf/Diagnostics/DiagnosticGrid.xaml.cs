using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NQuery.Authoring.CodeActions;
using NQuery.Text;

namespace NQuery.Authoring.Wpf
{
    public sealed partial class DiagnosticGrid
    {
        private Workspace _workspace;

        public DiagnosticGrid()
        {
            InitializeComponent();
        }

        public Workspace Workspace
        {
            get { return _workspace; }
            set
            {
                if (_workspace != null)
                    _workspace.CurrentDocumentChanged -= WorkspaceOnCurrentDocumentChanged;
                _workspace = value;
                if (_workspace != null)
                    _workspace.CurrentDocumentChanged += WorkspaceOnCurrentDocumentChanged;

                UpdateViewModel();
            }
        }

        private void WorkspaceOnCurrentDocumentChanged(object sender, EventArgs e)
        {
            UpdateViewModel();
        }

        private async void UpdateViewModel()
        {
            DataContext = await ComputeViewModelAsync();
        }

        private async Task<DiagnosticsViewModel> ComputeViewModelAsync()
        {
            if (_workspace == null)
                return null;

            var currentDocument = _workspace.CurrentDocument;
            var semanticModel = await currentDocument.GetSemanticModelAsync();
            var semanticsDiagnostics = semanticModel.GetDiagnostics();

            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var syntaxDiagnostics = syntaxTree.GetDiagnostics();

            var diagnostics = syntaxDiagnostics.Concat(semanticsDiagnostics);
            var codeIssues = semanticModel.GetIssues()
                                          .Where(i => i.Kind == CodeIssueKind.Error ||
                                                      i.Kind == CodeIssueKind.Warning);

            return new DiagnosticsViewModel(diagnostics, codeIssues, syntaxTree.Text);
        }

        public TextSpan? SelectedDiagnosticSpan
        {
            get
            {
                var viewModel = DiagnosticDataGrid.SelectedItem as DiagnosticViewModel;
                return viewModel?.Span;
            }
        }
    }
}
