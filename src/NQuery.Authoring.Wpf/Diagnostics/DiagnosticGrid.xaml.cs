using NQuery.Text;

namespace NQuery.Authoring.Wpf
{
    public sealed partial class DiagnosticGrid
    {
        public DiagnosticGrid()
        {
            InitializeComponent();
        }

        public void UpdateGrid(IEnumerable<Diagnostic> diagnostics, SourceText sourceText)
        {
            DataContext = diagnostics is null || sourceText is null
                              ? null
                              : new DiagnosticsViewModel(diagnostics, sourceText);
        }

        public Diagnostic SelectedDiagnostic
        {
            get
            {
                var viewModel = DiagnosticDataGrid.SelectedItem as DiagnosticViewModel;
                return viewModel is null ? null : viewModel.Diagnostic;
            }
        }
    }
}
