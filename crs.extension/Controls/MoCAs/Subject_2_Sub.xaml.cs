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
    /// Subject_2_Sub.xaml Interaction logic
    /// </summary>
    public partial class Subject_2_Sub : UserControl
    {
        static ConcurrentDictionary<int, string> voiceDict = new ConcurrentDictionary<int, string>
        {
            [0] = "Please tell me the name of this animal",
            [1] = "Please tell me the name of this animal again",
            [2] = "Please tell me the name of this animal again",
        };

        public Subject_2_Sub()
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
            // Play voice prompts, start timer operations, etc.

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
