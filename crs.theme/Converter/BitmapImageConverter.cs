using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace crs.theme.Converter
{
    public class BitmapImageConverter : MarkupExtension, IValueConverter
    {
        public BitmapImage DefaultBitmapImage { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            var resource = Application.Current.Resources[$"crs_{value?.ToString()}"] as BitmapImage;
            if (resource == null && DefaultBitmapImage != null)
            {
                resource = DefaultBitmapImage;
            }
            return resource;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
