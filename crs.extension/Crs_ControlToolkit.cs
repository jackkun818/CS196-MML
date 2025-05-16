using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace crs.extension
{
    public static class Crs_ControlToolkit
    {
        public static MemoryStream SaveControlAsStream(FrameworkElement control)
        {
            var width = control.ActualWidth;
            var height = control.ActualHeight;

            var renderTarget = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Default);
            renderTarget.Render(control);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderTarget));

            var memoryStream = new MemoryStream();
            encoder.Save(memoryStream);

            return memoryStream;
        }

        public static Bitmap SaveControlAsBitmap(FrameworkElement control)
        {
            using var stream = SaveControlAsStream(control);

            var bitmap = new Bitmap(stream);
            return bitmap;
        }
    }
}
