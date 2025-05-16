using crs.core;
using crs.core.DbModels;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace crs.game.Games
{
    /// <summary>
    /// GESI.xaml 的交互逻辑
    /// </summary>
    public class KeySimulator
    {
        private DispatcherTimer _rightKeyTimer;
        private Action<string> _loadContentAction;
        private Action<string> _highlightThumbnailAction;
        private Panel _thumbnailPanel;
        private int _selectedIndex;

        public KeySimulator(Panel thumbnailPanel, Action<string> loadContentAction, Action<string> highlightThumbnailAction)
        {
            _thumbnailPanel = thumbnailPanel;
            _loadContentAction = loadContentAction;
            _highlightThumbnailAction = highlightThumbnailAction;
            _rightKeyTimer = new DispatcherTimer();
            _rightKeyTimer.Tick += RightKeyTimer_Tick;
        }

        public void Start(bool isEnabled, int intervalInMilliseconds)
        {
            if (isEnabled)
            {
                _rightKeyTimer.Interval = TimeSpan.FromMilliseconds(intervalInMilliseconds);
                _rightKeyTimer.Start();
            }
            else
            {
                _rightKeyTimer.Stop();
            }
        }

        private void RightKeyTimer_Tick(object sender, EventArgs e)
        {


            if (_thumbnailPanel.Children.Count == 0) return; // 如果没有缩略图，直接返回

            // 向右滑动
            _selectedIndex = (_selectedIndex < _thumbnailPanel.Children.Count - 1) ? _selectedIndex + 1 : 0;

            // Debug: 输出当前选中的索引
            Console.WriteLine($"Selected index: {_selectedIndex}");

            SelectThumbnailByIndex(_selectedIndex);
        }


        private void SelectThumbnailByIndex(int index)
        {
            if (index < 0 || index >= _thumbnailPanel.Children.Count) return; // 边界检查

            var border = _thumbnailPanel.Children[index] as Border;
            if (border != null && border.Child is Image img)
            {
                string imagePath = ((BitmapImage)img.Source).UriSource.LocalPath;
                _loadContentAction?.Invoke(imagePath); // 加载主图片
                _highlightThumbnailAction?.Invoke(imagePath); // 高亮选中的缩略图
            }
        }
    }
    public class TimeTracker
    {
        private Stopwatch memoryStopwatch;
        private Stopwatch selectionStopwatch;
        public double totalMemoryTime;
        public double totalSelectionTime;


        public TimeTracker()
        {
            memoryStopwatch = new Stopwatch();
            selectionStopwatch = new Stopwatch();
            totalMemoryTime = 0;
            totalSelectionTime = 0;

        }

        // 开始记忆计时
        public void StartMemoryTimer()
        {
            memoryStopwatch.Restart();
        }

        // 结束记忆计时
        public void StopMemoryTimer()
        {
            memoryStopwatch.Stop();
            totalMemoryTime += memoryStopwatch.Elapsed.TotalSeconds;

        }

        // 开始选择计时
        public void StartSelectionTimer()
        {
            selectionStopwatch.Restart();
        }

        // 结束选择计时
        public void StopSelectionTimer()
        {
            selectionStopwatch.Stop();
            totalSelectionTime += selectionStopwatch.Elapsed.TotalSeconds;

        }
    }
    public partial class 容貌记忆力 : BaseUserControl
    {
        private Border selectedThumbnailBorder;
        private string[] memorizedImages; // 用户记忆的图片路径
        private string difficultyFolderPath;
        private string picFolderPath;
        private string infoFolderPath;
        private string[] allImages; // 所有图片路径
        private string selectedImagePath; // 用户当前选择的图片路径
        private bool isMemoryStage = true; // 标识当前是否在记忆阶段
        private int difficultyLevel; // 难度级别
        private int userSelections; // 用户选择次数
        private int correctSelections; // 正确选择的次数
        private int selectedIndex = 0;
        private int currentQuestionIndex; // 当前提问的索引
        private System.Timers.Timer timer;
        private int train_time; // 总训练时间

        /// <summary>
        ///  新的计时器用于训练时间
        /// </summary>
        private DispatcherTimer trainingTimer;



        private TimeSpan remainingTime;
        private bool nameOrNot = false;
        private bool jobOrNot = false;
        private bool numberOrNot = false;
        private List<string> correctImagePaths = new List<string>(); // 存储正确的图片路径
        private string correctImagePath;
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private Random random = new Random();
        private TimeTracker timeTracker;
        private DispatcherTimer clearInfoTextTimer;

        private int memoryNum = 0;
        private KeySimulator _keySimulator;

        private Queue<int> recentCorrectSelections = new Queue<int>();
        private Queue<int> recentTotalQuestions = new Queue<int>();

        /*------------------------------可调参数------------------------------*/
        private int change_up; // 上升阈值
        private int change_down; // 下降阈值
        private bool nameOnly;//只采用姓名
        private bool infoOnly;//只采用信息
        private bool nameAndInfo;//采用姓名以及信息
        private bool soundOrNot;//是否有提示音
        private bool imageOrNot;//是否有反馈图片
        private bool sameOrNot;//是否采用相同的图片
        private int time = 30; //训练时间     
        private int repeatNum;//图片重复       
        private int inputMode;//输入模式
        private bool SingleKey;//是否开启单键输入
        private int SingleKeyTime;//单键输入时间
        /*--------------------------------------------------------------------*/

        /*------------------------------评价指标------------------------------*/
        public int[] totalNum;
        public int[] errorNum;
        public double[] errorRate;
        public int[] errorPicNum;
        public int[] errorNameNum;
        public int[] errorJobNum;
        public int[] errorPhoneNum;
        public double memorizeTime;
        public double chooseTime;
        public int HighestLevel;


        // 定义一些累积变量来存储五次游戏的数据
        private int gameCounter = 0; // 游戏次数计数器
        private int totalCorrectGames = 0; // 累积的正确选择次数
        private int totalQuestionsInAllGames = 0; // 累积的总问题数量
        private List<Question> allQuestions = new List<Question>();
        private int currenCorrectSelection;
        private int totalWrongSelection;
        private int requiredSelections;
        private int totalCorrectSelections;

        private List<int> lastFiveGamesResults = new List<int>(); // 存储最近正确的结果
        private List<int> lastFiveGamesResults_2 = new List<int>(); // 存储最近错误的结果

        private int[] correctCountsByDifficulty = new int[22]; // 每个难度的正确次数
        private int[] errorCountsByDifficulty = new int[22];   // 每个难度的错误次数

        private int correctStreak = 0; // 连续正确计数
        private int wrongStreak = 0;   // 连续错误计数


        /*--------------------------------------------------------------------*/

        /*--------------------------------------------------------------------*/

        //设定参数

        public void StartCountdown(int minutes)
        {
            //train_time = minutes*60; // 设置总训练时间
            //remainingTime = TimeSpan.FromMinutes(minutes);
            UpdateCountdownText();
            timer.Start();
        }

        //单键模式间隔设置
        private void StartOrStopSimulation(bool start, int interval)
        {
            _keySimulator.Start(start, interval);
        }
        //评价指标函数
        private void InitializeNum()
        {
            totalNum = new int[21];
            errorNum = new int[21];
            errorRate = new double[21];
            errorPicNum = new int[21];
            errorNameNum = new int[21];
            errorJobNum = new int[21];
            errorPhoneNum = new int[21];
            memorizeTime = 0;
            chooseTime = 0;
        }

        //更新当前最高等级
        private void RenewHighestLevel(int difficultL)
        {
            HighestLevel = difficultL > HighestLevel ? difficultL : HighestLevel;
        }

        //键盘控制相关函数
        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (thumbnailPanel.Children.Count == 0) return; // 如果没有缩略图，直接返回

            if (e.Key == Key.Left && SingleKey == false)
            {
                // 向左滑动
                selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : thumbnailPanel.Children.Count - 1;
                SelectThumbnailByIndex(selectedIndex);
            }
            else if (e.Key == Key.Right && SingleKey == false)
            {
                // 向右滑动
                selectedIndex = (selectedIndex < thumbnailPanel.Children.Count - 1) ? selectedIndex + 1 : 0;
                SelectThumbnailByIndex(selectedIndex);
            }
            else if (e.Key == Key.Enter)
            {
                // 按下 Enter 键自动点击 button1
                Button1.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void SelectThumbnailByIndex(int index)
        {
            if (index < 0 || index >= thumbnailPanel.Children.Count) return; // 边界检查

            var border = thumbnailPanel.Children[index] as Border;
            if (border != null && border.Child is Image img)
            {
                string imagePath = ((BitmapImage)img.Source).UriSource.LocalPath;
                LoadContent(imagePath); // 加载主图片
                HighlightThumbnail(imagePath); // 高亮选中的缩略图
                selectedImagePath = imagePath; // 记录选中的图片路径

                // 显示txt内容（根据难度等级判断）
                if (difficultyLevel >= 7)
                {
                    string infoFilePath = Path.Combine(infoFolderPath, Path.GetFileNameWithoutExtension(imagePath).Split('-')[0] + ".txt");

                    if (File.Exists(infoFilePath) && isMemoryStage)
                    {
                        string[] infoLines = File.ReadAllLines(infoFilePath);

                        // 根据难度等级显示不同的行数
                        if (difficultyLevel >= 7 && difficultyLevel <= 11)
                        {
                            // 显示第一行
                            InfoText1.Text = infoLines.Length > 0 ? infoLines[0] : "";
                        }
                        else if (difficultyLevel >= 12 && difficultyLevel <= 16)
                        {
                            // 显示第一行和第二行
                            InfoText1.Text = string.Join(Environment.NewLine, infoLines.Take(2));
                        }
                        else if (difficultyLevel >= 17 && difficultyLevel <= 21)
                        {
                            // 显示前三行
                            InfoText1.Text = string.Join(Environment.NewLine, infoLines.Take(3));
                        }
                        else
                        {
                            InfoText1.Text = ""; // 超出范围，不显示内容
                        }
                    }
                    else
                    {
                        InfoText1.Text = ""; // 文件不存在或不在记忆阶段，清空显示内容
                    }
                }
                else
                {
                    InfoText1.Text = ""; // 难度小于7时，不显示任何信息
                }
            }
        }

        //设置倒计时
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (remainingTime > TimeSpan.Zero)
            {
                remainingTime = remainingTime.Add(TimeSpan.FromSeconds(-1));
                Dispatcher.Invoke(() => UpdateCountdownText()); // 更新 UI 必须通过 Dispatcher.Invoke
                int? timeInSeconds = (int?)remainingTime.TotalSeconds;
                TimeStatisticsAction?.Invoke(train_time, timeInSeconds); // 更新统计信息
            }
            else
            {
                timer.Stop();

                // 后台处理耗时操作
                errorRate[difficultyLevel - 1] = errorNum[difficultyLevel - 1] / (double)totalNum[difficultyLevel - 1];
                timeTracker.StopMemoryTimer();
                timeTracker.StopSelectionTimer();
                memorizeTime = timeTracker.totalMemoryTime / memoryNum;
                chooseTime = timeTracker.totalSelectionTime / totalNum[difficultyLevel - 1];

                Dispatcher.Invoke(() =>
                {
                    // 打开 Report 窗口并关闭当前窗口
                    //report reportWindow = new report(HighestLevel, errorNum, errorRate, errorPicNum, errorNameNum, errorJobNum, errorPhoneNum, memorizeTime, chooseTime);
                });
            }
        }

        private void UpdateCountdownText()
        {
            //CountdownTextBlock.Text = remainingTime.ToString(@"mm\:ss");
        }

        private void TrainingTimer_Tick(object sender, EventArgs e)
        {
            train_time--; // 训练时间倒计时

            // 调用委托
            TimeStatisticsAction?.Invoke(train_time, (int?)remainingTime.TotalSeconds);

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

        // 这个方法没有引用？
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (remainingTime > TimeSpan.Zero)
            {
                remainingTime = remainingTime.Add(TimeSpan.FromSeconds(-1));
                UpdateCountdownText();
            }
            else
            {
                errorRate[difficultyLevel - 1] = errorNum[difficultyLevel - 1] / (double)totalNum[difficultyLevel - 1];
                timeTracker.StopMemoryTimer();
                timeTracker.StopSelectionTimer();
                memorizeTime = timeTracker.totalMemoryTime / memoryNum;
                chooseTime = timeTracker.totalSelectionTime / totalNum[difficultyLevel - 1];
                timer.Stop();
                // 打开 Report 窗口
                //report reportWindow = new report(HighestLevel, errorNum, errorRate, errorPicNum, errorNameNum, errorJobNum, errorPhoneNum, memorizeTime, chooseTime)

                OnGameEnd();
            }
        }



        //播放提示音乐
        private void PlayMemoryAudio()
        {
            string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName;
            string project = Directory.GetParent(projectDirectory).FullName;
            string audioPath = Path.Combine(project, "sound.mp3");

            mediaPlayer.Open(new Uri(audioPath));
            mediaPlayer.Play();

        }

        // 设置难度级别并更新UI
        public void SetDifficultyLevel(int level)
        {
            difficultyLevel = level;

            if (difficultyLevel >= 7 && difficultyLevel <= 11)
            {
                nameOrNot = true;
            }
            if (difficultyLevel >= 12)
            {
                nameOrNot = true;
                jobOrNot = true; // 难度12以上，启用职业问题
            }
            if (difficultyLevel >= 17)
            {
                nameOrNot = true;
                jobOrNot = true;
                numberOrNot = true; // 难度16以上，启用电话号码问题
            }
        }

        // 初始化文件路径
        private void InitializePaths()
        {

            // 获取当前执行文件的目录
            string currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string targetDirectory = Path.Combine(currentDirectory, "Resources");

            // 设置 difficultyFolderPath 为 Games 目录中的 "容貌记忆力" 文件夹
            difficultyFolderPath = Path.Combine(targetDirectory, "容貌记忆力");

            picFolderPath = Path.Combine(difficultyFolderPath, "pic");
            //MessageBox.Show(picFolderPath);
            infoFolderPath = Path.Combine(difficultyFolderPath, "info");

            // 确保文件夹存在

            // 获取所有图片文件（.png 和 .jpg）
            var pngFiles = Directory.GetFiles(picFolderPath, "*.png");
            var jpgFiles = Directory.GetFiles(picFolderPath, "*.jpg");
            allImages = pngFiles.Concat(jpgFiles).ToArray();
        }


        // 开始记忆阶段
        private void StartMemoryStage()
        {
            if (difficultyLevel <= 6)
            {
                HandleDifficulty1To6(); // 处理难度1-6的逻辑
            }
            else if (difficultyLevel >= 7 && difficultyLevel <= 11)
            {
                HandleDifficulty7To11();// 处理难度7-11的逻辑

            }
            else if (difficultyLevel >= 12 && difficultyLevel <= 16)
            {
                HandleDifficulty12To16(); // 处理难度12-16的逻辑

            }
            else if (difficultyLevel >= 17 && difficultyLevel <= 21)
            {
                HandleDifficulty17To21();
            }
        }

        // 处理难度1-6的逻辑
        private void HandleDifficulty1To6()
        {
            DisplayRandomImages(); // 显示随机图片供用户记忆
            isMemoryStage = true; // 切换到记忆阶段
        }

        // 处理难度7-11的逻辑
        private void HandleDifficulty7To11()
        {
            int imagesToMemorize = difficultyLevel - 5;
            DisplayRandomImages(imagesToMemorize); // 显示随机图片供用户记忆
            nameOrNot = true;
            isMemoryStage = true; // 切换到记忆阶段
        }

        private void HandleDifficulty12To16()
        {
            int imagesToMemorize = difficultyLevel - 10; // 图片数量为难度-10
            DisplayRandomImages(imagesToMemorize);
            correctImagePaths.AddRange(memorizedImages); // 将记忆的图片路径添加到正确路径列表
            nameOrNot = true;
            jobOrNot = true;
            isMemoryStage = true;
        }

        private void HandleDifficulty17To21()
        {
            int imagesToMemorize = difficultyLevel - 15; // 图片数量为难度-15
            DisplayRandomImages(imagesToMemorize);
            correctImagePaths.AddRange(memorizedImages); // 将记忆的图片路径添加到正确路径列表
            nameOrNot = true;
            jobOrNot = true;
            numberOrNot = true;
            isMemoryStage = true;
        }

        // 显示指定数量的随机图片供用户记忆
        private void DisplayRandomImages(int imagesToMemorize = 0)
        {
            currenCorrectSelection = 0;
            isMemoryStage = true;

            StartOrStopSimulation(SingleKey, SingleKeyTime);
            timeTracker.StartMemoryTimer();
            imagesToMemorize = imagesToMemorize > 0 ? imagesToMemorize : difficultyLevel;

            if (imagesToMemorize > allImages.Length)
            {

                return;
            }

            // 使用字典来存储每个文件名和它们对应的图片路径
            Dictionary<string, string> uniqueImages = new Dictionary<string, string>();
            Random random = new Random();

            // 如果 repeatNum 大于 0 并且难度没有变化，则从上一次的记忆图片中选取 repeatNum 张图片
            if (repeatNum > 0 && memorizedImages != null && memorizedImages.Length > 0)
            {
                // 随机从上次记忆的图片中选取 repeatNum 张图片
                var repeatedImages = memorizedImages.OrderBy(x => random.Next()).Take(repeatNum);
                foreach (var image in repeatedImages)
                {
                    var fileName = Path.GetFileNameWithoutExtension(image).Split('-')[0];
                    if (!uniqueImages.ContainsKey(fileName))
                    {
                        uniqueImages[fileName] = image;
                    }
                }
            }

            // 当 repeatNum 为 0 时，确保当前记忆阶段的图片与上一个记忆阶段完全不同
            List<string> remainingImages = new List<string>(allImages);
            if (repeatNum == 0 && memorizedImages != null && memorizedImages.Length > 0)
            {
                foreach (var image in memorizedImages)
                {
                    var fileName = Path.GetFileNameWithoutExtension(image).Split('-')[0];
                    // 移除所有文件名相同的图片
                    remainingImages.RemoveAll(img => Path.GetFileNameWithoutExtension(img).Split('-')[0] == fileName);
                }
            }

            // 从剩余的图片中选择剩余的图片数量
            while (uniqueImages.Count < imagesToMemorize)
            {
                var randomImage = remainingImages[random.Next(remainingImages.Count)];
                var fileName = Path.GetFileNameWithoutExtension(randomImage).Split('-')[0];

                if (!uniqueImages.ContainsKey(fileName))
                {
                    uniqueImages[fileName] = randomImage;
                }
            }

            // 将选中的图片存储到 memorizedImages 数组中
            memorizedImages = uniqueImages.Values.ToArray();
            LoadThumbnails(memorizedImages); // 显示缩略图
            LoadContent(memorizedImages[0]);




        }


        // 加载并显示主图片
        private void LoadContent(string imagePath)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
            bitmap.EndInit();

            imageControl.Source = bitmap;
            imageControl.HorizontalAlignment = HorizontalAlignment.Center;
            imageControl.VerticalAlignment = VerticalAlignment.Center;
        }

        // 清空主图片区域
        private void ClearMainImage()
        {
            imageControl.Source = null;
        }

        // 加载缩略图
        private void LoadThumbnails(string[] imagePaths)
        {
            thumbnailPanel.Children.Clear(); // 清空当前缩略图面板

            for (int i = 0; i < imagePaths.Length; i++)
            {
                string imagePath = imagePaths[i];
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.DecodePixelWidth = 120; // 缩略图宽度
                bitmap.EndInit();

                Image thumbnail = new Image
                {
                    Source = bitmap,
                    Width = 120,  // 缩略图宽度
                    Height = 120, // 缩略图高度
                    Margin = new Thickness(2) // 缩小缩略图的边距
                };

                // 为每个缩略图添加点击事件
                thumbnail.MouseLeftButtonUp += (s, e) => Thumbnail_Click(imagePath, thumbnail);

                Border border = new Border
                {
                    Child = thumbnail,
                    Padding = new Thickness(0), // 设置边框填充为0，让边框更加贴合
                };
                thumbnailPanel.Children.Add(border); // 添加到面板
            }

            // 默认选中并加载第一张图片
            if (imagePaths.Length > 0)
            {
                selectedIndex = 0; // 默认选中第一个缩略图
                SelectThumbnailByIndex(selectedIndex);
                LoadContent(imagePaths[0]); // 显示第一张图片
            }
        }


        // 缩略图点击事件处理     
        private void Thumbnail_Click(string imagePath, Image thumbnail)
        {
            LoadContent(imagePath); // 加载主图片
            HighlightThumbnail(imagePath); // 高亮选中的缩略图
            selectedImagePath = imagePath; // 记录选中的图片路径

            // 如果在难度7及以上时，显示txt内容
            if (difficultyLevel >= 7)
            {
                string infoFilePath = Path.Combine(infoFolderPath, Path.GetFileNameWithoutExtension(imagePath).Split('-')[0] + ".txt");

                if (File.Exists(infoFilePath) && isMemoryStage)
                {
                    string[] infoLines = File.ReadAllLines(infoFilePath);

                    // 根据难度等级显示不同的行数
                    if (difficultyLevel >= 7 && difficultyLevel <= 11)
                    {
                        // 显示第一行
                        InfoText1.Text = infoLines.Length > 0 ? infoLines[0] : "";
                    }
                    else if (difficultyLevel >= 12 && difficultyLevel <= 16)
                    {
                        // 显示第一行和第二行
                        InfoText1.Text = string.Join(Environment.NewLine, infoLines.Take(2));
                    }
                    else if (difficultyLevel >= 17 && difficultyLevel <= 21)
                    {
                        // 显示前三行
                        InfoText1.Text = string.Join(Environment.NewLine, infoLines.Take(3));
                    }
                    else
                    {
                        InfoText1.Text = ""; // 超出范围，不显示内容
                    }
                }
                else
                {
                    InfoText1.Text = ""; // 文件不存在或不在记忆阶段，清空显示内容
                }
            }
            else
            {
                InfoText1.Text = ""; // 难度小于7时，不显示任何信息
            }
        }


        // 高亮选中的缩略图
        private void HighlightThumbnail(string imagePath)
        {
            if (selectedThumbnailBorder != null)
            {
                selectedThumbnailBorder.BorderThickness = new Thickness(0); // 移除上一个缩略图的高亮
            }

            foreach (UIElement element in thumbnailPanel.Children)
            {
                if (element is Border border && border.Child is Image img && img.Source is BitmapImage bitmap && bitmap.UriSource.ToString() == new Uri(imagePath, UriKind.Absolute).ToString())
                {
                    border.BorderBrush = Brushes.Red;
                    border.BorderThickness = new Thickness(1.5); // 较薄的红色边框
                    border.Padding = new Thickness(0); // 取消边框的填充

                    // 调整红色框的宽高，使其更贴合图片
                    border.Height = img.Height - 40;
                    border.Width = img.Width + 2;

                    border.VerticalAlignment = VerticalAlignment.Center; // 使边框与图片垂直居中对齐
                    border.HorizontalAlignment = HorizontalAlignment.Center; // 水平居中对齐

                    selectedThumbnailBorder = border;
                    break;
                }
            }
        }





        //选择阶段展示图像
        private void DisplaySelectionImages(bool sameOrNot)
        {
            StartOrStopSimulation(SingleKey, SingleKeyTime);
            memoryNum++;
            timeTracker.StopMemoryTimer();
            timeTracker.StartSelectionTimer();
            int imagesToMemorize = 0;
            if (difficultyLevel >= 17 && difficultyLevel <= 21)
            {
                imagesToMemorize = difficultyLevel - 15;
            }
            else if (difficultyLevel > 11 && difficultyLevel <= 16)
            {
                imagesToMemorize = difficultyLevel - 10; // 难度12-16时，记忆的图片数量为2-6
            }
            else if (difficultyLevel > 6 && difficultyLevel <= 11)
            {
                imagesToMemorize = difficultyLevel - 5; // 难度7-11时，记忆的图片数量为2-6
            }
            else if (difficultyLevel <= 6)
            {
                imagesToMemorize = difficultyLevel; // 难度1-6时，记忆的图片数量等于难度级别
            }

            // 用于存储选中的图片路径和文件名
            List<string> selectedImages = new List<string>();
            Random random = new Random();

            // 用于存储记忆图片的文件名（不包括序号）
            HashSet<string> memorizedFileNames = new HashSet<string>();

            // 确保选择包含记忆的图片
            foreach (var image in memorizedImages.Take(imagesToMemorize))
            {
                string fileName = Path.GetFileNameWithoutExtension(image).Split('-')[0];
                memorizedFileNames.Add(fileName);
                selectedImages.Add(image);
            }

            if (!sameOrNot)
            {
                // 替换记忆图片的图片为相同文件名的不同序号图片
                Dictionary<string, string> fileNameToImageMap = new Dictionary<string, string>();
                foreach (var image in memorizedImages.Take(imagesToMemorize))
                {
                    string fileName = Path.GetFileNameWithoutExtension(image).Split('-')[0];
                    fileNameToImageMap[fileName] = image;
                }

                selectedImages = selectedImages.Except(fileNameToImageMap.Values).ToList(); // 移除原记忆图片

                // 替换为不同序号的图片
                foreach (var fileName in fileNameToImageMap.Keys.ToList())
                {
                    var matchingImages = allImages
                        .Where(img => Path.GetFileNameWithoutExtension(img).StartsWith(fileName) &&
                                      img != fileNameToImageMap[fileName])
                        .ToArray();

                    if (matchingImages.Length > 0)
                    {
                        selectedImages.Add(matchingImages[random.Next(matchingImages.Length)]);
                    }
                }
            }

            // 确保选择的图片每个文件名只出现一个序号
            Dictionary<string, string> uniqueFileNames = new Dictionary<string, string>();
            foreach (var image in selectedImages)
            {
                string fileName = Path.GetFileNameWithoutExtension(image).Split('-')[0];
                if (!uniqueFileNames.ContainsKey(fileName))
                {
                    uniqueFileNames[fileName] = image;
                }
            }

            // 确保选中的图片不超过7张
            while (uniqueFileNames.Count < 7)
            {
                string randomImage = allImages[random.Next(allImages.Length)];
                string fileName = Path.GetFileNameWithoutExtension(randomImage).Split('-')[0];
                if (!uniqueFileNames.ContainsKey(fileName))
                {
                    uniqueFileNames[fileName] = randomImage;
                }
            }

            // 将字典转换为数组并随机打乱
            string[] finalImages = uniqueFileNames.Values.ToArray();
            finalImages = finalImages.OrderBy(x => random.Next()).ToArray();
            selectedImagePath = finalImages[0];
            // 显示缩略图
            LoadThumbnails(finalImages);

            // 清空主图片
            ClearMainImage();
        }

        public 容貌记忆力()
        {
            InitializeComponent();
        }

        public static Window GetTopLevelWindow(UserControl userControl)
        {
            DependencyObject current = userControl;
            while (current != null && !(current is Window))
            {
                current = VisualTreeHelper.GetParent(current);
            }

            return current as Window;
        }




        // 继续按钮点击事件处理
        private void PressContinue_Button(object sender, RoutedEventArgs e)
        {
            if (InfoText3.Text.Contains("请找出符合记忆阶段信息对应的人物图片"))
            {
                totalNum[difficultyLevel - 1]++;
            }
            if (isMemoryStage)
            {
                // 记忆阶段结束，进入选择阶段
                DisplaySelectionImages(sameOrNot); // 显示选择阶段的图片
                LoadContent(selectedImagePath);
                InfoText1.Text = "";
                InfoText2.Text = "选择记忆过的图片";
                InfoText3.Text = "请找出符合记忆阶段信息对应的人物图片";
                Button1.Content = "确认选择";
                isMemoryStage = false; // 切换到选择阶段
                userSelections = 0; // 重置用户选择次数
                correctSelections = 0; // 重置正确选择次数                                      
                                       //selectedImagePath = null;// 重新设置 selectedImagePath 为 null

                // 仅在难度大于6时调用 AskNextQuestion()
                if (difficultyLevel >= 7)
                {
                    AskNextQuestion(); // 进入提问阶段
                }
            }
            else
            {
                HandleSelection(); // 处理选择阶段的逻辑
            }
        }

        // 获取正确的图片路径
        private string GetCorrectImagePath()
        {
            // 根据问题类型，返回正确的图片路径
            if (InfoText2.Text.Contains("谁是") || InfoText2.Text.Contains("谁的号码是"))
            {
                // 返回与当前问题对应的信息的图片路径
                return memorizedImages[currentQuestionIndex];
            }

            // 如果问题类型是"谁的职业是"，不返回路径，而是返回null
            if (InfoText2.Text.Contains("谁的职业是"))
            {
                return null;
            }

            // 如果问题类型不匹配，返回null
            return null;
        }

        // 处理用户选择阶段

        private void LoadImage(string imagePath)
        {
            if (File.Exists(imagePath))
            {
                // 创建 BitmapImage 对象并设置图像源
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.EndInit();

                // 设置 Image 控件的源
                CorrectOrNot.Source = bitmap;
            }

        }
        private async void HandleSelection()
        {
            // 根据难度级别设置的问题数量
            requiredSelections = 0;
            if (difficultyLevel <= 6)
            {
                requiredSelections = difficultyLevel;
            }
            else if (difficultyLevel >= 7 && difficultyLevel <= 11)
            {
                requiredSelections = difficultyLevel - 5;
            }
            else if (difficultyLevel >= 12 && difficultyLevel <= 16)
            {
                requiredSelections = (difficultyLevel - 10) * 2;
            }
            else if (difficultyLevel >= 17 && difficultyLevel <= 21)
            {
                requiredSelections = (difficultyLevel - 15) * 3;
            }

            if (selectedImagePath == null)
            {
                MessageBox.Show("请选择一张图片！");
                return;
            }

            bool isCorrect = false;

            if (InfoText2.Text.Contains("选择记忆过的图片"))
            {
                isCorrect = memorizedImages.Any(image => Path.GetFileName(image).Split('-')[0] == Path.GetFileName(selectedImagePath).Split('-')[0]);
            }
            else if (InfoText2.Text.Contains("谁的职业是"))
            {
                // 根据职业信息判断正确性
                string selectedFileName = Path.GetFileNameWithoutExtension(selectedImagePath).Split('-')[0];
                string selectedInfoFilePath = Path.Combine(infoFolderPath, selectedFileName + ".txt");

                if (File.Exists(selectedInfoFilePath))
                {
                    string[] selectedInfoLines = File.ReadAllLines(selectedInfoFilePath);
                    string selectedJob = selectedInfoLines.Length > 1 ? selectedInfoLines[1] : "未知职业";
                    string correctJob = InfoText2.Text.Replace("谁的职业是 ", "").Replace("?", "").Trim();

                    isCorrect = selectedJob.Equals(correctJob, StringComparison.OrdinalIgnoreCase);
                }
            }
            else
            {
                isCorrect = Path.GetFileName(selectedImagePath).Split('-')[0].Equals(Path.GetFileName(correctImagePath).Split('-')[0], StringComparison.OrdinalIgnoreCase);
            }

            if (isCorrect)
            {
                //InfoText4.Text = "正确";
                string currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string targetDirectory = Path.Combine(currentDirectory, "Resources");
                // "容貌记忆力" 文件夹
                string FolderPath = Path.Combine(targetDirectory, "容貌记忆力");
                string picPath = Path.Combine(FolderPath, "correct.png");
                if (imageOrNot) LoadImage(picPath);
                if (soundOrNot) PlayWav(CorrectSoundPath);
                await Task.Delay(1500);

                InfoText4.Foreground = Brushes.Green;
                correctSelections++;
                currenCorrectSelection++;
                correctCountsByDifficulty[difficultyLevel]++; // 更新当前难度级别的正确次数
            }
            else
            {
                errorNum[difficultyLevel - 1]++;
                errorCountsByDifficulty[difficultyLevel]++; // 更新当前难度级别的错误次数

                if (soundOrNot == true)
                {
                    PlayMemoryAudio();
                }

                if (InfoText2.Text.Contains("谁的职业是"))
                {
                    errorJobNum[difficultyLevel - 1]++;
                }
                else if (InfoText2.Text.Contains("谁是"))
                {
                    errorNameNum[difficultyLevel - 1]++;
                }
                else if (InfoText2.Text.Contains("谁的号码是"))
                {
                    errorPhoneNum[difficultyLevel - 1]++;
                }
                else
                {
                    errorPicNum[difficultyLevel - 1]++;
                }
                //InfoText4.Text = "错误";
                string currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string targetDirectory = Path.Combine(currentDirectory, "Resources");
                // "容貌记忆力" 文件夹
                string FolderPath = Path.Combine(targetDirectory, "容貌记忆力");
                string picPath = Path.Combine(FolderPath, "error.png");

                if (imageOrNot) LoadImage(picPath);
                if (soundOrNot) PlayWav(ErrorSoundPath);
                await Task.Delay(1500);
            }

            userSelections++;

            if (userSelections >= requiredSelections)
            {
                ShowAccuracy(); // 显示正确率
                ResetToMemoryStage(); // 重置为记忆阶段
            }
            else
            {
                AskNextQuestion(); // 继续提问下一个问题
            }
        }


        private class Question
        {
            public string ImagePath { get; set; }
            public string QuestionText { get; set; }
        }
        private void AskNextQuestion()
        {
            // 在第一次提问前，生成并打乱所有问题
            if (currentQuestionIndex == 0)
            {
                GenerateAllQuestions(); // 根据难度等级生成相应数量的问题
                ShuffleQuestions(); // 打乱问题顺序
            }

            // 确保 currentQuestionIndex 在有效范围内
            if (currentQuestionIndex < allQuestions.Count)
            {
                // 选择下一个随机问题
                var currentQuestion = allQuestions[currentQuestionIndex];
                InfoText2.Text = currentQuestion.QuestionText;
                correctImagePath = currentQuestion.ImagePath;

                currentQuestionIndex++;
            }
            else
            {
                ShowAccuracy(); // 显示正确率
                ResetToMemoryStage(); // 重置为记忆阶段
            }
        }

        private void GenerateAllQuestions()
        {
            allQuestions.Clear(); // 清空上次的题目列表

            int questionsToGenerate = difficultyLevel; // 根据难度等级生成问题数量

            // 如果当前难度级别是1-6，生成相应数量的问题
            if (difficultyLevel <= 6)
            {
                for (int i = 0; i < questionsToGenerate; i++)
                {
                    if (i < memorizedImages.Length) // 确保不会超出记忆图片数量
                    {
                        string imagePath = memorizedImages[i];
                        allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = "选择记忆过的图片" });
                    }
                }
            }
            else
            {
                // 难度等级大于6时，继续使用已有逻辑
                for (int i = 0; i < memorizedImages.Length; i++)
                {
                    string imagePath = memorizedImages[i];
                    string infoFilePath = Path.Combine(infoFolderPath, Path.GetFileNameWithoutExtension(imagePath).Split('-')[0] + ".txt");

                    if (File.Exists(infoFilePath))
                    {
                        string[] infoLines = File.ReadAllLines(infoFilePath);
                        string name = infoLines.Length > 0 ? infoLines[0] : "未知姓名";
                        string job = infoLines.Length > 1 ? infoLines[1] : "未知职业";
                        string number = infoLines.Length > 2 ? infoLines[2] : "未知号码";
                        if (difficultyLevel >= 7 && difficultyLevel < 12)
                        {
                            if (nameOnly)
                            {
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = $"谁是 {name}?" });
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = "选择记忆过的图片" });
                            }
                            if (infoOnly)
                            {
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = "选择记忆过的图片" });
                            }
                            if (nameAndInfo)
                            {
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = $"谁是 {name}?" });
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = "选择记忆过的图片" });
                            }

                        }
                        if (difficultyLevel >= 12 && difficultyLevel < 17)
                        {
                            if (nameOnly)
                            {
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = $"谁是 {name}?" });
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = "选择记忆过的图片" });
                            }
                            if (infoOnly)
                            {
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = $"谁的职业是 {job}?" });
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = "选择记忆过的图片" });
                            }
                            if (nameAndInfo)
                            {
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = $"谁是 {name}?" });
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = $"谁的职业是 {job}?" });
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = "选择记忆过的图片" });
                            }

                        }
                        if (difficultyLevel >= 17 && difficultyLevel <= 21)
                        {
                            if (nameOnly)
                            {
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = $"谁是 {name}?" });
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = "选择记忆过的图片" });
                            }
                            if (infoOnly)
                            {
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = $"谁的职业是 {job}?" });
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = $"谁的号码是 {number}?" });
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = "选择记忆过的图片" });
                            }
                            if (nameAndInfo)
                            {
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = $"谁是 {name}?" });
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = $"谁的职业是 {job}?" });
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = $"谁的号码是 {number}?" });
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = "选择记忆过的图片" });
                            }

                        }


                    }
                }
            }
        }


        private void ShuffleQuestions()
        {
            // 使用随机算法打乱问题列表
            Random rng = new Random();
            int n = allQuestions.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                var value = allQuestions[k];
                allQuestions[k] = allQuestions[n];
                allQuestions[n] = value;
            }
        }


        // 显示正确率(评价指标)



        private void ShowAccuracy()
        {
            // 检查当前游戏是否全部正确
            if (currenCorrectSelection == requiredSelections)
            {
                correctStreak++;
                // wrongStreak = 0; // 重置错误计数
                totalCorrectSelections++;
                lastFiveGamesResults.Add(1); // 正确一次，存储1
            }
            else
            {
                wrongStreak++;
                // correctStreak = 0; // 重置正确计数
                lastFiveGamesResults_2.Add(1); // 错误一次，存储1
            }

            // 保证只存储最近五次的结果
            if (lastFiveGamesResults.Count > change_up)
            {
                lastFiveGamesResults.RemoveAt(0);
            }
            if (lastFiveGamesResults_2.Count > change_down)
            {
                lastFiveGamesResults_2.RemoveAt(0);
            }

            // 判断是否需要调整难度
            if (correctStreak >= change_up && difficultyLevel < 21)
            {
                difficultyLevel++;
                ResetStreaks(); // 重置计数器
                RenewHighestLevel(difficultyLevel);
            }
            else if (wrongStreak >= change_down)
            {
                if (difficultyLevel > 1)
                {
                    difficultyLevel--;
                    RenewHighestLevel(difficultyLevel);

                    // 根据特定难度调整标志变量
                    switch (difficultyLevel + 1) // 检查原始难度
                    {
                        case 7:
                            nameOrNot = false;
                            break;
                        case 12:
                            jobOrNot = false;
                            break;
                        case 17:
                            numberOrNot = false;
                            break;
                    }
                }

                ResetStreaks(); // 重置计数器
            }

            // 触发统计更新
            RightStatisticsAction?.Invoke(lastFiveGamesResults.Sum(), change_up);
            WrongStatisticsAction?.Invoke(lastFiveGamesResults_2.Sum(), change_down);
            LevelStatisticsAction?.Invoke(difficultyLevel, 20);
        }

        // 重置连续计数器
        private void ResetStreaks()
        {
            correctStreak = 0;
            wrongStreak = 0;
            lastFiveGamesResults.Clear();
            lastFiveGamesResults_2.Clear();

        }

        // 重置为记忆阶段
        private void ResetToMemoryStage()
        {
            timeTracker.StopSelectionTimer();
            //InfoText1.Text = "";
            InfoText2.Text = "";
            InfoText3.Text = "请记住下面的人物图像信息";

            // 创建一个 DispatcherTimer
            clearInfoTextTimer = new DispatcherTimer();
            clearInfoTextTimer.Interval = TimeSpan.FromSeconds(0); // 1秒间隔

            // 定义计时器的 Tick 事件处理程序
            clearInfoTextTimer.Tick += (s, args) =>
            {
                InfoText4.Text = ""; // 1秒后清空文本
                CorrectOrNot.Source = null;
                clearInfoTextTimer.Stop(); // 停止计时器
            };

            // 启动计时器
            clearInfoTextTimer.Start();

            Button1.Content = "记忆完成";
            selectedImagePath = null;
            currentQuestionIndex = 0; // 重置问题索引
            StartMemoryStage(); // 重新开始记忆阶段
        }
        private void StopAllTimers()
        {

            // 停止总用时计时器
            timer?.Stop();

            //
            trainingTimer?.Stop();

            // 停止清空文本计时器
            clearInfoTextTimer?.Stop();

            // 停止当前题目用时计时器（如果有的话）
            //timeTracker.StopMemoryTimer(); // 停止记忆阶段计时
            //timeTracker.StopSelectionTimer(); // 停止选择阶段计时
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

    }
    public partial class 容貌记忆力 : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
            int trainTime; int upThreshold; int downThreshold; int repeat;
            bool nameO; bool infoO; bool infoOname; bool sameO; bool soundO; bool single; int keyTime;

            // 此处应该由客户端传入参数为准，目前先用测试数据
            {
                int time = 1;
                int change_up = 5;
                int change_down = 5;
                int repeatNum = 0;
                bool nameOnly = true;
                bool infoOnly = false;
                bool nameAndInfo = false;
                bool soundOrNot = true;
                bool sameOrNot = true;
                bool SingleKey = false;
                int SingleKeyTime = 5000;

                trainTime = time;
                upThreshold = change_up;
                downThreshold = change_down;
                repeat = repeatNum;
                nameO = nameOnly;
                infoO = infoOnly;
                infoOname = nameAndInfo;
                sameO = sameOrNot;
                soundO = soundOrNot;
                single = SingleKey;
                keyTime = SingleKeyTime;
            }
            time = trainTime;
            change_up = upThreshold;
            change_down = downThreshold;
            repeatNum = repeat;
            nameOnly = nameO;
            infoOnly = infoO;
            nameAndInfo = infoOname;
            sameOrNot = sameO;
            soundOrNot = soundO;
            SingleKey = single;
            SingleKeyTime = keyTime;
            //this.KeyDown += Window_KeyDown;
            // 初始化正确和错误答案的计数数组
            timeTracker = new TimeTracker();
            _keySimulator = new KeySimulator(thumbnailPanel, LoadContent, HighlightThumbnail);
            InitializeNum();
            HighestLevel = 1;

            SetDifficultyLevel(1); // 默认难度级别为1   

            InitializePaths();

            /*LJN
            配置反馈资源的路径             
             */
            CorrectSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");


            {
                // 参数（包含模块参数信息）
                var baseParameter = BaseParameter;

                if (baseParameter.ProgramModulePars != null && baseParameter.ProgramModulePars.Any())

                {
                    Debug.WriteLine("ProgramModulePars 已加载数据：");

                    // 遍历 ProgramModulePars 打印出每个参数
                    foreach (var par in baseParameter.ProgramModulePars)
                    {
                        /*Debug.WriteLine($"ProgramId: {par.ProgramId},ModuleParId: {par.ModuleParId}, Value: {par.Value}, TableId: {par.TableId}");*/
                        if (par.ModulePar.ModuleId == baseParameter.ModuleId)
                        {
                            switch (par.ModuleParId)
                            {
                                case 179: // 难度
                                    difficultyLevel = par.Value.HasValue ? (int)par.Value.Value : 1;
                                    Debug.WriteLine($"HARDNESS: {difficultyLevel}");
                                    break;
                                case 125: // 治疗时间
                                    time = par.Value.HasValue ? (int)par.Value.Value : 25;
                                    Debug.WriteLine($"time={time}");
                                    break;
                                case 126: // 等级提高
                                    change_up = par.Value.HasValue ? (int)par.Value.Value : 5;
                                    Debug.WriteLine($"change_up={change_up}");
                                    break;
                                case 127: // 等级降低
                                    change_down = par.Value.HasValue ? (int)par.Value.Value : 5;
                                    Debug.WriteLine($"change_down ={change_down}");
                                    break;
                                case 129: // 通过关联信息识别
                                    infoOnly = par.Value == 1;
                                    Debug.WriteLine($"是否通过关联信息识别 ={infoOnly}");
                                    break;
                                case 130: // 通过姓名识别
                                    nameOnly = par.Value == 1;
                                    Debug.WriteLine($"是否通过姓名识别 ={nameOnly}");
                                    break;
                                case 131: // 姓名或关联信息识别
                                    nameAndInfo = par.Value == 1;
                                    Debug.WriteLine($"是否姓名或关联信息识别 ={nameAndInfo}");
                                    break;
                                case 132: // 听觉反馈
                                    soundOrNot = par.Value == 1;
                                    Debug.WriteLine($"是否听觉反馈 ={soundOrNot}");
                                    break;
                                case 133: // 相同的图片
                                    sameOrNot = par.Value == 1;
                                    Debug.WriteLine($"是否相同的图片 ={sameOrNot}");
                                    break;
                                case 264://视觉反馈
                                    imageOrNot = par.Value == 1;
                                    Debug.WriteLine($"imageOrNot ={imageOrNot}");
                                    break;
                                // 添加其他需要处理的 ModuleParId
                                default:
                                    Debug.WriteLine($"未处理的 ModuleParId:{par.ModuleParId} ");
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("没有数据");
                }

                // 调用委托，显示难度等级，正确和错误次数
                LevelStatisticsAction?.Invoke(difficultyLevel, 21);
                RightStatisticsAction?.Invoke(0, change_up);
                WrongStatisticsAction?.Invoke(0, change_down);
            }



            train_time = time * 60; // 设置总训练时间
            remainingTime = TimeSpan.FromMinutes(time);
        }

        protected override async Task OnStartAsync()
        {

            timer = new System.Timers.Timer(1000); // 每秒触发一次
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true; // 确保 Timer 每次都触发
            timer.Enabled = true; // 启动计时器
            timer.Start();

            trainingTimer?.Stop();
            trainingTimer = new DispatcherTimer();
            trainingTimer.Interval = TimeSpan.FromSeconds(1);
            trainingTimer.Tick += TrainingTimer_Tick;
            trainingTimer.Start(); // 启动训练计时器


            StartCountdown(time);
            StartMemoryStage(); // 开始记忆阶段

            // 调用委托
            VoiceTipAction?.Invoke("请找出您所记住的那个人的容貌。");
            SynopsisAction?.Invoke("您在屏幕上会先看到一个人的容貌图像，请记住其特征，记忆完成后用鼠标点击右下角的“记忆完成”按钮。随后后出现一系列的人物容貌图像，您需要根据您所记住的特征来区分出哪个是您刚刚所记住的那个人的容貌，并用鼠标点击右下角“确认选择”按钮。");
            RuleAction?.Invoke("您在屏幕上会先看到一个人的容貌图像，请记住其特征，记忆完成后用鼠标点击右下角的“记忆完成”按钮。随后后出现一系列的人物容貌图像，您需要根据您所记住的特征来区分出哪个是您刚刚所记住的那个人的容貌，并用鼠标点击右下角“确认选择”按钮。");//增加代码，调用函数，显示数字人下的文字

        }

        protected override async Task OnStopAsync()
        {
            StopAllTimers();
            memorizeTime = timeTracker.totalMemoryTime / memoryNum;
        }

        protected override async Task OnPauseAsync()
        {
            StopAllTimers();
        }

        protected override async Task OnNextAsync()
        {
            // 调整难度
            //ShowAccuracy();
            ResetToMemoryStage();

            // 调用委托
            VoiceTipAction?.Invoke("请找出您所记住的那个人的容貌。");
            SynopsisAction?.Invoke("您在屏幕上会先看到一个人的容貌图像，请记住其特征，记忆完成后用鼠标点击右下角的“记忆完成”按钮。随后后出现一系列的人物容貌图像，您需要根据您所记住的特征来区分出哪个是您刚刚所记住的那个人的容貌，并用鼠标点击右下角“确认选择”按钮。");
            RuleAction?.Invoke("您在屏幕上会先看到一个人的容貌图像，请记住其特征，记忆完成后用鼠标点击右下角的“记忆完成”按钮。随后后出现一系列的人物容貌图像，您需要根据您所记住的特征来区分出哪个是您刚刚所记住的那个人的容貌，并用鼠标点击右下角“确认选择”按钮。");
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new 容貌记忆力讲解();
        }

        // 插入写法
        private int GeterrorPicNum()
        {
            return errorPicNum[difficultyLevel - 1];
        }
        private int GeterrorNameNum()
        {
            return errorNameNum[difficultyLevel - 1];
        }
        private int GeterrorJobNum(int difficultyLevel)
        {
            return errorJobNum[difficultyLevel - 1];
        }
        private int GeterrorPhoneNum(int difficultyLevel)
        {
            return errorPhoneNum[difficultyLevel - 1];
        }
        private double CalculateAccuracy(int errorPicNumCount, int errorNameNumCount, int errorJobNumCount, int errorPhoneNumCount)
        {
            int errorNumCount = errorPicNumCount + errorNameNumCount + errorJobNumCount + errorPhoneNumCount;
            int totalCount = totalNum[difficultyLevel - 1];
            return totalCount > 0 ? Math.Round((double)errorNumCount / totalCount, 2) : 0;
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
                        int errorPicNumCount = 0;
                        int errorNameNumCount = 0;
                        int errorJobNumCount = 0;
                        int errorPhoneNumCount = 0;
                        int errorNumCount = 0;
                        

                        //int errorPicNumCount = errorPicNum[difficultyLevel - 1];
                        //int errorNameNumCount = errorNameNum[difficultyLevel - 1];
                        //int errorJobNumCount = errorJobNum[difficultyLevel - 1];
                        //int errorPhoneNumCount = errorPhoneNum[difficultyLevel - 1];
                        //int errorNumCount = errorNum[difficultyLevel - 1];
                        //if (double.IsNaN(memorizeTime))
                        //{//memorizeTime有可能为NaN，导致无法记录至数据库里，所以需要处理一下
                        //    memorizeTime = 0.0;
                        //}
                        //double time = Math.Round((double)memorizeTime, 2);
                        //double chotime = Math.Round((double)chooseTime, 2);
                        //if (errorPicNumCount == 0 && errorNameNumCount == 0 && errorJobNumCount == 0 && errorPhoneNumCount == 0)
                        //{
                        //    // 如果所有数据都为0，跳过此难度级别
                        //    Debug.WriteLine($" 没有数据，跳过.");
                        //}
                        //// 计算准确率
                        //double erroraccuracy = CalculateAccuracy(errorPicNumCount, errorNameNumCount, errorJobNumCount, errorPhoneNumCount);

                        for (int i = 1; i <= HighestLevel; i++)
                        {
                            errorPicNumCount += errorPicNum[difficultyLevel - 1];
                            errorNameNumCount += errorNameNum[difficultyLevel - 1];
                            errorJobNumCount += errorJobNum[difficultyLevel - 1];
                            errorPhoneNumCount += errorPhoneNum[difficultyLevel - 1];
                            errorNumCount += errorNum[difficultyLevel - 1];
                        }
                        if (double.IsNaN(memorizeTime))
                        {//memorizeTime有可能为NaN，导致无法记录至数据库里，所以需要处理一下
                            memorizeTime = 0.0;
                        }
                        double time = Math.Round((double)memorizeTime, 2);
                        double chotime = Math.Round((double)chooseTime, 2);
                        // 计算准确率
                        double erroraccuracy = CalculateAccuracy(errorPicNumCount, errorNameNumCount, errorJobNumCount, errorPhoneNumCount);



                        // 创建 Result 记录
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "容貌记忆力",
                            Eval = false,
                            Lv = HighestLevel, // 当前的难度级别
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
                                      ValueName = "等级",
                                      Value = HighestLevel,
                                      ModuleId = BaseParameter.ModuleId
                               },
                                new ResultDetail
                               {
                                      ResultId = result_id,
                                      ValueName = "错误全部",
                                      Value = errorNumCount,
                                    Maxvalue = errorNumCount,
                                    Minvalue = 0,
                                    Charttype = "柱状图" ,
                                      ModuleId = BaseParameter.ModuleId
                               },
                                new ResultDetail
                               {
                                      ResultId = result_id,
                                      ValueName = "错误全部(%)",
                                      Value = erroraccuracy * 100,
                                      ModuleId = BaseParameter.ModuleId
                               },
                               new ResultDetail
                               {
                                      ResultId = result_id,
                                      ValueName = "错误图片",
                                      Value = errorPicNumCount,
                                      ModuleId = BaseParameter.ModuleId
                               },
                               new ResultDetail
                               {
                                      ResultId = result_id,
                                      ValueName = "错误姓名",
                                      Value = errorNameNumCount,
                                      ModuleId = BaseParameter.ModuleId
                               },
                               new ResultDetail
                               {
                                      ResultId = result_id,
                                      ValueName = "错误职业",
                                      Value = errorJobNumCount,
                                      ModuleId = BaseParameter.ModuleId
                               },
                               new ResultDetail
                               {
                                      ResultId = result_id,
                                      ValueName = "错误电话",
                                      Value = errorPhoneNumCount,
                                      ModuleId = BaseParameter.ModuleId
                               },
                               new ResultDetail
                               {
                                      ResultId = result_id,
                                      ValueName = "平均记忆时间",
                                      Value = time,
                                   Maxvalue = (int?)time,
                                    Minvalue = 0,
                                    Charttype = "柱状图" ,
                                      ModuleId = BaseParameter.ModuleId
                               },
                               new ResultDetail
                               {
                                      ResultId = result_id,
                                      ValueName = "平均选择时间（s）",
                                      Value = chotime,
                                      ModuleId = BaseParameter.ModuleId
                               }


                        };

                        // 插入 ResultDetail 数据
                        db.ResultDetails.AddRange(resultDetails);
                        await db.SaveChangesAsync();
                        // 输出每个 ResultDetail 对象的数据
                        //Debug.WriteLine($"难度级别 {lv}:");
                        foreach (var detail in resultDetails)
                        {
                            Debug.WriteLine($" {detail.ValueName}:{detail.Value}, ModuleId: {detail.ModuleId}");
                        }


                        // 提交事务
                        await transaction.CommitAsync();
                        Debug.WriteLine("插入成功");
                    });
                }
                catch (Exception ex)
                {// 回滚事务
                    await transaction.RollbackAsync();
                    Debug.WriteLine(ex.ToString());
                }
            }
        }
    }
}