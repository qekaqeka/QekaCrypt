using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace QekaCrypt
{
    public class CryptProcess : DependencyObject
    { 

        public CryptMode CryptMode { get; private set; }
        public CryptDirection CryptDirection { get; private set; }
        public string CryptTarget { get; private set; }
        public byte[] Key { get; private set; }

        public bool IsStarted { get; private set; }

        private readonly object synchDummy = new(); 

        private readonly int keyLength;

        public delegate void CryptEventHandler(object sender, CryptEventArgs e);
        public event CryptEventHandler? DataReady;
        public event CryptEventHandler? Finished;

        public CryptProcess(CryptMode CryptMode, CryptDirection CryptDirection, string CryptTarget, string key)
        {
            this.CryptMode = CryptMode;
            this.CryptDirection = CryptDirection;
            this.CryptTarget = CryptTarget;
            Key = Encoding.UTF32.GetBytes(key);
            keyLength = Key.Length;
            IsStarted = false;
        }

        public void Start()
        {
            try
            {
                Monitor.Enter(synchDummy);
                IsStarted = true;
                switch (CryptMode)
                {
                    case CryptMode.Text:
                        UniformCryptText(CryptTarget);
                        break;
                    case CryptMode.File:
                        UniformCryptFile(CryptTarget);
                        break;
                    case CryptMode.Dir:
                        break;
                    default:
                        throw new Exception("Невозможный CryptMode");
                };
            } 
            finally
            {
                Monitor.Exit(synchDummy);
                IsStarted = false;
            }
        }

        private void UniformCryptText(string text)
        {
            if (CryptDirection == CryptDirection.Crypt)
                CryptText(text);
            else
                DecryptText(text);
        }
        private void UniformCryptFile(string filePath)
        {
            if (CryptDirection == CryptDirection.Crypt)
                CryptFile(filePath);
            else
                DecryptFile(filePath);
        }
        private void DecryptFile(string filePath)
        {
            long totalSize;

            int fileNameByteArrayLenght;

            BinaryReader br;
            try
            {
                br = new(File.Open(filePath, FileMode.Open));
                totalSize = new FileInfo(filePath).Length - 4;
                fileNameByteArrayLenght = BinaryPrimitives.ReadInt32BigEndian(br.ReadBytes(4));
            }
            catch (Exception)
            {
                throw new Exception("Произошла ошибка при открытии файла");
            }

            byte[] fileNameByteArray = DecryptByteArray(br.ReadBytes(fileNameByteArrayLenght));

            DataReady?.Invoke(this, new CryptEventArgs(fileNameByteArray, totalSize));

            int byteArraySize = keyLength * 256;
            byte[] byteArray = new byte[byteArraySize];

            for (long i = fileNameByteArrayLenght; i <= totalSize; i += byteArraySize)
            {
                try
                {
                    byteArray = br.ReadBytes(byteArraySize);
                }
                catch (Exception)
                {
                    throw new Exception("Произошла ошибка при чтении файла");
                }

                DataReady?.Invoke(this, new CryptEventArgs(DecryptByteArray(byteArray), totalSize));
            }

            Finished?.Invoke(this, new CryptEventArgs(Array.Empty<byte>(), totalSize));
            br.Dispose();
        }
        private void CryptFile(string filePath)
        {
            long fileSize;

            BinaryReader br;
            try
            {
                br = new(File.Open(filePath, FileMode.Open));
                fileSize = new FileInfo(filePath).Length;
            }
            catch (Exception)
            {
                throw new Exception("Произошла ошибка при открытии файла");
            }

            long totalSize;

            byte[] fileNameByteArray = Encoding.UTF32.GetBytes(new FileInfo(filePath).Name);

            byte[] fileNameByteArrayLength = BitConverter.GetBytes(fileNameByteArray.Length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(fileNameByteArrayLength);

            byte[] totalExtraInfoArray = fileNameByteArrayLength.Concat(CryptByteArray(fileNameByteArray)).ToArray();

            totalSize = fileSize + totalExtraInfoArray.Length;

            DataReady?.Invoke(this, new CryptEventArgs(totalExtraInfoArray, totalSize));

            int byteArraySize = keyLength * 256;
            byte[] byteArray = new byte[byteArraySize];

            for (long i = 0; i <= fileSize; i += byteArraySize)
            {
                try
                {
                    byteArray = br.ReadBytes(byteArraySize);
                }
                catch (Exception)
                {
                    throw new Exception("Произошла ошибка при чтении файла");
                }

                DataReady?.Invoke(this, new CryptEventArgs(CryptByteArray(byteArray), totalSize));
            }

            Finished?.Invoke(this, new CryptEventArgs(Array.Empty<byte>(), totalSize));
            br.Dispose();          
        }
        private void CryptText(string text)
        {
            int rawDataSize = text.Length;
            int totalSize = rawDataSize * 48;
            //Каждый символ преобразуется в 4 байта и каждый байт представляется в виде 3 символов,
            //каждый из которых весит 4 байта , 4 * 3 * 4 = 48

            StringBuilder result = new();
            for (int i = 0; i <= rawDataSize; i+=keyLength)
            {
                byte[] byteArray;
                if (rawDataSize - i >= keyLength)
                    byteArray = CryptByteArray(Encoding.UTF32.GetBytes(text.Substring(i, keyLength)));
                else
                    byteArray = CryptByteArray(Encoding.UTF32.GetBytes(text.Substring(i)));

                foreach (byte b in byteArray)
                {
                    result.Append(b.ToString("000"));
                }

                DataReady?.Invoke(this, new CryptEventArgs(Encoding.UTF32.GetBytes(result.ToString()), totalSize));
                result.Clear();
            }
            Finished?.Invoke(this, new CryptEventArgs(Array.Empty<byte>(), totalSize));
        }
        private byte[] CryptByteArray(byte[] byteArray)
        {
            for (int i = 0; i < byteArray.Length; i++)
            {
                int index = i % keyLength;
                try
                {
                    byteArray[i] += Convert.ToByte((Key[index] + index + keyLength) % 256);
                }
                catch (Exception)
                {
                    throw new Exception("Некорректный ключ");
                }
            }
            return byteArray;
        }

        private void DecryptText(string text)
        {
            if(text.Length % 12 != 0)
                throw new Exception("Данные повреждены, дешифровка невозможна");
            int byteCount = text.Length / 3; 
            int totalSize = byteCount / 4;

            int keyByteArrayLength = keyLength * 4;

            for (int i = 0; i < byteCount; i+= keyByteArrayLength)
            {
                int bytesForRead;
                if (byteCount - i >= keyByteArrayLength)
                    bytesForRead = keyByteArrayLength;
                else
                    bytesForRead = byteCount - i;

                byte[] bytes = new byte[bytesForRead];

                try
                {
                    for (int j = 0; j < bytesForRead; j++)
                    {
                        string temp =  text.Substring((i + j) * 3, 3);
                        bytes[j] = Convert.ToByte(temp);
                    }

                    DataReady?.Invoke(this, new CryptEventArgs(DecryptByteArray(bytes),totalSize));
                }
                catch (Exception)
                {
                    throw new Exception("Данные повреждены, дешифровка невозможна");
                }
            }
            Finished?.Invoke(this, new CryptEventArgs(Array.Empty<byte>(), totalSize));
        }

       

        private byte[] DecryptByteArray(byte[] byteArray)
        {
            for (int i = 0; i < byteArray.Length; i++)
            {
                int index = i % keyLength;
                try
                {
                    byteArray[i] -= Convert.ToByte((Key[index] + index + keyLength) % 256);
                }
                catch (Exception)
                {
                    throw new Exception("Некорректный ключ");
                }
            }
            return byteArray;
        }
    }
}
