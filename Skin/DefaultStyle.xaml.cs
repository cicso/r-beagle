using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Collections;

namespace WpfApplication2
{ 
    public partial class DefaultStyle : ResourceDictionary
    {
        // ---------------------------------------------------------------------------------------
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        // ---------------------------------------------------------------------------------------
        public enum resizeDirection : int
        {
            left = 1,
            right = 2,
            top = 3,
            bottom = 6,
            topleft = 4,
            topright = 5,
            bottomleft = 7,
            bottomright = 8
        }
        // ---------------------------------------------------------------------------------------

        protected Window get(object sender)
        {
            Window win = ((FrameworkElement)sender).TemplatedParent as Window;
            return (win != null) ? win : null;
        }

        // разворачиваение окна + таскание окна
        private void headerMouseLClick(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount > 1) { headerButtonMaximize(sender, null); }
            else if (e.LeftButton == MouseButtonState.Pressed) { get(sender).DragMove(); }
        }

        private void headerImageMouseClick(object sender, MouseButtonEventArgs e)
        {
            if (get(sender).WindowState == WindowState.Maximized)
                return;

            IntPtr handle = new WindowInteropHelper(get(sender)).Handle;
            SendMessage(handle, 0x112, (IntPtr)0xF100, (IntPtr)' ');
        }

        private void headerButtonMaximize(object sender, RoutedEventArgs e)
        {
            get(sender).WindowState
                = (get(sender).WindowState == WindowState.Maximized)
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void headerButtonClose(object sender, RoutedEventArgs e)
        {
            get(sender).Close();
        }

        private void headerButtonMinimize(object sender, RoutedEventArgs e)
        {
            get(sender).WindowState = WindowState.Minimized;
        }

        protected void resizeWindow(object sender, resizeDirection direction)
        {
            IntPtr handle = new WindowInteropHelper(get(sender)).Handle;
            SendMessage(handle, 0x112, (IntPtr)(0xF000 + direction), IntPtr.Zero);
            SendMessage(handle, 514, IntPtr.Zero, IntPtr.Zero);
        }

        private void resizeWindowTop(object sender, MouseButtonEventArgs e) { resizeWindow(sender, resizeDirection.top); }
        private void resizeWindowBottom(object sender, MouseButtonEventArgs e) { resizeWindow(sender, resizeDirection.bottom); }
        private void resizeWindowLeft(object sender, MouseButtonEventArgs e) { resizeWindow(sender, resizeDirection.left); }
        private void resizeWindowRight(object sender, MouseButtonEventArgs e) { resizeWindow(sender, resizeDirection.right); }
        private void resizeWindowTopLeft(object sender, MouseButtonEventArgs e) { resizeWindow(sender, resizeDirection.topleft); }
        private void resizeWindowTopRight(object sender, MouseButtonEventArgs e) { resizeWindow(sender, resizeDirection.topright); }
        private void resizeWindowBottomLeft(object sender, MouseButtonEventArgs e) { resizeWindow(sender, resizeDirection.bottomleft); }
        private void resizeWindowBottomRight(object sender, MouseButtonEventArgs e) { resizeWindow(sender, resizeDirection.bottomright); }

    }
}
