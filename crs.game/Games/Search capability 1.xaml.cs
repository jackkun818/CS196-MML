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
using System.Windows.Ink;

namespace crs.game.Games
{
    /// <summary>
    /// EXO.xaml Interaction logic
    /// </summary>
    public partial class Search capability 1 : BaseUserControl
    {
        /*
         Game parameters
         */
        private int max_time = 1; // Total duration of the window, unit minutes
        private int MaxLevel = 18;//The greatest difficulty level, adjustable
        private int LevelUp = 5;//Number of questions to do correctly with level improvement
        private int LevelDown = 5;//The number of questions to be done correctly after level reduction
        private bool IfLimitTime = false;//Is the answer time limit
        private int train_mode = 1; // Game mode, 1, 2, 3, 4	1.	Pattern 1: Find missing numbers in the range of numbers and enter them one by one from small to large. This pattern usually involves the user identifying and entering missing numbers. Mode 2: Identify different shapes that are superimposed on each other and select them from the bottom of the screen. This pattern involves the user's need to find the correct shape from the superimposed shape. Mode 4: Count out and enter the number of times each correct object appears in the picture.
        private int Level = 1; // Current game difficulty level
        private bool IfVisualFeedBack = true;//Visual feedback
        private bool IfAudioFeedBack = true;//Voice feedback
        /*
         Game variables
         */
        private int MinLevel = 1;//Minimum rating
        private List<int> missingNumbers;//Missing numbers
        private List<int> userInputNumbers;//Numbers entered by the user
        private string userInput; // Stores the numbers entered by the user

        private bool is_finish = true;//Is it done

        private int repeat_time = 0;

        private DispatcherTimer gameTimer; // Global timer
        private TimeSpan timeRemaining; // time left

        private DispatcherTimer PlayTimer;//Answer time limit, checkedIfLimitTimeOnly effective
        private int BaseTimeLimit = 60;//The minimum level limit time, seconds
        private int TimeLimitInterval = 5;//Time limit gap between different levels, seconds, arithmetic sequences
        private int PlayTime = 0;//The limited answer time is set according to the level changes

        private DispatcherTimer IntervalTimer;//The interval timer between questions
        private int CorrectInterval = 3;//Answer 3sinterval
        private int ErrorInterval = 5;//Wrong answer 5sinterval

        private int maxnumber = 5; // The maximum number displayed
        private int minnumber = 1;//The minimum number to be displayed
        private int miss_number = 2; // Number of disappearing numbers
        private List<int> AllNumbers = new List<int>();//A list of all numbers used to store
        private int mode1_display_size = 4; // Display box size: 1=Small, 2=Medium, 3=Big, 4=full screen

        private int success_time = 0;//Answer several questions in a row to display stars
        private int fail_time = 0;//Answer several questions in a row to display stars
        private bool IfLevelDown = false;//Do you need to lower the level
        private bool IfLevelUp = false;//Do I need to upgrade
        private Queue<bool> ResultQueue = new Queue<bool>();//Used to store the results of several recent questions, it is used to improve the difficulty of several consecutive answers. If the cumulative answer is correct, you do not need to use it.
        private Dictionary<int, TextBlock> NumTextDict = new Dictionary<int, TextBlock>();//Used to store numbers and correspondingTextblockObject, good index in later

        private int[] correctAnswers;
        private int[] wrongAnswers;
        private int[] ignoreAnswers;

        private int ErrorCount = 2;//How many times can a patient find the wrong number in a question?
        private int ErrorLimit = 2;

        public Search capability 1()
        {
            InitializeComponent();

            this.Cursor = Cursors.None; // Hide the default cursor
            CustomCursor.Visibility = Visibility.Visible; // Show custom cursor
            MouseMove += Window_MouseMove; // subscription MouseMove event
            CustomCursor.Width = 65; // Adjust to the width you want
            CustomCursor.Height = 65; // Adjust to the height you want
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

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            // Reduce remaining time per second
            if (timeRemaining > TimeSpan.Zero)
            {
                timeRemaining = timeRemaining.Add(TimeSpan.FromSeconds(-1));//Synchronous training timing per second
                int remainingSeconds = (int)timeRemaining.TotalSeconds;

                TimeStatisticsAction.Invoke(remainingSeconds, PlayTime);
                //remainingSecondsIt is total, the empty space behind is the current time, displayed on the right side of the patient end
                WrongStatisticsAction?.Invoke(fail_time, LevelDown);
                RightStatisticsAction?.Invoke(success_time, LevelUp);
            }
            else
            {
                gameTimer.Stop(); // Stop the timer

                OnGameEnd();
            }
        }

        private void InitializeGame()
        {
            //ResetGameState(); // Reset status before starting a new game

            //Show corresponding rules
            VoiceTipAction?.Invoke("Please find out the missing numbers in the digit range and enter them one by one from small to large.");
            RuleAction?.Invoke("Find missing numbers in the range of numbers and enter them one by one from small to large.");//Add code, call function, display the text under the digital person
            SetupGameMode1();
            repeat_time += 1;//Record the number of games
        }

        private void ResetGameState()
        {
            // Reset game state variables
            missingNumbers = new List<int>();
            userInputNumbers = new List<int>();
            userInput = string.Empty;
            UpdateTextBlock();
            AdjustDifficulty();
            MainGrid.RowDefinitions.Clear();
            MainGrid.ColumnDefinitions.Clear();

            // Reset mode-specific variables
            is_finish = true;

            // Remove dynamically added UI Elements, preserve static components
            for (int i = MainGrid.Children.Count - 1; i >= 0; i--)
            {
                var child = MainGrid.Children[i];
                if (child != textBlock && child != myCanvas)
                {
                    MainGrid.Children.RemoveAt(i);
                }
            }

            // Reset UI Component visibility
            textBlock.Visibility = Visibility.Collapsed;//Text box above the numeric keypad to display the numbers entered by the user
            myCanvas.Visibility = Visibility.Collapsed;//Where the numeric keypad is locatedcanvas
        }

        private void SetupGameMode1()
        {
            NumTextDict.Clear();
            ErrorCount = ErrorLimit;

            if (IfLimitTime)
            {//If the answer time is limited
                PlayTime = BaseTimeLimit + (Level - MinLevel) * TimeLimitInterval;//Calculate the time limit
                PlayTimer.Start();
            }

            textBlock.Visibility = Visibility.Visible;
            myCanvas.Visibility = Visibility.Visible;

            // Clear the contents of the last game
            MainGrid.Children.Clear();

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

            // Create a transparent rectangle with a tinted border
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
            missingNumbers = RemoveRandomNumbers(numbers);//At this timenumbersIt's been removedmissingNumbersThe list after that is incomplete
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
                    FontSize = Math.Sqrt(width * height / (maxnumber - minnumber) * 0.5) * 0.6, // Make the size of the number not too small
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
                    bool overlapsWithBorder = newRect.Left < borderLeft - 5 || newRect.Top < borderTop - 5 ||
                                              newRect.Right > borderRight - 5 || newRect.Bottom > borderBottom - 5;//-5. Want to add a margin to ensure that you do not touch the boundaries

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
        private void OnNumberButtonClick(object sender, RoutedEventArgs e)//The user enters the number and displays it in the box synchronously
        {
            if (sender is Button button)
            {//So what if the input is a double digit number?
                userInput += button.Content.ToString();
                UpdateTextBlock();
            }
        }

        // "✔" Press the event handler function on the button, that is, submit the answer to this answer
        private void OnSubmitButtonClick(object sender, RoutedEventArgs e)
        {//First of all, users can only submit one answer at a time, and there are only two chances of wrong answers.
            if (!string.IsNullOrEmpty(userInput))
            {
                //First parse the user input intoint
                int number = -1;
                // use TryParse To safely parse integers and avoid exceptions
                if (int.TryParse(userInput, out number))
                {
                    number = int.Parse(userInput);
                }
                else
                {
                    // Handle parsing failure（For example, the input is not a valid number）
                    number = -1; // Or set a default value
                }
                userInputNumbers.Add(number);//Add to the results

                userInput = string.Empty;
                UpdateTextBlock();
            }
            SubmitInput();//Mode 1 Submit input
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
        {//Why notdisplayTextBlock.TextanduserInputBonded
            displayTextBlock.Text = userInput;
        }

        private void SubmitInput()//It is to execute the main logic of judgment in this function
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

        private void EndGame()
        {
            ResetGameState(); // Reset game status
            InitializeGame(); // Start a new game
        }

        private void GroupResultCheck(bool IsNumFeedBack)//Complete the results of this question to determine whether the entire question is right or wrong
        {//IsNumFeedBackIt is used to judge whether a single number needs to be given feedback, which can prevent overlapping with the feedback of the entire question.
            bool IfChecked = false;//Set it to determine whether the entire question has been completed
            if (ErrorCount <= 0)
            {//I found several wrong numbers in this whole question. If I can't, the whole question will be wrong.
                fail_time++; IfChecked = true;
                wrongAnswers[Level] += 1;
                IntervalTimer.Interval = TimeSpan.FromSeconds(ErrorInterval);
                if (IfAudioFeedBack) PlayWav(ErrorSoundPath);
                if (IfVisualFeedBack) ShowFeedbackImage(ErrorImage, ErrorInterval * 1000);

                int ignoreCount = missingNumbers.Count(number => !userInputNumbers.Contains(number));
                ignoreAnswers[Level] += ignoreCount;

                is_finish = false;
            }
            else if (missingNumbers.Count <= 0)
            {//It means that all the numbers have been found, which is equivalent to answering correctly
                success_time++; IfChecked = true;
                correctAnswers[Level] += 1;
                IntervalTimer.Interval = TimeSpan.FromSeconds(CorrectInterval);
                if (IfAudioFeedBack) PlayWav(CorrectSoundPath);
                if (IfVisualFeedBack) ShowFeedbackImage(CorrectImage, CorrectInterval * 1000);
            }

            if (IfChecked)//Only after completing this whole question is the following logic
            {
                ResultCheck();
                //See if you need to adjust the difficulty of the game
                AdjustDifficulty();
                //Next, wait until the timer triggers before starting the next game, and no components can be triggered during the countdown period.
                PlayTime = BaseTimeLimit + (Level - MinLevel) * TimeLimitInterval;//Calculate the time limit
                PlayTimer.Stop();//Answer time limit timer stops
                OverLayGrid.IsEnabled = false;//Cover with a mask to block components
                IntervalTimer.Start();
            }
            else
            {//It means that only requires a single number of judgment feedback
                if (IsNumFeedBack)
                {
                    if (IfAudioFeedBack) PlayWav(CorrectSoundPath);
                    if (IfVisualFeedBack) ShowFeedbackImage(CorrectImage, StopDurations);
                }
                else
                {
                    if (IfAudioFeedBack) PlayWav(ErrorSoundPath);
                    if (IfVisualFeedBack) ShowFeedbackImage(ErrorImage, StopDurations);
                }
            }

        }

        int max_hardness = 1;
        private void AdjustDifficulty()//Mode 1 according tolevelValue to adjust the corresponding parameters
        {
            if (IfLevelUp && Level <= MaxLevel)//If the level of upgrade is sufficient
            {
                if (Level < MaxLevel) { Level += 1; }
                max_hardness = Math.Max(max_hardness, Level);
                IfLevelUp = false;
                ResultQueue.Clear();
                ResultInit();//Each lifting level must be cleared and recalculated
            }
            else if (IfLevelDown && Level >= MinLevel)
            {
                if (Level > MinLevel) { Level -= 1; }
                IfLevelDown = false; ResultQueue.Clear(); ResultInit();
            }
            LevelCheck();
            LevelStatisticsAction?.Invoke(Level, MaxLevel);
        }

        private void LevelCheck()//Observe whether the difficulty level has changed. If so, synchronize to the game parameter changes.
        {
            switch (Level)
            {
                case 1:
                    maxnumber = 5;
                    miss_number = 1;
                    mode1_display_size = 1;
                    break;
                case 2:
                    maxnumber = 7;
                    miss_number = 2;
                    mode1_display_size = 1;
                    break;
                case 3:
                    maxnumber = 8;
                    miss_number = 2;
                    mode1_display_size = 1; break;
                case 4:
                    maxnumber = 9;
                    miss_number = 3;
                    mode1_display_size = 1; break;
                case 5:
                    maxnumber = 10;
                    miss_number = 3;
                    mode1_display_size = 1; break;
                case 6:
                    maxnumber = 12;
                    miss_number = 3;
                    mode1_display_size = 2; break;
                case 7:
                    maxnumber = 14;
                    miss_number = 3;
                    mode1_display_size = 2; break;
                case 8:
                    maxnumber = 16;
                    miss_number = 4;
                    mode1_display_size = 2; break;
                case 9:
                    maxnumber = 18;
                    miss_number = 4;
                    mode1_display_size = 2; break;
                case 10:
                    maxnumber = 20;
                    miss_number = 4;
                    mode1_display_size = 3; break;
                case 11:
                    maxnumber = 24;
                    miss_number = 5;
                    mode1_display_size = 3; break;
                case 12:
                    maxnumber = 28;
                    miss_number = 5;
                    mode1_display_size = 3; break;
                case 13:
                    maxnumber = 30;
                    miss_number = 5;
                    mode1_display_size = 4; break;
                case 14:
                    maxnumber = 35;
                    miss_number = 5;
                    mode1_display_size = 4; break;
                case 15:
                    maxnumber = 38;
                    miss_number = 6;
                    mode1_display_size = 4; break;
                case 16:
                    maxnumber = 40;
                    miss_number = 6;
                    mode1_display_size = 4; break;
                case 17:
                    maxnumber = 45;
                    miss_number = 7;
                    mode1_display_size = 4; break;
                case 18:
                    maxnumber = 50;
                    miss_number = 8;
                    mode1_display_size = 4; break;
                default:
                    maxnumber = 5;
                    miss_number = 1;
                    mode1_display_size = 1; break;
            }
        }

        private void ResultCheck(bool isCorrect)//Determine whether the results of this question need to be recorded in the queue, etc.
        {//How many answers are correct or wrong in the cumulative answer
            ResultQueue.Enqueue(isCorrect);//Join the team
            if (ResultQueue.Count > Math.Max(LevelDown, LevelUp)) { ResultQueue.Dequeue(); }              //Because I joined the team, I need to leave the longest one first
            if (ResultQueue.Count >= Math.Min(LevelDown, LevelUp))
            {//It means that a certain number of questions have been done, and it is necessary to check whether the level of lifting is needed.
                int ErrorNums = 0;
                int CorrectNums = 0;
                //have no ideaLevelDownandLevelUpThe relationship between size and size needs to be discussed in a classified manner
                if (LevelUp - LevelDown > 0)
                {//Explain that the number of questions answered has reachedLevelDownIt needs to be judged whether it is necessary or notLevelDown
                    ErrorNums = ResultQueue.Take(LevelDown).Count(x => !x);
                    if (ResultQueue.Count >= LevelUp)
                    {
                        CorrectNums = ResultQueue.Take(LevelUp).Count(x => x);
                    }
                }
                else if (LevelDown - LevelUp > 0)
                {//Explain that the number of questions answered has reachedLevelUpIt needs to be judged whether it is necessary or notLevelUp
                    CorrectNums = ResultQueue.Take(LevelUp).Count(x => x);
                    if (ResultQueue.Count >= LevelDown)
                    {
                        ErrorNums = ResultQueue.Take(LevelDown).Count(x => !x);
                    }
                }
                else
                {//illustrateLevelDown=LevelUp
                    ErrorNums = ResultQueue.Take(LevelDown).Count(x => !x);
                    CorrectNums = ResultQueue.Take(LevelUp).Count(x => x);
                }
                if (CorrectNums >= LevelUp) { IfLevelUp = true; }
                if (ErrorNums >= LevelDown) { IfLevelDown = true; }
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
            ResultQueue.Clear();
        }

        private void PlayTimer_Tick(object sender, EventArgs e)//Response time limit
        {
            //WillPlayTimeShow it up
            TimeStatisticsAction.Invoke((int)timeRemaining.TotalSeconds, PlayTime);
            PlayTime--;
            if (PlayTime <= 0)//Time has come
            {
                //Forced error
                fail_time++;
                wrongAnswers[Level] += 1;
                IntervalTimer.Interval = TimeSpan.FromSeconds(ErrorInterval);
                if (IfAudioFeedBack) PlayWav(ErrorSoundPath);
                if (IfVisualFeedBack) ShowFeedbackImage(ErrorImage, ErrorInterval * 1000);

                int ignoreCount = missingNumbers.Count(number => !userInputNumbers.Contains(number));
                ignoreAnswers[Level] += ignoreCount;

                is_finish = false;

                ResultCheck();//See if you need to adjust the difficulty of the game
                AdjustDifficulty();
                //Next, wait until the timer triggers before starting the next game, and no components can be triggered during the countdown period.
                PlayTime = BaseTimeLimit + (Level - MinLevel) * TimeLimitInterval;//Calculate the time limit
                OverLayGrid.IsEnabled = false;//Cover with a mask to block components
                IntervalTimer.Start();

                PlayTimer.Stop();
            }
        }

        private void IntervalTimer_Tick(object sender, EventArgs e)//Answer interval timer
        {// This will be triggered to enter the next question
            // Reinitialize game content
            InitializeGame(); // Start the game again
            // Reset user input
            userInputNumbers.Clear();
            userInput = string.Empty;
            UpdateTextBlock();

            OverLayGrid.IsEnabled = true;
            if (IfLimitTime)
            {
                PlayTime = BaseTimeLimit + (Level - MinLevel) * TimeLimitInterval;//Calculate the time limit
                PlayTimer.Start();//Start answering limit timer
            }
            IntervalTimer.Stop();//Stop the timer
        }



        /*LJN
    Added resources for visual and sound feedback
    */
        private SoundPlayer soundPlayer; // Used to sing
        public string ErrorSoundPath;//The wrong sound file path, inOnStartAsync()Medium configuration
        public string CorrectSoundPath;//The correct sound file path is inOnStartAsync()Medium configuration
        private int StopDurations = 1000; // Stop time,ms

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

        private async void ShowFeedbackImage(Image image, int StopDurations)//StopDurationsThe unit isms
        {//Image showing feedback
            image.Visibility = Visibility.Visible;

            // Delay the specified time（For example, 1 second）
            await Task.Delay(StopDurations);

            image.Visibility = Visibility.Collapsed;
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
    }
    public partial class Search capability 1 : BaseUserControl
    {
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

            //Initialize the result array
            correctAnswers = new int[MaxLevel + 1];
            wrongAnswers = new int[MaxLevel + 1];
            ignoreAnswers = new int[MaxLevel + 1];
            IfLevelDown = false; IfLevelUp = false;

            max_time = 1; // Total duration of the window, unit minutes
            train_mode = 1; // Game mode, 1
            Level = 1; // Current game difficulty level

            ResultQueue.Clear();//Clear the results
            repeat_time = 0;
            ErrorCount = ErrorLimit;
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
                                case 292: // Treatment time
                                    max_time = par.Value.HasValue ? (int)par.Value.Value : 1;
                                    Debug.WriteLine($"max_time={max_time}");
                                    break;
                                case 297://Auditory feedback
                                    IfAudioFeedBack = par.Value == 1;
                                    break;
                                case 298://Visual feedback
                                    IfVisualFeedBack = par.Value == 1;
                                    break;
                                case 293://Level improvement
                                    LevelUp = par.Value.HasValue ? (int)par.Value.Value : 5;
                                    break;
                                case 294://Level down
                                    LevelDown = par.Value.HasValue ? (int)par.Value.Value : 5;
                                    break;
                                case 296://Is the answer time limit
                                    IfLimitTime = par.Value == 1;
                                    break;
                                case 295://Preset level
                                    Level = par.Value.HasValue ? (int)par.Value.Value : 1;
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
            gameTimer.Start(); // Start timing

            ResetGameState(); // Reset game status
            InitializeGame(); // Start a new game//It's actually updating the status
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
            // Adjust the difficulty
            InitializeGame(); // Start a new game EndGame();//It's actually updating the status
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new Search ability 1 explanation();
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
                        int rep = repeat_time;
                        int totalCount = wrongCount * (rep + 1);
                        int Count = totalCount + correctCount;
                        double accuracy = Math.Round((double)correctCount / (double)Count, 2);

                        // create Result Record
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "Search capability 1",
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
                                    ValueName = "Correct rate",
                                    Value = accuracy * 100, // Stored as a percentage
                                    ModuleId =  BaseParameter.ModuleId
                                },
                                //  new ResultDetail
                                //{
                                //    ResultId = result_id,
                                //    Order = 1,
                                //    ValueName = "Total number of opportunities",
                                //    Value = totalCount,
                                //    ModuleId = BaseParameter.ModuleId
                                //},
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
        }

    }
}
