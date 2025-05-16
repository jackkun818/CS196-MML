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
    /// 细节记忆力.xaml 的交互逻辑
    /// </summary>
    public partial class 细节记忆力 : BaseUserControl
    {

        public 细节记忆力()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 前端获取的参数
        /// </summary>
        int allTrainTime;
        bool hasVisualFeedback; //视觉反馈
        bool hasSoundFeedback;  //听觉返回
        bool isQuestionJumpingEnabled;  //题目自动跳转
        static int correctFeenbackTime = 3;
        static int errorFeenbackTime = 5;
        bool hasMemoryTimeLimit;
        bool hasAnswerTimeLimit;
        int INCREASE; // 提高难度的阈值
        int DECREASE;  // 降低难度的阈值
        int maxRepeatTimes;    //最大重复次数
        int Mode = 1;//1多项选择，2自由


        /// <summary>
        /// 等级有关参数
        /// </summary>
        int level = 1;
        int questionCount = 1;
        int allowedErrorCount = 0;
        int curTrainTime;


        /// <summary>
        /// 记录报告参数
        /// </summary>
        int max_hardness = 0;
        int errorCount = 0;
        int correctCount = 0;
        int repeatCount = 0;




        /// <summary>
        /// 计时器
        /// </summary>
        DispatcherTimer trainingTimer; // 新的计时器用于总训练时间
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
        /// 读取每个难度的所有问题
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
                string filePath = $"Resources/细节记忆力/Level{i}.json";
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
        /// 当前题目情况
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
        /// 模块逻辑
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
                        //记忆时间结束
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
                        //作答时间结束
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
 添加进来视觉、声音反馈的资源
 */
        private SoundPlayer soundPlayer; // 用来放歌
        public string ErrorSoundPath;//错误的声音文件路径，在OnStartAsync()中配置
        public string CorrectSoundPath;//正确的声音文件路径，在OnStartAsync()中配置
        private int StopDurations = 2000; // 停止时间，ms

        private string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory; // 当前项目的绝对路径
        private string ResourcesPath = System.IO.Path.Combine("Resources", "词语记忆力");//这里用的固定是词语记忆力的路径，后期可改

        private void PlayWav(string filePath)
        {//播放本地的wav文件
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
        {//显示反馈的图片
            image.Visibility = Visibility.Visible;
            int StopDurations = image.Name switch
            {
                "CorrectImage" => 3000,
                "ErrorImage" => 5000,
                _ => 3000
            };
            // 延迟指定的时间（例如1秒）
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
    public partial class 细节记忆力 : BaseUserControl, IGameBase
    {

        protected override async Task OnInitAsync()
        {
            /*LJN
            配置反馈资源的路径             
             */
            CorrectSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");
            // 为 Image 控件加载图片 Source
            CorrectImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Correct.png"), UriKind.RelativeOrAbsolute));
            ErrorImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Error.png"), UriKind.RelativeOrAbsolute));


            {
                level = 4;
                allTrainTime = 30 * 60;
                hasVisualFeedback = true; //视觉反馈
                hasSoundFeedback = true;  //听觉返回
                isQuestionJumpingEnabled = true;  //题目自动跳转
                hasMemoryTimeLimit = true;
                hasAnswerTimeLimit = true;
                INCREASE = 5; // 提高难度的阈值
                DECREASE = 5;  // 降低难度的阈值
                maxRepeatTimes = 0;    //最大重复次数
            }

            var baseParameter = BaseParameter;
            if (baseParameter.ProgramModulePars != null && baseParameter.ProgramModulePars.Any())
            {
                Debug.WriteLine("ProgramModulePars 已加载数据：");

                // 遍历 ProgramModulePars 打印出每个参数
                foreach (var par in baseParameter.ProgramModulePars)
                {
                    /*Debug.WriteLine($"ProgramId: {par.ProgramId}, ModuleParId: {par.ModuleParId}, Value: {par.Value}, TableId: {par.TableId}");*/
                    if (par.ModulePar.ModuleId == baseParameter.ModuleId)
                    {
                        switch (par.ModuleParId) // 完成赋值
                        {
                            case 103: // 治疗时间 
                                allTrainTime = par.Value.HasValue ? (int)par.Value.Value * 60 : 30 * 60;
                                Debug.WriteLine($"TRAIN_TIME={allTrainTime}");
                                break;
                            case 104: // 重复
                                maxRepeatTimes = par.Value.HasValue ? (int)par.Value.Value : 0;
                                Debug.WriteLine($"重复次数={maxRepeatTimes}");
                                break;
                            case 105: // 记忆时间限制
                                hasMemoryTimeLimit = par.Value.HasValue ? par.Value.Value == 1 : true;
                                Debug.WriteLine($"hasMemoryTimeLimit={hasMemoryTimeLimit}");
                                break;
                            case 106: // 作答时间限制
                                hasAnswerTimeLimit = par.Value.HasValue ? par.Value.Value == 1 : true;
                                Debug.WriteLine($"hasAnswerTimeLimit={hasAnswerTimeLimit}");
                                break;
                            case 107: // 题目自动跳转
                                isQuestionJumpingEnabled = par.Value.HasValue ? par.Value.Value == 1 : true;
                                Debug.WriteLine($"isQuestionJumpingEnabled={isQuestionJumpingEnabled}");
                                break;
                            case 108: // 等级提高
                                INCREASE = par.Value.HasValue ? (int)par.Value.Value : 5;
                                Debug.WriteLine($"INCREASE={INCREASE}");
                                break;
                            case 109: // 等级降低
                                DECREASE = par.Value.HasValue ? (int)par.Value.Value : 5;
                                Debug.WriteLine($"DECREASE ={DECREASE}");
                                break;
                            case 137: // 模式选择
                                Mode = par.Value.HasValue ? (int)par.Value.Value : 1;
                                //Debug.WriteLine($"DECREASE ={DECREASE}");
                                break;
                            case 262://视觉反馈
                                hasVisualFeedback = par.Value == 1;
                                Debug.WriteLine($"是否视觉反馈 ={hasVisualFeedback}");
                                break;
                            case 263: // 听觉反馈
                                hasSoundFeedback = par.Value == 1;
                                Debug.WriteLine($"是否听觉反馈 ={hasSoundFeedback}");
                                break;
                            case 224:
                                level = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"HARDNESS ={level}");
                                break;
                            // 添加其他需要处理的 ModuleParId
                            default:
                                Debug.WriteLine($"未处理的 ModuleParId: {par.ModuleParId}");
                                break;
                        }
                    }
                }
            }
            else
            {
                Debug.WriteLine("没有数据");
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

            VoiceTipAction?.Invoke("请记住故事的细节并找出合适的选项。");
            SynopsisAction?.Invoke("首先您会在屏幕上看到一段小故事，请您记住其细节，并用鼠标点击屏幕上的OK键；随后您会在屏幕上看到有关于小故事的问题和四个选项，用鼠标选择合适的选项并用鼠标点击屏幕上的OK键来确认选择。");
            RuleAction?.Invoke("首先您会在屏幕上看到一段小故事，请您记住其细节，并用鼠标点击屏幕上的OK键；随后您会在屏幕上看到有关于小故事的问题和四个选项，用鼠标选择合适的选项并用鼠标点击屏幕上的OK键来确认选择。");
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


            VoiceTipAction?.Invoke("请记住故事的细节并找出合适的选项。");
            SynopsisAction?.Invoke("首先您会在屏幕上看到一段小故事，请您记住其细节，并用鼠标点击屏幕上的OK键；随后您会在屏幕上看到有关于小故事的问题和四个选项，用鼠标选择合适的选项并用鼠标点击屏幕上的OK键来确认选择。");
            RuleAction?.Invoke("首先您会在屏幕上看到一段小故事，请您记住其细节，并用鼠标点击屏幕上的OK键；随后您会在屏幕上看到有关于小故事的问题和四个选项，用鼠标选择合适的选项并用鼠标点击屏幕上的OK键来确认选择。");
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

                        // 创建 Result 记录
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "细节记忆力",
                            Lv = max_hardness, // 当前的难度级别
                            Eval = false,
                            ScheduleId = BaseParameter.ScheduleId ?? null, // 假设的 schedule_id，可以替换为实际值

                        };
                        Debug.WriteLine($"截止");
                        db.Results.Add(newResult);
                        await db.SaveChangesAsync();

                        // 获得 result_id
                        int result_id = newResult.ResultId;

                        // 创建 ResultDetail 对象列表
                        var resultDetails = new List<ResultDetail>
                            {
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = -1,
                                    ValueName = "等级",
                                    Value = max_hardness,
                                    Maxvalue = 20,
                                    Minvalue = 1,
                                    ModuleId = BaseParameter.ModuleId //  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "问题总数",
                                    Value = correctCount+errorCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "错误",
                                    Value = errorCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "重复",
                                    Value = repeatCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "正确率",
                                    Value = accuracy,
                                    Charttype = "柱状图" ,
                                    ModuleId = BaseParameter.ModuleId
                                }
                            };

                        // 插入 ResultDetail 数据
                        db.ResultDetails.AddRange(resultDetails);
                        await db.SaveChangesAsync();

                        // 输出每个 ResultDetail 对象的数据
                        Debug.WriteLine($"难度级别 {max_hardness}:");
                        foreach (var detail in resultDetails)
                        {
                            Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId}");
                        }


                        // 提交事务
                        await transaction.CommitAsync();
                        Debug.WriteLine("插入成功");
                    });
                }
                catch (Exception ex)
                {
                    // 回滚事务
                    await transaction.RollbackAsync();
                    Debug.WriteLine(ex.ToString());
                }
            }
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new 细节记忆力讲解();
        }


    }
}
