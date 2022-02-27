using System.Windows;

namespace NQuery.Authoring.Wpf
{
    public sealed partial class ShowPlanView
    {
        public static readonly DependencyProperty ShowPlanProperty = DependencyProperty.Register("ShowPlan", typeof(ShowPlanNode), typeof(ShowPlanView), new FrameworkPropertyMetadata((s, e) => ShowPlanNodeChanged(s, e)));

        public ShowPlanView()
        {
            InitializeComponent();
        }

        public ShowPlanNode ShowPlan
        {
            get { return (ShowPlanNode)GetValue(ShowPlanProperty); }
            set { SetValue(ShowPlanProperty, value); }
        }

        private static void ShowPlanNodeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var view = (ShowPlanView) sender;
            var model = (ShowPlanNode) e.NewValue;
            var viewModel = model is null ? null : new ShowPlanViewModel(model);

            view._planTreeView.DataContext = viewModel;
        }
    }
}
