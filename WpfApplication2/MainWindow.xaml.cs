using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Collections;

namespace WpfApplication2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int value = 0;
        public MainWindow()
        {
            InitializeComponent();

            myProgBar.Value = value;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            value++;
            myProgBar.Value = value;
        }

        private void folderAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = dialog.SelectedPath;
                item.ToolTip = item.Content;
                folderList.Items.Add(item);
            }
        }

        private void folderDel_Click(object sender, RoutedEventArgs e)
        {
            var selecter = new List<ListBoxItem>(folderList.SelectedItems.Cast<ListBoxItem>());
            foreach (ListBoxItem item in selecter)
                folderList.Items.Remove(item);
        }
    }
}
