using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace crs.game.Games
{
    /// <summary>
    /// 视觉修复训练讲解.xaml 的交互逻辑
    /// </summary>
    public partial class 视觉修复训练讲解 : BaseUserControl
    {
        private Polygon _triangle;
        private Rectangle _whiteSquare;
        private DispatcherTimer _shapeChangeTimer;
        private DispatcherTimer _revertTimer;
        private DispatcherTimer _squareAppearanceTimer;
        private DispatcherTimer _squareFlashTimer;
        private DispatcherTimer _endTrainingTimer;
        private bool _centerPointCorrect = false;
        private bool _squareCorrect = false;
        private DateTime _changeTime1;
        private DateTime _changeTime2;
        private Random _random;
        private bool _centerPointChecked = false; // 中心点是否已检查
        private bool _squareChecked = false; // 白色方形是否已检查
        private bool _centerPointMissed = false; // 是否错过了中心点按键
        private bool _squareMissed = false; // 是否错过了白色方形按键

        public Action GameBeginAction { get; set; }

        public Func<string, Task> VoicePlayFunc { get; set; }

        public 视觉修复训练讲解()
        {
            InitializeComponent();



            this.Loaded += 视觉修复训练讲解_Loaded;
            Focus();

        }

        private void 视觉修复训练讲解_Loaded(object sender, RoutedEventArgs e)
        {
            // 页面加载时确保按键和焦点行为
            Button_2_Click(null, null);
            Focus();  // 确保焦点在窗口上
        }

        private void InitializeTraining()
        {
            _random = new Random();

            // 创建并设置三角形，但初始状态为隐藏
            _triangle = new Polygon
            {
                Points = new PointCollection(new Point[]
                {
                    new Point(20, 0),  // 顶点
                    new Point(0, 34.64), // 左下角
                    new Point(40, 34.64) // 右下角
                }),
                Fill = new SolidColorBrush(Color.FromRgb(184, 134, 11)), // 暗黄色
                Visibility = Visibility.Hidden,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            MainGrid.Children.Add(_triangle);

            // 创建并设置白色正方形，但初始状态为隐藏
            _whiteSquare = new Rectangle
            {
                Width = 50,
                Height = 50,
                Fill = Brushes.White,
                Visibility = Visibility.Hidden
            };
            MainGrid.Children.Add(_whiteSquare);

            // 初始化定时器
            _shapeChangeTimer = new DispatcherTimer();
            _shapeChangeTimer.Interval = TimeSpan.FromSeconds(5); // 5秒后变形
            _shapeChangeTimer.Tick += ShapeChangeTimer_Tick;
            _shapeChangeTimer.Start();

            _squareAppearanceTimer = new DispatcherTimer();
            _squareAppearanceTimer.Interval = TimeSpan.FromSeconds(10); // 10秒后显示正方形
            _squareAppearanceTimer.Tick += SquareAppearanceTimer_Tick;
            _squareAppearanceTimer.Start();

            // 结束训练的定时器（12秒后）
            _endTrainingTimer = new DispatcherTimer();
            _endTrainingTimer.Interval = TimeSpan.FromSeconds(12);
            _endTrainingTimer.Tick += EndTrainingTimer_Tick;
            _endTrainingTimer.Start();

            // 监听键盘事件
            this.KeyDown += MainWindow_KeyDown;
            this.Focus();
        }

        private void ShapeChangeTimer_Tick(object sender, EventArgs e)
        {
            // 切换到三角形
            FocusPoint.Visibility = Visibility.Hidden;
            _triangle.Visibility = Visibility.Visible;
            _changeTime1 = DateTime.Now;

            // 启动定时器，在0.3秒后恢复圆形
            _revertTimer = new DispatcherTimer();
            _revertTimer.Interval = TimeSpan.FromSeconds(0.3);
            _revertTimer.Tick += RevertTimer_Tick;
            _revertTimer.Start();
            _shapeChangeTimer.Stop();

            // 设置一个定时器来标记错过中心点事件
            var missedCenterPointTimer = new DispatcherTimer();
            missedCenterPointTimer.Interval = TimeSpan.FromSeconds(1.5);
            missedCenterPointTimer.Tick += (s, args) =>
            {
                _centerPointMissed = true;
                missedCenterPointTimer.Stop();
            };
            missedCenterPointTimer.Start();
        }

        private void RevertTimer_Tick(object sender, EventArgs e)
        {
            // 恢复圆形
            FocusPoint.Visibility = Visibility.Visible;
            _triangle.Visibility = Visibility.Hidden;

            _revertTimer.Stop();
        }

        private void SquareAppearanceTimer_Tick(object sender, EventArgs e)
        {
            // 在右下方区域随机显示白色正方形
            double xPosition = _random.Next(0, 800);
            double yPosition = _random.Next(0, 800);
            _whiteSquare.Margin = new Thickness(200 + xPosition, 200 + yPosition, 0, 0); // 200像素偏移以适应窗口中央
            _whiteSquare.Visibility = Visibility.Visible;
            _changeTime2 = DateTime.Now;

            _squareFlashTimer = new DispatcherTimer();
            _squareFlashTimer.Interval = TimeSpan.FromSeconds(0.3);
            _squareFlashTimer.Tick += SquareFlashTimer_Tick;
            _squareFlashTimer.Start();
            _squareAppearanceTimer.Stop();

            // 设置一个定时器来标记错过白色方形事件
            var missedSquareTimer = new DispatcherTimer();
            missedSquareTimer.Interval = TimeSpan.FromSeconds(1.5);
            missedSquareTimer.Tick += (s, args) =>
            {
                _squareMissed = true;
                missedSquareTimer.Stop();
            };
            missedSquareTimer.Start();
        }

        private void SquareFlashTimer_Tick(object sender, EventArgs e)
        {
            // 隐藏白色正方形
            _whiteSquare.Visibility = Visibility.Hidden;
            _squareFlashTimer.Stop();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                if (!_centerPointChecked && !_centerPointMissed)
                {
                    // 判断中心点变化
                    var reactionTime1 = DateTime.Now - _changeTime1;
                    if (reactionTime1.TotalSeconds <= 1.5)
                    {
                        _centerPointCorrect = true; // 用户在中心点变化后正确按下了Enter键
                    }
                    _centerPointChecked = true; // 标记中心点已检查
                }
                else if (!_squareChecked && !_squareMissed)
                {
                    // 判断白色方形闪烁
                    var reactionTime2 = DateTime.Now - _changeTime2;
                    if (reactionTime2.TotalSeconds <= 1.5)
                    {
                        _squareCorrect = true; // 用户在白色方形闪烁后正确按下了Enter键
                    }
                    _squareChecked = true; // 标记白色方形已检查
                }
            }
        }
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!_centerPointChecked && !_centerPointMissed)
                {
                    // 判断中心点变化
                    var reactionTime1 = DateTime.Now - _changeTime1;
                    if (reactionTime1.TotalSeconds <= 1.5)
                    {
                        _centerPointCorrect = true; // 用户在中心点变化后正确按下了Enter键
                    }
                    _centerPointChecked = true; // 标记中心点已检查
                }
                else if (!_squareChecked && !_squareMissed)
                {
                    // 判断白色方形闪烁
                    var reactionTime2 = DateTime.Now - _changeTime2;
                    if (reactionTime2.TotalSeconds <= 1.5)
                    {
                        _squareCorrect = true; // 用户在白色方形闪烁后正确按下了Enter键
                    }
                    _squareChecked = true; // 标记白色方形已检查
                }
            }
        }

        private void EndTrainingTimer_Tick(object sender, EventArgs e)
        {
            _endTrainingTimer.Stop();

            // 显示训练结果
            string resultMessage = $"中心点正确: {_centerPointCorrect}\n" +
                                   $"白色方形正确: {_squareCorrect}";
            MessageBox.Show(resultMessage, "训练结果", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
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
                        Text_3.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;

                        // 隐藏试玩部分内容
                        MainGrid.Visibility = Visibility.Collapsed;

                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Content = "下一步";
                        Button_2.Margin = new Thickness(329, 850, 0, 0);
                        Button_3.Margin = new Thickness(770, 850, 0, 0);

                        await OnVoicePlayAsync(Text_1.Text);
                    }
                    break;
                case 1:
                    {
                        // 显示讲解的第一个界面
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Visible;
                        Image_2.Visibility = Visibility.Visible;
                        Text_3.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;

                        // 隐藏试玩部分内容
                        MainGrid.Visibility = Visibility.Collapsed;
                        Button_1.Visibility = Visibility.Visible;
                        Button_1.IsEnabled = true;
                        Button_2.Margin = new Thickness(550, 850, 0, 0);
                        Button_3.Margin = new Thickness(911, 850, 0, 0);

                        await OnVoicePlayAsync(Text_2.Text);
                    }
                    break;
                case 2:
                    {
                        // 显示讲解的第二个界面
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Text_3.Visibility = Visibility.Visible;
                        Image_3.Visibility = Visibility.Visible;



                        // 隐藏试玩部分的控件
                        MainGrid.Visibility = Visibility.Collapsed;

                        Button_1.IsEnabled = true;
                        Button_2.Content = "试玩";

                        await OnVoicePlayAsync(Text_3.Text);
                    }
                    break;
                case 3:
                    {
                        // 进入试玩界面
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Text_3.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;

                        // 显示试玩部分的控件
                        MainGrid.Visibility = Visibility.Visible;

                        // 隐藏讲解部分的按钮
                        Button_1.Visibility = Visibility.Collapsed;
                        //Button_1.IsEnabled = false;
                        Button_2.Visibility = Visibility.Collapsed;
                        //Button_2.IsEnabled = false;
                        Button_3.Margin = new Thickness(550, 850, 0, 0);

                        // 强制焦点保持在窗口
                        Focus();

                        InitializeTraining();
                        //LJN，在讲解模块调用委托
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("现在屏幕中央会有一个固定的绿点。在您注视它的过程当中，它会不定时发生变化，当您察觉到变化发生时按下键盘上的OK键；当屏幕中央出现白色小方块时，也请您按下键盘上的OK键。");//增加代码，调用函数，显示数字人下的文字
                        //LJN
                    }
                    break;
            }
        }



    }
}
