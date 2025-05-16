using HandyControl.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;

namespace crs.extension
{
    public class Crs_MessageBox
    {
        public static MessageBoxResult Show(string message, string caption = "提示", MessageBoxButton button = MessageBoxButton.OK)
        {
            return MessageBox.Show(new MessageBoxInfo
            {
                Message = message,
                Caption = caption,
                Button = button,
                IconBrushKey = ResourceToken.AccentBrush,
                IconKey = ResourceToken.AskGeometry,
                StyleKey = "MessageBoxCustom"
            });
        }
    }
}
