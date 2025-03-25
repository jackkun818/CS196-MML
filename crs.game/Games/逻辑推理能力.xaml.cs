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
    /// 逻辑推理能力.xaml 的交互逻辑
    /// </summary>
    public partial class 逻辑推理能力 : BaseUserControl
    {
        private string imagePath; // 存储找到的文件夹路径
        private const int MAX_GAME = 10;
        private string[] imagePaths;
        private string[] directoryPaths;

        private int[] timecount;//该数组记录每次任务是否正确或者错误，正确记录反应时间

        private int display_index;
        private int questionCount; // 记录已展示的题目数量
        private int correctCount; // 记录正确回答的数量
        private int incorrectCount; // 记录错误回答的数量
        private DateTime startTime; // 开始时间
        private double total_time; // 总答题时间
        private DispatcherTimer gameTimer; // 计时器
        private TimeSpan totalGameTime; // 总游戏时间
        private int currentPosition = 1;

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            totalGameTime = totalGameTime.Add(TimeSpan.FromSeconds(1)); // 每次增加一秒// 获取总秒数
            int totalSeconds = (int)totalGameTime.TotalSeconds;

            // 调用委托
            TimeStatisticsAction?.Invoke(totalSeconds, totalSeconds);

        }

        public 逻辑推理能力()
        {
            InitializeComponent();
        }

        private string FindImagePath()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            {
                string targetDirectory = Path.Combine(currentDirectory, "逻辑推理能力");
                if (Directory.Exists(targetDirectory))
                {
                    return targetDirectory;
                }
                return null;
            }
            {
                string targetDirectory = Path.Combine(currentDirectory, @"Games\逻辑推理能力");
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
                    string targetDirectory = Path.Combine(targetParentDirectory, "逻辑推理能力");
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

        private void ShowImage()
        {
            directoryPaths = Directory.GetDirectories(imagePath); // 获取所有题目的路径
            Random rand = new Random();
            int index = rand.Next(directoryPaths.Length);
            imagePaths = Directory.GetFiles(directoryPaths[index]); // 某一道题目所在文件夹

            // 确保文件数量足够
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
                MessageBox.Show("当前题目文件夹中的文件不足，请检查！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                OnGameEnd();
            }
            //currentPosition = 1;
            //Grid.SetColumn(Select, currentPosition);

            Image.Source = null;
            startTime = DateTime.Now; // 记录开始时间
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
            if (questionCount >= MAX_GAME) // 展示两道题目后结束
            {
                //MessageBox.Show($"总答题时间: {total_time:F2} 秒\n正确回答: {correctCount}\n错误回答: {incorrectCount}");
                //逻辑推理能力报告 nwd=new 逻辑推理能力报告(total_time/MAX_GAME,correctCount,incorrectCount);
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
            DateTime endTime = DateTime.Now; // 记录结束时间
            TimeSpan duration = endTime - startTime; // 计算作答时间
            int durationInMilliseconds = (int)duration.TotalMilliseconds;
            if (display_index == 0) // 假定第5张图片对应正确答案
            {
                correctCount++; // 正确计数加1
                timecount[questionCount] = durationInMilliseconds;

                PlayWav(CorrectSoundPath);
                ShowFeedbackImage(CorrectImage);
            }
            else
            {
                incorrectCount++; // 错误计数加1
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

            total_time += duration.TotalSeconds; // 累加总时间
            RightStatisticsAction?.Invoke(correctCount, 10);
            WrongStatisticsAction?.Invoke(incorrectCount, 10);
            CheckQuestionCount();

            display_index = -1;
        }

        private void Button_Click_(object sender, RoutedEventArgs e)
        {
            OnGameEnd();
        }


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


    }
    public partial class 逻辑推理能力 : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {

            CorrectSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");
            // 为 Image 控件加载图片 Source
            CorrectImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Correct.png"), UriKind.RelativeOrAbsolute));
            ErrorImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Error.png"), UriKind.RelativeOrAbsolute));

            questionCount = 0; // 初始化题目计数
            correctCount = 0; // 初始化正确计数
            incorrectCount = 0; // 初始化错误计数
            total_time = 0; // 初始化总时间
            display_index = -1;

            // 查找目标文件夹路径
            imagePath = FindImagePath();
            if (imagePath == null)
            {
                MessageBox.Show("未找到名为“逻辑推理能力”的文件夹。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
            totalGameTime = TimeSpan.Zero; // 重置总时间
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1); // 每秒更新一次
            gameTimer.Tick += GameTimer_Tick; // 绑定计时器事件
        }

        protected override async Task OnStartAsync()
        {

            gameTimer.Start(); // 启动计时器
            ShowImage();
            // 调用委托
            VoiceTipAction?.Invoke("请在提供的若干图像中找出一个合适选项。");
            SynopsisAction?.Invoke("您会看到屏幕上有一个图像序列，该序列的图像相互之间存在着某种规律，您需要思考发现规律，并在提供的若干图像中用左右按键选择并按下OK键确认来找出一个合适选项。");
            RuleAction?.Invoke("您会看到屏幕上有一个图像序列，该序列的图像相互之间存在着某种规律，您需要思考发现规律，并在提供的若干图像中用左右按键选择并按下OK键确认来找出一个合适选项。");//增加代码，调用函数，显示数字人下的文字
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

            // 调用委托
            VoiceTipAction?.Invoke("请在提供的若干图像中找出一个合适选项。");
            SynopsisAction?.Invoke("您会看到屏幕上有一个图像序列，该序列的图像相互之间存在着某种规律，您需要思考发现规律，并在提供的若干图像中用左右按键选择并按下OK键确认来找出一个合适选项。");
            RuleAction?.Invoke("您会看到屏幕上有一个图像序列，该序列的图像相互之间存在着某种规律，您需要思考发现规律，并在提供的若干图像中用左右按键选择并按下OK键确认来找出一个合适选项。");
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new 逻辑思维能力讲解();
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
                        // 获取当前难度级别的数据
                        int correctCount = GetCorrectNum();
                        int wrongCount = GetWrongNum();
                        int ignoreCount = GetIgnoreNum();
                        int count = questionCount;
                        double Zvaluecorrect = (GetCorrectNum() - 4.8) / 3.2;
                        double totalMilliseconds = totalGameTime.TotalMilliseconds;  // 转换为double类型的毫秒数
                        double time = (double)total_time / questionCount;
                        // 计算准确率
                        double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);
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
                                        ValueName = "任务",
                                        Value = count,
                                        ModuleId = BaseParameter.ModuleId
                                    },
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
                                    ValueName = "正确率",
                                    Value = Math.Round(accuracy * 100, 2), // 以百分比形式存储
                                    ModuleId = BaseParameter.ModuleId
                                },
                                    new ResultDetail
                                    {
                                        ResultId = result_id,
                                        ValueName = "平均作答时间(s)",
                                        Value = Math.Round(time, 2),
                                        ModuleId = BaseParameter.ModuleId
                                    },
                                     new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Z值正确答案的数目",
                                    Value = Math.Round(Zvaluecorrect, 2), // 以百分比形式存储
                                    ModuleId = BaseParameter.ModuleId
                                }
                            };

                        resultDetails.AddRange(timecount.Select((value, index) => new ResultDetail
                        {
                            ResultId = result_id,
                            ValueName = "任务顺序,时间[s]",
                            Value = (double)value / 1000.0,
                            Abscissa = index + 1,
                            Charttype = "折线图",
                            ModuleId = BaseParameter.ModuleId,
                        }).ToList());


                        // 插入 ResultDetail 数据
                        db.ResultDetails.AddRange(resultDetails);
                        await db.SaveChangesAsync();
                        // 输出每个 ResultDetail 对象的数据
                        /*Debug.WriteLine($"难度级别 {lv}:");*/
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
