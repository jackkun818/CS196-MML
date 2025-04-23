using crs.core.DbModels;
using crs.core;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Media;
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
using System.Runtime.Serialization;


namespace crs.game.Games
{
    /// <summary>
    /// Word memory.xaml Interaction logic
    /// </summary>
    public partial class Word_memory : BaseUserControl
    {
        /*
         * Store game parameters, requiredcoderModify, or read and write from the database
         */
        public int MemoryWordNum = 10;//Element vocabulary, default 5
        public int TrainingMode = 3;//Vocabulary difficulty, 1 simple 2 medium 3 difficult 4 mixed, mixed vocabulary refers to the vocabulary difficulty mixing in the difficulty level table, simple/medium/Difficult vocabulary means that all vocabulary difficulties in the difficulty level table are corresponding to simplicity./medium/difficulty
        public int RunDirection = 0;//Vocabulary movement direction, 1 left or right
        public int RunSpeed = 1;//Speed ​​value, 1-6. The bigger the faster
        public int TreatDurations = 1;//Training time, unit minutes
        public int IfVisionFeedBack = 1;//Visual feedback, 1 has 0 has no
        public int IfAudioFeedBack = 1;//Voice feedback, 1 has 0 has no
        public int IfTextFeedBack = 0;//Text feedback, 1 has 0 or not, 20241206 Text feedback combined with picture feedbackpngIt's inside, no more
        public int SelectedDifficulty = 1;//The selected difficulty level is 1 by default
        public int MemorizeTimeLimit = 1;//The time limit for the memory stage, 1 is limited to several minutes, 0 does not, the default is 1
        public int LevelUp = 3;//Only by answering 3 questions correctly can you improve your level
        public int LevelDown = 3;//Answering 3 questions wrong will reduce the level
        int max_hardness = 0;

        /*
         Game logic processing related functions
         */
        private void CheckIfSet()
        {//Check whether the training settings are selected. If not,
            //Then the number of words and word types are determined based on the difficulty level list and the selected difficulty level.
            if (IfSet == 0)
            {//Changes in requirements, only the speed value needs to be modified

                //MemoryWordNum = (int)LevelTableDifficultyLevel.Rows[SelectedDifficulty - 1][1];
                //TrainingMode = (int)LevelTableDifficultyLevel.Rows[SelectedDifficulty - 1][2];
                //AllowedError = (int)LevelTableDifficultyLevel.Rows[SelectedDifficulty - 1][3];

                RunSpeed = 5;//The default speed value is 5
            }
        }

        private void ShowWordsToMemorize()
        {//Show the words that need to be remembered inWordsToMemorizeTextBLockmiddle
            string RelativePath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Words", $"{GetWordLevel(TrainingMode)}.txt");
            WordsToMemorizeList = LoadPartWords(RelativePath, MemoryWordNum);
            WordsToMemorizeTextBLock.Text = string.Join(" ", WordsToMemorizeList);
            WordsToMemorizeTextBLock.FontSize = FontSizeDict[(int)((LevelTableDifficultyLevel.Rows[SelectedDifficulty - 1])["Number of words"])];
            AllWordList = RemoveCommonElements(LoadAllWords(RelativePath), WordsToMemorizeList);
            MemorizeTipTextBlock.Visibility = Visibility.Visible;//The prompt is visible
            GetWordsReady();//Prepare the words to be displayed
            // Timer for initializing memory stage
            MemorizeSeconds = MemorizeSecondsLimt;//Because it is a countdown, an initial value is required
            MemorizeTimer = new DispatcherTimer();
            MemorizeTimer.Interval = TimeSpan.FromSeconds(1); // In seconds
            MemorizeTimer.Tick += MemorizeTimer_Tick;
            MemorizeTimer.Start();
        }

        private void GetWordsReady()
        {//Used to prepare the words you need to display, throughList<string> AllWordList,List<string> WordsToMemorizeListThese two lists are sorted out
            //existAllWordListRandom draw(WordsGroupNums*MemoryWordNum-MemoryWordNum)indivual
            List<string> OtherWords = GetRandomElements(AllWordList, WordsGroupNums * MemoryWordNum - MemoryWordNum);
            //FinallyOtherWordsandWordsToMemorizeListMerge and disrupt
            Random random = new Random();
            WordsToShow = ((OtherWords.Concat(WordsToMemorizeList).ToList()).OrderBy(x => random.Next())).ToList();
        }

        private void PositionRectangle(int direction)
        {//Show red rectangle(Location of the answer area)
            if (direction == 1)
            {
                Canvas.SetLeft(TargetArea, 40); // WillRectangleMove to the left
            }
            else
            {
                double canvasWidth = WordArea.ActualWidth;
                Canvas.SetLeft(TargetArea, canvasWidth - TargetArea.Width - 40); // WillRectangleMove to the right
            }
            TargetArea.Visibility = Visibility.Visible;
        }

        private void CreateTextBlocksOffScreen()
        {//Put a fewTextBlockThe object is created first and the parameters are adjusted, including initialization
            double canvasHeight = WordArea.ActualHeight;
            double canvasWidth = WordArea.ActualWidth;

            // Fixed width and height for each TextBlock
            double textBlockWidth = 200;
            int NumberOfTextBlocksSet = (WordsToShow.Count < NumberOfTextBlocks) ? WordsToShow.Count : NumberOfTextBlocks;//Check before the program beginstextblockThe relationship between quantity and vocabulary quantity and processing
            for (int i = 0; i < NumberOfTextBlocksSet; i++)
            {
                TextBlock textBlock = new TextBlock
                {
                    //Text = GetRandomWord(AllWordList, WordsToMemorizeList),
                    Text = WordsToShow[0],//Get the first element
                    Background = Brushes.Transparent, // Set transparent background
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Height = canvasHeight,
                    Width = textBlockWidth,
                    FontFamily = new FontFamily("Times New Roman"), // Set fonts
                    FontSize = 160,// Set the font size
                    Visibility = Visibility.Collapsed//Hide it at the beginning
                };
                //If createdNumberOfTextBlocksindivualtext blockWhenWordsToShowNot so many, thenWordsToShowRandomly select
                if (NumberOfTextBlocksSet > WordsToShow.Count)
                {
                    WordsToShow = GetRandomElements(WordsToShow, WordsToShow.Count);//It's equivalent toWordsToShowupset
                }
                else
                {
                    WordsToShow.RemoveAt(0);//To count, each timetextblockoftextAfter assignment, you have toWordsToShowReduce one
                }
                AdjustTextBlockSize(textBlock);

                // Add the TextBlock to Canvas
                WordArea.Children.Add(textBlock);

                // Calculate vertical center position for the TextBlock
                double textBlockHeight = textBlock.Height;
                double verticalCenterPosition = (canvasHeight - textBlockHeight) / 2;

                // Set initial position off-screen 
                textBlockWidth = textBlock.Width;//Adjusted so needs to be updated
                double initialLeftPosition = RunDirection == 1 ? canvasWidth : -textBlockWidth;
                Canvas.SetLeft(textBlock, initialLeftPosition);
                Canvas.SetTop(textBlock, verticalCenterPosition); // Set vertical centering position


                // Will TextBlock The detection state is initialized to false
                TextBlockDetected[textBlock] = false;

                //Add to the list
                CreatedTextBlocks.Add(textBlock);  // Add to list
            }
        }

        private void AnimateTextBlocks(int direction, double speed)
        {//Set each according to the speed valuetextblockAnimation
            double canvasWidth = WordArea.ActualWidth;
            double textBlockWidth = 200;
            double durationInSeconds = 11 - speed; // Speed 1 (slowest) -> 10 seconds, Speed 10 (fastest) -> 1 second

            // Calculate delay per TextBlock to avoid them starting at the same time
            double delayInterval = durationInSeconds / NumberOfTextBlocks;

            for (int i = 0; i < WordArea.Children.Count; i++)
            {
                if (WordArea.Children[i] is TextBlock textBlock)
                {
                    double from = direction == 1 ? canvasWidth : -textBlockWidth;
                    double to = direction == 1 ? -textBlockWidth : canvasWidth;
                    textBlock.Visibility = Visibility.Collapsed;
                    StartTextBlockAnimation(textBlock, from, to, durationInSeconds, TimeSpan.FromSeconds(i * delayInterval));
                }
            }
        }

        private void StartTextBlockAnimation(TextBlock textBlock, double from, double to, double durationInSeconds, TimeSpan beginTime)
        {//For eachtextblock, set and start their animation
            if (AnimationCount >= WordsGroupNums * MemoryWordNum)
            {//If the number of animations is greater than the number of materials for this question, there is no need to start animation again.
                AnimationCount = 0; return;
            }
            else
            {
                // Create and configure the animation
                DoubleAnimation animation = new DoubleAnimation
                {
                    From = from,
                    To = to,
                    Duration = new Duration(TimeSpan.FromSeconds(durationInSeconds)),
                    BeginTime = beginTime
                };

                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, textBlock);
                Storyboard.SetTargetProperty(animation, new PropertyPath(Canvas.LeftProperty));
                // Update text content every time the animation ends
                storyboard.Completed += (s, e) =>
                {
                    if (!IfNextGroup)
                    {//Prevent the material from appearing in a set of questions, but the extra animations repeatedly trigger this part of the logic code after the end
                        WordsAll++; // Number of vocabulary every time there is an animation ending++
                        WordsAllTemp++;

                        if (WordsToMemorizeList.Contains(textBlock.Text) && !TextBlockDetected[textBlock])
                        { // Before updating the content, make a judgment to see if it is a word to remember, and there is no key press
                            WordsIgnore++;
                            WordsIgnoreTemp++;
                        }

                        if (WordsAllTemp >= WordsGroupNums * MemoryWordNum)
                        {//It means that the words for this group of questions are about to be displayed, and it's time to start the next round
                            UpdateGroupResult();//First, based on the answers and answers in this group, increase the corresponding number of correct answers and wrong answers.
                            AdjustDifficulty();//Then select the difficulty of lifting and setting the corresponding parameters according to the number of questions
                            BeginNextGroup();//Turn on the next round of display play
                            ParameterSet();
                            return;
                        }
                        //textBlock.Text = GetRandomWord(AllWordList, WordsToMemorizeList);
                        textBlock.Text = WordsToShow.Count > 0 ? WordsToShow[0] : string.Empty;//Get the first element
                        if (WordsToShow.Count > 0) WordsToShow.RemoveAt(0);//To count, each timetextblockoftextAfter assignment, you have toWordsToShowReduce one
                        AdjustTextBlockSize(textBlock); // Dynamic adjustment TextBlock width
                        StartTextBlockAnimation(textBlock, from, to, durationInSeconds, TimeSpan.Zero); // Restart the painting
                    }

                };

                // Reset detection status before starting the animation
                TextBlockDetected[textBlock] = false;
                TextBlockAnimations.Add(storyboard); // Add animation to list


                // for Canvas Set the clipping area to ensure TextBlock exist Canvas The external part is not visible
                ApplyCanvasClip(WordArea);
                // Check location and update visibility during the animation
                storyboard.CurrentTimeInvalidated += (s, e) =>
                {
                    UpdateTextBlockVisibility(textBlock, WordArea);
                };
                AnimationCount++;//Animation Count
                textBlock.Visibility = Visibility.Visible;//It will be displayed when the animation is really started
                storyboard.Begin();
            }
        }

        // Apply the clipping area to Canvas
        private void ApplyCanvasClip(Canvas containerCanvas)
        {//Through cropping,textblockIn thiscanvasThe part visible is not visible
            // Create a with Canvas Rectangles of the same size
            RectangleGeometry clipGeometry = new RectangleGeometry
            {
                Rect = new Rect(0, 0, containerCanvas.ActualWidth, containerCanvas.ActualHeight)
            };

            // Take the rectangle as Canvas Crop area
            containerCanvas.Clip = clipGeometry;
        }

        private void UpdateTextBlockVisibility(TextBlock textBlock, Canvas containerCanvas)
        {//For the exercisetextblockCheck if they should appear
            // Get TextBlock The current left margin
            double left = Canvas.GetLeft(textBlock);
            double textBlockWidth = textBlock.ActualWidth;
            // Get Canvas Width of
            double canvasLeft = 0; // Assumptions Canvas The left border of 0
            double canvasRight = containerCanvas.ActualWidth;
            // Judgment TextBlock Is it there Canvas Within the scope of
            if (left + textBlockWidth > canvasLeft && left < canvasRight)
            {
                // exist Canvas Within the range, the settings are visible
                textBlock.Visibility = Visibility.Visible;
            }
            else
            {
                // More than Canvas Range, set hidden
                textBlock.Visibility = Visibility.Hidden;
            }
        }

        private void AdjustTextBlockWidth(TextBlock textBlock)
        {//Dynamic adjustmentTextBlockWidth of
            // use FormattedText To measure text width
            var formattedText = new FormattedText(
                textBlock.Text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
                textBlock.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                1);

            // Dynamic adjustment TextBlock Width of
            textBlock.Width = formattedText.Width;
        }

        private void AdjustTextBlockSize(TextBlock textBlock)
        {
            // use FormattedText To measure text size
            var formattedText = new FormattedText(
                textBlock.Text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
                textBlock.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                1);

            // Dynamic adjustment TextBlock Width and height
            textBlock.Width = formattedText.Width;
            textBlock.Height = formattedText.Height;
            textBlock.Visibility = Visibility.Collapsed;

        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {//Countdown starts operation and updates
            if (CountdownSeconds > 0)
            {
                CountdownSeconds--;
                TimeStatisticsAction?.Invoke((int)CountdownSeconds, 0);
                CountdownDisplay.Text = CountdownSeconds.ToString(); // Countdown display on the update interface//No change in demand is shown
            }
            else
            {
                CountdownTimer.Stop();
                // Manually trigger the end button click event
                RoutedEventArgs args = new RoutedEventArgs();
                EndClick(this, args);
            }
        }

        // TimeroftickEvent handler
        private void MemorizeTimer_Tick(object sender, EventArgs e)
        {
            //TimeStatisticsAction?.Invoke(MemorizeSeconds, MemorizeSeconds);//Show on the screen
            MemorizeSeconds--;
            //Is there a memory stage limitation? If so, press manually when it is greater than a certain time.OKButton
            if (MemorizeSeconds <= 0)
            {//Just start to judge after the timeout
                if (MemorizeTimeLimit == 1)
                {//There is a time limit
                    MemorizeOKButtonClick(this, new RoutedEventArgs());//Click manuallyOKButton
                }
            }

        }

        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {// Key detection
            // Check whether the key you pressed is the key you specified
            if (e.Key == System.Windows.Input.Key.Enter) // Suppose the key you specified is the Enter key
            {
                CheckIntersection();//See if there is any intersection
            }
        }

        private async void CheckIntersection()
        {
            // Get Rectangle The boundary of
            double rectLeft = Canvas.GetLeft(TargetArea);
            double rectTop = Canvas.GetTop(TargetArea);
            // Check if it is NaN And give default values
            if (double.IsNaN(rectLeft)) rectLeft = 0;
            if (double.IsNaN(rectTop)) rectTop = 0;
            // create Rectangle boundary
            Rect rectangleBounds = new Rect(rectLeft, rectTop, TargetArea.Width, TargetArea.Height);
            foreach (var child in WordArea.Children)
            {
                if (child is TextBlock textBlock)
                {
                    // if TextBlock Not detected yet
                    if (!TextBlockDetected[textBlock])
                    {
                        // Get TextBlock The boundary of
                        double textBlockLeft = Canvas.GetLeft(textBlock);
                        double textBlockTop = Canvas.GetTop(textBlock);
                        // Check if it is NaN And give default values
                        if (double.IsNaN(textBlockLeft)) textBlockLeft = 0;
                        if (double.IsNaN(textBlockTop)) textBlockTop = 0;
                        // create TextBlock boundary
                        Rect textBlockBounds = new Rect(textBlockLeft, textBlockTop, textBlock.Width, textBlock.ActualHeight);
                        // Check for overlap
                        if (rectangleBounds.IntersectsWith(textBlockBounds))
                        {
                            bool isCorrect = WordsToMemorizeList.Contains(textBlock.Text);
                            // Make judgment and update _ViewModel counter
                            if (isCorrect)
                            {
                                WordsCorrect++; // Update the correct count
                                WordsCorrectTemp++;
                                TargetArea.Stroke = new SolidColorBrush(Colors.Green);
                                if (IfAudioFeedBack == 1) // Whether to make sound
                                {
                                    PlayWav(CorrectSound);
                                }
                                if (IfVisionFeedBack == 1)
                                {
                                    ShowFeedbackImage(CorrectImage);
                                }
                                if (IfTextFeedBack == 1)
                                {
                                    ShowFeedbackTextBlock(CorrectTextBlock); // Show correct text feedback
                                }
                            }
                            else
                            {
                                WordsError++; // Update Error Count
                                WordsErrorTemp++;
                                TargetArea.Stroke = new SolidColorBrush(Colors.Red);
                                if (IfAudioFeedBack == 1) // Whether to make sound
                                {
                                    PlayWav(ErrorSound);
                                }
                                if (IfVisionFeedBack == 1)
                                {
                                    ShowFeedbackImage(ErrorImage);
                                }
                                if (IfTextFeedBack == 1)
                                {
                                    ShowFeedbackTextBlock(ErrorTextBlock); // Show correct text feedback
                                }
                            }
                            // MessageBox.Show(textBlock.Text);
                            TextBlockDetected[textBlock] = true; // Update detection status

                            // Stop all animations
                            foreach (var storyboard in TextBlockAnimations)
                            {
                                storyboard.Pause();
                            }
                            // Delay StopDurations millisecond
                            await Task.Delay(StopDurations);
                            TargetArea.Stroke = new SolidColorBrush(Colors.Black);//Restore color
                            // Restart all animations
                            foreach (var storyboard in TextBlockAnimations)
                            {
                                storyboard.Resume();
                            }

                            break; // Exit the loop after finding an overlapping text
                        }
                    }
                }
            }
        }

        /*This is where I update the difficulty！*/
        private void AdjustDifficulty()
        {//After completing several questions, we need to see if we need to adjust the difficulty level
            if (GroupCorrect >= LevelUp)
            {//If the number of groups of questions you have done correctly reaches a certain number, you can upgrade the level
                if (SelectedDifficulty < MaxLevel) SelectedDifficulty += 1; GroupResultInit();
            }
            else if (GroupError >= LevelDown)
            {//Too many mistakes to reduce
                if (SelectedDifficulty > MinLevel) SelectedDifficulty -= 1; GroupResultInit();
            }
            else
            {
                return;//It means that there are not enough right and not enough wrong, and there is no need to lift and lower, so let's continue
            }
            ParameterSet();// Set parameters according to the difficulty level
            max_hardness = Math.Max(max_hardness, SelectedDifficulty);
        }

        private void BeginNextGroup()
        {//This function is written to get it back to its initial state without changing the parameters
            InitializeComponent();
            //The first thing you see is the memory stage component
            WordsToMemorizeTextBLock.Visibility = Visibility.Visible;
            MemorizeOKButton.Visibility = Visibility.Visible;//Memory stage two components display
            PlayGrid.Visibility = Visibility.Collapsed;
            ShowWordsToMemorize();//Let the words that need to be remembered
            IfHaveStarted = false;
            StartButton.IsEnabled = true;
            // Stop all animations
            foreach (var storyboard in TextBlockAnimations) { storyboard.Remove(); }//Remove animation directly
            // Remove the created TextBlock Object
            foreach (var textBlock in CreatedTextBlocks) { WordArea.Children.Remove(textBlock); }
            CreatedTextBlocks.Clear();  // Clear the list
            // Stop the timer
            if (MemorizeTimer != null && MemorizeTimer.IsEnabled)
            {
                MemorizeTimer.Stop();
                MemorizeTimer.Tick -= MemorizeTimer_Tick;
                MemorizeTimer = null;
                //at this timeMemorizeSecondsIt's the number of seconds to remember
            }
            InitTempResults();//Reset some counts
            IfNextGroup = true;
        }

        private void ParameterSet()
        {//This is after the parameter modification, manually synchronize to the game settings to ensure that the modification takes effect
            switch (TrainingMode)
            {// Only when mixed vocabulary is selected, you need to change the vocabulary difficulty in real time according to the difficulty table. If you don't need any other situations, you can fix it.
                case 1: TrainingMode = 1; break;
                case 2: TrainingMode = 2; break;
                case 3: TrainingMode = 3; break;
                case 4: TrainingMode = (int)LevelTableDifficultyLevel.Rows[SelectedDifficulty - 1][2]; break;
                default: TrainingMode = (int)LevelTableDifficultyLevel.Rows[SelectedDifficulty - 1][2]; break;
            }
            //Mainly set the vocabulary difficulty and the number of vocabulary to be memorized
            MemoryWordNum = (int)LevelTableDifficultyLevel.Rows[SelectedDifficulty - 1][1];
            AllowedError = (int)LevelTableDifficultyLevel.Rows[SelectedDifficulty - 1][3];
            //RunSpeed = 5;//The default speed value is 5
        }

        private void GroupResultInit()
        {//After each increase and decrease the difficulty, the correct and wrong question groups recorded must be updated to zero.
            GroupCorrect = 0;
            GroupError = 0;
        }

        private void UpdateGroupResult()
        {//All the materials and vocabulary of a group of questions have floated over, and it depends on the number of answers and so on to determine whether the group of questions has been correct.
            if (WordsIgnoreTemp > 0 || WordsErrorTemp - AllowedError > 0)
            {//If there is a word, it is necessary to remember, but if it is missed, it will be calculated directly. Or, if there is no missed selection, it will be determined whether there are words other than the vocabulary of the element. If there are and the number of times exceeds the corresponding limit, it will be calculated wrong.
                GroupError++;
            }
            else
            {//The above two judgments can be determined whether the situation is correct
                GroupCorrect++;
            }
            InitTempResults();//Clear these count values
            UpdateDisplay();//Each change is displayed synchronouslyUIGo up
            // And keep records
            AllLevelResult[SelectedDifficulty] = AllLevelResult.GetValueOrDefault(SelectedDifficulty) ?? new Dictionary<string, int>();
            AllLevelResult[SelectedDifficulty]["Correct number of questions"] = AllLevelResult[SelectedDifficulty].GetValueOrDefault("Correct number of questions") + GroupCorrect;
            AllLevelResult[SelectedDifficulty]["Number of wrong questions"] = AllLevelResult[SelectedDifficulty].GetValueOrDefault("Number of wrong questions") + GroupError;
        }

        /*
        Button triggers class function
        */

        private void MemorizeOKButtonClick(object sender, RoutedEventArgs e)
        {//After clicking on memory is completed, the components in the memory stage should be hidden.
            WordsToMemorizeTextBLock.Visibility = Visibility.Collapsed;
            MemorizeOKButton.Visibility = Visibility.Collapsed;//The memory stage two components disappear
            PlayGrid.Visibility = Visibility.Visible;//All components in the training stage are classified into thisgridLet him show
            MemorizeTipTextBlock.Visibility = Visibility.Collapsed;//Tips are not visible
            // Stop the timer
            if (MemorizeTimer != null && MemorizeTimer.IsEnabled)
            {
                MemorizeTimer.Stop();
                MemorizeTimer.Tick -= MemorizeTimer_Tick;
                MemorizeTimer = null;
                //at this timeMemorizeSecondsIt's the number of seconds to remember
            }
            IfNextGroup = false;//Set flag bits
            IfHaveStarted = false;
            StartButton.IsEnabled = true;
        }

        private void StartClick(object sender, RoutedEventArgs e)
        {
            if (IfHaveStarted == false)
            {
                CreateTextBlocksOffScreen();//Create severaltextblock
                //Changes in demand(20241015), the forced direction is from left to right
                RunDirection = 0;//Manual assignment to move to the right
                PositionRectangle(RunDirection);
                AnimateTextBlocks(RunDirection, (double)RunSpeed);

            }
            IfHaveStarted = true;
            StartButton.IsEnabled = false;
        }

        private void EndClick(object sender, RoutedEventArgs e)
        {
            if (RunMode == 1)
            {//It means it is a formal training mode and needs to be turned onreportwindow
                //ReportWindow reportWindow = new ReportWindow(SettingTable, ResultTable);//By the way, twotablePassed over to display
                OnGameEnd();
            }
            else
            {//It means it's just a training mode, it's good to go back to the original appearance
                return;
            }
        }
        /*
         Store temporary variables, no needcoderRevise
         */
        public int RunMode = 1;//What mode to start? 1 formal training 0 is just a practice. 20241205 is an invalid parameter, fixed to 1
        public int IfSet = 0;//Whether the training settings are checked, 1 has 0 or not, only the training settings are not checked,(Vocabulary difficulty, how many mistakes are allowed, etc.)The parameters can be determined based on the difficulty level list. 20241205 is an invalid parameter, fixed to 0

        public int WordsGroupNums = 7;//A set of questions has(7*Element vocabulary)This is how a material vocabulary appears. For example, if there are 3 target memory materials, the total number of materials that appear on the conveyor belt is 3.*7=21, of which 3 target memory materials will appear.

        private List<string> AllWordList = new List<string>();//All word lists read from the file except for words that need to be memorized, are used to display animation
        private List<string> WordsToMemorizeList = new List<string>();//Store a list of words to remember
        private List<string> WordsToShow = new List<string>();//20241205 requirements change, used to save the words that need to be displayed in a list and then display them.

        private string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory; // The absolute path to the current project
        private string ResourcesPath = System.IO.Path.Combine("Resources", "Word memory");

        private bool IfHaveStarted = false;//Have you clicked the Start button
        private bool IfNextGroup = false;//Whether you are ready to display the next question group? Setting this flag is to ensure that you enter the next question group. The logical judgment after the animation ends will not involve restarting it.

        private int NumberOfTextBlocks = 3;//To createTextBlockThe number of default is three
        private Dictionary<TextBlock, bool> TextBlockDetected = new Dictionary<TextBlock, bool>(); // Initialize the detection status dictionary; // Store each TextBlock Detection status
        private List<TextBlock> CreatedTextBlocks = new List<TextBlock>();//Used to store the createdtextblockObject

        private Random RandomObject = new Random();//Random object
        private List<Storyboard> TextBlockAnimations = new List<Storyboard>(); // List stores all animations

        private double CountdownSeconds = 0;//Timer timer count
        private DispatcherTimer CountdownTimer;//Timer object
        private DispatcherTimer MemorizeTimer;//Timer object in memory stage
        private int MemorizeSecondsLimt = 1 * 60;//How long does it take to limit memory?s
        private int MemorizeSeconds = 0;//Time spent on memory stage

        private SoundPlayer soundPlayer; // Used to sing

        public string ErrorSound;//The wrong voice
        public string CorrectSound;//The correct sound
        private int StopDurations = 2000; // Stop time,ms

        private DataTable LevelTableDifficultyLevel = new DataTable();//Level list parameters

        private int TreatNum = 0;//Treatment number, indicating the treatment training starting from the first time
        private string DateNow = DateTime.Now.ToString("yyyy/M/d"); // Get the current date

        public int AllowedError = -1;//The number of wrong words allowed,-1 is not limited to the number of errors

        private int MaxLevel = 30;//The highest difficulty level
        private int MinLevel = 1;//Minimum difficulty level

        private Dictionary<int, Dictionary<string, int>> AllLevelResult = new Dictionary<int, Dictionary<string, int>>();//A dictionary that stores game results at various difficulty levels, using difficulty levelsintTo index the game results, the game results are stored in dictionary form, with the total number of words, the correct number, etc. . .
        private int WordsAllTemp = 0;//The total number of words floating in the material of a set of questions
        private int WordsIgnoreTemp = 0;//The number of words that are ignored in the words that float past the materials of a set of questions
        private int WordsCorrectTemp = 0;//The correct number of words floating in the material of a set of questions
        private int WordsErrorTemp = 0;//The wrong number of words floating in the material of a set of questions

        //Declare the array to store the results
        private int[] correctAnswers;//Not to mention the length
        private int[] wrongAnswers;
        private int[] igonreAnswer;

        //This pile of data from the entire game
        private int WordsAll = 0;//Total number of words floating past
        private int WordsIgnore = 0;//The number of words that are ignored in the words that have passed
        private int WordsCorrect = 0;//The correct number of words in the words that float past
        private int WordsError = 0;//The wrong number of words in the words floating past
        private int AnimationCount = 0;// The number of animations that have been started is used to limit the repetition of unnecessary animations.

        //This pile of data during the game process after grouping one by one in the next period
        private int GroupAll = 0;//How many questions are answered in total
        private int GroupCorrect = 0;//How many groups of questions have been answered correctly
        private int GroupError = 0;//How many wrong questions are answered

        //Different vocabulary sizes need to be remembered for different vocabulary
        private Dictionary<int, int> FontSizeDict = new Dictionary<int, int>
        {//{Number of words, font size}
            {1,100},{2,100},{3,100},{4,100},{5,100},{6,98},{7,80},{8,70},{9,60},{10,55}
        };

        /*
         Store function functions, no needcoderRevise
         */

        private List<string> LoadAllWords(string FileName)
        {//Load all local words
            Encoding encoding = Encoding.UTF8;
            List<string> words = new List<string>();

            // First read all lines in the file
            using (StreamReader file = new StreamReader(FileName))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    words.Add(line.Trim());
                }
            }
            return words;
        }

        private List<string> LoadPartWords(string filename, int count)
        {//Read some words from local
            Encoding encoding = Encoding.UTF8;
            List<string> words = new List<string>();

            // First read all lines in the file
            List<string> allLines = new List<string>();
            using (StreamReader file = new StreamReader(filename))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    allLines.Add(line.Trim());
                }
            }

            // Use the current time as seed
            Random random = new Random();
            int linesToRead = Math.Min(count, allLines.Count);

            // Randomly select unrepeated rows
            for (int i = 0; i < linesToRead; i++)
            {
                int index = random.Next(allLines.Count);
                words.Add(allLines[index]);
                allLines.RemoveAt(index); // Make sure the same line is not repeated
            }
            return words;
        }

        public List<string> RemoveCommonElements(List<string> A, List<string> B)
        {//A-B
            // use Except Method Getting A Does not exist in B Elements
            List<string> result = A.Except(B).ToList();
            return result;
        }

        private List<string> GetRandomElements(List<string> OriginList, int Count)
        {//Randomly disrupt the list and take the frontCountto achieve the purpose of random selection
            Random random = new Random();
            return OriginList.OrderBy(x => random.Next()).Take(Count).ToList();
        }

        static private string GetWordLevel(int TrainingMode = 0)
        {//according toTrainingModeThe choice returns the difficulty string of the vocabulary
            switch (TrainingMode)
            {
                case 1: return "Easy";
                case 2: return "Medium";
                case 3: return "Hard";
                case 4: return "Hybrid";
                default: return "Easy";
            }
        }

        private string GetRandomWord(List<string> AllWordList, List<string> WordsToMemorizeList)
        {//Randomly drawn from two lists, randomly fromAllWordListandWordsToMemorizeListtwolistRandomly picked
            // Make sure both lists are loaded
            if ((AllWordList == null || AllWordList.Count == 0) && (WordsToMemorizeList == null || WordsToMemorizeList.Count == 0))
            {
                return "No Words Loaded";
            }
            // Random selection list, 0 express WordList，1 express _ViewModel.WordsToMemorizeList
            int listSelector = RandomObject.Next(0, 2);
            List<string> selectedList;
            if (listSelector == 0 && AllWordList != null && AllWordList.Count > 0)
            {
                selectedList = AllWordList;
            }
            else if (WordsToMemorizeList != null && WordsToMemorizeList.Count > 0)
            {
                selectedList = WordsToMemorizeList;
            }
            else if (AllWordList != null && AllWordList.Count > 0)
            {
                // If none of the above meets the criteria, use the remaining non-empty list
                selectedList = AllWordList;
            }
            else
            {
                return "No Words Loaded";
            }

            // Select an element randomly from the selected list
            int index = RandomObject.Next(selectedList.Count);
            return selectedList[index];
        }

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

            // Delay the specified time（For example, 2 seconds）
            await Task.Delay(StopDurations);

            image.Visibility = Visibility.Collapsed;
        }

        private async void ShowFeedbackTextBlock(TextBlock textBlock)
        {
            textBlock.Visibility = Visibility.Visible;

            // Delay the specified time（For example, 2 seconds）
            await Task.Delay(StopDurations);

            textBlock.Visibility = Visibility.Collapsed;
        }

        public DataTable DifficultyLevelInit()
        {//Construct the level list
            DataTable LevelTable = new DataTable();

            LevelTable.Columns.Add("grade", typeof(int));
            LevelTable.Columns.Add("Number of words", typeof(int));
            LevelTable.Columns.Add("Word Type", typeof(int));
            LevelTable.Columns.Add("Number of errors allowed", typeof(int));

            LevelTable.Rows.Add(1, 1, 0, 0);
            LevelTable.Rows.Add(2, 1, 1, 0);
            LevelTable.Rows.Add(3, 1, 2, 0);
            LevelTable.Rows.Add(4, 2, 0, 0);
            LevelTable.Rows.Add(5, 2, 1, 0);
            LevelTable.Rows.Add(6, 2, 2, 0);
            LevelTable.Rows.Add(7, 3, 0, 0);
            LevelTable.Rows.Add(8, 3, 1, 0);
            LevelTable.Rows.Add(9, 3, 2, 0);
            LevelTable.Rows.Add(10, 4, 0, 0);
            LevelTable.Rows.Add(11, 4, 1, 0);
            LevelTable.Rows.Add(12, 4, 2, 0);
            LevelTable.Rows.Add(13, 5, 0, 1);
            LevelTable.Rows.Add(14, 5, 1, 1);
            LevelTable.Rows.Add(15, 5, 2, 1);
            LevelTable.Rows.Add(16, 6, 0, 1);
            LevelTable.Rows.Add(17, 6, 1, 1);
            LevelTable.Rows.Add(18, 6, 2, 1);
            LevelTable.Rows.Add(19, 7, 2, 1);
            LevelTable.Rows.Add(20, 7, 1, 1);
            LevelTable.Rows.Add(21, 7, 2, 1);
            LevelTable.Rows.Add(22, 8, 0, 1);
            LevelTable.Rows.Add(23, 8, 1, 1);
            LevelTable.Rows.Add(24, 8, 2, 1);
            LevelTable.Rows.Add(25, 9, 0, 2);
            LevelTable.Rows.Add(26, 9, 1, 2);
            LevelTable.Rows.Add(27, 9, 2, 2);
            LevelTable.Rows.Add(28, 10, 0, 2);
            LevelTable.Rows.Add(29, 10, 1, 2);
            LevelTable.Rows.Add(30, 10, 2, 2);
            return LevelTable;
        }

        public string TrainingModeToString(int TrainingMode)
        {//BundleTrainingModeConvert to string input, easy to fill indatatablein
            switch (TrainingMode)
            {
                case 0: return "Simple vocabulary";
                case 1: return "Medium vocabulary";
                case 2: return "Difficult vocabulary";
                case 3: return "Mixed vocabulary";
                default: return "Simple vocabulary";
            }
        }

        private void UpdateDisplay()
        {//The correct number of correct answers to the question groupUIsuperior
            RightStatisticsAction?.Invoke(GroupCorrect, LevelUp);
            WrongStatisticsAction?.Invoke(GroupError, LevelDown);
            LevelStatisticsAction?.Invoke(SelectedDifficulty, MaxLevel);
        }

        private void InitTempResults()
        {//After switching difficulty level, some quantities need to be reset
            WordsAllTemp = 0;//The total number of words floating in the material of a set of questions
            WordsIgnoreTemp = 0;//The number of words that are ignored in the words that float past the materials of a set of questions
            WordsCorrectTemp = 0;//The correct number of words floating in the material of a set of questions
            WordsErrorTemp = 0;//The wrong number of words floating in the material of a set of questions
        }

        private void AllLevelResultToArray()
        {
            int Length = AllLevelResult.Count;//This refers to how many levels have you played in total
            correctAnswers = new int[Length];
            wrongAnswers = new int[Length];
            igonreAnswer = new int[Length];

            for (int i = 0; i < Length; i++)//Traverse all difficulty levels
            {//iActually it's the difficulty level-1
                Dictionary<string, int> LevelResult = AllLevelResult[i + 1];
                correctAnswers[i] = LevelResult["Correct number of questions"];
                wrongAnswers[i] = LevelResult["Number of wrong questions"];
                //igonreAnswer[i] = LevelResult["Ignore words"];
                igonreAnswer[i] = 0;//This item has not been counted
            }
        }

    }

    public partial class Word_memory : BaseUserControl
    {
        public Word_memory()
        {
            InitializeComponent();
        }

        protected override async Task OnInitAsync()
        {
            InitializeComponent();
            ////The first thing you see is the memory stage component
            WordsToMemorizeTextBLock.Visibility = Visibility.Visible;
            MemorizeOKButton.Visibility = Visibility.Visible;//Memory stage two components display
            PlayGrid.Visibility = Visibility.Collapsed;
            LevelTableDifficultyLevel = DifficultyLevelInit();


            CorrectSound = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSound = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");
            // for Image Control loading picture Source
            CorrectImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Correct.png"), UriKind.RelativeOrAbsolute));
            ErrorImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Error.png"), UriKind.RelativeOrAbsolute));
            bool Left = false;//Vocabulary moves to the left, temporary variables, used to interact with the database without modification
            bool Right = false;

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
                            case 70: // Treatment time 
                                TreatDurations = par.Value.HasValue ? (int)par.Value.Value : 60;
                                Debug.WriteLine($"TreatDurations={TreatDurations}");
                                break;
                            //case 71: // Element vocabulary
                            //    MemoryWordNum = par.Value.HasValue ? (int)par.Value.Value : 10;
                            //    Debug.WriteLine($"MemoryWordNum={MemoryWordNum}");
                            //    break;
                            case 315://Vocabulary Type
                                TrainingMode = par.Value.HasValue ? (int)par.Value.Value : 1;
                                break;
                            case 76://Auditory feedback
                                IfAudioFeedBack = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"IfAudioFeedBack ={IfAudioFeedBack}");
                                break;
                            case 77://Visual feedback
                                IfVisionFeedBack = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"IfVisionFeedBack ={IfVisionFeedBack}");
                                break;
                            case 247://Word movement speed
                                RunSpeed = par.Value.HasValue ? (int)par.Value.Value : 5;
                                Debug.WriteLine($"RunSpeed ={RunSpeed}");
                                break;
                            case 158://Difficulty level
                                SelectedDifficulty = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"SelectedDifficulty ={SelectedDifficulty}");
                                break;
                            case 276://Is it limited memory time
                                MemorizeTimeLimit = par.Value.HasValue ? (int)par.Value.Value : 1;
                                break;
                            case 277:// Level improvement
                                LevelUp = par.Value.HasValue ? (int)par.Value.Value : 3;
                                break;
                            case 278:// Level down
                                LevelDown = par.Value.HasValue ? (int)par.Value.Value : 3;
                                break;
                            // Add other things that need to be processed ModuleParId
                            default:
                                Debug.WriteLine($"Unprocessed ModuleParId: {par.ModuleParId}");
                                break;
                        }

                        //Organize temporary variables
                        if ((bool)Left == true && (bool)Right == false) { RunDirection = 1; }//Words move to the left
                        else if ((bool)Left == true && (bool)Right == false) { RunDirection = 0; }
                        else { RunDirection = 0; }//Forced to move to the right to ensure that the default value exists
                        MemoryWordNum = (int)LevelTableDifficultyLevel.Rows[SelectedDifficulty - 1][1];
                        AllowedError = (int)LevelTableDifficultyLevel.Rows[SelectedDifficulty - 1][3];
                    }
                }
            }
            else
            {
                Debug.WriteLine("No data");
            }
        }

        protected override async Task OnStartAsync()
        {
            // Restart all animations
            if (TextBlockAnimations != null)
            {
                foreach (var storyboard in TextBlockAnimations)
                {
                    storyboard.Resume();
                }
            }

            WordsToMemorizeTextBLock.Visibility = Visibility.Visible;
            MemorizeOKButton.Visibility = Visibility.Visible;//Memory stage two components display
            PlayGrid.Visibility = Visibility.Collapsed;
            CheckIfSet();//Check the parameter settings before displaying the words you need to remember
            ShowWordsToMemorize();//Let the words that need to be remembered

            if (CountdownTimer != null)
            {// Stop and release the previous timer（If so）
                CountdownTimer.Stop();
                CountdownTimer.Tick -= CountdownTimer_Tick;
                CountdownTimer = null;
            }
            // Set the countdown and start the timer
            CountdownSeconds = TreatDurations * 60; // Set countdown time
            CountdownTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1) // The timer triggers once per second
            };
            CountdownTimer.Tick += CountdownTimer_Tick;
            CountdownDisplay.Text = CountdownSeconds.ToString();//20241206 demand changes are not displayed
            CountdownTimer.Start();

            // Calling delegate
            VoiceTipAction?.Invoke("Please find the repeated words from the words that appear on the screen.");
            SynopsisAction?.Invoke("Please remember the words that appear on the screen during the memory stage. After the memory is completed, press the keyboardOKkey. Then there will be a series of words on the screen moving from left to right. When you see the words you remembered in the memory stage and move into the box, press theOKkey.");
            RuleAction?.Invoke("Please remember the words that appear on the screen during the memory stage. After the memory is completed, press the keyboardOKkey. Then there will be a series of words on the screen moving from left to right. When you see the words you remembered in the memory stage and move into the box, press theOKkey.");//Add code, call function, display the text under the digital person
        }

        protected override async Task OnStopAsync()
        {
            CountdownTimer?.Stop();
            // Stop all animations
            foreach (var storyboard in TextBlockAnimations)
            {
                storyboard.Stop();
            }
            TextBlockAnimations.Clear();
            //My function
            AllLevelResultToArray();
        }

        protected override async Task OnPauseAsync()
        {
            CountdownTimer?.Stop();
            foreach (var storyboard in TextBlockAnimations)
            {
                storyboard.Pause();
            }
        }

        protected override async Task OnNextAsync()
        {
            // Restart all animations
            foreach (var storyboard in TextBlockAnimations)
            {
                storyboard.Stop();
            }
            TextBlockAnimations.Clear();
            WordsToMemorizeTextBLock.Visibility = Visibility.Visible;
            MemorizeOKButton.Visibility = Visibility.Visible;//Memory stage two components display
            PlayGrid.Visibility = Visibility.Collapsed;
            CheckIfSet();//Check the parameter settings before displaying the words you need to remember
            ShowWordsToMemorize();//Let the words that need to be remembered
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new Explanation of word memory();
        }

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
            return igonreAnswer[difficultyLevel];
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
                        int ignoreCount = 0;


                        // Calculation accuracy
                        double accuracy = 0;

                        

                        //int correctCount = GetCorrectNum(lv);
                        //int wrongCount = GetWrongNum(lv);
                        //int ignoreCount = GetIgnoreNum(lv);

                        //if (correctCount == 0 && wrongCount == 0 && ignoreCount == 0)
                        //{
                        //    // If all data is 0, skip this difficulty level
                        //    Debug.WriteLine($"Difficulty level {lv}: No data, skip.");
                        //    continue;
                        //}

                        //// Calculation accuracy
                        //double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);

                        foreach (var item in AllLevelResult)
                        {
                            correctCount += GetCorrectNum(item.Key);
                            wrongCount += GetWrongNum(item.Key);
                            ignoreCount += GetIgnoreNum(item.Key);
                        }
                        accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);

                        // create Result Record
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "Vocabulary memory",
                            Lv = max_hardness, // Current difficulty level
                            Eval = false,
                            ScheduleId = BaseParameter.ScheduleId ?? null, // Assumption schedule_id, can be replaced with the actual value

                        };
                        Debug.WriteLine($"Deadline");
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
                                    Order = -1,
                                    ValueName = "grade",
                                    Value = max_hardness,
                                    Maxvalue = 20,
                                    Minvalue = 1,
                                    ModuleId = BaseParameter.ModuleId //  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "Total number vocabulary",
                                    Value = WordsAll,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "mistake",
                                    Value = WordsError,
                                    Maxvalue = WordsError,
                                    Minvalue = 0,
                                    Charttype = "Bar chart" ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "neglect",
                                    Value = WordsIgnore,
                                    Maxvalue = WordsIgnore,
                                    Minvalue = 0,
                                    Charttype = "Bar chart" ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "Memorize time",
                                    Value = MemorizeSeconds,
                                    ModuleId = BaseParameter.ModuleId
                                }
                            };

                        // insert ResultDetail data
                        db.ResultDetails.AddRange(resultDetails);
                        await db.SaveChangesAsync();

                        // Output each ResultDetail Object data
                        Debug.WriteLine($"Difficulty level {max_hardness}:");
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
