using crs.core;
using crs.core.DbModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Threading;

namespace crs.game.Games
{
    /// <summary>
    /// SAKA.xaml 的交互逻辑
    /// </summary>
    public partial class 视空间训练 : BaseUserControl
    {
        private string[] JudgementPath = new string[]
        {
            "SAKA/judgement/right.png",
            "SAKA/judgement/wrong.png"
        };
        private readonly string[][] imagePaths_bg = new string[][]
        {

            new string[]
            {
                "SAKA/n/1.JPG",
                "SAKA/n/2.JPG",
                "SAKA/n/3.png",
                "SAKA/n/4.png",
                "SAKA/n/5.png",
                "SAKA/n/6.png",
                "SAKA/n/7.png",
                "SAKA/n/8.png",
                "SAKA/n/9.png",
                "SAKA/n/10.png",
                "SAKA/n/11.png",
                "SAKA/n/12.png",
                "SAKA/n/13.png",
                "SAKA/n/14.png",
                "SAKA/n/15.png",
                "SAKA/n/16.png",
                "SAKA/n/17.png",
                "SAKA/n/18.png",
                "SAKA/n/19.png",
                "SAKA/n/20.png",
                "SAKA/n/21.png",
                "SAKA/n/22.png",
                "SAKA/n/23.png",
                "SAKA/n/24.png",
                "SAKA/n/25.png",
                "SAKA/n/26.png",
                "SAKA/n/27.png",
                "SAKA/n/28.png",
                "SAKA/n/29.png",
                "SAKA/n/30.png",
                "SAKA/n/31.png",
                "SAKA/n/32.png",
                "SAKA/n/33.png",
                "SAKA/n/34.png"
            }
        };
        private readonly string[][] imagePaths_tg = new string[][]
{
    new string[]
    {
        "SAKA/Img_tg/1/1.png",
        "SAKA/Img_tg/1/2.png",
        "SAKA/Img_tg/1/3.png",
        "SAKA/Img_tg/1/4.png",
        "SAKA/Img_tg/1/5.png",
        "SAKA/Img_tg/1/6.png",
    },
    new string[]
    {
        "SAKA/Img_tg/2/1.png",
        "SAKA/Img_tg/2/2.png",
        "SAKA/Img_tg/2/3.png",
        "SAKA/Img_tg/2/4.png",
    }
};
        private readonly string[][] imagePaths_ct = new string[][]
        {
    new string[]
    {
        "SAKA/Img_ct/1/1.png",
        "SAKA/Img_ct/1/2.png",
        "SAKA/Img_ct/1/3.png",
    },
    new string[]
    {
        "SAKA/Img_ct/2/1.png",
        "SAKA/Img_ct/2/2.png",
    }
        };
        //难度1, 2, 3，6，9，14, 19, 20非常特殊需要用完全不同的逻辑写 详情见函数LoadImages()
        private readonly string[][] imagePaths_specific = new string[][]
{
    new string[]
    {
        "SAKA/left/1/1.png",
        "SAKA/left/1/2.png",
        "SAKA/left/1/5.png",
        "SAKA/left/1/6.png",
    },
    new string[] {
        "SAKA/right/1/3.png",
        "SAKA/right/1/4.png",
        "SAKA/right/1/7.png",
        "SAKA/right/1/8.png",
    },
    new string[] {
        "SAKA/left/2/1.png",
        "SAKA/left/2/2.png",
        "SAKA/left/2/5.png",
        "SAKA/left/2/6.png",
    },
    new string[] {
        "SAKA/right/2/3.png",
        "SAKA/right/2/4.png",
        "SAKA/right/2/7.png",
        "SAKA/right/2/8.png",
    },
    new string[] {
        "SAKA/left/3/1.png",
        "SAKA/left/3/2.png",
        "SAKA/left/3/5.png",
        "SAKA/left/3/6.png",
    },
    new string[] {
        "SAKA/right/3/3.png",
        "SAKA/right/3/4.png",
        "SAKA/right/3/7.png",
        "SAKA/right/3/8.png",
    },
    new string[] {
        "SAKA/left/6/1.png",
        "SAKA/left/6/2.png",
        "SAKA/left/6/5.png",
        "SAKA/left/6/6.png",
    },
    new string[] {
        "SAKA/right/6/3.png",
        "SAKA/right/6/4.png",
        "SAKA/right/6/7.png",
        "SAKA/right/6/8.png",
    },
    new string[] {
        "SAKA/left/9/1.png",
        "SAKA/left/9/2.png",
        "SAKA/left/9/5.png",
        "SAKA/left/9/6.png",
    },
    new string[] {
        "SAKA/right/9/3.png",
        "SAKA/right/9/4.png",
        "SAKA/right/9/7.png",
        "SAKA/right/9/8.png",
    },
    new string[] {
        "SAKA/left/14/1.png",
        "SAKA/left/14/2.png",
        "SAKA/left/14/5.png",
        "SAKA/left/14/6.png",
    },
    new string[] {
        "SAKA/right/14/3.png",
        "SAKA/right/14/4.png",
        "SAKA/right/14/7.png",
        "SAKA/right/14/8.png",
    },
    new string[] {
        "SAKA/left/19/1.png",
        "SAKA/left/19/2.png",
        "SAKA/left/19/3.png",
        "SAKA/left/19/7.png",
        "SAKA/left/19/8.png",
        "SAKA/left/19/9.png",
    },
    new string[] {
        "SAKA/right/19/10.png",
        "SAKA/right/19/11.png",
        "SAKA/right/19/12.png",
        "SAKA/right/19/4.png",
        "SAKA/right/19/5.png",
        "SAKA/right/19/6.png",
    },
    new string[] {
        "SAKA/left/20/1.png",
        "SAKA/left/20/2.png",
        "SAKA/left/20/3.png",
        "SAKA/left/20/7.png",
        "SAKA/left/20/8.png",
        "SAKA/left/20/9.png",
    },
    new string[] {
        "SAKA/right/20/10.png",
        "SAKA/right/20/11.png",
        "SAKA/right/20/12.png",
        "SAKA/right/20/4.png",
        "SAKA/right/20/5.png",
        "SAKA/right/20/6.png",
    }
};

        private string currentDirectory = Directory.GetCurrentDirectory();

        private int TRAIN_TIME; // 训练持续时间（单位：秒）
        private int max_time;
        private int train_time;
        private int hardness;
        int max_hardness = 0;
        private int remainingTime;
        private int costTime;
        private int randomNumber = 0; //特殊情况的判断变量
        private int LevelUp = 3;
        private int LevelDown = 5;
        private int TotalCount = 0;
        private int stimulus_interval = 3;//刺激间隔

        private const int MAX_HARDNESS = 34; // 难度上限

        private bool IS_RESTRICT_TIME = true; // 限制练习时间是否启用
        private bool IS_BEEP = true;
        private bool is_game = true;
        private bool is_auto = true; // 是否自动跳转
        private bool is_enter = false;

        private DispatcherTimer timer;
        private DispatcherTimer trainingTimer; // 新的计时器用于训练时间
        private DispatcherTimer countTimer;
        private DispatcherTimer StarTimer = new DispatcherTimer();//用来显示治疗师端的星星
        Random random_tg = new Random();

        private double x_ct, y_ct;
        private double x_tg, y_tg;
        private int count = 0;

        private List<int>[] RightCostTime;
        private List<int>[] LeftCostTime;
        private List<int>[] ComplexCostTime;

        private int[] TotalLeftCount;
        private int[] TotalRightCount;
        private int[] TotalCorrectLeftCount;
        private int[] TotalCorrectRightCount;
        private int[] ComplexCount;
        private int[] ComplexCorrectCount;
        private int[] ComplexRightUpCount;
        private int[] ComplexLeftUpCount;
        private int[] ComplexRightDownCount;
        private int[] ComplexLeftDownCount;
        private int[] correctAnswers; // 存储每个难度的正确答案数量
        private int[] wrongAnswers; // 存储每个难度的错误答案数量
        private int[] ignoreAnswers;

        private double[] TotalLeftAccuracy;
        private double[] TotalRightAccuracy;
        private double[] ComplexAccuracy;
        private double[] AverageRightCostTime;
        private double[] AverageLeftCostTime;
        private double[] ComplexAverageTime;

        // 在类中添加一个列表来存储最近5道题的结果
        private List<bool> recentResults = new List<bool>();


        //加上反馈
        //反馈
        private SoundPlayer soundPlayer; // 用来放歌
        public string ErrorSoundPath;//错误的声音文件路径，在OnStartAsync()中配置
        public string CorrectSoundPath;//正确的声音文件路径，在OnStartAsync()中配置
        private int StopDurations = 2000; // 停止时间，ms
        private string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory; // 当前项目的绝对路径
        private string ResourcesPath = System.IO.Path.Combine("Resources", "词语记忆力");//这里用的固定是词语记忆力的路径，后期可改

        private void FeedBackInit()
        {
            /*LJN
            配置反馈资源的路径             
             */
            CorrectSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");
            // 为 Image 控件加载图片 Source
            CorrectImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Correct.png"), UriKind.RelativeOrAbsolute));
            ErrorImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Error.png"), UriKind.RelativeOrAbsolute));

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

        private async void ShowFeedbackImage(Image image)
        {//显示反馈的图片
            image.Visibility = Visibility.Visible;

            // 延迟指定的时间（例如1秒）
            await Task.Delay(stimulus_interval * 1000);

            image.Visibility = Visibility.Collapsed;
        }

        public 视空间训练()
        {
            InitializeComponent();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            remainingTime--;
            costTime++;

            TimeStatisticsAction.Invoke(train_time, remainingTime);
            if (remainingTime <= 0)
            {
                timer.Stop();
                ignoreAnswers[hardness]++;
                LoadImages();
                remainingTime = max_time;
            }

        }
        private void TrainingTimer_Tick(object sender, EventArgs e)
        {
            train_time--; // 训练时间倒计时

            TimeStatisticsAction.Invoke(train_time, remainingTime);

            if (train_time <= 0)
            {
                timer.Stop(); // 停止主计时器
                trainingTimer.Stop(); // 停止训练计时器
                OnGameEnd();

            }
        }
        private void Countimer_Tick(object sender, EventArgs e)
        {
            countTimer?.Stop();
            LoadImages();
            judgement.Source = null;
            is_enter = false;
        }
        private void LoadImages()
        {
            is_enter = false;
            remainingTime = max_time;
            timer.Start();
            // 清空之前的图片
            ImageGrid.Children.Clear();

            double centerX = ImageGrid.ActualWidth / 2;
            double centerY = ImageGrid.ActualHeight / 2;
            double angle = 0; // 0 到 2π
            double radius;
            int tg_width = 300;
            int tg_height = 200;
            int ct_width = 90;
            int ct_height = 90;
            int horizon = 0;
            Image image_tg = new Image();
            Image image_ct = new Image();
            //特殊难度代码
            if (hardness == 1 || hardness == 2 || hardness == 3 || hardness == 6 || hardness == 9 || hardness == 14 || hardness == 19 || hardness == 20)
            {
                //特殊的情况是难度1，2，3，6，9，19，20 都要用全新的逻辑去写
                if (hardness == 1)
                {
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

                else if (hardness == 2)
                {
                    Random random = new Random();
                    randomNumber = random.Next(2);
                    if (randomNumber == 0)
                    {
                        Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);

                        int arrayLength = imagePaths_specific[2].Length;
                        //这里的index需要根据难度读取一定范围的图片
                        int index1 = random1.Next(arrayLength);
                        Image image = new Image
                        {
                            Source = new BitmapImage(new Uri(imagePaths_specific[2][index1], UriKind.Relative)),
                            Stretch = System.Windows.Media.Stretch.Uniform
                        };
                        ImageGrid.Children.Add(image);

                    }
                    if (randomNumber == 1)
                    {
                        Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);

                        int arrayLength = imagePaths_specific[3].Length;
                        //这里的index需要根据难度读取一定范围的图片
                        int index1 = random1.Next(arrayLength);
                        Image image = new Image
                        {
                            Source = new BitmapImage(new Uri(imagePaths_specific[3][index1], UriKind.Relative)),
                            Stretch = System.Windows.Media.Stretch.Uniform
                        };
                        ImageGrid.Children.Add(image);
                    }
                }
                else if (hardness == 3)
                {
                    Random random = new Random();
                    randomNumber = random.Next(2);
                    if (randomNumber == 0)
                    {
                        Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);

                        int arrayLength = imagePaths_specific[4].Length;
                        //这里的index需要根据难度读取一定范围的图片
                        int index1 = random1.Next(arrayLength);
                        Image image = new Image
                        {
                            Source = new BitmapImage(new Uri(imagePaths_specific[4][index1], UriKind.Relative)),
                            Stretch = System.Windows.Media.Stretch.Uniform
                        };
                        ImageGrid.Children.Add(image);

                    }
                    if (randomNumber == 1)
                    {
                        Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);

                        int arrayLength = imagePaths_specific[5].Length;
                        //这里的index需要根据难度读取一定范围的图片
                        int index1 = random1.Next(arrayLength);
                        Image image = new Image
                        {
                            Source = new BitmapImage(new Uri(imagePaths_specific[5][index1], UriKind.Relative)),
                            Stretch = System.Windows.Media.Stretch.Uniform
                        };
                        ImageGrid.Children.Add(image);
                    }
                }
                else if (hardness == 6)
                {
                    Random random = new Random();
                    randomNumber = random.Next(2);
                    if (randomNumber == 0)
                    {
                        Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);

                        int arrayLength = imagePaths_specific[6].Length;
                        //这里的index需要根据难度读取一定范围的图片
                        int index1 = random1.Next(arrayLength);
                        Image image = new Image
                        {
                            Source = new BitmapImage(new Uri(imagePaths_specific[6][index1], UriKind.Relative)),
                            Stretch = System.Windows.Media.Stretch.Uniform
                        };
                        ImageGrid.Children.Add(image);

                    }
                    if (randomNumber == 1)
                    {
                        Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);

                        int arrayLength = imagePaths_specific[7].Length;
                        //这里的index需要根据难度读取一定范围的图片
                        int index1 = random1.Next(arrayLength);
                        Image image = new Image
                        {
                            Source = new BitmapImage(new Uri(imagePaths_specific[7][index1], UriKind.Relative)),
                            Stretch = System.Windows.Media.Stretch.Uniform
                        };
                        ImageGrid.Children.Add(image);
                    }
                }

                else if (hardness == 9)
                {
                    Random random = new Random();
                    randomNumber = random.Next(2);
                    if (randomNumber == 0)
                    {
                        Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);

                        int arrayLength = imagePaths_specific[8].Length;
                        //这里的index需要根据难度读取一定范围的图片
                        int index1 = random1.Next(arrayLength);
                        Image image = new Image
                        {
                            Source = new BitmapImage(new Uri(imagePaths_specific[8][index1], UriKind.Relative)),
                            Stretch = System.Windows.Media.Stretch.Uniform
                        };
                        ImageGrid.Children.Add(image);

                    }
                    if (randomNumber == 1)
                    {
                        Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);

                        int arrayLength = imagePaths_specific[9].Length;
                        //这里的index需要根据难度读取一定范围的图片
                        int index1 = random1.Next(arrayLength);
                        Image image = new Image
                        {
                            Source = new BitmapImage(new Uri(imagePaths_specific[9][index1], UriKind.Relative)),
                            Stretch = System.Windows.Media.Stretch.Uniform
                        };
                        ImageGrid.Children.Add(image);
                    }
                }

                else if (hardness == 14)
                {
                    Random random = new Random();
                    randomNumber = random.Next(2);
                    if (randomNumber == 0)
                    {
                        Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);

                        int arrayLength = imagePaths_specific[10].Length;
                        //这里的index需要根据难度读取一定范围的图片
                        int index1 = random1.Next(arrayLength);
                        Image image = new Image
                        {
                            Source = new BitmapImage(new Uri(imagePaths_specific[10][index1], UriKind.Relative)),
                            Stretch = System.Windows.Media.Stretch.Uniform
                        };
                        ImageGrid.Children.Add(image);

                    }
                    if (randomNumber == 1)
                    {
                        Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);

                        int arrayLength = imagePaths_specific[11].Length;
                        //这里的index需要根据难度读取一定范围的图片
                        int index1 = random1.Next(arrayLength);
                        Image image = new Image
                        {
                            Source = new BitmapImage(new Uri(imagePaths_specific[12][index1], UriKind.Relative)),
                            Stretch = System.Windows.Media.Stretch.Uniform
                        };
                        ImageGrid.Children.Add(image);
                    }
                }
                else if (hardness == 19)
                {
                    Random random = new Random();
                    randomNumber = random.Next(2);
                    if (randomNumber == 0)
                    {
                        Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);

                        int arrayLength = imagePaths_specific[12].Length;
                        //这里的index需要根据难度读取一定范围的图片
                        int index1 = random1.Next(arrayLength);
                        Image image = new Image
                        {
                            Source = new BitmapImage(new Uri(imagePaths_specific[12][index1], UriKind.Relative)),
                            Stretch = System.Windows.Media.Stretch.Uniform
                        };
                        ImageGrid.Children.Add(image);

                    }
                    if (randomNumber == 1)
                    {
                        Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);

                        int arrayLength = imagePaths_specific[13].Length;
                        //这里的index需要根据难度读取一定范围的图片
                        int index1 = random1.Next(arrayLength);
                        Image image = new Image
                        {
                            Source = new BitmapImage(new Uri(imagePaths_specific[13][index1], UriKind.Relative)),
                            Stretch = System.Windows.Media.Stretch.Uniform
                        };
                        ImageGrid.Children.Add(image);
                    }
                }
                else if (hardness == 20)
                {
                    Random random = new Random();
                    randomNumber = random.Next(2);
                    if (randomNumber == 0)
                    {
                        Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);

                        int arrayLength = imagePaths_specific[14].Length;
                        //这里的index需要根据难度读取一定范围的图片
                        int index1 = random1.Next(arrayLength);
                        Image image = new Image
                        {
                            Source = new BitmapImage(new Uri(imagePaths_specific[14][index1], UriKind.Relative)),
                            Stretch = System.Windows.Media.Stretch.Uniform
                        };
                        ImageGrid.Children.Add(image);

                    }
                    if (randomNumber == 1)
                    {
                        Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);

                        int arrayLength = imagePaths_specific[15].Length;
                        //这里的index需要根据难度读取一定范围的图片
                        int index1 = random1.Next(arrayLength);
                        Image image = new Image
                        {
                            Source = new BitmapImage(new Uri(imagePaths_specific[15][index1], UriKind.Relative)),
                            Stretch = System.Windows.Media.Stretch.Uniform
                        };
                        ImageGrid.Children.Add(image);
                    }
                }
                if (randomNumber == 0)
                {
                    TotalLeftCount[hardness - 1]++;
                }
                if (randomNumber == 1)
                {
                    TotalRightCount[hardness - 1]++;
                }
            }
            //通用情况的代码
            else
            {
                if (hardness >= 4 && hardness <= 5)
                {
                    horizon = 50;
                    Image image_bg = new Image
                    {
                        Source = new BitmapImage(new Uri(imagePaths_bg[0][hardness - 1], UriKind.Relative)),
                        Stretch = System.Windows.Media.Stretch.Uniform
                    };
                    ImageGrid.Children.Add(image_bg);


                    // 加载目标图像
                    Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);
                    int arraylength1 = imagePaths_tg[0].Length;
                    int index1 = random1.Next(arraylength1);
                    image_tg.Source = new BitmapImage(new Uri(imagePaths_tg[0][index1], UriKind.Relative));
                    image_tg.Stretch = System.Windows.Media.Stretch.Uniform;
                    image_tg.Width = tg_width;
                    image_tg.Height = tg_height;

                    angle = random_tg.NextDouble() * 2 * Math.PI;
                    radius = random_tg.NextDouble() * horizon;
                    x_tg = radius * Math.Cos(angle) - image_tg.Width / 2;
                    y_tg = radius * Math.Sin(angle) - image_tg.Height / 2;

                    Random random_ct = new Random();
                    // 加载基准点（太阳/月亮）

                    Random random2 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);
                    int arraylength2 = imagePaths_ct[0].Length;
                    int index2 = random1.Next(arraylength2);
                    image_ct.Source = new BitmapImage(new Uri(imagePaths_ct[0][index2], UriKind.Relative));
                    image_ct.Stretch = System.Windows.Media.Stretch.Uniform;
                    image_ct.Width = ct_width;
                    image_ct.Height = ct_height;

                    angle = random_ct.NextDouble() * 2 * Math.PI;
                    radius = random_ct.NextDouble() * horizon;
                    x_ct = radius * Math.Cos(angle) - image_ct.Width / 2;
                    y_ct = radius * Math.Sin(angle) - image_ct.Height / 2;
                    if (x_ct - x_tg > 0 && x_ct - x_tg < 50)
                    {
                        x_tg -= 50;
                    }
                    if (x_tg - x_ct > 0 && x_tg - x_ct < 50)
                    {
                        x_ct -= 50;
                    }
                    if (y_ct - y_tg > 0 && y_tg - y_ct < 50)
                    {
                        y_tg -= 50;
                    }
                    if (y_tg - y_ct > 0 && y_tg - y_ct < 50)
                    {
                        y_ct -= 50;
                    }
                    image_ct.Margin = new Thickness(x_ct, y_ct, 0, 0);
                    ImageGrid.Children.Add(image_ct);
                    image_tg.Margin = new Thickness(x_tg, y_tg, 0, 0);
                    ImageGrid.Children.Add(image_tg);
                }
                if (hardness >= 7 && hardness <= 8)
                {
                    horizon = 150;
                    Image image_bg = new Image
                    {
                        Source = new BitmapImage(new Uri(imagePaths_bg[0][hardness - 1], UriKind.Relative)),
                        Stretch = System.Windows.Media.Stretch.Uniform
                    };
                    ImageGrid.Children.Add(image_bg);


                    // 加载目标图像
                    Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);
                    int arraylength1 = imagePaths_tg[0].Length;
                    int index1 = random1.Next(arraylength1);
                    image_tg.Source = new BitmapImage(new Uri(imagePaths_tg[0][index1], UriKind.Relative));
                    image_tg.Stretch = System.Windows.Media.Stretch.Uniform;
                    image_tg.Width = tg_width;
                    image_tg.Height = tg_height;

                    angle = random_tg.NextDouble() * 2 * Math.PI;
                    radius = random_tg.NextDouble() * horizon;
                    x_tg = radius * Math.Cos(angle) - image_tg.Width / 2;
                    y_tg = radius * Math.Sin(angle) - image_tg.Height / 2;

                    Random random_ct = new Random();
                    // 加载基准点（太阳/月亮）

                    Random random2 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);
                    int arraylength2 = imagePaths_ct[0].Length;
                    int index2 = random2.Next(arraylength2);
                    image_ct.Source = new BitmapImage(new Uri(imagePaths_ct[0][index2], UriKind.Relative));
                    image_ct.Stretch = System.Windows.Media.Stretch.Uniform;
                    image_ct.Width = ct_width;
                    image_ct.Height = ct_height;

                    angle = random_ct.NextDouble() * 2 * Math.PI;
                    radius = random_ct.NextDouble() * horizon;
                    x_ct = radius * Math.Cos(angle) - image_ct.Width / 2;
                    y_ct = radius * Math.Sin(angle) - image_ct.Height / 2;
                    if (x_ct - x_tg > 0 && x_ct - x_tg < 50)
                    {
                        x_tg -= 50;
                    }
                    if (x_tg - x_ct > 0 && x_tg - x_ct < 50)
                    {
                        x_ct -= 50;
                    }
                    if (y_ct - y_tg > 0 && y_tg - y_ct < 50)
                    {
                        y_tg -= 50;
                    }
                    if (y_tg - y_ct > 0 && y_tg - y_ct < 50)
                    {
                        y_ct -= 50;
                    }
                    image_ct.Margin = new Thickness(x_ct, y_ct, 0, 0);
                    ImageGrid.Children.Add(image_ct);
                    image_tg.Margin = new Thickness(x_tg, y_tg, 0, 0);
                    ImageGrid.Children.Add(image_tg);
                }
                if (hardness >= 10 && hardness <= 11)
                {
                    horizon = 250;
                    Image image_bg = new Image
                    {
                        Source = new BitmapImage(new Uri(imagePaths_bg[0][hardness - 1], UriKind.Relative)),
                        Stretch = System.Windows.Media.Stretch.Uniform
                    };
                    ImageGrid.Children.Add(image_bg);


                    // 加载目标图像
                    Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);
                    int arraylength1 = imagePaths_tg[0].Length;
                    int index1 = random1.Next(arraylength1);
                    image_tg.Source = new BitmapImage(new Uri(imagePaths_tg[0][index1], UriKind.Relative));
                    image_tg.Stretch = System.Windows.Media.Stretch.Uniform;
                    image_tg.Width = tg_width;
                    image_tg.Height = tg_height;

                    angle = random_tg.NextDouble() * 2 * Math.PI;
                    radius = random_tg.NextDouble() * horizon;
                    x_tg = radius * Math.Cos(angle) - image_tg.Width / 2;
                    y_tg = radius * Math.Sin(angle) - image_tg.Height / 2;

                    Random random_ct = new Random();
                    // 加载基准点（太阳/月亮）

                    Random random2 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);
                    int arraylength2 = imagePaths_ct[0].Length;
                    int index2 = random1.Next(arraylength2);
                    image_ct.Source = new BitmapImage(new Uri(imagePaths_ct[0][index2], UriKind.Relative));
                    image_ct.Stretch = System.Windows.Media.Stretch.Uniform;
                    image_ct.Width = ct_width;
                    image_ct.Height = ct_height;

                    angle = random_ct.NextDouble() * 2 * Math.PI;
                    radius = random_ct.NextDouble() * horizon;
                    x_ct = radius * Math.Cos(angle) - image_ct.Width / 2;
                    y_ct = radius * Math.Sin(angle) - image_ct.Height / 2;
                    if (x_ct - x_tg > 0 && x_ct - x_tg < 50)
                    {
                        x_tg -= 50;
                    }
                    if (x_tg - x_ct > 0 && x_tg - x_ct < 50)
                    {
                        x_ct -= 50;
                    }
                    if (y_ct - y_tg > 0 && y_tg - y_ct < 50)
                    {
                        y_tg -= 50;
                    }
                    if (y_tg - y_ct > 0 && y_tg - y_ct < 50)
                    {
                        y_ct -= 50;
                    }
                    image_ct.Margin = new Thickness(x_ct, y_ct, 0, 0);
                    ImageGrid.Children.Add(image_ct);
                    image_tg.Margin = new Thickness(x_tg, y_tg, 0, 0);
                    ImageGrid.Children.Add(image_tg);
                }
                //轮廓背景为草地 目标为蝴蝶
                if (hardness >= 12 && hardness <= 13)
                {
                    horizon = 250;
                    Image image_bg = new Image
                    {
                        Source = new BitmapImage(new Uri(imagePaths_bg[0][hardness - 1], UriKind.Relative)),
                        Stretch = System.Windows.Media.Stretch.Uniform
                    };
                    ImageGrid.Children.Add(image_bg);


                    // 加载目标图像
                    Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);
                    int arraylength1 = imagePaths_tg[1].Length;
                    int index1 = random1.Next(arraylength1);
                    image_tg.Source = new BitmapImage(new Uri(imagePaths_tg[1][index1], UriKind.Relative));
                    image_tg.Stretch = System.Windows.Media.Stretch.Uniform;
                    image_tg.Width = tg_width;
                    image_tg.Height = tg_height;

                    angle = random_tg.NextDouble() * 2 * Math.PI;
                    radius = random_tg.NextDouble() * horizon;
                    x_tg = radius * Math.Cos(angle) - image_tg.Width / 2;
                    y_tg = radius * Math.Sin(angle) - image_tg.Height / 2;

                    Random random_ct = new Random();
                    // 加载基准点（太阳/月亮）

                    Random random2 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);
                    int length = imagePaths_ct[1].Length;
                    int index2 = random1.Next(length);
                    image_ct.Source = new BitmapImage(new Uri(imagePaths_ct[1][index2], UriKind.Relative));
                    image_ct.Stretch = System.Windows.Media.Stretch.Uniform;
                    image_ct.Width = ct_width;
                    image_ct.Height = ct_height;

                    angle = random_ct.NextDouble() * 2 * Math.PI;
                    radius = random_ct.NextDouble() * horizon;
                    x_ct = radius * Math.Cos(angle) - image_ct.Width / 2;
                    y_ct = radius * Math.Sin(angle) - image_ct.Height / 2;
                    if (x_ct - x_tg > 0 && x_ct - x_tg < 50)
                    {
                        x_tg -= 50;
                    }
                    if (x_tg - x_ct > 0 && x_tg - x_ct < 50)
                    {
                        x_ct -= 50;
                    }
                    if (y_ct - y_tg > 0 && y_tg - y_ct < 50)
                    {
                        y_tg -= 50;
                    }
                    if (y_tg - y_ct > 0 && y_tg - y_ct < 50)
                    {
                        y_ct -= 50;
                    }
                    image_ct.Margin = new Thickness(x_ct, y_ct, 0, 0);
                    ImageGrid.Children.Add(image_ct);
                    image_tg.Margin = new Thickness(x_tg, y_tg, 0, 0);
                    ImageGrid.Children.Add(image_tg);
                }
                if (hardness >= 15 && hardness <= 16)
                {
                    horizon = 250;
                    Image image_bg = new Image
                    {
                        Source = new BitmapImage(new Uri(imagePaths_bg[0][hardness - 1], UriKind.Relative)),
                        Stretch = System.Windows.Media.Stretch.Uniform
                    };
                    ImageGrid.Children.Add(image_bg);


                    // 加载目标图像
                    Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);
                    int arraylength1 = imagePaths_tg[0].Length;
                    int index1 = random1.Next(arraylength1);
                    image_tg.Source = new BitmapImage(new Uri(imagePaths_tg[0][index1], UriKind.Relative));
                    image_tg.Stretch = System.Windows.Media.Stretch.Uniform;
                    image_tg.Width = tg_width;
                    image_tg.Height = tg_height;

                    angle = random_tg.NextDouble() * 2 * Math.PI;
                    radius = random_tg.NextDouble() * horizon;
                    x_tg = radius * Math.Cos(angle) - image_tg.Width / 2;
                    y_tg = radius * Math.Sin(angle) - image_tg.Height / 2;

                    Random random_ct = new Random();
                    // 加载基准点（太阳/月亮）

                    Random random2 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);
                    int arraylength2 = imagePaths_ct[0].Length;
                    int index2 = random1.Next(arraylength2);
                    image_ct.Source = new BitmapImage(new Uri(imagePaths_ct[0][index2], UriKind.Relative));
                    image_ct.Stretch = System.Windows.Media.Stretch.Uniform;
                    image_ct.Width = ct_width;
                    image_ct.Height = ct_height;

                    angle = random_ct.NextDouble() * 2 * Math.PI;
                    radius = random_ct.NextDouble() * horizon;
                    x_ct = radius * Math.Cos(angle) - image_ct.Width / 2;
                    y_ct = radius * Math.Sin(angle) - image_ct.Height / 2;
                    if (x_ct - x_tg > 0 && x_ct - x_tg < 50)
                    {
                        x_tg -= 50;
                    }
                    if (x_tg - x_ct > 0 && x_tg - x_ct < 50)
                    {
                        x_ct -= 50;
                    }
                    if (y_ct - y_tg > 0 && y_tg - y_ct < 50)
                    {
                        y_tg -= 50;
                    }
                    if (y_tg - y_ct > 0 && y_tg - y_ct < 50)
                    {
                        y_ct -= 50;
                    }
                    image_ct.Margin = new Thickness(x_ct, y_ct, 0, 0);
                    ImageGrid.Children.Add(image_ct);
                    image_tg.Margin = new Thickness(x_tg, y_tg, 0, 0);
                    ImageGrid.Children.Add(image_tg);
                }

                if (hardness >= 17 && hardness <= 18)
                {
                    horizon = 250;
                    Image image_bg = new Image
                    {
                        Source = new BitmapImage(new Uri(imagePaths_bg[0][hardness - 1], UriKind.Relative)),
                        Stretch = System.Windows.Media.Stretch.Uniform
                    };
                    ImageGrid.Children.Add(image_bg);


                    // 加载目标图像
                    Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);
                    int arraylength1 = imagePaths_tg[1].Length;
                    int index1 = random1.Next(arraylength1);
                    image_tg.Source = new BitmapImage(new Uri(imagePaths_tg[1][index1], UriKind.Relative));
                    image_tg.Stretch = System.Windows.Media.Stretch.Uniform;
                    image_tg.Width = tg_width;
                    image_tg.Height = tg_height;

                    angle = random_tg.NextDouble() * 2 * Math.PI;
                    radius = random_tg.NextDouble() * horizon;
                    x_tg = radius * Math.Cos(angle) - image_tg.Width / 2;
                    y_tg = radius * Math.Sin(angle) - image_tg.Height / 2;

                    Random random_ct = new Random();
                    // 加载基准点（太阳/月亮）

                    Random random2 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);
                    int arraylength2 = imagePaths_ct[1].Length;
                    int index2 = random1.Next(arraylength2);
                    image_ct.Source = new BitmapImage(new Uri(imagePaths_ct[1][index2], UriKind.Relative));
                    image_ct.Stretch = System.Windows.Media.Stretch.Uniform;
                    image_ct.Width = ct_width;
                    image_ct.Height = ct_height;

                    angle = random_ct.NextDouble() * 2 * Math.PI;
                    radius = random_ct.NextDouble() * horizon;
                    x_ct = radius * Math.Cos(angle) - image_ct.Width / 2;
                    y_ct = radius * Math.Sin(angle) - image_ct.Height / 2;
                    if (x_ct - x_tg > 0 && x_ct - x_tg < 50)
                    {
                        x_tg -= 50;
                    }
                    if (x_tg - x_ct > 0 && x_tg - x_ct < 50)
                    {
                        x_ct -= 50;
                    }
                    if (y_ct - y_tg > 0 && y_tg - y_ct < 50)
                    {
                        y_tg -= 50;
                    }
                    if (y_tg - y_ct > 0 && y_tg - y_ct < 50)
                    {
                        y_ct -= 50;
                    }
                    image_ct.Margin = new Thickness(x_ct, y_ct, 0, 0);
                    ImageGrid.Children.Add(image_ct);
                    image_tg.Margin = new Thickness(x_tg, y_tg, 0, 0);
                    ImageGrid.Children.Add(image_tg);
                }
                if (hardness >= 21 && hardness <= 21)
                {
                    horizon = 350;
                    Image image_bg = new Image
                    {
                        Source = new BitmapImage(new Uri(imagePaths_bg[0][hardness - 1], UriKind.Relative)),
                        Stretch = System.Windows.Media.Stretch.Uniform
                    };
                    ImageGrid.Children.Add(image_bg);


                    // 加载目标图像
                    Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);
                    int arraylength1 = imagePaths_tg[0].Length;
                    int index1 = random1.Next(arraylength1);
                    image_tg.Source = new BitmapImage(new Uri(imagePaths_tg[0][index1], UriKind.Relative));
                    image_tg.Stretch = System.Windows.Media.Stretch.Uniform;
                    image_tg.Width = tg_width;
                    image_tg.Height = tg_height;

                    angle = random_tg.NextDouble() * 2 * Math.PI;
                    radius = random_tg.NextDouble() * horizon;
                    x_tg = radius * Math.Cos(angle) - image_tg.Width / 2;
                    y_tg = radius * Math.Sin(angle) - image_tg.Height / 2;

                    Random random_ct = new Random();
                    // 加载基准点（太阳/月亮）

                    Random random2 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);
                    int arraylength2 = imagePaths_ct[0].Length;
                    int index2 = random1.Next(arraylength2);
                    image_ct.Source = new BitmapImage(new Uri(imagePaths_ct[0][index2], UriKind.Relative));
                    image_ct.Stretch = System.Windows.Media.Stretch.Uniform;
                    image_ct.Width = ct_width;
                    image_ct.Height = ct_height;

                    angle = random_ct.NextDouble() * 2 * Math.PI;
                    radius = random_ct.NextDouble() * horizon;
                    x_ct = radius * Math.Cos(angle) - image_ct.Width / 2;
                    y_ct = radius * Math.Sin(angle) - image_ct.Height / 2;
                    if (x_ct - x_tg > 0 && x_ct - x_tg < 50)
                    {
                        x_tg -= 50;
                    }
                    if (x_tg - x_ct > 0 && x_tg - x_ct < 50)
                    {
                        x_ct -= 50;
                    }
                    if (y_ct - y_tg > 0 && y_tg - y_ct < 50)
                    {
                        y_tg -= 50;
                    }
                    if (y_tg - y_ct > 0 && y_tg - y_ct < 50)
                    {
                        y_ct -= 50;
                    }
                    image_ct.Margin = new Thickness(x_ct, y_ct, 0, 0);
                    ImageGrid.Children.Add(image_ct);
                    image_tg.Margin = new Thickness(x_tg, y_tg, 0, 0);
                    ImageGrid.Children.Add(image_tg);
                }
                if (hardness >= 22 && hardness <= 23)
                {
                    horizon = 350;
                    Image image_bg = new Image
                    {
                        Source = new BitmapImage(new Uri(imagePaths_bg[0][hardness - 1], UriKind.Relative)),
                        Stretch = System.Windows.Media.Stretch.Uniform
                    };
                    ImageGrid.Children.Add(image_bg);


                    // 加载目标图像
                    Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);
                    int arraylength1 = imagePaths_tg[1].Length;
                    int index1 = random1.Next(arraylength1);
                    image_tg.Source = new BitmapImage(new Uri(imagePaths_tg[1][index1], UriKind.Relative));
                    image_tg.Stretch = System.Windows.Media.Stretch.Uniform;
                    image_tg.Width = tg_width;
                    image_tg.Height = tg_height;

                    angle = random_tg.NextDouble() * 2 * Math.PI;
                    radius = random_tg.NextDouble() * horizon;
                    x_tg = radius * Math.Cos(angle) - image_tg.Width / 2;
                    y_tg = radius * Math.Sin(angle) - image_tg.Height / 2;

                    Random random_ct = new Random();
                    // 加载基准点（太阳/月亮）

                    Random random2 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);
                    int arraylength2 = imagePaths_ct[1].Length;
                    int index2 = random1.Next(arraylength2);
                    image_ct.Source = new BitmapImage(new Uri(imagePaths_ct[1][index2], UriKind.Relative));
                    image_ct.Stretch = System.Windows.Media.Stretch.Uniform;
                    image_ct.Width = ct_width;
                    image_ct.Height = ct_height;

                    angle = random_ct.NextDouble() * 2 * Math.PI;
                    radius = random_ct.NextDouble() * horizon;
                    x_ct = radius * Math.Cos(angle) - image_ct.Width / 2;
                    y_ct = radius * Math.Sin(angle) - image_ct.Height / 2;
                    if (x_ct - x_tg > 0 && x_ct - x_tg < 50)
                    {
                        x_tg -= 50;
                    }
                    if (x_tg - x_ct > 0 && x_tg - x_ct < 50)
                    {
                        x_ct -= 50;
                    }
                    if (y_ct - y_tg > 0 && y_tg - y_ct < 50)
                    {
                        y_tg -= 50;
                    }
                    if (y_tg - y_ct > 0 && y_tg - y_ct < 50)
                    {
                        y_ct -= 50;
                    }
                    image_ct.Margin = new Thickness(x_ct, y_ct, 0, 0);
                    ImageGrid.Children.Add(image_ct);
                    image_tg.Margin = new Thickness(x_tg, y_tg, 0, 0);
                    ImageGrid.Children.Add(image_tg);
                }
                if (hardness >= 24 && hardness <= 26)
                {
                    horizon = 350;
                    Image image_bg = new Image
                    {
                        Source = new BitmapImage(new Uri(imagePaths_bg[0][hardness - 1], UriKind.Relative)),
                        Stretch = System.Windows.Media.Stretch.Uniform
                    };
                    ImageGrid.Children.Add(image_bg);


                    // 加载目标图像
                    Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);
                    int arraylength1 = imagePaths_tg[0].Length;
                    int index1 = random1.Next(arraylength1);
                    image_tg.Source = new BitmapImage(new Uri(imagePaths_tg[0][index1], UriKind.Relative));
                    image_tg.Stretch = System.Windows.Media.Stretch.Uniform;
                    image_tg.Width = tg_width;
                    image_tg.Height = tg_height;

                    angle = random_tg.NextDouble() * 2 * Math.PI;
                    radius = random_tg.NextDouble() * horizon;
                    x_tg = radius * Math.Cos(angle) - image_tg.Width / 2;
                    y_tg = radius * Math.Sin(angle) - image_tg.Height / 2;

                    Random random_ct = new Random();
                    // 加载基准点（太阳/月亮）

                    Random random2 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);
                    int arraylength2 = imagePaths_ct[0].Length;
                    int index2 = random1.Next(arraylength2);
                    image_ct.Source = new BitmapImage(new Uri(imagePaths_ct[0][index2], UriKind.Relative));
                    image_ct.Stretch = System.Windows.Media.Stretch.Uniform;
                    image_ct.Width = ct_width;
                    image_ct.Height = ct_height;

                    angle = random_ct.NextDouble() * 2 * Math.PI;
                    radius = random_ct.NextDouble() * horizon;
                    x_ct = radius * Math.Cos(angle) - image_ct.Width / 2;
                    y_ct = radius * Math.Sin(angle) - image_ct.Height / 2;
                    if (x_ct - x_tg > 0 && x_ct - x_tg < 50)
                    {
                        x_tg -= 50;
                    }
                    if (x_tg - x_ct > 0 && x_tg - x_ct < 50)
                    {
                        x_ct -= 50;
                    }
                    if (y_ct - y_tg > 0 && y_tg - y_ct < 50)
                    {
                        y_tg -= 50;
                    }
                    if (y_tg - y_ct > 0 && y_tg - y_ct < 50)
                    {
                        y_ct -= 50;
                    }
                    image_ct.Margin = new Thickness(x_ct, y_ct, 0, 0);
                    ImageGrid.Children.Add(image_ct);
                    image_tg.Margin = new Thickness(x_tg, y_tg, 0, 0);
                    ImageGrid.Children.Add(image_tg);
                }
                if (hardness >= 27 && hardness <= 28)
                {
                    horizon = 350;
                    Image image_bg = new Image
                    {
                        Source = new BitmapImage(new Uri(imagePaths_bg[0][hardness - 1], UriKind.Relative)),
                        Stretch = System.Windows.Media.Stretch.Uniform
                    };
                    ImageGrid.Children.Add(image_bg);


                    // 加载目标图像
                    Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);
                    int arraylength1 = imagePaths_tg[1].Length;
                    int index1 = random1.Next(arraylength1);
                    image_tg.Source = new BitmapImage(new Uri(imagePaths_tg[1][index1], UriKind.Relative));
                    image_tg.Stretch = System.Windows.Media.Stretch.Uniform;
                    image_tg.Width = tg_width;
                    image_tg.Height = tg_height;

                    angle = random_tg.NextDouble() * 2 * Math.PI;
                    radius = random_tg.NextDouble() * horizon;
                    x_tg = radius * Math.Cos(angle) - image_tg.Width / 2;
                    y_tg = radius * Math.Sin(angle) - image_tg.Height / 2;

                    Random random_ct = new Random();
                    // 加载基准点（太阳/月亮）

                    Random random2 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);
                    int arraylength2 = imagePaths_ct[1].Length;
                    int index2 = random1.Next(arraylength2);
                    image_ct.Source = new BitmapImage(new Uri(imagePaths_ct[1][index2], UriKind.Relative));
                    image_ct.Stretch = System.Windows.Media.Stretch.Uniform;
                    image_ct.Width = ct_width;
                    image_ct.Height = ct_height;

                    angle = random_ct.NextDouble() * 2 * Math.PI;
                    radius = random_ct.NextDouble() * horizon;
                    x_ct = radius * Math.Cos(angle) - image_ct.Width / 2;
                    y_ct = radius * Math.Sin(angle) - image_ct.Height / 2;
                    if (x_ct - x_tg > 0 && x_ct - x_tg < 50)
                    {
                        x_tg -= 50;
                    }
                    if (x_tg - x_ct > 0 && x_tg - x_ct < 50)
                    {
                        x_ct -= 50;
                    }
                    if (y_ct - y_tg > 0 && y_tg - y_ct < 50)
                    {
                        y_tg -= 50;
                    }
                    if (y_tg - y_ct > 0 && y_tg - y_ct < 50)
                    {
                        y_ct -= 50;
                    }
                    image_ct.Margin = new Thickness(x_ct, y_ct, 0, 0);
                    ImageGrid.Children.Add(image_ct);
                    image_tg.Margin = new Thickness(x_tg, y_tg, 0, 0);
                    ImageGrid.Children.Add(image_tg);
                }
                if (hardness >= 29 && hardness <= 34)
                {
                    horizon = 350;
                    Image image_bg = new Image
                    {
                        Source = new BitmapImage(new Uri(imagePaths_bg[0][hardness - 1], UriKind.Relative)),
                        Stretch = System.Windows.Media.Stretch.Uniform
                    };
                    ImageGrid.Children.Add(image_bg);


                    // 加载目标图像
                    Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);
                    int arraylength1 = imagePaths_tg[0].Length;
                    int index1 = random1.Next(arraylength1);
                    image_tg.Source = new BitmapImage(new Uri(imagePaths_tg[0][index1], UriKind.Relative));
                    image_tg.Stretch = System.Windows.Media.Stretch.Uniform;
                    //难度越小图片越大
                    image_tg.Width = tg_width + 50 - hardness;
                    image_tg.Height = tg_height + 50 - hardness;

                    angle = random_tg.NextDouble() * 2 * Math.PI;
                    radius = random_tg.NextDouble() * horizon;
                    x_tg = radius * Math.Cos(angle) - image_tg.Width / 2;
                    y_tg = radius * Math.Sin(angle) - image_tg.Height / 2;

                    Random random_ct = new Random();
                    // 加载基准点（太阳/月亮）

                    Random random2 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);
                    int arraylength2 = imagePaths_ct[0].Length;
                    int index2 = random1.Next(arraylength2);
                    image_ct.Source = new BitmapImage(new Uri(imagePaths_ct[0][index2], UriKind.Relative));
                    image_ct.Stretch = System.Windows.Media.Stretch.Uniform;
                    image_ct.Width = ct_width;
                    image_ct.Height = ct_height;

                    angle = random_ct.NextDouble() * 2 * Math.PI;
                    radius = random_ct.NextDouble() * horizon;
                    x_ct = radius * Math.Cos(angle) - image_ct.Width / 2;
                    y_ct = radius * Math.Sin(angle) - image_ct.Height / 2;
                    if (x_ct - x_tg > 0 && x_ct - x_tg < 50)
                    {
                        x_tg -= 50;
                    }
                    if (x_tg - x_ct > 0 && x_tg - x_ct < 50)
                    {
                        x_ct -= 50;
                    }
                    if (y_ct - y_tg > 0 && y_tg - y_ct < 50)
                    {
                        y_tg -= 50;
                    }
                    if (y_tg - y_ct > 0 && y_tg - y_ct < 50)
                    {
                        y_ct -= 50;
                    }
                    image_ct.Margin = new Thickness(x_ct, y_ct, 0, 0);
                    ImageGrid.Children.Add(image_ct);
                    image_tg.Margin = new Thickness(x_tg, y_tg, 0, 0);
                    ImageGrid.Children.Add(image_tg);
                }
                if (x_ct > x_tg)
                {
                    TotalLeftCount[hardness - 1]++;
                }
                if (x_ct < x_tg)
                {
                    TotalRightCount[hardness - 1]++;
                }
                ComplexCount[hardness - 1]++;
            }
            TotalCount++;
            //MessageBox.Show(TotalCount.ToString());
            //MessageBox.Show(correctAnswers[hardness - 1].ToString());
            //MessageBox.Show(wrongAnswers[hardness - 1].ToString());
            //MessageBox.Show(ignoreAnswers[hardness - 1].ToString());
        }
        private void resetboollist()
        {
            recentResults.Clear();
        }
        private void AdjustDifficulty()
        {
            int correctCount = 0;
            int wrongCount = 0;
            // 添加当前题目结果到recentResults列表

            // 只保留最近5个结果
            int max = Math.Max(LevelUp, LevelDown); // 假设Max是Math类中的Max方法

            // 确保recentResults集合的大小不超过max
            if (recentResults.Count > max)
            {
                recentResults.RemoveAt(0); // 移除第一个元素
            }

            if (recentResults.Count >= Math.Min(LevelUp, LevelDown))
            {
                // 计算最近答题中正确答案的数量
                for (int i = recentResults.Count - LevelUp; i < recentResults.Count; i++)
                {
                    correctCount += recentResults[i] ? 1 : 0;
                }
                for (int i = recentResults.Count - LevelDown; i < recentResults.Count; i++)
                {
                    wrongCount += recentResults[i] ? 0 : 1;
                }

                // 提高难度
                if (correctCount == LevelUp && hardness < 24)
                {
                    hardness++;
                    max_hardness = Math.Max(max_hardness, hardness);
                    resetboollist();
                }

                // 降低难度
                else if (wrongCount == LevelDown && hardness > 1)
                {
                    hardness--;
                    resetboollist();
                }
            }
        }
        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

            timer?.Stop();
            //特殊情况代码
            if (hardness == 1 || hardness == 2 || hardness == 3 || hardness == 6 || hardness == 9 || hardness == 14 || hardness == 19 || hardness == 20)
            {
                if (!is_enter)
                {
                    if (e.Key == Key.Left)
                    {
                        bool iscorrect = randomNumber == 0;
                        if (iscorrect)
                        {
                            is_enter = true;
                            //textblock.Text = "恭喜你答对了！";
                            //textblock.Foreground = new SolidColorBrush(Colors.Green);

                            //停顿刺激间隔时间
                            countTimer.Interval = TimeSpan.FromSeconds(stimulus_interval);
                            judgement.Source = new BitmapImage(new Uri(JudgementPath[0], UriKind.Relative));
                            PlayWav(CorrectSoundPath);
                            ShowFeedbackImage(CorrectImage);
                            countTimer.Start();

                            LeftCostTime[hardness - 1].Add(costTime);
                            correctAnswers[hardness - 1]++; // 更新对应难度的正确答案计数
                            TotalCorrectLeftCount[hardness - 1]++;
                            recentResults.Add(true);
                            // 检查列表长度并删除第一个元素以保持列表长度为5
                            if (recentResults.Count > Math.Max(LevelUp, LevelDown))
                            {
                                recentResults.RemoveAt(0);
                            }
                        }
                        else
                        {
                            is_enter = true;
                            //textblock.Text = "很遗憾答错了！";
                            //textblock.Foreground = new SolidColorBrush(Colors.Red);

                            //停顿刺激间隔时间
                            countTimer.Interval = TimeSpan.FromSeconds(stimulus_interval);
                            judgement.Source = new BitmapImage(new Uri(JudgementPath[1], UriKind.Relative));
                            PlayWav(ErrorSoundPath);
                            ShowFeedbackImage(ErrorImage);
                            countTimer.Start();

                            wrongAnswers[hardness - 1]++; // 更新对应难度的错误答案计数
                            if (IS_BEEP)
                                Console.Beep();
                            recentResults.Add(false);

                            // 检查列表长度并删除第一个元素以保持列表长度为5
                            if (recentResults.Count > Math.Max(LevelUp, LevelDown))
                            {
                                recentResults.RemoveAt(0);
                            }
                        }
                        AdjustDifficulty();
                    }

                    if (e.Key == Key.Right)
                    {
                        is_enter = true;
                        if (is_auto)
                        {
                            bool iscorrect = randomNumber == 1;
                            if (iscorrect)
                            {
                                //textblock.Text = "恭喜你答对了！";
                                //textblock.Foreground = new SolidColorBrush(Colors.Green);

                                //停顿刺激间隔时间
                                countTimer.Interval = TimeSpan.FromSeconds(stimulus_interval);
                                judgement.Source = new BitmapImage(new Uri(JudgementPath[0], UriKind.Relative));
                                PlayWav(CorrectSoundPath);
                                ShowFeedbackImage(CorrectImage);
                                countTimer.Start();

                                RightCostTime[hardness - 1].Add(costTime);
                                ComplexCostTime[hardness - 1].Add(costTime);
                                correctAnswers[hardness - 1]++; // 更新对应难度的正确答案计数
                                TotalCorrectRightCount[hardness - 1]++;

                                recentResults.Add(true);

                                // 检查列表长度并删除第一个元素以保持列表长度为5
                                if (recentResults.Count > 5)
                                {
                                    recentResults.RemoveAt(0);
                                }

                            }
                            else
                            {
                                //textblock.Text = "很遗憾答错了！";
                                //textblock.Foreground = new SolidColorBrush(Colors.Red);

                                //停顿刺激间隔时间
                                countTimer.Interval = TimeSpan.FromSeconds(stimulus_interval);
                                judgement.Source = new BitmapImage(new Uri(JudgementPath[1], UriKind.Relative));
                                PlayWav(ErrorSoundPath);
                                ShowFeedbackImage(ErrorImage);
                                countTimer.Start();

                                wrongAnswers[hardness - 1]++; // 更新对应难度的错误答案计数
                                if (IS_BEEP)
                                    Console.Beep();
                                recentResults.Add(false);

                                // 检查列表长度并删除第一个元素以保持列表长度为5
                                if (recentResults.Count > 5)
                                {
                                    recentResults.RemoveAt(0);
                                }

                            }
                            AdjustDifficulty();
                        }


                    }
                }
                if (e.Key == Key.Enter && is_enter == true)
                {
                    countTimer?.Stop();
                    LoadImages();
                    judgement.Source = null;
                    is_enter = false;
                }
            }
            //通用情况代码
            else
            {
                if (!is_enter)
                {
                    if (e.Key == Key.Left)
                    {
                        if (is_auto)
                        {
                            bool isCorrect = x_tg < x_ct;
                            if (isCorrect)
                            {
                                is_enter = true;
                                //textblock.Text = "恭喜你答对了！";
                                //textblock.Foreground = new SolidColorBrush(Colors.Green);

                                //停顿刺激间隔时间
                                countTimer.Interval = TimeSpan.FromSeconds(stimulus_interval);
                                judgement.Source = new BitmapImage(new Uri(JudgementPath[0], UriKind.Relative));
                                PlayWav(CorrectSoundPath);
                                ShowFeedbackImage(CorrectImage);
                                countTimer.Start();

                                LeftCostTime[hardness - 1].Add(costTime);
                                ComplexCostTime[hardness - 1].Add(costTime);
                                ComplexCorrectCount[hardness - 1]++;
                                correctAnswers[hardness - 1]++; // 更新对应难度的正确答案计数
                                TotalCorrectLeftCount[hardness - 1]++;
                                if (y_ct > y_tg)
                                {
                                    ComplexLeftDownCount[hardness - 1]++;
                                }
                                if (y_ct < y_tg)
                                {
                                    ComplexLeftUpCount[hardness - 1]++;
                                }
                                recentResults.Add(true);

                                // 检查列表长度并删除第一个元素以保持列表长度为5
                                if (recentResults.Count > 5)
                                {
                                    recentResults.RemoveAt(0);
                                }

                            }
                            else
                            {
                                is_enter = true;
                                //textblock.Text = "很遗憾答错了！";
                                //textblock.Foreground = new SolidColorBrush(Colors.Red);

                                //停顿刺激间隔时间
                                countTimer.Interval = TimeSpan.FromSeconds(stimulus_interval);
                                judgement.Source = new BitmapImage(new Uri(JudgementPath[1], UriKind.Relative));
                                PlayWav(ErrorSoundPath);
                                ShowFeedbackImage(ErrorImage);
                                countTimer.Start();

                                wrongAnswers[hardness - 1]++; // 更新对应难度的错误答案计数
                                if (IS_BEEP)
                                    Console.Beep();

                                recentResults.Add(false);

                                // 检查列表长度并删除第一个元素以保持列表长度为5
                                if (recentResults.Count > 5)
                                {
                                    recentResults.RemoveAt(0);
                                }

                            }
                            AdjustDifficulty();
                        }
                        else
                        {
                            bool isCorrect = x_tg < x_ct;
                            if (isCorrect)
                            {
                                is_enter = true;
                                //textblock.Text = "恭喜你答对了！";
                                //textblock.Foreground = new SolidColorBrush(Colors.Green);

                                judgement.Source = new BitmapImage(new Uri(JudgementPath[0], UriKind.Relative));
                                PlayWav(CorrectSoundPath);
                                ShowFeedbackImage(CorrectImage);
                                LeftCostTime[hardness - 1].Add(costTime);
                                ComplexCostTime[hardness - 1].Add(costTime);
                                ComplexCorrectCount[hardness - 1]++;
                                correctAnswers[hardness - 1]++; // 更新对应难度的正确答案计数
                                TotalCorrectLeftCount[hardness - 1]++;
                                if (y_ct > y_tg)
                                {
                                    ComplexLeftDownCount[hardness - 1]++;
                                }
                                if (y_ct < y_tg)
                                {
                                    ComplexLeftUpCount[hardness - 1]++;
                                }
                                recentResults.Add(true);

                                // 检查列表长度并删除第一个元素以保持列表长度为5
                                if (recentResults.Count > 5)
                                {
                                    recentResults.RemoveAt(0);
                                }

                            }
                            else
                            {
                                is_enter = true;
                                //textblock.Text = "很遗憾答错了！";
                                //textblock.Foreground = new SolidColorBrush(Colors.Red);

                                judgement.Source = new BitmapImage(new Uri(JudgementPath[1], UriKind.Relative));
                                PlayWav(ErrorSoundPath);
                                ShowFeedbackImage(ErrorImage);
                                wrongAnswers[hardness - 1]++; // 更新对应难度的错误答案计数
                                if (IS_BEEP)
                                    Console.Beep();

                                recentResults.Add(false);

                                // 检查列表长度并删除第一个元素以保持列表长度为5
                                if (recentResults.Count > 5)
                                {
                                    recentResults.RemoveAt(0);
                                }

                            }
                            AdjustDifficulty();
                        }
                    }
                    if (e.Key == Key.Right)
                    {
                        if (is_auto)
                        {
                            bool isCorrect = x_tg > x_ct;
                            if (isCorrect)
                            {
                                is_enter = true;
                                //textblock.Text = "恭喜你答对了！";
                                //textblock.Foreground = new SolidColorBrush(Colors.Green);

                                //停顿刺激间隔时间
                                countTimer.Interval = TimeSpan.FromSeconds(stimulus_interval);
                                judgement.Source = new BitmapImage(new Uri(JudgementPath[0], UriKind.Relative));
                                PlayWav(CorrectSoundPath);
                                ShowFeedbackImage(CorrectImage);
                                countTimer.Start();

                                RightCostTime[hardness - 1].Add(costTime);
                                if (y_ct > y_tg)
                                {
                                    ComplexRightDownCount[hardness - 1]++;
                                }
                                if (y_ct < y_tg)
                                {
                                    ComplexRightUpCount[hardness - 1]++;
                                }
                                ComplexCorrectCount[hardness - 1]++;
                                correctAnswers[hardness]++; // 更新对应难度的正确答案计数
                                TotalCorrectRightCount[hardness - 1]++;
                                recentResults.Add(true);

                                // 检查列表长度并删除第一个元素以保持列表长度为5
                                if (recentResults.Count > 5)
                                {
                                    recentResults.RemoveAt(0);
                                }
                            }
                            else
                            {
                                //textblock.Text = "很遗憾答错了！";
                                //textblock.Foreground = new SolidColorBrush(Colors.Red);

                                //停顿刺激间隔时间
                                countTimer.Interval = TimeSpan.FromSeconds(stimulus_interval);
                                judgement.Source = new BitmapImage(new Uri(JudgementPath[1], UriKind.Relative));
                                PlayWav(ErrorSoundPath);
                                ShowFeedbackImage(ErrorImage);
                                countTimer.Start();

                                wrongAnswers[hardness]++; // 更新对应难度的错误答案计数
                                if (IS_BEEP)
                                    Console.Beep();

                                recentResults.Add(false);

                                // 检查列表长度并删除第一个元素以保持列表长度为5
                                if (recentResults.Count > 5)
                                {
                                    recentResults.RemoveAt(0);
                                }
                            }
                            AdjustDifficulty();
                        }
                        else
                        {
                            bool isCorrect = x_tg > x_ct;
                            if (isCorrect)
                            {
                                is_enter = true;
                                //textblock.Text = "恭喜你答对了！";
                                //textblock.Foreground = new SolidColorBrush(Colors.Green);
                                judgement.Source = new BitmapImage(new Uri(JudgementPath[0], UriKind.Relative));
                                PlayWav(CorrectSoundPath);
                                ShowFeedbackImage(CorrectImage);
                                RightCostTime[hardness - 1].Add(costTime);
                                if (y_ct > y_tg)
                                {
                                    ComplexRightDownCount[hardness - 1]++;
                                }
                                if (y_ct < y_tg)
                                {
                                    ComplexRightUpCount[hardness - 1]++;
                                }
                                ComplexCorrectCount[hardness - 1]++;
                                correctAnswers[hardness]++; // 更新对应难度的正确答案计数
                                TotalCorrectRightCount[hardness - 1]++;
                                recentResults.Add(true);

                                // 检查列表长度并删除第一个元素以保持列表长度为5
                                if (recentResults.Count > 5)
                                {
                                    recentResults.RemoveAt(0);
                                }
                            }
                            else
                            {
                                //textblock.Text = "很遗憾答错了！";
                                //textblock.Foreground = new SolidColorBrush(Colors.Red);
                                judgement.Source = new BitmapImage(new Uri(JudgementPath[1], UriKind.Relative));
                                PlayWav(ErrorSoundPath);
                                ShowFeedbackImage(ErrorImage);
                                wrongAnswers[hardness]++; // 更新对应难度的错误答案计数
                                if (IS_BEEP)
                                    Console.Beep();

                                recentResults.Add(false);

                                // 检查列表长度并删除第一个元素以保持列表长度为5
                                if (recentResults.Count > 5)
                                {
                                    recentResults.RemoveAt(0);
                                }
                            }
                            AdjustDifficulty();
                        }
                    }
                }
                if (e.Key == Key.Enter && is_enter == true)
                {
                    countTimer?.Stop();
                    LoadImages();
                    judgement.Source = null;
                    is_enter = false;
                }
            }

            if (TotalCorrectLeftCount[hardness - 1] != 0)
            {
                TotalLeftAccuracy[hardness - 1] = ((double)TotalCorrectLeftCount[hardness - 1] / (double)TotalLeftCount[hardness - 1]);
            }
            if (TotalCorrectRightCount[hardness - 1] != 0)
            {
                TotalRightAccuracy[hardness - 1] = ((double)TotalCorrectRightCount[hardness - 1] / (double)TotalRightCount[hardness - 1]);
            }
            if (ComplexCount[hardness - 1] != 0)
            {
                ComplexAccuracy[hardness - 1] = ((double)ComplexCorrectCount[hardness - 1] / (double)ComplexCount[hardness - 1]);
            }
            if (ComplexCostTime[hardness - 1] != null && ComplexCostTime[hardness - 1].Any())
            {
                ComplexAverageTime[hardness - 1] = ComplexCostTime[hardness - 1].Average();
            }
            if (LeftCostTime[hardness - 1] != null && LeftCostTime[hardness - 1].Any())
            {
                AverageLeftCostTime[hardness - 1] = LeftCostTime[hardness - 1].Average();
            }
            if (RightCostTime[hardness - 1] != null && RightCostTime[hardness - 1].Any())
            {
                AverageRightCostTime[hardness - 1] = RightCostTime[hardness - 1].Average();
            }
        }
    }
    public partial class 视空间训练 : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {

            max_time = 10;
            TRAIN_TIME = 120; // 训练持续时间（单位：秒）
            IS_RESTRICT_TIME = false; // 限制练习时间是否启用
            IS_BEEP = true; // 是否发出声音
            hardness = 1; // 难度级别
            train_time = TRAIN_TIME;
            TotalCount = 0;

            // 参数（包含模块参数信息）
            var baseParameter = BaseParameter;
            if (baseParameter.ProgramModulePars != null && baseParameter.ProgramModulePars.Any())
            {
                Debug.WriteLine("ProgramModulePars 已加载数据：");

                // 遍历 ProgramModulePars 打印出每个参数
                foreach (var par in baseParameter.ProgramModulePars)
                {
                    /*Debug.WriteLine($"ProgramId: {par.ProgramId}, ModuleParId: {par.ModuleParId}, Value: {par.Value}, TableId: {par.TableId}");*/
                    if (par.ModulePar.ModuleId == baseParameter.ModuleId)
                    {
                        switch (par.ModuleParId) // 完成赋值
                        {
                            case 83: // 治疗时间 
                                train_time = par.Value.HasValue ? (int)par.Value.Value * 60 : 120;
                                Debug.WriteLine($"TRAIN_TIME = {train_time}");
                                break;
                            case 85: // 等级提高
                                LevelUp = par.Value.HasValue ? (int)par.Value.Value : 3;
                                Debug.WriteLine($"LevelUp = {LevelUp}");
                                break;
                            case 86: // 等级降低
                                LevelDown = par.Value.HasValue ? (int)par.Value.Value : 3;
                                Debug.WriteLine($"LevelDown = {LevelDown}");
                                break;
                            case 88: // 听觉反馈
                                IS_BEEP = par.Value == 1;
                                Debug.WriteLine($"是否听觉反馈 = {IS_BEEP}");
                                break;
                            case 90: // 限制时间
                                IS_RESTRICT_TIME = par.Value == 0;
                                Debug.WriteLine($"是否限制时间 = {IS_RESTRICT_TIME}");
                                break;
                            case 317://刺激间隔
                                stimulus_interval = par.Value.HasValue ? (int)par.Value.Value : 3;
                                Debug.WriteLine($"刺激间隔 = {stimulus_interval}");
                                break;
                            // 添加其他需要处理的 ModuleParId
                            case 176://等级
                                hardness = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"初始难度 = {hardness}");
                                break;
                        }
                    }
                }
            }
            else
            {
                Debug.WriteLine("没有数据");
            }

            remainingTime = max_time;
            costTime = 0;
            correctAnswers = new int[MAX_HARDNESS + 1];
            wrongAnswers = new int[MAX_HARDNESS + 1];
            ignoreAnswers = new int[MAX_HARDNESS + 1];
            TotalLeftCount = new int[MAX_HARDNESS + 1];
            TotalRightCount = new int[MAX_HARDNESS + 1];
            TotalCorrectLeftCount = new int[MAX_HARDNESS + 1];
            TotalCorrectRightCount = new int[MAX_HARDNESS + 1];
            ComplexCount = new int[MAX_HARDNESS + 1];
            ComplexRightUpCount = new int[MAX_HARDNESS + 1];
            ComplexLeftUpCount = new int[MAX_HARDNESS + 1];
            ComplexRightDownCount = new int[MAX_HARDNESS + 1];
            ComplexLeftDownCount = new int[MAX_HARDNESS + 1];
            TotalLeftAccuracy = new double[MAX_HARDNESS + 1];
            TotalRightAccuracy = new double[MAX_HARDNESS + 1];
            ComplexAccuracy = new double[MAX_HARDNESS + 1];
            ComplexCorrectCount = new int[MAX_HARDNESS + 1];

            RightCostTime = new List<int>[MAX_HARDNESS + 1];
            LeftCostTime = new List<int>[MAX_HARDNESS + 1];
            ComplexCostTime = new List<int>[MAX_HARDNESS + 1];
            AverageRightCostTime = new double[MAX_HARDNESS + 1];
            AverageLeftCostTime = new double[MAX_HARDNESS + 1];
            ComplexAverageTime = new double[MAX_HARDNESS + 1];

            max_hardness = Math.Max(max_hardness, hardness);

            string targetDirectory = Path.Combine(currentDirectory, "Img");

            for (int i = 0; i < MAX_HARDNESS + 1; i++)
            {
                correctAnswers[i] = 0;
                wrongAnswers[i] = 0;
                ignoreAnswers[i] = 0;
                TotalLeftCount[i] = 0;
                TotalRightCount[i] = 0;
                TotalCorrectLeftCount[i] = 0;
                TotalCorrectRightCount[i] = 0;
                ComplexCount[i] = 0;
                ComplexRightUpCount[i] = 0;
                ComplexLeftUpCount[i] = 0;
                ComplexRightDownCount[i] = 0;
                ComplexLeftDownCount[i] = 0;
                TotalLeftAccuracy[i] = 0;
                TotalRightAccuracy[i] = 0;
                ComplexAccuracy[i] = 0;
                ComplexCorrectCount[i] = 0;
                RightCostTime[i] = new List<int>();
                LeftCostTime[i] = new List<int>();
                ComplexCostTime[i] = new List<int>();
                AverageRightCostTime[i] = 0;
                AverageLeftCostTime[i] = 0;
                ComplexAverageTime[i] = 0;
            }

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            countTimer = new DispatcherTimer();
            countTimer.Interval = TimeSpan.FromSeconds(0);
            countTimer.Tick += Countimer_Tick;

            trainingTimer = new DispatcherTimer();
            trainingTimer.Interval = TimeSpan.FromSeconds(1);
            trainingTimer.Tick += TrainingTimer_Tick;

            StarTimer = new DispatcherTimer();
            StarTimer.Interval = TimeSpan.FromSeconds(1);
            StarTimer.Tick += StarTimer_Tick;
            // 调用委托
            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
            RightStatisticsAction?.Invoke(0, LevelUp);
            WrongStatisticsAction?.Invoke(0, LevelDown);
        }

        private void StarTimer_Tick(object sender, EventArgs e)
        {
            //调用委托
            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
            RightStatisticsAction?.Invoke(recentResults.Count(r => r == true), LevelUp);
            WrongStatisticsAction?.Invoke(recentResults.Count(r => r == false), LevelDown);
        }

        protected override async Task OnStartAsync()
        {
            FeedBackInit();
            timer?.Stop();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            trainingTimer?.Stop();
            trainingTimer.Start(); // 启动训练计时器
            StarTimer.Start();
            countTimer?.Stop();

            LoadImages();

            // 调用委托
            VoiceTipAction?.Invoke("请辨认屏幕中出现的物体在太阳的左边还是右边。");
            SynopsisAction?.Invoke("在屏幕上每隔一段时间会有车，船等问题出现在太阳附近，请辨认出这个物体在太阳的左边还是右边，然后在键盘上对应地按下左右方向键来确认选择。");
            RuleAction?.Invoke("在屏幕上每隔一段时间会有车，船等问题出现在太阳附近，请辨认出这个物体在太阳的左边还是右边，然后在键盘上对应地按下左右方向键来确认选择。");
        }

        protected override async Task OnStopAsync()
        {
            timer?.Stop();
            trainingTimer?.Stop();
            countTimer?.Stop();
        }

        protected override async Task OnPauseAsync()
        {
            timer?.Stop();
            trainingTimer?.Stop();
            countTimer?.Stop();
        }

        protected override async Task OnNextAsync()
        {
            // 调整难度
            AdjustDifficulty();
            LoadImages();

            // 调用委托
            VoiceTipAction?.Invoke("请辨认屏幕中出现的物体在太阳的左边还是右边。");
            SynopsisAction?.Invoke("在屏幕上每隔一段时间会有车，船等问题出现在太阳附近，请辨认出这个物体在太阳的左边还是右边，然后在键盘上对应地按下左右方向键来确认选择。");
            RuleAction?.Invoke("在屏幕上每隔一段时间会有车，船等问题出现在太阳附近，请辨认出这个物体在太阳的左边还是右边，然后在键盘上对应地按下左右方向键来确认选择。");
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new 眼动训练讲解();
        }
        private int GetCorrectNum(int difficultyLevel)
        {
            return correctAnswers[difficultyLevel];
        }
        private int GetIgnoreNum(int difficultyLevel)
        {
            return ignoreAnswers[difficultyLevel];
        }
        private int GetWrongNum(int difficultyLevel)
        {
            return wrongAnswers[difficultyLevel];
        }
        private double GetTotalLeftAccuracy(int difficultyLevel)
        {
            return TotalLeftAccuracy[difficultyLevel];
        }
        private double GetTotalRightAccuracy(int difficultyLevel)
        {
            return TotalRightAccuracy[difficultyLevel];
        }
        private double GetComplexAccuracy(int difficultyLevel)
        {
            return ComplexAccuracy[difficultyLevel];
        }
        private int GetComplexLeftUpCount(int difficultyLevel)
        {
            return ComplexLeftUpCount[difficultyLevel];
        }
        private int GetComplexLeftDownCount(int difficultyLevel)
        {
            return ComplexLeftDownCount[difficultyLevel];
        }
        private int GetComplexRightUpCount(int difficultyLevel)
        {
            return ComplexRightUpCount[difficultyLevel];
        }
        private int GetComplexRightDownCount(int difficultyLevel)
        {
            return ComplexRightDownCount[difficultyLevel];
        }
        private int GetwrongCount(int difficultyLevel)
        {
            return wrongAnswers[difficultyLevel] + ignoreAnswers[difficultyLevel];
        }
        private double GetAverageLeftCostTime(int difficultyLevel)
        {
            return AverageLeftCostTime[difficultyLevel];
        }
        private double GetAverageRightCostTime(int difficultyLevel)
        {
            return AverageRightCostTime[difficultyLevel];
        }
        private double GetComplexAverageTime(int difficultyLevel)
        {
            return ComplexAverageTime[difficultyLevel];
        }


        private double CalculateAccuracy(int correctCount, int wrongCount, int ignoreCount)
        {
            int totalAnswers = correctCount + wrongCount + ignoreCount;
            return totalAnswers > 0 ? Math.Round((double)correctCount / totalAnswers, 2) : 0;
        }

        private async Task updateDataAsync()
        {
            var baseParameter = BaseParameter;

            var program_id = baseParameter.ProgramId;
            Crs_Db2Context db = baseParameter.Db;

            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    await Task.Run(async () =>
                    {
                        // 获取当前难度级别的数据
                        int correctCount = 0;
                        int ignoreCount = 0;
                        int wrongCount = 0;

                        double _totalleftCorrectCount = 0;
                        double _totalrightCorrectCount = 0;
                        double _totalleftCount = 0;
                        double _totalrightCount = 0;

                        double _complexCount = 0;
                        double _complexCorrectCount = 0;

                        int complexleftup = 0;
                        int complexleftdown = 0;
                        int complexrightup = 0;
                        int complexrightdown = 0;

                        double _averageleftTime = 0;
                        double _averagerightTime = 0;
                        double _comaverageTime = 0;




                        //double averageleft = GetAverageLeftCostTime(lv);
                        //double averageright = GetAverageRightCostTime(lv);
                        //double comaverage = GetComplexAverageTime(lv);

                        for (int lv = 0; lv < max_hardness; lv++)
                        {
                            correctCount += GetCorrectNum(lv);
                            ignoreCount += GetIgnoreNum(lv);
                            wrongCount += GetWrongNum(lv);

                            _totalleftCorrectCount += TotalCorrectLeftCount[lv];
                            _totalrightCorrectCount += TotalCorrectRightCount[lv];
                            _totalleftCount += TotalLeftCount[lv];
                            _totalrightCount += TotalRightCount[lv];

                            _complexCorrectCount += ComplexCorrectCount[lv];
                            _complexCount += (double)ComplexCount[lv];

                            complexleftup += GetComplexLeftUpCount(lv);
                            complexleftdown += GetComplexLeftDownCount(lv);
                            complexrightup += GetComplexRightUpCount(lv);
                            complexrightdown += GetComplexRightDownCount(lv);

                            _averageleftTime += TotalCorrectLeftCount[lv] * GetAverageLeftCostTime(lv);
                            _averagerightTime += TotalCorrectRightCount[lv] * GetAverageRightCostTime(lv);
                            _comaverageTime += ComplexCorrectCount[lv] * GetComplexAverageTime(lv);
                        }

                        double averageleft = 0;
                        double averageright = 0;
                        double comaverage = 0;
                        if (_totalleftCount > 0)
                        {
                            averageleft = _averageleftTime / _totalleftCount;
                        }
                        if (_totalrightCount > 0)
                        {
                            averageright = _averagerightTime / _totalrightCount;
                        }
                        if (_complexCorrectCount > 0)
                        {
                            comaverage = _comaverageTime / _complexCorrectCount;
                        }


                        double totalleftacc = 0;
                        if (_totalleftCount > 0)
                        {
                            totalleftacc = _totalleftCorrectCount / _totalleftCount;
                        }
                        double totalrightacc = 0;
                        if (_totalrightCount > 0)
                        {
                            totalrightacc = _totalrightCorrectCount / _totalrightCount;
                        }
                        double complexacc = 0;
                        if (_complexCount > 0)
                        {
                            complexacc = _complexCorrectCount / _complexCount;
                        }

                        int totalcount = correctCount + wrongCount + ignoreCount;
                        int wrongallCount = wrongCount + ignoreCount;
                        // 计算准确率
                        double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);



                        // 创建 Result 记录
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "视空间训练",
                            Eval = false,
                            Lv = max_hardness, // 当前的难度级别
                            ScheduleId = BaseParameter.ScheduleId ?? null // 假设的 Schedule_id，可以替换为实际值
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
                                    Order = 0,
                                    ValueName = "等级",
                                    Value = max_hardness,
                                    ModuleId = BaseParameter.ModuleId //  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order= 12,
                                    ValueName = "刺激",
                                    Value = totalcount,
                                    ModuleId = BaseParameter.ModuleId //  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order= 1,
                                    ValueName = "正确 左半边（%）",
                                    Value = totalleftacc * 100,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 2,
                                    ValueName = "正确 右半边（%）",
                                    Value = totalrightacc * 100,
                                    Charttype = "柱状图" ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 3, 
                                    ValueName = "正确 双物体（%）",
                                    Value = complexacc * 100, // 以百分比形式存储
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 4,
                                    ValueName = "正确 左上",
                                    Value = complexleftup,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 5,
                                    ValueName = "正确 左下",
                                    Value = complexleftdown ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 6,
                                    ValueName = "正确 右上",
                                    Value = complexrightup,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 7,  
                                    ValueName = "正确 右下",
                                    Value = complexrightdown,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order=8,
                                    ValueName = "错误 总数",
                                    Value = wrongallCount,
                                    Maxvalue = wrongallCount,
                                    Charttype = "柱状图" ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order=9,
                                    ValueName = "中间值 反应时间 左半边（ms）",
                                    Value = averageleft * 1000,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                   ResultId = result_id,
                                   Order=10,
                                    ValueName = "中间值 反应时间 右半边（ms）",
                                    Value = averageright * 1000,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                   ResultId = result_id,
                                   Order=11,
                                    ValueName = "中间值 反应时间 双物体（ms）",
                                    Value = comaverage * 1000,
                                    ModuleId = BaseParameter.ModuleId
                                }
                            };

                        // 插入 ResultDetail 数据
                        db.ResultDetails.AddRange(resultDetails);
                        await db.SaveChangesAsync();

                        // 输出每个 ResultDetail 对象的数据
                        Debug.WriteLine($"难度级别 {max_hardness}:");
                        foreach (var detail in resultDetails)
                        {
                            Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId}");
                        }


                        // 提交事务
                        await transaction.CommitAsync();
                        Debug.WriteLine("插入成功");
                    });
                }
                catch (Exception ex)
                {
                    // 回滚事务
                    await transaction.RollbackAsync();
                    Debug.WriteLine(ex.ToString());
                }
            }
        }
    }
}
