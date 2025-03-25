using System;
using System.Collections.Generic;
using System.IO;
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
using Path = System.IO.Path;

namespace crs.game.Games
{
    /// <summary>
    /// 警觉能力讲解.xaml 的交互逻辑
    /// </summary>
    public partial class 警觉能力讲解 : BaseUserControl
    {
        private DispatcherTimer waitTimer;
        private DispatcherTimer waitTimer_withoutSound;
        private string correctImage_path;
        private string currentDirectory;
        private string targetDirectory;
        private bool buttonClick;
        // "容貌记忆力" 文件夹
        private string FolderPath;
        public Action GameBeginAction { get; set; }
        public bool Correct = false;
        public bool Sound = false;
        public Func<string, Task> VoicePlayFunc { get; set; }

        public 警觉能力讲解()
        {
            InitializeComponent();

            this.Focusable = true;
            this.Focus();

            this.Loaded += 警觉能力讲解_Loaded;
            init_path();


        }

        private void init_path()
        {
            currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            targetDirectory = Path.Combine(Directory.GetParent(currentDirectory).Parent.Parent.FullName, "crs.game", "Games");
            FolderPath = Path.Combine(targetDirectory, "警觉能力");
        }

        private void 警觉能力讲解_Loaded(object sender, RoutedEventArgs e)
        {
            // 页面加载时确保按键和焦点行为
            Button_2_Click(null, null);
            this.Focus();  // 确保焦点在窗口上
        }

        private void LoadImage(string imagePath)
        {
            if (File.Exists(imagePath))
            {
                // 创建 BitmapImage 对象并设置图像源
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.EndInit();

                // 设置 Image 控件的源
                CorrectOrNot.Source = bitmap;
            }

        }


        private void ShowImage()
        {
            Sound = false;
            RandomImage.Visibility = Visibility.Hidden;
            TipBlock.Text = "";
            CorrectOrNot.Source = null;
            RandomImage.Width = 245;
            RandomImage.Height = 245;

            // 如果定时器已存在，则停止并重置
            waitTimer?.Stop();

            // 创建新的定时器
            waitTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3) // 设置等待时间为 3 秒
            };

            waitTimer.Tick += (s, args) =>
            {
                waitTimer.Stop();
                RandomImage.Visibility = Visibility.Visible;

                // 定义隐藏定时器
                var hideTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };

                hideTimer.Tick += async (hideSender, hideArgs) =>
                {
                    hideTimer.Stop();
                    await Task.Delay(1000);
                    RandomImage.Visibility = Visibility.Hidden;

                    // 判断用户是否点击确认按钮
                    if (!Correct) // 假设 Correct 表示用户是否已点击确认按钮
                    {
                        correctImage_path = Path.Combine(FolderPath, "wrong.png");
                        LoadImage(correctImage_path);
                        await Task.Delay(2000);
                        ShowImage(); // 重新调用 ShowImage
                    }
                };

                hideTimer.Start();
            };

            waitTimer.Start();
        }
        private async void JinggaoButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage == 1)
            {
                // 调用声源显示方法
                await PlayWarningSoundAsync();
            }
        }

        private async Task PlayWarningSoundAsync()
        {
            // 播放警告声
            Console.Beep(800, 200);
            await Task.Delay(200); // 确保蜂鸣声播放完毕
        }

        private void ShowImage_withoutSound()
        {
            buttonClick = false;
            Sound = true;
            TipBlock.Text = "";
            RandomImage.Visibility = Visibility.Hidden;
            RandomImage.Width = 245;
            RandomImage.Height = 245;

            if (waitTimer_withoutSound != null)
            {
                waitTimer_withoutSound.Stop();
            }

            waitTimer_withoutSound = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };

            waitTimer_withoutSound.Tick += (s, args) =>
            {
                waitTimer_withoutSound.Stop();

                Task.Run(() => Console.Beep(800, 400));
                RandomImage.Visibility = Visibility.Visible;

                // 定义隐藏定时器
                var hideTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };

                hideTimer.Tick += async (hideSender, hideArgs) =>
                {
                    hideTimer.Stop();
                    await Task.Delay(1000);
                    if (OkButton.Visibility != Visibility.Visible)
                    {
                        RandomImage.Visibility = Visibility.Hidden;
                    }
                    

                    // 如果用户没有点击确认按钮，Correct 仍为 false，则重新调用 ShowImage
                    if (!buttonClick)
                    {
                        Correct = false; // 确保状态重置
                        correctImage_path = Path.Combine(FolderPath, "wrong.png");
                        LoadImage(correctImage_path);
                        await Task.Delay(2000);
                        ShowImage();
                    }
                };

                hideTimer.Start();
            };

            waitTimer_withoutSound.Start();
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // 检查是否按下 Enter 键
            if (e.Key == Key.Enter)
            {
                // 调用 ConfirmButton_Click 方法
                Button_Click(sender, e);
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if(Sound)
            {
                buttonClick = true;
            }
            if (RandomImage.Visibility == Visibility.Visible)
            {
                TipBlock.FontSize = 40;
                if (Sound)
                {

                    correctImage_path = Path.Combine(FolderPath, "correct.png");
                    LoadImage(correctImage_path);
                    
                    
                    //TipBlock.Text = "       恭喜你答对了！";

                }

                TipBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#06CE77"));

                Correct = true;
                if (Correct && Sound)
                {
                    OkButton.Visibility = Visibility.Visible;
                    anjian.Visibility = Visibility.Collapsed;
                }
                else
                {
                    //await Task.Delay(1000);
                    ShowImage_withoutSound();
                }


            }
            else
            {
                if (Sound)
                {
                    waitTimer_withoutSound?.Stop();
                }
                else
                {
                    waitTimer?.Stop(); // 停止定时器
                }
                //TipBlock.FontSize = 40;
                //TipBlock.Text = "       很遗憾答错了！";
                //TipBlock.Foreground = new SolidColorBrush(Colors.Red);

                correctImage_path = Path.Combine(FolderPath, "wrong.png");
                LoadImage(correctImage_path);
                OkButton.Visibility = Visibility.Collapsed;
                // 等待两秒钟
                await Task.Delay(2000);
                ShowImage();

            }

        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // 开始答题的相关逻辑
            OnGameBegin();
        }

        int currentPage = -2;

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
                        Text_3.Visibility = Visibility.Collapsed;
                        Text_4.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;
                        JinggaoButton.Visibility = Visibility.Collapsed;

                        // 隐藏试玩部分内容
                        RandomImage.Visibility = Visibility.Collapsed;
                        TargetImage.Visibility = Visibility.Collapsed;
                        anjian.Visibility = Visibility.Collapsed;
                        TipBlock.Visibility = Visibility.Collapsed;
                        TipBlock1.Visibility = Visibility.Collapsed;
                        mubiaowu.Visibility = Visibility.Collapsed;
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
                        Text_3.Visibility = Visibility.Visible;
                        Text_4.Visibility = Visibility.Visible;
                        Image_2.Visibility = Visibility.Visible;
                        Image_3.Visibility = Visibility.Visible;
                        JinggaoButton.Visibility = Visibility.Visible;


                        // 隐藏试玩部分的控件
                        RandomImage.Visibility = Visibility.Collapsed;
                        TargetImage.Visibility = Visibility.Collapsed;
                        anjian.Visibility = Visibility.Collapsed;
                        TipBlock.Visibility = Visibility.Collapsed;
                        TipBlock1.Visibility = Visibility.Collapsed;
                        mubiaowu.Visibility = Visibility.Collapsed;
                        Button_1.IsEnabled = true;
                        Button_2.Content = "试玩";
                        Button_2.Margin = new Thickness(550, 850, 0, 0);
                        Button_3.Margin = new Thickness(911, 850, 0, 0);
                        Button_1.Visibility = Visibility.Visible;

                        await OnVoicePlayAsync(Text_2.Text);
                        await OnVoicePlayAsync(Text_3.Text);
                        await OnVoicePlayAsync(Text_4.Text);
                    }
                    break;
                case 2:
                    {
                        // 进入试玩界面
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Text_3.Visibility = Visibility.Collapsed;
                        Text_4.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;
                        JinggaoButton.Visibility = Visibility.Collapsed;
                        // 显示试玩部分的控件
                        RandomImage.Visibility = Visibility.Visible;
                        TargetImage.Visibility = Visibility.Visible;
                        anjian.Visibility = Visibility.Visible;
                        TipBlock.Visibility = Visibility.Visible;
                        TipBlock1.Visibility = Visibility.Collapsed;
                        mubiaowu.Visibility = Visibility.Visible;
                        // 隐藏讲解部分的按钮
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Collapsed;
                        ShowImage();
                        // 强制焦点保持在窗口
                        this.Focus();
                        //LJN，在讲解模块调用委托
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("现在您在屏幕左侧可以看到需要确认选取的目标图，当屏幕出现带有警告声或者没有警告声的目标物出现时，请用鼠标点击确认按钮。");//增加代码，调用函数，显示数字人下的文字
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

