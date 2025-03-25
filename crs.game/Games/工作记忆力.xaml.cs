using crs.game.Games;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
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
using crs.core;
using crs.core.DbModels;
using System.IO;
using System.Windows.Controls.Primitives;
using System.Media;

namespace crs.game.Games
{
    /// <summary>
    /// WOME.xaml 的交互逻辑
    /// </summary>
    public partial class 工作记忆力 : BaseUserControl
    {

        // 自定义Adorner类
        public class CheckMarkAdorner : Adorner
        {
            private readonly System.Windows.Shapes.Path _checkMark;

            public CheckMarkAdorner(UIElement element, System.Windows.Shapes.Path checkMark)
                : base(element)
            {
                _checkMark = checkMark;
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                // 创建一个ScaleTransform来缩放勾选标记
                ScaleTransform scaleTransform = new ScaleTransform
                {
                    ScaleX = 5,
                    ScaleY = 5,
                    CenterX = _checkMark.Width / 2,
                    CenterY = _checkMark.Height / 2
                };

                // 将缩放变换应用到勾选标记上
                _checkMark.RenderTransform = scaleTransform;

                // 将勾选标记定位在AdornedElement的右上角
                TranslateTransform translateTransform = new TranslateTransform
                {
                    X = 50,
                    Y = 100
                };

                // 将平移变换应用到勾选标记上
                _checkMark.RenderTransform = new TransformGroup
                {
                    Children =
                    {
                        scaleTransform,
                        translateTransform
                    }
                };

                // 使用DrawingContext绘制Path
                drawingContext.DrawGeometry(
                    Brushes.Transparent, // 勾选标记不需要填充
                    new Pen(_checkMark.Stroke, _checkMark.StrokeThickness),
                    _checkMark.Data
                );
            }
        }



        private readonly string[][] imagePaths = new string[][]
        {
            new string[]
            {
                "WOME/Spades_K.jpg", "WOME/Spades_J.jpg", "WOME/Spades_10.jpg", "WOME/Hearts_10.jpg",
                "WOME/Hearts_9.jpg", "WOME/Hearts_8.jpg", "WOME/Hearts_J.jpg", "WOME/Hearts_K.jpg",
                "WOME/Spades_8.jpg", "WOME/Spades_9.jpg", "WOME/Diamonds_J.jpg", "WOME/Clubs_1.jpg",
                "WOME/Clubs_Q.jpg", "WOME/Diamonds_K.jpg", "WOME/Clubs_2.jpg", "WOME/Clubs_3.jpg",
                "WOME/Clubs_7.jpg",  "WOME/Clubs_6.jpg", "WOME/Clubs_10.jpg",
                "WOME/Diamonds_9.jpg", "WOME/Clubs_4.jpg", "WOME/Clubs_5.jpg", "WOME/Diamonds_8.jpg",
                "WOME/Diamonds_5.jpg", "WOME/Clubs_8.jpg", "WOME/Clubs_9.jpg", "WOME/Diamonds_4.jpg",
                "WOME/Diamonds_6.jpg", "WOME/Diamonds_7.jpg", "WOME/Diamonds_3.jpg", "WOME/Diamonds_2.jpg",
                "WOME/Diamonds_Q.jpg", "WOME/Clubs_K.jpg", "WOME/Clubs_J.jpg", "WOME/Diamonds_10.jpg",
                "WOME/Diamonds_1.jpg",  "WOME/Hearts_3.jpg", "WOME/Spades_4.jpg",
                "WOME/Hearts_2.jpg", "WOME/Spades_5.jpg", "WOME/Spades_7.jpg", "WOME/Hearts_Q.jpg",
                "WOME/Spades_6.jpg", "WOME/Hearts_1.jpg", "WOME/Hearts_5.jpg", "WOME/Spades_2.jpg",
                "WOME/Hearts_4.jpg", "WOME/Spades_3.jpg", "WOME/Spades_1.jpg", "WOME/Hearts_6.jpg",
                "WOME/Spades_Q.jpg", "WOME/Hearts_7.jpg"

            }
        };

        private int max_time = 1; // 窗口总的持续时间，单位分钟
        private int right_card_number = 2;
        private int total_card_number = 0;
        private int train_mode = 3; // 游戏模式，1、2或3
        private bool is_gaming = false;
        private int sucess_time = 0;
        private int fail_time = 0;
        private int level = 1; // 当前游戏难度等级
        int max_hardness = 1;
        private DispatcherTimer gameTimer;
        private DispatcherTimer displayTimer;
        private DispatcherTimer delayTimer;
        private int remainingTime;
        private int remainingDisplayTime;
        private string targetSuit; // 目标花色
        private List<string> rightCards; // 存储正确的卡片路径
        private List<string> selectedCardsOrder; // 玩家选择的卡片顺序
        private double totalAnswerTime = 0; // 总答题时间，单位为秒
        private int answerCount = 0; // 答题次数
        private DateTime gameStartTime; // 记录游戏开始的时间

        public int IfVisionFeedBack = 1;//视觉反馈，1有0没有
        public int IfAudioFeedBack = 1;//声音反馈，1有0没有

        //-------------------------------------报告所需参数---------------------------------
        private int type_input = 1;//输入方式，默认鼠标
        private int MaxGames = 10;//任务数量
        private int repet_time = 1;//重复数量
        private bool is_repe = false;//指令重复
        private bool is_display = true;//指令显示 -- 视觉指令
        private bool distraction = false;//分心
        private bool transformation = false;//转移
        private bool learn_gain = false;//听觉获得 
        private bool is_france = false;//纸牌法语
        private int card_display_time = 3; // 正确卡牌的显示时间，单位秒
        private double averageAnswerTime = 0; // 平均答题时间，单位为秒
        //-------------------------------------------------------------------------------
        private Queue<bool> recentResults = new Queue<bool>(5); // 记录最近5次游戏结果的队列
        private DispatcherTimer countdowntimer;

        private int[] correctAnswers = new int[70];
        private int[] wrongAnswers = new int[70];
        private int[] ignoreAnswers = new int[70];



        private int LEVEL_UP_THRESHOLD = 85; // 提高难度的正确率阈值（百分比）
        private int LEVEL_DOWN_THRESHOLD = 70; // 降低难度的正确率阈值（百分比）


        #region 10.31改进

        //
        int _cardWidth = 95;
        int _cardHeight = 110;

        void SetImageUniformGrid(UniformGrid uniformGrid, int num)
        {
            if (num <= 5)
            {
                _cardWidth = 120;
                _cardHeight = 150;
                uniformGrid.Columns = num;
            }
            else
            {
                _cardWidth = 95;
                _cardHeight = 110;
                uniformGrid.Columns = 9;
            }
        }

        void InitImageBackContainer2()
        {
            imageBackContainer2.Children.Clear();
            for (int i = 1; i <= 9; i++)
            {
                var img2 = new Rectangle
                {
                    Height = 110,  // 修改纸牌高度
                    Width = 65,   // 修改纸牌宽度
                    Fill = new SolidColorBrush(Colors.LightGray),
                    Margin = new Thickness(2),

                };
                imageBackContainer2.Children.Add(img2);
            }
        }

        #endregion


        private void InitializeGameSettings()
        {


            // 根据level调整游戏设置
            SetLevelSettings();

            remainingTime = max_time * 60; // 将分钟转换为秒

            if (gameTimer != null)
            {
                // 如果计时器正在运行，先停止它
                if (gameTimer.IsEnabled)
                {
                    gameTimer.Stop();
                }

                // 取消之前注册的事件（防止重复注册事件）
                gameTimer.Tick -= GameTimer_Tick;

                // 将 displayTimer 置为 null，表示它已被清理
                gameTimer = null;
            }
            gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            gameTimer.Tick += GameTimer_Tick;
        }
        public 工作记忆力()
        {
            InitializeComponent();
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

        public void Speak(string text) // 无用函数
        {
            //Process.Start(new ProcessStartInfo
            //{
            //    FileName = "cmd.exe",
            //    Arguments = $"/c mshta vbscript:Execute(\"CreateObject(\"\"SAPI.SpVoice\"\").Speak(\"\"{text}\"\") (window.close)\")",
            //    CreateNoWindow = true,
            //    UseShellExecute = false
            //});
        }
        // 在需要播报的地方调用

        private void beginButton_Click(object sender, RoutedEventArgs e)
        {
            confirm.IsEnabled = true;
            begin.IsEnabled = false;

            //清空图片，并把文本显示为默认
            imageContainer2.Children.Clear();
            imageBackContainer2.Children.Clear();
            imageContainer3.Children.Clear();
            textBlock.Background = new SolidColorBrush(Colors.Green);
            textBlock.Child = new TextBlock
            {
                Text = "",
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 36,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };



            if (is_gaming) return;



            is_gaming = true;
            gameStartTime = DateTime.Now; // 记录游戏开始的时间

            // 设置 modeTextBlock 显示当前的游戏模式提示
            if (train_mode == 1)
            {
                if (is_display)
                {
                    modeTextBlock.Text = "记住纸牌并选择相同的卡牌";
                }

                if (learn_gain)
                {
                    Speak("记住纸牌并选择相同的卡牌");
                }
                DisplayRightCardsMode1();
            }
            else if (train_mode == 2)
            {
                if (is_display)
                {

                    modeTextBlock.Text = "选择指定花色的牌";

                }
                if (learn_gain)
                {
                    Speak("选择指定花色的牌");
                }
                DisplaySuitHintAndCards();
            }
            else if (train_mode == 3)
            {
                if (is_display)
                {
                    modeTextBlock.Text = "按顺序记住并选择卡牌";
                }

                if (learn_gain)
                {
                    Speak("按顺序记住并选择卡牌");
                }
                DisplayRightCardsMode3();
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            remainingTime--;

            TimeStatisticsAction.Invoke(remainingTime, remainingDisplayTime);
            if (remainingTime <= 0)
            {
                gameTimer.Stop();

                //VIG_Report reportWindow = new VIG_Report(LEVEL_UP_THRESHOLD, LEVEL_DOWN_THRESHOLD, max_time, max_time, true, true, correctAnswers, wrongAnswers, ignoreAnswers, level);
                string correctAnswersString = string.Join(", ", correctAnswers);
                string wrongAnswersString = string.Join(", ", wrongAnswers);
                string ignoreAnswersString = string.Join(", ", ignoreAnswers);

                // 创建要显示的消息
                string message = $"Correct Answers: {correctAnswersString}\n" +
                                 $"Wrong Answers: {wrongAnswersString}\n" +
                                 $"Ignore Answers: {ignoreAnswersString}";

                // 使用 MessageBox 显示消息
                //MessageBox.Show(message, "Answers Information");
                OnGameEnd();
            }
        }



        // 游戏模式1的逻辑
        private void DisplayRightCardsMode1()
        {
            //题目规则的显示逻辑需要对不同的模式分开显示，所以显示规则的委托调用写到不同的模式显示函数里面
            RuleAction?.Invoke("请记住屏幕上显示的纸牌并在触摸屏上选择相同的卡牌。");//增加代码，调用函数，显示数字人下的文字
            VoiceTipAction?.Invoke("请您记住屏幕上显示的纸牌并在触摸屏上选择相同的卡牌。");

            // 清空之前的图片
            imageContainer.Children.Clear();
            imageBackContainer.Children.Clear();
            imageBackContainer.Visibility = Visibility.Hidden;
            imageBackContainer2.Children.Clear();

            rightCards = new List<string>();

            // 使用随机数生成器
            var random = new Random();

            // 每次随机生成不同的卡片
            int[] randomIndexes = Enumerable.Range(0, imagePaths[0].Length).OrderBy(x => random.Next()).Take(right_card_number).ToArray();

            SetImageUniformGrid(imageContainer, randomIndexes.Length);
            SetImageUniformGrid(imageBackContainer, randomIndexes.Length);


            // 显示选中的图片，并将其路径保存到 rightCards 列表
            foreach (int index in randomIndexes)
            {
                string imagePath = imagePaths[0][index];
                rightCards.Add(imagePath);
                var img = new Image
                {
                    Source = new BitmapImage(new Uri(imagePath, UriKind.Relative)),
                    Height = _cardHeight,  // 修改纸牌高度
                    Width = _cardWidth,   // 修改纸牌宽度
                    Margin = new Thickness(2)
                };
                imageContainer.Children.Add(img);

                if (level >= 10)
                {
                    var img2 = new Image
                    {
                        Source = new BitmapImage(new Uri("WOME/Joker_B.jpg", UriKind.Relative)),
                        Height = _cardHeight,  // 修改纸牌高度
                        Width = _cardWidth,   // 修改纸牌宽度
                        Margin = new Thickness(2)
                    };
                    imageBackContainer.Children.Add(img2);
                }

            }

            // 设置剩余展示时间
            remainingDisplayTime = card_display_time;

            // 显示剩余展示时间
            //TimeTextBlock.Text = remainingDisplayTime.ToString();


            // 检查是否已经存在 displayTimer 实例
            if (displayTimer != null)
            {
                // 如果计时器正在运行，先停止它
                if (displayTimer.IsEnabled)
                {
                    displayTimer.Stop();
                }

                // 取消之前注册的事件（防止重复注册事件）
                displayTimer.Tick -= DisplayTimer_TickMode1;

                // 将 displayTimer 置为 null，表示它已被清理
                displayTimer = null;
            }

            // 重新初始化 displayTimer
            displayTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            displayTimer.Tick += DisplayTimer_TickMode1;
            displayTimer.Start();
        }

        private void DisplayTimer_TickMode1(object sender, EventArgs e)
        {
            remainingDisplayTime--;
            TimeStatisticsAction.Invoke(remainingTime, remainingDisplayTime);
            // 更新TimeTextBlock中的剩余展示时间
            //TimeTextBlock.Text = remainingDisplayTime.ToString();

            if (remainingDisplayTime <= 0)
            {
                InitImageBackContainer2();

                if (level >= 10) imageBackContainer.Visibility = Visibility.Visible;

                displayTimer.Stop();
                DisplayAllCardsMode1();
            }
        }

        private void DisplayAllCardsMode1()
        {
            var random = new Random();
            var allCards = rightCards.ToList();

            // 添加额外的随机卡片到 allCards 中，直到达到 total_card_number
            var additionalCards = imagePaths[0].Except(rightCards).OrderBy(x => random.Next()).Take(total_card_number - rightCards.Count).ToList();
            allCards.AddRange(additionalCards);

            // 打乱顺序，确保与上次不同
            allCards = allCards.OrderBy(x => random.Next()).ToList();

            SetImageUniformGrid(imageContainer3, allCards.Count);
            SetImageUniformGrid(imageBackContainer3, allCards.Count);

            imageBackContainer3.Children.Clear();
            imageContainer3.Children.Clear();
            foreach (var card in allCards)
            {
                var img = new Image
                {
                    Source = new BitmapImage(new Uri(card, UriKind.Relative)),
                    Height = _cardHeight,  // 修改纸牌高度
                    Width = _cardWidth,   // 修改纸牌宽度
                    Margin = new Thickness(2),
                    Tag = card
                };
                img.MouseLeftButtonUp += CardImage_MouseLeftButtonUpMode1;
                imageContainer3.Children.Add(img);

                var img2 = new Rectangle
                {
                    Height = _cardHeight,  // 修改纸牌高度
                    Width = _cardWidth - 30,   // 修改纸牌宽度
                    Fill = new SolidColorBrush(Colors.LightGray),
                    Margin = new Thickness(2),

                };
                imageBackContainer3.Children.Add(img2);
            }
        }

        private void CardImage_MouseLeftButtonUpMode1(object sender, MouseButtonEventArgs e)
        {
            if (!is_gaming) return;

            var clickedImage = sender as Image;
            if (clickedImage == null) return;


            // 如果图像在 container2 中，移到 container3
            if (imageContainer2.Children.Contains(clickedImage))
            {
                imageContainer2.Children.Remove(clickedImage); // 从 container2 中删除图像

                foreach (Image item in imageContainer3.Children)
                {
                    if (item.Tag == clickedImage.Tag)
                    {
                        item.Visibility = Visibility.Visible;
                    }
                }
            }
            // 如果图像在 container3 中，移回 container2
            else if (imageContainer3.Children.Contains(clickedImage))
            {
                clickedImage.Visibility = Visibility.Hidden;

                Image img = new Image
                {
                    Source = clickedImage.Source,
                    Height = 110,
                    Width = 95,
                    Margin = new Thickness(2),
                    Tag = clickedImage.Tag
                };
                img.MouseLeftButtonUp += CardImage_MouseLeftButtonUpMode1;

                imageContainer2.Children.Add(img);    // 将图像重新添加到 container2
            }
        }
        private void DelayTimer_TickMode1(object sender, EventArgs e)
        {
            delayTimer.Stop();
            CheckPlayerSelectionMode1();
        }
        private void CheckPlayerSelectionMode1()
        {
            var selectedCards = imageContainer2.Children.OfType<Image>()
                .Select(img => (img.Source as BitmapImage).UriSource.ToString()).ToList();

            foreach (Image img in imageContainer2.Children)
            {
                if (rightCards.Contains((img.Source as BitmapImage).UriSource.ToString()))
                {
                    AddCheckMark(img, 1);
                }
                else
                {
                    AddCheckMark(img, 2);
                }
            }
            foreach (Image img in imageContainer3.Children)
            {
                if (rightCards.Contains((img.Source as BitmapImage).UriSource.ToString()))
                {
                    bool flag = false;
                    foreach (Image img2 in imageContainer2.Children)
                    {
                        if (img.Tag == img2.Tag)
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (flag) continue;
                    AddCheckMark(img, 3);
                }

            }


            // 检查是否多选
            if (selectedCards.Count > right_card_number)
            {

                wrongAnswers[level] += 1;
                if (IfAudioFeedBack == 1) PlayWav(ErrorSoundPath);
                if (IfVisionFeedBack == 1) ShowFeedbackImage(ErrorImage);
                UpdateResultDisplay(false);
                EndGame(false);
                return;
            }

            // 检查是否全部正确
            if (rightCards.All(selectedCards.Contains) && selectedCards.Count == right_card_number)
            {

                correctAnswers[level]++;
                if (IfAudioFeedBack == 1) PlayWav(CorrectSoundPath);
                if (IfVisionFeedBack == 1) ShowFeedbackImage(CorrectImage);
                UpdateResultDisplay(true);
                EndGame(true);
            }
            else
            {
                wrongAnswers[level] += 1;
                if (IfAudioFeedBack == 1) PlayWav(ErrorSoundPath);
                if (IfVisionFeedBack == 1) ShowFeedbackImage(ErrorImage);
                UpdateResultDisplay(false);
                EndGame(false);
            }
        }

        // 游戏模式2的逻辑
        // 游戏模式2的逻辑
        private void DisplaySuitHintAndCards()
        {
            //题目规则的显示逻辑需要对不同的模式分开显示，所以显示规则的委托调用写到不同的模式显示函数里面
            RuleAction?.Invoke("请在触摸屏上选择指定花色的牌。");//增加代码，调用函数，显示数字人下的文字
            VoiceTipAction?.Invoke("请你在触摸屏上选择指定花色的牌。");

            // 清空之前的提示
            imageContainer.Children.Clear();
            imageBackContainer.Children.Clear();
            imageBackContainer.Visibility = Visibility.Hidden;
            imageBackContainer2.Children.Clear();

            // 定义英文花色和其对应的中文
            var suitTranslations = new Dictionary<string, string>
    {
        { "Spades", "黑桃" },
        { "Hearts", "红心" },
        { "Diamonds", "方块" },
        { "Clubs", "梅花" }
    };

            // 随机选择一个花色
            var suits = new string[] { "Spades", "Hearts", "Diamonds", "Clubs" };
            var random = new Random();
            var englishSuit = suits[random.Next(suits.Length)];  // 选择一个随机的英文花色
            targetSuit = englishSuit;  // 保留英文花色，用于卡片筛选

            // 显示花色提示时使用中文
            suitTextBlock_model2.Text = $"请选择 {suitTranslations[targetSuit]} 花色的卡片";  // 中文花色
                                                                                      //var textBlock = new TextBlock
                                                                                      //{
                                                                                      //    Text = $"请选择 {suitTranslations[targetSuit]} 花色的卡片",  // 中文花色
                                                                                      //    FontSize = 24,
                                                                                      //    Foreground = new SolidColorBrush(Colors.Black),
                                                                                      //    HorizontalAlignment = HorizontalAlignment.Center,
                                                                                      //    VerticalAlignment = VerticalAlignment.Center
                                                                                      //};
                                                                                      //imageContainer.Children.Add(textBlock);



            // 设置剩余展示时间
            remainingDisplayTime = card_display_time;

            // 创建并启动提示展示计时器
            displayTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            displayTimer.Tick += DisplayTimer_TickMode2;
            displayTimer.Start();
        }

        private void DisplayTimer_TickMode2(object sender, EventArgs e)
        {
            remainingDisplayTime--;

            TimeStatisticsAction.Invoke(remainingTime, remainingDisplayTime);
            // 更新TimeTextBlock中的剩余展示时间
            //TimeTextBlock.Text = remainingDisplayTime.ToString();

            if (remainingDisplayTime <= 0)
            {
                InitImageBackContainer2();
                displayTimer.Stop();
                suitTextBlock_model2.Text = "";
                DisplaySuitCards(targetSuit);
            }
        }

        private void DisplaySuitCards(string suit)
        {
            var random = new Random();
            var allCards = new List<string>();

            // 保证至少有 right_card_number 张指定花色的卡片
            var suitCards = imagePaths[0].Where(path => path.Contains(suit)).OrderBy(x => random.Next()).Take(right_card_number).ToList();
            allCards.AddRange(suitCards);

            // 添加额外的随机卡片到 allCards 中，确保这些卡片不是指定花色
            var additionalCards = imagePaths[0]
                .Where(path => !path.Contains(suit))  // 排除目标花色的卡片
                .OrderBy(x => random.Next())
                .Take(total_card_number - suitCards.Count)
                .ToList();
            allCards.AddRange(additionalCards);

            // 打乱顺序，确保与上次不同
            allCards = allCards.OrderBy(x => random.Next()).ToList();

            SetImageUniformGrid(imageContainer3, allCards.Count);
            SetImageUniformGrid(imageBackContainer3, allCards.Count);

            // 清空并展示卡片
            imageContainer3.Children.Clear();
            imageBackContainer3.Children.Clear();
            foreach (var card in allCards)
            {
                var img = new Image
                {
                    Source = new BitmapImage(new Uri(card, UriKind.Relative)),
                    Height = _cardHeight,  // 修改纸牌高度
                    Width = _cardWidth,   // 修改纸牌宽度
                    Margin = new Thickness(2),
                    Tag = card
                };
                img.MouseLeftButtonUp += CardImage_MouseLeftButtonUpMode2;
                imageContainer3.Children.Add(img);


                var img2 = new Rectangle
                {
                    Height = _cardHeight,  // 修改纸牌高度
                    Width = _cardWidth - 30,   // 修改纸牌宽度
                    Fill = new SolidColorBrush(Colors.LightGray),
                    Margin = new Thickness(2),
                };
                imageBackContainer3.Children.Add(img2);
            }
        }

        private void CardImage_MouseLeftButtonUpMode2(object sender, MouseButtonEventArgs e)
        {
            if (!is_gaming) return;

            var clickedImage = sender as Image;
            if (clickedImage == null) return;

            // 如果图像在 container2 中，移到 container3
            if (imageContainer2.Children.Contains(clickedImage))
            {
                imageContainer2.Children.Remove(clickedImage); // 从 container2 中删除图像

                foreach (Image item in imageContainer3.Children)
                {
                    if (item.Tag == clickedImage.Tag)
                    {
                        item.Visibility = Visibility.Visible;
                    }
                }
            }
            // 如果图像在 container3 中，移回 container2
            else if (imageContainer3.Children.Contains(clickedImage))
            {
                clickedImage.Visibility = Visibility.Hidden;

                Image img = new Image
                {
                    Source = clickedImage.Source,
                    Height = 110,
                    Width = 95,
                    Margin = new Thickness(2),
                    Tag = clickedImage.Tag
                };
                img.MouseLeftButtonUp += CardImage_MouseLeftButtonUpMode2;

                imageContainer2.Children.Add(img);    // 将图像重新添加到 container2
            }
        }

        private void CardImage_MouseLeftButtonUpContainer3(object sender, MouseButtonEventArgs e)
        {
            var clickedImage = sender as Image;
            if (clickedImage == null) return;

            // 检查点击的图像是否在 container3 中
            if (imageContainer3.Children.Contains(clickedImage))
            {
                imageContainer3.Children.Remove(clickedImage); // 从 container3 中删除图像
                imageContainer2.Children.Add(clickedImage);    // 将图像重新添加到 container2

                // 修改点击事件为处理 container2 的逻辑
                clickedImage.MouseLeftButtonUp -= CardImage_MouseLeftButtonUpContainer3;
                clickedImage.MouseLeftButtonUp += CardImage_MouseLeftButtonUpMode1;
            }
        }


        private void DelayTimer_TickMode2(object sender, EventArgs e)
        {
            delayTimer.Stop();
            CheckPlayerSelectionMode2();
        }

        private void CheckPlayerSelectionMode2()
        {
            var selectedCards = imageContainer2.Children.OfType<Image>()
                .Select(img => (img.Source as BitmapImage).UriSource.ToString()).ToList();

            foreach (Image img in imageContainer2.Children)
            {
                if ((img.Source as BitmapImage).UriSource.ToString().Contains(targetSuit))
                {
                    AddCheckMark(img, 1);
                }
                else
                {
                    AddCheckMark(img, 2);
                }
            }
            foreach (Image img in imageContainer3.Children)
            {
                if ((img.Source as BitmapImage).UriSource.ToString().Contains(targetSuit))
                {
                    bool flag = false;
                    foreach (Image img2 in imageContainer2.Children)
                    {
                        if (img.Tag == img2.Tag)
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (flag) continue;
                    AddCheckMark(img, 3);
                }

            }


            // 检查是否多选
            if (selectedCards.Count > right_card_number)
            {

                wrongAnswers[level] += 1;
                if (IfAudioFeedBack == 1) PlayWav(ErrorSoundPath);
                if (IfVisionFeedBack == 1) ShowFeedbackImage(ErrorImage);
                UpdateResultDisplay(false);
                EndGame(false);
                return;
            }

            // 检查是否所有选中的卡片都属于目标花色
            if (selectedCards.All(card => card.Contains(targetSuit)) && selectedCards.Count == right_card_number)
            {
                correctAnswers[level] += 1;
                if (IfAudioFeedBack == 1) PlayWav(CorrectSoundPath);
                if (IfVisionFeedBack == 1) ShowFeedbackImage(CorrectImage);
                UpdateResultDisplay(true);
                EndGame(true);
            }
            else
            {
                wrongAnswers[level] += 1;
                if (IfAudioFeedBack == 1) PlayWav(ErrorSoundPath);
                if (IfVisionFeedBack == 1) ShowFeedbackImage(ErrorImage);
                UpdateResultDisplay(false);
                EndGame(false);
            }
        }

        // 游戏模式3的逻辑
        private void DisplayRightCardsMode3()
        {
            //题目规则的显示逻辑需要对不同的模式分开显示，所以显示规则的委托调用写到不同的模式显示函数里面
            RuleAction?.Invoke("在屏幕上按顺序记住并选择卡牌。");//增加代码，调用函数，显示数字人下的文字
            VoiceTipAction?.Invoke("在屏幕上按顺序记住并选择卡牌。");
            // 清空之前的图片
            imageContainer.Children.Clear();
            imageBackContainer.Children.Clear();
            imageBackContainer.Visibility = Visibility.Hidden;
            imageBackContainer2.Children.Clear();

            rightCards = new List<string>();
            selectedCardsOrder = new List<string>();

            // 使用随机数生成器
            var random = new Random();

            // 每次随机生成不同的卡片顺序
            int[] randomIndexes = Enumerable.Range(0, imagePaths[0].Length).OrderBy(x => random.Next()).Take(right_card_number).ToArray();

            SetImageUniformGrid(imageContainer, randomIndexes.Length);
            SetImageUniformGrid(imageBackContainer, randomIndexes.Length);

            // 按顺序选择 right_card_number 张图片
            foreach (int index in randomIndexes)
            {
                string imagePath = imagePaths[0][index];
                rightCards.Add(imagePath);
                var img = new Image
                {
                    Source = new BitmapImage(new Uri(imagePath, UriKind.Relative)),
                    Height = _cardHeight,  // 修改纸牌高度
                    Width = _cardWidth,   // 修改纸牌宽度
                    Margin = new Thickness(2)
                };
                imageContainer.Children.Add(img);

                if (level >= 10)
                {
                    var img2 = new Image
                    {
                        Source = new BitmapImage(new Uri("WOME/Joker_B.jpg", UriKind.Relative)),
                        Height = _cardHeight,  // 修改纸牌高度
                        Width = _cardWidth,   // 修改纸牌宽度
                        Margin = new Thickness(2)
                    };
                    imageBackContainer.Children.Add(img2);
                }
            }

            // 设置剩余展示时间
            remainingDisplayTime = card_display_time;

            // 显示剩余展示时间
            //TimeTextBlock.Text = remainingDisplayTime.ToString();

            // 创建并启动图片展示计时器
            displayTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            displayTimer.Tick += DisplayTimer_TickMode3;
            displayTimer.Start();
        }

        private void DisplayTimer_TickMode3(object sender, EventArgs e)
        {
            remainingDisplayTime--;
            TimeStatisticsAction.Invoke(remainingTime, remainingDisplayTime);
            // 更新TimeTextBlock中的剩余展示时间
            //TimeTextBlock.Text = remainingDisplayTime.ToString();

            if (remainingDisplayTime <= 0)
            {
                InitImageBackContainer2();

                if (level >= 10) imageBackContainer.Visibility = Visibility.Visible;

                displayTimer.Stop();
                DisplayAllCardsMode3();
            }
        }

        private void DisplayAllCardsMode3()
        {
            var random = new Random();
            var allCards = rightCards.ToList();

            // 添加额外的随机卡片到 allCards 中，直到达到 total_card_number
            var additionalCards = imagePaths[0].Except(rightCards).OrderBy(x => random.Next()).Take(total_card_number - rightCards.Count).ToList();
            allCards.AddRange(additionalCards);

            // 打乱顺序，确保与上次不同
            allCards = allCards.OrderBy(x => random.Next()).ToList();

            SetImageUniformGrid(imageContainer3, allCards.Count);
            SetImageUniformGrid(imageBackContainer3, allCards.Count);

            imageContainer3.Children.Clear();
            imageBackContainer3.Children.Clear();
            foreach (var card in allCards)
            {
                var img = new Image
                {
                    Source = new BitmapImage(new Uri(card, UriKind.Relative)),
                    Height = _cardHeight,  // 修改纸牌高度
                    Width = _cardWidth,   // 修改纸牌宽度
                    Margin = new Thickness(2),
                    Tag = card
                };
                img.MouseLeftButtonUp += CardImage_MouseLeftButtonUpMode3;
                imageContainer3.Children.Add(img);


                var img2 = new Rectangle
                {
                    Height = _cardHeight,  // 修改纸牌高度
                    Width = _cardWidth - 30,   // 修改纸牌宽度
                    Fill = new SolidColorBrush(Colors.LightGray),
                    Margin = new Thickness(2),
                };
                imageBackContainer3.Children.Add(img2);
            }
        }

        private void CardImage_MouseLeftButtonUpMode3(object sender, MouseButtonEventArgs e)
        {
            if (!is_gaming) return;

            var clickedImage = sender as Image;
            if (clickedImage == null) return;

            // 如果图像在 container2 中，移到 container3
            if (imageContainer2.Children.Contains(clickedImage))
            {
                imageContainer2.Children.Remove(clickedImage); // 从 container2 中删除图像

                foreach (Image item in imageContainer3.Children)
                {
                    if (item.Tag == clickedImage.Tag)
                    {
                        item.Visibility = Visibility.Visible;
                    }
                }
            }
            // 如果图像在 container3 中，移回 container2
            else if (imageContainer3.Children.Contains(clickedImage))
            {
                clickedImage.Visibility = Visibility.Hidden;

                Image img = new Image
                {
                    Source = clickedImage.Source,
                    Height = 110,
                    Width = 95,
                    Margin = new Thickness(2),
                    Tag = clickedImage.Tag
                };
                img.MouseLeftButtonUp += CardImage_MouseLeftButtonUpMode3;

                imageContainer2.Children.Add(img);    // 将图像重新添加到 container2
            }
        }
        private void DelayTimer_TickMode3(object sender, EventArgs e)
        {
            delayTimer.Stop();
            CheckPlayerSelectionMode3();
        }

        private void CheckPlayerSelectionMode3()
        {
            var selectedCards = imageContainer2.Children.OfType<Image>()
               .Select(img => (img.Source as BitmapImage).UriSource.ToString()).ToList();

            bool isCorrectOrder = selectedCards.SequenceEqual(rightCards);


            // 检查是否多选
            if (selectedCards.Count > right_card_number)
            {

                wrongAnswers[level] += 1;
                if (IfAudioFeedBack == 1) PlayWav(ErrorSoundPath);
                if (IfVisionFeedBack == 1) ShowFeedbackImage(ErrorImage);
                UpdateResultDisplay(false);
                EndGame(false);
                return;
            }

            // 检查顺序是否正确
            if (isCorrectOrder && selectedCards.Count == right_card_number)
            {
                correctAnswers[level] += 1;
                if (IfAudioFeedBack == 1) PlayWav(CorrectSoundPath);
                if (IfVisionFeedBack == 1) ShowFeedbackImage(CorrectImage);
                UpdateResultDisplay(true);
                EndGame(true);
            }
            else
            {
                wrongAnswers[level] += 1;
                UpdateResultDisplay(false);
                if (IfAudioFeedBack == 1) PlayWav(ErrorSoundPath);
                if (IfVisionFeedBack == 1) ShowFeedbackImage(ErrorImage);
                EndGame(false);
            }
        }

        private void confirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (remainingDisplayTime > 0)
            {
                return;
            }
            if (displayTimer != null && displayTimer.IsEnabled)
            {

                return;
            }

            if (!is_gaming)
            {
                return;
            }

            if (false)
            {

                return;
            }


            confirm.IsEnabled = false;
            begin.IsEnabled = true;



            if (train_mode == 1)
            {
                CheckPlayerSelectionMode1();
                if (level >= 10) imageBackContainer.Visibility = Visibility.Hidden;
            }
            else if (train_mode == 2)
            {
                CheckPlayerSelectionMode2();
            }
            else if (train_mode == 3)
            {
                CheckPlayerSelectionMode3();
                if (level >= 10) imageBackContainer.Visibility = Visibility.Hidden;
            }



            int correctCount = 0;
            int incorrectCount = 0;
            // 遍历队列并统计正确和错误的数量
            foreach (bool result in recentResults)
            {
                if (result)
                {
                    correctCount++;
                }
                else
                {
                    incorrectCount++;
                }
            }

            RightStatisticsAction?.Invoke(correctCount, 5);
            WrongStatisticsAction?.Invoke(incorrectCount, 5);
        }

        // 在Image上添加红色的勾选标记 ,1绿色，2红色，3橙色
        private void AddCheckMark(Image image, int selecteType)
        {
            SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Green);
            Geometry date = Geometry.Parse("M 10,70 L 30,80 M 30,80 L 60,50");
            switch (selecteType)
            {
                case 1:
                    solidColorBrush = new SolidColorBrush(Colors.Green);
                    date = Geometry.Parse("M 10,70 L 30,80 M 30,80 L 60,50");
                    break;
                case 2:
                    solidColorBrush = new SolidColorBrush(Colors.Red);
                    date = Geometry.Parse("M 10,40 L 60,80 M 10,80 L 60,40");
                    break;
                case 3:
                    solidColorBrush = new SolidColorBrush(Colors.Orange);
                    date = Geometry.Parse("M 10,70 L 30,80 M 30,80 L 60,50");
                    break;
            }

            // 创建一个Path来绘制勾选标记
            System.Windows.Shapes.Path checkMark = new System.Windows.Shapes.Path
            {
                Stroke = solidColorBrush, // 勾选颜色
                StrokeThickness = 5,
                Data = date,
                Width = 50,
                Height = 50,
            };

            // 创建一个AdornerDecorator

            AdornerLayer layer = AdornerLayer.GetAdornerLayer(image);
            if (layer != null)
            {
                // 创建一个Adorner
                CheckMarkAdorner adorner = new CheckMarkAdorner(image, checkMark);
                layer.Add(adorner);

            }
        }
        private void UpdateResultDisplay(bool isSuccess)
        {
            if (isSuccess)
            {
                textBlock.Background = new SolidColorBrush(Colors.Green);
                textBlock.Child = new TextBlock
                {
                    Text = "成功",
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 36,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
            }
            else
            {
                textBlock.Background = new SolidColorBrush(Colors.Red);
                textBlock.Child = new TextBlock
                {
                    Text = "失败",
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 36,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
            }
        }

        private void EndGame(bool gameCompleted)
        {

            is_gaming = false;

            double gameTime = (DateTime.Now - gameStartTime).TotalSeconds;
            // 显示当前时间和游戏开始时间
            //MessageBox.Show($"Current Time (DateTime.Now): {DateTime.Now}\nGame Start Time: {gameStartTime}", "Time Information");
            totalAnswerTime += gameTime;
            answerCount++;
            averageAnswerTime = totalAnswerTime / answerCount;
            string message = $"Average Answer Time: {averageAnswerTime} seconds";

            // 使用 MessageBox 显示消息
            //MessageBox.Show(message, "Average Answer Time");

            //modeTextBlock.Text = string.Empty;
            if (recentResults.Count >= 5)
            {
                recentResults.Dequeue(); // 移除最早的结果
            }
            recentResults.Enqueue(gameCompleted); // 添加当前结果
            int correctCount = recentResults.Count(result => result);
            int wrongCount = recentResults.Count(result => !result);
            if (gameCompleted)
            {
                // 处理游戏完成后的逻辑，比如显示成绩等
                // 这里可以重置状态并准备下一次游戏
                if (level < 69)
                {
                    if (correctCount >= repet_time)
                    {
                        level++; // 这里假设每次游戏成功后都提升等级
                        max_hardness = Math.Max(max_hardness, level);
                    }
                }
                SetLevelSettings(); // 调整设置
            }
            else
            {
                if (level > 1)
                {
                    if (wrongCount >= repet_time)
                    {
                        level--;
                    }

                }
                SetLevelSettings(); // 调整设置
            }
        }

        private void SetLevelSettings()
        {

            LevelStatisticsAction?.Invoke(level, 69);
            switch (level)
            {
                case 1:
                    train_mode = 1;
                    right_card_number = 1;
                    total_card_number = 4;
                    levelTextBlock.Text = "村级";
                    break;
                case 2:
                    train_mode = 1;
                    right_card_number = 1;
                    total_card_number = 5;
                    levelTextBlock.Text = "村级";
                    break;
                case 3:
                    train_mode = 1;
                    right_card_number = 2;
                    total_card_number = 4;
                    levelTextBlock.Text = "村级";
                    break;
                case 4:
                    train_mode = 1;
                    right_card_number = 3;
                    total_card_number = 5;
                    levelTextBlock.Text = "村级";
                    break;
                case 5:
                    train_mode = 1;
                    right_card_number = 3;
                    total_card_number = 5;
                    levelTextBlock.Text = "村级";
                    break;
                case 6:
                    train_mode = 1;
                    right_card_number = 3;
                    total_card_number = 6;
                    levelTextBlock.Text = "村级";
                    break;
                case 7:
                    train_mode = 2;
                    right_card_number = 3;
                    total_card_number = 6;
                    levelTextBlock.Text = "村级";
                    break;
                case 8:
                    train_mode = 2;
                    right_card_number = 3;
                    total_card_number = 7;
                    levelTextBlock.Text = "村级";
                    break;
                case 9:
                    train_mode = 2;
                    right_card_number = 3;
                    total_card_number = 8;
                    levelTextBlock.Text = "村级";
                    break;
                case 10:
                    train_mode = 2;
                    right_card_number = 3;
                    total_card_number = 5;
                    levelTextBlock.Text = "镇级";
                    break;
                case 11:
                    train_mode = 2;
                    right_card_number = 3;
                    total_card_number = 6;
                    levelTextBlock.Text = "镇级";
                    break;
                case 12:
                    train_mode = 2;
                    right_card_number = 3;
                    total_card_number = 7;
                    levelTextBlock.Text = "镇级";
                    break;
                case 13:
                    train_mode = 2;
                    right_card_number = 3;
                    total_card_number = 8;
                    levelTextBlock.Text = "镇级";
                    break;
                case 14:
                    train_mode = 2;
                    right_card_number = 3;
                    total_card_number = 9;
                    levelTextBlock.Text = "镇级";
                    break;
                case 15:
                    train_mode = 3;
                    right_card_number = 5;
                    total_card_number = 5;
                    levelTextBlock.Text = "镇级";
                    break;
                case 16:
                    train_mode = 3;
                    right_card_number = 4;
                    total_card_number = 4;
                    levelTextBlock.Text = "镇级";
                    break;
                case 17:
                    train_mode = 3;
                    right_card_number = 4;
                    total_card_number = 4;
                    levelTextBlock.Text = "镇级";
                    break;
                case 18:
                    train_mode = 1;
                    right_card_number = 4;
                    total_card_number = 5;
                    levelTextBlock.Text = "地区级";
                    break;
                case 19:
                    train_mode = 1;
                    right_card_number = 4;
                    total_card_number = 6;
                    levelTextBlock.Text = "地区级";
                    break;
                case 20:
                    train_mode = 1;
                    right_card_number = 4;
                    total_card_number = 7;
                    levelTextBlock.Text = "地区级";
                    break;
                case 21:
                    train_mode = 1;
                    right_card_number = 4;
                    total_card_number = 8;
                    levelTextBlock.Text = "地区级";
                    break;
                case 22:
                    train_mode = 1;
                    right_card_number = 4;
                    total_card_number = 9;
                    levelTextBlock.Text = "地区级";
                    break;
                case 23:
                    train_mode = 2;
                    right_card_number = 4;
                    total_card_number = 7;
                    levelTextBlock.Text = "地区级";
                    break;
                case 24:
                    train_mode = 2;
                    right_card_number = 4;
                    total_card_number = 8;
                    levelTextBlock.Text = "国家级";
                    break;
                case 25:
                    train_mode = 2;
                    right_card_number = 4;
                    total_card_number = 9;
                    levelTextBlock.Text = "国家级";
                    break;
                case 26:
                    train_mode = 3;
                    right_card_number = 5;
                    total_card_number = 5;
                    levelTextBlock.Text = "国家级";
                    break;
                case 27:
                    train_mode = 3;
                    right_card_number = 4;
                    total_card_number = 4;
                    levelTextBlock.Text = "国家级";
                    break;
                case 28:
                    train_mode = 1;
                    right_card_number = 5;
                    total_card_number = 6;
                    levelTextBlock.Text = "国家级";
                    break;
                case 29:
                    train_mode = 1;
                    right_card_number = 5;
                    total_card_number = 7;
                    levelTextBlock.Text = "国家级";
                    break;
                case 30:
                    train_mode = 1;
                    right_card_number = 5;
                    total_card_number = 8;
                    levelTextBlock.Text = "国家级";
                    break;
                case 31:
                    train_mode = 1;
                    right_card_number = 5;
                    total_card_number = 9;
                    levelTextBlock.Text = "国家级";
                    break;
                case 32:
                    train_mode = 1;
                    right_card_number = 5;
                    total_card_number = 10;
                    levelTextBlock.Text = "国家级";
                    break;
                case 33:
                    train_mode = 1;
                    right_card_number = 5;
                    total_card_number = 11;
                    levelTextBlock.Text = "国家级";
                    break;
                case 34:
                    train_mode = 2;
                    right_card_number = 5;
                    total_card_number = 8;
                    levelTextBlock.Text = "欧洲级";
                    break;
                case 35:
                    train_mode = 2;
                    right_card_number = 5;
                    total_card_number = 9;
                    levelTextBlock.Text = "欧洲级";
                    break;
                case 36:
                    train_mode = 2;
                    right_card_number = 5;
                    total_card_number = 10;
                    levelTextBlock.Text = "欧洲级";
                    break;
                case 37:
                    train_mode = 2;
                    right_card_number = 5;
                    total_card_number = 11;
                    levelTextBlock.Text = "欧洲级";
                    break;
                case 38:
                    train_mode = 3;
                    right_card_number = 8;
                    total_card_number = 8;
                    levelTextBlock.Text = "欧洲级";
                    break;
                case 39:
                    train_mode = 3;
                    right_card_number = 7;
                    total_card_number = 7;
                    levelTextBlock.Text = "欧洲级";
                    break;
                case 40:
                    train_mode = 1;
                    right_card_number = 6;
                    total_card_number = 9;
                    levelTextBlock.Text = "欧洲级";
                    break;
                case 41:
                    train_mode = 1;
                    right_card_number = 6;
                    total_card_number = 10;
                    levelTextBlock.Text = "欧洲级";
                    break;
                case 42:
                    train_mode = 1;
                    right_card_number = 6;
                    total_card_number = 11;
                    levelTextBlock.Text = "世界级";
                    break;
                case 43:
                    train_mode = 1;
                    right_card_number = 6;
                    total_card_number = 12;
                    levelTextBlock.Text = "世界级";
                    break;
                case 44:
                    train_mode = 2;
                    right_card_number = 6;
                    total_card_number = 9;
                    levelTextBlock.Text = "世界级";
                    break;
                case 45:
                    train_mode = 2;
                    right_card_number = 6;
                    total_card_number = 10;
                    levelTextBlock.Text = "世界级";
                    break;
                case 46:
                    train_mode = 2;
                    right_card_number = 6;
                    total_card_number = 11;
                    levelTextBlock.Text = "世界级";
                    break;
                case 47:
                    train_mode = 3;
                    right_card_number = 9;
                    total_card_number = 9;
                    levelTextBlock.Text = "世界级";
                    break;
                case 48:
                    train_mode = 3;
                    right_card_number = 7;
                    total_card_number = 7;
                    levelTextBlock.Text = "世界级";
                    break;
                case 49:
                    train_mode = 3;
                    right_card_number = 7;
                    total_card_number = 7;
                    levelTextBlock.Text = "世界级";
                    break;
                case 50:
                    train_mode = 1;
                    right_card_number = 7;
                    total_card_number = 10;
                    levelTextBlock.Text = "世界级";
                    break;
                case 51:
                    train_mode = 1;
                    right_card_number = 7;
                    total_card_number = 11;
                    levelTextBlock.Text = "世界级";
                    break;
                case 52:
                    train_mode = 1;
                    right_card_number = 7;
                    total_card_number = 12;
                    levelTextBlock.Text = "世界级";
                    break;
                case 53:
                    train_mode = 1;
                    right_card_number = 7;
                    total_card_number = 13;
                    levelTextBlock.Text = "世界级";
                    break;
                case 54:
                    train_mode = 1;
                    right_card_number = 7;
                    total_card_number = 14;
                    levelTextBlock.Text = "世界级";
                    break;
                case 55:
                    train_mode = 2;
                    right_card_number = 7;
                    total_card_number = 9;
                    levelTextBlock.Text = "世界级";
                    break;
                case 56:
                    train_mode = 2;
                    right_card_number = 7;
                    total_card_number = 10;
                    levelTextBlock.Text = "世界级";
                    break;
                case 57:
                    train_mode = 2;
                    right_card_number = 7;
                    total_card_number = 11;
                    levelTextBlock.Text = "世界级";
                    break;
                case 58:
                    train_mode = 3;
                    right_card_number = 8;
                    total_card_number = 8;
                    levelTextBlock.Text = "世界级";
                    break;
                case 59:
                    train_mode = 3;
                    right_card_number = 8;
                    total_card_number = 8;
                    levelTextBlock.Text = "世界级";
                    break;
                case 60:
                    train_mode = 1;
                    right_card_number = 8;
                    total_card_number = 11;
                    levelTextBlock.Text = "世界级";
                    break;
                case 61:
                    train_mode = 1;
                    right_card_number = 8;
                    total_card_number = 12;
                    levelTextBlock.Text = "世界级";
                    break;
                case 62:
                    train_mode = 1;
                    right_card_number = 8;
                    total_card_number = 13;
                    levelTextBlock.Text = "世界级";
                    break;
                case 63:
                    train_mode = 1;
                    right_card_number = 8;
                    total_card_number = 14;
                    levelTextBlock.Text = "世界级";
                    break;
                case 64:
                    train_mode = 2;
                    right_card_number = 8;
                    total_card_number = 9;
                    levelTextBlock.Text = "世界级";
                    break;
                case 65:
                    train_mode = 2;
                    right_card_number = 8;
                    total_card_number = 10;
                    levelTextBlock.Text = "世界级";
                    break;
                case 66:
                    train_mode = 2;
                    right_card_number = 8;
                    total_card_number = 11;
                    levelTextBlock.Text = "世界级";
                    break;
                case 67:
                    train_mode = 3;
                    right_card_number = 9;
                    total_card_number = 9;
                    levelTextBlock.Text = "世界级";
                    break;
                case 68:
                    train_mode = 3;
                    right_card_number = 9;
                    total_card_number = 9;
                    levelTextBlock.Text = "世界级";
                    break;
                case 69:
                    train_mode = 3;
                    right_card_number = 9;
                    total_card_number = 9;
                    levelTextBlock.Text = "世界级";
                    break;
            }
        }



        /* protected override async Task OnStopAsync()
         {
             throw new NotImplementedException();
         }*/

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

        private async void ShowFeedbackImage(Image image)
        {//显示反馈的图片
            image.Visibility = Visibility.Visible;

            // 延迟指定的时间（例如1秒）
            await Task.Delay(StopDurations);

            image.Visibility = Visibility.Collapsed;
        }
    }
    public partial class 工作记忆力 : BaseUserControl
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


            max_time = 10; // 窗口总的持续时间，单位分钟
            card_display_time = 3; // 正确卡牌的显示时间，单位秒
            right_card_number = 2;
            total_card_number = 0;
            train_mode = 3; // 游戏模式，1、2或3
            level = 1; // 当前游戏难度等级

            {
                // 参数（包含模块参数信息）
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
                            switch (par.ModuleParId)
                            {

                                case 57: // 治疗时间
                                    max_time = par.Value.HasValue ? (int)par.Value.Value : 30;
                                    Debug.WriteLine($"治疗时间 ={max_time}");
                                    break;
                                //case 58: //正确卡牌数量
                                //    right_card_number = par.Value.HasValue ? (int)par.Value.Value : 2;
                                //    Debug.WriteLine($"正确卡牌数量 ={right_card_number}");
                                //    break;
                                //case 59: // 总体卡牌数量
                                //    total_card_number = par.Value.HasValue ? (int)par.Value.Value : 0;
                                //    Debug.WriteLine($"总体卡牌数量 ={total_card_number}");
                                //    break;
                                case 60: // 卡片的显示时间
                                    card_display_time = par.Value.HasValue ? (int)par.Value.Value : 1;
                                    Debug.WriteLine($"卡片的显示时间={card_display_time}");
                                    break;
                                //case 63: // 训练模式
                                //    train_mode = par.Value.HasValue ? (int)par.Value.Value : 3;
                                //    Debug.WriteLine($"训练模式={train_mode}");
                                    break;
                                case 153: // 等级
                                    level = par.Value.HasValue ? (int)par.Value.Value : 1;
                                    Debug.WriteLine($"当前游戏难度 ={level}");
                                    break;
                                // case 65: // 成功时间
                                //     sucess_time = par.Value.HasValue ? (int)par.Value.Value : 0;
                                //     Debug.WriteLine($"成功时间 ={sucess_time}");
                                //     break;
                                //case 66: // 失败时间
                                //    fail_time = par.Value.HasValue ? (int)par.Value.Value : 0;
                                //    Debug.WriteLine($"失败时间 ={fail_time}");
                                //    break;
                                //case 67: // 是否游戏
                                //    is_gaming = par.Value == 1;
                                //    Debug.WriteLine($"是否游戏 ={is_gaming}");
                                    //break;
                                //case 68: // 等级提高
                                //    LEVEL_UP_THRESHOLD = par.Value.HasValue ? (int)par.Value.Value : 85;
                                //    Debug.WriteLine($"等级提高 ={LEVEL_UP_THRESHOLD}");
                                //    break;
                                //case 69: // 等级降低
                                //    LEVEL_DOWN_THRESHOLD = par.Value.HasValue ? (int)par.Value.Value : 70;
                                //    Debug.WriteLine($"等级降低 ={LEVEL_DOWN_THRESHOLD}");
                                //    break;
                                case 259://视觉反馈
                                    IfVisionFeedBack = par.Value.HasValue ? (int)par.Value.Value : 1;
                                    Debug.WriteLine($"视觉反馈 ={IfVisionFeedBack}");
                                    break;
                                case 260://听觉反馈
                                    IfAudioFeedBack = par.Value.HasValue ? (int)par.Value.Value : 1;
                                    Debug.WriteLine($"听觉反馈 ={IfAudioFeedBack}");
                                    break;
                                // 添加其他需要处理的 ModuleParId
                                default:
                                    Debug.WriteLine($"未处理的 ModuleParId: {par.ModuleParId} ");
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("没有数据");
                }
            }

            max_hardness = Math.Max(max_hardness, level);

            gameStartTime = DateTime.Now; // 记录游戏开始的时间
            InitializeGameSettings();
            // 调用委托
            LevelStatisticsAction?.Invoke(level, 69);
            RightStatisticsAction?.Invoke(0, 1);
            WrongStatisticsAction?.Invoke(0, 1);
        }

        protected override async Task OnStartAsync()
        {
            confirm.IsEnabled = true;
            begin.IsEnabled = false;

            if (is_gaming == false)
            {
                gameTimer.Start();
                SetLevelSettings();
                is_gaming = true;
                SynopsisAction?.Invoke("您接下来会看到三种游戏模式，其规则分别如下：\r\n模式一：记住屏幕上显示的纸牌并在触摸屏上选择相同的卡牌。\r\n模式二：在触摸屏上选择指定花色的牌。\r\n模式三：在屏幕上按顺序记住并选择卡牌。\r\n");

                // 设置 modeTextBlock 显示当前的游戏模式提示
                if (train_mode == 1)
                {
                    if (is_display)
                    {
                        modeTextBlock.Text = "记住纸牌并选择相同的卡牌";
                        //SynopsisAction?.Invoke("请记住屏幕上显示的纸牌并在触摸屏上选择相同的卡牌。");
                        //显示逻辑变更
                        //VoiceTipAction?.Invoke("请记住屏幕上显示的纸牌并在触摸屏上选择相同的卡牌。");

                    }

                    if (learn_gain)
                    {
                        VoiceTipAction?.Invoke("请记住屏幕上显示的纸牌并在触摸屏上选择相同的卡牌");
                    }
                    DisplayRightCardsMode1();
                }
                else if (train_mode == 2)
                {
                    if (is_display)
                    {
                        modeTextBlock.Text = "选择指定花色的牌";
                        //SynopsisAction?.Invoke("请在触摸屏上选择指定花色的牌");
                        //显示逻辑变更
                        //VoiceTipAction?.Invoke("请在触摸屏上选择指定花色的牌");
                    }

                    if (learn_gain)
                    {
                        VoiceTipAction?.Invoke("请在触摸屏上选择指定花色的牌");
                    }
                    DisplaySuitHintAndCards();
                }
                else if (train_mode == 3)
                {
                    if (is_display)
                    {
                        modeTextBlock.Text = "按顺序记住并选择卡牌";
                        //SynopsisAction?.Invoke("按顺序记住并选择卡牌");
                        //显示逻辑变更
                        //VoiceTipAction?.Invoke("请在屏幕上按顺序记住并选择卡牌");
                    }

                    if (learn_gain)
                    {
                        VoiceTipAction?.Invoke("请在屏幕上按顺序记住并选择卡牌");
                    }

                    DisplayRightCardsMode3();
                }
                // 调用委托
            }
            else
            {
                gameTimer.Start();
                is_gaming = true;
                if (displayTimer != null && displayTimer.IsEnabled)
                {
                    displayTimer.Start(); // 停止训练计时器
                }
            }
        }

        protected override async Task OnStopAsync()
        {
            if (gameTimer != null && gameTimer.IsEnabled)
            {
                // 计时器已存在且已经停止
                gameTimer?.Stop(); // 停止主计时器
            }
            if (displayTimer != null && displayTimer.IsEnabled)
            {
                displayTimer?.Stop(); // 停止训练计时器
            }
        }

        protected override async Task OnPauseAsync()
        {

            gameTimer?.Stop(); // 停止主计时器
            displayTimer?.Stop(); // 停止训练计时器
        }

        protected override async Task OnNextAsync()
        {
            // 调整难度
            imageContainer2.Children.Clear();
            imageContainer3.Children.Clear();
            imageContainer.Children.Clear();

            SetLevelSettings();
            is_gaming = true;

            // 设置 modeTextBlock 显示当前的游戏模式提示
            if (train_mode == 1)
            {
                modeTextBlock.Text = "记住纸牌并选择相同的卡牌";
                DisplayRightCardsMode1();
            }
            else if (train_mode == 2)
            {
                modeTextBlock.Text = "选择指定花色的牌";
                DisplaySuitHintAndCards();
            }
            else if (train_mode == 3)
            {
                modeTextBlock.Text = "按顺序记住并选择卡牌";
                DisplayRightCardsMode3();
            }
            // 调用委托
            VoiceTipAction?.Invoke("测试返回语音指令信息");
            SynopsisAction?.Invoke("测试题目说明信息");
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new 工作记忆力讲解();
        }

        // 插入写法
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
                        int trainmode = train_mode;
                        int repeat = repet_time;


                        //int correctCount = GetCorrectNum(lv);
                        //int wrongCount = GetWrongNum(lv);
                        //int ignoreCount = GetIgnoreNum(lv);
                        //int trainmode = train_mode;
                        //int repeat = repet_time;
                        //// 计算准确率
                        //double average = Math.Round((double)averageAnswerTime, 3);
                        //Debug.WriteLine($"难度级别 {average}:");
                        //double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);
                        for (int lv = 1; lv <= max_hardness; lv++)
                        {
                            // 获取当前难度级别的数据
                            correctCount += GetCorrectNum(lv);
                            wrongCount += GetWrongNum(lv);
                            ignoreCount += GetIgnoreNum(lv);
                        }

                        // 计算准确率
                        double average = Math.Round((double)averageAnswerTime, 3);
                        double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);


                        // 创建 Result 记录
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "工作记忆力",
                            Eval = false,
                            Lv = max_hardness, // 当前的难度级别
                            ScheduleId = BaseParameter.ScheduleId ?? null // 假设的 Schedule_id，可以替换为实际值
                        };
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
                                    ValueName = "等级",
                                    Value = max_hardness,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                //new ResultDetail
                                //{
                                //    ResultId = result_id,
                                //    ValueName = "任务显示",
                                //    Value = trainmode,
                                //    ModuleId = BaseParameter.ModuleId
                                //},
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "正确",
                                    Value = correctCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "错误",
                                    Value = wrongCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "重复",
                                    Value = repeat,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "平均求解时间（ms）",
                                    Value = average * 1000, // 以百分比形式存储
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


    }
}
