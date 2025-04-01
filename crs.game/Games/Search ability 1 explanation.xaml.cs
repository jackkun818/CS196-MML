using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.Numerics;
using System.Media;
using log4net.Core;
namespace crs.game.Games
{
    /// <summary>
    /// Search ability 2 explanation.xaml Interaction logic
    /// </summary>
    public partial class Search ability 1 explanation : BaseUserControl
    {
        private readonly string[][] imagePaths = new string[][]
        {
            new string[]
            {
                "EXO/1/1.png", "EXO/1/2.png", "EXO/1/3.png", "EXO/1/4.png",
                "EXO/1/5.png", "EXO/1/6.png", "EXO/1/7.png", "EXO/1/8.png",
                "EXO/1/9.png", "EXO/1/10.png", "EXO/1/11.png", "EXO/1/12.png",
                "EXO/1/13.png", "EXO/1/14.png", "EXO/1/15.png", "EXO/1/16.png",
                "EXO/1/17.png", "EXO/1/18.png", "EXO/1/19.png", "EXO/1/20.png",
                "EXO/1/21.png", "EXO/1/22.png", "EXO/1/23.png", "EXO/1/24.png",
                "EXO/1/25.png", "EXO/1/26.png", "EXO/1/27.png", "EXO/1/28.png",
                "EXO/1/29.png", "EXO/1/30.png", "EXO/1/31.png", "EXO/1/32.png"
            },
            new string[]
            {
                "EXO/2/Rocket1.png", "EXO/2/Rocket2.png", "EXO/2/Star.png", "EXO/2/Planet1.png",
                "EXO/2/Planet3.png", "EXO/2/Sun.png", "EXO/2/Planet2.png", "EXO/2/Meteor.png",
                "EXO/2/Planet4.png", "EXO/2/Spaceship.png", "EXO/2/Background.png"
            }

        };

        private int fix_bug = 0;
        private List<int> trialModes = new List<int> { 1}; // List of modes to try
        private int currentTrialModeIndex = 0; // Current trial mode index
        private bool isTrialPhase = true; // Is it in the trial stage?
        private int max_time = 1; // Total duration of the window, unit minutes
        private int train_mode = 1; // Game mode, 1, 2, 3, 4
        private bool is_gaming = false;
        private int success_time = 0;
        private int fail_time = 0;
        private int level = 1; // Current game difficulty level
        private List<int> missingNumbers;
        private List<int> userInputNumbers;
        private string userInput; // Stores the numbers entered by the user

        private int right_picture_number = 4; // The correct number of pictures displayed
        private int chose_picture_number = 6; // Number of selectable pictures displayed

        private int max_right_display = 2; // The maximum number of correct pictures to display
        private int mini_right_display = 1; // Minimum number of correct pictures to display
        private int mislead_picture_display_number = 4; // The total number of displays in the interfering picture

        private List<System.Windows.Controls.Image> correctImages; // List of correct pictures
        private List<System.Windows.Controls.Image> selectedImages; // User selected image

        private Queue<bool> recentResults = new Queue<bool>(5); // Queue to record the last 5 game results
        private int level_updata_threshold = 3; // The correct or wrong threshold for difficulty update
        private int maxnumber = 5; // The maximum number displayed
        private int minnumber = 1;//The minimum number to be displayed
        private int miss_number = 2; // Number of disappearing numbers
        private int mode1_display_size = 1; // Display box size: 1=Small, 2=Medium, 3=Big, 4=full screen

        private const int MaxGames = 10;
        private int[] correctAnswers = new int[MaxGames];
        private int[] wrongAnswers = new int[MaxGames];
        private int[] ignoreAnswers = new int[MaxGames];

        private DispatcherTimer gameTimer; // Global timer
        private TimeSpan timeRemaining; // time left

        private Canvas selectionCanvas; // Declare in class selectionCanvas As a global variable
        private bool isCorrectAnswer = false; // Add fields to record whether the user answers correctly  
        public Action GameBeginAction { get; set; }
        private WrapPanel choicePanel; // Panel for saving selected images
        public Func<string, Task> VoicePlayFunc { get; set; }
        private DispatcherTimer IntervalTimer;//The interval timer between questions
        private int CorrectInterval = 2;//Answer 3sinterval
        private int ErrorInterval = 2;//Wrong answer 5sinterval

        private Dictionary<int, TextBlock> NumTextDict = new Dictionary<int, TextBlock>();//Used to store numbers and correspondingTextblockObject, good index in later
        private List<int> AllNumbers = new List<int>();//A list of all numbers used to store
        private int ErrorCount = 0;
        private int ErrorLimit = 2;
        public Search ability 1 explanation()
        {
            InitializeComponent();
            InitializeGame();
            AdjustDifficulty(level);
            AdjustDifficultyMode2(level);
            AdjustDifficultyMode3(level);
            AdjustDifficultyMode4(level);
            // Initialize the timer
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1); // Triggered once every second
            gameTimer.Tick += GameTimer_Tick;
            timeRemaining = TimeSpan.FromMinutes(max_time); // Set the time when the entire window exists
            gameTimer.Start(); // Start timing

            IntervalTimer = new DispatcherTimer();
            IntervalTimer.Tick += IntervalTimer_Tick;

            this.Loaded += Search ability 1 explanation_Loaded;

            this.Cursor = Cursors.None; // Hide the default cursor
            CustomCursor.Visibility = Visibility.Visible; // Show custom cursor
            MouseMove += Window_MouseMove; // subscription MouseMove event
            CustomCursor.Width = 65; // Adjust to the width you want
            CustomCursor.Height = 65; // Adjust to the height you want

        }

        private void Search ability 1 explanation_Loaded(object sender, RoutedEventArgs e)
        {
            // Ensure key presses and focus behavior when page loads
            Button_2_Click(null, null);
            this.Focus();  // Make sure the focus is on the window
            ErrorCount = ErrorLimit;
        }

        private void IntervalTimer_Tick(object sender, EventArgs e)//Answer interval timer
        {// This will be triggered to enter the next question
            OverLayGrid.IsEnabled = true;
            IntervalTimer.Stop();//Stop the timer
            InitializeGame();
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            // Reduce remaining time per second
            if (timeRemaining > TimeSpan.Zero)
            {
                timeRemaining = timeRemaining.Add(TimeSpan.FromSeconds(-1));
                UpdateTimerDisplay(); // Update timer display
            }
            else
            {
                gameTimer.Stop(); // Stop the timer
                CloseApplication(); // Close the entire application window
            }
        }

        private void UpdateTimerDisplay()
        {
            // Here you can choose whether to display time, but according to your requirements, there is no time display logic for a single game.
            // If you do not want to display the time, you can omit the implementation of this method
        }

        private void CloseApplication()
        {
            // Close the entire application window;

        }

        private void InitializeGame()
        {
            ResetGameState(); // Reset status before starting a new game
            switch (train_mode)
            {
                case 1:
                    //modeTextBlock.Text = "Find missing numbers in the range of numbers and enter them one by one from small to large";
                    break;
                case 2:
                    //modeTextBlock.Text = "Identify different shapes that are superimposed and select them from the bottom of the screen";
                    break;
                case 3:
                    //modeTextBlock.Text = "The target object you need to find appears on the lower part of the screen, and find these objects from the upper picture.";
                    break;
                case 4:
                    // modeTextBlock.Text = "Count out and enter the number of times each correct object appears in the picture";
                    break;
                default:
                    //modeTextBlock.Text = "Unknown mode";
                    break;
            }

            // Hide components to make sure they are not displayed in non-mode 1
            confirm.Visibility = Visibility.Collapsed;
            textBlock.Visibility = Visibility.Collapsed;
            myCanvas.Visibility = Visibility.Collapsed;
            confirm.Visibility = Visibility.Collapsed;

            //AdjustDifficulty(level); // According to currentlevelAdjust the difficulty of the game

            // Initialize the game mode
            if (train_mode == 1)
            {
                SetupGameMode1();
            }
            else if (train_mode == 2)
            {
                SetupGameMode2();
            }
            else if (train_mode == 3)
            {
                SetupGameMode3();
            }
            else if (train_mode == 4)
            {
                SetupGameMode4();
            }

            end.Visibility = Visibility.Collapsed;
        }
        private void SetupGameMode2()
        {
            confirm.Visibility = Visibility.Visible;
            //Panel.SetZIndex(confirm, 999); // 999 It is a relatively large value, ensure it is at the top level
            // Clear previous content
            MainGrid.Children.Clear();

            // for MainGrid Add a row definition
            MainGrid.RowDefinitions.Clear();
            MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(3, GridUnitType.Star) }); // Overlapping image area above
            MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) }); // Select the picture area below

            // Show the correct image of the overlay
            correctImages = new List<System.Windows.Controls.Image>();
            selectedImages = new List<System.Windows.Controls.Image>();
            DisplayOverlayImages();

            // Shows available pictures
            DisplayChoiceImages();
        }

        private void DisplayOverlayImages()
        {
            Canvas overlayCanvas = new Canvas
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 300,  // Width can be adjusted according to requirements
                Height = 300  // The height can be adjusted according to the needs
            };

            Random rand = new Random();
            List<int> indices = Enumerable.Range(0, imagePaths[0].Length).OrderBy(x => rand.Next()).Take(right_picture_number).ToList();

            foreach (int index in indices)
            {
                string imagePath = imagePaths[0][index];
                System.Windows.Controls.Image img = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri(imagePath, UriKind.Relative)),
                    Width = 100,
                    Height = 100,
                    Opacity = 0.5, // Set transparency
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    Tag = imagePath  // For each picture Tag Attribute assignment
                };

                // Randomly adjust the position and rotation angle of the picture to produce an overlapping effect
                double left = rand.Next(50);
                double top = rand.Next(50);
                double angle = rand.Next(-15, 15);

                Canvas.SetLeft(img, left);
                Canvas.SetTop(img, top);

                RotateTransform rotateTransform = new RotateTransform(angle);
                img.RenderTransform = rotateTransform;

                correctImages.Add(img);  // Add the correct image to the list
                overlayCanvas.Children.Add(img);
            }

            Grid.SetRow(overlayCanvas, 0);
            MainGrid.Children.Add(overlayCanvas);
        }
        private void DisplayChoiceImages()
        {
            choicePanel = new WrapPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20)
            };


            Random rand = new Random();
            List<int> indices = new List<int>();

            // First make sure all the correct images displayed overlap are added to the selection list
            foreach (var correctImg in correctImages)
            {
                string correctImagePath = (string)correctImg.Tag;
                int correctIndex = Array.IndexOf(imagePaths[0], correctImagePath);
                indices.Add(correctIndex);
            }

            // Fill in the remaining selection images to ensure the total number is reached chose_picture_number
            while (indices.Count < chose_picture_number)
            {
                int index = rand.Next(imagePaths[0].Length);
                if (!indices.Contains(index))
                {
                    indices.Add(index);
                }
            }

            // Randomize the order of the pictures
            indices = indices.OrderBy(x => rand.Next()).ToList();

            foreach (int index in indices)
            {
                string imagePath = imagePaths[0][index];
                System.Windows.Controls.Image img = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri(imagePath, UriKind.Relative)),
                    Width = 80,
                    Height = 80,
                    Margin = new Thickness(10),
                    Tag = imagePath  // For each picture Tag Attribute assignment
                };

                Border border = new Border
                {
                    BorderThickness = new Thickness(2),
                    BorderBrush = Brushes.Transparent, // No border at the beginning
                    Child = img
                };

                border.MouseLeftButtonDown += (sender, e) =>
                {
                    if (selectedImages.Contains(img))
                    {
                        // If selected, unselect
                        border.BorderBrush = Brushes.Transparent;
                        selectedImages.Remove(img);
                    }
                    else
                    {
                        if (correctImages.Any(c => (string)c.Tag == (string)img.Tag))
                        {
                            border.BorderBrush = Brushes.Green;
                        }
                        else
                        {
                            border.BorderBrush = Brushes.Red;
                        }
                        selectedImages.Add(img);
                    }
                };

                choicePanel.Children.Add(border);
            }

            Grid.SetRow(choicePanel, 1);
            MainGrid.Children.Add(choicePanel);
        }

        private void SetupGameMode4()
        {
            confirm.Visibility = Visibility.Visible;
            textBlock.Visibility = Visibility.Visible;
            myCanvas.Visibility = Visibility.Visible;

            // ClearMainGridEverything in
            MainGrid.Children.Clear();

            // Create aGridTo place background pictures and select pictures
            Grid gameGrid = new Grid();

            System.Windows.Controls.Image backgroundImage = new System.Windows.Controls.Image
            {
                Source = new BitmapImage(new Uri("EXO/2/Background.png", UriKind.Relative)),
                Stretch = Stretch.Uniform,
                Width = 1000,  // Adjust the width of the background image
                Height = 660,  // Adjust the height of the background image
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, -100, 0, 0)  // passMarginMove the attribute up by 100 pixels
            };


            // Add background image toGridofBackgroundlayer
            gameGrid.Children.Add(backgroundImage);

            // Initialize the correct picture list
            correctImages = new List<System.Windows.Controls.Image>();

            // Randomly select the correct image and add it tocorrectImagesIn the list
            Random rand = new Random();
            List<int> correctIndices = new List<int>();

            while (correctIndices.Count < right_picture_number)
            {
                int index = rand.Next(imagePaths[1].Length);

                if (imagePaths[1][index] != "EXO/2/Background.png" && !correctIndices.Contains(index))
                {
                    correctIndices.Add(index);

                    // Add the correct image tocorrectImagesIn the list
                    System.Windows.Controls.Image img = new System.Windows.Controls.Image
                    {
                        Source = new BitmapImage(new Uri(imagePaths[1][index], UriKind.Relative)),
                        Width = 80,
                        Height = 80,
                        Margin = new Thickness(10)
                    };

                    correctImages.Add(img);
                }
            }

            // Create a selection image to displayCanvas
            selectionCanvas = new Canvas
            {
                Width = 500,  // Keeps consistent with the width of the background image
                Height = 300  // Consistent with the background image height
            };

            // existCanvasAfter loading, display the selectable picture
            selectionCanvas.Loaded += (s, e) =>
            {
                DisplaySelectableImagesMode4(selectionCanvas, rand);
            };

            // Create a white borderBorder, indicating a randomly generated range
            Border selectionBorder = new Border
            {
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(2),
                Width = 1000,  // The width of the border, withCanvasStay consistent
                Height = 600,  // The height of the border, withCanvasStay consistent
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, -100, 0, 0)  // passMarginMove the attribute up by 100 pixels
            };

            // Create aStackPanelTo include selectionCanvasand correct picturesPanel
            StackPanel mainPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 20, 0, 0)
            };

            // WillCanvasAdd toGridThe foreground layer（Above the background picture）
            gameGrid.Children.Add(selectionCanvas);

            // Will have bordersBorderAdd toCanvasAbove, it indicates the random generation range
            gameGrid.Children.Add(selectionBorder);

            // existStackPanelThe correct picture to be selected is displayed below
            DisplayCorrectImagesMode4(mainPanel);

            // WillStackPanelAdd toGridBelow
            gameGrid.Children.Add(mainPanel);

            // WillGridAdd toMainGridmiddle
            MainGrid.Children.Add(gameGrid);
        }

        private void DisplaySelectableImagesMode4(Canvas selectionCanvas, Random rand)
        {
            // Boundary information
            double leftBound = -203.2;
            double rightBound = 750.8;
            double topBound = -171.98;
            double bottomBound = 312.52;

            // Initialize the image display count
            List<System.Windows.Controls.Image> imagesToDisplay = new List<System.Windows.Controls.Image>();

            // Add correct picture to the picture display list
            foreach (var correctImage in correctImages)
            {
                // Randomly generate display times for each correct image
                int displayCount = rand.Next(mini_right_display, max_right_display + 1);
                for (int i = 0; i < displayCount; i++)
                {
                    System.Windows.Controls.Image imgCopy = new System.Windows.Controls.Image
                    {
                        Source = correctImage.Source,
                        Width = 80,
                        Height = 80,
                        Margin = new Thickness(10)
                    };
                    imagesToDisplay.Add(imgCopy);
                }
            }

            // Add interfering pictures to the picture display list
            List<int> remainingIndices = Enumerable.Range(0, imagePaths[1].Length)
                .Where(i => !correctImages.Any(c => ((BitmapImage)c.Source).UriSource.ToString().EndsWith(imagePaths[1][i]))
                             && imagePaths[1][i] != "EXO/2/Background.png")
                .OrderBy(x => rand.Next())
                .Take(mislead_picture_display_number)
                .ToList();

            foreach (var index in remainingIndices)
            {
                System.Windows.Controls.Image img = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri(imagePaths[1][index], UriKind.Relative)),
                    Width = 80,
                    Height = 80,
                    Margin = new Thickness(10)
                };

                imagesToDisplay.Add(img);
            }

            // Randomize the location of the image and display the image inCanvasmiddle
            foreach (var img in imagesToDisplay)
            {
                // Random generationXandYCoordinates to ensure that the image does not exceed the background image boundary
                double maxLeft = rightBound - img.Width;
                double maxTop = bottomBound - img.Height;

                double left = rand.NextDouble() * (maxLeft - leftBound) + leftBound;
                double top = rand.NextDouble() * (maxTop - topBound) + topBound;

                // Create borders and set image locations
                Border border = new Border
                {
                    BorderThickness = new Thickness(2),
                    BorderBrush = Brushes.Transparent, // No border at the beginning
                    Child = img
                };

                // set up Border Location
                Canvas.SetLeft(border, left);
                Canvas.SetTop(border, top);

                selectionCanvas.Children.Add(border);
            }
        }

        private void confirmButton_Click4(object sender, RoutedEventArgs e)
        {
            bool isCorrect = true;
            int index = 0;

            foreach (var correctImage in correctImages)
            {
                string imageUri = ((BitmapImage)correctImage.Source).UriSource.ToString().Replace("pack://application:,,,", "");

                // Calculate whether the number of players input matches the actual number
                int correctImageCount = selectionCanvas.Children.OfType<Border>()
                    .Count(border => ((BitmapImage)((System.Windows.Controls.Image)border.Child).Source).UriSource.ToString().Replace("pack://application:,,,", "") == imageUri);

                if (userInputNumbers[index] != correctImageCount)
                {
                    isCorrect = false;
                    break;
                }

                index++;
            }

            if (isCorrect)
            {
                success_time++;
            }
            else
            {
                fail_time++;
            }

            EndGame(); // Trigger end game logic
        }

        private void DisplayCorrectImagesMode4(StackPanel mainPanel)
        {
            StackPanel correctPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0) // Adjust the correct image display position
            };

            // Create aHashSetUsed for deduplication, ensuring that each correct image is displayed only once
            HashSet<string> displayedImages = new HashSet<string>();

            foreach (var img in correctImages)
            {
                // Get the pictureURI
                string imageUri = ((BitmapImage)img.Source).UriSource.ToString();

                // If the image has been displayed, skip
                if (displayedImages.Contains(imageUri))
                    continue;

                // Add toHashSetto prevent repeated display
                displayedImages.Add(imageUri);

                // Show the image once in the lower panel
                System.Windows.Controls.Image correctImg = new System.Windows.Controls.Image
                {
                    Source = img.Source,
                    Width = 80,
                    Height = 80,
                    Margin = new Thickness(10)
                };

                correctPanel.Children.Add(correctImg);
            }

            mainPanel.Children.Add(correctPanel);
        }

        private void SetupGameMode3()
        {
            // Make the confirmation button visible
            confirm.Visibility = Visibility.Visible;

            // ClearMainGridEverything in
            MainGrid.Children.Clear();

            MainGrid.RowDefinitions.Clear();
            MainGrid.ColumnDefinitions.Clear();
            // Create aGridTo place background pictures and select pictures
            Grid gameGrid = new Grid();

            // Create background image
            System.Windows.Controls.Image backgroundImage = new System.Windows.Controls.Image
            {
                Source = new BitmapImage(new Uri("EXO/2/Background.png", UriKind.Relative)),
                Stretch = Stretch.Uniform,
                Width = 1000,  // Adjust the width of the background image
                Height = 660,  // Adjust the height of the background image
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, -100, 0, 0)  // passMarginMove the attribute up by 100 pixels
            };

            // Add background image toGridofBackgroundlayer
            gameGrid.Children.Add(backgroundImage);

            // Initialize the correct picture list
            correctImages = new List<System.Windows.Controls.Image>();
            selectedImages = new List<System.Windows.Controls.Image>();

            // Randomly select the correct image and add it tocorrectImagesIn the list
            Random rand = new Random();
            List<int> correctIndices = new List<int>();

            while (correctIndices.Count < right_picture_number)
            {
                int index = rand.Next(imagePaths[1].Length);

                if (imagePaths[1][index] != "EXO/2/Background.png" && !correctIndices.Contains(index))
                {
                    correctIndices.Add(index);

                    System.Windows.Controls.Image img = new System.Windows.Controls.Image
                    {
                        Source = new BitmapImage(new Uri(imagePaths[1][index], UriKind.Relative)),
                        Width = 80,
                        Height = 80,
                        Margin = new Thickness(10)
                    };

                    correctImages.Add(img);
                }
            }

            // Create a selection image to displayCanvas
            Canvas selectionCanvas = new Canvas
            {
                Width = 1000,  // Keeps consistent with the width of the background image
                Height = 660  // Consistent with the background image height
            };

            // existCanvasAfter loading, display the selectable picture
            selectionCanvas.Loaded += (s, e) =>
            {
                DisplaySelectableImages(selectionCanvas, rand);
            };

            // Create a white borderBorder, indicating a randomly generated range
            Border selectionBorder = new Border
            {
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(2),
                Width = 1000,  // The width of the border, withCanvasStay consistent
                Height = 600,  // The height of the border, withCanvasStay consistent
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, -100, 0, 0)  // passMarginMove the attribute up by 100 pixels
            };

            // Create aStackPanelTo include selectionCanvasand correct picturesPanel
            StackPanel mainPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 20, 0, 0)
            };

            // WillCanvasAdd toGridThe foreground layer（Above the background picture）
            gameGrid.Children.Add(selectionCanvas);

            // Will have bordersBorderAdd toCanvasAbove, it indicates the random generation range
            gameGrid.Children.Add(selectionBorder);

            // existStackPanelThe correct picture to be selected is displayed below    
            DisplayCorrectImages(mainPanel);

            // WillStackPanelAdd toGridBelow
            gameGrid.Children.Add(mainPanel);

            // WillGridAdd toMainGridmiddle
            MainGrid.Children.Add(gameGrid);
        }
        private void DisplayCorrectImages(StackPanel mainPanel)
        {
            StackPanel correctPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0) // Adjust the correct image display position
            };

            foreach (var img in correctImages)
            {
                System.Windows.Controls.Image correctImg = new System.Windows.Controls.Image
                {
                    Source = img.Source,
                    Width = 80,
                    Height = 80,
                    Margin = new Thickness(10)
                };
                correctPanel.Children.Add(correctImg);
            }

            mainPanel.Children.Add(correctPanel);
        }

        private void DisplaySelectableImages(Canvas selectionCanvas, Random rand)
        {
            // Boundary information
            double leftBound = -3.2;
            double rightBound = 950.8;
            double topBound = 1.98;
            double bottomBound = 482.52;

            // Create a list of indexes for selectable images and first add the index for the correct image
            List<int> selectableIndices = correctImages
                .Select(img => Array.IndexOf(imagePaths[1], ((BitmapImage)img.Source).UriSource.ToString().Replace("pack://application:,,,", "")))
                .ToList();

            // Randomly select from the remaining pictures until you reachchose_picture_number
            List<int> remainingIndices = Enumerable.Range(0, imagePaths[1].Length)
                .Where(i => !selectableIndices.Contains(i) && imagePaths[1][i] != "EXO/2/Background.png")
                .OrderBy(x => rand.Next())
                .Take(chose_picture_number - selectableIndices.Count)
                .ToList();

            selectableIndices.AddRange(remainingIndices);

            // Randomize the order of the pictures
            selectableIndices = selectableIndices.OrderBy(x => rand.Next()).ToList();

            foreach (int index in selectableIndices)
            {
                System.Windows.Controls.Image img = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri(imagePaths[1][index], UriKind.Relative)),
                    Width = 80,
                    Height = 80,
                    Margin = new Thickness(10)
                };

                // Random generationXandYCoordinates to ensure that the image does not exceed the background image boundary
                double maxLeft = rightBound - img.Width;
                double maxTop = bottomBound - img.Height;

                double left = rand.NextDouble() * (maxLeft - leftBound) + leftBound;
                double top = rand.NextDouble() * (maxTop - topBound) + topBound;

                // Create borders and set image locations
                Border border = new Border
                {
                    BorderThickness = new Thickness(2),
                    BorderBrush = Brushes.Transparent, // No border at the beginning
                    Child = img
                };

                // set up Border Location
                Canvas.SetLeft(border, left);
                Canvas.SetTop(border, top);

                border.MouseLeftButtonDown += (sender, e) =>
                {
                    if (correctImages.Any(c => ((BitmapImage)c.Source).UriSource.ToString() == ((BitmapImage)img.Source).UriSource.ToString()))
                    {
                        if (selectedImages.Contains(img))
                        {
                            // The image has been selected, unselect
                            border.BorderBrush = Brushes.Transparent;
                            selectedImages.Remove(img);
                        }
                        else
                        {
                            // The picture is not selected, make the selection
                            border.BorderBrush = Brushes.Green;
                            selectedImages.Add(img);
                        }
                    }
                    else
                    {
                        if (selectedImages.Contains(img))
                        {
                            // The wrong picture has been selected, unselect
                            border.BorderBrush = Brushes.Transparent;
                            selectedImages.Remove(img);
                        }
                        else
                        {
                            // The wrong picture is not selected, select
                            border.BorderBrush = Brushes.Red;
                            selectedImages.Add(img);
                        }
                    }
                };

                selectionCanvas.Children.Add(border);
            }
        }

        private void confirmButton_Click2(object sender, RoutedEventArgs e)
        {
            bool isCorrect = false;

            if (train_mode == 2)
            {
                // use Tag Comparison of attributes
                bool allCorrectSelected = correctImages.All(ci => selectedImages.Any(si => (string)si.Tag == (string)ci.Tag));
                bool noIncorrectSelected = selectedImages.All(si => correctImages.Any(ci => (string)si.Tag == (string)ci.Tag));

                // The answer is considered correct only if all correct images are selected and the wrong images are not selected.
                isCorrect = allCorrectSelected && noIncorrectSelected;
                isCorrectAnswer = isCorrect;

                if (isCorrect)
                {
                    success_time++;
                    correctAnswers[level] += 1;
                }
                else
                {
                    fail_time++;
                    wrongAnswers[level] += 1;
                    int ignoredCount = correctImages.Count(ci => !selectedImages.Any(si => ((BitmapImage)si.Source).UriSource == ((BitmapImage)ci.Source).UriSource));
                    ignoreAnswers[level] += ignoredCount;
                    modeTextBlock.Text = "Sorry to answer wrong！";
                    modeTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                    foreach (var border in choicePanel.Children.OfType<Border>())
                    {
                        border.BorderBrush = Brushes.Transparent; // Reset border color
                    }

                    // Clear the list of pictures selected by the player
                    selectedImages.Clear(); // Clear the selected image list
                    DispatcherTimer timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(3); // Set the interval of 3 seconds
                    timer.Tick += (s, args) =>
                    {
                        //InitializeGame(); // Start the game again
                        modeTextBlock.Text = "";
                        modeTextBlock.Foreground = new SolidColorBrush(Colors.Black);
                        timer.Stop();
                    };
                    timer.Start();
                }

                // Update the recent game result queue（Mode 2）
                EndGame(); // Trigger end game logic
            }
            else if (train_mode == 3)
            {
                // Confirmation logic for mode 3
                isCorrect = selectedImages.Count == correctImages.Count &&
                            selectedImages.All(si => correctImages.Any(ci => ((BitmapImage)si.Source).UriSource == ((BitmapImage)ci.Source).UriSource));
                isCorrectAnswer = isCorrect;
                if (isCorrect)
                {
                    success_time++;
                    correctAnswers[level] += 1;
                    // modeTextBlock.Text = "Congratulations on getting right！";
                    modeTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                    Panel.SetZIndex(end, 999);
                    end.Visibility = Visibility.Visible;

                    confirm.Visibility = Visibility.Collapsed;
                    // Call EndGame method
                    // EndGame();
                }
                else
                {
                    fail_time++;
                    wrongAnswers[level] += 1;
                    int ignoredCount = correctImages.Count(ci => !selectedImages.Any(si => ((BitmapImage)si.Source).UriSource == ((BitmapImage)ci.Source).UriSource));
                    ignoreAnswers[level] += ignoredCount;

                    // Reinitialize game content
                    modeTextBlock.Text = "Sorry to answer wrong！";
                    modeTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                    DispatcherTimer timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(3); // Set the interval of 3 seconds
                    foreach (var selectedImage in selectedImages)
                    {
                        // Reset the border color of the image to cancel the selection
                        var border = selectedImage.Parent as Border;
                        if (border != null)
                        {
                            border.BorderBrush = Brushes.Transparent; // Restore to the unselected state
                        }
                    }
                    selectedImages.Clear(); // Clear the selected image list




                    timer.Tick += (s, args) =>
                    {
                        //InitializeGame(); // Start the game again
                        //modeTextBlock.Text = "The target object you need to find appears on the lower part of the screen, and find these objects from the upper picture.";

                        modeTextBlock.Text = "";
                        modeTextBlock.Foreground = new SolidColorBrush(Colors.Black);
                        timer.Stop();
                    };
                    timer.Start();
                }

                UpdateRecentResultsMode3(isCorrect);
                EndGame(); // Trigger end game logic
            }
            if (train_mode == 4) // Make sure this is the logic of pattern 4
            {
                isCorrect = true;
                // Get a list of the number of pictures entered by the user
                List<int> userInputCounts = new List<int>(userInputNumbers);

                // Initialize the correct image count
                Dictionary<string, int> correctImageCounts = new Dictionary<string, int>();

                foreach (var correctImage in correctImages)
                {
                    string imageUri = ((BitmapImage)correctImage.Source).UriSource.ToString().Replace("pack://application:,,,", "");

                    if (!correctImageCounts.ContainsKey(imageUri))
                    {
                        correctImageCounts[imageUri] = 0;
                    }

                    correctImageCounts[imageUri]++;
                }

                foreach (var correctImageUri in correctImageCounts.Keys)
                {
                    // Get the correct number of pictures that actually appear in the background image
                    int actualCount = selectionCanvas.Children.OfType<Border>()
                        .Count(border => ((BitmapImage)((System.Windows.Controls.Image)border.Child).Source).UriSource.ToString().Replace("pack://application:,,,", "") == correctImageUri);

                    // Check whether the number entered by the player matches the actual number
                    if (!userInputCounts.Contains(actualCount))
                    {
                        isCorrect = false;
                        break;
                    }

                    // Removes the number of matches to avoid duplicate matches
                    userInputCounts.Remove(actualCount);
                }
                isCorrectAnswer = isCorrect;
                if (isCorrect)
                {
                    success_time++;
                    correctAnswers[level] += 1;
                    modeTextBlock.Text = "Congratulations on getting right！";
                    modeTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                    Panel.SetZIndex(end, 999);
                    end.Visibility = Visibility.Visible;
                    confirm.Visibility = Visibility.Collapsed;
                    // Call EndGame method
                    // EndGame();
                }
                else
                {
                    if (fix_bug != 0)
                    {
                        fail_time++;
                        wrongAnswers[level] += 1;
                        int ignoredCount = correctImages.Count(ci => !selectedImages.Any(si => ((BitmapImage)si.Source).UriSource == ((BitmapImage)ci.Source).UriSource));
                        ignoreAnswers[level] += ignoredCount;
                        // Reinitialize game content
                        modeTextBlock.Text = "Sorry to answer wrong！";
                        modeTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                        DispatcherTimer timer = new DispatcherTimer();
                        timer.Interval = TimeSpan.FromSeconds(3); // Set the interval of 3 seconds
                        timer.Tick += (s, args) =>
                        {
                            //InitializeGame(); // Start the game again
                            // Reset user input
                            userInputNumbers.Clear();
                            userInput = string.Empty;
                            //UpdateTextBlock();
                            modeTextBlock.Text = "";
                            modeTextBlock.Foreground = new SolidColorBrush(Colors.Black);
                            // Stop the timer to prevent repeated triggering
                            timer.Stop();
                        };
                        timer.Start();
                    }
                }
                fix_bug++;
                // Update the latest game results
                UpdateRecentResultsMode4(isCorrect);

                // Adjust the difficulty
                AdjustDifficultyBasedOnResultsMode4();

                EndGame(); // Trigger end game logic
            }

            if (train_mode == 1)
            {
                SubmitInput();
            }
        }

        private void UpdateRecentResultsMode3(bool isCorrect)
        {
            if (recentResults.Count >= 5)
            {
                recentResults.Dequeue(); // Remove the earliest results
            }
            recentResults.Enqueue(isCorrect); // Add the current result

            if (recentResults.Count == 5)
            {
                //AdjustDifficultyBasedOnResultsMode3(); // Adjust the difficulty when the result reaches 5 times
            }

            LevelStatisticsAction?.Invoke(level, 18);

            int correctCount = recentResults.Count(result => result);
            int wrongCount = recentResults.Count(result => !result);

            // Update statistics on correct and errors
            RightStatisticsAction?.Invoke(correctCount, 5);
            WrongStatisticsAction?.Invoke(wrongCount, 5);
        }

        private void UpdateRecentResultsMode2(bool isCorrect)
        {
            if (recentResults.Count >= 5)
            {
                recentResults.Dequeue(); // Remove the earliest results
            }
            recentResults.Enqueue(isCorrect); // Add the current result

            if (recentResults.Count == 5)
            {
                AdjustDifficultyBasedOnResultsMode2();
            }
        }

        private void AdjustDifficultyBasedOnResultsMode2()
        {
            int correctCount = recentResults.Count(result => result);
            int wrongCount = recentResults.Count(result => !result);

            if (correctCount >= level_updata_threshold)
            {
                IncreaseDifficultyMode2();
            }
            else if (wrongCount >= level_updata_threshold)
            {
                DecreaseDifficultyMode2();
            }
        }

        private void IncreaseDifficultyMode2()
        {
            if (level < 18) // Assume that the maximum difficulty is level 18
            {
                level++;
                AdjustDifficultyMode2(level);
            }
        }

        // Reduce difficulty（Mode 2）
        private void DecreaseDifficultyMode2()
        {
            if (level > 1) // Assume that the minimum difficulty is level 1
            {
                level--;
                AdjustDifficultyMode2(level);
            }
        }

        private void ResetGameState()
        {
            // Reset all game status
            MainGrid.RowDefinitions.Clear();
            MainGrid.ColumnDefinitions.Clear();
            missingNumbers = new List<int>();
            userInputNumbers = new List<int>();
            userInput = string.Empty;
            UpdateTextBlock();
        }

        private void SetupGameMode1()
        {
            /*LJN
            Configure the path to feedback resources             
             */
            CorrectSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");
            // for Image Control loading picture Source
            CorrectImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Correct.png"), UriKind.RelativeOrAbsolute));
            ErrorImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Error.png"), UriKind.RelativeOrAbsolute));


            confirm.Visibility = Visibility.Collapsed;
            textBlock.Visibility = Visibility.Visible;
            myCanvas.Visibility = Visibility.Visible;
            if (isTrialPhase)
            {
                confirm.Visibility = Visibility.Collapsed;
            }
            // Clear the contents of the last game
            MainGrid.Children.Clear();

            // Check and remove `confirm1` Parent container
            if (confirm.Parent != null)
            {
                ((Grid)confirm.Parent).Children.Remove(confirm);
            }

            MainGrid2.Children.Add(confirm);

            // according to mode1_display_size Set the size of the digital display box
            double width = 1072;
            double height = 920;

            switch (mode1_display_size)
            {
                case 1:
                    width = width * 0.6;
                    height = height * 0.6;
                    break;
                case 2:
                    width = width * 0.7;
                    height = height * 0.7;
                    break;
                case 3:
                    width = width * 0.85;
                    height = height * 0.85;
                    break;
                case 4:
                    width = width * 1.0;  // Set the size manually
                    height = height * 1.0; // Set the size manually
                    break;
            }

            // Create a transparent rectangle with white borders
            Border gameAreaBorder = new Border
            {
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(5),
                Width = width,
                Height = height,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            MainGrid.Children.Add(gameAreaBorder);

            // Randomly generate a list of numbers and remove several numbers
            List<int> numbers = Enumerable.Range(minnumber, maxnumber).ToList();
            AllNumbers = new List<int>(numbers); ;//Store all numbers,Prepare for subsequent selected numbers to appear
            missingNumbers = RemoveRandomNumbers(numbers);
            // Used to store occupied locations（use HashSet Improve efficiency）
            HashSet<Rect> usedPositions = new HashSet<Rect>();
            // Display the remaining numbers and randomly distributed in the display area
            Canvas numbersCanvas = new Canvas();
            Random rand = new Random();

            // Assumptions gameAreaBorder Already exists and has its width and height
            double borderLeft = 0;
            double borderTop = 0;
            double borderRight = width;
            double borderBottom = height;

            foreach (int number in AllNumbers)
            {
                TextBlock numberText = new TextBlock
                {
                    Text = number.ToString(),
                    FontSize = Math.Sqrt(width * height / (maxnumber - minnumber - miss_number) * 0.5) * 0.6, // Make the size of the number not too small
                    Foreground = Brushes.Black,
                    Visibility = Visibility.Visible
                };

                if (numbers.Contains(number)) { numberText.Visibility = Visibility.Visible; }
                else { numberText.Visibility = Visibility.Hidden; numberText.Foreground = Brushes.Orange; } // If it is a missing number, it will be hidden

                NumTextDict[number] = numberText;//Create numbers totextblockIndex of

                // Try to find a random non-overlapping position
                bool positionFound = false;
                double left = 0, top = 0;
                int attempts = 0;  // Limit the number of attempts to prevent infinite loops
                while (!positionFound && attempts < 10000)
                {
                    // Generate a random position
                    left = rand.Next(0, Math.Max(1, (int)(width - numberText.FontSize)));
                    top = rand.Next(0, Math.Max(1, (int)(height - numberText.FontSize)));

                    // Create a new rectangle to represent numberText Location
                    Rect newRect = new Rect(left, top, numberText.FontSize, numberText.FontSize);

                    // Check if it overlaps with an existing location
                    bool overlapsWithExisting = usedPositions.Any(existingRect => existingRect.IntersectsWith(newRect));

                    // Check if it is exceeded gameAreaBorder The boundary of
                    bool overlapsWithBorder = newRect.Left < borderLeft || newRect.Top < borderTop ||
                                              newRect.Right > borderRight || newRect.Bottom > borderBottom;

                    // If there is no overlap and no boundary is exceeded, the position is valid
                    if (!overlapsWithExisting && !overlapsWithBorder)
                    {
                        usedPositions.Add(newRect);  // If there is no overlap, record the position
                        positionFound = true;
                    }

                    attempts++;  // Increase the number of attempts
                }

                // If the right location is found
                if (positionFound)
                {
                    // Set the location
                    Canvas.SetLeft(numberText, left);
                    Canvas.SetTop(numberText, top);
                    numbersCanvas.Children.Add(numberText);
                }
            }

            gameAreaBorder.Child = numbersCanvas;
        }

        private List<int> RemoveRandomNumbers(List<int> numbers)
        {
            Random rand = new Random();
            List<int> removedNumbers = new List<int>();
            while (removedNumbers.Count < miss_number)
            {
                int index = rand.Next(numbers.Count);
                if (numbers[index] == minnumber || numbers[index] == maxnumber) { continue; }//Make sure that the minimum and maximum values ​​are not removed
                removedNumbers.Add(numbers[index]);
                numbers.RemoveAt(index);
            }

            return removedNumbers.OrderBy(n => n).ToList(); // Returns the removed number（Sort by）
        }


        // The event handling function is pressed by a number button
        private void OnNumberButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                int number = int.Parse(button.Content.ToString());
                userInputNumbers.Add(number);
                userInput += button.Content.ToString();
                UpdateTextBlock();
            }
        }

        // "✔" Button press event handling function
        private void OnSubmitButtonClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(userInput))
            {
                int number = int.Parse(userInput);//Get user input
                userInputNumbers.Add(number);//Add to the results
                userInput = string.Empty;
                UpdateTextBlock();
                SubmitInput();//Mode 1 Submit input



            }
        }
        // "confirm" The event handler function is pressed by the button,OnBackButtonClickFunction
        private void confirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (train_mode == 4) // Make sure this is the logic of pattern 4
            {
                // Get a list of the number of pictures entered by the user
                List<int> userInputCounts = new List<int>(userInputNumbers);

                // Initialize the correct image count
                Dictionary<string, int> correctImageCounts = new Dictionary<string, int>();

                foreach (var correctImage in correctImages)
                {
                    string imageUri = ((BitmapImage)correctImage.Source).UriSource.ToString().Replace("pack://application:,,,", "");

                    if (!correctImageCounts.ContainsKey(imageUri))
                    {
                        correctImageCounts[imageUri] = 0;
                    }

                    correctImageCounts[imageUri]++;
                }

                bool isCorrect = true;

                foreach (var correctImageUri in correctImageCounts.Keys)
                {
                    // Get the correct number of pictures that actually appear in the background image
                    int actualCount = selectionCanvas.Children.OfType<Border>()
                        .Count(border => ((BitmapImage)((System.Windows.Controls.Image)border.Child).Source).UriSource.ToString().Replace("pack://application:,,,", "") == correctImageUri);

                    // Check whether the number entered by the player matches the actual number
                    if (!userInputCounts.Contains(actualCount))
                    {
                        isCorrect = false;
                        break;
                    }

                    // Removes the number of matches to avoid duplicate matches
                    userInputCounts.Remove(actualCount);
                }

                if (isCorrect)
                {
                    success_time++;
                }
                else
                {
                    fail_time++;
                }

                // Update the latest game results
                UpdateRecentResultsMode4(isCorrect);

                // Adjust the difficulty
                AdjustDifficultyBasedOnResultsMode4();

                EndGame(); // Trigger end game logic
            }
            else
            {
                SubmitInput();
            }
        }

        private void UpdateRecentResultsMode4(bool isCorrect)
        {
            if (recentResults.Count >= 5)
            {
                recentResults.Dequeue(); // Remove the earliest results
            }
            recentResults.Enqueue(isCorrect); // Add the current result
        }

        private void AdjustDifficultyBasedOnResultsMode4()
        {
            // First check whether the results have reached 5 times in the last 5 times
            if (recentResults.Count < 5)
            {
                return; // If the result is less than 5 times, the difficulty will not be updated
            }

            // Calculate the correct and errors in the last 5 times
            int correctCount = recentResults.Count(result => result);
            int wrongCount = recentResults.Count(result => !result);

            if (correctCount >= level_updata_threshold)
            {
                IncreaseDifficultyMode4();
            }
            else if (wrongCount >= level_updata_threshold)
            {
                DecreaseDifficultyMode4();
            }
        }

        private void IncreaseDifficultyMode4()
        {
            if (level < 18) // Assume that the maximum difficulty is level 18
            {
                level++;
                AdjustDifficultyMode4(level);
            }
        }

        private void DecreaseDifficultyMode4()
        {
            if (level > 1) // Assume that the minimum difficulty is level 1
            {
                level--;
                AdjustDifficultyMode4(level);
            }
        }

        // "←" Button press event handling function, new function: delete the previous input number
        private void OnBackButtonClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(userInput))
            {
                // Delete the last number
                userInputNumbers.RemoveAt(userInputNumbers.Count - 1);
                userInput = userInput.Substring(0, userInput.Length - 1);
                UpdateTextBlock();
            }
        }

        private void UpdateTextBlock()
        {
            displayTextBlock.Text = userInput;
        }

        private void SubmitInput()
        {
            //Compare the number entered by the user this time with the missing number list
            int InputNum = userInputNumbers.LastOrDefault(); ;//Get the last input
            bool isCorrect = missingNumbers.Contains(InputNum);
            if (AllNumbers.Contains(InputNum))//The number entered by the user is within range
            {
                if (isCorrect)
                {//If the number entered by the user this time is one of the missing numbers, it will be displayed in the number area on the right
                    NumTextDict[InputNum].Visibility = Visibility.Visible;//The numbers are visible
                    missingNumbers.Remove(InputNum);//Remove this number from the missing number list
                }
                else
                {
                    NumTextDict[InputNum].Foreground = Brushes.Red;//Red as a warning
                    ErrorCount--;
                }
            }
            else
            {//If it is not within the scope, it is wrong
                ErrorCount--;
                isCorrect = false;
            }
            GroupResultCheck(isCorrect);//Do you need to determine whether the entire question is correct or not?
        }

        private void GroupResultCheck(bool IsNumFeedBack)//Complete the results of this question to determine whether the entire question is right or wrong
        {//IsNumFeedBackIt is used to judge whether a single number needs to be given feedback, which can prevent overlapping with the feedback of the entire question.
            bool IfChecked = false;//Set it to determine whether the entire question has been completed
            if (ErrorCount <= 0)
            {
                ErrorCount = ErrorLimit;
                IfChecked = true;
                // Reinitialize game content
                PlayWav(ErrorSoundPath);
                ShowFeedbackImage(ErrorImage);
                IntervalTimer.Interval = TimeSpan.FromSeconds(ErrorInterval);
                //modeTextBlock.Text = "Sorry to answer wrong！";
                //modeTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(3); // Set the interval of 3 seconds
                timer.Tick += (s, args) =>
                {
                    //InitializeGame(); // Start the game again
                    // Reset user input
                    userInputNumbers.Clear();
                    userInput = string.Empty;
                    UpdateTextBlock();
                    //modeTextBlock.Text = "Find missing numbers in the range of numbers and enter them one by one from small to large";
                    modeTextBlock.Foreground = new SolidColorBrush(Colors.Black);
                    // Stop the timer to prevent repeated triggering
                    timer.Stop();
                };
                OverLayGrid.IsEnabled = false;
                IntervalTimer.Start();
                timer.Start(); // Start the timer

            }
            else if (missingNumbers.Count <= 0)
            {
                IfChecked = true;isCorrectAnswer = true;
                PlayWav(CorrectSoundPath);
                ShowFeedbackImage(CorrectImage);
                //modeTextBlock.Text = "Congratulations on getting right！";
                //modeTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                myCanvas.IsEnabled = false;//Prevent users from subsequent input
                IntervalTimer.Interval = TimeSpan.FromSeconds(CorrectInterval);
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(2); // Set the interval of 3 seconds
                timer.Tick += (s, args) =>
                {
                    //modeTextBlock.Text = "Find missing numbers in the range of numbers and enter them one by one from small to large";
                    modeTextBlock.Text = "";
                    modeTextBlock.Foreground = new SolidColorBrush(Colors.Black);
                    // Stop the timer to prevent repeated triggering
                    timer.Stop();
                };


                timer.Start(); // Start the timer
                Panel.SetZIndex(end, 999);
                end.Visibility = Visibility.Visible;
                confirm.Visibility = Visibility.Collapsed;
                // Call EndGame method
                // EndGame();
            }

            if(IfChecked==false)
            {//It means that only requires a single number of judgment feedback
                if (IsNumFeedBack)
                {
                    PlayWav(CorrectSoundPath);
                    ShowFeedbackImage(CorrectImage);
                }
                else
                {
                    PlayWav(ErrorSoundPath);
                    ShowFeedbackImage(ErrorImage);
                }
            }

        }

        private void UpdateRecentResults(bool isCorrect)
        {
            if (recentResults.Count >= 5)
            {
                recentResults.Dequeue(); // Remove the earliest results
            }
            recentResults.Enqueue(isCorrect); // Add the current result

            if (recentResults.Count == 5)
            {
                AdjustDifficultyBasedOnResults();
            }
        }

        private void AdjustDifficultyBasedOnResults()
        {
            int correctCount = recentResults.Count(result => result);
            int wrongCount = recentResults.Count(result => !result);

            if (correctCount >= level_updata_threshold)
            {
                IncreaseDifficulty();
            }
            else if (wrongCount >= level_updata_threshold)
            {
                DecreaseDifficulty();
            }
        }

        private void IncreaseDifficulty()
        {
            if (level < 18) // Assume that the maximum difficulty is level 18
            {
                level++;
                AdjustDifficulty(level);
            }
        }

        private void DecreaseDifficulty()
        {
            if (level > 1) // Assume that the minimum difficulty is level 1
            {
                level--;
                AdjustDifficulty(level);
            }
        }

        private void EndGame()
        {
            if (isTrialPhase)
            {
                if (isCorrectAnswer)
                {
                    // The user answers correctly and enters the next trial module
                    currentTrialModeIndex++;
                    if (currentTrialModeIndex < trialModes.Count)
                    {
                        // There is also a mode that needs to be tried, continue to try the next mode
                        train_mode = trialModes[currentTrialModeIndex];
                        InitializeGame();
                    }
                    else
                    {
                        // All modes have been tried and displayed“Enter the module”Button
                        isTrialPhase = false;
                        Panel.SetZIndex(end, 999);
                        end.Visibility = Visibility.Visible;
                        end.Content = "Enter the game"; // Modify button content
                        confirm.Visibility = Visibility.Collapsed;
                        modeTextBlock.Text = "All modes are finished";
                        OnGameBegin();//Start the game directly
                    }
                }
                else
                {
                    // The user did not answer correctly and restarted the current module
                    //modeTextBlock.Text = "Sorry to answer wrong！";
                    //modeTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                    //DispatcherTimer timer = new DispatcherTimer();
                    //timer.Interval = TimeSpan.FromSeconds(3); // Set the interval of 3 seconds
                    //timer.Tick += (s, args) =>
                    //{
                    //    InitializeGame(); // Restart the current module again
                    //    timer.Stop();
                    //};
                    //timer.Start();
                }
            }
            else
            {
                // Formal game logic, start the game again
                OnGameBegin();
            }
        }


        private void AdjustDifficulty(int level)
        {
            switch (level)
            {
                case 1:
                    maxnumber = 5;
                    miss_number = 1;
                    mode1_display_size = 1; // Small
                    break;
                case 2:
                    maxnumber = 7;
                    miss_number = 2;
                    mode1_display_size = 1; // Small
                    break;
                case 3:
                    maxnumber = 8;
                    miss_number = 2;
                    mode1_display_size = 1; // Small
                    break;
                case 4:
                    maxnumber = 9;
                    miss_number = 3;
                    mode1_display_size = 1; // Small
                    break;
                case 5:
                    maxnumber = 9;
                    miss_number = 3;
                    mode1_display_size = 1; // Small
                    break;
                case 6:
                    maxnumber = 12;
                    miss_number = 3;
                    mode1_display_size = 2; // middle
                    break;
                case 7:
                    maxnumber = 14;
                    miss_number = 3;
                    mode1_display_size = 2; // middle
                    break;
                case 8:
                    maxnumber = 16;
                    miss_number = 4;
                    mode1_display_size = 2; // middle
                    break;
                case 9:
                    maxnumber = 18;
                    miss_number = 4;
                    mode1_display_size = 2; // middle
                    break;
                case 10:
                    maxnumber = 20;
                    miss_number = 4;
                    mode1_display_size = 3; // big
                    break;
                case 11:
                    maxnumber = 24;
                    miss_number = 5;
                    mode1_display_size = 3; // big
                    break;
                case 12:
                    maxnumber = 28;
                    miss_number = 5;
                    mode1_display_size = 3; // big
                    break;
                case 13:
                    maxnumber = 30;
                    miss_number = 5;
                    mode1_display_size = 4; // full screen
                    break;
                case 14:
                    maxnumber = 35;
                    miss_number = 5;
                    mode1_display_size = 4; // full screen
                    break;
                case 15:
                    maxnumber = 38;
                    miss_number = 6;
                    mode1_display_size = 4; // full screen
                    break;
                case 16:
                    maxnumber = 40;
                    miss_number = 6;
                    mode1_display_size = 4; // full screen
                    break;
                case 17:
                    maxnumber = 45;
                    miss_number = 7;
                    mode1_display_size = 4; // full screen
                    break;
                case 18:
                    maxnumber = 50;
                    miss_number = 8;
                    mode1_display_size = 4; // full screen
                    break;
                default:
                    maxnumber = 5;
                    miss_number = 1;
                    mode1_display_size = 1; // Small
                    break;
            }
        }

        private void AdjustDifficultyMode2(int level)
        {
            switch (level)
            {
                case 1:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    break;
                case 2:
                    right_picture_number = 2;
                    chose_picture_number = 6;
                    break;
                case 3:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    break;
                case 4:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    break;
                case 5:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    break;
                case 6:
                    right_picture_number = 2;
                    chose_picture_number = 6;
                    break;
                case 7:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    break;
                case 8:
                    right_picture_number = 2;
                    chose_picture_number = 6;
                    break;
                case 9:
                    right_picture_number = 3;
                    chose_picture_number = 6;
                    break;
                case 10:
                    right_picture_number = 3;
                    chose_picture_number = 8;
                    break;
                case 11:
                    right_picture_number = 3;
                    chose_picture_number = 6;
                    break;
                case 12:
                    right_picture_number = 3;
                    chose_picture_number = 8;
                    break;
                case 13:
                    right_picture_number = 3;
                    chose_picture_number = 6;
                    break;
                case 14:
                    right_picture_number = 3;
                    chose_picture_number = 8;
                    break;
                case 15:
                    right_picture_number = 3;
                    chose_picture_number = 6;
                    break;
                case 16:
                    right_picture_number = 3;
                    chose_picture_number = 8;
                    break;
                case 17:
                    right_picture_number = 3;
                    chose_picture_number = 12; // 4x3
                    break;
                case 18:
                    right_picture_number = 3;
                    chose_picture_number = 18; // 6x3
                    break;
                default:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    break;
            }
        }

        private void AdjustDifficultyMode3(int level)
        {
            switch (level)
            {
                case 1:
                    right_picture_number = 2;
                    chose_picture_number = 3; // 2 correct pictures + 1 interfering picture
                    break;
                case 2:
                    right_picture_number = 2;
                    chose_picture_number = 4; // 2 correct pictures + 2 interfering pictures
                    break;
                case 3:
                    right_picture_number = 3;
                    chose_picture_number = 5; // 3 correct pictures + 2 interfering pictures
                    break;
                case 4:
                    right_picture_number = 3;
                    chose_picture_number = 6; // 3 correct pictures + 3 interfering pictures
                    break;
                case 5:
                    right_picture_number = 3;
                    chose_picture_number = 7; // 3 correct pictures + 4 interfering pictures
                    break;
                case 6:
                    right_picture_number = 4;
                    chose_picture_number = 8; // 4 correct pictures + 4 interfering pictures
                    break;
                case 7:
                    right_picture_number = 4;
                    chose_picture_number = 9; // 4 correct pictures + 5 interfering pictures
                    break;
                case 8:
                    right_picture_number = 4;
                    chose_picture_number = 10; // 4 correct pictures + 6 interfering pictures
                    break;
                case 9:
                    right_picture_number = 5;
                    chose_picture_number = 11; // 5 correct pictures + 6 interfering pictures
                    break;
                case 10:
                    right_picture_number = 5;
                    chose_picture_number = 12; // 5 correct pictures + 7 interfering pictures
                    break;
                case 11:
                    right_picture_number = 6;
                    chose_picture_number = 13; // 6 correct pictures + 7 interfering pictures
                    break;
                case 12:
                    right_picture_number = 6;
                    chose_picture_number = 14; // 6 correct pictures + 8 interfering pictures
                    break;
                case 13:
                    right_picture_number = 7;
                    chose_picture_number = 15; // 7 correct pictures + 8 interfering pictures
                    break;
                case 14:
                    right_picture_number = 7;
                    chose_picture_number = 16; // 7 correct pictures + 9 interfering pictures
                    break;
                case 15:
                    right_picture_number = 8;
                    chose_picture_number = 17; // 8 correct pictures + 9 interfering pictures
                    break;
                case 16:
                    right_picture_number = 8;
                    chose_picture_number = 18; // 8 correct pictures + 10 interfering pictures
                    break;
                case 17:
                    right_picture_number = 9;
                    chose_picture_number = 19; // 9 correct pictures + 10 interfering pictures
                    break;
                case 18:
                    right_picture_number = 10;
                    chose_picture_number = 20; // 10 correct pictures + 10 interfering pictures
                    break;
                default:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    break;
            }
        }

        private void AdjustDifficultyMode4(int level)
        {
            switch (level)
            {
                case 1:
                    right_picture_number = 1; // Types of objects to count
                    max_right_display = 2;
                    mini_right_display = 2;
                    mislead_picture_display_number = 4; // Unrelated items
                    break;
                case 2:
                    right_picture_number = 1;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 5;
                    break;
                case 3:
                    right_picture_number = 1;
                    max_right_display = 4;
                    mini_right_display = 4;
                    mislead_picture_display_number = 6;
                    break;
                case 4:
                    right_picture_number = 1;
                    max_right_display = 5;
                    mini_right_display = 5;
                    mislead_picture_display_number = 7;
                    break;
                case 5:
                    right_picture_number = 1;
                    max_right_display = 6;
                    mini_right_display = 6;
                    mislead_picture_display_number = 8;
                    break;
                case 6:
                    right_picture_number = 2;
                    max_right_display = 4;
                    mini_right_display = 4;
                    mislead_picture_display_number = 6;
                    break;
                case 7:
                    right_picture_number = 2;
                    max_right_display = 5;
                    mini_right_display = 5;
                    mislead_picture_display_number = 7;
                    break;
                case 8:
                    right_picture_number = 2;
                    max_right_display = 6;
                    mini_right_display = 3;
                    mislead_picture_display_number = 8;
                    break;
                case 9:
                    right_picture_number = 2;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 9;
                    break;
                case 10:
                    right_picture_number = 2;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 10;
                    break;
                case 11:
                    right_picture_number = 3;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 7;
                    break;
                case 12:
                    right_picture_number = 3;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 8;
                    break;
                case 13:
                    right_picture_number = 3;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 9;
                    break;
                case 14:
                    right_picture_number = 3;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 10;
                    break;
                case 15:
                    right_picture_number = 3;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 11;
                    break;
                case 16:
                    right_picture_number = 3;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 15;
                    break;
                case 17:
                    right_picture_number = 3;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 20;
                    break;
                case 18:
                    right_picture_number = 3;
                    max_right_display = 3;
                    mini_right_display = 3;
                    mislead_picture_display_number = 25;
                    break;
                default:
                    right_picture_number = 1;
                    max_right_display = 2;
                    mini_right_display = 2;
                    mislead_picture_display_number = 4;
                    break;
            }
        }


        private void end_Click(object sender, RoutedEventArgs e)
        {
            // Related logic for starting answering questions
            EndGame();
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

                        MainGrid.Visibility = Visibility.Collapsed;
                        MainGrid2.Visibility = Visibility.Collapsed;

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
                        MainGrid.Visibility = Visibility.Collapsed;
                        MainGrid2.Visibility = Visibility.Collapsed;

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
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        MainGrid2.Visibility = Visibility.Visible;
                        MainGrid.Visibility = Visibility.Visible;
                        // Force focus to remain in the window
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Collapsed;

                        //LJN, instructing the delegate in the explanation module
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("Find missing numbers in the range of numbers and enter them one by one from small to large.");//Add code, call function, display the text under the digital person
                        //LJN
                    }
                    break;
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)//Bind custom mouse cursor and default mouse cursor
        {
            Point position = e.GetPosition(this);
            Canvas.SetLeft(CustomCursor, position.X);
            Canvas.SetTop(CustomCursor, position.Y);
            // Get Canvas The boundary of
            double canvasLeft = Canvas.GetLeft(CustomCursor);
            double canvasTop = Canvas.GetTop(CustomCursor);
            double canvasWidth = this.ActualWidth;
            double canvasHeight = this.ActualHeight;

            // Get CustomCursor width and height
            double cursorWidth = CustomCursor.Width;
            double cursorHeight = CustomCursor.Height;

            // if CustomCursor More than Canvas boundaries, cut
            if (canvasLeft + cursorWidth > canvasWidth)
            {
                Canvas.SetLeft(CustomCursor, canvasWidth - cursorWidth); // Limit to the right boundary
            }
            if (canvasTop + cursorHeight > canvasHeight)
            {
                Canvas.SetTop(CustomCursor, canvasHeight - cursorHeight); // Limit to the lower boundary
            }
            if (canvasLeft < 0)
            {
                Canvas.SetLeft(CustomCursor, 0); // Limit to the left border
            }
            if (canvasTop < 0)
            {
                Canvas.SetTop(CustomCursor, 0); // Limit to the upper boundary
            }

            // if CustomCursor Exceeded Canvas Range, cut out the excess
            Rect clipRect = new Rect(0, 0, canvasWidth, canvasHeight);
            CustomCursor.Clip = new RectangleGeometry(clipRect);  // Crop areas using rectangles
        }

        private SoundPlayer soundPlayer; // Used to sing
        public string ErrorSoundPath;//The wrong sound file path, inOnStartAsync()Medium configuration
        public string CorrectSoundPath;//The correct sound file path is inOnStartAsync()Medium configuration
        private int StopDurations = 2000; // Stop time,ms

        private string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory; // The absolute path to the current project
        private string ResourcesPath = System.IO.Path.Combine("Resources", "Word memory");//The fixed path used here is the memory path of word, which can be changed later.



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

        private async void ShowFeedbackImage(System.Windows.Controls.Image image)//StopDurationsThe unit isms
        {//Image showing feedback
            image.Visibility = Visibility.Visible;

            // Delay the specified time（For example, 1 second）
            await Task.Delay(StopDurations);

            image.Visibility = Visibility.Collapsed;
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
