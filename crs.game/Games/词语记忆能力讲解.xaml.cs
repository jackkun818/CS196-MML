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
    /// 词汇记忆力能力讲解.xaml 的交互逻辑
    /// </summary>
    public partial class 词语记忆能力讲解 : BaseUserControl
    {
        private int gametime;
        private bool istrue;
        private DispatcherTimer gameTimer;
        private DispatcherTimer timer;

        public Action GameBeginAction { get; set; }

        public Func<string, Task> VoicePlayFunc { get; set; }

        public 词语记忆能力讲解()
        {
            InitializeComponent();
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(4);
            gameTimer.Tick += GameTimer_Tick;
            this.Loaded += 词汇记忆能力讲解_Loaded;                       
        }

        private void 词汇记忆能力讲解_Loaded(object sender, RoutedEventArgs e)
        {
            // 页面加载时确保按键和焦点行为
            Button_2_Click(null, null);
            this.Focus();  // 确保焦点在窗口上
        }

        private void startgame()
        {
            FeedbackImage.Visibility = Visibility.Collapsed;
            istrue = true;
            TipBlock.Text = null;
            TipBlock.Foreground = new SolidColorBrush(Colors.Black);
            WordBlock.Foreground = new SolidColorBrush(Colors.Black);
            WordBlock.Text = "请找出一个重复出现的词语!";
            gameTimer.Start();
            WordBlock.FontSize=50;

        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            FeedbackImage.Visibility = Visibility.Collapsed;
            gameTimer.Stop();
            gametime = 0;
            TipBlock.Text = null;// 
            WordBlock.FontSize = 100;
            anjian1.Visibility = Visibility.Visible;
            anjian2.Visibility = Visibility.Visible;
            TipBlock.Foreground = new SolidColorBrush(Colors.Black);
            WordBlock.Foreground = new SolidColorBrush(Colors.Black);
            WordBlock.Text = "苹 果";
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop(); // 停止定时器
            startgame();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            gametime++;
            if(gametime==1)
            {
                istrue=false;
                WordBlock.Text = "橙 子";
            }
            if(gametime==2)
            {
                istrue = false;
                WordBlock.Text = "苹 果";
            }
            if (gametime==3 && !istrue)
            {
                
                FeedbackImage.Source = new BitmapImage(new Uri("./pic/WOMT/error.png", UriKind.Relative)); //更改地方：把答错了换成图片展示，设置错误图片,把“很遗憾答错了”替换掉
                FeedbackImage.Visibility = Visibility.Visible;
                OkButton.Visibility = Visibility.Collapsed;
                anjian1.Visibility=Visibility.Collapsed;
                anjian2.Visibility=Visibility.Collapsed;
               
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(3); // 设置为3秒
                timer.Tick += Timer_Tick; // 订阅 Tick 事件
                
                timer.Start();
            }
            else if(gametime == 3 && istrue)
            {
                FeedbackImage.Source = new BitmapImage(new Uri("./pic/WOMT/right.png", UriKind.Relative)); //更改地方：把答对了换成图片展示，设置正确图片,把“恭喜你答对了”替换掉
                FeedbackImage.Visibility = Visibility.Visible;
                OkButton.Visibility = Visibility.Visible;
                anjian1.Visibility = Visibility.Collapsed;
                anjian2.Visibility = Visibility.Collapsed;
            }
        }

        //更改地方：增加功能，绑定方向的左右键对应是否按钮，按下左右就相当于点击是否按钮
        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // 检查按下的键是否是方向左键或右键
            if (e.Key == Key.Left)
            {
                // 模拟“是”按钮的点击，调用 Button_Click 方法
                Button_Click(anjian1, null);  // 模拟点击"是"按钮
            }
            else if (e.Key == Key.Right)
            {
                // 模拟“否”按钮的点击，调用 Button_Click_1 方法
                Button_Click_1(anjian2, null);  // 模拟点击"否"按钮
            }

            // 其他按键处理（如 Enter 键）
            if (e.Key == Key.Enter)
            {
                // 可以处理其他按键的逻辑，如按下 Enter 键
            }
        }
        //更改结束



        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            gametime++;
            if (gametime == 1)
            {
                WordBlock.Text = "橙 子";
            }
            if (gametime == 2)
            {
                WordBlock.Text = "苹 果";
            }
            if (gametime == 3)
            {
                istrue=false;
            }
            if (gametime == 3 && istrue)
            {
                FeedbackImage.Source = new BitmapImage(new Uri("./pic/WOMT/right.png", UriKind.Relative)); //更改地方：把答对了换成图片展示，设置正确图片,把“恭喜你答对了”替换掉
                FeedbackImage.Visibility = Visibility.Visible;
                OkButton.Visibility = Visibility.Visible;
                anjian1.Visibility = Visibility.Collapsed;
                anjian2.Visibility = Visibility.Collapsed;
            }
            else if (gametime == 3 && !istrue)
            {
                FeedbackImage.Source = new BitmapImage(new Uri("./pic/WOMT/error.png", UriKind.Relative)); //更改地方：把答错了换成图片展示，设置错误图片,把“很遗憾答错了”替换掉
                FeedbackImage.Visibility = Visibility.Visible;
                OkButton.Visibility = Visibility.Collapsed;
                anjian1.Visibility = Visibility.Collapsed;
                anjian2.Visibility = Visibility.Collapsed;
               
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(3); // 设置为3秒
                timer.Tick += Timer_Tick; // 订阅 Tick 事件
               
                timer.Start();
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

                        Text_2.Visibility = Visibility.Collapsed;


                        // 隐藏试玩部分内容
                        WordBlock.Visibility = Visibility.Collapsed;
                        anjian1.Visibility = Visibility.Collapsed;
                        anjian2.Visibility = Visibility.Collapsed;
                        TipBlock.Visibility = Visibility.Collapsed;
                        FeedbackImage.Visibility = Visibility.Collapsed;
                        Button_1.IsEnabled = false;
                        Button_2.Content = "试玩";

                        await OnVoicePlayAsync(Text_1.Text);
                    }
                    break;
                case 1:
                    {
                        // 进入试玩界面
                        Text_1.Visibility = Visibility.Collapsed;

                        Text_2.Visibility = Visibility.Collapsed;


                        // 显示试玩部分的控件
                        WordBlock.Visibility = Visibility.Visible;
                        anjian1.Visibility = Visibility.Collapsed;
                        anjian2.Visibility = Visibility.Collapsed;
                        TipBlock.Visibility = Visibility.Visible;
                        FeedbackImage.Visibility = Visibility.Visible;
                        // 隐藏讲解部分的按钮
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Collapsed;
                        //LJN，在讲解模块调用委托
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("在屏幕上将有一些词重复出现,请找到重复出现的词语。当重复的词出现时，请您在屏幕上点击“是”按钮，否则点击“否”按钮");//增加代码，调用函数，显示数字人下的文字
                        
                        //LJN
                        startgame();

                        this.Focus();
                       
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
