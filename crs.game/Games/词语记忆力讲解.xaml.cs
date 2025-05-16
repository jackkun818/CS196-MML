
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Linq;
using System.Data;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Text;
using System.Security.Claims;

namespace crs.game.Games
{
    /// <summary>
    /// 词语记忆力讲解.xaml 的交互逻辑
    /// </summary>
    public partial class 词语记忆力讲解 : BaseUserControl
    {
        private int NumberOfTextBlocks = 3;//要飘过的单词的个数
        private List<string> WordList = new List<string> { "这不是", "我", "不需要", "别记住", "得", "地", "双词" };
        private List<string> WordsToMemorizeList = new List<string>();

        private int RunDirection = 1;//词汇运动方向，1右0左
        private int StopDurations = 2000; // 停止时间，ms
        private int Speed = 5;
        private int TreatDurations = 1;//分钟为单位
        private double CountdownSeconds = 0;//倒计时的秒数

        public Action GameBeginAction { get; set; }

        public Func<string, Task> VoicePlayFunc { get; set; }

        public 词语记忆力讲解()
        {
            InitializeComponent();

            WordsToMemorizeList = WordsToMemorize.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            //需要记住的词汇我才用的方法是把那个textblock里面的text按空格分开得到一个列表，这个逻辑可改
            this.Loaded += TrainWindow_Loaded; // 确保在窗口加载完成后，设置按钮初始位置
            this.KeyDown += OnHostWindow_KeyDown; // 监听键盘事件


            //下面是一些初始化相关的，不用修改
            // 初始化倒计时相关
            CountdownSeconds = TreatDurations * 60; // 倒计时10秒，你可以根据需求更改
            CountdownTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1) // 计时器每秒触发一次
            };
            CountdownTimer.Tick += CountdownTimer_Tick;
            RandomObject = new Random();
            TextBlockDetected = new Dictionary<TextBlock, bool>(); // 初始化检测状态字典
            //string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory; // 当前项目的绝对路径
            //BaseDirectory = Path.Combine(BaseDirectory, "../../../crs.game/Games/pic/WORT");
            string BaseDirectory = "./pic/WORT/";
            CorrectSound = Path.Combine(BaseDirectory, $"Correct.wav");
            ErrorSound = Path.Combine(BaseDirectory, $"Error.wav");
            CorrectImage.Source = new BitmapImage(new Uri(Path.Combine(BaseDirectory, "Correct.png"), UriKind.RelativeOrAbsolute));
            ErrorImage.Source = new BitmapImage(new Uri(Path.Combine(BaseDirectory, "Error.png"), UriKind.RelativeOrAbsolute));
            TextBlockAnimations = new List<Storyboard>(); // 初始化动画列表

            this.Loaded += 词语记忆力讲解_Loaded;


        }

        private void 词语记忆力讲解_Loaded(object sender, RoutedEventArgs e)
        {
            // 页面加载时确保按键和焦点行为
            Button_2_Click(null, null);
            this.Focus();  // 确保焦点在窗口上
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            if (CountdownSeconds > 0)
            {
                CountdownSeconds--;
                CountdownDisplay.Text = CountdownSeconds.ToString(); // 更新界面上的倒计时显示
            }
            else
            {
                CountdownTimer.Stop();
                //倒计时结束了就自动点击结束按钮
                //OkButton_Click();
            }
        }

        private void TrainWindow_Loaded(object sender, RoutedEventArgs e)
        {//多写这一层是为了确保窗口在加载上的时候就开始创建textblock对象
            this.Focus(); // 将焦点设置到主窗口
            CreateTextBlocksOffScreen();

            BeltBorder.Visibility = Visibility.Collapsed;
        }

        private void CreateTextBlocksOffScreen()
        {
            double canvasHeight = WordArea.ActualHeight;
            double canvasWidth = WordArea.ActualWidth;


            // Fixed width and height for each TextBlock
            double textBlockWidth = 200;

            for (int i = 0; i < NumberOfTextBlocks; i++)
            {
                TextBlock textBlock = new TextBlock
                {
                    Text = GetRandomWord(),
                    Background = Brushes.Transparent, // 设置背景透明
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Height = canvasHeight,
                    Width = textBlockWidth,
                    FontFamily = new FontFamily("Times New Roman"), // 设置字体
                    FontSize = 160, // 设置字体大小
                    Visibility = Visibility.Hidden//一开始先隐藏掉
                };

                AdjustTextBlockSize(textBlock);

                // Add the TextBlock to Canvas
                WordArea.Children.Add(textBlock);

                // Calculate vertical center position for the TextBlock
                double textBlockHeight = textBlock.Height;
                double verticalCenterPosition = (canvasHeight - textBlockHeight) / 2;

                // Set initial position off-screen 
                double initialLeftPosition = RunDirection == 1 ? canvasWidth : -textBlockWidth;
                Canvas.SetLeft(textBlock, initialLeftPosition);
                Canvas.SetTop(textBlock, verticalCenterPosition); // 设置垂直居中位置

                // 将 TextBlock 的检测状态初始化为 false
                TextBlockDetected[textBlock] = false;
            }
        }

        private void AnimateTextBlocks(int direction, double speed)
        {
            double canvasWidth = WordArea.ActualWidth;
            double textBlockWidth = 200;
            double durationInSeconds = 11 - speed; // Speed 1 (slowest) -> 10 seconds, Speed 10 (fastest) -> 1 second

            // Calculate delay per TextBlock to avoid them starting at the same time
            double delayInterval = durationInSeconds / NumberOfTextBlocks;

            for (int i = 0; i < WordArea.Children.Count; i++)
            {
                if (WordArea.Children[i] is TextBlock textBlock)
                {
                    double from = direction == 0 ? canvasWidth : -textBlockWidth;
                    double to = direction == 0 ? -textBlockWidth : canvasWidth;

                    StartTextBlockAnimation(textBlock, from, to, durationInSeconds, TimeSpan.FromSeconds(i * delayInterval));
                }
            }
        }

        private void StartTextBlockAnimation(TextBlock textBlock, double from, double to, double durationInSeconds, TimeSpan beginTime)
        {
            // Create and configure the animation
            DoubleAnimation animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(durationInSeconds)),
                BeginTime = beginTime
            };

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, textBlock);
            Storyboard.SetTargetProperty(animation, new PropertyPath(Canvas.LeftProperty));

            // 每次动画结束时都更新文本内容
            storyboard.Completed += (s, e) =>
            {

                textBlock.Text = GetRandomWord();

                AdjustTextBlockSize(textBlock); // 动态调整 TextBlock 宽度
                StartTextBlockAnimation(textBlock, from, to, durationInSeconds, TimeSpan.Zero); // 重启动画
            };

            // 在启动动画前重置检测状态
            TextBlockDetected[textBlock] = false;
            TextBlockAnimations.Add(storyboard); // 将动画添加到列表

            // 动画进行期间，检查位置并更新可见性
            storyboard.CurrentTimeInvalidated += (s, e) =>
            {
                UpdateTextBlockVisibility(textBlock, WordArea);
            };
            // 为 Canvas 设置剪裁区域，确保 TextBlock 在 Canvas 外部部分不可见
            ApplyCanvasClip(WordArea);
            storyboard.Begin();
        }

        // 应用剪裁区域到 Canvas
        private void ApplyCanvasClip(Canvas containerCanvas)
        {//通过裁剪，实现textblock在这个canvas中部分可见部分不可见
            // 创建一个与 Canvas 相同大小的矩形
            RectangleGeometry clipGeometry = new RectangleGeometry
            {
                Rect = new Rect(0, 0, containerCanvas.ActualWidth, containerCanvas.ActualHeight)
            };

            // 将该矩形作为 Canvas 的剪裁区域
            containerCanvas.Clip = clipGeometry;
        }

        private void UpdateTextBlockVisibility(TextBlock textBlock, Canvas containerCanvas)
        {//对于运动中的textblock检查一下他们是否该出现
            // 获取 TextBlock 的当前左边距
            double left = Canvas.GetLeft(textBlock);
            double textBlockWidth = textBlock.ActualWidth;
            // 获取 Canvas 的宽度
            double canvasLeft = 0; // 假设 Canvas 的左边界为 0
            double canvasRight = containerCanvas.ActualWidth;
            // 判断 TextBlock 是否在 Canvas 的范围内
            if (left + textBlockWidth > canvasLeft && left < canvasRight)
            {
                // 在 Canvas 范围内，设置可见
                textBlock.Visibility = Visibility.Visible;
            }
            else
            {
                // 超出 Canvas 范围，设置隐藏
                textBlock.Visibility = Visibility.Hidden;
            }
        }

        private void AdjustTextBlockWidth(TextBlock textBlock)
        {
            // 使用 FormattedText 来测量文本宽度
            var formattedText = new FormattedText(
                textBlock.Text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
                textBlock.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                1);

            // 动态调整 TextBlock 的宽度
            textBlock.Width = formattedText.Width;
        }

        private void AdjustTextBlockSize(TextBlock textBlock)
        {
            // 使用 FormattedText 来测量文本尺寸
            var formattedText = new FormattedText(
                textBlock.Text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
                textBlock.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                1);

            // 动态调整 TextBlock 的宽度和高度
            textBlock.Width = formattedText.Width;
            textBlock.Height = formattedText.Height;
        }

        private void PositionRectangle(int direction)
        {
            if (direction == 0)
            {
                Canvas.SetLeft(TargetArea, 40); // 将Rectangle移到左侧
            }
            else
            {
                double canvasWidth = WordArea.ActualWidth;
                Canvas.SetLeft(TargetArea, canvasWidth - TargetArea.Width - 40); // 将Rectangle移到右侧
            }
            TargetArea.Visibility = Visibility.Visible;
        }

        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) // 按键检测
        {//这里需要注意是否是button被focus了，否则按键会识别成对button按下
            // 检查按下的键是否是你指定的键
            if (e.Key == System.Windows.Input.Key.Enter) // 假设你指定的键是回车键
            {
                CheckIntersection();
            }
        }

        private async void CheckIntersection()
        {
            // 获取 Rectangle 的边界
            double rectLeft = Canvas.GetLeft(TargetArea);
            double rectTop = Canvas.GetTop(TargetArea);

            // 检查是否为 NaN 并给予默认值
            if (double.IsNaN(rectLeft)) rectLeft = 0;
            if (double.IsNaN(rectTop)) rectTop = 0;

            // 创建 Rectangle 边界
            Rect rectangleBounds = new Rect(rectLeft, rectTop, TargetArea.Width, TargetArea.Height);

            foreach (var child in WordArea.Children)
            {
                if (child is TextBlock textBlock)
                {
                    // 如果 TextBlock 尚未被检测到
                    if (!TextBlockDetected[textBlock])
                    {
                        // 获取 TextBlock 的边界
                        double textBlockLeft = Canvas.GetLeft(textBlock);
                        double textBlockTop = Canvas.GetTop(textBlock);

                        // 检查是否为 NaN 并给予默认值
                        if (double.IsNaN(textBlockLeft)) textBlockLeft = 0;
                        if (double.IsNaN(textBlockTop)) textBlockTop = 0;

                        // 创建 TextBlock 边界
                        Rect textBlockBounds = new Rect(textBlockLeft, textBlockTop, textBlock.Width, textBlock.ActualHeight);

                        // 检查是否有重叠
                        if (rectangleBounds.IntersectsWith(textBlockBounds))
                        {
                            // 进行判断并更新 _ViewModel 计数器
                            if (WordsToMemorizeList.Contains(textBlock.Text))
                            {
                                PlayWav(CorrectSound);
                                ShowFeedbackImage(CorrectImage);
                                ShowFeedbackTextBlock(CorrectTextBlock); // 显示正确文本反馈
                            }
                            else
                            {
                                PlayWav(ErrorSound);
                                ShowFeedbackImage(ErrorImage);
                                ShowFeedbackTextBlock(ErrorTextBlock); // 显示正确文本反馈
                            }
                            // MessageBox.Show(textBlock.Text);
                            TextBlockDetected[textBlock] = true; // 更新检测状态

                            // 停止所有动画
                            foreach (var storyboard in TextBlockAnimations)
                            {
                                storyboard.Pause();
                            }

                            // 延迟 StopDurations 毫秒
                            await Task.Delay(StopDurations);

                            // 重新启动所有动画
                            foreach (var storyboard in TextBlockAnimations)
                            {
                                storyboard.Resume();
                            }

                            break; // 找到一个重叠的文本后退出循环
                        }
                    }
                }
            }
        }


        private void MemorizeOK_Click(object sender, RoutedEventArgs e)
        {//记忆完成，开始试玩
            if (IfHaveStarted == false)
            {
                int RunDirectionStart = RunDirection;
                int RunSpeed = Speed;
                WordsToMemorize.Visibility = Visibility.Hidden;//把需要记忆的单词给隐藏了
                BeltBorder.Visibility = Visibility.Visible;//把传送带边界框显示出来
                PositionRectangle(RunDirectionStart);
                AnimateTextBlocks(RunDirectionStart, (double)RunSpeed);
                MemorizeOK.Visibility = Visibility.Collapsed;
                Button_3.Margin = new Thickness(550, 850, 0, 0);
                OkButton.Visibility = Visibility.Visible;
                OkButton.Margin = new Thickness(1100, 850, 0, 0);
                // 重置倒计时并启动计时器
                CountdownSeconds = TreatDurations * 60; // 重新设置倒计时时间
                CountdownDisplay.Text = CountdownSeconds.ToString();
                CountdownTimer.Start();
            }
            IfHaveStarted = true;
        }
        //这里存放一些临时变量，coder不用修改的
        private List<Storyboard> TextBlockAnimations; // 列表存储所有动画
        private SoundPlayer soundPlayer; // 用来放歌
        public string ErrorSound;
        public string CorrectSound;
        private Random RandomObject;
        private Dictionary<TextBlock, bool> TextBlockDetected; // 存储每个 TextBlock 的检测状态
        private bool IfHaveStarted = false;//用来限制按钮只能按一次，第二次按没有作用的标志位
        private DispatcherTimer CountdownTimer;

        //这里存放一些功能函数，不用修改
        private string GetRandomWord()
        {//获取随机内容
            // 确保两个列表都已经加载
            if ((WordList == null || WordList.Count == 0) && (WordsToMemorizeList == null || WordsToMemorizeList.Count == 0))
            {
                return "No Words Loaded";
            }
            // 随机选择列表，0 表示 WordList，1 表示 _ViewModel.WordsToMemorizeList
            int listSelector = RandomObject.Next(0, 2);
            List<string> selectedList;
            if (listSelector == 0 && WordList != null && WordList.Count > 0)
            {
                selectedList = WordList;
            }
            else if (WordsToMemorizeList != null && WordsToMemorizeList.Count > 0)
            {
                selectedList = WordsToMemorizeList;
            }
            else if (WordList != null && WordList.Count > 0)
            {
                // 如果上面都不符合条件，那就使用剩下的非空列表
                selectedList = WordList;
            }
            else
            {
                return "No Words Loaded";
            }

            // 从选择的列表中随机选取一个元素
            int index = RandomObject.Next(selectedList.Count);
            return selectedList[index];
        }
        private void PlayWav(string filePath)
        {
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
        {
            image.Visibility = Visibility.Visible;

            // 延迟指定的时间（例如2秒）
            await Task.Delay(StopDurations);

            image.Visibility = Visibility.Collapsed;
        }

        private async void ShowFeedbackTextBlock(TextBlock textBlock)
        {
            textBlock.Visibility = Visibility.Visible;

            // 延迟指定的时间（例如2秒）
            await Task.Delay(StopDurations);

            textBlock.Visibility = Visibility.Collapsed;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            //这里是为了让rectangle不那么快显示出来
            TargetArea.Visibility = Visibility.Visible;
            // 开始答题的相关逻辑
            OnGameBegin();

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
                        // 显示讲解的第一个界面
                        Text_1.Visibility = Visibility.Visible;
                        Image_1.Visibility = Visibility.Visible;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;

                        // 隐藏试玩部分内容
                        CountdownDisplay.Visibility = Visibility.Collapsed;
                        WordsToMemorize.Visibility = Visibility.Collapsed;
                        MemorizeOK.Visibility = Visibility.Collapsed;
                        OkButton.Visibility = Visibility.Collapsed;
                        WordArea.Visibility = Visibility.Collapsed;
                        TargetArea.Visibility = Visibility.Collapsed;
                        CorrectImage.Visibility = Visibility.Collapsed;
                        ErrorImage.Visibility = Visibility.Collapsed;
                        CorrectTextBlock.Visibility = Visibility.Collapsed;
                        ErrorTextBlock.Visibility = Visibility.Collapsed;
                        //Button_1.IsEnabled = false;
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Content = "下一步";
                        Button_2.Margin = new Thickness(329, 850, 0, 0);
                        Button_3.Margin = new Thickness(770, 850, 0, 0);
                        await OnVoicePlayAsync(Text_1.Text);
                    }
                    break;
                case 1:
                    {
                        // 显示讲解的第二个界面
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Visible;
                        Image_2.Visibility = Visibility.Visible;



                        // 隐藏试玩部分的控件
                        CountdownDisplay.Visibility = Visibility.Collapsed;
                        WordsToMemorize.Visibility = Visibility.Collapsed;
                        MemorizeOK.Visibility = Visibility.Collapsed;
                        OkButton.Visibility = Visibility.Collapsed;
                        WordArea.Visibility = Visibility.Collapsed;
                        TargetArea.Visibility = Visibility.Collapsed;
                        CorrectImage.Visibility = Visibility.Collapsed;
                        ErrorImage.Visibility = Visibility.Collapsed;
                        CorrectTextBlock.Visibility = Visibility.Collapsed;
                        ErrorTextBlock.Visibility = Visibility.Collapsed;
                        Button_1.Visibility = Visibility.Visible;
                        Button_1.IsEnabled = true;
                        Button_2.Content = "试玩";
                        Button_2.Margin = new Thickness(550, 850, 0, 0);
                        Button_3.Margin = new Thickness(911, 850, 0, 0);

                        await OnVoicePlayAsync(Text_2.Text);
                    }
                    break;
                case 2:
                    {
                        // 进入试玩界面
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;

                        // 显示试玩部分的控件
                        //CountdownDisplay.Visibility = Visibility.Visible;
                        WordsToMemorize.Visibility = Visibility.Visible;
                        MemorizeOK.Visibility = Visibility.Visible;
                        OkButton.Visibility = Visibility.Collapsed;
                        WordArea.Visibility = Visibility.Visible;
                        //TargetArea.Visibility = Visibility.Visible;
                        //CorrectImage.Visibility = Visibility.Visible;
                        //ErrorImage.Visibility = Visibility.Visible;
                        //CorrectTextBlock.Visibility = Visibility.Visible;
                        //ErrorTextBlock.Visibility = Visibility.Visible;
                        // 隐藏讲解部分的按钮
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Visible;
                        Button_3.Margin = new Thickness(770, 850, 0, 0);

                        // 强制焦点保持在窗口
                        this.Focus();

                        //LJN，在讲解模块调用委托
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("请记住记忆阶段屏幕上所出现的词语，记忆完成后，按下键盘上OK键。随后在屏幕上会有一系列词语从左往右移动，当您看到与记忆阶段所记住的词语相匹配时并移动至方框内时，请按下键盘上的OK键。");//增加代码，调用函数，显示数字人下的文字
                        //LJN
                    }
                    break;
            }
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
