using crs.core;
using crs.core.DbModels;
using crs.game.Games;
using Microsoft.Identity.Client.NativeInterop;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Numerics;
using System.Media;
using Newtonsoft.Json;

namespace crs.game.Games
{
    /// <summary>
    /// Detail memory.xaml Interaction logic
    /// </summary>
    public partial class Detail memory : BaseUserControl
    {

        public Detail memory()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Parameters obtained by the front end
        /// </summary>
        int allTrainTime;
        bool hasVisualFeedback; //Visual feedback
        bool hasSoundFeedback;  //Auditory return
        bool isQuestionJumpingEnabled;  //The title jumps automatically
        static int correctFeenbackTime = 3;
        static int errorFeenbackTime = 5;
        bool hasMemoryTimeLimit;
        bool hasAnswerTimeLimit;
        int INCREASE; // Increase the threshold for difficulty
        int DECREASE;  // Threshold for reducing difficulty
        int maxRepeatTimes;    //Maximum number of repetitions
        int Mode = 1;//1 multiple choices, 2 free


        /// <summary>
        /// Level-related parameters
        /// </summary>
        int level = 1;
        int questionCount = 1;
        int allowedErrorCount = 0;
        int curTrainTime;


        /// <summary>
        /// Record report parameters
        /// </summary>
        int max_hardness = 0;
        int errorCount = 0;
        int correctCount = 0;
        int repeatCount = 0;




        /// <summary>
        /// Timer
        /// </summary>
        DispatcherTimer trainingTimer; // New timer for total training time
        void InitAllTimer()
        {
            trainingTimer = new DispatcherTimer();
            trainingTimer.Interval = TimeSpan.FromSeconds(1);
            trainingTimer.Tick += TrainingTimer_Tick;
        }

        void StartAllTimer()
        {
            trainingTimer?.Start();
        }
        void StopAllTimer()
        {
            trainingTimer?.Stop();
        }








        /// <summary>
        /// Read all questions for each difficulty
        /// </summary>
        Random rand = new Random();
        class QuestionItem
        {
            public string Question { get; set; }
            public string OptionA { get; set; }
            public string OptionB { get; set; }
            public string OptionC { get; set; }
            public string OptionD { get; set; }
            public string Answer { get; set; }
        }

        class StoryItem
        {
            public StoryItem(StoryItem x)
            {
                if (x == null)
                {
                    this.Story = "";
                    this.QuestionsList = new List<QuestionItem>();
                }
                else
                {
                    this.Story = x.Story;
                    this.QuestionsList = new List<QuestionItem>();
                    this.QuestionsList.AddRange(x.QuestionsList);
                }
            }
            public string Story { get; set; }
            public List<QuestionItem> QuestionsList { get; set; }

        }

        List<List<StoryItem>> allStoryData;
        StoryController storyController;
        class StoryController
        {
            List<List<StoryItem>> _currentStoryData { get; set; }
            List<List<StoryItem>> currentStoryData { get; set; }
            Random rand = new Random();

            public StoryController(List<List<StoryItem>> allStoryData)
            {
                currentStoryData = new List<List<StoryItem>>();
                for (int i = 0; i <= 10; i++)
                {
                    currentStoryData.Add(new List<StoryItem>());
                }
                _currentStoryData = allStoryData;
            }
            public StoryItem GetNewStory(int level = 1)
            {
                int index = level switch
                {
                    1 => 1,
                    <= 4 => 4,
                    <= 7 => 7,
                    <= 10 => 10,
                    _ => 1
                };
                if (currentStoryData[index].Count == 0)
                {
                    currentStoryData[index].AddRange(_currentStoryData[index]);
                }
                int tmp = rand.Next(currentStoryData[index].Count);
                StoryItem item = currentStoryData[index][tmp];
                currentStoryData[index].RemoveAt(tmp);
                return item;
            }
        }


        void InitAndReadStoryData()
        {
            allStoryData = new List<List<StoryItem>>();
            for (int i = 0; i <= 10; i++)
            {
                allStoryData.Add(new List<StoryItem>());
            }

            for (int i = 1; i <= 10; i++)
            {
                string filePath = $"Resources/Detail memory/Level{i}.json";
                if (!File.Exists(filePath))
                {
                    continue;
                }
                string jsonContent;
                try
                {
                    jsonContent = File.ReadAllText(filePath);
                    allStoryData[i] = JsonConvert.DeserializeObject<List<StoryItem>>(jsonContent);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading JSON file: " + ex.Message);
                }
            }
            storyController = new StoryController(allStoryData);



        }

        /// <summary>
        /// Current question situation
        /// </summary>
        int curStage = 1;
        int curStoryMemoryTime = 60;
        int curQuestionAnswerTime = 60;
        int curFeedbackTime = 5;
        int feedbackTime = 3;
        string curOption = "";

        int curStoryReadTimes = 0;

        StoryItem _curStoryItem;
        StoryItem curStoryItem;
        QuestionItem curQuestionItem;


        List<int> curStoryRecords = new List<int>();
        List<int> curLevelRecords = new List<int>();

        /// <summary>
        /// Module logic
        /// </summary>
        void SetLevelParam(int _level)
        {
            level = _level;
            max_hardness = Math.Max(max_hardness, level);
            (questionCount, allowedErrorCount) = level switch
            {
                1 => (1, 0),
                2 => (2, 0),
                3 => (3, 0),
                4 => (4, 0),
                5 => (5, 1),
                6 => (6, 1),
                7 => (7, 1),
                8 => (8, 1),
                9 => (9, 2),
                10 => (10, 2),
                _ => (0, 0),
            };
        }
        void CheckStory()
        {
            int correctCount = curStoryRecords.Sum();
            int errorCount = curStoryRecords.Count - correctCount;
            if (errorCount <= allowedErrorCount)
            {
                curLevelRecords.Add(1);
                NewRound();
            }
            else
            {
                if (curStoryReadTimes < maxRepeatTimes)
                {
                    curStoryReadTimes++;
                    repeatCount++;

                    curStoryItem = new StoryItem(_curStoryItem);

                }
                else
                {
                    curLevelRecords.Add(0);
                    NewRound();
                }
            }
            curStoryRecords.Clear();
            int correct = curLevelRecords.Sum();
            int error = curLevelRecords.Count - correct;
            if (correct >= INCREASE)
            {
                if (level < 10)
                {
                    level++;
                    SetLevelParam(level);
                    NewRound();
                    LevelStatisticsAction?.Invoke(level, 10);
                }
                curLevelRecords.Clear();
            }
            if (error >= DECREASE)
            {
                if (level > 1)
                {
                    level--;
                    SetLevelParam(level);
                    NewRound();
                    LevelStatisticsAction?.Invoke(level, 10);
                }
                curLevelRecords.Clear();
            }
            correct = curLevelRecords.Sum();
            error = curLevelRecords.Count - correct;
            RightStatisticsAction?.Invoke(correct, INCREASE);
            WrongStatisticsAction?.Invoke(error, DECREASE);
        }

        private void OptionA_Click(object sender, RoutedEventArgs e)
        {
            UnhighlightAllButtons();
            OptionA.Background = new SolidColorBrush(Colors.LightBlue);
            curOption = "OptionA";
        }

        private void OptionB_Click(object sender, RoutedEventArgs e)
        {
            UnhighlightAllButtons();
            OptionB.Background = new SolidColorBrush(Colors.LightBlue);
            curOption = "OptionB";
        }

        private void OptionC_Click(object sender, RoutedEventArgs e)
        {
            UnhighlightAllButtons();
            OptionC.Background = new SolidColorBrush(Colors.LightBlue);
            curOption = "OptionC";
        }

        private void OptionD_Click(object sender, RoutedEventArgs e)
        {
            UnhighlightAllButtons();
            OptionD.Background = new SolidColorBrush(Colors.LightBlue);
            curOption = "OptionD";
        }

        private void UnhighlightAllButtons()
        {
            OptionA.Background = new SolidColorBrush(Colors.Transparent);
            OptionB.Background = new SolidColorBrush(Colors.Transparent);
            OptionC.Background = new SolidColorBrush(Colors.Transparent);
            OptionD.Background = new SolidColorBrush(Colors.Transparent);
        }


        void NewRound()
        {
            curStoryItem = storyController.GetNewStory(level);
            _curStoryItem = new StoryItem(curStoryItem);

            curStoryReadTimes = 0;

            StoryTextBox.Text = curStoryItem.Story;
            StoryPanel.Visibility = Visibility.Visible;
            QuestionPanel1.Visibility = Visibility.Hidden;
            QuestionPanel2.Visibility = Visibility.Hidden;

            curOption = "";
            inputTextBox.Text = "";

            curStoryRecords.Clear();

        }
        void NewQuestion()
        {
            if (curStoryItem.QuestionsList.Count <= 0)
            {
                return;
            }
            int index = rand.Next(0, curStoryItem.QuestionsList.Count);
            curQuestionItem = curStoryItem.QuestionsList[index];
            curStoryItem.QuestionsList.RemoveAt(index);

            QuestionTextBox1.Text = curQuestionItem.Question;
            QuestionTextBox2.Text = curQuestionItem.Question;
            OptionA.Content = curQuestionItem.OptionA;
            OptionB.Content = curQuestionItem.OptionB;
            OptionC.Content = curQuestionItem.OptionC;
            OptionD.Content = curQuestionItem.OptionD;
        }


        void OKButtonClick(object sender, RoutedEventArgs e)
        {
            if (curStage == 1)
            {
                NewQuestion();
                StoryPanel.Visibility = Visibility.Hidden;
                if (Mode == 1)
                {
                    QuestionPanel1.Visibility = Visibility.Visible;
                }
                else
                {
                    QuestionPanel2.Visibility = Visibility.Visible;
                }

                EnableAllButton();
                curStage = 2;
                curQuestionAnswerTime = 62 - level * 2;
            }
            else if (curStage == 2)
            {
                if (Mode == 1 && string.Equals(curOption, ""))
                {
                    return;
                }
                if (Mode == 1)
                {
                    if (string.Equals(curOption, curQuestionItem.Answer))
                    {
                        curStage = 3;
                        feedbackTime = 3;
                        curStoryRecords.Add(1);
                        correctCount++;

                        Border optionBorder = FindName(curOption + "Border") as Border;
                        if (optionBorder != null)
                        {
                            optionBorder.BorderBrush = new SolidColorBrush(Colors.Green);
                        }
                        UnenableAllButton();

                        if (hasVisualFeedback)
                        {
                            ShowFeedbackImage(CorrectImage);
                        }
                        if (hasSoundFeedback)
                        {
                            PlayWav(CorrectSoundPath);
                        }
                    }
                    else
                    {
                        curStage = 3;
                        feedbackTime = 5;
                        curStoryRecords.Add(0);
                        errorCount++;

                        Border optionBorder = FindName(curOption + "Border") as Border;
                        if (optionBorder != null)
                        {
                            optionBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                        }
                        UnenableAllButton();

                        if (hasVisualFeedback)
                        {
                            ShowFeedbackImage(ErrorImage);
                        }
                        if (hasSoundFeedback)
                        {
                            PlayWav(ErrorSoundPath);
                        }
                    }
                }
                else if (Mode == 2)
                {
                    inputTextBox.IsEnabled = false;
                    string inputContent = inputTextBox.Text;
                    string answerContent = curQuestionItem.Answer switch
                    {
                        "OptionA" => curQuestionItem.OptionA,
                        "OptionB" => curQuestionItem.OptionB,
                        "OptionC" => curQuestionItem.OptionC,
                        "OptionD" => curQuestionItem.OptionD,
                        _ => ""
                    };
                    if (string.Equals(inputContent, answerContent))
                    {
                        curStage = 3;
                        feedbackTime = 3;
                        curStoryRecords.Add(1);
                        correctCount++;

                        if (hasVisualFeedback)
                        {
                            ShowFeedbackImage(CorrectImage);
                        }
                        if (hasSoundFeedback)
                        {
                            PlayWav(CorrectSoundPath);
                        }
                    }
                    else
                    {
                        curStage = 3;
                        feedbackTime = 5;
                        curStoryRecords.Add(0);
                        errorCount++;

                        if (hasVisualFeedback)
                        {
                            ShowFeedbackImage(ErrorImage);
                        }
                        if (hasSoundFeedback)
                        {
                            PlayWav(ErrorSoundPath);
                        }
                    }
                }
            }
            else if (curStage == 3)
            {
                Border optionBorder = FindName(curOption + "Border") as Border;
                if (optionBorder != null)
                {
                    optionBorder.BorderBrush = new SolidColorBrush(Colors.Transparent);
                }
                UnhighlightAllButtons();
                HiddenFeedbackImage();
                EnableAllButton();
                inputTextBox.IsEnabled = true;
                inputTextBox.Text = "";
                if (_curStoryItem.QuestionsList.Count - curStoryItem.QuestionsList.Count >= questionCount)
                {
                    CheckStory();
                    curStage = 1;
                    curStoryMemoryTime = 60;

                    StoryTextBox.Text = curStoryItem.Story;
                    StoryPanel.Visibility = Visibility.Visible;
                    QuestionPanel1.Visibility = Visibility.Hidden;
                    QuestionPanel2.Visibility = Visibility.Hidden;
                }
                else
                {
                    NewQuestion();
                    curStage = 2;
                    curQuestionAnswerTime = 62 - level * 2;
                }
                curOption = "";
            }

        }


        private async void TrainingTimer_Tick(object sender, EventArgs e)
        {
            curTrainTime--;

            if (curTrainTime <= 0)
            {
                StopAllTimer();
                OnGameEnd();
            }
            if (curStage == 1)
            {
                if (hasMemoryTimeLimit)
                {
                    curStoryMemoryTime--;
                    if (curStoryMemoryTime <= 0)
                    {
                        NewQuestion();
                        //Memory time ends
                        StoryPanel.Visibility = Visibility.Hidden;
                        if (Mode == 1)
                        {
                            QuestionPanel1.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            QuestionPanel2.Visibility = Visibility.Visible;
                        }
                        EnableAllButton();
                        curStage = 2;
                        curQuestionAnswerTime = 62 - level * 2;
                    }
                }
                else
                {
                    curStoryMemoryTime = 0;
                }
            }
            else if (curStage == 2)
            {
                if (hasAnswerTimeLimit)
                {
                    curQuestionAnswerTime--;
                    if (curQuestionAnswerTime <= 0)
                    {
                        //The answer time ends
                        if (Mode == 1)
                        {
                            if (string.Equals(curOption, curQuestionItem.Answer))
                            {
                                curStage = 3;
                                feedbackTime = 3;
                                curStoryRecords.Add(1);
                                correctCount++;

                                Border optionBorder = FindName(curOption + "Border") as Border;
                                if (optionBorder != null)
                                {
                                    optionBorder.BorderBrush = new SolidColorBrush(Colors.Green);
                                }
                                UnenableAllButton();

                                if (hasVisualFeedback)
                                {
                                    ShowFeedbackImage(CorrectImage);
                                }
                                if (hasSoundFeedback)
                                {
                                    PlayWav(CorrectSoundPath);
                                }
                            }
                            else
                            {
                                curStage = 3;
                                feedbackTime = 5;
                                curStoryRecords.Add(0);
                                errorCount++;

                                Border optionBorder = FindName(curOption + "Border") as Border;
                                if (optionBorder != null)
                                {
                                    optionBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                                }
                                UnenableAllButton();

                                if (hasVisualFeedback)
                                {
                                    ShowFeedbackImage(ErrorImage);
                                }
                                if (hasSoundFeedback)
                                {
                                    PlayWav(ErrorSoundPath);
                                }
                            }
                        }
                        else if (Mode == 2)
                        {
                            inputTextBox.IsEnabled = false;
                            string inputContent = inputTextBox.Text;
                            string answerContent = curQuestionItem.Answer switch
                            {
                                "OptionA" => curQuestionItem.OptionA,
                                "OptionB" => curQuestionItem.OptionB,
                                "OptionC" => curQuestionItem.OptionC,
                                "OptionD" => curQuestionItem.OptionD,
                                _ => ""
                            };
                            if (string.Equals(inputContent, answerContent))
                            {
                                curStage = 3;
                                feedbackTime = 3;
                                curStoryRecords.Add(1);
                                correctCount++;

                                if (hasVisualFeedback)
                                {
                                    ShowFeedbackImage(CorrectImage);
                                }
                                if (hasSoundFeedback)
                                {
                                    PlayWav(CorrectSoundPath);
                                }
                            }
                            else
                            {
                                curStage = 3;
                                feedbackTime = 5;
                                curStoryRecords.Add(0);
                                errorCount++;

                                if (hasVisualFeedback)
                                {
                                    ShowFeedbackImage(ErrorImage);
                                }
                                if (hasSoundFeedback)
                                {
                                    PlayWav(ErrorSoundPath);
                                }
                            }
                        }
                    }

                }
                else
                {
                    curQuestionAnswerTime = 0;
                }
            }
            else if (curStage == 3)
            {
                if (isQuestionJumpingEnabled)
                {
                    feedbackTime--;
                    if (feedbackTime <= 0)
                    {
                        Border optionBorder = FindName(curOption + "Border") as Border;
                        if (optionBorder != null)
                        {
                            optionBorder.BorderBrush = new SolidColorBrush(Colors.Transparent);
                        }
                        UnhighlightAllButtons();
                        HiddenFeedbackImage();
                        EnableAllButton();
                        inputTextBox.IsEnabled = true;
                        inputTextBox.Text = "";
                        if (_curStoryItem.QuestionsList.Count - curStoryItem.QuestionsList.Count >= questionCount)
                        {
                            CheckStory();
                            curStage = 1;
                            curStoryMemoryTime = 60;


                            StoryTextBox.Text = curStoryItem.Story;
                            StoryPanel.Visibility = Visibility.Visible;
                            QuestionPanel1.Visibility = Visibility.Hidden;
                            QuestionPanel2.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            NewQuestion();
                            curStage = 2;
                            curQuestionAnswerTime = 62 - level * 2;
                        }
                        curOption = "";
                    }
                }
                else
                {
                    feedbackTime = 0;
                }
            }
            TimeStatisticsAction?.Invoke(curTrainTime, curStage switch
            {
                1 => curStoryMemoryTime,
                2 => curQuestionAnswerTime,
                3 => feedbackTime,
                _ => 0
            });

        }

        void EnableAllButton()
        {
            OptionA.IsEnabled = true;
            OptionB.IsEnabled = true;
            OptionC.IsEnabled = true;
            OptionD.IsEnabled = true;
        }
        void UnenableAllButton()
        {
            OptionA.IsEnabled = false;
            OptionB.IsEnabled = false;
            OptionC.IsEnabled = false;
            OptionD.IsEnabled = false;
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

        private void ShowFeedbackImage(Image image)
        {//Image showing feedback
            image.Visibility = Visibility.Visible;
            int StopDurations = image.Name switch
            {
                "CorrectImage" => 3000,
                "ErrorImage" => 5000,
                _ => 3000
            };
            // Delay the specified time（For example, 1 second）
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(StopDurations);
            timer.Tick += (sender, e) =>
            {
                HiddenFeedbackImage();
                timer.Stop();
            };
            timer.Start();

        }
        private void HiddenFeedbackImage()
        {
            CorrectImage.Visibility = Visibility.Collapsed;
            ErrorImage.Visibility = Visibility.Collapsed;
        }

    }
    public partial class Detail memory : BaseUserControl, IGameBase
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


            {
                level = 4;
                allTrainTime = 30 * 60;
                hasVisualFeedback = true; //Visual feedback
                hasSoundFeedback = true;  //Auditory return
                isQuestionJumpingEnabled = true;  //The title jumps automatically
                hasMemoryTimeLimit = true;
                hasAnswerTimeLimit = true;
                INCREASE = 5; // Increase the threshold for difficulty
                DECREASE = 5;  // Threshold for reducing difficulty
                maxRepeatTimes = 0;    //Maximum number of repetitions
            }

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
                            case 103: // Treatment time 
                                allTrainTime = par.Value.HasValue ? (int)par.Value.Value * 60 : 30 * 60;
                                Debug.WriteLine($"TRAIN_TIME={allTrainTime}");
                                break;
                            case 104: // repeat
                                maxRepeatTimes = par.Value.HasValue ? (int)par.Value.Value : 0;
                                Debug.WriteLine($"Number of repetitions={maxRepeatTimes}");
                                break;
                            case 105: // Memory time limit
                                hasMemoryTimeLimit = par.Value.HasValue ? par.Value.Value == 1 : true;
                                Debug.WriteLine($"hasMemoryTimeLimit={hasMemoryTimeLimit}");
                                break;
                            case 106: // Response time limit
                                hasAnswerTimeLimit = par.Value.HasValue ? par.Value.Value == 1 : true;
                                Debug.WriteLine($"hasAnswerTimeLimit={hasAnswerTimeLimit}");
                                break;
                            case 107: // The title jumps automatically
                                isQuestionJumpingEnabled = par.Value.HasValue ? par.Value.Value == 1 : true;
                                Debug.WriteLine($"isQuestionJumpingEnabled={isQuestionJumpingEnabled}");
                                break;
                            case 108: // Level improvement
                                INCREASE = par.Value.HasValue ? (int)par.Value.Value : 5;
                                Debug.WriteLine($"INCREASE={INCREASE}");
                                break;
                            case 109: // Level down
                                DECREASE = par.Value.HasValue ? (int)par.Value.Value : 5;
                                Debug.WriteLine($"DECREASE ={DECREASE}");
                                break;
                            case 137: // Mode selection
                                Mode = par.Value.HasValue ? (int)par.Value.Value : 1;
                                //Debug.WriteLine($"DECREASE ={DECREASE}");
                                break;
                            case 262://Visual feedback
                                hasVisualFeedback = par.Value == 1;
                                Debug.WriteLine($"Is it visual feedback? ={hasVisualFeedback}");
                                break;
                            case 263: // Auditory feedback
                                hasSoundFeedback = par.Value == 1;
                                Debug.WriteLine($"Whether to hear feedback ={hasSoundFeedback}");
                                break;
                            case 224:
                                level = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"HARDNESS ={level}");
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

            SetLevelParam(level);
            InitAndReadStoryData();
            InitAllTimer();
            curTrainTime = allTrainTime;
            curStoryMemoryTime = 60;
            curStage = 1;
            NewRound();
            max_hardness = Math.Max(max_hardness, level);

            RightStatisticsAction?.Invoke(0, INCREASE);
            WrongStatisticsAction?.Invoke(0, DECREASE);
            LevelStatisticsAction?.Invoke(level, 10);
        }



        protected override async Task OnStartAsync()
        {
            StartAllTimer();

            VoiceTipAction?.Invoke("Remember the details of the story and find the right options.");
            SynopsisAction?.Invoke("First of all, you will see a small story on the screen. Please remember the details and click on the screen with your mouse.OKkey；Then you will see questions about the short story and four options on the screen. Use the mouse to select the appropriate option and click on the screen.OKkey to confirm the selection.");
            RuleAction?.Invoke("First of all, you will see a small story on the screen. Please remember the details and click on the screen with your mouse.OKkey；Then you will see questions about the short story and four options on the screen. Use the mouse to select the appropriate option and click on the screen.OKkey to confirm the selection.");
        }

        protected override async Task OnStopAsync()
        {
            StopAllTimer();

        }

        protected override async Task OnPauseAsync()
        {
            StopAllTimer();

        }

        protected override async Task OnNextAsync()
        {
            curStage = 1;
            curStoryMemoryTime = 60;

            NewRound();


            VoiceTipAction?.Invoke("Remember the details of the story and find the right options.");
            SynopsisAction?.Invoke("First of all, you will see a small story on the screen. Please remember the details and click on the screen with your mouse.OKkey；Then you will see questions about the short story and four options on the screen. Use the mouse to select the appropriate option and click on the screen.OKkey to confirm the selection.");
            RuleAction?.Invoke("First of all, you will see a small story on the screen. Please remember the details and click on the screen with your mouse.OKkey；Then you will see questions about the short story and four options on the screen. Use the mouse to select the appropriate option and click on the screen.OKkey to confirm the selection.");
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
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
                        double accuracy = ((correctCount + errorCount) > 0) ? 1f * correctCount / (correctCount + errorCount) : 0;

                        // create Result Record
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "Detail memory",
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
                                    ValueName = "Total number of problems",
                                    Value = correctCount+errorCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "mistake",
                                    Value = errorCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "repeat",
                                    Value = repeatCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "Correct rate",
                                    Value = accuracy,
                                    Charttype = "Bar chart" ,
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

        protected override IGameBase OnGetExplanationExample()
        {
            return new Detailed memory explanation();
        }


    }
}
