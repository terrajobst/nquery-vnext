using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace NQueryDesigner.Controls
{
    internal sealed class ResizeThumb : Thumb
    {
        public ResizeThumb()
        {
            DragDelta += ResizeThumbDragDelta;
        }

        private void ResizeThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            var item = DataContext as FrameworkElement;

            if (item != null)
            {
                if (double.IsNaN(item.Width))
                    item.Width = item.ActualWidth;

                if (double.IsNaN(item.Height))
                    item.Height = item.ActualHeight;

                double deltaVertical, deltaHorizontal;

                switch (VerticalAlignment)
                {
                    case VerticalAlignment.Bottom:
                        deltaVertical = Math.Min(-e.VerticalChange, item.ActualHeight - item.MinHeight);
                        item.Height -= deltaVertical;
                        break;
                    case VerticalAlignment.Top:
                        deltaVertical = Math.Min(e.VerticalChange, item.ActualHeight - item.MinHeight);
                        Canvas.SetTop(item, Canvas.GetTop(item) + deltaVertical);
                        item.Height -= deltaVertical;
                        break;
                }

                switch (HorizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        deltaHorizontal = Math.Min(e.HorizontalChange, item.ActualWidth - item.MinWidth);
                        Canvas.SetLeft(item, Canvas.GetLeft(item) + deltaHorizontal);
                        item.Width -= deltaHorizontal;
                        break;
                    case HorizontalAlignment.Right:
                        deltaHorizontal = Math.Min(-e.HorizontalChange, item.ActualWidth - item.MinWidth);
                        item.Width -= deltaHorizontal;
                        break;
                }
            }

            e.Handled = true;
        }
    }
}