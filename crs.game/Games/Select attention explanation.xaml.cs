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

namespace crs.game.Games
{
    /// <summary>
    /// Select attention explanation.xaml Interaction logic
    /// </summary>
    public partial class Select_attention_explanation : BaseUserControl
    {
        private DispatcherTimer waitTimer;
        private DispatcherTimer timer;
        private bool istrue;
        private int current;//1 should be pressed, 2 should not be pressed

        public Action GameBeginAction { get; set; }

        public Func<string, Task> VoicePlayFunc { get; set; }

        public Select_attention_explanation()
        {
            InitializeComponent();

            current = 1;
            istrue = true;
            

            this.Loaded += Select_attention_explanation_Loaded;


        }

        private void Select_attention_explanation_Loaded(object sender, RoutedEventArgs e)
        {
            // Ensure key presses and focus behavior when page loads
            Button_2_Click(null, null);
           
        }

        private void ShowImage()
        {
            RandomImage.Visibility = Visibility.Hidden;
            FeedbackImage.Visibility = Visibility.Collapsed;

            if (current == 1)
            {
                RandomImage.Source = new BitmapImage(new Uri("GONT1.png", UriKind.Relative));
                RandomImage.Width = 270; // Enlarged width
                RandomImage.Height = 270; // Amplified height
                RandomImage.Margin = new Thickness(0, 50, 0, 0); // Move down by 50 pixels
            }
            else
            {
                RandomImage.Source = new BitmapImage(new Uri("GONT2.png", UriKind.Relative));
                RandomImage.Width = 270; // Enlarged width
                RandomImage.Height = 270; // Amplified height
                RandomImage.Margin = new Thickness(0, 50, 0, 0); // Move down by 50 pixels

            }

            // If the timer already exists, stop and reset
            waitTimer?.Stop();

            // Display picture timer
            waitTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3) // Set the waiting time to 3 Second
            };

            waitTimer.Tick += (s, args) =>
            {
                waitTimer.Stop(); // Stop the timer
                RandomImage.Visibility = Visibility.Visible;

                // Waiting for answering timer
                timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(3) // Set the waiting time to 3 Second
                };

                timer.Tick += (s, args) =>
                {
                    timer.Stop(); // Stop the timer
                    if (current == 1)
                    {
                        istrue = false;
                        current = 2;
                        ShowImage();
                    }
                    else
                    {
                        if (istrue == true)
                        {
                            FeedbackImage.Source = new BitmapImage(new Uri("./pic/GONT/right.png", UriKind.Relative)); //Change the place: Change the correct answer to the picture to display, set the correct picture
                            FeedbackImage.Visibility = Visibility.Visible;
                            OkButton.Visibility = Visibility.Visible;
                            queren.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            FeedbackImage.Source = new BitmapImage(new Uri("./pic/GONT/error.png", UriKind.Relative)); //Change the place: Change the correct answer to the picture to display, set the correct picture
                            FeedbackImage.Visibility = Visibility.Visible;
                            OkButton.Visibility = Visibility.Collapsed;
                            

                            // Timer for wrong answer
                            DispatcherTimer delayTimer = new DispatcherTimer
                            {
                                Interval = TimeSpan.FromSeconds(3) // Set the waiting time to 3 Second
                            };

                            delayTimer.Tick += (s, args) =>
                            {
                                delayTimer.Stop(); // Stop the delay timer
                                current = 1;
                                istrue = true;
                                ShowImage();
                            };

                            delayTimer.Start(); // Start the delay timer
                        }
                    }

                };

                timer.Start(); // Start the timer
            };

            waitTimer.Start(); // Start the timer
        }

        //Add code to bind the keyboardenterKeys and"confirm"Button
        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Check whether the key you pressed is the key you specified
            if (e.Key == System.Windows.Input.Key.Enter) // Check whether the Enter key is pressed
            {
                // Simulate button click, call Button_Click The method is to define the click event of the confirm button
                Button_Click(null, null);
            }
        }
        //End of change

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            timer?.Stop();
            if (current == 1)
            {
                current = 2;
                ShowImage();
            }
            else
            {
                waitTimer?.Stop(); // Stop the timer
                FeedbackImage.Source = new BitmapImage(new Uri("./pic/GONT/error.png", UriKind.Relative)); //Change the place: Change the correct answer to the picture to display, set the correct picture
                FeedbackImage.Visibility = Visibility.Visible;
                OkButton.Visibility = Visibility.Collapsed;
                

                // Wait for three seconds
                await Task.Delay(3000);

                // Call ShowImage method
                current = 1;
                istrue = true;
                ShowImage();
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
            if (currentPage == 2)
            {
                // If the current is the last page of the explanation, click"Trial"Then enter the trial interface
                currentPage = 3;
                PageSwitch(); // Switch to the trial interface and execute ShowImage
            }
            else
            {
                // Otherwise, go to the next page to explain
                currentPage++;
                PageSwitch();
            }
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
                        Text_3.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;

                        // Hide part of the trial
                        FeedbackImage.Visibility = Visibility.Collapsed;
                        TargetImage.Visibility = Visibility.Collapsed;
                        TipBlock1.Visibility = Visibility.Collapsed;
                        queren.Visibility = Visibility.Collapsed;
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Content = "Next step";
                        Button_2.Margin = new Thickness(329, 850, 0, 0);
                        Button_3.Margin = new Thickness(770, 850, 0, 0);
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
                        Text_3.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;

                        // Hide the controls for the trial play section
                        FeedbackImage.Visibility = Visibility.Collapsed;
                        TargetImage.Visibility = Visibility.Collapsed;
                        TipBlock1.Visibility = Visibility.Collapsed;
                        queren.Visibility = Visibility.Collapsed;
                        Button_1.IsEnabled = true;
                        Button_2.Content = "Next step";
                        Button_2.Margin = new Thickness(550, 850, 0, 0);
                        Button_3.Margin = new Thickness(911, 850, 0, 0);
                        Button_1.Visibility = Visibility.Visible;
                        await OnVoicePlayAsync(Text_2.Text);
                    }
                    break;
                case 2:
                    {
                        // Show the third interface of explanation
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Text_3.Visibility = Visibility.Visible;
                        Image_3.Visibility = Visibility.Visible;

                        // Hide the controls for the trial play section
                        FeedbackImage.Visibility = Visibility.Collapsed;
                        TargetImage.Visibility = Visibility.Collapsed;
                        TipBlock1.Visibility = Visibility.Collapsed;
                        queren.Visibility = Visibility.Collapsed;
                        Button_1.IsEnabled = true;
                        Button_2.Content = "Trial";

                        await OnVoicePlayAsync(Text_3.Text);
                    }
                    break;
                case 3:
                    {
                        // Enter the trial interface to display the controls in the trial part
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Text_3.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;

                        // Controls showing the trial part
                        FeedbackImage.Visibility = Visibility.Visible;
                        TargetImage.Visibility = Visibility.Visible;
                        TipBlock1.Visibility = Visibility.Visible;
                        queren.Visibility = Visibility.Visible;

                        // Hide the button for the explanation section
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Collapsed;

                        // Force focus to remain in the window
                        this.Focus();

                        // Start calling immediately when entering the trial interface ShowImage(), start the trial
                        ShowImage();

                        //LJN, instructing the delegate in the explanation module
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("There will be a target object on the right side of the screen. When the target object appears on the left side of the screen, press theOKno response is required in the rest of the situation.");//Add code, call function, display the text under the digital person
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
