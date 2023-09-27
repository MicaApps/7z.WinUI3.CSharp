using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7zip.ViewModels
{
    public class PathToFileNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                return Path.GetFileName(value.ToString());
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }

    public class BooleanInversionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool b)
                return !b;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class BasicCalculateConverter : IValueConverter
    {
        public virtual object Convert(object value, Type targetType, object parameter, string language)
        {
            string para = parameter as string;
            char op = para[0];
            double rightVal = double.Parse(para[1..]);
            double leftVal = System.Convert.ToDouble(value);
            return op switch
            {
                '+' => leftVal + rightVal,
                '-' => leftVal - rightVal,
                '*' => leftVal * rightVal,
                '/' => leftVal / rightVal,
                '%' => leftVal % rightVal,
                _ => throw new InvalidDataException($"{nameof(BasicCalculateConverter)}: Invalid Number Or Operator.")
            };
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Convert(value, targetType, parameter, language);
        }
    }
}
