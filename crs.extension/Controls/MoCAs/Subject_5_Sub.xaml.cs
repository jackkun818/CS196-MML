using crs.extension.Models;
using HandyControl.Controls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace crs.extension.Controls.MoCAs
{
    /// <summary>
    /// Subject_5_Sub.xaml 的交互逻辑
    /// </summary>
    public partial class Subject_5_Sub : UserControl
    {
        static ConcurrentDictionary<int, string> voiceDict = new ConcurrentDictionary<int, string>
        {
            [0] = "下面我要读出一系列数字，请注意听，每当我读到1的时候，您就拍一下手，当我读其他数字的时候不要按：五。二。一。三。九。四。一。一。八。零。六。二。一。五。一。九。四。五。一。一。一。四。一。九。零。五。一。一。二",
        };

        public Subject_5_Sub()
        {
            InitializeComponent();
            this.Loaded += Subject_Sub_Loaded;
            this.Unloaded += Subject_Sub_Unloaded;
        }

        private void Subject_Sub_Unloaded(object sender, RoutedEventArgs e)
        {
            Crs_WaveOutToolkit.AudioCancel();
        }

        private async void Subject_Sub_Loaded(object sender, RoutedEventArgs e)
        {            
            // 播放语音提示、启动定时器操作等

            Crs_WaveOutToolkit.AudioCancel();
            _ = Crs_WaveInToolkit.RecordAudioCompleteAsync(true);

            if (voiceDict.TryGetValue(0, out var voiceText))
            {
                var result = await Crs_WaveOutToolkit.TextToSpeechAsync(voiceText);
                if (result.status)
                {
                    Crs_WaveOutToolkit.AudioPlay(result.voiceBuffer);
                }
            }
        }
    }
}
