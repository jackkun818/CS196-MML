using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Threading;
using crs.core;
using System.Diagnostics;
using crs.core.DbModels;
using System.Windows.Media.Animation;
using System.Windows.Forms;
using System.Media;
using static log4net.Appender.RollingFileAppender;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace crs.game.Games
{
    /// <summary>
    /// BILD.xaml Interaction logic
    /// </summary>
    public partial class Graphic_memory : BaseUserControl
    {
        private readonly string[][] imagePaths = new string[][]
        {
            new string[]
{
    "BILD/1/watch.jpg",
    "BILD/1/banana.jpg",
    "BILD/1/watermelon.jpg",
    "BILD/1/nectarine.jpg",
    "BILD/1/Grape.jpg",
    "BILD/1/fish.jpg",
    "BILD/1/rabbit.jpg",
    "BILD/1/shoe.jpg",
    "BILD/1/panda.jpg",
    "BILD/1/turtle.jpg",
    "BILD/1/sheep.jpg",
    "BILD/1/pig.jpg",
},
            new string[]
            {
                "BILD/2/Telephone.jpg",
                "BILD/2/Pineapple.jpg",
                "BILD/2/TV set.jpg",
                "BILD/2/tomato.jpg",
                "BILD/2/Hairdryer.jpg",
                "BILD/2/hammer.jpg",
                "BILD/2/elephant.jpg",
                "BILD/2/computer.jpg"
            },
            new string[]
            {
                "BILD/3/Butterfly.jpg",
                "BILD/3/tangerine.jpg",
                "BILD/3/dog.jpg",
                "BILD/3/Screws.jpg",
                "BILD/3/apple.jpg",
                "BILD/3/pear.jpg",
                "BILD/3/Alarm clock.jpg",
                "BILD/3/Scissors.jpg",
                "BILD/3/kiwi.jpg",
                "BILD/3/cat.jpg"
            }

        };

        // Used to save the selected image path
        private List<string> selectedImagePaths = new List<string>();
        private int LEVEL_DURATION = 1;
        private int total_picture_number_para = 7; // This parameter is multiplied by the correct number of pictures to the total number of pictures
        private int right_picture_number = 3; // The correct number of pictures displayed
        private int train_mode = 1;//Training mode, 1 picture selection, 2 picture selection, 3 name selection
        private int LEVEL_UP_THRESHOLD = 85; // Improve the accuracy threshold for difficulty（percentage）
        private int LEVEL_DOWN_THRESHOLD = 70; // Reduce the accuracy threshold for difficulty（percentage）
        private int max_time = 30;
        private bool IS_REALISTIC = true; // Whether the picture is displayed as a real object（The real picture is displayed by default）
        private int[] correctAnswers = new int[10];
        private int[] wrongAnswers = new int[10];
        private int[] ignoreAnswers = new int[10];
        private const int MaxGames = 10;
        private int hardness = 1;
        int max_hardness = 0;
        private const int MAX_HARDNESS = 9; // Maximum difficulty level
        private const int MIN_HARDNESS = 1;//Minimum level
        private DispatcherTimer sharedTimer;//This is the timer for the total treatment time, such as 30 minutes
        private Queue<bool> recentResults = new Queue<bool>(5); // Queue that records the last 5 selection results
        private int imageGenerationCounter = 0;//Used to temporarily count in a timer
        private double imageGenerationInterval = 5.0; // Control how many times the image is generated every time, the unit iss, it needs to be multiplied by 1,000
        private DispatcherTimer colorResetTimer;
        //--------------window2--------------------

        private List<string> rightImagePaths; // Correct image path
        private List<string> totalImagePaths; // The total question bank picture path
        private int totalPictureMultiplier = 7; // Parameters: This parameter is multiplied by the correct number of pictures to the total number of pictures

        private bool IS_FIXED_INTERVAL = false; // Is the item interval fixed?（Not fixed by default）
        private double SPEED_FACTOR = 1.0; // The speed factor of the conveyor belt（default value）
        private bool IS_VISUAL_FEEDBACK = true; // Is there any visual feedback
        private bool IS_AUDITORY_FEEDBACK = true; // Is there any acoustic feedback
        private int TYPE_OF_INPUT = 0; // Which input method to choose
        private int CHOOSE_IMAGE_COUNT = 10; // imageBorderNumber of pictures displayed in

        private bool IS_IMAGE_DETAIL = true; // Related to the difficulty, decide whether the image type is selected or not
        private bool IS_IMAGE_HARD = true; // Related to difficulty, it determines whether the image type is difficult or simple
        private double DISPLAY_TIME = 6; // Total display time of picture glide
        private double REAL_DISPLAY_TIME = 6;//The actual total time
        double RATE_OF_ERRORIMAGE = 0.5; // Display error（That is, nonimage2,3,4）Probability
        double Correct_decision_rate = 0;
        private int totalDecisions;
        private int correctDecisions;
        private int errorDecisions;
        private int missDecisions;
        private const int WAIT_DELAY = 1;
        private readonly Brush defaultSelectionBoxStrokeColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));
        private int gameIndex = 0;

        //private int remainingTime = 30;
        private int GameremainingTime = 10;
        //private DispatcherTimer trainingTimer; // Program duration timer
        private DispatcherTimer gameTimer; // Timer provided by the material
        private DateTime LastTickTime = new DateTime();//RecordGameTimer_TickWhen was the last trigger
        private DateTime StopTime = new DateTime();//Record when it triggersStopDurations

        private Random random = new Random();
        private int continueButtonPressCount = 0;// Number of times the button is pressed
        private bool isGameRunning = false; // Is the logo game going on
        public event Action<int> GameremainingTimeUpdated;
        public event Action<int, int[], int[]> GameStatsUpdated;


        public class ImageInfo
        {
            public string ImagePath { get; set; }
            public bool AnimationCanceled { get; set; } = false;
        }

        public Graphic_memory(int hardness_, int training_mode, int[] correctAnswers1, int[] wrongAnswers1, int[] ignoreAnswers1)
        {//It seems that this constructor will not be called
            InitializeComponent();
            correctAnswers = correctAnswers1;
            wrongAnswers = wrongAnswers1;
            ignoreAnswers = ignoreAnswers1;
            hardness = hardness_;
            hard_set();
            sharedTimer = new DispatcherTimer();
            sharedTimer.Interval = TimeSpan.FromSeconds(1);
            sharedTimer.Tick += OnTick;
            // Randomly select the specified number of non-repetitive pictures
            train_mode = training_mode;
            List<string> allImages = imagePaths.SelectMany(x => x).ToList();
            Random random = new Random();
            selectedImagePaths = allImages.OrderBy(x => random.Next()).Take(right_picture_number).ToList();

            // according to training_mode Set the visibility of pictures or text
            if (training_mode == 1 || training_mode == 2)
            {
                // Show pictures, hide text
                SetImagesVisible();
            }
            else if (training_mode == 3)
            {
                // Show text, hide pictures
                SetTextsVisible();
            }
        }
        public Graphic_memory()
        {
            InitializeComponent();
        }

        private void ChangeSelectionBoxColor(Brush newColor)
        {
            // Change SelectionBox of Stroke color
            SelectionBox.Stroke = newColor;

            // Create a DispatcherTimer, set to 2 seconds before triggering
            if (colorResetTimer != null && colorResetTimer.IsEnabled)
            {
                colorResetTimer.Stop();
            }
            else
            {
                colorResetTimer = new DispatcherTimer();
                colorResetTimer.Interval = TimeSpan.FromSeconds(1);
                colorResetTimer.Tick += ColorResetTimer_Tick;
            }

            colorResetTimer.Start();
        }

        private void ColorResetTimer_Tick(object sender, EventArgs e)//This is the timer for the color change of the frame
        {
            // recover SelectionBox of Stroke The color is the default color
            SelectionBox.Stroke = defaultSelectionBoxStrokeColor;

            // Stop the timer
            colorResetTimer.Stop();
        }

        private void OnTick(object sender, EventArgs e)//The global timer is triggered once every second, such as 30 minutes, updates the remaining time and statistics of the game, and ends the game when the time runs out.
        {
            if (max_time > 0)
            {
                //TimeStatisticsAction.Invoke(10, 10);
                max_time--;
                //TimeStatisticsAction.Invoke(max_time, GameremainingTime);
                if (isGameRunning)
                {
                    TimeStatisticsAction.Invoke(max_time, 0);
                }
                else
                {
                    TimeStatisticsAction.Invoke(max_time, MemorizeLimitTime);
                }
                LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
                //int correctCount = 0;
                //int incorrectCount = 0;
                //foreach (bool result in recentResults)
                //{
                //    if (result)
                //    {
                //        correctCount++;
                //    }
                //    else
                //    {
                //        incorrectCount++;
                //    }
                //}

            }
            else
            {
                sharedTimer.Stop();
                OnGameEnd();
            }
        }

        public static System.Windows.Window GetTopLevelWindow(System.Windows.Controls.UserControl userControl)
        {
            DependencyObject current = userControl;
            while (current != null && !(current is System.Windows.Window))
            {
                current = VisualTreeHelper.GetParent(current);
            }

            return current as System.Windows.Window;
        }

        private void SetImagesVisible()
        {
            //// Clear previous pictures
            //imageContainer.Children.Clear();

            //// Dynamically add pictures to UniformGrid
            //foreach (var imagePath in selectedImagePaths)
            //{
            //    Image imageControl = new Image
            //    {
            //        Source = new BitmapImage(new Uri(imagePath, UriKind.Relative)),
            //        Stretch = Stretch.Uniform,
            //        Margin = new Thickness(5),
            //        Height = ImageSizeDict[hardness]//Here the height can be adjusted dynamically according to the number of pictures,Use it directly herehardnessInstead of the quantity is because in the level difficulty list,hardnessand quantity are equal in the rank table
            //    };

            //    imageContainer.Children.Add(imageControl);
            //}
            //imageContainer.Columns = ImageContainerColumn[selectedImagePaths.Count];
            SetImagesPosition();
        }

        private void SetTextsVisible()
        {
            //// Clear previous text
            //imageContainer.Children.Clear();

            //// Dynamically add text to UniformGrid
            //foreach (var imagePath in selectedImagePaths)
            //{
            //    TextBlock textBlock = new TextBlock
            //    {
            //        Text = Path.GetFileNameWithoutExtension(imagePath),
            //        FontSize = 120,
            //        Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black),
            //        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            //        VerticalAlignment = VerticalAlignment.Center,
            //        TextWrapping = TextWrapping.Wrap
            //    };

            //    imageContainer.Children.Add(textBlock);
            //}
            SetTextsPosition();

        }

        // ContinueButton_Click Event handler
        private void ContinueButton_Click(object sender, RoutedEventArgs e)//Click function to start the game
        {
            if (IfMemorizeLimit) { EndMemorizeLimit(); }//Each time you press this button, you must end the timing of the memory stage.

            if (selectedImagePaths.Count > 0)
            {
                // Create and open a new window while passing the selected image path list
                //BILD_Answerwindow window1 = new BILD_Answerwindow(selectedImagePaths, train_mode, hardness);
                //window1.GameStatsUpdated += OnGameStatsUpdated;
                //window1.GameremainingTimeUpdated += OnGameremainingTimeUpdated;
                //window1.Show();
                Grid1.Visibility = Visibility.Collapsed;
                Grid2.Visibility = Visibility.Visible;

                // Start the program timer
                // Initialize the remaining time（In seconds）
                //remainingTime = remainingTime * 60;
                hard_set();
                //train_mode = 1;

                //GameremainingTime = 30;
                //GameremainingTime = 30;
                rightImagePaths = selectedImagePaths;

                GenerateTotalImagePaths();
                // Start the program timer
                GameremainingTime = LEVEL_DURATION * 60;

                //This timer is useless, just comment it out
                //trainingTimer = new DispatcherTimer();
                //trainingTimer.Interval = TimeSpan.FromSeconds(1); // Updated once every second
                //trainingTimer.Tick += TrainingTimer_Tick;
                //trainingTimer.Start();
                StartGame();
            }
            else
            {
                //MessageBox.Show("No image path was selected.");
            }
        }

        //private void OnGameremainingTimeUpdated(int remainingTime)//Useless functions
        //{// Example of calling an operation
        //    TimeStatisticsAction.Invoke(GameremainingTime, max_time);
        //}

        private void StartGame()
        {
            if (!isGameRunning)
            {
                isGameRunning = true;
                // Start the game timer
                // Check if it already exists displayTimer Example
                if (gameTimer != null)
                {
                    // If the timer is running, stop it first
                    if (gameTimer.IsEnabled)
                    {
                        gameTimer.Stop();
                    }

                    // Cancel previous registration events（Prevent duplicate registration events）
                    gameTimer.Tick -= GameTimer_Tick;

                    // Will displayTimer Set as null, indicating that it has been cleaned
                    gameTimer = null;
                }
                //Set the animation speed every time you start the game
                REAL_DISPLAY_TIME = DISPLAY_TIME / SPEED_FACTOR; // Synthesize real time,becauseSPEED_FACTORIt is a multiple, the larger the speed, the faster the time is, so the shorter the time
                imageGenerationInterval = (double)(REAL_DISPLAY_TIME / 3); //The interval setting, the multiple divided by this can be roughly determined as follows: how many times are divided is equivalent to how many picture materials will appear on a conveyor belt

                gameTimer = new DispatcherTimer();
                gameTimer.Interval = TimeSpan.FromMilliseconds(imageGenerationInterval * 1000);//*1000 is because millisecond accuracy is required here
                gameTimer.Tick += GameTimer_Tick;
                gameTimer.Start();
            }

        }

        private void NotifyGameStatsUpdated()//This should be a useless function too
        {
            GameStatsUpdated?.Invoke(hardness, correctAnswers, wrongAnswers);
        }

        //protected override void OnClosed(EventArgs e)
        //{
        //    base.OnClosed(e);
        //    TimerManager.TimerElapsed -= OnTimerElapsed;
        //}

        //private void TrainingTimer_Tick(object sender, EventArgs e)//The training timer is triggered once every second, updating the remaining time of training and related statistics.
        //{//Useless function,GameremainingTimeUpdatedfornull
        //    if (true)
        //    {
        //        //remainingTime--;
        //        GameremainingTimeUpdated?.Invoke(GameremainingTime);
        //    }
        //    else
        //    {
        //        //trainingTimer.Stop();
        //        //gameTimer?.Stop();
        //        //isGameRunning = false;
        //        //AUFM_Report reportWindow = new AUFM_Report(LEVEL_UP_THRESHOLD, LEVEL_DOWN_THRESHOLD, max_time, LEVEL_DURATION, true, IS_REALISTIC, correctAnswers, wrongAnswers, ignoreAnswers);
        //    }
        //}

        private void GameTimer_Tick(object sender, EventArgs e)//The game timer is triggered every set time, updates the remaining time of the game, generates a new picture, and detects the game end condition.
        {
            LastTickTime = DateTime.Now;
            //if (GameremainingTime > 0)
            if (true)
            {
                GameremainingTime--;
                try
                {
                    //TimeStatisticsAction.Invoke(max_time, GameremainingTime);
                }
                catch (Exception ex)
                {
                    // Exception logs or other processing methods can be recorded
                    // Console.WriteLine($"Exception occurred: {ex.Message}");
                }
                //imageGenerationCounter++;
                //if (imageGenerationCounter >= imageGenerationInterval)
                //{
                //    imageGenerationCounter = 0;  // Reset the counter
                //}
                if (ImageNumTemp > 0)
                {//It means that there are not enough material pictures yet
                    ShowRandomImage1();
                }
            }
            else
            {//No need to be based nowGameremainingTimeTime limit to force the game to end
                EndGame();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (IfPerformingAction == false)
            {
                IfPerformingAction = true;
                PerformAction();
                IfPerformingAction = false;
            }

        }

        private void PerformAction()
        {
            try
            {
                // Get SelectionBox Position and size
                GeneralTransform selectionTransform = SelectionBox.TransformToVisual(ImageGrid);
                Rect selectionRect = selectionTransform.TransformBounds(new Rect(0, 0, SelectionBox.ActualWidth, SelectionBox.ActualHeight));

                // Tag whether overlapping images are found
                bool isOverlapFound = false;
                System.Windows.Controls.Image overlappedImage = null;

                // Traversal imageContainer2 In each picture, check whether it is with SelectionBox overlapping
                foreach (UIElement element in imageContainer2.Children)
                {
                    if (element is System.Windows.Controls.Image image)
                    {
                        if (ImageDetectDict[image] == false)//Only images that have not been detected need to enter this logic
                        {
                            GeneralTransform imageTransform = image.TransformToVisual(ImageGrid);
                            Rect imageRect = imageTransform.TransformBounds(new Rect(0, 0, image.ActualWidth, image.ActualHeight));

                            // Check if it overlaps
                            if (selectionRect.IntersectsWith(imageRect))
                            {
                                isOverlapFound = true;
                                overlappedImage = image;
                                break;
                            }
                        }

                    }
                }

                // Process according to overlapping results
                if (isOverlapFound && overlappedImage != null)
                {
                    // Get overlapping images ImageTagInfo Object
                    var info = overlappedImage.Tag as ImageTagInfo;
                    if (info != null)
                    {
                        //// Clear the animation without triggering Completed event
                        //overlappedImage.RenderTransform.BeginAnimation(TranslateTransform.XProperty, null);
                        //// set up AnimationStopped Logo
                        //info.AnimationStopped = true;

                        //Move part of the logic code toCompletedIn the event, it needs to be triggeredCompletedevent

                        // Get the path to overlapping images
                        string imagePath = info.ImagePath;
                        string imageName = System.IO.Path.GetFileNameWithoutExtension(imagePath); // Get the name of the image（Extended extension）

                        // Check whether the image path is in the correct question bank
                        bool isCorrect = rightImagePaths.Any(path => imagePath.EndsWith(path, StringComparison.OrdinalIgnoreCase));

                        // Updated based on inspection results correctDecisions and SelectionBox The stroke color and update Border and TextBlock
                        if (isCorrect)
                        {
                            correctDecisions++;
                            //ChangeSelectionBoxColor(new SolidColorBrush(Colors.Green)); // Change to green and restore after 2 seconds
                            SelectionBox.Stroke = new SolidColorBrush(Colors.Green);//Change the color here and wait until the animation is paused before restoring
                            correctAnswers[hardness]++;
                            textBlock.Background = new SolidColorBrush(Colors.Green);
                            textBlock1.Text = imageName + " correct！";
                            ImageCorrectTemp++;//This question is correct.
                            ImageDetectDict[overlappedImage] = true;//Record the detection status
                            PauseThenStart();
                            if (IS_AUDITORY_FEEDBACK) PlayWav(CorrectSoundPath);
                            if (IS_VISUAL_FEEDBACK) ShowFeedbackImage(CorrectImage);
                            if (recentResults.Count >= 5)
                            {
                                recentResults.Dequeue(); // Remove the earliest results
                            }
                            recentResults.Enqueue(true); // Add the current result
                        }
                        else
                        {
                            errorDecisions++;
                            //ChangeSelectionBoxColor(new SolidColorBrush(Colors.Red)); // Change to red and restore after 2 seconds
                            SelectionBox.Stroke = new SolidColorBrush(Colors.Red);//Change the color here and wait until the animation is paused before restoring
                            wrongAnswers[hardness]++;
                            textBlock.Background = new SolidColorBrush(Colors.Red);
                            textBlock1.Text = imageName + " mistake！";
                            ImageErrorTemp++;//This question is wrong answered
                            ImageDetectDict[overlappedImage] = true;//Record the detection status
                            PauseThenStart();
                            if (IS_AUDITORY_FEEDBACK) PlayWav(ErrorSoundPath);
                            if (IS_VISUAL_FEEDBACK) ShowFeedbackImage(ErrorImage);
                            if (recentResults.Count >= 5)
                            {
                                recentResults.Dequeue(); // Remove the earliest results
                            }
                            recentResults.Enqueue(false); // Add the current result;
                        }
                        NotifyGameStatsUpdated();
                        // from imageContainer2 Remove the picture in
                        //imageContainer2.Children.Remove(overlappedImage);
                        //Want to finish the animation of this material, so I'm not hereremove
                    }
                    else
                    {
                        // deal with info for null The situation
                        //Console.WriteLine("overlappedImage.Tag is not of type ImageTagInfo.");
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error in PerformAction: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public class ImageTagInfo
        {
            public string ImagePath { get; set; }
            public bool AnimationStopped { get; set; } = false;
        }

        private void ShowRandomImage1()
        {
            try
            {
                // examine totalImagePaths Is it null Or empty
                if (totalImagePaths == null || totalImagePaths.Count == 0)
                {
                    throw new Exception("totalImagePaths is null or empty. Please ensure it is initialized properly.");
                }

                // Randomly select a picture from the general question bank. The specific method is: disrupt it in advance and select it in order.
                string imagePath = totalImagePaths[ImageNumTemp - 1];

                // examine imageContainer2 Is it null
                if (imageContainer2 == null)
                {
                    throw new Exception("imageContainer2 is null. Please ensure it is initialized properly.");
                }

                System.Windows.Controls.Image newImage;
                if (train_mode == 2)
                {
                    // Extract filename and remove extension
                    string imageName = System.IO.Path.GetFileNameWithoutExtension(imagePath);

                    // Create a text image containing the file name
                    RenderTargetBitmap renderBitmap = CreateTextImage(imageName);

                    newImage = new System.Windows.Controls.Image
                    {
                        Source = renderBitmap,
                        Width = 325,
                        Height = renderBitmap.Height,
                        //Margin = new Thickness(5),
                        // Unified use ImageTagInfo
                        Tag = new ImageTagInfo { ImagePath = imagePath }
                    };
                }
                else
                {
                    // Display pictures normally
                    BitmapImage bitmap = new BitmapImage(new Uri(imagePath, UriKind.Relative));
                    if (IfPictureInstruction && train_mode == 3)
                    {//If you need to display the image description, create astackpanel
                        // create StackPanel Package Image and instructions TextBlock
                        StackPanel imageWithDescription = new StackPanel
                        {
                            Orientation = System.Windows.Controls.Orientation.Vertical, // Vertical layout
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        // create TextBlock As an explanation
                        TextBlock description = new TextBlock
                        {
                            Text = Path.GetFileNameWithoutExtension(imagePath), // Get the file name and remove the suffix
                            FontSize = InstructionFontSize, // Set the font size
                            Foreground = new SolidColorBrush(Colors.Black),
                            TextAlignment = TextAlignment.Center, // Center text
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                            Margin = new Thickness(0, 2, 0, 0) // Set the spacing with pictures
                        };
                        Image ImageWithoutDescription = new System.Windows.Controls.Image
                        {
                            Source = bitmap,
                            Width = 325,
                            Height = imageContainer2.ActualHeight - InstructionFontSize - 2,//-2. There is still that onemargin
                            Margin = new Thickness(5),
                            // Unified use ImageTagInfo
                            Tag = new ImageTagInfo { ImagePath = imagePath }
                        };
                        // Will Image and TextBlock Add to StackPanel
                        imageWithDescription.Children.Add(description);
                        imageWithDescription.Children.Add(ImageWithoutDescription);
                        // Convert to single Image
                        newImage = ConvertStackPanelToImage(imageWithDescription, imagePath);
                    }
                    else
                    {
                        newImage = new System.Windows.Controls.Image
                        {
                            Source = bitmap,
                            Width = 325,
                            Height = imageContainer2.ActualHeight * 0.8,
                            Margin = new Thickness(5),
                            // Unified use ImageTagInfo
                            Tag = new ImageTagInfo { ImagePath = imagePath }
                        };
                    }

                }

                // Make sure the image is displayed on the top layer
                System.Windows.Controls.Panel.SetZIndex(newImage, int.MaxValue); // Will ZIndex Set to maximum value

                // Add new image to imageContainer2
                imageContainer2.Children.Add(newImage);

                //Record detection status to prevent repeated detection
                ImageDetectDict[newImage] = false;//Record the detection status
                ImageIgnoreDict[newImage] = false;
                //Make sure the displayed material is in the middle of the conveyor belt
                double verticalCenterPosition = (imageContainer2.ActualHeight - newImage.Height) / 2;
                Canvas.SetTop(newImage, verticalCenterPosition); // Set vertical centering position

                // Animation mobile pictures
                AnimateImage(newImage);
            }

            catch (Exception ex)
            {
                //MessageBox.Show($"Error in ShowRandomImage1: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private RenderTargetBitmap CreateTextImage(string text)
        {
            // Create text blocks to display text
            TextBlock textBlock = new TextBlock
            {
                Text = text, // Use filename instead of full path
                FontSize = 128, // Increase font size
                Foreground = new SolidColorBrush(Colors.Black),
                Background = new SolidColorBrush(Colors.Transparent),
                //Width = 375, // Increase the width of the text block
                //Height = 200, // Increase the height of the text block
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };
            // Measure the actual width and height of a text block
            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            textBlock.Arrange(new Rect(new Size(textBlock.DesiredSize.Width, textBlock.DesiredSize.Height)));
            // Create with actual size RenderTargetBitmap
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int)textBlock.DesiredSize.Width,
                (int)textBlock.DesiredSize.Height,
                96, 96, PixelFormats.Pbgra32);
            renderBitmap.Render(textBlock);
            ////Render text blocks as bitmaps, increasing width and height
            //RenderTargetBitmap renderBitmap = new RenderTargetBitmap(375, 200, 96, 96, PixelFormats.Pbgra32);
            //textBlock.Measure(new Size(375, 200));
            //textBlock.Arrange(new Rect(new Size(375, 200)));
            //renderBitmap.Render(textBlock);

            return renderBitmap;
        }

        private void AnimateImage(System.Windows.Controls.Image img)
        {
            if (ImageNumTemp <= 0)//Explain that the material to be displayed in this group of questions has been displayed
            {
                return;//Is this question correct?
            }
            else
            {
                try
                {
                    double fromValue = -img.Width;
                    double toValue = imageContainer2.ActualWidth; //Move from the left side of the conveyor belt to the right side

                    //TranslateTransform translateTransform = new TranslateTransform();
                    //img.RenderTransform = translateTransform;
                    DoubleAnimation animation = new DoubleAnimation
                    {
                        From = fromValue,
                        To = toValue,
                        Duration = new Duration(TimeSpan.FromSeconds(REAL_DISPLAY_TIME))
                    };

                    Storyboard storyboard = new Storyboard();
                    storyboard.Children.Add(animation);
                    Storyboard.SetTarget(animation, img);
                    Storyboard.SetTargetProperty(animation, new PropertyPath(Canvas.LeftProperty));

                    storyboard.Completed += (s, e) =>
                    {
                        try
                        {
                            // Try to img.Tag Convert to ImageTagInfo
                            var tagInfo = img.Tag as ImageTagInfo;
                            if (tagInfo != null)
                            {
                                if (tagInfo.AnimationStopped)
                                {
                                    // The animation has been stopped manually, and subsequent processing has been skipped
                                    return;
                                }

                                // Hide the picture after the animation is completed
                                img.Source = null;
                                // from imageContainer2 Remove the picture in
                                imageContainer2.Children.Remove(img);
                                missDecisions++;

                                // Get the path to the image
                                string imagePath = tagInfo.ImagePath;

                                // Check whether the image path is in the correct question bank
                                bool isCorrect = rightImagePaths.Any(path => imagePath.EndsWith(path, StringComparison.OrdinalIgnoreCase));

                                // If it is the correct image that is not selected and not detected, add ignoreAnswers[hardness] Value of
                                if (isCorrect && ImageDetectDict[img] == false)
                                {
                                    ignoreAnswers[hardness]++;
                                    // System.Windows.MessageBox.Show("Answers Information");
                                    ImageIgnoreTemp++;//Update the counts raised in this group

                                }
                                ImageAnimationEnd--;//Each end of an animation means that a material has been gone for a lifetime
                                if (ImageAnimationEnd <= 0)//AlthoughImageNumTemp<=It's 0, but this logic will still enter after the animation before it ends, so it needs to be judged.
                                {//Being able to enter this is already the last animation of this group of questions. It's time to see if this group of questions is correct.
                                    //tagInfo.AnimationStopped = true;
                                    GroupCheck();//How did this question work?
                                    AdjustGroupDifficulty();
                                    BeginNextGroup();
                                }

                                //This animation is over, so remove the animation from the queue
                                if (ImageIgnoreDict[img] == true)
                                {
                                    SelectionBox.Stroke = new SolidColorBrush(Colors.Black);
                                }


                                StoryBoardsList.Remove(storyboard);
                                // Unsubscribe at the end of the animation
                                CompositionTarget.Rendering -= null; // Remove event listening
                            }
                            else
                            {
                                // deal with tagInfo for null The situation
                                // You can log or perform other operations
                                //Console.WriteLine("img.Tag is not of type ImageTagInfo.");
                            }
                        }
                        catch (Exception ex)
                        {
                            // Exception logs or other processing methods can be recorded
                            //Console.WriteLine($"Exception occurred: {ex.Message}");
                        }
                    };

                    //Used to output missing feedback,  Get SelectionBox The right border of
                    GeneralTransform transform = SelectionBox.TransformToAncestor(this); // this Refers to the current one UserControl
                    Rect rect = transform.TransformBounds(new Rect(0, 0, SelectionBox.ActualWidth, SelectionBox.ActualHeight));
                    double selectionBoxRight = rect.Right; // Get the right border
                    CompositionTarget.Rendering += (sender, e) =>
                    {// register CompositionTarget.Rendering event
                        double imgLeft = Canvas.GetLeft(img);                        // Get the current one Image The left position
                        if (imgLeft > selectionBoxRight)                        // Determine whether it exceeds the right boundary
                        {
                            // Execute logical code blocks
                            ShowIgnoreFeedBack(img);
                        }
                    };

                    ApplyCanvasClip(imageContainer2);//Area cutting of conveyor belts

                    //Each animation starts with an update count
                    ImageNumTemp--;// Because this number is the total number, subtraction
                    storyboard.Begin();
                    StoryBoardsList.Add(storyboard);//Save thisstoryboardObject, used for later pause
                    //translateTransform.BeginAnimation(TranslateTransform.XProperty, animation);
                }
                catch (Exception ex)
                {
                    // Exception logs or other processing methods can be recorded
                    //Console.WriteLine($"Error in AnimateImage: {ex.Message}\n{ex.StackTrace}");
                }
            }

        }

        private void EndGame()
        {

            // Add the current game result to the corresponding array
            //wrongAnswers[hardness] += errorDecisions;
            //correctAnswers[hardness] += correctDecisions;

            if (hardness < 8)
            {
                if (errorDecisions > 0)
                {
                    // wrongAnswers[hardness] += errorDecisions;
                }
                else
                {
                    //                  correctAnswers[hardness] += correctDecisions;
                }
            }
            else
            {
                if (errorDecisions > 1)
                {
                    //wrongAnswers[hardness] += errorDecisions;
                }
                else
                {
                    // correctAnswers[hardness] += correctDecisions;
                }
            }

            //string correctAnswersString = string.Join(", ", correctAnswers);
            //string wrongAnswersString = string.Join(", ", wrongAnswers);
            //string ignoreAnswersString = string.Join(", ", ignoreAnswers);

            //// Create a message to display
            //string message = $"Correct Answers: {correctAnswersString}\n" +
            //                 $"Wrong Answers: {wrongAnswersString}\n" +
            //                 $"Ignore Answers: {ignoreAnswersString}";

            // use MessageBox Show message
            //System.Windows.MessageBox.Show(message, "Answers Information");


            // Reset these variables

            // Clear imageContainer2 All animations in and remove pictures
            foreach (UIElement element in imageContainer2.Children)
            {
                if (element is System.Windows.Controls.Image image)
                {
                    // Stop animation without triggering Completed event
                    //image.RenderTransform.BeginAnimation(TranslateTransform.XProperty, null);
                    //Logical modification still needs to be triggeredCompletedThe event
                    // Clear image resources
                    image.Source = null;
                }
                else if (element is TextBlock textBlock)
                {
                    // Stop any animations related to text and clear text
                    textBlock.Text = string.Empty;
                }
            }

            imageContainer2.Children.Clear(); // Remove all image elements



            //// Adjust the difficulty
            ////AdjustDifficulty();
            //// Show new random pictures
            //totalDecisions = 0;
            //correctDecisions = 0;
            //errorDecisions = 0;
            //missDecisions = 0;
            //// Stop the game timer

            //if (gameTimer != null)
            //{
            //    gameTimer.Stop();
            //    gameTimer = null; // Clear the timer
            //}

            //isGameRunning = false;
            //// Reset SelectionBox Stroke color
            //gameIndex++;

            //// Open AUFM_Report window and close the current window
            ////MainWindow reportWindow = new MainWindow(hardness, train_mode, correctAnswers, wrongAnswers, ignoreAnswers);
            //Grid1.Visibility = Visibility.Visible;
            //Grid2.Visibility = Visibility.Collapsed;

            //hard_set();

            //// Randomly select the specified number of non-repetitive pictures

            //List<string> allImages = imagePaths.SelectMany(x => x).ToList();
            //Random random = new Random();
            //selectedImagePaths = allImages.OrderBy(x => random.Next()).Take(right_picture_number).ToList();
            //// according to training_mode Set the visibility of pictures or text
            //if (train_mode == 1 || train_mode == 2)
            //{
            //    // Show pictures, hide text
            //    SetImagesVisible();
            //}
            //else if (train_mode == 3)
            //{
            //    // Show text, hide pictures
            //    SetTextsVisible();
            //}
        }

        private void AdjustDifficulty()
        {
            NotifyGameStatsUpdated();
            switch (hardness)
            {
                case 1:
                    if (errorDecisions >= 0)
                    {
                    }
                    else
                    {
                        hardness++;
                    }

                    break;
                case 2:
                    if (errorDecisions >= 0)
                    {
                        hardness--;
                    }
                    else
                    {
                        hardness++;
                    }

                    break;
                case 3:
                    if (errorDecisions >= 0)
                    {
                        hardness--;
                    }
                    else
                    {
                        hardness++;
                    }

                    break;
                case 4:
                    if (errorDecisions >= 0)
                    {
                        hardness--;
                    }
                    else
                    {
                        hardness++;
                    }

                    break;
                case 5:
                    if (errorDecisions >= 0)
                    {
                        hardness--;
                    }
                    else
                    {
                        hardness++;
                    }

                    break;
                case 6:
                    if (errorDecisions >= 0)
                    {
                        hardness--;
                    }
                    else
                    {
                        hardness++;
                    }

                    break;
                case 7:
                    if (errorDecisions >= 0)
                    {
                        hardness--;
                    }
                    else
                    {
                        hardness++;
                    }

                    break;
                case 8:
                    if (errorDecisions >= 1)
                    {
                        hardness--;
                    }
                    else
                    {
                        hardness++;
                    }

                    break;
                case 9:
                    if (errorDecisions >= 1)
                    {
                        hardness--;
                    }
                    else
                    {
                    }

                    break;
                default:
                    throw new Exception("Unknown difficulty level");
            }
        }

        public void hard_set()
        {//Difficulty level settings
            switch (hardness)
            {
                case 1:
                    //train_mode = 2;//20241208, the game mode does not change with the difficulty level
                    //IS_IMAGE_DETAIL = false;
                    //IS_IMAGE_HARD = false;
                    right_picture_number = 1;
                    RATE_OF_ERRORIMAGE = 0.33;
                    //DISPLAY_TIME = 8;
                    ErrorAllow = 0;
                    break;
                case 2:
                    //train_mode = 2;//20241208, the game mode does not change with the difficulty level
                    //IS_IMAGE_DETAIL = false;
                    //IS_IMAGE_HARD = false;
                    right_picture_number = 2;
                    RATE_OF_ERRORIMAGE = 0.30;
                    //DISPLAY_TIME = 8;
                    ErrorAllow = 0;
                    break;
                case 3:
                    //train_mode = 2;//20241208, the game mode does not change with the difficulty level
                    //IS_IMAGE_DETAIL = true;
                    //IS_IMAGE_HARD = false;
                    right_picture_number = 3;
                    RATE_OF_ERRORIMAGE = 0.28;
                    //DISPLAY_TIME = 7;
                    ErrorAllow = 0;
                    break;
                case 4:
                    //train_mode = 2;//20241208, the game mode does not change with the difficulty level
                    //IS_IMAGE_DETAIL = true;
                    //IS_IMAGE_HARD = false;
                    right_picture_number = 4;
                    RATE_OF_ERRORIMAGE = 0.26;
                    //DISPLAY_TIME = 7;
                    ErrorAllow = 0;
                    break;
                case 5:
                    //train_mode = 2;//20241208, the game mode does not change with the difficulty level
                    //IS_IMAGE_DETAIL = true;
                    //IS_IMAGE_HARD = true;
                    right_picture_number = 5;
                    RATE_OF_ERRORIMAGE = 0.24;
                    //DISPLAY_TIME = 7;
                    ErrorAllow = 0;
                    break;
                case 6:
                    //train_mode = 2;//20241208, the game mode does not change with the difficulty level
                    //IS_IMAGE_DETAIL = true;
                    //IS_IMAGE_HARD = true;
                    right_picture_number = 6;
                    RATE_OF_ERRORIMAGE = 0.22;
                    //DISPLAY_TIME = 7;
                    ErrorAllow = 0;
                    break;
                case 7:
                    //train_mode = 2;//20241208, the game mode does not change with the difficulty level
                    //IS_IMAGE_DETAIL = true;
                    //IS_IMAGE_HARD = true;
                    right_picture_number = 7;
                    RATE_OF_ERRORIMAGE = 0.20;
                    //DISPLAY_TIME = 6;
                    ErrorAllow = 0;
                    break;
                case 8:
                    //train_mode = 2;//20241208, the game mode does not change with the difficulty level
                    //IS_IMAGE_DETAIL = true;
                    //IS_IMAGE_HARD = true;
                    right_picture_number = 8;
                    RATE_OF_ERRORIMAGE = 0.15;
                    //DISPLAY_TIME = 6;
                    ErrorAllow = 1;
                    break;
                case 9:
                    //train_mode = 2;//20241208, the game mode does not change with the difficulty level
                    //IS_IMAGE_DETAIL = true;
                    //IS_IMAGE_HARD = true;
                    right_picture_number = 9;
                    RATE_OF_ERRORIMAGE = 0.10;
                    //DISPLAY_TIME = 6;
                    ErrorAllow = 1;
                    break;
                default:
                    throw new Exception("Unknown difficulty level");
            }
            ImageNumTemp = hardness * totalPictureMultiplier;//Set the total number of materials that should appear, which has a significant relationship with the difficulty level, so I wrote it inswitchIt's outside

        }

        private void GenerateTotalImagePaths()
        {
            try
            {
                // initialization totalImagePaths And add rightImagePaths Elements in
                if (rightImagePaths == null || rightImagePaths.Count == 0)
                {
                    throw new Exception("rightImagePaths is null or empty.");
                }

                totalImagePaths = new List<string>(rightImagePaths);

                int totalNumberOfPictures = rightImagePaths.Count * totalPictureMultiplier;
                //int totalNumberOfPictures = rightImagePaths.Count * GroupFactor;
                Console.WriteLine($"Total number of pictures needed: {totalNumberOfPictures}");

                // Get all possible image paths
                List<string> allImages = imagePaths.SelectMany(x => x).ToList();
                if (allImages == null || allImages.Count == 0)
                {
                    throw new Exception("allImages is null or empty.");
                }
                Console.WriteLine($"Total number of all images: {allImages.Count}");

                // Remove the correct image path you have selected from all image paths, making sure that no duplicate selections are selected
                var remainingImages = allImages.Except(rightImagePaths).ToList();
                Console.WriteLine($"Remaining images after excluding right images: {remainingImages.Count}");

                Random random = new Random();

                // filling totalImagePathsuntil the required number of pictures is reached
                while (totalImagePaths.Count < totalNumberOfPictures && remainingImages.Count > 0)
                {
                    int randomIndex = random.Next(remainingImages.Count);
                    totalImagePaths.Add(remainingImages[randomIndex]);
                    remainingImages.RemoveAt(randomIndex);
                    Console.WriteLine($"Added image to totalImagePaths, remaining images: {remainingImages.Count}");
                }

                // If the remaining pictures are not enough to fill in the total number, issue a warning
                if (totalImagePaths.Count < totalNumberOfPictures)
                {
                    //MessageBox.Show("The total number of pictures is insufficient, please adjust totalPictureMultiplier parameter.");
                }
                else
                {
                    Console.WriteLine($"Successfully filled totalImagePaths with {totalImagePaths.Count} images.");
                }
                //After the path is integrated, it will be messed up
                totalImagePaths = totalImagePaths.OrderBy(x => random.Next()).ToList();
                ImageNumTemp = totalImagePaths.Count;//If you don't have enough pictures, use all the materials
                ImageAnimationEnd = ImageNumTemp;
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error in GenerateTotalImagePaths: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void DisplayImagePaths()
        {
            StackPanel stackPanel = new StackPanel();

            foreach (var imagePath in totalImagePaths)
            {
                Image imageControl = new Image
                {
                    Source = new BitmapImage(new Uri(imagePath, UriKind.Relative)),
                    Stretch = System.Windows.Media.Stretch.Uniform,
                    Margin = new Thickness(5)
                };

                stackPanel.Children.Add(imageControl);
            }

            this.Content = stackPanel;
        }


        /*LJN
 Added resources for visual and sound feedback
 */
        private SoundPlayer soundPlayer; // Used to sing
        public string ErrorSoundPath;//The wrong sound file path, inOnStartAsync()Medium configuration
        public string CorrectSoundPath;//The correct sound file path is inOnStartAsync()Medium configuration
        private int StopDurations = 2000; // Stop time,ms
        private List<Storyboard> StoryBoardsList = new List<Storyboard>();//Used to enter and exit animation objects

        private string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory; // The absolute path to the current project
        private string ResourcesPath = System.IO.Path.Combine("Resources", "Word memory");//The fixed path used here is the memory path of word, which can be changed later.

        //private int GroupFactor = 7;//Total number of materials in a set of questions = GroupFactor*Number of elemental vocabulary,This one istotalPictureMultiplierPlay the same role, the two are exactly the same
        private int ImageAnimationEnd = 0;//Count the number of ended materials to facilitate logical processing at the end of the animation
        private int ImageNumTemp = 0;//Total number of materials in a set of questions
        private int ImageCorrectTemp = 0;//Number of correct answers in a set of questions
        private int ImageErrorTemp = 0;//Number of materials that are wrong in a set of questions
        private int ImageIgnoreTemp = 0;//Number of ignored materials in a set of questions
        private int ErrorAllow = 0;//If you answer wrong in a set of materials, as long as it does not exceed this number, this set of questions will still be considered correct

        private int GroupCorrect = 0;//How many groups of questions should be answered correctly
        private int GroupError = 0;//How many wrong questions

        public int LevelUp = 3;//Only by answering 3 questions correctly can you improve your level
        public int LevelDown = 3;//Answering 3 questions wrong will reduce the level
        private bool IfPictureInstruction = false;//Is the picture description displayed?
        private int InstructionFontSize = 50;//The size of the picture description

        private bool IfMemorizeLimit = true;//Is it limited memory time
        private int MemorizeLimitTime = 60;//Limited memory time, units
        private DispatcherTimer MemorizeLimit = new DispatcherTimer(); // Timer for memory time limit

        private Dictionary<int, Dictionary<string, int>> AllLevelResult = new Dictionary<int, Dictionary<string, int>>();//A dictionary that stores game results at various difficulty levels, using difficulty levelsintTo index the game results, the game results are stored in dictionary form, with the total number of words, the correct number, etc. . .

        private Dictionary<int, int> FontSizeDict = new Dictionary<int, int>//Different numbers of text pictures should also be different in size
        {//{Number of texts, size of text}
            {1,200},{2,200},{3,150},{4,350},{5,350},{6,350},{7,250},{8,250},{9,250},{10,100}
        };

        private Dictionary<Image, bool> ImageDetectDict = new Dictionary<Image, bool>();//Dictionary is used to store the correspondingImageHave you tested it to prevent repeated detection

        private Dictionary<Image, bool> ImageIgnoreDict = new Dictionary<Image, bool>();//The dictionary uses to judge thisimageHave you ever judged itignore

        private bool IfPerformingAction = false;//Add a flag, because now the keyboard and mouse can set offOKbutton, in order to prevent repeated entry into this part of the logic, set a flag bit to add more judgment

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
            await Task.Delay(StopDurations);

            image.Visibility = Visibility.Collapsed;
        }

        private void SetTrainMode()//Set the corresponding parameters according to the selected mode
        {
            switch (train_mode)
            {
                case 1: IS_IMAGE_DETAIL = true; IS_IMAGE_HARD = true; break;
                case 2: IS_IMAGE_DETAIL = false; IS_IMAGE_HARD = false; break;
                case 3: IS_IMAGE_DETAIL = true; IS_IMAGE_HARD = true; break;
                default: IS_IMAGE_DETAIL = false; IS_IMAGE_HARD = false; break;//Default Mode 2
            }
        }

        private void InitTempResults()//Clear the count value in a set of questions
        {
            ImageErrorTemp = 0;
            ImageCorrectTemp = 0;
            ImageIgnoreTemp = 0;
        }

        private void InitGroupResults()//After the difficulty level is increasedGroupLevel of data needs to be reset
        {
            InitTempResults();
            GroupCorrect = 0;
            GroupError = 0;
            UpdateGroupUI();
        }

        private void GroupCheck()//How well did this question be done, right or wrong
        {
            //Ignore or make too many mistakes directly and calculate the whole group of questions, and the rest are correct
            if ((ImageErrorTemp - ErrorAllow) > 0 || ImageIgnoreTemp > 0) { GroupError++; }
            else { GroupCorrect++; }
            //After the check is finished, it's time to clear the count
            InitTempResults();//Clear these count values
            UpdateGroupUI();//Synchronize the results toUI
            // And keep records
            AllLevelResult[hardness] = AllLevelResult.GetValueOrDefault(hardness) ?? new Dictionary<string, int>();
            AllLevelResult[hardness]["Correct number of questions"] = AllLevelResult[hardness].GetValueOrDefault("Correct number of questions") + GroupCorrect;
            AllLevelResult[hardness]["Number of wrong questions"] = AllLevelResult[hardness].GetValueOrDefault("Number of wrong questions") + GroupError;

        }

        private void BeginNextGroup()//Turn on display of the next set of questions
        {
            //Clear the count first,This step isGroupCheckCompleted
            //Put it againcontainerThe insidechildrenofImageGive it clear,A total of twocontainer
            imageContainer.Children.Clear();
            imageContainer2.Children.Clear();

            //Execute the logic at the beginning of the memory stage, select the material and display it
            Grid1.Visibility = Visibility.Visible;
            Grid2.Visibility = Visibility.Collapsed;

            List<string> allImages = imagePaths.SelectMany(x => x).ToList();
            Random random = new Random();
            selectedImagePaths = allImages.OrderBy(x => random.Next()).Take(right_picture_number).ToList();
            //Reset counter for memory time limit
            if (IfMemorizeLimit) { StartMemorizeLimit(); }
            //ImageDetect dictionary initialization
            ImageDetectDict.Clear();

            // according to training_mode Set the visibility of pictures or text
            if (train_mode == 1 || train_mode == 2)
            {
                // Show pictures, hide text
                SetImagesVisible();
            }
            else if (train_mode == 3)
            {
                // Show text, hide pictures
                SetTextsVisible();
            }

            //Set relevant flags
            isGameRunning = false;//This is equivalent to enabling the enable button in the lower right corner of the conveyor belt

            gameIndex++;
            // Stop the game timer,gameTimerIt is used to generate material on a new conveyor belt, so it needs to be stopped
            if (gameTimer != null)
            {
                gameTimer.Stop();
                gameTimer = null; // Clear the timer
            }
        }

        private void UpdateGroupUI()//every timeGroupIf the results change, theUIsuperior
        {
            //Each change is displayed synchronouslyUIGo up
            RightStatisticsAction?.Invoke(GroupCorrect, LevelUp);
            WrongStatisticsAction?.Invoke(GroupError, LevelDown);
        }

        private void AdjustGroupDifficulty()//See if the difficulty needs to be adjusted based on the number of groups currently made
        {
            if (GroupCorrect >= LevelUp)//Need to upgrade
            {
                if (hardness < MAX_HARDNESS)
                {//It means that you really need to upgrade
                    hardness++; //The count needs to be cleared after the difficulty level is adjusted
                    max_hardness = Math.Max(max_hardness, hardness);
                }
                else
                {//There are so many things in the highest level, I can't get it up again and it's full！Clear the results and repeat the cycle

                }
                InitGroupResults();
            }
            else if (GroupError >= LevelDown)//Need to be downgraded
            {
                if (hardness > MIN_HARDNESS)
                {//It really needs to be downgraded
                    hardness--; //The count needs to be cleared after the difficulty level is adjusted
                }
                else
                {//How many mistakes are there when the level is the lowest? I really can't do it again.

                }
                InitGroupResults();
            }
            else//No need to adjust, continue
            {

            }
            hard_set();//Use the adjusted difficulty level to modify parameters
        }

        private void ApplyCanvasClip(Canvas containerCanvas)// Apply the clipping area to Canvas
        {//Through cropping,textblockIn thiscanvasThe part visible is not visible
            // Create a with Canvas Rectangles of the same size
            RectangleGeometry clipGeometry = new RectangleGeometry
            {
                Rect = new Rect(0, 0, containerCanvas.ActualWidth, containerCanvas.ActualHeight)
            };

            // Take the rectangle as Canvas Crop area
            containerCanvas.Clip = clipGeometry;
        }

        private void MemorizeLimit_Tick(object sender, EventArgs e)//Timer for memory time limit
        {
            MemorizeLimitTime--;
            TimeStatisticsAction?.Invoke(max_time, MemorizeLimitTime);//Show toUI
            if (MemorizeLimitTime <= 0)
            {
                //If this function is triggered, it means that the memory limit is reached. Press the simulation manually.ContinueButton
                ContinueButton_Click(this, new RoutedEventArgs());//Click manuallyOKButton
                                                                  //Manually end memory stage
            }

        }

        private void StartMemorizeLimit()//Start memory limited time
        {
            MemorizeLimitTime = 60;
            MemorizeLimit = new DispatcherTimer();//Set time limit
            MemorizeLimit.Interval = TimeSpan.FromSeconds(1);  // Set to trigger once every second
            MemorizeLimit.Tick += MemorizeLimit_Tick;  // Bind Tick event
            MemorizeLimit.Start();

        }

        private void EndMemorizeLimit()//Eliminate memory limit
        {
            if (MemorizeLimit != null)
            {
                MemorizeLimit.Stop();            // Stop the timer
                MemorizeLimit.Tick -= MemorizeLimit_Tick;            // Unbind event handler
                MemorizeLimit = null;            // Clear timer object reference
            }
        }

        private async void PauseThenStart()//Each time you answer correctly and wrong, you specify to pause all animations for a period of time.
        {
            foreach (var storyboard in StoryBoardsList) { storyboard.Pause(); }
            // If the timer is running, stop it first
            StopTime = DateTime.Now;//Record pause time
            if (gameTimer.IsEnabled) { gameTimer.Stop(); }//This is the timer that generates the image of the material, and it also needs to be stopped
            double PauseDuration = CalculateStopSpan();
            if (StopDurations - PauseDuration >= 0)
            {
                await Task.Delay((int)PauseDuration);//Delay the timer's pause first
                gameTimer.Start(); // Recover timer at midpoint
                await Task.Delay(StopDurations - (int)PauseDuration); // Delay the remaining part
            }
            else
            {//In fact, it is impossibleStopDurations- PauseDuration<0, if so, there is a problem
                await Task.Delay((int)StopDurations);//Delay the timer's pause first
                gameTimer.Start(); // Recover timer at midpoint
                await Task.Delay((int)PauseDuration - StopDurations); // Delay the remaining part
            }
            // Restore all Storyboard
            foreach (var storyboard in StoryBoardsList) { storyboard.Resume(); }
            SelectionBox.Stroke = new SolidColorBrush(Colors.Black);//Show the color back in time
        }

        private double CalculateStopSpan()//The calculation needs to begameTimerTime of stopping
        {
            /*
             becausegameTimerthread cannot be paused, butStopDurationsNot expected during this periodGameTimer_Ticktrigger
            So ifStopDurationsTriggered duringGameTimer_Tick, need to be calculatedgameTimerofstopTime
            To ensureStopDurationsAfter the end, it can be triggered normally according to cyclesGameTimer_Tick, thereby ensuring that the material pictures appear at equal intervals
             */
            double delay = (StopTime - LastTickTime).TotalMilliseconds;
            while (delay > StopDurations)
            {
                delay += StopDurations;
            }

            return StopDurations - delay;
        }

        private void SetImagesPosition()
        {
            // make sure Grid1 The layout is completed to obtain its height
            imageContainer.RowDefinitions.Clear();
            imageContainer.Children.Clear();

            // Specify height
            double gridHeight = 920 * 0.92 * 0.8;

            int[] columnsPerRow = { };
            int imageCount = 0;

            switch (selectedImagePaths.Count)
            {
                case 1:
                    columnsPerRow = new int[] { 1 }; break;
                case 2:
                    columnsPerRow = new int[] { 2 }; break;
                case 3:
                    columnsPerRow = new int[] { 2, 1 }; break;
                case 4:
                    columnsPerRow = new int[] { 2, 2 }; break;
                case 5:
                    columnsPerRow = new int[] { 3, 2 }; break;
                case 6:
                    columnsPerRow = new int[] { 3, 3 }; break;
                case 7:
                    columnsPerRow = new int[] { 2, 2, 3 }; break;
                case 8:
                    columnsPerRow = new int[] { 3, 3, 2 }; break;
                case 9:
                    columnsPerRow = new int[] { 3, 3, 3 }; break;
                default:
                    columnsPerRow = new int[] { 1 }; break;
            }

            // Dynamically set the line
            for (int rowIndex = 0; rowIndex < columnsPerRow.Length; rowIndex++)
            {
                int rowCount = columnsPerRow.Length;

                // Calculate the height of each row
                double rowHeight = gridHeight / rowCount;

                // Add a row definition
                imageContainer.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(rowHeight)
                });

                // Create a child Grid
                Grid subGrid = new Grid
                {
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                // Define columns
                int columnCount = columnsPerRow[rowIndex];
                for (int colIndex = 0; colIndex < columnCount; colIndex++)
                {
                    subGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }

                // Add a picture
                for (int colIndex = 0; colIndex < columnCount && imageCount < selectedImagePaths.Count; colIndex++)
                {
                    Image imageControl = new Image
                    {
                        Source = new BitmapImage(new Uri(selectedImagePaths[imageCount], UriKind.Relative)),
                        Stretch = Stretch.Uniform,
                        Margin = new Thickness(5)
                    };

                    Grid.SetColumn(imageControl, colIndex);
                    subGrid.Children.Add(imageControl);
                    imageCount++;
                }

                // Add sub Grid To the Lord Grid Target
                Grid.SetRow(subGrid, rowIndex);
                imageContainer.Children.Add(subGrid);
            }
        }

        private void SetTextsPosition()
        {
            // make sure Grid1 The layout is completed to obtain its height
            imageContainer.RowDefinitions.Clear();
            imageContainer.Children.Clear();

            // Specify height
            double gridHeight = 920 * 0.92 * 0.8;

            int[] columnsPerRow = { };
            int imageCount = 0;

            switch (selectedImagePaths.Count)
            {
                case 1:
                    columnsPerRow = new int[] { 1 }; break;
                case 2:
                    columnsPerRow = new int[] { 2 }; break;
                case 3:
                    columnsPerRow = new int[] { 2, 1 }; break;
                case 4:
                    columnsPerRow = new int[] { 2, 2 }; break;
                case 5:
                    columnsPerRow = new int[] { 3, 2 }; break;
                case 6:
                    columnsPerRow = new int[] { 3, 3 }; break;
                case 7:
                    columnsPerRow = new int[] { 2, 2, 3 }; break;
                case 8:
                    columnsPerRow = new int[] { 3, 3, 2 }; break;
                case 9:
                    columnsPerRow = new int[] { 3, 3, 3 }; break;
                default:
                    columnsPerRow = new int[] { 1 }; break;
            }

            // Dynamically set the line
            for (int rowIndex = 0; rowIndex < columnsPerRow.Length; rowIndex++)
            {
                int rowCount = columnsPerRow.Length;

                // Calculate the height of each row
                double rowHeight = gridHeight / rowCount;

                // Add a row definition
                imageContainer.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(rowHeight)
                });

                // Create a child Grid
                Grid subGrid = new Grid
                {
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                // Define columns
                int columnCount = columnsPerRow[rowIndex];
                for (int colIndex = 0; colIndex < columnCount; colIndex++)
                {
                    subGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }

                // Add a picture
                for (int colIndex = 0; colIndex < columnCount && imageCount < selectedImagePaths.Count; colIndex++)
                {
                    TextBlock textBlock = new TextBlock
                    {
                        Text = Path.GetFileNameWithoutExtension(selectedImagePaths[imageCount]),
                        FontSize = Math.Min(120, rowHeight * 0.8),
                        Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black),
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(20, 0, 20, 0) // Set left and right margins to ensure beautiful spacing
                    };

                    Grid.SetColumn(textBlock, colIndex);
                    subGrid.Children.Add(textBlock);
                    imageCount++;
                }

                // Add sub Grid To the Lord Grid Target
                Grid.SetRow(subGrid, rowIndex);
                imageContainer.Children.Add(subGrid);
            }
        }

        private Image ConvertStackPanelToImage(StackPanel stackPanel, string imagePath)
        {
            // Rendering StackPanel In-place map
            Size size = new Size(325, imageContainer2.ActualHeight);
            stackPanel.Measure(size);
            stackPanel.Arrange(new Rect(new Point(0, 0), size));

            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int)size.Width,
                (int)size.Height,
                96, // DPI X
                96, // DPI Y
                PixelFormats.Pbgra32); // use Pbgra32 Format
            renderBitmap.Render(stackPanel);

            // Create a new one Image Object
            return new Image
            {
                Source = renderBitmap,
                Width = renderBitmap.Width,
                Height = renderBitmap.Height,
                Tag = new ImageTagInfo { ImagePath = imagePath }
            };
        }

        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {// Key detection
            // Check whether the key you pressed is the key you specified
            if (e.Key == System.Windows.Input.Key.Enter) // Suppose the key you specified is the Enter key
            {
                if (IfPerformingAction == false)
                {
                    IfPerformingAction = true;
                    PerformAction();//Enter the judgment logic
                    IfPerformingAction = false;
                }
            }
        }

        private void ShowIgnoreFeedBack(Image image)//Judge thisimageIs the object missing? If so, you need toBOxFrame changes color
        {
            if (ImageIgnoreDict.ContainsKey(image) && ImageIgnoreDict[image] == false)
            {
                if (ImageDetectDict.ContainsKey(image) && ImageDetectDict[image] == false)//This box has not been detected
                {
                    var tagInfo = image.Tag as ImageTagInfo;
                    string imagePath = tagInfo.ImagePath;
                    bool isCorrect = rightImagePaths.Any(path => imagePath.EndsWith(path, StringComparison.OrdinalIgnoreCase));
                    if (isCorrect)//It means it's really missing
                    {
                        SelectionBox.Stroke = new SolidColorBrush(Colors.DarkGray);//The frame changes color to remind
                        ImageIgnoreDict[image] = true;
                    }
                }
            }
        }
    }

    public partial class Graphic_memory : BaseUserControl
    {
        private bool is_pause = false;

        protected override async Task OnInitAsync()
        {
            /*LJN
            Configure the path to feedback resources             
             */
            CorrectSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");
            // for Image Control loading picture Source
            CorrectImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Correct.png"), UriKind.RelativeOrAbsolute));
            ErrorImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Error.png"), UriKind.RelativeOrAbsolute));


            train_mode = 1;
            hardness = 1;
            sharedTimer = new DispatcherTimer();
            sharedTimer.Interval = TimeSpan.FromSeconds(1);
            sharedTimer.Tick += OnTick;

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

                            case 156: //Game Level
                                hardness = par.Value.HasValue ? (int)par.Value.Value : 1;
                                break;
                            case 21: // Treatment time 
                                max_time = par.Value.HasValue ? (int)par.Value.Value : 30;
                                Debug.WriteLine($"Training time={max_time}");
                                break;
                            case 22: // Element vocabulary
                                total_picture_number_para = par.Value.HasValue ? (int)par.Value.Value : 7;
                                Debug.WriteLine($"Element vocabulary={total_picture_number_para}");
                                break;
                            case 26: // Level improvement
                                LEVEL_UP_THRESHOLD = par.Value.HasValue ? (int)par.Value.Value : 85;
                                Debug.WriteLine($"DECREASE ={LEVEL_UP_THRESHOLD}");
                                break;
                            case 27: // Level down
                                LEVEL_DOWN_THRESHOLD = par.Value.HasValue ? (int)par.Value.Value : 50;
                                Debug.WriteLine($"DECREASE ={LEVEL_DOWN_THRESHOLD}");
                                break;
                            case 23://Visual feedback
                                IS_VISUAL_FEEDBACK = par.Value == 1;
                                Debug.WriteLine($"IS_VISUAL_FEEDBACK ={IS_VISUAL_FEEDBACK}");
                                break;
                            case 25: //Auditory feedback
                                IS_AUDITORY_FEEDBACK = par.Value == 1;
                                Debug.WriteLine($"IS_AUDITORY_FEEDBACK ={IS_AUDITORY_FEEDBACK}");
                                break;
                            case 279:// Game Mode
                                train_mode = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"train_mode ={train_mode}");
                                break;
                            case 280://Is it limited memory time
                                IfMemorizeLimit = par.Value == 1;
                                Debug.WriteLine($"IfMemorizeLimt ={IfMemorizeLimit}");
                                break;
                            case 281://Is the picture description displayed?
                                IfPictureInstruction = par.Value == 1;
                                Debug.WriteLine($"IfPictureInstruction ={IfPictureInstruction}");
                                break;
                            case 272://Pipeline speed
                                SPEED_FACTOR = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"SPEED_FACTOR ={SPEED_FACTOR}");
                                break;
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

            max_time = max_time * 60;
            hard_set();
            // Calling delegate
            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
            RightStatisticsAction?.Invoke(0, LevelUp);
            WrongStatisticsAction?.Invoke(0, LevelDown);
        }

        protected override async Task OnStartAsync()
        {

            if (!isGameRunning)
            {
                if (!is_pause)
                {
                    // Randomly select the specified number of non-repetitive pictures
                    hard_set();
                    List<string> allImages = imagePaths.SelectMany(x => x).ToList();
                    Random random = new Random();
                    selectedImagePaths = allImages.OrderBy(x => random.Next()).Take(right_picture_number).ToList();
                    // according to training_mode Set the visibility of pictures or text



                    if (train_mode == 1 || train_mode == 2)
                    {
                        // Show pictures, hide text
                        SetImagesVisible();
                    }
                    else if (train_mode == 3)
                    {
                        // Show text, hide pictures
                        SetTextsVisible();
                    }
                    StartMemorizeLimit();//Because at the beginning it was a memory screen
                }
                sharedTimer.Start();
                is_pause = false; sharedTimer.Start();

            }
            else
            {
                gameTimer.Start();
                sharedTimer.Start();
            }

            // Calling delegate
            VoiceTipAction?.Invoke("When you see text matching the image you remember passes through the box, press theEnterkey.");
            SynopsisAction?.Invoke("First you will see an image on the interface, remember it and click with the mouseOKkey；Then you will see a row of text descriptions corresponding to the image passing from left to right. When you see the text matching the image you remember passes through the box, press theEnterkey to select.");
            RuleAction?.Invoke("First you will see an image on the interface, remember it and click with the mouseOKkey；Then you will see a row of text descriptions corresponding to the image passing from left to right. When you see the text matching the image you remember passes through the box, press theEnterkey to select.");//Add code, call function, display the text under the digital person

        }

        protected override async Task OnStopAsync()
        {
            sharedTimer.Stop();
            if (gameTimer != null)
            {
                gameTimer.Stop();
            }
        }

        protected override async Task OnPauseAsync()
        {
            if (isGameRunning)
            {
                gameTimer.Stop();
            }
            sharedTimer.Stop();

            is_pause = true;
        }

        protected override async Task OnNextAsync()
        {
            // Adjust the difficulty

            // Randomly select the specified number of non-repetitive pictures
            //if (!isGameRunning)
            if (true)
            {
                //List<string> allImages = imagePaths.SelectMany(x => x).ToList();
                //Random random = new Random();
                //selectedImagePaths = allImages.OrderBy(x => random.Next()).Take(right_picture_number).ToList();

                //// according to training_mode Set the visibility of pictures or text
                //if (train_mode == 1 || train_mode == 2)
                //{
                //    // Show pictures, hide text
                //    SetImagesVisible();
                //}
                //else if (train_mode == 3)
                //{
                //    // Show text, hide pictures
                //    SetTextsVisible();
                //}
                BeginNextGroup();

                // Calling delegate
                VoiceTipAction?.Invoke("When you see text matching the image you remember passes through the box, press theEnterkey.");
                SynopsisAction?.Invoke("First you will see an image on the interface, remember it and click with the mouseOKkey；Then you will see a row of text descriptions corresponding to the image passing from left to right. When you see the text matching the image you remember passes through the box, press theEnterkey to select.");
                RuleAction?.Invoke("First you will see an image on the interface, remember it and click with the mouseOKkey；Then you will see a row of text descriptions corresponding to the image passing from left to right. When you see the text matching the image you remember passes through the box, press theEnterkey to select.");
            }
            else
            {
                EndGame();
            }
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new Graphic_memory_explanation();
        }

        private int GetCorrectNum(int difficultylevel)
        {
            return correctAnswers[difficultylevel];
        }
        private int GetWrongNum(int difficultylevel)
        {
            return wrongAnswers[difficultylevel];
        }
        private int GetIgnoreNum(int difficultylevel)
        {
            return ignoreAnswers[difficultylevel];
        }
        private double CalculateAccuracy(int correctCount1, int wrongCount1, int ignoreCount1)
        {
            int totalAnswers = correctCount1 + wrongCount1 + ignoreCount1;
            return totalAnswers > 0 ? Math.Round((double)correctCount1 / totalAnswers, 2) : 0;
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
                        int correctCount2 = 0;
                        int wrongCount2 = 0;
                        int ignoreCount2 = 0;
                        int totalCount = 0;
                        double accuracy = 0;

                        //int correctCount2 = GetCorrectNum(lv);
                        //int wrongCount2 = GetWrongNum(lv);
                        //int ignoreCount2 = GetIgnoreNum(lv);
                        //int totalCount = correctCount2 + wrongCount2 + ignoreCount2;
                        //double accuracy = CalculateAccuracy(correctCount2, wrongCount2, ignoreCount2);


                        for (int lv = 1; lv <= max_hardness; lv++)
                        {
                            correctCount2 += GetCorrectNum(lv);
                            wrongCount2 += GetWrongNum(lv);
                            ignoreCount2 += GetIgnoreNum(lv);
                        }
                        totalCount = correctCount2 + wrongCount2 + ignoreCount2;
                        accuracy = CalculateAccuracy(correctCount2, wrongCount2, ignoreCount2);

                        // create Result Record
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "Graphic memory",
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
                                    Maxvalue = 9,
                                    Minvalue = 1,
                                    ModuleId = BaseParameter.ModuleId //  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "Total number of tasks",
                                    Value = totalCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "correct",
                                    Value = correctCount2,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "Correct rate",
                                    Value = accuracy * 100, // Stored as a percentage
                                    Maxvalue = 100,
                                    Minvalue = 0,
                                    Charttype = "Bar chart" ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                 new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "Wrongly selected",
                                    Value = wrongCount2, // Stored as a percentage
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "Missed selection",
                                    Maxvalue = ignoreCount2,
                                    Minvalue = 0,
                                    Value = ignoreCount2, // Stored as a percentage
                                    Charttype = "Bar chart" ,
                                    ModuleId = BaseParameter.ModuleId
                                }
                            };

                        // insert ResultDetail data
                        db.ResultDetails.AddRange(resultDetails);
                        await db.SaveChangesAsync();

                        // Output each ResultDetail Object data
                        Debug.WriteLine($"Difficulty level {hardness}:");
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

