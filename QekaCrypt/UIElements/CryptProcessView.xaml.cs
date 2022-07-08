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


#nullable enable
namespace QekaCrypt
{
    public partial class CryptProcessView : UserControl
    { 

        private static readonly DependencyPropertyKey PercentPropertyKey;
        public static readonly DependencyProperty PercentProperty;

        public static readonly DependencyProperty CryptProcessProperty;

        private static readonly DependencyPropertyKey StatePropertyKey;
        public static readonly DependencyProperty StateProperty;

        private BinaryWriter? streamWriter;
        private readonly StringBuilder resultBuilder = new();

        public double Percent
        {
            get => (double)GetValue(PercentProperty);
            private set => SetValue(PercentPropertyKey, (object)value);
        }

        public static object CoercePercent(DependencyObject dp, object value)
        {
            double num = (double)value;
            if (num < 0.0d)
                return 0.0d;
            return num > 100.0d ? 100.0d : num;
        }

        public CryptProcess CryptProcess
        {
            get => (CryptProcess)GetValue(CryptProcessView.CryptProcessProperty);
            set => SetValue(CryptProcessProperty, value);
        }

        public string State
        {
            get => (string)GetValue(StateProperty);
            private set => SetValue(StatePropertyKey, value);
        }

        static CryptProcessView()
        {
            FrameworkPropertyMetadata propertyMetadata1 = new();
            propertyMetadata1.AffectsRender = true;
            propertyMetadata1.DefaultValue = 0.0d;
            propertyMetadata1.CoerceValueCallback = CoercePercent;

            PercentPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Percent), typeof(double), typeof(CryptProcessView), propertyMetadata1);
            PercentProperty = PercentPropertyKey.DependencyProperty;

            CryptProcessProperty = DependencyProperty.Register(nameof(CryptProcess), typeof(CryptProcess), typeof(CryptProcessView));

            FrameworkPropertyMetadata propertyMetadata2 = new();
            propertyMetadata2.DefaultValue = string.Empty;

            StatePropertyKey = DependencyProperty.RegisterReadOnly(nameof(State), typeof(string), typeof(CryptProcessView), propertyMetadata2);
            StateProperty = StatePropertyKey.DependencyProperty;
        }

        public event ResultReadyHandler? ResultReady;

        public CryptProcessView() => InitializeComponent();

        private void DataReady(object sender, CryptEventArgs e)
        {
            if (streamWriter != null)
            {
                try
                {
                    streamWriter.Write(e.Data);
                }
                catch (Exception)
                {
                    Error("Ошибка с записью данных");
                    return;
                }
                streamWriter.Flush();
            }
            else if (streamWriter == null)
            {
                string path = string.Empty;
                try
                {
                    path = Dispatcher.Invoke(() => CryptProcess.CryptTarget.Substring(0, CryptProcess.CryptTarget.LastIndexOf('\\') + 1)) + Encoding.UTF32.GetString(e.Data);
                    streamWriter = new BinaryWriter(File.Open(path, FileMode.Create));
                }
                catch (Exception)
                {
                    Error("Ошибка при создании файла для дешифрованных данных");
                    return;
                }
                finally
                {
                    resultBuilder.Clear();
                    resultBuilder.Append(path);
                }
            }
            double percent = Dispatcher.Invoke(() => Percent) + (double)e.Data.Length / e.TotalSize * 100.0;
            Dispatcher.Invoke(() => Percent = percent);
        }

        private void TextDataReady(object sender, CryptEventArgs e)
        {
            double percent = Dispatcher.Invoke(() => Percent) + (double)e.Data.Length / e.TotalSize * 100.0;
            Dispatcher.Invoke(() => Percent = percent);
            resultBuilder.Append(Encoding.UTF32.GetString(e.Data));
        }

        public void Start()
        {
            resultBuilder.Clear();
            CryptProcess.DataReady -= TextDataReady;
            CryptProcess.DataReady -= DataReady;
            CryptProcess.Finished -= CryptProcess_Finished;
            if (CryptProcess.CryptMode == CryptMode.Text)
            {
                CryptProcess.DataReady += TextDataReady;
                streamWriter?.Dispose();
                streamWriter = null;
            }
            else
            {
                if (CryptProcess.CryptDirection == CryptDirection.Crypt)
                {
                    string path = string.Empty;
                    try
                    {
                        path = CryptProcess.CryptTarget + ".qkcr";
                        streamWriter = new BinaryWriter(File.Open(path, FileMode.Create));
                    }
                    catch (Exception)
                    {
                        Error("Ошибка при создании qkcr файла");
                        return;
                    }
                    finally
                    {
                        resultBuilder.Clear();
                        resultBuilder.Append(path);
                    }
                }
                else
                    streamWriter = null;
                CryptProcess.DataReady += new CryptProcess.CryptEventHandler(DataReady);
            }
            Dispatcher.Invoke(() => RestoreInterface());
            CryptProcess.Finished += CryptProcess_Finished;
            Task.Run(() => StartCryptProcess());
        }

        private void StartCryptProcess()
        {
            try
            {
                CryptProcess.Start();
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }

        private void Error(string message) => Dispatcher.Invoke(() =>
        {
            mainTextBlock.Foreground = (Brush)Brushes.Red;
            State = message;
            restartButton.Visibility = Visibility.Visible;
            mainProgressBar.Foreground = (Brush)Brushes.DarkRed;
        });

        private void RestoreInterface()
        {
            mainProgressBar.Foreground = (Brush)Brushes.Green;
            restartButton.Visibility = Visibility.Collapsed;
            mainTextBlock.Foreground = (Brush)Brushes.Black;
            State = "ОК";
            Percent = 0.0;
        }

        private void CryptProcess_Finished(object sender, CryptEventArgs e)
        {
            Dispatcher.Invoke((Action)(() => Percent = 100.0));
            streamWriter?.Dispose();
            streamWriter = null;

            ResultReady?.Invoke(this, new ResultReadyEventArgs(resultBuilder.ToString()));
        }

        private void restartButton_Click(object sender, RoutedEventArgs e) => Start();

        private void infoButton_Click(object sender, RoutedEventArgs e) => infoPopup.IsOpen = true;

        private void closeInfoPopup_Executed(object sender, ExecutedRoutedEventArgs e) => infoPopup.IsOpen = false;

        private void closeInfoPopup_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = infoPopup.IsOpen;
      
        public delegate void ResultReadyHandler(object sender, ResultReadyEventArgs e);
    }
}
