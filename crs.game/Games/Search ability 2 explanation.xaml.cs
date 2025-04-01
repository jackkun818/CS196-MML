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
using HandyControl.Controls;
using Window = System.Windows.Window;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static System.Net.Mime.MediaTypeNames;
using Image = System.Windows.Controls.Image;

namespace crs.game.Games
{
    /// <summary>
    /// EXO.xaml Interaction logic
    /// </summary>
    public partial class Search ability 2 explanation : BaseUserControl
    {
        private readonly string[][] imagePaths = new string[][]
        {
            new string[]
            {//This is the path to the colored picture
                    "Games/EXO/color/Figure/Explosion shape_colored.png","Games/EXO/color/Figure/Explosion round shape_colored.png","Games/EXO/color/Figure/Wavy_colored.png","Games/EXO/color/Figure/Title form_colored.png","Games/EXO/color/Figure/Fat cross_colored.png","Games/EXO/color/Figure/Flower shape_colored.png","Games/EXO/color/Figure/prohibit_colored.png","Games/EXO/color/Figure/lightning_colored.png","Games/EXO/color/Figure/Three quarter round_colored.png","Games/EXO/color/Figure/Heart shape_colored.png","Games/EXO/color/Figure/Cloud shape_colored.png",
                    "Games/EXO/color/Figure/sun_colored.png","Games/EXO/color/Geometry/Pentagram_colored.png","Games/EXO/color/Geometry/Pentagram_colored.png","Games/EXO/color/Geometry/Down arrow_colored.png","Games/EXO/color/Geometry/Right arrow_colored.png","Games/EXO/color/Geometry/Lshape_colored.png","Games/EXO/color/Geometry/Octagon_colored.png","Games/EXO/color/Geometry/hexagon_colored.png","Games/EXO/color/Geometry/Heptagon_colored.png","Games/EXO/color/Geometry/Cut round shape_colored.png","Games/EXO/color/Geometry/Decimal shape_colored.png","Games/EXO/color/Geometry/rightVarrow_colored.png","Games/EXO/color/Geometry/Water drops_colored.png",
            },
            new string[]//This is the path to the black picture
            {
                    "Games/EXO/black/Figure/Explosion shape_black.png","Games/EXO/black/Figure/Explosion round shape_black.png","Games/EXO/black/Figure/Wavy_black.png","Games/EXO/black/Figure/Title form_black.png","Games/EXO/black/Figure/Fat cross_black.png","Games/EXO/black/Figure/Flower shape_black.png","Games/EXO/black/Figure/prohibit_black.png","Games/EXO/black/Figure/lightning_black.png","Games/EXO/black/Figure/Three quarter round_black.png","Games/EXO/black/Figure/Heart shape_black.png","Games/EXO/black/Figure/Cloud shape_black.png",
                    "Games/EXO/black/Figure/sun_black.png","Games/EXO/black/Geometry/Pentagram_black.png","Games/EXO/black/Geometry/Down arrow_black.png","Games/EXO/black/Geometry/Right arrow_black.png","Games/EXO/black/Geometry/Lshape_black.png","Games/EXO/black/Geometry/Octagon_black.png","Games/EXO/black/Geometry/hexagon_black.png","Games/EXO/black/Geometry/Heptagon_black.png","Games/EXO/black/Geometry/Cut round shape_black.png","Games/EXO/black/Geometry/Decimal shape_black.png","Games/EXO/black/Geometry/rightVarrow_black.png","Games/EXO/black/Geometry/Water drops_black.png","Games/EXO/black/Geometry/Pentagram_black.png",
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

        private List<Image> correctImages; // List of correct pictures
        private List<Image> selectedImages; // User selected image
        private int right_picture_number = 4; // The correct number of pictures displayed,That is, the number of pictures used to overlap
        private int chose_picture_number = 6; // Number of selectable pictures displayed

        private int success_time = 0;//How many questions should I answer correctly
        private int fail_time = 0;//How many questions are answered wrongly
        private bool IfLevelDown = false;//Is it necessary to downgrade the flag position
        private bool IfLevelUp = false;//Is it necessary to upgrade the level flag position

        private int[] correctAnswers;//Stores the result array for each level
        private int[] wrongAnswers;
        private int[] ignoreAnswers;

        private int ColorMode = 0;//Color mode, 0 colored, 1 black, related to the path stored above

        private Dictionary<Border, bool> BorderStatusDict = new Dictionary<Border, bool>();

        private Dictionary<int, int[]> SelectImageGridStructDict = new Dictionary<int, int[]>//Used to specify how many columns and rows are there in the box below 
        {//The number of shapes available,(Row, column)
                { 3, new int[] {1,3} },
                { 6, new int[] {2,3} },
                { 8, new int[] {2,4} },
                {4*3,new int[] {3,4} },
                {6*3,new int[] {3,6} }
        };

        private double MainGridMaxHeight = 828;//MainGrid What is the maximum visual test
        private double OverLayImageGridHeight = 0;
        private double SelectImageGridHeight = 0;//What are the heights of these two lines? Given a specified value for later calculation

        private int CurrentPage = 1;//Used to indicate which page is currently, and the corresponding components need to be displayed and hidden
        private bool IsCorrect = false;//Used to determine whether the user has answered this question correctly
        public Search ability 2 explanation()
        {
            InitializeComponent();
            //The explanation is different from the ontology, so I have to write it here
            //The game difficulty level, sound feedback and other uses default values, and the global timer and other things will not be initialized.
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
            
            correctImages = new List<Image>();
            selectedImages = new List<Image>();

            PageSwitch();

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

        private void GameTimer_Tick(object sender, EventArgs e)//This timer is used to see how much time is left, and the entire game time
        {
            // Reduce remaining time per second
            if (timeRemaining > TimeSpan.Zero)
            {
                timeRemaining = timeRemaining.Add(TimeSpan.FromSeconds(-1));
                int remainingSeconds = (int)timeRemaining.TotalSeconds;

                TimeStatisticsAction.Invoke(remainingSeconds, PlayTime);
                WrongStatisticsAction?.Invoke(fail_time, LevelDown);
                RightStatisticsAction?.Invoke(success_time, LevelUp);
            }
            else
            {
                gameTimer.Stop(); // Stop the timer
                if (IfLimitTime) { PlayTimer.Stop(); }
                IntervalTimer.Stop();
                OnGameEnd();
            }
        }

        private void InitializeGame()
        {
            //ResetGameState(); // Reset status before starting a new game
            VoiceTipAction?.Invoke("Please identify the different shapes that are superimposed and select them from the bottom of the screen.");
            RuleAction?.Invoke("Identify the different shapes that are superimposed and select them from the bottom of the screen.");
            SetupGameMode2();
        }

        private void SetupGameMode2()
        {
            LevelCheck();
            if (IfLimitTime)
            {//If the answer time is limited
                PlayTime = BaseTimeLimit + (Level - MinLevel) * TimeLimitInterval;//Calculate the time limit
                PlayTimer.Start();
            }
            confirm.Visibility = Visibility.Visible;
            //Panel.SetZIndex(confirm, 999); // 999 It is a relatively large value, ensure it is at the top level
            // Show the correct image of the overlay
            AdjustMainGrid();
            DisplayOverlayImages();
            // Shows available pictures
            DisplayChoiceImages();
        }

        private void AdjustMainGrid()//Dynamically change the row height distribution according to the number of elements to be displayed.
        {
            switch (SelectImageGridStructDict[chose_picture_number][0])
            {//Get the number of rows to be displayed
                case 1:
                    MainGrid.RowDefinitions[0].Height = new GridLength(6.5, GridUnitType.Star);
                    MainGrid.RowDefinitions[1].Height = new GridLength(3.5, GridUnitType.Star);
                    OverLayImageGridHeight = 0.7 * MainGridMaxHeight;
                    SelectImageGridHeight = 0.3 * MainGridMaxHeight;
                    break;
                case 2:
                    MainGrid.RowDefinitions[0].Height = new GridLength(6, GridUnitType.Star);
                    MainGrid.RowDefinitions[1].Height = new GridLength(4, GridUnitType.Star);
                    OverLayImageGridHeight = 0.6 * MainGridMaxHeight;
                    SelectImageGridHeight = 0.4 * MainGridMaxHeight;
                    break;
                case 3:
                    MainGrid.RowDefinitions[0].Height = new GridLength(5, GridUnitType.Star);
                    MainGrid.RowDefinitions[1].Height = new GridLength(5, GridUnitType.Star);
                    OverLayImageGridHeight = 0.5 * MainGridMaxHeight;
                    SelectImageGridHeight = 0.5 * MainGridMaxHeight;
                    break;
                default:
                    MainGrid.RowDefinitions[0].Height = new GridLength(7, GridUnitType.Star);
                    MainGrid.RowDefinitions[1].Height = new GridLength(3, GridUnitType.Star);
                    OverLayImageGridHeight = 0.7 * MainGridMaxHeight;
                    SelectImageGridHeight = 0.3 * MainGridMaxHeight;
                    break;
            }

        }

        private void DisplayOverlayImages()
        {
            // Get all TextBlock element
            var textBlocks = OverLayImageGrid.Children.OfType<TextBlock>().ToList();
            // Clear Grid of Children
            OverLayImageGrid.Children.Clear();
            // Re-add TextBlock element
            foreach (var textBlock in textBlocks){OverLayImageGrid.Children.Add(textBlock);}
            Canvas overlayCanvas = new Canvas
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Width = OverLayImageGridHeight - 10,  // Width can be adjusted according to requirements
                Height = OverLayImageGridHeight - 10  // The height can be adjusted according to the needs
            };

            Random rand = new Random();
            List<int> indices = Enumerable.Range(0, imagePaths[ColorMode].Length).OrderBy(x => rand.Next()).Take(right_picture_number).ToList();

            foreach (int index in indices)
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePaths[ColorMode][index], UriKind.Relative);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                Image img = new Image
                {
                    Source = bitmap,
                    Width = OverLayImageGridHeight - 20,
                    Height = OverLayImageGridHeight - 20,
                    //Opacity = 0.5, // Set transparency
                    RenderTransformOrigin = new Point(0.5, 0.5)
                };

                //// Randomly adjust the position and rotation angle of the picture to produce an overlapping effect
                //double left = rand.Next(50);
                //double top = rand.Next(50);

                //Canvas.SetLeft(img, left);
                //Canvas.SetTop(img, top);
                //No rotation and positional movement are provided, and the centers are overlapped directly, so different shapes are required to distinguish
                correctImages.Add(img);  // Add the correct image to the list
                overlayCanvas.Children.Add(img);
            }

            Grid.SetRow(overlayCanvas, 0);
            OverLayImageGrid.Children.Add(overlayCanvas);//TowardsGridAdd elements to it
        }

        private void DisplayChoiceImages()
        {
            // Get all TextBlock element
            var textBlocks = SelectImageGrid.Children.OfType<TextBlock>().ToList();
            // Clear Grid of Children
            SelectImageGrid.Children.Clear();
            // Re-add TextBlock element
            foreach (var textBlock in textBlocks) { SelectImageGrid.Children.Add(textBlock); }


            Random rand = new Random();
            List<int> indices = new List<int>();

            // First make sure all the correct images displayed overlap are added to the selection list
            foreach (var correctImg in correctImages)
            {
                // Here we directly get the index of the correct image
                int correctIndex = Array.IndexOf(imagePaths[ColorMode], ((BitmapImage)correctImg.Source).UriSource.ToString().Replace("pack://application:,,,", ""));
                indices.Add(correctIndex);
            }

            // Fill in the remaining selection images to ensure the total number is reached chose_picture_number
            while (indices.Count < chose_picture_number)
            {
                int index = rand.Next(imagePaths[ColorMode].Length);
                if (!indices.Contains(index))
                {
                    indices.Add(index);
                }
            }

            // Randomize the order of the pictures
            indices = indices.OrderBy(x => rand.Next()).ToList();

            // Initialize aGrid, used to store these pictures to be selected
            Grid ChoiceGrid = new Grid();
            int Rows = SelectImageGridStructDict[chose_picture_number][0];
            int Columns = SelectImageGridStructDict[chose_picture_number][1];

            for (int i = 0; i < Rows; i++)
            {
                ChoiceGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }
            for (int j = 0; j < Columns; j++)
            {
                ChoiceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            int NumOfImage = 0;//Create this for easy counting, confirmationImagePut in which row and column
            foreach (int index in indices)
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePaths[ColorMode][index], UriKind.Relative);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                Image img = new Image
                {
                    Source = bitmap,
                    Width = (1340 - 10) / (Columns),
                    Height = (SelectImageGridHeight - 10) / (Rows),
                    Margin = new Thickness(2)
                };

                Border border = new Border
                {
                    BorderThickness = new Thickness(5),
                    BorderBrush = Brushes.Transparent, // No border at the beginning
                    Child = img
                };
                BorderStatusDict[border] = false;
                border.MouseLeftButtonDown += Border_MouseLeftButtonDown;//Realize the color change of frame after selection

                // Will Border Add to Grid middle
                Grid.SetRow(border, NumOfImage / Columns);
                Grid.SetColumn(border, NumOfImage % Columns);
                NumOfImage++;
                ChoiceGrid.Children.Add(border);
            }
            SelectImageGrid.Children.Add(ChoiceGrid);
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

        private void confirmButton_Click2(object sender, RoutedEventArgs e)//Only after clicking the button will you determine whether the answer is correct or wrong
        {
            if(IsCorrect==false)
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

                //Correct answer logic
                bool isCorrect = (selectedImages.Count == correctImages.Count) &&
                            selectedImages.All(si => correctImages.Any(ci => ((BitmapImage)si.Source).UriSource == ((BitmapImage)ci.Source).UriSource));
                IsCorrect = IsCorrect | isCorrect;//Use or can guarantee that once one is true,IsCorrectIt's true afterwards
                if (isCorrect)
                {
                    IntervalTimer.Interval = TimeSpan.FromSeconds(CorrectInterval);
                    if (IfAudioFeedBack) PlayWav(CorrectSoundPath);
                    if (IfVisualFeedBack) ShowFeedbackImage(CorrectImage, CorrectInterval * 1000);
                }
                else
                {
                    IntervalTimer.Interval = TimeSpan.FromSeconds(ErrorInterval);
                    if (IfAudioFeedBack) PlayWav(ErrorSoundPath);
                    if (IfVisualFeedBack) ShowFeedbackImage(ErrorImage, ErrorInterval * 1000);
                }
                //ResultCheck();//No more checking results

                //See if you need to adjust the difficulty of the game
                AdjustDifficulty();

                //Next, wait until the timer triggers before starting the next game, and no components can be triggered during the countdown period.
                if (IfLimitTime)
                {
                    PlayTime = BaseTimeLimit + (Level - MinLevel) * TimeLimitInterval;//Calculate the time limit
                    PlayTimer.Stop();//Answer time limit timer stops
                }
                confirm.IsEnabled = false;//Cover with a mask to block components
                SelectImageGrid.IsEnabled = true;
                IntervalTimer.Start();
            }
            else
            {
                OnGameBegin();//Once the answer is correct, click and start the game
            }

        }

        private void ResultCheck()//Determine whether the number of questions you have done correctly in total, and whether you need to upgrade or not
        {
            if (success_time >= LevelUp) { IfLevelUp = true; }
            if (fail_time >= LevelDown) { IfLevelDown = true; }
        }

        private void ResultInit()//The temporary record results must be set to zero after each lifting and lowering.
        {
            success_time = 0;
            fail_time = 0;
        }

        private void LevelCheck()//Observe whether the difficulty level has changed. If so, synchronize to the game parameter changes.
        {
            switch (Level)
            {
                case 1:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    ColorMode = 0;
                    break;
                case 2:
                    right_picture_number = 2;
                    chose_picture_number = 6;
                    ColorMode = 0;
                    break;
                case 3:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    ColorMode = 0;
                    break;
                case 4:
                    right_picture_number = 2;
                    chose_picture_number = 6;
                    ColorMode = 0;
                    break;
                case 5:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    ColorMode = 1;
                    break;
                case 6:
                    right_picture_number = 2;
                    chose_picture_number = 6;
                    ColorMode = 1;
                    break;
                case 7:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    ColorMode = 1;
                    break;
                case 8:
                    right_picture_number = 2;
                    chose_picture_number = 6;
                    ColorMode = 1;
                    break;
                case 9:
                    right_picture_number = 3;
                    chose_picture_number = 6;
                    ColorMode = 0;
                    break;
                case 10:
                    right_picture_number = 3;
                    chose_picture_number = 8;
                    ColorMode = 0;
                    break;
                case 11:
                    right_picture_number = 3;
                    chose_picture_number = 6;
                    ColorMode = 0;
                    break;
                case 12:
                    right_picture_number = 3;
                    chose_picture_number = 8;
                    ColorMode = 0;
                    break;
                case 13:
                    right_picture_number = 3;
                    chose_picture_number = 6;
                    ColorMode = 1;
                    break;
                case 14:
                    right_picture_number = 3;
                    chose_picture_number = 8;
                    ColorMode = 1;
                    break;
                case 15:
                    right_picture_number = 3;
                    chose_picture_number = 6;
                    ColorMode = 1;
                    break;
                case 16:
                    right_picture_number = 3;
                    chose_picture_number = 8;
                    ColorMode = 1;
                    break;
                case 17:
                    right_picture_number = 3;
                    chose_picture_number = 4 * 3;
                    ColorMode = 1;
                    break;
                case 18:
                    right_picture_number = 3;
                    chose_picture_number = 6 * 3;
                    ColorMode = 1;
                    break;
                default:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    ColorMode = 0;
                    break;
            }
        }

        private void AdjustDifficulty()//Mode 1 according tolevelValue to adjust the corresponding parameters
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
            LevelCheck();
            //LevelStatisticsAction?.Invoke(Level, MaxLevel);//Don't show back
        }

        private void ResetGameState()//Reset game status
        {
            // Clear the user selected image
            selectedImages?.Clear();
            // Clear the correct picture list
            correctImages?.Clear();

            OverLayImageGrid.Children.Clear();
            SelectImageGrid.Children.Clear();

            // Reset UI Component visibility
            confirm.Visibility = Visibility.Collapsed;
        }

        private void EndGame()
        {
            ResetGameState(); // Reset game status
            InitializeGame(); // Start a new game
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
                ResultCheck();
                //See if you need to adjust the difficulty of the game
                AdjustDifficulty();
                confirm.IsEnabled = false;//Cover with a mask to block components
                SelectImageGrid.IsEnabled = true;
                IntervalTimer.Start();//Forced to enter the interval timer, thus skipping this question

                PlayTimer.Stop();
            }
        }

        private void IntervalTimer_Tick(object sender, EventArgs e)//Answer interval timer
        {// This will be triggered to enter the next question
            if(!IsCorrect)
            {
                confirm.IsEnabled = true;
                SelectImageGrid.IsEnabled = true;
                if (PlayTimer != null)
                {
                    PlayTimer.Start();//Start answering limit timer
                }
                //Update the game state with this function to update
                ResetGameState(); // Reset game status
                InitializeGame(); // Start a new game
            }
            else
            {
                confirm.Content = "Enter the game";
                confirm.IsEnabled = true;
            }

            IntervalTimer.Stop();//Stop the timer

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

        private async void ShowFeedbackImage(System.Windows.Controls.Image image, int StopDurations)//StopDurationsThe unit isms
        {//Image showing feedback
            image.Visibility = Visibility.Visible;

            // Delay the specified time（For example, 1 second）
            await Task.Delay(StopDurations);

            image.Visibility = Visibility.Collapsed;
        }

        private void LastStep_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage--;
            PageSwitch();
        }

        private void NextStep_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage++;
            PageSwitch();
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {//Skip the entire trial and start the body directly
            OnGameBegin();
        }

        private void Try_Click(object sender, RoutedEventArgs e)
        {
            ExplainViewBox.Visibility = Visibility.Collapsed;
            ExplainButtonsGrid.Visibility = Visibility.Collapsed;
            VoiceTipAction?.Invoke("Please identify the different shapes that are superimposed and select them from the bottom of the screen.");
            SetTitleVisibleAction?.Invoke(true);
            RuleAction?.Invoke("Identify the different shapes that are superimposed and select them from the bottom of the screen.");
            SetupGameMode2();//Display components,Then you can start the game
        }

        private async void PageSwitch()//according topageValue to display components
        {
            switch(CurrentPage)
            {
                case 1://First page, only next step and skip
                    ExplainButtonsGrid.Columns = 2;
                    LastStep.Visibility = Visibility.Collapsed;
                    NextStep.Visibility = Visibility.Visible;
                    Skip.Visibility = Visibility.Visible;
                    Try.Visibility = Visibility.Collapsed;

                    //Next is the issue of picture display
                    OverLay.Visibility = Visibility.Visible;
                    Choice.Visibility = Visibility.Collapsed;
                    Total.Visibility = Visibility.Collapsed;

                    //Next is the prompt display question
                    Tip.Text = "Next you will see the result of overlapping shapes and patterns below";
                    await OnVoicePlayAsync(Tip.Text);
                    break;
                case 2://The second page, both the previous step and the next step and the skip
                    ExplainButtonsGrid.Columns = 3;
                    LastStep.Visibility = Visibility.Visible;
                    NextStep.Visibility = Visibility.Visible;
                    Skip.Visibility = Visibility.Visible;
                    Try.Visibility = Visibility.Collapsed;

                    //Next is the issue of picture display
                    OverLay.Visibility = Visibility.Visible;
                    Choice.Visibility = Visibility.Visible;
                    Total.Visibility = Visibility.Collapsed;

                    //Next is the prompt display question
                    Tip.Text = "Below the overlapping pattern are several shapes to be selected, and the overlapping pattern is made of several overlapping shapes to be selected.";
                    await OnVoicePlayAsync(Tip.Text);
                    break;
                case 3://The third page, only the previous step, try and skip
                    ExplainButtonsGrid.Columns = 3;
                    LastStep.Visibility = Visibility.Visible;
                    NextStep.Visibility = Visibility.Collapsed;
                    Skip.Visibility = Visibility.Visible;
                    Try.Visibility = Visibility.Visible;

                    //Next is the issue of picture display
                    OverLay.Visibility = Visibility.Collapsed;
                    Choice.Visibility = Visibility.Collapsed;
                    Total.Visibility = Visibility.Visible;

                    //Next is the prompt display question
                    Tip.Text = "Please select the pattern you selected with your mouse, click after selectingOKKey confirm answer";
                    await OnVoicePlayAsync(Tip.Text);
                    break;
                default:
                    System.Windows.MessageBox.Show("If you have any problems with your network, please contact the administrator");
                    break;
            }
        }


    }
    public partial class Search ability 2 explanation : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
            correctAnswers = new int[MaxLevel + 1];
            wrongAnswers = new int[MaxLevel + 1];
            ignoreAnswers = new int[MaxLevel + 1];
            IfLevelDown = false; IfLevelUp = false;

            max_time = 30; // Total duration of the window, unit minutes
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
                                case 139: // Treatment time
                                    max_time = par.Value.HasValue ? (int)par.Value.Value : 1;
                                    Debug.WriteLine($"max_time={max_time}");
                                    break;
                                case 177: //Game Level
                                    Level = par.Value.HasValue ? (int)par.Value.Value : 1;
                                    Debug.WriteLine($"Level ={Level}");
                                    break;
                                case 268://Auditory feedback
                                    IfAudioFeedBack = par.Value == 1;
                                    break;
                                case 269://Visual feedback
                                    IfVisualFeedBack = par.Value == 1;
                                    break;
                                case 300://Level improvement
                                    LevelUp = par.Value.HasValue ? (int)par.Value.Value : 5;
                                    break;
                                case 301://Level down
                                    LevelDown = par.Value.HasValue ? (int)par.Value.Value : 5;
                                    break;
                                case 302://Limited answer time
                                    IfLimitTime = par.Value == 1;
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


            // Calling delegate
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
        }

        protected override async Task OnPauseAsync()
        {
            gameTimer.Stop(); // Stop the timer

        }

        protected override async Task OnNextAsync()
        {
            // Adjust the difficulty
            EndGame();
            // Calling delegate
            VoiceTipAction?.Invoke("Test returns voice command information");
            SynopsisAction?.Invoke("Test question description information");
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new Search ability 2 explanation();
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
                        for (int lv = 1; lv <= Level; lv++)
                        {
                            // Get data at the current difficulty level
                            int correctCount = GetCorrectNum(lv);
                            int wrongCount = GetWrongNum(lv);
                            int mode = 2;
                            int rep = 0;
                            int totalCount = wrongCount * (rep + 1);
                            int Count = totalCount + correctCount;
                            if (correctCount == 0 && wrongCount == 0)
                            {
                                // If all data is 0, skip this difficulty level
                                Debug.WriteLine($"Difficulty level {lv}: No data, skip.");
                                continue;
                            }
                            // Calculation accuracy
                            double accuracy = Math.Round((double)correctCount / (double)Count, 2);
                            // create Result Record
                            var newResult = new Result
                            {
                                ProgramId = program_id, // program_id
                                Report = "Search capability 2",
                                Eval = false,
                                Lv = lv, // Current difficulty level
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
                                    ValueName = "grade",
                                    Value = lv,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Table of contents",
                                    Value = mode,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                 new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Correct rate",
                                    Value = accuracy * 100, // Stored as a percentage
                                    ModuleId =  BaseParameter.ModuleId
                                },
                                  new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Total number of opportunities",
                                    Value = totalCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                   new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Number of opportunities used",
                                    Value = Count,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Correct times",
                                    Value = correctCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Errors",
                                    Value = wrongCount,
                                    ModuleId =  BaseParameter.ModuleId
                                }
                            };
                            // insert ResultDetail data
                            db.ResultDetails.AddRange(resultDetails);
                            await db.SaveChangesAsync();
                            // Output each ResultDetail Object data
                            Debug.WriteLine($"Difficulty level {lv}:");
                            foreach (var detail in resultDetails)
                            {
                                Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId} ");
                            }
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
