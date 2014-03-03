using System;

namespace NQueryDesigner.ViewModels
{
    internal sealed class SelectionRowViewModel : ViewModel
    {
        private readonly DesignerViewModel _parent;
        private string _column;
        private string _alias;
        private string _table;
        private string _aggregate;
        private string _sort;

        public SelectionRowViewModel(DesignerViewModel parent)
        {
            _parent = parent;
        }

        private void Update()
        {
            _parent.UpdateForModel();
        }

        public string Column
        {
            get { return _column; }
            set
            {
                if (_column != value)
                {
                    _column = value;
                    OnPropertyChanged();
                    Update();
                }
            }
        }

        public string Alias
        {
            get { return _alias; }
            set
            {
                if (_alias != value)
                {
                    _alias = value;
                    OnPropertyChanged();
                    Update();
                }
            }
        }

        public string Table
        {
            get { return _table; }
            set
            {
                if (_table != value)
                {
                    _table = value;
                    OnPropertyChanged();
                    Update();
                }
            }
        }

        public string Aggregate
        {
            get { return _aggregate; }
            set
            {
                if (_aggregate != value)
                {
                    _aggregate = value;
                    OnPropertyChanged();
                    Update();
                }
            }
        }

        public string Sort
        {
            get { return _sort; }
            set
            {
                if (_sort != value)
                {
                    _sort = value;
                    OnPropertyChanged();
                    Update();
                }
            }
        }
    }
}