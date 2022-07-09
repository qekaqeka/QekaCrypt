using QekaCrypt;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;

namespace QekaCrypt
{
    public partial class CryptProcessView : UserControl
    {
        public CryptProcessHandler CryptProcessHandler
        {
            get { return (CryptProcessHandler)GetValue(CryptProcessHandlerProperty); }
            set { SetValue(CryptProcessHandlerProperty, value); }
        }
        public static readonly DependencyProperty CryptProcessHandlerProperty =
            DependencyProperty.Register("CryptProcessHandler", typeof(CryptProcessHandler), typeof(CryptProcessView));

        public CryptProcessView() => InitializeComponent();

        private void RestartButton_Click(object sender, RoutedEventArgs e) => CryptProcessHandler.Start();

        private void InfoButton_Click(object sender, RoutedEventArgs e) => infoPopup.IsOpen = true;

        private void CloseInfoPopup_Executed(object sender, ExecutedRoutedEventArgs e) => infoPopup.IsOpen = false;

        private void CloseInfoPopup_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = infoPopup.IsOpen;
    }
}
