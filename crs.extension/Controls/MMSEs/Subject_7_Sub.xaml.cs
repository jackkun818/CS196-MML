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

namespace crs.extension.Controls.MMSEs
{
    /// <summary>
    /// Subject_7_Sub.xaml Interaction logic
    /// </summary>
    public partial class Subject_7_Sub : UserControl
    {
        static ConcurrentDictionary<int, string> voiceDict = new ConcurrentDictionary<int, string>
        {
            [0] = "I want to read a sentence below, please repeat it clearly. This sentence is: Forty-four stone lions.",
        };

        public Subject_7_Sub()
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
            // Play voice prompts, start timer operations, etc.

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
