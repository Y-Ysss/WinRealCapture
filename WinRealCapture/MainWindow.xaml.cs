using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
// using System.Windows.Shapes;
using WinRealCapture.Models;

namespace WinRealCapture
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // エラーラベルは非表示にしておく
            HideConsole();

            // バージョン表示
            System.Diagnostics.FileVersionInfo ver =
                System.Diagnostics.FileVersionInfo.GetVersionInfo(
                System.Reflection.Assembly.GetExecutingAssembly().Location);
            this.Title += " Ver" + ver.ProductVersion;

            // ユーザデータから前回のディレクトリ読み出し
            try
            {
                SavingDirectoryTextBox.Text = Properties.Settings.Default["SavingDirectory"].ToString();
                if (SavingDirectoryTextBox.Text == "") throw new Exception("");
            }
            catch (Exception)
            {
                // Default directory
                SavingDirectoryTextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            }

            // キーボードフック開始
            KeyboardHook.StartHook(OnCtrlF2);

            // ListBoxに追加
            foreach (var dir in Properties.Settings.Default.SavedDirectories)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = dir;
                DirectoriesListComboBox.Items.Add(item);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // キーボードフック終了
            KeyboardHook.EndHook();
        }

        // コンソール表示
        private void ShowConsole(int color, string str)
        {
            ConsoleLabel.Visibility = Visibility.Visible;
            HideConsoleLabel.Visibility = Visibility.Visible;
            ConsoleLabel.Content = str;
            if(color == 1)
            {
                ConsoleLabel.Foreground = Brushes.Red;
            } else if(color == 2)
            {
                ConsoleLabel.Foreground = Brushes.Blue;
            }
            else
            {
                ConsoleLabel.Foreground = Brushes.Black;
            }
        }
        private void HideConsole()
        {
            ConsoleLabel.Visibility = Visibility.Collapsed;
            HideConsoleLabel.Visibility = Visibility.Collapsed;
            ConsoleLabel.Content = "";
        }

        // Ctrl + F2 が押されたときに呼ばれるところ
        private void OnCtrlF2()
        {
            // DoCapture();
            Debug.WriteLine("Ctrl+F2");
            HideConsole();
            try
            {
                string savingDirectory = SavingDirectoryTextBox.Text;

                // ディレクトリ有無チェック
                if (!Directory.Exists(savingDirectory))
                {
                    throw new Exception(string.Format("SavingDirectory \"{0}\" not found", savingDirectory));
                }

                // キャプチャ実施
                string fpath = Capture.CaptureActiveWindow(savingDirectory);

                // リストボックスに要素追加
                SavedFileListBox.Items.Add(new SavedItem { FilePath = fpath });

                // この時点でも SavingDirectory をユーザデータとして保存しておく (保存先を手動で変えられた場合を想定)
                Properties.Settings.Default["SavingDirectory"] = savingDirectory;
                Properties.Settings.Default.Save();
            }
            catch(Exception ex)
            {
                ShowConsole(1, "CaptureError : " + ex.Message);
            }
        }



        private void SavingDirectorySelectButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.Description = "Select saving directory";
            dialog.RootFolder = Environment.SpecialFolder.MyComputer;
            dialog.SelectedPath = SavingDirectoryTextBox.Text;
            dialog.ShowNewFolderButton = true;
            var ret = dialog.ShowDialog();
            if(ret== System.Windows.Forms.DialogResult.OK)
            {
                SavingDirectoryTextBox.Text = dialog.SelectedPath;

                // ユーザデータとして保存しておく
                Properties.Settings.Default["SavingDirectory"] = dialog.SelectedPath;
                Properties.Settings.Default.Save();
            }
        }

        // リストボックスの要素が選択されたらプレビュー画像を表示する
        private void SavedFileListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = SavedFileListBox.SelectedItem as SavedItem;
            if (item == null) return;
            try
            {
                // http://stackoverflow.com/questions/6430299/bitmapimage-in-wpf-does-lock-file
                // var image = new BitmapImage(new Uri(item.FilePath)); // これだとファイルがロックされてしまう
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(item.FilePath);
                image.EndInit();
                PreviewImage.Source = image;
            }
            catch (Exception)
            {
                PreviewImage.Source = null;
            }
        }

        // リストボックス要素のダブルクリックまたはEnter押下でファイルを直接開く
        private void SavedFileListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelectedFile();
        }
        private void SavedFileListBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key== Key.Enter)
            {
                OpenSelectedFile();
            }
            else if (e.Key == Key.Delete)
            {
                DeleteSelectedFile();
            }
        }
        private void OpenSelectedFile()
        {
            var item = SavedFileListBox.SelectedItem as SavedItem;
            if (item == null) return;
            try
            {
                HideConsole();
                Process.Start(item.FilePath);
            }
            catch (Exception ex)
            {
                ShowConsole(1, "FileOpenError : " + ex.Message);
            }
        }
        private void DeleteSelectedFile()
        {
            var item = SavedFileListBox.SelectedItem as SavedItem;
            if (item == null) return;
            try
            {
                HideConsole();
                SavedFileListBox.Items.Remove(item);
                PreviewImage.Source = null;
                File.Delete(item.FilePath);
            }
            catch (Exception)
            {
            }
        }

        // 終了メニュー
        private void ExitMenu_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Help - Access GitHub メニュー
        private void GitHubMenu_Click(object sender, RoutedEventArgs e)
        {
            try {
                Process.Start("https://github.com/kobake/WinRealCapture");
            }
            catch(Exception ex)
            {
                ShowConsole(1, "OpenUrlError : " + ex.Message);
            }
        }


        private void AddDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.SavedDirectories.Contains(SavingDirectoryTextBox.Text))
            {
                DirectoriesListComboBox.Items.Add(SavingDirectoryTextBox.Text);
                Properties.Settings.Default.SavedDirectories.Add(SavingDirectoryTextBox.Text);
                Properties.Settings.Default.Save();
                ShowConsole(2, "Add \"" + SavingDirectoryTextBox.Text + "\"");
            }
        }

        private void DirectoriesListComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SavingDirectoryTextBox.Text = Properties.Settings.Default.SavedDirectories[DirectoriesListComboBox.SelectedIndex];
        }

        private void HideConsoleLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            HideConsole();
        }

        // ContextMenu
        private void OpenFileContextMenu_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectedFile();
        }

        private void OpenDirectoryContextMenu_Click(object sender, RoutedEventArgs e)
        {
            var item = SavedFileListBox.SelectedItem as SavedItem;
            System.Diagnostics.Process.Start("EXPLORER.EXE", @"/select," + item.FilePath);
        }

        private void DeleteFileContextMenu_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedFile();
        }

    }
}
