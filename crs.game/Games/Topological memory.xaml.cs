using crs.core;
using crs.core.DbModels;
using crs.game.Games;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection.Emit;
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
using System.Windows.Threading;

namespace crs.game.Games
{
    /// <summary>
    /// MEMO.xaml Interaction logic
    /// </summary>
    // Difficulty data structure
    public class DifficultyLevel
    {
        public int Level { get; set; } // grade
        public int ImageCount { get; set; } // Number of pictures
        public int ImageContrast { get; set; } //Image contrast
        public int MaxErrors { get; set; } // Maximum error
        public int MaxMemoryTime { get; set; } // Maximum memory time

        // Added attributes
        public double AvgMemoryTime { get; set; } = 0; // Average memory time
        public double LongestMemoryTime { get; set; } = 0; // The longest memory time
        public double LongestAnswerTime { get; set; } = 0; // Maximum answer time

        // Question statistics
        public int TotalQuestions { get; set; } = 0; // Number of questions
        public int CorrectAnswers { get; set; } = 0; // Correct quantity
        public int WrongAnswers { get; set; } = 0; // Number of errors
    }

    // Training configuration class,Stopwatch and memory time are enabled by default
    public class TrainingConfig
    {
        public int TreatmentTime { get; set; } // Treatment time
        public int RepetitionCount { get; set; } // Number of repetitions
        public bool ShowClock { get; set; } = true;// Whether to display a stopwatch
        public bool LimitMemoryTime { get; set; } = true; // Is it limited memory time
        public bool AutoTurn { get; set; } = false;
        public bool LimitAnswerTime { get; set; } = false; // Is the answer time limit
        public bool is_beep { get; set; } = true;
        public bool is_visual { get; set; } = true;
        public ImageMaterial SelectedMaterial { get; set; } // Selected picture materials

        public TrainingConfig(int T, int R, bool S = true, bool L = true)
        {
            this.TreatmentTime = T;
            this.RepetitionCount = R;
            if (S) { this.ShowClock = L; }
            if (L)
            {
                this.LimitMemoryTime = L;
            }
            this.SelectedMaterial = new ImageMaterial();
        }
    }

    // Picture Materials
    public class ImageMaterial
    {
        public string Name { get; set; } // Image collection name
        public List<string> ImagePaths { get; set; } // Picture list
        public bool IsCustom { get; set; } // Whether it is customized for users

        public ImageMaterial() // defaultgeneral
        {
            Name = "generally";
            IsCustom = false;

            // Get the current execution directory and point to the superior directory, return to the project home directory
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var projectPath = Path.Combine(basePath, "Resources", "MEMO", "general");
            projectPath = Path.GetFullPath(projectPath); // Get the absolute path
            ImagePaths = LoadImagesFromFolder(projectPath); // Loading pictures
        }


        private List<string> LoadImagesFromFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"The directory {folderPath} does not exist.");
            }

            // Supported image formats
            string[] supportedExtensions = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp" };

            List<string> imagePaths = new List<string>();

            // Load all supported image files from folder
            foreach (var extension in supportedExtensions)
            {
                var files = Directory.GetFiles(folderPath, extension);
                imagePaths.AddRange(files);
            }

            return imagePaths;
        }
    }

    // Difficulty setting management
    public class DifficultyManager
    {
        public List<DifficultyLevel> DifficultyLevels { get; private set; }

        public DifficultyManager()
        {
            DifficultyLevels = new List<DifficultyLevel>
        {
                new DifficultyLevel { Level = 1, ImageCount = 3, ImageContrast = 1, MaxErrors = 0, MaxMemoryTime = 60 },
                new DifficultyLevel { Level = 2, ImageCount = 4, ImageContrast = 1, MaxErrors = 2, MaxMemoryTime = 60 },
                new DifficultyLevel { Level = 3, ImageCount = 5, ImageContrast = 1, MaxErrors = 2, MaxMemoryTime = 60 },
                new DifficultyLevel { Level = 4, ImageCount = 6, ImageContrast = 1, MaxErrors = 2, MaxMemoryTime = 60 },
                new DifficultyLevel { Level = 5, ImageCount = 7, ImageContrast = 2, MaxErrors = 3, MaxMemoryTime = 60 },
                new DifficultyLevel { Level = 6, ImageCount = 8, ImageContrast = 2, MaxErrors = 4, MaxMemoryTime = 60 },
                new DifficultyLevel { Level = 7, ImageCount = 8, ImageContrast = 3, MaxErrors = 4, MaxMemoryTime = 60 },
                new DifficultyLevel { Level = 8, ImageCount = 9, ImageContrast = 2, MaxErrors = 4, MaxMemoryTime = 60 },
                new DifficultyLevel { Level = 9, ImageCount = 9, ImageContrast = 3, MaxErrors = 4, MaxMemoryTime = 60 },
                new DifficultyLevel { Level = 10, ImageCount = 10, ImageContrast = 2, MaxErrors = 4, MaxMemoryTime = 60 },
                new DifficultyLevel { Level = 11, ImageCount = 10, ImageContrast = 3, MaxErrors = 4, MaxMemoryTime = 60 },
                new DifficultyLevel { Level = 12, ImageCount = 12, ImageContrast = 2, MaxErrors = 5, MaxMemoryTime = 60 },
                new DifficultyLevel { Level = 13, ImageCount = 12, ImageContrast = 3, MaxErrors = 5, MaxMemoryTime = 60 },
                new DifficultyLevel { Level = 14, ImageCount = 12, ImageContrast = 4, MaxErrors = 5, MaxMemoryTime = 60 },
                new DifficultyLevel { Level = 15, ImageCount = 14, ImageContrast = 2, MaxErrors = 6, MaxMemoryTime = 60 },
                new DifficultyLevel { Level = 16, ImageCount = 14, ImageContrast = 3, MaxErrors = 6, MaxMemoryTime = 60 },
                new DifficultyLevel { Level = 17, ImageCount = 14, ImageContrast = 4, MaxErrors = 6, MaxMemoryTime = 60},
                new DifficultyLevel { Level = 18, ImageCount = 16, ImageContrast = 2, MaxErrors = 6, MaxMemoryTime = 60 },
                new DifficultyLevel { Level = 19, ImageCount = 16, ImageContrast = 3, MaxErrors = 6, MaxMemoryTime = 60 },
                new DifficultyLevel { Level = 20, ImageCount = 16, ImageContrast = 4, MaxErrors = 6, MaxMemoryTime = 60 }
        };
        }

        public DifficultyLevel GetDifficultyLevel(int level) // Access functions
        {
            return DifficultyLevels.FirstOrDefault(d => d.Level == level);
        }
    }

    // Main interface
    public partial class Topological memory : BaseUserControl
    {

        private TimeSpan _trainTime; // Total training time
        private TimeSpan _remainingTime; // Current question time
        private DispatcherTimer _updateTimer; // New timer
        private DispatcherTimer _windowTimer; // Total timer
        private TrainingConfig _trainingConfig; // Training parameters
        private DifficultyManager _difficultyManager; // Difficulty parameters
        private DifficultyLevel _currentDifficulty; // Current difficulty and parameters
        private int _level;
        private int _currentRound; // Number of training rounds
        private DispatcherTimer _memoryTimer; // Memory stage timer
        private string _targetImagePath; // Target image path
        private Queue<bool> _recentResults = new Queue<bool>(5); // Record the last 5 game results
        private int _totalCountdown; //Total countdown
        private bool is_beep = true; // Beep
        private DateTime _startAnswerTime;

        // Material reuse
        private List<string> _lastSelectedImages = null;

        // Real-time correct or wrong
        private bool _wasLastAnswerCorrect = true; // 

        // Output indicators
        private int _totalQuestions;
        private int _correctAnswers;
        private int _wrongAnswers;
        private int _timeoutCount;
        private DateTime _startTime;
        private int _treatmentTimeInSeconds;
        private int _repeat = 5;

        // Keyboard operation
        private int _selectedButtonIndex = -1; // Index of currently selected button
        private List<Button> _buttons = new List<Button>(); // keepPatternGridButtons in

        private bool AnswerState = false;   // false Represents memories, trueRepresentative answer

        public Topological memory()
        {
            this.Loaded += OnLoaded;
            this.KeyDown += OnKeyDown;
            this.PreviewKeyDown += OnPreviewKeyDown;

            InitializeComponent();


        }


        private void InitializeNextButton()
        {
            if (_trainingConfig.AutoTurn)
            {
                NextButton.Visibility = Visibility.Collapsed; // Hide when jumping automatically
                NextButton.IsEnabled = false; // Disable clicks
                ReadyButton.IsEnabled = true;
            }
            else
            {
                NextButton.Visibility = Visibility.Visible; // Displayed when non-automatically jumped
                NextButton.IsEnabled = false; // Disabled by default, enable it when the answer is finished
                ReadyButton.IsEnabled = true;
            }
        }



        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Initialize button collection
            _buttons = PatternGrid.Children.OfType<Button>().ToList();
            _selectedButtonIndex = -1;
            UpdateSelectedButton();
            InitializeNextButton();
            // Set focus to window to prevent PatternGrid Or other controls take the focus
            Keyboard.Focus(this);
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Default focus navigation logic for shielded direction keys
            if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down)
            {
                e.Handled = false; // Block default behavior
            }
        }


        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            // Removal KeyDown Event Subscription
            var window = GetTopLevelWindow(this);
            if (window != null)
            {
                window.KeyDown -= OnKeyDown;
            }
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
        private void OnUpdateTimerTick(object sender, EventArgs e)
        {
            _totalCountdown--; // This is the overall time count

            // Countdown to the memory stage
            if (!AnswerState && _trainingConfig.LimitMemoryTime && _remainingTime.TotalSeconds > 0)
            {
                _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(1));
                if (_remainingTime.TotalSeconds <= 0)
                {
                    StartRecallPhase();
                }
            }
            // Countdown to answering stage
            else if (AnswerState && _trainingConfig.LimitAnswerTime && _remainingTime.TotalSeconds > 0)
            {
                _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(1));
                if (_remainingTime.TotalSeconds <= 0)
                {
                    // Timeout processing
                    _wrongAnswers++;
                    _currentDifficulty.WrongAnswers++;
                    StartNextRound();
                }
            }

            // Trigger statistics update event
            //TimeStatisticsAction?.Invoke((int?)_trainTime.TotalSeconds, (int?)_remainingTime.TotalSeconds);
            TimeStatisticsAction?.Invoke(_totalCountdown, (int?)_remainingTime.TotalSeconds);
        }
        private void InitializeUpdateTimer()
        {
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromSeconds(1); // Updated once every second
            _updateTimer.Tick += OnUpdateTimerTick;
        }
        // structureTimers
        private void InitializeWindowTimer(TimeSpan totalDuration)
        {
            _windowTimer = new DispatcherTimer();
            _windowTimer.Interval = totalDuration;
            _windowTimer.Tick += OnWindowTimerTick;
            _windowTimer.Start(); // Start the total timer
        }

        private void InitializeTimers()
        {
            _memoryTimer = new DispatcherTimer();
            _memoryTimer.Interval = TimeSpan.FromSeconds(_trainingConfig?.LimitMemoryTime == true ? _currentDifficulty.MaxMemoryTime : 3600); // Is it time limit? ? ? ?
            _memoryTimer.Tick += OnMemoryTimerTick;
        }


        private void OnWindowTimerTick(object sender, EventArgs e)
        {
            // Stop all timers
            _updateTimer?.Stop();
            _memoryTimer?.Stop();
            _windowTimer?.Stop();
            OnGameEnd();
        }
        // Level setting function
        public void SetDifficulty(int level)
        {
            _currentDifficulty = _difficultyManager.GetDifficultyLevel(level);
        }
        // Start the test
        private void OnStartButtonClick()
        {
            _startTime = DateTime.Now;
            _currentRound = 0;
            _remainingTime = TimeSpan.FromSeconds(_currentDifficulty.MaxMemoryTime); // Reset remaining memory time
            _updateTimer.Start(); // Start the update timer
            StartNextRound();
        }

        private void StartNextRound()
        {
            AnswerState = false;
            int correctCount = _recentResults.Count(result => result);
            int wrongCount = _recentResults.Count(result => !result);
            // Determine whether adjustments are required
            if (_recentResults.Count == 5)
            {
                // correctCount >= _trainingConfig.RepetitionCount
                if (correctCount >= 5)
                {
                    IncreaseDifficulty();
                }
                else if (wrongCount >= 5)
                {
                    DecreaseDifficulty();
                }
                _recentResults.Clear();
                correctCount = 0;
                wrongCount = 0;
            }

            if ((DateTime.Now - _startTime).TotalSeconds < _treatmentTimeInSeconds)
            {
                StartMemoryPhase();
            }
            else
            {
                OutputResultsGrid();
            }

            _remainingTime = TimeSpan.FromSeconds(_currentDifficulty.MaxMemoryTime); // Reset remaining memory time

            LevelStatisticsAction?.Invoke(_currentDifficulty.Level, _difficultyManager.DifficultyLevels.Count);
            RightStatisticsAction?.Invoke(correctCount, 5);
            WrongStatisticsAction?.Invoke(wrongCount, 5);
        }

        private void IncreaseDifficulty()
        {
            int currentLevel = _currentDifficulty.Level;
            if (currentLevel < _difficultyManager.DifficultyLevels.Count)
            {
                SetDifficulty(currentLevel + 1);

            }
        }


        private void DecreaseDifficulty()
        {
            int currentLevel = _currentDifficulty.Level;
            if (currentLevel > 1)
            {
                SetDifficulty(currentLevel - 1);

            }

        }

        private void UpdateSelectedButton()
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                var button = _buttons[i];

                // Set background and border styles
                button.Background = i == _selectedButtonIndex ? Brushes.LightBlue : Brushes.Transparent;
                button.BorderBrush = i == _selectedButtonIndex ? Brushes.Blue : Brushes.Transparent;
                button.BorderThickness = new Thickness(i == _selectedButtonIndex ? 2 : 0);
                button.InvalidateVisual();
            }

        }


        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (_buttons == null || !_buttons.Any()) return;

            e.Handled = true; // Prevent further spread of events

            switch (e.Key)
            {
                case Key.Left:
                    if (AnswerState == true)
                    {
                        _selectedButtonIndex = (_selectedButtonIndex - 1 + _buttons.Count) % _buttons.Count;
                        UpdateSelectedButton(); // Update the selected button style
                    }
                    break;
                case Key.Right:
                    if (AnswerState == true)
                    {
                        _selectedButtonIndex = (_selectedButtonIndex + 1) % _buttons.Count;
                        UpdateSelectedButton(); // Update the selected button style
                    }
                    break;
                case Key.Up:
                    if (AnswerState == true && GetButtonsPerRow() > 1)
                    {
                        _selectedButtonIndex = Math.Max(0, _selectedButtonIndex - 4);
                        UpdateSelectedButton(); // Update the selected button style
                    }
                    break;
                case Key.Down:
                    if (AnswerState == true && GetButtonsPerRow() > 1)
                    {
                        _selectedButtonIndex = Math.Min(_buttons.Count - 1, _selectedButtonIndex + 4);
                        UpdateSelectedButton(); // Update the selected button style
                    }
                    break;
                case Key.Enter:
                    if (_selectedButtonIndex >= 0 && _selectedButtonIndex < _buttons.Count && AnswerState == true)
                    {
                        var selectedButton = _buttons[_selectedButtonIndex];
                        if (selectedButton.IsEnabled) // Check if the button is enabled
                        {
                            OnImageButtonClick(selectedButton, null); // Call mouse click logic
                        }
                        break;
                    }
                    if (AnswerState == false && NextButton.IsEnabled == false)
                        OnReadyButtonClick(null, null);
                    if (AnswerState == false && NextButton.IsEnabled == true && _trainingConfig.AutoTurn == false)
                    {
                        StartNextRound();
                        NextButton.IsEnabled = false;
                    }

                    break;
                case Key.Space:
                    /*                    if (ReadyButton.IsEnabled)
                                            OnReadyButtonClick(null, null); // Simulation Preparation Button Click
                                        break;*/
                    break;
                case Key.N:
                    /*                    if (NextButton.IsEnabled)
                                            OnNextButtonClick(null, null);
                                        break;*/
                    ; break;
            }

            UpdateSelectedButton(); // Update the selected button style
        }

        private int GetButtonsPerRow()
        {
            return (int)Math.Sqrt(_buttons.Count); // Assume a square arrangement
        }


        // Memory stage
        private void StartMemoryPhase()
        {
            Keyboard.Focus(this);

            AnswerState = false;

            RecallText.Visibility = Visibility.Visible;


            _totalQuestions++;
            /*if (is_beep) System.Media.SystemSounds.Beep.Play();*/


            // Update prompt information to prompt users to remember the pattern and its location
            /*TipsTextBlock.Text = "Please remember as many patterns as possible and their locations as possible";*/
            // Start memorizing time

            // Load the image path and make sure the randomly selected images are not duplicated
            var selectedImages = _trainingConfig.SelectedMaterial.ImagePaths.ToList();
            var random = new Random();
            var selectedForButtons = new List<string>();

            if (!_wasLastAnswerCorrect && _lastSelectedImages != null && _lastSelectedImages.Count > 0)
            {
                selectedForButtons = _lastSelectedImages.ToList(); // Copy one
            }


            PatternGrid.Children.Clear();

            // If the previous question is wrong, reuse the previous question material
            if (!_wasLastAnswerCorrect && _lastSelectedImages != null && _lastSelectedImages.Count > 0)
            {
                selectedForButtons = _lastSelectedImages.ToList(); // Copy one
            }
            else
            {
                // The previous question is correct, and a random batch will be re-ran
                var allImages = _trainingConfig.SelectedMaterial.ImagePaths.ToList();
                selectedForButtons = new List<string>();
                for (int i = 0; i < _currentDifficulty.ImageCount; i++)
                {
                    int randomIndex = random.Next(allImages.Count);
                    selectedForButtons.Add(allImages[randomIndex]);
                    allImages.RemoveAt(randomIndex);
                }
            }

            foreach (var imagePath in selectedForButtons)
            {
                var image = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri(imagePath)),
                    Stretch = Stretch.Uniform
                };
                var viewbox = new Viewbox
                {
                    Child = image,
                    Stretch = Stretch.Uniform
                };
                var button = new Button
                {
                    Tag = imagePath,
                    Width = 150,
                    Height = 150,
                    Margin = new Thickness(5),
                    Content = viewbox,
                    IsEnabled = false
                };
                PatternGrid.Children.Add(button);
            }

            foreach (Button button in PatternGrid.Children.OfType<Button>())
            {
                button.IsEnabled = false;
                button.Focusable = false;
            }

            // Keyboard operation initialization Keyboard
            _buttons = PatternGrid.Children.OfType<Button>().ToList();
            _selectedButtonIndex = -1;
            UpdateSelectedButton();

            // Recorded _lastSelectedImages
            _lastSelectedImages = selectedForButtons;


            // Randomly select a pattern from the selected button as the memory target
            _targetImagePath = selectedForButtons[random.Next(selectedForButtons.Count)];

            //Memory stage does not show memory targets
            RecallImage.Source = null;





        }

        // 
        private void OnMemoryTimerTick(object sender, EventArgs e)
        {/*
            _memoryTimer.Stop(); // Timer to stop memory stage*/
            if (_trainingConfig.LimitMemoryTime && _remainingTime.TotalSeconds <= 0)
            {
                // It will be automatically judged as error after memory time
                /*                _wrongAnswers++; // Increase the number of errors
                                _currentDifficulty.WrongAnswers++; */// Number of errors to update the current level

                // Update prompt information, display error
                /*                IncorrectTextBox.Background = Brushes.LightCoral;
                                CorrectTextBox.Background = Brushes.White;*//*

                // Automatically enter the next question
                StartNextRound();*/
                StartRecallPhase();
            }
            else
            {
                StartRecallPhase();
            }
            /*StartRecallPhase();*/  // Automatically enter the memory stage
        }

        private void OnReadyButtonClick(object sender, RoutedEventArgs e)
        {
            _memoryTimer.Stop(); // Timer to stop memory stage
            double currentMemoryTime = _currentDifficulty.MaxMemoryTime - _remainingTime.TotalSeconds;
            _currentDifficulty.AvgMemoryTime = (_currentDifficulty.AvgMemoryTime * _currentDifficulty.TotalQuestions + currentMemoryTime) / (_currentDifficulty.TotalQuestions + 1);

            if (currentMemoryTime > _currentDifficulty.LongestMemoryTime)
            {
                _currentDifficulty.LongestMemoryTime = currentMemoryTime;
            }

            // Record the answer start time
            _startAnswerTime = DateTime.Now;

            StartRecallPhase();  // Go directly to the memory stage
        }

        // Memory stage
        private void StartRecallPhase()
        {
            Debug.WriteLine("Recall");
            AnswerState = true;
            foreach (Button button in PatternGrid.Children.OfType<Button>())
            {

                button.Focusable = true; // Allows to gain focus
            }

            RecallText.Visibility = Visibility.Collapsed;

            _selectedButtonIndex = 0;
            UpdateSelectedButton();

            Keyboard.Focus(this);

            // Record the current memory time

            _updateTimer.Start();

            _remainingTime = TimeSpan.FromSeconds(_currentDifficulty.MaxMemoryTime); // Reset remaining memory time

            double currentMemoryTime = _currentDifficulty.MaxMemoryTime - _remainingTime.TotalSeconds;

            // Updated average memory time and maximum memory time
            _currentDifficulty.AvgMemoryTime = (_currentDifficulty.AvgMemoryTime * _currentDifficulty.TotalQuestions + currentMemoryTime) / (_currentDifficulty.TotalQuestions + 1);
            if (currentMemoryTime > _currentDifficulty.LongestMemoryTime)
            {
                _currentDifficulty.LongestMemoryTime = currentMemoryTime;
            }


            /*if (is_beep) System.Media.SystemSounds.Beep.Play(); // Beepsound*/
            // Will PatternGrid All buttons in the cover（Hide original pattern）
            foreach (Button button in PatternGrid.Children)
            {
                // Method 1: Clear button content
                // button.Content = null;
                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                var wenhao = Path.Combine(basePath, "Resources", "MEMO", "question mark.jpg");

                // Method 2: Show a uniform overlay pattern, such as a question mark or other symbol
                button.Content = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri(wenhao)), // Use overlay pattern path
                    Stretch = Stretch.Uniform
                };
                /*                button.IsEnabled = true;
                                button.Click += OnImageButtonClick;*/

            }
            Debug.WriteLine("Already covered in a unified manner");

            int delay = 1;
            System.Timers.Timer atimer = new System.Timers.Timer(delay);
            atimer.Elapsed += (s, args) =>
            {
                atimer.Stop();
                atimer.Dispose();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (Button button in PatternGrid.Children)
                    {
                        button.IsEnabled = true;
                        button.Click += OnImageButtonClick;
                    }
                });
            };
            atimer.Start();



            // Show the target pattern on the right RecallImage middle
            RecallImage.Source = new BitmapImage(new Uri(_targetImagePath));

            // Update prompt information
            /*TipsTextBlock.Text = "Please recall the pattern position and click on the target pattern";*/


        }

        private void OnNextButtonClick(object sender, RoutedEventArgs e)
        {
            ReadyButton.IsEnabled = true;

            NextButton.IsEnabled = false; // Disable buttons to prevent repeated clicks
            StartNextRound(); // Jump to the next question
        }

        private void OnImageButtonClick(object sender, RoutedEventArgs e)
        {
            var clickedButton = sender as Button;
            OnImageButtonClick(clickedButton, e); // Reuse logic
        }

        private void OnImageButtonClick(Button clickedButton, RoutedEventArgs e)
        {

            if (clickedButton == null) return;
            if (InputManager.Current.MostRecentInputDevice is MouseDevice)
            {
                // If it is triggered by the mouse, it will be return, do nothing
                return;
            }


            _updateTimer.Stop();
            // Disable all buttons to prevent repeated clicks
            foreach (Button button in PatternGrid.Children)
            {
                button.IsEnabled = false;
            }

            ReadyButton.IsEnabled = false;
            NextButton.IsEnabled = false;

            var clickedImagePath = clickedButton.Tag.ToString();
            bool isCorrect = clickedImagePath == _targetImagePath;

            _wasLastAnswerCorrect = isCorrect;


            /*if (is_beep) System.Media.SystemSounds.Beep.Play();*/

            // Record the current answer time
            double currentAnswerTime = (DateTime.Now - _startAnswerTime).TotalSeconds;

            // Updated maximum answer time
            _currentDifficulty.LongestAnswerTime = (_currentDifficulty.LongestAnswerTime * _currentDifficulty.TotalQuestions + currentAnswerTime) / (_currentDifficulty.TotalQuestions + 1);

            // Record the number of questions
            _currentDifficulty.TotalQuestions++;

            //Show original image
            foreach (Button button in PatternGrid.Children)
            {
                var originalImagePath = button.Tag.ToString();


                // Set background color according to click results
                if (button == clickedButton)
                {
                    button.Content = new System.Windows.Controls.Image
                    {
                        Source = new BitmapImage(new Uri(originalImagePath)), // Show original pattern
                        Stretch = Stretch.Uniform
                    };
                    button.Background = isCorrect ? Brushes.LightGreen : Brushes.LightCoral;
                }
                else
                {
                    button.Background = Brushes.Transparent; // Keep other buttons transparent background
                }

            }
            // Stop seconds





            if (isCorrect)
            {
                _currentDifficulty.CorrectAnswers++; // Update the correct number of current level
                if (is_beep)
                    PlayWav(CorrectSoundPath);
                if (this._trainingConfig.is_visual) ShowFeedbackImage(CorrectImage);
            }
            else
            {
                _currentDifficulty.WrongAnswers++; // Number of errors to update the current level
                if (is_beep)
                    PlayWav(ErrorSoundPath);
                if (this._trainingConfig.is_visual) ShowFeedbackImage(ErrorImage);
            }

            // Update the recent game result queue
            if (_recentResults.Count >= 5)
                _recentResults.Dequeue(); // Remove all results
            _recentResults.Enqueue(isCorrect); // Add the current result

            // Delay
            int delay = isCorrect ? 3000 : 5000;
            System.Timers.Timer timer = new System.Timers.Timer(delay);
            timer.Elapsed += (s, args) =>
            {
                timer.Stop();
                timer.Dispose();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    _updateTimer.Start();
                    AnswerState = false;
                    if (_trainingConfig.AutoTurn)
                    {
                        // Automatic jump logic

                        StartNextRound(); // Jump to the next question
                        /*                        foreach (Button button in PatternGrid.Children)
                                                {
                                                    button.IsEnabled = true; // Restore pattern button
                                                }*/
                        ReadyButton.IsEnabled = true; // recover Ready Button
                    }
                    else
                    {

                        // Non-automatic jump logic, only recovery NextButton
                        NextButton.IsEnabled = true; // Enable Next Button
                    }
                });
            };
            timer.Start();

        }

        private void OutputResultsGrid()
        {
            double accuracy = (_correctAnswers / (double)_totalQuestions) * 100;

            var results = new List<KeyValuePair<string, string>>
            {
/*        new KeyValuePair<string, string>("Total number of questions", _totalQuestions.ToString()),
        new KeyValuePair<string, string>("Correct number", _correctAnswers.ToString()),
        new KeyValuePair<string, string>("Error number", _wrongAnswers.ToString()),
        new KeyValuePair<string, string>("Timeout number", _timeoutCount.ToString()),
        new KeyValuePair<string, string>("Correct rate", $"{accuracy:F2}%"),*/
        new KeyValuePair<string, string>("Average memory time", $"{_currentDifficulty.AvgMemoryTime:F2}Second"),
        new KeyValuePair<string, string>("The longest memory time", $"{_currentDifficulty.LongestMemoryTime:F2}Second"),
        new KeyValuePair<string, string>("Maximum answer time", $"{_currentDifficulty.LongestAnswerTime:F2}Second"),
        new KeyValuePair<string, string>("Number of questions", $"{_currentDifficulty.TotalQuestions}"),
        new KeyValuePair<string, string>("Number of errors", $"{_currentDifficulty.WrongAnswers}"),
        new KeyValuePair<string, string>("Correct quantity", $"{_currentDifficulty.CorrectAnswers}")
            };
            //MEMO_Report nwd = new MEMO_Report(5, _correctAnswers, _wrongAnswers, _timeoutCount);
        }


        // Reset button click event
        /*        private void OnResetButtonClick(object sender, RoutedEventArgs e)
                {
                    PatternGrid.Children.Clear();
                    RecallImage.Source = null;
                }*/

        /*LJN
       Added resources for visual and sound feedback
       */
        private SoundPlayer soundPlayer; // Used to sing
        public string ErrorSoundPath;//The wrong sound file path, inOnStartAsync()Medium configuration
        public string CorrectSoundPath;//The correct sound file path is inOnStartAsync()Medium configuration
        private int StopDurations = 1000; // Stop time,ms

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
    public partial class Topological memory : BaseUserControl
    {

        private void MEMO_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void MEMO_Unloaded(object sender, RoutedEventArgs e)
        {
            _updateTimer.Stop();
            _memoryTimer.Stop();
            _windowTimer.Stop();
        }


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



            int level = 1; int trainTime = 1; int repeatNumber = 5; bool isClockVisible = false; bool memTimeLimit = true;
            _difficultyManager = new DifficultyManager();
            _trainingConfig = new TrainingConfig(trainTime, repeatNumber, isClockVisible, memTimeLimit);

            //SetDifficulty(level);
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
                        switch (par.ModuleParId) // Complete assignment
                        {
                            case 42: // Treatment time 
                                _trainingConfig.TreatmentTime = par.Value.HasValue ? (int)par.Value.Value : 60;
                                Debug.WriteLine($"TRAIN_TIME={_trainingConfig.TreatmentTime}");
                                break;
                            //case 44: // Number of repetitions
                            //    _trainingConfig.RepetitionCount = par.Value.HasValue ? (int)par.Value.Value : 5;
                            //    Debug.WriteLine($"INCREASE={_trainingConfig.RepetitionCount}");
                            //    break;
                            case 45: // Memory time limit
                                _trainingConfig.LimitMemoryTime = par.Value == 1;
                                Debug.WriteLine($"DECREASE ={_trainingConfig.LimitMemoryTime}");
                                break;
                            case 46: // Auditory feedback
                                _trainingConfig.is_beep = par.Value == 1;
                                Debug.WriteLine($"Whether to make a sound={_trainingConfig.is_beep}");
                                break;
                            case 261://Visual feedback
                                _trainingConfig.is_visual = par.Value == 1;
                                Debug.WriteLine($"Is it visual feedback?={_trainingConfig.is_visual}");
                                break;
                            // Add other things that need to be processed ModuleParId
                            case 159://grade
                                level = par.Value.HasValue ? (int)par.Value.Value : 5;
                                //_currentDifficulty.Level = level;
                                Debug.WriteLine($"grade={level}");
                                break;
                            case 274://Response time limit
                                _trainingConfig.LimitAnswerTime = par.Value == 1;
                                break;
                            case 275://The title jumps automatically
                                _trainingConfig.AutoTurn = par.Value == 1;
                                break;
                            default:
                                Debug.WriteLine($"Unprocessed ModuleParId: {par.ModuleParId}");
                                break;
                        }
                    }
                }
                SetDifficulty(level);

            }
            else
            {
                Debug.WriteLine("No data");
            }
            SetDifficulty(level);
            // Initialize statistical variables
            _totalQuestions = 0;
            _correctAnswers = 0;
            _wrongAnswers = 0;
            _timeoutCount = 0;
            _treatmentTimeInSeconds = _trainingConfig.TreatmentTime * 60;


            //Start training
            //OnStartButtonClick();

            _totalCountdown = _trainingConfig.TreatmentTime * 60;

            // Calling delegate
            LevelStatisticsAction?.Invoke(_currentDifficulty.Level, _difficultyManager.DifficultyLevels.Count);
            RightStatisticsAction?.Invoke(0, 5);
            WrongStatisticsAction?.Invoke(0, 5);
        }

        protected override async Task OnStartAsync()
        {

            InitializeTimers();
            InitializeWindowTimer(TimeSpan.FromMinutes(10));

            // Initialize training time and remaining time
            _trainTime = TimeSpan.Zero;
            //_remainingTime = TimeSpan.FromSeconds(_trainingConfig.TreatmentTime * 60); // Initialize the remaining time according to the training time
            InitializeUpdateTimer(); // Initialize the update timer
            //StartNextRound();
            OnStartButtonClick();
            // Calling delegate
            VoiceTipAction?.Invoke("First, you will see several graphics on the interface. Please quickly remember their order, and then click on the keyboard.OKkey；The three graphics will be overwritten. Please find the location corresponding to the target graphics on the right side of the screen as quickly as possible, select it through the left and right keys on the keyboard and click it with the mouse.OKkey.");
            SynopsisAction?.Invoke("Test question description information");
            RuleAction?.Invoke("First, you will see several graphics on the interface. Please quickly remember their order, and then click on the keyboard.OKkey；The three graphics will be overwritten. Please find the location corresponding to the target graphics on the right side of the screen as quickly as possible, select it through the left and right keys on the keyboard and click it with the mouse.OKkey.");//Add code, call function, display the text under the digital person

        }

        protected override async Task OnStopAsync()
        {
            _updateTimer?.Stop();
            _memoryTimer?.Stop();
            _windowTimer?.Stop();
        }

        protected override async Task OnPauseAsync()
        {
            _updateTimer.Stop();
            _memoryTimer.Stop();
            _windowTimer.Stop();
        }

        protected override async Task OnNextAsync()
        {
            // Adjust the difficulty
            StartNextRound();

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
            return new Topological memory explanation();
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
                        //......
                        int imageCount = 0;
                        double avgMemoryTime = 0;
                        double longestMemoryTime = 0;
                        double longestAnswerTime = 0;
                        int totalQuestions = 0;
                        int correctAnswers = 0;
                        int wrongAnswers = 0;

                        double _avgMemoryTime = 0;
                        double _longestAnswerTime = 0;
                        int max_hardness = 0;
                        for (int i = 0; i < _difficultyManager.DifficultyLevels.Count; i++)
                        {
                            DifficultyLevel difficulty = _difficultyManager.DifficultyLevels[i];
                            if (difficulty.AvgMemoryTime == 0 &&
                                difficulty.LongestMemoryTime == 0 &&
                                difficulty.LongestAnswerTime == 0 &&
                                difficulty.TotalQuestions == 0) { continue; }
                            max_hardness = Math.Max(max_hardness, i + 1);
                            imageCount += difficulty.ImageCount;

                            longestMemoryTime = Math.Max(longestMemoryTime, difficulty.LongestMemoryTime);
                            totalQuestions += difficulty.TotalQuestions;
                            correctAnswers += difficulty.CorrectAnswers;
                            wrongAnswers += difficulty.WrongAnswers;

                            _avgMemoryTime += difficulty.TotalQuestions * difficulty.AvgMemoryTime;
                            _longestAnswerTime += difficulty.TotalQuestions * difficulty.LongestAnswerTime;
                        }
                        if (totalQuestions > 0)
                        {
                            _avgMemoryTime /= totalQuestions;
                        }
                        if (totalQuestions > 0)
                        {
                            _longestAnswerTime /= totalQuestions;
                        }

                        // create Result Record
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "Topological memory", // Custom content
                            Eval = false,
                            Lv = max_hardness, // Current difficulty level
                            ScheduleId = BaseParameter.ScheduleId ?? null // Assumption Schedule_id, can be replaced with actual
                        };
                        db.Results.Add(newResult);
                        await db.SaveChangesAsync();

                        // Get result_id
                        int result_id = newResult.ResultId;

                        // Traversal DifficultyLevel attributes in ResultDetail Insert the database
                        // --PS: There will beOrder, marked belowOrderThe order of
                        var resultDetails = new List<ResultDetail>
                            {
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "grade",
                                    Value = max_hardness,
                                    Maxvalue = 20,
                                    Minvalue = 1,
                                    ModuleId = BaseParameter.ModuleId,
                                    Order = 0 ,
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Number of pictures",
                                    Value = imageCount,
                                    ModuleId = BaseParameter.ModuleId,
                                    Charttype = "Bar chart" ,
                                    Order = 1
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Average memory time(s)",
                                    Value = Math.Round(_avgMemoryTime,2),
                                    ModuleId = BaseParameter.ModuleId,
                                    Order = 5
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "The longest memory time(s)",
                                    Value = Math.Round(longestMemoryTime,2),
                                    ModuleId = BaseParameter.ModuleId,
                                    Order = 6
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Average answer time(s)",
                                    Value = Math.Round(_longestAnswerTime,2),
                                    ModuleId = BaseParameter.ModuleId,
                                    Order = 7
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Number of questions",
                                    Value = totalQuestions,
                                    ModuleId = BaseParameter.ModuleId,
                                    Order = 2
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Correct quantity",
                                    Value = correctAnswers,
                                    ModuleId = BaseParameter.ModuleId,
                                     Order=3
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Number of errors",
                                    Value = wrongAnswers,
                                    ModuleId = BaseParameter.ModuleId,
                                    Charttype = "Bar chart" ,
                                    Order=4
                                }
                            };

                        // insert ResultDetail data
                        db.ResultDetails.AddRange(resultDetails);
                        await db.SaveChangesAsync();

                        // Output attribute data for each difficulty level
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