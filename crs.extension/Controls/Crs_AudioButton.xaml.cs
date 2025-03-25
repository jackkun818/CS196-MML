using crs.extension.Models;
using System;
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

namespace crs.extension.Controls
{
    /// <summary>
    /// Crs_AudioButton.xaml 的交互逻辑
    /// </summary>
    public partial class Crs_AudioButton : UserControl
    {
        public Crs_AudioButton()
        {
            InitializeComponent();
            this.Loaded += Crs_AudioButton_Loaded;
            this.Unloaded += Crs_AudioButton_Unloaded;
        }

        private void Crs_AudioButton_Unloaded(object sender, RoutedEventArgs e)
        {
            _ = Crs_WaveInToolkit.RecordAudioCompleteAsync(true);
        }

        private void Crs_AudioButton_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is SubjectChildrenItem childrenItem)
            {
                childrenItem.IsUseAudio = true;
            }
        }

        private async void AnswerButton_Click(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as ToggleButton;
            if (toggleButton.DataContext is SubjectChildrenItem childrenItem)
            {
                this.RaiseEvent(new RoutedEventArgs(ClickEvent, toggleButton));

                if ((bool)toggleButton.IsChecked)
                {
                    childrenItem.AudioData = null;
                    var result = await Crs_WaveInToolkit.RecordAudioStartAsync();
                    if (result.status)
                    {
                        childrenItem.AudioData = result.voiceBuffer;
                    }
                    toggleButton.IsChecked = false;
                    return;
                }

                await Crs_WaveInToolkit.RecordAudioCompleteAsync();
                return;
            }
        }


        public object AudioData
        {
            get { return (object)GetValue(AudioDataProperty); }
            set { SetValue(AudioDataProperty, value); }
        }

        public static readonly DependencyProperty AudioDataProperty =
            DependencyProperty.Register("AudioData", typeof(object), typeof(Crs_AudioButton), new PropertyMetadata(null));



        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Crs_AudioButton));

        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }
    }
}
