using crs.core;
using crs.core.DbModels;
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
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Media.Animation;



namespace crs.game.Games
{
    /// <summary>
    /// 警觉训练.xaml 的交互逻辑
    /// </summary>
    public partial class 警觉训练 : BaseUserControl
    {
        public 警觉训练()
        {
            InitializeComponent();

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
        bool fixedHasnotWarningVoice = false;       //是否一定没有警告音
        int changeWarningVoice = 1;  //根据等级

        bool showCockpit = false;   //是否显示驾驶舱



        /// <summary>
        /// 等级有关参数
        /// </summary>
        int level = 1;
        int max_hardness = 1;
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

        List<(BitmapImage, int)> images_object_curLevel = new List<(BitmapImage, int)>();

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
                BitmapImage _item = images_object_1[random.Next(images_object_1.Count)];
                var item = (_item, 1);
                if (!images_object_curLevel.Contains(item))
                {
                    images_object_curLevel.Add(item);
                }
            }
            while (images_object_curLevel.Count < trainingMaterialCount_1 + trainingMaterialCount_2)
            {
                BitmapImage _item = images_object_2[random.Next(images_object_2.Count)];
                var item = (_item, 2);
                if (!images_object_curLevel.Contains(item))
                {
                    images_object_curLevel.Add(item);
                }
            }
            while (images_object_curLevel.Count < trainingMaterialCount_1 + trainingMaterialCount_2 + trainingMaterialCount_3)
            {
                BitmapImage _item = images_object_3[random.Next(images_object_3.Count)];
                var item = (_item, 3);
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
            if (changeWarningVoice == 1)
            {
                //if (fixedHasWarningVoice)
                //{
                //    hasWarningVoice = true;
                //}
                //else
                //{
                //    hasWarningVoice = false;

                //}
            }
            else if (changeWarningVoice == 2)
            {
                hasWarningVoice = false;
            }
            else if (changeWarningVoice == 3)
            {
                hasWarningVoice = true;
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


        }

        void NextRound()
        {
            SetLevelParametre(level);
            InitCanvas();
            levelTrainTime = 0;
            previousLevelTrainTime = 0;
            startTime = DateTime.Now;
            curLevelStateType = 1;
            storyboard?.Stop();
            myImage.Source = new BitmapImage();

        }

        void AdjustLevel()
        {
            if (record.Count >= stimulationCount)
            {
                if ((float)100 * record.Sum() / record.Count >= INCREASE)
                {
                    if (level < 16)
                    {
                        level += 1;
                        max_hardness = Math.Max(max_hardness, level);
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
            // 调用委托
            LevelStatisticsAction?.Invoke(level, 16);
            RightStatisticsAction?.Invoke(record.Sum(), stimulationCount);
            WrongStatisticsAction?.Invoke(record.Count - record.Sum(), stimulationCount);
        }

        void InitCanvas()
        {
            // 获取当前工作目录
            string currentDirectory = Directory.GetCurrentDirectory();
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
                string cockpitImageFolderPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Resources/警觉训练/驾驶舱/");
                BitmapImage cockpitImage = new BitmapImage();
                cockpitImage.BeginInit();
                cockpitImage.UriSource = new Uri(cockpitImageFolderPath + "/1.png"); // 图像资源路径
                cockpitImage.CacheOption = BitmapCacheOption.OnLoad; // 优化加载性能
                cockpitImage.EndInit();
                cockpit.Source = cockpitImage;
                cockpit.Visibility = Visibility.Visible;
            }
            else
            {
                cockpit.Visibility = Visibility.Collapsed;
            }

            myImage = new Image();
            myImage.Source = new BitmapImage();
            myImage.Height = 150;

            Canvas.SetLeft(myImage, 570);
            

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

                    }
                    else
                    {
                        totalReactionTime += (int)(endTime - startTime).TotalMilliseconds;
                        countReactionTimes += 1;

                        mistakeCount += 1;
                        record.Add(0);
                        allLevelRecord[level].Add(0);
                        allLevelReactionTime[level] += new Vector2((float)(endTime - startTime).TotalMilliseconds, 1);
                    }
                    myImage.Source = new BitmapImage();
                    storyboard?.Stop();
                    AudioPlayerPlay_Stop();

                    previousLevelTrainTime = levelTrainTime;
                    startTime = DateTime.Now;

                    curLevelStateType = 5;


                }
                else
                {
                    mistakeCount += 1;
                    record.Add(0);
                    allLevelRecord[level].Add(0);
                }

                if (record.Count >= stimulationCount)
                {
                    CalculateScore();
                    AdjustLevel();
                    NextRound();
                }
                // 调用委托
                LevelStatisticsAction?.Invoke(level, 16);
                RightStatisticsAction?.Invoke(record.Sum(), stimulationCount);
                WrongStatisticsAction?.Invoke(record.Count - record.Sum(), stimulationCount);

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
                Stop();
                OnGameEnd();
            }

            TimeStatisticsAction?.Invoke(totalTrainTime, levelTrainTime);
        }
        async void TimerLevelTrain_Tick(object sender, EventArgs e)
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

                    var item = images_object_curLevel[random.Next(images_object_curLevel.Count)];
                    myImage.Source = item.Item1;
                    if (item.Item2 == 1)
                    {
                        myImage.Height = double.NaN;
                        myImage.Width = 260;
                    }
                    else if (item.Item2 == 2)
                    {
                        myImage.Height = 150;
                        myImage.Width = double.NaN;
                    }
                    else if (item.Item2 == 3)
                    {
                        myImage.Height = 220;
                        myImage.Width = double.NaN;
                    }
                    if (objectPositionType == 1)
                    {
                        Canvas.SetLeft(myImage, 570);
                    }
                    else if (item.Item2 == 1 || objectPositionType == 2)
                    {
                        Canvas.SetLeft(myImage, 470 + random.Next(1) * 200);
                    }
                    else if (objectPositionType == 3)
                    {
                        MoveLeft2RightAnimation(null, null);
                    }
                    Canvas.SetBottom(myImage, 240);

                    startTime = DateTime.Now;

                    curLevelStateType = 3;

                }
                else
                {
                    if ((endTime - startTime).TotalMilliseconds > stimulationInterval_midTime)
                    {
                        previousLevelTrainTime = levelTrainTime;


                        var item = images_object_curLevel[random.Next(images_object_curLevel.Count)];
                        myImage.Source = item.Item1;
                        if (item.Item2 == 1)
                        {
                            myImage.Height = double.NaN;
                            myImage.Width = 260;
                        }
                        else if (item.Item2 == 2)
                        {
                            myImage.Height = 150;
                            myImage.Width = double.NaN;
                        }
                        else if (item.Item2 == 3)
                        {
                            myImage.Height = 220;
                            myImage.Width = double.NaN;
                        }
                        if (objectPositionType == 1)
                        {
                            Canvas.SetLeft(myImage, 570);
                        }
                        else if (item.Item2 == 1 || objectPositionType == 2)
                        {
                            Canvas.SetLeft(myImage, 470 + random.Next(1) * 200);
                        }
                        else if (objectPositionType == 3)
                        {
                            MoveLeft2RightAnimation(null, null);
                        }
                        Canvas.SetBottom(myImage, 240);


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
                    OnGameEnd();
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

        Storyboard storyboard = new Storyboard();
        void MoveLeft2RightAnimation(object sender, EventArgs e)
        {
            // 创建向右移动的动画
            DoubleAnimation rightAnimation = new DoubleAnimation();
            rightAnimation.From = 470; // 起点
            rightAnimation.To = 670; // 终点
            rightAnimation.Duration = new Duration(TimeSpan.FromSeconds(4)); // 移动时间
            rightAnimation.Completed += MoveRight2LeftAnimation; // 动画完成后的事件

            // 设置动画的目标对象和属性
            Storyboard.SetTarget(rightAnimation, myImage);
            Storyboard.SetTargetProperty(rightAnimation, new PropertyPath(Canvas.LeftProperty));

            // 创建Storyboard并添加动画
            storyboard.Children.Clear();
            storyboard.Children.Add(rightAnimation);

            // 开始动画
            storyboard.Begin();
        }
        void MoveRight2LeftAnimation(object sender, EventArgs e)
        {
            // 创建向左移动的动画
            DoubleAnimation leftAnimation = new DoubleAnimation();
            leftAnimation.From = 670; // 起点
            leftAnimation.To = 470; // 终点
            leftAnimation.Duration = new Duration(TimeSpan.FromSeconds(4)); // 移动时间
            leftAnimation.Completed += MoveLeft2RightAnimation; // 动画完成后的事件

            // 设置动画的目标对象和属性
            Storyboard.SetTarget(leftAnimation, myImage);
            Storyboard.SetTargetProperty(leftAnimation, new PropertyPath(Canvas.LeftProperty));

            // 创建Storyboard并添加动画
            storyboard.Children.Clear();
            storyboard.Children.Add(leftAnimation);

            // 开始动画
            storyboard.Begin();
        }

    }
    public partial class 警觉训练 : BaseUserControl, IGameBase
    {


        protected override async Task OnInitAsync()
        {
            ///从前端获取参数
            ///
            var baseParameter = BaseParameter;
            if (baseParameter.ProgramModulePars != null && baseParameter.ProgramModulePars.Any())
            {
                Debug.WriteLine("ProgramModulePars 已加载数据：");
                // 遍历 ProgramModulePars 打印出每个参数
                foreach (var par in baseParameter.ProgramModulePars)
                {
                    //Debug.WriteLine($"ProgramId: {par.ProgramId}, ModuleParId: {par.ModuleParId}, Value: {par.Value}, TableId: {par.TableId}");
                    if (par.ModulePar.ModuleId == baseParameter.ModuleId)
                    {
                        switch (par.ModuleParId) // 完成赋值
                        {
                            case 160: // 等级
                                level = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"level={level}");
                                break;
                            case 180: // 治疗时间
                                totalTrain_time = par.Value.HasValue ? (int)(par.Value.Value) : 10;
                                break;
                            case 181: //等级提高
                                INCREASE = par.Value.HasValue ? (int)par.Value.Value : 3 * 20;
                                break;
                            case 182: //等级降低
                                DECREASE = par.Value.HasValue ? (int)par.Value.Value : 2 * 20;
                                break;
                            case 183: //受刺激的次数
                                stimulationCount = par.Value.HasValue ? (int)par.Value.Value : 20;
                                break;
                            case 184:  //响应时间
                                fixedReactionTime_preTime = par.Value.HasValue ? (int)par.Value.Value : 1;
                                fixedReactionTime_preTime = fixedReactionTime_preTime * 1000;
                                break;
                            case 185: // 前项
                                stimulationInterval_preTime = (int)(par.Value.HasValue ? (int)par.Value.Value : 2.5);
                                stimulationInterval_preTime = stimulationInterval_preTime * 1000;
                                break;
                            case 186: //从警告音到刺激处
                                stimulationInterval_midTime = (int)(par.Value.HasValue ? (int)par.Value.Value : 1.5);
                                stimulationInterval_midTime = stimulationInterval_midTime * 1000;
                                break;
                            case 187: //受刺激之后
                                stimulationInterval_postTime = (int)(par.Value.HasValue ? (int)par.Value.Value : 2.5);
                                stimulationInterval_postTime = stimulationInterval_postTime * 1000;
                                break;
                            case 188: //到有警告音的时间
                                stopWarningVioce_preTime = par.Value.HasValue ? (int)par.Value.Value : 60;
                                break;
                            case 189: //警告音之后的时间
                                stopWarningVioce_postTime = par.Value.HasValue ? (int)par.Value.Value : 60;
                                break;
                            case 190:   //驾驶舱显示
                                showCockpit = par.Value.HasValue ? (bool)(par.Value.Value == 1) : false;
                                break;
                            case 191: //警告音按照等级
                                changeWarningVoice = par.Value.HasValue ? (int)par.Value.Value : 1;
                                break;
                            //case 192: 警告音声音关闭
                            //    fixedHasnotWarningVoice = par.Value.HasValue ? (bool)(par.Value.Value == 1) : false;
                            //    break;


                            default:
                                Debug.WriteLine($"未处理的 ModuleParId: {par.ModuleParId}");
                                break;
                        }
                    }
                }
            }
            else
            {
                Debug.WriteLine("没有数据");
            }



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

            max_hardness = Math.Max(max_hardness, level);
            // 调用委托
            LevelStatisticsAction?.Invoke(level, 16);
            RightStatisticsAction?.Invoke(0, stimulationCount);
            WrongStatisticsAction?.Invoke(0, stimulationCount);
        }



        protected override async Task OnStartAsync()
        {
            pauseState = false;
            StartAllTimer();

            VoiceTipAction?.Invoke("当您看到物体出现时，请迅速按下键盘上的OK键。");
            SynopsisAction?.Invoke("您在屏幕上会看到一个道路上的图像，接下来图像上会出现某些物体，当您看到物体出现时，请迅速按下键盘上的OK键。");
            RuleAction?.Invoke("您在屏幕上会看到一个道路上的图像，接下来图像上会出现某些物体，当您看到物体出现时，请迅速按下键盘上的OK键。");

        }
        protected override async Task OnStopAsync()
        {
            StopAllTimer();
        }
        public void Stop(bool isReport = false)
        {
            StopAllTimer();
        }

        protected override async Task OnPauseAsync()
        {
            pauseState = true;
            StopAllTimer();
        }

        protected override async Task OnNextAsync()
        {

            NextRound();

            VoiceTipAction?.Invoke("当您看到物体出现时，请迅速按下键盘上的OK键。");
            SynopsisAction?.Invoke("您在屏幕上会看到一个道路上的图像，接下来图像上会出现某些物体，当您看到物体出现时，请迅速按下键盘上的OK键。");
            RuleAction?.Invoke("您在屏幕上会看到一个道路上的图像，接下来图像上会出现某些物体，当您看到物体出现时，请迅速按下键盘上的OK键。");
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
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
                        //
                        int correctCount = 0;
                        int mistakeCount = 0;
                        int correctRate = 0;
                        int averageReactionTime = 0;
                        //
                        //int hardness = lv;
                        //int correctCount = allLevelRecord[lv].Sum();
                        //int mistakeCount = allLevelRecord[lv].Count - allLevelRecord[lv].Sum();
                        //int correctRate = (int)((float)100 * correctCount / correctCount + mistakeCount);
                        //int averageReactionTime = (int)(allLevelReactionTime[lv].X / allLevelReactionTime[lv].Y);

                        for (int lv = 1; lv <= max_hardness; lv++)
                        {
                            correctCount += allLevelRecord[lv].Sum();
                            mistakeCount += allLevelRecord[lv].Count - allLevelRecord[lv].Sum();

                        }

                        correctRate = (correctCount + mistakeCount) > 0 ? (int)((float)100 * correctCount / (correctCount + mistakeCount)) : 0;
                        averageReactionTime = countReactionTimes > 0 ? totalReactionTime / countReactionTimes : 0;

                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "警觉训练",
                            Eval = false,
                            Lv = max_hardness,
                            //否则为null，且前⾯的for循环要去掉
                            ScheduleId = BaseParameter.ScheduleId ?? null // 
                        }; db.Results.Add(newResult);
                        await db.SaveChangesAsync();
                        // 获得 result_id
                        int result_id = newResult.ResultId;

                        // 创建 ResultDetail 对象列表
                        //更新不同参数请在底下更新，根据数据结构，只更改resultDetail{}内的
                        var resultDetails = new List<ResultDetail>
                            {
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "等级",
                                    Value = max_hardness ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "正确次数",
                                    Value = correctCount ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "错误次数",
                                    Value = mistakeCount ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "正确率",
                                    Value = correctRate ,
                                    Charttype = "柱状图" ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "平均反应时间",
                                    Value = averageReactionTime ,
                                    Charttype = "柱状图" ,
                                    ModuleId = BaseParameter.ModuleId
                                },


                            };

                        // 插⼊ ResultDetail 数据
                        db.ResultDetails.AddRange(resultDetails);
                        await db.SaveChangesAsync();
                        // 输出每个 ResultDetail 对象的数据
                        Debug.WriteLine($"难度级别 {max_hardness}:");
                        foreach (var detail in resultDetails)
                        {

                            Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId}");

                        }

                        await transaction.CommitAsync();
                        Debug.WriteLine("插⼊成功");
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
        protected override IGameBase OnGetExplanationExample()
        {
            return new 警觉训练讲解();
        }

    }



}

