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
    /// Explanation of visual repair training.xaml Interaction logic
    /// </summary>
    public partial class Explanation of visual repair training : BaseUserControl
    {
        private Polygon _triangle;
        private Rectangle _whiteSquare;
        private DispatcherTimer _shapeChangeTimer;
        private DispatcherTimer _revertTimer;
        private DispatcherTimer _squareAppearanceTimer;
        private DispatcherTimer _squareFlashTimer;
        private DispatcherTimer _endTrainingTimer;
        private bool _centerPointCorrect = false;
        private bool _squareCorrect = false;
        private DateTime _changeTime1;
        private DateTime _changeTime2;
        private Random _random;
        private bool _centerPointChecked = false; // Is the center point checked
        private bool _squareChecked = false; // Is the white square checked
        private bool _centerPointMissed = false; // Did you miss the center click button
        private bool _squareMissed = false; // Did you miss the white square key

        public Action GameBeginAction { get; set; }

        public Func<string, Task> VoicePlayFunc { get; set; }

        public Explanation of visual repair training()
        {
            InitializeComponent();



            this.Loaded += Explanation of visual repair training_Loaded;
            Focus();

        }

        private void Explanation of visual repair training_Loaded(object sender, RoutedEventArgs e)
        {
            // Ensure key presses and focus behavior when page loads
            Button_2_Click(null, null);
            Focus();  // Make sure the focus is on the window
        }

        private void InitializeTraining()
        {
            _random = new Random();

            // Create and set a triangle, but the initial state is hidden
            _triangle = new Polygon
            {
                Points = new PointCollection(new Point[]
                {
                    new Point(20, 0),  // vertex
                    new Point(0, 34.64), // Lower left corner
                    new Point(40, 34.64) // Lower right corner
                }),
                Fill = new SolidColorBrush(Color.FromRgb(184, 134, 11)), // Dark yellow
                Visibility = Visibility.Hidden,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            MainGrid.Children.Add(_triangle);

            // Create and set a white square, but the initial state is hidden
            _whiteSquare = new Rectangle
            {
                Width = 50,
                Height = 50,
                Fill = Brushes.White,
                Visibility = Visibility.Hidden
            };
            MainGrid.Children.Add(_whiteSquare);

            // Initialize the timer
            _shapeChangeTimer = new DispatcherTimer();
            _shapeChangeTimer.Interval = TimeSpan.FromSeconds(5); // Deformation in 5 seconds
            _shapeChangeTimer.Tick += ShapeChangeTimer_Tick;
            _shapeChangeTimer.Start();

            _squareAppearanceTimer = new DispatcherTimer();
            _squareAppearanceTimer.Interval = TimeSpan.FromSeconds(10); // A square is displayed after 10 seconds
            _squareAppearanceTimer.Tick += SquareAppearanceTimer_Tick;
            _squareAppearanceTimer.Start();

            // Timer to end training（After 12 seconds）
            _endTrainingTimer = new DispatcherTimer();
            _endTrainingTimer.Interval = TimeSpan.FromSeconds(12);
            _endTrainingTimer.Tick += EndTrainingTimer_Tick;
            _endTrainingTimer.Start();

            // Listen to keyboard events
            this.KeyDown += MainWindow_KeyDown;
            this.Focus();
        }

        private void ShapeChangeTimer_Tick(object sender, EventArgs e)
        {
            // Switch to triangle
            FocusPoint.Visibility = Visibility.Hidden;
            _triangle.Visibility = Visibility.Visible;
            _changeTime1 = DateTime.Now;

            // Start the timer at 0.Return to circle after 3 seconds
            _revertTimer = new DispatcherTimer();
            _revertTimer.Interval = TimeSpan.FromSeconds(0.3);
            _revertTimer.Tick += RevertTimer_Tick;
            _revertTimer.Start();
            _shapeChangeTimer.Stop();

            // Set a timer to mark missed center point events
            var missedCenterPointTimer = new DispatcherTimer();
            missedCenterPointTimer.Interval = TimeSpan.FromSeconds(1.5);
            missedCenterPointTimer.Tick += (s, args) =>
            {
                _centerPointMissed = true;
                missedCenterPointTimer.Stop();
            };
            missedCenterPointTimer.Start();
        }

        private void RevertTimer_Tick(object sender, EventArgs e)
        {
            // Restore the circle
            FocusPoint.Visibility = Visibility.Visible;
            _triangle.Visibility = Visibility.Hidden;

            _revertTimer.Stop();
        }

        private void SquareAppearanceTimer_Tick(object sender, EventArgs e)
        {
            // The white squares are randomly displayed in the lower right area
            double xPosition = _random.Next(0, 800);
            double yPosition = _random.Next(0, 800);
            _whiteSquare.Margin = new Thickness(200 + xPosition, 200 + yPosition, 0, 0); // 200 pixel offset to fit the center of the window
            _whiteSquare.Visibility = Visibility.Visible;
            _changeTime2 = DateTime.Now;

            _squareFlashTimer = new DispatcherTimer();
            _squareFlashTimer.Interval = TimeSpan.FromSeconds(0.3);
            _squareFlashTimer.Tick += SquareFlashTimer_Tick;
            _squareFlashTimer.Start();
            _squareAppearanceTimer.Stop();

            // Set a timer to mark missed white square events
            var missedSquareTimer = new DispatcherTimer();
            missedSquareTimer.Interval = TimeSpan.FromSeconds(1.5);
            missedSquareTimer.Tick += (s, args) =>
            {
                _squareMissed = true;
                missedSquareTimer.Stop();
            };
            missedSquareTimer.Start();
        }

        private void SquareFlashTimer_Tick(object sender, EventArgs e)
        {
            // Hide white square
            _whiteSquare.Visibility = Visibility.Hidden;
            _squareFlashTimer.Stop();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                if (!_centerPointChecked && !_centerPointMissed)
                {
                    // Determine the change of the center point
                    var reactionTime1 = DateTime.Now - _changeTime1;
                    if (reactionTime1.TotalSeconds <= 1.5)
                    {
                        _centerPointCorrect = true; // The user pressed correctly after the center point changedEnterkey
                    }
                    _centerPointChecked = true; // Marking center point checked
                }
                else if (!_squareChecked && !_squareMissed)
                {
                    // Judge white square flashing
                    var reactionTime2 = DateTime.Now - _changeTime2;
                    if (reactionTime2.TotalSeconds <= 1.5)
                    {
                        _squareCorrect = true; // The user pressed correctly after flashing the white squareEnterkey
                    }
                    _squareChecked = true; // Mark white square checked
                }
            }
        }
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!_centerPointChecked && !_centerPointMissed)
                {
                    // Determine the change of the center point
                    var reactionTime1 = DateTime.Now - _changeTime1;
                    if (reactionTime1.TotalSeconds <= 1.5)
                    {
                        _centerPointCorrect = true; // The user pressed correctly after the center point changedEnterkey
                    }
                    _centerPointChecked = true; // Marking center point checked
                }
                else if (!_squareChecked && !_squareMissed)
                {
                    // Judge white square flashing
                    var reactionTime2 = DateTime.Now - _changeTime2;
                    if (reactionTime2.TotalSeconds <= 1.5)
                    {
                        _squareCorrect = true; // The user pressed correctly after flashing the white squareEnterkey
                    }
                    _squareChecked = true; // Mark white square checked
                }
            }
        }

        private void EndTrainingTimer_Tick(object sender, EventArgs e)
        {
            _endTrainingTimer.Stop();

            // Show training results
            string resultMessage = $"The center point is correct: {_centerPointCorrect}\n" +
                                   $"White square correct: {_squareCorrect}";
            MessageBox.Show(resultMessage, "Training results", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
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

                        // Hide part of the trial
                        MainGrid.Visibility = Visibility.Collapsed;

                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Content = "Next step";
                        Button_2.Margin = new Thickness(329, 850, 0, 0);
                        Button_3.Margin = new Thickness(770, 850, 0, 0);

                        await OnVoicePlayAsync(Text_1.Text);
                    }
                    break;
                case 1:
                    {
                        // Display the first interface of explanation
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Visible;
                        Image_2.Visibility = Visibility.Visible;
                        Text_3.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;

                        // Hide part of the trial
                        MainGrid.Visibility = Visibility.Collapsed;
                        Button_1.Visibility = Visibility.Visible;
                        Button_1.IsEnabled = true;
                        Button_2.Margin = new Thickness(550, 850, 0, 0);
                        Button_3.Margin = new Thickness(911, 850, 0, 0);

                        await OnVoicePlayAsync(Text_2.Text);
                    }
                    break;
                case 2:
                    {
                        // Display the second interface of the explanation
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Text_3.Visibility = Visibility.Visible;
                        Image_3.Visibility = Visibility.Visible;



                        // Hide the controls for the trial play section
                        MainGrid.Visibility = Visibility.Collapsed;

                        Button_1.IsEnabled = true;
                        Button_2.Content = "Trial";

                        await OnVoicePlayAsync(Text_3.Text);
                    }
                    break;
                case 3:
                    {
                        // Enter the trial interface
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        Text_3.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;

                        // Controls showing the trial part
                        MainGrid.Visibility = Visibility.Visible;

                        // Hide the button for the explanation section
                        Button_1.Visibility = Visibility.Collapsed;
                        //Button_1.IsEnabled = false;
                        Button_2.Visibility = Visibility.Collapsed;
                        //Button_2.IsEnabled = false;
                        Button_3.Margin = new Thickness(550, 850, 0, 0);

                        // Force focus to remain in the window
                        Focus();

                        InitializeTraining();
                        //LJN, instructing the delegate in the explanation module
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("There is now a fixed green dot in the center of the screen. As you watch it, it changes from time to time, and when you notice the change is happening, press theOKkey；When a small white square appears in the center of the screen, please press theOKkey.");//Add code, call function, display the text under the digital person
                        //LJN
                    }
                    break;
            }
        }



    }
}
