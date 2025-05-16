using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace crs.game.Games
{
    public partial class 逻辑思维能力讲解 : BaseUserControl
    {
        private const int MAX_DELAY = 5000; // 5秒
        private string imagePath;
        private string[] imagePaths;
        private string[] directoryPaths;
        private string[] questionPaths;
        private string[] answerPaths;
        private int hardness;
        private int index;
        private Image lastClickedImage;
        private int[] correctAnswers;
        private int[] wrongAnswers;
        private int max_time = 10;
        private int TRAIN_TIME;
        private bool IS_RESTRICT_TIME = true;
        private bool IS_BEEP = true;
        private int train_time;
        private DispatcherTimer timer;
        private DispatcherTimer trainingTimer;

        private bool isAnswered = false; // 标识是否已经作答

        public Action GameBeginAction { get; set; }
        public Func<string, Task> VoicePlayFunc { get; set; }

        public 逻辑思维能力讲解()
        {
            InitializeComponent();
            hardness = 1;
            correctAnswers = new int[23];
            wrongAnswers = new int[23];
            max_time = 60;
            TRAIN_TIME = 60;
            IS_RESTRICT_TIME = true;
            IS_BEEP = true;
            train_time = TRAIN_TIME;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(MAX_DELAY);
            timer.Tick += Timer_Tick;

            trainingTimer = new DispatcherTimer();
            trainingTimer.Interval = TimeSpan.FromSeconds(1);
            trainingTimer.Tick += TrainingTimer_Tick;

            imagePath = FindImagePath();
            if (imagePath != null)
            {
                AddImages();
                AddButtons();
            }
            else
            {
                MessageBox.Show("未找到名为“逻辑思维能力”的文件夹。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }

            this.Loaded += 逻辑思维能力讲解_Loaded;
        }

        private void 逻辑思维能力讲解_Loaded(object sender, RoutedEventArgs e)
        {
            Button_2_Click(null, null);
        }

        private void TrainingTimer_Tick(object sender, EventArgs e)
        {
            train_time--;
            if (train_time <= 0)
            {
                timer.Stop();
                trainingTimer.Stop();
                this.Close();
            }
        }
        private void Close()
        {
           // throw new NotImplementedException();
        }

        private string FindImagePath()
        {
            //string targetDirectory = @"D:\CCCCC\cognition_software-main\crs.game\Games\逻辑思维能力1\";
            string targetDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Games\逻辑思维能力1\");
            if (Directory.Exists(targetDirectory))
            {
                return targetDirectory;
            }
            return null;
        }
        /*
private string FindImagePath()
{
    string currentDirectory = Directory.GetCurrentDirectory();
    {
        string targetDirectory = Path.Combine(currentDirectory, "逻辑思维能力1");
        if (Directory.Exists(targetDirectory))
        {
            return targetDirectory;
        }
        return null;
    }
    {
        string targetDirectory = Path.Combine(currentDirectory, @"Games\逻辑思维能力1");
        if (Directory.Exists(targetDirectory))
        {
            return targetDirectory;
        }
        return null;
    }
}
*/

        private void AddImages()
        {
            Random rand = new Random();
            directoryPaths = Directory.GetDirectories(imagePath);
            directoryPaths = directoryPaths.OrderBy(path => int.Parse(path.Split('\\').Last())).ToArray();
            imagePaths = Directory.GetDirectories(directoryPaths[hardness - 1]);

            index = rand.Next(imagePaths.Length);
            string newFolderPath = Path.Combine(imagePaths[index], "Q");
            questionPaths = Directory.GetFiles(newFolderPath);

            for (int i = 0; i < questionPaths.Length; i++)
            {
                Image img = new Image
                {
                    Source = new BitmapImage(new Uri(questionPaths[i])),
                    Width = 150,
                    Height = 150
                };
                ImagePanel.Children.Add(img);
            }

            Button additionalButton = new Button
            {
                Width = 150,
                Height = 150,
                Margin = new Thickness(5),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d0e3b6")),
                BorderBrush = Brushes.Transparent
            };

            additionalButton.Click += AdditionalButton_Click;
            Image buttonImg = new Image { Source = null, Width = 150, Height = 150 };
            additionalButton.Content = buttonImg;
            ImagePanel.Children.Add(additionalButton);
        }

        private void StartTimer()
        {
            if (timer != null && timer.IsEnabled)
            {
                timer.Stop();
            }
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!isAnswered && currentPage == 2)  // 确保是在试玩阶段，并且没有答题
            {
                timer.Stop();
                wrongAnswers[hardness - 1]++;
                //MessageBox.Show("超出作答时间");
                ClearAndLoadNewImages();
                StartTimer();
            }
        }

        private void ClearAndLoadNewImages()
        {
            // 重置已回答标志
            isAnswered = false;

            ImagePanel.Children.Clear();
            ButtonPanel.Children.Clear();
            AddImages();
            AddButtons();
            StartTimer();
        }

        private void AdditionalButton_Click(object sender, RoutedEventArgs e)
        {
            Button additionalButton = sender as Button;
            Image buttonImg = additionalButton.Content as Image;

            if (buttonImg.Source != null && lastClickedImage != null)
            {
                lastClickedImage.Source = buttonImg.Source;
                buttonImg.Source = null;
                lastClickedImage = null;
            }
        }

        private void AddButtons()
        {
            string newFolderPath = Path.Combine(imagePaths[index], "A");
            answerPaths = Directory.GetFiles(newFolderPath);
            Random rand = new Random();
            answerPaths = answerPaths.OrderBy(x => rand.Next()).Take(6).ToArray(); // 保证加载6个选项

            for (int i = 0; i < answerPaths.Length; i++)
            {
                Button btn = new Button
                {
                    Width = 150,
                    Height = 150,
                    Margin = new Thickness(5),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d0e3b6")),
                    BorderBrush = Brushes.Transparent
                };

                Image img = new Image
                {
                    Source = new BitmapImage(new Uri(answerPaths[i])),
                    Width = 150,
                    Height = 150
                };

                btn.Content = img;
                btn.Click += Button_Click;
                ButtonPanel.Children.Add(btn);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            Image clickedImage = clickedButton.Content as Image;

            Button additionalButton = ImagePanel.Children.OfType<Button>().LastOrDefault();
            Image additionalButtonImage = additionalButton?.Content as Image;

            if (additionalButtonImage.Source == null)
            {
                additionalButtonImage.Source = clickedImage.Source;
                clickedImage.Source = null;
                lastClickedImage = clickedImage;
            }
        }

        private async void Confirm_Button_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage == 2)  // 确保这是在试玩阶段
            {
                isAnswered = true;  // 标记为已经作答
                (sender as Button).IsEnabled = false;

                // 确保答题时计时器停止
                StopTimerIfNeeded();

                // 找到最后一个图片框中的图片
                Button additionalButton = ImagePanel.Children.OfType<Button>().LastOrDefault();
                Image additionalButtonImage = additionalButton?.Content as Image;

                // 获取答案路径中的第一个图片
                string firstImagePath = Directory.GetFiles(Path.Combine(imagePaths[index], "A")).FirstOrDefault();
                bool isCorrect = additionalButtonImage.Source != null && additionalButtonImage.Source.ToString() == new BitmapImage(new Uri(firstImagePath)).ToString();

                if (isCorrect)
                {
                    correctAnswers[hardness - 1]++;
                    textblock.Text = "恭喜你答对了！";
                    textblock.Foreground = new SolidColorBrush(Colors.Green);
                    // 停止所有计时器以确保不再有超时提示
                    StopTimerIfNeeded();
                    if (trainingTimer != null && trainingTimer.IsEnabled)
                    {
                        trainingTimer.Stop();
                    }

                    // 答对后显示 "进入训练" 按钮
                    OkButton.Visibility = Visibility.Visible;  // 显示进入训练的按钮
                }
                else
                {
                    wrongAnswers[hardness - 1]++;
                    textblock.Text = "很遗憾答错了！";
                    textblock.Foreground = new SolidColorBrush(Colors.Red);
                }

                await Task.Delay(TimeSpan.FromSeconds(1));

                // 重新加载图片和按钮，并重启计时器
                ClearAndLoadNewImages();
                StartTimer(); // 如果游戏继续，重新启动计时器
                (sender as Button).IsEnabled = true;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // 停止试玩部分的计时器
            StopTimerIfNeeded();

            // 清理时间警告消息
            textblock.Text = string.Empty;  // 清空警告信息
            textblock.Foreground = new SolidColorBrush(Colors.Black);  // 恢复文本颜色

            // 开始训练的逻辑
            OnGameBegin();

            // 进入训练阶段，启动训练定时器
            if (!trainingTimer.IsEnabled)
            {
                trainingTimer.Start();
                train_time = TRAIN_TIME;  // 重置训练时间
            }
        }

        int currentPage = -1;

        private void Button_1_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 0)
            {
                currentPage--;
                PageSwitch();
            }
        }

        private void Button_2_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < 2)
            {
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
                        Text_1.Visibility = Visibility.Visible;
                        Image_1.Visibility = Visibility.Visible;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        textblock.Visibility = Visibility.Collapsed;
                        ImagePanel.Visibility = Visibility.Collapsed;
                        ButtonPanel.Visibility = Visibility.Collapsed;
                        Button4.Visibility = Visibility.Collapsed;

                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Content = "下一步";
                        Button_2.Margin = new Thickness(329, 850, 0, 0);
                        Button_3.Margin = new Thickness(770, 850, 0, 0);

                        await OnVoicePlayAsync(Text_1.Text);
                    }
                    break;

                case 1:
                    {
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Visible;
                        Image_2.Visibility = Visibility.Visible;
                        textblock.Visibility = Visibility.Collapsed;
                        ImagePanel.Visibility = Visibility.Collapsed;
                        ButtonPanel.Visibility = Visibility.Collapsed;
                        Button4.Visibility = Visibility.Collapsed;
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
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;

                        textblock.Visibility = Visibility.Visible;
                        ImagePanel.Visibility = Visibility.Visible;
                        ButtonPanel.Visibility = Visibility.Visible;
                        Button4.Visibility = Visibility.Visible;
                        StartTimer(); // 确保定时器在这个阶段启动

                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Visible;
                        Button_3.Margin = new Thickness(770, 850, 0, 0);

                        if (!trainingTimer.IsEnabled)
                        {
                            trainingTimer.Start();
                            train_time = TRAIN_TIME;
                        }

                        this.Focus();
                        //LJN，在讲解模块调用委托
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("您会看到屏幕上有一个图像序列，该序列的图像相互之间存在着某种规律，您需要思考发现规律，并在提供的若干图像中用鼠标点击来找出一个合适选项。");//增加代码，调用函数，显示数字人下的文字
                        //LJN
                    }
                    break;
            }
        }

        async Task VoicePlayer(string message)
        {
            var voicePlayFunc = VoicePlayFunc;
            if (voicePlayFunc == null)
            {
                return;
            }
            await voicePlayFunc.Invoke(message);
        }

        private void StopTimerIfNeeded()
        {
            if (timer != null && timer.IsEnabled)
            {
                timer.Stop();
            }
        }
    }
}
