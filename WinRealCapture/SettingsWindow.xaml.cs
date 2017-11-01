using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WinRealCapture
{
    /// <summary>
    /// SettingsWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            foreach (var dir in Properties.Settings.Default.SavedDirectories)
            {
                ListViewItem item = new ListViewItem();
                item.Content = dir;
                SavedDirectoryListView.Items.Add(item);
            }
        }

        HashSet<string> index = new HashSet<string>();
        private void CancelRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            ListViewItem item = SavedDirectoryListView.SelectedItem as ListViewItem;
            index.Remove(item.Content.ToString());
            item.Style = (Style)(Resources[""]);
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            ListViewItem item = SavedDirectoryListView.SelectedItem as ListViewItem;
            index.Add(item.Content.ToString());
            item.Style = (Style)(Resources["ChangeListViewItem"]);
        }

        private void RemoveAllButton_Click(object sender, RoutedEventArgs e)
        {
            SavedDirectoryListView.ItemContainerStyle = (Style)(Resources["ChangeListViewItem"]);
            foreach(ListViewItem item in SavedDirectoryListView.Items)
            {
                index.Add(item.Content.ToString());
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (string i in index)
            {
                Properties.Settings.Default.SavedDirectories.Remove(i);
            }
            Properties.Settings.Default.Save();
            index.Clear();
        }


    }
}
