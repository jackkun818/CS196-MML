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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskBand;

namespace crs.game.Games
{
    /// <summary>
    /// 反应能力讲解.xaml 的交互逻辑
    /// </summary>
    public partial class 反应能力讲解 : BaseUserControl
    {
        private DispatcherTimer timer;

        public Action GameBeginAction { get; set; }

        public Func<string, Task> VoicePlayFunc { get; set; }

        public 反应能力讲解()
        {
            InitializeComponent();

            TipBlock.Text = null;// "请根据提示在左侧出现图片后按下对应的按键。";
            TargetImage.Source = new BitmapImage(new Uri("反应能力/1.png", UriKind.Relative));
            RandomImage.Source = null;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += Timer_Tick;

            this.Loaded += 反应能力讲解_Loaded;


        }

        private void 反应能力讲解_Loaded(object sender, RoutedEventArgs e)
        {
            // 页面加载时确保按键和焦点行为
            Button_2_Click(null, null);
            this.Focus();  // 确保焦点在窗口上
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            timer.Stop();
            RandomImage.Source = new BitmapImage(new Uri("反应能力/1.png", UriKind.Relative));
        }
        private void Window_MouseMove(object sender, MouseEventArgs e)//绑定自定义鼠标光标和默认鼠标光标
        {

            Point position = e.GetPosition(this);
            Canvas.SetLeft(CustomCursor, position.X - (CustomCursor.Width / 2));
            Canvas.SetTop(CustomCursor, position.Y - (CustomCursor.Height / 2));
        }
        private void OnGameStart()//调用自定义鼠标光标函数
        {
            this.Cursor = Cursors.None; // 隐藏默认光标
            CustomCursor.Visibility = Visibility.Visible; // 显示自定义光标
            MouseMove += Window_MouseMove; // 订阅 MouseMove 事件
            CustomCursor.Width = 65; // 调整为你想要的宽度
            CustomCursor.Height = 65; // 调整为你想要的高度
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right && RandomImage.Source != null)
            {
                e.Handled = true; // 阻止其他控件处理此事件
                TipBlock.FontSize = 40;
                TipBlock.Text = "  恭喜你答对了！";
                TipBlock.Foreground = new SolidColorBrush(Colors.Green);
                OkButton.Visibility = Visibility.Visible;
            }
            else
            {
                e.Handled = true; // 阻止其他控件处理此事件
                TipBlock.FontSize = 40;
                TipBlock.Text = "  很遗憾答错了！";
                TipBlock.Foreground = new SolidColorBrush(Colors.Red);
                OkButton.Visibility = Visibility.Collapsed;
                timer?.Stop();
                timer?.Start();
                RandomImage.Source = null;
            }
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
                        Image_3.Visibility = Visibility.Collapsed;


                        // 隐藏试玩部分内容
                        TipBlock.Visibility = Visibility.Collapsed;
                        TargetImage.Visibility = Visibility.Collapsed;
                        RandomImage.Visibility = Visibility.Collapsed;
                        TipBlock1.Visibility = Visibility.Collapsed;
                        fuhao.Visibility = Visibility.Collapsed;
                       
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
                        Text_2.Visibility = Visibility.Visible;
                        Image_2.Visibility = Visibility.Visible;
                        Image_3.Visibility = Visibility.Visible;




                        // 隐藏试玩部分的控件
                        TipBlock.Visibility = Visibility.Collapsed;
                        TargetImage.Visibility = Visibility.Collapsed;
                        RandomImage.Visibility = Visibility.Collapsed;
                        TipBlock1.Visibility = Visibility.Collapsed;
                        fuhao.Visibility = Visibility.Collapsed;
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
                        TipBlock.Visibility = Visibility.Visible;
                        TargetImage.Visibility = Visibility.Visible;
                        RandomImage.Visibility = Visibility.Visible;
                        TipBlock1.Visibility = Visibility.Visible;
                        fuhao.Visibility = Visibility.Visible;
                        // 隐藏讲解部分的按钮
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Collapsed;
                        timer.Start();
                        // 强制焦点保持在窗口
                        this.Focusable = true;
                        this.Focus();  // 将焦点放在 UserControl 上
                        Keyboard.Focus(this);  // 立即捕获键盘焦点
                        this.KeyDown += Window_KeyDown;  // 确保按键事件被绑定

                        //LJN，在讲解模块调用委托
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("请记住并熟悉屏幕上四个标志所对应的按键，在标志出现后在键盘上按下对应的按键。");//增加代码，调用函数，显示数字人下的文字
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
