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
    public class Bitmap2ImageSourceConverter : MarkupExtension, IValueConverter
    {
        private Bitmap image = null;
        public string DefaultImagePath { get; set; }
        public int? DecodePixelWidth { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var defaultImagePath = DefaultImagePath;
                if (image == null && !string.IsNullOrWhiteSpace(defaultImagePath) && File.Exists(defaultImagePath))
                {
                    image = new Bitmap(defaultImagePath);
                }

                var bitmap = value as Bitmap;
                bitmap ??= image;
                if (bitmap == null)
                {
                    return null;
                }

                using var memoryStream = new MemoryStream();
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

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
                Debug.WriteLine($"Bitmap2ImageSourceConverter.Convert.Error->{ex.ToString()}");
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
