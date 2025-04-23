using crs.core.DbModels;
using crs.core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Media;

namespace crs.game.Games
{
    /// <summary>
    /// Visual repair training.xaml Interaction logic
    /// </summary>
    public partial class Visual_repair_training : BaseUserControl
    {   // Parameters to be uploaded to the report
        private int totalShapeChanges = 0; // The number of times the fixed point in the center of the screen changes
        private int successfulFocusPresses = 0; // The number of times the patient successfully responds to changes in the center of the screen
        private int successfulFocusPercentage = 0;// Percentage of patients' successful response to changes in the center of the screen
        private int totalSquareFlashes = 0; // The number of times the white square stimulation flashes
        private int successfulSquarePresses = 0; // The number of times the patient successfully responded to white square stimulation
        private int successfulSquarePercentage = 0;// Percentage of patients successfully responding to white square stimuli



        private Random random = new Random();
        private double verticalRangeMin = -180;
        private double verticalRangeMax = 180;
        private double horizontalRangeMin = 0;
        private double horizontalRangeMax = 1800;
        private DispatcherTimer trainingTimer;
        private int train_time;

        private Stopwatch stopwatch;
        private DispatcherTimer checkTimer;
        private int restInterval = 120; // Rest for 10 seconds every 120 seconds
        private int restDuration = 10; // Rest duration（Second）
        private bool showRestImage;
        private bool isResting = false;
        private Direction selectedVisionDirection; // Default direction
        private double shapeChangeElapsedTime = 0; // Used to track the time of shape changes
        private double currentShapeChangeInterval = 0; // The interval between current shape changes
        private bool isShapeChanged = false;
        private bool isFirstPhase = true; // Identify the current training phase
        private Rectangle blinkSquare; // Flashing blocks
        private Polygon triangle;
        private double blinkElapsedTime = 0; // Variables used to track time
        private bool isBlinkVisible = false; // Is the current block visible?

        // Manage block movement
        private int moveCount = 0; // Record the number of horizontal movements of the square
        private double initialTopMargin = 0; // Record the initial upper margin of the block
        private bool canMoveSquare = true; // Used to control the movement of blocks in the second stage

        // Parameters to be uploaded to the report
        private Stopwatch reactionStopwatch = new Stopwatch(); // Used to measure user response time


        private DispatcherTimer restTimer;
        private int remainingTime;
        private Queue<bool> recentResults = new Queue<bool>(5);
        private int TrainDirection = 1;//Use 1-6.Direction6 directions
        public enum Direction
        {
            LeftUp,
            RightUp,
            Left,
            Right,
            LeftDown,
            RightDown
        }

        private bool IfAudioFeedBack = true;
        private bool IfVisualFeedBack = true;

        private void MoveBlinkSquareRandomly()
        {
            double newLeftMargin = random.NextDouble() * (horizontalRangeMax - horizontalRangeMin) + horizontalRangeMin;
            double newTopMargin = random.NextDouble() * (verticalRangeMax - verticalRangeMin) + verticalRangeMin;

            blinkSquare.Margin = new Thickness(newLeftMargin, newTopMargin, 0, 0);
        }        
        private void CheckTimer_Tick(object sender, EventArgs e)
        {
            if (!isResting && stopwatch.Elapsed.TotalSeconds >= restInterval)
            {
                isResting = true;
                ShowRestWindow(); // Call the method to display the rest window
            }
        }
        private async void ShowRestWindow()
        {
            stopwatch.Stop(); // Stop timing
            Rest(showRestImage);

            // Continue training after rest
            await Task.Delay(TimeSpan.FromSeconds(restDuration)); // Simulate a 10-second break

            // Recover the timer after the rest and continue training
            stopwatch.Restart(); // Reset and start Stopwatch
            isResting = false;
        }
        private void SetInitialPosition(Direction direction)
        {
            switch (direction)
            {
                case Direction.LeftUp:
                    blinkSquare.Margin = new Thickness(-60, -60, 0, 0);
                    break;
                case Direction.RightUp:
                    blinkSquare.Margin = new Thickness(60, -60, 0, 0);
                    break;
                case Direction.Left:
                    blinkSquare.Margin = new Thickness(-60, 0, 0, 0);
                    break;
                case Direction.Right:
                    blinkSquare.Margin = new Thickness(60, 0, 0, 0);
                    break;
                case Direction.LeftDown:
                    blinkSquare.Margin = new Thickness(-60, 60, 0, 0);
                    break;
                case Direction.RightDown:
                    blinkSquare.Margin = new Thickness(60, 60, 0, 0);
                    break;
            }
        }
        private void TrainingTimer_Tick(object sender, EventArgs e)
        {
            train_time--; // Countdown to training time

            // Calling delegate
            TimeStatisticsAction?.Invoke(train_time, 0);
            if (train_time <= 0)
            {
                // Stop all timers
                trainingTimer.Stop();
                checkTimer.Stop();
                stopwatch.Stop();
                reactionStopwatch.Stop();
                currentShapeChangeInterval = 10000;
                //viewModel.UpdateResults(totalShapeChanges, successfulFocusPresses, totalSquareFlashes, successfulSquarePresses);
                // Create a report window
                //var reportWindow = new reportWindow(viewModel);

                OnGameEnd();
            }

        }
        private void InitializeTraining()
        {
            InitializeComponent();

            currentShapeChangeInterval = GetRandomInterval(4, 7); // Phase 1 4-6 Second


            // Used at initializationXAMLdefined inFocusPoint

            // Initialize a triangle object
            triangle = new Polygon
            {
                Points = new PointCollection(new Point[]
                {
                    new Point(20, 0),  // vertex
                    new Point(0, 34.64), // Lower left corner
                    new Point(40, 34.64) // Lower right corner
                }),
                Fill = new SolidColorBrush(Color.FromRgb(184, 134, 11)), // Dark yellow
                Width = 40,
                Height = 34.64, // The height of an equilateral triangle
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Visibility = Visibility.Hidden  // The initial state is hidden
            };

            TrainGrid.Children.Add(triangle); // Add toGridbut hidden at the beginning

            // Initialize the block and set its position, it is not visible by default
            blinkSquare = new Rectangle
            {
                Width = 30,
                Height = 30,
                Fill = Brushes.White,
                Visibility = Visibility.Hidden // The initial state is invisible
            };

            TrainGrid.Children.Add(blinkSquare);

            // Set the initial position according to the passed direction parameters
            SetInitialPosition(selectedVisionDirection);
            CompositionTarget.Rendering += OnRendering;
        }
        private double GetRandomInterval(int minSeconds, int maxSeconds)
        {
            Random rand = new Random();
            return rand.Next(minSeconds, maxSeconds);
        }
        private void OnRendering(object sender, EventArgs e)
        {
            shapeChangeElapsedTime += 1.0 / 60.0; // Assume that 60 frames are rendered per second

            if (shapeChangeElapsedTime >= currentShapeChangeInterval)
            {
                if (!isShapeChanged)
                {
                    // Hide the circle and display the triangle
                    FocusPoint.Visibility = Visibility.Hidden;
                    triangle.Visibility = Visibility.Visible;
                    isShapeChanged = true;

                    // Increase the total number of shape changes
                    totalShapeChanges++;

                    // Start timed user response time
                    reactionStopwatch.Restart();

                    // 0.Return to a circle after 5 seconds
                    currentShapeChangeInterval = 0.5;
                }
                else
                {
                    // Hide the triangle and restore it to a circle
                    triangle.Visibility = Visibility.Hidden;
                    FocusPoint.Visibility = Visibility.Visible;

                    // Adjust the next change interval according to the stage
                    currentShapeChangeInterval = isFirstPhase ? GetRandomInterval(4, 7) : GetRandomInterval(15, 20);
                }

                shapeChangeElapsedTime = 0.0; // Reset the timer
            }
            // Check whether the user timed out and did not press the key
            if (isShapeChanged && reactionStopwatch.Elapsed.TotalSeconds > 1.5)
            {
                reactionStopwatch.Stop();
                HandleFailure(); // The user did not press the key, and the processing was failed
            }

            // Control blocks flash（Only in the second stage）
            if (!isFirstPhase)
            {
                blinkElapsedTime += 1.0 / 60.0;

                if (isBlinkVisible)
                {
                    if (blinkElapsedTime >= 0.2) // Show 0.Hide in 2 seconds
                    {
                        blinkSquare.Visibility = Visibility.Hidden;
                        isBlinkVisible = false;
                        blinkElapsedTime = 0.0;
                        canMoveSquare = true; // The square has been flashing, allowing the next move
                    }
                }
                else
                {
                    if (blinkElapsedTime >= 2.3) // Every 2.Flash once in 5 seconds (2.3 + 0.2 = 2.5)
                    {
                        blinkSquare.Visibility = Visibility.Visible;
                        isBlinkVisible = true;
                        blinkElapsedTime = 0.0;

                        // Increase the number of block flicker times
                        totalSquareFlashes++;
                    }
                }
            }
        }
        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Debug.WriteLine("Detected Enter Press the key");

            if (e.Key == Key.Enter && isShapeChanged)
            {
                // Stop timing and get user response time
                reactionStopwatch.Stop();
                double reactionTime = reactionStopwatch.Elapsed.TotalSeconds;

                if (reactionTime >= 0.15 && reactionTime <= 1.5)
                {
                    // The user pressed in the correct time window Enter Key, determine successful
                    successfulFocusPresses++; // Increase success times
                    recentResults.Enqueue(true);
                    if (IfAudioFeedBack) PlayWav(CorrectSoundPath);
                    if (IfVisualFeedBack) ShowFeedbackImage(CorrectImage);
                    if (recentResults.Count > 5)
                    {
                        recentResults.Dequeue();
                    }
                    int totalCorrect = recentResults.Count(result => result); // statistics true The number of correct answers
                    int totalWrong = recentResults.Count(result => !result);
                    RightStatisticsAction?.Invoke(totalCorrect, 5);
                    WrongStatisticsAction?.Invoke(totalWrong, 5);
                    isShapeChanged = false;

                    triangle.Visibility = Visibility.Hidden;  // Hide triangle
                    FocusPoint.Visibility = Visibility.Visible;  // Show circle

                    if (isFirstPhase)
                    {
                        // Entering the second stage
                        isFirstPhase = false;
                        currentShapeChangeInterval = GetRandomInterval(15, 20); // Second phase interval adjustment
                        shapeChangeElapsedTime = 0.0; // Reset the timer
                    }
                }
                else
                {
                    // The user did not press the correct time window Enter Key, judgment failed
                    HandleFailure();
                }
            }
            else if (e.Key == Key.Enter && !isFirstPhase && canMoveSquare)
            {
                // Pressed successfully Enter key, move blocks
                MoveBlinkSquare();
                canMoveSquare = false; // Prohibited subsequent Enter Decide until the block flashes again
                // Record user successfully pressed after flashing on white squareEnterNumber of times
                successfulSquarePresses++;
                recentResults.Enqueue(true);
                if (IfAudioFeedBack) PlayWav(CorrectSoundPath);
                if (IfVisualFeedBack) ShowFeedbackImage(CorrectImage);
                if (recentResults.Count > 5)
                {
                    recentResults.Dequeue();
                }
                int totalCorrect = recentResults.Count(result => result); // statistics true The number of correct answers
                int totalWrong = recentResults.Count(result => !result);
                RightStatisticsAction?.Invoke(totalCorrect, 5);
                WrongStatisticsAction?.Invoke(totalWrong, 5);
            }
        }
        private void HandleFailure()
        {
            // Reset to circle in case of failure
            isShapeChanged = false;
            triangle.Visibility = Visibility.Hidden;  // Hide triangle
            FocusPoint.Visibility = Visibility.Visible;  // Show circle
            recentResults.Enqueue(false);
            if (recentResults.Count > 5)
            {
                recentResults.Dequeue();
            }
            int totalCorrect = recentResults.Count(result => result); // statistics true The number of correct answers
            int totalWrong = recentResults.Count(result => !result);
            RightStatisticsAction?.Invoke(totalCorrect, 5);
            WrongStatisticsAction?.Invoke(totalWrong, 5);

            currentShapeChangeInterval = isFirstPhase ? GetRandomInterval(4, 7) : GetRandomInterval(15, 20);
            shapeChangeElapsedTime = 0.0; // Reset the timer            
        }
        private void MoveBlinkSquare()
        {
            if (selectedVisionDirection == Direction.Left || selectedVisionDirection == Direction.Right)
            {
                // If the direction is left and right, the blocks will be moved randomly
                MoveBlinkSquareRandomly();
            }
            else
            {
                moveCount++;

                double currentLeftMargin = blinkSquare.Margin.Left;
                double currentTopMargin = blinkSquare.Margin.Top;

                switch (selectedVisionDirection)
                {
                    case Direction.RightDown:
                        if (moveCount < 5)
                        {
                            blinkSquare.Margin = new Thickness(currentLeftMargin + 60, currentTopMargin, 0, 0);
                        }
                        else
                        {
                            moveCount = 0;
                            initialTopMargin += 60;
                            currentLeftMargin -= 180; // Move the starting point of each row to the bottom right
                            blinkSquare.Margin = new Thickness(currentLeftMargin, initialTopMargin, 0, 0);
                        }
                        break;

                    case Direction.LeftDown:
                        if (moveCount < 5)
                        {
                            blinkSquare.Margin = new Thickness(currentLeftMargin - 60, currentTopMargin, 0, 0);
                        }
                        else
                        {
                            moveCount = 0;
                            initialTopMargin += 60;
                            currentLeftMargin += 180; // Each row starts to the lower left
                            blinkSquare.Margin = new Thickness(currentLeftMargin, initialTopMargin, 0, 0);
                        }
                        break;

                    case Direction.RightUp:
                        if (moveCount < 5)
                        {
                            blinkSquare.Margin = new Thickness(currentLeftMargin + 60, currentTopMargin, 0, 0);
                        }
                        else
                        {
                            moveCount = 0;
                            initialTopMargin -= 60;
                            currentLeftMargin -= 180; // Move the starting point of each row to the upper right
                            blinkSquare.Margin = new Thickness(currentLeftMargin, initialTopMargin, 0, 0);
                        }
                        break;

                    case Direction.LeftUp:
                        if (moveCount < 5)
                        {
                            blinkSquare.Margin = new Thickness(currentLeftMargin - 60, currentTopMargin, 0, 0);
                        }
                        else
                        {
                            moveCount = 0;
                            initialTopMargin -= 60;
                            currentLeftMargin += 180; // Move the starting point of each row to the upper left
                            blinkSquare.Margin = new Thickness(currentLeftMargin, initialTopMargin, 0, 0);
                        }
                        break;
                }
            }
        }
        private void Button_Click_Continue(object sender, RoutedEventArgs e)
        {
            TipGrid.Visibility = Visibility.Collapsed;
            RestGrid.Visibility = Visibility.Collapsed;
            TrainGrid.Visibility = Visibility.Visible;
        }
        private void Rest(bool showRestImage, int restDuration = 10)
        {
            TrainGrid.Visibility = Visibility.Collapsed;
            TipGrid.Visibility = Visibility.Collapsed;
            RestGrid.Visibility = Visibility.Visible;
            remainingTime = restDuration;

            // according to showRestImage Parameter setting interface content
            if (showRestImage)
            {
                RestLabel.Visibility = Visibility.Hidden;  // Hide text
                RestImage.Visibility = Visibility.Visible; // Show pictures
                RestImage.Source = new BitmapImage(new Uri("Visual repair training.jpg", UriKind.Relative));
            }
            else
            {
                RestLabel.Visibility = Visibility.Visible;  // Show text
                RestImage.Visibility = Visibility.Hidden;   // Hide pictures
            }

            // Set and start the timer
            restTimer = new DispatcherTimer();
            restTimer.Interval = TimeSpan.FromSeconds(1);
            restTimer.Tick += RestTimer_Tick;
            restTimer.Start();
        }
        private void RestTimer_Tick(object sender, EventArgs e)
        {
            remainingTime--;

            if (remainingTime <= 0)
            {
                restTimer.Stop();
                RestGrid.Visibility = Visibility.Collapsed;
            }

            RestLabel.Content = $"rest {remainingTime} Second"; // Update the displayed text
        }

        /*LJN
 Added resources for visual and sound feedback
 */
        private SoundPlayer soundPlayer; // Used to sing
        public string ErrorSoundPath;//The wrong sound file path, inOnStartAsync()Medium configuration
        public string CorrectSoundPath;//The correct sound file path is inOnStartAsync()Medium configuration
        private int StopDurations = 1000; // Stop time,ms

        private string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory; // The absolute path to the current project
        private string ResourcesPath = System.IO.Path.Combine("Resources", "Word memory");//The fixed path used here is the memory path of word, which can be changed later.

        private void PlayWav(string filePath)
        {//Play locallywavdocument
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
        {//Image showing feedback
            image.Visibility = Visibility.Visible;

            // Delay the specified time（For example, 1 second）
            await Task.Delay(StopDurations);

            image.Visibility = Visibility.Collapsed;
        }
    }
    public partial class Visual_repair_training : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
            InitializeTraining();
            /*LJN
            Configure the path to feedback resources             
             */
            CorrectSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");
            // for Image Control loading picture Source
            CorrectImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Correct.png"), UriKind.RelativeOrAbsolute));
            ErrorImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Error.png"), UriKind.RelativeOrAbsolute));



            TrainGrid.Visibility = Visibility.Collapsed;
            RestGrid.Visibility = Visibility.Collapsed;
            TipGrid.Visibility = Visibility.Visible;



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
                            case 174: // Treatment time 
                                train_time = par.Value.HasValue ? (int)par.Value.Value : 60;
                                Debug.WriteLine($"TRAIN_TIME={train_time}");
                                break;
                            case 316://Training location
                                TrainDirection = par.Value.HasValue ? (int)par.Value.Value : 1;
                                break;
                                /*//These directions are integrated into one direction
                            case 225: // Is the control direction upper left
                                      // Assumptions par.Value yes double? Type, judge based on the value
                                bool isLeftUp = par.Value.HasValue ? (par.Value.Value != 0) : false;
                                if (isLeftUp)
                                {
                                    selectedVisionDirection = Direction.LeftUp;
                                }
                                Debug.WriteLine($"IS_LEFT_UP={isLeftUp}");
                                break;
                            case 226: // Is the control direction upper right?
                                bool isRightUp = par.Value.HasValue ? (par.Value.Value != 0) : false;
                                if (isRightUp)
                                {
                                    selectedVisionDirection = Direction.RightUp;
                                }
                                Debug.WriteLine($"IS_RIGHT_UP={isRightUp}");
                                break;
                            case 227: // Whether the control direction is left
                                bool isLeft = par.Value.HasValue ? (par.Value.Value != 0) : false;
                                if (isLeft)
                                {
                                    selectedVisionDirection = Direction.Left;
                                }
                                Debug.WriteLine($"IS_LEFT={isLeft}");
                                break;
                            case 228: // Is the control direction right?
                                bool isRight = par.Value.HasValue ? (par.Value.Value != 0) : false;
                                if (isRight)
                                {
                                    selectedVisionDirection = Direction.Right;
                                }
                                Debug.WriteLine($"IS_RIGHT={isRight}");
                                break;
                            case 229: // Is the control direction lower left
                                bool isLeftDown = par.Value.HasValue ? (par.Value.Value != 0) : false;
                                if (isLeftDown)
                                {
                                    selectedVisionDirection = Direction.LeftDown;
                                }
                                Debug.WriteLine($"IS_LEFT_DOWN={isLeftDown}");
                                break;
                            case 230: // Is the control direction lower right
                                bool isRightDown = par.Value.HasValue ? (par.Value.Value != 0) : false;
                                if (isRightDown)
                                {
                                    selectedVisionDirection = Direction.RightDown;
                                }
                                Debug.WriteLine($"IS_RIGHT_DOWN={isRightDown}");
                                break;
                                */
                            case 243: // Control whether there are pictures during rest
                                      // The value read from the database is to determine whether it is non-zero. Non-zero means displaying the picture
                                bool isShowRestImage = par.Value.HasValue ? (par.Value.Value != 0) : false;

                                // Set according to the database value showRestImage parameter
                                showRestImage = isShowRestImage;

                                // Output debug information to confirm the read value
                                Debug.WriteLine($"showRestImage from database: {showRestImage}");
                                break;
                            case 265://Auditory feedback
                                IfAudioFeedBack = par.Value == 1;
                                break;
                            case 266://Visual feedback
                                IfVisualFeedBack = par.Value == 1;
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

            switch (TrainDirection)
            {//Here makesTrainDiectionThe value of the map to the direction
                case 1: selectedVisionDirection = Direction.LeftUp; break;
                case 2: selectedVisionDirection = Direction.RightUp; break;
                case 3: selectedVisionDirection = Direction.LeftDown; break;
                case 4: selectedVisionDirection = Direction.RightDown; break;
                case 5: selectedVisionDirection = Direction.Left; break;
                case 6: selectedVisionDirection = Direction.Right; break;
                default:MessageBox.Show("If you have any problems with your network, please contact the administrator"); break;
            }

            // Calling delegate
            LevelStatisticsAction?.Invoke(1,1);
            RightStatisticsAction?.Invoke(0, 5);
            WrongStatisticsAction?.Invoke(0, 5);
        }

        protected override async Task OnStartAsync()
        {
            // Set the initial block position
            SetInitialPosition(selectedVisionDirection);
            // Set the training timer
            trainingTimer = new DispatcherTimer();
            trainingTimer.Interval = TimeSpan.FromMinutes(1);
            trainingTimer.Tick += TrainingTimer_Tick;
            trainingTimer.Start();


            // initialization Stopwatch
            stopwatch = new Stopwatch();
            stopwatch.Start();

            // Initialize the periodic check time DispatcherTimer
            checkTimer = new DispatcherTimer();
            checkTimer.Interval = TimeSpan.FromSeconds(10); // Check the time every 10 seconds
            checkTimer.Tick += CheckTimer_Tick;
            checkTimer.Start();

            // Calling delegate
            VoiceTipAction?.Invoke("Please press the keyboard when the green dot changes or small white squares appear.OKkey.");
            SynopsisAction?.Invoke("There is now a fixed green dot in the center of the screen. As you watch it, it changes from time to time, and when you notice the change is happening, press theOKkey；When a small white square appears in the center of the screen, please press theOKkey.");
            RuleAction?.Invoke("There is now a fixed green dot in the center of the screen. As you watch it, it changes from time to time, and when you notice the change is happening, press theOKkey；When a small white square appears in the center of the screen, please press theOKkey.");//Add code, call function, display the text under the digital person

        }

        protected override async Task OnStopAsync()
        {
            trainingTimer.Stop();
            checkTimer.Stop();
            stopwatch.Stop();
            reactionStopwatch.Stop();
        }

        protected override async Task OnPauseAsync()
        {
            trainingTimer.Stop();
            checkTimer.Stop();
            stopwatch.Stop();
            reactionStopwatch.Stop();
        }

        protected override async Task OnNextAsync()
        {
            // Calling delegate
            VoiceTipAction?.Invoke("Please press the keyboard when the green dot changes or small white squares appear.OKkey.");
            SynopsisAction?.Invoke("There is now a fixed green dot in the center of the screen. As you watch it, it changes from time to time, and when you notice the change is happening, press theOKkey；When a small white square appears in the center of the screen, please press theOKkey.");
            RuleAction?.Invoke("There is now a fixed green dot in the center of the screen. As you watch it, it changes from time to time, and when you notice the change is happening, press theOKkey；When a small white square appears in the center of the screen, please press theOKkey.");
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new Explanation of visual repair training();
        }

        /*
        private int totalShapeChanges = 0; // The number of times the fixed point in the center of the screen changes
        private int successfulFocusPresses = 0; // The number of times the patient successfully responds to changes in the center of the screen
        private int successfulFocusPercentage = 0 // Percentage of patients' successful response to changes in the center of the screen
        private int totalSquareFlashes = 0; // The number of times the white square stimulation flashes
        private int successfulSquarePresses = 0; // The number of times the patient successfully responded to white square stimulation
        private int successfulSquarePercentage = 0 // Percentage of patients successfully responding to white square stimuli
        */
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
                            if (totalShapeChanges == 0 && successfulFocusPercentage == 0 && successfulFocusPresses == 0 && totalSquareFlashes ==0 
                                && successfulSquarePercentage ==0
                                && successfulSquarePresses==0)
                            {
                                // If all data is 0, skip this difficulty level
                                Debug.WriteLine($"No data, skip.");
                                //continue;
                            }
                            Debug.WriteLine("Do it hereResultInsert 1");
                            
                        // create Result Record

                        successfulFocusPercentage = (successfulFocusPresses / totalShapeChanges) * 100;
                        successfulSquarePercentage = (successfulSquarePresses / totalSquareFlashes) * 100;

                        var newResult = new Result
                            {
                                ProgramId = BaseParameter.ProgramId, // program_id
                                Report = "Visual repair training",
                                Eval = false,
                                Lv = null, // Current difficulty level
                                ScheduleId = BaseParameter.ScheduleId // Assumption Schedule_id, can be replaced with the actual value
                            };
                            Debug.WriteLine($"Deadline");
                            db.Results.Add(newResult);
                            await db.SaveChangesAsync();

                            // get result_id
                            int result_id = newResult.ResultId;

                            // create ResultDetail Object List

                            // Change the following to your corresponding report parameters-------------------------------------
                            var resultDetails = new List<ResultDetail>
                            {
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Stimulate",
                                    Value = totalSquareFlashes,
                                    ModuleId = BaseParameter.ModuleId //  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "correct Stimulate",
                                    Value = successfulSquarePresses,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "correct Stimulate%",
                                    Value = successfulSquarePercentage,
                                    Maxvalue = 100,
                                    Minvalue = 0,
                                    Charttype = "Bar chart" ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Fixed inspection",
                                    Value = totalShapeChanges,
                                    ModuleId = BaseParameter.ModuleId //  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "correct Fixed inspection",
                                    Value = successfulFocusPresses,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "correct Fixed check %",
                                    Value = successfulFocusPercentage,
                                    Maxvalue = 100,
                                    Minvalue = 0,
                                    Charttype = "Bar chart" ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                            };

                            // insert ResultDetail data
                            db.ResultDetails.AddRange(resultDetails);
                            await db.SaveChangesAsync();

                            // Output each ResultDetail Object data
                            //Debug.WriteLine($"Difficulty level {lv}:");
                            foreach (var detail in resultDetails)
                            {
                                Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId}");
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