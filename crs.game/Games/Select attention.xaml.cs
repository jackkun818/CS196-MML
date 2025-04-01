using crs.core;
using crs.core.DbModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace crs.game.Games
{
    /// <summary>
    /// Select attention.xaml Interaction logic
    /// </summary>
    public partial class Select attention : BaseUserControl
    {
        private string imagePath; // Store the found folder path
        private string[] imagePaths;
        private const int DELAY = 5000; // Waiting for 5000msShow next picture
        private const int MAX_GAME = 42;
        private BitmapImage targetImage;
        private BitmapImage leftImage;

        private string targetImagePath; // Save the path to the target image
        private List<BitmapImage> gameImages; // 42 pictures stored after the disruption
        private int attemptCount = 0;
        private Stopwatch stopwatch;
        private double total_time = 0;
        private int wrong = 0;
        private int skip = 0;
        private int score = 0;//Record only when the target object appears and press confirm data
        private CancellationTokenSource cancellationTokenSource; // Used to cancel loading tasks
        class TaskResult
        {
            public int TaskNumber { get; set; } // Task number
            public double? ReactionTime { get; set; } // Reaction time（Second）, nullable type
            public string Result { get; set; } // Task results: correct, wrong or missed
        }
        private List<TaskResult> taskResults = new List<TaskResult>();

        private DispatcherTimer gameTimer; // Timer
        private TimeSpan totalGameTime; // Total game time
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            totalGameTime = totalGameTime.Add(TimeSpan.FromSeconds(1)); // One second each time// Get the total number of seconds
            int totalSeconds = (int)totalGameTime.TotalSeconds;

            // Calling delegate
            TimeStatisticsAction?.Invoke(totalSeconds, totalSeconds);

        }
        private void InitializeImages()
        {
            // Clear the list to make sure there is no residual data
            gameImages = new List<BitmapImage>();

            // Find the image path
            string targetDirectory = FindImagePath();
            if (targetDirectory == null)
            {
                MessageBox.Show("The target folder is not found, please check the path.", "mistake", MessageBoxButton.OK, MessageBoxImage.Error);
                OnGameEnd();
                return;
            }

            // Load all pictures（Suppose there are 3 pictures）
            imagePaths = Directory.GetFiles(targetDirectory, "*.png", SearchOption.TopDirectoryOnly);

            if (imagePaths.Length < 2)
            {
                MessageBox.Show("The number of pictures is wrong, and at least 2 pictures are required.", "mistake", MessageBoxButton.OK, MessageBoxImage.Error);
                OnGameEnd();
                return;
            }

            // Select a target image
            LoadTargetImage(); // Randomly select a picture as the target object and store it in `targetImagePath`

            // Get the non-target image path
            var nonTargetImagesPaths = imagePaths.Where(path => path != targetImagePath).ToArray();

            // Add to 21 A picture of the target object
            for (int i = 0; i < 21; i++)
            {
                gameImages.Add(CreateBitmapImage(targetImagePath)); // Add a target image
            }

            // Add other pictures randomly until the total number is reached（For example, 42 pictures）
            Random rand = new Random();
            while (gameImages.Count < 42)
            {
                string randomNonTargetPath = nonTargetImagesPaths[rand.Next(nonTargetImagesPaths.Length)];
                gameImages.Add(CreateBitmapImage(randomNonTargetPath)); // Randomly add non-target images
            }

            // Chaotic order
            ShuffleGameImages();

            // Final check for target and non-target quantity
            int targetCount = gameImages.Count(img => img.UriSource.ToString() == new Uri(targetImagePath, UriKind.Absolute).ToString());
            int nonTargetCount = gameImages.Count - targetCount;


        }

        // Helper Method: Select the target image and set the path
        private void LoadTargetImage()
        {
            Random rand = new Random();
            int index = rand.Next(imagePaths.Length);
            targetImagePath = imagePaths[index];
            targetImage = new BitmapImage(new Uri(targetImagePath, UriKind.Absolute));
            TargetImage.Source = targetImage;
        }

        // Helper Method: Create a new one BitmapImage Example
        private BitmapImage CreateBitmapImage(string imagePath)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(imagePath, UriKind.Absolute);
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            return bitmapImage;
        }

        // Fisher-Yates The shuffle algorithm ensures complete randomness
        private void ShuffleGameImages()
        {
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            for (int i = gameImages.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                var temp = gameImages[i];
                gameImages[i] = gameImages[j];
                gameImages[j] = temp;
            }
        }

        public Select attention()
        {
            InitializeComponent();
        }

        private string FindImagePath()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            {
                string targetDirectory = Path.Combine(currentDirectory, "Select attention");
                if (Directory.Exists(targetDirectory))
                {
                    return targetDirectory;
                }
                return null;
            }
            {
                string targetDirectory = Path.Combine(currentDirectory, @"Games\Select attention");
                if (Directory.Exists(targetDirectory))
                {
                    return targetDirectory;
                }
                return null;
            }

        }

        /*
        private string FindImagePath()
        {
            string currentDirectory = Directory.GetCurrentDirectory();

            while (true)
            {
                // Check if the current directory exists“crs.game”Folders
                string targetParentDirectory = Path.Combine(currentDirectory, "crs.game");
                if (Directory.Exists(targetParentDirectory))
                {
                    string targetDirectory = Path.Combine(targetParentDirectory, "Select attention");
                    if (Directory.Exists(targetDirectory))
                    {
                        return targetDirectory; // Find the folder and return to the path
                    }
                }

                // Move up to parent directory
                DirectoryInfo parentDirectory = Directory.GetParent(currentDirectory);
                if (parentDirectory == null)
                {
                    break; // Arrive at the root directory and stop searching
                }
                currentDirectory = parentDirectory.FullName; // Update the current directory
            }

            return null; // No folder found
        }
        */

        private void LoadTargetImageSync()
        {
            // Randomly select an image as the target object
            Random rand = new Random();
            int index = rand.Next(imagePaths.Length);
            targetImagePath = imagePaths[index]; // Save the selected target image path
            targetImage = new BitmapImage(new Uri(targetImagePath, UriKind.Absolute));
            TargetImage.Source = targetImage;


        }

        private async void LoadNewLeftImage()
        {

            // If a loading task is currently running, cancel it
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;



            // Check if the maximum number of games is reached
            if (attemptCount >= gameImages.Count)
            {
                // Game end logic
                OnGameEnd();
                return;
            }

            // from `gameImages` Get pictures in order in the list（Make sure that each test is displayed in a different order）
            leftImage = gameImages[attemptCount];
            LeftImage.Source = leftImage;
            attemptCount++;

            // Start changing and make sure the image is randomly located within the interface
            // make sure Canvas The size of
            LeftImageCanvas.UpdateLayout();

            // Get Canvas and actual size of the picture
            double imageWidth = LeftImage.Width;
            double imageHeight = LeftImage.Height;
            double canvasWidth = LeftImageCanvas.ActualWidth;
            double canvasHeight = LeftImageCanvas.ActualHeight;

            // Prevent image width or height from exceeding Canvas The size of
            double maxX = Math.Max(0, canvasWidth - imageWidth);
            double maxY = Math.Max(0, canvasHeight - imageHeight);

            // Generate random positions
            Random rand = new Random();
            double randomX = rand.NextDouble() * maxX;
            double randomY = rand.NextDouble() * maxY;

            // Set the picture location
            Canvas.SetLeft(LeftImage, randomX);
            Canvas.SetTop(LeftImage, randomY);

            // Record time
            stopwatch = Stopwatch.StartNew();

            try
            {
                await Task.Delay(DELAY, token); // waitDELAYtime
            }
            catch (TaskCanceledException)
            {
                // The task is cancelled and returns directly
                return;
            }

            // Determine whether it is a target image, record missing
            if (leftImage.UriSource == targetImage.UriSource)
            {
                skip++;
                total_time += DELAY;
                taskResults.Add(new TaskResult
                {
                    TaskNumber = attemptCount,
                    ReactionTime = 0,
                    Result = "Leaked"
                });
            }

            // Update statistics
            RightStatisticsAction?.Invoke(score, 10);
            WrongStatisticsAction?.Invoke(wrong + skip, 10);

            // Recursively call to load a new image until the maximum number of times is reached
            if (attemptCount < MAX_GAME)
            {
                LoadNewLeftImage();
            }
            else
            {
                // Force trigger the game to end, making sure to end the game after the last picture
                OnGameEnd();
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            stopwatch.Stop();
            double reactionTime = stopwatch.Elapsed.TotalSeconds;
            if (leftImage.UriSource == targetImage.UriSource)
            {
                score++;
                total_time += stopwatch.Elapsed.TotalSeconds;
                taskResults.Add(new TaskResult
                {
                    TaskNumber = attemptCount,
                    ReactionTime = reactionTime,
                    Result = "correct"
                });
            }
            else
            {
                wrong++;
                taskResults.Add(new TaskResult
                {
                    TaskNumber = attemptCount,
                    ReactionTime = 0,
                    Result = "mistake"
                });
            }
            RightStatisticsAction?.Invoke(score, 10);
            WrongStatisticsAction?.Invoke(wrong + skip, 10);
            LoadNewLeftImage(); // Load new image immediately after clicking the button
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Stop the timer
            stopwatch?.Stop();
            // Close the window
            gameTimer?.Stop();
            OnGameEnd();

        }
    }
    public partial class Select attention : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
            imagePath = FindImagePath(); // Find the destination folder path
            mubiaowu.Visibility = Visibility.Visible;
            if (imagePath != null)
            {
                imagePaths = Directory.GetFiles(imagePath); // Get all files in the folder
                LoadTargetImageSync();
            }
            else
            {
                MessageBox.Show("The target folder is not found, please check the path.", "mistake", MessageBoxButton.OK, MessageBoxImage.Error);
                OnGameEnd();
            }
            LevelStatisticsAction?.Invoke(0, 0);
            RightStatisticsAction?.Invoke(0, 10);
            WrongStatisticsAction?.Invoke(0, 10);
            //
            totalGameTime = TimeSpan.Zero; // Reset total time
            gameTimer = new DispatcherTimer();
            gameTimer.Tick += GameTimer_Tick; // Bind timer event
            gameTimer.Interval = TimeSpan.FromSeconds(1); // Updated once every second
        }

        //Add code to bind the keyboardenterKeys and“confirm”Button
        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Check whether the key you pressed is the key you specified
            if (e.Key == System.Windows.Input.Key.Enter) // Check whether the Enter key is pressed
            {
                // Simulate button click, call ConfirmButton_Click The method is to define the click event of the confirm button
                ConfirmButton_Click(null, null);
            }
        }
        //End of change

        protected override async Task OnStartAsync()
        {
            InitializeImages(); // Make sure to initialize the picture list when starting the game
            gameTimer.Start(); // Start the timer

            LoadNewLeftImage();
            // Calling delegate
            VoiceTipAction?.Invoke("When the target object appears on the left side of the screen, press theOKkey.");
            SynopsisAction?.Invoke("There will be a target object on the right side of the screen. When the target object appears on the left side of the screen, press theOKno response is required in the rest of the situation.");
            RuleAction?.Invoke("There will be a target object on the right side of the screen. When the target object appears on the left side of the screen, press theOKno response is required in the rest of the situation.");//Add code, call function, display the text under the digital person
        }

        protected override async Task OnStopAsync()
        {
            stopwatch?.Stop();
            gameTimer?.Stop();
        }

        protected override async Task OnPauseAsync()
        {
            stopwatch?.Stop();
            gameTimer?.Stop();
        }

        protected override async Task OnNextAsync()
        {

            // Calling delegate
            VoiceTipAction?.Invoke("When the target object appears on the left side of the screen, press theOKkey.");
            SynopsisAction?.Invoke("There will be a target object on the right side of the screen. When the target object appears on the left side of the screen, press theOKno response is required in the rest of the situation.");
            RuleAction?.Invoke("There will be a target object on the right side of the screen. When the target object appears on the left side of the screen, press theOKno response is required in the rest of the situation.");
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new Select attention explanation();
        }

        private int GetCorrectNum()
        {
            return score;
        }
        private int GetWrongNum()
        {
            return wrong;
        }
        private int GetIgnoreNum()
        {
            return skip;
        }
        //The accuracy rate is fixed with 21 as the denominator
        private double CalculateAccuracy(int correctCount)
        {
            const int totalRequiredCorrect = 21; // answer 21 That's it 100%
            return correctCount >= totalRequiredCorrect ? 1.0 : Math.Round((double)correctCount / totalRequiredCorrect, 2);
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
                        // Get data at the current difficulty level
                        int correctCount = GetCorrectNum();
                        int wrongCount = GetWrongNum();
                        int ignoreCount = GetIgnoreNum();
                        double totalMilliseconds = totalGameTime.TotalMilliseconds;  // Convert todoubleNumber of milliseconds of type
                        double time = Math.Round((double)totalMilliseconds / attemptCount, 2);
                        // Calculation accuracy
                        double accuracy = CalculateAccuracy(correctCount);
                        // calculate ZValue reaction control value
                        double zControlValue = Math.Round((double)Math.Abs(wrongCount - 0.6) / 1.2, 2);
                        // Only the correct click time of the target object
                        double correctTotalReactionTime = taskResults
                            .Where(tr => tr.Result == "correct" && tr.ReactionTime.HasValue)
                            .Sum(tr => tr.ReactionTime.Value * 1000); // Convert to milliseconds

                        // use score As the correct number
                        double averageReactionTime = score > 0 ? correctTotalReactionTime / score : 0;
                        // calculate ZValue Response Speed ​​Value
                        double zSpeedValue = Math.Round((double)(435 - averageReactionTime) / 87, 2);

                        // create Result Record
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "Logical reasoning ability assessment report",
                            Eval = true,
                            Lv = null, // Current difficulty level
                            ScheduleId = BaseParameter.ScheduleId ?? null // Assumption Schedule_id, can be replaced with actual
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
                                    ValueName = "correct",
                                    Value = correctCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "mistake",
                                    Value = wrongCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Leaked",
                                    Value = ignoreCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Correct rate",
                                    Value = Math.Round(accuracy * 100, 2), // Stored as a percentage
                                    ModuleId = BaseParameter.ModuleId
                                },
                                 new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Average reaction time(ms)",
                                    Value = Math.Round(time, 2), // Stored as a percentage
                                    ModuleId = BaseParameter.ModuleId
                                },
                                 new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "ZValue Response Speed ​​Value",
                                    Value = Math.Round(zSpeedValue, 2),
                                    ModuleId = BaseParameter.ModuleId
                                 },
                                  new ResultDetail
                                 {
                                    ResultId = result_id,
                                    ValueName = "ZValue reaction control value",
                                    Value = Math.Round(zControlValue, 2),
                                    ModuleId = BaseParameter.ModuleId
                                 }

                            };



                        resultDetails.AddRange(taskResults.OrderBy(tr => tr.TaskNumber).Select((value, index) => new ResultDetail
                        {
                            ResultId = result_id,
                            ValueName = "Task order,Reaction time[ms]",
                            Value = Math.Round(value.ReactionTime * 1000 ?? 0, 2), // like ReactionTime for null, then the value is 0
                            Abscissa = value.TaskNumber,
                            Charttype = "Line chart",
                            ModuleId = BaseParameter.ModuleId,
                        }).ToList()
                        );


                        // insert ResultDetail data
                        db.ResultDetails.AddRange(resultDetails);
                        await db.SaveChangesAsync();
                        // Output each ResultDetail Object data
                        /*Debug.WriteLine($"Difficulty level {lv}:");*/
                        foreach (var detail in resultDetails)
                        {
                            Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId}");
                        }
                        var sortedTaskResults = taskResults.OrderBy(tr => tr.TaskNumber).ToList();

                        // Output the results of each task
                        foreach (var task in sortedTaskResults)
                        {
                            Debug.WriteLine($"Task {task.TaskNumber}: Reaction Time = {task.ReactionTime}s, Result = {task.Result}");
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
