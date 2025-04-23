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
    /// Explanation of vocabulary memory ability.xaml Interaction logic
    /// </summary>
    public partial class Explanation_of_word_memory_ability : BaseUserControl
    {
        private int gametime;
        private bool istrue;
        private DispatcherTimer gameTimer;
        private DispatcherTimer timer;

        public Action GameBeginAction { get; set; }

        public Func<string, Task> VoicePlayFunc { get; set; }

        public Explanation_of_word_memory_ability()
        {
            InitializeComponent();
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(4);
            gameTimer.Tick += GameTimer_Tick;
            this.Loaded += Explanation_of_vocabulary_memory_ability_Loaded;                       
        }

        private void Explanation_of_vocabulary_memory_ability_Loaded(object sender, RoutedEventArgs e)
        {
            // Ensure key presses and focus behavior when page loads
            Button_2_Click(null, null);
            this.Focus();  // Make sure the focus is on the window
        }

        private void startgame()
        {
            FeedbackImage.Visibility = Visibility.Collapsed;
            istrue = true;
            TipBlock.Text = null;
            TipBlock.Foreground = new SolidColorBrush(Colors.Black);
            WordBlock.Foreground = new SolidColorBrush(Colors.Black);
            WordBlock.Text = "Please find a word that appears repeatedly!";
            gameTimer.Start();
            WordBlock.FontSize=50;

        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            FeedbackImage.Visibility = Visibility.Collapsed;
            gameTimer.Stop();
            gametime = 0;
            TipBlock.Text = null;// 
            WordBlock.FontSize = 100;
            anjian1.Visibility = Visibility.Visible;
            anjian2.Visibility = Visibility.Visible;
            TipBlock.Foreground = new SolidColorBrush(Colors.Black);
            WordBlock.Foreground = new SolidColorBrush(Colors.Black);
            WordBlock.Text = "apple fruit";
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop(); // Stop the timer
            startgame();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            gametime++;
            if(gametime==1)
            {
                istrue=false;
                WordBlock.Text = "orange child";
            }
            if(gametime==2)
            {
                istrue = false;
                WordBlock.Text = "apple fruit";
            }
            if (gametime==3 && !istrue)
            {
                
                FeedbackImage.Source = new BitmapImage(new Uri("./pic/WOMT/error.png", UriKind.Relative)); //Change place: Change the wrong answer to the picture to display, set the wrong picture,Bundle"Sorry to answer wrong"Replace
                FeedbackImage.Visibility = Visibility.Visible;
                OkButton.Visibility = Visibility.Collapsed;
                anjian1.Visibility=Visibility.Collapsed;
                anjian2.Visibility=Visibility.Collapsed;
               
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(3); // Set to 3 seconds
                timer.Tick += Timer_Tick; // subscription Tick event
                
                timer.Start();
            }
            else if(gametime == 3 && istrue)
            {
                FeedbackImage.Source = new BitmapImage(new Uri("./pic/WOMT/right.png", UriKind.Relative)); //Change the place: Change the correct answer to the picture to display, set the correct picture,Bundle"Congratulations on getting right"Replace
                FeedbackImage.Visibility = Visibility.Visible;
                OkButton.Visibility = Visibility.Visible;
                anjian1.Visibility = Visibility.Collapsed;
                anjian2.Visibility = Visibility.Collapsed;
            }
        }

        //Change place: Add function, the left and right keys in the binding direction correspond to whether the buttons. Pressing the left and right is equivalent to clicking the buttons
        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Check whether the pressed key is a left or right direction key
            if (e.Key == Key.Left)
            {
                // simulation"yes"Click the button, call Button_Click method
                Button_Click(anjian1, null);  // Simulation click"yes"Button
            }
            else if (e.Key == Key.Right)
            {
                // simulation"no"Click the button, call Button_Click_1 method
                Button_Click_1(anjian2, null);  // Simulation click"no"Button
            }

            // Other key processing（like Enter key）
            if (e.Key == Key.Enter)
            {
                // Can handle other key logic, such as pressing Enter key
            }
        }
        //End of change



        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            gametime++;
            if (gametime == 1)
            {
                WordBlock.Text = "orange child";
            }
            if (gametime == 2)
            {
                WordBlock.Text = "apple fruit";
            }
            if (gametime == 3)
            {
                istrue=false;
            }
            if (gametime == 3 && istrue)
            {
                FeedbackImage.Source = new BitmapImage(new Uri("./pic/WOMT/right.png", UriKind.Relative)); //Change the place: Change the correct answer to the picture to display, set the correct picture,Bundle"Congratulations on getting right"Replace
                FeedbackImage.Visibility = Visibility.Visible;
                OkButton.Visibility = Visibility.Visible;
                anjian1.Visibility = Visibility.Collapsed;
                anjian2.Visibility = Visibility.Collapsed;
            }
            else if (gametime == 3 && !istrue)
            {
                FeedbackImage.Source = new BitmapImage(new Uri("./pic/WOMT/error.png", UriKind.Relative)); //Change place: Change the wrong answer to the picture to display, set the wrong picture,Bundle"Sorry to answer wrong"Replace
                FeedbackImage.Visibility = Visibility.Visible;
                OkButton.Visibility = Visibility.Collapsed;
                anjian1.Visibility = Visibility.Collapsed;
                anjian2.Visibility = Visibility.Collapsed;
               
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(3); // Set to 3 seconds
                timer.Tick += Timer_Tick; // subscription Tick event
               
                timer.Start();
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

                        Text_2.Visibility = Visibility.Collapsed;


                        // Hide part of the trial
                        WordBlock.Visibility = Visibility.Collapsed;
                        anjian1.Visibility = Visibility.Collapsed;
                        anjian2.Visibility = Visibility.Collapsed;
                        TipBlock.Visibility = Visibility.Collapsed;
                        FeedbackImage.Visibility = Visibility.Collapsed;
                        Button_1.IsEnabled = false;
                        Button_2.Content = "Trial";

                        await OnVoicePlayAsync(Text_1.Text);
                    }
                    break;
                case 1:
                    {
                        // Enter the trial interface
                        Text_1.Visibility = Visibility.Collapsed;

                        Text_2.Visibility = Visibility.Collapsed;


                        // Controls showing the trial part
                        WordBlock.Visibility = Visibility.Visible;
                        anjian1.Visibility = Visibility.Collapsed;
                        anjian2.Visibility = Visibility.Collapsed;
                        TipBlock.Visibility = Visibility.Visible;
                        FeedbackImage.Visibility = Visibility.Visible;
                        // Hide the button for the explanation section
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Collapsed;
                        //LJN, instructing the delegate in the explanation module
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("There will be some words repeated on the screen,Please find the repeated words. When the repeated words appear, please click on the screen"yes"Button, otherwise click"no"Button");//Add code, call function, display the text under the digital person
                        
                        //LJN
                        startgame();

                        this.Focus();
                       
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
