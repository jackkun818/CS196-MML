using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Automation;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using crs.core;
using crs.core.DbModels;
using System.IO;
using System.Collections.Generic;
using Microsoft.Identity.Client.NativeInterop;
using System.Media;
using static SkiaSharp.SKImageFilter;
using System.Runtime.InteropServices;

namespace crs.game.Games
{
    public partial class Focus_on_attention : BaseUserControl
    {
        private readonly string[] JudgementPath = new string[]
        {
            "VOR/judgement/right.png",
            "VOR/judgement/wrong.png"
        };

        private readonly string[][] imagePaths = new string[][]
        {
            new string[]
{
    "Focus on attention/1/1.png",
    "Focus on attention/1/2.png",
    "Focus on attention/1/3.png",
    "Focus on attention/1/4.png",
    "Focus on attention/1/5.png",
    "Focus on attention/1/6.png",
    "Focus on attention/1/7.png",
    "Focus on attention/1/8.png",
    "Focus on attention/1/9.png",
},
new string[]
{
    "Focus on attention/2/1.png",
    "Focus on attention/2/2.png",
    "Focus on attention/2/3.png",
    "Focus on attention/2/4.png",
    "Focus on attention/2/5.png",
    "Focus on attention/2/6.png",
    "Focus on attention/2/7.png",
    "Focus on attention/2/8.png",
    "Focus on attention/2/9.png",
},
new string[]
{
    "Focus on attention/3/1.png",
    "Focus on attention/3/2.png",
    "Focus on attention/3/3.png",
    "Focus on attention/3/4.png",
    "Focus on attention/3/5.png",
    "Focus on attention/3/6.png",
    "Focus on attention/3/7.png",
    "Focus on attention/3/8.png",
    "Focus on attention/3/9.png",
},
new string[]
{
    "Focus on attention/4/1.png",
    "Focus on attention/4/2.png",
    "Focus on attention/4/3.png",
    "Focus on attention/4/4.png",
    "Focus on attention/4/5.png",
    "Focus on attention/4/6.png",
    "Focus on attention/4/7.png",
    "Focus on attention/4/8.png",
    "Focus on attention/4/9.png",
},
new string[]
{
    "Focus on attention/5/1.png",
    "Focus on attention/5/2.png",
    "Focus on attention/5/3.png",
    "Focus on attention/5/4.png",
    "Focus on attention/5/5.png",
    "Focus on attention/5/6.png",
    "Focus on attention/5/7.png",
    "Focus on attention/5/8.png",
    "Focus on attention/5/9.png",
},
new string[]
{
    "Focus on attention/6/1.png",
    "Focus on attention/6/2.png",
    "Focus on attention/6/3.png",
    "Focus on attention/6/4.png",
    "Focus on attention/6/5.png",
    "Focus on attention/6/6.png",
    "Focus on attention/6/7.png",
    "Focus on attention/6/8.png",
    "Focus on attention/6/9.png",
},
new string[]
{
    "Focus on attention/7/1.png",
    "Focus on attention/7/2.png",
    "Focus on attention/7/3.png",
    "Focus on attention/7/4.png",
    "Focus on attention/7/5.png",
    "Focus on attention/7/6.png",
    "Focus on attention/7/7.png",
    "Focus on attention/7/8.png",
    "Focus on attention/7/9.png",
},
new string[]
{
    "Focus on attention/8/1.png",
    "Focus on attention/8/2.png",
    "Focus on attention/8/3.png",
    "Focus on attention/8/4.png",
    "Focus on attention/8/5.png",
    "Focus on attention/8/6.png",
    "Focus on attention/8/7.png",
    "Focus on attention/8/8.png",
    "Focus on attention/8/9.png",
},
        };

        private int imageCount;
        private int max_time = 60;
        private const int WAIT_DELAY = 1;
        private const int MAX_HARDNESS = 9;
        private int INCREASE; // Increase the threshold for difficulty
        private int DECREASE;  // Threshold for reducing difficulty
        private int TRAIN_TIME = 60; // Training duration（Unit: seconds）
        private bool IS_RESTRICT_TIME = true; // Whether to enable exercise time
        private bool IS_BEEP = true;
        private bool IS_VISUAL = true;
        private int train_time;
        private int counter;
        private int randomIndex;
        private Random random;
        private const int moveAmount = 2;
        private int left;
        private int top;
        private int hardness;
        int max_hardness = 1;
        private int[] correctAnswers; // Store the number of correct answers for each difficulty
        private int[] wrongAnswers; // Store the number of error answers per difficulty
        private int[] igonreAnswer;
        private int[] leftAnswer;
        private int[] rightAnswer;
        private int[] centerAnswer;
        private int[] lefttime;
        private int[] righttime;
        private int[] centertime;
        private DispatcherTimer timer;
        private DispatcherTimer countTimer;
        private int remainingTime;
        private DispatcherTimer trainingTimer; // New timer for training time
        private int correct = 0;
        private int wrong = 0;
        private bool is_game = true;
        private bool is_auto = true; // Whether it jumps automatically
        private bool is_enter = false;


        /// <summary>
        /// Time to answer the question
        /// </summary>
        DateTime startTime = new DateTime();
        DateTime endTime = new DateTime();

        public Focus_on_attention()
        {
            InitializeComponent();
            this.UpdateLayout();
        }
        private void NewAnswer(bool ans)
        {
            if (ans)
            {
                if (wrong != 0)
                {
                    wrong = 0;
                }
                if (correct < INCREASE - 1)
                    correct++;
                else
                    AdjustDifficulty();
            }
            else
            {
                if (correct != 0)
                {
                    correct = 0;
                }
                if (wrong < DECREASE - 1) wrong++;
                else AdjustDifficulty();
            }
            RightStatisticsAction?.Invoke(correct, INCREASE);
            WrongStatisticsAction?.Invoke(wrong, DECREASE);
        }

        private void ShowRandomImage()
        {
            randomIndex = random.Next(imageCount);
            RandomImage.Source = new BitmapImage(new Uri($@"../{imagePaths[(hardness - 1) / 3][randomIndex]}", UriKind.Relative));

            if (IS_RESTRICT_TIME)
            {
                remainingTime = max_time;
                timer.Start();
            }

            startTime = DateTime.Now;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            remainingTime--;

            // Calling delegate
            TimeStatisticsAction?.Invoke(train_time, remainingTime);

            if (remainingTime <= 0)
            {
                timer.Stop();
                igonreAnswer[hardness]++;
                NewAnswer(false);
                ShowRandomImage();
                remainingTime = max_time;
                timer.Start();
            }
        }

        private void TrainingTimer_Tick(object sender, EventArgs e)
        {
            train_time--; // Countdown to training time

            // Calling delegate
            TimeStatisticsAction?.Invoke(train_time, remainingTime);

            if (train_time <= 0)
            {
                timer.Stop(); // Stop the main timer
                trainingTimer.Stop(); // Stop training timer
                //Focus on attention report reportWindow = new Focus on attention report(_INCREASE, _DECREASE, max_time, TRAIN_TIME, IS_RESTRICT_TIME, IS_BEEP, correctAnswers, wrongAnswers, igonreAnswer);
                //reportWindow.ShowDialog(); // Open the report window
                //this.Close(); // Close the current window
                //StopAction?.Invoke();

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
                    Source = new BitmapImage(new Uri($@"../{imagePaths[(hardness - 1) / 3][i]}", UriKind.Relative)),
                    Stretch = System.Windows.Media.Stretch.Uniform
                };
                int row = 1 + (i / 3) * 2;
                int column = 1 + (i % 3) * 2;
                Grid.SetRow(image, row);
                Grid.SetColumn(image, column);
                ImageGrid.Children.Add(image);
            }
        }

        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && is_enter == false)
            {
                endTime = DateTime.Now;
                TimeSpan duration = endTime - startTime; // Calculate the answer time
                int durationInMilliseconds = (int)duration.TotalMilliseconds;
                switch (((top - 1) * 3 / 2 + (left - 1) / 2) % 3)
                {
                    case 0:
                        leftAnswer[hardness]++;
                        lefttime[hardness] += durationInMilliseconds;
                        break;
                    case 1:
                        centerAnswer[hardness]++;
                        centertime[hardness] += durationInMilliseconds;
                        break;
                    case 2:
                        rightAnswer[hardness]++;
                        righttime[hardness] += durationInMilliseconds;
                        break;
                    default:
                        break;
                }

                bool isCorrect = (top - 1) * 3 / 2 + (left - 1) / 2 == randomIndex;
                is_game = false;
                if (isCorrect)
                {
                    correctAnswers[hardness]++; // Update the correct answer count for corresponding difficulty
                    if (IS_VISUAL) ShowFeedbackImage(isCorrect);
                    if (IS_BEEP) PlayWav(CorrectSoundPath);
                    NewAnswer(isCorrect); // Update recent results with new logic
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
                    if (IS_VISUAL) ShowFeedbackImage(isCorrect);
                    if (IS_BEEP) PlayWav(ErrorSoundPath);
                    wrongAnswers[hardness]++; // Update the error answer count for corresponding difficulty
                    NewAnswer(false);
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
                return;
            }
            if (e.Key == Key.Enter && is_enter == true)
            {
                countTimer?.Stop();
                NextQuestion();
                judgement.Visibility = Visibility.Collapsed;
                is_enter = false;
            }
            else if (is_game)
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
        private void AdjustDifficulty()
        {
            timer?.Stop();

            // Increase difficulty
            if (correct >= INCREASE - 1 && hardness < 9)
            {
                hardness++;
                max_hardness = Math.Max(max_hardness, hardness);
                max_time = 58 + hardness * 2;
                remainingTime = max_time;
                imageCount = (hardness % 3) * 3;
                if (imageCount == 0)
                    imageCount = 9;
                correct = 0; wrong = 0;
                LoadImages(imageCount);
            }
            // Reduce difficulty
            else if (wrong >= DECREASE - 1 && hardness > 1)
            {
                hardness--;
                max_time = 58 + hardness * 2;
                remainingTime = max_time;
                imageCount = (hardness % 3) * 3;
                if (imageCount == 0)
                    imageCount = 9;
                correct = 0; wrong = 0;
                LoadImages(imageCount);

            }
            else if (correct == INCREASE && hardness == MAX_HARDNESS)
            {
                correct--;
            }
            else if (wrong == DECREASE && hardness == 1)
                wrong--;

            RightStatisticsAction?.Invoke(correct, INCREASE);
            WrongStatisticsAction?.Invoke(wrong, DECREASE);
            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
        }

        private void ChangeSelectionBoxColor(Color color)
        {
            SelectionBox.Stroke = new SolidColorBrush(color);
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

    public partial class Focus_on_attention : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
            /*LJN
            Configure the path to feedback resources             
             */
            CorrectSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");
            countTimer = new DispatcherTimer();
            countTimer.Interval = TimeSpan.FromSeconds(0);
            countTimer.Tick += Countimer_Tick;
            IS_RESTRICT_TIME = true; // Whether to enable exercise time
            remainingTime = max_time;
            random = new Random();
            counter = 0;
            train_time = TRAIN_TIME;
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
                            case 1: // Treatment time 
                                TRAIN_TIME = par.Value.HasValue ? (int)par.Value.Value : 60;
                                Debug.WriteLine($"TRAIN_TIME={TRAIN_TIME}");
                                break;
                            case 2: // Level improvement
                                INCREASE = par.Value.HasValue ? (int)par.Value.Value : 5;
                                Debug.WriteLine($"INCREASE={INCREASE}");
                                break;
                            case 3: // Level down
                                DECREASE = par.Value.HasValue ? (int)par.Value.Value : 5;
                                Debug.WriteLine($"DECREASE ={DECREASE}");
                                break;
                            case 7: // Auditory feedback
                                IS_BEEP = par.Value == 1;
                                Debug.WriteLine($"Whether to hear feedback ={IS_BEEP}");
                                break;
                            case 9://Visual feedback
                                IS_VISUAL = par.Value == 1;
                                Debug.WriteLine($"Is it visual feedback? ={IS_VISUAL}");
                                break;
                            case 151: //grade
                                hardness = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"HARDNESS ={hardness}");
                                break;
                            case 314: // Limited answer time
                                IS_RESTRICT_TIME = par.Value == 1;
                                Debug.WriteLine($"Is the answer time limit ={IS_RESTRICT_TIME}");
                                break;
                            case 311: // The title jumps automatically
                                is_auto = par.Value == 1;
                                Debug.WriteLine($"Is the question automatically jumping? ={IS_RESTRICT_TIME}");
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

            max_time = 58 + hardness * 2;

            // Initialize a count array of correct and wrong answers
            imageCount = (hardness % 3) * 3;
            if (imageCount == 0)
                imageCount = 9;
            train_time = 60 * TRAIN_TIME;
            //LJN, to prevent the array from crossing the bounds, manually add 1
            correctAnswers = new int[MAX_HARDNESS + 1];
            wrongAnswers = new int[MAX_HARDNESS + 1];
            igonreAnswer = new int[MAX_HARDNESS + 1];
            leftAnswer = new int[MAX_HARDNESS + 1];
            rightAnswer = new int[MAX_HARDNESS + 1];
            centerAnswer = new int[MAX_HARDNESS + 1];
            lefttime = new int[MAX_HARDNESS + 1];
            righttime = new int[MAX_HARDNESS + 1];
            centertime = new int[MAX_HARDNESS + 1];
            for (int i = 0; i < correctAnswers.Length; i++)
            {
                correctAnswers[i] = 0;
                wrongAnswers[i] = 0;
                igonreAnswer[i] = 0;
                leftAnswer[i] = 0;
                rightAnswer[i] = 0;
                centerAnswer[i] = 0;
                lefttime[i] = 0;
                righttime[i] = 0;
                centertime[i] = 0;
            }
            max_hardness = Math.Max(max_hardness, hardness);

            // Calling delegate
            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
            RightStatisticsAction?.Invoke(0, INCREASE);
            WrongStatisticsAction?.Invoke(0, DECREASE);
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

            LoadImages(imageCount);
            ShowRandomImage();
            left = 1;
            top = 1;

            // Calling delegate
            VoiceTipAction?.Invoke("Please find the same picture as the target picture on the right in the picture on the left.");
            SynopsisAction?.Invoke("A target picture will appear on the right side of the screen, and three pictures that are slightly different from the target picture will appear on the left side. Please select one of the pictures through the left and right keys of the keyboard to make it the same as the target picture on the right, and press theOKKey confirms selection.");
            RuleAction?.Invoke("A target picture will appear on the right side of the screen, and three pictures that are slightly different from the target picture will appear on the left side. Please select one of the pictures through the left and right keys of the keyboard to make it the same as the target picture on the right, and press theOKKey confirms selection.");//Add code, call function, display the text under the digital person

        }

        protected override async Task OnStopAsync()
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
            LoadImages(imageCount);
            ShowRandomImage();
            left = 1;
            top = 1;
            // Calling delegate
            VoiceTipAction?.Invoke("Please find the same picture as the target picture on the right in the picture on the left.");
            SynopsisAction?.Invoke("A target picture will appear on the right side of the screen, and three pictures that are slightly different from the target picture will appear on the left side. Please select one of the pictures through the left and right keys of the keyboard to make it the same as the target picture on the right, and press theOKKey confirms selection.");
            RuleAction?.Invoke("A target picture will appear on the right side of the screen, and three pictures that are slightly different from the target picture will appear on the left side. Please select one of the pictures through the left and right keys of the keyboard to make it the same as the target picture on the right, and press theOKKey confirms selection.");
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new Focus on explanation();
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
            return igonreAnswer[difficultyLevel];
        }
        private int GetleftAnswer(int difficultyLevel)
        {
            return leftAnswer[difficultyLevel];
        }
        private int GetcenterAnswer(int difficultyLevel)
        {
            return centerAnswer[difficultyLevel];
        }
        private int GetrightAnswer(int difficultyLevel)
        {
            return rightAnswer[difficultyLevel];
        }
        private int Getrighttime(int difficultyLevel)
        {
            return righttime[difficultyLevel];
        }
        private int Getcentertime(int difficultyLevel)
        {
            return centertime[difficultyLevel];
        }
        private int Getlefttime(int difficultyLevel)
        {
            return lefttime[difficultyLevel];
        }
        private double CalculateAccuracy(int lv)
        {
            int total = correctAnswers[lv] + wrongAnswers[lv] + igonreAnswer[lv];
            int a = GetCorrectNum(lv);
            Debug.WriteLine($"When calculating, the total number is{total}, the correct number is{a}");
            double accuracy = (double)GetCorrectNum(lv) / total;  // Convert to double type
            return total > 0 ? Math.Round(accuracy, 0) : 0;
        }
        private int Total(int lv)
        {
            int total = correctAnswers[lv] + wrongAnswers[lv] + igonreAnswer[lv];
            return total;
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
                        int totalcount = 0;

                        int left_answer = 0;
                        int center_answer = 0;
                        int right_answer = 0;
                        int lefttime = 0;
                        int centertime = 0;
                        int righttime = 0;



                        for (int lv = 1; lv <= max_hardness; lv++)
                        {
                            correctCount += GetCorrectNum(lv);
                            wrongCount += GetWrongNum(lv);
                            ignoreCount += GetIgnoreNum(lv);
                            totalcount += Total(lv);

                            left_answer = GetleftAnswer(lv);
                            center_answer += GetcenterAnswer(lv);
                            right_answer += GetrightAnswer(lv);

                            lefttime += Getlefttime(lv);
                            centertime += Getcentertime(lv);
                            righttime += Getrighttime(lv);

                        }

                        double left_time = 0;
                        if (left_answer != 0)
                        {
                            left_time = (double)lefttime / left_answer;
                            //left_time = (double)left_time / 1000;
                            left_time = Math.Round((double)left_time, 2);  // Keep two decimal places
                        }
                        double center_time = 0;
                        if (center_answer != 0)
                        {
                            center_time = (double)centertime / center_answer;
                            //center_time = (double)centertime / 1000;
                            center_time = Math.Round((double)center_time, 2);  // Keep two decimal places
                        }
                        double right_time = 0;
                        if (right_answer != 0)
                        {
                            right_time = (double)righttime / right_answer;
                            //right_time = (double)right_time / 1000;
                            right_time = Math.Round((double)right_time, 2);  // Keep two decimal places
                        }

                        double accuracy = 0;
                        if (totalcount != 0)
                        {
                            accuracy = (double)(1.0 * correctCount) / totalcount;  // Calculation accuracy
                            accuracy = Math.Round((double)accuracy, 2);  // Keep two decimal places
                        }
                        double acctime = 0;
                        if (totalcount != 0)
                        {
                            acctime = (double)(lefttime + centertime + righttime) / totalcount;
                            acctime = Math.Round((double)acctime, 2);  // Keep two decimal places
                        }

                        // create Result Record
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "Focus on attention",
                            Eval = false,
                            Lv = max_hardness, // Current difficulty level
                            ScheduleId = BaseParameter.ScheduleId ?? null // Assumption Schedule_id, can be replaced with the actual value
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
                                    ValueName = "Average reaction time(Left side)(ms)",
                                    Value = left_time,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Average reaction time(middle)(ms)",
                                    Value = center_time,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Average reaction time(Right side)(ms)",
                                    Value = right_time,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Average reaction time(ms)",
                                    Value = acctime ,
                                    Maxvalue = 2000,
                                    Minvalue = 0,
                                    Charttype = "Bar chart" ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Select the total number",
                                    Value = totalcount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Correct number",
                                    Value = correctCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Error number",
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
                                    Value = accuracy * 100,
                                    Maxvalue = 100,
                                    Minvalue = 0,
                                    Charttype = "Bar chart" ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "grade",
                                    Value = max_hardness,
                                    ModuleId = BaseParameter.ModuleId
                                },

                            };

                        // insert ResultDetail data
                        db.ResultDetails.AddRange(resultDetails);
                        await db.SaveChangesAsync();

                        // Output each ResultDetail Object data
                        Debug.WriteLine($"Difficulty level {max_hardness}:");
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
