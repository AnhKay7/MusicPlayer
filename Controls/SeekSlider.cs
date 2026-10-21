using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace MusicPlayer.Controls
{
    public class SeekSlider : Slider
    {
        public bool IsUserDragging { get; private set; }
        public event EventHandler? UserSeekCompleted;

        private void UpdateValueFromMouse(MouseEventArgs e)
        {
            double ratio = e.GetPosition(this).X / ActualWidth;
            Value = Minimum + Math.Clamp(ratio, 0, 1) * (Maximum - Minimum);
        }
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            IsUserDragging = true;
            CaptureMouse();
            UpdateValueFromMouse(e);
            e.Handled = true;
        }
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (IsUserDragging)
                UpdateValueFromMouse(e);
        }
        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (!IsUserDragging)
                return;
            IsUserDragging = false;
            ReleaseMouseCapture();
            UserSeekCompleted?.Invoke(this, e);
        }
    }
}
