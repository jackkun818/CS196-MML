using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThoughtWorks.QRCode.Codec.Data;
using ThoughtWorks.QRCode.Codec;

namespace crs.extension
{
    public static class Crs_QrCodeToolkit
    {
        /// <summary>
        /// Return to QR code picture
        /// </summary>
        public static Bitmap Encode(string text)
        {
            var qrCodeEncoder = new QRCodeEncoder();
            //qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
            //qrCodeEncoder.QRCodeScale = 4;
            //qrCodeEncoder.QRCodeVersion = 29;
            //qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;
            return qrCodeEncoder.Encode(text);
        }

        /// <summary>
        /// Define parameters,Generate QR code
        /// </summary>
        public static void Create(string text, string path)
            => Encode(text).Save(path);

        /// <summary>
        /// Return the string defined by the QR code
        /// </summary>
        public static string Decode(Bitmap image)
        {
            var qrCodeBitmapImage = new QRCodeBitmapImage(image);
            var qrCodeDecoder = new QRCodeDecoder();
            return qrCodeDecoder.decode(qrCodeBitmapImage);
        }

        /// <summary>
        /// Return the string defined by the QR code
        /// </summary>
        public static string Decode(string path)
            => Decode(new Bitmap(path));
    }
}
