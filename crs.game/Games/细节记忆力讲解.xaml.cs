using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
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

namespace crs.game.Games
{
    /// <summary>
    /// 细节记忆力讲解.xaml 的交互逻辑
    /// </summary>
    public partial class 细节记忆力讲解 : BaseUserControl
    {


        public 细节记忆力讲解()
        {
            InitializeComponent();

            SetLevelParametre(level);
            LoadResources();

            InitializeTimer();
            InitCanvas();
            InitRecord();

            NextRound();


            this.Loaded += 细节记忆力讲解_Loaded;
        }

        private void 细节记忆力讲解_Loaded(object sender, RoutedEventArgs e)
        {
            Button_2_Click(null, null);
        }

        void StartGame()
        {
            StartAllTimer();
        }

        /// <summary>
        /// 前端获取的参数
        /// </summary>
        int totalTrain_time = 10;     //训练时间
        int _maxStoryRetellCount = 0;    //最大重读故事次数
        int questionTimeLimit = 10;     //获取时间期限（s），单个问题

        bool memoryMode_storySelect = false;    //记忆模式-故事选择
        bool memoryMode_shortTermMemory = true;     //记忆模式-短期记忆力

        int longTermMemory_questionCount = 0;       //长期记忆力
        bool hasRewardImage = false;    //奖励照片

        /// <summary>
        /// 等级有关参数
        /// </summary>
        int level = 1;
        int questionCount = 1;
        int allowedErrorCount = 0;      //允许错误数
        int maxStoryRetellCount = 0;    //最大重读故事次数(实际)
        bool recallStage_choiceQuestions = true;    //回忆阶段，选择
        bool recallStage_free = false;  //回忆阶段，自由
        int inputMode = 1;  //1电脑键盘，2鼠标，3触摸屏，4定制键盘

        int curLevelRepeatCount = 0;   //当前难度的尝试次数

        /// <summary>
        /// 当前题目情况
        /// </summary>
        int correctCount = 0;
        int mistakeCount = 0;
        List<Vector3> allLevelRecord;


        /// <summary>
        /// 计时器
        /// </summary>
        private DispatcherTimer _totalTimerTrain;  //总治疗时间
        private DispatcherTimer _levelTimerTrain;   //
        int totalTrainTime = 1000; //总治疗时间
        int levelTrainTime = 20;

        int curLevelState = 1;      //1记忆阶段，2回忆阶段


        /// <summary>
        /// UI
        /// </summary>
        TextBlock _story;
        TextBlock _question1;
        TextBlock _question2;
        TextBlock _answer1;
        TextBlock _answer2;
        TextBlock _answer3;
        TextBlock _answer4;
        TextBox inputTextBox;

        private Border selectedOption = null;
        int selectedOptionId = 0;  //1 2 3 4

        ///
        int curQuestionAnswerId = 0;
        string curQuestionAnswer = "";

        /// <summary>
        /// 读取每个难度的所有问题
        /// </summary>
        public class Question
        {
            public string level;
            public string story;
            public string question;
            public string answer1;
            public string answer2;
            public string answer3;
            public string answer4;
            public int realAnswer;
            Question(string _level, string _story, string _quersion,
                string _answer1, string _answer2, string _answer3, string _answer4, int _realAnswer)
            {
                level = _level;
                story = _story;
                question = _quersion;
                answer1 = _answer1;
                answer2 = _answer2;
                answer3 = _answer3;
                answer4 = _answer4;
                realAnswer = _realAnswer;

            }
            public Question(string[] lines)
            {
                level = lines[0];
                story = lines[1];
                question = lines[2];
                answer1 = lines[3];
                answer2 = lines[4];
                answer3 = lines[5];
                answer4 = lines[6];
                realAnswer = int.Parse(lines[7]);
            }
        }

        List<List<Question>> allQuestion;

        Random random = new Random();

        void NextRound()
        {
            levelTrainTime = questionTimeLimit;
            curLevelState = 1;

            selectedOptionId = 0;
            ClearInputContent();
            if (selectedOption != null)
            {
                selectedOption.Background = Brushes.White;
            }

            Question item = allQuestion[level][random.Next(allQuestion[level].Count)];
            _story.Text = item.story;
            _question1.Text = item.question;
            _question2.Text = item.question;
            _answer1.Text = item.answer1;
            _answer2.Text = item.answer2;
            _answer3.Text = item.answer3;
            _answer4.Text = item.answer4;

            curQuestionAnswerId = item.realAnswer + 1;
            switch (item.realAnswer)
            {
                case 1:
                    curQuestionAnswer = item.answer1;
                    break;
                case 2:
                    curQuestionAnswer = item.answer2;
                    break;
                case 3:
                    curQuestionAnswer = item.answer3;
                    break;
                case 4:
                    curQuestionAnswer = item.answer4;
                    break;
            }
            ShowStory();

        }


        void SetLevelParametre(int level = 1)
        {
            switch (level)
            {
                case 1:
                    questionCount = 1; allowedErrorCount = 0; maxStoryRetellCount = 2;
                    break;
                case 2:
                    questionCount = 2; allowedErrorCount = 0; maxStoryRetellCount = 2;
                    break;
                case 3:
                    questionCount = 3; allowedErrorCount = 0; maxStoryRetellCount = 2;
                    break;
                case 4:
                    questionCount = 4; allowedErrorCount = 0; maxStoryRetellCount = 2;
                    break;
                case 5:
                    questionCount = 5; allowedErrorCount = 0; maxStoryRetellCount = 2;
                    break;
                case 6:
                    questionCount = 6; allowedErrorCount = 0; maxStoryRetellCount = 2;
                    break;
                case 7:
                    questionCount = 9; allowedErrorCount = 1; maxStoryRetellCount = 3;
                    break;
                case 8:
                    questionCount = 12; allowedErrorCount = 1; maxStoryRetellCount = 3;
                    break;
                case 9:
                    questionCount = 15; allowedErrorCount = 2; maxStoryRetellCount = 4;
                    break;
                case 10:
                    questionCount = 18; allowedErrorCount = 2; maxStoryRetellCount = 4;
                    break;
            }

        }

        void InitRecord()
        {
            allLevelRecord = new List<Vector3>();
            for (int i = 0; i <= 20; i++)
            {
                allLevelRecord.Add(new Vector3(0f, 0f, 0f));
            }
        }

        void AdjustLevel()
        {
            if (correctCount + mistakeCount >= questionCount)
            {
                if (mistakeCount <= allowedErrorCount)
                {
                    if (level < 10)
                    {
                        level++;
                    }
                    SetLevelParametre(level);
                }
                else
                {
                    if (curLevelRepeatCount < maxStoryRetellCount)
                    {
                        curLevelRepeatCount++;
                        allLevelRecord[level] += new Vector3(0, 0, 1);
                    }
                    else
                    {
                        if (level > 1)
                        {
                            level--;
                        }
                    }
                    SetLevelParametre(level);
                }
                correctCount = 0;
                mistakeCount = 0;
            }
            LevelStatisticsAction?.Invoke(level, 10);
        }

        void LoadResources()
        {
            //先初始化
            allQuestion = new List<List<Question>>();
            for (int i = 0; i <= 10; i++)
            {
                allQuestion.Add(new List<Question>());
            }

            string currentDirectory = Directory.GetCurrentDirectory();
            string folderPath = System.IO.Path.Combine(currentDirectory, "Resources/细节记忆力/");
            try
            {
                for (int i = 1; i <= 10; i++)
                {
                    string txtFolderPath = folderPath + String.Format("Level{0}/", i);
                    string[] files = Directory.GetFiles(txtFolderPath, "*.txt"); // 获取所有txt文件
                    foreach (string file in files)
                    {
                        using (StreamReader sr = new StreamReader(file))
                        {
                            string[] lines = new string[10];

                            for (int j = 0; j < 8; j++)
                            {
                                lines[j] = sr.ReadLine();
                            }
                            Question _question = new Question(lines);
                            allQuestion[i].Add(_question);
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("发生错误: " + ex.Message);
            }
        }

        private void InitCanvas()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string folderPath = System.IO.Path.Combine(currentDirectory, "Resources/细节记忆力/");

            Image_1.Source = new BitmapImage(new Uri(folderPath + "x.png", UriKind.Absolute));
            Image_2.Source = new BitmapImage(new Uri(folderPath + "y.png", UriKind.Absolute));

            canvasStory.Children.Clear();
            canvasQuestion1.Children.Clear();
            canvasQuestion2.Children.Clear();

            AddOKButtonToCanvas(canvasStory);
            AddOKButtonToCanvas(canvasQuestion1);
            AddOKButtonToCanvas(canvasQuestion2);

            //初始化canvasStory
            if (true)
            {
                // 创建一个矩形作为方形区域的背景
                Rectangle storyRectangle = new Rectangle
                {
                    Width = 1000,
                    Height = 500,
                    Stroke = Brushes.Black, // 边框颜色
                    Fill = Brushes.White, // 填充颜色
                };

                // 创建一个TextBlock作为小故事文本
                TextBlock storyTextBlock = new TextBlock
                {
                    Text = "",
                    Foreground = Brushes.Black,
                    FontSize = 26,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(10)
                };

                // 设置TextBlock的宽度和高度
                storyTextBlock.Width = 800; // 减去Margin的宽度
                storyTextBlock.Height = 480; // 减去Margin的高度

                // 将TextBlock添加到Canvas
                Canvas.SetLeft(storyRectangle, (canvasStory.Width - storyRectangle.Width) / 2);
                Canvas.SetTop(storyRectangle, (canvasStory.Height - storyRectangle.Height) / 2);
                canvasStory.Children.Add(storyRectangle);

                Canvas.SetLeft(storyTextBlock, (canvasStory.Width - storyTextBlock.Width) / 2 + 10); // 偏移量为Margin
                Canvas.SetTop(storyTextBlock, (canvasStory.Height - storyTextBlock.Height) / 2 + 10); // 偏移量为Margin
                canvasStory.Children.Add(storyTextBlock);
                _story = storyTextBlock;
            }


            //初始化canvasQuestion1
            if (true)
            {
                // 创建方形区域的背景
                Border quizArea = new Border
                {
                    Width = 1000,
                    Height = 500,
                    Background = Brushes.White,
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(150, 100, 150, 100) // 居中
                };

                // 创建问题文本
                TextBlock questionText = new TextBlock
                {
                    Text = "请选择正确的答案：",
                    Foreground = Brushes.Black,
                    FontSize = 26,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(10),
                    Width = 800,
                    Height = 400
                };

                canvasQuestion1.Children.Add(quizArea);


                // 将问题文本和方形区域添加到Canvas
                Canvas.SetTop(questionText, 100);
                Canvas.SetLeft(questionText, 200);
                canvasQuestion1.Children.Add(questionText);

                // 创建选项
                CreateOption("A", quizArea, new Point(300, 300), canvasQuestion1);
                CreateOption("B", quizArea, new Point(700, 300), canvasQuestion1);
                CreateOption("C", quizArea, new Point(300, 460), canvasQuestion1);
                CreateOption("D", quizArea, new Point(700, 460), canvasQuestion1);


                _question1 = questionText;
            }

            //初始化canvasQuestion2
            if (true)
            {
                // 创建方形区域的背景
                Border inputArea = new Border
                {
                    Width = 500,
                    Height = 250,
                    Background = Brushes.White,
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(150, 100, 150, 100) // 居中
                };

                // 创建问题文本
                TextBlock questionText = new TextBlock
                {
                    Text = "请输入你的答案：",
                    FontSize = 16,
                    Margin = new Thickness(20)
                };

                // 创建输入框
                inputTextBox = new TextBox
                {
                    Width = 400,
                    Height = 30,

                    VerticalContentAlignment = VerticalAlignment.Center,
                    BorderBrush = Brushes.Black, // 边框颜色
                    BorderThickness = new Thickness(1)   // 边框厚度
                };



                // 将问题文本、输入框和获取输入内容的按钮添加到方形区域
                inputArea.Child = questionText;
                canvasQuestion2.Children.Add(inputArea);

                // 调整输入框的位置
                Canvas.SetTop(inputTextBox, 220);
                Canvas.SetLeft(inputTextBox, 200);
                canvasQuestion2.Children.Add(inputTextBox);

                _question2 = questionText;
            }
        }


        private void CreateOption(string text, Border parent, Point location, Canvas canvas)
        {
            Border option = new Border
            {
                Background = Brushes.White,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Width = 300,
                Height = 60,
                //Margin = new Thickness(10, 10, 0, 0),
                Cursor = Cursors.Hand
            };

            TextBlock optionText = new TextBlock
            {
                Text = text,
                FontSize = 26,
                Margin = new Thickness(5),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            option.MouseLeftButtonDown += (s, e) =>
            {
                if (selectedOption != null)
                {
                    selectedOption.Background = Brushes.White;
                }
                option.Background = Brushes.LightGreen;
                selectedOption = option;
                switch (text)
                {
                    case "A":
                        selectedOptionId = 1;
                        break;
                    case "B":
                        selectedOptionId = 2;
                        break;
                    case "C":
                        selectedOptionId = 3;
                        break;
                    case "D":
                        selectedOptionId = 4;
                        break;
                }

                // MessageBox.Show($"当前选中的选项是：{text}");
            };

            option.Child = optionText;
            canvas.Children.Add(option);
            Canvas.SetLeft(option, location.X);
            Canvas.SetTop(option, location.Y);
            parent.Child = null; // 防止将option添加到parent中

            switch (text)
            {
                case "A":
                    _answer1 = optionText;
                    break;
                case "B":
                    _answer2 = optionText;
                    break;
                case "C":
                    _answer3 = optionText;
                    break;
                case "D":
                    _answer4 = optionText;
                    break;
            }
        }

        string GetInputContent()
        {
            return inputTextBox.Text;
        }
        void ClearInputContent()
        {
            inputTextBox.Text = string.Empty;
        }
        void ShowStory()
        {
            canvasStory.Visibility = Visibility.Visible;

            canvasQuestion1.Visibility = Visibility.Hidden;

            canvasQuestion2.Visibility = Visibility.Hidden;

        }
        void ShowQuestion1()
        {
            canvasStory.Visibility = Visibility.Hidden;

            canvasQuestion1.Visibility = Visibility.Visible;

            canvasQuestion2.Visibility = Visibility.Hidden;

        }
        void ShowQuestion2()
        {
            canvasStory.Visibility = Visibility.Hidden;

            canvasQuestion1.Visibility = Visibility.Hidden;

            canvasQuestion2.Visibility = Visibility.Visible;

        }
        void AddOKButtonToCanvas(Canvas canvas)
        {
            double x = 700;
            double y = 800;

            // 创建一个圆形的Ellipse作为按钮的背景
            Ellipse circle = new Ellipse
            {
                Width = 100,
                Height = 100,
                Stroke = Brushes.Black, // 边框颜色
                Fill = Brushes.LightBlue, // 填充颜色
            };

            // 创建一个TextBlock作为按钮文本
            TextBlock text = new TextBlock
            {
                Text = "OK",
                Foreground = Brushes.Black,
                FontSize = 26,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // 将TextBlock添加到Ellipse中
            circle.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            circle.Arrange(new Rect(circle.DesiredSize));
            circle.Dispatcher.Invoke(() =>
            {
                circle.Clip = new RectangleGeometry { Rect = new Rect(0, 0, circle.ActualWidth, circle.ActualHeight) };
                circle.StrokeThickness = 3;
                circle.UseLayoutRounding = true;
            });

            // 创建一个Button
            Button circleButton = new Button
            {
                Content = text,
                Background = Brushes.Transparent, // 按钮背景透明
                Cursor = Cursors.Hand,
                BorderBrush = Brushes.Transparent, // 按钮边框透明
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = circle.Width,
                Height = circle.Height
            };

            // 绑定点击事件
            circleButton.Click += CircleButton_Click;

            // 将Ellipse和Button添加到Canvas
            Canvas.SetLeft(circle, x - circle.Width / 2);
            Canvas.SetTop(circle, y - circle.Height / 2);
            canvas.Children.Add(circle);
            Canvas.SetLeft(circleButton, x - circleButton.Width / 2);
            Canvas.SetTop(circleButton, y - circleButton.Height / 2);
            canvas.Children.Add(circleButton);
        }

        private void CircleButton_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("圆形按钮被点击！");
            if (curLevelState == 1)
            {
                curLevelState = 2;
                if (recallStage_choiceQuestions == true)
                {
                    ShowQuestion1();
                }
                else
                {
                    ShowQuestion2();
                }
                Text_Result.Text = "";
                levelTrainTime = questionTimeLimit;
            }
            else if (curLevelState == 2)
            {
                if (recallStage_choiceQuestions == true)
                {
                    if (selectedOptionId == curQuestionAnswerId)
                    {
                        correctCount++;
                        allLevelRecord[level] += new Vector3(1, 0, 0);

                        Text_Result.Visibility = Visibility.Visible;
                        Text_Result.Text = "回答正确。";

                        end.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        mistakeCount++;
                        allLevelRecord[level] += new Vector3(0, 1, 0);

                        Text_Result.Visibility = Visibility.Visible;
                        Text_Result.Text = "回答错误。";

                        AdjustLevel();
                        NextRound();
                    }
                }
                else
                {
                    if (GetInputContent() == curQuestionAnswer)
                    {
                        correctCount++;
                        allLevelRecord[level] += new Vector3(1, 0, 0);
                    }
                    else
                    {
                        mistakeCount++;
                        allLevelRecord[level] += new Vector3(0, 1, 0);
                    }
                }


            }

        }

        private void InitializeTimer()
        {
            _totalTimerTrain = new DispatcherTimer();
            _totalTimerTrain.Interval = TimeSpan.FromSeconds(1);
            _totalTimerTrain.Tick += TimerTotalTrain_Tick;

            _levelTimerTrain = new DispatcherTimer();
            _levelTimerTrain.Interval = TimeSpan.FromSeconds(1);
            _levelTimerTrain.Tick += TimerLevelTrain_Tick;
        }

        void StartAllTimer()
        {
            _totalTimerTrain?.Start();
            _levelTimerTrain?.Start();
        }

        void StopAllTimer()
        {
            _totalTimerTrain?.Stop();
            _levelTimerTrain?.Stop();
        }

        private void TimerLevelTrain_Tick(object sender, EventArgs e)
        {
            levelTrainTime--;

            if (curLevelState == 1)
            {
                if (levelTrainTime <= 0)
                {
                    curLevelState = 2;
                    if (recallStage_choiceQuestions == true)
                    {
                        ShowQuestion1();

                    }
                    else
                    {
                        ShowQuestion2();
                    }
                    levelTrainTime = questionTimeLimit;
                }
            }
            else if (curLevelState == 2)
            {
                if (levelTrainTime <= 0)
                {

                    if (recallStage_choiceQuestions == true)
                    {
                        if (selectedOptionId == curQuestionAnswerId)
                        {
                            correctCount++;
                            allLevelRecord[level] += new Vector3(1, 0, 0);
                        }
                        else
                        {
                            mistakeCount++;
                            allLevelRecord[level] += new Vector3(0, 1, 0);
                        }
                    }
                    else
                    {
                        if (GetInputContent() == curQuestionAnswer)
                        {
                            correctCount++;
                            allLevelRecord[level] += new Vector3(1, 0, 0);
                        }
                        else
                        {
                            mistakeCount++;
                            allLevelRecord[level] += new Vector3(0, 1, 0);
                        }
                    }
                    AdjustLevel();
                    NextRound();
                }
            }


        }

        private void TimerTotalTrain_Tick(object sender, EventArgs e)
        {
            totalTrainTime--;
            if (totalTrainTime < 0)
            {

            }
            TimeStatisticsAction?.Invoke(totalTrainTime, levelTrainTime);
        }




        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            OnGameBegin();
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
                        Text_2.Visibility = Visibility.Hidden;
                        Image_2.Visibility = Visibility.Hidden;

                        Button_1.IsEnabled = false;
                        Button_2.Content = "下一步";

                        await OnVoicePlayAsync(Text_1.Text);

                    }
                    break;
                case 1:
                    {
                        Text_1.Visibility = Visibility.Hidden;
                        Image_1.Visibility = Visibility.Hidden;
                        Text_2.Visibility = Visibility.Visible;
                        Image_2.Visibility = Visibility.Visible;

                        Button_1.IsEnabled = true;
                        Button_2.Content = "试玩";

                        await OnVoicePlayAsync(Text_2.Text);
                    }
                    break;
                case 2:
                    {
                        Text_1.Visibility = Visibility.Hidden;
                        Image_1.Visibility = Visibility.Hidden;
                        Text_2.Visibility = Visibility.Hidden;
                        Image_2.Visibility = Visibility.Hidden;

                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Collapsed;

                        ALL_Canvs.Visibility = Visibility.Visible;

                        //LJN，在讲解模块调用委托
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("首先您会在屏幕上看到一段小故事，请您记住其细节，并用鼠标点击屏幕上的OK键；随后您会在屏幕上看到有关于小故事的问题和四个选项，用鼠标选择合适的选项并用鼠标点击屏幕上的OK键来确认选择。");//增加代码，调用函数，显示数字人下的文字
                        //LJN

                        StartGame();
                    }
                    break;
            }
        }
    }
}
