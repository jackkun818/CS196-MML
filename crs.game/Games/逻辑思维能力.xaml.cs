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
    /// LODE.xaml 的交互逻辑
    /// </summary>
    public partial class 逻辑思维能力 : BaseUserControl
    {
        private int countdownTime;
        private const int MAX_DELAY = 5000; // 5秒
        private const int AMOUNT = 1; // 阈值的基数
        private string imagePath;
        private string[] imagePaths;
        private string[] directoryPaths;
        private string[] questionPaths;
        private string[] answerPaths;
        private int hardness;
        private int index;
        private Image lastClickedImage;
        private int[] correctAnswers; // 存储每个难度的正确答案数量
        private int[] wrongAnswers; // 存储每个难度的错误答案数量
        private int[] ignoreAnswers;
        private int imageCount;
        private int max_time = 10;
        private const int WAIT_DELAY = 1;
        private const int MAX_HARDNESS = 23;
        private int TRAIN_TIME; // 训练持续时间（单位：秒）
        private int cost_time;
        private bool IS_RESTRICT_TIME = false; // 限制练习时间是否启用
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
        private DispatcherTimer trainingTimer; // 新的计时器用于训练时间
        private DispatcherTimer countTimer;
        private List<bool> boolList = new List<bool>(5);

        private int[] TotalCountByHardness;
        private double[] TotalAccuracyByHardness;
        private double[] AverageTimeByHardness;
        private List<int>[] CostTime;
        private double[] AverageCostTime;

        private bool is_enter = false;

        private int LevelUp = 3; // 提高难度的阈值
        private int LevelDown = 3; // 降低难度的阈值
        private int stimulus_interval = 3;
        public 逻辑思维能力()
        {
            InitializeComponent();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (countdownTime > 0)
            {
                countdownTime--; // 每秒减少1
                cost_time += 1;
                TimeStatisticsAction.Invoke(train_time, countdownTime);
                // 更新UI（例如，显示剩余时间）
            }
            else
            {
                timer.Stop();
                ignoreAnswers[hardness - 1]++; // 记录忽略
                //wrongAnswers[hardness - 1]++; //不确定超时是否计入错误
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
            //MessageBox.Show(IS_RESTRICT_TIME.ToString()); //我已经把他搞成false了 但是时间还是正常走

            if (IS_RESTRICT_TIME)
            {
                train_time--; // 训练时间倒计时

            }
            TimeStatisticsAction.Invoke(train_time, countdownTime);

            if (train_time <= 0)
            {
                timer.Stop(); // 停止主计时器
                trainingTimer.Stop(); // 停止训练计时器
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
            string targetDirectory = System.IO.Path.Combine(currentDirectory, "逻辑思维能力");
            return targetDirectory;
            //while (true)
            //{
            //    string targetParentDirectory = System.IO.Path.Combine(currentDirectory, "crs.game");
            //    if (Directory.Exists(targetParentDirectory))
            //    {
            //        string targetDirectory = System.IO.Path.Combine(targetParentDirectory, "逻辑思维能力");
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
                string targetDirectory = Path.Combine(currentDirectory, "逻辑思维能力");
                if (Directory.Exists(targetDirectory))
                {
                    return targetDirectory;
                }
                return null;
            }
            {
                string targetDirectory = Path.Combine(currentDirectory, @"Games\逻辑思维能力");
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
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d0e3b6")), // 设置背景颜色
                BorderBrush = Brushes.Transparent // 去掉边框颜色
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
            timer.Start(); // 重新启动计时器
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
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d0e3b6")), // 设置背景颜色
                    BorderBrush = Brushes.Transparent // 去掉边框颜色
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
                timer.Stop(); // 停止计时器
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
                    correctAnswers[hardness - 1]++; // 记录正确
                    RightStatisticsAction?.Invoke(correctAnswers[hardness - 1], LevelUp);
                    //await Task.Delay(TimeSpan.FromSeconds(WAIT_DELAY));
                    boolList.Add(true);

                    // 检查列表长度并删除第一个元素以保持列表长度为5
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

                    wrongAnswers[hardness - 1]++; // 记录错误
                    WrongStatisticsAction?.Invoke(wrongAnswers[hardness - 1], LevelDown);
                    if (IS_BEEP) PlayWav(ErrorSoundPath);
                    if (IS_VISUAL) ShowFeedbackImage(ErrorImage);
                    countTimer.Start();
                    boolList.Add(false);

                    // 检查列表长度并删除第一个元素以保持列表长度为5
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
            // 添加当前题目结果到recentResults列表

            // 只保留最近5个结果
            int max = Math.Max(LevelUp, LevelDown); // 假设Max是Math类中的Max方法

            // 确保recentResults集合的大小不超过max
            if (boolList.Count > max)
            {
                boolList.RemoveAt(0); // 移除第一个元素
            }

            if (boolList.Count == max)
            {
                // 计算最近答题中正确答案的数量
                for (int i = boolList.Count - LevelUp; i < boolList.Count; i++)
                {
                    correctCount += boolList[i] ? 1 : 0; // 假设recentResults[i]是bool类型，正确则加1
                }
                for (int i = boolList.Count - LevelDown; i < boolList.Count; i++)
                {
                    wrongCount += boolList[i] ? 0 : 1; // 假设recentResults[i]是bool类型，错误则加1
                }

                // 提高难度
                if (correctCount == LevelUp && hardness < 24)
                {
                    hardness++;
                    resetboollist();
                }

                // 降低难度
                else if (wrongCount == LevelDown && hardness > 1)
                {
                    hardness--;
                    resetboollist();
                }
            }
        }
        private void ResetCounts()
        {
            // 重置当前难度的正确和错误计数
            correctAnswers[hardness - 1] = 0;
            wrongAnswers[hardness - 1] = 0;
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

        private async void ShowFeedbackImage(Image image)
        {//显示反馈的图片
            image.Visibility = Visibility.Visible;
        }


        //加个计时器，用来同步显示治疗师端的星星
        private DispatcherTimer StarTimer = new DispatcherTimer();
    }
    public partial class 逻辑思维能力 : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
            /*LJN
            配置反馈资源的路径             
             */
            CorrectSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");
            // 为 Image 控件加载图片 Source
            CorrectImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Correct.png"), UriKind.RelativeOrAbsolute));
            ErrorImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Error.png"), UriKind.RelativeOrAbsolute));

            // 参数（包含模块参数信息）
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
                            case 178: // 等级
                                hardness = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"HARDNESS: {hardness}");
                                break;
                            case 35: // 治疗时间 
                                train_time = par.Value.HasValue ? (int)par.Value.Value * 60 : 60;
                                Debug.WriteLine($"TRAIN_TIME={train_time}");
                                break;
                            case 36: // 等级提高
                                LevelUp = par.Value.HasValue ? (int)par.Value.Value : 3;
                                Debug.WriteLine($"INCREASE={LevelUp}");
                                break;
                            case 37: // 等级降低
                                LevelDown = par.Value.HasValue ? (int)par.Value.Value : 3;
                                Debug.WriteLine($"DECREASE ={LevelDown}");
                                break;
                            case 38: // 项目每等级
                                /*                                IS_RESTRICT_TIME = par.Value == 1;
                                                                Debug.WriteLine($"是否限制时间 ={IS_BEEP}");*/
                                break;
                            case 39: // 限制解答时间
                                IS_RESTRICT_TIME = par.Value == 1;
                                Debug.WriteLine($"限制解答时间 ={IS_RESTRICT_TIME}");
                                break;
                            case 40: // 听觉反馈
                                IS_BEEP = par.Value == 1;
                                Debug.WriteLine($"是否听觉反馈 ={IS_BEEP}");
                                break;
                            case 270://视觉反馈
                                IS_VISUAL = par.Value == 1;
                                break;
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

            // 调用委托
            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
            RightStatisticsAction?.Invoke(0, LevelUp);
            WrongStatisticsAction?.Invoke(0, LevelDown);
            StarTimer = new DispatcherTimer();
            StarTimer.Interval = TimeSpan.FromSeconds(1); // 每秒触发一次
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
            // 设置倒计时的初始值（例如，10秒）
            countdownTime = max_time; // 或者您可以设置为其他值

            timer?.Stop();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start(); // 启动计时器

            trainingTimer?.Stop();
            trainingTimer = new DispatcherTimer();
            trainingTimer.Interval = TimeSpan.FromSeconds(1);
            trainingTimer.Tick += TrainingTimer_Tick;
            trainingTimer.Start(); // 启动训练计时器

            countTimer?.Stop();
            countTimer = new DispatcherTimer();
            countTimer.Interval = TimeSpan.FromSeconds(1);
            countTimer.Tick += Countimer_Tick;
            countTimer.Start();

            // 调用委托
            VoiceTipAction?.Invoke("请在提供的若干图像中找出一个合适选项。");
            SynopsisAction?.Invoke("您会看到屏幕上有一个图像序列，该序列的图像相互之间存在着某种规律，您需要思考发现规律，并在提供的若干图像中用鼠标点击来找出一个合适选项。");
            RuleAction?.Invoke("您会看到屏幕上有一个图像序列，该序列的图像相互之间存在着某种规律，您需要思考发现规律，并在提供的若干图像中用鼠标点击来找出一个合适选项。");//增加代码，调用函数，显示数字人下的文字

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
            // 调整难度
            AdjustDifficulty();
            ClearAndLoadNewImages();

            // 调用委托
            VoiceTipAction?.Invoke("请在提供的若干图像中找出一个合适选项。");
            SynopsisAction?.Invoke("您会看到屏幕上有一个图像序列，该序列的图像相互之间存在着某种规律，您需要思考发现规律，并在提供的若干图像中用鼠标点击来找出一个合适选项。");
            RuleAction?.Invoke("您会看到屏幕上有一个图像序列，该序列的图像相互之间存在着某种规律，您需要思考发现规律，并在提供的若干图像中用鼠标点击来找出一个合适选项。");
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new 逻辑思维能力讲解();
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
                            // 获取当前难度级别的数据
                            int correctCount = GetCorrectNum(lv);
                            int wrongCount = GetWrongNum(lv);
                            int ignoreCount = GetIgnoreNum(lv);
                            int totalCount = correctCount + wrongCount + ignoreCount;
                            double averageTime = GetAverageCostTime(lv);
                            if (totalCount == 0 && averageTime == 0)
                            {
                                continue;
                            }
                            // 计算准确率
                            double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);

                            // 创建 Result 记录
                            var newResult = new Result
                            {
                                ProgramId = program_id, // program_id
                                Report = "逻辑思维能力",
                                Eval = false,
                                Lv = lv+1, // 当前的难度级别
                                ScheduleId = BaseParameter.ScheduleId ?? null // 假设的 Schedule_id，可以替换为实际值
                            };//每个难度等级都会覆盖上去，所以到最后显示的就是最大难度等级

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
                                    ValueName = "等级",
                                    Value = lv+1,
                                    Maxvalue = 23,
                                    Minvalue = 1,
                                    Charttype = "柱状图" ,
                                    ModuleId = BaseParameter.ModuleId //  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "系列图片数量",
                                    Value = totalCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "正确",
                                    Value = correctCount,
                                    Maxvalue = correctCount,
                                    Minvalue = 0,
                                    Charttype = "柱状图" ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "正确的（%）",
                                    Value = accuracy * 100, // 以百分比形式存储
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "错误",
                                    Value = wrongCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "遗漏",
                                    Value =ignoreCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "平均作答时间",
                                    Value = averageTime,
                                    Maxvalue = (int?)averageTime,
                                    Minvalue = 0,
                                    Charttype = "柱状图" ,
                                    ModuleId = BaseParameter.ModuleId
                                }
                             };

                            // 插入 ResultDetail 数据
                            db.ResultDetails.AddRange(resultDetails);
                            await db.SaveChangesAsync();

                            // 输出每个 ResultDetail 对象的数据
                            Debug.WriteLine($"难度级别 {lv + 1}:");
                            foreach (var detail in resultDetails)
                            {
                                Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId}");
                            }
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