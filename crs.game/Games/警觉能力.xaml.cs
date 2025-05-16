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
    /// 警觉能力.xaml 的交互逻辑
    /// </summary>
    public partial class 警觉能力 : BaseUserControl
    {
        private List<double> soundReactionTimes = new List<double>(); // 有声音的反应时间
        private List<double> noSoundReactionTimes = new List<double>(); // 无声音的反应时
        private bool is_beep = true;//是否有声音提示
        private const int GAMETIME = 40;
        private const int wait_delay = 4000; // 等待用户4000ms作出反应
        private const int min_delay = 1000;   // 生成图片等待时间最小值
        private const int max_delay = 3001;   // 生成图片等待时间最大值
        //正确率统计
        private int beep_num = 0;//声音提示有多少次
        private int beep_forget = 0;//声音提示的时候遗漏了多少次
        private int nonbeep_num = 0;//无声有多少次
        private int nonbeep_forget = 0;//无声遗漏了多少次
        private List<bool> beepSequence; // 用于存储 20 个有声音提示和 20 个无声音提示的随机顺序
        private int beepIndex = 0; // 用于跟踪当前任务的索引
        //
        private BitmapImage targetImage;
        private int attemptCount = 0;
        private Stopwatch stopwatch;
        private double total_time = 0;
        private int forget = 0; // 记录忘记点击的次数
        private CancellationTokenSource cts; // 用于取消超时任务
        string[] imagePaths; // 存储文件夹中照片
        private DispatcherTimer gameTimer; // 计时器
        private TimeSpan totalGameTime; // 总游戏时间

        private int correctWithBeep = 0; // 有声音提示答对的次数
        private int correctWithoutBeep = 0; // 无声音提示答对的次数

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            totalGameTime = totalGameTime.Add(TimeSpan.FromSeconds(1)); // 每次增加一秒// 获取总秒数
            int totalSeconds = (int)totalGameTime.TotalSeconds;

            // 调用委托
            TimeStatisticsAction?.Invoke(totalSeconds, totalSeconds);

        }


        private string FindImagePath()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            {
                string targetDirectory = Path.Combine(currentDirectory, "警觉能力");
                if (Directory.Exists(targetDirectory))
                {
                    return targetDirectory;
                }
                return null;
            }
            {
                string targetDirectory = Path.Combine(currentDirectory, @"Games\警觉能力");
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
        // 检查当前目录是否存在“crs.game”文件夹
        string targetParentDirectory = Path.Combine(currentDirectory, "crs.game");
        if (Directory.Exists(targetParentDirectory))
        {
            string targetDirectory = Path.Combine(targetParentDirectory, "警觉能力");
            if (Directory.Exists(targetDirectory))
            {
                return targetDirectory; // 找到文件夹，返回路径
            }
        }

        // 向上移动到父目录
        DirectoryInfo parentDirectory = Directory.GetParent(currentDirectory);
        if (parentDirectory == null)
        {
            break; // 到达根目录，停止查找
        }
        currentDirectory = parentDirectory.FullName; // 更新当前目录
    }

    return null; // 未找到文件夹
    */

        public 警觉能力()
        {
            InitializeComponent();
            this.Focusable = true;
            this.Focus();
            GenerateBeepSequence(); // 初始化声音提示序列
        }
        private void GenerateBeepSequence()//添加 20 个有声音的提示（true）和 20 个无声音的提示
        {
            beepSequence = new List<bool>();

            // 添加 20 个有声音的提示（true）和 20 个无声音的提示（false）
            beepSequence.AddRange(Enumerable.Repeat(true, 20));
            beepSequence.AddRange(Enumerable.Repeat(false, 20));

            // 随机打乱顺序
            // Fisher-Yates 洗牌算法
            Random rand = new Random();
            for (int i = beepSequence.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                // 交换位置 i 和 j 的元素
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

        private void PrintReactionTimes()//调试反应时间输出用函数
        {
            Debug.WriteLine("有声音的反应时间:");
            foreach (var time in soundReactionTimes)
            {
                Debug.WriteLine(time + " 秒");
            }

            Debug.WriteLine("无声音的反应时间:");
            foreach (var time in noSoundReactionTimes)
            {
                Debug.WriteLine(time + " 秒");
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 设置 UserControl 获取焦点
            this.Focus();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // 检查是否按下 Enter 键
            if (e.Key == Key.Enter)
            {

                // 调用 ConfirmButton_Click 方法
                ConfirmButton_Click(sender, e);
            }
        }

        private async void LoadNewImage()
        {
            if (attemptCount >= GAMETIME)
            {
                //MessageBox.Show($"测试结束！\n" + $"总反应时间: {total_time}秒" + $"总遗漏次数: {forget}次", "测试结果");
                //警觉能力报告 nwd=new 警觉能力报告(total_time/GAMETIME,is_beep,forget);
                //nwd.Show();
                nonbeep_num = attemptCount - beep_num;
                nonbeep_forget = forget - beep_forget;
                StopAllTasks();
                //PrintReactionTimes();
                OnGameEnd();
                return;
            }

            // 取消之前的任务
            try
            {
                cts?.Cancel();
            }
            catch (ObjectDisposedException)
            {

            }
            //cts?.Cancel();
            cts = new CancellationTokenSource(); // 创建新的 CancellationTokenSource

            // 清空当前图片
            TargetImage.Source = null;

            var token = cts.Token;

            Random rand = new Random();
            int delay = rand.Next(min_delay, max_delay);
            try
            {
                await Task.Delay(delay, token); // 使用取消令牌
            }
            catch (TaskCanceledException)
            {

            }
            //await Task.Delay(delay, token); // 使用取消令牌

            // 随机设置 TargetImage 的位置
            // 显示图片
            TargetImage.Source = targetImage;
            stopwatch = Stopwatch.StartNew(); // 图片显示后立即开始计时
            double maxX = ImageCanvas.ActualWidth - TargetImage.Width;
            double maxY = ImageCanvas.ActualHeight - TargetImage.Height;

            double randomX = rand.NextDouble() * maxX;
            double randomY = rand.NextDouble() * maxY;

            Canvas.SetLeft(TargetImage, randomX);
            Canvas.SetTop(TargetImage, randomY);

            attemptCount++;
            RightStatisticsAction?.Invoke(attemptCount - forget, 10);
            WrongStatisticsAction?.Invoke((forget), 10);

            // 使用预生成的声音提示序列
            is_beep = beepSequence[beepIndex];
            beepIndex++; // 移动到下一个任务

            if (is_beep)
            {
                beep_num++;
                Task.Run(() => Console.Beep(800, 200)); // 异步播放警告声
            }
            try
            {
                await Task.Delay(wait_delay, token); // 等待DELAY时间
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
            TargetImage.Source = null; // 清空当前图片
            LoadNewImage(); // 重新加载新图片
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (TargetImage.Source == null)
            {

                //MessageBox.Show("错误触碰按钮！");
                return;
            }

            stopwatch.Stop();
            if (is_beep)
            {
                soundReactionTimes.Add(stopwatch.Elapsed.TotalSeconds);
                correctWithBeep++; // 有声音提示时答对
            }
            else
            {
                noSoundReactionTimes.Add(stopwatch.Elapsed.TotalSeconds);
                correctWithoutBeep++; // 无声音提示时答对
            }
            total_time += stopwatch.Elapsed.TotalSeconds;
            LoadNewImage(); // 重新加载新图片
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EvaluateTest evaluateTestWindow = Application.Current.Windows.OfType<EvaluateTest>().FirstOrDefault();
            if (evaluateTestWindow != null)
            {
                evaluateTestWindow.SetContinueAssessment(false); // 停止后续窗口的展示
            }
            StopAllTasks(); // 在关闭窗口时停止所有任务
            OnGameEnd();
        }

        private void StopAllTasks()
        {
            // 取消当前的 CancellationTokenSource
            try
            {
                cts?.Cancel();
            }
            catch (ObjectDisposedException)
            {

            }
            //cts?.Cancel();
            cts?.Dispose(); // 释放资源
        }
    }
    public partial class 警觉能力 : BaseUserControl
    {

        protected override async Task OnInitAsync()
        {
            string imagePath = FindImagePath();
            mubiaowu.Visibility = Visibility.Visible;
            if (imagePath != null)
            {
                // 如果找到了文件夹，获取该文件夹中的所有文件
                imagePaths = Directory.GetFiles(imagePath);
                LoadTargetImage();
            }
            else
            {
                MessageBox.Show("未找到名为“警觉能力”的文件夹。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                OnGameEnd();
            }
            LevelStatisticsAction?.Invoke(0, 0);
            RightStatisticsAction?.Invoke(0, 10);
            WrongStatisticsAction?.Invoke(0, 10);

            totalGameTime = TimeSpan.Zero; // 重置总时间
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1); // 每秒更新一次
            gameTimer.Tick += GameTimer_Tick; // 绑定计时器事件
        }

        protected override async Task OnStartAsync()
        {
            gameTimer.Start(); // 启动计时器
            LoadNewImage();
            // 调用委托
            VoiceTipAction?.Invoke("当屏幕出现目标物时，请用鼠标点击确认按钮。");
            SynopsisAction?.Invoke("现在您在右侧可以看到需要确认选取的目标图，当屏幕出现带有警告声或者没有警告声的目标物时，请点击确认");
            RuleAction?.Invoke("现在您在右侧可以看到需要确认选取的目标图，当屏幕出现带有警告声或者没有警告声的目标物时，请点击确认");//增加代码，调用函数，显示数字人下的文字
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

            // 调用委托
            VoiceTipAction?.Invoke("当屏幕出现目标物时，请用鼠标点击确认按钮。");
            SynopsisAction?.Invoke("现在您在右侧可以看到需要确认选取的目标图，当屏幕出现带有警告声或者没有警告声的目标物时，请点击确认");
            RuleAction?.Invoke("现在您在右侧可以看到需要确认选取的目标图，当屏幕出现带有警告声或者没有警告声的目标物时，请点击确认");//增加代码，调用函数，显示数字人下的文字
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new 警觉能力讲解();
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

        private int GetCorrectNum_beep()//声音提示正确个数
        {
            return beep_num - beep_forget;
        }
        private int GetCorrectNum_nonbeep()//无声正确个数
        {
            return nonbeep_num - nonbeep_forget;
        }

        private double CalculateAccuracy(int correctCount)
        {
            const int totalRequiredCorrect = 21; // 答对 21 个即为 100%
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

                    // 获取当前难度级别的数据
                    int correctCount = GetCorrectNum();
                    int wrongCount = GetWrongNum();
                    int ignoreCount = GetIgnoreNum();
                    double totalMilliseconds = totalGameTime.TotalMilliseconds;  // 转换为double类型的毫秒数
                    double time = (double)totalMilliseconds / attemptCount;
                    // 计算准确率
                    double accuracy = CalculateAccuracy(correctCount);

                    // 计算有警告声的总反应时间和平均反应时间（只计算正确的）
                    double totalReactionTimeWithBeep = soundReactionTimes.Sum() * 1000; // 转换为毫秒
                    double averageReactionTimeWithBeep = correctWithBeep > 0 ? totalReactionTimeWithBeep / correctWithBeep : 0;

                    // 计算无警告声的总反应时间和平均反应时间（只计算正确的）
                    double totalReactionTimeWithoutBeep = noSoundReactionTimes.Sum() * 1000; // 转换为毫秒
                    double averageReactionTimeWithoutBeep = correctWithoutBeep > 0 ? totalReactionTimeWithoutBeep / correctWithoutBeep : 0;

                    // 计算有警告声 Z值和无警告声 Z值
                    double zValueWithBeep = Math.Round((double)(262 - averageReactionTimeWithBeep) / 67, 2);
                    double zValueWithoutBeep = Math.Round((double)(268 - averageReactionTimeWithoutBeep) / 71, 2);

                    // 创建 Result 记录
                    var newResult = new Result
                    {
                        ProgramId = BaseParameter.ProgramId, // program_id
                        Report = "警觉能力评估报告",
                        Eval = true,
                        Lv = null, // 当前的难度级别
                        ScheduleId = BaseParameter.ScheduleId ?? null // 假设的 Schedule_id，可以替换为实际
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
                                    Order = 0,
                                    ValueName = "正确次数",
                                    Value = correctCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                     ResultId = result_id,
                                     Order = 2,
                                     ValueName = "有警告声正确次数",
                                     Value = correctWithBeep,
                                     ModuleId = BaseParameter.ModuleId
                                 },
                                new ResultDetail
                                {
                                     ResultId = result_id,
                                     ValueName = "无警告声正确次数",
                                     Order = 5,
                                     Value = correctWithoutBeep,
                                     ModuleId = BaseParameter.ModuleId
                                },

                                new ResultDetail
                                {
                                     ResultId = result_id,
                                     ValueName = "有警告声遗漏次数",
                                     Order = 3,
                                     Value = beep_forget,
                                     ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                     ResultId = result_id,
                                     ValueName = "无警告声遗漏次数",
                                     Order = 6,
                                     Value = nonbeep_forget,
                                     ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order =1,
                                    ValueName = "平均值反应时间(ms)",
                                    Value = Math.Round(time, 2), // 以百分比形式存储
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 4,
                                    ValueName = "有警告声Z值",
                                    Value = Math.Round(zValueWithBeep, 2),
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order =7,
                                    ValueName = "无警告声Z值",
                                    Value = Math.Round(zValueWithoutBeep, 2),
                                    ModuleId = BaseParameter.ModuleId
                                }
                                };

                    resultDetails.AddRange(soundReactionTimes.Select((value, index) => new ResultDetail
                    {
                        ResultId = result_id,
                        ValueName = "任务(有警告音),时间[ms]",
                        Value = Math.Round(value * 1000, 2),
                        Abscissa = index + 1,
                        Charttype = "折线图",
                        ModuleId = BaseParameter.ModuleId
                    }).Reverse().ToList());

                    resultDetails.AddRange(noSoundReactionTimes.Select((value, index) => new ResultDetail
                    {
                        ResultId = result_id,
                        ValueName = "任务(无警告音),时间[ms]",
                        Value = Math.Round(value * 1000, 2),
                        Abscissa = index + 1,
                        Charttype = "折线图",
                        ModuleId = BaseParameter.ModuleId
                    }).Reverse().ToList());


                    // 插入 ResultDetail 数据
                    db.ResultDetails.AddRange(resultDetails);
                    await db.SaveChangesAsync();
                    PrintReactionTimes();
                    // 输出每个 ResultDetail 对象的数据
                    /*Debug.WriteLine($"难度级别 {lv}:");*/
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

                    // 提交事务
                    await transaction.CommitAsync();
                    Debug.WriteLine("插入成功");
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
