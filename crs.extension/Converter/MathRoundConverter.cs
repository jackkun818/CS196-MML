using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace crs.extension.Converter
{
    public class MathRoundConverter : MarkupExtension, IValueConverter
    {
        public string Format { get; set; } = "{0:0.00}";
        public int Digits { get; set; } = 2;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            var _value = System.Convert.ToDouble(value);
            return string.Format(Format, Math.Round(_value, Digits).ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
