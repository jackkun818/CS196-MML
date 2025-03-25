using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace crs.extension.Converter
{
    public class Boolean2VisibilityConverter : MarkupExtension, IValueConverter
    {
        public bool IsReversal { get; set; } = false;
        public Visibility FalseVisibilityType { get; set; } = Visibility.Collapsed;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibility = Visibility.Visible;
            if (value == null)
            {
                return visibility;
            }

            var @bool = System.Convert.ToBoolean(@value);
            switch (@bool, IsReversal)
            {
                case (true, true):
                    visibility = FalseVisibilityType;
                    break;
                case (true, false):
                    visibility = Visibility.Visible;
                    break;
                case (false, true):
                    visibility = Visibility.Visible;
                    break;
                case (false, false):
                    visibility = FalseVisibilityType;
                    break;
            }
            return visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
