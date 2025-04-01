
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Linq;
using System.Data;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Text;
using System.Security.Claims;

namespace crs.game.Games
{
    /// <summary>
    /// Explanation of word memory.xaml Interaction logic
    /// </summary>
    public partial class Explanation of word memory : BaseUserControl
    {
        private int NumberOfTextBlocks = 3;//Number of words to pass by
        private List<string> WordList = new List<string> { "This is not", "I", "unnecessary", "Don't remember", "have to", "land", "Double words" };
        private List<string> WordsToMemorizeList = new List<string>();

        private int RunDirection = 1;//Vocabulary movement direction, 1 right 0 left
        private int StopDurations = 2000; // Stop time,ms
        private int Speed = 5;
        private int TreatDurations = 1;//Minutes in units
        private double CountdownSeconds = 0;//Number of seconds to count down

        public Action GameBeginAction { get; set; }

        public Func<string, Task> VoicePlayFunc { get; set; }

        public Explanation of word memory()
        {
            InitializeComponent();

            WordsToMemorizeList = WordsToMemorize.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            //The method I use to remember is to put thattextblockThe insidetextSeparate the space to get a list, this logic can be changed
            this.Loaded += TrainWindow_Loaded; // Make sure to set the initial position of the button after the window is loaded
            this.KeyDown += OnHostWindow_KeyDown; // Listen to keyboard events


            //The following are some initialization-related issues, no modification is required
            // Initialization countdown related
            CountdownSeconds = TreatDurations * 60; // Countdown 10 seconds, you can change it according to your needs
            CountdownTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1) // The timer triggers once per second
            };
            CountdownTimer.Tick += CountdownTimer_Tick;
            RandomObject = new Random();
            TextBlockDetected = new Dictionary<TextBlock, bool>(); // Initialize the detection status dictionary
            //string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory; // The absolute path to the current project
            //BaseDirectory = Path.Combine(BaseDirectory, "../../../crs.game/Games/pic/WORT");
            string BaseDirectory = "./pic/WORT/";
            CorrectSound = Path.Combine(BaseDirectory, $"Correct.wav");
            ErrorSound = Path.Combine(BaseDirectory, $"Error.wav");
            CorrectImage.Source = new BitmapImage(new Uri(Path.Combine(BaseDirectory, "Correct.png"), UriKind.RelativeOrAbsolute));
            ErrorImage.Source = new BitmapImage(new Uri(Path.Combine(BaseDirectory, "Error.png"), UriKind.RelativeOrAbsolute));
            TextBlockAnimations = new List<Storyboard>(); // Initialize the animation list

            this.Loaded += Explanation of word memory_Loaded;


        }

        private void Explanation of word memory_Loaded(object sender, RoutedEventArgs e)
        {
            // Ensure key presses and focus behavior when page loads
            Button_2_Click(null, null);
            this.Focus();  // Make sure the focus is on the window
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            if (CountdownSeconds > 0)
            {
                CountdownSeconds--;
                CountdownDisplay.Text = CountdownSeconds.ToString(); // Countdown display on the update interface
            }
            else
            {
                CountdownTimer.Stop();
                //When the countdown is over, click the end button automatically
                //OkButton_Click();
            }
        }

        private void TrainWindow_Loaded(object sender, RoutedEventArgs e)
        {//Write this layer more to ensure that the window starts to be created when it is loadedtextblockObject
            this.Focus(); // Set focus to main window
            CreateTextBlocksOffScreen();

            BeltBorder.Visibility = Visibility.Collapsed;
        }

        private void CreateTextBlocksOffScreen()
        {
            double canvasHeight = WordArea.ActualHeight;
            double canvasWidth = WordArea.ActualWidth;


            // Fixed width and height for each TextBlock
            double textBlockWidth = 200;

            for (int i = 0; i < NumberOfTextBlocks; i++)
            {
                TextBlock textBlock = new TextBlock
                {
                    Text = GetRandomWord(),
                    Background = Brushes.Transparent, // Set transparent background
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Height = canvasHeight,
                    Width = textBlockWidth,
                    FontFamily = new FontFamily("Times New Roman"), // Set fonts
                    FontSize = 160, // Set the font size
                    Visibility = Visibility.Hidden//Hide it at the beginning
                };

                AdjustTextBlockSize(textBlock);

                // Add the TextBlock to Canvas
                WordArea.Children.Add(textBlock);

                // Calculate vertical center position for the TextBlock
                double textBlockHeight = textBlock.Height;
                double verticalCenterPosition = (canvasHeight - textBlockHeight) / 2;

                // Set initial position off-screen 
                double initialLeftPosition = RunDirection == 1 ? canvasWidth : -textBlockWidth;
                Canvas.SetLeft(textBlock, initialLeftPosition);
                Canvas.SetTop(textBlock, verticalCenterPosition); // Set vertical centering position

                // Will TextBlock The detection state is initialized to false
                TextBlockDetected[textBlock] = false;
            }
        }

        private void AnimateTextBlocks(int direction, double speed)
        {
            double canvasWidth = WordArea.ActualWidth;
            double textBlockWidth = 200;
            double durationInSeconds = 11 - speed; // Speed 1 (slowest) -> 10 seconds, Speed 10 (fastest) -> 1 second

            // Calculate delay per TextBlock to avoid them starting at the same time
            double delayInterval = durationInSeconds / NumberOfTextBlocks;

            for (int i = 0; i < WordArea.Children.Count; i++)
            {
                if (WordArea.Children[i] is TextBlock textBlock)
                {
                    double from = direction == 0 ? canvasWidth : -textBlockWidth;
                    double to = direction == 0 ? -textBlockWidth : canvasWidth;

                    StartTextBlockAnimation(textBlock, from, to, durationInSeconds, TimeSpan.FromSeconds(i * delayInterval));
                }
            }
        }

        private void StartTextBlockAnimation(TextBlock textBlock, double from, double to, double durationInSeconds, TimeSpan beginTime)
        {
            // Create and configure the animation
            DoubleAnimation animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(durationInSeconds)),
                BeginTime = beginTime
            };

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, textBlock);
            Storyboard.SetTargetProperty(animation, new PropertyPath(Canvas.LeftProperty));

            // Update text content every time the animation ends
            storyboard.Completed += (s, e) =>
            {

                textBlock.Text = GetRandomWord();

                AdjustTextBlockSize(textBlock); // Dynamic adjustment TextBlock width
                StartTextBlockAnimation(textBlock, from, to, durationInSeconds, TimeSpan.Zero); // Restart the painting
            };

            // Reset detection status before starting the animation
            TextBlockDetected[textBlock] = false;
            TextBlockAnimations.Add(storyboard); // Add animation to list

            // Check location and update visibility during the animation
            storyboard.CurrentTimeInvalidated += (s, e) =>
            {
                UpdateTextBlockVisibility(textBlock, WordArea);
            };
            // for Canvas Set the clipping area to ensure TextBlock exist Canvas The external part is not visible
            ApplyCanvasClip(WordArea);
            storyboard.Begin();
        }

        // Apply the clipping area to Canvas
        private void ApplyCanvasClip(Canvas containerCanvas)
        {//Through cropping,textblockIn thiscanvasThe part visible is not visible
            // Create a with Canvas Rectangles of the same size
            RectangleGeometry clipGeometry = new RectangleGeometry
            {
                Rect = new Rect(0, 0, containerCanvas.ActualWidth, containerCanvas.ActualHeight)
            };

            // Take the rectangle as Canvas Crop area
            containerCanvas.Clip = clipGeometry;
        }

        private void UpdateTextBlockVisibility(TextBlock textBlock, Canvas containerCanvas)
        {//For the exercisetextblockCheck if they should appear
            // Get TextBlock The current left margin
            double left = Canvas.GetLeft(textBlock);
            double textBlockWidth = textBlock.ActualWidth;
            // Get Canvas Width of
            double canvasLeft = 0; // Assumptions Canvas The left border of 0
            double canvasRight = containerCanvas.ActualWidth;
            // Judgment TextBlock Is it there Canvas Within the scope of
            if (left + textBlockWidth > canvasLeft && left < canvasRight)
            {
                // exist Canvas Within the range, the settings are visible
                textBlock.Visibility = Visibility.Visible;
            }
            else
            {
                // More than Canvas Range, set hidden
                textBlock.Visibility = Visibility.Hidden;
            }
        }

        private void AdjustTextBlockWidth(TextBlock textBlock)
        {
            // use FormattedText To measure text width
            var formattedText = new FormattedText(
                textBlock.Text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
                textBlock.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                1);

            // Dynamic adjustment TextBlock Width of
            textBlock.Width = formattedText.Width;
        }

        private void AdjustTextBlockSize(TextBlock textBlock)
        {
            // use FormattedText To measure text size
            var formattedText = new FormattedText(
                textBlock.Text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
                textBlock.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                1);

            // Dynamic adjustment TextBlock Width and height
            textBlock.Width = formattedText.Width;
            textBlock.Height = formattedText.Height;
        }

        private void PositionRectangle(int direction)
        {
            if (direction == 0)
            {
                Canvas.SetLeft(TargetArea, 40); // WillRectangleMove to the left
            }
            else
            {
                double canvasWidth = WordArea.ActualWidth;
                Canvas.SetLeft(TargetArea, canvasWidth - TargetArea.Width - 40); // WillRectangleMove to the right
            }
            TargetArea.Visibility = Visibility.Visible;
        }

        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) // Key detection
        {//Please note whether it isbuttonquiltfocus, otherwise the keys will be recognized in pairsbuttonPress
            // Check whether the key you pressed is the key you specified
            if (e.Key == System.Windows.Input.Key.Enter) // Suppose the key you specified is the Enter key
            {
                CheckIntersection();
            }
        }

        private async void CheckIntersection()
        {
            // Get Rectangle The boundary of
            double rectLeft = Canvas.GetLeft(TargetArea);
            double rectTop = Canvas.GetTop(TargetArea);

            // Check if it is NaN And give default values
            if (double.IsNaN(rectLeft)) rectLeft = 0;
            if (double.IsNaN(rectTop)) rectTop = 0;

            // create Rectangle boundary
            Rect rectangleBounds = new Rect(rectLeft, rectTop, TargetArea.Width, TargetArea.Height);

            foreach (var child in WordArea.Children)
            {
                if (child is TextBlock textBlock)
                {
                    // if TextBlock Not detected yet
                    if (!TextBlockDetected[textBlock])
                    {
                        // Get TextBlock The boundary of
                        double textBlockLeft = Canvas.GetLeft(textBlock);
                        double textBlockTop = Canvas.GetTop(textBlock);

                        // Check if it is NaN And give default values
                        if (double.IsNaN(textBlockLeft)) textBlockLeft = 0;
                        if (double.IsNaN(textBlockTop)) textBlockTop = 0;

                        // create TextBlock boundary
                        Rect textBlockBounds = new Rect(textBlockLeft, textBlockTop, textBlock.Width, textBlock.ActualHeight);

                        // Check for overlap
                        if (rectangleBounds.IntersectsWith(textBlockBounds))
                        {
                            // Make judgment and update _ViewModel counter
                            if (WordsToMemorizeList.Contains(textBlock.Text))
                            {
                                PlayWav(CorrectSound);
                                ShowFeedbackImage(CorrectImage);
                                ShowFeedbackTextBlock(CorrectTextBlock); // Show correct text feedback
                            }
                            else
                            {
                                PlayWav(ErrorSound);
                                ShowFeedbackImage(ErrorImage);
                                ShowFeedbackTextBlock(ErrorTextBlock); // Show correct text feedback
                            }
                            // MessageBox.Show(textBlock.Text);
                            TextBlockDetected[textBlock] = true; // Update detection status

                            // Stop all animations
                            foreach (var storyboard in TextBlockAnimations)
                            {
                                storyboard.Pause();
                            }

                            // Delay StopDurations millisecond
                            await Task.Delay(StopDurations);

                            // Restart all animations
                            foreach (var storyboard in TextBlockAnimations)
                            {
                                storyboard.Resume();
                            }

                            break; // Exit the loop after finding an overlapping text
                        }
                    }
                }
            }
        }


        private void MemorizeOK_Click(object sender, RoutedEventArgs e)
        {//Memory is completed, start trial play
            if (IfHaveStarted == false)
            {
                int RunDirectionStart = RunDirection;
                int RunSpeed = Speed;
                WordsToMemorize.Visibility = Visibility.Hidden;//Hide the words that need to be remembered
                BeltBorder.Visibility = Visibility.Visible;//Show the conveyor belt bounding box
                PositionRectangle(RunDirectionStart);
                AnimateTextBlocks(RunDirectionStart, (double)RunSpeed);
                MemorizeOK.Visibility = Visibility.Collapsed;
                Button_3.Margin = new Thickness(550, 850, 0, 0);
                OkButton.Visibility = Visibility.Visible;
                OkButton.Margin = new Thickness(1100, 850, 0, 0);
                // Reset the countdown and start the timer
                CountdownSeconds = TreatDurations * 60; // Reset countdown time
                CountdownDisplay.Text = CountdownSeconds.ToString();
                CountdownTimer.Start();
            }
            IfHaveStarted = true;
        }
        //Here are some temporary variables.coderNo need to modify
        private List<Storyboard> TextBlockAnimations; // List stores all animations
        private SoundPlayer soundPlayer; // Used to sing
        public string ErrorSound;
        public string CorrectSound;
        private Random RandomObject;
        private Dictionary<TextBlock, bool> TextBlockDetected; // Store each TextBlock Detection status
        private bool IfHaveStarted = false;//Used to limit the button to be pressed only once, and the second time you press the flag position without effect
        private DispatcherTimer CountdownTimer;

        //There are some function functions here, no need to modify
        private string GetRandomWord()
        {//Get random content
            // Make sure both lists are loaded
            if ((WordList == null || WordList.Count == 0) && (WordsToMemorizeList == null || WordsToMemorizeList.Count == 0))
            {
                return "No Words Loaded";
            }
            // Random selection list, 0 express WordList，1 express _ViewModel.WordsToMemorizeList
            int listSelector = RandomObject.Next(0, 2);
            List<string> selectedList;
            if (listSelector == 0 && WordList != null && WordList.Count > 0)
            {
                selectedList = WordList;
            }
            else if (WordsToMemorizeList != null && WordsToMemorizeList.Count > 0)
            {
                selectedList = WordsToMemorizeList;
            }
            else if (WordList != null && WordList.Count > 0)
            {
                // If none of the above meets the criteria, use the remaining non-empty list
                selectedList = WordList;
            }
            else
            {
                return "No Words Loaded";
            }

            // Select an element randomly from the selected list
            int index = RandomObject.Next(selectedList.Count);
            return selectedList[index];
        }
        private void PlayWav(string filePath)
        {
            if (soundPlayer != null)
            {
                soundPlayer.Stop();
                soundPlayer.Dispose();
            }

            soundPlayer = new SoundPlayer(filePath);
            soundPlayer.Load();
            soundPlayer.Play();
        }

        private async void ShowFeedbackImage(Image image)
        {
            image.Visibility = Visibility.Visible;

            // Delay the specified time（For example, 2 seconds）
            await Task.Delay(StopDurations);

            image.Visibility = Visibility.Collapsed;
        }

        private async void ShowFeedbackTextBlock(TextBlock textBlock)
        {
            textBlock.Visibility = Visibility.Visible;

            // Delay the specified time（For example, 2 seconds）
            await Task.Delay(StopDurations);

            textBlock.Visibility = Visibility.Collapsed;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            //Here is to makerectangleNot so fast to show
            TargetArea.Visibility = Visibility.Visible;
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

                        // Hide part of the trial
                        CountdownDisplay.Visibility = Visibility.Collapsed;
                        WordsToMemorize.Visibility = Visibility.Collapsed;
                        MemorizeOK.Visibility = Visibility.Collapsed;
                        OkButton.Visibility = Visibility.Collapsed;
                        WordArea.Visibility = Visibility.Collapsed;
                        TargetArea.Visibility = Visibility.Collapsed;
                        CorrectImage.Visibility = Visibility.Collapsed;
                        ErrorImage.Visibility = Visibility.Collapsed;
                        CorrectTextBlock.Visibility = Visibility.Collapsed;
                        ErrorTextBlock.Visibility = Visibility.Collapsed;
                        //Button_1.IsEnabled = false;
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



                        // Hide the controls for the trial play section
                        CountdownDisplay.Visibility = Visibility.Collapsed;
                        WordsToMemorize.Visibility = Visibility.Collapsed;
                        MemorizeOK.Visibility = Visibility.Collapsed;
                        OkButton.Visibility = Visibility.Collapsed;
                        WordArea.Visibility = Visibility.Collapsed;
                        TargetArea.Visibility = Visibility.Collapsed;
                        CorrectImage.Visibility = Visibility.Collapsed;
                        ErrorImage.Visibility = Visibility.Collapsed;
                        CorrectTextBlock.Visibility = Visibility.Collapsed;
                        ErrorTextBlock.Visibility = Visibility.Collapsed;
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
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;

                        // Controls showing the trial part
                        //CountdownDisplay.Visibility = Visibility.Visible;
                        WordsToMemorize.Visibility = Visibility.Visible;
                        MemorizeOK.Visibility = Visibility.Visible;
                        OkButton.Visibility = Visibility.Collapsed;
                        WordArea.Visibility = Visibility.Visible;
                        //TargetArea.Visibility = Visibility.Visible;
                        //CorrectImage.Visibility = Visibility.Visible;
                        //ErrorImage.Visibility = Visibility.Visible;
                        //CorrectTextBlock.Visibility = Visibility.Visible;
                        //ErrorTextBlock.Visibility = Visibility.Visible;
                        // Hide the button for the explanation section
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Visible;
                        Button_3.Margin = new Thickness(770, 850, 0, 0);

                        // Force focus to remain in the window
                        this.Focus();

                        //LJN, instructing the delegate in the explanation module
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("Please remember the words that appear on the screen during the memory stage. After the memory is completed, press the keyboardOKkey. Then there will be a series of words on the screen moving from left to right. When you see the words you remembered in the memory stage and move into the box, press theOKkey.");//Add code, call function, display the text under the digital person
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
