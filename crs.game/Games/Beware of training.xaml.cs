using crs.core;
using crs.core.DbModels;
using crs.game.Games;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Identity.Client.NativeInterop;
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
    public partial class Beware of training : BaseUserControl
    {
            //Game parameters that need to be read from the database
        private int Level = 0;//Current game level, 1-9
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
        private int ItemsInLevel = 90;//How many levels will there be in totalitemFloating past
        private double ErrorRate = 0.8;//Refers to all that floats pastitemHow many of them are wrong? The ratio should not be selected
        private int ShowTime = 8;//Refers to aitemThe total time required from left to right refers to the different speeds of different levels, units
        private int ShowIntervel = 2;//Refers to differentitemThe interval that appears is checked with whether to check itIfItemEqualIntervalIt's related
        private double MaxIntervel = 3;//When checkeditemThe maximum interval after random interval
        private double MinIntervel = 1;//When checkeditemThe minimum interval after random interval
        private double FixedIntervel = 1.5;//When checkeditemThe interval after equal interval
        private int MaxLevel = 8;//The maximum level is because the material is only ready to level 8
        private int MinLevel = 1;//Minimum level
        private bool IfChange = false;//Does the difficulty level need to be changed
        private int EverMaxLevel = 1;//The maximum level that has been reached during this game is used to put it in the report

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
        

        public Beware of training()
        {
            InitializeComponent();
            //Call the class constructor first, then call itOnInitAsync()
            //Click to start before calling            OnStartAsync();
            AnswersInit();
            /*
             Module writing ideas
            The idea of ​​picture material is: put all folders inpngRead into oneList
            The idea of ​​a conveyor belt is: use oneListTo store the animation object, the new image will be initialized and placed inListAfter the animation is over, it will be removed fromListmiddleremoveLose
             */
        }

        private void LevelSet()//Adjustment completedLevelAfterwards, manually update parameters before the next round of game starts
        {
            switch(Level)
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

            if (Level - EverMaxLevel >= 0) { EverMaxLevel = Level; }

            ReadItems();//Change every timeLevelThenListGet ready
        }

        private void ReadItems()//Put localitemRead into memory according to the specified level and place itAllItemsmiddle
        {
            ItemsPath =  System.IO.Path.Combine(BaseDirectory,"Games" ,"VIG", "real",$"{Level}");
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
            // when ConveyArea After the complete loading and rendering is completed, call ShowSelectableItem method
            ShowSelectableItem();
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
            int NonRefNum = (int)Math.Ceiling( ItemsInLevel * ErrorRate);
            int RefNum = ItemsInLevel - NonRefNum;
            ToShowItems.AddRange(GetRandomSamples(NonReferenceItems, NonRefNum));//Are the defective ones on the assembly line that patients should choose
            ToShowItems.AddRange(GetRandomSamples(ReferenceItems,RefNum));
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
                    Height = ConveyArea.ActualHeight
                };
                ConveyArea.Children.Add(img);

                ItemStatusDict[img] = false;//Detection status becomes

                DoubleAnimation animation = new DoubleAnimation
                {
                    From = -300,//Move from the leftmost side of the conveyor belt to the rightmost side
                    To = ConveyArea.ActualWidth,
                    Duration = new Duration(TimeSpan.FromSeconds(ShowTime/ RunSpeedFactor))
                };

                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                StoryBoardToImageDict[storyboard] = img;
                storyboard.Completed +=(s,e)=> StoryBoardEnd(img,storyboard);//Things to call at the end of the animation

                CompositionTarget.Rendering += (sender, e) =>
                {// register CompositionTarget.Rendering event
                    double imgLeft = Canvas.GetLeft(img);                        // Get the current one Image The left position
                    if (imgLeft > Canvas.GetLeft(TargetRect)+ TargetRect.ActualWidth)// Determine whether it exceeds the right boundary
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
            //Use a timer to determine when each animation needs to be triggered, so as to achieve equal or non-equidistant occurrenceitem
            StartStoryBoardsTimer();//All animations are created and then started together
        }

        private void StoryBoardEnd(Image img,Storyboard storyboard)//What to fire after each animation is finished
        {
            StoryBoardStartedList.Remove(storyboard);//Remove this animation object from the list
            ConveyArea.Children.Remove(img);//Put thisimgfromcanvasRemoved in
            ItemStatusDict.Remove(img);//Delete this key-value pair


            if (ItemStatusDict.Count<=0)
            {//Explain that all created animations are over
                IfLevelChange();//First determine whether level upgrade is required
                //This part of the result is initialized
                ReInit();
                //Start a new game
                StartGame();
            }
        }

        private void  IfLevelChange()//Judging whether the level has increased based on the answer results
        {
            if(LevelRight>=LevelUp && Level<MaxLevel)
            {//Upgrade priority
                Level += 1;LevelSet(); IfChange = true;
            }
            else if(LevelWrong>=LevelDown && Level>MinLevel)
            {
                Level -= 1;LevelSet(); IfChange = true;
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
                    if (ItemStatusDict[img]==false)
                    {
                        // Get img The boundary of                        // Check if it is NaN And give default values
                        double imgLeft = Canvas.GetLeft(img); if (double.IsNaN(imgLeft)) imgLeft = 0;
                        double imgTop = Canvas.GetTop(img); if (double.IsNaN(imgTop)) imgTop = 0;

                        Rect imgBounds = new Rect(imgLeft, imgTop, img.ActualWidth, img.ActualHeight);
                        // Check for overlap
                        if (rectangleBounds.IntersectsWith(imgBounds))
                        {
                            //Make a judgment on whether it is correct
                            if(ReferenceItems.Contains(img.Tag))
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
            if (ItemStatusDict.ContainsKey(img) && ItemStatusDict[img]==false)
            {//Not checked
                if(!ReferenceItems.Contains(img.Tag))
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
            EverMaxLevel = Level;
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
            correctAnswers = new int[MaxLevel+1];
            wrongAnswers = new int[MaxLevel + 1];
            ignoreAnswers = new int[MaxLevel + 1];
            LevelRight = 0;LevelWrong = 0;LevelIgnore = 0;
        }

        private void LevelResultInit()//Clear the level result after the difficulty level is updated
        {
            LevelRight = 0; LevelWrong = 0; LevelIgnore = 0;
        }

        private void StoryBoardTimer_Elapsed(object sender, EventArgs e)//Used to trigger the start of the animation regularly
        {
            Application.Current.Dispatcher.Invoke(() =>
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
                        if (Math.Abs(CurrentTime - FixedIntervel/RunSpeedFactor * 1000) <= 1)
                        {//The timing has reached a fixed interval, and it's time to start another animation
                            if (StoryBoardIndex >= StoryBoardsList.Count)
                            {                            // Check if all animations have started
                                StoryBoardTimer.Stop(); // Stop the timer
                                StoryBoardTimer.Elapsed -= StoryBoardTimer_Elapsed;
                                return;
                            }
                            StoryBoardsList[StoryBoardIndex].Begin();
                            StoryBoardToImageDict[StoryBoardsList[StoryBoardIndex]].Visibility = Visibility.Visible;
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
                            StoryBoardsList[StoryBoardIndex].Begin();
                            StoryBoardToImageDict[StoryBoardsList[StoryBoardIndex]].Visibility = Visibility.Visible;
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
    public partial class Beware of training : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
            {
                // parameter（Includes module parameter information）
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
                            switch (par.ModuleParId)
                            {

                                case 166: // grade
                                    Level = par.Value.HasValue ? (int)par.Value.Value : 1;
                                    break;
                                case 113: // Treatment time
                                    MaxTime = par.Value.HasValue ? (int)par.Value.Value : 30;
                                    break;
                                case 319://Image type, abstract or real
                                    ImageReality = par.Value.HasValue ? (int)par.Value.Value : 0;
                                    break;
                                case 320: // Level improvement
                                    LevelUp = par.Value.HasValue ? (int)par.Value.Value : 30;
                                    break;
                                case 321: // Level down
                                    LevelDown = par.Value.HasValue ? (int)par.Value.Value : 30;
                                    break;
                                case 117: // Is the item interval fixed?
                                    IfItemEqualInterval = par.Value == 1;
                                    break;
                                case 118: // Speed ​​factor
                                    RunSpeedFactor = par.Value.HasValue ? (double)par.Value.Value : 1.0;
                                    break;
                                case 122: //sound
                                    IfAudioFeedBack = par.Value == 1;
                                    break;
                                case 123: //Visual feedback
                                    IfVisionFeedBack = par.Value == 1;
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
        }

        protected override async Task OnStartAsync()
        {
            Init();//Resource Initialization
            LevelSet();//Make the parameters effective
            ShowRefItem();//showrefItems
            //ShowSelectableItem();//Create animations and start in a certain order,ThisconveyAreaCall it after rendering
            //Then start the detection button press
            MaxTimer.Start();//The total timer starts to count
            StarTimer.Start();

            // Calling delegate
            VoiceTipAction?.Invoke("There will be several images above the conveyor belt on the screen, and then a series of images will move from left to right on the screen. When you see an image that is different from the one above the conveyor belt, please pressOKkey");
            SynopsisAction?.Invoke("There will be several images above the conveyor belt on the screen, and then a series of images will move from left to right on the screen. When you see an image that is different from the one above the conveyor belt, please pressOKkey");
            RuleAction?.Invoke("There will be several images above the conveyor belt on the screen, and then a series of images will move from left to right on the screen. When you see an image that is different from the one above the conveyor belt, please pressOKkey");//Add code, call function, display the text under the digital person

        }

        protected override async Task OnStopAsync()
        {
            OnGameEnd();//Forced end game
        }

        protected override async Task OnPauseAsync()
        {
            // Stop all animations
            PauseStoryBoards();
            // Delay StopDurations millisecond
            IfPause = true;//This flag will involve the trigger of the timer.StoryBoardTimer_Tick

        }

        protected override async Task OnNextAsync()
        {

            //This part of the result is initialized
            ReInit();
            IfLevelChange();//First determine whether level upgrade is required
            //Start a new game
            StartGame();
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new Beware of training explanations();
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
                        int ignoreCount = 0;
                        int allcount = 0;
                        // Calculation accuracy
                        double accuracy = 0;
                        int max_hardness = 0;

                        //int correctCount = GetCorrectNum(lv);
                        //int wrongCount = GetWrongNum(lv);
                        //int ignoreCount = GetIgnoreNum(lv);
                        //int allcount = correctCount + wrongCount + ignoreCount;
                        //if (correctCount == 0 && wrongCount == 0 && ignoreCount == 0)
                        //{
                        //    // If all data is 0, skip this difficulty level
                        //    Debug.WriteLine($"Difficulty level {lv}: No data, skip.");
                        //    continue;
                        //}
                        //// Calculation accuracy
                        //double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);
                        for (int lv = 0; lv < correctAnswers.Length; lv++)
                        {

                            // Get data at the current difficulty level
                            if (correctCount == 0 && wrongCount == 0 && ignoreCount == 0)
                            {
                                // If all data is 0, skip this difficulty level
                                Debug.WriteLine($"Difficulty level {lv}: No data, skip.");
                                continue;
                            }
                            max_hardness = lv + 1;

                            correctCount += GetCorrectNum(lv);
                            wrongCount += GetWrongNum(lv);
                            ignoreCount += GetIgnoreNum(lv);
                        }

                        allcount = correctCount + wrongCount + ignoreCount;
                        accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);

                        // create Result Record
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "Beware of training 2",
                            Eval = false,
                            Lv = EverMaxLevel, // Current difficulty level//EverMaxLevelIt is a variable defined later, specifically used to deal with the problem of the maximum difficulty level that has been reached.
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
                                    Value = EverMaxLevel,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "Total number of tasks",
                                    Value = allcount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "Correct rate（%）",
                                    Value = accuracy * 100,
                                    Charttype = "Bar chart" ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "correct",
                                    Value = correctCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                  new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "neglect",
                                    Value = ignoreCount,
                                    Charttype = "Bar chart" ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "mistake",
                                    Value = wrongCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
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
