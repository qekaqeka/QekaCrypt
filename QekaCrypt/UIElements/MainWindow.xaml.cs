using Microsoft.Win32;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace QekaCrypt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (cryptTargetTextBox.Focus())
                cryptTargetTextBox.SelectAll();
        }

        private void Decrypt_Click(object sender, RoutedEventArgs e)
        {
            CryptProcessStart(CryptDirection.Decrypt);
        }

        private void Crypt_Click(object sender, RoutedEventArgs e)
        {
            CryptProcessStart(CryptDirection.Crypt);
        }

        private void CryptProcessStart(CryptDirection cryptDirection)
        {
            CryptProcess cryptProcess = new (SettingsPanel.CryptMode,
                                                        cryptDirection,
                                                        cryptTargetTextBox.Text,
                                                        keyTextBox.Text);
            CryptProcessHandler cryptProcessHandler = new(cryptProcess);
            CryptProcessView cryptProcessView = new()
            {
                CryptProcessHandler = cryptProcessHandler
            };
            cryptProcessViewPanel.CryptProcessViewCollection.Add(cryptProcessView);
            cryptProcessHandler.ResultReady += InstallResult;
            cryptProcessHandler.Start();
        }

        private void InstallResult(object sender, ResultReadyEventArgs e)
        {
            if (sender is not CryptProcessHandler cryptProcessView)
                return;


            Dispatcher.Invoke(() => cryptTargetTextBox.Text = e.Result);
        }

        private void FileReviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsPanel.CryptMode == CryptMode.File)
            {
                OpenFileDialog fileDialog = new()
                {
                    Filter = "All Files (*.*) | *.*",
                    Multiselect = false
                };

                if (fileDialog.ShowDialog() == true)
                    cryptTargetTextBox.Text = fileDialog.FileName;
            }
            else
            {
                Ookii.Dialogs.Wpf.VistaFolderBrowserDialog folderDialog = new()
                {
                    Multiselect = false
                };
                if (folderDialog.ShowDialog() == true)
                    cryptTargetTextBox.Text = folderDialog.SelectedPath;
            }
        }

    }
}
