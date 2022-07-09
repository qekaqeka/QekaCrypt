using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QekaCrypt
{
    /// <summary>
    /// Логика взаимодействия для CryptProcessViewPanel.xaml
    /// </summary>
    public partial class CryptProcessViewPanel : UserControl
    {
        public readonly static DependencyProperty CryptProcessViewCollectionProperty;

        private readonly CommandBinding closeCommandBinding;

        private void CloseCommandExecuted(object? sender, ExecutedRoutedEventArgs e)
        {
            if (sender is not CryptProcessView cryptProcessView)
                return;

            CryptProcessViewCollection.Remove(cryptProcessView);

            GC.Collect();
        }
        private static void CloseCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (sender is not CryptProcessView cryptProcessView)
                return;

            e.CanExecute = ! cryptProcessView.CryptProcessHandler.CryptProcess.Occupied;
        }
        static CryptProcessViewPanel()
        {
            CryptProcessViewCollectionProperty = DependencyProperty.Register("CryptProcessViewCollection", typeof(ObservableCollection<CryptProcessView>), typeof(CryptProcessViewPanel));
        }

        public ObservableCollection<CryptProcessView> CryptProcessViewCollection
        {
            get { return (ObservableCollection<CryptProcessView>)GetValue(CryptProcessViewCollectionProperty); }
            set { SetValue(CryptProcessViewCollectionProperty, value); }
        }
        public CryptProcessViewPanel()
        {
            InitializeComponent();
            MaxHeight = textBlock.Height;
            MinHeight = textBlock.Height;
            CryptProcessViewCollection = new();
            CryptProcessViewCollection.CollectionChanged += CryptProcessViewCollectionChanged;

            closeCommandBinding = new();
            closeCommandBinding.Command = ApplicationCommands.Close;
            closeCommandBinding.Executed += CloseCommandExecuted;
            closeCommandBinding.CanExecute += CloseCommandBinding_CanExecute;
        }

        void CryptProcessViewCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender == null)
                return;

            if (CryptProcessViewCollection.Count == 0)
            {
                MaxHeight = textBlock.Height;
                return;
            }

            if(e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    CryptProcessView cryptProcessView = (CryptProcessView)item;
                    cryptProcessView.CommandBindings.Add(closeCommandBinding);
                }
            }

            MaxHeight = textBlock.Height + (CryptProcessViewCollection.First().Height * CryptProcessViewCollection.Count);
        }

    }
}
