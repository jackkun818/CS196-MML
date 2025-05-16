using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using System.Windows.Markup;
using System.Diagnostics;

namespace crs.extension.Converter
{
    public class BitmapStream2ImageSourceConverter : IValueConverter
    {
        public int? DecodePixelWidth { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var bitmapData = value as byte[];
                if (bitmapData == null)
                {
                    return null;
                }

                using var memoryStream = new MemoryStream(bitmapData);

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;

                var decodePixelWidth = DecodePixelWidth;
                if (decodePixelWidth != null)
                {
                    bitmapImage.DecodePixelWidth = decodePixelWidth.Value;
                }

                bitmapImage.EndInit();

                if (bitmapImage.CanFreeze)
                {
                    bitmapImage.Freeze();
                }
                return bitmapImage;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BitmapStream2ImageSourceConverter.Convert.Error->{ex.ToString()}");
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
