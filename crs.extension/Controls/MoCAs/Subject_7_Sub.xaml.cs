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
    /// Subject_7_Sub.xaml 的交互逻辑
    /// </summary>
    public partial class Subject_7_Sub : UserControl
    {
        static ConcurrentDictionary<int, string> voiceDict = new ConcurrentDictionary<int, string>
        {
            [0] = "下面我要读出一句话，请清楚地重复一遍。这句话是：我只知道今天李明是帮过忙的人",
            [1] = "接下来我再读出一句话，请清楚地重复一遍。这句话是：当狗在房间的时候，猫总是藏在沙发下",
        };

        public Subject_7_Sub()
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
