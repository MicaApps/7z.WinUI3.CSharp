using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7zip.Helpers
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

    public class IsPausedSymbolIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is true)
            {
                return new SymbolIcon(Symbol.Play);
            }
            return new SymbolIcon(Symbol.Pause);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 提供int类型值包含关系的转换器。
    /// </summary>
    public class IsInt32ValueContainedByConverter : IValueConverter
    {
        /// <summary>
        /// 返回一个bool值，该值指示了<paramref name="parameter"/>代表的int类型集合是否包含<paramref name="value"/>。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter">由int集合构成的字符串，字符串应为以下格式: val1|val2|val3 ...</param>
        /// <param name="language"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int val;
            try
            {
                val = (int)value;
            }
            catch
            {
                val = 0;
            }

            var matchValues =
                from string s in parameter.ToString().Split('|')
                select int.Parse(s);

            return matchValues.Contains(val);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 提供字符串类型值包含关系的转换器。
    /// </summary>
    public class IsStringContainedByConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string val = value.ToString();

            return parameter.ToString().Split('|').Contains(val);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
