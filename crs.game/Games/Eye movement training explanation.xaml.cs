using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Threading;

namespace crs.game.Games
{
    /// <summary>
    /// Eye movement training explanation.xaml Interaction logic
    /// </summary>
    public partial class Eye_movement_training_explanation : BaseUserControl
    {
        private string[] JudgementPath = new string[]
        {
            "SAKA/judgement/right.png",
            "SAKA/judgement/wrong.png"
        };
        private readonly string[][] imagePaths_specific = new string[][]
       {

            new string[]
            {
                "Img4/left/1/1.png",
                "Img4/left/1/2.png",
                "Img4/left/1/5.png",
                "Img4/left/1/6.png",
            },
            new string[] {
                "Img4/right/1/3.png",
                "Img4/right/1/4.png",
                "Img4/right/1/7.png",
                "Img4/right/1/8.png",
            },
            new string[] {
                "Img4/left/2/1.png",
                "Img4/left/2/2.png",
                "Img4/left/2/5.png",
                "Img4/left/2/6.png",
            }
       };
        private string currentDirectory = Directory.GetCurrentDirectory();

        private double INCREASE; // Improve the accuracy of difficulty
        private double DECREASE;  // Reduce the error rate of difficulty
        private int TRAIN_TIME; // Training duration（Unit: seconds）
        private bool IS_RESTRICT_TIME = true; // Whether to enable exercise time
        private bool IS_BEEP = true;
        private int max_time;
        private int train_time;
        private int counter;
        private int hardness;
        private const int MAX_HARDNESS = 34; // Highlights of difficulty
        private Random random;
        private int[] correctAnswers; // Store the number of correct answers for each difficulty
        private int[] wrongAnswers; // Store the number of error answers per difficulty
        private int[] igonreAnswer;
        private DispatcherTimer timer;
        private int remainingTime;
        private int randomNumber = 0; //Variables for judgment in special cases
        private DispatcherTimer trainingTimer; // New timer for training time
        Random random_tg = new Random();
        private double x_ct, y_ct;
        private double x_tg, y_tg;
        private int count = 0;
        private List<bool> boolList = new List<bool>(5);

        public Action GameBeginAction { get; set; }

        public Func<string, Task> VoicePlayFunc { get; set; }

        public Eye_movement_training_explanation()
        {
            InitializeComponent();

            max_time = 60;
            INCREASE = 0; // Increase the threshold for difficulty
            DECREASE = 0;  // Threshold for reducing difficulty
            TRAIN_TIME = 60; // Training duration（Unit: seconds）
            train_time = TRAIN_TIME;
            IS_RESTRICT_TIME = true; // Whether to enable exercise time
            IS_BEEP = true;
            hardness = 1;
            remainingTime = max_time;
            correctAnswers = new int[MAX_HARDNESS + 1];
            wrongAnswers = new int[MAX_HARDNESS + 1];
            igonreAnswer = new int[MAX_HARDNESS + 1];
            string targetDirectory = Path.Combine(currentDirectory, "Img");

            for (int i = 0; i < correctAnswers.Length; i++)
            {
                correctAnswers[i] = 0;
                wrongAnswers[i] = 0;
                igonreAnswer[i] = 0;
            }

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            trainingTimer = new DispatcherTimer();
            trainingTimer.Interval = TimeSpan.FromSeconds(1);
            trainingTimer.Tick += TrainingTimer_Tick;
            trainingTimer.Start(); // Start the training timer
           

            this.Loaded += Eye_movement_training_explanation_Loaded;


        }

        private void Eye_movement_training_explanation_Loaded(object sender, RoutedEventArgs e)
        {
            // Ensure key presses and focus behavior when page loads
            Button_2_Click(null, null);
            this.Focus();  // Make sure the focus is on the window
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            remainingTime--;

            if (remainingTime <= 0)
            {
                timer.Stop();
                igonreAnswer[hardness]++;
                LoadImages();
                remainingTime = max_time;
                timer.Start();
            }
        }

        private void TrainingTimer_Tick(object sender, EventArgs e)
        {
            train_time--; // Countdown to training time

            if (train_time <= 0)
            {
                timer.Stop(); // Stop the main timer
                trainingTimer.Stop(); // Stop training timer
                this.Close(); // Close the current window
            }
        }
        private void Close()
        {
            //throw new NotImplementedException();
        }


        private void LoadImages()
        {
            // Clear the previous pictures
            ImageGrid.Children.Clear();

            double centerX = ImageGrid.ActualWidth / 2;
            double centerY = ImageGrid.ActualHeight / 2;
            Image image_tg = new Image();
            Image image_ct = new Image();


            Random random = new Random();
            randomNumber = random.Next(2);
            if (randomNumber == 0)
            {
                Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);

                int arrayLength = imagePaths_specific[0].Length;
                //HereindexYou need to read a certain range of pictures according to the difficulty
                int index1 = random1.Next(arrayLength);
                Image image = new Image
                {
                    Source = new BitmapImage(new Uri(imagePaths_specific[0][index1], UriKind.Relative)),
                    Stretch = System.Windows.Media.Stretch.Uniform
                };
                ImageGrid.Children.Add(image);

            }
            else if (randomNumber == 1)
            {
                Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);

                int arrayLength = imagePaths_specific[1].Length;
                //HereindexYou need to read a certain range of pictures according to the difficulty
                int index1 = random1.Next(arrayLength);
                Image image = new Image
                {
                    Source = new BitmapImage(new Uri(imagePaths_specific[1][index1], UriKind.Relative)),
                    Stretch = System.Windows.Media.Stretch.Uniform
                };
                ImageGrid.Children.Add(image);
            }



        }







        //Also, there are additional separations here……
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Process left arrow keys
            if (e.Key == Key.Left)
            {
                e.Handled = true; // Prevent other controls from processing this event
                bool isCorrect = randomNumber == 0;
                HandleAnswer(isCorrect);
            }
            // Process the right arrow key
            else if (e.Key == Key.Right)
            {
                e.Handled = true; // Prevent other controls from processing this event
                bool isCorrect = randomNumber == 1;
                HandleAnswer(isCorrect);
            }
        }

        private void HandleAnswer(bool isCorrect)
        {
            if (isCorrect)
            {
                //textblock.Text = "Congratulations on getting right！";
                //textblock.Foreground = new SolidColorBrush(Colors.Green);
                judgement.Source = new BitmapImage(new Uri(JudgementPath[0], UriKind.Relative));
                //textblock.Margin = new Thickness(500,720,0,0);
                OkButton.Visibility = Visibility.Visible;
            }
            else
            {
                if (IS_BEEP)
                    Console.Beep();

                //textblock.Text = "Sorry to answer wrong！";
                //textblock.Foreground = new SolidColorBrush(Colors.Red);
                judgement.Source = new BitmapImage(new Uri(JudgementPath[1], UriKind.Relative));
                //textblock.Margin = new Thickness(500, 720, 0, 0);
                OkButton.Visibility = Visibility.Collapsed;
            }
            LoadImages();
        }




        private void ImageGrid_Loaded(object sender, RoutedEventArgs e)
        {
            LoadImages(); // Called here LoadImages make sure ImageGrid The size of the
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
                        Image_2.Visibility = Visibility.Visible;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;

                        // Hide part of the trial
                        ImageGrid.Visibility = Visibility.Collapsed;
                        //textblock.Visibility = Visibility.Collapsed;

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
                        Image_2.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Visible;
                        Image_3.Visibility = Visibility.Visible;



                        // Hide the controls for the trial play section
                        ImageGrid.Visibility = Visibility.Collapsed;
                        //textblock.Visibility = Visibility.Collapsed;
                        Button_1.Visibility = Visibility.Visible;
                        Button_1.IsEnabled = true;
                        Button_2.Content = "Trial";
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
                        Image_2.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;

                        // Controls showing the trial part
                        ImageGrid.Visibility = Visibility.Visible;
                        //textblock.Visibility = Visibility.Visible;
                        // Hide the button for the explanation section
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Visible;
                        Button_3.Margin = new Thickness(550, 900, 0, 0);
                        this.Focusable = true;
                        this.Focus();  // Focus on UserControl superior
                        Keyboard.Focus(this);  // Capture keyboard focus now

                        // Loading pictures
                        LoadImages();

                        // make sure KeyDown The incident takes effect immediately
                        this.KeyDown += Window_PreviewKeyDown;  // Make sure the key event is bound

                        //LJN, instructing the delegate in the explanation module
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("20241114 to be supplemented");//Add code, call function, display the text under the digital person
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
