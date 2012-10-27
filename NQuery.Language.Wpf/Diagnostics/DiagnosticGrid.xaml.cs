using System;
using System.Collections.Generic;

namespace NQuery.Language.Wpf
{
    public sealed partial class DiagnosticGrid
    {
        public DiagnosticGrid()
        {
            InitializeComponent();
        }

        public void UpdateGrid(IEnumerable<Diagnostic> diagnostics, TextBuffer textBuffer)
        {
            DataContext = new DiagnosticsViewModel(diagnostics, textBuffer);
        }

        public Diagnostic SelectedDiagnostic
        {
            get
            {
                var viewModel = DiagnosticDataGrid.SelectedItem as DiagnosticViewModel;
                return viewModel == null ? null : viewModel.Diagnostic;
            }
        }
    }
}
