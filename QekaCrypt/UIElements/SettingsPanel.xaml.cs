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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.IconPacks;

namespace QekaCrypt
{
    /// <summary>
    /// Логика взаимодействия для SettingsPanel.xaml
    /// </summary>
    public partial class SettingsPanel : UserControl
    {
        public SettingsPanel()
        {
            InitializeComponent();
        }
        public CryptMode CryptMode
        {
            get { return (CryptMode)GetValue(CryptModeProperty); }
            private set { SetValue(CryptModeProperty, value); }
        }

        public bool DeleteAfterAction 
        { 
            get
            {
                return DeleteAfterActionCheckBox.IsChecked??throw new ArgumentNullException();
            } 
        }

        public static readonly DependencyProperty CryptModeProperty; 

        static SettingsPanel()
        {
            var fwm = new FrameworkPropertyMetadata(CryptMode.Text);
            CryptModeProperty = DependencyProperty.Register("CryptMode",typeof(CryptMode), typeof(SettingsPanel), fwm);
        }

        
        
        private void HideShowModesButton_Click(object sender, RoutedEventArgs e)
        {
            
            DoubleAnimation doubleAnimation = new();
            switch (SettingPanelBorder.Width)
            {
                case 0:
                    doubleAnimation.To = 200;
                    break;
                case 200:
                    doubleAnimation.To = 0;
                    break;
                default:
                    return;
            }
            doubleAnimation.Duration = TimeSpan.FromSeconds(0.5);
            doubleAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn };
            doubleAnimation.Completed += DoubleAnimation_Completed;
            SettingPanelBorder.BeginAnimation(WidthProperty, doubleAnimation);
        }

        private void DoubleAnimation_Completed(object? sender, EventArgs e)
        {
            if (SettingPanelBorder.Width == 200)
                HideShowModesButton.Content = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.ArrowLeftSolid };
            else if(SettingPanelBorder.Width == 0)
                HideShowModesButton.Content = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.ArrowRightSolid };
        }

        private void TextMode_Checked(object sender, RoutedEventArgs e)
        {
            CryptMode = CryptMode.Text;
        }

        private void FileMode_Checked(object sender, RoutedEventArgs e)
        {
            CryptMode = CryptMode.File;
        }

        private void DirMode_Checked(object sender, RoutedEventArgs e)
        {
            CryptMode = CryptMode.Dir;
        }
    }
}
