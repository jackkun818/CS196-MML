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
using System.Windows.Input;
using System.Collections.Generic;

namespace crs.game.Games
{
    /// <summary>
    /// Alert ability.xaml Interaction logic
    /// </summary>
    public partial class Alert_ability : BaseUserControl
    {
        private List<double> soundReactionTimes = new List<double>(); // Response time with sound
        private List<double> noSoundReactionTimes = new List<double>(); // When there is no sound reaction
        private bool is_beep = true;//Is there any sound prompt
        private const int GAMETIME = 40;
        private const int wait_delay = 4000; // Waiting for 4000 usersmsRespond
        private const int min_delay = 1000;   // Minimum value of image waiting time
        private const int max_delay = 3001;   // Maximum waiting time for generating image
        //Correct rate statistics
        private int beep_num = 0;//How many times did the sound prompt
        private int beep_forget = 0;//How many times did the sound be missed
        private int nonbeep_num = 0;//How many times have there been silence
        private int nonbeep_forget = 0;//How many times have I missed silently
        private List<bool> beepSequence; // For storage 20 There are sound prompts and 20 Random order of no sound prompts
        private int beepIndex = 0; // Used to track the index of the current task
        //
        private BitmapImage targetImage;
        private int attemptCount = 0;
        private Stopwatch stopwatch;
        private double total_time = 0;
        private int forget = 0; // Record the number of times you have forgotten to click
        private CancellationTokenSource cts; // Used to cancel timeout tasks
        string[] imagePaths; // Store photos in folders
        private DispatcherTimer gameTimer; // Timer
        private TimeSpan totalGameTime; // Total game time

        private int correctWithBeep = 0; // Number of times a sound prompts to answer correctly
        private int correctWithoutBeep = 0; // No sound prompts correct answers

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            totalGameTime = totalGameTime.Add(TimeSpan.FromSeconds(1)); // One second each time// Get the total number of seconds
            int totalSeconds = (int)totalGameTime.TotalSeconds;

            // Calling delegate
            TimeStatisticsAction?.Invoke(totalSeconds, totalSeconds);

        }


        private string FindImagePath()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            {
                string targetDirectory = Path.Combine(currentDirectory, "Alert ability");
                if (Directory.Exists(targetDirectory))
                {
                    return targetDirectory;
                }
                return null;
            }
            {
                string targetDirectory = Path.Combine(currentDirectory, @"Games\Alert ability");
                if (Directory.Exists(targetDirectory))
                {
                    return targetDirectory;
                }
                return null;
            }
        }
        /*
    while (true)
    {
        // Check if the current directory exists"crs.game"Folders
        string targetParentDirectory = Path.Combine(currentDirectory, "crs.game");
        if (Directory.Exists(targetParentDirectory))
        {
            string targetDirectory = Path.Combine(targetParentDirectory, "Alert ability");
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
    */

        public Alert_ability()
        {
            InitializeComponent();
            this.Focusable = true;
            this.Focus();
            GenerateBeepSequence(); // Initialize the sound prompt sequence
        }
        private void GenerateBeepSequence()//Add to 20 A sound tip（true）and 20 A soundless prompt
        {
            beepSequence = new List<bool>();

            // Add to 20 A sound tip（true）and 20 A soundless prompt（false）
            beepSequence.AddRange(Enumerable.Repeat(true, 20));
            beepSequence.AddRange(Enumerable.Repeat(false, 20));

            // Random disruption order
            // Fisher-Yates Shuffle algorithm
            Random rand = new Random();
            for (int i = beepSequence.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                // Switch location i and j Elements
                bool temp = beepSequence[i];
                beepSequence[i] = beepSequence[j];
                beepSequence[j] = temp;
            }
            beepSequence = beepSequence.OrderBy(x => rand.Next()).ToList();
        }

        private void LoadTargetImage()
        {
            Random rand = new Random();
            int index = rand.Next(imagePaths.Length);
            targetImage = new BitmapImage(new Uri(System.IO.Path.Combine(Directory.GetCurrentDirectory(), imagePaths[index])));
            TImage.Source = targetImage;
        }

        private void PrintReactionTimes()//Debugging the response time output function
        {
            Debug.WriteLine("Response time with sound:");
            foreach (var time in soundReactionTimes)
            {
                Debug.WriteLine(time + " Second");
            }

            Debug.WriteLine("No sound reaction time:");
            foreach (var time in noSoundReactionTimes)
            {
                Debug.WriteLine(time + " Second");
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // set up UserControl Get focus
            this.Focus();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Check whether to press Enter key
            if (e.Key == Key.Enter)
            {

                // Call ConfirmButton_Click method
                ConfirmButton_Click(sender, e);
            }
        }

        private async void LoadNewImage()
        {
            if (attemptCount >= GAMETIME)
            {
                //MessageBox.Show($"Test ends！\n" + $"Total reaction time: {total_time}Second" + $"Total number of omissions: {forget}Second-rate", "Test results");
                //Alert ability report nwd=new Alert ability report(total_time/GAMETIME,is_beep,forget);
                //nwd.Show();
                nonbeep_num = attemptCount - beep_num;
                nonbeep_forget = forget - beep_forget;
                StopAllTasks();
                //PrintReactionTimes();
                OnGameEnd();
                return;
            }

            // Cancel previous task
            try
            {
                cts?.Cancel();
            }
            catch (ObjectDisposedException)
            {

            }
            //cts?.Cancel();
            cts = new CancellationTokenSource(); // Create a new one CancellationTokenSource

            // Clear the current picture
            TargetImage.Source = null;

            var token = cts.Token;

            Random rand = new Random();
            int delay = rand.Next(min_delay, max_delay);
            try
            {
                await Task.Delay(delay, token); // Use cancel token
            }
            catch (TaskCanceledException)
            {

            }
            //await Task.Delay(delay, token); // Use cancel token

            // Random settings TargetImage Location
            // Show pictures
            TargetImage.Source = targetImage;
            stopwatch = Stopwatch.StartNew(); // Time count starts immediately after the picture is displayed
            double maxX = ImageCanvas.ActualWidth - TargetImage.Width;
            double maxY = ImageCanvas.ActualHeight - TargetImage.Height;

            double randomX = rand.NextDouble() * maxX;
            double randomY = rand.NextDouble() * maxY;

            Canvas.SetLeft(TargetImage, randomX);
            Canvas.SetTop(TargetImage, randomY);

            attemptCount++;
            RightStatisticsAction?.Invoke(attemptCount - forget, 10);
            WrongStatisticsAction?.Invoke((forget), 10);

            // Use pre-generated sound prompt sequence
            is_beep = beepSequence[beepIndex];
            beepIndex++; // Move to the next task

            if (is_beep)
            {
                beep_num++;
                Task.Run(() => Console.Beep(800, 200)); // Asynchronous playback warning sound
            }
            try
            {
                await Task.Delay(wait_delay, token); // waitDELAYtime
            }
            catch (TaskCanceledException)
            {
                return;
            }



            //attemptCount++;
            RightStatisticsAction?.Invoke(attemptCount - forget, 10);
            forget++;
            if (is_beep)
            {
                beep_forget++;
            }
            total_time += wait_delay / 1000;
            TargetImage.Source = null; // Clear the current picture
            LoadNewImage(); // Reload new image
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (TargetImage.Source == null)
            {

                //MessageBox.Show("Touch button incorrectly！");
                return;
            }

            stopwatch.Stop();
            if (is_beep)
            {
                soundReactionTimes.Add(stopwatch.Elapsed.TotalSeconds);
                correctWithBeep++; // Answer correctly when there is a sound prompt
            }
            else
            {
                noSoundReactionTimes.Add(stopwatch.Elapsed.TotalSeconds);
                correctWithoutBeep++; // Answer correctly when there is no sound prompt
            }
            total_time += stopwatch.Elapsed.TotalSeconds;
            LoadNewImage(); // Reload new image
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EvaluateTest evaluateTestWindow = Application.Current.Windows.OfType<EvaluateTest>().FirstOrDefault();
            if (evaluateTestWindow != null)
            {
                evaluateTestWindow.SetContinueAssessment(false); // Stop displaying the subsequent window
            }
            StopAllTasks(); // Stop all tasks when closing the window
            OnGameEnd();
        }

        private void StopAllTasks()
        {
            // Cancel the current CancellationTokenSource
            try
            {
                cts?.Cancel();
            }
            catch (ObjectDisposedException)
            {

            }
            //cts?.Cancel();
            cts?.Dispose(); // Free up resources
        }
    }
    public partial class Alert_ability : BaseUserControl
    {

        protected override async Task OnInitAsync()
        {
            string imagePath = FindImagePath();
            mubiaowu.Visibility = Visibility.Visible;
            if (imagePath != null)
            {
                // If a folder is found, get all files in that folder
                imagePaths = Directory.GetFiles(imagePath);
                LoadTargetImage();
            }
            else
            {
                MessageBox.Show("No name found \"Alert ability\" folder.", "mistake", MessageBoxButton.OK, MessageBoxImage.Error);
                OnGameEnd();
            }
            LevelStatisticsAction?.Invoke(0, 0);
            RightStatisticsAction?.Invoke(0, 10);
            WrongStatisticsAction?.Invoke(0, 10);

            totalGameTime = TimeSpan.Zero; // Reset total time
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1); // Updated once every second
            gameTimer.Tick += GameTimer_Tick; // Bind timer event
        }

        protected override async Task OnStartAsync()
        {
            gameTimer.Start(); // Start the timer
            LoadNewImage();
            // Calling delegate
            VoiceTipAction?.Invoke("When the target appears on the screen, please click the confirm button with the mouse.");
            SynopsisAction?.Invoke("Now you can see the selected target map on the right. When the target object with a warning sound or no warning sound appears on the screen, please click Confirm");
            RuleAction?.Invoke("Now you can see the selected target map on the right. When the target object with a warning sound or no warning sound appears on the screen, please click Confirm");//Add code, call function, display the text under the digital person
        }

        protected override async Task OnStopAsync()
        {

            StopAllTasks();
            stopwatch?.Stop();
            gameTimer?.Stop();
        }

        protected override async Task OnPauseAsync()
        {
            StopAllTasks();
            stopwatch?.Stop();
            gameTimer?.Stop();
        }

        protected override async Task OnNextAsync()
        {

            // Calling delegate
            VoiceTipAction?.Invoke("When the target appears on the screen, please click the confirm button with the mouse.");
            SynopsisAction?.Invoke("Now you can see the selected target map on the right. When the target object with a warning sound or no warning sound appears on the screen, please click Confirm");
            RuleAction?.Invoke("Now you can see the selected target map on the right. When the target object with a warning sound or no warning sound appears on the screen, please click Confirm");//Add code, call function, display the text under the digital person
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new Explanation_of_alertness();
        }

        private int GetCorrectNum()
        {
            return attemptCount - forget;
        }
        private int GetWrongNum()
        {
            return 0;
        }
        private int GetIgnoreNum()
        {
            return forget;
        }

        private int GetCorrectNum_beep()//The correct number of sound prompts
        {
            return beep_num - beep_forget;
        }
        private int GetCorrectNum_nonbeep()//Silent correct number
        {
            return nonbeep_num - nonbeep_forget;
        }

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

                    // Get data at the current difficulty level
                    int correctCount = GetCorrectNum();
                    int wrongCount = GetWrongNum();
                    int ignoreCount = GetIgnoreNum();
                    double totalMilliseconds = totalGameTime.TotalMilliseconds;  // Convert todoubleNumber of milliseconds of type
                    double time = (double)totalMilliseconds / attemptCount;
                    // Calculation accuracy
                    double accuracy = CalculateAccuracy(correctCount);

                    // Calculate the total and average reaction time with warning sounds（Only the correct ones are calculated）
                    double totalReactionTimeWithBeep = soundReactionTimes.Sum() * 1000; // Convert to milliseconds
                    double averageReactionTimeWithBeep = correctWithBeep > 0 ? totalReactionTimeWithBeep / correctWithBeep : 0;

                    // Calculate the total and average reaction time of no warning sounds（Only the correct ones are calculated）
                    double totalReactionTimeWithoutBeep = noSoundReactionTimes.Sum() * 1000; // Convert to milliseconds
                    double averageReactionTimeWithoutBeep = correctWithoutBeep > 0 ? totalReactionTimeWithoutBeep / correctWithoutBeep : 0;

                    // Calculate warning sound ZValue and no warning sound Zvalue
                    double zValueWithBeep = Math.Round((double)(262 - averageReactionTimeWithBeep) / 67, 2);
                    double zValueWithoutBeep = Math.Round((double)(268 - averageReactionTimeWithoutBeep) / 71, 2);

                    // create Result Record
                    var newResult = new Result
                    {
                        ProgramId = BaseParameter.ProgramId, // program_id
                        Report = "Alert ability assessment report",
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
                                    Order = 0,
                                    ValueName = "Correct times",
                                    Value = correctCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                     ResultId = result_id,
                                     Order = 2,
                                     ValueName = "The number of times there is a warning sound",
                                     Value = correctWithBeep,
                                     ModuleId = BaseParameter.ModuleId
                                 },
                                new ResultDetail
                                {
                                     ResultId = result_id,
                                     ValueName = "No warning sound correct number",
                                     Order = 5,
                                     Value = correctWithoutBeep,
                                     ModuleId = BaseParameter.ModuleId
                                },

                                new ResultDetail
                                {
                                     ResultId = result_id,
                                     ValueName = "Number of warning omissions",
                                     Order = 3,
                                     Value = beep_forget,
                                     ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                     ResultId = result_id,
                                     ValueName = "No warning omissions",
                                     Order = 6,
                                     Value = nonbeep_forget,
                                     ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order =1,
                                    ValueName = "Average reaction time(ms)",
                                    Value = Math.Round(time, 2), // Stored as a percentage
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 4,
                                    ValueName = "There is a warning soundZvalue",
                                    Value = Math.Round(zValueWithBeep, 2),
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order =7,
                                    ValueName = "No warning soundZvalue",
                                    Value = Math.Round(zValueWithoutBeep, 2),
                                    ModuleId = BaseParameter.ModuleId
                                }
                                };

                    resultDetails.AddRange(soundReactionTimes.Select((value, index) => new ResultDetail
                    {
                        ResultId = result_id,
                        ValueName = "Task(There is a warning sound),time[ms]",
                        Value = Math.Round(value * 1000, 2),
                        Abscissa = index + 1,
                        Charttype = "Line chart",
                        ModuleId = BaseParameter.ModuleId
                    }).Reverse().ToList());

                    resultDetails.AddRange(noSoundReactionTimes.Select((value, index) => new ResultDetail
                    {
                        ResultId = result_id,
                        ValueName = "Task(No warning tone),time[ms]",
                        Value = Math.Round(value * 1000, 2),
                        Abscissa = index + 1,
                        Charttype = "Line chart",
                        ModuleId = BaseParameter.ModuleId
                    }).Reverse().ToList());


                    // insert ResultDetail data
                    db.ResultDetails.AddRange(resultDetails);
                    await db.SaveChangesAsync();
                    PrintReactionTimes();
                    // Output each ResultDetail Object data
                    /*Debug.WriteLine($"Difficulty level {lv}:");*/
                    foreach (var detail in resultDetails)
                    {
                        Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId}");
                    }
                    /*
                    Debug.WriteLine("beepNum:");
                    Debug.WriteLine(beep_num);
                    Debug.WriteLine("nonbeepNum:");
                    Debug.WriteLine(nonbeep_num);
                    Debug.WriteLine("beepforget:");
                    Debug.WriteLine(beep_forget);
                    Debug.WriteLine("nonbeepforget:");
                    Debug.WriteLine(nonbeep_forget);*/

                    // Submit transactions
                    await transaction.CommitAsync();
                    Debug.WriteLine("Insert successfully");
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
