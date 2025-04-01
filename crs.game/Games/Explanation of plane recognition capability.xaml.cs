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
    /// Reaction behavior explanation.xaml Interaction logic
    /// </summary>
    public partial class Explanation of plane recognition capability : BaseUserControl
    {
        private string[] JudgementPath = new string[]
        {
            "VOR/judgement/right.png",
            "VOR/judgement/wrong.png"
        };

        private readonly string[][] imagePaths = new string[][]
        {
            new string[]
            {
                "Img3/1/1.jpg",
                "Img3/1/2.jpg",
                "Img3/1/3.jpg",
                "Img3/1/4.jpg",
                "Img3/1/5.jpg",
                "Img3/1/6.jpg",
                "Img3/1/7.jpg",
                "Img3/1/8.jpg",
                "Img3/1/9.jpg",
            },
            new string[]
            {
                "Img3/2/1.jpg",
                "Img3/2/2.jpg",
                "Img3/2/3.jpg",
                "Img3/2/4.jpg",
                "Img3/2/5.jpg",
                "Img3/2/6.jpg",
                "Img3/2/7.jpg",
                "Img3/2/8.jpg",
                "Img3/2/9.jpg",
            },
            new string[]
            {
                "Img3/3/1.jpg",
                "Img3/3/2.jpg",
                "Img3/3/3.jpg",
                "Img3/3/4.jpg",
                "Img3/3/5.jpg",
                "Img3/3/6.jpg",
                "Img3/3/7.jpg",
                "Img3/3/8.jpg",
                "Img3/3/9.jpg",
            },
            new string[]
            {
                "Img3/4/1.jpg",
                "Img3/4/2.jpg",
                "Img3/4/3.jpg",
                "Img3/4/4.jpg",
                "Img3/4/5.jpg",
                "Img3/4/6.jpg",
                "Img3/4/7.jpg",
                "Img3/4/8.jpg",
                "Img3/4/9.jpg",
            },
            new string[]
            {
                "Img3/5/1.jpg",
                "Img3/5/2.jpg",
                "Img3/5/3.jpg",
                "Img3/5/4.jpg",
                "Img3/5/5.jpg",
                "Img3/5/6.jpg",
                "Img3/5/7.jpg",
                "Img3/5/8.jpg",
                "Img3/5/9.jpg",
            },
            new string[]
            {
                "Img3/6/1.jpg",
                "Img3/6/2.jpg",
                "Img3/6/3.jpg",
                "Img3/6/4.jpg",
                "Img3/6/5.jpg",
                "Img3/6/6.jpg",
                "Img3/6/7.jpg",
                "Img3/6/8.jpg",
                "Img3/6/9.jpg",
            },
            new string[]
            {
                "Img3/7/1.jpg",
                "Img3/7/2.jpg",
                "Img3/7/3.jpg",
                "Img3/7/4.jpg",
                "Img3/7/5.jpg",
                "Img3/7/6.jpg",
                "Img3/7/7.jpg",
                "Img3/7/8.jpg",
                "Img3/7/9.jpg",
            },
            new string[]
            {
                "Img3/8/1.jpg",
                "Img3/8/2.jpg",
                "Img3/8/3.jpg",
                "Img3/8/4.jpg",
                "Img3/8/5.jpg",
                "Img3/8/6.jpg",
                "Img3/8/7.jpg",
                "Img3/8/8.jpg",
                "Img3/8/9.jpg",
            },
        };

        private int imageCount;
        private int max_time = 10;
        private const int WAIT_DELAY = 1;
        private const int MAX_HARDNESS = 40;
        private int INCREASE = 5; // Increase the threshold for difficulty
        private int DECREASE = 5;  // Threshold for reducing difficulty
        private int TRAIN_TIME = 60; // Training duration（Unit: seconds）
        private bool IS_RESTRICT_TIME = true; // Whether to enable exercise time
        private bool IS_BEEP = true;
        private int train_time;
        private int counter;
        private int randomIndex;
        private Random random;
        private Random randomforrotate;
        private const int moveAmount = 2;
        private int left;
        private int top;
        private int hardness;
        private DispatcherTimer timer;
        private int remainingTime;
        private DispatcherTimer trainingTimer; // New timer for training time
        private bool is_game = true;
        private int[][] RotationDegreeList;

        public Action GameBeginAction { get; set; }

        public Func<string, Task> VoicePlayFunc { get; set; }

        public Explanation of plane recognition capability()
        {
            InitializeComponent();
            max_time = 60;
            INCREASE = 2; // Increase the threshold for difficulty
            DECREASE = 2;  // Threshold for reducing difficulty
            TRAIN_TIME = 60; // Training duration（Unit: seconds）
            IS_RESTRICT_TIME = true; // Whether to enable exercise time
            IS_BEEP = true;
            hardness = 1;
            remainingTime = max_time;
            imageCount = (hardness % 3) * 3;
            if (imageCount == 0)
                imageCount = 9;
            random = new Random();
            counter = 0;
            train_time = TRAIN_TIME;
            RotationDegreeList = new int[4][];

            for (int i = 0; i < 4; i++)
            {
                RotationDegreeList[i] = new int[4];
                for (int j = 0; j < 4; j++)
                {
                    RotationDegreeList[i][j] = 0;
                }
            }
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            trainingTimer = new DispatcherTimer();
            trainingTimer.Interval = TimeSpan.FromSeconds(1);
            trainingTimer.Tick += TrainingTimer_Tick;
            trainingTimer.Start(); // Start the training timer

            LoadImages(imageCount);
            ShowRandomImage();
            left = 1;
            top = 1;

            this.Loaded += Explanation of plane recognition capability_Loaded;
            // Capture global key press events, use PreviewKeyDown Capture events earlier
            this.PreviewKeyDown += Window_PreviewKeyDown;

            // Make sure the focus remains in the window
            this.Focusable = true;  // Make sure the window gets focus
            this.Focus();  // Force focus to the current window

        }

        private void Explanation of plane recognition capability_Loaded(object sender, RoutedEventArgs e)
        {
            // Ensure key presses and focus behavior when page loads
            Button_2_Click(null, null);
            FocusWindow();
        }

        private void FocusWindow()
        {
            // Force focus to be kept at ImageGrid or SelectionBox superior
            Keyboard.Focus(SelectionBox);  // or ImageGrid
        }

        private void ShowRandomImage()
        {
            randomIndex = random.Next(imageCount);
            RandomImage.Source = new BitmapImage(new Uri(imagePaths[(hardness - 1) / 3][randomIndex], UriKind.Relative));

            if (IS_RESTRICT_TIME)
            {
                remainingTime = max_time;
                timer.Start();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            remainingTime--;
            var rotateTransform = new RotateTransform { Angle = RotationDegreeList[(top + 1) / 2][(left + 1) / 2] };
            SelectionBox.RenderTransform = rotateTransform;
            SelectionBox.RenderTransformOrigin = new Point(0.5, 0.5);
            if (remainingTime <= 0)
            {
                timer.Stop();
                LoadImages(imageCount);
                ShowRandomImage();
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


        private void LoadImages(int imageCount)
        {
            // Clear the previous pictures
            for (int i = ImageGrid.Children.Count - 1; i >= 0; i--)
            {
                if (ImageGrid.Children[i] is Image)
                {
                    ImageGrid.Children.RemoveAt(i);
                }
            }
            // Load new picture
            for (int i = 0; i < imageCount; i++)
            {
                Image image = new Image
                {
                    Source = new BitmapImage(new Uri(imagePaths[(hardness - 1) / 3][i], UriKind.Relative)),
                    Stretch = System.Windows.Media.Stretch.Uniform
                };
                randomforrotate = new Random();
                int rotationDegrees = 0;
                if (hardness >= 1 && hardness <= 3)
                {
                    rotationDegrees = randomforrotate.Next(4) * 90;
                    var rotateTransform = new RotateTransform { Angle = rotationDegrees };
                    image.RenderTransform = rotateTransform;
                    image.RenderTransformOrigin = new Point(0.5, 0.5);
                }
                int row = 1 + (i / 3) * 2;
                int column = 1 + (i % 3) * 2;
                Grid.SetRow(image, row);
                Grid.SetColumn(image, column);
                ImageGrid.Children.Add(image);
                if (i < 3) RotationDegreeList[1][i + 1] = rotationDegrees;
                if (i >= 3 && i < 6) RotationDegreeList[2][i % 3 + 1] = rotationDegrees;
                if (i >= 6) RotationDegreeList[3][i % 3 + 1] = rotationDegrees;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                bool isCorrect = (top - 1) * 3 / 2 + (left - 1) / 2 == randomIndex;
                if (isCorrect)
                {
                    ChangeSelectionBoxColor((Color)ColorConverter.ConvertFromString("#00ff00"));
                    judgement.Source = new BitmapImage(new Uri(JudgementPath[0], UriKind.Relative));
                    //textblock.Text = "Congratulations on getting right！";
                    //textblock.Margin = new Thickness(50, 700, 0, 0);
                    //textblock.Foreground = new SolidColorBrush(Colors.Green);
                    OkButton.Visibility = Visibility.Visible;
                }
                else
                {
                    if (IS_BEEP)
                        Console.Beep();
                    ChangeSelectionBoxColor((Color)ColorConverter.ConvertFromString("#ff0000"));
                    judgement.Source = new BitmapImage(new Uri(JudgementPath[1], UriKind.Relative));
                    //textblock.Text = "Sorry to answer wrong！";
                    //textblock.Margin = new Thickness(50, 700, 0, 0);
                    //textblock.Foreground = new SolidColorBrush(Colors.Red);
                    OkButton.Visibility = Visibility.Collapsed;
                }
                is_game = false;
            }
                
            else
            {
                if (is_game) {
                    if (top > 1 && e.Key == Key.Up)
                        top -= moveAmount;
                    if (top < (imageCount / 3) * 2 - 1 && e.Key == Key.Down)
                        top += moveAmount;
                    if (left > 1 && e.Key == Key.Left)
                        left -= moveAmount;
                    if (left < 5 && e.Key == Key.Right)
                        left += moveAmount;
                }
                
            }

            Grid.SetColumn(SelectionBox, left);
            Grid.SetRow(SelectionBox, top);
            var rotateTransform = new RotateTransform { Angle = RotationDegreeList[(top + 1) / 2][(left + 1) / 2] };
            SelectionBox.RenderTransform = rotateTransform;
            SelectionBox.RenderTransformOrigin = new Point(0.5, 0.5);

            // Make sure the focus remains SelectionBox superior
            FocusSelectionBoxWithDelay();

            // Prevent the incident from spreading
            e.Handled = true;
        }

        private void FocusSelectionBoxWithDelay()
        {
            var rotateTransform = new RotateTransform { Angle = RotationDegreeList[(top + 1) / 2][(left + 1) / 2] };
            SelectionBox.RenderTransform = rotateTransform;
            SelectionBox.RenderTransformOrigin = new Point(0.5, 0.5);
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Keyboard.Focus(SelectionBox);
            }), DispatcherPriority.Background);
        }

        private async void ChangeSelectionBoxColor(Color color)
        {
            SelectionBox.Stroke = new SolidColorBrush(color);

            // Wait for the specified time
            await Task.Delay(TimeSpan.FromSeconds(WAIT_DELAY));

            // Restore the original color
            SelectionBox.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3a98d1"));
            is_game = true;

            left = 1;
            top = 1;
            var rotateTransform = new RotateTransform { Angle = RotationDegreeList[(top + 1) / 2][(left + 1) / 2] };
            SelectionBox.RenderTransform = rotateTransform;
            SelectionBox.RenderTransformOrigin = new Point(0.5, 0.5);
            Grid.SetColumn(SelectionBox, left);
            Grid.SetRow(SelectionBox, top);
            
            // Adjust the difficulty
            LoadImages(imageCount);
            ShowRandomImage();is_game = true;
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
                        Text_1.Visibility = Visibility.Visible;
                        Image_1.Visibility = Visibility.Visible;
                        Text_2.Visibility = Visibility.Collapsed; 
                        Image_2.Visibility = Visibility.Collapsed; 
                        Image_3.Visibility = Visibility.Collapsed;
                        ImageGrid.Visibility = Visibility.Collapsed;
                        image.Visibility = Visibility.Collapsed;
                        SelectionBox.Visibility = Visibility.Collapsed;
                        RandomImage.Visibility = Visibility.Collapsed;
                        textblock.Visibility = Visibility.Collapsed;
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
                        Image_3.Visibility = Visibility.Visible;
                        ImageGrid.Visibility = Visibility.Collapsed;
                        ImageGrid.Visibility = Visibility.Collapsed;
                        image.Visibility = Visibility.Collapsed;
                        SelectionBox.Visibility = Visibility.Collapsed;
                        RandomImage.Visibility = Visibility.Collapsed;
                        textblock.Visibility = Visibility.Collapsed;
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
                        Image_3.Visibility = Visibility.Collapsed;

                        // Controls showing the trial part
                        ImageGrid.Visibility = Visibility.Visible;
                        image.Visibility = Visibility.Visible;
                        SelectionBox.Visibility = Visibility.Visible;
                        RandomImage.Visibility = Visibility.Visible;
                        textblock.Visibility = Visibility.Visible;
                        OkButton.Visibility = Visibility.Collapsed;
                        // Hide the button for the explanation section
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Visible;
                        Button_3.Margin = new Thickness(550, 850, 0, 0);

                        // Force focus to remain in the window
                        this.Focus();

                        //LJN, instructing the delegate in the explanation module
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("You will see an image on the right side of the screen, and you will see several different images after being rotated on the left side. You need to identify on the screen which image is obtained by rotating the image on the right side of the screen, and select it through the up, down, left and right keys of the keyboard and pressOKKey confirmation.");//Add code, call function, display the text under the digital person
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