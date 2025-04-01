using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static crs.game.Games.Graphic memory;

namespace crs.game.Games
{
    /// <summary>
    /// Graphic memory explanation.xaml Interaction logic
    /// </summary>
    public partial class Graphic memory explanation : BaseUserControl
    {
        private readonly string[][] imagePaths = new string[][]
         {

new string[]
{
    "BILD1/2/Telephone.jpg",
    "BILD1/2/Pineapple.jpg",
    "BILD1/2/TV set.jpg",
    "BILD1/2/tomato.jpg",
    "BILD1/2/Hairdryer.jpg",
    "BILD1/2/hammer.jpg",
    "BILD1/2/elephant.jpg",
    "BILD1/2/computer.jpg"
},
new string[]
{
    "BILD1/3/Butterfly.jpg",
    "BILD1/3/tangerine.jpg",
    "BILD1/3/dog.jpg",
    "BILD1/3/Screws.jpg",
    "BILD1/3/apple.jpg",
    "BILD1/3/pear.jpg",
    "BILD1/3/Alarm clock.jpg",
    "BILD1/3/Scissors.jpg",
    "BILD1/3/kiwi.jpg",
    "BILD1/3/cat.jpg"
}

         };

        // Used to save the selected image path
        private List<string> selectedImagePaths = new List<string>();
        private int LEVEL_DURATION = 1;
        private int total_picture_number_para = 7; // This parameter is multiplied by the correct number of pictures to the total number of pictures
        private int right_picture_number = 3; // The correct number of pictures displayed
        private int train_mode = 1;
        private int LEVEL_UP_THRESHOLD = 85; // Improve the accuracy threshold for difficulty（percentage）
        private int LEVEL_DOWN_THRESHOLD = 70; // Reduce the accuracy threshold for difficulty（percentage）
        private int max_time = 30;
        private bool IS_REALISTIC = true; // Whether the picture is displayed as a real object（The real picture is displayed by default）
        private int[] correctAnswers = new int[10];
        private int[] wrongAnswers = new int[10];
        private int[] ignoreAnswers = new int[10];
        private const int MaxGames = 10;
        private int hardness = 1;
        private const int MAX_HARDNESS = 9; // Maximum difficulty level
        private DispatcherTimer sharedTimer;
        private Queue<bool> recentResults = new Queue<bool>(5); // Queue that records the last 5 selection results


        private int imageGenerationCounter = 0;

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
        private double DISPLAY_TIME = 3; // Total display time of picture glide
        double RATE_OF_ERRORIMAGE = 0.5; // Display error（That is, nonimage2,3,4）Probability
        double Correct_decision_rate = 0;
        private int totalDecisions;
        private int correctDecisions;
        private int errorDecisions;
        private int missDecisions;
        private const int WAIT_DELAY = 1;

        private int gameIndex = 0;
        private readonly Brush defaultSelectionBoxStroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));
        //private int remainingTime = 30;
        private int GameremainingTime = 10;
        private DispatcherTimer trainingTimer; // Program duration timer
        private DispatcherTimer gameTimer; // Single game timer
        private Random random = new Random();
        private int continueButtonPressCount = 0;// Number of times the button is pressed
        private bool isGameRunning = false; // Is the logo game going on
        public event Action<int> GameremainingTimeUpdated;
        public event Action<int, int[], int[]> GameStatsUpdated;
        private int imageGenerationInterval = 5; // Control how many every few Tick Generate an image once
        public Action GameBeginAction { get; set; }

        public Func<string, Task> VoicePlayFunc { get; set; }

        public Graphic memory explanation()
        {
            InitializeComponent();

            hard_set();
            sharedTimer = new DispatcherTimer();
            sharedTimer.Interval = TimeSpan.FromSeconds(1);
            sharedTimer.Tick += OnTick;
            // Randomly select the specified number of non-repetitive pictures

            List<string> allImages = imagePaths.SelectMany(x => x).ToList();

            Random random = new Random();
            selectedImagePaths = allImages.OrderBy(x => random.Next()).Take(right_picture_number).ToList();
            // Will selectedImagePaths Splicing the content into a string
            string selectedImagesMessage = string.Join("\n", selectedImagePaths);

            // use MessageBox show selectedImagePaths Contents
            //MessageBox.Show(selectedImagesMessage, "Selected Image Paths");

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
            this.Loaded += Graphic memory explanation_Loaded;

        
        
        }

        private void Graphic memory explanation_Loaded(object sender, RoutedEventArgs e)
        {
            Button_2_Click(null, null);
this.Focus();  
        }

        private void ChangeSelectionBoxColor(Brush newColor)
        {
            // Change SelectionBox of Stroke color
            SelectionBox.Stroke = newColor;

            // Create a DispatcherTimer, set to 2 seconds before triggering
            DispatcherTimer colorResetTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            colorResetTimer.Tick += (s, e) =>
            {
                // recover SelectionBox of Stroke The color is default
                SelectionBox.Stroke = defaultSelectionBoxStroke;

                // Stop and remove the timer
                colorResetTimer.Stop();
            };
            colorResetTimer.Start();
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (max_time > 0)
            {
                //TimeStatisticsAction.Invoke(10, 10);
                max_time--;
                int correctCount = 0;
                int incorrectCount = 0;
                foreach (bool result in recentResults)
                {
                    if (result)
                    {
                        correctCount++;
                    }
                    else
                    {
                        incorrectCount++;
                    }
                }


            }
            else
            {
                sharedTimer.Stop();

            }
        }

        public static Window GetTopLevelWindow(UserControl userControl)
        {
            DependencyObject current = userControl;
            while (current != null && !(current is Window))
            {
                current = VisualTreeHelper.GetParent(current);
            }

            return current as Window;
        }





        private void SetImagesVisible()
        {
            try
            {
                // examine imageContainer Is it initialized correctly
                if (imageContainer == null)
                {
                    //MessageBox.Show("imageContainer is null. Please ensure it is initialized properly.");
                    return;
                }

                // Clear previous pictures
                imageContainer.Children.Clear();

                // examine selectedImagePaths Whether the image path is included
                if (selectedImagePaths == null || selectedImagePaths.Count == 0)
                {
                    //MessageBox.Show("selectedImagePaths is null or empty. Please ensure it is initialized properly.");
                    return;
                }

                // Dynamically add pictures to UniformGrid
                foreach (var imagePath in selectedImagePaths)
                {
                    try
                    {
                        // Check whether the image path is valid
                        if (string.IsNullOrWhiteSpace(imagePath))
                        {
                            //MessageBox.Show($"Image path is null or empty: {imagePath}");
                            continue;
                        }

                        // Try to load the image
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.UriSource = new Uri(imagePath, UriKind.Relative);
                        bitmapImage.EndInit();

                        System.Windows.Controls.Image imageControl = new System.Windows.Controls.Image()
                        {
                            Source = bitmapImage,
                            Stretch = Stretch.Uniform,
                            Margin = new Thickness(5)
                        };

                        imageContainer.Children.Add(imageControl);

                        // If loading successfully, display the successfully loaded information
                        //MessageBox.Show($"Successfully loaded image: {imagePath}");
                    }
                    catch (Exception ex)
                    {
                        // If loading fails, catch the exception and display the error message
                        //MessageBox.Show($"Failed to load image: {imagePath}\nException: {ex.Message}");
                    }
                }

                // examine imageContainer.Children Content in
                StringBuilder childrenInfo = new StringBuilder();
                childrenInfo.AppendLine("imageContainer.Children contains the following items:");

                foreach (UIElement child in imageContainer.Children)
                {
                    if (child is System.Windows.Controls.Image image)
                    {
                        childrenInfo.AppendLine($"Image with Source: {image.Source?.ToString() ?? "No Source"}");
                    }
                    else
                    {
                        childrenInfo.AppendLine($"Unknown UIElement of type: {child.GetType().Name}");
                    }
                }

                //MessageBox.Show(childrenInfo.ToString(), "Children Information");
            }
            catch (Exception ex)
            {
                // Capture outer exceptions and display error messages
                //MessageBox.Show($"Error in SetImagesVisible: {ex.Message}\n{ex.StackTrace}");
            }
        }
        

        private void SetTextsVisible()
        {
            // Clear previous text
            imageContainer.Children.Clear();

            // Dynamically add text to UniformGrid
            foreach (var imagePath in selectedImagePaths)
            {
                TextBlock textBlock = new TextBlock
                {
                    Text = System.IO.Path.GetFileNameWithoutExtension(imagePath),
                    FontSize = 24,
                    Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };

                imageContainer.Children.Add(textBlock);
            }

        }

        // ContinueButton_Click Event handler
        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
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

                train_mode = 2;
                GameremainingTime = LEVEL_DURATION * 60;
                //GameremainingTime = 30;
                //GameremainingTime = 30;
                rightImagePaths = selectedImagePaths;

                GenerateTotalImagePaths();
                // Start the program timer
                trainingTimer = new DispatcherTimer();
                trainingTimer.Interval = TimeSpan.FromSeconds(1); // Updated once every second
                trainingTimer.Tick += TrainingTimer_Tick;
                trainingTimer.Start();
                StartGame();
            }
            else
            {
                MessageBox.Show("No image path was selected.");
            }
        }



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

                gameTimer = new DispatcherTimer();
                gameTimer.Interval = TimeSpan.FromSeconds(1);
                gameTimer.Tick += GameTimer_Tick;
                gameTimer.Start();
            }

        }

        private void NotifyGameStatsUpdated()
        {
            GameStatsUpdated?.Invoke(hardness, correctAnswers, wrongAnswers);
        }

        //protected override void OnClosed(EventArgs e)
        //{
        //    base.OnClosed(e);
        //    TimerManager.TimerElapsed -= OnTimerElapsed;
        //}
        private void TrainingTimer_Tick(object sender, EventArgs e)
        {
            if (true)
            {
                //remainingTime--;
                GameremainingTimeUpdated?.Invoke(GameremainingTime);
            }
            else
            {
                //trainingTimer.Stop();
                //gameTimer?.Stop();
                //isGameRunning = false;
                //AUFM_Report reportWindow = new AUFM_Report(LEVEL_UP_THRESHOLD, LEVEL_DOWN_THRESHOLD, max_time, LEVEL_DURATION, true, IS_REALISTIC, correctAnswers, wrongAnswers, ignoreAnswers);
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (GameremainingTime > 0)
            {
                GameremainingTime--;
                try
                {
                   // TimeStatisticsAction.Invoke(max_time, GameremainingTime);
                }
                catch (Exception ex)
                {
                    // Exception logs or other processing methods can be recorded
                    // Console.WriteLine($"Exception occurred: {ex.Message}");
                }
                imageGenerationCounter++;
                if (imageGenerationCounter >= imageGenerationInterval)
                {
                    ShowRandomImage1();
                    imageGenerationCounter = 0;  // Reset the counter
                }
            }
            else
            {
                EndGame();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            PerformAction();
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

                // Process according to overlapping results
                if (isOverlapFound && overlappedImage != null)
                {
                    // Clear the animation without triggering Completed event
                    overlappedImage.RenderTransform.BeginAnimation(TranslateTransform.XProperty, null);

                    // Get the path to overlapping images, directly fromTagAttribute acquisition
                    string imagePath = overlappedImage.Tag.ToString();
                    string imageName = System.IO.Path.GetFileNameWithoutExtension(imagePath); // Get the name of the image（Extended extension）

                    // Check whether the image path is in the correct question bank
                    bool isCorrect = rightImagePaths.Any(path => imagePath.EndsWith(path, StringComparison.OrdinalIgnoreCase));

                    // Updated based on inspection results correctDecisions and SelectionBox The stroke color and update Border and TextBlock
                    if (isCorrect)
                    {
                        correctDecisions++;
                        ChangeSelectionBoxColor(new SolidColorBrush(Colors.Green)); // Change to green and restore after 2 seconds
                        textBlock.Background = new SolidColorBrush(Colors.Green);
                        textBlock1.Text = imageName + " correct！";
                        if (recentResults.Count >= 5)
                        {
                            recentResults.Dequeue(); // Remove the earliest results
                        }
                        recentResults.Enqueue(true); // Add the current result
                    }
                    else
                    {
                        errorDecisions++;

                        ChangeSelectionBoxColor(new SolidColorBrush(Colors.Red)); // Change to red and restore after 2 seconds

                        textBlock.Background = new SolidColorBrush(Colors.Red);
                        textBlock1.Text = imageName + " mistake！";
                        if (recentResults.Count >= 5)
                        {
                            recentResults.Dequeue(); // Remove the earliest results
                        }
                        recentResults.Enqueue(false); // Add the current result;
                    }
                    NotifyGameStatsUpdated();
                    // from imageContainer2 Remove the picture in
                    imageContainer2.Children.Remove(overlappedImage);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error in PerformAction: {ex.Message}\n{ex.StackTrace}");
            }
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

                // Randomly select an image from the general question bank
                string imagePath = totalImagePaths[random.Next(totalImagePaths.Count)];

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
                        Height = 175,
                        Margin = new Thickness(5),
                        // Unified use ImageTagInfo
                        Tag = new ImageTagInfo { ImagePath = imagePath }
                    };
                }
                else
                {
                    // Display pictures normally
                    BitmapImage bitmap = new BitmapImage(new Uri(imagePath, UriKind.Relative));

                    newImage = new System.Windows.Controls.Image
                    {
                        Source = bitmap,
                        Width = 325,
                        Height = 175,
                        Margin = new Thickness(5),
                        // Unified use ImageTagInfo
                        Tag = new ImageTagInfo { ImagePath = imagePath }
                    };
                }

                // Make sure the image is displayed on the top layer
                System.Windows.Controls.Panel.SetZIndex(newImage, int.MaxValue); // Will ZIndex Set to maximum value

                // Set the image in imageContainer2 I'm in the middle
                CenterImageInContainer(newImage);

                // Add new image to imageContainer2
                imageContainer2.Children.Add(newImage);

                // Animation mobile pictures
                AnimateImage(newImage);
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error in ShowRandomImage1: {ex.Message}\n{ex.StackTrace}");
            }
        }


        private void CenterImageInContainer(System.Windows.Controls.Image img)
        {
            // make sure imageContainer2 Have measured and have a valid size
            if (imageContainer2.ActualWidth == 0 || imageContainer2.ActualHeight == 0)
            {
                // Available to subscribe imageContainer2 of Loaded Event, make sure this method is called after the layout is completed
                imageContainer2.Loaded += (s, e) => CenterImageInContainer(img);
                return;
            }

            double containerWidth = imageContainer2.ActualWidth;
            double containerHeight = imageContainer2.ActualHeight;

            double imgWidth = img.Width;
            double imgHeight = img.Height;

            // Calculate the centered position
            double left = (containerWidth - imgWidth) / 2;
            double top = (containerHeight - imgHeight) / 2;

            // Set the location of the image
            Canvas.SetLeft(img, left);
            Canvas.SetTop(img, top);
        }
        private RenderTargetBitmap CreateTextImage(string text)
        {
            // Create text blocks to display text
            TextBlock textBlock = new TextBlock
            {
                Text = text, // Use filename instead of full path
                FontSize = 128, // Increase font size
                Foreground = new SolidColorBrush(Colors.Black),
                Background = new SolidColorBrush(Colors.Transparent), // Set the background to transparent
                Width = 375, // Increase the width of the text block

                Height = 200, // Increase the height of the text block
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };

            // Render text blocks as bitmaps, increasing width and height
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(375, 200, 96, 96, PixelFormats.Pbgra32);
            textBlock.Measure(new Size(375, 200));
            textBlock.Arrange(new Rect(new Size(375, 200)));
            renderBitmap.Render(textBlock);

            return renderBitmap;
        }
        private void AnimateImage(System.Windows.Controls.Image img)
        {
            try
            {
                double fromValue = 0;
                double windowWidth = 1280; // Fixed window width
                double toValue = windowWidth - img.ActualWidth -350; // The width of the window minus the width of the image

                TranslateTransform translateTransform = new TranslateTransform();
                img.RenderTransform = translateTransform;

                double REAL_TIME = DISPLAY_TIME * SPEED_FACTOR; // Synthesize real time
                imageGenerationInterval = (int)(REAL_TIME /2) ;  // For example, one is added every 2 seconds interval, you can adjust this multiple

                DoubleAnimation animation = new DoubleAnimation
                {
                    From = fromValue,
                    To = toValue,
                    Duration = new Duration(TimeSpan.FromSeconds(REAL_TIME))
                };
                animation.Completed += (s, e) =>
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

                            // If it is the correct picture that is not selected, add ignoreAnswers[hardness] Value of
                            if (isCorrect)
                            {
                                ignoreAnswers[hardness]++;
                               // System.Windows.MessageBox.Show("Answers Information");
                            }
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

                translateTransform.BeginAnimation(TranslateTransform.XProperty, animation);
            }
            catch (Exception ex)
            {
                // Exception logs or other processing methods can be recorded
                //Console.WriteLine($"Error in AnimateImage: {ex.Message}\n{ex.StackTrace}");
            }
        }


        private void EndGame()
        {
            // Add the current game result to the corresponding array

            if (errorDecisions > 1)
            {
                result_text.Text = "Sorry to answer wrong！";
                result_text.Foreground = new SolidColorBrush(Colors.Red);

                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(3); // Set the interval of 3 seconds
                timer.Tick += (s, args) =>
                {
                    // Actions performed after 3 seconds
                    // Clear imageContainer2 All animations in and remove pictures
                    result_text.Text = "";
                    foreach (UIElement element in imageContainer2.Children)
                    {
                        if (element is System.Windows.Controls.Image image)
                        {
                            // Stop animation
                            image.RenderTransform.BeginAnimation(TranslateTransform.XProperty, null);
                            // Clear image resources
                            image.Source = null;
                        }
                    }
                    imageContainer2.Children.Clear(); // Remove all image elements

                    // Adjust the difficulty

                    // Show new random pictures
                    totalDecisions = 0;
                    correctDecisions = 0;
                    errorDecisions = 0;
                    missDecisions = 0;
                    // Stop the game timer

                    if (gameTimer != null)
                    {
                        gameTimer.Stop();
                        gameTimer = null; // Clear the timer
                    }

                    isGameRunning = false;
                    // Reset SelectionBox Stroke color
                    gameIndex++;

                    // Open AUFM_Report window and close the current window
                    Grid1.Visibility = Visibility.Visible;
                    Grid2.Visibility = Visibility.Collapsed;

                    hard_set();

                    // Randomly select the specified number of non-repetitive pictures
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

                    // Stop the timer to prevent repeated triggering
                    timer.Stop();
                };

                timer.Start(); // Start the timer
            }
            else
            {
                result_text.Text = "Congratulations on getting right！";
                result_text.Foreground = new SolidColorBrush(Colors.Green);
                end.Visibility = Visibility.Visible;
            }
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
        {
            switch (hardness)
            {
                case 1:
                    IS_IMAGE_DETAIL = false;
                    IS_IMAGE_HARD = false;
                    CHOOSE_IMAGE_COUNT = 2;
                    RATE_OF_ERRORIMAGE = 0.33;
                    DISPLAY_TIME = 8;
                    break;
                case 2:
                    IS_IMAGE_DETAIL = false;
                    IS_IMAGE_HARD = false;
                    CHOOSE_IMAGE_COUNT = 3;
                    RATE_OF_ERRORIMAGE = 0.30;
                    DISPLAY_TIME = 8;
                    break;
                case 3:
                    IS_IMAGE_DETAIL = true;
                    IS_IMAGE_HARD = false;
                    CHOOSE_IMAGE_COUNT = 4;
                    RATE_OF_ERRORIMAGE = 0.28;
                    DISPLAY_TIME = 7;
                    break;
                case 4:
                    IS_IMAGE_DETAIL = true;
                    IS_IMAGE_HARD = false;
                    CHOOSE_IMAGE_COUNT = 6;
                    RATE_OF_ERRORIMAGE = 0.26;
                    DISPLAY_TIME = 7;
                    break;
                case 5:
                    IS_IMAGE_DETAIL = true;
                    IS_IMAGE_HARD = true;
                    CHOOSE_IMAGE_COUNT = 4;
                    RATE_OF_ERRORIMAGE = 0.24;
                    DISPLAY_TIME = 7;
                    break;
                case 6:
                    IS_IMAGE_DETAIL = true;
                    IS_IMAGE_HARD = true;
                    CHOOSE_IMAGE_COUNT = 6;
                    RATE_OF_ERRORIMAGE = 0.22;
                    DISPLAY_TIME = 7;
                    break;
                case 7:
                    IS_IMAGE_DETAIL = true;
                    IS_IMAGE_HARD = true;
                    CHOOSE_IMAGE_COUNT = 6;
                    RATE_OF_ERRORIMAGE = 0.20;
                    DISPLAY_TIME = 6;
                    break;
                case 8:
                    IS_IMAGE_DETAIL = true;
                    IS_IMAGE_HARD = true;
                    CHOOSE_IMAGE_COUNT = 9;
                    RATE_OF_ERRORIMAGE = 0.15;
                    DISPLAY_TIME = 6;
                    break;
                case 9:
                    IS_IMAGE_DETAIL = true;
                    IS_IMAGE_HARD = true;
                    CHOOSE_IMAGE_COUNT = 9;
                    RATE_OF_ERRORIMAGE = 0.10;
                    DISPLAY_TIME = 6;
                    break;
                default:
                    throw new Exception("Unknown difficulty level");
            }
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
                System.Windows.Controls.Image imageControl = new System.Windows.Controls.Image()
                {
                    Source = new BitmapImage(new Uri(imagePath, UriKind.Relative)),
                    Stretch = System.Windows.Media.Stretch.Uniform,
                    Margin = new Thickness(5)
                };

                stackPanel.Children.Add(imageControl);
            }

            this.Content = stackPanel;
        }


        private void end_Click(object sender, RoutedEventArgs e)
        {
            // Related logic for starting answering questions
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
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Grid1.Visibility = Visibility.Collapsed;
                        Grid2.Visibility = Visibility.Collapsed;
                        Button_1.IsEnabled = false;
                        Button_2.Content = "Next step";
						Button_1.Visibility = Visibility.Collapsed;
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
                        Grid1.Visibility = Visibility.Collapsed;
                        Grid2.Visibility = Visibility.Collapsed;
                        Button_1.IsEnabled = true;
                        Button_2.Content = "Trial";
						Button_1.Visibility = Visibility.Visible;
						Button_2.Margin = new Thickness(550, 850, 0, 0);
						Button_3.Margin = new Thickness(911, 850, 0, 0);

						await OnVoicePlayAsync(Text_2.Text);
                    }
                    break;
                case 2:
                    {
                        // Enter the trial interface
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;

                        // Controls showing the trial part
                        Grid1.Visibility = Visibility.Visible;
                       

                        // Hide the button for the explanation section
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Collapsed;

                        // Force focus to remain in the window
                        this.Focus();

                        //LJN, instructing the delegate in the explanation module
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("First you will see an image on the interface, remember it and click with the mouse\tOKkey；Then you will see a row of text descriptions corresponding to the image passing from left to right. When you see the text matching the image you remember passes through the box, press theEnterkey to select.");//Add code, call function, display the text under the digital person
                        //LJN

                    }
                    break;
            }
        }


        /// <summary>
        /// Voice playback of explanation content
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

