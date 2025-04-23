using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace crs.game.Games
{
    public partial class Explanation_of_logical_thinking_ability : BaseUserControl
    {
        private const int MAX_DELAY = 5000; // 5 seconds
        private string imagePath;
        private string[] imagePaths;
        private string[] directoryPaths;
        private string[] questionPaths;
        private string[] answerPaths;
        private int hardness;
        private int index;
        private Image lastClickedImage;
        private int[] correctAnswers;
        private int[] wrongAnswers;
        private int max_time = 10;
        private int TRAIN_TIME;
        private bool IS_RESTRICT_TIME = true;
        private bool IS_BEEP = true;
        private int train_time;
        private DispatcherTimer timer;
        private DispatcherTimer trainingTimer;

        private bool isAnswered = false; // Is the mark answered?

        public Action GameBeginAction { get; set; }
        public Func<string, Task> VoicePlayFunc { get; set; }

        public Explanation_of_logical_thinking_ability()
        {
            InitializeComponent();
            hardness = 1;
            correctAnswers = new int[23];
            wrongAnswers = new int[23];
            max_time = 60;
            TRAIN_TIME = 60;
            IS_RESTRICT_TIME = true;
            IS_BEEP = true;
            train_time = TRAIN_TIME;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(MAX_DELAY);
            timer.Tick += Timer_Tick;

            trainingTimer = new DispatcherTimer();
            trainingTimer.Interval = TimeSpan.FromSeconds(1);
            trainingTimer.Tick += TrainingTimer_Tick;

            imagePath = FindImagePath();
            if (imagePath != null)
            {
                AddImages();
                AddButtons();
            }
            else
            {
                MessageBox.Show("No name found"Logical thinking ability"folder.", "mistake", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }

            this.Loaded += Explanation_of_logical_thinking_ability_Loaded;
        }

        private void Explanation_of_logical_thinking_ability_Loaded(object sender, RoutedEventArgs e)
        {
            Button_2_Click(null, null);
        }

        private void TrainingTimer_Tick(object sender, EventArgs e)
        {
            train_time--;
            if (train_time <= 0)
            {
                timer.Stop();
                trainingTimer.Stop();
                this.Close();
            }
        }
        private void Close()
        {
           // throw new NotImplementedException();
        }

        private string FindImagePath()
        {
            //string targetDirectory = @"D:\CCCCC\cognition_software-main\crs.game\Games\Logical thinking ability 1\";
            string targetDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Games\Logical thinking ability 1\");
            if (Directory.Exists(targetDirectory))
            {
                return targetDirectory;
            }
            return null;
        }
        /*
private string FindImagePath()
{
    string currentDirectory = Directory.GetCurrentDirectory();
    {
        string targetDirectory = Path.Combine(currentDirectory, "Logical thinking ability 1");
        if (Directory.Exists(targetDirectory))
        {
            return targetDirectory;
        }
        return null;
    }
    {
        string targetDirectory = Path.Combine(currentDirectory, @"Games\Logical thinking ability 1");
        if (Directory.Exists(targetDirectory))
        {
            return targetDirectory;
        }
        return null;
    }
}
*/

        private void AddImages()
        {
            Random rand = new Random();
            directoryPaths = Directory.GetDirectories(imagePath);
            directoryPaths = directoryPaths.OrderBy(path => int.Parse(path.Split('\\').Last())).ToArray();
            imagePaths = Directory.GetDirectories(directoryPaths[hardness - 1]);

            index = rand.Next(imagePaths.Length);
            string newFolderPath = Path.Combine(imagePaths[index], "Q");
            questionPaths = Directory.GetFiles(newFolderPath);

            for (int i = 0; i < questionPaths.Length; i++)
            {
                Image img = new Image
                {
                    Source = new BitmapImage(new Uri(questionPaths[i])),
                    Width = 150,
                    Height = 150
                };
                ImagePanel.Children.Add(img);
            }

            Button additionalButton = new Button
            {
                Width = 150,
                Height = 150,
                Margin = new Thickness(5),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d0e3b6")),
                BorderBrush = Brushes.Transparent
            };

            additionalButton.Click += AdditionalButton_Click;
            Image buttonImg = new Image { Source = null, Width = 150, Height = 150 };
            additionalButton.Content = buttonImg;
            ImagePanel.Children.Add(additionalButton);
        }

        private void StartTimer()
        {
            if (timer != null && timer.IsEnabled)
            {
                timer.Stop();
            }
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!isAnswered && currentPage == 2)  // Make sure it is in the trial stage and there are no questions answered
            {
                timer.Stop();
                wrongAnswers[hardness - 1]++;
                //MessageBox.Show("Answer time exceeded");
                ClearAndLoadNewImages();
                StartTimer();
            }
        }

        private void ClearAndLoadNewImages()
        {
            // Reset the answered flag
            isAnswered = false;

            ImagePanel.Children.Clear();
            ButtonPanel.Children.Clear();
            AddImages();
            AddButtons();
            StartTimer();
        }

        private void AdditionalButton_Click(object sender, RoutedEventArgs e)
        {
            Button additionalButton = sender as Button;
            Image buttonImg = additionalButton.Content as Image;

            if (buttonImg.Source != null && lastClickedImage != null)
            {
                lastClickedImage.Source = buttonImg.Source;
                buttonImg.Source = null;
                lastClickedImage = null;
            }
        }

        private void AddButtons()
        {
            string newFolderPath = Path.Combine(imagePaths[index], "A");
            answerPaths = Directory.GetFiles(newFolderPath);
            Random rand = new Random();
            answerPaths = answerPaths.OrderBy(x => rand.Next()).Take(6).ToArray(); // Guaranteed to load 6 options

            for (int i = 0; i < answerPaths.Length; i++)
            {
                Button btn = new Button
                {
                    Width = 150,
                    Height = 150,
                    Margin = new Thickness(5),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d0e3b6")),
                    BorderBrush = Brushes.Transparent
                };

                Image img = new Image
                {
                    Source = new BitmapImage(new Uri(answerPaths[i])),
                    Width = 150,
                    Height = 150
                };

                btn.Content = img;
                btn.Click += Button_Click;
                ButtonPanel.Children.Add(btn);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            Image clickedImage = clickedButton.Content as Image;

            Button additionalButton = ImagePanel.Children.OfType<Button>().LastOrDefault();
            Image additionalButtonImage = additionalButton?.Content as Image;

            if (additionalButtonImage.Source == null)
            {
                additionalButtonImage.Source = clickedImage.Source;
                clickedImage.Source = null;
                lastClickedImage = clickedImage;
            }
        }

        private async void Confirm_Button_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage == 2)  // Make sure this is in the trial stage
            {
                isAnswered = true;  // Mark as answered
                (sender as Button).IsEnabled = false;

                // Make sure the timer stops when answering questions
                StopTimerIfNeeded();

                // Find the picture in the last picture box
                Button additionalButton = ImagePanel.Children.OfType<Button>().LastOrDefault();
                Image additionalButtonImage = additionalButton?.Content as Image;

                // Get the first image in the answer path
                string firstImagePath = Directory.GetFiles(Path.Combine(imagePaths[index], "A")).FirstOrDefault();
                bool isCorrect = additionalButtonImage.Source != null && additionalButtonImage.Source.ToString() == new BitmapImage(new Uri(firstImagePath)).ToString();

                if (isCorrect)
                {
                    correctAnswers[hardness - 1]++;
                    textblock.Text = "Congratulations on getting right！";
                    textblock.Foreground = new SolidColorBrush(Colors.Green);
                    // Stop all timers to ensure no more timeout prompts
                    StopTimerIfNeeded();
                    if (trainingTimer != null && trainingTimer.IsEnabled)
                    {
                        trainingTimer.Stop();
                    }

                    // Show after correct answer "Enter training" Button
                    OkButton.Visibility = Visibility.Visible;  // Show the button to enter training
                }
                else
                {
                    wrongAnswers[hardness - 1]++;
                    textblock.Text = "Sorry to answer wrong！";
                    textblock.Foreground = new SolidColorBrush(Colors.Red);
                }

                await Task.Delay(TimeSpan.FromSeconds(1));

                // Reload the picture and buttons and restart the timer
                ClearAndLoadNewImages();
                StartTimer(); // If the game continues, restart the timer
                (sender as Button).IsEnabled = true;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Stop the trial part timer
            StopTimerIfNeeded();

            // Clean up time warning message
            textblock.Text = string.Empty;  // Clear warning information
            textblock.Foreground = new SolidColorBrush(Colors.Black);  // Restore text color

            // Logic of starting training
            OnGameBegin();

            // Enter the training stage and start the training timer
            if (!trainingTimer.IsEnabled)
            {
                trainingTimer.Start();
                train_time = TRAIN_TIME;  // Reset training time
            }
        }

        int currentPage = -1;

        private void Button_1_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 0)
            {
                currentPage--;
                PageSwitch();
            }
        }

        private void Button_2_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < 2)
            {
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
                        Text_1.Visibility = Visibility.Visible;
                        Image_1.Visibility = Visibility.Visible;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        textblock.Visibility = Visibility.Collapsed;
                        ImagePanel.Visibility = Visibility.Collapsed;
                        ButtonPanel.Visibility = Visibility.Collapsed;
                        Button4.Visibility = Visibility.Collapsed;

                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Content = "Next step";
                        Button_2.Margin = new Thickness(329, 850, 0, 0);
                        Button_3.Margin = new Thickness(770, 850, 0, 0);

                        await OnVoicePlayAsync(Text_1.Text);
                    }
                    break;

                case 1:
                    {
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Visible;
                        Image_2.Visibility = Visibility.Visible;
                        textblock.Visibility = Visibility.Collapsed;
                        ImagePanel.Visibility = Visibility.Collapsed;
                        ButtonPanel.Visibility = Visibility.Collapsed;
                        Button4.Visibility = Visibility.Collapsed;
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
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;

                        textblock.Visibility = Visibility.Visible;
                        ImagePanel.Visibility = Visibility.Visible;
                        ButtonPanel.Visibility = Visibility.Visible;
                        Button4.Visibility = Visibility.Visible;
                        StartTimer(); // Make sure the timer starts at this stage

                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Visible;
                        Button_3.Margin = new Thickness(770, 850, 0, 0);

                        if (!trainingTimer.IsEnabled)
                        {
                            trainingTimer.Start();
                            train_time = TRAIN_TIME;
                        }

                        this.Focus();
                        //LJN, instructing the delegate in the explanation module
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("You will see a sequence of images on the screen, and the images of the sequence have some pattern between each other. You need to think about discovering the pattern and click on the mouse in several images provided to find a suitable option.");//Add code, call function, display the text under the digital person
                        //LJN
                    }
                    break;
            }
        }

        async Task VoicePlayer(string message)
        {
            var voicePlayFunc = VoicePlayFunc;
            if (voicePlayFunc == null)
            {
                return;
            }
            await voicePlayFunc.Invoke(message);
        }

        private void StopTimerIfNeeded()
        {
            if (timer != null && timer.IsEnabled)
            {
                timer.Stop();
            }
        }
    }
}
