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
using System.Windows.Threading;

namespace crs.game.Games
{
    /// <summary>
    /// 眼动训练讲解.xaml 的交互逻辑
    /// </summary>
    public partial class 眼动训练讲解 : BaseUserControl
    {
        private string[] JudgementPath = new string[]
        {
            "SAKA/judgement/right.png",
            "SAKA/judgement/wrong.png"
        };
        private readonly string[][] imagePaths_specific = new string[][]
       {

            new string[]
            {
                "Img4/left/1/1.png",
                "Img4/left/1/2.png",
                "Img4/left/1/5.png",
                "Img4/left/1/6.png",
            },
            new string[] {
                "Img4/right/1/3.png",
                "Img4/right/1/4.png",
                "Img4/right/1/7.png",
                "Img4/right/1/8.png",
            },
            new string[] {
                "Img4/left/2/1.png",
                "Img4/left/2/2.png",
                "Img4/left/2/5.png",
                "Img4/left/2/6.png",
            }
       };
        private string currentDirectory = Directory.GetCurrentDirectory();

        private double INCREASE; // 提高难度的准确率
        private double DECREASE;  // 降低难度的错误率
        private int TRAIN_TIME; // 训练持续时间（单位：秒）
        private bool IS_RESTRICT_TIME = true; // 限制练习时间是否启用
        private bool IS_BEEP = true;
        private int max_time;
        private int train_time;
        private int counter;
        private int hardness;
        private const int MAX_HARDNESS = 34; // 难度上限
        private Random random;
        private int[] correctAnswers; // 存储每个难度的正确答案数量
        private int[] wrongAnswers; // 存储每个难度的错误答案数量
        private int[] igonreAnswer;
        private DispatcherTimer timer;
        private int remainingTime;
        private int randomNumber = 0; //特殊情况的判断变量
        private DispatcherTimer trainingTimer; // 新的计时器用于训练时间
        Random random_tg = new Random();
        private double x_ct, y_ct;
        private double x_tg, y_tg;
        private int count = 0;
        private List<bool> boolList = new List<bool>(5);

        public Action GameBeginAction { get; set; }

        public Func<string, Task> VoicePlayFunc { get; set; }

        public 眼动训练讲解()
        {
            InitializeComponent();

            max_time = 60;
            INCREASE = 0; // 提高难度的阈值
            DECREASE = 0;  // 降低难度的阈值
            TRAIN_TIME = 60; // 训练持续时间（单位：秒）
            train_time = TRAIN_TIME;
            IS_RESTRICT_TIME = true; // 限制练习时间是否启用
            IS_BEEP = true;
            hardness = 1;
            remainingTime = max_time;
            correctAnswers = new int[MAX_HARDNESS + 1];
            wrongAnswers = new int[MAX_HARDNESS + 1];
            igonreAnswer = new int[MAX_HARDNESS + 1];
            string targetDirectory = Path.Combine(currentDirectory, "Img");

            for (int i = 0; i < correctAnswers.Length; i++)
            {
                correctAnswers[i] = 0;
                wrongAnswers[i] = 0;
                igonreAnswer[i] = 0;
            }

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            trainingTimer = new DispatcherTimer();
            trainingTimer.Interval = TimeSpan.FromSeconds(1);
            trainingTimer.Tick += TrainingTimer_Tick;
            trainingTimer.Start(); // 启动训练计时器
           

            this.Loaded += 眼动训练讲解_Loaded;


        }

        private void 眼动训练讲解_Loaded(object sender, RoutedEventArgs e)
        {
            // 页面加载时确保按键和焦点行为
            Button_2_Click(null, null);
            this.Focus();  // 确保焦点在窗口上
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            remainingTime--;

            if (remainingTime <= 0)
            {
                timer.Stop();
                igonreAnswer[hardness]++;
                LoadImages();
                remainingTime = max_time;
                timer.Start();
            }
        }

        private void TrainingTimer_Tick(object sender, EventArgs e)
        {
            train_time--; // 训练时间倒计时

            if (train_time <= 0)
            {
                timer.Stop(); // 停止主计时器
                trainingTimer.Stop(); // 停止训练计时器
                this.Close(); // 关闭当前窗口
            }
        }
        private void Close()
        {
            //throw new NotImplementedException();
        }


        private void LoadImages()
        {
            // 清空之前的图片
            ImageGrid.Children.Clear();

            double centerX = ImageGrid.ActualWidth / 2;
            double centerY = ImageGrid.ActualHeight / 2;
            Image image_tg = new Image();
            Image image_ct = new Image();


            Random random = new Random();
            randomNumber = random.Next(2);
            if (randomNumber == 0)
            {
                Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);

                int arrayLength = imagePaths_specific[0].Length;
                //这里的index需要根据难度读取一定范围的图片
                int index1 = random1.Next(arrayLength);
                Image image = new Image
                {
                    Source = new BitmapImage(new Uri(imagePaths_specific[0][index1], UriKind.Relative)),
                    Stretch = System.Windows.Media.Stretch.Uniform
                };
                ImageGrid.Children.Add(image);

            }
            else if (randomNumber == 1)
            {
                Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);

                int arrayLength = imagePaths_specific[1].Length;
                //这里的index需要根据难度读取一定范围的图片
                int index1 = random1.Next(arrayLength);
                Image image = new Image
                {
                    Source = new BitmapImage(new Uri(imagePaths_specific[1][index1], UriKind.Relative)),
                    Stretch = System.Windows.Media.Stretch.Uniform
                };
                ImageGrid.Children.Add(image);
            }



        }







        //同样的这里也是要额外的分开……
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 处理左箭头按键
            if (e.Key == Key.Left)
            {
                e.Handled = true; // 阻止其他控件处理此事件
                bool isCorrect = randomNumber == 0;
                HandleAnswer(isCorrect);
            }
            // 处理右箭头按键
            else if (e.Key == Key.Right)
            {
                e.Handled = true; // 阻止其他控件处理此事件
                bool isCorrect = randomNumber == 1;
                HandleAnswer(isCorrect);
            }
        }

        private void HandleAnswer(bool isCorrect)
        {
            if (isCorrect)
            {
                //textblock.Text = "恭喜你答对了！";
                //textblock.Foreground = new SolidColorBrush(Colors.Green);
                judgement.Source = new BitmapImage(new Uri(JudgementPath[0], UriKind.Relative));
                //textblock.Margin = new Thickness(500,720,0,0);
                OkButton.Visibility = Visibility.Visible;
            }
            else
            {
                if (IS_BEEP)
                    Console.Beep();

                //textblock.Text = "很遗憾答错了！";
                //textblock.Foreground = new SolidColorBrush(Colors.Red);
                judgement.Source = new BitmapImage(new Uri(JudgementPath[1], UriKind.Relative));
                //textblock.Margin = new Thickness(500, 720, 0, 0);
                OkButton.Visibility = Visibility.Collapsed;
            }
            LoadImages();
        }




        private void ImageGrid_Loaded(object sender, RoutedEventArgs e)
        {
            LoadImages(); // 在这里调用 LoadImages 确保 ImageGrid 的大小已经初始化
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
                        Image_2.Visibility = Visibility.Visible;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;

                        // 隐藏试玩部分内容
                        ImageGrid.Visibility = Visibility.Collapsed;
                        //textblock.Visibility = Visibility.Collapsed;

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
                        Image_2.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Visible;
                        Image_3.Visibility = Visibility.Visible;



                        // 隐藏试玩部分的控件
                        ImageGrid.Visibility = Visibility.Collapsed;
                        //textblock.Visibility = Visibility.Collapsed;
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
                        // 进入试玩界面
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;

                        // 显示试玩部分的控件
                        ImageGrid.Visibility = Visibility.Visible;
                        //textblock.Visibility = Visibility.Visible;
                        // 隐藏讲解部分的按钮
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Visible;
                        Button_3.Margin = new Thickness(550, 900, 0, 0);
                        this.Focusable = true;
                        this.Focus();  // 将焦点放在 UserControl 上
                        Keyboard.Focus(this);  // 立即捕获键盘焦点

                        // 加载图片
                        LoadImages();

                        // 确保 KeyDown 事件立即生效
                        this.KeyDown += Window_PreviewKeyDown;  // 确保按键事件被绑定

                        //LJN，在讲解模块调用委托
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("20241114待补充");//增加代码，调用函数，显示数字人下的文字
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
