using System;
using System.Collections.Generic;
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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Path = System.IO.Path;

namespace crs.game.Games
{
    /// <summary>
    /// Explanation of alertness.xaml Interaction logic
    /// </summary>
    public partial class Explanation of alertness : BaseUserControl
    {
        private DispatcherTimer waitTimer;
        private DispatcherTimer waitTimer_withoutSound;
        private string correctImage_path;
        private string currentDirectory;
        private string targetDirectory;
        private bool buttonClick;
        // "Appearance and memory" Folders
        private string FolderPath;
        public Action GameBeginAction { get; set; }
        public bool Correct = false;
        public bool Sound = false;
        public Func<string, Task> VoicePlayFunc { get; set; }

        public Explanation of alertness()
        {
            InitializeComponent();

            this.Focusable = true;
            this.Focus();

            this.Loaded += Explanation of alertness_Loaded;
            init_path();


        }

        private void init_path()
        {
            currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            targetDirectory = Path.Combine(Directory.GetParent(currentDirectory).Parent.Parent.FullName, "crs.game", "Games");
            FolderPath = Path.Combine(targetDirectory, "Alert ability");
        }

        private void Explanation of alertness_Loaded(object sender, RoutedEventArgs e)
        {
            // Ensure key presses and focus behavior when page loads
            Button_2_Click(null, null);
            this.Focus();  // Make sure the focus is on the window
        }

        private void LoadImage(string imagePath)
        {
            if (File.Exists(imagePath))
            {
                // create BitmapImage Object and set image source
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.EndInit();

                // set up Image Source of the control
                CorrectOrNot.Source = bitmap;
            }

        }


        private void ShowImage()
        {
            Sound = false;
            RandomImage.Visibility = Visibility.Hidden;
            TipBlock.Text = "";
            CorrectOrNot.Source = null;
            RandomImage.Width = 245;
            RandomImage.Height = 245;

            // If the timer already exists, stop and reset
            waitTimer?.Stop();

            // Create a new timer
            waitTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3) // Set the waiting time to 3 Second
            };

            waitTimer.Tick += (s, args) =>
            {
                waitTimer.Stop();
                RandomImage.Visibility = Visibility.Visible;

                // Define a hidden timer
                var hideTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };

                hideTimer.Tick += async (hideSender, hideArgs) =>
                {
                    hideTimer.Stop();
                    await Task.Delay(1000);
                    RandomImage.Visibility = Visibility.Hidden;

                    // Determine whether the user clicks the confirm button
                    if (!Correct) // Assumptions Correct Indicates whether the user has clicked the confirm button
                    {
                        correctImage_path = Path.Combine(FolderPath, "wrong.png");
                        LoadImage(correctImage_path);
                        await Task.Delay(2000);
                        ShowImage(); // Recall ShowImage
                    }
                };

                hideTimer.Start();
            };

            waitTimer.Start();
        }
        private async void JinggaoButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage == 1)
            {
                // Call the sound source display method
                await PlayWarningSoundAsync();
            }
        }

        private async Task PlayWarningSoundAsync()
        {
            // Play warning sound
            Console.Beep(800, 200);
            await Task.Delay(200); // Make sure the beep is finished
        }

        private void ShowImage_withoutSound()
        {
            buttonClick = false;
            Sound = true;
            TipBlock.Text = "";
            RandomImage.Visibility = Visibility.Hidden;
            RandomImage.Width = 245;
            RandomImage.Height = 245;

            if (waitTimer_withoutSound != null)
            {
                waitTimer_withoutSound.Stop();
            }

            waitTimer_withoutSound = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };

            waitTimer_withoutSound.Tick += (s, args) =>
            {
                waitTimer_withoutSound.Stop();

                Task.Run(() => Console.Beep(800, 400));
                RandomImage.Visibility = Visibility.Visible;

                // Define a hidden timer
                var hideTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };

                hideTimer.Tick += async (hideSender, hideArgs) =>
                {
                    hideTimer.Stop();
                    await Task.Delay(1000);
                    if (OkButton.Visibility != Visibility.Visible)
                    {
                        RandomImage.Visibility = Visibility.Hidden;
                    }
                    

                    // If the user does not click the confirm button,Correct Still for false, then call again ShowImage
                    if (!buttonClick)
                    {
                        Correct = false; // Make sure the status is reset
                        correctImage_path = Path.Combine(FolderPath, "wrong.png");
                        LoadImage(correctImage_path);
                        await Task.Delay(2000);
                        ShowImage();
                    }
                };

                hideTimer.Start();
            };

            waitTimer_withoutSound.Start();
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Check whether to press Enter key
            if (e.Key == Key.Enter)
            {
                // Call ConfirmButton_Click method
                Button_Click(sender, e);
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if(Sound)
            {
                buttonClick = true;
            }
            if (RandomImage.Visibility == Visibility.Visible)
            {
                TipBlock.FontSize = 40;
                if (Sound)
                {

                    correctImage_path = Path.Combine(FolderPath, "correct.png");
                    LoadImage(correctImage_path);
                    
                    
                    //TipBlock.Text = "       Congratulations on getting right！";

                }

                TipBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#06CE77"));

                Correct = true;
                if (Correct && Sound)
                {
                    OkButton.Visibility = Visibility.Visible;
                    anjian.Visibility = Visibility.Collapsed;
                }
                else
                {
                    //await Task.Delay(1000);
                    ShowImage_withoutSound();
                }


            }
            else
            {
                if (Sound)
                {
                    waitTimer_withoutSound?.Stop();
                }
                else
                {
                    waitTimer?.Stop(); // Stop the timer
                }
                //TipBlock.FontSize = 40;
                //TipBlock.Text = "       Sorry to answer wrong！";
                //TipBlock.Foreground = new SolidColorBrush(Colors.Red);

                correctImage_path = Path.Combine(FolderPath, "wrong.png");
                LoadImage(correctImage_path);
                OkButton.Visibility = Visibility.Collapsed;
                // Wait for two seconds
                await Task.Delay(2000);
                ShowImage();

            }

        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Related logic for starting answering questions
            OnGameBegin();
        }

        int currentPage = -2;

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
                        Text_3.Visibility = Visibility.Collapsed;
                        Text_4.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;
                        JinggaoButton.Visibility = Visibility.Collapsed;

                        // Hide part of the trial
                        RandomImage.Visibility = Visibility.Collapsed;
                        TargetImage.Visibility = Visibility.Collapsed;
                        anjian.Visibility = Visibility.Collapsed;
                        TipBlock.Visibility = Visibility.Collapsed;
                        TipBlock1.Visibility = Visibility.Collapsed;
                        mubiaowu.Visibility = Visibility.Collapsed;
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Content = "Next step";
                        Button_2.Margin = new Thickness(329, 850, 0, 0);
                        Button_3.Margin = new Thickness(770, 850, 0, 0);

                        await OnVoicePlayAsync(Text_1.Text);
                    }
                    break;

                case 1:
                    {
                        // Display the second interface of the explanation
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Visible;
                        Text_3.Visibility = Visibility.Visible;
                        Text_4.Visibility = Visibility.Visible;
                        Image_2.Visibility = Visibility.Visible;
                        Image_3.Visibility = Visibility.Visible;
                        JinggaoButton.Visibility = Visibility.Visible;


                        // Hide the controls for the trial play section
                        RandomImage.Visibility = Visibility.Collapsed;
                        TargetImage.Visibility = Visibility.Collapsed;
                        anjian.Visibility = Visibility.Collapsed;
                        TipBlock.Visibility = Visibility.Collapsed;
                        TipBlock1.Visibility = Visibility.Collapsed;
                        mubiaowu.Visibility = Visibility.Collapsed;
                        Button_1.IsEnabled = true;
                        Button_2.Content = "Trial";
                        Button_2.Margin = new Thickness(550, 850, 0, 0);
                        Button_3.Margin = new Thickness(911, 850, 0, 0);
                        Button_1.Visibility = Visibility.Visible;

                        await OnVoicePlayAsync(Text_2.Text);
                        await OnVoicePlayAsync(Text_3.Text);
                        await OnVoicePlayAsync(Text_4.Text);
                    }
                    break;
                case 2:
                    {
                        // Enter the trial interface
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Text_3.Visibility = Visibility.Collapsed;
                        Text_4.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;
                        JinggaoButton.Visibility = Visibility.Collapsed;
                        // Controls showing the trial part
                        RandomImage.Visibility = Visibility.Visible;
                        TargetImage.Visibility = Visibility.Visible;
                        anjian.Visibility = Visibility.Visible;
                        TipBlock.Visibility = Visibility.Visible;
                        TipBlock1.Visibility = Visibility.Collapsed;
                        mubiaowu.Visibility = Visibility.Visible;
                        // Hide the button for the explanation section
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Collapsed;
                        ShowImage();
                        // Force focus to remain in the window
                        this.Focus();
                        //LJN, instructing the delegate in the explanation module
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("Now you can see the selected target map on the left side of the screen. When a target object with a warning sound or no warning sound appears on the screen, please click the confirmation button with the mouse.");//Add code, call function, display the text under the digital person
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

