using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crs.theme.Extensions
{
    public enum MessageBoxButton
    {
        OK,
        Cancel,
        OKOrCancel,
        CustomReport
    }

    public interface IMessageBox
    {
        MessageBoxButton Button { get; set; }
        string Message { get; set; }
    }
}
