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
using System.Windows.Navigation;




namespace crs.game.Games
{

    /// <summary>
    /// Explanation of appearance and memory.xaml Interaction logic
    /// </summary>
    public partial class Explanation of appearance and memory : BaseUserControl
    {
        private string[] imagePaths;
        private string selectedImagePath;
        private string displayedImagePath;
        private bool isMemoryStage = true;
        private Border selectedThumbnailBorder;
        public Action GameBeginAction { get; set; }

        public Func<string, Task> VoicePlayFunc { get; set; }


        public Explanation of appearance and memory()
        {
            InitializeComponent();
            // Get the picture folder under the relative path
            string imageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources/Appearance and memory/GESI");
            imagePaths = Directory.GetFiles(imageFolder, "*.jpg");
            // Randomly draw a picture
            Random random = new Random();
            displayedImagePath = imagePaths[random.Next(imagePaths.Length)];
            // Show randomly drawn pictures
            DisplayedImage.Source = new BitmapImage(new Uri(displayedImagePath));
            InfoText4.Text = "";
            // hidebutton3
            button3.Visibility = Visibility.Collapsed;
            
            this.Loaded += Explanation of appearance and memory_Loaded;
        }

        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (ThumbnailPanel.Children.Count == 0) return; // If there is no thumbnail, return directly

            // Find the index of the currently selected image path
            int currentIndex = Array.IndexOf(imagePaths, selectedImagePath);

            if (e.Key == Key.Left)
            {
                // Swipe left
                if (currentIndex > 0)
                {
                    currentIndex--; // Select the previous one
                }
                else
                {
                    currentIndex = imagePaths.Length - 1; // Loop to the last one
                }
                selectedImagePath = imagePaths[currentIndex]; // Update the selected image path
                SelectThumbnailByImagePath(selectedImagePath); // Highlight selected thumbnails
            }
            else if (e.Key == Key.Right)
            {
                // Swipe right
                if (currentIndex < imagePaths.Length - 1)
                {
                    currentIndex++; // Select Next
                }
                else
                {
                    currentIndex = 0; // Loop to the first
                }
                selectedImagePath = imagePaths[currentIndex]; // Update the selected image path
                SelectThumbnailByImagePath(selectedImagePath); // Highlight selected thumbnails
            }
            else if (e.Key == Key.Enter)
            {
                // Press Enter Keys automatically click button2
                if (button2.Visibility == Visibility.Visible) // make sure button2 visible
                {
                    button2.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
            }
        }

        // Auxiliary method of selecting thumbnails according to the image path
        private void SelectThumbnailByImagePath(string imagePath)
        {
            // Cancel the previously highlighted thumbnail
            if (selectedThumbnailBorder != null)
            {
                selectedThumbnailBorder.BorderBrush = Brushes.Transparent; // Revert to transparent border
                selectedThumbnailBorder.BorderThickness = new Thickness(1); // Revert to default border thickness
            }

            // Iterate through the thumbnail and find the corresponding path to highlight
            foreach (UIElement element in ThumbnailPanel.Children)
            {
                if (element is Border border && border.Child is Image img &&
                    img.Source is BitmapImage bitmap &&
                    bitmap.UriSource.LocalPath.Equals(new Uri(imagePath, UriKind.Absolute).LocalPath, StringComparison.OrdinalIgnoreCase))
                {
                    border.BorderBrush = Brushes.Red; // Change the border color to red
                    border.BorderThickness = new Thickness(2); // Change the border thickness

                    selectedThumbnailBorder = border; // Record the currently highlighted thumbnail

                    // Force refresh
                    border.InvalidateVisual();
                    ThumbnailPanel.InvalidateVisual();
                    break;
                }
            }

            // Update the displayed image
            DisplayedImage.Source = new BitmapImage(new Uri(selectedImagePath));
        }


        // Helpful methods for selecting thumbnails


        private void Explanation of appearance and memory_Loaded(object sender, RoutedEventArgs e)
        {
            Button_2_Click(null, null);
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

        private async void PressContinue_Button(object sender, RoutedEventArgs e)
        {
            if (isMemoryStage)
            {
                // The user completes memory and enters the selection stage
                InfoText4.Text = "";
                CorrectOrNot.Source = null;
                DisplayThumbnails();
                DisplayedImage.Source = null;
                isMemoryStage = false;  // Switch to the selection phase
                InfoText3.Text = "Please select the corresponding person image based on the recorded information";
                button2.Content = "OK";
                //InfoText2.Text = "Select a picture of memory";
            }
            else
            {
                // Check if the user selection is correct
                if (selectedImagePath == displayedImagePath)
                {
                    //InfoText4.Text = "correct";
                    //InfoText4.Foreground = Brushes.Green;
                    string currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    string targetDirectory = Path.Combine(currentDirectory, "Resources");
                    // "Appearance and memory" Folders
                    string FolderPath = Path.Combine(targetDirectory, "Appearance and memory");
                    string picFolderPath = Path.Combine(FolderPath, "correct.png");
                    LoadImage(picFolderPath);

                    button2.Visibility = Visibility.Collapsed;
                    button3.Visibility = Visibility.Visible;  // showbutton3
                }
                else
                {
                    //InfoText4.Text = "mistake";
                    //InfoText4.Foreground = Brushes.Red;
                    string currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    string targetDirectory = Path.Combine(currentDirectory, "Resources");
                    // "Appearance and memory" Folders
                    string FolderPath = Path.Combine(targetDirectory, "Appearance and memory");
                    string picFolderPath = Path.Combine(FolderPath, "error.png");
                    LoadImage(picFolderPath);
                    await Task.Delay(1500);
                    CorrectOrNot.Source = null;
                    button2.Content = "OK";
                }

            }
        }

        // Highlight selected thumbnails
        private void HighlightThumbnail(string imagePath)
        {
            // Remove previously highlighted thumbnails
            if (selectedThumbnailBorder != null)
            {
                selectedThumbnailBorder.BorderBrush = Brushes.Transparent;
                selectedThumbnailBorder.BorderThickness = new Thickness(0);
                selectedThumbnailBorder = null;
            }

            foreach (UIElement element in ThumbnailPanel.Children)
            {
                if (element is Border border && border.Child is Image img &&
                    img.Source is BitmapImage bitmap &&
                    bitmap.UriSource.LocalPath.Equals(new Uri(imagePath, UriKind.Absolute).LocalPath, StringComparison.OrdinalIgnoreCase))
                {
                    border.BorderBrush = Brushes.Red;
                    border.BorderThickness = new Thickness(2);

                    selectedThumbnailBorder = border;

                    // Force refresh
                    border.InvalidateVisual();
                    ThumbnailPanel.InvalidateVisual();
                    break;
                }
            }
        }


        private void DisplayThumbnails()
        {
            ThumbnailPanel.Children.Clear();
            ThumbnailPanel.Visibility = Visibility.Visible;

            foreach (var imagePath in imagePaths)
            {
                // Create a new Border
                Border border = new Border
                {
                    Width = 100,
                    Height = 65,
                    Margin = new Thickness(5),
                    BorderBrush = Brushes.Transparent, // Default border color
                    BorderThickness = new Thickness(1) // Default border thickness
                };

                // Create thumbnails
                Image thumbnail = new Image
                {
                    Source = new BitmapImage(new Uri(imagePath)),
                    Width = 100,
                    Height = 100,
                    Tag = imagePath // Store the path in Tag middle
                };

                thumbnail.MouseLeftButtonUp += Thumbnail_Click; // Subscribe to click events

                // Will Image Add to Border
                border.Child = thumbnail;

                // Will Border Add to ThumbnailPanel
                ThumbnailPanel.Children.Add(border);
            }
        }




        //Thumbnail
        

        private void Thumbnail_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // make sure sender It's one Border
            Border selectedBorder = (sender as FrameworkElement)?.Parent as Border;

            if (selectedBorder != null)
            {
                // If there is a previously selected Border, restore its style
                if (selectedThumbnailBorder != null)
                {
                    selectedThumbnailBorder.BorderBrush = Brushes.Transparent; // Revert to transparent border
                    selectedThumbnailBorder.BorderThickness = new Thickness(1); // Revert to default border thickness
                }

                // Set the currently selected Border
                selectedThumbnailBorder = selectedBorder;
                selectedThumbnailBorder.BorderBrush = Brushes.Red; // Change the border color to red
                selectedThumbnailBorder.BorderThickness = new Thickness(2); // Optional: Change the border thickness

                // Get Image path and update the display
                Image selectedImage = (Image)selectedBorder.Child;
                selectedImagePath = selectedImage.Tag.ToString();
                DisplayedImage.Source = new BitmapImage(new Uri(selectedImagePath));
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
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        button2.Visibility = Visibility.Collapsed;
                        DisplayedImage.Visibility = Visibility.Collapsed;
                        InfoText3.Visibility = Visibility.Collapsed;
                        //Button_1.IsEnabled = false;
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Content = "Next step";
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

                        Button_1.Visibility = Visibility.Visible;
                        Button_1.IsEnabled = true;
                        Button_2.Content = "Trial";
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
                        InfoText4.Visibility = Visibility.Visible;
                        InfoText3.Visibility = Visibility.Visible;
                        DisplayedImage.Visibility = Visibility.Visible;
                        button2.Visibility = Visibility.Visible;
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;

                        Button_3.Height = 67;
                        Button_3.Width = 241;
                        Button_3.FontSize = 40;
                        Button_3.Margin = new Thickness(0, 895, 0, 0);
                        Button_3.HorizontalAlignment = HorizontalAlignment.Center;

                        //LJN, instructing the delegate in the explanation module
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("You will first see a person's appearance on the screen. Please remember its characteristics. After the memory is completed, click the mouse in the lower right corner.“Memory completion”button. Then a series of characters' appearance images appear. You need to distinguish which one is the person you just remembered based on the characteristics you remember, and click on the lower right corner with the mouse“Confirm selection”button.");//Add code, call function, display the text under the digital person
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
