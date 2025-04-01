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
    /// Alert training.xaml Interaction logic
    /// </summary>
    public partial class Alert training : BaseUserControl
    {
        public Alert training()
        {
            InitializeComponent();

        }

        private Random random = new Random();


        /// <summary>
        /// Parameters obtained by the front end
        /// </summary>
        int totalTrain_time = 10;     //Training time
        int INCREASE = 95; // Increase the threshold for difficulty
        int DECREASE = 80;  // Threshold for reducing difficulty
        int stimulationCount = 20;   //Number of stimulations

        int _stimulationInterval_preTime = 2500;    //The front end of the stimulation interval ms
        int _stimulationInterval_midTime = 2500;  //The interval between stimuli⸺From warning⾳Go to the stimulation（ms）
        int _stimulationInterval_postTime = 2500;     //The interval between stimuli⸺After being stimulated（ms）

        //actual
        int stimulationInterval_preTime = 2500;    //The front end of the stimulation interval ms
        int stimulationInterval_midTime = 2500;  //The interval between stimuli⸺From warning⾳Go to the stimulation（ms）
        int stimulationInterval_postTime = 2500;     //The interval between stimuli⸺After being stimulated（ms）

        int stopWarningVioce_preTime = 10;  //⾃Moving⽌train⸺There is a warning⾳Time(s)
        int stopWarningVioce_postTime = 10;  //⾃Moving⽌train⸺warn⾳The time afterward（s）

        bool useLongReactionTime = true;    //
        bool useShortReactionTime = false;    //
        bool useFixedReactionTime = false;//
        int fixedReactionTime_preTime = 2000;   //ms

        bool fixedHasWarningVoice = false;       //Is there a warning sound
        bool fixedHasnotWarningVoice = false;       //Is there no warning sound
        int changeWarningVoice = 1;  //According to level

        bool showCockpit = false;   //Whether the cockpit is displayed



        /// <summary>
        /// Level-related parameters
        /// </summary>
        int level = 1;
        int max_hardness = 1;
        int backgroundType = 1;   //1 country road, 2 forest roads, 3 rural roads, 4 urban roads
        bool hasWarningVoice = false;

        int longReactionTime = 1200;
        int shortReactionTime = 700;

        int objectPositionType = 1;     //1 center, 2 right-Left, 3 changes
        bool isBackgroundGrey = true; // Black and white and color

        int trainingMaterialCount_1 = 1;    //car
        int trainingMaterialCount_2 = 1;    //animal
        int trainingMaterialCount_3 = 1;    //people

        int roadSignType = 1;   //1 None, 2 Traffic Signs, 3 Traffic Signs and Traffic Lights

        int curLevelReactionTime = 1200;        //Current maximum reaction time

        /// <summary>
        /// Timer
        /// </summary>
        private DispatcherTimer _totalTimerTrain;  //Total treatment time
        private DispatcherTimer _levelTimerTrain;   //

        int totalTrainTime = 1000; //Total treatment time
        int levelTrainTime = 0;
        int previousLevelTrainTime = 0;
        DateTime startTime;
        DateTime endTime;

        int curLevelStateType = 1;   //1. The preceding item appears, 2. The warning sound appears, 3. The warning sound appears, 4. The warning sound appears, 5.

        bool pauseState = true;

        /// <summary>
        /// Record scores
        /// </summary>
        int correctCount = 0;
        int mistakeCount = 0;
        int totalReactionTime = 0;//The total reaction time of this training
        int countReactionTimes = 0;//Total number of reactions in this training
        List<int> record;
        List<List<int>> allLevelRecord; //Record the results for each level

        List<Vector2> allLevelReactionTime;




        /// <summary>
        /// Loading assets
        /// </summary>
        List<BitmapImage> images_object_1 = new List<BitmapImage>();     // List of images used to store
        List<BitmapImage> images_object_2 = new List<BitmapImage>();     // List of images used to store
        List<BitmapImage> images_object_3 = new List<BitmapImage>();     // List of images used to store

        List<(BitmapImage, int)> images_object_curLevel = new List<(BitmapImage, int)>();

        Image myImage;
        void LoadResources()
        {
            images_object_1.Clear(); // Clear the existing picture list
            images_object_2.Clear(); // Clear the existing picture list
            images_object_3.Clear(); // Clear the existing picture list

            string currentDirectory = Directory.GetCurrentDirectory();
            string imageFolderPath = System.IO.Path.Combine(currentDirectory, "Resources/Alert training/material/");


            try
            {
                string folderPath = System.IO.Path.Combine(imageFolderPath, "car/");
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


                folderPath = System.IO.Path.Combine(imageFolderPath, "animal/");
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


                folderPath = System.IO.Path.Combine(imageFolderPath, "people/");
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

            //If fixed or no warning sound
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

            //If using a fixed maximum reaction time
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
            // Calling delegate
            LevelStatisticsAction?.Invoke(level, 16);
            RightStatisticsAction?.Invoke(record.Sum(), stimulationCount);
            WrongStatisticsAction?.Invoke(record.Count - record.Sum(), stimulationCount);
        }

        void InitCanvas()
        {
            // Get the current working directory
            string currentDirectory = Directory.GetCurrentDirectory();
            string imageFolderPath = System.IO.Path.Combine(currentDirectory, "Resources/Alert training/");

            audioPlayer.Source = new Uri(imageFolderPath + "Warning sound/1.mp3", UriKind.RelativeOrAbsolute);

            imageFolderPath = System.IO.Path.Combine(currentDirectory, "Resources/Alert training/background/");

            switch (backgroundType)
            {
                case 1:
                    imageFolderPath += "Country road/";
                    break;
                case 2:
                    imageFolderPath += "Forest road/";
                    break;
                case 3:
                    imageFolderPath += "Rural roads/";
                    break;
                case 4:
                    imageFolderPath += "City roads/";
                    break;
            }

            switch (roadSignType)
            {
                case 1:
                    imageFolderPath += "none/";
                    break;
                case 2:
                    imageFolderPath += "Traffic Signs/";
                    break;
                case 3:
                    imageFolderPath += "Traffic signs and traffic lights/";
                    break;
            }

            //ImageBrush backgroundBrush = new ImageBrush();
            BitmapImage bitmapBG = new BitmapImage();
            bitmapBG.BeginInit();
            bitmapBG.UriSource = new Uri(imageFolderPath + "1.png"); // Image resource path
            bitmapBG.CacheOption = BitmapCacheOption.OnLoad; // Optimized loading performance
            bitmapBG.EndInit();
            //// set up ImageBrush of ImageSource property
            //backgroundBrush.ImageSource = bitmapBG;
            //// set up Canvas of Background property
            //canvas.Background = backgroundBrush;

            WriteableBitmap writeableBitmap = new WriteableBitmap(bitmapBG);

            // Convert to grayscale image
            if (isBackgroundGrey == true)
            {

                TransformToGrayScale(writeableBitmap);
            }
            // Set asCanvasbackground
            canvas.Background = new ImageBrush(writeableBitmap);

            //Whether the cockpit is displayed
            if (showCockpit == true)
            {
                string cockpitImageFolderPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Resources/Alert training/Cockpit/");
                BitmapImage cockpitImage = new BitmapImage();
                cockpitImage.BeginInit();
                cockpitImage.UriSource = new Uri(cockpitImageFolderPath + "/1.png"); // Image resource path
                cockpitImage.CacheOption = BitmapCacheOption.OnLoad; // Optimized loading performance
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

        //Convert image to grayscale image
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
        /// Play warning tones
        /// </summary>
        bool isAudioPlayerPlaying = false;
        void AudioPlayerPlay_Once()
        {
            audioPlayer.Stop(); // Make sure the audio stops
            audioPlayer.Play(); // Play once
        }


        void AudioPlayerPlay_Repeat()
        {
            if (isAudioPlayerPlaying == false)
            {
                isAudioPlayerPlaying = true;
                audioPlayer.Stop(); // Make sure the audio stops
                audioPlayer.LoadedBehavior = MediaState.Manual;
                audioPlayer.UnloadedBehavior = MediaState.Manual;
                audioPlayer.MediaEnded += AudioPlayer_MediaEnded; // Add media end event
                audioPlayer.Play(); // Start playing
            }
        }

        private void AudioPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            audioPlayer.Position = TimeSpan.Zero; // Reset to start
            audioPlayer.Play(); // Continue playing
        }
        void AudioPlayerPlay_Stop()
        {
            isAudioPlayerPlaying = false;
            audioPlayer.Stop(); // Stop playing
            audioPlayer.MediaEnded -= AudioPlayer_MediaEnded; // Remove media end event
        }

        /// <summary>
        /// Keyboard events
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
                // Calling delegate
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
            // Create an animation that moves to the right
            DoubleAnimation rightAnimation = new DoubleAnimation();
            rightAnimation.From = 470; // starting point
            rightAnimation.To = 670; // end
            rightAnimation.Duration = new Duration(TimeSpan.FromSeconds(4)); // Move time
            rightAnimation.Completed += MoveRight2LeftAnimation; // Events after animation is completed

            // Animate the target object and properties
            Storyboard.SetTarget(rightAnimation, myImage);
            Storyboard.SetTargetProperty(rightAnimation, new PropertyPath(Canvas.LeftProperty));

            // createStoryboardAnd add animation
            storyboard.Children.Clear();
            storyboard.Children.Add(rightAnimation);

            // Start animation
            storyboard.Begin();
        }
        void MoveRight2LeftAnimation(object sender, EventArgs e)
        {
            // Create an animation that moves to the left
            DoubleAnimation leftAnimation = new DoubleAnimation();
            leftAnimation.From = 670; // starting point
            leftAnimation.To = 470; // end
            leftAnimation.Duration = new Duration(TimeSpan.FromSeconds(4)); // Move time
            leftAnimation.Completed += MoveLeft2RightAnimation; // Events after animation is completed

            // Animate the target object and properties
            Storyboard.SetTarget(leftAnimation, myImage);
            Storyboard.SetTargetProperty(leftAnimation, new PropertyPath(Canvas.LeftProperty));

            // createStoryboardAnd add animation
            storyboard.Children.Clear();
            storyboard.Children.Add(leftAnimation);

            // Start animation
            storyboard.Begin();
        }

    }
    public partial class Alert training : BaseUserControl, IGameBase
    {


        protected override async Task OnInitAsync()
        {
            ///Get parameters from the front end
            ///
            var baseParameter = BaseParameter;
            if (baseParameter.ProgramModulePars != null && baseParameter.ProgramModulePars.Any())
            {
                Debug.WriteLine("ProgramModulePars Loaded data:");
                // Traversal ProgramModulePars Print out each parameter
                foreach (var par in baseParameter.ProgramModulePars)
                {
                    //Debug.WriteLine($"ProgramId: {par.ProgramId}, ModuleParId: {par.ModuleParId}, Value: {par.Value}, TableId: {par.TableId}");
                    if (par.ModulePar.ModuleId == baseParameter.ModuleId)
                    {
                        switch (par.ModuleParId) // Complete assignment
                        {
                            case 160: // grade
                                level = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"level={level}");
                                break;
                            case 180: // Treatment time
                                totalTrain_time = par.Value.HasValue ? (int)(par.Value.Value) : 10;
                                break;
                            case 181: //Level improvement
                                INCREASE = par.Value.HasValue ? (int)par.Value.Value : 3 * 20;
                                break;
                            case 182: //Level down
                                DECREASE = par.Value.HasValue ? (int)par.Value.Value : 2 * 20;
                                break;
                            case 183: //Number of stimulations
                                stimulationCount = par.Value.HasValue ? (int)par.Value.Value : 20;
                                break;
                            case 184:  //Response time
                                fixedReactionTime_preTime = par.Value.HasValue ? (int)par.Value.Value : 1;
                                fixedReactionTime_preTime = fixedReactionTime_preTime * 1000;
                                break;
                            case 185: // Previous item
                                stimulationInterval_preTime = (int)(par.Value.HasValue ? (int)par.Value.Value : 2.5);
                                stimulationInterval_preTime = stimulationInterval_preTime * 1000;
                                break;
                            case 186: //From warning to stimulation
                                stimulationInterval_midTime = (int)(par.Value.HasValue ? (int)par.Value.Value : 1.5);
                                stimulationInterval_midTime = stimulationInterval_midTime * 1000;
                                break;
                            case 187: //After being stimulated
                                stimulationInterval_postTime = (int)(par.Value.HasValue ? (int)par.Value.Value : 2.5);
                                stimulationInterval_postTime = stimulationInterval_postTime * 1000;
                                break;
                            case 188: //Time to have warning sound
                                stopWarningVioce_preTime = par.Value.HasValue ? (int)par.Value.Value : 60;
                                break;
                            case 189: //Time after the warning sound
                                stopWarningVioce_postTime = par.Value.HasValue ? (int)par.Value.Value : 60;
                                break;
                            case 190:   //Cockpit display
                                showCockpit = par.Value.HasValue ? (bool)(par.Value.Value == 1) : false;
                                break;
                            case 191: //Warning sound according to level
                                changeWarningVoice = par.Value.HasValue ? (int)par.Value.Value : 1;
                                break;
                            //case 192: Warning sound is off
                            //    fixedHasnotWarningVoice = par.Value.HasValue ? (bool)(par.Value.Value == 1) : false;
                            //    break;


                            default:
                                Debug.WriteLine($"Unprocessed ModuleParId: {par.ModuleParId}");
                                break;
                        }
                    }
                }
            }
            else
            {
                Debug.WriteLine("No data");
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
            // Calling delegate
            LevelStatisticsAction?.Invoke(level, 16);
            RightStatisticsAction?.Invoke(0, stimulationCount);
            WrongStatisticsAction?.Invoke(0, stimulationCount);
        }



        protected override async Task OnStartAsync()
        {
            pauseState = false;
            StartAllTimer();

            VoiceTipAction?.Invoke("When you see an object appear, quickly press theOKkey.");
            SynopsisAction?.Invoke("You will see an image on the road on the screen, and then some objects will appear on the image. When you see the object appear, quickly press theOKkey.");
            RuleAction?.Invoke("You will see an image on the road on the screen, and then some objects will appear on the image. When you see the object appear, quickly press theOKkey.");

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

            VoiceTipAction?.Invoke("When you see an object appear, quickly press theOKkey.");
            SynopsisAction?.Invoke("You will see an image on the road on the screen, and then some objects will appear on the image. When you see the object appear, quickly press theOKkey.");
            RuleAction?.Invoke("You will see an image on the road on the screen, and then some objects will appear on the image. When you see the object appear, quickly press theOKkey.");
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
                            Report = "Alert training",
                            Eval = false,
                            Lv = max_hardness,
                            //Otherwise it isnull, and before⾯offorThe loop needs to be removed
                            ScheduleId = BaseParameter.ScheduleId ?? null // 
                        }; db.Results.Add(newResult);
                        await db.SaveChangesAsync();
                        // get result_id
                        int result_id = newResult.ResultId;

                        // create ResultDetail Object List
                        //Please update different parameters below, and only change according to the data structure.resultDetail{}Internal
                        var resultDetails = new List<ResultDetail>
                            {
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "grade",
                                    Value = max_hardness ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Correct times",
                                    Value = correctCount ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Errors",
                                    Value = mistakeCount ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Correct rate",
                                    Value = correctRate ,
                                    Charttype = "Bar chart" ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Average reaction time",
                                    Value = averageReactionTime ,
                                    Charttype = "Bar chart" ,
                                    ModuleId = BaseParameter.ModuleId
                                },


                            };

                        // Insert⼊ ResultDetail data
                        db.ResultDetails.AddRange(resultDetails);
                        await db.SaveChangesAsync();
                        // Output each ResultDetail Object data
                        Debug.WriteLine($"Difficulty level {max_hardness}:");
                        foreach (var detail in resultDetails)
                        {

                            Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId}");

                        }

                        await transaction.CommitAsync();
                        Debug.WriteLine("Insert⼊success");
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
        protected override IGameBase OnGetExplanationExample()
        {
            return new Alert training explanation();
        }

    }



}

