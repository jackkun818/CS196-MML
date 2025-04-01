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
    /// SAKA.xaml Interaction logic
    /// </summary>
    public partial class Visual space training : BaseUserControl
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
        //Difficulty 1, 2, 3，6，9，14, 19, 20 Very special needs to be written in completely different logic See the function for detailsLoadImages()
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

        private int TRAIN_TIME; // Training duration（Unit: seconds）
        private int max_time;
        private int train_time;
        private int hardness;
        int max_hardness = 0;
        private int remainingTime;
        private int costTime;
        private int randomNumber = 0; //Variables for judgment in special cases
        private int LevelUp = 3;
        private int LevelDown = 5;
        private int TotalCount = 0;
        private int stimulus_interval = 3;//Stimulation interval

        private const int MAX_HARDNESS = 34; // Highlights of difficulty

        private bool IS_RESTRICT_TIME = true; // Whether to enable exercise time
        private bool IS_BEEP = true;
        private bool is_game = true;
        private bool is_auto = true; // Whether it jumps automatically
        private bool is_enter = false;

        private DispatcherTimer timer;
        private DispatcherTimer trainingTimer; // New timer for training time
        private DispatcherTimer countTimer;
        private DispatcherTimer StarTimer = new DispatcherTimer();//Used to display the stars on the therapist side
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
        private int[] correctAnswers; // Store the number of correct answers for each difficulty
        private int[] wrongAnswers; // Store the number of error answers per difficulty
        private int[] ignoreAnswers;

        private double[] TotalLeftAccuracy;
        private double[] TotalRightAccuracy;
        private double[] ComplexAccuracy;
        private double[] AverageRightCostTime;
        private double[] AverageLeftCostTime;
        private double[] ComplexAverageTime;

        // Add a list to the class to store the results of the last 5 questions
        private List<bool> recentResults = new List<bool>();


        //Add feedback
        //feedback
        private SoundPlayer soundPlayer; // Used to sing
        public string ErrorSoundPath;//The wrong sound file path, inOnStartAsync()Medium configuration
        public string CorrectSoundPath;//The correct sound file path is inOnStartAsync()Medium configuration
        private int StopDurations = 2000; // Stop time,ms
        private string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory; // The absolute path to the current project
        private string ResourcesPath = System.IO.Path.Combine("Resources", "Word memory");//The fixed path used here is the memory path of word, which can be changed later.

        private void FeedBackInit()
        {
            /*LJN
            Configure the path to feedback resources             
             */
            CorrectSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");
            // for Image Control loading picture Source
            CorrectImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Correct.png"), UriKind.RelativeOrAbsolute));
            ErrorImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Error.png"), UriKind.RelativeOrAbsolute));

        }

        private void PlayWav(string filePath)
        {//Play locallywavdocument
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
        {//Image showing feedback
            image.Visibility = Visibility.Visible;

            // Delay the specified time（For example, 1 second）
            await Task.Delay(stimulus_interval * 1000);

            image.Visibility = Visibility.Collapsed;
        }

        public Visual space training()
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
            train_time--; // Countdown to training time

            TimeStatisticsAction.Invoke(train_time, remainingTime);

            if (train_time <= 0)
            {
                timer.Stop(); // Stop the main timer
                trainingTimer.Stop(); // Stop training timer
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
            // Clear the previous pictures
            ImageGrid.Children.Clear();

            double centerX = ImageGrid.ActualWidth / 2;
            double centerY = ImageGrid.ActualHeight / 2;
            double angle = 0; // 0 arrive 2π
            double radius;
            int tg_width = 300;
            int tg_height = 200;
            int ct_width = 90;
            int ct_height = 90;
            int horizon = 0;
            Image image_tg = new Image();
            Image image_ct = new Image();
            //Special difficulty code
            if (hardness == 1 || hardness == 2 || hardness == 3 || hardness == 6 || hardness == 9 || hardness == 14 || hardness == 19 || hardness == 20)
            {
                //Special situations are difficulty 1, 2, 3, 6, 9, 19, 20 All have to be written with new logic
                if (hardness == 1)
                {
                    Random random = new Random();
                    randomNumber = random.Next(2);
                    if (randomNumber == 0)
                    {
                        Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);

                        int arrayLength = imagePaths_specific[0].Length;
                        //HereindexYou need to read a certain range of pictures according to the difficulty
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
                        //HereindexYou need to read a certain range of pictures according to the difficulty
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
                        //HereindexYou need to read a certain range of pictures according to the difficulty
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
                        //HereindexYou need to read a certain range of pictures according to the difficulty
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
                        //HereindexYou need to read a certain range of pictures according to the difficulty
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
                        //HereindexYou need to read a certain range of pictures according to the difficulty
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
                        //HereindexYou need to read a certain range of pictures according to the difficulty
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
                        //HereindexYou need to read a certain range of pictures according to the difficulty
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
                        //HereindexYou need to read a certain range of pictures according to the difficulty
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
                        //HereindexYou need to read a certain range of pictures according to the difficulty
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
                        //HereindexYou need to read a certain range of pictures according to the difficulty
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
                        //HereindexYou need to read a certain range of pictures according to the difficulty
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
                        //HereindexYou need to read a certain range of pictures according to the difficulty
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
                        //HereindexYou need to read a certain range of pictures according to the difficulty
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
                        //HereindexYou need to read a certain range of pictures according to the difficulty
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
                        //HereindexYou need to read a certain range of pictures according to the difficulty
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
            //Common case code
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


                    // Load the target image
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
                    // Loading the base point（sun/moon）

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


                    // Load the target image
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
                    // Loading the base point（sun/moon）

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


                    // Load the target image
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
                    // Loading the base point（sun/moon）

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
                //Outline background for grass Target is a butterfly
                if (hardness >= 12 && hardness <= 13)
                {
                    horizon = 250;
                    Image image_bg = new Image
                    {
                        Source = new BitmapImage(new Uri(imagePaths_bg[0][hardness - 1], UriKind.Relative)),
                        Stretch = System.Windows.Media.Stretch.Uniform
                    };
                    ImageGrid.Children.Add(image_bg);


                    // Load the target image
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
                    // Loading the base point（sun/moon）

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


                    // Load the target image
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
                    // Loading the base point（sun/moon）

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


                    // Load the target image
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
                    // Loading the base point（sun/moon）

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


                    // Load the target image
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
                    // Loading the base point（sun/moon）

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


                    // Load the target image
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
                    // Loading the base point（sun/moon）

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


                    // Load the target image
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
                    // Loading the base point（sun/moon）

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


                    // Load the target image
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
                    // Loading the base point（sun/moon）

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


                    // Load the target image
                    Random random1 = new Random((int)DateTime.Now.Ticks % Int32.MaxValue);
                    int arraylength1 = imagePaths_tg[0].Length;
                    int index1 = random1.Next(arraylength1);
                    image_tg.Source = new BitmapImage(new Uri(imagePaths_tg[0][index1], UriKind.Relative));
                    image_tg.Stretch = System.Windows.Media.Stretch.Uniform;
                    //The less difficult the picture is, the bigger the picture is
                    image_tg.Width = tg_width + 50 - hardness;
                    image_tg.Height = tg_height + 50 - hardness;

                    angle = random_tg.NextDouble() * 2 * Math.PI;
                    radius = random_tg.NextDouble() * horizon;
                    x_tg = radius * Math.Cos(angle) - image_tg.Width / 2;
                    y_tg = radius * Math.Sin(angle) - image_tg.Height / 2;

                    Random random_ct = new Random();
                    // Loading the base point（sun/moon）

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
            // Add the current question results torecentResultsList

            // Only the last 5 results are retained
            int max = Math.Max(LevelUp, LevelDown); // AssumptionsMaxyesMathIn the classMaxmethod

            // make surerecentResultsThe size of the set does not exceedmax
            if (recentResults.Count > max)
            {
                recentResults.RemoveAt(0); // Remove the first element
            }

            if (recentResults.Count >= Math.Min(LevelUp, LevelDown))
            {
                // Calculate the number of correct answers in recent questions
                for (int i = recentResults.Count - LevelUp; i < recentResults.Count; i++)
                {
                    correctCount += recentResults[i] ? 1 : 0;
                }
                for (int i = recentResults.Count - LevelDown; i < recentResults.Count; i++)
                {
                    wrongCount += recentResults[i] ? 0 : 1;
                }

                // Increase difficulty
                if (correctCount == LevelUp && hardness < 24)
                {
                    hardness++;
                    max_hardness = Math.Max(max_hardness, hardness);
                    resetboollist();
                }

                // Reduce difficulty
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
            //Special case code
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
                            //textblock.Text = "Congratulations on getting right！";
                            //textblock.Foreground = new SolidColorBrush(Colors.Green);

                            //Pause stimulation interval time
                            countTimer.Interval = TimeSpan.FromSeconds(stimulus_interval);
                            judgement.Source = new BitmapImage(new Uri(JudgementPath[0], UriKind.Relative));
                            PlayWav(CorrectSoundPath);
                            ShowFeedbackImage(CorrectImage);
                            countTimer.Start();

                            LeftCostTime[hardness - 1].Add(costTime);
                            correctAnswers[hardness - 1]++; // Update the correct answer count for corresponding difficulty
                            TotalCorrectLeftCount[hardness - 1]++;
                            recentResults.Add(true);
                            // Check list length and delete the first element to keep list length 5
                            if (recentResults.Count > Math.Max(LevelUp, LevelDown))
                            {
                                recentResults.RemoveAt(0);
                            }
                        }
                        else
                        {
                            is_enter = true;
                            //textblock.Text = "Sorry to answer wrong！";
                            //textblock.Foreground = new SolidColorBrush(Colors.Red);

                            //Pause stimulation interval time
                            countTimer.Interval = TimeSpan.FromSeconds(stimulus_interval);
                            judgement.Source = new BitmapImage(new Uri(JudgementPath[1], UriKind.Relative));
                            PlayWav(ErrorSoundPath);
                            ShowFeedbackImage(ErrorImage);
                            countTimer.Start();

                            wrongAnswers[hardness - 1]++; // Update the error answer count for corresponding difficulty
                            if (IS_BEEP)
                                Console.Beep();
                            recentResults.Add(false);

                            // Check list length and delete the first element to keep list length 5
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
                                //textblock.Text = "Congratulations on getting right！";
                                //textblock.Foreground = new SolidColorBrush(Colors.Green);

                                //Pause stimulation interval time
                                countTimer.Interval = TimeSpan.FromSeconds(stimulus_interval);
                                judgement.Source = new BitmapImage(new Uri(JudgementPath[0], UriKind.Relative));
                                PlayWav(CorrectSoundPath);
                                ShowFeedbackImage(CorrectImage);
                                countTimer.Start();

                                RightCostTime[hardness - 1].Add(costTime);
                                ComplexCostTime[hardness - 1].Add(costTime);
                                correctAnswers[hardness - 1]++; // Update the correct answer count for corresponding difficulty
                                TotalCorrectRightCount[hardness - 1]++;

                                recentResults.Add(true);

                                // Check list length and delete the first element to keep list length 5
                                if (recentResults.Count > 5)
                                {
                                    recentResults.RemoveAt(0);
                                }

                            }
                            else
                            {
                                //textblock.Text = "Sorry to answer wrong！";
                                //textblock.Foreground = new SolidColorBrush(Colors.Red);

                                //Pause stimulation interval time
                                countTimer.Interval = TimeSpan.FromSeconds(stimulus_interval);
                                judgement.Source = new BitmapImage(new Uri(JudgementPath[1], UriKind.Relative));
                                PlayWav(ErrorSoundPath);
                                ShowFeedbackImage(ErrorImage);
                                countTimer.Start();

                                wrongAnswers[hardness - 1]++; // Update the error answer count for corresponding difficulty
                                if (IS_BEEP)
                                    Console.Beep();
                                recentResults.Add(false);

                                // Check list length and delete the first element to keep list length 5
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
            //General Case Code
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
                                //textblock.Text = "Congratulations on getting right！";
                                //textblock.Foreground = new SolidColorBrush(Colors.Green);

                                //Pause stimulation interval time
                                countTimer.Interval = TimeSpan.FromSeconds(stimulus_interval);
                                judgement.Source = new BitmapImage(new Uri(JudgementPath[0], UriKind.Relative));
                                PlayWav(CorrectSoundPath);
                                ShowFeedbackImage(CorrectImage);
                                countTimer.Start();

                                LeftCostTime[hardness - 1].Add(costTime);
                                ComplexCostTime[hardness - 1].Add(costTime);
                                ComplexCorrectCount[hardness - 1]++;
                                correctAnswers[hardness - 1]++; // Update the correct answer count for corresponding difficulty
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

                                // Check list length and delete the first element to keep list length 5
                                if (recentResults.Count > 5)
                                {
                                    recentResults.RemoveAt(0);
                                }

                            }
                            else
                            {
                                is_enter = true;
                                //textblock.Text = "Sorry to answer wrong！";
                                //textblock.Foreground = new SolidColorBrush(Colors.Red);

                                //Pause stimulation interval time
                                countTimer.Interval = TimeSpan.FromSeconds(stimulus_interval);
                                judgement.Source = new BitmapImage(new Uri(JudgementPath[1], UriKind.Relative));
                                PlayWav(ErrorSoundPath);
                                ShowFeedbackImage(ErrorImage);
                                countTimer.Start();

                                wrongAnswers[hardness - 1]++; // Update the error answer count for corresponding difficulty
                                if (IS_BEEP)
                                    Console.Beep();

                                recentResults.Add(false);

                                // Check list length and delete the first element to keep list length 5
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
                                //textblock.Text = "Congratulations on getting right！";
                                //textblock.Foreground = new SolidColorBrush(Colors.Green);

                                judgement.Source = new BitmapImage(new Uri(JudgementPath[0], UriKind.Relative));
                                PlayWav(CorrectSoundPath);
                                ShowFeedbackImage(CorrectImage);
                                LeftCostTime[hardness - 1].Add(costTime);
                                ComplexCostTime[hardness - 1].Add(costTime);
                                ComplexCorrectCount[hardness - 1]++;
                                correctAnswers[hardness - 1]++; // Update the correct answer count for corresponding difficulty
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

                                // Check list length and delete the first element to keep list length 5
                                if (recentResults.Count > 5)
                                {
                                    recentResults.RemoveAt(0);
                                }

                            }
                            else
                            {
                                is_enter = true;
                                //textblock.Text = "Sorry to answer wrong！";
                                //textblock.Foreground = new SolidColorBrush(Colors.Red);

                                judgement.Source = new BitmapImage(new Uri(JudgementPath[1], UriKind.Relative));
                                PlayWav(ErrorSoundPath);
                                ShowFeedbackImage(ErrorImage);
                                wrongAnswers[hardness - 1]++; // Update the error answer count for corresponding difficulty
                                if (IS_BEEP)
                                    Console.Beep();

                                recentResults.Add(false);

                                // Check list length and delete the first element to keep list length 5
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
                                //textblock.Text = "Congratulations on getting right！";
                                //textblock.Foreground = new SolidColorBrush(Colors.Green);

                                //Pause stimulation interval time
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
                                correctAnswers[hardness]++; // Update the correct answer count for corresponding difficulty
                                TotalCorrectRightCount[hardness - 1]++;
                                recentResults.Add(true);

                                // Check list length and delete the first element to keep list length 5
                                if (recentResults.Count > 5)
                                {
                                    recentResults.RemoveAt(0);
                                }
                            }
                            else
                            {
                                //textblock.Text = "Sorry to answer wrong！";
                                //textblock.Foreground = new SolidColorBrush(Colors.Red);

                                //Pause stimulation interval time
                                countTimer.Interval = TimeSpan.FromSeconds(stimulus_interval);
                                judgement.Source = new BitmapImage(new Uri(JudgementPath[1], UriKind.Relative));
                                PlayWav(ErrorSoundPath);
                                ShowFeedbackImage(ErrorImage);
                                countTimer.Start();

                                wrongAnswers[hardness]++; // Update the error answer count for corresponding difficulty
                                if (IS_BEEP)
                                    Console.Beep();

                                recentResults.Add(false);

                                // Check list length and delete the first element to keep list length 5
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
                                //textblock.Text = "Congratulations on getting right！";
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
                                correctAnswers[hardness]++; // Update the correct answer count for corresponding difficulty
                                TotalCorrectRightCount[hardness - 1]++;
                                recentResults.Add(true);

                                // Check list length and delete the first element to keep list length 5
                                if (recentResults.Count > 5)
                                {
                                    recentResults.RemoveAt(0);
                                }
                            }
                            else
                            {
                                //textblock.Text = "Sorry to answer wrong！";
                                //textblock.Foreground = new SolidColorBrush(Colors.Red);
                                judgement.Source = new BitmapImage(new Uri(JudgementPath[1], UriKind.Relative));
                                PlayWav(ErrorSoundPath);
                                ShowFeedbackImage(ErrorImage);
                                wrongAnswers[hardness]++; // Update the error answer count for corresponding difficulty
                                if (IS_BEEP)
                                    Console.Beep();

                                recentResults.Add(false);

                                // Check list length and delete the first element to keep list length 5
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
    public partial class Visual space training : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {

            max_time = 10;
            TRAIN_TIME = 120; // Training duration（Unit: seconds）
            IS_RESTRICT_TIME = false; // Whether to enable exercise time
            IS_BEEP = true; // Whether to make a sound
            hardness = 1; // Difficulty level
            train_time = TRAIN_TIME;
            TotalCount = 0;

            // parameter（Includes module parameter information）
            var baseParameter = BaseParameter;
            if (baseParameter.ProgramModulePars != null && baseParameter.ProgramModulePars.Any())
            {
                Debug.WriteLine("ProgramModulePars Loaded data:");

                // Traversal ProgramModulePars Print out each parameter
                foreach (var par in baseParameter.ProgramModulePars)
                {
                    /*Debug.WriteLine($"ProgramId: {par.ProgramId}, ModuleParId: {par.ModuleParId}, Value: {par.Value}, TableId: {par.TableId}");*/
                    if (par.ModulePar.ModuleId == baseParameter.ModuleId)
                    {
                        switch (par.ModuleParId) // Complete assignment
                        {
                            case 83: // Treatment time 
                                train_time = par.Value.HasValue ? (int)par.Value.Value * 60 : 120;
                                Debug.WriteLine($"TRAIN_TIME = {train_time}");
                                break;
                            case 85: // Level improvement
                                LevelUp = par.Value.HasValue ? (int)par.Value.Value : 3;
                                Debug.WriteLine($"LevelUp = {LevelUp}");
                                break;
                            case 86: // Level down
                                LevelDown = par.Value.HasValue ? (int)par.Value.Value : 3;
                                Debug.WriteLine($"LevelDown = {LevelDown}");
                                break;
                            case 88: // Auditory feedback
                                IS_BEEP = par.Value == 1;
                                Debug.WriteLine($"Whether to hear feedback = {IS_BEEP}");
                                break;
                            case 90: // Limit time
                                IS_RESTRICT_TIME = par.Value == 0;
                                Debug.WriteLine($"Whether to limit time = {IS_RESTRICT_TIME}");
                                break;
                            case 317://Stimulation interval
                                stimulus_interval = par.Value.HasValue ? (int)par.Value.Value : 3;
                                Debug.WriteLine($"Stimulation interval = {stimulus_interval}");
                                break;
                            // Add other things that need to be processed ModuleParId
                            case 176://grade
                                hardness = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"Initial difficulty = {hardness}");
                                break;
                        }
                    }
                }
            }
            else
            {
                Debug.WriteLine("No data");
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
            // Calling delegate
            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
            RightStatisticsAction?.Invoke(0, LevelUp);
            WrongStatisticsAction?.Invoke(0, LevelDown);
        }

        private void StarTimer_Tick(object sender, EventArgs e)
        {
            //Calling delegate
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
            trainingTimer.Start(); // Start the training timer
            StarTimer.Start();
            countTimer?.Stop();

            LoadImages();

            // Calling delegate
            VoiceTipAction?.Invoke("Please identify whether the object appears on the screen is on the left or right of the sun.");
            SynopsisAction?.Invoke("On the screen, there will be problems such as cars and boats appearing near the sun every once in a while. Please identify whether this object is on the left or right of the sun, and then press the left and right direction keys on the keyboard to confirm the selection.");
            RuleAction?.Invoke("On the screen, there will be problems such as cars and boats appearing near the sun every once in a while. Please identify whether this object is on the left or right of the sun, and then press the left and right direction keys on the keyboard to confirm the selection.");
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
            // Adjust the difficulty
            AdjustDifficulty();
            LoadImages();

            // Calling delegate
            VoiceTipAction?.Invoke("Please identify whether the object appears on the screen is on the left or right of the sun.");
            SynopsisAction?.Invoke("On the screen, there will be problems such as cars and boats appearing near the sun every once in a while. Please identify whether this object is on the left or right of the sun, and then press the left and right direction keys on the keyboard to confirm the selection.");
            RuleAction?.Invoke("On the screen, there will be problems such as cars and boats appearing near the sun every once in a while. Please identify whether this object is on the left or right of the sun, and then press the left and right direction keys on the keyboard to confirm the selection.");
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new Eye movement training explanation();
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
                        // Get data at the current difficulty level
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
                        // Calculation accuracy
                        double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);



                        // create Result Record
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "Visual space training",
                            Eval = false,
                            Lv = max_hardness, // Current difficulty level
                            ScheduleId = BaseParameter.ScheduleId ?? null // Assumption Schedule_id, can be replaced with the actual value
                        };

                        db.Results.Add(newResult);
                        await db.SaveChangesAsync();

                        // get result_id
                        int result_id = newResult.ResultId;

                        // create ResultDetail Object List
                        var resultDetails = new List<ResultDetail>
                            {
                                 new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 0,
                                    ValueName = "grade",
                                    Value = max_hardness,
                                    ModuleId = BaseParameter.ModuleId //  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order= 12,
                                    ValueName = "Stimulate",
                                    Value = totalcount,
                                    ModuleId = BaseParameter.ModuleId //  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order= 1,
                                    ValueName = "correct Left half（%）",
                                    Value = totalleftacc * 100,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 2,
                                    ValueName = "correct Right half（%）",
                                    Value = totalrightacc * 100,
                                    Charttype = "Bar chart" ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 3, 
                                    ValueName = "correct Two objects（%）",
                                    Value = complexacc * 100, // Stored as a percentage
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 4,
                                    ValueName = "correct Top left",
                                    Value = complexleftup,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 5,
                                    ValueName = "correct Lower left",
                                    Value = complexleftdown ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 6,
                                    ValueName = "correct Top right",
                                    Value = complexrightup,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 7,  
                                    ValueName = "correct Lower right",
                                    Value = complexrightdown,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order=8,
                                    ValueName = "mistake total",
                                    Value = wrongallCount,
                                    Maxvalue = wrongallCount,
                                    Charttype = "Bar chart" ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order=9,
                                    ValueName = "Intermediate value Reaction time Left half（ms）",
                                    Value = averageleft * 1000,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                   ResultId = result_id,
                                   Order=10,
                                    ValueName = "Intermediate value Reaction time Right half（ms）",
                                    Value = averageright * 1000,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                   ResultId = result_id,
                                   Order=11,
                                    ValueName = "Intermediate value Reaction time Two objects（ms）",
                                    Value = comaverage * 1000,
                                    ModuleId = BaseParameter.ModuleId
                                }
                            };

                        // insert ResultDetail data
                        db.ResultDetails.AddRange(resultDetails);
                        await db.SaveChangesAsync();

                        // Output each ResultDetail Object data
                        Debug.WriteLine($"Difficulty level {max_hardness}:");
                        foreach (var detail in resultDetails)
                        {
                            Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId}");
                        }


                        // Submit transactions
                        await transaction.CommitAsync();
                        Debug.WriteLine("Insert successfully");
                    });
                }
                catch (Exception ex)
                {
                    // Roll back transactions
                    await transaction.RollbackAsync();
                    Debug.WriteLine(ex.ToString());
                }
            }
        }
    }
}
