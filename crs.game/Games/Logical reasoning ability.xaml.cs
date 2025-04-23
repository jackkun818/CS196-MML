using crs.core;
using crs.core.DbModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace crs.game.Games
{
    /// <summary>
    /// Logical reasoning ability.xaml Interaction logic
    /// </summary>
    public partial class Logical_reasoning_ability : BaseUserControl
    {
        private string imagePath; // Store the found folder path
        private const int MAX_GAME = 10;
        private string[] imagePaths;
        private string[] directoryPaths;

        private int[] timecount;//This array records whether each task is correct or wrong, and correctly records the reaction time

        private int display_index;
        private int questionCount; // Record the number of topics that have been displayed
        private int correctCount; // Record the number of correct answers
        private int incorrectCount; // Record the number of wrong answers
        private DateTime startTime; // Start time
        private double total_time; // Total answer time
        private DispatcherTimer gameTimer; // Timer
        private TimeSpan totalGameTime; // Total game time
        private int currentPosition = 1;

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            totalGameTime = totalGameTime.Add(TimeSpan.FromSeconds(1)); // One second each time// Get the total number of seconds
            int totalSeconds = (int)totalGameTime.TotalSeconds;

            // Calling delegate
            TimeStatisticsAction?.Invoke(totalSeconds, totalSeconds);

        }

        public Logical_reasoning_ability()
        {
            InitializeComponent();
        }

        private string FindImagePath()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            {
                string targetDirectory = Path.Combine(currentDirectory, "Logical reasoning ability");
                if (Directory.Exists(targetDirectory))
                {
                    return targetDirectory;
                }
                return null;
            }
            {
                string targetDirectory = Path.Combine(currentDirectory, @"Games\Logical reasoning ability");
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
                // Check if the current directory exists"crs.game"Folders
                string targetParentDirectory = Path.Combine(currentDirectory, "crs.game");
                if (Directory.Exists(targetParentDirectory))
                {
                    string targetDirectory = Path.Combine(targetParentDirectory, "Logical reasoning ability");
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

        private void ShowImage()
        {
            directoryPaths = Directory.GetDirectories(imagePath); // Get the path to all questions
            Random rand = new Random();
            int index = rand.Next(directoryPaths.Length);
            imagePaths = Directory.GetFiles(directoryPaths[index]); // A folder where a question is located

            // Ensure that the number of files is sufficient
            if (imagePaths.Length >= 8)
            {
                QImage1.Source = new BitmapImage(new Uri(imagePaths[0]));
                QImage2.Source = new BitmapImage(new Uri(imagePaths[1]));
                QImage3.Source = new BitmapImage(new Uri(imagePaths[2]));
                QImage4.Source = new BitmapImage(new Uri(imagePaths[3]));
                AImage1.Source = new BitmapImage(new Uri(imagePaths[4]));
                AImage2.Source = new BitmapImage(new Uri(imagePaths[5]));
                AImage3.Source = new BitmapImage(new Uri(imagePaths[6]));
                AImage4.Source = new BitmapImage(new Uri(imagePaths[7]));
            }
            else
            {
                MessageBox.Show("There are insufficient files in the current question folder, please check！", "mistake", MessageBoxButton.OK, MessageBoxImage.Error);
                OnGameEnd();
            }
            //currentPosition = 1;
            //Grid.SetColumn(Select, currentPosition);

            Image.Source = null;
            startTime = DateTime.Now; // Record the start time
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (display_index != -1)
            {
                switch (display_index)
                {
                    case 0:
                        AImage1.Source = Image.Source;
                        break;
                    case 1:
                        AImage2.Source = Image.Source;
                        break;
                    case 2:
                        AImage3.Source = Image.Source;
                        break;
                    case 3:
                        AImage4.Source = Image.Source;
                        break;
                    default:
                        break;
                }
            }
            Image.Source = null;
            display_index = -1;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //Button_Click(sender, e);
            if (display_index != -1)
                return;
            Image.Source = AImage1.Source;
            AImage1.Source = null;
            display_index = 0;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //Button_Click(sender, e);
            if (display_index != -1)
                return;
            Image.Source = AImage2.Source;
            AImage2.Source = null;
            display_index = 1;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //Button_Click(sender, e);
            if (display_index != -1)
                return;
            Image.Source = AImage3.Source;
            AImage3.Source = null;
            display_index = 2;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            //Button_Click(sender, e);
            if (display_index != -1)
                return;
            Image.Source = AImage4.Source;
            AImage4.Source = null;
            display_index = 3;

        }

        private void CheckQuestionCount()
        {
            questionCount++;
            if (questionCount >= MAX_GAME) // Show two questions and end
            {
                //MessageBox.Show($"Total answer time: {total_time:F2} Second\nCorrect answer: {correctCount}\nIncorrect answer: {incorrectCount}");
                //Logical reasoning ability report nwd=new Logical reasoning ability report(total_time/MAX_GAME,correctCount,incorrectCount);
                //nwd.Show();
                OnGameEnd();
            }
            else
            {
                ShowImage();
            }
        }

        async private void ConFirm_Click(object sender, RoutedEventArgs e)
        {
            DateTime endTime = DateTime.Now; // Record the end time
            TimeSpan duration = endTime - startTime; // Calculate the answer time
            int durationInMilliseconds = (int)duration.TotalMilliseconds;
            if (display_index == 0) // Assume that the fifth picture corresponds to the correct answer
            {
                correctCount++; // Correct count plus 1
                timecount[questionCount] = durationInMilliseconds;

                PlayWav(CorrectSoundPath);
                ShowFeedbackImage(CorrectImage);
            }
            else
            {
                incorrectCount++; // Error count plus 1
                //timecount[questionCount] = -1;
                timecount[questionCount] = durationInMilliseconds;

                PlayWav(ErrorSoundPath);
                ShowFeedbackImage(ErrorImage);
            }
            Confirm_Button.IsEnabled = false;

            await Task.Delay(3000);

            CorrectImage.Visibility = Visibility.Collapsed;
            ErrorImage.Visibility = Visibility.Collapsed;
            Confirm_Button.IsEnabled = true;

            total_time += duration.TotalSeconds; // Total accumulated time
            RightStatisticsAction?.Invoke(correctCount, 10);
            WrongStatisticsAction?.Invoke(incorrectCount, 10);
            CheckQuestionCount();

            display_index = -1;
        }

        private void Button_Click_(object sender, RoutedEventArgs e)
        {
            OnGameEnd();
        }


        private SoundPlayer soundPlayer; // Used to sing
        public string ErrorSoundPath;//The wrong sound file path, inOnStartAsync()Medium configuration
        public string CorrectSoundPath;//The correct sound file path is inOnStartAsync()Medium configuration
        private int StopDurations = 2000; // Stop time,ms

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
        }


    }
    public partial class Logical_reasoning_ability : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {

            CorrectSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");
            // for Image Control loading picture Source
            CorrectImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Correct.png"), UriKind.RelativeOrAbsolute));
            ErrorImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Error.png"), UriKind.RelativeOrAbsolute));

            questionCount = 0; // Initialize the question count
            correctCount = 0; // Initialize the correct count
            incorrectCount = 0; // Initialize error count
            total_time = 0; // Total initialization time
            display_index = -1;

            // Find the destination folder path
            imagePath = FindImagePath();
            if (imagePath == null)
            {
                MessageBox.Show("No name found"Logical reasoning ability"folder.", "mistake", MessageBoxButton.OK, MessageBoxImage.Error);
                OnGameEnd();
            }
            LevelStatisticsAction?.Invoke(0, 0);
            RightStatisticsAction?.Invoke(0, 10);
            WrongStatisticsAction?.Invoke(0, 10);
            timecount = new int[MAX_GAME];
            for (int i = 0; i < MAX_GAME; i++)
            {
                timecount[i] = 0;
            }
            totalGameTime = TimeSpan.Zero; // Reset total time
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1); // Updated once every second
            gameTimer.Tick += GameTimer_Tick; // Bind timer event
        }

        protected override async Task OnStartAsync()
        {

            gameTimer.Start(); // Start the timer
            ShowImage();
            // Calling delegate
            VoiceTipAction?.Invoke("Please find a suitable option in the several images provided.");
            SynopsisAction?.Invoke("You will see an image sequence on the screen, and the images of the sequence have some pattern between each other. You need to think about discovering the pattern, and use the left and right buttons to select and press the several images provided.OKKey confirmation to find a suitable option.");
            RuleAction?.Invoke("You will see an image sequence on the screen, and the images of the sequence have some pattern between each other. You need to think about discovering the pattern, and use the left and right buttons to select and press the several images provided.OKKey confirmation to find a suitable option.");//Add code, call function, display the text under the digital person
        }

        protected override async Task OnStopAsync()
        {
            gameTimer?.Stop();
        }

        protected override async Task OnPauseAsync()
        {
            gameTimer?.Stop();
        }

        protected override async Task OnNextAsync()
        {

            // Calling delegate
            VoiceTipAction?.Invoke("Please find a suitable option in the several images provided.");
            SynopsisAction?.Invoke("You will see an image sequence on the screen, and the images of the sequence have some pattern between each other. You need to think about discovering the pattern, and use the left and right buttons to select and press the several images provided.OKKey confirmation to find a suitable option.");
            RuleAction?.Invoke("You will see an image sequence on the screen, and the images of the sequence have some pattern between each other. You need to think about discovering the pattern, and use the left and right buttons to select and press the several images provided.OKKey confirmation to find a suitable option.");
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new Explanation of logical thinking ability();
        }

        private int GetCorrectNum()
        {
            return correctCount;
        }
        private int GetWrongNum()
        {
            return incorrectCount;
        }
        private int GetIgnoreNum()
        {
            return 0;
        }
        private double CalculateAccuracy(int correctCount, int wrongCount, int ignoreCount)
        {
            int totalAnswers = correctCount + wrongCount + ignoreCount;
            return totalAnswers > 0 ? Math.Round((double)correctCount / totalAnswers, 2) : 0;
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
                        int count = questionCount;
                        double Zvaluecorrect = (GetCorrectNum() - 4.8) / 3.2;
                        double totalMilliseconds = totalGameTime.TotalMilliseconds;  // Convert todoubleNumber of milliseconds of type
                        double time = (double)total_time / questionCount;
                        // Calculation accuracy
                        double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);
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
                                        ValueName = "Task",
                                        Value = count,
                                        ModuleId = BaseParameter.ModuleId
                                    },
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
                                    ValueName = "Correct rate",
                                    Value = Math.Round(accuracy * 100, 2), // Stored as a percentage
                                    ModuleId = BaseParameter.ModuleId
                                },
                                    new ResultDetail
                                    {
                                        ResultId = result_id,
                                        ValueName = "Average answer time(s)",
                                        Value = Math.Round(time, 2),
                                        ModuleId = BaseParameter.ModuleId
                                    },
                                     new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "ZNumber of correct answers",
                                    Value = Math.Round(Zvaluecorrect, 2), // Stored as a percentage
                                    ModuleId = BaseParameter.ModuleId
                                }
                            };

                        resultDetails.AddRange(timecount.Select((value, index) => new ResultDetail
                        {
                            ResultId = result_id,
                            ValueName = "Task order,time[s]",
                            Value = (double)value / 1000.0,
                            Abscissa = index + 1,
                            Charttype = "Line chart",
                            ModuleId = BaseParameter.ModuleId,
                        }).ToList());


                        // insert ResultDetail data
                        db.ResultDetails.AddRange(resultDetails);
                        await db.SaveChangesAsync();
                        // Output each ResultDetail Object data
                        /*Debug.WriteLine($"Difficulty level {lv}:");*/
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
