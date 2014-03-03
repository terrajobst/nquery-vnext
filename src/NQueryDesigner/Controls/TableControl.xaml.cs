using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NQueryDesigner.Controls
{
    internal partial class TableControl
    {
        public TableControl()
        {
            InitializeComponent();
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            Panel panel;
            UIElement parent;
            if (TryGetContainer(this, out panel, out parent))
            {
                var others = panel.Children.OfType<UIElement>().OrderBy(Panel.GetZIndex);

                var zIndex = 0;
                foreach (var element in others)
                {
                    if (element != parent)
                    {
                        Panel.SetZIndex(element, zIndex);
                        zIndex++;
                    }
                }

                zIndex++;
                Panel.SetZIndex(parent, zIndex);
            }

            base.OnPreviewMouseDown(e);
        }

        private static bool TryGetContainer<T>(UIElement obj, out T container, out UIElement parent)
            where T: class
        {
            parent = obj;
            container = null;

            while (obj != null)
            {
                container = obj as T;
                if (container != null)
                    return true;

                parent = obj;
                obj = VisualTreeHelper.GetParent(obj) as UIElement;
            }

            return false;
        }
    }
}
