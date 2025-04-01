using Microsoft.IdentityModel.Abstractions;
using System;
using System.Collections.Generic;
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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace crs.game.Games
{
    /// <summary>
    /// Topological memory explanation.xaml Interaction logic
    /// </summary>
    public partial class Topological memory explanation : BaseUserControl
    {
        private Button[] buttons;
        private string[] imagePaths = { "Resources/MEMO/general/sample1.jpg", "Resources/MEMO/general/sample2.jpg", "Resources/MEMO/general/sample3.jpg" };
        private string recallImagePath = "Resources/MEMO/general/sample2.jpg";
        private DispatcherTimer timer;
        private int countdownTime = 5;
        private Button selectedButton;
        private int StopDurations = 1000;

        private string oImage;

        private int gameStage = 0; // PressOKFunctions executed: 0 for Prepare and start, 1 is the pattern selection process, 2 is the training

        private string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory; // The absolute path to the current project
        private string ResourcesPath = System.IO.Path.Combine("Resources", "Word memory");//The fixed path used here is the memory path of word, which can be changed later.
        private SoundPlayer soundPlayer; // Used to sing
        public string ErrorSoundPath;//The wrong sound file path, inOnStartAsync()Medium configuration
        public string CorrectSoundPath;//The correct sound file path is inOnStartAsync()Medium configuration
        public Topological memory explanation()
        {
            InitializeComponent();

            // Keys
            this.KeyDown += OnKeyDown;
            this.PreviewKeyDown += OnKeyDown;
            this.Focusable = true;
            this.Focus(); // Actively get focus

            // Get the current working directory
            string currentDirectory = Directory.GetCurrentDirectory();
            string imageFolderPath = System.IO.Path.Combine(currentDirectory, "Resources/MEMO/General");
            string p2 = System.IO.Path.Combine(currentDirectory, "Resources/MEMO");
            BitmapImage image1 = new BitmapImage(new Uri(p2 + "/x.png", UriKind.Absolute));
            BitmapImage image2 = new BitmapImage(new Uri(p2 + "/y.png", UriKind.Absolute));


            Image_1.Source = image1;
            Image_2.Source = image2;

            CorrectSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");
            // for Image Control loading picture Source
            CorrectImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Correct.png"), UriKind.RelativeOrAbsolute));
            WrongImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Error.png"), UriKind.RelativeOrAbsolute));




            // Initialize the button array and create the button
            buttons = new Button[3];
            for (int i = 0; i < 3; i++)
            {
                Button button = new Button();
                button.Click += Button_Click;
                button.Background = new ImageBrush(new BitmapImage(new Uri(imagePaths[i], UriKind.Relative)));
                button.Width = 150;
                button.Height = 150;
                buttons[i] = button;
                PatternGrid.Children.Add(button);
                button.IsHitTestVisible = false;
            }

            // Initialize the timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            this.Loaded += Topological memory explanation_Loaded;


        }

        private void Topological memory explanation_Loaded(object sender, RoutedEventArgs e)
        {
            Button_2_Click(null, null);
        }

        private void OnReadyButtonClick(object sender, RoutedEventArgs e)
        {
            // Initialization countdown
            if (gameStage == 0)
            {
                countdownTime = 5;
                TipsTextBlock.Text = $"Remember the target pattern and its location,  {countdownTime}The pattern disappears in seconds";


                // Start the timer
                timer.Start();
            }
            else if (gameStage == 1)
            {
                // If no buttons are selected
                if (selectedButton == null)
                {
                    TipsTextBlock.Text = "Please select the pattern to verify first！";
                    return;
                }

                // Restore selected button picture
                var originalImage = new BitmapImage(
                    new Uri((string)selectedButton.Tag, UriKind.Relative));

                selectedButton.Background = new ImageBrush(originalImage);

                // Disable all button interactions
                foreach (var btn in buttons)
                {
                    btn.IsHitTestVisible = false;
                }

                // Verification results
                if ((string)selectedButton.Tag == recallImagePath)
                {
                    /*CorrectTextBox.Background = Brushes.Green;*/
                    ShowFeedbackImage(CorrectImage);
                    TipsTextBlock.Text = "Congratulations, the right choice！PressOKKey to enter training";
                    PlayWav(CorrectSoundPath);
                    ReadyButton.Visibility = Visibility.Visible;
                    ReadyButton.Content = "Enter training";

                    gameStage = 2;

                }
                else
                {
                    /*IncorrectTextBox.Background = Brushes.Red;*/
                    ShowFeedbackImage(WrongImage);
                    TipsTextBlock.Text = "Error, pressOKTry again！";
                    PlayWav(ErrorSoundPath);
                    ReadyButton.Visibility = Visibility.Visible;
                    ReadyButton.Content = "Try again";

                    gameStage = 0;
                    ResetButtonImages();
                }

                // Clear the selected state
                selectedButton.BorderThickness = new Thickness(0);
                selectedButton = null;
                

            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            countdownTime--;

            if (countdownTime > 0)
            {
                // Update prompt text
                TipsTextBlock.Text = $"Remember the target pattern and its position,   {countdownTime}The pattern disappears in seconds";
            }
            else
            {
                // Stop the timer
                timer.Stop();
                // OKKey decision change

                gameStage = 1;
                Stage_Ready(); // Switch to the preparation stage

                // Enable buttons and clear patterns
                foreach (var button in buttons)
                {
                    button.IsHitTestVisible = true;
                    // While setting a gray background, store the actual image path inTagmiddle
                    button.Tag = ((ImageBrush)button.Background).ImageSource.ToString();
                    button.Background = Brushes.Gray;
                }

                string currentDirectory = Directory.GetCurrentDirectory();
                string imageFolderPath = System.IO.Path.Combine(currentDirectory, "Resources/MEMO/General");
                RecallButton.Source = new BitmapImage(new Uri(imageFolderPath + "/sample2.jpg", UriKind.Absolute));

                // Update prompt text
                TipsTextBlock.Text = "PressOKKeys to select the covered target pattern";
            }
        }

        private void OnResetButtonClick(object sender, RoutedEventArgs e)
        {
            // Reset the button pattern
            for (int i = 0; i < 3; i++)
            {
                buttons[i].Background = new ImageBrush(new BitmapImage(new Uri(imagePaths[i], UriKind.Relative)));
                buttons[i].IsHitTestVisible = false; // Disable button click function
            }

            // Reset prompt text and color
/*            CorrectTextBox.Background = Brushes.White;
            IncorrectTextBox.Background = Brushes.White;*/
            TipsTextBlock.Text = string.Empty;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Defensive inspection
            if (sender == null || !(sender is Button clickedButton)) return;

            // Clear the button style selected previously
            if (selectedButton != null)
            {
                selectedButton.BorderThickness = new Thickness(0);
            }

            // Update the selected button
            selectedButton = clickedButton;
            selectedButton.BorderThickness = new Thickness(3);
            selectedButton.BorderBrush = Brushes.Yellow;

            // Record the selected image path
            oImage = selectedButton.Tag as string;
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
                        // Display the first interface of explanation
                        Text_1.Visibility = Visibility.Visible;
                        Image_1.Visibility = Visibility.Visible;
                        Text_2.Visibility = Visibility.Hidden;
                        Image_2.Visibility = Visibility.Hidden;



                        Button_1.IsEnabled = false;
                        Button_2.Content = "Next step";

                        await OnVoicePlayAsync(Text_1.Text);
                    }
                    break;
                case 1:
                    {
                        // Display the first interface of explanation
                        Text_1.Visibility = Visibility.Hidden;
                        Image_1.Visibility = Visibility.Hidden;
                        Text_2.Visibility = Visibility.Visible;
                        Image_2.Visibility = Visibility.Visible;

                        Button_1.IsEnabled = true;
                        Button_2.Content = "Trial";

                        await OnVoicePlayAsync(Text_2.Text);
                    }
                    break;
                case 2:
                    {
                        // Display the second interface of the explanation
                        Text_1.Visibility = Visibility.Hidden;
                        Image_1.Visibility = Visibility.Hidden;
                        Text_2.Visibility = Visibility.Hidden;
                        Image_2.Visibility = Visibility.Hidden;

                        Button_1.IsEnabled = true;
                        Button_2.Content = "Trial";

                        Button_1.Visibility = Visibility.Hidden;
                        Button_2.Visibility = Visibility.Hidden;

                        grid.Visibility = Visibility.Visible;


                        //LJN, instructing the delegate in the explanation module
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("You will see three graphics on the interface. Please remember their order quickly. After the countdown is over, the three graphics will be overwritten. Please find out and select the location corresponding to the target graphics on the right side of the screen as quickly as possible. according to“↑↓←→”Select and press“Enter”confirmOK");//Add code, call function, display the text under the digital person
                        //LJN

                    }
                    break;

            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (gameStage == 0)
            {
                // Only allowed during the unprepared phase Enter Trigger preparation
                if (e.Key == Key.Enter)
                {
                    OnReadyButtonClick(null, null);
                    e.Handled = true;
                }
                return;
            }

            // Arrow key operation is allowed in the preparation phase
            switch (e.Key)
            {
                case Key.Left:
                    MoveSelection(-1);
                    e.Handled = true;
                    break;
                case Key.Right:
                    MoveSelection(1);
                    e.Handled = true;
                    break;
                case Key.Enter:
                    if (gameStage == 1)
                    {
                        OnReadyButtonClick(null, null);

                    }else if (gameStage == 2)
                    {
                        OnGameBegin(); //Start training
                        
                    }
                    e.Handled = true;
                    break;


            }
        }
        private void MoveSelection(int delta)
        {
            if (selectedButton == null)
            {
                selectedButton = buttons[0];
            }
            else
            {
                int index = Array.IndexOf(buttons, selectedButton);
                index = (index + delta + buttons.Length) % buttons.Length;
                selectedButton = buttons[index];
            }
            selectedButton.Focus(); // Focus Follow
            UpdateSelectedButton();
        }

        // Show feedback pictures
        private async void ShowFeedbackImage(Image image)
        {//Image showing feedback
            if (image == null) return;

            // Show pictures（Force main thread）
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                image.Visibility = Visibility.Visible;
            });

            // wait 5 Second
            await Task.Delay(5000);

            // Hide pictures（Force main thread）
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                image.Visibility = Visibility.Collapsed;
            });
        }

        private void UpdateSelectedButton()
        {
            foreach (var btn in buttons)
            {
                btn.BorderThickness = btn == selectedButton ? new Thickness(3) : new Thickness(0);
                btn.BorderBrush = Brushes.Yellow;
            }
        }

        private void Stage_UnReady()
        {
            // Disable all buttons
            foreach (var btn in buttons)
            {
                btn.IsEnabled = false;
                btn.Focusable = false; // No focus
                btn.ClearValue(Button.BorderBrushProperty); // Clear the selected style
            }

            // set up Enter Key triggers preparation logic
            ReadyButton.IsDefault = true;
            RecallButton.Visibility = Visibility.Collapsed; // Hidden memory patterns
            this.Focus(); // Focus back to main control
        }

        private void Stage_Ready()
        {
            // Enable all buttons
            foreach (var btn in buttons)
            {
                btn.IsEnabled = true;
                btn.Focusable = true; // Allows to gain focus
            }

            // set up Enter Key trigger verification logic
            ReadyButton.IsDefault = false;
            RecallButton.Visibility = Visibility.Visible; // Show memory patterns
            buttons[0].Focus(); // Focus to the first button
            gameStage = 1;
        }

        private void keyboard_click(object sender, RoutedEventArgs e)
        {
            var clickedButton = sender as Button;
            Button_Click(clickedButton, e);
        }
        private void ResetButtonImages()
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                // from imagePaths Load the original image
                var originalImage = new BitmapImage(new Uri(imagePaths[i], UriKind.Relative));
                buttons[i].Background = new ImageBrush(originalImage);
                buttons[i].Tag = imagePaths[i]; // make sure Tag Store the correct path
            }
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
    }
}
