using crs.game.Games;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
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
using crs.core;
using crs.core.DbModels;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Media;


namespace crs.game.Games
{
    /// <summary>
    /// VOR.xaml Interaction logic
    /// </summary>
    public partial class Plane_recognition_capability : BaseUserControl
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
    "VOR/1/1.jpg",
    "VOR/1/2.jpg",
    "VOR/1/3.jpg",
    "VOR/1/4.jpg",
    "VOR/1/5.jpg",
    "VOR/1/6.jpg",
    "VOR/1/7.jpg",
    "VOR/1/8.jpg",
    "VOR/1/9.jpg",
},
new string[]
{
    "VOR/2/1.jpg",
    "VOR/2/2.jpg",
    "VOR/2/3.jpg",
    "VOR/2/4.jpg",
    "VOR/2/5.jpg",
    "VOR/2/6.jpg",
    "VOR/2/7.jpg",
    "VOR/2/8.jpg",
    "VOR/2/9.jpg",
},
new string[]
{
    "VOR/3/1.jpg",
    "VOR/3/2.jpg",
    "VOR/3/3.jpg",
    "VOR/3/4.jpg",
    "VOR/3/5.jpg",
    "VOR/3/6.jpg",
    "VOR/3/7.jpg",
    "VOR/3/8.jpg",
    "VOR/3/9.jpg",
},
new string[]
{
    "VOR/4/1.jpg",
    "VOR/4/2.jpg",
    "VOR/4/3.jpg",
    "VOR/4/4.jpg",
    "VOR/4/5.jpg",
    "VOR/4/6.jpg",
    "VOR/4/7.jpg",
    "VOR/4/8.jpg",
    "VOR/4/9.jpg",
},
new string[]
{
    "VOR/5/1.jpg",
    "VOR/5/2.jpg",
    "VOR/5/3.jpg",
    "VOR/5/4.jpg",
    "VOR/5/5.jpg",
    "VOR/5/6.jpg",
    "VOR/5/7.jpg",
    "VOR/5/8.jpg",
    "VOR/5/9.jpg",
},
new string[]
{
    "VOR/6/1.jpg",
    "VOR/6/2.jpg",
    "VOR/6/3.jpg",
    "VOR/6/4.jpg",
    "VOR/6/5.jpg",
    "VOR/6/6.jpg",
    "VOR/6/7.jpg",
    "VOR/6/8.jpg",
    "VOR/6/9.jpg",
},
new string[]
{
    "VOR/7/1.jpg",
    "VOR/7/2.jpg",
    "VOR/7/3.jpg",
    "VOR/7/4.jpg",
    "VOR/7/5.jpg",
    "VOR/7/6.jpg",
    "VOR/7/7.jpg",
    "VOR/7/8.jpg",
    "VOR/7/9.jpg",
},
new string[]
{
    "VOR/8/1.jpg",
    "VOR/8/2.jpg",
    "VOR/8/3.jpg",
    "VOR/8/4.jpg",
    "VOR/8/5.jpg",
    "VOR/8/6.jpg",
    "VOR/8/7.jpg",
    "VOR/8/8.jpg",
    "VOR/8/9.jpg",
},

        };

        private int imageCount;
        private int max_time = 60;
        private int WAIT_DELAY = 1;
        private const int MAX_HARDNESS = 24;
        //private int INCREASE = 5; // Increase the threshold for difficulty
        //private int DECREASE = 5;  // Threshold for reducing difficulty
        private int TRAIN_TIME = 60; // Training duration（Unit: seconds）
        private int cost_time;
        private bool IS_RESTRICT_TIME = true; // Whether to enable exercise time
        private bool IS_BEEP = true;
        private bool IS_VISUAL = true;
        private int train_time;
        private int counter;
        private int randomIndex;
        private Random random;
        private Random randomforrotate;
        private const int moveAmount = 2;
        private int left;
        private int top;
        private int hardness;
        private int max_hardness = 0;
        private int[] correctAnswers; // Store the number of correct answers for each difficulty
        private int[] wrongAnswers; // Store the number of error answers per difficulty
        private int[] igonreAnswer;
        private DispatcherTimer timer;
        private int remainingTime;
        private DispatcherTimer trainingTimer; // New timer for training time
        private DispatcherTimer countTimer;
        private int[] TotalCountByHardness;
        private double[] TotalAccuracyByHardness;
        private double[] AverageTimeByHardness;
        private List<int>[] CostTime;
        private int[] MinCostTime;
        private int[] MaxCostTime;
        private int[][] RotationDegreeList;

        private int LevelUp = 3;
        private int LevelDown = 5;

        private bool is_game = true;
        private bool is_auto = true; // Whether it jumps automatically
        private bool is_enter = false;
        public Plane_recognition_capability()
        {
            InitializeComponent();
        }


        private void ShowRandomImage()
        {
            randomIndex = random.Next(imageCount);
            RandomImage.Source = new BitmapImage(new Uri(imagePaths[(hardness - 1) / 3][randomIndex], UriKind.Relative));

            if (IS_RESTRICT_TIME)
            {
                remainingTime = max_time - (hardness - 1) * 2;
                timer.Start();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {

            remainingTime--;
            cost_time += 1;
            TimeStatisticsAction.Invoke(train_time, remainingTime);

            if (remainingTime <= 0)
            {
                timer.Stop();
                igonreAnswer[hardness]++;
                //wrongAnswers[hardness]++;
                LoadImages(imageCount);
                ShowRandomImage();
                remainingTime = max_time - (hardness - 1) * 2;
                timer.Start();
            }
            TotalCountByHardness[hardness - 1] = correctAnswers[hardness - 1] + wrongAnswers[hardness - 1];
            if (TotalCountByHardness[hardness - 1] != 0)
            {
                TotalAccuracyByHardness[hardness - 1] = correctAnswers[hardness - 1] / TotalCountByHardness[hardness - 1];
            }
        }

        private void TrainingTimer_Tick(object sender, EventArgs e)
        {
            train_time--; // Countdown to training time
            TimeStatisticsAction.Invoke(train_time, remainingTime);

            if (train_time <= 0)
            {
                timer.Stop(); // Stop the main timer
                trainingTimer.Stop(); // Stop training timer
                //VOR_Report reportWindow = new VOR_Report(INCREASE, DECREASE, max_time, TRAIN_TIME, IS_RESTRICT_TIME, IS_BEEP, correctAnswers, wrongAnswers, igonreAnswer);
                //reportWindow.Show(); // Open the report window
                OnGameEnd();
            }
        }
        private void Countimer_Tick(object sender, EventArgs e)
        {

            countTimer?.Stop();
            NextQuestion();
            judgement.Visibility = Visibility.Collapsed;
            is_enter = false;
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
                if (hardness >= 4 && hardness <= 6)
                {
                    rotationDegrees = randomforrotate.Next(4) * 90;
                    var rotateTransform = new RotateTransform { Angle = rotationDegrees };
                    image.RenderTransform = rotateTransform;
                    image.RenderTransformOrigin = new Point(0.5, 0.5);
                }
                if (hardness >= 7 && hardness <= 9)
                {
                    rotationDegrees = randomforrotate.Next(4) * 90;
                    var rotateTransform = new RotateTransform { Angle = rotationDegrees };
                    image.RenderTransform = rotateTransform;
                    image.RenderTransformOrigin = new Point(0.5, 0.5);
                }
                if (hardness >= 10 && hardness <= 12)
                {
                    rotationDegrees = randomforrotate.Next(36) * 90;
                    var rotateTransform = new RotateTransform { Angle = rotationDegrees };
                    image.RenderTransform = rotateTransform;
                    image.RenderTransformOrigin = new Point(0.5, 0.5);
                }
                if (hardness >= 13 && hardness <= 15)
                {
                    rotationDegrees = randomforrotate.Next(4) * 90;
                    var rotateTransform = new RotateTransform { Angle = rotationDegrees };
                    image.RenderTransform = rotateTransform;
                    image.RenderTransformOrigin = new Point(0.5, 0.5);
                }
                if (hardness >= 16 && hardness <= 18)
                {
                    rotationDegrees = randomforrotate.Next(36) * 90;
                    var rotateTransform = new RotateTransform { Angle = rotationDegrees };
                    image.RenderTransform = rotateTransform;
                    image.RenderTransformOrigin = new Point(0.5, 0.5);
                }
                if (hardness >= 19 && hardness <= 21)
                {
                    rotationDegrees = randomforrotate.Next(4) * 90;
                    var rotateTransform = new RotateTransform { Angle = rotationDegrees };
                    image.RenderTransform = rotateTransform;
                    image.RenderTransformOrigin = new Point(0.5, 0.5);
                }
                if (hardness >= 22 && hardness <= 24)
                {
                    rotationDegrees = randomforrotate.Next(36) * 90;
                    var rotateTransform = new RotateTransform { Angle = rotationDegrees };
                    image.RenderTransform = rotateTransform;
                    image.RenderTransformOrigin = new Point(0.5, 0.5);

                }

                if (i < 3) RotationDegreeList[1][i + 1] = rotationDegrees;
                if (i >= 3 && i < 6) RotationDegreeList[2][i % 3 + 1] = rotationDegrees;
                if (i >= 6) RotationDegreeList[3][i % 3 + 1] = rotationDegrees;

                int row = 1 + (i / 3) * 2;
                int column = 1 + (i % 3) * 2;
                Grid.SetRow(image, row);
                Grid.SetColumn(image, column);
                ImageGrid.Children.Add(image);
            }
            //top = 1;
            //left = 1;
        }

        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

            if (e.Key == Key.Enter && is_enter == false)
            {

                CostTime[hardness - 1].Add(cost_time);
                cost_time = 0;
                MinCostTime[hardness - 1] = CostTime[hardness - 1].Min();
                MaxCostTime[hardness - 1] = CostTime[hardness - 1].Max();
                is_game = false;
                bool isCorrect = (top - 1) * 3 / 2 + (left - 1) / 2 == randomIndex;
                if (isCorrect)
                {
                    //textblock.Text = "Congratulations on getting right！";
                    //textblock.Foreground = new SolidColorBrush(Colors.Green);
                    if (IS_VISUAL) ShowFeedbackImage(isCorrect);
                    if (IS_BEEP) PlayWav(CorrectSoundPath);
                    correctAnswers[hardness]++; // Update the correct answer count for corresponding difficulty
                    recentResults.Add(true);

                    if (is_auto)
                    {
                        is_enter = true;
                        timer?.Stop();
                        countTimer.Interval = TimeSpan.FromSeconds(3);
                        ChangeSelectionBoxColor((Color)ColorConverter.ConvertFromString("#00ff00"));
                        countTimer.Start();
                    }
                    else
                    {
                        is_enter = true;
                        timer?.Stop();
                        ChangeSelectionBoxColor((Color)ColorConverter.ConvertFromString("#00ff00"));
                    }

                }
                else
                {
                    //textblock.Text = "Sorry to answer wrong！";
                    //textblock.Foreground = new SolidColorBrush(Colors.Red);
                    if (IS_VISUAL) ShowFeedbackImage(isCorrect);
                    if (IS_BEEP) PlayWav(ErrorSoundPath);
                    wrongAnswers[hardness]++; // Update the error answer count for corresponding difficulty
                    recentResults.Add(false);
                    //if (IS_BEEP)
                    //    Console.Beep();

                    if (is_auto)
                    {
                        is_enter = true;
                        timer?.Stop();
                        countTimer.Interval = TimeSpan.FromSeconds(5);
                        ChangeSelectionBoxColor((Color)ColorConverter.ConvertFromString("#ff0000"));
                        countTimer.Start();

                    }
                    else
                    {
                        is_enter = true;
                        timer?.Stop();
                        ChangeSelectionBoxColor((Color)ColorConverter.ConvertFromString("#ff0000"));
                    }

                }
                AdjustDifficulty();
                return;
            }
            if (e.Key == Key.Enter && is_enter == true)
            {
                countTimer?.Stop();
                NextQuestion();
                judgement.Visibility = Visibility.Collapsed;
                is_enter = false;
            }
            else if (is_game && is_enter == false)
            {
                if (top > 1 && e.Key == Key.Up)
                    top -= moveAmount;
                if (top < (imageCount / 3) * 2 - 1 && e.Key == Key.Down)
                    top += moveAmount;
                if (left > 1 && e.Key == Key.Left)
                    left -= moveAmount;
                if (left < 5 && e.Key == Key.Right)
                    left += moveAmount;
            }
            Grid.SetColumn(SelectionBox, left);
            Grid.SetRow(SelectionBox, top);
            //var rotateTransform = new RotateTransform { Angle = RotationDegreeList[(top+1)/2][(left+1)/2] };
            //SelectionBox.RenderTransform = rotateTransform;
            //SelectionBox.RenderTransformOrigin = new Point(0.5, 0.5);


        }

        // Add a list to the class to store the results of the last 5 questions
        private List<bool> recentResults = new List<bool>();


        private void resetboollist()
        {
            recentResults.Clear();
        }
        private void AdjustDifficulty()
        {
            int correctCount = 0;
            int wrongCount = 0;
            // Add the current question results torecentResultsList

            // Only the last 5 results are retained
            int max = Math.Max(LevelUp, LevelDown); // AssumptionsMaxyesMathIn the classMaxmethod

            // make surerecentResultsThe size of the set does not exceedmax
            if (recentResults.Count > max)
            {
                recentResults.RemoveAt(0); // Remove the first element
            }

            if (recentResults.Count == Math.Min(LevelUp, LevelDown))
            {
                // Calculate the number of correct answers in recent questions
                for (int i = recentResults.Count - LevelUp; i < recentResults.Count; i++)
                {
                    correctCount += recentResults[i] ? 1 : 0; // AssumptionsrecentResults[i]yesboolType, if correct, add 1
                }
                for (int i = recentResults.Count - LevelDown; i < recentResults.Count; i++)
                {
                    wrongCount += recentResults[i] ? 0 : 1; // AssumptionsrecentResults[i]yesboolType, if error is added 1
                }

                // Increase difficulty
                if (correctCount == LevelUp && hardness < 24)
                {
                    hardness++;
                    max_hardness = Math.Max(max_hardness, hardness);
                    resetboollist(); // Assumptionsresetboollistis a method for resettingboollist


                }

                // Reduce difficulty
                else if (wrongCount == LevelDown && hardness > 1) // It should bewrongCount == LevelDown
                {
                    hardness--;
                    resetboollist(); // Assumptionsresetboollistis a method for resettingboollist


                }
            }


            correctCount = recentResults.Count(result => result == true);
            wrongCount = recentResults.Count(result => result == false);
            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
            RightStatisticsAction?.Invoke(correctCount, LevelUp);
            WrongStatisticsAction?.Invoke(wrongCount, LevelDown);
        }

        private void ChangeSelectionBoxColor(Color color)
        {
            SelectionBox.Stroke = new SolidColorBrush(color);
        }

        private void NextQuestion()
        {

            SelectionBox.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3a98d1"));
            is_game = true;
            left = 1;
            top = 1;
            Grid.SetColumn(SelectionBox, left);
            Grid.SetRow(SelectionBox, top);
            ShowRandomImage();
            imageCount = (hardness % 3) * 3;
            if (imageCount == 0)
                imageCount = 9;
            LoadImages(imageCount);
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

        private async void ShowFeedbackImage(bool CorrectOrError)
        {//Image showing feedback
            if (CorrectOrError)
            {//It's right
                judgement.Source = new BitmapImage(new Uri(JudgementPath[0], UriKind.Relative));
                judgement.Visibility = Visibility.Visible;


                if (is_auto)
                {
                    await Task.Delay(3000);
                    judgement.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                judgement.Source = new BitmapImage(new Uri(JudgementPath[1], UriKind.Relative));
                judgement.Visibility = Visibility.Visible;

                if (is_auto)
                {
                    await Task.Delay(5000);
                    judgement.Visibility = Visibility.Collapsed;
                }
            }
        }



    }
    public partial class Plane_recognition_capability : BaseUserControl
    {

        protected override async Task OnInitAsync()
        {
            /*LJN
            Configure the path to feedback resources             
             */
            CorrectSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");
            //this.KeyDown += Window_KeyDown;
            //Debug.WriteLine("INIT");
            //var baseParameter = BaseParameter;
            int increase; int decrease; int mt; int tt; bool irt; bool ib; int hardness_;

            // The parameters passed by the client should be preceded by the client. Currently, test data is used first.
            {


                int max_time = 60;
                int INCREASE = 0; // Increase the threshold for difficulty
                int DECREASE = 0;  // Threshold for reducing difficulty
                int TRAIN_TIME = 60; // Training duration（Unit: seconds）
                bool IS_RESTRICT_TIME = true; // Whether to enable exercise time
                bool IS_BEEP = true; // Whether to make a sound
                int hardness = 1; // Difficulty level


                increase = INCREASE;
                decrease = DECREASE;
                mt = max_time;
                tt = TRAIN_TIME;
                irt = IS_RESTRICT_TIME;
                ib = IS_BEEP;
                hardness_ = hardness;
            }


            max_time = mt;
            //INCREASE = increase; // Increase the threshold for difficulty
            //DECREASE = decrease;  // Threshold for reducing difficulty
            LevelUp = increase; // Increase the threshold for difficulty
            LevelDown = decrease;  // Threshold for reducing difficulty
            TRAIN_TIME = tt; // Training duration（Unit: seconds）
            IS_RESTRICT_TIME = irt; // Whether to enable exercise time
            IS_BEEP = ib;
            hardness = hardness_;
            remainingTime = max_time - (hardness - 1) * 2; ;
            imageCount = (hardness % 3) * 3;
            if (imageCount == 0)
                imageCount = 9;
            random = new Random();
            counter = 0;



            {
                // parameter（Includes module parameter information）
                var baseParameter = BaseParameter;
                if (baseParameter.ProgramModulePars != null && baseParameter.ProgramModulePars.Any())
                {
                    Debug.WriteLine("ProgramModulePars Loaded data:");
                    // Traversal ProgramModulePars Print out each parameter
                    foreach (var par in baseParameter.ProgramModulePars)
                    {
                        //Debug.WriteLine($"ProgramId: {par.ProgramId}, ModuleParId: {par.ModuleParId}, Value: {par.Value}, TableId: {par.TableId}");
                        if (par.ModulePar.ModuleId == baseParameter.ModuleId)
                        {
                            switch (par.ModuleParId)
                            {
                                case 47: // Treatment time
                                    TRAIN_TIME = par.Value.HasValue ? (int)par.Value.Value * 60 : 25;
                                    Debug.WriteLine($"TRAIN_TIME={TRAIN_TIME}");
                                    break;
                                case 48: // Level improvement
                                    LevelUp = par.Value.HasValue ? (int)par.Value.Value : 3;
                                    Debug.WriteLine($"LevelUp={LevelUp}");
                                    break;
                                case 49: // Level down
                                    LevelDown = par.Value.HasValue ? (int)par.Value.Value : 3;
                                    Debug.WriteLine($"LevelDown ={LevelDown}");
                                    break;
                                case 53: // Auditory feedback
                                    IS_BEEP = par.Value == 1;
                                    Debug.WriteLine($"Whether to hear feedback ={IS_BEEP}");
                                    break;
                                case 51: // Answer time limit
                                    IS_RESTRICT_TIME = par.Value == 1;
                                    Debug.WriteLine($"Is there a time limit for answers ={IS_RESTRICT_TIME}");
                                    break;
                                // Add other things that need to be processed ModuleParId
                                case 170://grade
                                    hardness = par.Value.HasValue ? (int)par.Value.Value : 1;
                                    Debug.WriteLine($"HARDNESS: {hardness}");
                                    break;
                                case 257:
                                    IS_VISUAL = par.Value == 1;
                                    Debug.WriteLine($"Is it visual feedback? ={IS_VISUAL}");
                                    break;
                                case 273:
                                    is_auto = par.Value == 1;
                                    Debug.WriteLine($"Is the question automatically jumped ={is_auto}");
                                    break;
                                default:// Initial difficulty
                                    //hardness = par.Value.HasValue ? (int)par.Value.Value : 1;
                                    //Debug.WriteLine($"HARDNESS: {hardness}");
                                    break;

                            }
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("No data");
                }
            }


            train_time = TRAIN_TIME;
            // Initialize a count array of correct and wrong answers
            RotationDegreeList = new int[4][];

            for (int i = 0; i < 4; i++)
            {
                RotationDegreeList[i] = new int[4];
                for (int j = 0; j < 4; j++)
                {
                    RotationDegreeList[i][j] = 0;
                }
            }

            correctAnswers = new int[MAX_HARDNESS + 1];
            wrongAnswers = new int[MAX_HARDNESS + 1];
            igonreAnswer = new int[MAX_HARDNESS + 1];
            TotalCountByHardness = new int[MAX_HARDNESS + 1];
            TotalAccuracyByHardness = new double[MAX_HARDNESS + 1];
            CostTime = new List<int>[MAX_HARDNESS + 1];
            MinCostTime = new int[MAX_HARDNESS + 1];
            MaxCostTime = new int[MAX_HARDNESS + 1];
            for (int i = 0; i < correctAnswers.Length; i++)
            {
                correctAnswers[i] = 0;
                wrongAnswers[i] = 0;
                igonreAnswer[i] = 0;
                TotalAccuracyByHardness[i] = 0;
                TotalCountByHardness[i] = 0;
                CostTime[i] = new List<int>();
                MinCostTime[i] = 0;
                MaxCostTime[i] = 0;
            }

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            trainingTimer = new DispatcherTimer();
            trainingTimer.Interval = TimeSpan.FromSeconds(1);
            trainingTimer.Tick += TrainingTimer_Tick;

            countTimer = new DispatcherTimer();
            countTimer.Interval = TimeSpan.FromSeconds(0);
            countTimer.Tick += Countimer_Tick;
            // Calling delegate
            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
            RightStatisticsAction?.Invoke(0, LevelUp);
            WrongStatisticsAction?.Invoke(0, LevelDown);
        }

        protected override async Task OnStartAsync()
        {

            timer?.Stop();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            trainingTimer?.Stop();
            trainingTimer = new DispatcherTimer();
            trainingTimer.Interval = TimeSpan.FromSeconds(1);
            trainingTimer.Tick += TrainingTimer_Tick;
            trainingTimer.Start(); // Start the training timer
            imageCount = (hardness % 3) * 3;
            if (imageCount == 0)
                imageCount = 9;
            LoadImages(imageCount);
            left = 1;
            top = 1;
            ShowRandomImage();
            //var rotateTransform = new RotateTransform { Angle = RotationDegreeList[(top + 1) / 2][(left + 1) / 2] };
            //SelectionBox.RenderTransform = rotateTransform;
            //SelectionBox.RenderTransformOrigin = new Point(0.5, 0.5);

            // Calling delegate
            VoiceTipAction?.Invoke("Please identify on the screen which image is obtained by rotating the image on the right side of the screen.");
            SynopsisAction?.Invoke("You will see an image on the right side of the screen, and you will see several different images after being rotated on the left side. You need to identify on the screen which image is obtained by rotating the image on the right side of the screen, and select it through the up, down, left and right keys of the keyboard and pressOKKey confirmation.");
            RuleAction?.Invoke("You will see an image on the right side of the screen, and you will see several different images after being rotated on the left side. You need to identify on the screen which image is obtained by rotating the image on the right side of the screen, and select it through the up, down, left and right keys of the keyboard and pressOKKey confirmation.");//Add code, call function, display the text under the digital person

        }

        protected override async Task OnStopAsync()//Need to be inserted
        {
            timer?.Stop();
            trainingTimer?.Stop();
        }

        protected override async Task OnPauseAsync()
        {
            timer?.Stop();
            trainingTimer?.Stop();
        }

        protected override async Task OnNextAsync()
        {

            ShowRandomImage();

            // Calling delegate
            VoiceTipAction?.Invoke("Please identify on the screen which image is obtained by rotating the image on the right side of the screen.");
            SynopsisAction?.Invoke("You will see an image on the right side of the screen, and you will see several different images after being rotated on the left side. You need to identify on the screen which image is obtained by rotating the image on the right side of the screen, and select it through the up, down, left and right keys of the keyboard and pressOKKey confirmation.");
            RuleAction?.Invoke("You will see an image on the right side of the screen, and you will see several different images after being rotated on the left side. You need to identify on the screen which image is obtained by rotating the image on the right side of the screen, and select it through the up, down, left and right keys of the keyboard and pressOKKey confirmation.");
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new Explanation of plane recognition capability();
        }

        // Insert writing
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
            return igonreAnswer[difficultyLevel];
        }
        private int GetMinCostTime(int difficultyLevel)
        {
            return MinCostTime[difficultyLevel - 1];
        }
        private int GetMaxCostTime(int difficultyLevel)
        {
            return MaxCostTime[difficultyLevel - 1];
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
                        int correctCount = 0;
                        int wrongCount = 0;
                        int ignoreCount = 0;
                        int totalCount = 0;
                        int mincosttimeCount = 10;
                        int maxcosttimeCount = 0;
                        // Calculation accuracy
                        double accuracy = 0;
                        //int correctCount = GetCorrectNum(lv);
                        //int wrongCount = GetWrongNum(lv);
                        //int ignoreCount = GetIgnoreNum(lv);
                        //int totalCount = correctCount + wrongCount + ignoreCount;
                        //int mincosttimeCount = GetMinCostTime(lv);
                        //int maxcosttimeCount = GetMaxCostTime(lv);
                        //if (totalCount == 0 && mincosttimeCount == 0 && maxcosttimeCount == 0)
                        //{
                        //    continue;
                        //}
                        //// Calculation accuracy
                        //double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);

                        max_hardness = Math.Max(max_hardness, hardness);//Before generating a report, manually update the maximum difficulty
                        for (int lv = 1; lv <= max_hardness; lv++)
                        {
                            correctCount += GetCorrectNum(lv);
                            wrongCount += GetWrongNum(lv);
                            ignoreCount += GetIgnoreNum(lv);
                            mincosttimeCount = Math.Min(mincosttimeCount, CostTime[lv - 1].Min());
                            maxcosttimeCount = Math.Max(maxcosttimeCount, CostTime[lv - 1].Max());
                        }
                        totalCount = correctCount + wrongCount + ignoreCount;
                        accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);

                        // create Result Record
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "Plane recognition capability",
                            Eval = false,
                            Lv = max_hardness, // Current difficulty level
                            ScheduleId = BaseParameter.ScheduleId ?? null// Assumption Schedule_id, can be replaced with the actual value
                        };
                        db.Results.Add(newResult);
                        await db.SaveChangesAsync(); //Commented here
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
                                    Value = hardness,
                                    Maxvalue = 24,
                                    Minvalue = 1,
                                    ModuleId = BaseParameter.ModuleId //  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "all",
                                    Value = totalCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "mistake",
                                    Value = wrongCount,
                                    Maxvalue = wrongCount,
                                    Minvalue = 1,
                                    Charttype = "Bar chart" ,
                                    ModuleId =  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "Leaked",
                                    Value = ignoreCount,
                                    ModuleId =  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                   ResultId = result_id,
                                   Order = 1,
                                    ValueName = "Minimum reaction time(s)",
                                    Value = mincosttimeCount,
                                    ModuleId =  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "Maximum reaction time(s)",
                                    Value = maxcosttimeCount,
                                    ModuleId =  BaseParameter.ModuleId
                                }
                            };
                        // insert ResultDetail data
                        db.ResultDetails.AddRange(resultDetails);
                        await db.SaveChangesAsync();
                        // Output each ResultDetail Object data
                        Debug.WriteLine($"Difficulty level:");
                        foreach (var detail in resultDetails)
                        {
                            Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId} ");
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
