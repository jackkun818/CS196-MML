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
using System.Text.Json;
using Newtonsoft.Json;
using System.Runtime.Intrinsics.Arm;

namespace crs.game.Games
{
    /// <summary>
    /// EXO.xaml Interaction logic
    /// </summary>
    public partial class Search capability 3 : BaseUserControl
    {
        private readonly List<List<string>> imagePaths = new List<List<string>>
        {
            new List<string>
            {
                
            },

            new List<string>
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
        private int FindItem = 0;//Used to giveimagePathsIndexing is convenient for debugging

        private int Complexity = 1;//FindNeed notCountComplexity in the difficulty level list of modes, 1 simple, 2 medium, 3 complex, 4 very complex
        private string ComplexityFolder;//findThe corresponding complexity in the difficulty level of the mode is used to index folders
        private int right_picture_number = 4; // The correct number of pictures displayed
        private int chose_picture_number = 6; // The total number of selectable pictures displayed, including correct and incorrect

        private Dictionary<Border, bool> BorderStatusDict = new Dictionary<Border, bool>();//FindMode: used to storeBorderSelected state to determine whether the frame color needs to be changed；

        //FindCountMode
        private int FindAndCountMode = 2;//These two variables are only used to refer to during early debugging, and are more intuitive than numbers 3 or 4
        private int FindCountItem = 1;//Used to giveimagePathsIndexing is convenient for debugging

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

        private string BackGroundPath;//The background path is replaced by a variable, so it is easy to modify

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
        private int success_time = 0;
        private int fail_time = 0;

        private bool IfLevelDown = false;//Is it necessary to lower the level
        private bool IfLevelUp = false;//Is it necessary to upgrade level

        private string userInput; // Stores the numbers entered by the user

        private bool is_finish = false;

        //------------------Report parameters-------------------------------------
        private int train_mode = 1; // Game mode, 1, 2, 3, 4	1.	Pattern 1: Find missing numbers in the range of numbers and enter them one by one from small to large. This pattern usually involves the user identifying and entering missing numbers.
                                    //Mode 2: Identify different shapes that are superimposed on each other and select them from the bottom of the screen. This pattern involves the user's need to find the correct shape from the superimposed shape.
                                    //Mode 4: Count out and enter the number of times each correct object appears in the picture.

        private int repet_time = 1;

        //----------------------------------------------------------------

        private int repet_count = 0;

        public Search capability 3()
        {
            InitializeComponent();
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
        private void SetupFindMode()
        {
            //Draw the background first
            BackGround.Loaded += BackGround_Loaded;
            //Prepare materials
            GetItemsReady();
            // Initialize the correct picture list
            correctImages = new List<Image>();
            selectedImages = new List<Image>();

            // Randomly select the correct image and add it tocorrectImagesIn the list
            Random rand = new Random();
            List<int> correctIndices = new List<int>();
            while (correctIndices.Count < right_picture_number)
            {
                int index = rand.Next(imagePaths[FindItem].Count);

                if (imagePaths[FindItem][index] != BackGroundPath && !correctIndices.Contains(index))
                {//Select the correct picture
                    correctIndices.Add(index);
                    Image img = new Image
                    {
                        Source = new BitmapImage(new Uri(imagePaths[FindItem][index], UriKind.Relative)),
                    };
                    correctImages.Add(img);
                }
            }

            if (IfFisrtSet == false)
            {//The first time you draw content, you need to wait for componentsloadOnly after finishing can you draw
                SelectionAreaGrid.Loaded += (s, e) => DisplaySelectableImages(SelectionCanvas, rand);//existSelectionCanvasShow pictures,Because dynamic adjustment is required,loadShow again after finish
                TargetAreaGrid.Loaded += (s, e) => DisplayCorrectImages(TargetItemGrid);//existTargetItemGridShow pictures to be selected,Because dynamic adjustment is required,loadCall it again afterward
                IfFisrtSet = true;
            }
            else
            {//Not the first time, it's called directly
                BackGround_Loaded(BackGround, new RoutedEventArgs());
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
                Stretch = Stretch.Fill,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            BackGround.Children.Add(backgroundImage);//Add toBackGroundGridmiddle

        }

        private void DisplayCorrectImages(UniformGrid TargetItemGrid)//Show the following line of pictures for patients
        {
            TargetItemGrid.Children.Clear();
            TargetItemGrid.Columns = correctImages.Count;//Make sure to display it in one line
            double Size = Math.Min(TargetItemGrid.ActualWidth / correctImages.Count-10, TargetItemGrid.ActualHeight-10);//-10 is to keep a little margin to ensure complete display
            foreach (var img in correctImages)
            {
                Image correctImg = new Image
                {
                    Source = img.Source,
                    Width = Size*0.7,
                    Height = Size*0.7,//Make sure it's square
                    Margin = new Thickness(10)
                };
                TargetItemGrid.Children.Add(correctImg);//Add to thisuniformgridinside
            }


        }

        private void DisplaySelectableImages(Canvas selectionCanvas, Random rand)//Show the above pictures that patients click to select
        {
            selectionCanvas.Children.Clear();//Clear it before each call
                                             // Get the background image BackGround
            /*
             The background image itself isFillFill, so you need to calculate the scaling ratio of its length and width direction first, and scale it synchronouslyitemofsize
            Then the result is measured at the same timejsonIn the filepxThe distance must be scaled
            pxAfter scaling, it must be converted intoCanvas.SetLeftThe device-independent pixels used（Device-Independent Pixels, DIP）distance
            WPFThe space size and distance unit in it are 1/96 inches,[Physical unit size]=[Independent equipment unit size] × [ScreenDPIScore]
            The physical size displayed on the screen(Pixels)= WPFUnits of(1/96) * Screen settingsDPI
            Because the Blue Lake is usedPXdistance,WPFUsed isDIPdistance
            The material background is treated like this: DirectFillTo the wholeGirdmiddle
            Material: First, zoom, press Width material/Wide background Material height/High background   This scale is scaled, the correspondingmarginGo to zoom too
             */
            var backGround = (Grid)SelectionAreaGrid.FindName("BackGround");
            double bgWidthScale = backGround.ActualWidth/((imageData.BackgroundImage.Size.Width / 96) * screenDpiHorizontal);
            double bgHeightScale = backGround.ActualHeight / ((imageData.BackgroundImage.Size.Height / 96) * screenDpiVertical);
            // Get the rectangle boundary of the background image,TransformToAncestor Used to get the position relative to the parent element
            var backgroundBounds = backGround.TransformToAncestor(SelectionAreaGrid)
                .TransformBounds(new Rect(0, 0, backGround.ActualWidth, backGround.ActualHeight));

            // Background image border information
            double bgLeft = backgroundBounds.Left; // Background image relative SelectionAreaGrid The left position
            double bgTop = backgroundBounds.Top;   // Background image relative SelectionAreaGrid The upper position

            // Create a list of indexes for selectable images and first add the index for the correct image
            List<int> selectableIndices = correctImages
                .Select(img => imagePaths[FindItem].IndexOf(((BitmapImage)img.Source).UriSource.ToString().Replace("pack://application:,,,", "")))
                .ToList();


            // Randomly select from the remaining pictures until you reachchose_picture_number
            List<int> remainingIndices = Enumerable.Range(0, imagePaths[FindItem].Count)
                .Where(i => !selectableIndices.Contains(i) && imagePaths[FindItem][i] != BackGroundPath)
                .OrderBy(x => rand.Next())
                .Take(chose_picture_number - selectableIndices.Count)//TakeOnly the first few will be selected, so it will not be repeated unless the number is insufficient
                .ToList();

            selectableIndices.AddRange(remainingIndices);

            // Randomize the order of the pictures
            selectableIndices = selectableIndices.OrderBy(x => rand.Next()).ToList();
            foreach (int index in selectableIndices)
            {
                Image img = new Image
                {
                    Source = new BitmapImage(new Uri(imagePaths[FindItem][index], UriKind.Relative)),
                    Width = (imageData.Images[index].Size.Width / 96) * screenDpiHorizontal * bgWidthScale,
                    Height = (imageData.Images[index].Size.Height / 96) * screenDpiVertical * bgHeightScale,
                };
                Border border = new Border
                {
                    BorderThickness = new Thickness(2),
                    BorderBrush = Brushes.Transparent,
                    Child = img
                };

                //set up Border Location
                // Get icon relative background image PxMargin
                double marginLeft = (imageData.Images[index].PxMargin.Left / 96) * screenDpiHorizontal * bgWidthScale;
                double marginTop = (imageData.Images[index].PxMargin.Up / 96) * screenDpiVertical * bgHeightScale;
                // Calculate icon relative Canvas Location
                double iconLeft = bgLeft + marginLeft; // relatively Canvas The left distance
                double iconTop = bgTop + marginTop; // relatively Canvas The upper distance
                Canvas.SetLeft(border, iconLeft);//relativelycanvasOffset
                Canvas.SetTop(border, iconTop);
                Canvas.SetZIndex(border, int.MaxValue);  // Will border Set to the top level

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
            BorderColorChange();//First deal with the color change of the frame, optimize the space: make a judgment once, share various operations, rather than separate judgments for various operations

            bool isCorrect = selectedImages.Count == correctImages.Count &&
                        selectedImages.All(si => correctImages.Any(ci => ((BitmapImage)si.Source).UriSource == ((BitmapImage)ci.Source).UriSource));
            if (isCorrect)
            {
                success_time++;
                correctAnswers[Level] += 1;
                IntervalTimer.Interval = TimeSpan.FromSeconds(CorrectInterval);
                if (IfAudioFeedBack) PlayWav(CorrectSoundPath);
                if (IfVisualFeedBack) ShowFeedbackImage(CorrectImage, CorrectInterval * 1000);
            }
            else
            {
                fail_time++;
                wrongAnswers[Level] += 1;
                IntervalTimer.Interval = TimeSpan.FromSeconds(ErrorInterval);
                if (IfAudioFeedBack) PlayWav(ErrorSoundPath);
                if (IfVisualFeedBack) ShowFeedbackImage(ErrorImage, ErrorInterval * 1000);
                int ignoreCount = correctImages.Count(ci => !selectedImages.Any(si => ((BitmapImage)si.Source).UriSource == ((BitmapImage)ci.Source).UriSource));
                ignoreAnswers[Level] += ignoreCount;
                is_finish = false;
            }

            ResultCheck();// After the judgment is completed, it should be checked


            AdjustFindModeDifficulty(); //See if you need to adjust the difficulty of the game

            //Next, wait until the timer triggers before starting the next game, and no components can be triggered during the countdown period.
            if (IfLimitTime)
            {
                PlayTime = BaseTimeLimit + (Level - MinLevel) * TimeLimitInterval;//Calculate the time limit
                PlayTimer.Stop();//Answer time limit timer stops
            }
            FoundButton.IsEnabled = false;
            IntervalTimer.Start();
        }

        int max_hardness = 1;
        private void AdjustFindModeDifficulty()//according tolevelValue to adjust the corresponding parameters
        {
            if (IfLevelUp && Level <= MaxLevel)//If the level of upgrade is sufficient
            {
                if (Level < MaxLevel) { Level += 1; }
                IfLevelUp = false; ResultInit();//Each lifting level must be cleared and recalculated
                max_hardness = Math.Max(max_hardness, Level);
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
            GetComplexityFolder();
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

        private void GetComplexityFolder()//Convert complexity fromintConvert tostring
        {
            switch(Complexity)
            {
                case 1:ComplexityFolder = "Easy";break;
                case 2:ComplexityFolder = "Medium";break;
                case 3:ComplexityFolder = "Hard";break;
                case 4:ComplexityFolder = "VeryHard";break;
                default:ComplexityFolder = "VeryHard";break;
            }
        }
        /*FindCountMode*/
        private void SetupFindCountMode()
        {
            //Put the two feedback pictures on the right side to look better
            CorrectImage.HorizontalAlignment = HorizontalAlignment.Right;
            ErrorImage.HorizontalAlignment = HorizontalAlignment.Right;
            //Load the background first
            BackGroundPath = "EXO/2/Background.png";
            BackGroundFindCount.Loaded += BackGroundFindCount_Loaded;

            // Initialize the correct picture list
            correctImages = new List<Image>();
            IndexOfCount = 0;

            // Randomly select the correct image and add it tocorrectImagesIn the list
            Random rand = new Random();
            List<int> correctIndices = new List<int>();

            while (correctIndices.Count < right_picture_number)
            {
                int index = rand.Next(imagePaths[FindCountItem].Count);

                if (imagePaths[FindCountItem][index] != BackGroundPath && !correctIndices.Contains(index))
                {
                    correctIndices.Add(index);

                    // Add the correct image tocorrectImagesIn the list
                    Image img = new Image
                    {
                        Source = new BitmapImage(new Uri(imagePaths[FindCountItem][index], UriKind.Relative)),
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
                DisplayCountableImages(FindCountCanvas, rand);//Random pictures appearing on the background board
                DisplayToCountImages();
            }


        }

        private void BackGroundFindCount_Loaded(object sender, RoutedEventArgs e)//BundleBackGroundPlus
        {
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
            List<int> remainingIndices = Enumerable.Range(0, imagePaths[FindCountItem].Count)
                .Where(i => !correctImages.Any(c => ((BitmapImage)c.Source).UriSource.ToString().EndsWith(imagePaths[FindCountItem][i]))
                             && imagePaths[FindCountItem][i] != BackGroundPath)
                .OrderBy(x => rand.Next())
                .Take(mislead_picture_display_number)
                .ToList();

            foreach (var index in remainingIndices)
            {
                Image img = new Image
                {
                    Source = new BitmapImage(new Uri(imagePaths[FindCountItem][index], UriKind.Relative)),
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
            int UserInputCount = -1;
            if (!string.IsNullOrEmpty(userInput))//Clear the display
            {
                // use TryParse To safely parse integers and avoid exceptions
                if (int.TryParse(userInput, out UserInputCount))
                {
                    UserInputCount = int.Parse(userInput);
                }
                else
                {
                    // Handle parsing failure（For example, the input is not a valid number）
                    UserInputCount = -1; // Or set a default value
                }
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
                    correctAnswers[Level] += 1;
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
            if(isCorrect)
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

            ResultCheck();
            AdjustFindCountModeDifficulty();

            //Next, wait until the timer triggers before starting the next game, and no components can be triggered during the countdown period.
            if (IfLimitTime)
            {
                PlayTime = BaseTimeLimit + (Level - MinLevel) * TimeLimitInterval;//Calculate the time limit
                PlayTimer.Stop();//Answer time limit timer stops
            }
            myCanvas.IsEnabled = false;
            IntervalTimer.Start();
        }

        private void CountImageBorderChange()//After the answer is wrong, you need to miss the numberborderTurns yellow
        {
            Image CountingObject = CountBordersList[IndexOfCount].Child as Image;//Find out which number of objects the user is counting
            foreach(var (key,value) in ImageToBordersDict)
            {
                if(((BitmapImage)CountingObject.Source).UriSource == ((BitmapImage)key.Source).UriSource)
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
            if(train_mode==FindMode)
            {
                // Clear the user selected image
                selectedImages?.Clear();
                // Clear the correct picture list
                correctImages?.Clear();

                SelectionCanvas.Children.Clear();//Clear some options that appear on the background image
                TargetItemGrid.Children.Clear();//Clear the pictures to be selected

            }
            else if(train_mode==FindAndCountMode)
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
                if(train_mode==FindMode)
                {
                    AdjustFindModeDifficulty(); //See if you need to adjust the difficulty of the game
                    FoundButton.IsEnabled = false;
                }
                else if(train_mode == FindAndCountMode)
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
            if(train_mode == FindMode)
            {
                FoundButton.IsEnabled = true;
            } 
            if(train_mode == FindAndCountMode)
            {
                myCanvas.IsEnabled = true;
                ErrorCount = 0;//Reset errors
            }
            if(IfLimitTime)
            {
                PlayTimer.Start();//Start answering limit timer
            }

            IntervalTimer.Stop();//Stop the timer

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

        private async void ShowFeedbackImage(Image image, int StopDurations=2000)//StopDurationsThe unit isms
        {//Image showing feedback
            image.Visibility = Visibility.Visible;

            // Delay the specified time（For example, 1 second）
            await Task.Delay(StopDurations);

            image.Visibility = Visibility.Collapsed;
        }


    }
    public partial class Search capability 3 : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
            correctAnswers = new int[MaxLevel + 1];
            wrongAnswers = new int[MaxLevel + 1];
            ignoreAnswers = new int[MaxLevel + 1];
            IfLevelDown = false; IfLevelUp = false;

            max_time = 1; // Total duration of the window, unit minutes
            train_mode = 3; // Game mode, 1, 2 or 3
            Level = 1; // Current game difficulty level

        //Read the database
        {
            // parameter（Includes module parameter information）
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
                        switch (par.ModuleParId)
                        {
                            case 304: // Treatment time
                                max_time = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"max_time={max_time}");
                                break;
                            case 303: // Game Mode
                                train_mode = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"train_mode ={train_mode}");
                                break;
                            case 310: //Game Level
                                Level = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"level ={Level}");
                                break;
                            case 308://Auditory feedback
                                IfAudioFeedBack = par.Value == 1;
                                break;
                            case 307://Visual feedback
                                IfVisualFeedBack = par.Value == 1;
                                break;
                            case 309://Limited answer time
                                IfLimitTime = par.Value == 1;
                                break;
                            case 305://Improve the level
                                LevelUp = par.Value.HasValue ? (int)par.Value.Value : 5;
                                break;
                            case 306://Level down
                                LevelDown = par.Value.HasValue ? (int)par.Value.Value : 5;
                                break;
                            // Add other things that need to be processed ModuleParId
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


        }

            max_hardness = Math.Max(max_hardness, Level);

            LevelStatisticsAction?.Invoke(Level, MaxLevel);
            RightStatisticsAction?.Invoke(0, LevelUp);
            WrongStatisticsAction?.Invoke(0, LevelDown);


            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1); // Triggered once every second
            gameTimer.Tick += GameTimer_Tick;
            timeRemaining = TimeSpan.FromMinutes(max_time); // Set the time when the entire window exists

            if (IfLimitTime)
            {
                PlayTime = BaseTimeLimit + (Level - MinLevel) * TimeLimitInterval;//Calculate the time limit
                PlayTimer = new DispatcherTimer();
                PlayTimer.Interval = TimeSpan.FromSeconds(1);//Set to trigger once after 1 second
                PlayTimer.Tick += PlayTimer_Tick;
            }

            IntervalTimer = new DispatcherTimer();
            IntervalTimer.Tick += IntervalTimer_Tick;

            // Get the current monitor's DPI
            screenDpiHorizontal = GetDpiHorizontal();  // Horizontal direction DPI
            screenDpiVertical = GetDpiVertical();      // Vertical direction DPI
        }

        protected override async Task OnStartAsync()
        {
            /*LJN
            Configure the path to feedback resources             
             */
            CorrectSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");
            // for Image Control loading picture Source
            CorrectImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Correct.png"), UriKind.RelativeOrAbsolute));
            ErrorImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Error.png"), UriKind.RelativeOrAbsolute));


            gameTimer.Start(); // Start timing

            ResetGameState(); // Reset game status
            InitializeGame(); // Start a new game

            // Calling delegate
            SynopsisAction?.Invoke("There are four game modes, and the rules are as follows:\r\n Pattern One: Find missing numbers in the range of numbers and enter them one by one from small to large.\r\nPattern 2: Identify different shapes that are superimposed and select them from the bottom of the screen\r\nMode 3: Find out the position of the object at the bottom of the screen in the picture and select it.\r\nMode 4: Count out and enter the number of times each correct object appears in the picture.");
        }

        protected override async Task OnStopAsync()
        {
            gameTimer.Stop(); // Stop the timer
            if (IfLimitTime) { PlayTimer.Stop(); }
            IntervalTimer.Stop();
        }

        protected override async Task OnPauseAsync()
        {
            gameTimer.Stop(); // Stop the timer
            PlayTimer.Stop();
            IntervalTimer.Stop();
        }

        protected override async Task OnNextAsync()
        {
            gameTimer.Stop(); // Stop the timer
            PlayTimer.Stop();
            IntervalTimer.Stop();
            // Adjust the difficulty
            ResetGameState(); // Reset game status
            InitializeGame(); // Start a new game
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {//Because I gave the body firstinitSo at this timetrain_modeThe value has been read and can be passed
            return new Search ability 3 explanation(train_mode);
        }


        // Insert writing
        private int GetCorrectNum(int difficultyLevel)
        {
            return correctAnswers[difficultyLevel];
        }
        private int GetWrongNum(int difficultyLevel)
        {
            return wrongAnswers[difficultyLevel];
        }
        private int GetIgnoreNum(int difficultyLevel)
        {
            return ignoreAnswers[difficultyLevel];
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

                        int correctCount = 0;
                        int wrongCount = 0;

                        for (int lv = 1; lv <= max_hardness; lv++)
                        {
                            // Get data at the current difficulty level
                            correctCount += GetCorrectNum(lv);
                            wrongCount += GetWrongNum(lv);


                        }
                        int mode = train_mode;
                        int rep = repet_time;
                        int totalCount = wrongCount * (rep + 1);
                        int Count = totalCount + correctCount;
                        double accuracy = Math.Round((double)correctCount / (double)Count, 2);

                        // create Result Record
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "Search capability 3",
                            Eval = false,
                            Lv = max_hardness, // Current difficulty level
                            ScheduleId = BaseParameter.ScheduleId ?? null// Assumption Schedule_id, can be replaced with the actual value
                        };
                        db.Results.Add(newResult);
                        await db.SaveChangesAsync(); //Commented here
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
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "model",
                                    Value = mode,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                 new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "Correct rate",
                                    Value = accuracy * 100, // Stored as a percentage
                                    ModuleId =  BaseParameter.ModuleId
                                },
                                  new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "Total number of opportunities",
                                    Value = totalCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                   new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "Number of opportunities used",
                                    Value = Count,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "Correct times",
                                    Value = correctCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "Errors",
                                    Value = wrongCount,
                                    ModuleId =  BaseParameter.ModuleId
                                }
                            };
                        // insert ResultDetail data
                        db.ResultDetails.AddRange(resultDetails);
                        await db.SaveChangesAsync();
                        // Output each ResultDetail Object data
                        Debug.WriteLine($"Difficulty level {max_hardness}:");
                        foreach (var detail in resultDetails)
                        {
                            Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId} ");
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

            //using (var transaction = await db.Database.BeginTransactionAsync())
            //{
            //    try
            //    {
            //        await Task.Run(async () =>
            //        {
            //            for (int lv = 1; lv <= Level; lv++)
            //            {
            //                // Get data at the current difficulty level
            //                int correctCount = GetCorrectNum(lv);
            //                int wrongCount = GetWrongNum(lv);
            //                int mode = train_mode;
            //                int rep = repet_time;
            //                int totalCount = wrongCount * (rep + 1);
            //                int Count = totalCount + correctCount;
            //                if (correctCount == 0 && wrongCount == 0)
            //                {
            //                    // If all data is 0, skip this difficulty level
            //                    Debug.WriteLine($"Difficulty level {lv}: No data, skip.");
            //                    continue;
            //                }
            //                // Calculation accuracy
            //                double accuracy = Math.Round((double)correctCount / (double)Count, 2);
            //                // create Result Record
            //                var newResult = new Result
            //                {
            //                    ProgramId = program_id, // program_id
            //                    Report = "Search capability 2",
            //                    Eval = false,
            //                    Lv = lv, // Current difficulty level
            //                    ScheduleId = BaseParameter.ScheduleId ?? null// Assumption Schedule_id, can be replaced with the actual value
            //                };
            //                db.Results.Add(newResult);
            //                await db.SaveChangesAsync(); //Commented here
            //                // get result_id
            //                int result_id = newResult.ResultId;
            //                // create ResultDetail Object List
            //                var resultDetails = new List<ResultDetail>
            //                {
            //                   new ResultDetail
            //                    {
            //                        ResultId = result_id,
            //                        ValueName = "grade",
            //                        Value = lv,
            //                        ModuleId = BaseParameter.ModuleId
            //                    },
            //                    new ResultDetail
            //                    {
            //                        ResultId = result_id,
            //                        ValueName = "Table of contents",
            //                        Value = mode,
            //                        ModuleId = BaseParameter.ModuleId
            //                    },
            //                     new ResultDetail
            //                    {
            //                        ResultId = result_id,
            //                        ValueName = "Correct rate",
            //                        Value = accuracy * 100, // Stored as a percentage
            //                        ModuleId =  BaseParameter.ModuleId
            //                    },
            //                      new ResultDetail
            //                    {
            //                        ResultId = result_id,
            //                        ValueName = "Total number of opportunities",
            //                        Value = totalCount,
            //                        ModuleId = BaseParameter.ModuleId
            //                    },
            //                       new ResultDetail
            //                    {
            //                        ResultId = result_id,
            //                        ValueName = "Number of opportunities used",
            //                        Value = Count,
            //                        ModuleId = BaseParameter.ModuleId
            //                    },
            //                    new ResultDetail
            //                    {
            //                        ResultId = result_id,
            //                        ValueName = "Correct times",
            //                        Value = correctCount,
            //                        ModuleId = BaseParameter.ModuleId
            //                    },
            //                    new ResultDetail
            //                    {
            //                        ResultId = result_id,
            //                        ValueName = "Errors",
            //                        Value = wrongCount,
            //                        ModuleId =  BaseParameter.ModuleId
            //                    }
            //                };
            //                // insert ResultDetail data
            //                db.ResultDetails.AddRange(resultDetails);
            //                await db.SaveChangesAsync();
            //                // Output each ResultDetail Object data
            //                Debug.WriteLine($"Difficulty level {lv}:");
            //                foreach (var detail in resultDetails)
            //                {
            //                    Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId} ");
            //                }
            //            }
            //            // Submit transactions
            //            await transaction.CommitAsync();
            //            Debug.WriteLine("Insert successfully");
            //        });
            //    }
            //    catch (Exception ex)
            //    {
            //        // Roll back transactions
            //        await transaction.RollbackAsync();
            //        Debug.WriteLine(ex.ToString());
            //    }
            //}
        }

    }

    public partial class Search capability 3
    {
        /*
         JsonFile reading
         */
        // Define the class to correspond to JSON structure
        public class ImageData
        {
            public BackgroundImageObject BackgroundImage { get; set; }
            public List<ImagesObject> Images { get; set; }
        }

        public class BackgroundImageObject
        {
            public string Path { get; set; }
            public Position PxPosition { get; set; }
            public SizeObject Size { get; set; }
            public string Info { get; set; }
        }

        public class ImagesObject
        {
            public string Path { get; set; }
            public Position PxMargin { get; set; }
            public SizeObject Size { get; set; }
            public string Info { get; set; }
            public int Index { get; set; }
        }

        public class Position
        {//Equivalent tomargin, butpxFor units
            public double Left { get; set; }
            public double Up { get; set; }
            public double Right { get; set; }
            public double Down { get; set; }
        }

        public class SizeObject
        {//bypxFor units
            public double Width { get; set; }
            public double Height { get; set; }

        }

        public class JsonExample
        {
            public static ImageData ReadJson(string filePath)
            {
                // Read JSON File content
                string json = File.ReadAllText(filePath);

                // Will JSON Deserialize strings to ImageData Object
                ImageData imageData = JsonConvert.DeserializeObject<ImageData>(json);

                return imageData;
            }

            public static void WriteJson(string filePath, ImageData imageData)
            {
                // Will ImageData Object serialization to JSON String
                string json = JsonConvert.SerializeObject(imageData, Formatting.Indented);

                // Will JSON String to file
                File.WriteAllText(filePath, json);
            }
        }

        private ImageData imageData;//Used to store data
        private double screenDpiHorizontal;  // Horizontal direction DPI
        private double screenDpiVertical;      // Vertical direction DPI
        public double GetDpiHorizontal()
        {
            var visual = (Visual)Application.Current.MainWindow;
            var transform = PresentationSource.FromVisual(visual).CompositionTarget.TransformToDevice;
            return transform.M11 * 96; // M11 It is the horizontal scaling factor
        }

        public double GetDpiVertical()
        {
            var visual = (Visual)Application.Current.MainWindow;
            var transform = PresentationSource.FromVisual(visual).CompositionTarget.TransformToDevice;
            return transform.M22 * 96; // M22 is the vertical scaling factor
        }

        private void GetItemsReady()//forFindPattern preparation material
        {
            //Prepare the question materials
            string jsonFolderPath = @$"Games/EXO/PicMaterial/{ComplexityFolder}/Json";
            string[] filePaths = Directory.GetFiles(jsonFolderPath);
            var jsonList = filePaths.Select(filePath => System.IO.Path.GetFileName(filePath)).ToList();
            Random rand = new Random();
            string jsonName = jsonList.OrderBy(x => rand.Next()).FirstOrDefault();//Randomly arrange and select the first one
            // File path
            string filePath = @$"Games/EXO/PicMaterial/{ComplexityFolder}/Json/{jsonName}";

            // Read JSON document
            imageData = JsonExample.ReadJson(filePath);
            imagePaths[FindItem].Clear();
            foreach (ImagesObject ImageObject in imageData.Images)
            {
                // You can also add paths to a group in the array element by element.
                imagePaths[FindItem].Add($"EXO/PicMaterial/{ComplexityFolder}/Pic/{System.IO.Path.GetFileNameWithoutExtension(jsonName)}/{ImageObject.Path}");  // Add elements to the first group
            }
            BackGroundPath = $"EXO/PicMaterial/{ComplexityFolder}/Pic/{System.IO.Path.GetFileNameWithoutExtension(jsonName)}/{imageData.BackgroundImage.Path}";
            imagePaths[FindItem].Add(BackGroundPath);//Add background

        }
    } 
}
