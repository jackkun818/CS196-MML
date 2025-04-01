using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;
namespace crs.game.Games
{
    /// <summary>
    /// Explanation of memory breadth.xaml Interaction logic
    /// </summary>
    public partial class Explanation of memory breadth : BaseUserControl
    {
        private bool istrue;
        private const int GridSize = 5;
        private const double DELAY = 1.0; // Time interval between two adjacent blocks
        private const int MAX_BLOCKS = 3; // Set the maximum number of blocks to 3
        private List<Button> buttons = new List<Button>();
        private List<int> sequence = new List<int>();
        private List<int> selectedIndices = new List<int>(); // Record the index of the block selected by the player
        private Stopwatch stopwatch = new Stopwatch();
        private DispatcherTimer countdownTimer; // Countdown timer
        private List<DispatcherTimer> sequenceTimers = new List<DispatcherTimer>(); // Timer to store display blocks
        private bool isShowingSequence; // Are blocks being displayed
        private DispatcherTimer gameTimer; // Timer



        public Explanation of memory breadth()
        {
            InitializeComponent();
            InitializeGrid();

            this.Loaded += Explanation of memory breadth_Loaded;
        }

        private void Explanation of memory breadth_Loaded(object sender, RoutedEventArgs e)
        {
            Button_2_Click(null, null);
        }

        private void InitializeGrid()
        {
            // Get the current working directory
            string currentDirectory = Directory.GetCurrentDirectory();
            string imageFolderPath = System.IO.Path.Combine(currentDirectory, "../../../crs.game/Games/pic/JIYI");
            BitmapImage image1 = new BitmapImage(new Uri(imageFolderPath + "1.jpg", UriKind.Relative));
            BitmapImage image2 = new BitmapImage(new Uri(imageFolderPath + "2.jpg", UriKind.Relative));
            BitmapImage image3 = new BitmapImage(new Uri(imageFolderPath + "3.jpg", UriKind.Relative));
            Image_1.Source = image1;
            Image_2.Source = image2;
            Image_3.Source = image3;

            GameGrid.Children.Clear();


            buttons.Clear(); // Clear the button list at the same time

            // Dynamically calculate the height of the button
            double rowHeight = GameGrid.Height / GameGrid.RowDefinitions.Count;

            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    Button button = new Button
                    {
                        //LJN,Modify the color
                        Background = Brushes.White, // Set the initial background color to gray
                        //LJN,Modify spacing 2->5
                        Margin = new Thickness(5),
                        FontSize = 24, // Set the default font size
                        Content = "",

                        //LJN,Modify the height and letbuttonFull of thatgridCell
                        //Width = 100,
                        Height = rowHeight,
                        //LJN,Apply custom styles
                        Style= CreateCustomButtonStyle(), // Apply custom styles
                        //LJN,The button is not clickable when creating it, and you can't click until you click the trial button.
                        IsEnabled = false // Set the button to be clickable
                    };

                    button.Click += Button_Click;
                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);
                    GameGrid.Children.Add(button);
                    buttons.Add(button);
                }
            }
        }
        private void Window_MouseMove(object sender, MouseEventArgs e)//Bind custom mouse cursor and default mouse cursor
        {
         
            Point position = e.GetPosition(this);
            Canvas.SetLeft(CustomCursor, position.X - (CustomCursor.Width / 2));
            Canvas.SetTop(CustomCursor, position.Y - (CustomCursor.Height / 2));
        }
        private void OnGameStart()//Call custom mouse cursor function
        {
            this.Cursor = Cursors.None; // Hide the default cursor
            CustomCursor.Visibility = Visibility.Visible; // Show custom cursor
            MouseMove += Window_MouseMove; // subscription MouseMove event
            CustomCursor.Width = 65; // Adjust to the width you want
            CustomCursor.Height = 65; // Adjust to the height you want
        }
        private void StartGame()
        {
            StatusTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            sequence.Clear(); // Clear the displayed block sequence
            selectedIndices.Clear(); // Clear the index of the selected blocks by the player
            isShowingSequence = true;
            istrue = true; // Reset the correct flag
            StatusTextBlock.Text = "Please remember the order in which the red squares light up and press the corresponding squares in the order they are displayed after displaying all red squares."; // Set up text to start
            StartInitialCountdown(); // Start the initial countdown
            //LJN,Used to display pictures
            CreateImage();
            OnGameStart();
        }

        private void ShowNextRound()
        {
            int numberToShow = 3; // Number of blocks displayed in each round
            Random rand = new Random();
            HashSet<int> shownIndices = new HashSet<int>();
            for (int i = 0; i < numberToShow; i++)
            {
                int index;
                do
                {
                    index = rand.Next(buttons.Count);
                } while (shownIndices.Contains(index));

                shownIndices.Add(index);
                sequence.Add(index);
            }
            stopwatch.Restart();
            ShowSequence(0);
        }

        private void StartInitialCountdown()
        {
            countdownTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            int countdownTime = 5; // 5 seconds countdown
            countdownTimer.Tick += (s, args) =>
            {
                if (countdownTime > 0)
                {
                    countdownTime--;
                }
                else
                {
                    countdownTimer.Stop();
                    StatusTextBlock.Text = "Now start displaying the blocks.";
                    ShowNextRound(); // The blocks begin to be displayed after the countdown is over
                }
            };
            countdownTimer.Start();
        }

        private void ShowSequence(int index)
        {
            if (index < sequence.Count)
            {
                int buttonIndex = sequence[index];
                buttons[buttonIndex].Background = Brushes.Red;

                DispatcherTimer timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(DELAY)
                };
                timer.Tick += (s, args) =>
                {
                    //LJN,Modify the color towhite
                    buttons[buttonIndex].Background = Brushes.White; // Hide blocks
                    timer.Stop();
                    sequenceTimers.Remove(timer); // Remove the timer
                    ShowSequence(index + 1); // Show the next block
                };
                timer.Start();
                sequenceTimers.Add(timer); // Add to list
            }
            else
            {
                // All blocks are displayed and prompt the user
                isShowingSequence = false;
                StatusTextBlock.Text = "Now please press the corresponding blocks in turn";

                //LJN,The button can be enabled at this time and can be clicked
                foreach (var button in buttons)
                {
                    button.IsEnabled = true; // Make the button clickable
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            int clickedIndex = buttons.IndexOf(clickedButton);

            // Logic is processed only after the display is finished
            if (!isShowingSequence)
            {
                // Check whether players click on blocks in order
                if (selectedIndices.Count < sequence.Count)
                {
                    if (clickedIndex != sequence[selectedIndices.Count])
                    {
                        istrue = false;
                    }

                    if (selectedIndices.Contains(clickedIndex))
                    {//If this has been selectedbutton, thenbuttonBack to its original state
                        selectedIndices.Remove(clickedIndex);
                        //LJN,Restore to white
                        clickedButton.Background = Brushes.White; // Revert to gray
                        clickedButton.Content = ""; // Clear content
                        istrue = true;
                    }
                    else
                    {
                        selectedIndices.Add(clickedIndex);
                        //LJN,Modify the color after clicking to orange
                        clickedButton.Background = Brushes.Orange;
                        clickedButton.Content = (selectedIndices.Count).ToString(); // Show the order of the current clicks
                        clickedButton.Foreground = Brushes.White; // Set the font color to white
                        clickedButton.FontSize = 36; // Increase font size
                    }
                }

                // Determine whether all blocks have been clicked correctly
                if (selectedIndices.Count == sequence.Count)
                {
                    foreach (var index in selectedIndices)
                    {
                        if (sequence.Contains(index))
                        {
                            buttons[index].Background = Brushes.Green; // Correct options
                            buttons[index].Content = "✔"; // Show correct mark
                        }
                        else
                        {
                            buttons[index].Background = Brushes.Red; // Wrong option
                            buttons[index].Content = "✖"; // Show error mark
                        }
                        //LJN,Prevent conflicts, disable the button and make it clickable only when appropriate
                        foreach (var button in buttons)
                        {
                            button.IsEnabled = false; // Make the button clickable
                        }
                    }
                    if (istrue)
                    {
                        StatusTextBlock1.FontSize = 40;
                        StatusTextBlock1.Text = "      Congratulations on getting right！";
                        StatusTextBlock1.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#06CE77"));
                        isShowingSequence = true;
                        OkButton.Visibility = Visibility.Visible;
                        //LJN,Show pictures,Hide text
                        CorrectImage.Visibility = Visibility.Visible;
                        StatusTextBlock.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        StatusTextBlock.FontSize = 40;
                        StatusTextBlock1.Text = "      Sorry to answer wrong！";
                        StatusTextBlock1.Foreground = new SolidColorBrush(Colors.Red);
                        OkButton.Visibility = Visibility.Collapsed;
                        //LJN,Show pictures,Hide text
                        ErrorImage.Visibility = Visibility.Visible;
                        StatusTextBlock.Visibility = Visibility.Collapsed;

                        DispatcherTimer waitTimer = new DispatcherTimer
                        {
                            Interval = TimeSpan.FromSeconds(3) // Set the waiting time to 3 Second
                        };
                        waitTimer.Tick += (s, args) =>
                        {
                            waitTimer.Stop(); // Stop the timer
                            InitializeGrid(); // Initialize the grid
                            //LJN,Clear the text prompts
                            StatusTextBlock1.Text = "";
                            //LJN,Hide the image feedback and display the prompt text
                            ErrorImage.Visibility = Visibility.Collapsed;
                            CorrectImage.Visibility = Visibility.Collapsed;
                            StatusTextBlock.Visibility = Visibility.Visible;
                            StartGame(); // Start the game
                        };
                        waitTimer.Start(); // Start the timer
                    }
                }
            }
        }

        private void StopAllTimers()
        {
            // Stop countdown timer
            countdownTimer?.Stop();

            // Stop the timer for all display blocks
            foreach (var timer in sequenceTimers)
            {
                timer.Stop();
            }
           
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            //Add logic related to start answering questions
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
                        Text_3.Visibility = Visibility.Hidden;
                        Image_3.Visibility = Visibility.Hidden;
                       
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Content = "Next step";
                        Button_2.Margin = new Thickness(329, 850, 0, 0);
                        Button_3.Margin = new Thickness(770, 850, 0, 0);


                        await OnVoicePlayAsync(Text_1.Text);
                    }
                    break;
                case 1:
                    {
                        Text_1.Visibility = Visibility.Hidden;
                        Image_1.Visibility = Visibility.Hidden;
                        Text_2.Visibility = Visibility.Visible;
                        Image_2.Visibility = Visibility.Visible;
                        Text_3.Visibility = Visibility.Hidden;
                        Image_3.Visibility = Visibility.Hidden;
                       
                        Button_1.Visibility = Visibility.Visible;
                        Button_1.IsEnabled = true;
                        Button_2.Content = "Next step";
                        Button_2.Margin = new Thickness(550, 850, 0, 0);
                        Button_3.Margin = new Thickness(911, 850, 0, 0);

                        await OnVoicePlayAsync(Text_2.Text);
                    }
                    break;
                case 2:
                    {
                        Text_1.Visibility = Visibility.Hidden;
                        Image_1.Visibility = Visibility.Hidden;
                        Text_2.Visibility = Visibility.Hidden;
                        Image_2.Visibility = Visibility.Hidden;
                        Text_3.Visibility = Visibility.Visible;
                        Image_3.Visibility = Visibility.Visible;
                       
                        Button_2.Content = "Trial";
                        MouseMove += Window_MouseMove; // subscription MouseMove event
                        await OnVoicePlayAsync(Text_3.Text);
                    }
                    break;
                case 3:
                    {
                        Text_1.Visibility = Visibility.Hidden;
                        Image_1.Visibility = Visibility.Hidden;
                        Text_2.Visibility = Visibility.Hidden;
                        Image_2.Visibility = Visibility.Hidden;
                        Text_3.Visibility = Visibility.Hidden;
                        Image_3.Visibility = Visibility.Hidden;
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Collapsed;
                        //LJN, instructing the delegate in the explanation module
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("Now you see 5×5 squares, and then the squares will be displayed in order. Please remember them. After the square is hidden, click the squares in sequence.");//Add code, call function, display the text under the digital person

                        //LJN
                        StartGame();
                    }
                    break;
            }
        }


        //LJN,Add some functions and styles
        private Style CreateCustomButtonStyle()
        {//Pack a style
            // Create button style
            Style buttonStyle = new Style(typeof(Button));

            // Set background to white
            buttonStyle.Setters.Add(new Setter(Button.BackgroundProperty, Brushes.White));

            // Set shadow effect
            ////LJN,Add shadow effect
            //Effect = new DropShadowEffect
            //{
            //    Color = Colors.Gray,      // Shadow color
            //    BlurRadius = 10,          // Fuzzy radius
            //    ShadowDepth = 5,          // Shadow depth
            //    Direction = 315,          // Shadow direction, angle
            //    Opacity = 0.5             // Shadow Transparency
            //},
            buttonStyle.Setters.Add(new Setter(Button.EffectProperty, new DropShadowEffect
            {
                Color = Colors.Gray,
                BlurRadius = 10,
                ShadowDepth = 5,
                Direction = 315,
                Opacity = 0.5
            }));

            // Custom templates to remove default visual changes when mouse over
            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            border.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Button.BorderBrushProperty));
            border.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Button.BorderThicknessProperty));

            ////LJN,Cancel the length and width setting, keep filling
            //HorizontalAlignment = HorizontalAlignment.Stretch,  // Set horizontal fill
            //VerticalAlignment = VerticalAlignment.Stretch,   // Set vertical fill
            FrameworkElementFactory contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            border.AppendChild(contentPresenter);
            template.VisualTree = border;

            ////LJN,Make the mouse not change when it moves abovebuttonThe color of
            //FocusVisualStyle = null
            // Triggers to ensure that the background remains unchanged while the mouse is hovered
            System.Windows.Trigger isMouseOverTrigger = new System.Windows.Trigger { Property = Button.IsMouseOverProperty, Value = true };
            isMouseOverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, Brushes.White));
            template.Triggers.Add(isMouseOverTrigger);

            // Set the template to style
            buttonStyle.Setters.Add(new Setter(Button.TemplateProperty, template));

            return buttonStyle;
        }

        //LJN,Add some functions and styles
        private void CreateImage()
        {//Create two images from localJIYIFolder reading
            //First get the full path to the image
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;            // Get the root directory of the current project(Right nowbinTable of contents)
            //@ Symbols are used to define verbatim strings（verbatim string）, make special characters in the string（Such as backslash \）Explained as is and no longer required to escape.

            // Construct the relative path of the image
            string correctRelativePath = @"Games\pic\JIYI\Correct.png";
            string errorRelativePath = @"Games\pic\JIYI\Error.png";

            // use Path.Combine To splice the absolute path
            string correctImagePath = System.IO.Path.Combine(currentDirectory, correctRelativePath);
            string errorImagePath = System.IO.Path.Combine(currentDirectory, errorRelativePath);
            CorrectImage.Source = new BitmapImage(new Uri(correctImagePath, UriKind.RelativeOrAbsolute));
            ErrorImage.Source = new BitmapImage(new Uri(errorImagePath, UriKind.RelativeOrAbsolute));
            ErrorImage.Visibility = Visibility.Collapsed;
            CorrectImage.Visibility = Visibility.Collapsed;


            //For the trial version, hide the original text prompts
            StatusTextBlock1.Visibility = Visibility.Collapsed;
        }
    }
}

