using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace crs.game.Games
{
    /// <summary>
    /// 工作记忆力讲解.xaml 的交互逻辑
    /// </summary>
    public partial class 工作记忆力讲解 : BaseUserControl
    {
        private readonly string[][] imagePaths = new string[][]
         {
            new string[]
            {
                "Img2/1/Spades_K.jpg", "Img2/1/Spades_J.jpg", "Img2/1/Spades_10.jpg", "Img2/1/Hearts_10.jpg",
                "Img2/1/Hearts_9.jpg", "Img2/1/Hearts_8.jpg", "Img2/1/Hearts_J.jpg", "Img2/1/Hearts_K.jpg",
                "Img2/1/Spades_8.jpg", "Img2/1/Spades_9.jpg", "Img2/1/Diamonds_J.jpg", "Img2/1/Clubs_1.jpg",
                "Img2/1/Clubs_Q.jpg", "Img2/1/Diamonds_K.jpg", "Img2/1/Clubs_2.jpg", "Img2/1/Clubs_3.jpg",
                "Img2/1/Clubs_7.jpg",  "Img2/1/Clubs_6.jpg", "Img2/1/Clubs_10.jpg",
                "Img2/1/Diamonds_9.jpg", "Img2/1/Clubs_4.jpg", "Img2/1/Clubs_5.jpg", "Img2/1/Diamonds_8.jpg",
                "Img2/1/Diamonds_5.jpg", "Img2/1/Clubs_8.jpg", "Img2/1/Clubs_9.jpg", "Img2/1/Diamonds_4.jpg",
                "Img2/1/Diamonds_6.jpg", "Img2/1/Diamonds_7.jpg", "Img2/1/Diamonds_3.jpg", "Img2/1/Diamonds_2.jpg",
                "Img2/1/Diamonds_Q.jpg", "Img2/1/Clubs_K.jpg", "Img2/1/Clubs_J.jpg", "Img2/1/Diamonds_10.jpg",
                "Img2/1/Diamonds_1.jpg", "Img2/1/Hearts_3.jpg", "Img2/1/Spades_4.jpg",
                "Img2/1/Hearts_2.jpg", "Img2/1/Spades_5.jpg", "Img2/1/Spades_7.jpg", "Img2/1/Hearts_Q.jpg",
                "Img2/1/Spades_6.jpg", "Img2/1/Hearts_1.jpg", "Img2/1/Hearts_5.jpg", "Img2/1/Spades_2.jpg",
                "Img2/1/Hearts_4.jpg", "Img2/1/Spades_3.jpg", "Img2/1/Spades_1.jpg", "Img2/1/Hearts_6.jpg",
                "Img2/1/Spades_Q.jpg", "Img2/1/Hearts_7.jpg"
            }
         };
        private int max_time = 1; // 窗口总的持续时间，单位分钟
        private int card_display_time = 3; // 正确卡牌的显示时间，单位秒
        private int right_card_number = 2;
        private int total_card_number = 0;
        private int train_mode = 1; // 游戏模式，1、2或3
        private bool is_gaming = false;
        private int sucess_time = 0;
        private int fail_time = 0;
        private int level = 1; // 当前游戏难度等级
        private DispatcherTimer gameTimer;
        private DispatcherTimer displayTimer;
        private DispatcherTimer delayTimer;
        private int remainingTime;
        private int remainingDisplayTime;
        private string targetSuit; // 目标花色
        private List<string> rightCards; // 存储正确的卡片路径
        private List<string> selectedCardsOrder; // 玩家选择的卡片顺序
        private const int MaxGames = 10;


        private int[] correctAnswers = new int[MaxGames];
        private int[] wrongAnswers = new int[MaxGames];
        private int[] ignoreAnswers = new int[MaxGames];


        private int LEVEL_UP_THRESHOLD = 85; // 提高难度的正确率阈值（百分比）
        private int LEVEL_DOWN_THRESHOLD = 70; // 降低难度的正确率阈值（百分比）
        public Action GameBeginAction { get; set; }

        public Func<string, Task> VoicePlayFunc { get; set; }

        public 工作记忆力讲解()
        {
            InitializeComponent();
            InitializeGameSettings();

            // 窗口加载时启动max_time计时器
            gameTimer.Start();

            this.Loaded += 工作记忆力讲解_Loaded;


        }

        private void 工作记忆力讲解_Loaded(object sender, RoutedEventArgs e)
        {
            // 页面加载时确保按键和焦点行为
            Button_2_Click(null, null);
            this.Focus();  // 确保焦点在窗口上
        }

        private void InitializeGameSettings()
        {
            // 根据level调整游戏设置
            //
            //SetLevelSettings();

            remainingTime = max_time * 60; // 将分钟转换为秒
            gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            gameTimer.Tick += GameTimer_Tick;
        }



        private void beginButton_Click(object sender, RoutedEventArgs e)
        {
            //清空图片，并把文本显示为默认
            imageContainer2.Children.Clear();
            imageContainer3.Children.Clear();
            textBlock.Background = new SolidColorBrush(Colors.Green);
            textBlock.Child = new TextBlock
            {
                Text = "",
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 26,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };


            if (is_gaming) return;

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
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            remainingTime--;

            if (remainingTime <= 0)
            {
                gameTimer.Stop();
                correctAnswers[0] = sucess_time;
                wrongAnswers[0] = fail_time;
                ignoreAnswers[0] = 0;

            }
        }

        // 游戏模式1的逻辑
        private void DisplayRightCardsMode1()
        {
            // 清空之前的图片
            imageContainer.Children.Clear();
            rightCards = new List<string>();

            // 使用随机数生成器
            var random = new Random();

            // 每次随机生成不同的卡片
            int[] randomIndexes = Enumerable.Range(0, imagePaths[0].Length).OrderBy(x => random.Next()).Take(right_card_number).ToArray();

            // 显示选中的图片，并将其路径保存到 rightCards 列表
            foreach (int index in randomIndexes)
            {
                string imagePath = imagePaths[0][index];
                rightCards.Add(imagePath);
                var img = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri(imagePath, UriKind.Relative)),
                    Height = 100,
                    Margin = new Thickness(5)
                };
                imageContainer.Children.Add(img);
            }

            // 设置剩余展示时间
            remainingDisplayTime = card_display_time;

            // 显示剩余展示时间
            TimeTextBlock.Text = remainingDisplayTime.ToString();

            // 创建并启动图片展示计时器
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

            // 更新TimeTextBlock中的剩余展示时间
            TimeTextBlock.Text = remainingDisplayTime.ToString();

            if (remainingDisplayTime <= 0)
            {
                displayTimer.Stop();
                imageContainer.Children.Clear();
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

            imageContainer3.Children.Clear();
            foreach (var card in allCards)
            {
                System.Windows.Controls.Image img = new System.Windows.Controls.Image()
                {
                    Source = new BitmapImage(new Uri(card, UriKind.Relative)),
                    Height = 100,
                    Margin = new Thickness(5),
                    Tag = card
                };
                img.MouseLeftButtonUp += CardImage_MouseLeftButtonUpMode1;
                imageContainer3.Children.Add(img);
            }
        }

        private void CardImage_MouseLeftButtonUpMode1(object sender, MouseButtonEventArgs e)
        {
            if (!is_gaming) return;

            var clickedImage = sender as System.Windows.Controls.Image;
            if (clickedImage == null) return;

            // 如果图像在 container2 中，移到 container3
            if (imageContainer2.Children.Contains(clickedImage))
            {
                imageContainer2.Children.Remove(clickedImage); // 从 container2 中删除图像
                imageContainer3.Children.Add(clickedImage);    // 将图像添加到 container3
            }
            // 如果图像在 container3 中，移回 container2
            else if (imageContainer3.Children.Contains(clickedImage))
            {
                imageContainer3.Children.Remove(clickedImage); // 从 container3 中删除图像
                imageContainer2.Children.Add(clickedImage);    // 将图像重新添加到 container2
            }
        }
        private void DelayTimer_TickMode1(object sender, EventArgs e)
        {
            delayTimer.Stop();
            CheckPlayerSelectionMode1();
        }
        private void CheckPlayerSelectionMode1()
        {
            var selectedCards = imageContainer2.Children.OfType<System.Windows.Controls.Image>()
                .Select(img => (img.Source as BitmapImage).UriSource.ToString()).ToList();

            // 检查是否多选
            if (selectedCards.Count > right_card_number)
            {
                //MessageBox.Show("您选择了过多的卡片，多选不得分！");
                fail_time++;
                UpdateResultDisplay(false);
                EndGame(false);
                return;
            }

            // 检查是否全部正确
            if (rightCards.All(selectedCards.Contains) && selectedCards.Count == right_card_number)
            {
                sucess_time++;
                UpdateResultDisplay(true);
                EndGame(true);
            }
            else
            {
                fail_time++;
                UpdateResultDisplay(false);
                EndGame(false);
            }
        }

        // 游戏模式2的逻辑
        private void DisplaySuitHintAndCards()
        {
            // 清空之前的提示
            imageContainer.Children.Clear();

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

            suitTextBlock_model2.Text = $"请选择 {suitTranslations[targetSuit]} 花色的卡片";  // 中文花色
            // 显示花色提示
            //var textBlock = new TextBlock
            //{
            //    Text = $"请选择 {targetSuit} 花色的卡片",
            //    FontSize = 36,
            //    Foreground = new SolidColorBrush(Colors.Black),
            //    HorizontalAlignment = HorizontalAlignment.Center,
            //    VerticalAlignment = VerticalAlignment.Center
            //};
            //imageContainer.Children.Add(textBlock);

            // 设置剩余展示时间
            remainingDisplayTime = card_display_time;

            // 显示剩余展示时间
            TimeTextBlock.Text = remainingDisplayTime.ToString();

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

            // 更新TimeTextBlock中的剩余展示时间
            TimeTextBlock.Text = remainingDisplayTime.ToString();

            if (remainingDisplayTime <= 0)
            {
                displayTimer.Stop();
                imageContainer.Children.Clear();
                suitTextBlock_model2.Text = "";
                DisplaySuitCards(targetSuit);
            }
        }

        private void DisplaySuitCards(string suit)
        {
            var random = new Random();
            var allCards = new List<string>();

            // 保证至少有right_card_number张指定花色的卡片
            var suitCards = imagePaths[0].Where(path => path.Contains(suit)).OrderBy(x => random.Next()).Take(right_card_number).ToList();
            allCards.AddRange(suitCards);

            // 添加额外的随机卡片到 allCards 中，直到达到 total_card_number
            var additionalCards = imagePaths[0].Except(suitCards).OrderBy(x => random.Next()).Take(total_card_number - suitCards.Count).ToList();
            allCards.AddRange(additionalCards);

            // 打乱顺序，确保与上次不同
            allCards = allCards.OrderBy(x => random.Next()).ToList();

            imageContainer3.Children.Clear();
            foreach (var card in allCards)
            {
                var img = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri(card, UriKind.Relative)),
                    Height = 100,
                    Margin = new Thickness(5),
                    Tag = card
                };
                img.MouseLeftButtonUp += CardImage_MouseLeftButtonUpMode2;
                imageContainer3.Children.Add(img);
            }
        }

        private void CardImage_MouseLeftButtonUpMode2(object sender, MouseButtonEventArgs e)
        {
            if (!is_gaming) return;

            var clickedImage = sender as System.Windows.Controls.Image;
            if (clickedImage == null) return;

            // 如果图像在 container2 中，移到 container3
            if (imageContainer2.Children.Contains(clickedImage))
            {
                imageContainer2.Children.Remove(clickedImage); // 从 container2 中删除图像
                imageContainer3.Children.Add(clickedImage);    // 将图像添加到 container3
            }
            // 如果图像在 container3 中，移回 container2
            else if (imageContainer3.Children.Contains(clickedImage))
            {
                imageContainer3.Children.Remove(clickedImage); // 从 container3 中删除图像
                imageContainer2.Children.Add(clickedImage);    // 将图像重新添加到 container2
            }
        }

        private void CardImage_MouseLeftButtonUpContainer3(object sender, MouseButtonEventArgs e)
        {
            var clickedImage = sender as System.Windows.Controls.Image;
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
            var selectedCards = imageContainer2.Children.OfType<System.Windows.Controls.Image>()
                .Select(img => (img.Source as BitmapImage).UriSource.ToString()).ToList();

            // 检查是否多选
            if (selectedCards.Count > right_card_number)
            {
                //essageBox.Show("您选择了过多的卡片，多选不得分！");
                fail_time++;
                UpdateResultDisplay(false);
                EndGame(false);
                return;
            }

            // 检查是否所有选中的卡片都属于目标花色
            if (selectedCards.All(card => card.Contains(targetSuit)) && selectedCards.Count == right_card_number)
            {
                sucess_time++;
                UpdateResultDisplay(true);
                EndGame(true);
            }
            else
            {
                fail_time++;
                UpdateResultDisplay(false);
                EndGame(false);
            }
        }

        // 游戏模式3的逻辑
        private void DisplayRightCardsMode3()
        {
            // 清空之前的图片
            imageContainer.Children.Clear();
            rightCards = new List<string>();
            selectedCardsOrder = new List<string>();

            // 使用随机数生成器
            var random = new Random();

            // 每次随机生成不同的卡片顺序
            int[] randomIndexes = Enumerable.Range(0, imagePaths[0].Length).OrderBy(x => random.Next()).Take(right_card_number).ToArray();

            // 按顺序选择 right_card_number 张图片
            foreach (int index in randomIndexes)
            {
                string imagePath = imagePaths[0][index];
                rightCards.Add(imagePath);
                var img = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri(imagePath, UriKind.Relative)),
                    Height = 100,
                    Margin = new Thickness(5)
                };
                imageContainer.Children.Add(img);
            }

            // 设置剩余展示时间
            remainingDisplayTime = card_display_time;

            // 显示剩余展示时间
            TimeTextBlock.Text = remainingDisplayTime.ToString();

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

            // 更新TimeTextBlock中的剩余展示时间
            TimeTextBlock.Text = remainingDisplayTime.ToString();

            if (remainingDisplayTime <= 0)
            {
                displayTimer.Stop();
                imageContainer.Children.Clear();
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

            imageContainer3.Children.Clear();
            foreach (var card in allCards)
            {
                var img = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri(card, UriKind.Relative)),
                    Height = 100,
                    Margin = new Thickness(5),
                    Tag = card
                };
                img.MouseLeftButtonUp += CardImage_MouseLeftButtonUpMode3;
                imageContainer3.Children.Add(img);
            }
        }

        private void CardImage_MouseLeftButtonUpMode3(object sender, MouseButtonEventArgs e)
        {
            if (!is_gaming) return;

            var clickedImage = sender as System.Windows.Controls.Image;
            if (clickedImage == null) return;

            // 如果图像在 container2 中，移到 container3
            if (imageContainer2.Children.Contains(clickedImage))
            {
                imageContainer2.Children.Remove(clickedImage); // 从 container2 中删除图像
                imageContainer3.Children.Add(clickedImage);    // 将图像添加到 container3
            }
            // 如果图像在 container3 中，移回 container2
            else if (imageContainer3.Children.Contains(clickedImage))
            {
                imageContainer3.Children.Remove(clickedImage); // 从 container3 中删除图像
                imageContainer2.Children.Add(clickedImage);    // 将图像重新添加到 container2
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
                //MessageBox.Show("您选择了过多的卡片，多选不得分！");
                fail_time++;
                UpdateResultDisplay(false);
                EndGame(false);
                return;
            }

            // 检查顺序是否正确
            if (isCorrectOrder && selectedCards.Count == right_card_number)
            {
                sucess_time++;
                UpdateResultDisplay(true);
                EndGame(true);
            }
            else
            {
                fail_time++;
                UpdateResultDisplay(false);
                EndGame(false);
            }
        }

        private void confirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (displayTimer != null && displayTimer.IsEnabled)
            {
                //MessageBox.Show("请等待卡牌展示倒计时结束后再进行确认！");
                return;
            }

            if (!is_gaming)
            {
                return;
            }

            if (false)
            {
                MessageBox.Show("请先选择指定数量的卡片！");
                return;
            }

            if (train_mode == 1)
            {
                CheckPlayerSelectionMode1();
            }
            else if (train_mode == 2)
            {
                CheckPlayerSelectionMode2();
            }
            else if (train_mode == 3)
            {
                CheckPlayerSelectionMode3();
            }
        }
        private void UpdateResultDisplay(bool isSuccess)
        {
            if (isSuccess)
            {
                textBlock.Background = new SolidColorBrush(Colors.Green);
                textBlock.Child = new TextBlock
                {
                    Text = "恭喜你答对了",
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 36,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };
            }
            else
            {
                textBlock.Background = new SolidColorBrush(Colors.Red);
                textBlock.Child = new TextBlock
                {
                    Text = "很遗憾答错了",
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 36,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };
            }
        }

        private void EndGame(bool gameCompleted)
        {
            is_gaming = false;
            imageContainer2.Children.Clear();
            imageContainer3.Children.Clear();
            modeTextBlock.Text = string.Empty;



            if (gameCompleted)
            {
                if (level == 15)
                {
                    end.Visibility = Visibility.Visible;
                    confirm.Visibility = Visibility.Collapsed;
                    begin.Visibility = Visibility.Collapsed;
                }
                else if (level == 1)
                {
                    level = 7;
                }
                else if (level == 7)
                {
                    level = 15;
                }
                SetLevelSettings();
            }
            else
            {


            }
        }

        private void SetLevelSettings()
        {
            switch (level)
            {
                case 1:
                    train_mode = 1;
                    right_card_number = 2;
                    total_card_number = 4;
                    levelTextBlock.Text = "村级";
                    break;
                case 2:
                    train_mode = 1;
                    right_card_number = 3;
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

        private void end_Click(object sender, RoutedEventArgs e)
        {
            // 开始答题的相关逻辑
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
                        // 显示讲解的第一个界面

                        Text_1.Visibility = Visibility.Visible;
                        Image_1.Visibility = Visibility.Visible;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Text_3.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;
                        Text_4.Visibility = Visibility.Collapsed;
                        Image_4.Visibility = Visibility.Collapsed;
                        Text_5.Visibility = Visibility.Collapsed;
                        Image_5.Visibility = Visibility.Collapsed;
                        Text_6.Visibility = Visibility.Collapsed;
                        Image_6.Visibility = Visibility.Collapsed;
                        brownBorder.Visibility = Visibility.Collapsed;
                        imageContainer.Visibility = Visibility.Collapsed;
                        imageContainer2.Visibility = Visibility.Collapsed;
                        imageContainer3.Visibility = Visibility.Collapsed;
                        textBlock.Visibility = Visibility.Collapsed;
                        modeTextBlock.Visibility = Visibility.Collapsed;
                        levelTextBlock.Visibility = Visibility.Collapsed;
                        TimeTextBlock.Visibility = Visibility.Collapsed;
                        begin.Visibility = Visibility.Collapsed;
                        confirm.Visibility = Visibility.Collapsed;
                        blackrow1.Visibility = Visibility.Collapsed;
                        blackrow2.Visibility = Visibility.Collapsed;
                        blackrow3.Visibility = Visibility.Collapsed;
                        Button_1.IsEnabled = false;
                        Button_2.Content = "下一步";
						Button_1.Visibility = Visibility.Collapsed;
						Button_2.Margin = new Thickness(329, 850, 0, 0);
						Button_3.Margin = new Thickness(770, 850, 0, 0);

						await OnVoicePlayAsync(Text_1.Text);
                    }
                    break;
                case 1:
                    {
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Visible;
                        Image_2.Visibility = Visibility.Visible;
                        Text_3.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;
                        Text_4.Visibility = Visibility.Collapsed;
                        Image_4.Visibility = Visibility.Collapsed;
                        Text_5.Visibility = Visibility.Collapsed;
                        Image_5.Visibility = Visibility.Collapsed;
                        Text_6.Visibility = Visibility.Collapsed;
                        Image_6.Visibility = Visibility.Collapsed;
                        brownBorder.Visibility = Visibility.Collapsed;
                        imageContainer.Visibility = Visibility.Collapsed;
                        imageContainer2.Visibility = Visibility.Collapsed;
                        imageContainer3.Visibility = Visibility.Collapsed;
                        textBlock.Visibility = Visibility.Collapsed;
                        modeTextBlock.Visibility = Visibility.Collapsed;
                        levelTextBlock.Visibility = Visibility.Collapsed;
                        TimeTextBlock.Visibility = Visibility.Collapsed;
                        begin.Visibility = Visibility.Collapsed;
                        confirm.Visibility = Visibility.Collapsed;
                        blackrow1.Visibility = Visibility.Collapsed;
                        blackrow2.Visibility = Visibility.Collapsed;
                        blackrow3.Visibility = Visibility.Collapsed;
                        Button_1.IsEnabled = true;
                        Button_2.Content = "下一步";
						Button_1.Visibility = Visibility.Visible;
						Button_2.Margin = new Thickness(550, 850, 0, 0);
						Button_3.Margin = new Thickness(911, 850, 0, 0);


						await OnVoicePlayAsync(Text_2.Text);
                    }
                    break;
                case 2:
                    {
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Text_3.Visibility = Visibility.Visible;
                        Image_3.Visibility = Visibility.Visible;
                        Text_4.Visibility = Visibility.Collapsed;
                        Image_4.Visibility = Visibility.Collapsed;
                        Text_5.Visibility = Visibility.Collapsed;
                        Image_5.Visibility = Visibility.Collapsed;
                        Text_6.Visibility = Visibility.Collapsed;
                        Image_6.Visibility = Visibility.Collapsed;
                        brownBorder.Visibility = Visibility.Collapsed;
                        imageContainer.Visibility = Visibility.Collapsed;
                        imageContainer2.Visibility = Visibility.Collapsed;
                        imageContainer3.Visibility = Visibility.Collapsed;
                        textBlock.Visibility = Visibility.Collapsed;
                        modeTextBlock.Visibility = Visibility.Collapsed;
                        levelTextBlock.Visibility = Visibility.Collapsed;
                        TimeTextBlock.Visibility = Visibility.Collapsed;
                        begin.Visibility = Visibility.Collapsed;
                        confirm.Visibility = Visibility.Collapsed;
                        blackrow1.Visibility = Visibility.Collapsed;
                        blackrow2.Visibility = Visibility.Collapsed;
                        blackrow3.Visibility = Visibility.Collapsed;
                        Button_1.IsEnabled = true;
						Button_2.Content = "下一步";

						await OnVoicePlayAsync(Text_3.Text);
                    }
                    break;
                case 3:
                    {
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Text_3.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;
                        Text_4.Visibility = Visibility.Visible;
                        Image_4.Visibility = Visibility.Visible;
                        Text_5.Visibility = Visibility.Collapsed;
                        Image_5.Visibility = Visibility.Collapsed;
                        Text_6.Visibility = Visibility.Collapsed;
                        Image_6.Visibility = Visibility.Collapsed;
                        brownBorder.Visibility = Visibility.Collapsed;
                        imageContainer.Visibility = Visibility.Collapsed;
                        imageContainer2.Visibility = Visibility.Collapsed;
                        imageContainer3.Visibility = Visibility.Collapsed;
                        textBlock.Visibility = Visibility.Collapsed;
                        modeTextBlock.Visibility = Visibility.Collapsed;
                        levelTextBlock.Visibility = Visibility.Collapsed;
                        TimeTextBlock.Visibility = Visibility.Collapsed;
                        begin.Visibility = Visibility.Collapsed;
                        confirm.Visibility = Visibility.Collapsed;
                        blackrow1.Visibility = Visibility.Collapsed;
                        blackrow2.Visibility = Visibility.Collapsed;
                        blackrow3.Visibility = Visibility.Collapsed;
                        Button_1.IsEnabled = true;
                        Button_2.Content = "下一步";
                        await OnVoicePlayAsync(Text_4.Text);

                    }
                    break;
                case 4:
                    {
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Text_3.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;
                        Text_4.Visibility = Visibility.Collapsed;
                        Image_4.Visibility = Visibility.Collapsed;
                        Text_5.Visibility = Visibility.Visible;
                        Image_5.Visibility = Visibility.Visible;
                        Text_6.Visibility = Visibility.Collapsed;
                        Image_6.Visibility = Visibility.Collapsed;

                        brownBorder.Visibility = Visibility.Collapsed;
                        imageContainer.Visibility = Visibility.Collapsed;
                        imageContainer2.Visibility = Visibility.Collapsed;
                        imageContainer3.Visibility = Visibility.Collapsed;
                        textBlock.Visibility = Visibility.Collapsed;
                        modeTextBlock.Visibility = Visibility.Collapsed;
                        levelTextBlock.Visibility = Visibility.Collapsed;
                        TimeTextBlock.Visibility = Visibility.Collapsed;
                        begin.Visibility = Visibility.Collapsed;
                        confirm.Visibility = Visibility.Collapsed;
                        blackrow1.Visibility = Visibility.Collapsed;
                        blackrow2.Visibility = Visibility.Collapsed;
                        blackrow3.Visibility = Visibility.Collapsed;
                        Button_1.IsEnabled = true;
                        Button_2.Content = "下一步";
                        await OnVoicePlayAsync(Text_5.Text);

                    }
                    break;
                case 5:
                    {
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Text_3.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;
                        Text_4.Visibility = Visibility.Collapsed;
                        Image_4.Visibility = Visibility.Collapsed;
                        Text_5.Visibility = Visibility.Collapsed;
                        Image_5.Visibility = Visibility.Collapsed;
                        Text_6.Visibility = Visibility.Visible;
                        Image_6.Visibility = Visibility.Visible;

                        brownBorder.Visibility = Visibility.Collapsed;
                        imageContainer.Visibility = Visibility.Collapsed;
                        imageContainer2.Visibility = Visibility.Collapsed;
                        imageContainer3.Visibility = Visibility.Collapsed;
                        textBlock.Visibility = Visibility.Collapsed;
                        modeTextBlock.Visibility = Visibility.Collapsed;
                        levelTextBlock.Visibility = Visibility.Collapsed;
                        TimeTextBlock.Visibility = Visibility.Collapsed;
                        begin.Visibility = Visibility.Collapsed;
                        confirm.Visibility = Visibility.Collapsed;
                        blackrow1.Visibility = Visibility.Collapsed;
                        blackrow2.Visibility = Visibility.Collapsed;
                        blackrow3.Visibility = Visibility.Collapsed;
                        Button_1.IsEnabled = true;
                        Button_2.Content = "试玩";
                        await OnVoicePlayAsync(Text_6.Text);

                    }
                    break;
                case 6:
                    {
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Text_3.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;
                        Text_4.Visibility = Visibility.Collapsed;
                        Image_4.Visibility = Visibility.Collapsed;
                        Text_5.Visibility = Visibility.Collapsed;
                        Image_5.Visibility = Visibility.Collapsed;
                        Text_6.Visibility = Visibility.Collapsed;
                        Image_6.Visibility = Visibility.Collapsed;
                        brownBorder.Visibility = Visibility.Visible;
                        imageContainer.Visibility = Visibility.Visible;
                        imageContainer2.Visibility = Visibility.Visible;
                        imageContainer3.Visibility = Visibility.Visible;
                        textBlock.Visibility = Visibility.Visible;
                        modeTextBlock.Visibility = Visibility.Visible;
                        levelTextBlock.Visibility = Visibility.Visible;
                        TimeTextBlock.Visibility = Visibility.Collapsed;
                        begin.Visibility = Visibility.Visible;
                        confirm.Visibility = Visibility.Visible;
                        blackrow1.Visibility = Visibility.Visible;
                        blackrow2.Visibility = Visibility.Visible;
                        blackrow3.Visibility = Visibility.Visible;

                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Collapsed;

                        //LJN，在讲解模块调用委托
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("模式一：记住屏幕上显示的纸牌并在触摸屏上选择相同的卡牌。\r\n模式二：在触摸屏上选择指定花色的牌。\r\n模式三：在屏幕上按顺序记住并选择卡牌。\r\n");//增加代码，调用函数，显示数字人下的文字
                        //LJN
                    }
                    break;

            }
        }



        /// <summary>
        /// 讲解内容语音播放
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        async Task VoicePlayer(string message)
        {
            var voicePlayFunc = VoicePlayFunc;
            if (voicePlayFunc == null)
            {
                return;
            }

            await voicePlayFunc.Invoke(message);
        }
    }
}
