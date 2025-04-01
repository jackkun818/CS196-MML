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

namespace crs.extension.Controls.MoCAs
{
    /// <summary>
    /// Subject_11.xaml Interaction logic
    /// </summary>
    public partial class Subject_11 : UserControl
    {
        public Subject_11()
        {
            InitializeComponent();
        }

        #region Voice playback function
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
