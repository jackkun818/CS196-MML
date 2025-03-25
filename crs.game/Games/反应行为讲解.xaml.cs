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
using static System.Net.Mime.MediaTypeNames;

namespace crs.game.Games
{
    /// <summary>
    /// 反应行为讲解.xaml 的交互逻辑
    /// </summary>
    public partial class 反应行为讲解 : BaseUserControl
    {
        private DispatcherTimer timer;

        public Action GameBeginAction { get; set; }

        public Func<string, Task> VoicePlayFunc { get; set; }

        public 反应行为讲解()
        {
            InitializeComponent();
            TipBlock.Text = null;// "请根据提示在左侧出现图片后按下对应的按键。";
            TargetImage.Source = new BitmapImage(new Uri("反应行为/right.png", UriKind.Relative));
            RandomImage.Source = null;

            // 初始化定时器
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);  // 定时器设置为2秒
            timer.Tick += Timer_Tick;  // 绑定定时器事件

            this.Loaded += 反应行为讲解_Loaded;

            // 捕获全局按键事件, 使用 PreviewKeyDown 更早捕获事件
            this.PreviewKeyDown += Window_PreviewKeyDown;

            // 确保焦点保持在窗口
            this.Focusable = true;  // 确保窗口能够获得焦点
            this.Focus();  // 强制将焦点放到当前窗口
        }

        private void 反应行为讲解_Loaded(object sender, RoutedEventArgs e)
        {
            // 页面加载时确保按键和焦点行为
            Button_2_Click(null, null);
            this.Focus();  // 确保焦点在窗口上
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
        private void OnGameStart()//调用自定义鼠标光标函数
        {
            this.Cursor = Cursors.None; // 隐藏默认光标
            CustomCursor.Visibility = Visibility.Visible; // 显示自定义光标
            MouseMove += Window_MouseMove; // 订阅 MouseMove 事件
            CustomCursor.Width = 65; // 调整为你想要的宽度
            CustomCursor.Height = 65; // 调整为你想要的高度
        }
        private void Timer_Tick(object? sender, EventArgs e)
        {
            // 当定时器触发时显示随机图片
            RandomImage.Source = new BitmapImage(new Uri("反应行为/right.png", UriKind.Relative));

            // 停止定时器，等待用户按键输入
            timer.Stop();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 捕获所有按键事件, 防止其他控件获取焦点
            e.Handled = true;  // 处理事件，防止系统进一步处理按键事件

            // 用户按下右键且图片已显示
            if (e.Key == Key.Right && RandomImage.Source != null)
            {
                TipBlock.FontSize = 40;
                TipBlock.Text = "  恭喜你答对了！";
                TipBlock.Foreground = new SolidColorBrush(Colors.Green);
                OkButton.Visibility = Visibility.Visible;

                // 停止定时器
                timer.Stop();
            }
            else if (e.Key != Key.Right && RandomImage.Source != null)
            {
                // 用户答错的情况
                TipBlock.FontSize = 40;
                TipBlock.Text = "  很遗憾答错了！";
                TipBlock.Foreground = new SolidColorBrush(Colors.Red);
                OkButton.Visibility = Visibility.Collapsed;

                // 停止并重启定时器，重新显示图片
                timer.Stop();
                RandomImage.Source = null;
                timer.Start();
            }

            // 强制焦点保持在窗口，防止按键丢失焦点
            this.Focus();
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

                        // 隐藏试玩部分内容
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;
                        TargetImage.Visibility = Visibility.Collapsed;
                        TipBlock.Visibility = Visibility.Collapsed;
                        RandomImage.Visibility = Visibility.Collapsed;
                        fuhaotext.Visibility = Visibility.Collapsed;
                        KeyPromptText.Visibility = Visibility.Collapsed;

                        Button_1.IsEnabled = false;
                        Button_2.Content = "下一步";
						Button_1.Visibility = Visibility.Collapsed;
						Button_2.Margin = new Thickness(329, 850, 0, 0);
						Button_3.Margin = new Thickness(770, 850, 0, 0);
                        OnGameStart();
                        await OnVoicePlayAsync(Text_1.Text);
                    }
                    break;
                case 1:
                    {
                        // 显示讲解的第二个界面
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;

                        // 第二个讲解界面
                        Text_2.Visibility = Visibility.Visible;
                        Image_2.Visibility = Visibility.Visible;
                        Image_3.Visibility = Visibility.Visible;

                        // 隐藏试玩部分的控件
                        TargetImage.Visibility = Visibility.Collapsed;
                        TipBlock.Visibility = Visibility.Collapsed;
                        RandomImage.Visibility = Visibility.Collapsed;
                        fuhaotext.Visibility = Visibility.Collapsed;
                        KeyPromptText.Visibility = Visibility.Collapsed;

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
                        // 进入试玩界面
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;

                        // 显示试玩部分的控件
                        TargetImage.Visibility = Visibility.Visible;
                        TipBlock.Visibility = Visibility.Visible;
                        RandomImage.Visibility = Visibility.Visible;
                        fuhaotext.Visibility = Visibility.Visible;
                        KeyPromptText.Visibility = Visibility.Visible;

                        // 隐藏讲解部分的按钮
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Collapsed;

                        // 强制焦点保持在窗口
                        this.Focus();
                        //LJN，在讲解模块调用委托
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("请记住并熟悉屏幕上的标志所对应的按键，在该标志出现后在键盘上按下对应的按键。");//增加代码，调用函数，显示数字人下的文字
                        //LJN
                        StartTrial();
                    }
                    break;
            }
        }

        private void StartTrial()
        {
            this.Focus();  // 强制将焦点保持在窗口上

            // 启动定时器
            timer.Start();

            // 初始化提示
            RandomImage.Source = null;
            TipBlock.Text = null;// "请根据提示按下对应按键";
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
