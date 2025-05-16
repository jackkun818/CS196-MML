using crs.extension.Models;
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
    /// Subject_9_Sub.xaml 的交互逻辑
    /// </summary>
    public partial class Subject_9_Sub : UserControl
    {
        static ConcurrentDictionary<int, string> voiceDict = new ConcurrentDictionary<int, string>
        {
            [0] = "下面我说出两个物体，请您说出这两个物体的相似性。，，火车和自行车的相似之处在哪",
            [1] = "接下来我再说两个物体，请您说出这两个物体的相似性。，，手表和尺子的相似之处在哪",
        };

        public Subject_9_Sub()
        {
            InitializeComponent();
            this.Loaded += Subject_Sub_Loaded;
            this.Unloaded += Subject_Sub_Unloaded;
        }

        private void Subject_Sub_Unloaded(object sender, RoutedEventArgs e)
        {
            carousel.PageChanged -= Carousel_PageChanged;

            Crs_WaveOutToolkit.AudioCancel();
        }

        private void Subject_Sub_Loaded(object sender, RoutedEventArgs e)
        {
            carousel.PageChanged += Carousel_PageChanged;
        }

        private async void Carousel_PageChanged(object sender, RoutedEventArgs e)
        {
            // 播放语音提示、启动定时器操作等

            Crs_WaveOutToolkit.AudioCancel();
            _ = Crs_WaveInToolkit.RecordAudioCompleteAsync(true);

            if (voiceDict.TryGetValue(carousel.PageIndex, out var voiceText))
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
