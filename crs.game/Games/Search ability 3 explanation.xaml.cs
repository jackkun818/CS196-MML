using crs.core.DbModels;
using crs.core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Media;
using System.Windows.Controls.Primitives;
using Spire.Additions.Xps.Schema.Mc;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace crs.game.Games
{
    /// <summary>
    /// EXO.xaml Interaction logic
    /// </summary>
    public partial class Search ability 3 explanation : BaseUserControl
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

        /*
         Parameters that need to be read from the database and can be changed by the user
         */
        private bool IfLimitTime = false;//Is the answer time limit
        private int LevelUp = 5;//Number of questions to do correctly with level improvement
        private int LevelDown = 5;//The number of questions to be done correctly after level reduction
        private int max_time = 1; // Total treatment time
        private bool IfVisualFeedBack = true;//Visual feedback
        private bool IfAudioFeedBack = true;//Voice feedback
        private int Level = 1; // Current game difficulty level
        /*
         Parameters during game operation
         */

        //FindMode
        private int FindMode = 1;//It's just used to refer to it during early debugging, which is more intuitive than the numbers 3 or 4.

        private int Complexity = 1;//FindNeed notCountComplexity in the difficulty level list of modes, 1 simple, 2 medium, 3 complex, 4 very complex
        private int right_picture_number = 4; // The correct number of pictures displayed
        private int chose_picture_number = 6; // The total number of selectable pictures displayed, including correct and incorrect

        private Dictionary<Border, bool> BorderStatusDict = new Dictionary<Border, bool>();//FindMode: used to storeBorderSelected state to determine whether the frame color needs to be changed；

        private bool IfFoundButtonClicked = false;
        //FindCountMode
        private int FindAndCountMode = 2;//These two variables are only used to refer to during early debugging, and are more intuitive than numbers 3 or 4

        private int max_right_display = 2; // The maximum number of correct pictures to display,That is, the total number of objects to be counted
        private int min_right_display = 1; // Minimum number of correct pictures to display
        private int mislead_picture_display_number = 4; // The total number of displays in the interfering picture

        private List<Border> CountBordersList = new List<Border>();//Use one of the following items to countListStore it so that patients know one by one what graphics they want to count
        private int IndexOfCount = 0;//The patient is counting the number of things

        private int ErrorCount = 0;
        private int ErrorLimit = 2;//At most, how many times can the patient enter incorrectly in a question?

        private Dictionary<Image, Border> ImageToBordersDict = new Dictionary<Image, Border>();//passImageTo index all corresponding parts of the background board aboveborder, convenientborderChange of color 

        private Dictionary<Image, int> correctImagesCount = new Dictionary<Image, int>();//Correct pictures and corresponding numbers

        //Shared
        private bool IfFisrtSet = false;//Some components need to beloadAfter finishing, display the content to achieve dynamic adjustment

        private string BackGroundPath = "EXO/2/Background.png";//The background path is replaced by a variable, so it is easy to modify

        private List<Image> correctImages; // List of correct pictures
        private List<Image> selectedImages; // User selected image

        private int[] correctAnswers;
        private int[] wrongAnswers;
        private int[] ignoreAnswers;

        private int MaxLevel = 18;//The maximum level that can be achieved
        private int MinLevel = 1;//Minimum level that can be achieved

        private DispatcherTimer gameTimer; // Global timer
        private TimeSpan timeRemaining; // time left

        private DispatcherTimer PlayTimer;//Answer time limit, checkedIfLimitTimeOnly effective
        private int BaseTimeLimit = 60;//The minimum level limit time, seconds
        private int TimeLimitInterval = 5;//Time limit gap between different levels, seconds, arithmetic sequences
        private int PlayTime = 0;//The limited answer time is set according to the level changes

        private DispatcherTimer IntervalTimer;//The interval timer between questions
        private int CorrectInterval = 3;//Answer 3sinterval
        private int ErrorInterval = 5;//Wrong answer 5sinterval

        private bool is_gaming = false;
        private int success_time = 0;//Number of answers correctly
        private int fail_time = 0;

        private bool IfLevelDown = false;//Is it necessary to lower the level
        private bool IfLevelUp = false;//Is it necessary to upgrade level

        private string userInput; // Stores the numbers entered by the user

        private bool is_finish = false;

        //------------------Report parameters-------------------------------------
        private int train_mode = 1; // Game mode, 1, 2, 3, 4	1.	Pattern 1: Find missing numbers in the range of numbers and enter them one by one from small to large. This pattern usually involves the user identifying and entering missing numbers.
                                    //Mode 2: Identify different shapes that are superimposed on each other and select them from the bottom of the screen. This pattern involves the user's need to find the correct shape from the superimposed shape.
                                    //Mode 4: Count out and enter the number of times each correct object appears in the picture.

        private int CurrentPage = 1;//Where is the current page

        public Search ability 3 explanation(int TrainMode=3)//Ontology reading training mode can be passed
        {
            InitializeComponent();

            //Then initialize some values

            train_mode = TrainMode; // Game mode, 1, 2 or 3
            PageSwitchSet();//Load the page first
            Level = 1; // Current game difficulty level

            IntervalTimer = new DispatcherTimer();
            IntervalTimer.Tick += IntervalTimer_Tick;

            /*LJN
            Configure the path to feedback resources             
             */
            CorrectSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");
            // for Image Control loading picture Source
            CorrectImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Correct.png"), UriKind.RelativeOrAbsolute));
            ErrorImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Error.png"), UriKind.RelativeOrAbsolute));

            this.Cursor = Cursors.None; // Hide the default cursor
            CustomCursor.Visibility = Visibility.Visible; // Show custom cursor
            MouseMove += Window_MouseMove; // subscription MouseMove event
            CustomCursor.Width = 65; // Adjust to the width you want
            CustomCursor.Height = 65; // Adjust to the height you want
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

        public static Window GetTopLevelWindow(UserControl userControl)
        {
            DependencyObject current = userControl;
            while (current != null && !(current is Window))
            {
                current = VisualTreeHelper.GetParent(current);
            }

            return current as Window;
        }

        /*FindMode*/
        private async void SetupFindMode()
        {
            // Initialize the correct picture list
            correctImages = new List<Image>();
            selectedImages = new List<Image>();

            // Randomly select the correct image and add it tocorrectImagesIn the list
            Random rand = new Random();
            List<int> correctIndices = new List<int>();
            while (correctIndices.Count < right_picture_number)
            {
                int index = rand.Next(imagePaths[1].Length);

                if (imagePaths[1][index] != BackGroundPath && !correctIndices.Contains(index))
                {//Select the correct picture
                    correctIndices.Add(index);
                    Image img = new Image
                    {
                        Source = new BitmapImage(new Uri(imagePaths[1][index], UriKind.Relative)),
                    };
                    correctImages.Add(img);
                }
            }
            if (IfFisrtSet == false)
            {//The first time you draw content, you need to wait for componentsloadOnly after finishing can you draw
                SelectionAreaGrid.Loaded += (s, e) => DisplaySelectableImages(SelectionCanvas, rand);//existSelectionCanvasShow pictures,Because dynamic adjustment is required,loadShow again after finish
                TargetAreaGrid.Loaded += (s, e) => DisplayCorrectImages(TargetItemGrid);//existTargetItemGridShow pictures to be selected,Because dynamic adjustment is required,loadCall it again afterward
                BackGround.Loaded += BackGround_Loaded;                //Draw the background
                IfFisrtSet = true;
            }
            else
            {//Not the first time, it's called directly
                BackGround.Children.Clear();
                Image backgroundImage = new Image
                {
                    Source = new BitmapImage(new Uri(BackGroundPath, UriKind.Relative)),
                    //Stretch = Stretch.Uniform,
                    Stretch = Stretch.Fill,
                    Width = BackGround.ActualWidth,  // Adjust the width of the background image
                    Height = BackGround.ActualHeight,  // Adjust the height of the background image
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                BackGround.Children.Add(backgroundImage);//Add toBackGroundGridmiddle
                DisplaySelectableImages(SelectionCanvas, rand);
                DisplayCorrectImages(TargetItemGrid);
            }
        }

        private void BackGround_Loaded(object sender, RoutedEventArgs e)//When component loadsBackGround，LoadGo up
        {//Take the background image as large as possible
            // Create background image
            BackGround.Children.Clear();
            Image backgroundImage = new Image
            {
                Source = new BitmapImage(new Uri(BackGroundPath, UriKind.Relative)),
                //Stretch = Stretch.Uniform,
                Stretch = Stretch.Fill,
                Width = BackGround.ActualWidth,  // Adjust the width of the background image
                Height = BackGround.ActualHeight,  // Adjust the height of the background image
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            BackGround.Children.Add(backgroundImage);//Add toBackGroundGridmiddle
        }

        private void DisplayCorrectImages(UniformGrid TargetItemGrid)//Show the following line of pictures for patients
        {
            TargetItemGrid.Children.Clear();
            TargetItemGrid.Columns = correctImages.Count;//Make sure to display it in one line
            double Size = Math.Min(TargetItemGrid.ActualWidth / correctImages.Count - 10, TargetItemGrid.ActualHeight - 10);//-10 is to keep a little margin to ensure complete display
            foreach (var img in correctImages)
            {
                Image correctImg = new Image
                {
                    Source = img.Source,
                    Width = Size,
                    Height = Size,//Make sure it's square
                    Margin = new Thickness(10)
                };
                TargetItemGrid.Children.Add(correctImg);//Add to thisuniformgridinside
            }


        }

        private void DisplaySelectableImages(Canvas selectionCanvas, Random rand)//Show the above pictures that patients click to select
        {
            selectionCanvas.Children.Clear();//Clear it before each call
            //Get boundary information
            var selectionAreaGrid = (Grid)selectionCanvas.Parent; // Get SelectionCanvas parent control, i.e.SelectionAreaGrid
            var gridBounds = selectionAreaGrid.TransformToAncestor(this)
                .TransformBounds(new Rect(0, 0, selectionAreaGrid.ActualWidth, selectionAreaGrid.ActualHeight));

            // Boundary information
            double leftBound = gridBounds.Left;//One moreTransform
            double rightBound = gridBounds.Right;
            double topBound = gridBounds.Top;
            double bottomBound = gridBounds.Bottom;

            // Create a list of indexes for selectable images and first add the index for the correct image
            List<int> selectableIndices = correctImages
                .Select(img => Array.IndexOf(imagePaths[1], ((BitmapImage)img.Source).UriSource.ToString().Replace("pack://application:,,,", "")))
                .ToList();

            // Randomly select from the remaining pictures until you reachchose_picture_number
            List<int> remainingIndices = Enumerable.Range(0, imagePaths[1].Length)
                .Where(i => !selectableIndices.Contains(i) && imagePaths[1][i] != BackGroundPath)
                .OrderBy(x => rand.Next())
                .Take(chose_picture_number - selectableIndices.Count)//TakeOnly the first few will be selected, so it will not be repeated unless the number is insufficient
                .ToList();

            selectableIndices.AddRange(remainingIndices);

            // Randomize the order of the pictures
            selectableIndices = selectableIndices.OrderBy(x => rand.Next()).ToList();
            // A list of generated image locations for collision detection
            List<Rect> existingRects = new List<Rect>();
            foreach (int index in selectableIndices)
            {
                Image img = new Image
                {
                    Source = new BitmapImage(new Uri(imagePaths[1][index], UriKind.Relative)),
                    Width = 80,
                    Height = 80,
                    Margin = new Thickness(10)
                };
                Border border = new Border
                {
                    BorderThickness = new Thickness(2),
                    BorderBrush = Brushes.Transparent,
                    Child = img
                };
                double maxLeft = rightBound - img.Width;
                double maxTop = bottomBound - img.Height;
                Rect newRect;
                bool isOverlapping;
                // Randomly generate positions until there is no overlap
                do
                {
                    double left = rand.NextDouble() * (maxLeft - leftBound) + leftBound;
                    double top = rand.NextDouble() * (maxTop - topBound) + topBound;

                    newRect = new Rect(left, top, img.Width, img.Height);
                    isOverlapping = existingRects.Any(rect => rect.IntersectsWith(newRect));
                } while (isOverlapping);

                // Add the location of the new image to the existing location list
                existingRects.Add(newRect);
                // set up Border Location
                Canvas.SetLeft(border, newRect.Left - leftBound);//relativelycanvasOffset
                Canvas.SetTop(border, newRect.Top - topBound);

                BorderStatusDict[border] = false;//At the beginning, it was initialized but not selected
                border.MouseLeftButtonDown += Border_MouseLeftButtonDown;//Realize the color change of frame after selection

                selectionCanvas.Children.Add(border);
            }
        }

        private void BorderColorChange()//existFindIn mode, click the confirm button and the selected pictures should change color.
        {
            //Process the box color display of selected pictures
            foreach (var kvp in BorderStatusDict)
            {
                Border border = kvp.Key;
                border.MouseLeftButtonDown -= Border_MouseLeftButtonDown;//There is no color change mechanism when the answer has been revealed
                Image img = border.Child as Image;
                bool status = kvp.Value;
                if (status == true)//Only frames are processed for selected images
                {
                    if (correctImages.Any(c => ((BitmapImage)c.Source).UriSource.ToString() == ((BitmapImage)img.Source).UriSource.ToString()))
                    {//This judgment should be made after the answer is revealedBorderBrushChange of color！
                        border.BorderBrush = Brushes.Green;
                    }
                    else
                    {
                        border.BorderBrush = Brushes.Red;
                    }
                }
                else//Unselected photos may also be missed
                {
                    if (correctImages.Any(c => ((BitmapImage)c.Source).UriSource.ToString() == ((BitmapImage)img.Source).UriSource.ToString()))
                    {//This judgment should be made after the answer is revealedBorderBrushChange of color！
                        border.BorderBrush = Brushes.Orange;
                    }
                }
            }
        }

        private void FoundButton_Click(object sender, RoutedEventArgs e)//FindConfirm button in mode to judge main logic
        {

            if (success_time >= 1)
            {
                OnGameBegin();//If you have answered correctly and clicked"Enter the game"(OriginalFoundButtonButton),Start the game directly
            }
            else
            {
                BorderColorChange();//First deal with the color change of the frame, optimize the space: make a judgment once, share various operations, rather than separate judgments for various operations

                bool isCorrect = selectedImages.Count == correctImages.Count &&
                            selectedImages.All(si => correctImages.Any(ci => ((BitmapImage)si.Source).UriSource == ((BitmapImage)ci.Source).UriSource));
                if (isCorrect)
                {
                    success_time++;
                    IntervalTimer.Interval = TimeSpan.FromSeconds(CorrectInterval);
                    if (IfAudioFeedBack) PlayWav(CorrectSoundPath);
                    if (IfVisualFeedBack) ShowFeedbackImage(CorrectImage, CorrectInterval * 1000);
                }
                else
                {
                    if(IfFoundButtonClicked)
                    {
                        IntervalTimer.Interval = TimeSpan.FromSeconds(0.0001);
                        IfFoundButtonClicked = false;
                    }
                    else
                    {
                        fail_time++;
                        IntervalTimer.Interval = TimeSpan.FromSeconds(ErrorInterval);
                        if (IfAudioFeedBack) PlayWav(ErrorSoundPath);
                        if (IfVisualFeedBack) ShowFeedbackImage(ErrorImage, ErrorInterval * 1000);
                        is_finish = false;
                    }

                }
                FoundButton.IsEnabled = false;
                IntervalTimer.Start();
            }
        }

        private void AdjustFindModeDifficulty()//according tolevelValue to adjust the corresponding parameters
        {
            if (IfLevelUp && Level <= MaxLevel)//If the level of upgrade is sufficient
            {
                if (Level < MaxLevel) { Level += 1; }
                IfLevelUp = false; ResultInit();//Each lifting level must be cleared and recalculated
            }
            else if (IfLevelDown && Level >= MinLevel)
            {
                if (Level > MinLevel) { Level -= 1; }
                IfLevelDown = false; ResultInit();
            }
            FindModeLevelCheck(Level);//Follow the reportLevelAdjust the parameters to ensure that the parameters take effect
        }

        private void FindModeLevelCheck(int level)
        {
            switch (level)
            {
                case 1:
                    right_picture_number = 2;
                    chose_picture_number = 6; // 2 correct pictures + 4 interfering pictures
                    Complexity = 1;
                    break;
                case 2:
                    right_picture_number = 2;
                    chose_picture_number = 7; // 2 correct pictures + 5 interfering pictures
                    Complexity = 1;
                    break;
                case 3:
                    right_picture_number = 3;
                    chose_picture_number = 9; // 3 correct pictures + 6 interfering pictures
                    Complexity = 1;
                    break;
                case 4:
                    right_picture_number = 3;
                    chose_picture_number = 10; // 3 correct pictures + 7 interfering pictures
                    Complexity = 1;
                    break;
                case 5:
                    right_picture_number = 4;
                    chose_picture_number = 12; // 4 correct pictures + 8 interfering pictures
                    Complexity = 1;
                    break;
                case 6:
                    right_picture_number = 4;
                    chose_picture_number = 10; // 4 correct pictures + 6 interfering pictures
                    Complexity = 2;
                    break;
                case 7:
                    right_picture_number = 5;
                    chose_picture_number = 12; // 5 correct pictures + 7 interfering pictures
                    Complexity = 2;
                    break;
                case 8:
                    right_picture_number = 5;
                    chose_picture_number = 13; // 5 correct pictures + 8 interfering pictures
                    Complexity = 2;
                    break;
                case 9:
                    right_picture_number = 6;
                    chose_picture_number = 15; // 6 correct pictures + 9 interfering pictures
                    Complexity = 2;
                    break;
                case 10:
                    right_picture_number = 6;
                    chose_picture_number = 16; // 6 correct pictures + 10 interfering pictures
                    Complexity = 2;
                    break;
                case 11:
                    right_picture_number = 7;
                    chose_picture_number = 14; // 7 correct pictures + 7 interfering pictures
                    Complexity = 3;
                    break;
                case 12:
                    right_picture_number = 7;
                    chose_picture_number = 15; // 7 correct pictures + 8 interfering pictures
                    Complexity = 3;
                    break;
                case 13:
                    right_picture_number = 8;
                    chose_picture_number = 14; // 8 correct pictures + 9 interfering pictures
                    Complexity = 3;
                    break;
                case 14:
                    right_picture_number = 8;
                    chose_picture_number = 18; // 8 correct pictures + 10 interfering pictures
                    Complexity = 3;
                    break;
                case 15:
                    right_picture_number = 10;
                    chose_picture_number = 21; // 10 correct pictures + 11 interfering pictures
                    Complexity = 3;
                    break;
                case 16:
                    right_picture_number = 10;
                    chose_picture_number = 25; // 10 correct pictures + 15 interfering pictures
                    Complexity = 4;
                    break;
                case 17:
                    right_picture_number = 10;
                    chose_picture_number = 30; // 10 correct pictures + 20 interfering pictures
                    Complexity = 4;
                    break;
                case 18:
                    right_picture_number = 10;
                    chose_picture_number = 35; // 10 correct pictures + 25 interfering pictures
                    Complexity = 4;
                    break;
                default:
                    right_picture_number = 2;
                    chose_picture_number = 6; // 2 correct pictures + 4 interfering pictures
                    Complexity = 1;
                    break;
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            Image img = border.Child as Image;
            if (border != null && BorderStatusDict.ContainsKey(border))
            {
                // Color change logic for marquee selection
                if (BorderStatusDict[border] == false) // Not selected
                {
                    border.BorderBrush = Brushes.Blue;
                    BorderStatusDict[border] = true;
                    selectedImages.Add(img);//Add to list
                }
                else // Uncheck
                {
                    border.BorderBrush = Brushes.Transparent;
                    BorderStatusDict[border] = false;
                    selectedImages.Remove(img);//Uncheck
                }
            }
        }

        /*FindCountMode*/
        private void SetupFindCountMode()
        {
            //Put the two feedback pictures on the right side to look better
            CorrectImage.HorizontalAlignment = HorizontalAlignment.Right;
            ErrorImage.HorizontalAlignment = HorizontalAlignment.Right;
            //Load the background first
            BackGroundFindCount.Loaded += BackGroundFindCount_Loaded;

            // Initialize the correct picture list
            correctImages = new List<Image>();
            IndexOfCount = 0;

            // Randomly select the correct image and add it tocorrectImagesIn the list
            Random rand = new Random();
            List<int> correctIndices = new List<int>();

            while (correctIndices.Count < right_picture_number)
            {
                int index = rand.Next(imagePaths[1].Length);

                if (imagePaths[1][index] != BackGroundPath && !correctIndices.Contains(index))
                {
                    correctIndices.Add(index);

                    // Add the correct image tocorrectImagesIn the list
                    Image img = new Image
                    {
                        Source = new BitmapImage(new Uri(imagePaths[1][index], UriKind.Relative)),
                    };

                    correctImages.Add(img);
                }
            }
            if (IfFisrtSet == false)
            {//The first time you draw content, you need to wait for componentsloadOnly after finishing can you draw
                FindCountGrid.Loaded += (s, e) => DisplayCountableImages(FindCountCanvas, rand);//Random pictures appearing on the background board
                TargetAreaFindCount.Loaded += (s, e) => DisplayToCountImages();
                IfFisrtSet = true;

            }
            else
            {//Not the first time, it's called directly
                BackGroundFindCount.Children.Clear();
                Image backgroundImage = new Image
                {
                    Source = new BitmapImage(new Uri(BackGroundPath, UriKind.Relative)),
                    //Stretch = Stretch.Uniform,
                    Stretch = Stretch.Fill,
                    Width = BackGroundFindCount.ActualWidth,  // Adjust the width of the background image
                    Height = BackGroundFindCount.ActualHeight,  // Adjust the height of the background image
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                BackGroundFindCount.Children.Add(backgroundImage);//Store background board
                DisplayCountableImages(FindCountCanvas, rand);//Random pictures appearing on the background board
                DisplayToCountImages();
            }


        }

        private void BackGroundFindCount_Loaded(object sender, RoutedEventArgs e)//BundleBackGroundPlus
        {
            BackGroundFindCount.Children.Clear();
            Image backgroundImage = new Image
            {
                Source = new BitmapImage(new Uri(BackGroundPath, UriKind.Relative)),
                //Stretch = Stretch.Uniform,
                Stretch = Stretch.Fill,
                Width = BackGroundFindCount.ActualWidth,  // Adjust the width of the background image
                Height = BackGroundFindCount.ActualHeight,  // Adjust the height of the background image
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            BackGroundFindCount.Children.Add(backgroundImage);//Store background board
        }

        private void DisplayCountableImages(Canvas selectionCanvas, Random rand)//Show the above pictures that require patients to count one by one
        {
            selectionCanvas.Children.Clear();//Clear it before each call
            //Get boundary information
            var selectionAreaGrid = (Grid)selectionCanvas.Parent; // Get SelectionCanvas parent control, i.e.SelectionAreaGrid
            var gridBounds = selectionAreaGrid.TransformToAncestor(this)
                .TransformBounds(new Rect(0, 0, selectionAreaGrid.ActualWidth, selectionAreaGrid.ActualHeight));

            // Boundary information
            double leftBound = gridBounds.Left;//One moreTransform
            double rightBound = gridBounds.Right;
            double topBound = gridBounds.Top;
            double bottomBound = gridBounds.Bottom;

            // Initialize the image display count
            List<Image> imagesToDisplay = new List<Image>();

            // Add correct picture to the picture display list
            foreach (var correctImage in correctImages)
            {
                // Randomly generate display times for each correct image
                int displayCount = rand.Next(min_right_display, max_right_display + 1);
                correctImagesCount[correctImage] = displayCount;//Record the number of it
                for (int i = 0; i < displayCount; i++)
                {
                    Image imgCopy = new Image
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
                             && imagePaths[1][i] != BackGroundPath)
                .OrderBy(x => rand.Next())
                .Take(mislead_picture_display_number)
                .ToList();

            foreach (var index in remainingIndices)
            {
                Image img = new Image
                {
                    Source = new BitmapImage(new Uri(imagePaths[1][index], UriKind.Relative)),
                    Width = 80,
                    Height = 80,
                    Margin = new Thickness(10)
                };

                imagesToDisplay.Add(img);
            }

            // A list of generated image locations for collision detection
            List<Rect> existingRects = new List<Rect>();
            // Randomize the location of the image and display the image inCanvasmiddle
            foreach (var img in imagesToDisplay)
            {
                // Create borders and set image locations
                Border border = new Border
                {
                    BorderThickness = new Thickness(2),
                    BorderBrush = Brushes.Transparent, // No border at the beginning
                    Child = img
                };
                double maxLeft = rightBound - img.Width;
                double maxTop = bottomBound - img.Height;

                Rect newRect;
                bool isOverlapping;
                // Randomly generate positions until there is no overlap
                do
                {                // Random generationXandYCoordinates to ensure that the image does not exceed the background image boundary
                    double left = rand.NextDouble() * (maxLeft - leftBound) + leftBound;
                    double top = rand.NextDouble() * (maxTop - topBound) + topBound;

                    newRect = new Rect(left, top, img.Width, img.Height);
                    isOverlapping = existingRects.Any(rect => rect.IntersectsWith(newRect));
                } while (isOverlapping);

                // Add the location of the new image to the existing location list
                existingRects.Add(newRect);
                // set up Border Location
                Canvas.SetLeft(border, newRect.Left - leftBound);//relativelycanvasOffset
                Canvas.SetTop(border, newRect.Top - topBound);

                selectionCanvas.Children.Add(border);

                // Judgment img Is it the objects shown below, not the objects on the background board
                bool isInCorrectImage = correctImages.Any(correctImage =>
                    ((BitmapImage)correctImage.Source).UriSource == ((BitmapImage)img.Source).UriSource);
                if (isInCorrectImage)
                {
                    ImageToBordersDict[img] = border;
                }
            }
        }

        private void DisplayToCountImages()//Show the correct picture so that the patient knows what counts it needs to be
        {
            // Create aHashSetUsed for deduplication, ensuring that each correct image is displayed only once
            HashSet<string> displayedImages = new HashSet<string>();
            TargetItemGridFindCount.Columns = correctImages.Count;
            double Size = Math.Min(TargetItemGridFindCount.ActualWidth / correctImages.Count - 10, TargetItemGridFindCount.ActualHeight - 10);//-10 is to keep a little margin to ensure complete display
            foreach (var img in correctImages)
            {
                string imageUri = ((BitmapImage)img.Source).UriSource.ToString();                // Get the pictureURI
                if (displayedImages.Contains(imageUri)) continue;                // If the image has been displayed, skip
                displayedImages.Add(imageUri);                // Add toHashSetto prevent repeated display
                Image correctImg = new Image
                {                // Show the image once in the lower panel
                    Source = img.Source,
                    Width = Size,
                    Height = Size,
                    Margin = new Thickness(10),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                // Create borders to tell the patient who is counting now
                Border border = new Border
                {
                    BorderThickness = new Thickness(2),
                    BorderBrush = Brushes.Transparent, // No border at the beginning
                    Child = img,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                CountBordersList.Add(border);

                TargetItemGridFindCount.Children.Add(border);
            }
            UpdateTargetItemBorder();//And update the borders casually
        }

        private void UpdateTargetItemBorder()//According to the currentindexTo determine which thing the patient is currently counting
        {
            foreach (Border border in CountBordersList)
            {
                if (IndexOfCount == CountBordersList.IndexOf(border))
                {//What is currently counting, the border changes color
                    border.BorderBrush = Brushes.Blue;
                }
                else
                {//If not, keep it transparent
                    border.BorderBrush = Brushes.Transparent;
                }
            }
        }

        // The event handling function is pressed by a number button
        private void OnNumberButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                int number = int.Parse(button.Content.ToString());
                userInput += button.Content.ToString();
                UpdateTextBlock();
            }
        }

        // "✔" Button press event handling function
        private async void OnSubmitButtonClick(object sender, RoutedEventArgs e)
        {
            //First parse the user input intoint
            int UserInputCount = int.Parse(userInput);
            if (!string.IsNullOrEmpty(userInput))//Clear the display
            {
                userInput = string.Empty;
                UpdateTextBlock();
            }
            if (IndexOfCount <= CountBordersList.Count - 1 && IndexOfCount >= 0)//Only the index is within this range can be judged
            {
                Image CountingObject = CountBordersList[IndexOfCount].Child as Image;//Find out which number of objects the user is counting
                bool isCorrect = (correctImagesCount[CountingObject] == UserInputCount);//Determine whether the number is correct
                if (isCorrect)
                {
                    IndexOfCount++;
                    if (IndexOfCount >= CountBordersList.Count)
                    {//I can do it here and it means that the whole question is correct
                        GroupResultCheck(isCorrect);
                        return;//in advancereturnTo prevent repeated feedback from correct
                    }
                    myCanvas.IsEnabled = false;
                    int PauseDurations = 1000;//Pause interval
                    if (IfAudioFeedBack) PlayWav(CorrectSoundPath);
                    if (IfVisualFeedBack) ShowFeedbackImage(CorrectImage, PauseDurations);
                    await Task.Delay(PauseDurations);//Reward one second delay
                    myCanvas.IsEnabled = true;
                    UpdateTargetItemBorder();//Update borders, only update after rewards
                }
                else
                {
                    ErrorCount++;
                    if (ErrorCount >= ErrorLimit)
                    {//The whole question is wrong
                        GroupResultCheck(isCorrect);
                    }
                    else
                    {//It's just the first time I answered wrong, I can answer again
                        myCanvas.IsEnabled = false;
                        int PauseDurations = 1000;//Pause interval
                        if (IfAudioFeedBack) PlayWav(ErrorSoundPath);
                        if (IfVisualFeedBack) ShowFeedbackImage(ErrorImage, PauseDurations);
                        await Task.Delay(PauseDurations);//Reflect on one second
                        myCanvas.IsEnabled = true;
                    }

                }
            }
        }

        private void GroupResultCheck(bool isCorrect)//Make a judgment on the entire question
        {//It contains one thing that counts twice, and calculates that the whole question is wrong；All the things are counted correctly, and the whole question is correct
            if (isCorrect)
            {
                success_time++;
                if (IfAudioFeedBack) PlayWav(CorrectSoundPath);
                if (IfVisualFeedBack) ShowFeedbackImage(CorrectImage, CorrectInterval * 1000);
                IntervalTimer.Interval = TimeSpan.FromSeconds(CorrectInterval);

            }
            else
            {
                fail_time++;
                if (IfAudioFeedBack) PlayWav(ErrorSoundPath);
                if (IfVisualFeedBack) ShowFeedbackImage(ErrorImage, ErrorInterval * 1000);
                IntervalTimer.Interval = TimeSpan.FromSeconds(ErrorInterval);
                CountImageBorderChange();
                //int ignoreCount = correctImages.Count(ci => !selectedImages.Any(si => ((BitmapImage)si.Source).UriSource == ((BitmapImage)ci.Source).UriSource));
                //ignoreAnswers[Level] += ignoreCount;
                //No omission calculations are performed, because it may count too many or less
                is_finish = false;
            }
            myCanvas.IsEnabled = false;
            IntervalTimer.Start();
        }

        private void CountImageBorderChange()//After the answer is wrong, you need to miss the numberborderTurns yellow
        {
            Image CountingObject = CountBordersList[IndexOfCount].Child as Image;//Find out which number of objects the user is counting
            foreach (var (key, value) in ImageToBordersDict)
            {
                if (((BitmapImage)CountingObject.Source).UriSource == ((BitmapImage)key.Source).UriSource)
                {//If theirSourcesourcedouSame
                    value.BorderBrush = Brushes.Orange;
                }
            }
        }

        private void AdjustFindCountModeDifficulty()//AdjustmentFindCountMode difficulty level
        {
            if (IfLevelUp && Level <= MaxLevel)//If the level of upgrade is sufficient
            {
                if (Level < MaxLevel) { Level += 1; }
                IfLevelUp = false; ResultInit();//Each lifting level must be cleared and recalculated
            }
            else if (IfLevelDown && Level >= MinLevel)
            {
                if (Level > MinLevel) { Level -= 1; }
                IfLevelDown = false; ResultInit();
            }
            FindCountModeLevelCheck(Level);//Follow the reportLevelAdjust the parameters to ensure that the parameters take effect
        }

        // "←" Button press event handling function, new function: delete the previous input number
        private void OnBackButtonClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(userInput))
            {
                // Delete the last number
                userInput = userInput.Substring(0, userInput.Length - 1);
                UpdateTextBlock();
            }
        }

        private void UpdateTextBlock()
        {//Why not followuserInputWhat about binding
            displayTextBlock.Text = userInput;
        }

        private void FindCountModeLevelCheck(int level)
        {
            switch (level)
            {
                case 1:
                    right_picture_number = 1; // Types of objects to count
                    max_right_display = 5;
                    min_right_display = 2;
                    mislead_picture_display_number = 4; // Unrelated items
                    break;
                case 2:
                    right_picture_number = 1;
                    max_right_display = 6;
                    min_right_display = 3;
                    mislead_picture_display_number = 5;
                    break;
                case 3:
                    right_picture_number = 1;
                    max_right_display = 7;
                    min_right_display = 4;
                    mislead_picture_display_number = 6;
                    break;
                case 4:
                    right_picture_number = 1;
                    max_right_display = 8;
                    min_right_display = 5;
                    mislead_picture_display_number = 7;
                    break;
                case 5:
                    right_picture_number = 1;
                    max_right_display = 10;
                    min_right_display = 6;
                    mislead_picture_display_number = 8;
                    break;
                case 6:
                    right_picture_number = 1;
                    max_right_display = 7;
                    min_right_display = 4;
                    mislead_picture_display_number = 6;
                    break;
                case 7:
                    right_picture_number = 2;
                    max_right_display = 8;
                    min_right_display = 5;
                    mislead_picture_display_number = 7;
                    break;
                case 8:
                    right_picture_number = 2;
                    max_right_display = 10;
                    min_right_display = 3;
                    mislead_picture_display_number = 8;
                    break;
                case 9:
                    right_picture_number = 2;
                    max_right_display = 10;
                    min_right_display = 3;
                    mislead_picture_display_number = 9;
                    break;
                case 10:
                    right_picture_number = 2;
                    max_right_display = 10;
                    min_right_display = 3;
                    mislead_picture_display_number = 10;
                    break;
                case 11:
                    right_picture_number = 3;
                    max_right_display = 10;
                    min_right_display = 3;
                    mislead_picture_display_number = 7;
                    break;
                case 12:
                    right_picture_number = 3;
                    max_right_display = 10;
                    min_right_display = 3;
                    mislead_picture_display_number = 8;
                    break;
                case 13:
                    right_picture_number = 3;
                    max_right_display = 10;
                    min_right_display = 3;
                    mislead_picture_display_number = 9;
                    break;
                case 14:
                    right_picture_number = 3;
                    max_right_display = 10;
                    min_right_display = 3;
                    mislead_picture_display_number = 10;
                    break;
                case 15:
                    right_picture_number = 3;
                    max_right_display = 10;
                    min_right_display = 3;
                    mislead_picture_display_number = 11;
                    break;
                case 16:
                    right_picture_number = 3;
                    max_right_display = 10;
                    min_right_display = 3;
                    mislead_picture_display_number = 15;
                    break;
                case 17:
                    right_picture_number = 3;
                    max_right_display = 10;
                    min_right_display = 3;
                    mislead_picture_display_number = 20;
                    break;
                case 18:
                    right_picture_number = 3;
                    max_right_display = 10;
                    min_right_display = 3;
                    mislead_picture_display_number = 25;
                    break;
                default:
                    right_picture_number = 1;
                    max_right_display = 5;
                    min_right_display = 2;
                    mislead_picture_display_number = 4;
                    break;
            }
        }

        /*Shared*/
        private void GameTimer_Tick(object sender, EventArgs e)//This timer is used to see how much time is left, and the entire game time
        {
            // Reduce remaining time per second
            if (timeRemaining > TimeSpan.Zero)
            {
                timeRemaining = timeRemaining.Add(TimeSpan.FromSeconds(-1));
                int remainingSeconds = (int)timeRemaining.TotalSeconds;

                TimeStatisticsAction.Invoke(remainingSeconds, PlayTime);
                // Calling delegate
                LevelStatisticsAction?.Invoke(Level, MaxLevel);
                RightStatisticsAction?.Invoke(success_time, LevelUp);
                WrongStatisticsAction?.Invoke(fail_time, LevelDown);
            }
            else
            {
                gameTimer.Stop(); // Stop the timer

                OnGameEnd();
            }
        }

        private void ResultInit()//The temporary record results must be set to zero after each lifting and lowering.
        {
            success_time = 0;
            fail_time = 0;
            if (train_mode == FindAndCountMode) ErrorCount = 0;
        }

        private void ResultCheck()//Determine whether the number of questions you have done correctly in total, and whether you need to upgrade or not
        {
            if (success_time >= LevelUp) { IfLevelUp = true; }
            if (fail_time >= LevelDown) { IfLevelDown = true; }
        }

        private void ResetGameState()//Reset game status
        {
            if (train_mode == FindMode)
            {
                // Clear the user selected image
                selectedImages?.Clear();
                // Clear the correct picture list
                correctImages?.Clear();

                SelectionCanvas.Children.Clear();//Clear some options that appear on the background image
                TargetItemGrid.Children.Clear();//Clear the pictures to be selected

            }
            else if (train_mode == FindAndCountMode)
            {
                // Clear the user selected image
                selectedImages?.Clear();
                // Clear the correct picture list
                correctImages?.Clear();

                CountBordersList.Clear();
                ImageToBordersDict.Clear();

                FindCountCanvas.Children.Clear();
                TargetItemGridFindCount.Children.Clear();
            }
        }

        private void InitializeGame()
        {
            // Reset status before starting a new game
            if (train_mode == FindMode)
            {
                FindPatternGrid.Visibility = Visibility.Visible;//Show corresponding components
                FindAndCountPatternGrid.Visibility = Visibility.Collapsed;
                FindModeLevelCheck(Level);
                VoiceTipAction?.Invoke("Please find out the position of the object at the bottom of the screen in the picture above and click with the mouse to select it.");
                RuleAction?.Invoke("Please find out the position of the object at the bottom of the screen in the picture above and click with the mouse to select it.");
                SetupFindMode();
            }
            else if (train_mode == FindAndCountMode)
            {
                FindAndCountPatternGrid.Visibility = Visibility.Visible;//Show corresponding components
                FindPatternGrid.Visibility = Visibility.Collapsed;
                FindCountModeLevelCheck(Level);
                VoiceTipAction?.Invoke("Please count the number of times the image object below appears in the image above and enter it on the keyboard on the right");
                RuleAction?.Invoke("Please count the number of times the image object below appears in the image above and enter it on the keyboard on the right");
                SetupFindCountMode();
            }
            SetTitleVisibleAction?.Invoke(true);//Bundle“Question Rules”Four words are also displayed
        }

        private void PlayTimer_Tick(object sender, EventArgs e)//Response time limit
        {
            PlayTime--;
            if (PlayTime <= 0)//Time has come
            {
                //The next round of answers is started,This question is wrong
                fail_time++;
                wrongAnswers[Level] += 1;
                IntervalTimer.Interval = TimeSpan.FromSeconds(ErrorInterval);
                if (IfAudioFeedBack) PlayWav(ErrorSoundPath);
                if (IfVisualFeedBack) ShowFeedbackImage(ErrorImage, ErrorInterval * 1000);

                ResultCheck();// After the judgment is completed, it should be checked
                if (train_mode == FindMode)
                {
                    AdjustFindModeDifficulty(); //See if you need to adjust the difficulty of the game
                    FoundButton.IsEnabled = false;
                }
                else if (train_mode == FindAndCountMode)
                {
                    AdjustFindCountModeDifficulty();
                    myCanvas.IsEnabled = false;
                }
                //Next, wait until the timer triggers before starting the next game, and no components can be triggered during the countdown period.

                IntervalTimer.Start();
                PlayTime = BaseTimeLimit + (Level - MinLevel) * TimeLimitInterval;//Calculate the time limit
                PlayTimer.Stop();//Answer time limit timer stops
            }
        }

        private void IntervalTimer_Tick(object sender, EventArgs e)//Answer interval timer
        {// This will be triggered to enter the next question
            if (train_mode == FindMode)
            {
                FoundButton.IsEnabled = true;
            }
            if (train_mode == FindAndCountMode)
            {
                myCanvas.IsEnabled = true;
                ErrorCount = 0;//Reset errors
            }
            if (IfLimitTime)
            {
                PlayTimer.Start();//Start answering limit timer
            }

            IntervalTimer.Stop();//Stop the timer
            if(success_time>=1)
            {//It means that you have answered correctly and you don't need to update the game anymore. Just start the body
                if(train_mode==FindMode)
                {
                    FoundButton.Content = "Enter the game";
                }
                else
                {
                    ContinueToStart.Visibility = Visibility.Visible;
                    myCanvas.IsEnabled = false;
                    //MakeFindCountThe mode entry button is visible for clicking
                }
                return;//Prevent new problems from refreshing
            }
            //Update the game state with this function to update
            ResetGameState(); // Reset game status
            InitializeGame(); // Start a new game
        }
            
        /*LJN
         Added resources for visual and sound feedback
         */
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

        private async void ShowFeedbackImage(Image image, int StopDurations = 2000)//StopDurationsThe unit isms
        {//Image showing feedback
            image.Visibility = Visibility.Visible;

            // Delay the specified time（For example, 1 second）
            await Task.Delay(StopDurations);

            image.Visibility = Visibility.Collapsed;
        }

        private void LastStep_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage--;
            PageSwitchSet();
        }

        private void NextStep_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage++;
            PageSwitchSet();
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {//Skip the entire trial and start the body directly
            OnGameBegin();
        }

        private void Try_Click(object sender, RoutedEventArgs e)
        {
            ExplainGrid.Visibility = Visibility.Collapsed;//After starting the trial, hide the explained components
            //Start trying
            ResetGameState(); // Reset game status
            InitializeGame(); // Start a new game
            if(train_mode == FindMode)
            {//I don't know why I need to call this manually. It may take time to refresh it again. It is achieved with the help of this flag bit.
                IfFoundButtonClicked = true;
                FoundButton_Click(FoundButton, new RoutedEventArgs());
            }
            if(train_mode == FindAndCountMode)
            {
                IntervalTimer.Interval = TimeSpan.FromSeconds(0.0001);
                IntervalTimer.Start();
            }

        }

        private void SetVisibilityForAllChildren(UIElement element, Visibility visibility)//Set the visibility of the components of the leaf nodes uniformly
        {
            if (element is Panel panel)
            {
                // in the case of Panel type（like Grid、StackPanel wait）, iterate over its child elements
                foreach (UIElement child in panel.Children)
                {
                    SetVisibilityForAllChildren(child, visibility); // Recursive call
                }
            }
            else if (element is ContentControl contentControl && contentControl.Content is UIElement content)
            {
                // in the case of ContentControl type（like Button）, check its content
                SetVisibilityForAllChildren(content, visibility);
            }
            else
            {
                // If it is the smallest control（Leaf node）, set it Visibility
                element.Visibility = visibility;
            }
        }

        private void PageSwitchSet()//according topageValue to display components
        {
            SetVisibilityForAllChildren(ExplainPictureGrid, Visibility.Collapsed);//First hide all leaf components, just display the corresponding required ones
            if (train_mode == FindMode)//The explanation logic of the two modes is logically separated
            {
                switch (CurrentPage)
                {
                    case 1://The first page, unifiedIntroduction, only the middle text description is displayed
                           //Text description

                        Introduction.Text = "Now the explanation of finding the object mode will be carried out. Please click Next with the mouse to enter the explanation.";
                        Introduction.Visibility = Visibility.Visible;
                        //Picture description

                        //Button below
                        ExplainButtonsGrid.Columns = 3;
                        LastStep.Visibility = Visibility.Collapsed;
                        NextStep.Visibility = Visibility.Visible;
                        Try.Visibility = Visibility.Visible;
                        Skip.Visibility = Visibility.Visible;
                        break;
                    case 2://The second page, enterFindModeofBackGround
                           //Text prompts
                        ModeTextBlock.Visibility = Visibility.Visible;
                        ModeTextBlock.Text = "Find the target object pattern:";
                        ExplainTextBlock.Visibility = Visibility.Visible;
                        ExplainTextBlock.Text = "First, you will see many different objects appearing in the background image";

                        //Picture tips
                        FindModeBackGround.Visibility = Visibility.Visible;

                        //Button below
                        ExplainButtonsGrid.Columns = 4;
                        LastStep.Visibility = Visibility.Visible;
                        NextStep.Visibility = Visibility.Visible;
                        Try.Visibility = Visibility.Visible;
                        Skip.Visibility = Visibility.Visible;
                        break;
                    case 3://The third page, enterFindModeofFindModeItemsToFind
                           //Text prompts
                        ModeTextBlock.Visibility = Visibility.Visible;
                        ModeTextBlock.Text = "Find the target object pattern:";
                        ExplainTextBlock.Visibility = Visibility.Visible;
                        ExplainTextBlock.Text = "Then, you will see the image object you need to look for appears below the background image";

                        //Picture tips
                        FindModeBackGround.Visibility = Visibility.Visible;
                        FindModeItemsToFind.Visibility = Visibility.Visible;

                        //Button below
                        ExplainButtonsGrid.Columns = 4;
                        LastStep.Visibility = Visibility.Visible;
                        NextStep.Visibility = Visibility.Visible;
                        Try.Visibility = Visibility.Visible;
                        Skip.Visibility = Visibility.Visible;
                        break;
                    case 4://The fourth page, enterFindModeTotal
                           //Text prompts
                        ModeTextBlock.Visibility = Visibility.Visible;
                        ModeTextBlock.Text = "Find the target object pattern:";
                        ExplainTextBlock.Visibility = Visibility.Visible;
                        ExplainTextBlock.Text = "Finally, please use the mouse to click on the object you found on the background image and clickOKKey confirm selection";

                        //Picture tips
                        FindModeTotal.Visibility = Visibility.Visible;

                        //Button below
                        ExplainButtonsGrid.Columns = 4;
                        LastStep.Visibility = Visibility.Visible;
                        NextStep.Visibility = Visibility.Visible;
                        Try.Visibility = Visibility.Visible;
                        Skip.Visibility = Visibility.Visible;
                        break;
                    case 5:
                        //Text prompts
                        Introduction.Visibility = Visibility.Visible;
                        Introduction.Text = "Please click the trial button to start the trial, or click the skip button to enter the game";

                        //Picture tips

                        //Button below
                        ExplainButtonsGrid.Columns = 3;
                        LastStep.Visibility = Visibility.Visible;
                        NextStep.Visibility = Visibility.Collapsed;
                        Try.Visibility = Visibility.Visible;
                        Skip.Visibility = Visibility.Visible;
                        break;
                    default:
                        MessageBox.Show("There is a problem with your network, please contact the administrator");
                        break;
                }
            }
            else if(train_mode==FindAndCountMode)
            {
                switch(CurrentPage)
                {
                    case 1://The first page, unifiedIntroduction, only the middle text description is displayed
                           //Text description

                        Introduction.Text = "Now the explanation of finding objects and counting mode will be carried out. Please click Next with the mouse to enter the explanation.";
                        Introduction.Visibility = Visibility.Visible;
                        //Picture description

                        //Button below
                        ExplainButtonsGrid.Columns = 3;
                        LastStep.Visibility = Visibility.Collapsed;
                        NextStep.Visibility = Visibility.Visible;
                        Try.Visibility = Visibility.Visible;
                        Skip.Visibility = Visibility.Visible;
                        break;
                    case 2://The fifth page, enterFindCountModeofFindCountModeBackGround
                           //Text prompts
                        ModeTextBlock.Visibility = Visibility.Visible;
                        ExplainTextBlock.Visibility = Visibility.Visible;
                        ModeTextBlock.Text = "Find the target object and count the mode:";
                        ExplainTextBlock.Text = "First, you will see several image objects on the background image on the left";

                        //Picture tips
                        FindCountModeBackGround.Visibility = Visibility.Visible;

                        //Button below
                        ExplainButtonsGrid.Columns = 4;
                        LastStep.Visibility = Visibility.Visible;
                        NextStep.Visibility = Visibility.Visible;
                        Try.Visibility = Visibility.Visible;
                        Skip.Visibility = Visibility.Visible;
                        break;
                    case 3://The sixth page, enterFindCountModeofFindCountModeItemsToCount
                           //Text prompts
                        ModeTextBlock.Visibility = Visibility.Visible;
                        ExplainTextBlock.Visibility = Visibility.Visible;
                        ModeTextBlock.Text = "Find the target object and count the mode:";
                        ExplainTextBlock.Text = "Then you will see the object you need to count and marked with a blue box below the background image";

                        //Picture tips
                        FindCountModeBackGround.Visibility = Visibility.Visible;
                        FindCountModeItemsToCount.Visibility = Visibility.Visible;

                        //Button below
                        ExplainButtonsGrid.Columns = 4;
                        LastStep.Visibility = Visibility.Visible;
                        NextStep.Visibility = Visibility.Visible;
                        Try.Visibility = Visibility.Visible;
                        Skip.Visibility = Visibility.Visible;
                        break;
                    case 4://The seventh page, enterFindCountModeofFindCountModeTotal
                           //Text prompts
                        ModeTextBlock.Visibility = Visibility.Visible;
                        ExplainTextBlock.Visibility = Visibility.Visible;
                        ModeTextBlock.Text = "Find the target object and count the mode:";
                        ExplainTextBlock.Text = "When your count is complete, enter your answer on the numeric keypad on the right and click√Button to submit answer";

                        //Picture tips
                        FindCountModeTotal.Visibility = Visibility.Visible;

                        //Button below
                        ExplainButtonsGrid.Columns = 4;
                        LastStep.Visibility = Visibility.Visible;
                        NextStep.Visibility = Visibility.Visible;
                        Try.Visibility = Visibility.Visible;
                        Skip.Visibility = Visibility.Visible;
                        break;
                    case 5://The eighth page is over, and the player starts to try it out
                           //Text prompts
                        Introduction.Visibility = Visibility.Visible;
                        Introduction.Text = "Please click the trial button to start the trial, or click the skip button to enter the game";

                        //Picture tips

                        //Button below
                        ExplainButtonsGrid.Columns = 3;
                        LastStep.Visibility = Visibility.Visible;
                        NextStep.Visibility = Visibility.Collapsed;
                        Try.Visibility = Visibility.Visible;
                        Skip.Visibility = Visibility.Visible;
                        break;
                    default:
                        MessageBox.Show("There is a problem with your network, please contact the administrator");
                        break;
                }
            }
        }

        private void ContinueToStart_Click(object sender, RoutedEventArgs e)
        {
            OnGameBegin();
        }
    }
}
