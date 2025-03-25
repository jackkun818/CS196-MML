using crs.core.DbModels;
using crs.core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace crs.game.Games
{
    /// <summary>
    /// 记忆广度.xaml 的交互逻辑
    /// </summary>
    public partial class 记忆广度 : BaseUserControl
    {

        /*20241110新需求
         现在最高等级需要加到9，即记忆广度最高达到需要记忆9个方格，公司现在要求加上升级降级，每个等级两道题，需要连续作对两道才能到下一等级。
        如果在该等级中的两题无论前一道还是后一道做错，都得退回上一个等级，即做错一题立刻降级。
        公司现在要求结束的条件是连续做错两题，意思是连错两题直接结束评估。
        */
        private const int GridSize = 5;
        private const int MAX_BLOCKS = 9; // 最大方块数量
        private const double DELAY = 1.0; // 两相邻方块展示时间间隔
        private const int TOTAL_ROUNDS_PER_BLOCK = 2; // 每种方块数量的总轮数
        private List<Button> buttons = new List<Button>();
        private List<int> sequence = new List<int>();
        private List<int> selectedIndices = new List<int>(); // 记录玩家选中的方块索引
        private int currentBlockCount; // 当前展示的方块数量
        private int currentRound = 1; // 当前轮次
        private Stopwatch stopwatch = new Stopwatch();
        private DispatcherTimer countdownTimer; // 倒计时定时器
        private List<DispatcherTimer> sequenceTimers = new List<DispatcherTimer>(); // 存储展示方块的定时器
        private Dictionary<int, int> errorCounts; // 记录每个方块数量的错误次数
        // private Dictionary<int, int> correctCounts; // 记录每个方块数量的正确次数
        private bool isShowingSequence; // 是否正在展示方块
        private DispatcherTimer gameTimer; // 计时器
        private TimeSpan totalGameTime; // 总游戏时间

        //LJN,添加一些柱状图统计量
        private string DateNow = DateTime.Now.ToString("yyyy/M/d"); // 获取当前日期
        private int[] CorrectNums = new int[MAX_BLOCKS-1];//不同记忆广度的对应的正确数量，如[15,4,2,1]对应记忆广度2的时候答对15个，记忆广度3的时候答对4个，即CorrectNums[0]对应的是记忆广度2的正确个数，CorrectNums[1]对应的是记忆广度3的正确个数；MAX_BLOCKS-1是因为记忆广度从2开始，当MAX_BLOCKS为4的时候，一共有2 3 4三个记忆广度，计算单位是题目数
        private int MemorySpan = 2;//记忆广度最大到多少，默认从2开始
        private int IfEverWrong = 0;//用来记录连续错误的次数，连错两题直接结束评估
        private int[] PositionErrorNums = new int[MAX_BLOCKS - 1];//位置错误，位置错误就是你点了记忆显示方块之外的方块算位置错误，计算单位是题目数
        private int[] OrrderErrorNums = new int[MAX_BLOCKS - 1];//顺序错误，顺序错误是你点的方块都是他显示要点的方块，但是顺序错了，计算单位是题目数
        public 记忆广度()
        {
            InitializeComponent();
        }

        private void InitializeGrid()
        {
            GameGrid.Children.Clear();
            buttons.Clear(); // 同时清空按钮列表
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    Button button = new Button
                    {
                        //LJN,修改颜色
                        Background = Brushes.White, // 设置初始背景颜色为灰色
                        //LJN,修改间距2->5
                        Margin = new Thickness(5),
                        FontSize = 24, // 设置默认字体大小
                        Content = "", // 初始化按钮内容为空
                       //LJN,应用自定义样式
                        Style = CreateCustomButtonStyle() // 应用自定义样式
                    };
                    button.Click += Button_Click;
                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);
                    GameGrid.Children.Add(button);
                    buttons.Add(button);
                }
            }
        }
        //将所有方块设置为灰色,清除按钮内容
        private void ResetGridGray()
        {
            foreach (var item in buttons)
            {
                //LJN,修改成白色
                item.Background = Brushes.White;
                item.Content = "";
            }
        }

        private void StartGame()
        {
            sequence.Clear();
            ResetGridGray();
            isShowingSequence = true;
            StatusTextBlock.Text = "准备开始..."; // 设置准备开始的文本
            StartCountdown();
            //LJN,用来显示图片
            CreateImage();
            OnGameStart();
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            totalGameTime = totalGameTime.Add(TimeSpan.FromSeconds(1)); // 每次增加一秒// 获取总秒数
            int totalSeconds = (int)totalGameTime.TotalSeconds;

            // 调用委托
            TimeStatisticsAction?.Invoke(totalSeconds, totalSeconds);

        }

        private void StartCountdown()
        {
            countdownTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            int countdownTime = 5; // 5秒倒计时
            countdownTimer.Tick += (s, args) =>
            {
                if (countdownTime > 0)
                {
                    //LJN,倒计时开始前，使得文字显示出来
                    StatusTextBlock.Visibility = Visibility.Visible;
                    StatusTextBlock.Text = $"倒计时: {countdownTime}秒, 当前展示 {currentBlockCount} 个方块, 第{currentRound}轮";
                    countdownTime--;
                }
                else
                {
                    countdownTimer.Stop();
                    ShowNextRound();
                }
            };
            countdownTimer.Start();
        }

        private void ShowNextRound()
        {
            int numberToShow = currentBlockCount;
            sequence.Clear();
            Random rand = new Random();
            HashSet<int> shownIndices = new HashSet<int>();

            for (int i = 0; i < numberToShow; i++)
            {
                int index;
                do
                {
                    index = rand.Next(buttons.Count);
                } while (shownIndices.Contains(index));

                shownIndices.Add(index);
                sequence.Add(index);
            }
            stopwatch.Restart();
            ShowSequence(0);
        }
        private void OnGameStart()//调用自定义鼠标光标函数
        {
            this.Cursor = Cursors.None; // 隐藏默认光标
            CustomCursor.Visibility = Visibility.Visible; // 显示自定义光标
            MouseMove += Window_MouseMove; // 订阅 MouseMove 事件
            CustomCursor.Width = 65; // 调整为你想要的宽度
            CustomCursor.Height = 65; // 调整为你想要的高度
        }
     
        private void ShowSequence(int index)
        {
            if (index < sequence.Count)
            {
                int buttonIndex = sequence[index];
                buttons[buttonIndex].Background = Brushes.Red;

                DispatcherTimer timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(DELAY)
                };
                timer.Tick += (s, args) =>
                {
                    //LJN,随着倒计时，button要变回白色而不是灰色
                    buttons[buttonIndex].Background = Brushes.White; // 隐藏方块
                    timer.Stop();
                    sequenceTimers.Remove(timer); // 移除定时器
                    ShowSequence(index + 1); // 展示下一个方块
                };
                timer.Start();
                sequenceTimers.Add(timer); // 添加到列表中
            }
            else
            {
                // 所有方块展示完毕，提示用户
                isShowingSequence = false;
                StatusTextBlock.Text = "现在请依次按下对应方块";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (isShowingSequence)
            {
                return; // 如果正在展示方块，直接返回，不处理点击事件
            }

            Button clickedButton = sender as Button;
            int clickedIndex = buttons.IndexOf(clickedButton);

            // 如果按钮已经被选中，则取消选中
            if (selectedIndices.Contains(clickedIndex))
            {
                selectedIndices.Remove(clickedIndex);
                //LJN,恢复为白色
                clickedButton.Background = Brushes.White; // 恢复为灰色
                clickedButton.Content = ""; // 清空内容
            }
            else
            {
                // 添加选中的方块
                selectedIndices.Add(clickedIndex);
                //LJN,选中方块后变为橙色
                clickedButton.Background = Brushes.Orange; // 变为红色
                clickedButton.Content = (selectedIndices.Count).ToString(); // 显示当前点击的方块数
                clickedButton.Foreground = Brushes.White; // 设置字体颜色为白色
                clickedButton.FontSize = 36; // 增加字体大小
            }

            // 判断选中的方块数量
            if (selectedIndices.Count == sequence.Count)
            {
                int errors = 0; // 计算错误数量
                isShowingSequence = true;
                // 清理选择的方块并提供反馈
                for (int i = 0; i < sequence.Count; i++)
                {
                    if (sequence[i] == selectedIndices[i])
                    {
                        buttons[selectedIndices[i]].Background = Brushes.Green; // 正确的选项
                        buttons[selectedIndices[i]].Content = "✔"; // 显示正确标记

                    }
                    else
                    {
                        buttons[selectedIndices[i]].Background = Brushes.Red; // 错误的选项
                        buttons[selectedIndices[i]].Content = "✖"; // 显示错误标记
                        errorCounts[currentBlockCount]++;
                        errors++;                      
                    }
                }
                if(errors!=0)//说明有错误，来判断这道题的错误是什么类型的错误
                {
                    if (new HashSet<int>(sequence).SetEquals(selectedIndices))
                    {//判断错误信息，比较两个数组
                        //如果两个数组所包含的元素一模一样，说明是顺序错误
                        OrrderErrorNums[currentBlockCount - 2] += 1;
                    }
                    else
                    {//否则只能是位置错误了
                        PositionErrorNums[currentBlockCount - 2] += 1;
                    }
                }

                // 显示反馈信息
                string feedbackText;
                SolidColorBrush feedbackColor;

                if (errors == 0)
                {
                    feedbackText = "正确！";
                    feedbackColor = Brushes.Green; // 正确为绿色
                    //LJN,显示图片，隐藏文字
                    CorrectImage.Visibility = Visibility.Visible;
                    StatusTextBlock.Visibility = Visibility.Collapsed;
                    //LJN,记录各个广度下的正确的数量个数
                    CorrectNums[currentBlockCount - 2] += 1;
                }
                else
                {
                    feedbackText = "错误！";
                    feedbackColor = Brushes.Red; // 错误为红色
                    //LJN,显示图片，隐藏文字
                    ErrorImage.Visibility = Visibility.Visible;
                    StatusTextBlock.Visibility = Visibility.Collapsed;
                }

                // 更新状态文本
                StatusTextBlock.Text = feedbackText;
                StatusTextBlock.Foreground = feedbackColor; // 设置字体颜色

                selectedIndices.Clear(); // 清空选中的索引

                // 创建计时器以恢复状态文本颜色
                DispatcherTimer resetTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(3) // 3秒后恢复
                };
                resetTimer.Tick += (s, args) =>
                {
                    foreach (var button in buttons)
                    {
                        //LJN,恢复回白色
                        button.Background = Brushes.White; // 恢复为灰色
                        button.Content = ""; // 清空内容
                        button.Foreground = Brushes.Black; // 将字体颜色恢复为黑色
                    }
                    StatusTextBlock.Foreground = Brushes.Black; // 恢复字体颜色为黑色
                    resetTimer.Stop(); // 停止计时器
                    resetTimer = null; // 清空计时器引用
                    StatusTextBlock.Text = "";
                    //LJN,把图片反馈也给隐藏掉
                    ErrorImage.Visibility = Visibility.Collapsed;
                    CorrectImage.Visibility = Visibility.Collapsed;

                    // 继续游戏逻辑
                    currentRound++; // 增加当前轮次
                    isShowingSequence = true;
                    if(errors!=0)//如果这道题发生过错误，那么立即回退上一等级
                    {
                        currentRound = 1; // 重置当前轮次

                        IfEverWrong++;//这个是用来记录是否连续做错
                        if(IfEverWrong>=2)
                        {//连错两道，直接结束游戏
                         // 游戏结束，显示错误统计
                         //记忆广度报告 nwd = new 记忆广度报告(errorCounts);
                         //nwd.Show();
                            StopAllTimers();
                            //LJN,游戏结束前，更新记忆广度的值
                            MemorySpan = FindMaxMemorySpan();



                            OnGameEnd();
                        }
                        currentBlockCount--;                  //方块数量回退

                        if (currentBlockCount<2) currentBlockCount = 2;//作最小值限制
                        StartCountdown(); // 进行下一轮
                    }
                    else if (currentRound <= TOTAL_ROUNDS_PER_BLOCK)//当前轮次小于需要的轮次，同时还没发生过错误
                    {
                        if (IfEverWrong > 0) IfEverWrong = 0;//正常答对进入到下一轮应该将连错标志位置零
                        StartCountdown(); // 进行下一轮
                    }
                    else
                    {
                        currentRound = 1; // 重置当前轮次
                        currentBlockCount++; // 增加方块数量
                        if (currentBlockCount <= MAX_BLOCKS)
                        {
                            if (IfEverWrong > 0) IfEverWrong = 0;//正常答对进入到下一轮应该将连错标志位置零
                            StartCountdown(); // 进行下一轮的倒计时
                        }
                        else
                        {
                            // 游戏结束，显示错误统计
                            //记忆广度报告 nwd = new 记忆广度报告(errorCounts);
                            //nwd.Show();
                            StopAllTimers();

                            //LJN,游戏结束前，更新记忆广度的值
                            MemorySpan = FindMaxMemorySpan();

                            OnGameEnd();

                        }
                    }
                };
                resetTimer.Start(); // 启动计时器
            }
        }

        private void StopAllTimers()
        {
            // 停止倒计时定时器
            countdownTimer?.Stop();
            gameTimer?.Stop();
            // 停止所有展示方块的定时器
            foreach (var timer in sequenceTimers)
            {
                timer.Stop();
            }
            sequenceTimers.Clear(); // 清空定时器列表
        }

        //LJN,加入一些函数，样式
        private Style CreateCustomButtonStyle()
        {//包装好一个样式
            // 创建按钮样式
            Style buttonStyle = new Style(typeof(Button));

            // 设置背景为白色
            buttonStyle.Setters.Add(new Setter(Button.BackgroundProperty, Brushes.White));

            // 设置阴影效果
            ////LJN,加入阴影效果
            //Effect = new DropShadowEffect
            //{
            //    Color = Colors.Gray,      // 阴影颜色
            //    BlurRadius = 10,          // 模糊半径
            //    ShadowDepth = 5,          // 阴影深度
            //    Direction = 315,          // 阴影方向，角度
            //    Opacity = 0.5             // 阴影透明度
            //},
            buttonStyle.Setters.Add(new Setter(Button.EffectProperty, new DropShadowEffect
            {
                Color = Colors.Gray,
                BlurRadius = 10,
                ShadowDepth = 5,
                Direction = 315,
                Opacity = 0.5
            }));

            // 自定义模板，移除鼠标悬停时的默认视觉变化
            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            border.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Button.BorderBrushProperty));
            border.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Button.BorderThicknessProperty));

            ////LJN,取消长宽的设定，保持填充
            //HorizontalAlignment = HorizontalAlignment.Stretch,  // 设置水平填充
            //VerticalAlignment = VerticalAlignment.Stretch,   // 设置垂直填充
            FrameworkElementFactory contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            border.AppendChild(contentPresenter);
            template.VisualTree = border;

            ////LJN,使得鼠标移动到上面的时候不该变button的的颜色
            //FocusVisualStyle = null
            // 触发器，确保鼠标悬停时背景保持不变
            System.Windows.Trigger isMouseOverTrigger = new System.Windows.Trigger { Property = Button.IsMouseOverProperty, Value = true };
            isMouseOverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, Brushes.White));
            template.Triggers.Add(isMouseOverTrigger);

            // 将模板设置到样式
            buttonStyle.Setters.Add(new Setter(Button.TemplateProperty, template));

            return buttonStyle;
        }
        //LJN,加入一些函数，样式
        private void CreateImage()
        {//创建两张图片，从本地的JIYI文件夹读取
            //首先获取图片的完整路径
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;            // 获取当前项目的根目录(即bin目录)
            //@ 符号用于定义逐字字符串（verbatim string），使字符串中的特殊字符（如反斜杠 \）被原样解释，不再需要转义。

            // 构建图片的相对路径
            string correctRelativePath = @"Games\pic\JIYI\Correct.png";
            string errorRelativePath = @"Games\pic\JIYI\Error.png";

            // 使用 Path.Combine 来拼接绝对路径
            string correctImagePath = System.IO.Path.Combine(currentDirectory, correctRelativePath);
            string errorImagePath = System.IO.Path.Combine(currentDirectory, errorRelativePath);
            CorrectImage.Source = new BitmapImage(new Uri(correctImagePath, UriKind.RelativeOrAbsolute));
            ErrorImage.Source = new BitmapImage(new Uri(errorImagePath, UriKind.RelativeOrAbsolute));
            ErrorImage.Visibility = Visibility.Collapsed;
            CorrectImage.Visibility = Visibility.Collapsed;
        }

        private int FindMaxMemorySpan()
        {
            int maxIndex = -1;
            for (int i = 0; i < CorrectNums.Length; i++)
            {
                if (CorrectNums[i] > 0) // 判断是否有结果
                {
                    maxIndex = i; // 更新最大索引
                }
            }

            if (maxIndex == -1)
            {
                // 如果没有任何值大于0，返回null表示无有效结果
                return 2;
            }

            // 记忆广度从2开始，因此索引需要加2
            return maxIndex + 2;
        }
    }
    public partial class 记忆广度 : BaseUserControl
    {

        protected override async Task OnInitAsync()
        {
            currentBlockCount = 2; // 从2个方块开始
            InitializeGrid();
            errorCounts = new Dictionary<int, int>(); // 初始化错误次数字典

            for (int i = 2; i <= MAX_BLOCKS; i++)
            {
                errorCounts[i] = 0; // 初始化为 0
            }
            // 调用委托
            LevelStatisticsAction?.Invoke(currentBlockCount, MAX_BLOCKS);
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
            StartGame();
            // 调用委托
            VoiceTipAction?.Invoke("请在方块隐藏后按顺序依次点击方块。");
            SynopsisAction?.Invoke("现在您看到5×5的方块，然后会按照顺序显示方块，请您进行记忆，在方块隐藏后按顺序依次点击方块");
            RuleAction?.Invoke("现在您看到5×5的方块，然后会按照顺序显示方块，请您进行记忆，在方块隐藏后按顺序依次点击方块");//增加代码，调用函数，显示数字人下的文字
        }
        private void Window_MouseMove(object sender, MouseEventArgs e)//绑定自定义鼠标光标和默认鼠标光标
        {
            Point position = e.GetPosition(this);
            Canvas.SetLeft(CustomCursor, position.X - (CustomCursor.Width / 2));
            Canvas.SetTop(CustomCursor, position.Y - (CustomCursor.Height / 2));
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


        protected override async Task OnStopAsync()
        {
            this.Cursor = Cursors.Arrow; // 恢复默认光标
            CustomCursor.Visibility = Visibility.Collapsed; // 隐藏自定义光标
            MouseMove -= Window_MouseMove; // 取消订阅 MouseMove 事件

            MemorySpan = FindMaxMemorySpan();


            StopAllTimers();
        }

        protected override async Task OnPauseAsync()
        {
            StopAllTimers();
        }

        protected override async Task OnNextAsync()
        {
            // 调用委托
            VoiceTipAction?.Invoke("请在方块隐藏后按顺序依次点击方块。");
            SynopsisAction?.Invoke("现在您看到5×5的方块，然后会按照顺序显示方块，请您进行记忆，在方块隐藏后按顺序依次点击方块");
            RuleAction?.Invoke("现在您看到5×5的方块，然后会按照顺序显示方块，请您进行记忆，在方块隐藏后按顺序依次点击方块");//增加代码，调用函数，显示数字人下的文字
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new 记忆广度讲解();
        }

        private int GetCorrectNum()
        {
            // 18 ？？？

            //return 2* currentBlockCount-errorCounts[currentBlockCount];
            //return correctCounts.Values.Sum();
            return CorrectNums.Sum();
        }
        private int GetWrongNum()
        {
            return errorCounts.Values.Sum();
        }
        private int GetIgnoreNum()
        {
            return 0;
        }

        private int GetPosErrorNum()
        {
            return PositionErrorNums.Sum();
        }

        private int GetOrderErrorNum()
        {
            return OrrderErrorNums.Sum();
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
                        int positionErrorCount = GetPosErrorNum();
                        int orderErrorCount = GetOrderErrorNum();
                        double Zvalue = (MemorySpan - 3.1) / 0.8;
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
                                    Order = 1,
                                    ValueName = "最小等级",
                                    Value = 2,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 2,
                                    ValueName = "最大等级",
                                    Value =  MemorySpan ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 3,
                                    ValueName = "正确",
                                    Value = correctCount , // 以百分比形式存储
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "位置错误",
                                    Value = positionErrorCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "顺序错误",
                                    Value = orderErrorCount,
                                    ModuleId = BaseParameter.ModuleId
                                } ,
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Z值记忆广度",
                                    Value = Math.Round(Zvalue, 2),
                                    ModuleId = BaseParameter.ModuleId
                                }
                            };

                        resultDetails.AddRange(CorrectNums.Select((value, index) => new ResultDetail
                        {
                            ResultId = result_id,
                            ValueName = "记忆广度,正确",
                            Value = value,
                            Abscissa = index + 2,
                            Charttype = "柱状图",
                            ModuleId = BaseParameter.ModuleId
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
