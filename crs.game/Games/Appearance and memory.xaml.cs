using crs.core;
using crs.core.DbModels;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace crs.game.Games
{
    /// <summary>
    /// GESI.xaml Interaction logic
    /// </summary>
    public class KeySimulator
    {
        private DispatcherTimer _rightKeyTimer;
        private Action<string> _loadContentAction;
        private Action<string> _highlightThumbnailAction;
        private Panel _thumbnailPanel;
        private int _selectedIndex;

        public KeySimulator(Panel thumbnailPanel, Action<string> loadContentAction, Action<string> highlightThumbnailAction)
        {
            _thumbnailPanel = thumbnailPanel;
            _loadContentAction = loadContentAction;
            _highlightThumbnailAction = highlightThumbnailAction;
            _rightKeyTimer = new DispatcherTimer();
            _rightKeyTimer.Tick += RightKeyTimer_Tick;
        }

        public void Start(bool isEnabled, int intervalInMilliseconds)
        {
            if (isEnabled)
            {
                _rightKeyTimer.Interval = TimeSpan.FromMilliseconds(intervalInMilliseconds);
                _rightKeyTimer.Start();
            }
            else
            {
                _rightKeyTimer.Stop();
            }
        }

        private void RightKeyTimer_Tick(object sender, EventArgs e)
        {


            if (_thumbnailPanel.Children.Count == 0) return; // If there is no thumbnail, return directly

            // Swipe right
            _selectedIndex = (_selectedIndex < _thumbnailPanel.Children.Count - 1) ? _selectedIndex + 1 : 0;

            // Debug: Output the currently selected index
            Console.WriteLine($"Selected index: {_selectedIndex}");

            SelectThumbnailByIndex(_selectedIndex);
        }


        private void SelectThumbnailByIndex(int index)
        {
            if (index < 0 || index >= _thumbnailPanel.Children.Count) return; // Border check

            var border = _thumbnailPanel.Children[index] as Border;
            if (border != null && border.Child is Image img)
            {
                string imagePath = ((BitmapImage)img.Source).UriSource.LocalPath;
                _loadContentAction?.Invoke(imagePath); // Load the main picture
                _highlightThumbnailAction?.Invoke(imagePath); // Highlight selected thumbnails
            }
        }
    }
    public class TimeTracker
    {
        private Stopwatch memoryStopwatch;
        private Stopwatch selectionStopwatch;
        public double totalMemoryTime;
        public double totalSelectionTime;


        public TimeTracker()
        {
            memoryStopwatch = new Stopwatch();
            selectionStopwatch = new Stopwatch();
            totalMemoryTime = 0;
            totalSelectionTime = 0;

        }

        // Start memorizing timing
        public void StartMemoryTimer()
        {
            memoryStopwatch.Restart();
        }

        // End memory timing
        public void StopMemoryTimer()
        {
            memoryStopwatch.Stop();
            totalMemoryTime += memoryStopwatch.Elapsed.TotalSeconds;

        }

        // Start selecting timing
        public void StartSelectionTimer()
        {
            selectionStopwatch.Restart();
        }

        // End Select Timer
        public void StopSelectionTimer()
        {
            selectionStopwatch.Stop();
            totalSelectionTime += selectionStopwatch.Elapsed.TotalSeconds;

        }
    }
    public partial class Appearance and memory : BaseUserControl
    {
        private Border selectedThumbnailBorder;
        private string[] memorizedImages; // Image paths for users' memory
        private string difficultyFolderPath;
        private string picFolderPath;
        private string infoFolderPath;
        private string[] allImages; // All picture paths
        private string selectedImagePath; // The image path currently selected by the user
        private bool isMemoryStage = true; // Identify whether it is currently in the memory stage
        private int difficultyLevel; // Difficulty level
        private int userSelections; // Number of user selections
        private int correctSelections; // Number of correct choices
        private int selectedIndex = 0;
        private int currentQuestionIndex; // Index of current question
        private System.Timers.Timer timer;
        private int train_time; // Total training time

        /// <summary>
        ///  New timer for training time
        /// </summary>
        private DispatcherTimer trainingTimer;



        private TimeSpan remainingTime;
        private bool nameOrNot = false;
        private bool jobOrNot = false;
        private bool numberOrNot = false;
        private List<string> correctImagePaths = new List<string>(); // Store the correct image path
        private string correctImagePath;
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private Random random = new Random();
        private TimeTracker timeTracker;
        private DispatcherTimer clearInfoTextTimer;

        private int memoryNum = 0;
        private KeySimulator _keySimulator;

        private Queue<int> recentCorrectSelections = new Queue<int>();
        private Queue<int> recentTotalQuestions = new Queue<int>();

        /*------------------------------Adjustable parameters------------------------------*/
        private int change_up; // Rise threshold
        private int change_down; // Descent threshold
        private bool nameOnly;//Only use name
        private bool infoOnly;//Use information only
        private bool nameAndInfo;//Use name and information
        private bool soundOrNot;//Is there any prompt sound
        private bool imageOrNot;//Is there any feedback picture
        private bool sameOrNot;//Whether to use the same picture
        private int time = 30; //Training time     
        private int repeatNum;//Repeat pictures       
        private int inputMode;//Input mode
        private bool SingleKey;//Whether to enable single-key input
        private int SingleKeyTime;//Single key input time
        /*--------------------------------------------------------------------*/

        /*------------------------------Evaluation indicators------------------------------*/
        public int[] totalNum;
        public int[] errorNum;
        public double[] errorRate;
        public int[] errorPicNum;
        public int[] errorNameNum;
        public int[] errorJobNum;
        public int[] errorPhoneNum;
        public double memorizeTime;
        public double chooseTime;
        public int HighestLevel;


        // Define some cumulative variables to store data for five games
        private int gameCounter = 0; // Game count counter
        private int totalCorrectGames = 0; // Cumulative correct choices
        private int totalQuestionsInAllGames = 0; // Total number of problems accumulated
        private List<Question> allQuestions = new List<Question>();
        private int currenCorrectSelection;
        private int totalWrongSelection;
        private int requiredSelections;
        private int totalCorrectSelections;

        private List<int> lastFiveGamesResults = new List<int>(); // Store the most recent correct results
        private List<int> lastFiveGamesResults_2 = new List<int>(); // Store recent error results

        private int[] correctCountsByDifficulty = new int[22]; // The correct number of times per difficulty
        private int[] errorCountsByDifficulty = new int[22];   // Number of errors per difficulty

        private int correctStreak = 0; // Continuous correct counting
        private int wrongStreak = 0;   // Continuous Error Count


        /*--------------------------------------------------------------------*/

        /*--------------------------------------------------------------------*/

        //Set parameters

        public void StartCountdown(int minutes)
        {
            //train_time = minutes*60; // Set the total training time
            //remainingTime = TimeSpan.FromMinutes(minutes);
            UpdateCountdownText();
            timer.Start();
        }

        //Single-key mode interval settings
        private void StartOrStopSimulation(bool start, int interval)
        {
            _keySimulator.Start(start, interval);
        }
        //Evaluation index function
        private void InitializeNum()
        {
            totalNum = new int[21];
            errorNum = new int[21];
            errorRate = new double[21];
            errorPicNum = new int[21];
            errorNameNum = new int[21];
            errorJobNum = new int[21];
            errorPhoneNum = new int[21];
            memorizeTime = 0;
            chooseTime = 0;
        }

        //Update the current highest level
        private void RenewHighestLevel(int difficultL)
        {
            HighestLevel = difficultL > HighestLevel ? difficultL : HighestLevel;
        }

        //Keyboard control related functions
        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (thumbnailPanel.Children.Count == 0) return; // If there is no thumbnail, return directly

            if (e.Key == Key.Left && SingleKey == false)
            {
                // Swipe left
                selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : thumbnailPanel.Children.Count - 1;
                SelectThumbnailByIndex(selectedIndex);
            }
            else if (e.Key == Key.Right && SingleKey == false)
            {
                // Swipe right
                selectedIndex = (selectedIndex < thumbnailPanel.Children.Count - 1) ? selectedIndex + 1 : 0;
                SelectThumbnailByIndex(selectedIndex);
            }
            else if (e.Key == Key.Enter)
            {
                // Press Enter Keys automatically click button1
                Button1.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void SelectThumbnailByIndex(int index)
        {
            if (index < 0 || index >= thumbnailPanel.Children.Count) return; // Border check

            var border = thumbnailPanel.Children[index] as Border;
            if (border != null && border.Child is Image img)
            {
                string imagePath = ((BitmapImage)img.Source).UriSource.LocalPath;
                LoadContent(imagePath); // Load the main picture
                HighlightThumbnail(imagePath); // Highlight selected thumbnails
                selectedImagePath = imagePath; // Record the selected image path

                // showtxtcontent（Judging by difficulty level）
                if (difficultyLevel >= 7)
                {
                    string infoFilePath = Path.Combine(infoFolderPath, Path.GetFileNameWithoutExtension(imagePath).Split('-')[0] + ".txt");

                    if (File.Exists(infoFilePath) && isMemoryStage)
                    {
                        string[] infoLines = File.ReadAllLines(infoFilePath);

                        // Display different row counts according to difficulty level
                        if (difficultyLevel >= 7 && difficultyLevel <= 11)
                        {
                            // Show first line
                            InfoText1.Text = infoLines.Length > 0 ? infoLines[0] : "";
                        }
                        else if (difficultyLevel >= 12 && difficultyLevel <= 16)
                        {
                            // Show first and second rows
                            InfoText1.Text = string.Join(Environment.NewLine, infoLines.Take(2));
                        }
                        else if (difficultyLevel >= 17 && difficultyLevel <= 21)
                        {
                            // Show the first three lines
                            InfoText1.Text = string.Join(Environment.NewLine, infoLines.Take(3));
                        }
                        else
                        {
                            InfoText1.Text = ""; // Out of range, no content is displayed
                        }
                    }
                    else
                    {
                        InfoText1.Text = ""; // The file does not exist or is not in the memory stage, clear the display content
                    }
                }
                else
                {
                    InfoText1.Text = ""; // If the difficulty is less than 7, no information will be displayed
                }
            }
        }

        //Set countdown
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (remainingTime > TimeSpan.Zero)
            {
                remainingTime = remainingTime.Add(TimeSpan.FromSeconds(-1));
                Dispatcher.Invoke(() => UpdateCountdownText()); // renew UI Must pass Dispatcher.Invoke
                int? timeInSeconds = (int?)remainingTime.TotalSeconds;
                TimeStatisticsAction?.Invoke(train_time, timeInSeconds); // Update statistics
            }
            else
            {
                timer.Stop();

                // Time-consuming background processing
                errorRate[difficultyLevel - 1] = errorNum[difficultyLevel - 1] / (double)totalNum[difficultyLevel - 1];
                timeTracker.StopMemoryTimer();
                timeTracker.StopSelectionTimer();
                memorizeTime = timeTracker.totalMemoryTime / memoryNum;
                chooseTime = timeTracker.totalSelectionTime / totalNum[difficultyLevel - 1];

                Dispatcher.Invoke(() =>
                {
                    // Open Report Window and close the current window
                    //report reportWindow = new report(HighestLevel, errorNum, errorRate, errorPicNum, errorNameNum, errorJobNum, errorPhoneNum, memorizeTime, chooseTime);
                });
            }
        }

        private void UpdateCountdownText()
        {
            //CountdownTextBlock.Text = remainingTime.ToString(@"mm\:ss");
        }

        private void TrainingTimer_Tick(object sender, EventArgs e)
        {
            train_time--; // Countdown to training time

            // Calling delegate
            TimeStatisticsAction?.Invoke(train_time, (int?)remainingTime.TotalSeconds);

            if (train_time <= 0)
            {
                timer.Stop(); // Stop the main timer
                trainingTimer.Stop(); // Stop training timer
                //Focus on attention report reportWindow = new Focus on attention report(_INCREASE, _DECREASE, max_time, TRAIN_TIME, IS_RESTRICT_TIME, IS_BEEP, correctAnswers, wrongAnswers, igonreAnswer);
                //reportWindow.ShowDialog(); // Open the report window
                //this.Close(); // Close the current window
                //StopAction?.Invoke();

                OnGameEnd();
            }
        }

        // This method has no reference?
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (remainingTime > TimeSpan.Zero)
            {
                remainingTime = remainingTime.Add(TimeSpan.FromSeconds(-1));
                UpdateCountdownText();
            }
            else
            {
                errorRate[difficultyLevel - 1] = errorNum[difficultyLevel - 1] / (double)totalNum[difficultyLevel - 1];
                timeTracker.StopMemoryTimer();
                timeTracker.StopSelectionTimer();
                memorizeTime = timeTracker.totalMemoryTime / memoryNum;
                chooseTime = timeTracker.totalSelectionTime / totalNum[difficultyLevel - 1];
                timer.Stop();
                // Open Report window
                //report reportWindow = new report(HighestLevel, errorNum, errorRate, errorPicNum, errorNameNum, errorJobNum, errorPhoneNum, memorizeTime, chooseTime)

                OnGameEnd();
            }
        }



        //Play prompt music
        private void PlayMemoryAudio()
        {
            string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName;
            string project = Directory.GetParent(projectDirectory).FullName;
            string audioPath = Path.Combine(project, "sound.mp3");

            mediaPlayer.Open(new Uri(audioPath));
            mediaPlayer.Play();

        }

        // Set the difficulty level and update itUI
        public void SetDifficultyLevel(int level)
        {
            difficultyLevel = level;

            if (difficultyLevel >= 7 && difficultyLevel <= 11)
            {
                nameOrNot = true;
            }
            if (difficultyLevel >= 12)
            {
                nameOrNot = true;
                jobOrNot = true; // Difficulty is above 12, career problem is enabled
            }
            if (difficultyLevel >= 17)
            {
                nameOrNot = true;
                jobOrNot = true;
                numberOrNot = true; // Difficulty is above 16, issue of enabling phone number
            }
        }

        // Initialize file path
        private void InitializePaths()
        {

            // Get the directory of the current execution file
            string currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string targetDirectory = Path.Combine(currentDirectory, "Resources");

            // set up difficultyFolderPath for Games In the directory "Appearance and memory" Folders
            difficultyFolderPath = Path.Combine(targetDirectory, "Appearance and memory");

            picFolderPath = Path.Combine(difficultyFolderPath, "pic");
            //MessageBox.Show(picFolderPath);
            infoFolderPath = Path.Combine(difficultyFolderPath, "info");

            // Make sure the folder exists

            // Get all picture files（.png and .jpg）
            var pngFiles = Directory.GetFiles(picFolderPath, "*.png");
            var jpgFiles = Directory.GetFiles(picFolderPath, "*.jpg");
            allImages = pngFiles.Concat(jpgFiles).ToArray();
        }


        // Begin the memory stage
        private void StartMemoryStage()
        {
            if (difficultyLevel <= 6)
            {
                HandleDifficulty1To6(); // Difficulty of processing 1-6 logic
            }
            else if (difficultyLevel >= 7 && difficultyLevel <= 11)
            {
                HandleDifficulty7To11();// Difficulty of processing 7-Logic of 11

            }
            else if (difficultyLevel >= 12 && difficultyLevel <= 16)
            {
                HandleDifficulty12To16(); // Difficulty of processing 12-16 logic

            }
            else if (difficultyLevel >= 17 && difficultyLevel <= 21)
            {
                HandleDifficulty17To21();
            }
        }

        // Difficulty of processing 1-6 logic
        private void HandleDifficulty1To6()
        {
            DisplayRandomImages(); // Show random pictures for users to remember
            isMemoryStage = true; // Switch to memory stage
        }

        // Difficulty of processing 7-Logic of 11
        private void HandleDifficulty7To11()
        {
            int imagesToMemorize = difficultyLevel - 5;
            DisplayRandomImages(imagesToMemorize); // Show random pictures for users to remember
            nameOrNot = true;
            isMemoryStage = true; // Switch to memory stage
        }

        private void HandleDifficulty12To16()
        {
            int imagesToMemorize = difficultyLevel - 10; // The number of pictures is difficult-10
            DisplayRandomImages(imagesToMemorize);
            correctImagePaths.AddRange(memorizedImages); // Add the memorized image path to the correct path list
            nameOrNot = true;
            jobOrNot = true;
            isMemoryStage = true;
        }

        private void HandleDifficulty17To21()
        {
            int imagesToMemorize = difficultyLevel - 15; // The number of pictures is difficult-15
            DisplayRandomImages(imagesToMemorize);
            correctImagePaths.AddRange(memorizedImages); // Add the memorized image path to the correct path list
            nameOrNot = true;
            jobOrNot = true;
            numberOrNot = true;
            isMemoryStage = true;
        }

        // Displays a specified number of random images for user memory
        private void DisplayRandomImages(int imagesToMemorize = 0)
        {
            currenCorrectSelection = 0;
            isMemoryStage = true;

            StartOrStopSimulation(SingleKey, SingleKeyTime);
            timeTracker.StartMemoryTimer();
            imagesToMemorize = imagesToMemorize > 0 ? imagesToMemorize : difficultyLevel;

            if (imagesToMemorize > allImages.Length)
            {

                return;
            }

            // Use a dictionary to store each file name and their corresponding image path
            Dictionary<string, string> uniqueImages = new Dictionary<string, string>();
            Random random = new Random();

            // if repeatNum Greater than 0 And the difficulty has not changed, select from the last memory picture repeatNum A picture
            if (repeatNum > 0 && memorizedImages != null && memorizedImages.Length > 0)
            {
                // Randomly select from the last memory picture repeatNum A picture
                var repeatedImages = memorizedImages.OrderBy(x => random.Next()).Take(repeatNum);
                foreach (var image in repeatedImages)
                {
                    var fileName = Path.GetFileNameWithoutExtension(image).Split('-')[0];
                    if (!uniqueImages.ContainsKey(fileName))
                    {
                        uniqueImages[fileName] = image;
                    }
                }
            }

            // when repeatNum for 0 When making sure that the picture of the current memory stage is completely different from the previous memory stage
            List<string> remainingImages = new List<string>(allImages);
            if (repeatNum == 0 && memorizedImages != null && memorizedImages.Length > 0)
            {
                foreach (var image in memorizedImages)
                {
                    var fileName = Path.GetFileNameWithoutExtension(image).Split('-')[0];
                    // Remove all images with the same file name
                    remainingImages.RemoveAll(img => Path.GetFileNameWithoutExtension(img).Split('-')[0] == fileName);
                }
            }

            // Select the remaining number of pictures from the remaining pictures
            while (uniqueImages.Count < imagesToMemorize)
            {
                var randomImage = remainingImages[random.Next(remainingImages.Count)];
                var fileName = Path.GetFileNameWithoutExtension(randomImage).Split('-')[0];

                if (!uniqueImages.ContainsKey(fileName))
                {
                    uniqueImages[fileName] = randomImage;
                }
            }

            // Save the selected image to memorizedImages In the array
            memorizedImages = uniqueImages.Values.ToArray();
            LoadThumbnails(memorizedImages); // Show thumbnails
            LoadContent(memorizedImages[0]);




        }


        // Load and display the main image
        private void LoadContent(string imagePath)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
            bitmap.EndInit();

            imageControl.Source = bitmap;
            imageControl.HorizontalAlignment = HorizontalAlignment.Center;
            imageControl.VerticalAlignment = VerticalAlignment.Center;
        }

        // Clear the main picture area
        private void ClearMainImage()
        {
            imageControl.Source = null;
        }

        // Load thumbnails
        private void LoadThumbnails(string[] imagePaths)
        {
            thumbnailPanel.Children.Clear(); // Clear the current thumbnail panel

            for (int i = 0; i < imagePaths.Length; i++)
            {
                string imagePath = imagePaths[i];
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.DecodePixelWidth = 120; // Thumbnail width
                bitmap.EndInit();

                Image thumbnail = new Image
                {
                    Source = bitmap,
                    Width = 120,  // Thumbnail width
                    Height = 120, // Thumbnail height
                    Margin = new Thickness(2) // Reduce the margins of thumbnails
                };

                // Add click events for each thumbnail
                thumbnail.MouseLeftButtonUp += (s, e) => Thumbnail_Click(imagePath, thumbnail);

                Border border = new Border
                {
                    Child = thumbnail,
                    Padding = new Thickness(0), // Set the border to 0 to make the border fit more
                };
                thumbnailPanel.Children.Add(border); // Add to panel
            }

            // The first image is selected and loaded by default
            if (imagePaths.Length > 0)
            {
                selectedIndex = 0; // The first thumbnail is selected by default
                SelectThumbnailByIndex(selectedIndex);
                LoadContent(imagePaths[0]); // Show the first picture
            }
        }


        // Thumbnail click event processing     
        private void Thumbnail_Click(string imagePath, Image thumbnail)
        {
            LoadContent(imagePath); // Load the main picture
            HighlightThumbnail(imagePath); // Highlight selected thumbnails
            selectedImagePath = imagePath; // Record the selected image path

            // If it is difficult to 7 or above, it will be displayedtxtcontent
            if (difficultyLevel >= 7)
            {
                string infoFilePath = Path.Combine(infoFolderPath, Path.GetFileNameWithoutExtension(imagePath).Split('-')[0] + ".txt");

                if (File.Exists(infoFilePath) && isMemoryStage)
                {
                    string[] infoLines = File.ReadAllLines(infoFilePath);

                    // Display different row counts according to difficulty level
                    if (difficultyLevel >= 7 && difficultyLevel <= 11)
                    {
                        // Show first line
                        InfoText1.Text = infoLines.Length > 0 ? infoLines[0] : "";
                    }
                    else if (difficultyLevel >= 12 && difficultyLevel <= 16)
                    {
                        // Show first and second rows
                        InfoText1.Text = string.Join(Environment.NewLine, infoLines.Take(2));
                    }
                    else if (difficultyLevel >= 17 && difficultyLevel <= 21)
                    {
                        // Show the first three lines
                        InfoText1.Text = string.Join(Environment.NewLine, infoLines.Take(3));
                    }
                    else
                    {
                        InfoText1.Text = ""; // Out of range, no content is displayed
                    }
                }
                else
                {
                    InfoText1.Text = ""; // The file does not exist or is not in the memory stage, clear the display content
                }
            }
            else
            {
                InfoText1.Text = ""; // If the difficulty is less than 7, no information will be displayed
            }
        }


        // Highlight selected thumbnails
        private void HighlightThumbnail(string imagePath)
        {
            if (selectedThumbnailBorder != null)
            {
                selectedThumbnailBorder.BorderThickness = new Thickness(0); // Remove the highlight of the previous thumbnail
            }

            foreach (UIElement element in thumbnailPanel.Children)
            {
                if (element is Border border && border.Child is Image img && img.Source is BitmapImage bitmap && bitmap.UriSource.ToString() == new Uri(imagePath, UriKind.Absolute).ToString())
                {
                    border.BorderBrush = Brushes.Red;
                    border.BorderThickness = new Thickness(1.5); // Thinner red border
                    border.Padding = new Thickness(0); // Cancel the fill of borders

                    // Adjust the width and height of the red frame to make it more suitable for the picture
                    border.Height = img.Height - 40;
                    border.Width = img.Width + 2;

                    border.VerticalAlignment = VerticalAlignment.Center; // Align the border vertically with the picture
                    border.HorizontalAlignment = HorizontalAlignment.Center; // Align horizontally centered

                    selectedThumbnailBorder = border;
                    break;
                }
            }
        }





        //Selection stage display image
        private void DisplaySelectionImages(bool sameOrNot)
        {
            StartOrStopSimulation(SingleKey, SingleKeyTime);
            memoryNum++;
            timeTracker.StopMemoryTimer();
            timeTracker.StartSelectionTimer();
            int imagesToMemorize = 0;
            if (difficultyLevel >= 17 && difficultyLevel <= 21)
            {
                imagesToMemorize = difficultyLevel - 15;
            }
            else if (difficultyLevel > 11 && difficultyLevel <= 16)
            {
                imagesToMemorize = difficultyLevel - 10; // Difficulty 12-At 16 o'clock, the number of pictures I remember is 2-6
            }
            else if (difficultyLevel > 6 && difficultyLevel <= 11)
            {
                imagesToMemorize = difficultyLevel - 5; // Difficulty 7-At 11 o'clock, the number of pictures I remember is 2-6
            }
            else if (difficultyLevel <= 6)
            {
                imagesToMemorize = difficultyLevel; // Difficulty 1-At 6 o'clock, the number of pictures memorized is equal to the difficulty level
            }

            // Used to store selected image paths and file names
            List<string> selectedImages = new List<string>();
            Random random = new Random();

            // File name used to store memory pictures（Excluding serial number）
            HashSet<string> memorizedFileNames = new HashSet<string>();

            // Make sure to select the picture containing the memory
            foreach (var image in memorizedImages.Take(imagesToMemorize))
            {
                string fileName = Path.GetFileNameWithoutExtension(image).Split('-')[0];
                memorizedFileNames.Add(fileName);
                selectedImages.Add(image);
            }

            if (!sameOrNot)
            {
                // The pictures that replace the memory picture are pictures with different serial numbers of the same file name
                Dictionary<string, string> fileNameToImageMap = new Dictionary<string, string>();
                foreach (var image in memorizedImages.Take(imagesToMemorize))
                {
                    string fileName = Path.GetFileNameWithoutExtension(image).Split('-')[0];
                    fileNameToImageMap[fileName] = image;
                }

                selectedImages = selectedImages.Except(fileNameToImageMap.Values).ToList(); // Remove original memory picture

                // Replace with pictures with different serial numbers
                foreach (var fileName in fileNameToImageMap.Keys.ToList())
                {
                    var matchingImages = allImages
                        .Where(img => Path.GetFileNameWithoutExtension(img).StartsWith(fileName) &&
                                      img != fileNameToImageMap[fileName])
                        .ToArray();

                    if (matchingImages.Length > 0)
                    {
                        selectedImages.Add(matchingImages[random.Next(matchingImages.Length)]);
                    }
                }
            }

            // Make sure that only one serial number appears for each file name of the selected image
            Dictionary<string, string> uniqueFileNames = new Dictionary<string, string>();
            foreach (var image in selectedImages)
            {
                string fileName = Path.GetFileNameWithoutExtension(image).Split('-')[0];
                if (!uniqueFileNames.ContainsKey(fileName))
                {
                    uniqueFileNames[fileName] = image;
                }
            }

            // Make sure that there are no more than 7 selected images
            while (uniqueFileNames.Count < 7)
            {
                string randomImage = allImages[random.Next(allImages.Length)];
                string fileName = Path.GetFileNameWithoutExtension(randomImage).Split('-')[0];
                if (!uniqueFileNames.ContainsKey(fileName))
                {
                    uniqueFileNames[fileName] = randomImage;
                }
            }

            // Convert dictionary to array and randomly mess
            string[] finalImages = uniqueFileNames.Values.ToArray();
            finalImages = finalImages.OrderBy(x => random.Next()).ToArray();
            selectedImagePath = finalImages[0];
            // Show thumbnails
            LoadThumbnails(finalImages);

            // Clear main picture
            ClearMainImage();
        }

        public Appearance and memory()
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




        // Continue button click event processing
        private void PressContinue_Button(object sender, RoutedEventArgs e)
        {
            if (InfoText3.Text.Contains("Please find out the corresponding characters' pictures of the memory stage information."))
            {
                totalNum[difficultyLevel - 1]++;
            }
            if (isMemoryStage)
            {
                // The memory stage ends and enters the selection stage
                DisplaySelectionImages(sameOrNot); // Show pictures of the selection phase
                LoadContent(selectedImagePath);
                InfoText1.Text = "";
                InfoText2.Text = "Select the picture you remembered";
                InfoText3.Text = "Please find out the corresponding characters' pictures of the memory stage information.";
                Button1.Content = "Confirm selection";
                isMemoryStage = false; // Switch to the selection phase
                userSelections = 0; // Reset user selections
                correctSelections = 0; // Reset the correct selection                                      
                                       //selectedImagePath = null;// Reset selectedImagePath for null

                // Called only when the difficulty is greater than 6 AskNextQuestion()
                if (difficultyLevel >= 7)
                {
                    AskNextQuestion(); // Enter the questioning stage
                }
            }
            else
            {
                HandleSelection(); // Logic for processing selection phase
            }
        }

        // Get the correct image path
        private string GetCorrectImagePath()
        {
            // Return the correct image path according to the type of problem
            if (InfoText2.Text.Contains("Who is") || InfoText2.Text.Contains("Whose number is"))
            {
                // Returns the image path of the information corresponding to the current problem
                return memorizedImages[currentQuestionIndex];
            }

            // If the problem type is"Whose profession is", does not return the path, but returnsnull
            if (InfoText2.Text.Contains("Whose profession is"))
            {
                return null;
            }

            // If the problem type does not match, returnnull
            return null;
        }

        // Handle user selection phase

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
        private async void HandleSelection()
        {
            // Number of questions set according to difficulty level
            requiredSelections = 0;
            if (difficultyLevel <= 6)
            {
                requiredSelections = difficultyLevel;
            }
            else if (difficultyLevel >= 7 && difficultyLevel <= 11)
            {
                requiredSelections = difficultyLevel - 5;
            }
            else if (difficultyLevel >= 12 && difficultyLevel <= 16)
            {
                requiredSelections = (difficultyLevel - 10) * 2;
            }
            else if (difficultyLevel >= 17 && difficultyLevel <= 21)
            {
                requiredSelections = (difficultyLevel - 15) * 3;
            }

            if (selectedImagePath == null)
            {
                MessageBox.Show("Please select a picture！");
                return;
            }

            bool isCorrect = false;

            if (InfoText2.Text.Contains("Select the picture you remembered"))
            {
                isCorrect = memorizedImages.Any(image => Path.GetFileName(image).Split('-')[0] == Path.GetFileName(selectedImagePath).Split('-')[0]);
            }
            else if (InfoText2.Text.Contains("Whose profession is"))
            {
                // Judging the correctness based on occupational information
                string selectedFileName = Path.GetFileNameWithoutExtension(selectedImagePath).Split('-')[0];
                string selectedInfoFilePath = Path.Combine(infoFolderPath, selectedFileName + ".txt");

                if (File.Exists(selectedInfoFilePath))
                {
                    string[] selectedInfoLines = File.ReadAllLines(selectedInfoFilePath);
                    string selectedJob = selectedInfoLines.Length > 1 ? selectedInfoLines[1] : "Unknown occupation";
                    string correctJob = InfoText2.Text.Replace("Whose profession is ", "").Replace("?", "").Trim();

                    isCorrect = selectedJob.Equals(correctJob, StringComparison.OrdinalIgnoreCase);
                }
            }
            else
            {
                isCorrect = Path.GetFileName(selectedImagePath).Split('-')[0].Equals(Path.GetFileName(correctImagePath).Split('-')[0], StringComparison.OrdinalIgnoreCase);
            }

            if (isCorrect)
            {
                //InfoText4.Text = "correct";
                string currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string targetDirectory = Path.Combine(currentDirectory, "Resources");
                // "Appearance and memory" Folders
                string FolderPath = Path.Combine(targetDirectory, "Appearance and memory");
                string picPath = Path.Combine(FolderPath, "correct.png");
                if (imageOrNot) LoadImage(picPath);
                if (soundOrNot) PlayWav(CorrectSoundPath);
                await Task.Delay(1500);

                InfoText4.Foreground = Brushes.Green;
                correctSelections++;
                currenCorrectSelection++;
                correctCountsByDifficulty[difficultyLevel]++; // Update the correct number of times the current difficulty level is
            }
            else
            {
                errorNum[difficultyLevel - 1]++;
                errorCountsByDifficulty[difficultyLevel]++; // Number of errors to update the current difficulty level

                if (soundOrNot == true)
                {
                    PlayMemoryAudio();
                }

                if (InfoText2.Text.Contains("Whose profession is"))
                {
                    errorJobNum[difficultyLevel - 1]++;
                }
                else if (InfoText2.Text.Contains("Who is"))
                {
                    errorNameNum[difficultyLevel - 1]++;
                }
                else if (InfoText2.Text.Contains("Whose number is"))
                {
                    errorPhoneNum[difficultyLevel - 1]++;
                }
                else
                {
                    errorPicNum[difficultyLevel - 1]++;
                }
                //InfoText4.Text = "mistake";
                string currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string targetDirectory = Path.Combine(currentDirectory, "Resources");
                // "Appearance and memory" Folders
                string FolderPath = Path.Combine(targetDirectory, "Appearance and memory");
                string picPath = Path.Combine(FolderPath, "error.png");

                if (imageOrNot) LoadImage(picPath);
                if (soundOrNot) PlayWav(ErrorSoundPath);
                await Task.Delay(1500);
            }

            userSelections++;

            if (userSelections >= requiredSelections)
            {
                ShowAccuracy(); // Display correctness
                ResetToMemoryStage(); // Reset to memory stage
            }
            else
            {
                AskNextQuestion(); // Continue to ask the next question
            }
        }


        private class Question
        {
            public string ImagePath { get; set; }
            public string QuestionText { get; set; }
        }
        private void AskNextQuestion()
        {
            // Generate and disrupt all questions before the first question
            if (currentQuestionIndex == 0)
            {
                GenerateAllQuestions(); // Generate corresponding number of questions based on difficulty level
                ShuffleQuestions(); // Disrupt the order of problems
            }

            // make sure currentQuestionIndex Within the valid range
            if (currentQuestionIndex < allQuestions.Count)
            {
                // Select the next random question
                var currentQuestion = allQuestions[currentQuestionIndex];
                InfoText2.Text = currentQuestion.QuestionText;
                correctImagePath = currentQuestion.ImagePath;

                currentQuestionIndex++;
            }
            else
            {
                ShowAccuracy(); // Display correctness
                ResetToMemoryStage(); // Reset to memory stage
            }
        }

        private void GenerateAllQuestions()
        {
            allQuestions.Clear(); // Clear the last question list

            int questionsToGenerate = difficultyLevel; // Generate number of questions based on difficulty level

            // If the current difficulty level is 1-6. Generate corresponding number of questions
            if (difficultyLevel <= 6)
            {
                for (int i = 0; i < questionsToGenerate; i++)
                {
                    if (i < memorizedImages.Length) // Make sure that the number of memory pictures will not exceed
                    {
                        string imagePath = memorizedImages[i];
                        allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = "Select the picture you remembered" });
                    }
                }
            }
            else
            {
                // When the difficulty level is greater than 6, continue to use existing logic
                for (int i = 0; i < memorizedImages.Length; i++)
                {
                    string imagePath = memorizedImages[i];
                    string infoFilePath = Path.Combine(infoFolderPath, Path.GetFileNameWithoutExtension(imagePath).Split('-')[0] + ".txt");

                    if (File.Exists(infoFilePath))
                    {
                        string[] infoLines = File.ReadAllLines(infoFilePath);
                        string name = infoLines.Length > 0 ? infoLines[0] : "Unknown name";
                        string job = infoLines.Length > 1 ? infoLines[1] : "Unknown occupation";
                        string number = infoLines.Length > 2 ? infoLines[2] : "Unknown number";
                        if (difficultyLevel >= 7 && difficultyLevel < 12)
                        {
                            if (nameOnly)
                            {
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = $"Who is {name}?" });
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = "Select the picture you remembered" });
                            }
                            if (infoOnly)
                            {
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = "Select the picture you remembered" });
                            }
                            if (nameAndInfo)
                            {
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = $"Who is {name}?" });
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = "Select the picture you remembered" });
                            }

                        }
                        if (difficultyLevel >= 12 && difficultyLevel < 17)
                        {
                            if (nameOnly)
                            {
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = $"Who is {name}?" });
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = "Select the picture you remembered" });
                            }
                            if (infoOnly)
                            {
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = $"Whose profession is {job}?" });
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = "Select the picture you remembered" });
                            }
                            if (nameAndInfo)
                            {
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = $"Who is {name}?" });
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = $"Whose profession is {job}?" });
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = "Select the picture you remembered" });
                            }

                        }
                        if (difficultyLevel >= 17 && difficultyLevel <= 21)
                        {
                            if (nameOnly)
                            {
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = $"Who is {name}?" });
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = "Select the picture you remembered" });
                            }
                            if (infoOnly)
                            {
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = $"Whose profession is {job}?" });
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = $"Whose number is {number}?" });
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = "Select the picture you remembered" });
                            }
                            if (nameAndInfo)
                            {
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = $"Who is {name}?" });
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = $"Whose profession is {job}?" });
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = $"Whose number is {number}?" });
                                allQuestions.Add(new Question { ImagePath = imagePath, QuestionText = "Select the picture you remembered" });
                            }

                        }


                    }
                }
            }
        }


        private void ShuffleQuestions()
        {
            // Use random algorithm to disrupt the problem list
            Random rng = new Random();
            int n = allQuestions.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                var value = allQuestions[k];
                allQuestions[k] = allQuestions[n];
                allQuestions[n] = value;
            }
        }


        // Display correctness(Evaluation indicators)



        private void ShowAccuracy()
        {
            // Check that all the current games are correct
            if (currenCorrectSelection == requiredSelections)
            {
                correctStreak++;
                // wrongStreak = 0; // Reset error count
                totalCorrectSelections++;
                lastFiveGamesResults.Add(1); // Correctly once, store 1
            }
            else
            {
                wrongStreak++;
                // correctStreak = 0; // Reset the correct count
                lastFiveGamesResults_2.Add(1); // Error once, store 1
            }

            // Guaranteed to store only the last five results
            if (lastFiveGamesResults.Count > change_up)
            {
                lastFiveGamesResults.RemoveAt(0);
            }
            if (lastFiveGamesResults_2.Count > change_down)
            {
                lastFiveGamesResults_2.RemoveAt(0);
            }

            // Determine whether adjustments are required
            if (correctStreak >= change_up && difficultyLevel < 21)
            {
                difficultyLevel++;
                ResetStreaks(); // Reset the counter
                RenewHighestLevel(difficultyLevel);
            }
            else if (wrongStreak >= change_down)
            {
                if (difficultyLevel > 1)
                {
                    difficultyLevel--;
                    RenewHighestLevel(difficultyLevel);

                    // Adjust flag variables according to specific difficulty
                    switch (difficultyLevel + 1) // Check the original difficulty
                    {
                        case 7:
                            nameOrNot = false;
                            break;
                        case 12:
                            jobOrNot = false;
                            break;
                        case 17:
                            numberOrNot = false;
                            break;
                    }
                }

                ResetStreaks(); // Reset the counter
            }

            // Trigger statistics update
            RightStatisticsAction?.Invoke(lastFiveGamesResults.Sum(), change_up);
            WrongStatisticsAction?.Invoke(lastFiveGamesResults_2.Sum(), change_down);
            LevelStatisticsAction?.Invoke(difficultyLevel, 20);
        }

        // Reset the continuous counter
        private void ResetStreaks()
        {
            correctStreak = 0;
            wrongStreak = 0;
            lastFiveGamesResults.Clear();
            lastFiveGamesResults_2.Clear();

        }

        // Reset to memory stage
        private void ResetToMemoryStage()
        {
            timeTracker.StopSelectionTimer();
            //InfoText1.Text = "";
            InfoText2.Text = "";
            InfoText3.Text = "Please remember the following character image information";

            // Create a DispatcherTimer
            clearInfoTextTimer = new DispatcherTimer();
            clearInfoTextTimer.Interval = TimeSpan.FromSeconds(0); // 1 second interval

            // Define the timer Tick Event handler
            clearInfoTextTimer.Tick += (s, args) =>
            {
                InfoText4.Text = ""; // Clear the text in 1 second
                CorrectOrNot.Source = null;
                clearInfoTextTimer.Stop(); // Stop the timer
            };

            // Start the timer
            clearInfoTextTimer.Start();

            Button1.Content = "Memory completion";
            selectedImagePath = null;
            currentQuestionIndex = 0; // Reset the issue index
            StartMemoryStage(); // Start the memory phase again
        }
        private void StopAllTimers()
        {

            // Stop the total timer
            timer?.Stop();

            //
            trainingTimer?.Stop();

            // Stop clearing the text timer
            clearInfoTextTimer?.Stop();

            // Stop the current question timer（If so）
            //timeTracker.StopMemoryTimer(); // Stop memory stage timing
            //timeTracker.StopSelectionTimer(); // Stop selection phase timing
        }

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

    }
    public partial class Appearance and memory : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
            int trainTime; int upThreshold; int downThreshold; int repeat;
            bool nameO; bool infoO; bool infoOname; bool sameO; bool soundO; bool single; int keyTime;

            // The parameters passed by the client should be preceded by the client. Currently, test data is used first.
            {
                int time = 1;
                int change_up = 5;
                int change_down = 5;
                int repeatNum = 0;
                bool nameOnly = true;
                bool infoOnly = false;
                bool nameAndInfo = false;
                bool soundOrNot = true;
                bool sameOrNot = true;
                bool SingleKey = false;
                int SingleKeyTime = 5000;

                trainTime = time;
                upThreshold = change_up;
                downThreshold = change_down;
                repeat = repeatNum;
                nameO = nameOnly;
                infoO = infoOnly;
                infoOname = nameAndInfo;
                sameO = sameOrNot;
                soundO = soundOrNot;
                single = SingleKey;
                keyTime = SingleKeyTime;
            }
            time = trainTime;
            change_up = upThreshold;
            change_down = downThreshold;
            repeatNum = repeat;
            nameOnly = nameO;
            infoOnly = infoO;
            nameAndInfo = infoOname;
            sameOrNot = sameO;
            soundOrNot = soundO;
            SingleKey = single;
            SingleKeyTime = keyTime;
            //this.KeyDown += Window_KeyDown;
            // Initialize a count array of correct and wrong answers
            timeTracker = new TimeTracker();
            _keySimulator = new KeySimulator(thumbnailPanel, LoadContent, HighlightThumbnail);
            InitializeNum();
            HighestLevel = 1;

            SetDifficultyLevel(1); // The default difficulty level is 1   

            InitializePaths();

            /*LJN
            Configure the path to feedback resources             
             */
            CorrectSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");


            {
                // parameter（Includes module parameter information）
                var baseParameter = BaseParameter;

                if (baseParameter.ProgramModulePars != null && baseParameter.ProgramModulePars.Any())

                {
                    Debug.WriteLine("ProgramModulePars Loaded data:");

                    // Traversal ProgramModulePars Print out each parameter
                    foreach (var par in baseParameter.ProgramModulePars)
                    {
                        /*Debug.WriteLine($"ProgramId: {par.ProgramId},ModuleParId: {par.ModuleParId}, Value: {par.Value}, TableId: {par.TableId}");*/
                        if (par.ModulePar.ModuleId == baseParameter.ModuleId)
                        {
                            switch (par.ModuleParId)
                            {
                                case 179: // Difficulty
                                    difficultyLevel = par.Value.HasValue ? (int)par.Value.Value : 1;
                                    Debug.WriteLine($"HARDNESS: {difficultyLevel}");
                                    break;
                                case 125: // Treatment time
                                    time = par.Value.HasValue ? (int)par.Value.Value : 25;
                                    Debug.WriteLine($"time={time}");
                                    break;
                                case 126: // Level improvement
                                    change_up = par.Value.HasValue ? (int)par.Value.Value : 5;
                                    Debug.WriteLine($"change_up={change_up}");
                                    break;
                                case 127: // Level down
                                    change_down = par.Value.HasValue ? (int)par.Value.Value : 5;
                                    Debug.WriteLine($"change_down ={change_down}");
                                    break;
                                case 129: // Identification through association information
                                    infoOnly = par.Value == 1;
                                    Debug.WriteLine($"Whether it is identified through association information ={infoOnly}");
                                    break;
                                case 130: // Identification by name
                                    nameOnly = par.Value == 1;
                                    Debug.WriteLine($"Is it identified by name ={nameOnly}");
                                    break;
                                case 131: // Name or association information identification
                                    nameAndInfo = par.Value == 1;
                                    Debug.WriteLine($"Whether the name or association information is identified ={nameAndInfo}");
                                    break;
                                case 132: // Auditory feedback
                                    soundOrNot = par.Value == 1;
                                    Debug.WriteLine($"Whether to hear feedback ={soundOrNot}");
                                    break;
                                case 133: // Same picture
                                    sameOrNot = par.Value == 1;
                                    Debug.WriteLine($"Is the same picture ={sameOrNot}");
                                    break;
                                case 264://Visual feedback
                                    imageOrNot = par.Value == 1;
                                    Debug.WriteLine($"imageOrNot ={imageOrNot}");
                                    break;
                                // Add other things that need to be processed ModuleParId
                                default:
                                    Debug.WriteLine($"Unprocessed ModuleParId:{par.ModuleParId} ");
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("No data");
                }

                // Call delegate to display difficulty level, correct and error times
                LevelStatisticsAction?.Invoke(difficultyLevel, 21);
                RightStatisticsAction?.Invoke(0, change_up);
                WrongStatisticsAction?.Invoke(0, change_down);
            }



            train_time = time * 60; // Set the total training time
            remainingTime = TimeSpan.FromMinutes(time);
        }

        protected override async Task OnStartAsync()
        {

            timer = new System.Timers.Timer(1000); // Triggered once every second
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true; // make sure Timer Triggered every time
            timer.Enabled = true; // Start the timer
            timer.Start();

            trainingTimer?.Stop();
            trainingTimer = new DispatcherTimer();
            trainingTimer.Interval = TimeSpan.FromSeconds(1);
            trainingTimer.Tick += TrainingTimer_Tick;
            trainingTimer.Start(); // Start the training timer


            StartCountdown(time);
            StartMemoryStage(); // Begin the memory stage

            // Calling delegate
            VoiceTipAction?.Invoke("Please find out the appearance of the person you remember.");
            SynopsisAction?.Invoke("You will first see a person's appearance on the screen. Please remember its characteristics. After the memory is completed, click the mouse in the lower right corner.“Memory completion”button. Then a series of characters' appearance images appear. You need to distinguish which one is the person you just remembered based on the characteristics you remember, and click on the lower right corner with the mouse“Confirm selection”button.");
            RuleAction?.Invoke("You will first see a person's appearance on the screen. Please remember its characteristics. After the memory is completed, click the mouse in the lower right corner.“Memory completion”button. Then a series of characters' appearance images appear. You need to distinguish which one is the person you just remembered based on the characteristics you remember, and click on the lower right corner with the mouse“Confirm selection”button.");//Add code, call function, display the text under the digital person

        }

        protected override async Task OnStopAsync()
        {
            StopAllTimers();
            memorizeTime = timeTracker.totalMemoryTime / memoryNum;
        }

        protected override async Task OnPauseAsync()
        {
            StopAllTimers();
        }

        protected override async Task OnNextAsync()
        {
            // Adjust the difficulty
            //ShowAccuracy();
            ResetToMemoryStage();

            // Calling delegate
            VoiceTipAction?.Invoke("Please find out the appearance of the person you remember.");
            SynopsisAction?.Invoke("You will first see a person's appearance on the screen. Please remember its characteristics. After the memory is completed, click the mouse in the lower right corner.“Memory completion”button. Then a series of characters' appearance images appear. You need to distinguish which one is the person you just remembered based on the characteristics you remember, and click on the lower right corner with the mouse“Confirm selection”button.");
            RuleAction?.Invoke("You will first see a person's appearance on the screen. Please remember its characteristics. After the memory is completed, click the mouse in the lower right corner.“Memory completion”button. Then a series of characters' appearance images appear. You need to distinguish which one is the person you just remembered based on the characteristics you remember, and click on the lower right corner with the mouse“Confirm selection”button.");
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new Explanation of appearance and memory();
        }

        // Insert writing
        private int GeterrorPicNum()
        {
            return errorPicNum[difficultyLevel - 1];
        }
        private int GeterrorNameNum()
        {
            return errorNameNum[difficultyLevel - 1];
        }
        private int GeterrorJobNum(int difficultyLevel)
        {
            return errorJobNum[difficultyLevel - 1];
        }
        private int GeterrorPhoneNum(int difficultyLevel)
        {
            return errorPhoneNum[difficultyLevel - 1];
        }
        private double CalculateAccuracy(int errorPicNumCount, int errorNameNumCount, int errorJobNumCount, int errorPhoneNumCount)
        {
            int errorNumCount = errorPicNumCount + errorNameNumCount + errorJobNumCount + errorPhoneNumCount;
            int totalCount = totalNum[difficultyLevel - 1];
            return totalCount > 0 ? Math.Round((double)errorNumCount / totalCount, 2) : 0;
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
                        int errorPicNumCount = 0;
                        int errorNameNumCount = 0;
                        int errorJobNumCount = 0;
                        int errorPhoneNumCount = 0;
                        int errorNumCount = 0;
                        

                        //int errorPicNumCount = errorPicNum[difficultyLevel - 1];
                        //int errorNameNumCount = errorNameNum[difficultyLevel - 1];
                        //int errorJobNumCount = errorJobNum[difficultyLevel - 1];
                        //int errorPhoneNumCount = errorPhoneNum[difficultyLevel - 1];
                        //int errorNumCount = errorNum[difficultyLevel - 1];
                        //if (double.IsNaN(memorizeTime))
                        //{//memorizeTimeIt is possible forNaN, which cannot be recorded in the database, so it needs to be processed
                        //    memorizeTime = 0.0;
                        //}
                        //double time = Math.Round((double)memorizeTime, 2);
                        //double chotime = Math.Round((double)chooseTime, 2);
                        //if (errorPicNumCount == 0 && errorNameNumCount == 0 && errorJobNumCount == 0 && errorPhoneNumCount == 0)
                        //{
                        //    // If all data is 0, skip this difficulty level
                        //    Debug.WriteLine($" No data, skip.");
                        //}
                        //// Calculation accuracy
                        //double erroraccuracy = CalculateAccuracy(errorPicNumCount, errorNameNumCount, errorJobNumCount, errorPhoneNumCount);

                        for (int i = 1; i <= HighestLevel; i++)
                        {
                            errorPicNumCount += errorPicNum[difficultyLevel - 1];
                            errorNameNumCount += errorNameNum[difficultyLevel - 1];
                            errorJobNumCount += errorJobNum[difficultyLevel - 1];
                            errorPhoneNumCount += errorPhoneNum[difficultyLevel - 1];
                            errorNumCount += errorNum[difficultyLevel - 1];
                        }
                        if (double.IsNaN(memorizeTime))
                        {//memorizeTimeIt is possible forNaN, which cannot be recorded in the database, so it needs to be processed
                            memorizeTime = 0.0;
                        }
                        double time = Math.Round((double)memorizeTime, 2);
                        double chotime = Math.Round((double)chooseTime, 2);
                        // Calculation accuracy
                        double erroraccuracy = CalculateAccuracy(errorPicNumCount, errorNameNumCount, errorJobNumCount, errorPhoneNumCount);



                        // create Result Record
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "Appearance and memory",
                            Eval = false,
                            Lv = HighestLevel, // Current difficulty level
                            ScheduleId = BaseParameter.ScheduleId ?? null // Assumption Schedule_id, can be replaced with actual
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
                                      Value = HighestLevel,
                                      ModuleId = BaseParameter.ModuleId
                               },
                                new ResultDetail
                               {
                                      ResultId = result_id,
                                      ValueName = "All Errors",
                                      Value = errorNumCount,
                                    Maxvalue = errorNumCount,
                                    Minvalue = 0,
                                    Charttype = "Bar chart" ,
                                      ModuleId = BaseParameter.ModuleId
                               },
                                new ResultDetail
                               {
                                      ResultId = result_id,
                                      ValueName = "All Errors(%)",
                                      Value = erroraccuracy * 100,
                                      ModuleId = BaseParameter.ModuleId
                               },
                               new ResultDetail
                               {
                                      ResultId = result_id,
                                      ValueName = "Error picture",
                                      Value = errorPicNumCount,
                                      ModuleId = BaseParameter.ModuleId
                               },
                               new ResultDetail
                               {
                                      ResultId = result_id,
                                      ValueName = "Incorrect name",
                                      Value = errorNameNumCount,
                                      ModuleId = BaseParameter.ModuleId
                               },
                               new ResultDetail
                               {
                                      ResultId = result_id,
                                      ValueName = "Wrong occupation",
                                      Value = errorJobNumCount,
                                      ModuleId = BaseParameter.ModuleId
                               },
                               new ResultDetail
                               {
                                      ResultId = result_id,
                                      ValueName = "Error phone number",
                                      Value = errorPhoneNumCount,
                                      ModuleId = BaseParameter.ModuleId
                               },
                               new ResultDetail
                               {
                                      ResultId = result_id,
                                      ValueName = "Average memory time",
                                      Value = time,
                                   Maxvalue = (int?)time,
                                    Minvalue = 0,
                                    Charttype = "Bar chart" ,
                                      ModuleId = BaseParameter.ModuleId
                               },
                               new ResultDetail
                               {
                                      ResultId = result_id,
                                      ValueName = "Average selection time（s）",
                                      Value = chotime,
                                      ModuleId = BaseParameter.ModuleId
                               }


                        };

                        // insert ResultDetail data
                        db.ResultDetails.AddRange(resultDetails);
                        await db.SaveChangesAsync();
                        // Output each ResultDetail Object data
                        //Debug.WriteLine($"Difficulty level {lv}:");
                        foreach (var detail in resultDetails)
                        {
                            Debug.WriteLine($" {detail.ValueName}:{detail.Value}, ModuleId: {detail.ModuleId}");
                        }


                        // Submit transactions
                        await transaction.CommitAsync();
                        Debug.WriteLine("Insert successfully");
                    });
                }
                catch (Exception ex)
                {// Roll back transactions
                    await transaction.RollbackAsync();
                    Debug.WriteLine(ex.ToString());
                }
            }
        }
    }
}