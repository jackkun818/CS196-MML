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
    /// 选择注意力讲解.xaml 的交互逻辑
    /// </summary>
    public partial class 选择注意力讲解 : BaseUserControl
    {
        private DispatcherTimer waitTimer;
        private DispatcherTimer timer;
        private bool istrue;
        private int current;//1应该按，2不应该按

        public Action GameBeginAction { get; set; }

        public Func<string, Task> VoicePlayFunc { get; set; }

        public 选择注意力讲解()
        {
            InitializeComponent();

            current = 1;
            istrue = true;
            

            this.Loaded += 选择注意力讲解_Loaded;


        }

        private void 选择注意力讲解_Loaded(object sender, RoutedEventArgs e)
        {
            // 页面加载时确保按键和焦点行为
            Button_2_Click(null, null);
           
        }

        private void ShowImage()
        {
            RandomImage.Visibility = Visibility.Hidden;
            FeedbackImage.Visibility = Visibility.Collapsed;

            if (current == 1)
            {
                RandomImage.Source = new BitmapImage(new Uri("GONT1.png", UriKind.Relative));
                RandomImage.Width = 270; // 放大后的宽度
                RandomImage.Height = 270; // 放大后的高度
                RandomImage.Margin = new Thickness(0, 50, 0, 0); // 向下移动50个像素
            }
            else
            {
                RandomImage.Source = new BitmapImage(new Uri("GONT2.png", UriKind.Relative));
                RandomImage.Width = 270; // 放大后的宽度
                RandomImage.Height = 270; // 放大后的高度
                RandomImage.Margin = new Thickness(0, 50, 0, 0); // 向下移动50个像素

            }

            // 如果定时器已存在，则停止并重置
            waitTimer?.Stop();

            // 显示图片定时器
            waitTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3) // 设置等待时间为 3 秒
            };

            waitTimer.Tick += (s, args) =>
            {
                waitTimer.Stop(); // 停止定时器
                RandomImage.Visibility = Visibility.Visible;

                // 等待答题计时器
                timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(3) // 设置等待时间为 3 秒
                };

                timer.Tick += (s, args) =>
                {
                    timer.Stop(); // 停止定时器
                    if (current == 1)
                    {
                        istrue = false;
                        current = 2;
                        ShowImage();
                    }
                    else
                    {
                        if (istrue == true)
                        {
                            FeedbackImage.Source = new BitmapImage(new Uri("./pic/GONT/right.png", UriKind.Relative)); //更改地方：把答对了换成图片展示，设置正确图片
                            FeedbackImage.Visibility = Visibility.Visible;
                            OkButton.Visibility = Visibility.Visible;
                            queren.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            FeedbackImage.Source = new BitmapImage(new Uri("./pic/GONT/error.png", UriKind.Relative)); //更改地方：把答对了换成图片展示，设置正确图片
                            FeedbackImage.Visibility = Visibility.Visible;
                            OkButton.Visibility = Visibility.Collapsed;
                            

                            // 答错提示计时器
                            DispatcherTimer delayTimer = new DispatcherTimer
                            {
                                Interval = TimeSpan.FromSeconds(3) // 设置等待时间为 3 秒
                            };

                            delayTimer.Tick += (s, args) =>
                            {
                                delayTimer.Stop(); // 停止延迟定时器
                                current = 1;
                                istrue = true;
                                ShowImage();
                            };

                            delayTimer.Start(); // 启动延迟定时器
                        }
                    }

                };

                timer.Start(); // 启动计时器
            };

            waitTimer.Start(); // 启动定时器
        }

        //增加代码，绑定键盘的enter键和“确认”按钮
        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // 检查按下的键是否是你指定的键
            if (e.Key == System.Windows.Input.Key.Enter) // 检测是否按下回车键
            {
                // 模拟按钮点击，调用 Button_Click 方法，就是确认按钮的点击事件定义
                Button_Click(null, null);
            }
        }
        //更改结束

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            timer?.Stop();
            if (current == 1)
            {
                current = 2;
                ShowImage();
            }
            else
            {
                waitTimer?.Stop(); // 停止定时器
                FeedbackImage.Source = new BitmapImage(new Uri("./pic/GONT/error.png", UriKind.Relative)); //更改地方：把答对了换成图片展示，设置正确图片
                FeedbackImage.Visibility = Visibility.Visible;
                OkButton.Visibility = Visibility.Collapsed;
                

                // 等待三秒钟
                await Task.Delay(3000);

                // 调用 ShowImage 方法
                current = 1;
                istrue = true;
                ShowImage();
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
            if (currentPage == 2)
            {
                // 如果当前是讲解的最后一页，点击"试玩"后进入试玩界面
                currentPage = 3;
                PageSwitch(); // 切换到试玩界面并执行 ShowImage
            }
            else
            {
                // 否则，进入下一页讲解
                currentPage++;
                PageSwitch();
            }
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
                        FeedbackImage.Visibility = Visibility.Collapsed;
                        TargetImage.Visibility = Visibility.Collapsed;
                        TipBlock1.Visibility = Visibility.Collapsed;
                        queren.Visibility = Visibility.Collapsed;
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
                        Text_3.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;

                        // 隐藏试玩部分的控件
                        FeedbackImage.Visibility = Visibility.Collapsed;
                        TargetImage.Visibility = Visibility.Collapsed;
                        TipBlock1.Visibility = Visibility.Collapsed;
                        queren.Visibility = Visibility.Collapsed;
                        Button_1.IsEnabled = true;
                        Button_2.Content = "下一步";
                        Button_2.Margin = new Thickness(550, 850, 0, 0);
                        Button_3.Margin = new Thickness(911, 850, 0, 0);
                        Button_1.Visibility = Visibility.Visible;
                        await OnVoicePlayAsync(Text_2.Text);
                    }
                    break;
                case 2:
                    {
                        // 显示讲解的第三个界面
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Text_3.Visibility = Visibility.Visible;
                        Image_3.Visibility = Visibility.Visible;

                        // 隐藏试玩部分的控件
                        FeedbackImage.Visibility = Visibility.Collapsed;
                        TargetImage.Visibility = Visibility.Collapsed;
                        TipBlock1.Visibility = Visibility.Collapsed;
                        queren.Visibility = Visibility.Collapsed;
                        Button_1.IsEnabled = true;
                        Button_2.Content = "试玩";

                        await OnVoicePlayAsync(Text_3.Text);
                    }
                    break;
                case 3:
                    {
                        // 进入试玩界面，显示试玩部分的控件
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Text_3.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;

                        // 显示试玩部分的控件
                        FeedbackImage.Visibility = Visibility.Visible;
                        TargetImage.Visibility = Visibility.Visible;
                        TipBlock1.Visibility = Visibility.Visible;
                        queren.Visibility = Visibility.Visible;

                        // 隐藏讲解部分的按钮
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Collapsed;

                        // 强制焦点保持在窗口
                        this.Focus();

                        // 进入试玩界面时立即开始调用 ShowImage()，开始试玩
                        ShowImage();

                        //LJN，在讲解模块调用委托
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("屏幕右侧会有一个目标物体，当屏幕左侧出现该目标物体时，按下键盘上的OK键，其余情况则无需做出反应。");//增加代码，调用函数，显示数字人下的文字
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
