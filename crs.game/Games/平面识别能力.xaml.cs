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
    /// VOR.xaml 的交互逻辑
    /// </summary>
    public partial class 平面识别能力 : BaseUserControl
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
        //private int INCREASE = 5; // 提高难度的阈值
        //private int DECREASE = 5;  // 降低难度的阈值
        private int TRAIN_TIME = 60; // 训练持续时间（单位：秒）
        private int cost_time;
        private bool IS_RESTRICT_TIME = true; // 限制练习时间是否启用
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
        private int[] correctAnswers; // 存储每个难度的正确答案数量
        private int[] wrongAnswers; // 存储每个难度的错误答案数量
        private int[] igonreAnswer;
        private DispatcherTimer timer;
        private int remainingTime;
        private DispatcherTimer trainingTimer; // 新的计时器用于训练时间
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
        private bool is_auto = true; // 是否自动跳转
        private bool is_enter = false;
        public 平面识别能力()
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
            train_time--; // 训练时间倒计时
            TimeStatisticsAction.Invoke(train_time, remainingTime);

            if (train_time <= 0)
            {
                timer.Stop(); // 停止主计时器
                trainingTimer.Stop(); // 停止训练计时器
                //VOR_Report reportWindow = new VOR_Report(INCREASE, DECREASE, max_time, TRAIN_TIME, IS_RESTRICT_TIME, IS_BEEP, correctAnswers, wrongAnswers, igonreAnswer);
                //reportWindow.Show(); // 打开报告窗口
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
                    //textblock.Text = "恭喜你答对了！";
                    //textblock.Foreground = new SolidColorBrush(Colors.Green);
                    if (IS_VISUAL) ShowFeedbackImage(isCorrect);
                    if (IS_BEEP) PlayWav(CorrectSoundPath);
                    correctAnswers[hardness]++; // 更新对应难度的正确答案计数
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
                    //textblock.Text = "很遗憾答错了！";
                    //textblock.Foreground = new SolidColorBrush(Colors.Red);
                    if (IS_VISUAL) ShowFeedbackImage(isCorrect);
                    if (IS_BEEP) PlayWav(ErrorSoundPath);
                    wrongAnswers[hardness]++; // 更新对应难度的错误答案计数
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

        // 在类中添加一个列表来存储最近5道题的结果
        private List<bool> recentResults = new List<bool>();


        private void resetboollist()
        {
            recentResults.Clear();
        }
        private void AdjustDifficulty()
        {
            int correctCount = 0;
            int wrongCount = 0;
            // 添加当前题目结果到recentResults列表

            // 只保留最近5个结果
            int max = Math.Max(LevelUp, LevelDown); // 假设Max是Math类中的Max方法

            // 确保recentResults集合的大小不超过max
            if (recentResults.Count > max)
            {
                recentResults.RemoveAt(0); // 移除第一个元素
            }

            if (recentResults.Count == Math.Min(LevelUp, LevelDown))
            {
                // 计算最近答题中正确答案的数量
                for (int i = recentResults.Count - LevelUp; i < recentResults.Count; i++)
                {
                    correctCount += recentResults[i] ? 1 : 0; // 假设recentResults[i]是bool类型，正确则加1
                }
                for (int i = recentResults.Count - LevelDown; i < recentResults.Count; i++)
                {
                    wrongCount += recentResults[i] ? 0 : 1; // 假设recentResults[i]是bool类型，错误则加1
                }

                // 提高难度
                if (correctCount == LevelUp && hardness < 24)
                {
                    hardness++;
                    max_hardness = Math.Max(max_hardness, hardness);
                    resetboollist(); // 假设resetboollist是一个方法，用于重置boollist


                }

                // 降低难度
                else if (wrongCount == LevelDown && hardness > 1) // 这里应该是wrongCount == LevelDown
                {
                    hardness--;
                    resetboollist(); // 假设resetboollist是一个方法，用于重置boollist


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
    public partial class 平面识别能力 : BaseUserControl
    {

        protected override async Task OnInitAsync()
        {
            /*LJN
            配置反馈资源的路径             
             */
            CorrectSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");
            //this.KeyDown += Window_KeyDown;
            //Debug.WriteLine("INIT");
            //var baseParameter = BaseParameter;
            int increase; int decrease; int mt; int tt; bool irt; bool ib; int hardness_;

            // 此处应该由客户端传入参数为准，目前先用测试数据
            {


                int max_time = 60;
                int INCREASE = 0; // 提高难度的阈值
                int DECREASE = 0;  // 降低难度的阈值
                int TRAIN_TIME = 60; // 训练持续时间（单位：秒）
                bool IS_RESTRICT_TIME = true; // 限制练习时间是否启用
                bool IS_BEEP = true; // 是否发出声音
                int hardness = 1; // 难度级别


                increase = INCREASE;
                decrease = DECREASE;
                mt = max_time;
                tt = TRAIN_TIME;
                irt = IS_RESTRICT_TIME;
                ib = IS_BEEP;
                hardness_ = hardness;
            }


            max_time = mt;
            //INCREASE = increase; // 提高难度的阈值
            //DECREASE = decrease;  // 降低难度的阈值
            LevelUp = increase; // 提高难度的阈值
            LevelDown = decrease;  // 降低难度的阈值
            TRAIN_TIME = tt; // 训练持续时间（单位：秒）
            IS_RESTRICT_TIME = irt; // 限制练习时间是否启用
            IS_BEEP = ib;
            hardness = hardness_;
            remainingTime = max_time - (hardness - 1) * 2; ;
            imageCount = (hardness % 3) * 3;
            if (imageCount == 0)
                imageCount = 9;
            random = new Random();
            counter = 0;



            {
                // 参数（包含模块参数信息）
                var baseParameter = BaseParameter;
                if (baseParameter.ProgramModulePars != null && baseParameter.ProgramModulePars.Any())
                {
                    Debug.WriteLine("ProgramModulePars 已加载数据：");
                    // 遍历 ProgramModulePars 打印出每个参数
                    foreach (var par in baseParameter.ProgramModulePars)
                    {
                        //Debug.WriteLine($"ProgramId: {par.ProgramId}, ModuleParId: {par.ModuleParId}, Value: {par.Value}, TableId: {par.TableId}");
                        if (par.ModulePar.ModuleId == baseParameter.ModuleId)
                        {
                            switch (par.ModuleParId)
                            {
                                case 47: // 治疗时间
                                    TRAIN_TIME = par.Value.HasValue ? (int)par.Value.Value * 60 : 25;
                                    Debug.WriteLine($"TRAIN_TIME={TRAIN_TIME}");
                                    break;
                                case 48: // 等级提高
                                    LevelUp = par.Value.HasValue ? (int)par.Value.Value : 3;
                                    Debug.WriteLine($"LevelUp={LevelUp}");
                                    break;
                                case 49: // 等级降低
                                    LevelDown = par.Value.HasValue ? (int)par.Value.Value : 3;
                                    Debug.WriteLine($"LevelDown ={LevelDown}");
                                    break;
                                case 53: // 听觉反馈
                                    IS_BEEP = par.Value == 1;
                                    Debug.WriteLine($"是否听觉反馈 ={IS_BEEP}");
                                    break;
                                case 51: // 解答时间限制
                                    IS_RESTRICT_TIME = par.Value == 1;
                                    Debug.WriteLine($"是否有解答时间限制 ={IS_RESTRICT_TIME}");
                                    break;
                                // 添加其他需要处理的 ModuleParId
                                case 170://等级
                                    hardness = par.Value.HasValue ? (int)par.Value.Value : 1;
                                    Debug.WriteLine($"HARDNESS: {hardness}");
                                    break;
                                case 257:
                                    IS_VISUAL = par.Value == 1;
                                    Debug.WriteLine($"是否视觉反馈 ={IS_VISUAL}");
                                    break;
                                case 273:
                                    is_auto = par.Value == 1;
                                    Debug.WriteLine($"是否题目自动跳转 ={is_auto}");
                                    break;
                                default:// 初始难度
                                    //hardness = par.Value.HasValue ? (int)par.Value.Value : 1;
                                    //Debug.WriteLine($"HARDNESS: {hardness}");
                                    break;

                            }
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("没有数据");
                }
            }


            train_time = TRAIN_TIME;
            // 初始化正确和错误答案的计数数组
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
            // 调用委托
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
            trainingTimer.Start(); // 启动训练计时器
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

            // 调用委托
            VoiceTipAction?.Invoke("请在屏幕上识别出哪个图像是屏幕右侧图像旋转得到的。");
            SynopsisAction?.Invoke("您在屏幕右侧会看到一个图像，左侧会看到若干个旋转过后的不同图像，您需要在屏幕上识别出哪个图像是屏幕右侧图像旋转得到的，并通过键盘的上下左右键选中并按OK键确认。");
            RuleAction?.Invoke("您在屏幕右侧会看到一个图像，左侧会看到若干个旋转过后的不同图像，您需要在屏幕上识别出哪个图像是屏幕右侧图像旋转得到的，并通过键盘的上下左右键选中并按OK键确认。");//增加代码，调用函数，显示数字人下的文字

        }

        protected override async Task OnStopAsync()//需要插入
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

            // 调用委托
            VoiceTipAction?.Invoke("请在屏幕上识别出哪个图像是屏幕右侧图像旋转得到的。");
            SynopsisAction?.Invoke("您在屏幕右侧会看到一个图像，左侧会看到若干个旋转过后的不同图像，您需要在屏幕上识别出哪个图像是屏幕右侧图像旋转得到的，并通过键盘的上下左右键选中并按OK键确认。");
            RuleAction?.Invoke("您在屏幕右侧会看到一个图像，左侧会看到若干个旋转过后的不同图像，您需要在屏幕上识别出哪个图像是屏幕右侧图像旋转得到的，并通过键盘的上下左右键选中并按OK键确认。");
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new 平面识别能力讲解();
        }

        // 插入写法
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
                        // 计算准确率
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
                        //// 计算准确率
                        //double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);

                        max_hardness = Math.Max(max_hardness, hardness);//在生成报告之前，手动再更新一下最大难度
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

                        // 创建 Result 记录
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "平面识别能力",
                            Eval = false,
                            Lv = max_hardness, // 当前的难度级别
                            ScheduleId = BaseParameter.ScheduleId ?? null// 假设的 Schedule_id，可以替换为实际值
                        };
                        db.Results.Add(newResult);
                        await db.SaveChangesAsync(); //这里注释了
                                                     // 获得 result_id
                        int result_id = newResult.ResultId;
                        // 创建 ResultDetail 对象列表
                        var resultDetails = new List<ResultDetail>
                            {
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 0,
                                    ValueName = "等级",
                                    Value = hardness,
                                    Maxvalue = 24,
                                    Minvalue = 1,
                                    ModuleId = BaseParameter.ModuleId //  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "全部",
                                    Value = totalCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "错误",
                                    Value = wrongCount,
                                    Maxvalue = wrongCount,
                                    Minvalue = 1,
                                    Charttype = "柱状图" ,
                                    ModuleId =  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "遗漏",
                                    Value = ignoreCount,
                                    ModuleId =  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                   ResultId = result_id,
                                   Order = 1,
                                    ValueName = "最小反应时间(s)",
                                    Value = mincosttimeCount,
                                    ModuleId =  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "最大反应时间(s)",
                                    Value = maxcosttimeCount,
                                    ModuleId =  BaseParameter.ModuleId
                                }
                            };
                        // 插入 ResultDetail 数据
                        db.ResultDetails.AddRange(resultDetails);
                        await db.SaveChangesAsync();
                        // 输出每个 ResultDetail 对象的数据
                        Debug.WriteLine($"难度级别:");
                        foreach (var detail in resultDetails)
                        {
                            Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId} ");
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
