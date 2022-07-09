using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace QekaCrypt
{
    internal class CryptDirectionToStringConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CryptDirection cryptDirection = (CryptDirection)value;
            return cryptDirection switch
            {
                CryptDirection.Crypt => "Шифрование",
                CryptDirection.Decrypt => "Дешифрование",
                _ => throw new ArgumentException("Некорректные данные")
            };
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strCryptDirection = (string)value;
            if (strCryptDirection == "Шифрование")
                return CryptDirection.Crypt;
            else if (strCryptDirection == "Дешифрование")
                return CryptDirection.Decrypt;
            else
                throw new ArgumentException("Некорректные данные");
        }
    }
}