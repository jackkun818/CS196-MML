using crs.core.Services;
using crs.extension.Models;
using HandyControl.Controls;
using HarfBuzzSharp;
using NAudio.Wave;
using System;
using System.Collections.Concurrent;
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
using static crs.core.Services.Crs_TtsService;
using static crs.extension.Crs_Interface;

namespace crs.extension.Controls.MoCAs
{
    /// <summary>
    /// Subject_1_Sub.xaml Interaction logic
    /// </summary>
    public partial class Subject_1_Sub : UserControl
    {
        static ConcurrentDictionary<int, string> voiceDict = new ConcurrentDictionary<int, string>
        {
            [0] = "Please alternate numbers and text in order",
            [1] = "Please imitate and draw cubes in the blank space according to the following figure",
            [2] = "Please draw a clock in the blank space, fill in all the numbers and point out 11:10",
        };

        public Subject_1_Sub()
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
