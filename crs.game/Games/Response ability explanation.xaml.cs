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
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskBand;

namespace crs.game.Games
{
    /// <summary>
    /// Response ability explanation.xaml Interaction logic
    /// </summary>
    public partial class Response_ability_explanation : BaseUserControl
    {
        private DispatcherTimer timer;

        public Action GameBeginAction { get; set; }

        public Func<string, Task> VoicePlayFunc { get; set; }

        public Response_ability_explanation()
        {
            InitializeComponent();

            TipBlock.Text = null;// "Please press the corresponding button after the picture appears on the left according to the prompt.";
            TargetImage.Source = new BitmapImage(new Uri("Response ability/1.png", UriKind.Relative));
            RandomImage.Source = null;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += Timer_Tick;

            this.Loaded += Response_ability_explanation_Loaded;


        }

        private void Response_ability_explanation_Loaded(object sender, RoutedEventArgs e)
        {
            // Ensure key presses and focus behavior when page loads
            Button_2_Click(null, null);
            this.Focus();  // Make sure the focus is on the window
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            timer.Stop();
            RandomImage.Source = new BitmapImage(new Uri("Response ability/1.png", UriKind.Relative));
        }
        private void Window_MouseMove(object sender, MouseEventArgs e)//Bind custom mouse cursor and default mouse cursor
        {

            Point position = e.GetPosition(this);
            Canvas.SetLeft(CustomCursor, position.X - (CustomCursor.Width / 2));
            Canvas.SetTop(CustomCursor, position.Y - (CustomCursor.Height / 2));
        }
        private void OnGameStart()//Call custom mouse cursor function
        {
            this.Cursor = Cursors.None; // Hide the default cursor
            CustomCursor.Visibility = Visibility.Visible; // Show custom cursor
            MouseMove += Window_MouseMove; // subscription MouseMove event
            CustomCursor.Width = 65; // Adjust to the width you want
            CustomCursor.Height = 65; // Adjust to the height you want
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right && RandomImage.Source != null)
            {
                e.Handled = true; // Prevent other controls from processing this event
                TipBlock.FontSize = 40;
                TipBlock.Text = "  Congratulations on getting right！";
                TipBlock.Foreground = new SolidColorBrush(Colors.Green);
                OkButton.Visibility = Visibility.Visible;
            }
            else
            {
                e.Handled = true; // Prevent other controls from processing this event
                TipBlock.FontSize = 40;
                TipBlock.Text = "  Sorry to answer wrong！";
                TipBlock.Foreground = new SolidColorBrush(Colors.Red);
                OkButton.Visibility = Visibility.Collapsed;
                timer?.Stop();
                timer?.Start();
                RandomImage.Source = null;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Related logic for starting answering questions
            OnGameBegin();
        }

        int currentPage = -1;

        private void Button_1_Click(object sender, RoutedEventArgs e)
        {
            currentPage--;
            PageSwitch();
          
        }

        private void Button_2_Click(object sender, RoutedEventArgs e)
        {
            currentPage++;
            PageSwitch();
          
        }

        private void Button_3_Click(object sender, RoutedEventArgs e)
        {
            OnGameBegin();
        
        }

        async void PageSwitch()
        {
            switch (currentPage)
            {
                case 0:
                    {
                        // Display the first interface of explanation
                        Text_1.Visibility = Visibility.Visible;
                        Image_1.Visibility = Visibility.Visible;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;


                        // Hide part of the trial
                        TipBlock.Visibility = Visibility.Collapsed;
                        TargetImage.Visibility = Visibility.Collapsed;
                        RandomImage.Visibility = Visibility.Collapsed;
                        TipBlock1.Visibility = Visibility.Collapsed;
                        fuhao.Visibility = Visibility.Collapsed;
                       
                        Button_1.IsEnabled = false;
                        Button_2.Content = "Next step";
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Margin = new Thickness(329, 850, 0, 0);
                        Button_3.Margin = new Thickness(770, 850, 0, 0);
                        OnGameStart();
                        await OnVoicePlayAsync(Text_1.Text);
                    }
                    break;
                case 1:
                    {
                        // Display the second interface of the explanation
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Visible;
                        Image_2.Visibility = Visibility.Visible;
                        Image_3.Visibility = Visibility.Visible;




                        // Hide the controls for the trial play section
                        TipBlock.Visibility = Visibility.Collapsed;
                        TargetImage.Visibility = Visibility.Collapsed;
                        RandomImage.Visibility = Visibility.Collapsed;
                        TipBlock1.Visibility = Visibility.Collapsed;
                        fuhao.Visibility = Visibility.Collapsed;
                        Button_1.IsEnabled = true;
                        Button_2.Content = "Trial";
                        Button_1.Visibility = Visibility.Visible;
                        Button_2.Margin = new Thickness(550, 850, 0, 0);
                        Button_3.Margin = new Thickness(911, 850, 0, 0);

                        await OnVoicePlayAsync(Text_2.Text);
                    }
                    break;
                case 2:
                    {
                        // Enter the trial interface
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;


                        // Controls showing the trial part
                        TipBlock.Visibility = Visibility.Visible;
                        TargetImage.Visibility = Visibility.Visible;
                        RandomImage.Visibility = Visibility.Visible;
                        TipBlock1.Visibility = Visibility.Visible;
                        fuhao.Visibility = Visibility.Visible;
                        // Hide the button for the explanation section
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Collapsed;
                        timer.Start();
                        // Force focus to remain in the window
                        this.Focusable = true;
                        this.Focus();  // Focus on UserControl superior
                        Keyboard.Focus(this);  // Capture keyboard focus now
                        this.KeyDown += Window_KeyDown;  // Make sure the key event is bound

                        //LJN, instructing the delegate in the explanation module
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("Please remember and be familiar with the keys corresponding to the four logos on the screen, and press the corresponding keys on the keyboard after the logo appears.");//Add code, call function, display the text under the digital person
                        //LJN
                    }
                    break;
            }
        }


        /// <summary>
        /// Voice playback of explanation content
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        async Task VoicePlayer(string message)
        {
            var voicePlayFunc = VoicePlayFunc;
            if (voicePlayFunc == null)
            {
                return;
            }

            await voicePlayFunc.Invoke(message);
        }
    }
}
