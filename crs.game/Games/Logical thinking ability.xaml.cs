using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using crs.core;
using crs.core.DbModels;
using log4net.Core;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;


namespace crs.game.Games
{
    /// <summary>
    /// LODE.xaml Interaction logic
    /// </summary>
    public partial class Logical thinking ability : BaseUserControl
    {
        private int countdownTime;
        private const int MAX_DELAY = 5000; // 5 seconds
        private const int AMOUNT = 1; // The cardinality of the threshold
        private string imagePath;
        private string[] imagePaths;
        private string[] directoryPaths;
        private string[] questionPaths;
        private string[] answerPaths;
        private int hardness;
        private int index;
        private Image lastClickedImage;
        private int[] correctAnswers; // Store the number of correct answers for each difficulty
        private int[] wrongAnswers; // Store the number of error answers per difficulty
        private int[] ignoreAnswers;
        private int imageCount;
        private int max_time = 10;
        private const int WAIT_DELAY = 1;
        private const int MAX_HARDNESS = 23;
        private int TRAIN_TIME; // Training duration（Unit: seconds）
        private int cost_time;
        private bool IS_RESTRICT_TIME = false; // Whether to enable exercise time
        private bool IS_BEEP = true;
        private bool IS_VISUAL = true;
        private int train_time;
        private int counter;
        private int randomIndex;
        private Random random;
        private const int moveAmount = 2;
        private int left;
        private int top;
        private DispatcherTimer timer;
        private DispatcherTimer trainingTimer; // New timer for training time
        private DispatcherTimer countTimer;
        private List<bool> boolList = new List<bool>(5);

        private int[] TotalCountByHardness;
        private double[] TotalAccuracyByHardness;
        private double[] AverageTimeByHardness;
        private List<int>[] CostTime;
        private double[] AverageCostTime;

        private bool is_enter = false;

        private int LevelUp = 3; // Increase the threshold for difficulty
        private int LevelDown = 3; // Threshold for reducing difficulty
        private int stimulus_interval = 3;
        public Logical thinking ability()
        {
            InitializeComponent();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (countdownTime > 0)
            {
                countdownTime--; // 1 reduction per second
                cost_time += 1;
                TimeStatisticsAction.Invoke(train_time, countdownTime);
                // renewUI（For example, display the remaining time）
            }
            else
            {
                timer.Stop();
                ignoreAnswers[hardness - 1]++; // Records are ignored
                //wrongAnswers[hardness - 1]++; //Not sure if the timeout is counted as an error
                ClearAndLoadNewImages();
            }
            TotalCountByHardness[hardness - 1] = correctAnswers[hardness - 1] + wrongAnswers[hardness - 1];
            if (TotalCountByHardness[hardness - 1] != 0)
            {
                TotalAccuracyByHardness[hardness - 1] = correctAnswers[hardness - 1] / TotalCountByHardness[hardness - 1];
            }

        }
        private void TrainingTimer_Tick(object sender, EventArgs e)
        {
            //MessageBox.Show(IS_RESTRICT_TIME.ToString()); //I've made himfalseIt's But time still goes by normally

            if (IS_RESTRICT_TIME)
            {
                train_time--; // Countdown to training time

            }
            TimeStatisticsAction.Invoke(train_time, countdownTime);

            if (train_time <= 0)
            {
                timer.Stop(); // Stop the main timer
                trainingTimer.Stop(); // Stop training timer
                OnGameEnd();
               
            }
        }
        private void Countimer_Tick(object sender, EventArgs e)
        {

            countTimer?.Stop();
            ClearAndLoadNewImages();
            CorrectImage.Visibility = Visibility.Collapsed;
            ErrorImage.Visibility = Visibility.Collapsed;
            is_enter = false;
        }

        private string FindImagePath()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string targetDirectory = System.IO.Path.Combine(currentDirectory, "Logical thinking ability");
            return targetDirectory;
            //while (true)
            //{
            //    string targetParentDirectory = System.IO.Path.Combine(currentDirectory, "crs.game");
            //    if (Directory.Exists(targetParentDirectory))
            //    {
            //        string targetDirectory = System.IO.Path.Combine(targetParentDirectory, "Logical thinking ability");
            //        return targetDirectory;
            //    }
            //    DirectoryInfo parentDirectory = Directory.GetParent(currentDirectory);
            //    if (parentDirectory == null)
            //    {
            //        break;
            //    }
            //    currentDirectory = parentDirectory.FullName;
            //}
            //return null;
        }
        /*
        private string FindImagePath()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            {
                string targetDirectory = Path.Combine(currentDirectory, "Logical thinking ability");
                if (Directory.Exists(targetDirectory))
                {
                    return targetDirectory;
                }
                return null;
            }
            {
                string targetDirectory = Path.Combine(currentDirectory, @"Games\Logical thinking ability");
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
            string newFolderPath = System.IO.Path.Combine(imagePaths[index], "Q");

            questionPaths = Directory.GetFiles(newFolderPath);
            questionPaths = questionPaths.OrderBy(f =>
            {
                var match = Regex.Match(Path.GetFileNameWithoutExtension(f), @"\d+");
                return match.Success ? int.Parse(match.Value) : 0;
            }).ToArray();
            /*string message = string.Join(Environment.NewLine, questionPaths);
            MessageBox.Show(message);*/
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
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d0e3b6")), // Set background color
                BorderBrush = Brushes.Transparent // Remove border color
            };

            additionalButton.Click += AdditionalButton_Click;

            Image buttonImg = new Image
            {
                Source = null,
                Width = 150,
                Height = 150
            };

            additionalButton.Content = buttonImg;
            ImagePanel.Children.Add(additionalButton);
        }

        private void ClearAndLoadNewImages()
        {
            CorrectImage.Visibility = Visibility.Collapsed;
            ErrorImage.Visibility = Visibility.Collapsed;
            ImagePanel.Children.Clear();
            ButtonPanel.Children.Clear();
            AddImages();
            AddButtons();
            countdownTime = max_time;
            timer.Start(); // Restart the timer
            Confirm_Button.IsEnabled = true;
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
            string newFolderPath = System.IO.Path.Combine(imagePaths[index], "A");
            answerPaths = Directory.GetFiles(newFolderPath);
            Random rand = new Random();
            answerPaths = answerPaths.OrderBy(x => rand.Next()).ToArray();

            for (int i = 0; i < answerPaths.Length; i++)
            {
                Button btn = new Button
                {
                    Width = 150,
                    Height = 150,
                    Margin = new Thickness(5),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d0e3b6")), // Set background color
                    BorderBrush = Brushes.Transparent // Remove border color
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
            if (!is_enter)
            {
                timer.Stop(); // Stop the timer
                Button additionalButton = ImagePanel.Children.OfType<Button>().LastOrDefault();
                Image additionalButtonImage = additionalButton?.Content as Image;

                string firstImagePath = Directory.GetFiles(System.IO.Path.Combine(imagePaths[index], "A")).FirstOrDefault();
                bool isCorrect = additionalButtonImage.Source != null && additionalButtonImage.Source.ToString() == new BitmapImage(new Uri(firstImagePath)).ToString();
                additionalButton.BorderThickness = new Thickness(5);
                additionalButton.BorderBrush = Brushes.Transparent;
                CostTime[hardness - 1].Add(cost_time);
                cost_time = 0;
                AverageCostTime[hardness - 1] = CostTime[hardness - 1].Average();

                if (isCorrect)
                {

                    additionalButton.BorderThickness = new Thickness(5);
                    additionalButton.BorderBrush = Brushes.Green;
                    timer?.Stop();
                    countTimer.Interval = TimeSpan.FromSeconds(stimulus_interval);

                    if (IS_BEEP) PlayWav(CorrectSoundPath);
                    if (IS_VISUAL) ShowFeedbackImage(CorrectImage);
                    countTimer.Start();
                    correctAnswers[hardness - 1]++; // Correct record
                    RightStatisticsAction?.Invoke(correctAnswers[hardness - 1], LevelUp);
                    //await Task.Delay(TimeSpan.FromSeconds(WAIT_DELAY));
                    boolList.Add(true);

                    // Check list length and delete the first element to keep list length 5
                    if (boolList.Count > 5)
                    {
                        boolList.RemoveAt(0);
                    }
                }
                else
                {
                    additionalButton.BorderThickness = new Thickness(5);
                    additionalButton.BorderBrush = Brushes.Red;
                    timer?.Stop();
                    countTimer.Interval = TimeSpan.FromSeconds(stimulus_interval);

                    wrongAnswers[hardness - 1]++; // Recording errors
                    WrongStatisticsAction?.Invoke(wrongAnswers[hardness - 1], LevelDown);
                    if (IS_BEEP) PlayWav(ErrorSoundPath);
                    if (IS_VISUAL) ShowFeedbackImage(ErrorImage);
                    countTimer.Start();
                    boolList.Add(false);

                    // Check list length and delete the first element to keep list length 5
                    if (boolList.Count > 5)
                    {
                        boolList.RemoveAt(0);
                    }
                }
                Confirm_Button.IsEnabled = false;
                AdjustDifficulty();
                is_enter = true;
            }

        }
        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.Enter && is_enter == true)
            {
                countTimer?.Stop();
                ClearAndLoadNewImages();
                is_enter = false;
            }
        }
        private void resetboollist()
        {
            boolList.Clear();
        }
        private void AdjustDifficulty()
        {
            int correctCount = 0;
            int wrongCount = 0;
            // Add the current question results torecentResultsList

            // Only the last 5 results are retained
            int max = Math.Max(LevelUp, LevelDown); // AssumptionsMaxyesMathIn the classMaxmethod

            // make surerecentResultsThe size of the set does not exceedmax
            if (boolList.Count > max)
            {
                boolList.RemoveAt(0); // Remove the first element
            }

            if (boolList.Count == max)
            {
                // Calculate the number of correct answers in recent questions
                for (int i = boolList.Count - LevelUp; i < boolList.Count; i++)
                {
                    correctCount += boolList[i] ? 1 : 0; // AssumptionsrecentResults[i]yesboolType, if correct, add 1
                }
                for (int i = boolList.Count - LevelDown; i < boolList.Count; i++)
                {
                    wrongCount += boolList[i] ? 0 : 1; // AssumptionsrecentResults[i]yesboolType, if error is added 1
                }

                // Increase difficulty
                if (correctCount == LevelUp && hardness < 24)
                {
                    hardness++;
                    resetboollist();
                }

                // Reduce difficulty
                else if (wrongCount == LevelDown && hardness > 1)
                {
                    hardness--;
                    resetboollist();
                }
            }
        }
        private void ResetCounts()
        {
            // Reset the correct and error counts of the current difficulty
            correctAnswers[hardness - 1] = 0;
            wrongAnswers[hardness - 1] = 0;
        }

        /*LJN
 Added resources for visual and sound feedback
 */
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


        //Add a timer to synchronize the stars on the therapist side
        private DispatcherTimer StarTimer = new DispatcherTimer();
    }
    public partial class Logical thinking ability : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
            /*LJN
            Configure the path to feedback resources             
             */
            CorrectSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");
            // for Image Control loading picture Source
            CorrectImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Correct.png"), UriKind.RelativeOrAbsolute));
            ErrorImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Error.png"), UriKind.RelativeOrAbsolute));

            // parameter（Includes module parameter information）
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
                            case 178: // grade
                                hardness = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"HARDNESS: {hardness}");
                                break;
                            case 35: // Treatment time 
                                train_time = par.Value.HasValue ? (int)par.Value.Value * 60 : 60;
                                Debug.WriteLine($"TRAIN_TIME={train_time}");
                                break;
                            case 36: // Level improvement
                                LevelUp = par.Value.HasValue ? (int)par.Value.Value : 3;
                                Debug.WriteLine($"INCREASE={LevelUp}");
                                break;
                            case 37: // Level down
                                LevelDown = par.Value.HasValue ? (int)par.Value.Value : 3;
                                Debug.WriteLine($"DECREASE ={LevelDown}");
                                break;
                            case 38: // Project level
                                /*                                IS_RESTRICT_TIME = par.Value == 1;
                                                                Debug.WriteLine($"Whether to limit time ={IS_BEEP}");*/
                                break;
                            case 39: // Limit answer time
                                IS_RESTRICT_TIME = par.Value == 1;
                                Debug.WriteLine($"Limit answer time ={IS_RESTRICT_TIME}");
                                break;
                            case 40: // Auditory feedback
                                IS_BEEP = par.Value == 1;
                                Debug.WriteLine($"Whether to hear feedback ={IS_BEEP}");
                                break;
                            case 270://Visual feedback
                                IS_VISUAL = par.Value == 1;
                                break;
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
            correctAnswers = new int[MAX_HARDNESS + 1];
            wrongAnswers = new int[MAX_HARDNESS + 1];
            ignoreAnswers = new int[MAX_HARDNESS + 1];
            AverageCostTime = new double[MAX_HARDNESS + 1];
            TotalCountByHardness = new int[MAX_HARDNESS + 1];
            TotalAccuracyByHardness = new double[MAX_HARDNESS + 1];
            CostTime = new List<int>[MAX_HARDNESS + 1];
            for (int i = 0; i < correctAnswers.Length; i++)
            {
                correctAnswers[i] = 0;
                wrongAnswers[i] = 0;
                ignoreAnswers[i] = 0;
                TotalAccuracyByHardness[i] = 0;
                TotalCountByHardness[i] = 0;
                CostTime[i] = new List<int>();
                AverageCostTime[i] = 0;

            };

            imagePath = FindImagePath();

            // Calling delegate
            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
            RightStatisticsAction?.Invoke(0, LevelUp);
            WrongStatisticsAction?.Invoke(0, LevelDown);
            StarTimer = new DispatcherTimer();
            StarTimer.Interval = TimeSpan.FromSeconds(1); // Triggered once every second
            StarTimer.Tick += StarTimer_Tick;
        }

        private void StarTimer_Tick(object sender, EventArgs e)
        {
            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
            RightStatisticsAction?.Invoke(correctAnswers[hardness - 1], LevelUp);
            WrongStatisticsAction?.Invoke(wrongAnswers[hardness - 1], LevelDown);
        }

        protected override async Task OnStartAsync()
        {
            // Set the initial value of the countdown（For example, 10 seconds）
            countdownTime = max_time; // Or you can set it to another value

            timer?.Stop();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start(); // Start the timer

            trainingTimer?.Stop();
            trainingTimer = new DispatcherTimer();
            trainingTimer.Interval = TimeSpan.FromSeconds(1);
            trainingTimer.Tick += TrainingTimer_Tick;
            trainingTimer.Start(); // Start the training timer

            countTimer?.Stop();
            countTimer = new DispatcherTimer();
            countTimer.Interval = TimeSpan.FromSeconds(1);
            countTimer.Tick += Countimer_Tick;
            countTimer.Start();

            // Calling delegate
            VoiceTipAction?.Invoke("Please find a suitable option in the several images provided.");
            SynopsisAction?.Invoke("You will see a sequence of images on the screen, and the images of the sequence have some pattern between each other. You need to think about discovering the pattern and click on the mouse in several images provided to find a suitable option.");
            RuleAction?.Invoke("You will see a sequence of images on the screen, and the images of the sequence have some pattern between each other. You need to think about discovering the pattern and click on the mouse in several images provided to find a suitable option.");//Add code, call function, display the text under the digital person

        }


        protected override async Task OnStopAsync()
        {
            timer?.Stop();
            trainingTimer?.Stop();
            countTimer?.Stop();
        }

        protected override async Task OnPauseAsync()
        {
            timer?.Stop();
            trainingTimer?.Stop();
            countTimer?.Stop();
        }

        protected override async Task OnNextAsync()
        {
            // Adjust the difficulty
            AdjustDifficulty();
            ClearAndLoadNewImages();

            // Calling delegate
            VoiceTipAction?.Invoke("Please find a suitable option in the several images provided.");
            SynopsisAction?.Invoke("You will see a sequence of images on the screen, and the images of the sequence have some pattern between each other. You need to think about discovering the pattern and click on the mouse in several images provided to find a suitable option.");
            RuleAction?.Invoke("You will see a sequence of images on the screen, and the images of the sequence have some pattern between each other. You need to think about discovering the pattern and click on the mouse in several images provided to find a suitable option.");
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new Explanation of logical thinking ability();
        }

        private int GetCorrectNum(int difficultyLevel)
        {
            return correctAnswers[difficultyLevel];
        }
        private int GetWrongNum(int difficultyLevel)
        {
            return wrongAnswers[difficultyLevel];
        }
        private int GetIgnoreNum(int difficultyLevel)
        {
            return ignoreAnswers[difficultyLevel];
        }
        private double GetAverageCostTime(int difficultyLevel)
        {
            return AverageCostTime[difficultyLevel];
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
                        for (int lv = 0; lv < hardness; lv++)
                        {
                            // Get data at the current difficulty level
                            int correctCount = GetCorrectNum(lv);
                            int wrongCount = GetWrongNum(lv);
                            int ignoreCount = GetIgnoreNum(lv);
                            int totalCount = correctCount + wrongCount + ignoreCount;
                            double averageTime = GetAverageCostTime(lv);
                            if (totalCount == 0 && averageTime == 0)
                            {
                                continue;
                            }
                            // Calculation accuracy
                            double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);

                            // create Result Record
                            var newResult = new Result
                            {
                                ProgramId = program_id, // program_id
                                Report = "Logical thinking ability",
                                Eval = false,
                                Lv = lv+1, // Current difficulty level
                                ScheduleId = BaseParameter.ScheduleId ?? null // Assumption Schedule_id, can be replaced with the actual value
                            };//Each difficulty level will be covered, so the maximum difficulty level will be displayed at the end

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
                                    ValueName = "grade",
                                    Value = lv+1,
                                    Maxvalue = 23,
                                    Minvalue = 1,
                                    Charttype = "Bar chart" ,
                                    ModuleId = BaseParameter.ModuleId //  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Number of series pictures",
                                    Value = totalCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "correct",
                                    Value = correctCount,
                                    Maxvalue = correctCount,
                                    Minvalue = 0,
                                    Charttype = "Bar chart" ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Correct（%）",
                                    Value = accuracy * 100, // Stored as a percentage
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
                                    Value =ignoreCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Average answer time",
                                    Value = averageTime,
                                    Maxvalue = (int?)averageTime,
                                    Minvalue = 0,
                                    Charttype = "Bar chart" ,
                                    ModuleId = BaseParameter.ModuleId
                                }
                             };

                            // insert ResultDetail data
                            db.ResultDetails.AddRange(resultDetails);
                            await db.SaveChangesAsync();

                            // Output each ResultDetail Object data
                            Debug.WriteLine($"Difficulty level {lv + 1}:");
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