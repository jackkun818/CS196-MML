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
using static System.Net.Mime.MediaTypeNames;

namespace crs.game.Games
{
    /// <summary>
    /// Reaction behavior explanation.xaml Interaction logic
    /// </summary>
    public partial class Reaction_behavior_explanation : BaseUserControl
    {
        private DispatcherTimer timer;

        public Action GameBeginAction { get; set; }

        public Func<string, Task> VoicePlayFunc { get; set; }

        public Reaction_behavior_explanation()
        {
            InitializeComponent();
            TipBlock.Text = null;// "Please press the corresponding button after the picture appears on the left according to the prompt.";
            TargetImage.Source = new BitmapImage(new Uri("Response behavior/right.png", UriKind.Relative));
            RandomImage.Source = null;

            // Initialize the timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);  // Timer set to 2 seconds
            timer.Tick += Timer_Tick;  // Bind timer event

            this.Loaded += Reaction_behavior_explanation_Loaded;

            // Capture global key press events, use PreviewKeyDown Capture events earlier
            this.PreviewKeyDown += Window_PreviewKeyDown;

            // Make sure the focus remains in the window
            this.Focusable = true;  // Make sure the window gets focus
            this.Focus();  // Force focus to the current window
        }

        private void Reaction_behavior_explanation_Loaded(object sender, RoutedEventArgs e)
        {
            // Ensure key presses and focus behavior when page loads
            Button_2_Click(null, null);
            this.Focus();  // Make sure the focus is on the window
        }
        private void Window_MouseMove(object sender, MouseEventArgs e)//Bind custom mouse cursor and default mouse cursor
        {

            Point position = e.GetPosition(this);
            Canvas.SetLeft(CustomCursor, position.X - (CustomCursor.Width / 2));
            Canvas.SetTop(CustomCursor, position.Y - (CustomCursor.Height / 2));
            // Get Canvas The boundary of
            double canvasLeft = Canvas.GetLeft(CustomCursor);
            double canvasTop = Canvas.GetTop(CustomCursor);
            double canvasWidth = this.ActualWidth;
            double canvasHeight = this.ActualHeight;

            // Get CustomCursor width and height
            double cursorWidth = CustomCursor.Width;
            double cursorHeight = CustomCursor.Height;

            // if CustomCursor More than Canvas boundaries, cut
            if (canvasLeft + cursorWidth > canvasWidth)
            {
                Canvas.SetLeft(CustomCursor, canvasWidth - cursorWidth); // Limit to the right boundary
            }
            if (canvasTop + cursorHeight > canvasHeight)
            {
                Canvas.SetTop(CustomCursor, canvasHeight - cursorHeight); // Limit to the lower boundary
            }
            if (canvasLeft < 0)
            {
                Canvas.SetLeft(CustomCursor, 0); // Limit to the left border
            }
            if (canvasTop < 0)
            {
                Canvas.SetTop(CustomCursor, 0); // Limit to the upper boundary
            }

            // if CustomCursor Exceeded Canvas Range, cut out the excess
            Rect clipRect = new Rect(0, 0, canvasWidth, canvasHeight);
            CustomCursor.Clip = new RectangleGeometry(clipRect);  // Crop areas using rectangles
        }
        private void OnGameStart()//Call custom mouse cursor function
        {
            this.Cursor = Cursors.None; // Hide the default cursor
            CustomCursor.Visibility = Visibility.Visible; // Show custom cursor
            MouseMove += Window_MouseMove; // subscription MouseMove event
            CustomCursor.Width = 65; // Adjust to the width you want
            CustomCursor.Height = 65; // Adjust to the height you want
        }
        private void Timer_Tick(object? sender, EventArgs e)
        {
            // Random picture is displayed when the timer is triggered
            RandomImage.Source = new BitmapImage(new Uri("Response behavior/right.png", UriKind.Relative));

            // Stop the timer and wait for the user to enter the key
            timer.Stop();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Capture all key events, Prevent other controls from getting focus
            e.Handled = true;  // Process events to prevent the system from further processing key events

            // The user presses the right click and the picture is displayed
            if (e.Key == Key.Right && RandomImage.Source != null)
            {
                TipBlock.FontSize = 40;
                TipBlock.Text = "  Congratulations on getting right！";
                TipBlock.Foreground = new SolidColorBrush(Colors.Green);
                OkButton.Visibility = Visibility.Visible;

                // Stop the timer
                timer.Stop();
            }
            else if (e.Key != Key.Right && RandomImage.Source != null)
            {
                // User answered wrongly
                TipBlock.FontSize = 40;
                TipBlock.Text = "  Sorry to answer wrong！";
                TipBlock.Foreground = new SolidColorBrush(Colors.Red);
                OkButton.Visibility = Visibility.Collapsed;

                // Stop and restart the timer and redisplay the picture
                timer.Stop();
                RandomImage.Source = null;
                timer.Start();
            }

            // Force focus to remain in the window to prevent keys from losing focus
            this.Focus();
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

                        // Hide part of the trial
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;
                        TargetImage.Visibility = Visibility.Collapsed;
                        TipBlock.Visibility = Visibility.Collapsed;
                        RandomImage.Visibility = Visibility.Collapsed;
                        fuhaotext.Visibility = Visibility.Collapsed;
                        KeyPromptText.Visibility = Visibility.Collapsed;

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

                        // The second explanation interface
                        Text_2.Visibility = Visibility.Visible;
                        Image_2.Visibility = Visibility.Visible;
                        Image_3.Visibility = Visibility.Visible;

                        // Hide the controls for the trial play section
                        TargetImage.Visibility = Visibility.Collapsed;
                        TipBlock.Visibility = Visibility.Collapsed;
                        RandomImage.Visibility = Visibility.Collapsed;
                        fuhaotext.Visibility = Visibility.Collapsed;
                        KeyPromptText.Visibility = Visibility.Collapsed;

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
                        TargetImage.Visibility = Visibility.Visible;
                        TipBlock.Visibility = Visibility.Visible;
                        RandomImage.Visibility = Visibility.Visible;
                        fuhaotext.Visibility = Visibility.Visible;
                        KeyPromptText.Visibility = Visibility.Visible;

                        // Hide the button for the explanation section
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Collapsed;

                        // Force focus to remain in the window
                        this.Focus();
                        //LJN, instructing the delegate in the explanation module
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("Please remember and be familiar with the keys corresponding to the logo on the screen, and press the corresponding keys on the keyboard after the logo appears.");//Add code, call function, display the text under the digital person
                        //LJN
                        StartTrial();
                    }
                    break;
            }
        }

        private void StartTrial()
        {
            this.Focus();  // Force focus to be kept on the window

            // Start the timer
            timer.Start();

            // Initialization prompt
            RandomImage.Source = null;
            TipBlock.Text = null;// "Please press the corresponding button according to the prompt";
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
