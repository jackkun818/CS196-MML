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
    /// 选择注意力.xaml 的交互逻辑
    /// </summary>
    public partial class 选择注意力 : BaseUserControl
    {
        private string imagePath; // 存储找到的文件夹路径
        private string[] imagePaths;
        private const int DELAY = 5000; // 等待5000ms显示下一张图片
        private const int MAX_GAME = 42;
        private BitmapImage targetImage;
        private BitmapImage leftImage;

        private string targetImagePath; // 保存目标图片的路径
        private List<BitmapImage> gameImages; // 存储打乱后的42张图片
        private int attemptCount = 0;
        private Stopwatch stopwatch;
        private double total_time = 0;
        private int wrong = 0;
        private int skip = 0;
        private int score = 0;//记录只在目标物出现按下确认数据
        private CancellationTokenSource cancellationTokenSource; // 用于取消加载任务
        class TaskResult
        {
            public int TaskNumber { get; set; } // 任务编号
            public double? ReactionTime { get; set; } // 反应时间（秒），可空类型
            public string Result { get; set; } // 任务结果：正确、错误或遗漏
        }
        private List<TaskResult> taskResults = new List<TaskResult>();

        private DispatcherTimer gameTimer; // 计时器
        private TimeSpan totalGameTime; // 总游戏时间
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            totalGameTime = totalGameTime.Add(TimeSpan.FromSeconds(1)); // 每次增加一秒// 获取总秒数
            int totalSeconds = (int)totalGameTime.TotalSeconds;

            // 调用委托
            TimeStatisticsAction?.Invoke(totalSeconds, totalSeconds);

        }
        private void InitializeImages()
        {
            // 清空列表，确保没有残留数据
            gameImages = new List<BitmapImage>();

            // 找到图片路径
            string targetDirectory = FindImagePath();
            if (targetDirectory == null)
            {
                MessageBox.Show("未找到目标文件夹，请检查路径。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                OnGameEnd();
                return;
            }

            // 加载所有图片（假设有3张图片）
            imagePaths = Directory.GetFiles(targetDirectory, "*.png", SearchOption.TopDirectoryOnly);

            if (imagePaths.Length < 2)
            {
                MessageBox.Show("图片数量错误，需要至少2张图片。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                OnGameEnd();
                return;
            }

            // 选择目标图片
            LoadTargetImage(); // 随机选择一张图片作为目标物，并存储在 `targetImagePath`

            // 获取非目标物图片路径
            var nonTargetImagesPaths = imagePaths.Where(path => path != targetImagePath).ToArray();

            // 添加 21 张目标物图片
            for (int i = 0; i < 21; i++)
            {
                gameImages.Add(CreateBitmapImage(targetImagePath)); // 添加目标物图片
            }

            // 随机添加其他图片，直到达到总数（例如42张）
            Random rand = new Random();
            while (gameImages.Count < 42)
            {
                string randomNonTargetPath = nonTargetImagesPaths[rand.Next(nonTargetImagesPaths.Length)];
                gameImages.Add(CreateBitmapImage(randomNonTargetPath)); // 随机添加非目标物图片
            }

            // 打乱顺序
            ShuffleGameImages();

            // 最终检查目标物和非目标物数量
            int targetCount = gameImages.Count(img => img.UriSource.ToString() == new Uri(targetImagePath, UriKind.Absolute).ToString());
            int nonTargetCount = gameImages.Count - targetCount;


        }

        // Helper 方法：选择目标图片并设置路径
        private void LoadTargetImage()
        {
            Random rand = new Random();
            int index = rand.Next(imagePaths.Length);
            targetImagePath = imagePaths[index];
            targetImage = new BitmapImage(new Uri(targetImagePath, UriKind.Absolute));
            TargetImage.Source = targetImage;
        }

        // Helper 方法：创建新的 BitmapImage 实例
        private BitmapImage CreateBitmapImage(string imagePath)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(imagePath, UriKind.Absolute);
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            return bitmapImage;
        }

        // Fisher-Yates 洗牌算法确保完全随机
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

        public 选择注意力()
        {
            InitializeComponent();
        }

        private string FindImagePath()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            {
                string targetDirectory = Path.Combine(currentDirectory, "选择注意力");
                if (Directory.Exists(targetDirectory))
                {
                    return targetDirectory;
                }
                return null;
            }
            {
                string targetDirectory = Path.Combine(currentDirectory, @"Games\选择注意力");
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
                // 检查当前目录是否存在“crs.game”文件夹
                string targetParentDirectory = Path.Combine(currentDirectory, "crs.game");
                if (Directory.Exists(targetParentDirectory))
                {
                    string targetDirectory = Path.Combine(targetParentDirectory, "选择注意力");
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
        }
        */

        private void LoadTargetImageSync()
        {
            // 随机选择一张图片作为目标物
            Random rand = new Random();
            int index = rand.Next(imagePaths.Length);
            targetImagePath = imagePaths[index]; // 保存选择的目标图片路径
            targetImage = new BitmapImage(new Uri(targetImagePath, UriKind.Absolute));
            TargetImage.Source = targetImage;


        }

        private async void LoadNewLeftImage()
        {

            // 如果当前有加载任务正在运行，则取消它
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;



            // 检查是否达到最大游戏次数
            if (attemptCount >= gameImages.Count)
            {
                // 游戏结束逻辑
                OnGameEnd();
                return;
            }

            // 从 `gameImages` 列表中按顺序获取图片（确保每次测试展示顺序不同）
            leftImage = gameImages[attemptCount];
            LeftImage.Source = leftImage;
            attemptCount++;

            // 开始更改，确保图片随机位置在界面内
            // 确保 Canvas 的大小已被正确计算
            LeftImageCanvas.UpdateLayout();

            // 获取 Canvas 和图片的实际尺寸
            double imageWidth = LeftImage.Width;
            double imageHeight = LeftImage.Height;
            double canvasWidth = LeftImageCanvas.ActualWidth;
            double canvasHeight = LeftImageCanvas.ActualHeight;

            // 防止图片宽度或高度超出 Canvas 的大小
            double maxX = Math.Max(0, canvasWidth - imageWidth);
            double maxY = Math.Max(0, canvasHeight - imageHeight);

            // 生成随机位置
            Random rand = new Random();
            double randomX = rand.NextDouble() * maxX;
            double randomY = rand.NextDouble() * maxY;

            // 设置图片位置
            Canvas.SetLeft(LeftImage, randomX);
            Canvas.SetTop(LeftImage, randomY);

            // 记录时间
            stopwatch = Stopwatch.StartNew();

            try
            {
                await Task.Delay(DELAY, token); // 等待DELAY时间
            }
            catch (TaskCanceledException)
            {
                // 任务被取消，直接返回
                return;
            }

            // 判断是否为目标图片，记录遗漏
            if (leftImage.UriSource == targetImage.UriSource)
            {
                skip++;
                total_time += DELAY;
                taskResults.Add(new TaskResult
                {
                    TaskNumber = attemptCount,
                    ReactionTime = 0,
                    Result = "遗漏"
                });
            }

            // 更新统计信息
            RightStatisticsAction?.Invoke(score, 10);
            WrongStatisticsAction?.Invoke(wrong + skip, 10);

            // 递归调用加载新图片，直到达到最大次数
            if (attemptCount < MAX_GAME)
            {
                LoadNewLeftImage();
            }
            else
            {
                // 强制触发游戏结束，确保在最后一张图片后结束游戏
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
                    Result = "正确"
                });
            }
            else
            {
                wrong++;
                taskResults.Add(new TaskResult
                {
                    TaskNumber = attemptCount,
                    ReactionTime = 0,
                    Result = "错误"
                });
            }
            RightStatisticsAction?.Invoke(score, 10);
            WrongStatisticsAction?.Invoke(wrong + skip, 10);
            LoadNewLeftImage(); // 按钮点击后立即加载新图片
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // 停止计时器
            stopwatch?.Stop();
            // 关闭窗口
            gameTimer?.Stop();
            OnGameEnd();

        }
    }
    public partial class 选择注意力 : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
            imagePath = FindImagePath(); // 查找目标文件夹路径
            mubiaowu.Visibility = Visibility.Visible;
            if (imagePath != null)
            {
                imagePaths = Directory.GetFiles(imagePath); // 获取文件夹中的所有文件
                LoadTargetImageSync();
            }
            else
            {
                MessageBox.Show("未找到目标文件夹，请检查路径。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                OnGameEnd();
            }
            LevelStatisticsAction?.Invoke(0, 0);
            RightStatisticsAction?.Invoke(0, 10);
            WrongStatisticsAction?.Invoke(0, 10);
            //
            totalGameTime = TimeSpan.Zero; // 重置总时间
            gameTimer = new DispatcherTimer();
            gameTimer.Tick += GameTimer_Tick; // 绑定计时器事件
            gameTimer.Interval = TimeSpan.FromSeconds(1); // 每秒更新一次
        }

        //增加代码，绑定键盘的enter键和“确认”按钮
        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // 检查按下的键是否是你指定的键
            if (e.Key == System.Windows.Input.Key.Enter) // 检测是否按下回车键
            {
                // 模拟按钮点击，调用 ConfirmButton_Click 方法，就是确认按钮的点击事件定义
                ConfirmButton_Click(null, null);
            }
        }
        //更改结束

        protected override async Task OnStartAsync()
        {
            InitializeImages(); // 确保在开始游戏时初始化图片列表
            gameTimer.Start(); // 启动计时器

            LoadNewLeftImage();
            // 调用委托
            VoiceTipAction?.Invoke("当屏幕左侧出现目标物体时，按下键盘上的OK键。");
            SynopsisAction?.Invoke("屏幕右侧会有一个目标物体，当屏幕左侧出现该目标物体时，按下键盘上的OK键，其余情况则无需做出反应。");
            RuleAction?.Invoke("屏幕右侧会有一个目标物体，当屏幕左侧出现该目标物体时，按下键盘上的OK键，其余情况则无需做出反应。");//增加代码，调用函数，显示数字人下的文字
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

            // 调用委托
            VoiceTipAction?.Invoke("当屏幕左侧出现目标物体时，按下键盘上的OK键。");
            SynopsisAction?.Invoke("屏幕右侧会有一个目标物体，当屏幕左侧出现该目标物体时，按下键盘上的OK键，其余情况则无需做出反应。");
            RuleAction?.Invoke("屏幕右侧会有一个目标物体，当屏幕左侧出现该目标物体时，按下键盘上的OK键，其余情况则无需做出反应。");
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new 选择注意力讲解();
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
        //正确率固定以21作为分母
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
                    await Task.Run(async () =>
                    {
                        // 获取当前难度级别的数据
                        int correctCount = GetCorrectNum();
                        int wrongCount = GetWrongNum();
                        int ignoreCount = GetIgnoreNum();
                        double totalMilliseconds = totalGameTime.TotalMilliseconds;  // 转换为double类型的毫秒数
                        double time = Math.Round((double)totalMilliseconds / attemptCount, 2);
                        // 计算准确率
                        double accuracy = CalculateAccuracy(correctCount);
                        // 计算 Z值反应控制值
                        double zControlValue = Math.Round((double)Math.Abs(wrongCount - 0.6) / 1.2, 2);
                        // 仅累加目标物正确点击的反应时间
                        double correctTotalReactionTime = taskResults
                            .Where(tr => tr.Result == "正确" && tr.ReactionTime.HasValue)
                            .Sum(tr => tr.ReactionTime.Value * 1000); // 转换为毫秒

                        // 使用 score 作为正确个数
                        double averageReactionTime = score > 0 ? correctTotalReactionTime / score : 0;
                        // 计算 Z值反应速度值
                        double zSpeedValue = Math.Round((double)(435 - averageReactionTime) / 87, 2);

                        // 创建 Result 记录
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "逻辑推理能力评估报告",
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
                                    ValueName = "正确",
                                    Value = correctCount,
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
                                    Value = ignoreCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "正确率",
                                    Value = Math.Round(accuracy * 100, 2), // 以百分比形式存储
                                    ModuleId = BaseParameter.ModuleId
                                },
                                 new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "平均值反应时间(ms)",
                                    Value = Math.Round(time, 2), // 以百分比形式存储
                                    ModuleId = BaseParameter.ModuleId
                                },
                                 new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Z值反应速度值",
                                    Value = Math.Round(zSpeedValue, 2),
                                    ModuleId = BaseParameter.ModuleId
                                 },
                                  new ResultDetail
                                 {
                                    ResultId = result_id,
                                    ValueName = "Z值反应控制值",
                                    Value = Math.Round(zControlValue, 2),
                                    ModuleId = BaseParameter.ModuleId
                                 }

                            };



                        resultDetails.AddRange(taskResults.OrderBy(tr => tr.TaskNumber).Select((value, index) => new ResultDetail
                        {
                            ResultId = result_id,
                            ValueName = "任务顺序,反应时间[ms]",
                            Value = Math.Round(value.ReactionTime * 1000 ?? 0, 2), // 若 ReactionTime 为 null，则赋值为 0
                            Abscissa = value.TaskNumber,
                            Charttype = "折线图",
                            ModuleId = BaseParameter.ModuleId,
                        }).ToList()
                        );


                        // 插入 ResultDetail 数据
                        db.ResultDetails.AddRange(resultDetails);
                        await db.SaveChangesAsync();
                        // 输出每个 ResultDetail 对象的数据
                        /*Debug.WriteLine($"难度级别 {lv}:");*/
                        foreach (var detail in resultDetails)
                        {
                            Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId}");
                        }
                        var sortedTaskResults = taskResults.OrderBy(tr => tr.TaskNumber).ToList();

                        // 输出每次任务的结果
                        foreach (var task in sortedTaskResults)
                        {
                            Debug.WriteLine($"Task {task.TaskNumber}: Reaction Time = {task.ReactionTime}s, Result = {task.Result}");
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
