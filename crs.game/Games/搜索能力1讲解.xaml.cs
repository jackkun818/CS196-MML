using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.Numerics;
using System.Media;
using log4net.Core;
namespace crs.game.Games
{
    /// <summary>
    /// 搜索能力2讲解.xaml 的交互逻辑
    /// </summary>
    public partial class 搜索能力1讲解 : BaseUserControl
    {
        private readonly string[][] imagePaths = new string[][]
        {
            new string[]
            {
                "EXO/1/1.png", "EXO/1/2.png", "EXO/1/3.png", "EXO/1/4.png",
                "EXO/1/5.png", "EXO/1/6.png", "EXO/1/7.png", "EXO/1/8.png",
                "EXO/1/9.png", "EXO/1/10.png", "EXO/1/11.png", "EXO/1/12.png",
                "EXO/1/13.png", "EXO/1/14.png", "EXO/1/15.png", "EXO/1/16.png",
                "EXO/1/17.png", "EXO/1/18.png", "EXO/1/19.png", "EXO/1/20.png",
                "EXO/1/21.png", "EXO/1/22.png", "EXO/1/23.png", "EXO/1/24.png",
                "EXO/1/25.png", "EXO/1/26.png", "EXO/1/27.png", "EXO/1/28.png",
                "EXO/1/29.png", "EXO/1/30.png", "EXO/1/31.png", "EXO/1/32.png"
            },
            new string[]
            {
                "EXO/2/Rocket1.png", "EXO/2/Rocket2.png", "EXO/2/Star.png", "EXO/2/Planet1.png",
                "EXO/2/Planet3.png", "EXO/2/Sun.png", "EXO/2/Planet2.png", "EXO/2/Meteor.png",
                "EXO/2/Planet4.png", "EXO/2/Spaceship.png", "EXO/2/Background.png"
            }

        };

        private int fix_bug = 0;
        private List<int> trialModes = new List<int> { 1}; // 需要试玩的模式列表
        private int currentTrialModeIndex = 0; // 当前试玩的模式索引
        private bool isTrialPhase = true; // 是否处于试玩阶段
        private int max_time = 1; // 窗口总的持续时间，单位分钟
        private int train_mode = 1; // 游戏模式，1，2，3，4
        private bool is_gaming = false;
        private int success_time = 0;
        private int fail_time = 0;
        private int level = 1; // 当前游戏难度等级
        private List<int> missingNumbers;
        private List<int> userInputNumbers;
        private string userInput; // 存储用户输入的数字

        private int right_picture_number = 4; // 显示的正确图片数量
        private int chose_picture_number = 6; // 显示的可选择图片数量

        private int max_right_display = 2; // 最多显示的正确图片数量
        private int mini_right_display = 1; // 最少显示的正确图片数量
        private int mislead_picture_display_number = 4; // 干扰图片中总的显示数量

        private List<System.Windows.Controls.Image> correctImages; // 正确图片的列表
        private List<System.Windows.Controls.Image> selectedImages; // 用户选择的图片

        private Queue<bool> recentResults = new Queue<bool>(5); // 记录最近5次游戏结果的队列
        private int level_updata_threshold = 3; // 难度更新的正确或错误阈值
        private int maxnumber = 5; // 显示的最大数字
        private int minnumber = 1;//显示的最小数字
        private int miss_number = 2; // 消失的数字数量
        private int mode1_display_size = 1; // 显示框的大小：1=小，2=中，3=大，4=全屏

        private const int MaxGames = 10;
        private int[] correctAnswers = new int[MaxGames];
        private int[] wrongAnswers = new int[MaxGames];
        private int[] ignoreAnswers = new int[MaxGames];

        private DispatcherTimer gameTimer; // 全局计时器
        private TimeSpan timeRemaining; // 剩余时间

        private Canvas selectionCanvas; // 在类中声明 selectionCanvas 作为全局变量
        private bool isCorrectAnswer = false; // 添加字段，记录用户是否答对  
        public Action GameBeginAction { get; set; }
        private WrapPanel choicePanel; // 用于保存选择图片的面板
        public Func<string, Task> VoicePlayFunc { get; set; }
        private DispatcherTimer IntervalTimer;//题与题之间的间隔计时器
        private int CorrectInterval = 2;//答对3s间隔
        private int ErrorInterval = 2;//答错5s间隔

        private Dictionary<int, TextBlock> NumTextDict = new Dictionary<int, TextBlock>();//用来存储数字和对应的Textblock对象，后期好索引
        private List<int> AllNumbers = new List<int>();//用来存储所有数字的列表
        private int ErrorCount = 0;
        private int ErrorLimit = 2;
        public 搜索能力1讲解()
        {
            InitializeComponent();
            InitializeGame();
            AdjustDifficulty(level);
            AdjustDifficultyMode2(level);
            AdjustDifficultyMode3(level);
            AdjustDifficultyMode4(level);
            // 初始化计时器
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1); // 每秒触发一次
            gameTimer.Tick += GameTimer_Tick;
            timeRemaining = TimeSpan.FromMinutes(max_time); // 设定整个窗口存在的时间
            gameTimer.Start(); // 开始计时

            IntervalTimer = new DispatcherTimer();
            IntervalTimer.Tick += IntervalTimer_Tick;

            this.Loaded += 搜索能力1讲解_Loaded;

            this.Cursor = Cursors.None; // 隐藏默认光标
            CustomCursor.Visibility = Visibility.Visible; // 显示自定义光标
            MouseMove += Window_MouseMove; // 订阅 MouseMove 事件
            CustomCursor.Width = 65; // 调整为你想要的宽度
            CustomCursor.Height = 65; // 调整为你想要的高度

        }

        private void 搜索能力1讲解_Loaded(object sender, RoutedEventArgs e)
        {
            // 页面加载时确保按键和焦点行为
            Button_2_Click(null, null);
            this.Focus();  // 确保焦点在窗口上
            ErrorCount = ErrorLimit;
        }

        private void IntervalTimer_Tick(object sender, EventArgs e)//答题间隔计时器
        {// 这个触发了才能进入到下一个题目
            OverLayGrid.IsEnabled = true;
            IntervalTimer.Stop();//停止计时器
            InitializeGame();
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            // 每秒减少剩余时间
            if (timeRemaining > TimeSpan.Zero)
            {
                timeRemaining = timeRemaining.Add(TimeSpan.FromSeconds(-1));
                UpdateTimerDisplay(); // 更新计时器显示
            }
            else
            {
                gameTimer.Stop(); // 停止计时器
                CloseApplication(); // 关闭整个应用窗口
            }
        }

        private void UpdateTimerDisplay()
        {
            // 这里可以选择是否显示时间，但根据你的要求，单次游戏没有时间显示逻辑。
            // 如果你不希望显示时间，可以省略此方法的实现
        }

        private void CloseApplication()
        {
            // 关闭整个应用窗口;

        }

        private void InitializeGame()
        {
            ResetGameState(); // 在开始新游戏前重置状态
            switch (train_mode)
            {
                case 1:
                    //modeTextBlock.Text = "找出数字范围内的缺失数字，并将它们从小到大逐个输入";
                    break;
                case 2:
                    //modeTextBlock.Text = "识别出叠加在一起的不同形状，并将它们从屏幕下部选择出来";
                    break;
                case 3:
                    //modeTextBlock.Text = "需要寻找的目标对象出现屏幕的下部，从上部的图片中将这些对象寻找出来";
                    break;
                case 4:
                    // modeTextBlock.Text = "数出并输入每个正确对象在图片中出现的次数";
                    break;
                default:
                    //modeTextBlock.Text = "未知模式";
                    break;
            }

            // 隐藏组件，确保它们不会在非模式1下显示
            confirm.Visibility = Visibility.Collapsed;
            textBlock.Visibility = Visibility.Collapsed;
            myCanvas.Visibility = Visibility.Collapsed;
            confirm.Visibility = Visibility.Collapsed;

            //AdjustDifficulty(level); // 根据当前level调整游戏难度

            // 初始化游戏模式
            if (train_mode == 1)
            {
                SetupGameMode1();
            }
            else if (train_mode == 2)
            {
                SetupGameMode2();
            }
            else if (train_mode == 3)
            {
                SetupGameMode3();
            }
            else if (train_mode == 4)
            {
                SetupGameMode4();
            }

            end.Visibility = Visibility.Collapsed;
        }
        private void SetupGameMode2()
        {
            confirm.Visibility = Visibility.Visible;
            //Panel.SetZIndex(confirm, 999); // 999 是一个比较大的值，确保在最上层
            // 清除之前的内容
            MainGrid.Children.Clear();

            // 为 MainGrid 添加行定义
            MainGrid.RowDefinitions.Clear();
            MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(3, GridUnitType.Star) }); // 上方叠加图片区域
            MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) }); // 下方选择图片区域

            // 显示叠加的正确图片
            correctImages = new List<System.Windows.Controls.Image>();
            selectedImages = new List<System.Windows.Controls.Image>();
            DisplayOverlayImages();

            // 显示可供选择的图片
            DisplayChoiceImages();
        }

        private void DisplayOverlayImages()
        {
            Canvas overlayCanvas = new Canvas
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 300,  // 宽度可以根据需求调整
                Height = 300  // 高度可以根据需求调整
            };

            Random rand = new Random();
            List<int> indices = Enumerable.Range(0, imagePaths[0].Length).OrderBy(x => rand.Next()).Take(right_picture_number).ToList();

            foreach (int index in indices)
            {
                string imagePath = imagePaths[0][index];
                System.Windows.Controls.Image img = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri(imagePath, UriKind.Relative)),
                    Width = 100,
                    Height = 100,
                    Opacity = 0.5, // 设置透明度
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    Tag = imagePath  // 为每个图片的 Tag 属性赋值
                };

                // 随机调整图片的位置和旋转角度，使其产生重叠效果
                double left = rand.Next(50);
                double top = rand.Next(50);
                double angle = rand.Next(-15, 15);

                Canvas.SetLeft(img, left);
                Canvas.SetTop(img, top);

                RotateTransform rotateTransform = new RotateTransform(angle);
                img.RenderTransform = rotateTransform;

                correctImages.Add(img);  // 将正确图片添加到列表中
                overlayCanvas.Children.Add(img);
            }

            Grid.SetRow(overlayCanvas, 0);
            MainGrid.Children.Add(overlayCanvas);
        }
        private void DisplayChoiceImages()
        {
            choicePanel = new WrapPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20)
            };


            Random rand = new Random();
            List<int> indices = new List<int>();

            // 首先确保所有重叠显示的正确图片被添加到选择列表中
            foreach (var correctImg in correctImages)
            {
                string correctImagePath = (string)correctImg.Tag;
                int correctIndex = Array.IndexOf(imagePaths[0], correctImagePath);
                indices.Add(correctIndex);
            }

            // 填充剩余的选择图片，确保总数达到 chose_picture_number
            while (indices.Count < chose_picture_number)
            {
                int index = rand.Next(imagePaths[0].Length);
                if (!indices.Contains(index))
                {
                    indices.Add(index);
                }
            }

            // 随机化选择图片的顺序
            indices = indices.OrderBy(x => rand.Next()).ToList();

            foreach (int index in indices)
            {
                string imagePath = imagePaths[0][index];
                System.Windows.Controls.Image img = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri(imagePath, UriKind.Relative)),
                    Width = 80,
                    Height = 80,
                    Margin = new Thickness(10),
                    Tag = imagePath  // 为每个图片的 Tag 属性赋值
                };

                Border border = new Border
                {
                    BorderThickness = new Thickness(2),
                    BorderBrush = Brushes.Transparent, // 初始时无边框
                    Child = img
                };

                border.MouseLeftButtonDown += (sender, e) =>
                {
                    if (selectedImages.Contains(img))
                    {
                        // 如果已选中，取消选择
                        border.BorderBrush = Brushes.Transparent;
                        selectedImages.Remove(img);
                    }
                    else
                    {
                        if (correctImages.Any(c => (string)c.Tag == (string)img.Tag))
                        {
                            border.BorderBrush = Brushes.Green;
                        }
                        else
                        {
                            border.BorderBrush = Brushes.Red;
                        }
                        selectedImages.Add(img);
                    }
                };

                choicePanel.Children.Add(border);
            }

            Grid.SetRow(choicePanel, 1);
            MainGrid.Children.Add(choicePanel);
        }

        private void SetupGameMode4()
        {
            confirm.Visibility = Visibility.Visible;
            textBlock.Visibility = Visibility.Visible;
            myCanvas.Visibility = Visibility.Visible;

            // 清除MainGrid中的所有内容
            MainGrid.Children.Clear();

            // 创建一个Grid来放置背景图片和选择图片
            Grid gameGrid = new Grid();

            System.Windows.Controls.Image backgroundImage = new System.Windows.Controls.Image
            {
                Source = new BitmapImage(new Uri("EXO/2/Background.png", UriKind.Relative)),
                Stretch = Stretch.Uniform,
                Width = 1000,  // 调整背景图片的宽度
                Height = 660,  // 调整背景图片的高度
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, -100, 0, 0)  // 通过Margin属性上移100像素
            };


            // 将背景图片添加到Grid的Background层
            gameGrid.Children.Add(backgroundImage);

            // 初始化正确图片列表
            correctImages = new List<System.Windows.Controls.Image>();

            // 随机选择正确的图片，并将其添加到correctImages列表中
            Random rand = new Random();
            List<int> correctIndices = new List<int>();

            while (correctIndices.Count < right_picture_number)
            {
                int index = rand.Next(imagePaths[1].Length);

                if (imagePaths[1][index] != "EXO/2/Background.png" && !correctIndices.Contains(index))
                {
                    correctIndices.Add(index);

                    // 将正确图片添加到correctImages列表中
                    System.Windows.Controls.Image img = new System.Windows.Controls.Image
                    {
                        Source = new BitmapImage(new Uri(imagePaths[1][index], UriKind.Relative)),
                        Width = 80,
                        Height = 80,
                        Margin = new Thickness(10)
                    };

                    correctImages.Add(img);
                }
            }

            // 创建一个用于显示选择图片的Canvas
            selectionCanvas = new Canvas
            {
                Width = 500,  // 与背景图片的宽度保持一致
                Height = 300  // 与背景图片的高度保持一致
            };

            // 在Canvas加载完成后再显示可选择的图片
            selectionCanvas.Loaded += (s, e) =>
            {
                DisplaySelectableImagesMode4(selectionCanvas, rand);
            };

            // 创建一个带白色边框的Border，表示随机生成范围
            Border selectionBorder = new Border
            {
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(2),
                Width = 1000,  // 边框的宽度，与Canvas保持一致
                Height = 600,  // 边框的高度，与Canvas保持一致
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, -100, 0, 0)  // 通过Margin属性上移100像素
            };

            // 创建一个StackPanel来包含选择Canvas和正确图片Panel
            StackPanel mainPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 20, 0, 0)
            };

            // 将Canvas添加到Grid的前景层（背景图片之上）
            gameGrid.Children.Add(selectionCanvas);

            // 将带边框的Border添加到Canvas之上，表示随机生成范围
            gameGrid.Children.Add(selectionBorder);

            // 在StackPanel的下方显示需要选择的正确图片
            DisplayCorrectImagesMode4(mainPanel);

            // 将StackPanel添加到Grid的下方
            gameGrid.Children.Add(mainPanel);

            // 将Grid添加到MainGrid中
            MainGrid.Children.Add(gameGrid);
        }

        private void DisplaySelectableImagesMode4(Canvas selectionCanvas, Random rand)
        {
            // 边界信息
            double leftBound = -203.2;
            double rightBound = 750.8;
            double topBound = -171.98;
            double bottomBound = 312.52;

            // 初始化图片显示计数
            List<System.Windows.Controls.Image> imagesToDisplay = new List<System.Windows.Controls.Image>();

            // 添加正确图片到图片显示列表
            foreach (var correctImage in correctImages)
            {
                // 为每个正确图片随机生成显示次数
                int displayCount = rand.Next(mini_right_display, max_right_display + 1);
                for (int i = 0; i < displayCount; i++)
                {
                    System.Windows.Controls.Image imgCopy = new System.Windows.Controls.Image
                    {
                        Source = correctImage.Source,
                        Width = 80,
                        Height = 80,
                        Margin = new Thickness(10)
                    };
                    imagesToDisplay.Add(imgCopy);
                }
            }

            // 添加干扰图片到图片显示列表
            List<int> remainingIndices = Enumerable.Range(0, imagePaths[1].Length)
                .Where(i => !correctImages.Any(c => ((BitmapImage)c.Source).UriSource.ToString().EndsWith(imagePaths[1][i]))
                             && imagePaths[1][i] != "EXO/2/Background.png")
                .OrderBy(x => rand.Next())
                .Take(mislead_picture_display_number)
                .ToList();

            foreach (var index in remainingIndices)
            {
                System.Windows.Controls.Image img = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri(imagePaths[1][index], UriKind.Relative)),
                    Width = 80,
                    Height = 80,
                    Margin = new Thickness(10)
                };

                imagesToDisplay.Add(img);
            }

            // 随机化图片的位置，并将图片显示在Canvas中
            foreach (var img in imagesToDisplay)
            {
                // 随机生成X和Y坐标，确保图片不会超出背景图片边界
                double maxLeft = rightBound - img.Width;
                double maxTop = bottomBound - img.Height;

                double left = rand.NextDouble() * (maxLeft - leftBound) + leftBound;
                double top = rand.NextDouble() * (maxTop - topBound) + topBound;

                // 创建边框并设置图片位置
                Border border = new Border
                {
                    BorderThickness = new Thickness(2),
                    BorderBrush = Brushes.Transparent, // 初始时无边框
                    Child = img
                };

                // 设置 Border 的位置
                Canvas.SetLeft(border, left);
                Canvas.SetTop(border, top);

                selectionCanvas.Children.Add(border);
            }
        }

        private void confirmButton_Click4(object sender, RoutedEventArgs e)
        {
            bool isCorrect = true;
            int index = 0;

            foreach (var correctImage in correctImages)
            {
                string imageUri = ((BitmapImage)correctImage.Source).UriSource.ToString().Replace("pack://application:,,,", "");

                // 计算玩家输入的数量与实际数量是否匹配
                int correctImageCount = selectionCanvas.Children.OfType<Border>()
                    .Count(border => ((BitmapImage)((System.Windows.Controls.Image)border.Child).Source).UriSource.ToString().Replace("pack://application:,,,", "") == imageUri);

                if (userInputNumbers[index] != correctImageCount)
                {
                    isCorrect = false;
                    break;
                }

                index++;
            }

            if (isCorrect)
            {
                success_time++;
            }
            else
            {
                fail_time++;
            }

            EndGame(); // 触发结束游戏逻辑
        }

        private void DisplayCorrectImagesMode4(StackPanel mainPanel)
        {
            StackPanel correctPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0) // 调整正确图片的显示位置
            };

            // 创建一个HashSet用于去重，确保每个正确图片只显示一次
            HashSet<string> displayedImages = new HashSet<string>();

            foreach (var img in correctImages)
            {
                // 获取图片的URI
                string imageUri = ((BitmapImage)img.Source).UriSource.ToString();

                // 如果图片已经显示过，则跳过
                if (displayedImages.Contains(imageUri))
                    continue;

                // 添加到HashSet中，防止重复显示
                displayedImages.Add(imageUri);

                // 在下方面板中显示一次该图片
                System.Windows.Controls.Image correctImg = new System.Windows.Controls.Image
                {
                    Source = img.Source,
                    Width = 80,
                    Height = 80,
                    Margin = new Thickness(10)
                };

                correctPanel.Children.Add(correctImg);
            }

            mainPanel.Children.Add(correctPanel);
        }

        private void SetupGameMode3()
        {
            // 使确认按钮可见
            confirm.Visibility = Visibility.Visible;

            // 清除MainGrid中的所有内容
            MainGrid.Children.Clear();

            MainGrid.RowDefinitions.Clear();
            MainGrid.ColumnDefinitions.Clear();
            // 创建一个Grid来放置背景图片和选择图片
            Grid gameGrid = new Grid();

            // 创建背景图片
            System.Windows.Controls.Image backgroundImage = new System.Windows.Controls.Image
            {
                Source = new BitmapImage(new Uri("EXO/2/Background.png", UriKind.Relative)),
                Stretch = Stretch.Uniform,
                Width = 1000,  // 调整背景图片的宽度
                Height = 660,  // 调整背景图片的高度
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, -100, 0, 0)  // 通过Margin属性上移100像素
            };

            // 将背景图片添加到Grid的Background层
            gameGrid.Children.Add(backgroundImage);

            // 初始化正确图片列表
            correctImages = new List<System.Windows.Controls.Image>();
            selectedImages = new List<System.Windows.Controls.Image>();

            // 随机选择正确的图片，并将其添加到correctImages列表中
            Random rand = new Random();
            List<int> correctIndices = new List<int>();

            while (correctIndices.Count < right_picture_number)
            {
                int index = rand.Next(imagePaths[1].Length);

                if (imagePaths[1][index] != "EXO/2/Background.png" && !correctIndices.Contains(index))
                {
                    correctIndices.Add(index);

                    System.Windows.Controls.Image img = new System.Windows.Controls.Image
                    {
                        Source = new BitmapImage(new Uri(imagePaths[1][index], UriKind.Relative)),
                        Width = 80,
                        Height = 80,
                        Margin = new Thickness(10)
                    };

                    correctImages.Add(img);
                }
            }

            // 创建一个用于显示选择图片的Canvas
            Canvas selectionCanvas = new Canvas
            {
                Width = 1000,  // 与背景图片的宽度保持一致
                Height = 660  // 与背景图片的高度保持一致
            };

            // 在Canvas加载完成后再显示可选择的图片
            selectionCanvas.Loaded += (s, e) =>
            {
                DisplaySelectableImages(selectionCanvas, rand);
            };

            // 创建一个带白色边框的Border，表示随机生成范围
            Border selectionBorder = new Border
            {
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(2),
                Width = 1000,  // 边框的宽度，与Canvas保持一致
                Height = 600,  // 边框的高度，与Canvas保持一致
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, -100, 0, 0)  // 通过Margin属性上移100像素
            };

            // 创建一个StackPanel来包含选择Canvas和正确图片Panel
            StackPanel mainPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 20, 0, 0)
            };

            // 将Canvas添加到Grid的前景层（背景图片之上）
            gameGrid.Children.Add(selectionCanvas);

            // 将带边框的Border添加到Canvas之上，表示随机生成范围
            gameGrid.Children.Add(selectionBorder);

            // 在StackPanel的下方显示需要选择的正确图片    
            DisplayCorrectImages(mainPanel);

            // 将StackPanel添加到Grid的下方
            gameGrid.Children.Add(mainPanel);

            // 将Grid添加到MainGrid中
            MainGrid.Children.Add(gameGrid);
        }
        private void DisplayCorrectImages(StackPanel mainPanel)
        {
            StackPanel correctPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0) // 调整正确图片的显示位置
            };

            foreach (var img in correctImages)
            {
                System.Windows.Controls.Image correctImg = new System.Windows.Controls.Image
                {
                    Source = img.Source,
                    Width = 80,
                    Height = 80,
                    Margin = new Thickness(10)
                };
                correctPanel.Children.Add(correctImg);
            }

            mainPanel.Children.Add(correctPanel);
        }

        private void DisplaySelectableImages(Canvas selectionCanvas, Random rand)
        {
            // 边界信息
            double leftBound = -3.2;
            double rightBound = 950.8;
            double topBound = 1.98;
            double bottomBound = 482.52;

            // 创建一个可选择图片的索引列表，并首先添加正确图片的索引
            List<int> selectableIndices = correctImages
                .Select(img => Array.IndexOf(imagePaths[1], ((BitmapImage)img.Source).UriSource.ToString().Replace("pack://application:,,,", "")))
                .ToList();

            // 从剩余的图片中随机选择，直到达到chose_picture_number
            List<int> remainingIndices = Enumerable.Range(0, imagePaths[1].Length)
                .Where(i => !selectableIndices.Contains(i) && imagePaths[1][i] != "EXO/2/Background.png")
                .OrderBy(x => rand.Next())
                .Take(chose_picture_number - selectableIndices.Count)
                .ToList();

            selectableIndices.AddRange(remainingIndices);

            // 随机化选择图片的顺序
            selectableIndices = selectableIndices.OrderBy(x => rand.Next()).ToList();

            foreach (int index in selectableIndices)
            {
                System.Windows.Controls.Image img = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri(imagePaths[1][index], UriKind.Relative)),
                    Width = 80,
                    Height = 80,
                    Margin = new Thickness(10)
                };

                // 随机生成X和Y坐标，确保图片不会超出背景图片边界
                double maxLeft = rightBound - img.Width;
                double maxTop = bottomBound - img.Height;

                double left = rand.NextDouble() * (maxLeft - leftBound) + leftBound;
                double top = rand.NextDouble() * (maxTop - topBound) + topBound;

                // 创建边框并设置图片位置
                Border border = new Border
                {
                    BorderThickness = new Thickness(2),
                    BorderBrush = Brushes.Transparent, // 初始时无边框
                    Child = img
                };

                // 设置 Border 的位置
                Canvas.SetLeft(border, left);
                Canvas.SetTop(border, top);

                border.MouseLeftButtonDown += (sender, e) =>
                {
                    if (correctImages.Any(c => ((BitmapImage)c.Source).UriSource.ToString() == ((BitmapImage)img.Source).UriSource.ToString()))
                    {
                        if (selectedImages.Contains(img))
                        {
                            // 图片已被选中，取消选择
                            border.BorderBrush = Brushes.Transparent;
                            selectedImages.Remove(img);
                        }
                        else
                        {
                            // 图片未被选中，进行选择
                            border.BorderBrush = Brushes.Green;
                            selectedImages.Add(img);
                        }
                    }
                    else
                    {
                        if (selectedImages.Contains(img))
                        {
                            // 错误的图片已被选中，取消选择
                            border.BorderBrush = Brushes.Transparent;
                            selectedImages.Remove(img);
                        }
                        else
                        {
                            // 错误的图片未被选中，进行选择
                            border.BorderBrush = Brushes.Red;
                            selectedImages.Add(img);
                        }
                    }
                };

                selectionCanvas.Children.Add(border);
            }
        }

        private void confirmButton_Click2(object sender, RoutedEventArgs e)
        {
            bool isCorrect = false;

            if (train_mode == 2)
            {
                // 使用 Tag 属性进行比较
                bool allCorrectSelected = correctImages.All(ci => selectedImages.Any(si => (string)si.Tag == (string)ci.Tag));
                bool noIncorrectSelected = selectedImages.All(si => correctImages.Any(ci => (string)si.Tag == (string)ci.Tag));

                // 只有在所有正确图片都被选中且没有选中错误图片的情况下，才算答对
                isCorrect = allCorrectSelected && noIncorrectSelected;
                isCorrectAnswer = isCorrect;

                if (isCorrect)
                {
                    success_time++;
                    correctAnswers[level] += 1;
                }
                else
                {
                    fail_time++;
                    wrongAnswers[level] += 1;
                    int ignoredCount = correctImages.Count(ci => !selectedImages.Any(si => ((BitmapImage)si.Source).UriSource == ((BitmapImage)ci.Source).UriSource));
                    ignoreAnswers[level] += ignoredCount;
                    modeTextBlock.Text = "很遗憾答错了！";
                    modeTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                    foreach (var border in choicePanel.Children.OfType<Border>())
                    {
                        border.BorderBrush = Brushes.Transparent; // 重置边框颜色
                    }

                    // 清空玩家选择的图片列表
                    selectedImages.Clear(); // 清空已选择的图片列表
                    DispatcherTimer timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(3); // 设置3秒的间隔
                    timer.Tick += (s, args) =>
                    {
                        //InitializeGame(); // 再次重新开始游戏
                        modeTextBlock.Text = "";
                        modeTextBlock.Foreground = new SolidColorBrush(Colors.Black);
                        timer.Stop();
                    };
                    timer.Start();
                }

                // 更新最近的游戏结果队列（模式2）
                EndGame(); // 触发结束游戏逻辑
            }
            else if (train_mode == 3)
            {
                // 模式3的确认逻辑
                isCorrect = selectedImages.Count == correctImages.Count &&
                            selectedImages.All(si => correctImages.Any(ci => ((BitmapImage)si.Source).UriSource == ((BitmapImage)ci.Source).UriSource));
                isCorrectAnswer = isCorrect;
                if (isCorrect)
                {
                    success_time++;
                    correctAnswers[level] += 1;
                    // modeTextBlock.Text = "恭喜你答对了！";
                    modeTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                    Panel.SetZIndex(end, 999);
                    end.Visibility = Visibility.Visible;

                    confirm.Visibility = Visibility.Collapsed;
                    // 调用 EndGame 方法
                    // EndGame();
                }
                else
                {
                    fail_time++;
                    wrongAnswers[level] += 1;
                    int ignoredCount = correctImages.Count(ci => !selectedImages.Any(si => ((BitmapImage)si.Source).UriSource == ((BitmapImage)ci.Source).UriSource));
                    ignoreAnswers[level] += ignoredCount;

                    // 重新初始化游戏内容
                    modeTextBlock.Text = "很遗憾答错了！";
                    modeTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                    DispatcherTimer timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(3); // 设置3秒的间隔
                    foreach (var selectedImage in selectedImages)
                    {
                        // 重置图片的边框颜色，表示取消选择
                        var border = selectedImage.Parent as Border;
                        if (border != null)
                        {
                            border.BorderBrush = Brushes.Transparent; // 恢复到未选择状态
                        }
                    }
                    selectedImages.Clear(); // 清空已选择的图片列表




                    timer.Tick += (s, args) =>
                    {
                        //InitializeGame(); // 再次重新开始游戏
                        //modeTextBlock.Text = "需要寻找的目标对象出现屏幕的下部，从上部的图片中将这些对象寻找出来";

                        modeTextBlock.Text = "";
                        modeTextBlock.Foreground = new SolidColorBrush(Colors.Black);
                        timer.Stop();
                    };
                    timer.Start();
                }

                UpdateRecentResultsMode3(isCorrect);
                EndGame(); // 触发结束游戏逻辑
            }
            if (train_mode == 4) // 确保这是模式4的逻辑
            {
                isCorrect = true;
                // 获取用户输入的图片数量列表
                List<int> userInputCounts = new List<int>(userInputNumbers);

                // 初始化正确图片数量计数
                Dictionary<string, int> correctImageCounts = new Dictionary<string, int>();

                foreach (var correctImage in correctImages)
                {
                    string imageUri = ((BitmapImage)correctImage.Source).UriSource.ToString().Replace("pack://application:,,,", "");

                    if (!correctImageCounts.ContainsKey(imageUri))
                    {
                        correctImageCounts[imageUri] = 0;
                    }

                    correctImageCounts[imageUri]++;
                }

                foreach (var correctImageUri in correctImageCounts.Keys)
                {
                    // 获取背景图片中实际出现的正确图片数量
                    int actualCount = selectionCanvas.Children.OfType<Border>()
                        .Count(border => ((BitmapImage)((System.Windows.Controls.Image)border.Child).Source).UriSource.ToString().Replace("pack://application:,,,", "") == correctImageUri);

                    // 检查玩家输入的数量是否匹配实际数量
                    if (!userInputCounts.Contains(actualCount))
                    {
                        isCorrect = false;
                        break;
                    }

                    // 移除已匹配的数量，避免重复匹配
                    userInputCounts.Remove(actualCount);
                }
                isCorrectAnswer = isCorrect;
                if (isCorrect)
                {
                    success_time++;
                    correctAnswers[level] += 1;
                    modeTextBlock.Text = "恭喜你答对了！";
                    modeTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                    Panel.SetZIndex(end, 999);
                    end.Visibility = Visibility.Visible;
                    confirm.Visibility = Visibility.Collapsed;
                    // 调用 EndGame 方法
                    // EndGame();
                }
                else
                {
                    if (fix_bug != 0)
                    {
                        fail_time++;
                        wrongAnswers[level] += 1;
                        int ignoredCount = correctImages.Count(ci => !selectedImages.Any(si => ((BitmapImage)si.Source).UriSource == ((BitmapImage)ci.Source).UriSource));
                        ignoreAnswers[level] += ignoredCount;
                        // 重新初始化游戏内容
                        modeTextBlock.Text = "很遗憾答错了！";
                        modeTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                        DispatcherTimer timer = new DispatcherTimer();
                        timer.Interval = TimeSpan.FromSeconds(3); // 设置3秒的间隔
                        timer.Tick += (s, args) =>
                        {
                            //InitializeGame(); // 再次重新开始游戏
                            // 重置用户输入
                            userInputNumbers.Clear();
                            userInput = string.Empty;
                            //UpdateTextBlock();
                            modeTextBlock.Text = "";
                            modeTextBlock.Foreground = new SolidColorBrush(Colors.Black);
                            // 停止计时器，防止重复触发
                            timer.Stop();
                        };
                        timer.Start();
                    }
                }
                fix_bug++;
                // 更新最近的游戏结果
                UpdateRecentResultsMode4(isCorrect);

                // 调整难度
                AdjustDifficultyBasedOnResultsMode4();

                EndGame(); // 触发结束游戏逻辑
            }

            if (train_mode == 1)
            {
                SubmitInput();
            }
        }

        private void UpdateRecentResultsMode3(bool isCorrect)
        {
            if (recentResults.Count >= 5)
            {
                recentResults.Dequeue(); // 移除最早的结果
            }
            recentResults.Enqueue(isCorrect); // 添加当前结果

            if (recentResults.Count == 5)
            {
                //AdjustDifficultyBasedOnResultsMode3(); // 当结果达到5次时调整难度
            }

            LevelStatisticsAction?.Invoke(level, 18);

            int correctCount = recentResults.Count(result => result);
            int wrongCount = recentResults.Count(result => !result);

            // 更新正确和错误次数的统计
            RightStatisticsAction?.Invoke(correctCount, 5);
            WrongStatisticsAction?.Invoke(wrongCount, 5);
        }

        private void UpdateRecentResultsMode2(bool isCorrect)
        {
            if (recentResults.Count >= 5)
            {
                recentResults.Dequeue(); // 移除最早的结果
            }
            recentResults.Enqueue(isCorrect); // 添加当前结果

            if (recentResults.Count == 5)
            {
                AdjustDifficultyBasedOnResultsMode2();
            }
        }

        private void AdjustDifficultyBasedOnResultsMode2()
        {
            int correctCount = recentResults.Count(result => result);
            int wrongCount = recentResults.Count(result => !result);

            if (correctCount >= level_updata_threshold)
            {
                IncreaseDifficultyMode2();
            }
            else if (wrongCount >= level_updata_threshold)
            {
                DecreaseDifficultyMode2();
            }
        }

        private void IncreaseDifficultyMode2()
        {
            if (level < 18) // 假设最大难度是18级
            {
                level++;
                AdjustDifficultyMode2(level);
            }
        }

        // 降低难度（模式2）
        private void DecreaseDifficultyMode2()
        {
            if (level > 1) // 假设最低难度是1级
            {
                level--;
                AdjustDifficultyMode2(level);
            }
        }

        private void ResetGameState()
        {
            // 重置所有游戏状态
            MainGrid.RowDefinitions.Clear();
            MainGrid.ColumnDefinitions.Clear();
            missingNumbers = new List<int>();
            userInputNumbers = new List<int>();
            userInput = string.Empty;
            UpdateTextBlock();
        }

        private void SetupGameMode1()
        {
            /*LJN
            配置反馈资源的路径             
             */
            CorrectSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");
            // 为 Image 控件加载图片 Source
            CorrectImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Correct.png"), UriKind.RelativeOrAbsolute));
            ErrorImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Error.png"), UriKind.RelativeOrAbsolute));


            confirm.Visibility = Visibility.Collapsed;
            textBlock.Visibility = Visibility.Visible;
            myCanvas.Visibility = Visibility.Visible;
            if (isTrialPhase)
            {
                confirm.Visibility = Visibility.Collapsed;
            }
            // 清除上一次游戏的内容
            MainGrid.Children.Clear();

            // 检查并移除 `confirm1` 的父容器
            if (confirm.Parent != null)
            {
                ((Grid)confirm.Parent).Children.Remove(confirm);
            }

            MainGrid2.Children.Add(confirm);

            // 根据 mode1_display_size 设置数字显示框的大小
            double width = 1072;
            double height = 920;

            switch (mode1_display_size)
            {
                case 1:
                    width = width * 0.6;
                    height = height * 0.6;
                    break;
                case 2:
                    width = width * 0.7;
                    height = height * 0.7;
                    break;
                case 3:
                    width = width * 0.85;
                    height = height * 0.85;
                    break;
                case 4:
                    width = width * 1.0;  // 手动设置大小
                    height = height * 1.0; // 手动设置大小
                    break;
            }

            // 创建一个带有白色边框的透明长方形
            Border gameAreaBorder = new Border
            {
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(5),
                Width = width,
                Height = height,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            MainGrid.Children.Add(gameAreaBorder);

            // 随机生成数字列表并移除几个数字
            List<int> numbers = Enumerable.Range(minnumber, maxnumber).ToList();
            AllNumbers = new List<int>(numbers); ;//存储所有数字,为后续选中数字出现做准备
            missingNumbers = RemoveRandomNumbers(numbers);
            // 用来存储已占用的位置（使用 HashSet 提高效率）
            HashSet<Rect> usedPositions = new HashSet<Rect>();
            // 显示剩余的数字，并随机分布在显示区域中
            Canvas numbersCanvas = new Canvas();
            Random rand = new Random();

            // 假设 gameAreaBorder 已经存在，且有它的宽度和高度
            double borderLeft = 0;
            double borderTop = 0;
            double borderRight = width;
            double borderBottom = height;

            foreach (int number in AllNumbers)
            {
                TextBlock numberText = new TextBlock
                {
                    Text = number.ToString(),
                    FontSize = Math.Sqrt(width * height / (maxnumber - minnumber - miss_number) * 0.5) * 0.6, // 使得数字的大小也不会太小
                    Foreground = Brushes.Black,
                    Visibility = Visibility.Visible
                };

                if (numbers.Contains(number)) { numberText.Visibility = Visibility.Visible; }
                else { numberText.Visibility = Visibility.Hidden; numberText.Foreground = Brushes.Orange; } // 如果是缺失了的数字，则它要隐藏

                NumTextDict[number] = numberText;//建立数字到textblock的索引

                // 尝试找到一个随机的不重叠的位置
                bool positionFound = false;
                double left = 0, top = 0;
                int attempts = 0;  // 限制尝试次数，防止无限循环
                while (!positionFound && attempts < 10000)
                {
                    // 随机生成一个位置
                    left = rand.Next(0, Math.Max(1, (int)(width - numberText.FontSize)));
                    top = rand.Next(0, Math.Max(1, (int)(height - numberText.FontSize)));

                    // 创建一个新的矩形，表示 numberText 的位置
                    Rect newRect = new Rect(left, top, numberText.FontSize, numberText.FontSize);

                    // 检查是否与现有的位置重叠
                    bool overlapsWithExisting = usedPositions.Any(existingRect => existingRect.IntersectsWith(newRect));

                    // 检查是否超出了 gameAreaBorder 的边界
                    bool overlapsWithBorder = newRect.Left < borderLeft || newRect.Top < borderTop ||
                                              newRect.Right > borderRight || newRect.Bottom > borderBottom;

                    // 如果没有重叠且没有超出边界，则位置有效
                    if (!overlapsWithExisting && !overlapsWithBorder)
                    {
                        usedPositions.Add(newRect);  // 如果没有重叠，记录位置
                        positionFound = true;
                    }

                    attempts++;  // 增加尝试次数
                }

                // 如果找到了合适位置
                if (positionFound)
                {
                    // 设置位置
                    Canvas.SetLeft(numberText, left);
                    Canvas.SetTop(numberText, top);
                    numbersCanvas.Children.Add(numberText);
                }
            }

            gameAreaBorder.Child = numbersCanvas;
        }

        private List<int> RemoveRandomNumbers(List<int> numbers)
        {
            Random rand = new Random();
            List<int> removedNumbers = new List<int>();
            while (removedNumbers.Count < miss_number)
            {
                int index = rand.Next(numbers.Count);
                if (numbers[index] == minnumber || numbers[index] == maxnumber) { continue; }//确保不会把最小值和最大值去掉
                removedNumbers.Add(numbers[index]);
                numbers.RemoveAt(index);
            }

            return removedNumbers.OrderBy(n => n).ToList(); // 返回已移除的数字（排序后的）
        }


        // 数字按钮按下事件处理函数
        private void OnNumberButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                int number = int.Parse(button.Content.ToString());
                userInputNumbers.Add(number);
                userInput += button.Content.ToString();
                UpdateTextBlock();
            }
        }

        // "✔" 按钮按下事件处理函数
        private void OnSubmitButtonClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(userInput))
            {
                int number = int.Parse(userInput);//获取用户的输入
                userInputNumbers.Add(number);//加到结果里来
                userInput = string.Empty;
                UpdateTextBlock();
                SubmitInput();//模式1提交输入



            }
        }
        // "确认" 按钮按下事件处理函数，原OnBackButtonClick功能
        private void confirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (train_mode == 4) // 确保这是模式4的逻辑
            {
                // 获取用户输入的图片数量列表
                List<int> userInputCounts = new List<int>(userInputNumbers);

                // 初始化正确图片数量计数
                Dictionary<string, int> correctImageCounts = new Dictionary<string, int>();

                foreach (var correctImage in correctImages)
                {
                    string imageUri = ((BitmapImage)correctImage.Source).UriSource.ToString().Replace("pack://application:,,,", "");

                    if (!correctImageCounts.ContainsKey(imageUri))
                    {
                        correctImageCounts[imageUri] = 0;
                    }

                    correctImageCounts[imageUri]++;
                }

                bool isCorrect = true;

                foreach (var correctImageUri in correctImageCounts.Keys)
                {
                    // 获取背景图片中实际出现的正确图片数量
                    int actualCount = selectionCanvas.Children.OfType<Border>()
                        .Count(border => ((BitmapImage)((System.Windows.Controls.Image)border.Child).Source).UriSource.ToString().Replace("pack://application:,,,", "") == correctImageUri);

                    // 检查玩家输入的数量是否匹配实际数量
                    if (!userInputCounts.Contains(actualCount))
                    {
                        isCorrect = false;
                        break;
                    }

                    // 移除已匹配的数量，避免重复匹配
                    userInputCounts.Remove(actualCount);
                }

                if (isCorrect)
                {
                    success_time++;
                }
                else
                {
                    fail_time++;
                }

                // 更新最近的游戏结果
                UpdateRecentResultsMode4(isCorrect);

                // 调整难度
                AdjustDifficultyBasedOnResultsMode4();

                EndGame(); // 触发结束游戏逻辑
            }
            else
            {
                SubmitInput();
            }
        }

        private void UpdateRecentResultsMode4(bool isCorrect)
        {
            if (recentResults.Count >= 5)
            {
                recentResults.Dequeue(); // 移除最早的结果
            }
            recentResults.Enqueue(isCorrect); // 添加当前结果
        }

        private void AdjustDifficultyBasedOnResultsMode4()
        {
            // 首先检查最近5次结果是否达到了5次
            if (recentResults.Count < 5)
            {
                return; // 如果结果少于5次，不更新难度
            }

            // 计算最近5次中的正确和错误次数
            int correctCount = recentResults.Count(result => result);
            int wrongCount = recentResults.Count(result => !result);

            if (correctCount >= level_updata_threshold)
            {
                IncreaseDifficultyMode4();
            }
            else if (wrongCount >= level_updata_threshold)
            {
                DecreaseDifficultyMode4();
            }
        }

        private void IncreaseDifficultyMode4()
        {
            if (level < 18) // 假设最大难度是18级
            {
                level++;
                AdjustDifficultyMode4(level);
            }
        }

        private void DecreaseDifficultyMode4()
        {
            if (level > 1) // 假设最低难度是1级
            {
                level--;
                AdjustDifficultyMode4(level);
            }
        }

        // "←" 按钮按下事件处理函数，新功能：删除上一个输入的数字
        private void OnBackButtonClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(userInput))
            {
                // 删除最后一个数字
                userInputNumbers.RemoveAt(userInputNumbers.Count - 1);
                userInput = userInput.Substring(0, userInput.Length - 1);
                UpdateTextBlock();
            }
        }

        private void UpdateTextBlock()
        {
            displayTextBlock.Text = userInput;
        }

        private void SubmitInput()
        {
            //将用户本次输入的数字与缺失的数字列表进行比较
            int InputNum = userInputNumbers.LastOrDefault(); ;//获取最后一个输入
            bool isCorrect = missingNumbers.Contains(InputNum);
            if (AllNumbers.Contains(InputNum))//用户输入的数字在范围内
            {
                if (isCorrect)
                {//如果用户本次输入的数字是缺失的数字的其中之一，则显示到右侧数字区域
                    NumTextDict[InputNum].Visibility = Visibility.Visible;//数字可见
                    missingNumbers.Remove(InputNum);//缺失数字列表中去除这个数字
                }
                else
                {
                    NumTextDict[InputNum].Foreground = Brushes.Red;//红色以警示
                    ErrorCount--;
                }
            }
            else
            {//如果不在范围内直接错
                ErrorCount--;
                isCorrect = false;
            }
            GroupResultCheck(isCorrect);//检查需不需要判断这个整道题的对错
        }

        private void GroupResultCheck(bool IsNumFeedBack)//完整的这道题的结果检查，判断整道题的对错
        {//IsNumFeedBack是用来判断需不需要给单个数字判断反馈，可以防止和整道题的反馈重叠
            bool IfChecked = false;//设置它是用来判断这整道题到底做完没有
            if (ErrorCount <= 0)
            {
                ErrorCount = ErrorLimit;
                IfChecked = true;
                // 重新初始化游戏内容
                PlayWav(ErrorSoundPath);
                ShowFeedbackImage(ErrorImage);
                IntervalTimer.Interval = TimeSpan.FromSeconds(ErrorInterval);
                //modeTextBlock.Text = "很遗憾答错了！";
                //modeTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(3); // 设置3秒的间隔
                timer.Tick += (s, args) =>
                {
                    //InitializeGame(); // 再次重新开始游戏
                    // 重置用户输入
                    userInputNumbers.Clear();
                    userInput = string.Empty;
                    UpdateTextBlock();
                    //modeTextBlock.Text = "找出数字范围内的缺失数字，并将它们从小到大逐个输入";
                    modeTextBlock.Foreground = new SolidColorBrush(Colors.Black);
                    // 停止计时器，防止重复触发
                    timer.Stop();
                };
                OverLayGrid.IsEnabled = false;
                IntervalTimer.Start();
                timer.Start(); // 启动计时器

            }
            else if (missingNumbers.Count <= 0)
            {
                IfChecked = true;isCorrectAnswer = true;
                PlayWav(CorrectSoundPath);
                ShowFeedbackImage(CorrectImage);
                //modeTextBlock.Text = "恭喜你答对了！";
                //modeTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                myCanvas.IsEnabled = false;//防止用户后续输入
                IntervalTimer.Interval = TimeSpan.FromSeconds(CorrectInterval);
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(2); // 设置3秒的间隔
                timer.Tick += (s, args) =>
                {
                    //modeTextBlock.Text = "找出数字范围内的缺失数字，并将它们从小到大逐个输入";
                    modeTextBlock.Text = "";
                    modeTextBlock.Foreground = new SolidColorBrush(Colors.Black);
                    // 停止计时器，防止重复触发
                    timer.Stop();
                };


                timer.Start(); // 启动计时器
                Panel.SetZIndex(end, 999);
                end.Visibility = Visibility.Visible;
                confirm.Visibility = Visibility.Collapsed;
                // 调用 EndGame 方法
                // EndGame();
            }

            if(IfChecked==false)
            {//说明只是需要单独一个数字的判断反馈
                if (IsNumFeedBack)
                {
                    PlayWav(CorrectSoundPath);
                    ShowFeedbackImage(CorrectImage);
                }
                else
                {
                    PlayWav(ErrorSoundPath);
                    ShowFeedbackImage(ErrorImage);
                }
            }

        }

        private void UpdateRecentResults(bool isCorrect)
        {
            if (recentResults.Count >= 5)
            {
                recentResults.Dequeue(); // 移除最早的结果
            }
            recentResults.Enqueue(isCorrect); // 添加当前结果

            if (recentResults.Count == 5)
            {
                AdjustDifficultyBasedOnResults();
            }
        }

        private void AdjustDifficultyBasedOnResults()
        {
            int correctCount = recentResults.Count(result => result);
            int wrongCount = recentResults.Count(result => !result);

            if (correctCount >= level_updata_threshold)
            {
                IncreaseDifficulty();
            }
            else if (wrongCount >= level_updata_threshold)
            {
                DecreaseDifficulty();
            }
        }

        private void IncreaseDifficulty()
        {
            if (level < 18) // 假设最大难度是18级
            {
                level++;
                AdjustDifficulty(level);
            }
        }

        private void DecreaseDifficulty()
        {
            if (level > 1) // 假设最低难度是1级
            {
                level--;
                AdjustDifficulty(level);
            }
        }

        private void EndGame()
        {
            if (isTrialPhase)
            {
                if (isCorrectAnswer)
                {
                    // 用户答对，进入下一个试玩模块
                    currentTrialModeIndex++;
                    if (currentTrialModeIndex < trialModes.Count)
                    {
                        // 还有模式需要试玩，继续下一个模式的试玩
                        train_mode = trialModes[currentTrialModeIndex];
                        InitializeGame();
                    }
                    else
                    {
                        // 所有模式都已试玩完毕，显示“进入模块”按钮
                        isTrialPhase = false;
                        Panel.SetZIndex(end, 999);
                        end.Visibility = Visibility.Visible;
                        end.Content = "进入游戏"; // 修改按钮内容
                        confirm.Visibility = Visibility.Collapsed;
                        modeTextBlock.Text = "所有模式试玩完毕";
                        OnGameBegin();//直接开始游戏了
                    }
                }
                else
                {
                    // 用户未答对，重新开始当前模块
                    //modeTextBlock.Text = "很遗憾答错了！";
                    //modeTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                    //DispatcherTimer timer = new DispatcherTimer();
                    //timer.Interval = TimeSpan.FromSeconds(3); // 设置3秒的间隔
                    //timer.Tick += (s, args) =>
                    //{
                    //    InitializeGame(); // 再次重新开始当前模块
                    //    timer.Stop();
                    //};
                    //timer.Start();
                }
            }
            else
            {
                // 正式游戏逻辑，重新开始游戏
                OnGameBegin();
            }
        }


        private void AdjustDifficulty(int level)
        {
            switch (level)
            {
                case 1:
                    maxnumber = 5;
                    miss_number = 1;
                    mode1_display_size = 1; // 小
                    break;
                case 2:
                    maxnumber = 7;
                    miss_number = 2;
                    mode1_display_size = 1; // 小
                    break;
                case 3:
                    maxnumber = 8;
                    miss_number = 2;
                    mode1_display_size = 1; // 小
                    break;
                case 4:
                    maxnumber = 9;
                    miss_number = 3;
                    mode1_display_size = 1; // 小
                    break;
                case 5:
                    maxnumber = 9;
                    miss_number = 3;
                    mode1_display_size = 1; // 小
                    break;
                case 6:
                    maxnumber = 12;
                    miss_number = 3;
                    mode1_display_size = 2; // 中
                    break;
                case 7:
                    maxnumber = 14;
                    miss_number = 3;
                    mode1_display_size = 2; // 中
                    break;
                case 8:
                    maxnumber = 16;
                    miss_number = 4;
                    mode1_display_size = 2; // 中
                    break;
                case 9:
                    maxnumber = 18;
                    miss_number = 4;
                    mode1_display_size = 2; // 中
                    break;
                case 10:
                    maxnumber = 20;
                    miss_number = 4;
                    mode1_display_size = 3; // 大
                    break;
                case 11:
                    maxnumber = 24;
                    miss_number = 5;
                    mode1_display_size = 3; // 大
                    break;
                case 12:
                    maxnumber = 28;
                    miss_number = 5;
                    mode1_display_size = 3; // 大
                    break;
                case 13:
                    maxnumber = 30;
                    miss_number = 5;
                    mode1_display_size = 4; // 全屏
                    break;
                case 14:
                    maxnumber = 35;
                    miss_number = 5;
                    mode1_display_size = 4; // 全屏
                    break;
                case 15:
                    maxnumber = 38;
                    miss_number = 6;
                    mode1_display_size = 4; // 全屏
                    break;
                case 16:
                    maxnumber = 40;
                    miss_number = 6;
                    mode1_display_size = 4; // 全屏
                    break;
                case 17:
                    maxnumber = 45;
                    miss_number = 7;
                    mode1_display_size = 4; // 全屏
                    break;
                case 18:
                    maxnumber = 50;
                    miss_number = 8;
                    mode1_display_size = 4; // 全屏
                    break;
                default:
                    maxnumber = 5;
                    miss_number = 1;
                    mode1_display_size = 1; // 小
                    break;
            }
        }

        private void AdjustDifficultyMode2(int level)
        {
            switch (level)
            {
                case 1:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    break;
                case 2:
                    right_picture_number = 2;
                    chose_picture_number = 6;
                    break;
                case 3:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    break;
                case 4:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    break;
                case 5:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    break;
                case 6:
                    right_picture_number = 2;
                    chose_picture_number = 6;
                    break;
                case 7:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    break;
                case 8:
                    right_picture_number = 2;
                    chose_picture_number = 6;
                    break;
                case 9:
                    right_picture_number = 3;
                    chose_picture_number = 6;
                    break;
                case 10:
                    right_picture_number = 3;
                    chose_picture_number = 8;
                    break;
                case 11:
                    right_picture_number = 3;
                    chose_picture_number = 6;
                    break;
                case 12:
                    right_picture_number = 3;
                    chose_picture_number = 8;
                    break;
                case 13:
                    right_picture_number = 3;
                    chose_picture_number = 6;
                    break;
                case 14:
                    right_picture_number = 3;
                    chose_picture_number = 8;
                    break;
                case 15:
                    right_picture_number = 3;
                    chose_picture_number = 6;
                    break;
                case 16:
                    right_picture_number = 3;
                    chose_picture_number = 8;
                    break;
                case 17:
                    right_picture_number = 3;
                    chose_picture_number = 12; // 4x3
                    break;
                case 18:
                    right_picture_number = 3;
                    chose_picture_number = 18; // 6x3
                    break;
                default:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    break;
            }
        }

        private void AdjustDifficultyMode3(int level)
        {
            switch (level)
            {
                case 1:
                    right_picture_number = 2;
                    chose_picture_number = 3; // 2个正确图片 + 1个干扰图片
                    break;
                case 2:
                    right_picture_number = 2;
                    chose_picture_number = 4; // 2个正确图片 + 2个干扰图片
                    break;
                case 3:
                    right_picture_number = 3;
                    chose_picture_number = 5; // 3个正确图片 + 2个干扰图片
                    break;
                case 4:
                    right_picture_number = 3;
                    chose_picture_number = 6; // 3个正确图片 + 3个干扰图片
                    break;
                case 5:
                    right_picture_number = 3;
                    chose_picture_number = 7; // 3个正确图片 + 4个干扰图片
                    break;
                case 6:
                    right_picture_number = 4;
                    chose_picture_number = 8; // 4个正确图片 + 4个干扰图片
                    break;
                case 7:
                    right_picture_number = 4;
                    chose_picture_number = 9; // 4个正确图片 + 5个干扰图片
                    break;
                case 8:
                    right_picture_number = 4;
                    chose_picture_number = 10; // 4个正确图片 + 6个干扰图片
                    break;
                case 9:
                    right_picture_number = 5;
                    chose_picture_number = 11; // 5个正确图片 + 6个干扰图片
                    break;
                case 10:
                    right_picture_number = 5;
                    chose_picture_number = 12; // 5个正确图片 + 7个干扰图片
                    break;
                case 11:
                    right_picture_number = 6;
                    chose_picture_number = 13; // 6个正确图片 + 7个干扰图片
                    break;
                case 12:
                    right_picture_number = 6;
                    chose_picture_number = 14; // 6个正确图片 + 8个干扰图片
                    break;
                case 13:
                    right_picture_number = 7;
                    chose_picture_number = 15; // 7个正确图片 + 8个干扰图片
                    break;
                case 14:
                    right_picture_number = 7;
                    chose_picture_number = 16; // 7个正确图片 + 9个干扰图片
                    break;
                case 15:
                    right_picture_number = 8;
                    chose_picture_number = 17; // 8个正确图片 + 9个干扰图片
                    break;
                case 16:
                    right_picture_number = 8;
                    chose_picture_number = 18; // 8个正确图片 + 10个干扰图片
                    break;
                case 17:
                    right_picture_number = 9;
                    chose_picture_number = 19; // 9个正确图片 + 10个干扰图片
                    break;
                case 18:
                    right_picture_number = 10;
                    chose_picture_number = 20; // 10个正确图片 + 10个干扰图片
                    break;
                default:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    break;
            }
        }

        private void AdjustDifficultyMode4(int level)
        {
            switch (level)
            {
                case 1:
                    right_picture_number = 1; // 要计数对象的种类
                    max_right_display = 2;
                    mini_right_display = 2;
                    mislead_picture_display_number = 4; // 不相关物品种类
                    break;
                case 2:
                    right_picture_number = 1;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 5;
                    break;
                case 3:
                    right_picture_number = 1;
                    max_right_display = 4;
                    mini_right_display = 4;
                    mislead_picture_display_number = 6;
                    break;
                case 4:
                    right_picture_number = 1;
                    max_right_display = 5;
                    mini_right_display = 5;
                    mislead_picture_display_number = 7;
                    break;
                case 5:
                    right_picture_number = 1;
                    max_right_display = 6;
                    mini_right_display = 6;
                    mislead_picture_display_number = 8;
                    break;
                case 6:
                    right_picture_number = 2;
                    max_right_display = 4;
                    mini_right_display = 4;
                    mislead_picture_display_number = 6;
                    break;
                case 7:
                    right_picture_number = 2;
                    max_right_display = 5;
                    mini_right_display = 5;
                    mislead_picture_display_number = 7;
                    break;
                case 8:
                    right_picture_number = 2;
                    max_right_display = 6;
                    mini_right_display = 3;
                    mislead_picture_display_number = 8;
                    break;
                case 9:
                    right_picture_number = 2;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 9;
                    break;
                case 10:
                    right_picture_number = 2;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 10;
                    break;
                case 11:
                    right_picture_number = 3;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 7;
                    break;
                case 12:
                    right_picture_number = 3;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 8;
                    break;
                case 13:
                    right_picture_number = 3;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 9;
                    break;
                case 14:
                    right_picture_number = 3;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 10;
                    break;
                case 15:
                    right_picture_number = 3;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 11;
                    break;
                case 16:
                    right_picture_number = 3;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 15;
                    break;
                case 17:
                    right_picture_number = 3;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 20;
                    break;
                case 18:
                    right_picture_number = 3;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 25;
                    break;
                default:
                    right_picture_number = 1;
                    max_right_display = 2;
                    mini_right_display = 2;
                    mislead_picture_display_number = 4;
                    break;
            }
        }


        private void end_Click(object sender, RoutedEventArgs e)
        {
            // 开始答题的相关逻辑
            EndGame();
        }

        int currentPage = -1;

        private void Button_1_Click(object sender, RoutedEventArgs e)
        {
            currentPage--;
            PageSwitch();
        }

        private void Button_2_Click(object sender, RoutedEventArgs e)
        {
            currentPage++;
            PageSwitch();
        }

        private void Button_3_Click(object sender, RoutedEventArgs e)
        {
            OnGameBegin();
        }

        async void PageSwitch()
        {
            switch (currentPage)
            {
                case 0:
                    {
                        Text_1.Visibility = Visibility.Visible;
                        Image_1.Visibility = Visibility.Visible;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;

                        MainGrid.Visibility = Visibility.Collapsed;
                        MainGrid2.Visibility = Visibility.Collapsed;

                        Button_1.IsEnabled = false;
                        Button_2.Content = "下一步";
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Margin = new Thickness(329, 850, 0, 0);
                        Button_3.Margin = new Thickness(770, 850, 0, 0);

                        await OnVoicePlayAsync(Text_1.Text);
                    }
                    break;
                case 1:
                    {
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Visible;
                        Image_2.Visibility = Visibility.Visible;
                        MainGrid.Visibility = Visibility.Collapsed;
                        MainGrid2.Visibility = Visibility.Collapsed;

                        Button_1.IsEnabled = true;
                        Button_2.Content = "试玩";
                        Button_1.Visibility = Visibility.Visible;
                        Button_2.Margin = new Thickness(550, 850, 0, 0);
                        Button_3.Margin = new Thickness(911, 850, 0, 0);

                        await OnVoicePlayAsync(Text_2.Text);
                    }
                    break;
                case 2:
                    {
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        MainGrid2.Visibility = Visibility.Visible;
                        MainGrid.Visibility = Visibility.Visible;
                        // 强制焦点保持在窗口
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Collapsed;

                        //LJN，在讲解模块调用委托
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("找出数字范围内的缺失数字，并将它们从小到大逐个输入。");//增加代码，调用函数，显示数字人下的文字
                        //LJN
                    }
                    break;
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)//绑定自定义鼠标光标和默认鼠标光标
        {
            Point position = e.GetPosition(this);
            Canvas.SetLeft(CustomCursor, position.X);
            Canvas.SetTop(CustomCursor, position.Y);
            // 获取 Canvas 的边界
            double canvasLeft = Canvas.GetLeft(CustomCursor);
            double canvasTop = Canvas.GetTop(CustomCursor);
            double canvasWidth = this.ActualWidth;
            double canvasHeight = this.ActualHeight;

            // 获取 CustomCursor 的宽高
            double cursorWidth = CustomCursor.Width;
            double cursorHeight = CustomCursor.Height;

            // 如果 CustomCursor 超出 Canvas 的边界，进行裁剪
            if (canvasLeft + cursorWidth > canvasWidth)
            {
                Canvas.SetLeft(CustomCursor, canvasWidth - cursorWidth); // 限制在右边界
            }
            if (canvasTop + cursorHeight > canvasHeight)
            {
                Canvas.SetTop(CustomCursor, canvasHeight - cursorHeight); // 限制在下边界
            }
            if (canvasLeft < 0)
            {
                Canvas.SetLeft(CustomCursor, 0); // 限制在左边界
            }
            if (canvasTop < 0)
            {
                Canvas.SetTop(CustomCursor, 0); // 限制在上边界
            }

            // 如果 CustomCursor 的位置超出 Canvas 范围，裁剪掉超出的部分
            Rect clipRect = new Rect(0, 0, canvasWidth, canvasHeight);
            CustomCursor.Clip = new RectangleGeometry(clipRect);  // 使用矩形裁剪区域
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

        private async void ShowFeedbackImage(System.Windows.Controls.Image image)//StopDurations单位是ms
        {//显示反馈的图片
            image.Visibility = Visibility.Visible;

            // 延迟指定的时间（例如1秒）
            await Task.Delay(StopDurations);

            image.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 讲解内容语音播放
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        async Task VoicePlayer(string message)
        {
            var voicePlayFunc = VoicePlayFunc;
            if (voicePlayFunc == null)
            {
                return;
            }

            await voicePlayFunc.Invoke(message);
        }
    }
}
