using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Threading;
using crs.core;
using System.Diagnostics;
using crs.core.DbModels;
using System.Windows.Media.Animation;
using System.Windows.Forms;
using System.Media;
using static log4net.Appender.RollingFileAppender;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace crs.game.Games
{
    /// <summary>
    /// BILD.xaml 的交互逻辑
    /// </summary>
    public partial class 图形记忆力 : BaseUserControl
    {
        private readonly string[][] imagePaths = new string[][]
        {
            new string[]
{
    "BILD/1/手表.jpg",
    "BILD/1/香蕉.jpg",
    "BILD/1/西瓜.jpg",
    "BILD/1/油桃.jpg",
    "BILD/1/葡萄.jpg",
    "BILD/1/鱼.jpg",
    "BILD/1/兔子.jpg",
    "BILD/1/鞋子.jpg",
    "BILD/1/熊猫.jpg",
    "BILD/1/乌龟.jpg",
    "BILD/1/羊.jpg",
    "BILD/1/猪.jpg",
},
            new string[]
            {
                "BILD/2/电话机.jpg",
                "BILD/2/菠萝.jpg",
                "BILD/2/电视机.jpg",
                "BILD/2/番茄.jpg",
                "BILD/2/吹风机.jpg",
                "BILD/2/锤子.jpg",
                "BILD/2/大象.jpg",
                "BILD/2/电脑.jpg"
            },
            new string[]
            {
                "BILD/3/蝴蝶.jpg",
                "BILD/3/橘子.jpg",
                "BILD/3/狗.jpg",
                "BILD/3/螺丝钉.jpg",
                "BILD/3/苹果.jpg",
                "BILD/3/梨子.jpg",
                "BILD/3/闹钟.jpg",
                "BILD/3/剪刀.jpg",
                "BILD/3/猕猴桃.jpg",
                "BILD/3/猫.jpg"
            }

        };

        // 用于保存选中的图片路径
        private List<string> selectedImagePaths = new List<string>();
        private int LEVEL_DURATION = 1;
        private int total_picture_number_para = 7; // 该参数乘以正确的图片数量为总的图片数量
        private int right_picture_number = 3; // 显示的正确图片数量
        private int train_mode = 1;//训练模式，1记图选图，2记图选名，3记名选图
        private int LEVEL_UP_THRESHOLD = 85; // 提高难度的正确率阈值（百分比）
        private int LEVEL_DOWN_THRESHOLD = 70; // 降低难度的正确率阈值（百分比）
        private int max_time = 30;
        private bool IS_REALISTIC = true; // 图片是否显示为真实物体（默认显示真实图片）
        private int[] correctAnswers = new int[10];
        private int[] wrongAnswers = new int[10];
        private int[] ignoreAnswers = new int[10];
        private const int MaxGames = 10;
        private int hardness = 1;
        int max_hardness = 0;
        private const int MAX_HARDNESS = 9; // 最大难度等级
        private const int MIN_HARDNESS = 1;//最小等级
        private DispatcherTimer sharedTimer;//这个是总的治疗时间的计时器，如30分钟
        private Queue<bool> recentResults = new Queue<bool>(5); // 记录最近5次选择结果的队列
        private int imageGenerationCounter = 0;//用来在计时器当中临时计数的
        private double imageGenerationInterval = 5.0; // 控制每隔多少时间生成一次图片，是单位为s，要乘1000的
        private DispatcherTimer colorResetTimer;
        //--------------window2--------------------

        private List<string> rightImagePaths; // 正确图片的路径
        private List<string> totalImagePaths; // 总的题库图片路径
        private int totalPictureMultiplier = 7; // 参数：该参数乘以正确的图片数量为总的图片数量

        private bool IS_FIXED_INTERVAL = false; // 项目间隔是否固定（默认不固定）
        private double SPEED_FACTOR = 1.0; // 传送带的速度因素（默认值）
        private bool IS_VISUAL_FEEDBACK = true; // 是否有视觉回馈
        private bool IS_AUDITORY_FEEDBACK = true; // 是否有声学回馈
        private int TYPE_OF_INPUT = 0; // 选用哪种输入方式
        private int CHOOSE_IMAGE_COUNT = 10; // imageBorder内图片显示数量

        private bool IS_IMAGE_DETAIL = true; // 跟难度相关，决定选择图片类型是否细节
        private bool IS_IMAGE_HARD = true; // 跟难度相关，决定图片类型是难还是简单
        private double DISPLAY_TIME = 6; // 图片滑行的总展示时间
        private double REAL_DISPLAY_TIME = 6;//实际上的总时间
        double RATE_OF_ERRORIMAGE = 0.5; // 展示错误（即非image2,3,4）的概率
        double Correct_decision_rate = 0;
        private int totalDecisions;
        private int correctDecisions;
        private int errorDecisions;
        private int missDecisions;
        private const int WAIT_DELAY = 1;
        private readonly Brush defaultSelectionBoxStrokeColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));
        private int gameIndex = 0;

        //private int remainingTime = 30;
        private int GameremainingTime = 10;
        //private DispatcherTimer trainingTimer; // 程序持续时间计时器
        private DispatcherTimer gameTimer; // 素材提供的计时器
        private DateTime LastTickTime = new DateTime();//记录GameTimer_Tick上一次触发是什么时候
        private DateTime StopTime = new DateTime();//记录什么时候触发的StopDurations

        private Random random = new Random();
        private int continueButtonPressCount = 0;// 按钮按下的次数
        private bool isGameRunning = false; // 标志游戏是否正在进行
        public event Action<int> GameremainingTimeUpdated;
        public event Action<int, int[], int[]> GameStatsUpdated;


        public class ImageInfo
        {
            public string ImagePath { get; set; }
            public bool AnimationCanceled { get; set; } = false;
        }

        public 图形记忆力(int hardness_, int training_mode, int[] correctAnswers1, int[] wrongAnswers1, int[] ignoreAnswers1)
        {//似乎不会调用这个构造函数
            InitializeComponent();
            correctAnswers = correctAnswers1;
            wrongAnswers = wrongAnswers1;
            ignoreAnswers = ignoreAnswers1;
            hardness = hardness_;
            hard_set();
            sharedTimer = new DispatcherTimer();
            sharedTimer.Interval = TimeSpan.FromSeconds(1);
            sharedTimer.Tick += OnTick;
            // 随机选择指定数量的不重复图片
            train_mode = training_mode;
            List<string> allImages = imagePaths.SelectMany(x => x).ToList();
            Random random = new Random();
            selectedImagePaths = allImages.OrderBy(x => random.Next()).Take(right_picture_number).ToList();

            // 根据 training_mode 设置图片或文本的可见性
            if (training_mode == 1 || training_mode == 2)
            {
                // 显示图片，隐藏文本
                SetImagesVisible();
            }
            else if (training_mode == 3)
            {
                // 显示文本，隐藏图片
                SetTextsVisible();
            }
        }
        public 图形记忆力()
        {
            InitializeComponent();
        }

        private void ChangeSelectionBoxColor(Brush newColor)
        {
            // 更改 SelectionBox 的 Stroke 颜色
            SelectionBox.Stroke = newColor;

            // 创建一个 DispatcherTimer，设置为2秒后触发
            if (colorResetTimer != null && colorResetTimer.IsEnabled)
            {
                colorResetTimer.Stop();
            }
            else
            {
                colorResetTimer = new DispatcherTimer();
                colorResetTimer.Interval = TimeSpan.FromSeconds(1);
                colorResetTimer.Tick += ColorResetTimer_Tick;
            }

            colorResetTimer.Start();
        }

        private void ColorResetTimer_Tick(object sender, EventArgs e)//这个是框框颜色变化的计时器
        {
            // 恢复 SelectionBox 的 Stroke 颜色为默认颜色
            SelectionBox.Stroke = defaultSelectionBoxStrokeColor;

            // 停止定时器
            colorResetTimer.Stop();
        }

        private void OnTick(object sender, EventArgs e)//全局计时器每秒触发一次，如30分钟，更新游戏剩余时间和统计数据，时间耗尽时结束游戏。
        {
            if (max_time > 0)
            {
                //TimeStatisticsAction.Invoke(10, 10);
                max_time--;
                //TimeStatisticsAction.Invoke(max_time, GameremainingTime);
                if (isGameRunning)
                {
                    TimeStatisticsAction.Invoke(max_time, 0);
                }
                else
                {
                    TimeStatisticsAction.Invoke(max_time, MemorizeLimitTime);
                }
                LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
                //int correctCount = 0;
                //int incorrectCount = 0;
                //foreach (bool result in recentResults)
                //{
                //    if (result)
                //    {
                //        correctCount++;
                //    }
                //    else
                //    {
                //        incorrectCount++;
                //    }
                //}

            }
            else
            {
                sharedTimer.Stop();
                OnGameEnd();
            }
        }

        public static System.Windows.Window GetTopLevelWindow(System.Windows.Controls.UserControl userControl)
        {
            DependencyObject current = userControl;
            while (current != null && !(current is System.Windows.Window))
            {
                current = VisualTreeHelper.GetParent(current);
            }

            return current as System.Windows.Window;
        }

        private void SetImagesVisible()
        {
            //// 清除之前的图片
            //imageContainer.Children.Clear();

            //// 动态添加图片到 UniformGrid
            //foreach (var imagePath in selectedImagePaths)
            //{
            //    Image imageControl = new Image
            //    {
            //        Source = new BitmapImage(new Uri(imagePath, UriKind.Relative)),
            //        Stretch = Stretch.Uniform,
            //        Margin = new Thickness(5),
            //        Height = ImageSizeDict[hardness]//这里高度可以根据图片数量来动态调整,这里直接用hardness而不是数量是因为在等级难度列表中，hardness和数量在等级表中是相等的
            //    };

            //    imageContainer.Children.Add(imageControl);
            //}
            //imageContainer.Columns = ImageContainerColumn[selectedImagePaths.Count];
            SetImagesPosition();
        }

        private void SetTextsVisible()
        {
            //// 清除之前的文本
            //imageContainer.Children.Clear();

            //// 动态添加文本到 UniformGrid
            //foreach (var imagePath in selectedImagePaths)
            //{
            //    TextBlock textBlock = new TextBlock
            //    {
            //        Text = Path.GetFileNameWithoutExtension(imagePath),
            //        FontSize = 120,
            //        Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black),
            //        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            //        VerticalAlignment = VerticalAlignment.Center,
            //        TextWrapping = TextWrapping.Wrap
            //    };

            //    imageContainer.Children.Add(textBlock);
            //}
            SetTextsPosition();

        }

        // ContinueButton_Click 事件处理程序
        private void ContinueButton_Click(object sender, RoutedEventArgs e)//开始游戏的点击函数
        {
            if (IfMemorizeLimit) { EndMemorizeLimit(); }//每次按下这个按钮都要结束一下记忆阶段的计时

            if (selectedImagePaths.Count > 0)
            {
                // 创建并打开新窗口，同时传递选中的图片路径列表
                //BILD_Answerwindow window1 = new BILD_Answerwindow(selectedImagePaths, train_mode, hardness);
                //window1.GameStatsUpdated += OnGameStatsUpdated;
                //window1.GameremainingTimeUpdated += OnGameremainingTimeUpdated;
                //window1.Show();
                Grid1.Visibility = Visibility.Collapsed;
                Grid2.Visibility = Visibility.Visible;

                // 启动程序计时器
                // 初始化剩余时间（以秒为单位）
                //remainingTime = remainingTime * 60;
                hard_set();
                //train_mode = 1;

                //GameremainingTime = 30;
                //GameremainingTime = 30;
                rightImagePaths = selectedImagePaths;

                GenerateTotalImagePaths();
                // 启动程序计时器
                GameremainingTime = LEVEL_DURATION * 60;

                //这个计时器没啥用，直接注释掉
                //trainingTimer = new DispatcherTimer();
                //trainingTimer.Interval = TimeSpan.FromSeconds(1); // 每秒更新一次
                //trainingTimer.Tick += TrainingTimer_Tick;
                //trainingTimer.Start();
                StartGame();
            }
            else
            {
                //MessageBox.Show("未选择任何图片路径。");
            }
        }

        //private void OnGameremainingTimeUpdated(int remainingTime)//无用函数
        //{// 调用操作的示例
        //    TimeStatisticsAction.Invoke(GameremainingTime, max_time);
        //}

        private void StartGame()
        {
            if (!isGameRunning)
            {
                isGameRunning = true;
                // 启动游戏计时器
                // 检查是否已经存在 displayTimer 实例
                if (gameTimer != null)
                {
                    // 如果计时器正在运行，先停止它
                    if (gameTimer.IsEnabled)
                    {
                        gameTimer.Stop();
                    }

                    // 取消之前注册的事件（防止重复注册事件）
                    gameTimer.Tick -= GameTimer_Tick;

                    // 将 displayTimer 置为 null，表示它已被清理
                    gameTimer = null;
                }
                //每次开始游戏的时候都要设定动画速度
                REAL_DISPLAY_TIME = DISPLAY_TIME / SPEED_FACTOR; // 合成真正时间,因为SPEED_FACTOR是倍数，越大速度越快，所以时间越短
                imageGenerationInterval = (double)(REAL_DISPLAY_TIME / 3); //间隔设定，除以的这个倍数大致可以这么确定：除以多少倍就相当于一个传送带上会出现多少个图片素材

                gameTimer = new DispatcherTimer();
                gameTimer.Interval = TimeSpan.FromMilliseconds(imageGenerationInterval * 1000);//*1000是因为这里需要毫秒级的精度
                gameTimer.Tick += GameTimer_Tick;
                gameTimer.Start();
            }

        }

        private void NotifyGameStatsUpdated()//这个应该也是个无用函数
        {
            GameStatsUpdated?.Invoke(hardness, correctAnswers, wrongAnswers);
        }

        //protected override void OnClosed(EventArgs e)
        //{
        //    base.OnClosed(e);
        //    TimerManager.TimerElapsed -= OnTimerElapsed;
        //}

        //private void TrainingTimer_Tick(object sender, EventArgs e)//训练计时器每秒触发一次，更新训练的剩余时间和相关统计数据。
        //{//无用函数，GameremainingTimeUpdated为null
        //    if (true)
        //    {
        //        //remainingTime--;
        //        GameremainingTimeUpdated?.Invoke(GameremainingTime);
        //    }
        //    else
        //    {
        //        //trainingTimer.Stop();
        //        //gameTimer?.Stop();
        //        //isGameRunning = false;
        //        //AUFM_Report reportWindow = new AUFM_Report(LEVEL_UP_THRESHOLD, LEVEL_DOWN_THRESHOLD, max_time, LEVEL_DURATION, true, IS_REALISTIC, correctAnswers, wrongAnswers, ignoreAnswers);
        //    }
        //}

        private void GameTimer_Tick(object sender, EventArgs e)//游戏计时器每隔设定的时间触发一次，更新游戏剩余时间，生成新图片，检测游戏结束条件。
        {
            LastTickTime = DateTime.Now;
            //if (GameremainingTime > 0)
            if (true)
            {
                GameremainingTime--;
                try
                {
                    //TimeStatisticsAction.Invoke(max_time, GameremainingTime);
                }
                catch (Exception ex)
                {
                    // 可以记录异常日志或其他处理方式
                    // Console.WriteLine($"Exception occurred: {ex.Message}");
                }
                //imageGenerationCounter++;
                //if (imageGenerationCounter >= imageGenerationInterval)
                //{
                //    imageGenerationCounter = 0;  // 重置计数器
                //}
                if (ImageNumTemp > 0)
                {//说明还没生成足够的素材图片
                    ShowRandomImage1();
                }
            }
            else
            {//现在不需要根据GameremainingTime的时间限制来强制结束游戏
                EndGame();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (IfPerformingAction == false)
            {
                IfPerformingAction = true;
                PerformAction();
                IfPerformingAction = false;
            }

        }

        private void PerformAction()
        {
            try
            {
                // 获取 SelectionBox 的位置和尺寸
                GeneralTransform selectionTransform = SelectionBox.TransformToVisual(ImageGrid);
                Rect selectionRect = selectionTransform.TransformBounds(new Rect(0, 0, SelectionBox.ActualWidth, SelectionBox.ActualHeight));

                // 标记是否找到重叠的图片
                bool isOverlapFound = false;
                System.Windows.Controls.Image overlappedImage = null;

                // 遍历 imageContainer2 中的每一个图片，检查是否与 SelectionBox 重叠
                foreach (UIElement element in imageContainer2.Children)
                {
                    if (element is System.Windows.Controls.Image image)
                    {
                        if (ImageDetectDict[image] == false)//没有检测过的图片才需要进入到这个逻辑
                        {
                            GeneralTransform imageTransform = image.TransformToVisual(ImageGrid);
                            Rect imageRect = imageTransform.TransformBounds(new Rect(0, 0, image.ActualWidth, image.ActualHeight));

                            // 检查是否重叠
                            if (selectionRect.IntersectsWith(imageRect))
                            {
                                isOverlapFound = true;
                                overlappedImage = image;
                                break;
                            }
                        }

                    }
                }

                // 根据重叠结果进行处理
                if (isOverlapFound && overlappedImage != null)
                {
                    // 获取重叠图片的 ImageTagInfo 对象
                    var info = overlappedImage.Tag as ImageTagInfo;
                    if (info != null)
                    {
                        //// 清除动画而不触发 Completed 事件
                        //overlappedImage.RenderTransform.BeginAnimation(TranslateTransform.XProperty, null);
                        //// 设置 AnimationStopped 标志
                        //info.AnimationStopped = true;

                        //把一部分逻辑代码移动到了Completed事件中，所以需要触发Completed事件

                        // 获取重叠图片的路径
                        string imagePath = info.ImagePath;
                        string imageName = System.IO.Path.GetFileNameWithoutExtension(imagePath); // 获取图片的名称（不含扩展名）

                        // 检查该图片路径是否在正确的题库中
                        bool isCorrect = rightImagePaths.Any(path => imagePath.EndsWith(path, StringComparison.OrdinalIgnoreCase));

                        // 根据检查结果更新 correctDecisions 和 SelectionBox 的描边颜色，并更新 Border 和 TextBlock
                        if (isCorrect)
                        {
                            correctDecisions++;
                            //ChangeSelectionBoxColor(new SolidColorBrush(Colors.Green)); // 更改为绿色，并2秒后恢复
                            SelectionBox.Stroke = new SolidColorBrush(Colors.Green);//这里变更颜色，等动画暂停完了再恢复
                            correctAnswers[hardness]++;
                            textBlock.Background = new SolidColorBrush(Colors.Green);
                            textBlock1.Text = imageName + " 正确！";
                            ImageCorrectTemp++;//本组题里答对
                            ImageDetectDict[overlappedImage] = true;//记录一下检测状态
                            PauseThenStart();
                            if (IS_AUDITORY_FEEDBACK) PlayWav(CorrectSoundPath);
                            if (IS_VISUAL_FEEDBACK) ShowFeedbackImage(CorrectImage);
                            if (recentResults.Count >= 5)
                            {
                                recentResults.Dequeue(); // 移除最早的结果
                            }
                            recentResults.Enqueue(true); // 添加当前结果
                        }
                        else
                        {
                            errorDecisions++;
                            //ChangeSelectionBoxColor(new SolidColorBrush(Colors.Red)); // 更改为红色，并2秒后恢复
                            SelectionBox.Stroke = new SolidColorBrush(Colors.Red);//这里变更颜色，等动画暂停完了再恢复
                            wrongAnswers[hardness]++;
                            textBlock.Background = new SolidColorBrush(Colors.Red);
                            textBlock1.Text = imageName + " 错误！";
                            ImageErrorTemp++;//本组题里答错
                            ImageDetectDict[overlappedImage] = true;//记录一下检测状态
                            PauseThenStart();
                            if (IS_AUDITORY_FEEDBACK) PlayWav(ErrorSoundPath);
                            if (IS_VISUAL_FEEDBACK) ShowFeedbackImage(ErrorImage);
                            if (recentResults.Count >= 5)
                            {
                                recentResults.Dequeue(); // 移除最早的结果
                            }
                            recentResults.Enqueue(false); // 添加当前结果;
                        }
                        NotifyGameStatsUpdated();
                        // 从 imageContainer2 中移除图片
                        //imageContainer2.Children.Remove(overlappedImage);
                        //想要把这个素材的动画做完，所以不在这里remove
                    }
                    else
                    {
                        // 处理 info 为 null 的情况
                        //Console.WriteLine("overlappedImage.Tag is not of type ImageTagInfo.");
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error in PerformAction: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public class ImageTagInfo
        {
            public string ImagePath { get; set; }
            public bool AnimationStopped { get; set; } = false;
        }

        private void ShowRandomImage1()
        {
            try
            {
                // 检查 totalImagePaths 是否为 null 或为空
                if (totalImagePaths == null || totalImagePaths.Count == 0)
                {
                    throw new Exception("totalImagePaths is null or empty. Please ensure it is initialized properly.");
                }

                // 从总题库中随机选择一张图片，具体做法是：提前打乱，并按顺序选取
                string imagePath = totalImagePaths[ImageNumTemp - 1];

                // 检查 imageContainer2 是否为 null
                if (imageContainer2 == null)
                {
                    throw new Exception("imageContainer2 is null. Please ensure it is initialized properly.");
                }

                System.Windows.Controls.Image newImage;
                if (train_mode == 2)
                {
                    // 提取文件名并去掉扩展名
                    string imageName = System.IO.Path.GetFileNameWithoutExtension(imagePath);

                    // 创建包含文件名的文本图像
                    RenderTargetBitmap renderBitmap = CreateTextImage(imageName);

                    newImage = new System.Windows.Controls.Image
                    {
                        Source = renderBitmap,
                        Width = 325,
                        Height = renderBitmap.Height,
                        //Margin = new Thickness(5),
                        // 统一使用 ImageTagInfo
                        Tag = new ImageTagInfo { ImagePath = imagePath }
                    };
                }
                else
                {
                    // 正常显示图片
                    BitmapImage bitmap = new BitmapImage(new Uri(imagePath, UriKind.Relative));
                    if (IfPictureInstruction && train_mode == 3)
                    {//如果需要展示图片说明，创建一个stackpanel
                        // 创建 StackPanel 包装 Image 和说明 TextBlock
                        StackPanel imageWithDescription = new StackPanel
                        {
                            Orientation = System.Windows.Controls.Orientation.Vertical, // 垂直布局
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        // 创建 TextBlock 作为说明
                        TextBlock description = new TextBlock
                        {
                            Text = Path.GetFileNameWithoutExtension(imagePath), // 获取文件名并去掉后缀
                            FontSize = InstructionFontSize, // 设置字体大小
                            Foreground = new SolidColorBrush(Colors.Black),
                            TextAlignment = TextAlignment.Center, // 文本居中
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                            Margin = new Thickness(0, 2, 0, 0) // 设置与图片的间距
                        };
                        Image ImageWithoutDescription = new System.Windows.Controls.Image
                        {
                            Source = bitmap,
                            Width = 325,
                            Height = imageContainer2.ActualHeight - InstructionFontSize - 2,//-2是还有那个margin
                            Margin = new Thickness(5),
                            // 统一使用 ImageTagInfo
                            Tag = new ImageTagInfo { ImagePath = imagePath }
                        };
                        // 将 Image 和 TextBlock 添加到 StackPanel
                        imageWithDescription.Children.Add(description);
                        imageWithDescription.Children.Add(ImageWithoutDescription);
                        // 转换为单一 Image
                        newImage = ConvertStackPanelToImage(imageWithDescription, imagePath);
                    }
                    else
                    {
                        newImage = new System.Windows.Controls.Image
                        {
                            Source = bitmap,
                            Width = 325,
                            Height = imageContainer2.ActualHeight * 0.8,
                            Margin = new Thickness(5),
                            // 统一使用 ImageTagInfo
                            Tag = new ImageTagInfo { ImagePath = imagePath }
                        };
                    }

                }

                // 确保图片显示在最上层
                System.Windows.Controls.Panel.SetZIndex(newImage, int.MaxValue); // 将 ZIndex 设置为最大值

                // 将新图片添加到 imageContainer2
                imageContainer2.Children.Add(newImage);

                //记录检测状态，防止重复检测
                ImageDetectDict[newImage] = false;//记录一下检测状态
                ImageIgnoreDict[newImage] = false;
                //确保显示的素材在传送带的中间
                double verticalCenterPosition = (imageContainer2.ActualHeight - newImage.Height) / 2;
                Canvas.SetTop(newImage, verticalCenterPosition); // 设置垂直居中位置

                // 动画移动图片
                AnimateImage(newImage);
            }

            catch (Exception ex)
            {
                //MessageBox.Show($"Error in ShowRandomImage1: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private RenderTargetBitmap CreateTextImage(string text)
        {
            // 创建文本块来显示文本
            TextBlock textBlock = new TextBlock
            {
                Text = text, // 使用文件名而不是全路径
                FontSize = 128, // 增加字体大小
                Foreground = new SolidColorBrush(Colors.Black),
                Background = new SolidColorBrush(Colors.Transparent),
                //Width = 375, // 增加文本块的宽度
                //Height = 200, // 增加文本块的高度
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };
            // 测量文本块的实际宽度和高度
            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            textBlock.Arrange(new Rect(new Size(textBlock.DesiredSize.Width, textBlock.DesiredSize.Height)));
            // 使用实际大小来创建 RenderTargetBitmap
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int)textBlock.DesiredSize.Width,
                (int)textBlock.DesiredSize.Height,
                96, 96, PixelFormats.Pbgra32);
            renderBitmap.Render(textBlock);
            ////渲染文本块为位图，增加宽度和高度
            //RenderTargetBitmap renderBitmap = new RenderTargetBitmap(375, 200, 96, 96, PixelFormats.Pbgra32);
            //textBlock.Measure(new Size(375, 200));
            //textBlock.Arrange(new Rect(new Size(375, 200)));
            //renderBitmap.Render(textBlock);

            return renderBitmap;
        }

        private void AnimateImage(System.Windows.Controls.Image img)
        {
            if (ImageNumTemp <= 0)//说明这组题该显示的素材显示完了
            {
                return;//该计算这组题是否答对
            }
            else
            {
                try
                {
                    double fromValue = -img.Width;
                    double toValue = imageContainer2.ActualWidth; //从传送带左侧运动到右侧

                    //TranslateTransform translateTransform = new TranslateTransform();
                    //img.RenderTransform = translateTransform;
                    DoubleAnimation animation = new DoubleAnimation
                    {
                        From = fromValue,
                        To = toValue,
                        Duration = new Duration(TimeSpan.FromSeconds(REAL_DISPLAY_TIME))
                    };

                    Storyboard storyboard = new Storyboard();
                    storyboard.Children.Add(animation);
                    Storyboard.SetTarget(animation, img);
                    Storyboard.SetTargetProperty(animation, new PropertyPath(Canvas.LeftProperty));

                    storyboard.Completed += (s, e) =>
                    {
                        try
                        {
                            // 尝试将 img.Tag 转换为 ImageTagInfo
                            var tagInfo = img.Tag as ImageTagInfo;
                            if (tagInfo != null)
                            {
                                if (tagInfo.AnimationStopped)
                                {
                                    // 动画已被手动停止，跳过后续处理
                                    return;
                                }

                                // 动画完成后隐藏图片
                                img.Source = null;
                                // 从 imageContainer2 中移除图片
                                imageContainer2.Children.Remove(img);
                                missDecisions++;

                                // 获取图片的路径
                                string imagePath = tagInfo.ImagePath;

                                // 检查该图片路径是否在正确的题库中
                                bool isCorrect = rightImagePaths.Any(path => imagePath.EndsWith(path, StringComparison.OrdinalIgnoreCase));

                                // 如果是未被选择的正确图片且没有检测到，增加 ignoreAnswers[hardness] 的值
                                if (isCorrect && ImageDetectDict[img] == false)
                                {
                                    ignoreAnswers[hardness]++;
                                    // System.Windows.MessageBox.Show("Answers Information");
                                    ImageIgnoreTemp++;//更新本组提的计数

                                }
                                ImageAnimationEnd--;//每结束一个动画说明一个素材走完了一生
                                if (ImageAnimationEnd <= 0)//虽然ImageNumTemp<=0了，但是在它之前的动画结束后还是会进入这个逻辑，所以需要判断
                                {//能进这里已经是这组题的最后一个动画了，该看一下这组题有没有答对了
                                    //tagInfo.AnimationStopped = true;
                                    GroupCheck();//这组题做的结果如何
                                    AdjustGroupDifficulty();
                                    BeginNextGroup();
                                }

                                //这个动画结束了，就把这个动画从队列里除去
                                if (ImageIgnoreDict[img] == true)
                                {
                                    SelectionBox.Stroke = new SolidColorBrush(Colors.Black);
                                }


                                StoryBoardsList.Remove(storyboard);
                                // 动画结束时取消事件订阅
                                CompositionTarget.Rendering -= null; // 移除事件监听
                            }
                            else
                            {
                                // 处理 tagInfo 为 null 的情况
                                // 可以记录日志或执行其他操作
                                //Console.WriteLine("img.Tag is not of type ImageTagInfo.");
                            }
                        }
                        catch (Exception ex)
                        {
                            // 可以记录异常日志或其他处理方式
                            //Console.WriteLine($"Exception occurred: {ex.Message}");
                        }
                    };

                    //用来输出遗漏的反馈，  获取 SelectionBox 的右边界
                    GeneralTransform transform = SelectionBox.TransformToAncestor(this); // this 指当前的 UserControl
                    Rect rect = transform.TransformBounds(new Rect(0, 0, SelectionBox.ActualWidth, SelectionBox.ActualHeight));
                    double selectionBoxRight = rect.Right; // 获取右边界
                    CompositionTarget.Rendering += (sender, e) =>
                    {// 注册 CompositionTarget.Rendering 事件
                        double imgLeft = Canvas.GetLeft(img);                        // 获取当前 Image 的左侧位置
                        if (imgLeft > selectionBoxRight)                        // 判断是否超出右侧边界
                        {
                            // 执行逻辑代码块
                            ShowIgnoreFeedBack(img);
                        }
                    };

                    ApplyCanvasClip(imageContainer2);//对传送带进行区域裁剪

                    //每开始一个动画就得更新一下计数
                    ImageNumTemp--;// 因为这个数是总数，所以做减法
                    storyboard.Begin();
                    StoryBoardsList.Add(storyboard);//存进来这个storyboard对象，用于后面暂停
                    //translateTransform.BeginAnimation(TranslateTransform.XProperty, animation);
                }
                catch (Exception ex)
                {
                    // 可以记录异常日志或其他处理方式
                    //Console.WriteLine($"Error in AnimateImage: {ex.Message}\n{ex.StackTrace}");
                }
            }

        }

        private void EndGame()
        {

            // 将当前游戏结果添加到对应的数组中
            //wrongAnswers[hardness] += errorDecisions;
            //correctAnswers[hardness] += correctDecisions;

            if (hardness < 8)
            {
                if (errorDecisions > 0)
                {
                    // wrongAnswers[hardness] += errorDecisions;
                }
                else
                {
                    //                  correctAnswers[hardness] += correctDecisions;
                }
            }
            else
            {
                if (errorDecisions > 1)
                {
                    //wrongAnswers[hardness] += errorDecisions;
                }
                else
                {
                    // correctAnswers[hardness] += correctDecisions;
                }
            }

            //string correctAnswersString = string.Join(", ", correctAnswers);
            //string wrongAnswersString = string.Join(", ", wrongAnswers);
            //string ignoreAnswersString = string.Join(", ", ignoreAnswers);

            //// 创建要显示的消息
            //string message = $"Correct Answers: {correctAnswersString}\n" +
            //                 $"Wrong Answers: {wrongAnswersString}\n" +
            //                 $"Ignore Answers: {ignoreAnswersString}";

            // 使用 MessageBox 显示消息
            //System.Windows.MessageBox.Show(message, "Answers Information");


            // 重置这些变量

            // 清除 imageContainer2 中的所有动画并移除图片
            foreach (UIElement element in imageContainer2.Children)
            {
                if (element is System.Windows.Controls.Image image)
                {
                    // 停止动画而不触发 Completed 事件
                    //image.RenderTransform.BeginAnimation(TranslateTransform.XProperty, null);
                    //逻辑修改，还是需要触发Completed事件的
                    // 清除图片资源
                    image.Source = null;
                }
                else if (element is TextBlock textBlock)
                {
                    // 停止与文字相关的任何动画并清除文字
                    textBlock.Text = string.Empty;
                }
            }

            imageContainer2.Children.Clear(); // 移除所有图片元素



            //// 调整难度
            ////AdjustDifficulty();
            //// 显示新的随机图片
            //totalDecisions = 0;
            //correctDecisions = 0;
            //errorDecisions = 0;
            //missDecisions = 0;
            //// 停止游戏计时器

            //if (gameTimer != null)
            //{
            //    gameTimer.Stop();
            //    gameTimer = null; // 清除计时器
            //}

            //isGameRunning = false;
            //// 重置 SelectionBox 的描边颜色
            //gameIndex++;

            //// 打开 AUFM_Report 窗口，并关闭当前窗口
            ////MainWindow reportWindow = new MainWindow(hardness, train_mode, correctAnswers, wrongAnswers, ignoreAnswers);
            //Grid1.Visibility = Visibility.Visible;
            //Grid2.Visibility = Visibility.Collapsed;

            //hard_set();

            //// 随机选择指定数量的不重复图片

            //List<string> allImages = imagePaths.SelectMany(x => x).ToList();
            //Random random = new Random();
            //selectedImagePaths = allImages.OrderBy(x => random.Next()).Take(right_picture_number).ToList();
            //// 根据 training_mode 设置图片或文本的可见性
            //if (train_mode == 1 || train_mode == 2)
            //{
            //    // 显示图片，隐藏文本
            //    SetImagesVisible();
            //}
            //else if (train_mode == 3)
            //{
            //    // 显示文本，隐藏图片
            //    SetTextsVisible();
            //}
        }

        private void AdjustDifficulty()
        {
            NotifyGameStatsUpdated();
            switch (hardness)
            {
                case 1:
                    if (errorDecisions >= 0)
                    {
                    }
                    else
                    {
                        hardness++;
                    }

                    break;
                case 2:
                    if (errorDecisions >= 0)
                    {
                        hardness--;
                    }
                    else
                    {
                        hardness++;
                    }

                    break;
                case 3:
                    if (errorDecisions >= 0)
                    {
                        hardness--;
                    }
                    else
                    {
                        hardness++;
                    }

                    break;
                case 4:
                    if (errorDecisions >= 0)
                    {
                        hardness--;
                    }
                    else
                    {
                        hardness++;
                    }

                    break;
                case 5:
                    if (errorDecisions >= 0)
                    {
                        hardness--;
                    }
                    else
                    {
                        hardness++;
                    }

                    break;
                case 6:
                    if (errorDecisions >= 0)
                    {
                        hardness--;
                    }
                    else
                    {
                        hardness++;
                    }

                    break;
                case 7:
                    if (errorDecisions >= 0)
                    {
                        hardness--;
                    }
                    else
                    {
                        hardness++;
                    }

                    break;
                case 8:
                    if (errorDecisions >= 1)
                    {
                        hardness--;
                    }
                    else
                    {
                        hardness++;
                    }

                    break;
                case 9:
                    if (errorDecisions >= 1)
                    {
                        hardness--;
                    }
                    else
                    {
                    }

                    break;
                default:
                    throw new Exception("未知的难度级别");
            }
        }

        public void hard_set()
        {//难度等级设置
            switch (hardness)
            {
                case 1:
                    //train_mode = 2;//20241208，游戏模式不随难度等级变化而变化
                    //IS_IMAGE_DETAIL = false;
                    //IS_IMAGE_HARD = false;
                    right_picture_number = 1;
                    RATE_OF_ERRORIMAGE = 0.33;
                    //DISPLAY_TIME = 8;
                    ErrorAllow = 0;
                    break;
                case 2:
                    //train_mode = 2;//20241208，游戏模式不随难度等级变化而变化
                    //IS_IMAGE_DETAIL = false;
                    //IS_IMAGE_HARD = false;
                    right_picture_number = 2;
                    RATE_OF_ERRORIMAGE = 0.30;
                    //DISPLAY_TIME = 8;
                    ErrorAllow = 0;
                    break;
                case 3:
                    //train_mode = 2;//20241208，游戏模式不随难度等级变化而变化
                    //IS_IMAGE_DETAIL = true;
                    //IS_IMAGE_HARD = false;
                    right_picture_number = 3;
                    RATE_OF_ERRORIMAGE = 0.28;
                    //DISPLAY_TIME = 7;
                    ErrorAllow = 0;
                    break;
                case 4:
                    //train_mode = 2;//20241208，游戏模式不随难度等级变化而变化
                    //IS_IMAGE_DETAIL = true;
                    //IS_IMAGE_HARD = false;
                    right_picture_number = 4;
                    RATE_OF_ERRORIMAGE = 0.26;
                    //DISPLAY_TIME = 7;
                    ErrorAllow = 0;
                    break;
                case 5:
                    //train_mode = 2;//20241208，游戏模式不随难度等级变化而变化
                    //IS_IMAGE_DETAIL = true;
                    //IS_IMAGE_HARD = true;
                    right_picture_number = 5;
                    RATE_OF_ERRORIMAGE = 0.24;
                    //DISPLAY_TIME = 7;
                    ErrorAllow = 0;
                    break;
                case 6:
                    //train_mode = 2;//20241208，游戏模式不随难度等级变化而变化
                    //IS_IMAGE_DETAIL = true;
                    //IS_IMAGE_HARD = true;
                    right_picture_number = 6;
                    RATE_OF_ERRORIMAGE = 0.22;
                    //DISPLAY_TIME = 7;
                    ErrorAllow = 0;
                    break;
                case 7:
                    //train_mode = 2;//20241208，游戏模式不随难度等级变化而变化
                    //IS_IMAGE_DETAIL = true;
                    //IS_IMAGE_HARD = true;
                    right_picture_number = 7;
                    RATE_OF_ERRORIMAGE = 0.20;
                    //DISPLAY_TIME = 6;
                    ErrorAllow = 0;
                    break;
                case 8:
                    //train_mode = 2;//20241208，游戏模式不随难度等级变化而变化
                    //IS_IMAGE_DETAIL = true;
                    //IS_IMAGE_HARD = true;
                    right_picture_number = 8;
                    RATE_OF_ERRORIMAGE = 0.15;
                    //DISPLAY_TIME = 6;
                    ErrorAllow = 1;
                    break;
                case 9:
                    //train_mode = 2;//20241208，游戏模式不随难度等级变化而变化
                    //IS_IMAGE_DETAIL = true;
                    //IS_IMAGE_HARD = true;
                    right_picture_number = 9;
                    RATE_OF_ERRORIMAGE = 0.10;
                    //DISPLAY_TIME = 6;
                    ErrorAllow = 1;
                    break;
                default:
                    throw new Exception("未知的难度级别");
            }
            ImageNumTemp = hardness * totalPictureMultiplier;//设置应该出现的素材总数，跟难度等级有明显关系所以就写在switch外面了

        }

        private void GenerateTotalImagePaths()
        {
            try
            {
                // 初始化 totalImagePaths 并添加 rightImagePaths 中的元素
                if (rightImagePaths == null || rightImagePaths.Count == 0)
                {
                    throw new Exception("rightImagePaths is null or empty.");
                }

                totalImagePaths = new List<string>(rightImagePaths);

                int totalNumberOfPictures = rightImagePaths.Count * totalPictureMultiplier;
                //int totalNumberOfPictures = rightImagePaths.Count * GroupFactor;
                Console.WriteLine($"Total number of pictures needed: {totalNumberOfPictures}");

                // 获取所有可能的图片路径
                List<string> allImages = imagePaths.SelectMany(x => x).ToList();
                if (allImages == null || allImages.Count == 0)
                {
                    throw new Exception("allImages is null or empty.");
                }
                Console.WriteLine($"Total number of all images: {allImages.Count}");

                // 从所有图片路径中删除已经选择的正确图片路径，确保不会重复选择
                var remainingImages = allImages.Except(rightImagePaths).ToList();
                Console.WriteLine($"Remaining images after excluding right images: {remainingImages.Count}");

                Random random = new Random();

                // 填充 totalImagePaths，直到达到所需的图片数量
                while (totalImagePaths.Count < totalNumberOfPictures && remainingImages.Count > 0)
                {
                    int randomIndex = random.Next(remainingImages.Count);
                    totalImagePaths.Add(remainingImages[randomIndex]);
                    remainingImages.RemoveAt(randomIndex);
                    Console.WriteLine($"Added image to totalImagePaths, remaining images: {remainingImages.Count}");
                }

                // 如果剩余的图片不足以填充到总数，发出警告
                if (totalImagePaths.Count < totalNumberOfPictures)
                {
                    //MessageBox.Show("总图片数量不足，请调整 totalPictureMultiplier 参数。");
                }
                else
                {
                    Console.WriteLine($"Successfully filled totalImagePaths with {totalImagePaths.Count} images.");
                }
                //路径集成完之后顺手打乱一下
                totalImagePaths = totalImagePaths.OrderBy(x => random.Next()).ToList();
                ImageNumTemp = totalImagePaths.Count;//图片不足则把所有素材用上
                ImageAnimationEnd = ImageNumTemp;
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error in GenerateTotalImagePaths: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void DisplayImagePaths()
        {
            StackPanel stackPanel = new StackPanel();

            foreach (var imagePath in totalImagePaths)
            {
                Image imageControl = new Image
                {
                    Source = new BitmapImage(new Uri(imagePath, UriKind.Relative)),
                    Stretch = System.Windows.Media.Stretch.Uniform,
                    Margin = new Thickness(5)
                };

                stackPanel.Children.Add(imageControl);
            }

            this.Content = stackPanel;
        }


        /*LJN
 添加进来视觉、声音反馈的资源
 */
        private SoundPlayer soundPlayer; // 用来放歌
        public string ErrorSoundPath;//错误的声音文件路径，在OnStartAsync()中配置
        public string CorrectSoundPath;//正确的声音文件路径，在OnStartAsync()中配置
        private int StopDurations = 2000; // 停止时间，ms
        private List<Storyboard> StoryBoardsList = new List<Storyboard>();//用来进进出处动画对象

        private string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory; // 当前项目的绝对路径
        private string ResourcesPath = System.IO.Path.Combine("Resources", "词语记忆力");//这里用的固定是词语记忆力的路径，后期可改

        //private int GroupFactor = 7;//一组题中的素材总数 = GroupFactor*要素词汇的数量,这个跟totalPictureMultiplier起一样的作用，两个一模一样
        private int ImageAnimationEnd = 0;//统计已经结束了的素材数，方便在动画结束时的逻辑处理
        private int ImageNumTemp = 0;//一组题中的素材总数
        private int ImageCorrectTemp = 0;//一组题中的答对的素材数
        private int ImageErrorTemp = 0;//一组题中的答错的素材数
        private int ImageIgnoreTemp = 0;//一组题中的忽略的素材数
        private int ErrorAllow = 0;//在一组素材中答错只要不超过这个数，这组题就还是能算对

        private int GroupCorrect = 0;//答对多少组题
        private int GroupError = 0;//答错多少组题

        public int LevelUp = 3;//答对3组题才能提高等级
        public int LevelDown = 3;//答错3组题才会降低等级
        private bool IfPictureInstruction = false;//是否展示图片说明
        private int InstructionFontSize = 50;//图片说明的大小

        private bool IfMemorizeLimit = true;//是否限制记忆时间
        private int MemorizeLimitTime = 60;//限制的记忆时间，单位s
        private DispatcherTimer MemorizeLimit = new DispatcherTimer(); // 记忆时间限制的计时器

        private Dictionary<int, Dictionary<string, int>> AllLevelResult = new Dictionary<int, Dictionary<string, int>>();//一个字典，存放各个难度等级下的游戏结果，用难度等级的int来索引游戏结果，游戏结果以字典形式储存，有单词总数，正确数等等。。。

        private Dictionary<int, int> FontSizeDict = new Dictionary<int, int>//不同数量的文字图片，大小应该也不同
        {//{文字数量，文字大小}
            {1,200},{2,200},{3,150},{4,350},{5,350},{6,350},{7,250},{8,250},{9,250},{10,100}
        };

        private Dictionary<Image, bool> ImageDetectDict = new Dictionary<Image, bool>();//字典用来存储对应的Image有没有检测过，防止重复检测

        private Dictionary<Image, bool> ImageIgnoreDict = new Dictionary<Image, bool>();//字典用来判断这个image到底有没有判断过ignore

        private bool IfPerformingAction = false;//加入一个标志位，因为现在键盘鼠标都能出发OKbutton，为了防止重复进入该部分逻辑，就设置个标志位多加判断

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

            // 延迟指定的时间（例如1秒）
            await Task.Delay(StopDurations);

            image.Visibility = Visibility.Collapsed;
        }

        private void SetTrainMode()//根据所选的模式来设置对应的参数
        {
            switch (train_mode)
            {
                case 1: IS_IMAGE_DETAIL = true; IS_IMAGE_HARD = true; break;
                case 2: IS_IMAGE_DETAIL = false; IS_IMAGE_HARD = false; break;
                case 3: IS_IMAGE_DETAIL = true; IS_IMAGE_HARD = true; break;
                default: IS_IMAGE_DETAIL = false; IS_IMAGE_HARD = false; break;//默认模式2
            }
        }

        private void InitTempResults()//一组题里面的计数值清空
        {
            ImageErrorTemp = 0;
            ImageCorrectTemp = 0;
            ImageIgnoreTemp = 0;
        }

        private void InitGroupResults()//在难度等级升降后Group级别的数据需要重置
        {
            InitTempResults();
            GroupCorrect = 0;
            GroupError = 0;
            UpdateGroupUI();
        }

        private void GroupCheck()//这组题做得怎么样，算对还是算错
        {
            //有忽略或者是错太多直接算整组题都错，其余答对
            if ((ImageErrorTemp - ErrorAllow) > 0 || ImageIgnoreTemp > 0) { GroupError++; }
            else { GroupCorrect++; }
            //检查完了就该清空计数了
            InitTempResults();//对这些计数值进行清空
            UpdateGroupUI();//同步结果到UI
            // 并做好记录
            AllLevelResult[hardness] = AllLevelResult.GetValueOrDefault(hardness) ?? new Dictionary<string, int>();
            AllLevelResult[hardness]["正确组题数量"] = AllLevelResult[hardness].GetValueOrDefault("正确组题数量") + GroupCorrect;
            AllLevelResult[hardness]["错误组题数量"] = AllLevelResult[hardness].GetValueOrDefault("错误组题数量") + GroupError;

        }

        private void BeginNextGroup()//开启下一组题的显示
        {
            //先清空计数,这一步在GroupCheck中已经完成
            //再把container里面的children的Image给清楚干净,一共两个container
            imageContainer.Children.Clear();
            imageContainer2.Children.Clear();

            //执行最开始记忆阶段时的逻辑，选素材并显示
            Grid1.Visibility = Visibility.Visible;
            Grid2.Visibility = Visibility.Collapsed;

            List<string> allImages = imagePaths.SelectMany(x => x).ToList();
            Random random = new Random();
            selectedImagePaths = allImages.OrderBy(x => random.Next()).Take(right_picture_number).ToList();
            //记忆时间限制的计数器重置
            if (IfMemorizeLimit) { StartMemorizeLimit(); }
            //Image检测字典初始化
            ImageDetectDict.Clear();

            // 根据 training_mode 设置图片或文本的可见性
            if (train_mode == 1 || train_mode == 2)
            {
                // 显示图片，隐藏文本
                SetImagesVisible();
            }
            else if (train_mode == 3)
            {
                // 显示文本，隐藏图片
                SetTextsVisible();
            }

            //设置相关标志位
            isGameRunning = false;//这个相当于是用来使能那个传送带右下角那个使能按钮的

            gameIndex++;
            // 停止游戏计时器，gameTimer是用来生成新的传送带上的素材的，所以需要停止
            if (gameTimer != null)
            {
                gameTimer.Stop();
                gameTimer = null; // 清除计时器
            }
        }

        private void UpdateGroupUI()//每次Group的结果有变动就同步显示到UI上
        {
            //每次变更就同步显示到UI上去
            RightStatisticsAction?.Invoke(GroupCorrect, LevelUp);
            WrongStatisticsAction?.Invoke(GroupError, LevelDown);
        }

        private void AdjustGroupDifficulty()//根据目前做的组数看看是否需要调整难度
        {
            if (GroupCorrect >= LevelUp)//需要升等级
            {
                if (hardness < MAX_HARDNESS)
                {//说明确实需要升等级
                    hardness++; //难度等级调整后需要清空计数
                    max_hardness = Math.Max(max_hardness, hardness);
                }
                else
                {//在最高等级了还对这么多，没办法再升了爆满了！清一下结果重复循环算了

                }
                InitGroupResults();
            }
            else if (GroupError >= LevelDown)//需要降等级
            {
                if (hardness > MIN_HARDNESS)
                {//确实需要降等级
                    hardness--; //难度等级调整后需要清空计数
                }
                else
                {//等级最低了还错怎么多，真没办法，只能再来一次

                }
                InitGroupResults();
            }
            else//不用调整，继续
            {

            }
            hard_set();//利用调整完的难度等级去修改参数
        }

        private void ApplyCanvasClip(Canvas containerCanvas)// 应用剪裁区域到 Canvas
        {//通过裁剪，实现textblock在这个canvas中部分可见部分不可见
            // 创建一个与 Canvas 相同大小的矩形
            RectangleGeometry clipGeometry = new RectangleGeometry
            {
                Rect = new Rect(0, 0, containerCanvas.ActualWidth, containerCanvas.ActualHeight)
            };

            // 将该矩形作为 Canvas 的剪裁区域
            containerCanvas.Clip = clipGeometry;
        }

        private void MemorizeLimit_Tick(object sender, EventArgs e)//记忆时间限制的计时器
        {
            MemorizeLimitTime--;
            TimeStatisticsAction?.Invoke(max_time, MemorizeLimitTime);//显示到UI
            if (MemorizeLimitTime <= 0)
            {
                //如果触发了这个函数，说明记忆限制到了，模拟手动按下Continue按钮
                ContinueButton_Click(this, new RoutedEventArgs());//手动点下OK按钮
                                                                  //手动结束记忆阶段
            }

        }

        private void StartMemorizeLimit()//启动记忆限时
        {
            MemorizeLimitTime = 60;
            MemorizeLimit = new DispatcherTimer();//设置时间限时
            MemorizeLimit.Interval = TimeSpan.FromSeconds(1);  // 设置为1秒触发一次
            MemorizeLimit.Tick += MemorizeLimit_Tick;  // 绑定 Tick 事件
            MemorizeLimit.Start();

        }

        private void EndMemorizeLimit()//消除记忆限时
        {
            if (MemorizeLimit != null)
            {
                MemorizeLimit.Stop();            // 停止计时器
                MemorizeLimit.Tick -= MemorizeLimit_Tick;            // 解绑事件处理程序
                MemorizeLimit = null;            // 清除计时器对象引用
            }
        }

        private async void PauseThenStart()//每次答对答错都指定暂停所有动画一段时间
        {
            foreach (var storyboard in StoryBoardsList) { storyboard.Pause(); }
            // 如果计时器正在运行，先停止它
            StopTime = DateTime.Now;//记录暂停时间
            if (gameTimer.IsEnabled) { gameTimer.Stop(); }//这个是产生素材图片的计时器，也需要停止
            double PauseDuration = CalculateStopSpan();
            if (StopDurations - PauseDuration >= 0)
            {
                await Task.Delay((int)PauseDuration);//先把计时器的暂停给延迟完了
                gameTimer.Start(); // 在中间点恢复计时器
                await Task.Delay(StopDurations - (int)PauseDuration); // 再延迟剩余的部分
            }
            else
            {//事实上是不可能StopDurations- PauseDuration<0的，如果是，则有问题
                await Task.Delay((int)StopDurations);//先把计时器的暂停给延迟完了
                gameTimer.Start(); // 在中间点恢复计时器
                await Task.Delay((int)PauseDuration - StopDurations); // 再延迟剩余的部分
            }
            // 恢复所有 Storyboard
            foreach (var storyboard in StoryBoardsList) { storyboard.Resume(); }
            SelectionBox.Stroke = new SolidColorBrush(Colors.Black);//及时把颜色显示回来
        }

        private double CalculateStopSpan()//计算需要将gameTimer停止的时间
        {
            /*
             由于gameTimer的线程无法暂停，但StopDurations期间并不希望GameTimer_Tick触发
            所以如果在StopDurations期间触发了GameTimer_Tick，需要计算gameTimer的stop的时间
            以保证StopDurations结束后还能正常按周期触发GameTimer_Tick，从而保证素材图片等间隔出现
             */
            double delay = (StopTime - LastTickTime).TotalMilliseconds;
            while (delay > StopDurations)
            {
                delay += StopDurations;
            }

            return StopDurations - delay;
        }

        private void SetImagesPosition()
        {
            // 确保 Grid1 的布局已完成，以便获取其高度
            imageContainer.RowDefinitions.Clear();
            imageContainer.Children.Clear();

            // 指定高度
            double gridHeight = 920 * 0.92 * 0.8;

            int[] columnsPerRow = { };
            int imageCount = 0;

            switch (selectedImagePaths.Count)
            {
                case 1:
                    columnsPerRow = new int[] { 1 }; break;
                case 2:
                    columnsPerRow = new int[] { 2 }; break;
                case 3:
                    columnsPerRow = new int[] { 2, 1 }; break;
                case 4:
                    columnsPerRow = new int[] { 2, 2 }; break;
                case 5:
                    columnsPerRow = new int[] { 3, 2 }; break;
                case 6:
                    columnsPerRow = new int[] { 3, 3 }; break;
                case 7:
                    columnsPerRow = new int[] { 2, 2, 3 }; break;
                case 8:
                    columnsPerRow = new int[] { 3, 3, 2 }; break;
                case 9:
                    columnsPerRow = new int[] { 3, 3, 3 }; break;
                default:
                    columnsPerRow = new int[] { 1 }; break;
            }

            // 动态设置行
            for (int rowIndex = 0; rowIndex < columnsPerRow.Length; rowIndex++)
            {
                int rowCount = columnsPerRow.Length;

                // 计算每一行的高度
                double rowHeight = gridHeight / rowCount;

                // 添加行定义
                imageContainer.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(rowHeight)
                });

                // 创建子 Grid
                Grid subGrid = new Grid
                {
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                // 定义列
                int columnCount = columnsPerRow[rowIndex];
                for (int colIndex = 0; colIndex < columnCount; colIndex++)
                {
                    subGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }

                // 添加图片
                for (int colIndex = 0; colIndex < columnCount && imageCount < selectedImagePaths.Count; colIndex++)
                {
                    Image imageControl = new Image
                    {
                        Source = new BitmapImage(new Uri(selectedImagePaths[imageCount], UriKind.Relative)),
                        Stretch = Stretch.Uniform,
                        Margin = new Thickness(5)
                    };

                    Grid.SetColumn(imageControl, colIndex);
                    subGrid.Children.Add(imageControl);
                    imageCount++;
                }

                // 添加子 Grid 到主 Grid 的行
                Grid.SetRow(subGrid, rowIndex);
                imageContainer.Children.Add(subGrid);
            }
        }

        private void SetTextsPosition()
        {
            // 确保 Grid1 的布局已完成，以便获取其高度
            imageContainer.RowDefinitions.Clear();
            imageContainer.Children.Clear();

            // 指定高度
            double gridHeight = 920 * 0.92 * 0.8;

            int[] columnsPerRow = { };
            int imageCount = 0;

            switch (selectedImagePaths.Count)
            {
                case 1:
                    columnsPerRow = new int[] { 1 }; break;
                case 2:
                    columnsPerRow = new int[] { 2 }; break;
                case 3:
                    columnsPerRow = new int[] { 2, 1 }; break;
                case 4:
                    columnsPerRow = new int[] { 2, 2 }; break;
                case 5:
                    columnsPerRow = new int[] { 3, 2 }; break;
                case 6:
                    columnsPerRow = new int[] { 3, 3 }; break;
                case 7:
                    columnsPerRow = new int[] { 2, 2, 3 }; break;
                case 8:
                    columnsPerRow = new int[] { 3, 3, 2 }; break;
                case 9:
                    columnsPerRow = new int[] { 3, 3, 3 }; break;
                default:
                    columnsPerRow = new int[] { 1 }; break;
            }

            // 动态设置行
            for (int rowIndex = 0; rowIndex < columnsPerRow.Length; rowIndex++)
            {
                int rowCount = columnsPerRow.Length;

                // 计算每一行的高度
                double rowHeight = gridHeight / rowCount;

                // 添加行定义
                imageContainer.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(rowHeight)
                });

                // 创建子 Grid
                Grid subGrid = new Grid
                {
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                // 定义列
                int columnCount = columnsPerRow[rowIndex];
                for (int colIndex = 0; colIndex < columnCount; colIndex++)
                {
                    subGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }

                // 添加图片
                for (int colIndex = 0; colIndex < columnCount && imageCount < selectedImagePaths.Count; colIndex++)
                {
                    TextBlock textBlock = new TextBlock
                    {
                        Text = Path.GetFileNameWithoutExtension(selectedImagePaths[imageCount]),
                        FontSize = Math.Min(120, rowHeight * 0.8),
                        Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black),
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(20, 0, 20, 0) // 设置左右外边距，保证间距美观
                    };

                    Grid.SetColumn(textBlock, colIndex);
                    subGrid.Children.Add(textBlock);
                    imageCount++;
                }

                // 添加子 Grid 到主 Grid 的行
                Grid.SetRow(subGrid, rowIndex);
                imageContainer.Children.Add(subGrid);
            }
        }

        private Image ConvertStackPanelToImage(StackPanel stackPanel, string imagePath)
        {
            // 渲染 StackPanel 到位图
            Size size = new Size(325, imageContainer2.ActualHeight);
            stackPanel.Measure(size);
            stackPanel.Arrange(new Rect(new Point(0, 0), size));

            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int)size.Width,
                (int)size.Height,
                96, // DPI X
                96, // DPI Y
                PixelFormats.Pbgra32); // 使用 Pbgra32 格式
            renderBitmap.Render(stackPanel);

            // 创建新的 Image 对象
            return new Image
            {
                Source = renderBitmap,
                Width = renderBitmap.Width,
                Height = renderBitmap.Height,
                Tag = new ImageTagInfo { ImagePath = imagePath }
            };
        }

        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {// 按键检测
            // 检查按下的键是否是你指定的键
            if (e.Key == System.Windows.Input.Key.Enter) // 假设你指定的键是回车键
            {
                if (IfPerformingAction == false)
                {
                    IfPerformingAction = true;
                    PerformAction();//进入判断逻辑
                    IfPerformingAction = false;
                }
            }
        }

        private void ShowIgnoreFeedBack(Image image)//判断这个image对象是不是遗漏的，如果是则需要将BOx框框变色
        {
            if (ImageIgnoreDict.ContainsKey(image) && ImageIgnoreDict[image] == false)
            {
                if (ImageDetectDict.ContainsKey(image) && ImageDetectDict[image] == false)//这个框框没检测过
                {
                    var tagInfo = image.Tag as ImageTagInfo;
                    string imagePath = tagInfo.ImagePath;
                    bool isCorrect = rightImagePaths.Any(path => imagePath.EndsWith(path, StringComparison.OrdinalIgnoreCase));
                    if (isCorrect)//说明真的是遗漏的
                    {
                        SelectionBox.Stroke = new SolidColorBrush(Colors.DarkGray);//框框变色以提示
                        ImageIgnoreDict[image] = true;
                    }
                }
            }
        }
    }

    public partial class 图形记忆力 : BaseUserControl
    {
        private bool is_pause = false;

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


            train_mode = 1;
            hardness = 1;
            sharedTimer = new DispatcherTimer();
            sharedTimer.Interval = TimeSpan.FromSeconds(1);
            sharedTimer.Tick += OnTick;

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

                            case 156: //游戏等级
                                hardness = par.Value.HasValue ? (int)par.Value.Value : 1;
                                break;
                            case 21: // 治疗时间 
                                max_time = par.Value.HasValue ? (int)par.Value.Value : 30;
                                Debug.WriteLine($"训练时间={max_time}");
                                break;
                            case 22: // 要素词汇
                                total_picture_number_para = par.Value.HasValue ? (int)par.Value.Value : 7;
                                Debug.WriteLine($"要素词汇={total_picture_number_para}");
                                break;
                            case 26: // 等级提高
                                LEVEL_UP_THRESHOLD = par.Value.HasValue ? (int)par.Value.Value : 85;
                                Debug.WriteLine($"DECREASE ={LEVEL_UP_THRESHOLD}");
                                break;
                            case 27: // 等级降低
                                LEVEL_DOWN_THRESHOLD = par.Value.HasValue ? (int)par.Value.Value : 50;
                                Debug.WriteLine($"DECREASE ={LEVEL_DOWN_THRESHOLD}");
                                break;
                            case 23://视觉反馈
                                IS_VISUAL_FEEDBACK = par.Value == 1;
                                Debug.WriteLine($"IS_VISUAL_FEEDBACK ={IS_VISUAL_FEEDBACK}");
                                break;
                            case 25: //听觉反馈
                                IS_AUDITORY_FEEDBACK = par.Value == 1;
                                Debug.WriteLine($"IS_AUDITORY_FEEDBACK ={IS_AUDITORY_FEEDBACK}");
                                break;
                            case 279:// 游戏模式
                                train_mode = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"train_mode ={train_mode}");
                                break;
                            case 280://是否限制记忆时间
                                IfMemorizeLimit = par.Value == 1;
                                Debug.WriteLine($"IfMemorizeLimt ={IfMemorizeLimit}");
                                break;
                            case 281://是否展示图片说明
                                IfPictureInstruction = par.Value == 1;
                                Debug.WriteLine($"IfPictureInstruction ={IfPictureInstruction}");
                                break;
                            case 272://流水线速度
                                SPEED_FACTOR = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"SPEED_FACTOR ={SPEED_FACTOR}");
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

            max_time = max_time * 60;
            hard_set();
            // 调用委托
            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
            RightStatisticsAction?.Invoke(0, LevelUp);
            WrongStatisticsAction?.Invoke(0, LevelDown);
        }

        protected override async Task OnStartAsync()
        {

            if (!isGameRunning)
            {
                if (!is_pause)
                {
                    // 随机选择指定数量的不重复图片
                    hard_set();
                    List<string> allImages = imagePaths.SelectMany(x => x).ToList();
                    Random random = new Random();
                    selectedImagePaths = allImages.OrderBy(x => random.Next()).Take(right_picture_number).ToList();
                    // 根据 training_mode 设置图片或文本的可见性



                    if (train_mode == 1 || train_mode == 2)
                    {
                        // 显示图片，隐藏文本
                        SetImagesVisible();
                    }
                    else if (train_mode == 3)
                    {
                        // 显示文本，隐藏图片
                        SetTextsVisible();
                    }
                    StartMemorizeLimit();//因为一开始来的是记忆画面
                }
                sharedTimer.Start();
                is_pause = false; sharedTimer.Start();

            }
            else
            {
                gameTimer.Start();
                sharedTimer.Start();
            }

            // 调用委托
            VoiceTipAction?.Invoke("当您看到与您记住的图像相匹配的文字经过方框时，按下键盘上的Enter键。");
            SynopsisAction?.Invoke("首先您会在界面上看到一个图像，请记住它，并用鼠标点击OK键；随后您会看到一排图片对应的文字描述从左往右穿过，当您看到与您记住的图像相匹配的文字经过方框时，按下键盘上的Enter键来选择。");
            RuleAction?.Invoke("首先您会在界面上看到一个图像，请记住它，并用鼠标点击OK键；随后您会看到一排图片对应的文字描述从左往右穿过，当您看到与您记住的图像相匹配的文字经过方框时，按下键盘上的Enter键来选择。");//增加代码，调用函数，显示数字人下的文字

        }

        protected override async Task OnStopAsync()
        {
            sharedTimer.Stop();
            if (gameTimer != null)
            {
                gameTimer.Stop();
            }
        }

        protected override async Task OnPauseAsync()
        {
            if (isGameRunning)
            {
                gameTimer.Stop();
            }
            sharedTimer.Stop();

            is_pause = true;
        }

        protected override async Task OnNextAsync()
        {
            // 调整难度

            // 随机选择指定数量的不重复图片
            //if (!isGameRunning)
            if (true)
            {
                //List<string> allImages = imagePaths.SelectMany(x => x).ToList();
                //Random random = new Random();
                //selectedImagePaths = allImages.OrderBy(x => random.Next()).Take(right_picture_number).ToList();

                //// 根据 training_mode 设置图片或文本的可见性
                //if (train_mode == 1 || train_mode == 2)
                //{
                //    // 显示图片，隐藏文本
                //    SetImagesVisible();
                //}
                //else if (train_mode == 3)
                //{
                //    // 显示文本，隐藏图片
                //    SetTextsVisible();
                //}
                BeginNextGroup();

                // 调用委托
                VoiceTipAction?.Invoke("当您看到与您记住的图像相匹配的文字经过方框时，按下键盘上的Enter键。");
                SynopsisAction?.Invoke("首先您会在界面上看到一个图像，请记住它，并用鼠标点击OK键；随后您会看到一排图片对应的文字描述从左往右穿过，当您看到与您记住的图像相匹配的文字经过方框时，按下键盘上的Enter键来选择。");
                RuleAction?.Invoke("首先您会在界面上看到一个图像，请记住它，并用鼠标点击OK键；随后您会看到一排图片对应的文字描述从左往右穿过，当您看到与您记住的图像相匹配的文字经过方框时，按下键盘上的Enter键来选择。");
            }
            else
            {
                EndGame();
            }
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new 图形记忆力讲解();
        }

        private int GetCorrectNum(int difficultylevel)
        {
            return correctAnswers[difficultylevel];
        }
        private int GetWrongNum(int difficultylevel)
        {
            return wrongAnswers[difficultylevel];
        }
        private int GetIgnoreNum(int difficultylevel)
        {
            return ignoreAnswers[difficultylevel];
        }
        private double CalculateAccuracy(int correctCount1, int wrongCount1, int ignoreCount1)
        {
            int totalAnswers = correctCount1 + wrongCount1 + ignoreCount1;
            return totalAnswers > 0 ? Math.Round((double)correctCount1 / totalAnswers, 2) : 0;
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
                        int correctCount2 = 0;
                        int wrongCount2 = 0;
                        int ignoreCount2 = 0;
                        int totalCount = 0;
                        double accuracy = 0;

                        //int correctCount2 = GetCorrectNum(lv);
                        //int wrongCount2 = GetWrongNum(lv);
                        //int ignoreCount2 = GetIgnoreNum(lv);
                        //int totalCount = correctCount2 + wrongCount2 + ignoreCount2;
                        //double accuracy = CalculateAccuracy(correctCount2, wrongCount2, ignoreCount2);


                        for (int lv = 1; lv <= max_hardness; lv++)
                        {
                            correctCount2 += GetCorrectNum(lv);
                            wrongCount2 += GetWrongNum(lv);
                            ignoreCount2 += GetIgnoreNum(lv);
                        }
                        totalCount = correctCount2 + wrongCount2 + ignoreCount2;
                        accuracy = CalculateAccuracy(correctCount2, wrongCount2, ignoreCount2);

                        // 创建 Result 记录
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "图形记忆力",
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
                                    Order = 0,
                                    ValueName = "等级",
                                    Value = max_hardness,
                                    Maxvalue = 9,
                                    Minvalue = 1,
                                    ModuleId = BaseParameter.ModuleId //  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "任务总数",
                                    Value = totalCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "正确",
                                    Value = correctCount2,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "正确率",
                                    Value = accuracy * 100, // 以百分比形式存储
                                    Maxvalue = 100,
                                    Minvalue = 0,
                                    Charttype = "柱状图" ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                 new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "错选",
                                    Value = wrongCount2, // 以百分比形式存储
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "漏选",
                                    Maxvalue = ignoreCount2,
                                    Minvalue = 0,
                                    Value = ignoreCount2, // 以百分比形式存储
                                    Charttype = "柱状图" ,
                                    ModuleId = BaseParameter.ModuleId
                                }
                            };

                        // 插入 ResultDetail 数据
                        db.ResultDetails.AddRange(resultDetails);
                        await db.SaveChangesAsync();

                        // 输出每个 ResultDetail 对象的数据
                        Debug.WriteLine($"难度级别 {hardness}:");
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

