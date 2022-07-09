using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QekaCrypt
{
    public class CryptProcessHandler : DependencyObject
    {
        #region Properties
        private static readonly DependencyPropertyKey PercentPropertyKey;
        public static readonly DependencyProperty PercentProperty;

        private readonly CryptProcess cryptProcess;
        public CryptProcess CryptProcess 
        {
            get
            {
                return cryptProcess;
            }
        }

        private static readonly DependencyPropertyKey StatePropertyKey;
        public static readonly DependencyProperty StateProperty;

        public double Percent
        {
            get => Dispatcher.Invoke(() => (double)GetValue(PercentProperty));
            private set => Dispatcher.Invoke(() => SetValue(PercentPropertyKey, value));
        }

        public static object CoercePercent(DependencyObject dp, object value)
        {
            double num = (double)value;
            if (num < 0.0d)
                return 0.0d;
            return num > 100.0d ? 100.0d : num;
        }

        public string State
        {
            get => Dispatcher.Invoke(() => (string)GetValue(StateProperty));
            private set => Dispatcher.Invoke(() => SetValue(StatePropertyKey, value));
        }

        static CryptProcessHandler()
        {
            FrameworkPropertyMetadata PercentMetadata = new();
            PercentMetadata.AffectsRender = true;
            PercentMetadata.DefaultValue = 0.0d;
            PercentMetadata.CoerceValueCallback = CoercePercent;

            PercentPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Percent), typeof(double), typeof(CryptProcessHandler), PercentMetadata);
            PercentProperty = PercentPropertyKey.DependencyProperty;

            FrameworkPropertyMetadata StateMetadata = new();
            StateMetadata.DefaultValue = string.Empty;

            StatePropertyKey = DependencyProperty.RegisterReadOnly(nameof(State), typeof(string), typeof(CryptProcessHandler), StateMetadata);
            StateProperty = StatePropertyKey.DependencyProperty;
        }
        #endregion

        private BinaryWriter? streamWriter;
        private readonly StringBuilder resultBuilder = new();

        public delegate void ResultReadyHandler(object sender, ResultReadyEventArgs e);
        public event ResultReadyHandler? ResultReady;

        public CryptProcessHandler(CryptProcess cryptProcess)
        {
            this.cryptProcess = cryptProcess;
        }

        private void DataReady(object sender, CryptEventArgs e)
        {
            switch (e.ActionsOnData)
            {
                case ActionsOnData.Write:

                    if (streamWriter != null)
                    {
                        try
                        {
                            streamWriter.Write(e.Data);
                        }
                        catch (Exception)
                        {
                            State = "Ошибка с записью данных";
                            return;
                        }
                        streamWriter.Flush();
                    }
                    break;

                case ActionsOnData.CreateFile:

                    string path = string.Empty;
                    try
                    {
                        path = string.Concat(CryptProcess.CryptTarget.AsSpan(0, CryptProcess.CryptTarget.LastIndexOf('\\') + 1), Encoding.UTF32.GetString(e.Data));
                        streamWriter = new BinaryWriter(File.Open(path, FileMode.Create));
                    }
                    catch (Exception)
                    {
                        State = "Ошибка при создании файла для дешифрованных данных";
                        return;
                    }
                    finally
                    {
                        resultBuilder.Clear();
                        resultBuilder.Append(path);
                    }
                    break;

                case ActionsOnData.CreateDir:


                    break;

                default:


                    break;
            }
            Percent += (double)e.Data.Length / e.TotalSize * 100.0; ;
        }
        private void TextDataReady(object sender, CryptEventArgs e)
        {
            Percent += (double)e.Data.Length / e.TotalSize * 100.0;
            resultBuilder.Append(Encoding.UTF32.GetString(e.Data));
        }

        public void Start()
        {
            State = "OK";
            streamWriter?.Dispose();
            streamWriter = null;
            resultBuilder.Clear();
            CryptProcess.DataReady -= TextDataReady;
            CryptProcess.DataReady -= DataReady;
            CryptProcess.Finished -= CryptProcess_Finished;
            if (CryptProcess.CryptMode == CryptMode.Text)
            {
                CryptProcess.DataReady += TextDataReady;
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
                        State = "Ошибка при создании qkcr файла";
                        return;
                    }
                    finally
                    {
                        resultBuilder.Clear();
                        resultBuilder.Append(path);
                    }
                }
                else
                {
                    streamWriter = null;
                }
                CryptProcess.DataReady += DataReady;
            }
            CryptProcess.Finished += CryptProcess_Finished;
            Task.Run(() => StartCryptProcess(CryptProcess));
        }

        private void StartCryptProcess(CryptProcess cryptProcess)
        {
            try
            {
                cryptProcess.Start();
            }
            catch (Exception ex)
            {
                State = ex.Message;
            }
        }

        private void CryptProcess_Finished(object sender, CryptEventArgs e)
        {
            Percent = 100.0;
            streamWriter?.Dispose();
            streamWriter = null;

            ResultReady?.Invoke(this, new ResultReadyEventArgs(resultBuilder.ToString()));
        }

    }
}
