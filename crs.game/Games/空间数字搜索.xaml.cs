using crs.core;
using crs.core.DbModels;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace crs.game.Games
{
    /// <summary>
    /// 空间数字搜索.xaml 的交互逻辑
    /// </summary>
    public partial class 空间数字搜索 : BaseUserControl
    {
        private List<int> numbers;
        private int lastClickedNumber;
        private Stopwatch stopwatch;
        private DateTime startTime; // 记录表盘显示的开始时间
        private double[] timeIntervals; // 存储时间间隔
        private int a;
        private int wrongAccount; // 记录错误计数
        private int maxConsecutiveNumber; // 记录最长连续数字串的最大值
        private Brush defaultButtonBackground; // 存储按钮的默认背景颜色
        private DispatcherTimer gameTimer; // 计时器
        private TimeSpan totalGameTime; // 总游戏时间
        private double averagetime; // 新增变量，用于存储平均反应时间
        private TimeSpan previousTime;  // 记录上次正确点击的时间
        private double leftTimes = 0;
        private double rightTimes = 0;
        private double[] reactionTimes; // 更改地方：增加统计量用于存储每个数字的反应时间
        private double Zvalue;//23个数字反应时间差值

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            totalGameTime = totalGameTime.Add(TimeSpan.FromSeconds(1)); // 每次增加一秒
            int totalSeconds = (int)totalGameTime.TotalSeconds;

            // 调用委托
            TimeStatisticsAction?.Invoke(totalSeconds, totalSeconds);

        }

        public 空间数字搜索()
        {
            InitializeComponent();
            previousTime = TimeSpan.Zero;   // 初始设置为零
        }

        private void InitializeNumberGrid()
        {
            numbers = Enumerable.Range(1, 24).ToList();
            Random rand = new Random();
            numbers = numbers.OrderBy(x => rand.Next()).ToList(); // 打乱顺序
            foreach (var number in numbers)
            {
                Button button = new Button
                {
                    BorderThickness = new Thickness(0),
                    Content = number,
                    FontWeight = FontWeights.Bold, // 设置字体加粗
                    FontSize = 32,
                    Margin = new Thickness(5),
                    Style = CreateCustomButtonStyle(),
                    Background = defaultButtonBackground // 设置按钮的初始背景颜色
                };
                button.Click += NumberButton_Click;
                NumberGrid.Children.Add(button);
            }
            Run textPart1 = new Run("请找出: ")
            {
                Foreground = new SolidColorBrush(Colors.Black)
            };

            // 创建并添加橙色文本
            Run textPart2 = new Run((maxConsecutiveNumber + 1).ToString())
            {
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB346"))
            };

            // 清空当前的 Inlines
            tipblock.Inlines.Clear();

            // 将两个部分添加到 TextBlock
            tipblock.Inlines.Add(textPart1);
            tipblock.Inlines.Add(textPart2);

        }

        private async void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            int clickedNumber = Convert.ToInt32(clickedButton.Content);
            if (clickedNumber < maxConsecutiveNumber + 1)
                return;
            TimeSpan reactionTime;
            if (maxConsecutiveNumber == 0 && clickedNumber == maxConsecutiveNumber + 1)
            {
                TimeSpan timeSinceStart = DateTime.Now - startTime;
                timeIntervals[0] = timeSinceStart.TotalSeconds;
                reactionTimes[0] = timeSinceStart.TotalMilliseconds; //   更改地方：以毫秒为单位记录reactionTimes反应时间

                a = 0;
                maxConsecutiveNumber++;
                clickedButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB346"));
                clickedButton.Foreground = Brushes.White; // 设置前景色为白色
                stopwatch.Restart();
            }
            else
            {

                if (clickedNumber == maxConsecutiveNumber + 1)
                {
                    TimeSpan timeInterval = stopwatch.Elapsed;
                    // 存储时间间隔
                    timeIntervals[maxConsecutiveNumber - 1] = timeInterval.TotalSeconds; // 存储到对应索引

                    reactionTimes[clickedNumber - 1] = timeInterval.TotalMilliseconds; // 更改地方：添加 reactionTimes数组保存每个间隔时间，以毫秒为单位记录反应时间

                    maxConsecutiveNumber++;
                    clickedButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB346"));
                    clickedButton.Foreground = Brushes.White; // 设置前景色为白色
                                                              // Debug.WriteLine($"数字 {clickedNumber} 的反应时间: {reactionTimes[clickedNumber - 1]} 毫秒");

                    stopwatch.Restart();

                    if (maxConsecutiveNumber > 1)
                    {
                        a = maxConsecutiveNumber - 1; // 更新 a 为时间间隔的数量
                    }



                    // 检查是否所有数字都已被点击
                    if (maxConsecutiveNumber == 24)
                    {
                        string timeIntervalsMessage = "时间间隔:\n";
                        string reactionTimesMessage = "反应时间:\n"; // 更改地方：新增反应时间输出

                        // 遍历 timeIntervals 数组并构建消息字符串
                        for (int i = 0; i < timeIntervals.Length; i++)
                        {
                            timeIntervalsMessage += $"第 {i + 1} 个数字: {timeIntervals[i]} 毫秒\n";
                            reactionTimesMessage += $"第 {i + 1} 个数字反应时间: {reactionTimes[i]} 毫秒\n"; // 更改地方：新增反应时间输出
                            Debug.WriteLine($"第 {i + 1} 个数字反应时间: {reactionTimes[i]} 毫秒");

                        }

                        // 显示 MessageBox
                        //MessageBox.Show(timeIntervalsMessage, "时间间隔统计");
                        //空间数字搜索报告 nwd = new 空间数字搜索报告(wrongAccount, timeIntervals);
                        //nwd.Show();
                        stopwatch.Stop();
                        OnGameEnd();
                    }
                }
                else
                {
                    // 增加错误计数
                    wrongAccount++;
                    clickedButton.Background = Brushes.Black; // 设置按钮背景为黑色

                    // 等待0.5秒后恢复颜色
                    await Task.Delay(500); // 等待500毫秒
                    clickedButton.Background = defaultButtonBackground; // 恢复按钮背景为默认颜色
                    clickedButton.IsEnabled = true; // 重新启用按钮
                    //Debug.WriteLine($"错误点击了数字 {clickedNumber}，继续计时...");
                }
            }
            Run textPart1 = new Run("请找出: ")
            {
                Foreground = new SolidColorBrush(Colors.Black)
            };

            // 创建并添加橙色文本
            Run textPart2 = new Run((maxConsecutiveNumber + 1).ToString())
            {
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB346"))
            };

            // 清空当前的 Inlines
            tipblock.Inlines.Clear();

            // 将两个部分添加到 TextBlock
            tipblock.Inlines.Add(textPart1);
            tipblock.Inlines.Add(textPart2);
            RightStatisticsAction?.Invoke(maxConsecutiveNumber, 24);
            WrongStatisticsAction?.Invoke(wrongAccount, 24);
            // 更新状态
            lastClickedNumber = clickedNumber;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            stopwatch.Stop();
            OnGameEnd();
        }
    }
    public partial class 空间数字搜索 : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {

            lastClickedNumber = 0; // 初始化为0，表示未点击
            timeIntervals = new double[24];
            wrongAccount = 0;
            maxConsecutiveNumber = 0; // 初始化最大连续数字串为0
            defaultButtonBackground = Brushes.White; // 设置默认背景颜色
            LevelStatisticsAction?.Invoke(0, 0);
            RightStatisticsAction?.Invoke(0, 10);
            WrongStatisticsAction?.Invoke(0, 10);
            // 初始化 averagetime
            averagetime = 0.0;
            Zvalue = 0.0;
            totalGameTime = TimeSpan.Zero; // 重置总时间
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1); // 每秒更新一次
            gameTimer.Tick += GameTimer_Tick; // 绑定计时器事件

            reactionTimes = new double[24]; // 初始化存储24个数字反应时间的数组
        }

        protected override async Task OnStartAsync()
        {

            gameTimer.Start(); // 启动计时器
            InitializeNumberGrid();

            stopwatch = new Stopwatch();
            startTime = DateTime.Now; // 记录表盘显示的开始时间

            CustomCursor.Visibility = Visibility.Visible;//调用自定义鼠标光标
            this.Cursor = Cursors.None; // 隐藏默认光标
            CustomCursor.Width = 65; // 调整为你想要的宽度
            CustomCursor.Height = 65; // 调整为你想要的高度
            // 绑定鼠标移动事件
            this.MouseMove += Game_MouseMove;


            // 调用委托
            VoiceTipAction?.Invoke("请您用鼠标按照数字的顺序，依次用鼠标点击这些方块。");
            SynopsisAction?.Invoke("现在您看到顺序打乱的数字，请您使用鼠标按照顺序点击数字，点击这些数字速度越快越好！");
            RuleAction?.Invoke("现在您看到顺序打乱的数字，请您使用鼠标按照顺序点击数字，点击这些数字速度越快越好！");//增加代码，调用函数，显示数字人下的文字
        }


        private void Game_MouseMove(object sender, MouseEventArgs e)//绑定自定义鼠标光标和默认鼠标光标
        {
            var position = e.GetPosition(CursorCanvas);
            Canvas.SetLeft(CustomCursor, position.X - CustomCursor.Width / 2);
            Canvas.SetTop(CustomCursor, position.Y - CustomCursor.Height / 2);
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
            stopwatch.Stop();
            gameTimer?.Stop();
            this.Cursor = Cursors.Arrow; // 恢复默认光标
            CustomCursor.Visibility = Visibility.Collapsed; // 隐藏自定义光标
        }

        protected override async Task OnPauseAsync()
        {
            stopwatch.Stop();
            gameTimer?.Stop();
        }

        protected override async Task OnNextAsync()
        {

            // 调用委托
            VoiceTipAction?.Invoke("请您用鼠标按照数字的顺序，依次用鼠标点击这些方块。");
            SynopsisAction?.Invoke("现在您看到顺序打乱的数字，请您使用鼠标按照顺序点击数字，点击这些数字速度越快越好！");
            RuleAction?.Invoke("现在您看到顺序打乱的数字，请您使用鼠标按照顺序点击数字，点击这些数字速度越快越好！");
        }//增加代码，调用函数，显示数字人下的文字

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();

        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new 空间数字搜索讲解();
        }

        private int GetCorrectNum()
        {
            return 0;
        }
        private int GetWrongNum()
        {
            return wrongAccount;
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

        /*private async Task updateDataAsync(int program_id)
        {
            using (Crs_Db2Context db = new Crs_Db2Context())
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    // 获取当前难度级别的数据
                    int correctCount = GetCorrectNum();
                    int wrongCount = GetWrongNum();
                    int ignoreCount = GetIgnoreNum();

                    // 计算准确率
                    double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);
                    // 创建 Result 记录
                    var newResult = new Result
                    {
                        ProgramId = program_id,
                        Report = "空间数字搜索能力评估报告",
                        Eval = true,
                        Lv = null,
                        ScheduleId = BaseParameter.ScheduleId ?? null
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
                    ValueName = "错误次数",
                    Value = wrongCount,
                    ModuleId = BaseParameter.ModuleId
                }
            };

                    // 插入 ResultDetail 数据
                    db.ResultDetails.AddRange(resultDetails);
                    await db.SaveChangesAsync();

                    foreach (var detail in resultDetails)
                    {
                        Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId}");
                    }

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
        }*/
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
        private async Task updateDataAsync()
        {
            var baseParameter = BaseParameter;

            var program_id = baseParameter.ProgramId;
            Crs_Db2Context db = baseParameter.Db;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    int correctCount = GetCorrectNum();
                    int wrongCount = GetWrongNum();
                    int ignoreCount = GetIgnoreNum();
                    double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);
                    db.Database.SetCommandTimeout(TimeSpan.FromMinutes(8)); // 根据需要调整
                    double sum = 0;

                    for (int ii = 0; ii < a; ii++)
                    {
                        sum += timeIntervals[ii]; // 累加有效的时间间隔
                    }
                    averagetime = (sum / 24) * 1000; ; // 计算平均反应时间（单位：毫秒）
                    double Zvalueworkspeed = (2235 - averagetime) / 530;
                    for (int ii = 0; ii < a - 1; ii++)
                    {
                        Zvalue += Math.Abs((timeIntervals[ii + 1] - timeIntervals[ii]));
                    }
                    Zvalue = (Zvalue * 1000) / 23;
                    for (int ii = 0; ii < 6; ii++)
                    {
                        leftTimes += (timeIntervals[numbers[(4 * ii)] - 1] + timeIntervals[numbers[(4 * ii + 1)] - 1]);
                        rightTimes += (timeIntervals[numbers[(4 * ii + 2)] - 1] + timeIntervals[numbers[(4 * ii + 3)] - 1]);
                    }
                    leftTimes /= 12;
                    rightTimes /= 12;

                    var newResult = new Result
                    {
                        ProgramId = program_id,
                        Report = "空间数字搜索能力评估报告",
                        Eval = true,
                        Lv = null,
                        ScheduleId = BaseParameter.ScheduleId ?? null
                    };
                    db.Results.Add(newResult);
                    await db.SaveChangesAsync();

                    int result_id = newResult.ResultId;

                    var resultDetails = reactionTimes.Select((value, index) => new ResultDetail
                    {
                        ResultId = result_id,
                        ValueName = "寻找的数字,时间[ms]",
                        Charttype = "折线图",
                        Value = Math.Round(value, 2),
                        Abscissa = index + 1,
                        ModuleId = BaseParameter.ModuleId,
                    }).ToList();
                    resultDetails.Add(new ResultDetail
                    {
                        ResultId = result_id,
                        ValueName = "Z值工作速度",
                        Value = Math.Round(Zvalueworkspeed, 2), // 平均时间
                        ModuleId = BaseParameter.ModuleId
                    });
                    resultDetails.Add(new ResultDetail
                    {
                        ResultId = result_id,
                        ValueName = "平均反应时间",
                        Value = Math.Round(averagetime, 2), // 平均时间
                        ModuleId = BaseParameter.ModuleId
                    });
                    resultDetails.Add(new ResultDetail
                    {
                        ResultId = result_id,
                        ValueName = "左平均反应时间",
                        Value = Math.Round(leftTimes * 1000, 2), // 左平均时间
                        ModuleId = BaseParameter.ModuleId
                    });
                    resultDetails.Add(new ResultDetail
                    {
                        ResultId = result_id,
                        ValueName = "右平均反应时间",
                        Value = Math.Round(rightTimes * 1000, 2), // 右平均时间
                        ModuleId = BaseParameter.ModuleId
                    });
                    resultDetails.Add(new ResultDetail
                    {
                        ResultId = result_id,
                        ValueName = "23个差值平均(ms)",
                        Value = Math.Round(Zvalue, 2),
                        ModuleId = BaseParameter.ModuleId
                    });
                    resultDetails.Add(new ResultDetail
                    {
                        ResultId = result_id,
                        ValueName = "错误次数",
                        Value = wrongCount,
                        ModuleId = BaseParameter.ModuleId
                    });
                    db.ResultDetails.AddRange(resultDetails);
                    db.SaveChanges();

                    foreach (var detail in resultDetails)
                    {
                        Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId}");
                    }

                    transaction.Commit();
                    Debug.WriteLine("插入成功");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Debug.WriteLine(ex.ToString());
                }
            }
        }

    }

}

