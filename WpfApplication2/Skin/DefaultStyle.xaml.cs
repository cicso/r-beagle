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
        protected Window get(object sender)
        {
            Window win = ((FrameworkElement)sender).TemplatedParent as Window;
            return (win != null) ? win : null;
        }

        // разворачиваение окна + таскание окна
        private void headerMouseLClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) { get(sender).DragMove(); }
        }

        private void headerImageMouseClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void headerButtonClose(object sender, RoutedEventArgs e)
        {
            get(sender).Close();
        }

        private void headerButtonMinimize(object sender, RoutedEventArgs e)
        {
            get(sender).WindowState = WindowState.Minimized;
        }

    }
}
