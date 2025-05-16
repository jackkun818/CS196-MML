using Microsoft.IdentityModel.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
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
    /// 拓扑记忆力讲解.xaml 的交互逻辑
    /// </summary>
    public partial class 拓扑记忆力讲解 : BaseUserControl
    {
        private Button[] buttons;
        private string[] imagePaths = { "Resources/MEMO/general/sample1.jpg", "Resources/MEMO/general/sample2.jpg", "Resources/MEMO/general/sample3.jpg" };
        private string recallImagePath = "Resources/MEMO/general/sample2.jpg";
        private DispatcherTimer timer;
        private int countdownTime = 5;
        private Button selectedButton;
        private int StopDurations = 1000;

        private string oImage;

        private int gameStage = 0; // 按下OK执行的函数：0 为 准备并且开始，1为选择图案过程，2为进入训练

        private string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory; // 当前项目的绝对路径
        private string ResourcesPath = System.IO.Path.Combine("Resources", "词语记忆力");//这里用的固定是词语记忆力的路径，后期可改
        private SoundPlayer soundPlayer; // 用来放歌
        public string ErrorSoundPath;//错误的声音文件路径，在OnStartAsync()中配置
        public string CorrectSoundPath;//正确的声音文件路径，在OnStartAsync()中配置
        public 拓扑记忆力讲解()
        {
            InitializeComponent();

            // 键位
            this.KeyDown += OnKeyDown;
            this.PreviewKeyDown += OnKeyDown;
            this.Focusable = true;
            this.Focus(); // 主动获取焦点

            // 获取当前工作目录
            string currentDirectory = Directory.GetCurrentDirectory();
            string imageFolderPath = System.IO.Path.Combine(currentDirectory, "Resources/MEMO/General");
            string p2 = System.IO.Path.Combine(currentDirectory, "Resources/MEMO");
            BitmapImage image1 = new BitmapImage(new Uri(p2 + "/x.png", UriKind.Absolute));
            BitmapImage image2 = new BitmapImage(new Uri(p2 + "/y.png", UriKind.Absolute));


            Image_1.Source = image1;
            Image_2.Source = image2;

            CorrectSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");
            // 为 Image 控件加载图片 Source
            CorrectImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Correct.png"), UriKind.RelativeOrAbsolute));
            WrongImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Error.png"), UriKind.RelativeOrAbsolute));




            // 初始化按钮数组并创建按钮
            buttons = new Button[3];
            for (int i = 0; i < 3; i++)
            {
                Button button = new Button();
                button.Click += Button_Click;
                button.Background = new ImageBrush(new BitmapImage(new Uri(imagePaths[i], UriKind.Relative)));
                button.Width = 150;
                button.Height = 150;
                buttons[i] = button;
                PatternGrid.Children.Add(button);
                button.IsHitTestVisible = false;
            }

            // 初始化定时器
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            this.Loaded += 拓扑记忆力讲解_Loaded;


        }

        private void 拓扑记忆力讲解_Loaded(object sender, RoutedEventArgs e)
        {
            Button_2_Click(null, null);
        }

        private void OnReadyButtonClick(object sender, RoutedEventArgs e)
        {
            // 初始化倒计时
            if (gameStage == 0)
            {
                countdownTime = 5;
                TipsTextBlock.Text = $"请记住目标图案及其位置，  {countdownTime}秒后图案消失";


                // 启动定时器
                timer.Start();
            }
            else if (gameStage == 1)
            {
                // 如果没有选择任何按钮
                if (selectedButton == null)
                {
                    TipsTextBlock.Text = "请先选择要验证的图案！";
                    return;
                }

                // 恢复被选中的按钮图片
                var originalImage = new BitmapImage(
                    new Uri((string)selectedButton.Tag, UriKind.Relative));

                selectedButton.Background = new ImageBrush(originalImage);

                // 禁用所有按钮交互
                foreach (var btn in buttons)
                {
                    btn.IsHitTestVisible = false;
                }

                // 验证结果
                if ((string)selectedButton.Tag == recallImagePath)
                {
                    /*CorrectTextBox.Background = Brushes.Green;*/
                    ShowFeedbackImage(CorrectImage);
                    TipsTextBlock.Text = "恭喜，选择正确！按下OK键进入训练";
                    PlayWav(CorrectSoundPath);
                    ReadyButton.Visibility = Visibility.Visible;
                    ReadyButton.Content = "进入训练";

                    gameStage = 2;

                }
                else
                {
                    /*IncorrectTextBox.Background = Brushes.Red;*/
                    ShowFeedbackImage(WrongImage);
                    TipsTextBlock.Text = "错误，按下OK键再试一次！";
                    PlayWav(ErrorSoundPath);
                    ReadyButton.Visibility = Visibility.Visible;
                    ReadyButton.Content = "重试";

                    gameStage = 0;
                    ResetButtonImages();
                }

                // 清除选中状态
                selectedButton.BorderThickness = new Thickness(0);
                selectedButton = null;
                

            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            countdownTime--;

            if (countdownTime > 0)
            {
                // 更新提示文本
                TipsTextBlock.Text = $"记住目标图案及其位置，   {countdownTime}秒后图案消失";
            }
            else
            {
                // 停止定时器
                timer.Stop();
                // OK键位判定更改

                gameStage = 1;
                Stage_Ready(); // 切换到准备阶段

                // 启用按钮并清除图案
                foreach (var button in buttons)
                {
                    button.IsHitTestVisible = true;
                    // 在设置灰色背景的同时，将实际图片路径存储在Tag中
                    button.Tag = ((ImageBrush)button.Background).ImageSource.ToString();
                    button.Background = Brushes.Gray;
                }

                string currentDirectory = Directory.GetCurrentDirectory();
                string imageFolderPath = System.IO.Path.Combine(currentDirectory, "Resources/MEMO/General");
                RecallButton.Source = new BitmapImage(new Uri(imageFolderPath + "/sample2.jpg", UriKind.Absolute));

                // 更新提示文本
                TipsTextBlock.Text = "按下OK键选择被遮盖的目标图案";
            }
        }

        private void OnResetButtonClick(object sender, RoutedEventArgs e)
        {
            // 重新设置按钮的图案
            for (int i = 0; i < 3; i++)
            {
                buttons[i].Background = new ImageBrush(new BitmapImage(new Uri(imagePaths[i], UriKind.Relative)));
                buttons[i].IsHitTestVisible = false; // 禁用按钮点击功能
            }

            // 重置提示文本和颜色
/*            CorrectTextBox.Background = Brushes.White;
            IncorrectTextBox.Background = Brushes.White;*/
            TipsTextBlock.Text = string.Empty;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // 防御性检查
            if (sender == null || !(sender is Button clickedButton)) return;

            // 清除之前选中的按钮样式
            if (selectedButton != null)
            {
                selectedButton.BorderThickness = new Thickness(0);
            }

            // 更新选中按钮
            selectedButton = clickedButton;
            selectedButton.BorderThickness = new Thickness(3);
            selectedButton.BorderBrush = Brushes.Yellow;

            // 记录选中图片路径
            oImage = selectedButton.Tag as string;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
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
                        Text_2.Visibility = Visibility.Hidden;
                        Image_2.Visibility = Visibility.Hidden;



                        Button_1.IsEnabled = false;
                        Button_2.Content = "下一步";

                        await OnVoicePlayAsync(Text_1.Text);
                    }
                    break;
                case 1:
                    {
                        // 显示讲解的第一个界面
                        Text_1.Visibility = Visibility.Hidden;
                        Image_1.Visibility = Visibility.Hidden;
                        Text_2.Visibility = Visibility.Visible;
                        Image_2.Visibility = Visibility.Visible;

                        Button_1.IsEnabled = true;
                        Button_2.Content = "试玩";

                        await OnVoicePlayAsync(Text_2.Text);
                    }
                    break;
                case 2:
                    {
                        // 显示讲解的第二个界面
                        Text_1.Visibility = Visibility.Hidden;
                        Image_1.Visibility = Visibility.Hidden;
                        Text_2.Visibility = Visibility.Hidden;
                        Image_2.Visibility = Visibility.Hidden;

                        Button_1.IsEnabled = true;
                        Button_2.Content = "试玩";

                        Button_1.Visibility = Visibility.Hidden;
                        Button_2.Visibility = Visibility.Hidden;

                        grid.Visibility = Visibility.Visible;


                        //LJN，在讲解模块调用委托
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("您会在界面上会看到三个图形，请快速地记住他们的顺序，倒计时结束后三个图形会被覆盖，请你以最快的速度找出并选择与屏幕右侧目标图形对应的所在位置。按“↑↓←→”选择并按“Enter”确认OK");//增加代码，调用函数，显示数字人下的文字
                        //LJN

                    }
                    break;

            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (gameStage == 0)
            {
                // 未准备阶段只允许 Enter 触发准备
                if (e.Key == Key.Enter)
                {
                    OnReadyButtonClick(null, null);
                    e.Handled = true;
                }
                return;
            }

            // 准备阶段允许方向键操作
            switch (e.Key)
            {
                case Key.Left:
                    MoveSelection(-1);
                    e.Handled = true;
                    break;
                case Key.Right:
                    MoveSelection(1);
                    e.Handled = true;
                    break;
                case Key.Enter:
                    if (gameStage == 1)
                    {
                        OnReadyButtonClick(null, null);

                    }else if (gameStage == 2)
                    {
                        OnGameBegin(); //开启训练
                        
                    }
                    e.Handled = true;
                    break;


            }
        }
        private void MoveSelection(int delta)
        {
            if (selectedButton == null)
            {
                selectedButton = buttons[0];
            }
            else
            {
                int index = Array.IndexOf(buttons, selectedButton);
                index = (index + delta + buttons.Length) % buttons.Length;
                selectedButton = buttons[index];
            }
            selectedButton.Focus(); // 焦点跟随
            UpdateSelectedButton();
        }

        // 显示反馈图片
        private async void ShowFeedbackImage(Image image)
        {//显示反馈的图片
            if (image == null) return;

            // 显示图片（强制主线程）
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                image.Visibility = Visibility.Visible;
            });

            // 等待 5 秒
            await Task.Delay(5000);

            // 隐藏图片（强制主线程）
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                image.Visibility = Visibility.Collapsed;
            });
        }

        private void UpdateSelectedButton()
        {
            foreach (var btn in buttons)
            {
                btn.BorderThickness = btn == selectedButton ? new Thickness(3) : new Thickness(0);
                btn.BorderBrush = Brushes.Yellow;
            }
        }

        private void Stage_UnReady()
        {
            // 禁用所有按钮
            foreach (var btn in buttons)
            {
                btn.IsEnabled = false;
                btn.Focusable = false; // 禁止获得焦点
                btn.ClearValue(Button.BorderBrushProperty); // 清除选中样式
            }

            // 设置 Enter 键触发准备逻辑
            ReadyButton.IsDefault = true;
            RecallButton.Visibility = Visibility.Collapsed; // 隐藏回忆图案
            this.Focus(); // 焦点回到主控件
        }

        private void Stage_Ready()
        {
            // 启用所有按钮
            foreach (var btn in buttons)
            {
                btn.IsEnabled = true;
                btn.Focusable = true; // 允许获得焦点
            }

            // 设置 Enter 键触发验证逻辑
            ReadyButton.IsDefault = false;
            RecallButton.Visibility = Visibility.Visible; // 显示回忆图案
            buttons[0].Focus(); // 焦点到第一个按钮
            gameStage = 1;
        }

        private void keyboard_click(object sender, RoutedEventArgs e)
        {
            var clickedButton = sender as Button;
            Button_Click(clickedButton, e);
        }
        private void ResetButtonImages()
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                // 从 imagePaths 加载原始图片
                var originalImage = new BitmapImage(new Uri(imagePaths[i], UriKind.Relative));
                buttons[i].Background = new ImageBrush(originalImage);
                buttons[i].Tag = imagePaths[i]; // 确保 Tag 存储正确路径
            }
        }

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
    }
}
