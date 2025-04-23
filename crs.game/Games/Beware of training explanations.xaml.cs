using crs.core;
using crs.core.DbModels;
using crs.game;
using crs.game.Games;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Identity.Client.NativeInterop;
using Spire.Additions.Xps.Schema.Mc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static Azure.Core.HttpHeader;

namespace crs.game.Games
{
    /// <summary>
    /// VIG.xaml Interaction logic
    /// </summary>
    public partial class Beware_of_training_explanations : BaseUserControl
    {
        //Game parameters that need to be read from the database
        private int Level = 3;//Current game level, 1-9
        private int MaxTime = 30;//Training time, unit minutes
        private double RunSpeedFactor = 1;//Transmission speed, 0.3-2.0, as a transmission factor
        private bool IfVisionFeedBack = true;//Visual feedback, 1 has 0 has no
        private bool IfAudioFeedBack = true;//Voice feedback, 1 has 0 has no
        private int ImageReality = 0;//Image type, 0 abstract, 1 real
        private bool IfItemEqualInterval = true;//Is the spacing between different objects fixed?
        private int LevelUp = 30;//You can upgrade by answering several times in a task
        private int LevelDown = 30;//If you answer a number of mistakes in a task, you will be downgraded

        //Variables required during the program

        //Difficulty level related variables
        private int RefNum = 1;//Number of referenced objects
        private int ChoiceNum = 2;//The number of differentials refers to the number of materials that differ from the referenced object.
        private int ItemsInLevel = 10;//How many levels will there be in totalitemFloating past
        private double ErrorRate = 0.8;//Refers to all that floats pastitemHow many of them are wrong? The ratio should not be selected
        private int ShowTime = 8;//Refers to aitemThe total time required from left to right refers to the different speeds of different levels, units
        private int ShowIntervel = 2;//Refers to differentitemThe interval that appears is checked with whether to check itIfItemEqualIntervalIt's related
        private double MaxIntervel = 3;//When checkeditemThe maximum interval after random interval
        private double MinIntervel = 1;//When checkeditemThe minimum interval after random interval
        private double FixedIntervel = 1.5;//When checkeditemThe interval after equal interval
        private int MaxLevel = 8;//The maximum level is because the material is only ready to level 8
        private int MinLevel = 1;//Minimum level
        private bool IfChange = false;//Does the difficulty level need to be changed

        //Image material related variables
        private string ItemsPath = "VIG/real";
        private List<string> AllItems = new List<string>();//Used to store all the materials under this level, the file name is stored
        private List<string> ReferenceItems = new List<string>();//Store reference materials
        private List<string> NonReferenceItems = new List<string>();//Store non-referenced materials
        private List<string> ToShowItems = new List<string>();//The final need is usedshowThose of
        private bool IfDetailDiff = false;//Whether there are any details of the differentiated types of objects in the difficulty list, and the color outline differences exist in all difficulty levels.

        //Timer-related variables
        private DispatcherTimer MaxTimer = new DispatcherTimer(); // Training timer
        private DispatcherTimer StarTimer = new DispatcherTimer();// Use a timer to regularly display the stars on the therapist side, keeping them in sync
        private int MaxTimeCount = 0;//Game time timing

        //Animation-related variables
        private List<Storyboard> StoryBoardsList = new List<Storyboard>();//Used to enter and exit animation objects
        private List<Storyboard> StoryBoardStartedList = new List<Storyboard>();//Gather the animation objects that have already started together to facilitate pause
        private Dictionary<Image, bool> ItemStatusDict = new Dictionary<Image, bool>();//Used to store eachItemstatus to prevent repeated detection
        private System.Timers.Timer StoryBoardTimer = new System.Timers.Timer(1);//Create a timer to start the startup picture in time
        private int StoryBoardIndex = 0;//What is the animation to start?
        private Random RandomObject = new Random();//Random objects used
        private double RandomIntervel = 0;//Record the current random interval
        private int CurrentTime = 0;//How many counts have been reached?ms
        private bool IfPause = false;//Whether it is currently in the pause interval of correct answers or wrong answers is related toCurrentTimeWhether the count will continue to grow
        private Dictionary<Storyboard, Image> StoryBoardToImageDict = new Dictionary<Storyboard, Image>();//Store fromstoryboardarriveimgMapping of

        //feedback
        private SoundPlayer soundPlayer; // Used to sing
        public string ErrorSoundPath;//The wrong sound file path, inOnStartAsync()Medium configuration
        public string CorrectSoundPath;//The correct sound file path is inOnStartAsync()Medium configuration
        private int StopDurations = 1000; // Stop time,ms
        private string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory; // The absolute path to the current project
        private string ResourcesPath = System.IO.Path.Combine("Resources", "Word memory");//The fixed path used here is the memory path of word, which can be changed later.

        //Declare the array to store the results
        private int[] correctAnswers;//Not to mention the length
        private int[] wrongAnswers;
        private int[] ignoreAnswers;
        private int LevelRight = 0;//Floating pastItemsInLevelindivualItemHow many answers are correct? The difficulty level is updated and cleared by 0
        private int LevelWrong = 0;//Floating pastItemsInLevelindivualItemHow many answers are wrong? The difficulty level is updated and cleared by 0.
        private int LevelIgnore = 0;//Floating pastItemsInLevelindivualItemHow many missing answers have been answered? The difficulty level has been updated and cleared by 0.

        //Configuration variables used to turn pages during explanation
        private int CurrentPage = -1;
        private List<string> PicPathList = Directory.GetFiles(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Games", "VIG", "explain")).ToList();//Get all material file names of the specified level;
        private string CurrentPicPath;//The picture path currently displayed

        private bool IfTryClicked = false;

        public Beware_of_training_explanations()
        {
            InitializeComponent();
            //The first thing I entered is the trial explanation part
            CurrentPage = 1;
            ExplainGrid.Visibility = Visibility.Visible;
            PlayGrid.Visibility = Visibility.Collapsed;
            PageSwitch();//Go to the first page
        }

        private void LevelSet()//Adjustment completedLevelAfterwards, manually update parameters before the next round of game starts
        {
            //Mandatory stageLevel=2
            Level = 2;
            switch (Level)
            {
                case 1:
                    RefNum = 1;
                    ChoiceNum = 2;
                    IfDetailDiff = false;
                    ErrorRate = 0.33;
                    ShowTime = 8;
                    //besidesitemThe distance between needs to be set
                    MaxIntervel = 3;
                    MinIntervel = 1;
                    FixedIntervel = 1.5;
                    break;
                case 2:
                    RefNum = 1;
                    ChoiceNum = 3;
                    IfDetailDiff = true;
                    ErrorRate = 0.30;
                    ShowTime = 8;
                    //besidesitemThe distance between needs to be set
                    MinIntervel = 1;
                    MaxIntervel = 3;
                    FixedIntervel = 1.5;
                    break;
                case 3:
                    RefNum = 2;
                    ChoiceNum = 4;
                    IfDetailDiff = false;
                    ErrorRate = 0.28;
                    ShowTime = 7;
                    //besidesitemThe distance between needs to be set
                    MinIntervel = 1.5;
                    MaxIntervel = 4;
                    FixedIntervel = 2;
                    break;
                case 4:
                    RefNum = 2;
                    ChoiceNum = 6;
                    IfDetailDiff = true;
                    ErrorRate = 0.26;
                    ShowTime = 7;
                    //besidesitemThe distance between needs to be set
                    MinIntervel = 1.5;
                    MaxIntervel = 4;
                    FixedIntervel = 2;
                    break;
                case 5:
                    RefNum = 2;
                    ChoiceNum = 4;
                    IfDetailDiff = false;
                    ErrorRate = 0.24;
                    ShowTime = 7;
                    //besidesitemThe distance between needs to be set
                    MinIntervel = 2;
                    MaxIntervel = 5;
                    FixedIntervel = 3;
                    break;
                case 6:
                    RefNum = 2;
                    ChoiceNum = 6;
                    IfDetailDiff = true;
                    ErrorRate = 0.22;
                    ShowTime = 7;
                    //besidesitemThe distance between needs to be set
                    MinIntervel = 2;
                    MaxIntervel = 6;
                    FixedIntervel = 3;
                    break;
                case 7:
                    RefNum = 3;
                    ChoiceNum = 6;
                    IfDetailDiff = true;
                    ErrorRate = 0.2;
                    ShowTime = 6;
                    //besidesitemThe distance between needs to be set
                    MinIntervel = 2;
                    MaxIntervel = 8;
                    FixedIntervel = 3.5;
                    break;
                case 8:
                    RefNum = 3;
                    ChoiceNum = 9;
                    IfDetailDiff = true;
                    ErrorRate = 0.15;
                    ShowTime = 6;
                    //besidesitemThe distance between needs to be set
                    MinIntervel = 2;
                    MaxIntervel = 9;
                    FixedIntervel = 4;
                    break;
                case 9:
                    RefNum = 3;
                    ChoiceNum = 9;
                    IfDetailDiff = true;
                    ErrorRate = 0.1;
                    ShowTime = 6;
                    //besidesitemThe distance between needs to be set
                    MinIntervel = 2;
                    MaxIntervel = 10;
                    FixedIntervel = 4;
                    break;
                default:
                    MessageBox.Show("If you have any problems with your network, please contact the administrator！Error:LevelSet()");
                    break;
            }
            ReadItems();//Change every timeLevelThenListGet ready
        }

        private void ReadItems()//Put localitemRead into memory according to the specified level and place itAllItemsmiddle
        {
            ItemsPath = System.IO.Path.Combine(BaseDirectory, "Games", "VIG", "real", $"{Level}");
            AllItems = Directory.GetFiles(ItemsPath).ToList();//Get all material file names of the specified level
            //Read in and it's the complete path
            ReferenceItems = AllItems.Where(file => file.Contains("ref")).ToList();//Read the reference picture in
            NonReferenceItems = AllItems.Where(file => !file.Contains("ref")).ToList();//Read the reference picture in
        }

        private void ShowRefItem()//Will getRefItemDisplayed on the specified component
        {
            ReferenceInit();//Clear it before each display
            ReferenceItemGrid.Columns = ReferenceItems.Count;//Number of columns=Need to be displayeditemNumber, more beautiful
            foreach (var imgName in ReferenceItems)
            {
                Image correctImg = new Image
                {
                    Source = new BitmapImage(new Uri(imgName, UriKind.Absolute)),
                    Stretch = Stretch.Uniform,
                    Visibility = Visibility.Visible,
                    Tag = imgName
                };
                ReferenceItemGrid.Children.Add(correctImg);//Add to thisuniformgridinside
            }
        }

        private void ConveyArea_Loaded(object sender, RoutedEventArgs e)
        {
            //Call manuallyShowSelectableItem()Just
            PlayGrid.Visibility = Visibility.Visible;
            ExplainGrid.Visibility = Visibility.Collapsed;
            //ShowSelectableItem();
        }

        private List<string> GetRandomSamples(List<string> items, int count) //fromLIstRandom sampling
        {
            List<string> samples = new List<string>();
            for (int i = 0; i < count; i++)
            {
                int index = RandomObject.Next(items.Count);
                samples.Add(items[index]);
            }
            return samples;
        }

        private void GetItemsToShow()//JudgmentToShowItemsHave the number reached the specifiedItemsInLevelnumber
        {
            int NonRefNum = (int)Math.Ceiling(ItemsInLevel * ErrorRate);
            int RefNum = ItemsInLevel - NonRefNum;
            ToShowItems.AddRange(GetRandomSamples(NonReferenceItems, NonRefNum));//Are the defective ones on the assembly line that patients should choose
            ToShowItems.AddRange(GetRandomSamples(ReferenceItems, RefNum));
            ToShowItems = ToShowItems.OrderBy(x => RandomObject.Next()).ToList();//Random disruption
        }

        private void ShowSelectableItem()//WillitemGet it to convey one by one on the conveyor belt
        {
            ConveyAreaInit();//Clear the conveyor belt
            GetItemsToShow();
            foreach (var imgName in ToShowItems)
            {
                Image img = new Image
                {
                    Source = new BitmapImage(new Uri(imgName, UriKind.Absolute)),
                    Tag = imgName,
                    Visibility = Visibility.Collapsed,
                    //Height = ConveyArea.ActualHeight,//This value is actuallyConveyArea.Height,Need to read it yourself
                    Height = 285
                };

                img.Loaded += Img_Loaded;
                ConveyArea.Children.Add(img);
            }
            StartStoryBoardsTimer();
        }

        private void Img_Loaded(object sender, RoutedEventArgs e)
        {
            Image img = sender as Image;

            ItemStatusDict[img] = false;//Detection status becomes
            DoubleAnimation animation = new DoubleAnimation
            {
                From = -300,//Move from the leftmost side of the conveyor belt to the rightmost side
                //To = ConveyArea.ActualWidth,
                To = 1340,
                Duration = new Duration(TimeSpan.FromSeconds(ShowTime / RunSpeedFactor))
            };

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            StoryBoardToImageDict[storyboard] = img;
            storyboard.Completed += (s, e) => StoryBoardEnd(img, storyboard);//Things to call at the end of the animation

            CompositionTarget.Rendering += (sender, e) =>
            {// register CompositionTarget.Rendering event
                double imgLeft = Canvas.GetLeft(img);                        // Get the current one Image The left position
                if (imgLeft > Canvas.GetLeft(TargetRect) + TargetRect.ActualWidth)// Determine whether it exceeds the right boundary
                {
                    // Execute logical code blocks
                    CheckIfIgnore(img);
                }
            };
            Storyboard.SetTarget(animation, img);
            Storyboard.SetTargetProperty(animation, new PropertyPath(Canvas.LeftProperty));//This means DoubleAnimation The animation will act on img of Canvas.Left Attributes, that is, change img exist Canvas horizontal position on.
            StoryBoardsList.Add(storyboard); // Add the animation to the list, and then unify it into a certain function and start in a certain order.
            ApplyCanvasClip(ConveyArea);//Realize cutting, ensurecanvasThe outer part is not visible to prevent the exposing
        }

        private void StoryBoardEnd(Image img, Storyboard storyboard)//What to fire after each animation is finished
        {
            StoryBoardStartedList.Remove(storyboard);//Remove this animation object from the list
            ConveyArea.Children.Remove(img);//Put thisimgfromcanvasRemoved in
            ItemStatusDict.Remove(img);//Delete this key-value pair
            if (ItemStatusDict.Count <= 0)
            {//Explain that all created animations are over
                //During the trial, you should determine whether the patient has answered correctly. If the answer is correct, you can enter the body.
                if (LevelRight > 0)
                {// You can enter the body, don't repeat it
                    TipTextBlock.Text = "After the trial play is over, please click the button below to enter the training.";
                    StartTheGameButton.Visibility = Visibility.Visible;
                    //Display button, guide the button
                }
                else
                {
                    Init();//Resource Initialization
                    LevelSet();//Make the parameters effective
                    ShowRefItem();//showrefItems
                    IfTryClicked = true;
                    ShowSelectableItem();

                }

            }
        }

        private void IfLevelChange()//Judging whether the level has increased based on the answer results
        {
            if (LevelRight >= LevelUp && Level < MaxLevel)
            {//Upgrade priority
                Level += 1; LevelSet(); IfChange = true;
            }
            else if (LevelWrong >= LevelDown && Level > MinLevel)
            {
                Level -= 1; LevelSet(); IfChange = true;
            }
        }

        private void StartStoryBoardsTimer()//Start and end the animations in the animation list in a certain order
        {
            StoryBoardTimer.Start();//Start timing and prepare to start animation
        }

        private void ApplyCanvasClip(Canvas containerCanvas)//Through cropping,imageThe object is in thiscanvasThe part visible is not visible
        {
            // Create a with Canvas Rectangles of the same size
            RectangleGeometry clipGeometry = new RectangleGeometry
            {
                Rect = new Rect(0, 0, containerCanvas.ActualWidth, containerCanvas.ActualHeight)
            };
            // Take the rectangle as Canvas Crop area
            containerCanvas.Clip = clipGeometry;
        }

        private async void CheckIntersection()//Determine whether it is crossed
        {
            // Get Rectangle The boundary of            // Check if it is NaN And give default values
            double rectLeft = Canvas.GetLeft(TargetRect); if (double.IsNaN(rectLeft)) rectLeft = 0;
            double rectTop = Canvas.GetTop(TargetRect); if (double.IsNaN(rectTop)) rectTop = 0;

            // create Rectangle boundary
            Rect rectangleBounds = new Rect(rectLeft, rectTop, TargetRect.ActualWidth, TargetRect.ActualHeight);
            foreach (var child in ConveyArea.Children)
            {
                if (child is Image img)
                {
                    // if img Not detected yet
                    if (ItemStatusDict[img] == false)
                    {
                        // Get img The boundary of                        // Check if it is NaN And give default values
                        double imgLeft = Canvas.GetLeft(img); if (double.IsNaN(imgLeft)) imgLeft = 0;
                        double imgTop = Canvas.GetTop(img); if (double.IsNaN(imgTop)) imgTop = 0;

                        Rect imgBounds = new Rect(imgLeft, imgTop, img.ActualWidth, img.ActualHeight);
                        // Check for overlap
                        if (rectangleBounds.IntersectsWith(imgBounds))
                        {
                            //Make a judgment on whether it is correct
                            if (ReferenceItems.Contains(img.Tag))
                            {//in the case ofrefItemNo.1
                                wrongAnswers[Level] += 1;
                                LevelWrong += 1;
                                TargetRect.Stroke = new SolidColorBrush(Colors.Red);
                                if (IfAudioFeedBack == true) { PlayWav(ErrorSoundPath); }
                                if (IfVisionFeedBack == true) { ShowFeedbackImage(ErrorImage); }
                            }
                            else
                            {//It means that the patient has successfully found those defects in the assembly line
                                correctAnswers[Level] += 1;
                                LevelRight += 1;
                                TargetRect.Stroke = new SolidColorBrush(Colors.Green);
                                if (IfAudioFeedBack == true) { PlayWav(CorrectSoundPath); }
                                if (IfVisionFeedBack == true) { ShowFeedbackImage(CorrectImage); }

                            }

                            ItemStatusDict[img] = true; // Update detection status

                            // Stop all animations
                            PauseStoryBoards();
                            // Delay StopDurations millisecond
                            IfPause = true;//This flag will involve the trigger of the timer.StoryBoardTimer_Tick
                            await Task.Delay(StopDurations);
                            IfPause = false;
                            //TargetRectRestore color
                            TargetRect.Stroke = new SolidColorBrush(Colors.Black);
                            // Restart all animations
                            ResumeStoryBoards();
                            return; // Find an overlapping oneitemExit the loop later
                        }
                    }
                }
            }
        }

        private void CheckIfIgnore(Image img)//JudgmentimgIs there still a miss after moving to the right
        {
            if (ItemStatusDict.ContainsKey(img) && ItemStatusDict[img] == false)
            {//Not checked
                if (!ReferenceItems.Contains(img.Tag))
                {//And notrefItemOne of them means that the patient still has not found the defective product to be found
                    TargetRect.Stroke = new SolidColorBrush(Colors.DarkGray);//The frame changes color to remind
                    ignoreAnswers[Level] += 1;
                    LevelIgnore += 1;
                    ItemStatusDict[img] = true;
                }
            }
        }

        private void PauseStoryBoards()//pauseStoryBoardStartedListAll animations in
        {
            foreach (Storyboard StartedStoryBoard in StoryBoardStartedList)
            {
                StartedStoryBoard.Pause();
            }
        }

        private void ResumeStoryBoards()//restartStoryBoardStartedListAll animations in
        {
            foreach (Storyboard StartedStoryBoard in StoryBoardStartedList)
            {
                StartedStoryBoard.Resume();
            }
        }

        private void Init()//The total initial initialization function
        {
            ItemsInit();
            ReferenceInit();
            ConveyAreaInit();
            TimerInit();
            FeedBackInit();
            AnswersInit();
            LevelResultInit();
            IfChange = false; IfPause = false;
            TargetRect.Stroke = new SolidColorBrush(Colors.Black);
        }

        private void ReInit()//After the game starts, some variables are not required to be initialized before the next round of game starts.
        {

            ItemsInit();
            ReferenceInit();
            ConveyAreaReInit();
            TimerReInit();
            LevelResultInit();
            IfChange = false; IfPause = false;
            TargetRect.Stroke = new SolidColorBrush(Colors.Black);
        }

        private void StartGame()//After initialization, the function called
        {
            ReadItems();
            ShowRefItem();//showrefItems
            ShowSelectableItem();//Showcase what is on the conveyor belt
            //StoryBoardTimer.Elapsed += StoryBoardTimer_Elapsed;
        }

        private void ItemsInit()//Picture material initialization
        {
            AllItems = new List<string>();//Used to store all the materials under this level, the file name is stored
            ReferenceItems = new List<string>();//Store reference materials
            NonReferenceItems = new List<string>();
            ToShowItems = new List<string>();
            StoryBoardsList = new List<Storyboard>();
            StoryBoardStartedList = new List<Storyboard>();
            ItemStatusDict.Clear();
            StoryBoardIndex = 0;
            RandomIntervel = 0;
            CurrentTime = 0;
            StoryBoardToImageDict.Clear();
        }

        private void ReferenceInit()//Initialization of those cited images
        {//Actually, just clear it
            ReferenceItemGrid.Children.Clear();
        }

        private void ConveyAreaInit()//Clear the contents on the conveyor belt
        {
            ConveyArea.Loaded += ConveyArea_Loaded;
            ConveyArea.Children.Clear();
            StoryBoardsList.Clear();//The animation is clear
            StoryBoardIndex = 0;//Index setting 0
        }

        private void ConveyAreaReInit()//Re-empty the contents on the conveyor belt
        {
            ConveyArea.Children.Clear();
            StoryBoardsList.Clear();//The animation is clear
            StoryBoardIndex = 0;//Index setting 0
        }

        private void TimerInit()//Timer-related initialization
        {
            //Training timer initialization
            MaxTimer = new DispatcherTimer();
            MaxTimer.Interval = TimeSpan.FromSeconds(1);  // Set 1sUpdate once
            MaxTimer.Tick += MaxTimer_Tick;  // Bind Tick event
            MaxTimeCount = MaxTime * 60;// Convert to seconds

            //Therapist's end star display timer initialization
            StarTimer = new DispatcherTimer();
            StarTimer.Interval = TimeSpan.FromSeconds(1);//1sUpdate once
            StarTimer.Tick += StarTimer_Tick;

            //Animation timer initialization
            StoryBoardTimer = new System.Timers.Timer(1);//1msTrigger once to facilitate the start animation of the time interval
            //useDispatcherTimerCan't do 1 at allmsTrigger once, so useSystem.Timers.Timer
            StoryBoardTimer.Elapsed += StoryBoardTimer_Elapsed;
            CurrentTime = 0;//Time count value is zeroed
        }

        private void StarTimer_Tick(object sender, EventArgs e)
        {
            //Because the level can only be upgraded after the animation is over
            if (LevelRight >= LevelUp) { LevelRight = LevelUp; }
            if (LevelWrong >= LevelDown) { LevelWrong = LevelDown; }
            // Calling delegate
            LevelStatisticsAction?.Invoke(Level, MaxLevel);
            RightStatisticsAction?.Invoke(LevelRight, LevelUp);
            WrongStatisticsAction?.Invoke(LevelWrong, LevelDown);
        }

        private void TimerReInit()//The training event timer does not need to be initialized anymore
        {
            //Animation timer initialization
            StoryBoardTimer.Stop();

            CurrentTime = 0;//Time count value is zeroed
        }

        private void AnswersInit()//Initialization of the array that stores the results
        {
            correctAnswers = new int[MaxLevel + 1];
            wrongAnswers = new int[MaxLevel + 1];
            ignoreAnswers = new int[MaxLevel + 1];
            LevelRight = 0; LevelWrong = 0; LevelIgnore = 0;
        }

        private void LevelResultInit()//Clear the level result after the difficulty level is updated
        {
            LevelRight = 0; LevelWrong = 0; LevelIgnore = 0;
        }

        private void StoryBoardTimer_Elapsed(object sender, EventArgs e)//Used to trigger the start of the animation regularly
        {
            Application.Current.Dispatcher.Invoke(async () =>//Added one hereasyncVery important
            {//Switch back to main thread
                if (IfPause)
                {//If it is a pause phase,Pause all animations to simplify
                    PauseStoryBoards();
                }
                else
                {//If not, count normally
                    CurrentTime += 1;
                    if (IfItemEqualInterval)
                    {//If equal interval
                        if (Math.Abs(CurrentTime - FixedIntervel / RunSpeedFactor * 1000) <= 1)
                        {//The timing has reached a fixed interval, and it's time to start another animation
                            if (StoryBoardIndex >= StoryBoardsList.Count)
                            {                            // Check if all animations have started
                                StoryBoardTimer.Stop(); // Stop the timer
                                StoryBoardTimer.Elapsed -= StoryBoardTimer_Elapsed;
                                return;
                            }
                            StoryBoardToImageDict[StoryBoardsList[StoryBoardIndex]].Visibility = Visibility.Visible;
                            StoryBoardsList[StoryBoardIndex].Begin();
                            StoryBoardStartedList.Add(StoryBoardsList[StoryBoardIndex]);
                            StoryBoardIndex += 1;
                            CurrentTime = 0;//Start counting again
                        }
                    }
                    else
                    {//If not equal interval
                        if (Math.Abs(CurrentTime - RandomIntervel / RunSpeedFactor * 1000) <= 1)
                        {
                            if (StoryBoardIndex >= StoryBoardsList.Count)
                            {                            // Check if all animations have started
                                StoryBoardTimer.Stop(); // Stop the timer
                                StoryBoardTimer.Elapsed -= StoryBoardTimer_Elapsed;
                                return;
                            }
                            StoryBoardToImageDict[StoryBoardsList[StoryBoardIndex]].Visibility = Visibility.Visible;
                            StoryBoardsList[StoryBoardIndex].Begin();
                            StoryBoardStartedList.Add(StoryBoardsList[StoryBoardIndex]);
                            RandomIntervel = RandomObject.NextDouble() * (MaxIntervel - MinIntervel) + MinIntervel;//Update interval
                            StoryBoardIndex += 1;
                            CurrentTime = 0;

                        }
                    }
                }

            });

        }

        private void MaxTimer_Tick(object sender, EventArgs e)//The trigger event of total training time
        {
            MaxTimeCount -= 1;
            if (MaxTimeCount >= 0)
            {   //The first blank refers to the total countdown, and the second parameter refers to the countdown of this question
                TimeStatisticsAction?.Invoke((int)MaxTimeCount, (int)MaxTimeCount);//Show countdown
            }
            else
            {
                //Related logic forcing the game to end
                OnGameEnd();
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

        /*LJN
         Added resources for visual and sound feedback
         */
        private void FeedBackInit()
        {
            /*LJN
            Configure the path to feedback resources             
             */
            CorrectSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");
            // for Image Control loading picture Source
            CorrectImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Correct.png"), UriKind.RelativeOrAbsolute));
            ErrorImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Error.png"), UriKind.RelativeOrAbsolute));

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

            // Delay the specified time（For example, 1 second）
            await Task.Delay(StopDurations);

            image.Visibility = Visibility.Collapsed;
        }
    }
    public partial class Beware_of_training_explanations : BaseUserControl
    {
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

        async private void Try_Click(object sender, RoutedEventArgs e)
        {
            PlayGrid.Visibility = Visibility.Visible;
            ExplainGrid.Visibility = Visibility.Collapsed;
            Init();//Resource Initialization
            LevelSet();//Make the parameters effective
            ShowRefItem();//showrefItems
            IfTryClicked = true;
            ShowSelectableItem();

            //LevelSet();
            //IfLevelChange();//First determine whether level upgrade is required
            //                //This part of the result is initialized
            //ReInit();
            ////Start a new game
            //StartGame();


            SetTitleVisibleAction?.Invoke(true);//show"Question Rules"Four black letters
            RuleAction?.Invoke("There will be several images above the conveyor belt on the screen, and then a series of images will move from left to right on the screen. When you see an image that is different from the one above the conveyor belt, please pressOKkey");//Add code, call function, display the text under the digital person                                             
            //StartStoryBoardsTimer();
        }

        private async void PageSwitch()//according topageValue to display components
        {
            switch (CurrentPage)
            {
                case 1://First page, only next step and skip
                    ButtonExplainGrid.Columns = 2;
                    LastStep.Visibility = Visibility.Collapsed;
                    NextStep.Visibility = Visibility.Visible;
                    Skip.Visibility = Visibility.Visible;
                    Try.Visibility = Visibility.Collapsed;

                    //Next is the issue of picture display
                    PicExplain.Source = null;

                    //Next is the prompt display question
                    TextExplain.Text = "Now, this module will be explained to you. Please click the button below to enter the explanation.";
                    await OnVoicePlayAsync(TextExplain.Text);
                    break;
                case 2://The second page, both the previous step and the next step and the skip
                    ButtonExplainGrid.Columns = 3;
                    LastStep.Visibility = Visibility.Visible;
                    NextStep.Visibility = Visibility.Visible;
                    Skip.Visibility = Visibility.Visible;
                    Try.Visibility = Visibility.Collapsed;

                    //Next is the issue of picture display
                    CurrentPicPath = PicPathList.FirstOrDefault(item => item.Contains("Part1.png"));
                    PicExplain.Source = new BitmapImage(new Uri(CurrentPicPath, UriKind.RelativeOrAbsolute));


                    //Next is the prompt display question
                    TextExplain.Text = "First you will see several reference objects in the box.";
                    await OnVoicePlayAsync(TextExplain.Text);
                    break;
                case 3://The third page includes both the previous step, the next step, and the skipped
                    ButtonExplainGrid.Columns = 3;
                    LastStep.Visibility = Visibility.Visible;
                    NextStep.Visibility = Visibility.Visible;
                    Skip.Visibility = Visibility.Visible;
                    Try.Visibility = Visibility.Collapsed;

                    //Next is the issue of picture display
                    CurrentPicPath = PicPathList.FirstOrDefault(item => item.Contains("Part2.png"));
                    PicExplain.Source = new BitmapImage(new Uri(CurrentPicPath, UriKind.RelativeOrAbsolute));

                    //Next is the prompt display question
                    TextExplain.Text = "Then you will see several items moving from left to right on a conveyor belt.";
                    await OnVoicePlayAsync(TextExplain.Text);
                    break;
                case 4://The fourth page has both the previous step, the next step, and the skipped
                    ButtonExplainGrid.Columns = 3;
                    LastStep.Visibility = Visibility.Visible;
                    NextStep.Visibility = Visibility.Visible;
                    Skip.Visibility = Visibility.Visible;
                    Try.Visibility = Visibility.Collapsed;

                    //Next is the issue of picture display
                    CurrentPicPath = PicPathList.FirstOrDefault(item => item.Contains("Part3.png"));
                    PicExplain.Source = new BitmapImage(new Uri(CurrentPicPath, UriKind.RelativeOrAbsolute));

                    //Next is the prompt display question
                    TextExplain.Text = "You need to determine whether the item is different from the reference object above when it moves to the black box and pressOKkey to confirm that there is a difference.";
                    await OnVoicePlayAsync(TextExplain.Text);
                    break;
                case 5://The fifth page, only the previous step, try and skip
                    ButtonExplainGrid.Columns = 3;
                    LastStep.Visibility = Visibility.Visible;
                    NextStep.Visibility = Visibility.Collapsed;
                    Skip.Visibility = Visibility.Visible;
                    Try.Visibility = Visibility.Visible;

                    //Next is the issue of picture display
                    PicExplain.Source = null;

                    //Next is the prompt display question
                    TextExplain.Text = "After the explanation is completed, please click the trial button to start the trial, or click Skip to enter the official game";
                    await OnVoicePlayAsync(TextExplain.Text);
                    break;
                default:
                    System.Windows.MessageBox.Show("If you have any problems with your network, please contact the administrator");
                    break;
            }
        }
    }
}
