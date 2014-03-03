using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace NQueryDesigner.Controls
{
    internal sealed class MoveThumb : Thumb
    {
        public MoveThumb()
        {
            DragDelta += MoveThumbDragDelta;
        }

        private void MoveThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            var item = DataContext as UIElement;
            if (item == null)
                return;

            var left = Canvas.GetLeft(item);
            if (double.IsNaN(left))
                left = 0.0;

            var top = Canvas.GetTop(item);
            if (double.IsNaN(top))
                top = 0.0;

            var newLeft = Math.Max(0.0, left + e.HorizontalChange);
            var newTop = Math.Max(0.0, top + e.VerticalChange);

            Canvas.SetLeft(item, newLeft);
            Canvas.SetTop(item, newTop);
        }
    }
}