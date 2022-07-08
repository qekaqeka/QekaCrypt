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
            CryptProcess cryptographyProcess = new (SettingsPanel.CryptMode,
                                                        cryptDirection,
                                                        cryptTargetTextBox.Text,
                                                        keyTextBox.Text);

            CryptProcessView cryptographyProcessView = new()
            {
                CryptProcess = cryptographyProcess
            };
            cryptProcessViewPanel.CryptProcessViewCollection.Add(cryptographyProcessView);
            cryptographyProcessView.ResultReady += InstallResult;
            cryptographyProcessView.Start();
        }

        private void InstallResult(object sender, ResultReadyEventArgs e)
        {
            if (sender is not CryptProcessView cryptographyProcessView)
                return;


            Dispatcher.Invoke(() => cryptTargetTextBox.Text = e.Result);
        }

        private void fileReviewButton_Click(object sender, RoutedEventArgs e)
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
