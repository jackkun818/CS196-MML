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
    public partial class 专注注意力 : BaseUserControl
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
    "专注注意力/1/1.png",
    "专注注意力/1/2.png",
    "专注注意力/1/3.png",
    "专注注意力/1/4.png",
    "专注注意力/1/5.png",
    "专注注意力/1/6.png",
    "专注注意力/1/7.png",
    "专注注意力/1/8.png",
    "专注注意力/1/9.png",
},
new string[]
{
    "专注注意力/2/1.png",
    "专注注意力/2/2.png",
    "专注注意力/2/3.png",
    "专注注意力/2/4.png",
    "专注注意力/2/5.png",
    "专注注意力/2/6.png",
    "专注注意力/2/7.png",
    "专注注意力/2/8.png",
    "专注注意力/2/9.png",
},
new string[]
{
    "专注注意力/3/1.png",
    "专注注意力/3/2.png",
    "专注注意力/3/3.png",
    "专注注意力/3/4.png",
    "专注注意力/3/5.png",
    "专注注意力/3/6.png",
    "专注注意力/3/7.png",
    "专注注意力/3/8.png",
    "专注注意力/3/9.png",
},
new string[]
{
    "专注注意力/4/1.png",
    "专注注意力/4/2.png",
    "专注注意力/4/3.png",
    "专注注意力/4/4.png",
    "专注注意力/4/5.png",
    "专注注意力/4/6.png",
    "专注注意力/4/7.png",
    "专注注意力/4/8.png",
    "专注注意力/4/9.png",
},
new string[]
{
    "专注注意力/5/1.png",
    "专注注意力/5/2.png",
    "专注注意力/5/3.png",
    "专注注意力/5/4.png",
    "专注注意力/5/5.png",
    "专注注意力/5/6.png",
    "专注注意力/5/7.png",
    "专注注意力/5/8.png",
    "专注注意力/5/9.png",
},
new string[]
{
    "专注注意力/6/1.png",
    "专注注意力/6/2.png",
    "专注注意力/6/3.png",
    "专注注意力/6/4.png",
    "专注注意力/6/5.png",
    "专注注意力/6/6.png",
    "专注注意力/6/7.png",
    "专注注意力/6/8.png",
    "专注注意力/6/9.png",
},
new string[]
{
    "专注注意力/7/1.png",
    "专注注意力/7/2.png",
    "专注注意力/7/3.png",
    "专注注意力/7/4.png",
    "专注注意力/7/5.png",
    "专注注意力/7/6.png",
    "专注注意力/7/7.png",
    "专注注意力/7/8.png",
    "专注注意力/7/9.png",
},
new string[]
{
    "专注注意力/8/1.png",
    "专注注意力/8/2.png",
    "专注注意力/8/3.png",
    "专注注意力/8/4.png",
    "专注注意力/8/5.png",
    "专注注意力/8/6.png",
    "专注注意力/8/7.png",
    "专注注意力/8/8.png",
    "专注注意力/8/9.png",
},
        };

        private int imageCount;
        private int max_time = 60;
        private const int WAIT_DELAY = 1;
        private const int MAX_HARDNESS = 9;
        private int INCREASE; // 提高难度的阈值
        private int DECREASE;  // 降低难度的阈值
        private int TRAIN_TIME = 60; // 训练持续时间（单位：秒）
        private bool IS_RESTRICT_TIME = true; // 限制练习时间是否启用
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
        private int[] correctAnswers; // 存储每个难度的正确答案数量
        private int[] wrongAnswers; // 存储每个难度的错误答案数量
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
        private DispatcherTimer trainingTimer; // 新的计时器用于训练时间
        private int correct = 0;
        private int wrong = 0;
        private bool is_game = true;
        private bool is_auto = true; // 是否自动跳转
        private bool is_enter = false;


        /// <summary>
        /// 答题所用的时间
        /// </summary>
        DateTime startTime = new DateTime();
        DateTime endTime = new DateTime();

        public 专注注意力()
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

            // 调用委托
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
            train_time--; // 训练时间倒计时

            // 调用委托
            TimeStatisticsAction?.Invoke(train_time, remainingTime);

            if (train_time <= 0)
            {
                timer.Stop(); // 停止主计时器
                trainingTimer.Stop(); // 停止训练计时器
                //专注注意力报告 reportWindow = new 专注注意力报告(_INCREASE, _DECREASE, max_time, TRAIN_TIME, IS_RESTRICT_TIME, IS_BEEP, correctAnswers, wrongAnswers, igonreAnswer);
                //reportWindow.ShowDialog(); // 打开报告窗口
                //this.Close(); // 关闭当前窗口
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
            // 清空之前的图片
            for (int i = ImageGrid.Children.Count - 1; i >= 0; i--)
            {
                if (ImageGrid.Children[i] is Image)
                {
                    ImageGrid.Children.RemoveAt(i);
                }
            }
            // 加载新图片
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
                TimeSpan duration = endTime - startTime; // 计算作答时间
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
                    correctAnswers[hardness]++; // 更新对应难度的正确答案计数
                    if (IS_VISUAL) ShowFeedbackImage(isCorrect);
                    if (IS_BEEP) PlayWav(CorrectSoundPath);
                    NewAnswer(isCorrect); // 使用新的逻辑更新最近的结果
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
                    wrongAnswers[hardness]++; // 更新对应难度的错误答案计数
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

            // 提高难度
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
            // 降低难度
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
         添加进来视觉、声音反馈的资源
         */
        private SoundPlayer soundPlayer; // 用来放歌
        public string ErrorSoundPath;//错误的声音文件路径，在OnStartAsync()中配置
        public string CorrectSoundPath;//正确的声音文件路径，在OnStartAsync()中配置
        private int StopDurations = 2000; // 停止时间，ms

        private string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory; // 当前项目的绝对路径
        private string ResourcesPath = System.IO.Path.Combine("Resources", "词语记忆力");//这里用的固定是词语记忆力的路径，后期可改

        private void PlayWav(string filePath)
        {//播放本地的wav文件
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
        {//显示反馈的图片
            if (CorrectOrError)
            {//是答对
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

    public partial class 专注注意力 : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
            /*LJN
            配置反馈资源的路径             
             */
            CorrectSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");
            countTimer = new DispatcherTimer();
            countTimer.Interval = TimeSpan.FromSeconds(0);
            countTimer.Tick += Countimer_Tick;
            IS_RESTRICT_TIME = true; // 限制练习时间是否启用
            remainingTime = max_time;
            random = new Random();
            counter = 0;
            train_time = TRAIN_TIME;
            var baseParameter = BaseParameter;
            if (baseParameter.ProgramModulePars != null && baseParameter.ProgramModulePars.Any())
            {
                Debug.WriteLine("ProgramModulePars 已加载数据：");

                // 遍历 ProgramModulePars 打印出每个参数
                foreach (var par in baseParameter.ProgramModulePars)
                {
                    /*Debug.WriteLine($"ProgramId: {par.ProgramId}, ModuleParId: {par.ModuleParId}, Value: {par.Value}, TableId: {par.TableId}");*/
                    if (par.ModulePar.ModuleId == baseParameter.ModuleId)
                    {
                        switch (par.ModuleParId) // 完成赋值
                        {
                            case 1: // 治疗时间 
                                TRAIN_TIME = par.Value.HasValue ? (int)par.Value.Value : 60;
                                Debug.WriteLine($"TRAIN_TIME={TRAIN_TIME}");
                                break;
                            case 2: // 等级提高
                                INCREASE = par.Value.HasValue ? (int)par.Value.Value : 5;
                                Debug.WriteLine($"INCREASE={INCREASE}");
                                break;
                            case 3: // 等级降低
                                DECREASE = par.Value.HasValue ? (int)par.Value.Value : 5;
                                Debug.WriteLine($"DECREASE ={DECREASE}");
                                break;
                            case 7: // 听觉反馈
                                IS_BEEP = par.Value == 1;
                                Debug.WriteLine($"是否听觉反馈 ={IS_BEEP}");
                                break;
                            case 9://视觉反馈
                                IS_VISUAL = par.Value == 1;
                                Debug.WriteLine($"是否视觉反馈 ={IS_VISUAL}");
                                break;
                            case 151: //等级
                                hardness = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"HARDNESS ={hardness}");
                                break;
                            case 314: // 限制作答时间
                                IS_RESTRICT_TIME = par.Value == 1;
                                Debug.WriteLine($"是否限制作答时间 ={IS_RESTRICT_TIME}");
                                break;
                            case 311: // 题目自动跳转
                                is_auto = par.Value == 1;
                                Debug.WriteLine($"题目是否自动跳转 ={IS_RESTRICT_TIME}");
                                break;
                            // 添加其他需要处理的 ModuleParId
                            default:
                                Debug.WriteLine($"未处理的 ModuleParId: {par.ModuleParId}");
                                break;
                        }
                    }
                }
            }
            else
            {
                Debug.WriteLine("没有数据");
            }

            max_time = 58 + hardness * 2;

            // 初始化正确和错误答案的计数数组
            imageCount = (hardness % 3) * 3;
            if (imageCount == 0)
                imageCount = 9;
            train_time = 60 * TRAIN_TIME;
            //LJN，为了防止数组越界，手动加1
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

            // 调用委托
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
            trainingTimer.Start(); // 启动训练计时器

            LoadImages(imageCount);
            ShowRandomImage();
            left = 1;
            top = 1;

            // 调用委托
            VoiceTipAction?.Invoke("请您在左侧的图片中找出与右侧的目标图片相同的图片。");
            SynopsisAction?.Invoke("屏幕上右侧会出现一个目标图片，左侧会出现三个与目标图片有细微不同的图片，请您通过键盘的左右键选中其中一个图片使其与右侧的目标图片相同，并按下键盘上的OK键确认选择。");
            RuleAction?.Invoke("屏幕上右侧会出现一个目标图片，左侧会出现三个与目标图片有细微不同的图片，请您通过键盘的左右键选中其中一个图片使其与右侧的目标图片相同，并按下键盘上的OK键确认选择。");//增加代码，调用函数，显示数字人下的文字

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
            // 调用委托
            VoiceTipAction?.Invoke("请您在左侧的图片中找出与右侧的目标图片相同的图片。");
            SynopsisAction?.Invoke("屏幕上右侧会出现一个目标图片，左侧会出现三个与目标图片有细微不同的图片，请您通过键盘的左右键选中其中一个图片使其与右侧的目标图片相同，并按下键盘上的OK键确认选择。");
            RuleAction?.Invoke("屏幕上右侧会出现一个目标图片，左侧会出现三个与目标图片有细微不同的图片，请您通过键盘的左右键选中其中一个图片使其与右侧的目标图片相同，并按下键盘上的OK键确认选择。");
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new 专注注意力讲解();
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
            Debug.WriteLine($"计算时，总数为{total}，正确数为{a}");
            double accuracy = (double)GetCorrectNum(lv) / total;  // 转换为 double 类型
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
                            left_time = Math.Round((double)left_time, 2);  // 保留两位小数
                        }
                        double center_time = 0;
                        if (center_answer != 0)
                        {
                            center_time = (double)centertime / center_answer;
                            //center_time = (double)centertime / 1000;
                            center_time = Math.Round((double)center_time, 2);  // 保留两位小数
                        }
                        double right_time = 0;
                        if (right_answer != 0)
                        {
                            right_time = (double)righttime / right_answer;
                            //right_time = (double)right_time / 1000;
                            right_time = Math.Round((double)right_time, 2);  // 保留两位小数
                        }

                        double accuracy = 0;
                        if (totalcount != 0)
                        {
                            accuracy = (double)(1.0 * correctCount) / totalcount;  // 计算准确度
                            accuracy = Math.Round((double)accuracy, 2);  // 保留两位小数
                        }
                        double acctime = 0;
                        if (totalcount != 0)
                        {
                            acctime = (double)(lefttime + centertime + righttime) / totalcount;
                            acctime = Math.Round((double)acctime, 2);  // 保留两位小数
                        }

                        // 创建 Result 记录
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "专注注意力",
                            Eval = false,
                            Lv = max_hardness, // 当前的难度级别
                            ScheduleId = BaseParameter.ScheduleId ?? null // 假设的 Schedule_id，可以替换为实际值
                        };

                        db.Results.Add(newResult);
                        await db.SaveChangesAsync();

                        // 获得 result_id
                        int result_id = newResult.ResultId;

                        // 创建 ResultDetail 对象列表
                        var resultDetails = new List<ResultDetail>
                            {

                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "平均反应时间(左侧)(ms)",
                                    Value = left_time,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "平均反应时间(中间)(ms)",
                                    Value = center_time,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "平均反应时间(右侧)(ms)",
                                    Value = right_time,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "平均反应时间(ms)",
                                    Value = acctime ,
                                    Maxvalue = 2000,
                                    Minvalue = 0,
                                    Charttype = "柱状图" ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "选择总数",
                                    Value = totalcount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "正确数",
                                    Value = correctCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "错误数",
                                    Value = wrongCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "遗漏",
                                    Value = ignoreCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "正确率",
                                    Value = accuracy * 100,
                                    Maxvalue = 100,
                                    Minvalue = 0,
                                    Charttype = "柱状图" ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "等级",
                                    Value = max_hardness,
                                    ModuleId = BaseParameter.ModuleId
                                },

                            };

                        // 插入 ResultDetail 数据
                        db.ResultDetails.AddRange(resultDetails);
                        await db.SaveChangesAsync();

                        // 输出每个 ResultDetail 对象的数据
                        Debug.WriteLine($"难度级别 {max_hardness}:");
                        foreach (var detail in resultDetails)
                        {
                            Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId}");
                        }


                        // 提交事务
                        await transaction.CommitAsync();
                        Debug.WriteLine("插入成功");
                    });
                }
                catch (Exception ex)
                {
                    // 回滚事务
                    await transaction.RollbackAsync();
                    Debug.WriteLine(ex.ToString());
                }
            }
        }
    }
}
