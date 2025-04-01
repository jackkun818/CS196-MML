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
    /// Alert training explanation.xaml Interaction logic
    /// </summary>
    public partial class Alert training explanation : BaseUserControl
    {
        public Alert training explanation()
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


            this.Loaded += Alert training explanation_Loaded;
        }

        private void Alert training explanation_Loaded(object sender, RoutedEventArgs e)
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
        bool fixedHasnotWarningVoice = true;       //Is there no warning sound
        bool changeWarningVoice = false;  //According to level

        bool showCockpit = false;   //Whether the cockpit is displayed



        /// <summary>
        /// Level-related parameters
        /// </summary>
        int level = 1;
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

        List<BitmapImage> images_object_curLevel = new List<BitmapImage>();

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

            //If fixed or no warning sound
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
            string folderPath = System.IO.Path.Combine(currentDirectory, "Resources/Alert training/");

            Image_1.Source = new BitmapImage(new Uri(folderPath + "x.png", UriKind.Absolute));
            Image_2.Source = new BitmapImage(new Uri(folderPath + "y.png", UriKind.Absolute));

            // Get the current working directory
            currentDirectory = Directory.GetCurrentDirectory();
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
                string cockpitImageFolderPath = System.IO.Path.Combine(currentDirectory, "../../../crs.game/Alert training/Cockpit/");
                BitmapImage cockpitImage = new BitmapImage();
                cockpitImage.BeginInit();
                cockpitImage.UriSource = new Uri(cockpitImageFolderPath + "1.png"); // Image resource path
                cockpitImage.CacheOption = BitmapCacheOption.OnLoad; // Optimized loading performance
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
                        Button_2.Content = "Next step";

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
                        Button_2.Content = "Trial";

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
                        //LJN, instructing the delegate in the explanation module
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("You will see an image on the road on the screen, and then some objects will appear on the image. When you see the object appear, quickly press theOKkey.");//Add code, call function, display the text under the digital person
                        //LJN
                        StartGame();
                    }
                    break;
            }
        }
    }
}
