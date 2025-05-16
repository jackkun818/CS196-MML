using crs.extension;
using System.Windows.Controls;
using System.Windows.Interop;
using crs.theme.Extensions;

namespace crs.dialog.Views
{
    /// <summary>
    /// Interaction logic for PatientEdit
    /// </summary>
    public partial class PatientEdit : UserControl
    {
        public PatientEdit()
        {
            InitializeComponent();
        }

        private async void SimplePanel_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("非交互区域，请尝试点击返回按钮");
        }
    }
}
