using System;

namespace NQueryDesigner.ViewModels
{
    internal sealed class TableColumnViewModel : ViewModel
    {
        private readonly DesignerViewModel _parent;
        private readonly string _name;

        private bool _isIncluded;

        public TableColumnViewModel(DesignerViewModel parent, string name)
        {
            _parent = parent;
            _name = name;
        }

        public bool IsIncluded
        {
            get { return _isIncluded; }
            set
            {
                if (_isIncluded != value)
                {
                    _isIncluded = value;
                    OnPropertyChanged();
                    _parent.UpdateForModel();
                }
            }
        }

        public string Name
        {
            get { return _name; }
        }
    }
}