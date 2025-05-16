using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
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
using System.Windows.Shapes;
using System.Windows.Threading;
namespace crs.game.Games
{
    /// <summary>
    /// 警觉训练讲解.xaml 的交互逻辑
    /// </summary>
    public partial class 警觉训练讲解 : BaseUserControl
    {
        public 警觉训练讲解()
        {
            InitializeComponent();

            level = 1;

            SetLevelParametre(level);

            totalTrainTime = totalTrain_time * 60;
            levelTrainTime = 0;
            previousLevelTrainTime = 0;
            curLevelStateType = 1;


            InitRecords();

            LoadResources();
            AddCurLevelImage();

            InitCanvas();

            InitializeTimer();

            pauseState = false;


            this.Loaded += 警觉训练讲解_Loaded;
        }

        private void 警觉训练讲解_Loaded(object sender, RoutedEventArgs e)
        {
            Button_2_Click(null, null);
        }


        void StartGame()
        {
            pauseState = false;
            StartAllTimer();
        }


        private Random random = new Random();


        /// <summary>
        /// 前端获取的参数
        /// </summary>
        int totalTrain_time = 10;     //训练时间
        int INCREASE = 95; // 提高难度的阈值
        int DECREASE = 80;  // 降低难度的阈值
        int stimulationCount = 20;   //受刺激的次数

        int _stimulationInterval_preTime = 2500;    //刺激间隔的前端 ms
        int _stimulationInterval_midTime = 2500;  //刺激之间的间隔⸺从警告⾳到刺激处（ms）
        int _stimulationInterval_postTime = 2500;     //刺激之间的间隔⸺受刺激之后（ms）

        //实际
        int stimulationInterval_preTime = 2500;    //刺激间隔的前端 ms
        int stimulationInterval_midTime = 2500;  //刺激之间的间隔⸺从警告⾳到刺激处（ms）
        int stimulationInterval_postTime = 2500;     //刺激之间的间隔⸺受刺激之后（ms）

        int stopWarningVioce_preTime = 10;  //⾃动的中⽌训练⸺到有警告⾳的时间(s)
        int stopWarningVioce_postTime = 10;  //⾃动的中⽌训练⸺警告⾳之后的时间（s）

        bool useLongReactionTime = true;    //
        bool useShortReactionTime = false;    //
        bool useFixedReactionTime = false;//
        int fixedReactionTime_preTime = 2000;   //ms

        bool fixedHasWarningVoice = false;       //是否一定有警告音
        bool fixedHasnotWarningVoice = true;       //是否一定没有警告音
        bool changeWarningVoice = false;  //根据等级

        bool showCockpit = false;   //是否显示驾驶舱



        /// <summary>
        /// 等级有关参数
        /// </summary>
        int level = 1;
        int backgroundType = 1;   //1乡间小路，2林道，3乡村道路，4城市道路
        bool hasWarningVoice = false;

        int longReactionTime = 1200;
        int shortReactionTime = 700;

        int objectPositionType = 1;     //1中央，2右-左，3变化
        bool isBackgroundGrey = true; // 黑白和彩色

        int trainingMaterialCount_1 = 1;    //车
        int trainingMaterialCount_2 = 1;    //动物
        int trainingMaterialCount_3 = 1;    //人

        int roadSignType = 1;   //1无，2交通标志，3交通标志和红绿灯

        int curLevelReactionTime = 1200;        //当前的最大反应时间

        /// <summary>
        /// 计时器
        /// </summary>
        private DispatcherTimer _totalTimerTrain;  //总治疗时间
        private DispatcherTimer _levelTimerTrain;   //

        int totalTrainTime = 1000; //总治疗时间
        int levelTrainTime = 0;
        int previousLevelTrainTime = 0;
        DateTime startTime;
        DateTime endTime;

        int curLevelStateType = 1;   //1出现物体前项，2警告声音出现，3物体出现，4警告声音出现，5隐藏

        bool pauseState = true;

        /// <summary>
        /// 记录分数
        /// </summary>
        int correctCount = 0;
        int mistakeCount = 0;
        int totalReactionTime = 0;//本次训练总的反应时间
        int countReactionTimes = 0;//本次训练总的反应次数
        List<int> record;
        List<List<int>> allLevelRecord; //记录每个等级的结果

        List<Vector2> allLevelReactionTime;




        /// <summary>
        /// 加载素材资源
        /// </summary>
        List<BitmapImage> images_object_1 = new List<BitmapImage>();     // 用于存储图片的列表
        List<BitmapImage> images_object_2 = new List<BitmapImage>();     // 用于存储图片的列表
        List<BitmapImage> images_object_3 = new List<BitmapImage>();     // 用于存储图片的列表

        List<BitmapImage> images_object_curLevel = new List<BitmapImage>();

        Image myImage;
        void LoadResources()
        {
            images_object_1.Clear(); // 清空现有图片列表
            images_object_2.Clear(); // 清空现有图片列表
            images_object_3.Clear(); // 清空现有图片列表

            string currentDirectory = Directory.GetCurrentDirectory();
            string imageFolderPath = System.IO.Path.Combine(currentDirectory, "Resources/警觉训练/素材/");


            try
            {
                string folderPath = System.IO.Path.Combine(imageFolderPath, "车/");
                foreach (string file in Directory.GetFiles(folderPath))
                {
                    if (IsImageFile(file))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(file, UriKind.Absolute);
                        bitmap.EndInit();
                        images_object_1.Add(bitmap);
                    }
                }


                folderPath = System.IO.Path.Combine(imageFolderPath, "动物/");
                foreach (string file in Directory.GetFiles(folderPath))
                {
                    if (IsImageFile(file))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(file, UriKind.Absolute);
                        bitmap.EndInit();
                        images_object_2.Add(bitmap);
                    }
                }


                folderPath = System.IO.Path.Combine(imageFolderPath, "人/");
                foreach (string file in Directory.GetFiles(folderPath))
                {
                    if (IsImageFile(file))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(file, UriKind.Absolute);
                        bitmap.EndInit();
                        images_object_3.Add(bitmap);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error loading images: " + ex.Message);
            }
        }

        void AddCurLevelImage()
        {
            images_object_curLevel.Clear();
            while (images_object_curLevel.Count < trainingMaterialCount_1)
            {
                BitmapImage item = images_object_1[random.Next(images_object_1.Count)];
                if (!images_object_curLevel.Contains(item))
                {
                    images_object_curLevel.Add(item);
                }
            }
            while (images_object_curLevel.Count < trainingMaterialCount_1 + trainingMaterialCount_2)
            {
                BitmapImage item = images_object_2[random.Next(images_object_2.Count)];
                if (!images_object_curLevel.Contains(item))
                {
                    images_object_curLevel.Add(item);
                }
            }
            while (images_object_curLevel.Count < trainingMaterialCount_1 + trainingMaterialCount_2 + trainingMaterialCount_3)
            {
                BitmapImage item = images_object_3[random.Next(images_object_3.Count)];
                if (!images_object_curLevel.Contains(item))
                {
                    images_object_curLevel.Add(item);
                }
            }


        }
        private void InitRecords()
        {
            record = new List<int>();
            allLevelRecord = new List<List<int>>();
            allLevelReactionTime = new List<Vector2>();
            for (int i = 0; i < 20; i++)
            {
                allLevelRecord.Add(new List<int>());
                allLevelReactionTime.Add(new Vector2(0f, 0f));
            }
        }

        private bool IsImageFile(string file)
        {
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff" };
            var extension = System.IO.Path.GetExtension(file).ToLower();
            return imageExtensions.Contains(extension);
        }

        void SetLevelParametre(int level = 1)
        {
            switch (level)
            {
                case 1:
                    backgroundType = 1; hasWarningVoice = true; longReactionTime = 1200; shortReactionTime = 700;
                    objectPositionType = 1; isBackgroundGrey = true; roadSignType = 1;
                    break;
                case 2:
                    backgroundType = 2; hasWarningVoice = true; longReactionTime = 1150; shortReactionTime = 670;
                    objectPositionType = 1; isBackgroundGrey = true; roadSignType = 1;
                    break;
                case 3:
                    backgroundType = 3; hasWarningVoice = true; longReactionTime = 1060; shortReactionTime = 610;
                    objectPositionType = 1; isBackgroundGrey = false; roadSignType = 2;
                    break;
                case 4:
                    backgroundType = 3; hasWarningVoice = true; longReactionTime = 1200; shortReactionTime = 700;
                    objectPositionType = 2; isBackgroundGrey = false; roadSignType = 2;
                    break;
                case 5:
                    backgroundType = 1; hasWarningVoice = true; longReactionTime = 1020; shortReactionTime = 580;
                    objectPositionType = 2; isBackgroundGrey = false; roadSignType = 2;
                    break;
                case 6:
                    backgroundType = 3; hasWarningVoice = true; longReactionTime = 980; shortReactionTime = 550;
                    objectPositionType = 2; isBackgroundGrey = false; roadSignType = 2;
                    break;
                case 7:
                    backgroundType = 1; hasWarningVoice = false; longReactionTime = 940; shortReactionTime = 520;
                    objectPositionType = 2; isBackgroundGrey = false; roadSignType = 2;
                    break;
                case 8:
                    backgroundType = 1; hasWarningVoice = false; longReactionTime = 900; shortReactionTime = 490;
                    objectPositionType = 2; isBackgroundGrey = false; roadSignType = 2;
                    break;
                case 9:
                    backgroundType = 4; hasWarningVoice = false; longReactionTime = 870; shortReactionTime = 470;
                    objectPositionType = 3; isBackgroundGrey = false; roadSignType = 2;
                    break;
                case 10:
                    backgroundType = 3; hasWarningVoice = false; longReactionTime = 840; shortReactionTime = 450;
                    objectPositionType = 3; isBackgroundGrey = false; roadSignType = 2;
                    break;
                case 11:
                    backgroundType = 3; hasWarningVoice = false; longReactionTime = 810; shortReactionTime = 430;
                    objectPositionType = 3; isBackgroundGrey = false; roadSignType = 2;
                    break;
                case 12:
                    backgroundType = 4; hasWarningVoice = false; longReactionTime = 780; shortReactionTime = 410;
                    objectPositionType = 3; isBackgroundGrey = false; roadSignType = 2;
                    break;
                case 13:
                    backgroundType = 2; hasWarningVoice = false; longReactionTime = 750; shortReactionTime = 390;
                    objectPositionType = 3; isBackgroundGrey = false; roadSignType = 2;
                    break;
                case 14:
                    backgroundType = 4; hasWarningVoice = false; longReactionTime = 720; shortReactionTime = 370;
                    objectPositionType = 3; isBackgroundGrey = false; roadSignType = 2;
                    break;
                case 15:
                    backgroundType = 4; hasWarningVoice = true; longReactionTime = 700; shortReactionTime = 350;
                    objectPositionType = 3; isBackgroundGrey = false; roadSignType = 3;
                    break;
                case 16:
                    backgroundType = 4; hasWarningVoice = true; longReactionTime = 680; shortReactionTime = 330;
                    objectPositionType = 3; isBackgroundGrey = false; roadSignType = 3;
                    break;

            }
            Random random = new Random();
            int tmp = random.Next(0, 2);
            if (tmp == 0)
            {
                switch (level)
                {
                    case 1:
                        trainingMaterialCount_1 = 6; trainingMaterialCount_2 = 0; trainingMaterialCount_3 = 0;
                        break;
                    case 2:
                        trainingMaterialCount_1 = 0; trainingMaterialCount_2 = 6; trainingMaterialCount_3 = 0;
                        break;
                    case 3:
                        trainingMaterialCount_1 = 6; trainingMaterialCount_2 = 0; trainingMaterialCount_3 = 0;
                        break;
                    case 4:
                        trainingMaterialCount_1 = 8; trainingMaterialCount_2 = 0; trainingMaterialCount_3 = 0;
                        break;
                    case 5:
                        trainingMaterialCount_1 = 8; trainingMaterialCount_2 = 0; trainingMaterialCount_3 = 0;
                        break;
                    case 6:
                        trainingMaterialCount_1 = 6; trainingMaterialCount_2 = 1; trainingMaterialCount_3 = 1;
                        break;
                    case 7:
                        trainingMaterialCount_1 = 8; trainingMaterialCount_2 = 0; trainingMaterialCount_3 = 0;
                        break;
                    case 8:
                        trainingMaterialCount_1 = 8; trainingMaterialCount_2 = 0; trainingMaterialCount_3 = 0;
                        break;
                    case 9:
                        trainingMaterialCount_1 = 7; trainingMaterialCount_2 = 1; trainingMaterialCount_3 = 0;
                        break;
                    case 10:
                        trainingMaterialCount_1 = 3; trainingMaterialCount_2 = 5; trainingMaterialCount_3 = 0;
                        break;
                    case 11:
                        trainingMaterialCount_1 = 6; trainingMaterialCount_2 = 0; trainingMaterialCount_3 = 4;
                        break;
                    case 12:
                        trainingMaterialCount_1 = 6; trainingMaterialCount_2 = 1; trainingMaterialCount_3 = 3;
                        break;
                    case 13:
                        trainingMaterialCount_1 = 0; trainingMaterialCount_2 = 10; trainingMaterialCount_3 = 0;
                        break;
                    case 14:
                        trainingMaterialCount_1 = 5; trainingMaterialCount_2 = 1; trainingMaterialCount_3 = 4;
                        break;
                    case 15:
                        trainingMaterialCount_1 = 6; trainingMaterialCount_2 = 1; trainingMaterialCount_3 = 5;
                        break;
                    case 16:
                        trainingMaterialCount_1 = 5; trainingMaterialCount_2 = 3; trainingMaterialCount_3 = 6;
                        break;
                }
            }
            else
            {
                switch (level)
                {
                    case 1:
                        trainingMaterialCount_1 = 6; trainingMaterialCount_2 = 0; trainingMaterialCount_3 = 0;
                        break;
                    case 2:
                        trainingMaterialCount_1 = 0; trainingMaterialCount_2 = 6; trainingMaterialCount_3 = 0;
                        break;
                    case 3:
                        trainingMaterialCount_1 = 6; trainingMaterialCount_2 = 0; trainingMaterialCount_3 = 0;
                        break;
                    case 4:
                        trainingMaterialCount_1 = 8; trainingMaterialCount_2 = 0; trainingMaterialCount_3 = 0;
                        break;
                    case 5:
                        trainingMaterialCount_1 = 8; trainingMaterialCount_2 = 0; trainingMaterialCount_3 = 0;
                        break;
                    case 6:
                        trainingMaterialCount_1 = 6; trainingMaterialCount_2 = 1; trainingMaterialCount_3 = 1;
                        break;
                    case 7:
                        trainingMaterialCount_1 = 8; trainingMaterialCount_2 = 0; trainingMaterialCount_3 = 0;
                        break;
                    case 8:
                        trainingMaterialCount_1 = 7; trainingMaterialCount_2 = 0; trainingMaterialCount_3 = 1;
                        break;
                    case 9:
                        trainingMaterialCount_1 = 7; trainingMaterialCount_2 = 1; trainingMaterialCount_3 = 0;
                        break;
                    case 10:
                        trainingMaterialCount_1 = 3; trainingMaterialCount_2 = 5; trainingMaterialCount_3 = 0;
                        break;
                    case 11:
                        trainingMaterialCount_1 = 6; trainingMaterialCount_2 = 1; trainingMaterialCount_3 = 3;
                        break;
                    case 12:
                        trainingMaterialCount_1 = 7; trainingMaterialCount_2 = 0; trainingMaterialCount_3 = 3;
                        break;
                    case 13:
                        trainingMaterialCount_1 = 0; trainingMaterialCount_2 = 10; trainingMaterialCount_3 = 0;
                        break;
                    case 14:
                        trainingMaterialCount_1 = 3; trainingMaterialCount_2 = 2; trainingMaterialCount_3 = 5;
                        break;
                    case 15:
                        trainingMaterialCount_1 = 6; trainingMaterialCount_2 = 2; trainingMaterialCount_3 = 4;
                        break;
                    case 16:
                        trainingMaterialCount_1 = 5; trainingMaterialCount_2 = 3; trainingMaterialCount_3 = 6;
                        break;
                }
            }

            //如果固定出现或没有警告音
            if (changeWarningVoice == false)
            {
                if (fixedHasWarningVoice)
                {
                    hasWarningVoice = true;
                }
                else
                {
                    hasWarningVoice = false;

                }
            }
            if (hasWarningVoice == false)
            {
                stimulationInterval_midTime = 0;
            }

            //如果使用固定最大反应时间
            if (useFixedReactionTime == true)
            {
                curLevelReactionTime = fixedReactionTime_preTime;
            }
            else
            {
                if (useLongReactionTime == true)
                {
                    curLevelReactionTime = longReactionTime;
                }
                else
                {
                    curLevelReactionTime = shortReactionTime;
                }
            }
        }

        void CalculateScore()
        {
            if ((float)100 * record.Sum() / record.Count >= INCREASE)
            {
                if (level < 16)
                {
                    level += 1;
                }

            }
            else if ((float)100 * record.Sum() / record.Count <= DECREASE)
            {
                if (level > 1)
                {
                    level -= 1;
                }
            }

            record.Clear();
        }

        void NextRound()
        {
            SetLevelParametre(level);
            InitCanvas();
            levelTrainTime = 0;
            previousLevelTrainTime = 0;
            startTime = DateTime.Now;
            curLevelStateType = 1;


        }

        void AdjustLevel()
        {

        }

        void InitCanvas()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string folderPath = System.IO.Path.Combine(currentDirectory, "Resources/警觉训练/");

            Image_1.Source = new BitmapImage(new Uri(folderPath + "x.png", UriKind.Absolute));
            Image_2.Source = new BitmapImage(new Uri(folderPath + "y.png", UriKind.Absolute));

            // 获取当前工作目录
            currentDirectory = Directory.GetCurrentDirectory();
            string imageFolderPath = System.IO.Path.Combine(currentDirectory, "Resources/警觉训练/");

            audioPlayer.Source = new Uri(imageFolderPath + "警告音/1.mp3", UriKind.RelativeOrAbsolute);

            imageFolderPath = System.IO.Path.Combine(currentDirectory, "Resources/警觉训练/背景/");

            switch (backgroundType)
            {
                case 1:
                    imageFolderPath += "乡间小路/";
                    break;
                case 2:
                    imageFolderPath += "林道/";
                    break;
                case 3:
                    imageFolderPath += "乡村道路/";
                    break;
                case 4:
                    imageFolderPath += "城市道路/";
                    break;
            }

            switch (roadSignType)
            {
                case 1:
                    imageFolderPath += "无/";
                    break;
                case 2:
                    imageFolderPath += "交通标志/";
                    break;
                case 3:
                    imageFolderPath += "交通标志和交通信号灯/";
                    break;
            }

            //ImageBrush backgroundBrush = new ImageBrush();
            BitmapImage bitmapBG = new BitmapImage();
            bitmapBG.BeginInit();
            bitmapBG.UriSource = new Uri(imageFolderPath + "1.png"); // 图像资源路径
            bitmapBG.CacheOption = BitmapCacheOption.OnLoad; // 优化加载性能
            bitmapBG.EndInit();
            //// 设置 ImageBrush 的 ImageSource 属性
            //backgroundBrush.ImageSource = bitmapBG;
            //// 设置 Canvas 的 Background 属性
            //canvas.Background = backgroundBrush;

            WriteableBitmap writeableBitmap = new WriteableBitmap(bitmapBG);

            // 转换为灰度图像
            if (isBackgroundGrey == true)
            {

                TransformToGrayScale(writeableBitmap);
            }
            // 设置为Canvas背景
            canvas.Background = new ImageBrush(writeableBitmap);

            //是否显示驾驶舱
            if (showCockpit == true)
            {
                string cockpitImageFolderPath = System.IO.Path.Combine(currentDirectory, "../../../crs.game/警觉训练/驾驶舱/");
                BitmapImage cockpitImage = new BitmapImage();
                cockpitImage.BeginInit();
                cockpitImage.UriSource = new Uri(cockpitImageFolderPath + "1.png"); // 图像资源路径
                cockpitImage.CacheOption = BitmapCacheOption.OnLoad; // 优化加载性能
                cockpitImage.EndInit();
            }

            myImage = new Image();
            myImage.Source = new BitmapImage();
            myImage.Height = double.NaN;
            myImage.Width = 260;
            Canvas.SetLeft(myImage, 450);
            Canvas.SetBottom(myImage, 240);
            canvas.Children.Add(myImage);



        }

        //将图片转换为灰度图像
        private void TransformToGrayScale(WriteableBitmap source)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int[] pixels = new int[width * height];

            source.CopyPixels(pixels, width * 4, 0);

            for (int i = 0; i < pixels.Length; i++)
            {
                int color = pixels[i];
                byte grayScale = (byte)((color >> 16 & 0xFF) * 0.3 + (color >> 8 & 0xFF) * 0.59 + (color & 0xFF) * 0.11);
                pixels[i] = (grayScale << 16) | (grayScale << 8) | grayScale;
            }

            source.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);
        }


        /// <summary>
        /// 播放警告音
        /// </summary>
        bool isAudioPlayerPlaying = false;
        void AudioPlayerPlay_Once()
        {
            audioPlayer.Stop(); // 确保音频停止
            audioPlayer.Play(); // 播放一次
        }


        void AudioPlayerPlay_Repeat()
        {
            if (isAudioPlayerPlaying == false)
            {
                isAudioPlayerPlaying = true;
                audioPlayer.Stop(); // 确保音频停止
                audioPlayer.LoadedBehavior = MediaState.Manual;
                audioPlayer.UnloadedBehavior = MediaState.Manual;
                audioPlayer.MediaEnded += AudioPlayer_MediaEnded; // 添加媒体结束事件
                audioPlayer.Play(); // 开始播放
            }
        }

        private void AudioPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            audioPlayer.Position = TimeSpan.Zero; // 重置到开始
            audioPlayer.Play(); // 继续播放
        }
        void AudioPlayerPlay_Stop()
        {
            isAudioPlayerPlaying = false;
            audioPlayer.Stop(); // 停止播放
            audioPlayer.MediaEnded -= AudioPlayer_MediaEnded; // 移除媒体结束事件
        }

        /// <summary>
        /// 键盘事件
        /// </summary>
        bool _isSpaceKeyPressed = false;

        override protected void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (pauseState == true)
            {
                return;
            }
            if (e.Key == Key.Enter)
            {

                endTime = DateTime.Now;
                if (curLevelStateType == 3 || curLevelStateType == 4)
                {
                    if ((endTime - startTime).TotalMilliseconds <= curLevelReactionTime)
                    {
                        totalReactionTime += (int)(endTime - startTime).TotalMilliseconds;
                        countReactionTimes += 1;

                        correctCount += 1;
                        record.Add(1);
                        allLevelRecord[level].Add(1);
                        allLevelReactionTime[level] += new Vector2((float)(endTime - startTime).TotalMilliseconds, 1);

                        StopAllTimer();
                        end.Visibility = Visibility.Visible;

                    }
                    else
                    {
                        totalReactionTime += (int)(endTime - startTime).TotalMilliseconds;
                        countReactionTimes += 1;

                        mistakeCount += 1;
                        record.Add(0);
                        allLevelRecord[level].Add(0);
                        allLevelReactionTime[level] += new Vector2((float)(endTime - startTime).TotalMilliseconds, 1);


                        myImage.Source = new BitmapImage();
                        AudioPlayerPlay_Stop();

                        previousLevelTrainTime = levelTrainTime;
                        startTime = DateTime.Now;

                        curLevelStateType = 5;
                    }



                }
                else
                {
                    mistakeCount += 1;
                    record.Add(0);
                    allLevelRecord[level].Add(0);
                }

                if (record.Count > stimulationCount)
                {
                    CalculateScore();
                    AdjustLevel();
                    NextRound();
                }

            }
        }


        private void InitializeTimer()
        {
            _totalTimerTrain = new DispatcherTimer();
            _totalTimerTrain.Interval = TimeSpan.FromSeconds(1);
            _totalTimerTrain.Tick += TimerTotalTrain_Tick;

            _levelTimerTrain = new DispatcherTimer();
            _levelTimerTrain.Interval = TimeSpan.FromSeconds(1);
            _levelTimerTrain.Tick += TimerLevelTrain_Tick;
        }

        void StartAllTimer()
        {
            _totalTimerTrain?.Start();
            _levelTimerTrain?.Start();
        }

        void StopAllTimer()
        {
            _totalTimerTrain?.Stop();
            _levelTimerTrain?.Stop();
        }

        void TimerTotalTrain_Tick(object sender, EventArgs e)
        {
            totalTrainTime--;
            if (totalTrainTime <= 0)
            {

            }

            TimeStatisticsAction?.Invoke(totalTrainTime, levelTrainTime);
        }
        void TimerLevelTrain_Tick(object sender, EventArgs e)
        {
            levelTrainTime += 1;
            endTime = DateTime.Now;
            if (curLevelStateType == 1)
            {
                if ((endTime - startTime).TotalMilliseconds > stimulationInterval_preTime)
                {
                    previousLevelTrainTime = levelTrainTime;
                    startTime = DateTime.Now;

                    curLevelStateType = 2;

                    if (hasWarningVoice == true)
                    {
                        AudioPlayerPlay_Once();
                    }
                }
            }
            else if (curLevelStateType == 2)
            {
                if (hasWarningVoice == false)
                {
                    previousLevelTrainTime = levelTrainTime;


                    myImage.Source = images_object_curLevel[random.Next(images_object_curLevel.Count)];

                    startTime = DateTime.Now;

                    curLevelStateType = 3;

                }
                else
                {
                    if ((endTime - startTime).TotalMilliseconds > stimulationInterval_midTime)
                    {
                        previousLevelTrainTime = levelTrainTime;


                        myImage.Source = images_object_curLevel[random.Next(images_object_curLevel.Count)];

                        startTime = DateTime.Now;

                        curLevelStateType = 3;
                    }
                }
            }
            else if (curLevelStateType == 3)
            {
                if ((endTime - startTime).TotalMilliseconds > stopWarningVioce_preTime * 1000)
                {
                    //previousLevelTrainTime = levelTrainTime;

                    AudioPlayerPlay_Repeat();
                    curLevelStateType = 4;
                }
            }
            else if (curLevelStateType == 4)
            {
                if ((endTime - startTime).TotalMilliseconds > (stopWarningVioce_preTime + stopWarningVioce_postTime) * 1000)
                {
                    AudioPlayerPlay_Stop();
                }
            }
            else if (curLevelStateType == 5)
            {
                if ((endTime - startTime).TotalMilliseconds > stimulationInterval_postTime)
                {
                    previousLevelTrainTime = levelTrainTime;
                    endTime = DateTime.Now;

                    curLevelStateType = 1;
                }
            }


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
                        Text_1.Visibility = Visibility.Hidden;
                        Image_1.Visibility = Visibility.Hidden;
                        Text_2.Visibility = Visibility.Hidden;
                        Image_2.Visibility = Visibility.Hidden;

                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Collapsed;

                        ALL_Canvs.Visibility = Visibility.Visible;
                        //LJN，在讲解模块调用委托
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("您在屏幕上会看到一个道路上的图像，接下来图像上会出现某些物体，当您看到物体出现时，请迅速按下键盘上的OK键。");//增加代码，调用函数，显示数字人下的文字
                        //LJN
                        StartGame();
                    }
                    break;
            }
        }
    }
}
