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

namespace resourcer
{ 
    public partial class DefaultStyle : ResourceDictionary
    {
        // ---------------------------------------------------------------------------------------
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        // ---------------------------------------------------------------------------------------
        protected Window get(object sender)
        {
            Window win = ((FrameworkElement)sender).TemplatedParent as Window;
            return (win != null) ? win : null;
        }
        // ---------------------------------------------------------------------------------------

        // разворачиваение окна + таскание окна
        private void headerMouseLClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) { get(sender).DragMove(); }
        }

        private void headerButtonClose(object sender, RoutedEventArgs e)
        {
            get(sender).Close();
        }

        private void headerButtonMinimize(object sender, RoutedEventArgs e)
        {
            get(sender).WindowState = WindowState.Minimized;
        }

        private void headerImageMouseClick(object sender, MouseButtonEventArgs e)
        {
            IntPtr handle = new WindowInteropHelper(get(sender)).Handle;
            SendMessage(handle, 0x112, (IntPtr)0xF100, (IntPtr)' ');
        }

        private void resizeWindowGrip(object sender, MouseButtonEventArgs e)
        {
            IntPtr handle = new WindowInteropHelper(get(sender)).Handle;
            SendMessage(handle, 0x112, (IntPtr)(0xF000 + 8), IntPtr.Zero);
            SendMessage(handle, 514, IntPtr.Zero, IntPtr.Zero);
        }

    }
}