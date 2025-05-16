using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace crs.extension.Controls.MMSEs
{
    /// <summary>
    /// Subject_8.xaml 的交互逻辑
    /// </summary>
    public partial class Subject_8 : UserControl
    {
        public Subject_8()
        {
            InitializeComponent();
        }

        #region 播放语音功能

        async private void ExecuteAudioPlayClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string data = button.Tag as string;

            if (data != null)
            {
                var result = await Crs_WaveOutToolkit.TextToSpeechAsync(data);
                if (result.status)
                {
                    Crs_WaveOutToolkit.AudioPlay(result.voiceBuffer);
                }
            }
        }
        #endregion
    }
}
