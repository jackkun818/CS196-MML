using crs.core;
using crs.core.DbModels;
using Microsoft.Identity.Client.NativeInterop;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace crs.game.Games
{
    public partial class Response_behavior : BaseUserControl
    {
        private readonly string[] imagePaths =
        {
            "Response behavior/forbid.png",
            "Response behavior/left.png",
            "Response behavior/right.png",
            "Response behavior/down.jpeg",
            "Response behavior/wildanimal.jpg"
        };
        private readonly string correct_wav = "./Resources/Word memory/Effects/Correct.wav";
        private readonly string wrong_wav = "./Resources/Word memory/Effects/Error.wav";
        private int increase;//The answers were correct in 30
        private int decrease;//The answer was wrong among 30
        private Random random;
        private int MAX_HARDNESS = 15;
        private int MAX_REACT = 5000;
        private int counterA;//Number of related stimuli
        private int counterB;//wildquantity
        private int randomIndex;
        private int ignore;
        private int wrong;
        private int overtime;
        private DispatcherTimer displayTimer;
        private DispatcherTimer trainingTimer; // Added timer for training time
        private DispatcherTimer imageDisplayTimer;//Image display interval
        private int STIMULI_INTERVAL = 3000; // Stimulation interval
        private int TRAIN_TIME = 60; // Training time 60 seconds
        private int INCREASE = 5; // Threshold for increasing difficulty
        private int DECREASE = 5; // Threshold for reducing difficulty
        private int hardness = 1; // Set initial difficulty
        private double remainingTime; // The remaining time of the current question（Second）
        private int traintime; // Total remaining time（Second）
        private bool IS_BEEP = true;
        private bool IS_SCREEN = true;
        private int delayTime;
        private int max_hardness;
        private int press = 0;
        private double timecount = 0;


        /// <summary>
        /// Time to answer the question
        /// </summary>
        DateTime startTime = new DateTime();
        DateTime endTime = new DateTime();
        public Response_behavior()
        {
            InitializeComponent();

        }
        private async Task ChangeBorderColor()
        {
            if (BorderElement.Background is SolidColorBrush brush && brush.Color == Colors.Red)
                return;
            // Save the original color
            var originalColor = BorderElement.Background;

            // Set to red
            BorderElement.Background = new SolidColorBrush(Colors.Red);

            // wait 0.2 Second
            await Task.Delay(200);

            // Restore the original color
            BorderElement.Background = originalColor;
        }
        private void PlaySound(string filePath)
        {
            try
            {
                string absolutePath = System.IO.Path.GetFullPath(filePath);
                if (!System.IO.File.Exists(filePath))
                {
                    MessageBox.Show($"The sound effect file does not exist:{absolutePath}");
                    return;
                }
                SoundPlayer player = new SoundPlayer(filePath);
                player.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while playing sound effects:{ex.Message}");
            }
        }

        private void ShowRandomImage()
        {
            // Set the image source tonull
            RandomImage.Source = null;
            imageDisplayTimer?.Stop();
            // Generate a random delay time（500ms arrive 2000ms）
            imageDisplayTimer.Interval = TimeSpan.FromMilliseconds(STIMULI_INTERVAL);
            imageDisplayTimer?.Start();


        }
        private void OnImageDisplayTimerElapsed(object sender, EventArgs e)
        {
            imageDisplayTimer?.Stop();
            DisplayRandomImage(); // Show random images
        }

        private void OnDisplayTimerElapsed(object sender, EventArgs e)
        {
            // Reduce remaining time per second
            remainingTime--;
            //MessageBox.Show(remainingTime.ToString());
            // If the remaining time is reached 0, stop the timer and execute the settlement logic
            if (remainingTime < 0)
            {
                endTime = DateTime.Now;
                TimeSpan duration = endTime - startTime; // Calculate the answer time            
                timecount += (int)duration.TotalMilliseconds;
                press++;


                displayTimer?.Stop(); // Stop the timer
                if (randomIndex != 4)
                {
                    counterA++;
                    decrease++;
                    ignore++;
                }
                else if (randomIndex == 4)
                {
                    counterB++;
                    increase++;
                }
                AdjustDifficulty();
                DisplayRandomImage();
            }
        }
        private void OnTrainingTimerElapsed(object sender, EventArgs e)
        {
            traintime--;
            // Calling delegate, passing training time and remaining time
            TimeStatisticsAction?.Invoke(traintime, (int)remainingTime);
            // If TRAIN_TIME, stop all timers and open the report window
            if (traintime < 0)
            {
                StopAllTimers();
                OnGameEnd();
            }
        }

        private void StopAllTimers()
        {
            imageDisplayTimer?.Stop();
            displayTimer?.Stop();
            trainingTimer?.Stop();
        }
        private void DisplayRandomImage()
        {
            double maxY;
            double maxX;
            double left;
            double top;
            remainingTime = MAX_REACT / 1000;
            displayTimer?.Start();

            startTime = DateTime.Now;
            switch (hardness)
            {
                case 1:
                    randomIndex = random.Next(1); //forbid
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Collapsed;
                    left_arrow.Visibility = Visibility.Collapsed;
                    Right.Visibility = Visibility.Collapsed;
                    right_arrow.Visibility = Visibility.Collapsed;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 2:
                    randomIndex = random.Next(1); //forbid(full screen)
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Collapsed;
                    left_arrow.Visibility = Visibility.Collapsed;
                    Right.Visibility = Visibility.Collapsed;
                    right_arrow.Visibility = Visibility.Collapsed;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    maxX = ImageGrid.ActualWidth - RandomImage.Width;
                    maxY = ImageGrid.ActualWidth / 1.7 - RandomImage.Height;
                    left = random.NextDouble() * maxX;
                    top = random.NextDouble() * maxY;
                    RandomImage.Margin = new Thickness(left, top, 0, 0);
                    break;

                case 3:
                    randomIndex = random.Next(2);//forbid+left
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Collapsed;
                    right_arrow.Visibility = Visibility.Collapsed;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 4:
                    randomIndex = random.Next(2);//forbid+wild
                    if (randomIndex == 1)
                    {
                        randomIndex = 4;
                    }
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Collapsed;
                    left_arrow.Visibility = Visibility.Collapsed;
                    Left.Visibility = Visibility.Collapsed;
                    left_arrow.Visibility = Visibility.Collapsed;
                    Right.Visibility = Visibility.Collapsed;
                    right_arrow.Visibility = Visibility.Collapsed;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 5:
                    randomIndex = random.Next(2);//forbid+left(full screen)
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Collapsed;
                    right_arrow.Visibility = Visibility.Collapsed;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    maxX = ImageGrid.ActualWidth - RandomImage.Width;
                    maxY = ImageGrid.ActualWidth / 1.7 - RandomImage.Height;
                    left = random.NextDouble() * maxX;
                    top = random.NextDouble() * maxY;
                    RandomImage.Margin = new Thickness(left, top, 0, 0);
                    break;

                case 6:
                    randomIndex = random.Next(4);//forbid+left+wild
                    if (randomIndex == 2 || randomIndex == 3)
                    {
                        randomIndex = 4;
                    }
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Collapsed;
                    right_arrow.Visibility = Visibility.Collapsed;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 7:
                    randomIndex = random.Next(4);//forbid+left+wild(full screen)
                    if (randomIndex == 2 || randomIndex == 3)
                    {
                        randomIndex = 4;
                    }
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Collapsed;
                    right_arrow.Visibility = Visibility.Collapsed;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    maxX = ImageGrid.ActualWidth - RandomImage.Width;
                    maxY = ImageGrid.ActualWidth / 1.7 - RandomImage.Height;
                    left = random.NextDouble() * maxX;
                    top = random.NextDouble() * maxY;
                    RandomImage.Margin = new Thickness(left, top, 0, 0);
                    break;

                case 8:
                    randomIndex = random.Next(3);//forbid+left+right
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Visible;
                    right_arrow.Visibility = Visibility.Visible;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 9:
                    randomIndex = random.Next(3);//forbid+left+right(full screen)
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Visible;
                    right_arrow.Visibility = Visibility.Visible;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    maxX = ImageGrid.ActualWidth - RandomImage.Width;
                    maxY = ImageGrid.ActualWidth / 1.7 - RandomImage.Height;
                    left = random.NextDouble() * maxX;
                    top = random.NextDouble() * maxY;
                    RandomImage.Margin = new Thickness(left, top, 0, 0);
                    break;

                case 10:
                    randomIndex = random.Next(6);//forbid+left+right+wild
                    if (randomIndex > 2)
                    {
                        randomIndex = 4;
                    }
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Visible;
                    right_arrow.Visibility = Visibility.Visible;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 11:
                    randomIndex = random.Next(6);//forbid+left+right+wild(full screen)
                    if (randomIndex > 2)
                    {
                        randomIndex = 4;
                    }
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Visible;
                    right_arrow.Visibility = Visibility.Visible;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    maxX = ImageGrid.ActualWidth - RandomImage.Width;
                    maxY = ImageGrid.ActualWidth / 1.7 - RandomImage.Height;
                    left = random.NextDouble() * maxX;
                    top = random.NextDouble() * maxY;
                    RandomImage.Margin = new Thickness(left, top, 0, 0);
                    break;

                case 12:
                    randomIndex = random.Next(4);//forbid+left+right+forward
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Visible;
                    right_arrow.Visibility = Visibility.Visible;
                    forward.Visibility = Visibility.Visible;
                    Forward.Visibility = Visibility.Visible;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 13:
                    randomIndex = random.Next(4);//forbid+left+right+forward(full screen)
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Visible;
                    right_arrow.Visibility = Visibility.Visible;
                    forward.Visibility = Visibility.Visible;
                    Forward.Visibility = Visibility.Visible;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    maxX = ImageGrid.ActualWidth - RandomImage.Width;
                    maxY = ImageGrid.ActualWidth / 1.7 - RandomImage.Height;
                    left = random.NextDouble() * maxX;
                    top = random.NextDouble() * maxY;
                    RandomImage.Margin = new Thickness(left, top, 0, 0);
                    break;

                case 14:
                    randomIndex = random.Next(8);//forbid+left+right+wild
                    if (randomIndex > 3)
                    {
                        randomIndex = 4;
                    }
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Visible;
                    right_arrow.Visibility = Visibility.Visible;
                    forward.Visibility = Visibility.Visible;
                    Forward.Visibility = Visibility.Visible;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 15:
                    randomIndex = random.Next(8);//forbid+left+right+wild(full screen)
                    if (randomIndex > 2)
                    {
                        randomIndex = 4;
                    }
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Visible;
                    right_arrow.Visibility = Visibility.Visible;
                    forward.Visibility = Visibility.Visible;
                    Forward.Visibility = Visibility.Visible;
                    RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[randomIndex]}", UriKind.Relative));
                    maxX = ImageGrid.ActualWidth - RandomImage.Width;
                    maxY = ImageGrid.ActualWidth / 1.7 - RandomImage.Height;
                    left = random.NextDouble() * maxX;
                    top = random.NextDouble() * maxY;
                    RandomImage.Margin = new Thickness(left, top, 0, 0);
                    break;
                default:
                    // By default, undefined difficulty levels can be handled
                    break;
            }
        }

        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Down && e.Key != Key.Up && e.Key != Key.Left && e.Key != Key.Right)
                return;
            bool isCorrect = false;  // Declare and initialize isCorrect variable
            imageDisplayTimer?.Stop();
            displayTimer?.Stop(); // Stop the timer

            

            if (RandomImage.Source == null)
            {
                remainingTime = 0;
                decrease++;
                overtime++;
                if (IS_BEEP)
                    PlaySound(wrong_wav);
                if (IS_SCREEN)
                    _ = ChangeBorderColor();
                AdjustDifficulty();
                ShowRandomImage();
                return;
            }
            else if (RandomImage.Source != null)
            {
                if (randomIndex != 4)
                {
                    counterA++;
                }
                else if (randomIndex == 4)
                {
                    counterB++;
                }
                if (e.Key == Key.Down && randomIndex == 0)
                {
                    isCorrect = true;  // If the keys are correct, set isCorrect for true
                }
                else if (e.Key == Key.Left && randomIndex == 1)
                {
                    isCorrect = true;  // If the keys are correct, set isCorrect for true
                }
                else if (e.Key == Key.Right && randomIndex == 2)
                {
                    isCorrect = true;  // If the keys are correct, set isCorrect for true
                }
                else if (e.Key == Key.Up && randomIndex == 3)
                {
                    isCorrect = true;  // If the keys are correct, set isCorrect for true
                }
            }
            if (!isCorrect)
            {
                wrong++;
                if (IS_BEEP)
                    PlaySound(wrong_wav);
                if (IS_SCREEN)
                    _ = ChangeBorderColor();
            }
            else if (IS_BEEP)
            {
                PlaySound(correct_wav);
            }
            if (isCorrect)
            {
                increase++;
            }
            else
            {
                decrease++;
            }
            //timecount += MAX_REACT / 1000 - remainingTime;
            //press++;

            endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime; // Calculate the answer time            
            timecount += (int)duration.TotalMilliseconds;
            press++;

            remainingTime = 0;
            // Adjust the difficulty
            AdjustDifficulty();
            ShowRandomImage();
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

        private void AdjustDifficulty()
        {
            if (increase >= 30 && hardness <= MAX_HARDNESS)
            {
                // Increase difficulty
                hardness++;
                increase = 0;
                decrease = 0;
            }
            else if (decrease >= 30 && hardness > 1)
            {
                // Reduce difficulty
                increase = 0; decrease = 0;
                hardness--;
            }
            else if (hardness == 1 && decrease >= 30)
            {
                increase = 0; decrease = 0;
            }
            else if (hardness == MAX_HARDNESS && increase >= 30)
            {
                increase = 0; decrease = 0;
            }
            if (hardness > max_hardness)
                max_hardness = hardness;
            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
            RightStatisticsAction?.Invoke(increase, 30);
            WrongStatisticsAction?.Invoke(decrease, 30);
        }
    }

    public partial class Response_behavior : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
            random = new Random();
            //---------------------------------------------------------------------------------------------
            var baseParameter = BaseParameter;
            if (baseParameter.ProgramModulePars != null && baseParameter.ProgramModulePars.Any())
            {
                Debug.WriteLine("ProgramModulePars Loaded data:");

                // Traversal ProgramModulePars Print out each parameter
                foreach (var par in baseParameter.ProgramModulePars)
                {
                    /*Debug.WriteLine($"ProgramId: {par.ProgramId}, ModuleParId: {par.ModuleParId}, Value: {par.Value}, TableId: {par.TableId}");*/
                    if (par.ModulePar.ModuleId == baseParameter.ModuleId)
                    {
                        switch (par.ModuleParId) // Complete assignment
                        {
                            case 152://grade
                                hardness = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"HARDNESS: {hardness}");
                                break;
                            case 11: // Treatment time 
                                TRAIN_TIME = par.Value.HasValue ? (int)par.Value.Value * 60 : 30;
                                Debug.WriteLine($"TRAIN_TIME={TRAIN_TIME}");
                                break;
                            case 15: // Stimulation interval
                                STIMULI_INTERVAL = par.Value.HasValue ? (int)par.Value.Value : 3;
                                STIMULI_INTERVAL = STIMULI_INTERVAL * 1000;
                                Debug.WriteLine($"Stimulation interval{STIMULI_INTERVAL}");
                                break;
                            case 17: // Maximum reaction time
                                MAX_REACT = par.Value.HasValue ? (int)par.Value.Value : 3;
                                MAX_REACT = MAX_REACT * 1000;
                                Debug.WriteLine($"Maximum reaction time{MAX_REACT}");
                                break;
                            case 19: // Visual feedback
                                IS_SCREEN = par.Value == 1;
                                Debug.WriteLine($"Whether to hear feedback ={IS_BEEP}");
                                break;
                            case 20: // Auditory feedback
                                IS_BEEP = par.Value == 1;
                                Debug.WriteLine($"Is it visual feedback? ={IS_SCREEN}");
                                break;
                            // Add other things that need to be processed ModuleParId
                            default:
                                Debug.WriteLine($"Unprocessed ModuleParId: {par.ModuleParId}");
                                break;
                        }
                    }
                }
            }
            else
            {
                Debug.WriteLine("No data");
            }

            traintime = TRAIN_TIME;
            remainingTime = MAX_REACT / 1000;
            counterA = 0;
            counterB = 0;
            wrong = 0;
            overtime = 0;
            ignore = 0;
            Left.Visibility = Visibility.Collapsed;
            left_arrow.Visibility = Visibility.Collapsed;
            Right.Visibility = Visibility.Collapsed;
            right_arrow.Visibility = Visibility.Collapsed;
            forward.Visibility = Visibility.Collapsed;
            Forward.Visibility = Visibility.Collapsed;

            imageDisplayTimer = new DispatcherTimer();
            imageDisplayTimer.Tick += OnImageDisplayTimerElapsed;
            imageDisplayTimer?.Stop();

            displayTimer = new DispatcherTimer(); // initialization wildAnimalTimer
            displayTimer.Interval = TimeSpan.FromSeconds(1);
            displayTimer.Tick += OnDisplayTimerElapsed; // Bind Tick event
            displayTimer?.Stop(); // Stop the timer at initial

            trainingTimer = new DispatcherTimer(); // initialization trainingTimer
            trainingTimer.Interval = TimeSpan.FromSeconds(1); // Triggered once every second
            trainingTimer.Tick += OnTrainingTimerElapsed; // Bind Tick event

            // Calling delegate
            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
            RightStatisticsAction?.Invoke(0, 30);
            WrongStatisticsAction?.Invoke(0, 30);
        }

        protected override async Task OnStartAsync()
        {
            trainingTimer.Start(); // Start the training timer
            Forbid.Source = new BitmapImage(new Uri($@"../{imagePaths[0]}", UriKind.Relative));
            Left.Source = new BitmapImage(new Uri($@"../{imagePaths[1]}", UriKind.Relative));
            Right.Source = new BitmapImage(new Uri($@"../{imagePaths[2]}", UriKind.Relative));
            Forward.Source = new BitmapImage(new Uri($@"../{imagePaths[3]}", UriKind.Relative));
            ShowRandomImage();
            OnGameStart();
            // Calling delegate
            VoiceTipAction?.Invoke("Please remember and be familiar with the keys corresponding to the logo on the screen, and press the corresponding keys on the keyboard after the logo appears.");
            SynopsisAction?.Invoke("Test question description information");
            RuleAction?.Invoke("Please remember and be familiar with the keys corresponding to the logo on the screen, and press the corresponding keys on the keyboard after the logo appears.");//Add code, call function, display the text under the digital person

        }

        protected override async Task OnStopAsync()
        {
            StopAllTimers();
        }

        protected override async Task OnPauseAsync()
        {
            StopAllTimers();
        }

        protected override async Task OnNextAsync()
        {
            ShowRandomImage();
            // Calling delegate
            VoiceTipAction?.Invoke("Test returns voice command information");
            SynopsisAction?.Invoke("Test question description information");
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new Reaction_behavior_explanation();
        }

        private int GetCorrectNum()
        {   // Ca +CB - wrong - ignore - overtimeI didn't find itovertime
            int correct = counterA + counterB - wrong - ignore;
            if (correct > 0)
            {
                return correct;
            }
            else
            {
                return 0;
            }
        }
        private int GetWrongNum()
        {
            return wrong + overtime;
        }
        private int GetcounterA()
        {
            return counterA;
        }
        private int GetcounterB()
        {
            return counterB;
        }

        private int GetIgnoreNum()
        {
            return ignore;
        }
        private int GetovertimeNum()
        {
            return overtime;
        }
        private double Timeaverage()
        {
            return timecount / press;
        }

        private double CalculateAccuracy()
        {
            int total = counterA + counterB + overtime;
            int a = GetCorrectNum();
            Debug.WriteLine($"When calculating, the total number is{total}, the correct number is{a}");
            double accuracy = (double)GetCorrectNum() / total;  // Convert to double type
            // ----------------------------The problem is thatC# Dividing integers will remove the decimal part, so you have to force additiondoubleNot changing types
            return total > 0 ? Math.Round(accuracy, 2) : 0;
        }
        private async Task updateDataAsync()
        {
            var baseParameter = BaseParameter;

            var program_id = baseParameter.ProgramId;
            Crs_Db2Context db = baseParameter.Db;

            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    await Task.Run(async () =>
                    {
                        //Only the highest level of reports are required
                        for (int lv = max_hardness; lv <= max_hardness; lv++)
                        {
                            // Get data at the current difficulty level
                            int wrongCount = GetWrongNum();
                            int ignoreCount = GetIgnoreNum();
                            int counterA = GetcounterA();
                            int counterB = GetcounterB();
                            int counterAB = counterA + counterB;
                            //if (counterAB == 0)
                            //{
                            //    // If all data is 0, skip this difficulty level
                            //    Debug.WriteLine($"Difficulty level {lv}: No data, skip.");
                            //    continue;
                            //}
                            int overtime = GetovertimeNum();
                            int wrongall = wrongCount + ignoreCount;
                            int correctall = counterA + counterB - wrong - ignore - overtime;
                            double timeaverage = Timeaverage();
                            timeaverage =Math.Round(timeaverage, 2);
                            int correctCount = GetCorrectNum();
                            double accuracy = CalculateAccuracy();
                            accuracy = Math.Round(accuracy, 2);
                            // create Result Record
                            var newResult = new Result
                            {
                                ProgramId = program_id, // program_id
                                Report = "Response behavior",
                                Eval = false,
                                Lv = lv, // Current difficulty level
                                ScheduleId = BaseParameter.ScheduleId ?? null// Assumption Schedule_id, can be replaced with the actual value
                            };

                            db.Results.Add(newResult);
                            await db.SaveChangesAsync();

                            // get result_id
                            int result_id = newResult.ResultId;

                            // create ResultDetail Object List
                            var resultDetails = new List<ResultDetail>
                            {
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 0,
                                    ValueName = "grade",
                                    Value = max_hardness ,
                                    Maxvalue = 15,
                                    Minvalue = 1,
                                    ModuleId = BaseParameter.ModuleId //  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 6,
                                    ValueName = "Stimulate",
                                    Value = counterAB,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 7,
                                    ValueName = "Stimulus related",
                                    Value = counterA,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 8,
                                    ValueName = "Stimulation is not related",
                                    Value = counterB,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "Correct rate（%）",
                                    Charttype = "Bar chart" ,
                                    Value = accuracy * 100, // Stored as a percentage
                                    Maxvalue = 100,
                                    Minvalue = 0,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                 new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 2,
                                    ValueName = "correct total",
                                    Value = correctCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 3,
                                    ValueName = "mistake total",
                                    Value = wrongall,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 4,
                                    ValueName = "mistake keyboard",
                                    Value = wrongCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 5,
                                    ValueName = "Leaked",
                                    Value = ignoreCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 9,
                                    ValueName = "Average reaction time(ms)",
                                    Value = timeaverage,
                                    ModuleId = BaseParameter.ModuleId
                                },
                            };

                            // insert ResultDetail data
                            db.ResultDetails.AddRange(resultDetails);
                            await db.SaveChangesAsync();

                            // Output each ResultDetail Object data
                            Debug.WriteLine($"Difficulty level {lv}:");
                            foreach (var detail in resultDetails)
                            {
                                Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId}");
                            }
                        }

                        // Submit transactions
                        await transaction.CommitAsync();
                        Debug.WriteLine("Insert successfully");
                    });
                }
                catch (Exception ex)
                {
                    // Roll back transactions
                    await transaction.RollbackAsync();
                    Debug.WriteLine(ex.ToString());
                }
            }
        }
    }
}
