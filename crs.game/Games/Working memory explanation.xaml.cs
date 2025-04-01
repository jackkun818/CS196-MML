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
    /// Working memory explanation.xaml Interaction logic
    /// </summary>
    public partial class Working memory explanation : BaseUserControl
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
        private int max_time = 1; // Total duration of the window, unit minutes
        private int card_display_time = 3; // Correct card display time, unit seconds
        private int right_card_number = 2;
        private int total_card_number = 0;
        private int train_mode = 1; // Game mode, 1, 2 or 3
        private bool is_gaming = false;
        private int sucess_time = 0;
        private int fail_time = 0;
        private int level = 1; // Current game difficulty level
        private DispatcherTimer gameTimer;
        private DispatcherTimer displayTimer;
        private DispatcherTimer delayTimer;
        private int remainingTime;
        private int remainingDisplayTime;
        private string targetSuit; // Target color
        private List<string> rightCards; // Store the correct card path
        private List<string> selectedCardsOrder; // The order of cards selected by the player
        private const int MaxGames = 10;


        private int[] correctAnswers = new int[MaxGames];
        private int[] wrongAnswers = new int[MaxGames];
        private int[] ignoreAnswers = new int[MaxGames];


        private int LEVEL_UP_THRESHOLD = 85; // Improve the accuracy threshold for difficulty（percentage）
        private int LEVEL_DOWN_THRESHOLD = 70; // Reduce the accuracy threshold for difficulty（percentage）
        public Action GameBeginAction { get; set; }

        public Func<string, Task> VoicePlayFunc { get; set; }

        public Working memory explanation()
        {
            InitializeComponent();
            InitializeGameSettings();

            // Started when the window is loadedmax_timeTimer
            gameTimer.Start();

            this.Loaded += Working memory explanation_Loaded;


        }

        private void Working memory explanation_Loaded(object sender, RoutedEventArgs e)
        {
            // Ensure key presses and focus behavior when page loads
            Button_2_Click(null, null);
            this.Focus();  // Make sure the focus is on the window
        }

        private void InitializeGameSettings()
        {
            // according tolevelAdjust game settings
            //
            //SetLevelSettings();

            remainingTime = max_time * 60; // Convert minutes to seconds
            gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            gameTimer.Tick += GameTimer_Tick;
        }



        private void beginButton_Click(object sender, RoutedEventArgs e)
        {
            //Clear the picture and display the text as default
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

            // set up modeTextBlock Show the current game mode prompts
            if (train_mode == 1)
            {
                modeTextBlock.Text = "Remember the cards and select the same cards";
                DisplayRightCardsMode1();
            }
            else if (train_mode == 2)
            {
                modeTextBlock.Text = "Select the card with the specified suit";
                DisplaySuitHintAndCards();
            }
            else if (train_mode == 3)
            {
                modeTextBlock.Text = "Remember and select cards in order";
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

        // Logic of game mode 1
        private void DisplayRightCardsMode1()
        {
            // Clear the previous pictures
            imageContainer.Children.Clear();
            rightCards = new List<string>();

            // Using a random number generator
            var random = new Random();

            // Generate different cards randomly each time
            int[] randomIndexes = Enumerable.Range(0, imagePaths[0].Length).OrderBy(x => random.Next()).Take(right_card_number).ToArray();

            // Shows the selected image and saves its path to rightCards List
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

            // Set the remaining display time
            remainingDisplayTime = card_display_time;

            // Show remaining display time
            TimeTextBlock.Text = remainingDisplayTime.ToString();

            // Create and start the image display timer
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

            // renewTimeTextBlockRemaining display time in
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

            // Add extra random cards to allCards until it reaches total_card_number
            var additionalCards = imagePaths[0].Except(rightCards).OrderBy(x => random.Next()).Take(total_card_number - rightCards.Count).ToList();
            allCards.AddRange(additionalCards);

            // Disrupt the order, make sure it is different from the last time
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

            // If the image is container2 In, move to container3
            if (imageContainer2.Children.Contains(clickedImage))
            {
                imageContainer2.Children.Remove(clickedImage); // from container2 Delete the image in
                imageContainer3.Children.Add(clickedImage);    // Add an image to container3
            }
            // If the image is container3 In, move back container2
            else if (imageContainer3.Children.Contains(clickedImage))
            {
                imageContainer3.Children.Remove(clickedImage); // from container3 Delete the image in
                imageContainer2.Children.Add(clickedImage);    // Add the image to container2
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

            // Check if multiple choices
            if (selectedCards.Count > right_card_number)
            {
                //MessageBox.Show("You have selected too many cards, and you will not get points if you choose multiple cards！");
                fail_time++;
                UpdateResultDisplay(false);
                EndGame(false);
                return;
            }

            // Check if all are correct
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

        // The logic of game mode 2
        private void DisplaySuitHintAndCards()
        {
            // Tips before clearing
            imageContainer.Children.Clear();

            // Define English styles and their corresponding Chinese
            var suitTranslations = new Dictionary<string, string>
    {
        { "Spades", "Black peach" },
        { "Hearts", "Red Heart" },
        { "Diamonds", "Squares" },
        { "Clubs", "plum bossom" }
    };

            // Randomly select a color
            var suits = new string[] { "Spades", "Hearts", "Diamonds", "Clubs" };
            var random = new Random();
            var englishSuit = suits[random.Next(suits.Length)];  // Choose a random English color
            targetSuit = englishSuit;  // Keep English styles for card filtering

            suitTextBlock_model2.Text = $"Please select {suitTranslations[targetSuit]} Cards of colors";  // Chinese flower colour
            // Show color prompts
            //var textBlock = new TextBlock
            //{
            //    Text = $"Please select {targetSuit} Cards of colors",
            //    FontSize = 36,
            //    Foreground = new SolidColorBrush(Colors.Black),
            //    HorizontalAlignment = HorizontalAlignment.Center,
            //    VerticalAlignment = VerticalAlignment.Center
            //};
            //imageContainer.Children.Add(textBlock);

            // Set the remaining display time
            remainingDisplayTime = card_display_time;

            // Show remaining display time
            TimeTextBlock.Text = remainingDisplayTime.ToString();

            // Create and start prompt display timer
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

            // renewTimeTextBlockRemaining display time in
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

            // Guaranteed at leastright_card_numberA card with a specified color
            var suitCards = imagePaths[0].Where(path => path.Contains(suit)).OrderBy(x => random.Next()).Take(right_card_number).ToList();
            allCards.AddRange(suitCards);

            // Add extra random cards to allCards until it reaches total_card_number
            var additionalCards = imagePaths[0].Except(suitCards).OrderBy(x => random.Next()).Take(total_card_number - suitCards.Count).ToList();
            allCards.AddRange(additionalCards);

            // Disrupt the order, make sure it is different from the last time
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

            // If the image is container2 In, move to container3
            if (imageContainer2.Children.Contains(clickedImage))
            {
                imageContainer2.Children.Remove(clickedImage); // from container2 Delete the image in
                imageContainer3.Children.Add(clickedImage);    // Add an image to container3
            }
            // If the image is container3 In, move back container2
            else if (imageContainer3.Children.Contains(clickedImage))
            {
                imageContainer3.Children.Remove(clickedImage); // from container3 Delete the image in
                imageContainer2.Children.Add(clickedImage);    // Add the image to container2
            }
        }

        private void CardImage_MouseLeftButtonUpContainer3(object sender, MouseButtonEventArgs e)
        {
            var clickedImage = sender as System.Windows.Controls.Image;
            if (clickedImage == null) return;

            // Check if the clicked image is container3 middle
            if (imageContainer3.Children.Contains(clickedImage))
            {
                imageContainer3.Children.Remove(clickedImage); // from container3 Delete the image in
                imageContainer2.Children.Add(clickedImage);    // Add the image to container2

                // Modify the click event to handle container2 Logic
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

            // Check if multiple choices
            if (selectedCards.Count > right_card_number)
            {
                //essageBox.Show("You have selected too many cards, and you will not get points if you choose multiple cards！");
                fail_time++;
                UpdateResultDisplay(false);
                EndGame(false);
                return;
            }

            // Check whether all selected cards belong to the target version
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

        // Logic of Game Mode 3
        private void DisplayRightCardsMode3()
        {
            // Clear the previous pictures
            imageContainer.Children.Clear();
            rightCards = new List<string>();
            selectedCardsOrder = new List<string>();

            // Using a random number generator
            var random = new Random();

            // A different card order is generated randomly each time
            int[] randomIndexes = Enumerable.Range(0, imagePaths[0].Length).OrderBy(x => random.Next()).Take(right_card_number).ToArray();

            // Select in order right_card_number A picture
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

            // Set the remaining display time
            remainingDisplayTime = card_display_time;

            // Show remaining display time
            TimeTextBlock.Text = remainingDisplayTime.ToString();

            // Create and start the image display timer
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

            // renewTimeTextBlockRemaining display time in
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

            // Add extra random cards to allCards until it reaches total_card_number
            var additionalCards = imagePaths[0].Except(rightCards).OrderBy(x => random.Next()).Take(total_card_number - rightCards.Count).ToList();
            allCards.AddRange(additionalCards);

            // Disrupt the order, make sure it is different from the last time
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

            // If the image is container2 In, move to container3
            if (imageContainer2.Children.Contains(clickedImage))
            {
                imageContainer2.Children.Remove(clickedImage); // from container2 Delete the image in
                imageContainer3.Children.Add(clickedImage);    // Add an image to container3
            }
            // If the image is container3 In, move back container2
            else if (imageContainer3.Children.Contains(clickedImage))
            {
                imageContainer3.Children.Remove(clickedImage); // from container3 Delete the image in
                imageContainer2.Children.Add(clickedImage);    // Add the image to container2
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

            // Check if multiple choices
            if (selectedCards.Count > right_card_number)
            {
                //MessageBox.Show("You have selected too many cards, and you will not get points if you choose multiple cards！");
                fail_time++;
                UpdateResultDisplay(false);
                EndGame(false);
                return;
            }

            // Check if the order is correct
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
                //MessageBox.Show("Please wait for the card display countdown to be completed before confirming！");
                return;
            }

            if (!is_gaming)
            {
                return;
            }

            if (false)
            {
                MessageBox.Show("Please select the specified number of cards first！");
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
                    Text = "Congratulations on getting right",
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
                    Text = "Sorry to answer wrong",
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
                    levelTextBlock.Text = "Village level";
                    break;
                case 2:
                    train_mode = 1;
                    right_card_number = 3;
                    total_card_number = 5;
                    levelTextBlock.Text = "Village level";
                    break;
                case 3:
                    train_mode = 1;
                    right_card_number = 2;
                    total_card_number = 4;
                    levelTextBlock.Text = "Village level";
                    break;
                case 4:
                    train_mode = 1;
                    right_card_number = 3;
                    total_card_number = 5;
                    levelTextBlock.Text = "Village level";
                    break;
                case 5:
                    train_mode = 1;
                    right_card_number = 3;
                    total_card_number = 5;
                    levelTextBlock.Text = "Village level";
                    break;
                case 6:
                    train_mode = 1;
                    right_card_number = 3;
                    total_card_number = 6;
                    levelTextBlock.Text = "Village level";
                    break;
                case 7:
                    train_mode = 2;
                    right_card_number = 3;
                    total_card_number = 6;
                    levelTextBlock.Text = "Village level";
                    break;
                case 8:
                    train_mode = 2;
                    right_card_number = 3;
                    total_card_number = 7;
                    levelTextBlock.Text = "Village level";
                    break;
                case 9:
                    train_mode = 2;
                    right_card_number = 3;
                    total_card_number = 8;
                    levelTextBlock.Text = "Village level";
                    break;
                case 10:
                    train_mode = 2;
                    right_card_number = 3;
                    total_card_number = 5;
                    levelTextBlock.Text = "Town level";
                    break;
                case 11:
                    train_mode = 2;
                    right_card_number = 3;
                    total_card_number = 6;
                    levelTextBlock.Text = "Town level";
                    break;
                case 12:
                    train_mode = 2;
                    right_card_number = 3;
                    total_card_number = 7;
                    levelTextBlock.Text = "Town level";
                    break;
                case 13:
                    train_mode = 2;
                    right_card_number = 3;
                    total_card_number = 8;
                    levelTextBlock.Text = "Town level";
                    break;
                case 14:
                    train_mode = 2;
                    right_card_number = 3;
                    total_card_number = 9;
                    levelTextBlock.Text = "Town level";
                    break;
                case 15:
                    train_mode = 3;
                    right_card_number = 5;
                    total_card_number = 5;
                    levelTextBlock.Text = "Town level";
                    break;
                case 16:
                    train_mode = 3;
                    right_card_number = 4;
                    total_card_number = 4;
                    levelTextBlock.Text = "Town level";
                    break;
                case 17:
                    train_mode = 3;
                    right_card_number = 4;
                    total_card_number = 4;
                    levelTextBlock.Text = "Town level";
                    break;
                case 18:
                    train_mode = 1;
                    right_card_number = 4;
                    total_card_number = 5;
                    levelTextBlock.Text = "Regional level";
                    break;
                case 19:
                    train_mode = 1;
                    right_card_number = 4;
                    total_card_number = 6;
                    levelTextBlock.Text = "Regional level";
                    break;
                case 20:
                    train_mode = 1;
                    right_card_number = 4;
                    total_card_number = 7;
                    levelTextBlock.Text = "Regional level";
                    break;
                case 21:
                    train_mode = 1;
                    right_card_number = 4;
                    total_card_number = 8;
                    levelTextBlock.Text = "Regional level";
                    break;
                case 22:
                    train_mode = 1;
                    right_card_number = 4;
                    total_card_number = 9;
                    levelTextBlock.Text = "Regional level";
                    break;
                case 23:
                    train_mode = 2;
                    right_card_number = 4;
                    total_card_number = 7;
                    levelTextBlock.Text = "Regional level";
                    break;
                case 24:
                    train_mode = 2;
                    right_card_number = 4;
                    total_card_number = 8;
                    levelTextBlock.Text = "National level";
                    break;
                case 25:
                    train_mode = 2;
                    right_card_number = 4;
                    total_card_number = 9;
                    levelTextBlock.Text = "National level";
                    break;
                case 26:
                    train_mode = 3;
                    right_card_number = 5;
                    total_card_number = 5;
                    levelTextBlock.Text = "National level";
                    break;
                case 27:
                    train_mode = 3;
                    right_card_number = 4;
                    total_card_number = 4;
                    levelTextBlock.Text = "National level";
                    break;
                case 28:
                    train_mode = 1;
                    right_card_number = 5;
                    total_card_number = 6;
                    levelTextBlock.Text = "National level";
                    break;
                case 29:
                    train_mode = 1;
                    right_card_number = 5;
                    total_card_number = 7;
                    levelTextBlock.Text = "National level";
                    break;
                case 30:
                    train_mode = 1;
                    right_card_number = 5;
                    total_card_number = 8;
                    levelTextBlock.Text = "National level";
                    break;
                case 31:
                    train_mode = 1;
                    right_card_number = 5;
                    total_card_number = 9;
                    levelTextBlock.Text = "National level";
                    break;
                case 32:
                    train_mode = 1;
                    right_card_number = 5;
                    total_card_number = 10;
                    levelTextBlock.Text = "National level";
                    break;
                case 33:
                    train_mode = 1;
                    right_card_number = 5;
                    total_card_number = 11;
                    levelTextBlock.Text = "National level";
                    break;
                case 34:
                    train_mode = 2;
                    right_card_number = 5;
                    total_card_number = 8;
                    levelTextBlock.Text = "European level";
                    break;
                case 35:
                    train_mode = 2;
                    right_card_number = 5;
                    total_card_number = 9;
                    levelTextBlock.Text = "European level";
                    break;
                case 36:
                    train_mode = 2;
                    right_card_number = 5;
                    total_card_number = 10;
                    levelTextBlock.Text = "European level";
                    break;
                case 37:
                    train_mode = 2;
                    right_card_number = 5;
                    total_card_number = 11;
                    levelTextBlock.Text = "European level";
                    break;
                case 38:
                    train_mode = 3;
                    right_card_number = 8;
                    total_card_number = 8;
                    levelTextBlock.Text = "European level";
                    break;
                case 39:
                    train_mode = 3;
                    right_card_number = 7;
                    total_card_number = 7;
                    levelTextBlock.Text = "European level";
                    break;
                case 40:
                    train_mode = 1;
                    right_card_number = 6;
                    total_card_number = 9;
                    levelTextBlock.Text = "European level";
                    break;
                case 41:
                    train_mode = 1;
                    right_card_number = 6;
                    total_card_number = 10;
                    levelTextBlock.Text = "European level";
                    break;
                case 42:
                    train_mode = 1;
                    right_card_number = 6;
                    total_card_number = 11;
                    levelTextBlock.Text = "World-class";
                    break;
                case 43:
                    train_mode = 1;
                    right_card_number = 6;
                    total_card_number = 12;
                    levelTextBlock.Text = "World-class";
                    break;
                case 44:
                    train_mode = 2;
                    right_card_number = 6;
                    total_card_number = 9;
                    levelTextBlock.Text = "World-class";
                    break;
                case 45:
                    train_mode = 2;
                    right_card_number = 6;
                    total_card_number = 10;
                    levelTextBlock.Text = "World-class";
                    break;
                case 46:
                    train_mode = 2;
                    right_card_number = 6;
                    total_card_number = 11;
                    levelTextBlock.Text = "World-class";
                    break;
                case 47:
                    train_mode = 3;
                    right_card_number = 9;
                    total_card_number = 9;
                    levelTextBlock.Text = "World-class";
                    break;
                case 48:
                    train_mode = 3;
                    right_card_number = 7;
                    total_card_number = 7;
                    levelTextBlock.Text = "World-class";
                    break;
                case 49:
                    train_mode = 3;
                    right_card_number = 7;
                    total_card_number = 7;
                    levelTextBlock.Text = "World-class";
                    break;
                case 50:
                    train_mode = 1;
                    right_card_number = 7;
                    total_card_number = 10;
                    levelTextBlock.Text = "World-class";
                    break;
                case 51:
                    train_mode = 1;
                    right_card_number = 7;
                    total_card_number = 11;
                    levelTextBlock.Text = "World-class";
                    break;
                case 52:
                    train_mode = 1;
                    right_card_number = 7;
                    total_card_number = 12;
                    levelTextBlock.Text = "World-class";
                    break;
                case 53:
                    train_mode = 1;
                    right_card_number = 7;
                    total_card_number = 13;
                    levelTextBlock.Text = "World-class";
                    break;
                case 54:
                    train_mode = 1;
                    right_card_number = 7;
                    total_card_number = 14;
                    levelTextBlock.Text = "World-class";
                    break;
                case 55:
                    train_mode = 2;
                    right_card_number = 7;
                    total_card_number = 9;
                    levelTextBlock.Text = "World-class";
                    break;
                case 56:
                    train_mode = 2;
                    right_card_number = 7;
                    total_card_number = 10;
                    levelTextBlock.Text = "World-class";
                    break;
                case 57:
                    train_mode = 2;
                    right_card_number = 7;
                    total_card_number = 11;
                    levelTextBlock.Text = "World-class";
                    break;
                case 58:
                    train_mode = 3;
                    right_card_number = 8;
                    total_card_number = 8;
                    levelTextBlock.Text = "World-class";
                    break;
                case 59:
                    train_mode = 3;
                    right_card_number = 8;
                    total_card_number = 8;
                    levelTextBlock.Text = "World-class";
                    break;
                case 60:
                    train_mode = 1;
                    right_card_number = 8;
                    total_card_number = 11;
                    levelTextBlock.Text = "World-class";
                    break;
                case 61:
                    train_mode = 1;
                    right_card_number = 8;
                    total_card_number = 12;
                    levelTextBlock.Text = "World-class";
                    break;
                case 62:
                    train_mode = 1;
                    right_card_number = 8;
                    total_card_number = 13;
                    levelTextBlock.Text = "World-class";
                    break;
                case 63:
                    train_mode = 1;
                    right_card_number = 8;
                    total_card_number = 14;
                    levelTextBlock.Text = "World-class";
                    break;
                case 64:
                    train_mode = 2;
                    right_card_number = 8;
                    total_card_number = 9;
                    levelTextBlock.Text = "World-class";
                    break;
                case 65:
                    train_mode = 2;
                    right_card_number = 8;
                    total_card_number = 10;
                    levelTextBlock.Text = "World-class";
                    break;
                case 66:
                    train_mode = 2;
                    right_card_number = 8;
                    total_card_number = 11;
                    levelTextBlock.Text = "World-class";
                    break;
                case 67:
                    train_mode = 3;
                    right_card_number = 9;
                    total_card_number = 9;
                    levelTextBlock.Text = "World-class";
                    break;
                case 68:
                    train_mode = 3;
                    right_card_number = 9;
                    total_card_number = 9;
                    levelTextBlock.Text = "World-class";
                    break;
                case 69:
                    train_mode = 3;
                    right_card_number = 9;
                    total_card_number = 9;
                    levelTextBlock.Text = "World-class";
                    break;
            }
        }

        private void end_Click(object sender, RoutedEventArgs e)
        {
            // Related logic for starting answering questions
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
                        // Display the first interface of explanation

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
                        Button_2.Content = "Next step";
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
                        Button_2.Content = "Next step";
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
						Button_2.Content = "Next step";

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
                        Button_2.Content = "Next step";
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
                        Button_2.Content = "Next step";
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
                        Button_2.Content = "Trial";
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

                        //LJN, instructing the delegate in the explanation module
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("Mode One: Remember the cards displayed on the screen and select the same cards on the touch screen.\r\nMode 2: Select the card with the specified suit on the touch screen.\r\nMode Three: Remember and select cards in order on the screen.\r\n");//Add code, call function, display the text under the digital person
                        //LJN
                    }
                    break;

            }
        }



        /// <summary>
        /// Voice playback of explanation content
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
