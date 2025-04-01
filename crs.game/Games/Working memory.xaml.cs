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
    /// WOME.xaml Interaction logic
    /// </summary>
    public partial class Working memory : BaseUserControl
    {

        // CustomizeAdornerkind
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

                // Create aScaleTransformTo zoom the check mark
                ScaleTransform scaleTransform = new ScaleTransform
                {
                    ScaleX = 5,
                    ScaleY = 5,
                    CenterX = _checkMark.Width / 2,
                    CenterY = _checkMark.Height / 2
                };

                // Apply the zoom transformation to the check mark
                _checkMark.RenderTransform = scaleTransform;

                // Position the check mark onAdornedElementThe upper right corner of
                TranslateTransform translateTransform = new TranslateTransform
                {
                    X = 50,
                    Y = 100
                };

                // Apply the translation transformation to the check mark
                _checkMark.RenderTransform = new TransformGroup
                {
                    Children =
                    {
                        scaleTransform,
                        translateTransform
                    }
                };

                // useDrawingContextdrawPath
                drawingContext.DrawGeometry(
                    Brushes.Transparent, // Check marks do not need to be filled
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

        private int max_time = 1; // Total duration of the window, unit minutes
        private int right_card_number = 2;
        private int total_card_number = 0;
        private int train_mode = 3; // Game mode, 1, 2 or 3
        private bool is_gaming = false;
        private int sucess_time = 0;
        private int fail_time = 0;
        private int level = 1; // Current game difficulty level
        int max_hardness = 1;
        private DispatcherTimer gameTimer;
        private DispatcherTimer displayTimer;
        private DispatcherTimer delayTimer;
        private int remainingTime;
        private int remainingDisplayTime;
        private string targetSuit; // Target color
        private List<string> rightCards; // Store the correct card path
        private List<string> selectedCardsOrder; // The order of cards selected by the player
        private double totalAnswerTime = 0; // Total answer time, unit in seconds
        private int answerCount = 0; // Number of answers
        private DateTime gameStartTime; // Record the time when the game starts

        public int IfVisionFeedBack = 1;//Visual feedback, 1 has 0 has no
        public int IfAudioFeedBack = 1;//Voice feedback, 1 has 0 has no

        //-------------------------------------Report required parameters---------------------------------
        private int type_input = 1;//Input method, default mouse
        private int MaxGames = 10;//Number of tasks
        private int repet_time = 1;//Number of repetitions
        private bool is_repe = false;//Repeat instructions
        private bool is_display = true;//Instruction display -- Visual commands
        private bool distraction = false;//Distraction
        private bool transformation = false;//Transfer
        private bool learn_gain = false;//Acoustic gain 
        private bool is_france = false;//Cards French
        private int card_display_time = 3; // Correct card display time, unit seconds
        private double averageAnswerTime = 0; // Average answer time in seconds
        //-------------------------------------------------------------------------------
        private Queue<bool> recentResults = new Queue<bool>(5); // Queue to record the last 5 game results
        private DispatcherTimer countdowntimer;

        private int[] correctAnswers = new int[70];
        private int[] wrongAnswers = new int[70];
        private int[] ignoreAnswers = new int[70];



        private int LEVEL_UP_THRESHOLD = 85; // Improve the accuracy threshold for difficulty（percentage）
        private int LEVEL_DOWN_THRESHOLD = 70; // Reduce the accuracy threshold for difficulty（percentage）


        #region 10.31 Improvements

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
                    Height = 110,  // Modify the card height
                    Width = 65,   // Modify the card width
                    Fill = new SolidColorBrush(Colors.LightGray),
                    Margin = new Thickness(2),

                };
                imageBackContainer2.Children.Add(img2);
            }
        }

        #endregion


        private void InitializeGameSettings()
        {


            // according tolevelAdjust game settings
            SetLevelSettings();

            remainingTime = max_time * 60; // Convert minutes to seconds

            if (gameTimer != null)
            {
                // If the timer is running, stop it first
                if (gameTimer.IsEnabled)
                {
                    gameTimer.Stop();
                }

                // Cancel previous registration events（Prevent duplicate registration events）
                gameTimer.Tick -= GameTimer_Tick;

                // Will displayTimer Set as null, indicating that it has been cleaned
                gameTimer = null;
            }
            gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            gameTimer.Tick += GameTimer_Tick;
        }
        public Working memory()
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

        public void Speak(string text) // Useless functions
        {
            //Process.Start(new ProcessStartInfo
            //{
            //    FileName = "cmd.exe",
            //    Arguments = $"/c mshta vbscript:Execute(\"CreateObject(\"\"SAPI.SpVoice\"\").Speak(\"\"{text}\"\") (window.close)\")",
            //    CreateNoWindow = true,
            //    UseShellExecute = false
            //});
        }
        // Calling where it needs to be broadcast

        private void beginButton_Click(object sender, RoutedEventArgs e)
        {
            confirm.IsEnabled = true;
            begin.IsEnabled = false;

            //Clear the picture and display the text as default
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
            gameStartTime = DateTime.Now; // Record the time when the game starts

            // set up modeTextBlock Show the current game mode prompts
            if (train_mode == 1)
            {
                if (is_display)
                {
                    modeTextBlock.Text = "Remember the cards and select the same cards";
                }

                if (learn_gain)
                {
                    Speak("Remember the cards and select the same cards");
                }
                DisplayRightCardsMode1();
            }
            else if (train_mode == 2)
            {
                if (is_display)
                {

                    modeTextBlock.Text = "Select the card with the specified suit";

                }
                if (learn_gain)
                {
                    Speak("Select the card with the specified suit");
                }
                DisplaySuitHintAndCards();
            }
            else if (train_mode == 3)
            {
                if (is_display)
                {
                    modeTextBlock.Text = "Remember and select cards in order";
                }

                if (learn_gain)
                {
                    Speak("Remember and select cards in order");
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

                // Create a message to display
                string message = $"Correct Answers: {correctAnswersString}\n" +
                                 $"Wrong Answers: {wrongAnswersString}\n" +
                                 $"Ignore Answers: {ignoreAnswersString}";

                // use MessageBox Show message
                //MessageBox.Show(message, "Answers Information");
                OnGameEnd();
            }
        }



        // Logic of game mode 1
        private void DisplayRightCardsMode1()
        {
            //The display logic of the question rules needs to be displayed separately for different modes, so the delegate call of the display rules is written into different mode display functions.
            RuleAction?.Invoke("Remember the cards displayed on the screen and select the same cards on the touch screen.");//Add code, call function, display the text under the digital person
            VoiceTipAction?.Invoke("Please remember the cards displayed on the screen and select the same cards on the touch screen.");

            // Clear the previous pictures
            imageContainer.Children.Clear();
            imageBackContainer.Children.Clear();
            imageBackContainer.Visibility = Visibility.Hidden;
            imageBackContainer2.Children.Clear();

            rightCards = new List<string>();

            // Using a random number generator
            var random = new Random();

            // Generate different cards randomly each time
            int[] randomIndexes = Enumerable.Range(0, imagePaths[0].Length).OrderBy(x => random.Next()).Take(right_card_number).ToArray();

            SetImageUniformGrid(imageContainer, randomIndexes.Length);
            SetImageUniformGrid(imageBackContainer, randomIndexes.Length);


            // Shows the selected image and saves its path to rightCards List
            foreach (int index in randomIndexes)
            {
                string imagePath = imagePaths[0][index];
                rightCards.Add(imagePath);
                var img = new Image
                {
                    Source = new BitmapImage(new Uri(imagePath, UriKind.Relative)),
                    Height = _cardHeight,  // Modify the card height
                    Width = _cardWidth,   // Modify the card width
                    Margin = new Thickness(2)
                };
                imageContainer.Children.Add(img);

                if (level >= 10)
                {
                    var img2 = new Image
                    {
                        Source = new BitmapImage(new Uri("WOME/Joker_B.jpg", UriKind.Relative)),
                        Height = _cardHeight,  // Modify the card height
                        Width = _cardWidth,   // Modify the card width
                        Margin = new Thickness(2)
                    };
                    imageBackContainer.Children.Add(img2);
                }

            }

            // Set the remaining display time
            remainingDisplayTime = card_display_time;

            // Show remaining display time
            //TimeTextBlock.Text = remainingDisplayTime.ToString();


            // Check if it already exists displayTimer Example
            if (displayTimer != null)
            {
                // If the timer is running, stop it first
                if (displayTimer.IsEnabled)
                {
                    displayTimer.Stop();
                }

                // Cancel previous registration events（Prevent duplicate registration events）
                displayTimer.Tick -= DisplayTimer_TickMode1;

                // Will displayTimer Set as null, indicating that it has been cleaned
                displayTimer = null;
            }

            // Reinitialize displayTimer
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
            // renewTimeTextBlockRemaining display time in
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

            // Add extra random cards to allCards until it reaches total_card_number
            var additionalCards = imagePaths[0].Except(rightCards).OrderBy(x => random.Next()).Take(total_card_number - rightCards.Count).ToList();
            allCards.AddRange(additionalCards);

            // Disrupt the order, make sure it is different from the last time
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
                    Height = _cardHeight,  // Modify the card height
                    Width = _cardWidth,   // Modify the card width
                    Margin = new Thickness(2),
                    Tag = card
                };
                img.MouseLeftButtonUp += CardImage_MouseLeftButtonUpMode1;
                imageContainer3.Children.Add(img);

                var img2 = new Rectangle
                {
                    Height = _cardHeight,  // Modify the card height
                    Width = _cardWidth - 30,   // Modify the card width
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


            // If the image is container2 In, move to container3
            if (imageContainer2.Children.Contains(clickedImage))
            {
                imageContainer2.Children.Remove(clickedImage); // from container2 Delete the image in

                foreach (Image item in imageContainer3.Children)
                {
                    if (item.Tag == clickedImage.Tag)
                    {
                        item.Visibility = Visibility.Visible;
                    }
                }
            }
            // If the image is container3 In, move back container2
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

                imageContainer2.Children.Add(img);    // Add the image to container2
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


            // Check if multiple choices
            if (selectedCards.Count > right_card_number)
            {

                wrongAnswers[level] += 1;
                if (IfAudioFeedBack == 1) PlayWav(ErrorSoundPath);
                if (IfVisionFeedBack == 1) ShowFeedbackImage(ErrorImage);
                UpdateResultDisplay(false);
                EndGame(false);
                return;
            }

            // Check if all are correct
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

        // The logic of game mode 2
        // The logic of game mode 2
        private void DisplaySuitHintAndCards()
        {
            //The display logic of the question rules needs to be displayed separately for different modes, so the delegate call of the display rules is written into different mode display functions.
            RuleAction?.Invoke("Please select the card with the specified suit on the touch screen.");//Add code, call function, display the text under the digital person
            VoiceTipAction?.Invoke("Please select the card with the specified suit on the touch screen.");

            // Tips before clearing
            imageContainer.Children.Clear();
            imageBackContainer.Children.Clear();
            imageBackContainer.Visibility = Visibility.Hidden;
            imageBackContainer2.Children.Clear();

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

            // Use Chinese when displaying color prompts
            suitTextBlock_model2.Text = $"Please select {suitTranslations[targetSuit]} Cards of colors";  // Chinese flower colour
                                                                                      //var textBlock = new TextBlock
                                                                                      //{
                                                                                      //    Text = $"Please select {suitTranslations[targetSuit]} Cards of colors",  // Chinese flower colour
                                                                                      //    FontSize = 24,
                                                                                      //    Foreground = new SolidColorBrush(Colors.Black),
                                                                                      //    HorizontalAlignment = HorizontalAlignment.Center,
                                                                                      //    VerticalAlignment = VerticalAlignment.Center
                                                                                      //};
                                                                                      //imageContainer.Children.Add(textBlock);



            // Set the remaining display time
            remainingDisplayTime = card_display_time;

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

            TimeStatisticsAction.Invoke(remainingTime, remainingDisplayTime);
            // renewTimeTextBlockRemaining display time in
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

            // Guaranteed at least right_card_number A card with a specified color
            var suitCards = imagePaths[0].Where(path => path.Contains(suit)).OrderBy(x => random.Next()).Take(right_card_number).ToList();
            allCards.AddRange(suitCards);

            // Add extra random cards to allCards Make sure these cards are not specified
            var additionalCards = imagePaths[0]
                .Where(path => !path.Contains(suit))  // Exclude target style card
                .OrderBy(x => random.Next())
                .Take(total_card_number - suitCards.Count)
                .ToList();
            allCards.AddRange(additionalCards);

            // Disrupt the order, make sure it is different from the last time
            allCards = allCards.OrderBy(x => random.Next()).ToList();

            SetImageUniformGrid(imageContainer3, allCards.Count);
            SetImageUniformGrid(imageBackContainer3, allCards.Count);

            // Clear and display the card
            imageContainer3.Children.Clear();
            imageBackContainer3.Children.Clear();
            foreach (var card in allCards)
            {
                var img = new Image
                {
                    Source = new BitmapImage(new Uri(card, UriKind.Relative)),
                    Height = _cardHeight,  // Modify the card height
                    Width = _cardWidth,   // Modify the card width
                    Margin = new Thickness(2),
                    Tag = card
                };
                img.MouseLeftButtonUp += CardImage_MouseLeftButtonUpMode2;
                imageContainer3.Children.Add(img);


                var img2 = new Rectangle
                {
                    Height = _cardHeight,  // Modify the card height
                    Width = _cardWidth - 30,   // Modify the card width
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

            // If the image is container2 In, move to container3
            if (imageContainer2.Children.Contains(clickedImage))
            {
                imageContainer2.Children.Remove(clickedImage); // from container2 Delete the image in

                foreach (Image item in imageContainer3.Children)
                {
                    if (item.Tag == clickedImage.Tag)
                    {
                        item.Visibility = Visibility.Visible;
                    }
                }
            }
            // If the image is container3 In, move back container2
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

                imageContainer2.Children.Add(img);    // Add the image to container2
            }
        }

        private void CardImage_MouseLeftButtonUpContainer3(object sender, MouseButtonEventArgs e)
        {
            var clickedImage = sender as Image;
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


            // Check if multiple choices
            if (selectedCards.Count > right_card_number)
            {

                wrongAnswers[level] += 1;
                if (IfAudioFeedBack == 1) PlayWav(ErrorSoundPath);
                if (IfVisionFeedBack == 1) ShowFeedbackImage(ErrorImage);
                UpdateResultDisplay(false);
                EndGame(false);
                return;
            }

            // Check whether all selected cards belong to the target version
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

        // Logic of Game Mode 3
        private void DisplayRightCardsMode3()
        {
            //The display logic of the question rules needs to be displayed separately for different modes, so the delegate call of the display rules is written into different mode display functions.
            RuleAction?.Invoke("Remember and select cards in order on the screen.");//Add code, call function, display the text under the digital person
            VoiceTipAction?.Invoke("Remember and select cards in order on the screen.");
            // Clear the previous pictures
            imageContainer.Children.Clear();
            imageBackContainer.Children.Clear();
            imageBackContainer.Visibility = Visibility.Hidden;
            imageBackContainer2.Children.Clear();

            rightCards = new List<string>();
            selectedCardsOrder = new List<string>();

            // Using a random number generator
            var random = new Random();

            // A different card order is generated randomly each time
            int[] randomIndexes = Enumerable.Range(0, imagePaths[0].Length).OrderBy(x => random.Next()).Take(right_card_number).ToArray();

            SetImageUniformGrid(imageContainer, randomIndexes.Length);
            SetImageUniformGrid(imageBackContainer, randomIndexes.Length);

            // Select in order right_card_number A picture
            foreach (int index in randomIndexes)
            {
                string imagePath = imagePaths[0][index];
                rightCards.Add(imagePath);
                var img = new Image
                {
                    Source = new BitmapImage(new Uri(imagePath, UriKind.Relative)),
                    Height = _cardHeight,  // Modify the card height
                    Width = _cardWidth,   // Modify the card width
                    Margin = new Thickness(2)
                };
                imageContainer.Children.Add(img);

                if (level >= 10)
                {
                    var img2 = new Image
                    {
                        Source = new BitmapImage(new Uri("WOME/Joker_B.jpg", UriKind.Relative)),
                        Height = _cardHeight,  // Modify the card height
                        Width = _cardWidth,   // Modify the card width
                        Margin = new Thickness(2)
                    };
                    imageBackContainer.Children.Add(img2);
                }
            }

            // Set the remaining display time
            remainingDisplayTime = card_display_time;

            // Show remaining display time
            //TimeTextBlock.Text = remainingDisplayTime.ToString();

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
            TimeStatisticsAction.Invoke(remainingTime, remainingDisplayTime);
            // renewTimeTextBlockRemaining display time in
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

            // Add extra random cards to allCards until it reaches total_card_number
            var additionalCards = imagePaths[0].Except(rightCards).OrderBy(x => random.Next()).Take(total_card_number - rightCards.Count).ToList();
            allCards.AddRange(additionalCards);

            // Disrupt the order, make sure it is different from the last time
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
                    Height = _cardHeight,  // Modify the card height
                    Width = _cardWidth,   // Modify the card width
                    Margin = new Thickness(2),
                    Tag = card
                };
                img.MouseLeftButtonUp += CardImage_MouseLeftButtonUpMode3;
                imageContainer3.Children.Add(img);


                var img2 = new Rectangle
                {
                    Height = _cardHeight,  // Modify the card height
                    Width = _cardWidth - 30,   // Modify the card width
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

            // If the image is container2 In, move to container3
            if (imageContainer2.Children.Contains(clickedImage))
            {
                imageContainer2.Children.Remove(clickedImage); // from container2 Delete the image in

                foreach (Image item in imageContainer3.Children)
                {
                    if (item.Tag == clickedImage.Tag)
                    {
                        item.Visibility = Visibility.Visible;
                    }
                }
            }
            // If the image is container3 In, move back container2
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

                imageContainer2.Children.Add(img);    // Add the image to container2
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

                wrongAnswers[level] += 1;
                if (IfAudioFeedBack == 1) PlayWav(ErrorSoundPath);
                if (IfVisionFeedBack == 1) ShowFeedbackImage(ErrorImage);
                UpdateResultDisplay(false);
                EndGame(false);
                return;
            }

            // Check if the order is correct
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
            // Iterate through the queue and count the number of correct and errors
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

        // existImageAdd a red check mark on it ,1 green, 2 red, 3 orange
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

            // Create aPathTo draw the check mark
            System.Windows.Shapes.Path checkMark = new System.Windows.Shapes.Path
            {
                Stroke = solidColorBrush, // Check the color
                StrokeThickness = 5,
                Data = date,
                Width = 50,
                Height = 50,
            };

            // Create aAdornerDecorator

            AdornerLayer layer = AdornerLayer.GetAdornerLayer(image);
            if (layer != null)
            {
                // Create aAdorner
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
                    Text = "success",
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
                    Text = "fail",
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
            // Show current time and game start time
            //MessageBox.Show($"Current Time (DateTime.Now): {DateTime.Now}\nGame Start Time: {gameStartTime}", "Time Information");
            totalAnswerTime += gameTime;
            answerCount++;
            averageAnswerTime = totalAnswerTime / answerCount;
            string message = $"Average Answer Time: {averageAnswerTime} seconds";

            // use MessageBox Show message
            //MessageBox.Show(message, "Average Answer Time");

            //modeTextBlock.Text = string.Empty;
            if (recentResults.Count >= 5)
            {
                recentResults.Dequeue(); // Remove the earliest results
            }
            recentResults.Enqueue(gameCompleted); // Add the current result
            int correctCount = recentResults.Count(result => result);
            int wrongCount = recentResults.Count(result => !result);
            if (gameCompleted)
            {
                // Process the logic after the game is completed, such as displaying the results, etc.
                // Here you can reset the status and prepare for the next game
                if (level < 69)
                {
                    if (correctCount >= repet_time)
                    {
                        level++; // Here, it is assumed that each game will be upgraded after success
                        max_hardness = Math.Max(max_hardness, level);
                    }
                }
                SetLevelSettings(); // Adjust settings
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
                SetLevelSettings(); // Adjust settings
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
                    levelTextBlock.Text = "Village level";
                    break;
                case 2:
                    train_mode = 1;
                    right_card_number = 1;
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



        /* protected override async Task OnStopAsync()
         {
             throw new NotImplementedException();
         }*/

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

        private async void ShowFeedbackImage(Image image)
        {//Image showing feedback
            image.Visibility = Visibility.Visible;

            // Delay the specified time（For example, 1 second）
            await Task.Delay(StopDurations);

            image.Visibility = Visibility.Collapsed;
        }
    }
    public partial class Working memory : BaseUserControl
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


            max_time = 10; // Total duration of the window, unit minutes
            card_display_time = 3; // Correct card display time, unit seconds
            right_card_number = 2;
            total_card_number = 0;
            train_mode = 3; // Game mode, 1, 2 or 3
            level = 1; // Current game difficulty level

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

                                case 57: // Treatment time
                                    max_time = par.Value.HasValue ? (int)par.Value.Value : 30;
                                    Debug.WriteLine($"Treatment time ={max_time}");
                                    break;
                                //case 58: //Correct number of cards
                                //    right_card_number = par.Value.HasValue ? (int)par.Value.Value : 2;
                                //    Debug.WriteLine($"Correct number of cards ={right_card_number}");
                                //    break;
                                //case 59: // Overall number of cards
                                //    total_card_number = par.Value.HasValue ? (int)par.Value.Value : 0;
                                //    Debug.WriteLine($"Overall number of cards ={total_card_number}");
                                //    break;
                                case 60: // Card display time
                                    card_display_time = par.Value.HasValue ? (int)par.Value.Value : 1;
                                    Debug.WriteLine($"Card display time={card_display_time}");
                                    break;
                                //case 63: // Training mode
                                //    train_mode = par.Value.HasValue ? (int)par.Value.Value : 3;
                                //    Debug.WriteLine($"Training mode={train_mode}");
                                    break;
                                case 153: // grade
                                    level = par.Value.HasValue ? (int)par.Value.Value : 1;
                                    Debug.WriteLine($"Current game difficulty ={level}");
                                    break;
                                // case 65: // Time for success
                                //     sucess_time = par.Value.HasValue ? (int)par.Value.Value : 0;
                                //     Debug.WriteLine($"Time for success ={sucess_time}");
                                //     break;
                                //case 66: // Time of failure
                                //    fail_time = par.Value.HasValue ? (int)par.Value.Value : 0;
                                //    Debug.WriteLine($"Time of failure ={fail_time}");
                                //    break;
                                //case 67: // Is it a game
                                //    is_gaming = par.Value == 1;
                                //    Debug.WriteLine($"Is it a game ={is_gaming}");
                                    //break;
                                //case 68: // Level improvement
                                //    LEVEL_UP_THRESHOLD = par.Value.HasValue ? (int)par.Value.Value : 85;
                                //    Debug.WriteLine($"Level improvement ={LEVEL_UP_THRESHOLD}");
                                //    break;
                                //case 69: // Level down
                                //    LEVEL_DOWN_THRESHOLD = par.Value.HasValue ? (int)par.Value.Value : 70;
                                //    Debug.WriteLine($"Level down ={LEVEL_DOWN_THRESHOLD}");
                                //    break;
                                case 259://Visual feedback
                                    IfVisionFeedBack = par.Value.HasValue ? (int)par.Value.Value : 1;
                                    Debug.WriteLine($"Visual feedback ={IfVisionFeedBack}");
                                    break;
                                case 260://Auditory feedback
                                    IfAudioFeedBack = par.Value.HasValue ? (int)par.Value.Value : 1;
                                    Debug.WriteLine($"Auditory feedback ={IfAudioFeedBack}");
                                    break;
                                // Add other things that need to be processed ModuleParId
                                default:
                                    Debug.WriteLine($"Unprocessed ModuleParId: {par.ModuleParId} ");
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

            max_hardness = Math.Max(max_hardness, level);

            gameStartTime = DateTime.Now; // Record the time when the game starts
            InitializeGameSettings();
            // Calling delegate
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
                SynopsisAction?.Invoke("You will see three game modes, the rules are as follows:\r\nMode One: Remember the cards displayed on the screen and select the same cards on the touch screen.\r\nMode 2: Select the card with the specified suit on the touch screen.\r\nMode Three: Remember and select cards in order on the screen.\r\n");

                // set up modeTextBlock Show the current game mode prompts
                if (train_mode == 1)
                {
                    if (is_display)
                    {
                        modeTextBlock.Text = "Remember the cards and select the same cards";
                        //SynopsisAction?.Invoke("Remember the cards displayed on the screen and select the same cards on the touch screen.");
                        //Display logic changes
                        //VoiceTipAction?.Invoke("Remember the cards displayed on the screen and select the same cards on the touch screen.");

                    }

                    if (learn_gain)
                    {
                        VoiceTipAction?.Invoke("Remember the cards displayed on the screen and select the same cards on the touch screen");
                    }
                    DisplayRightCardsMode1();
                }
                else if (train_mode == 2)
                {
                    if (is_display)
                    {
                        modeTextBlock.Text = "Select the card with the specified suit";
                        //SynopsisAction?.Invoke("Please select the card with the specified suit on the touch screen");
                        //Display logic changes
                        //VoiceTipAction?.Invoke("Please select the card with the specified suit on the touch screen");
                    }

                    if (learn_gain)
                    {
                        VoiceTipAction?.Invoke("Please select the card with the specified suit on the touch screen");
                    }
                    DisplaySuitHintAndCards();
                }
                else if (train_mode == 3)
                {
                    if (is_display)
                    {
                        modeTextBlock.Text = "Remember and select cards in order";
                        //SynopsisAction?.Invoke("Remember and select cards in order");
                        //Display logic changes
                        //VoiceTipAction?.Invoke("Please remember and select cards in order on the screen");
                    }

                    if (learn_gain)
                    {
                        VoiceTipAction?.Invoke("Please remember and select cards in order on the screen");
                    }

                    DisplayRightCardsMode3();
                }
                // Calling delegate
            }
            else
            {
                gameTimer.Start();
                is_gaming = true;
                if (displayTimer != null && displayTimer.IsEnabled)
                {
                    displayTimer.Start(); // Stop training timer
                }
            }
        }

        protected override async Task OnStopAsync()
        {
            if (gameTimer != null && gameTimer.IsEnabled)
            {
                // The timer already exists and has stopped
                gameTimer?.Stop(); // Stop the main timer
            }
            if (displayTimer != null && displayTimer.IsEnabled)
            {
                displayTimer?.Stop(); // Stop training timer
            }
        }

        protected override async Task OnPauseAsync()
        {

            gameTimer?.Stop(); // Stop the main timer
            displayTimer?.Stop(); // Stop training timer
        }

        protected override async Task OnNextAsync()
        {
            // Adjust the difficulty
            imageContainer2.Children.Clear();
            imageContainer3.Children.Clear();
            imageContainer.Children.Clear();

            SetLevelSettings();
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
            // Calling delegate
            VoiceTipAction?.Invoke("Test returns voice command information");
            SynopsisAction?.Invoke("Test question description information");
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new Working memory explanation();
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
                        int trainmode = train_mode;
                        int repeat = repet_time;


                        //int correctCount = GetCorrectNum(lv);
                        //int wrongCount = GetWrongNum(lv);
                        //int ignoreCount = GetIgnoreNum(lv);
                        //int trainmode = train_mode;
                        //int repeat = repet_time;
                        //// Calculation accuracy
                        //double average = Math.Round((double)averageAnswerTime, 3);
                        //Debug.WriteLine($"Difficulty level {average}:");
                        //double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);
                        for (int lv = 1; lv <= max_hardness; lv++)
                        {
                            // Get data at the current difficulty level
                            correctCount += GetCorrectNum(lv);
                            wrongCount += GetWrongNum(lv);
                            ignoreCount += GetIgnoreNum(lv);
                        }

                        // Calculation accuracy
                        double average = Math.Round((double)averageAnswerTime, 3);
                        double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);


                        // create Result Record
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "Working memory",
                            Eval = false,
                            Lv = max_hardness, // Current difficulty level
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
                                    ValueName = "grade",
                                    Value = max_hardness,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                //new ResultDetail
                                //{
                                //    ResultId = result_id,
                                //    ValueName = "Task display",
                                //    Value = trainmode,
                                //    ModuleId = BaseParameter.ModuleId
                                //},
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "correct",
                                    Value = correctCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "mistake",
                                    Value = wrongCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "repeat",
                                    Value = repeat,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Average solution time（ms）",
                                    Value = average * 1000, // Stored as a percentage
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
