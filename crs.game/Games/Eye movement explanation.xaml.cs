using crs.core;
using crs.core.DbModels;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Reflection;

namespace crs.game.Games
{
    /// <summary>
    /// Eye movement explanation.xaml Interaction logic
    /// </summary>
    public partial class Eye_movement_explanation : BaseUserControl
    {
        public class EyeTrackerWrapper
        {
            private const string V = "Tobii_test.dll";

            // use P/Invoke Import C++ DLL Functions in
            [DllImport(V, CallingConvention = CallingConvention.Cdecl)]
            public static extern bool InitEyeTracker(int screenWidth, int screenHeight);

            [DllImport(V, CallingConvention = CallingConvention.Cdecl)]
            public static extern int GetGazeIndex(IntPtr areas, int numAreas, uint dwellTime);

            [DllImport(V, CallingConvention = CallingConvention.Cdecl)]
            public static extern void ReleaseEyeTracker();

            [DllImport(V, CallingConvention = CallingConvention.Cdecl)]
            public static extern void GetGazeCoordinates(out int x, out int y);

            [DllImport(V, CallingConvention = CallingConvention.Cdecl)]
            public static extern void GetRandomCoordinates(out int x, out int y);


        }

        bool useMouseCoordinates = false;

        BitmapImage player;
        BitmapImage background;
        Image playerImage;
        Ellipse curGaze;
        TextBlock textBlock123;
        public Eye_movement_explanation()
        {
            InitializeComponent();
            InitializeEyeTracker();

            // Get the current working directory
            string currentDirectory = Directory.GetCurrentDirectory();
            string imageFolderPath = System.IO.Path.Combine(currentDirectory, "Resources/Eye movement follow/");
            player = new BitmapImage(new Uri(imageFolderPath + "1.jpg", UriKind.Relative));
            background = new BitmapImage(new Uri(imageFolderPath + "2.jpg", UriKind.Relative));

            ImageBrush backgroundBrush = new ImageBrush();
            backgroundBrush.ImageSource = background;
            canvas.Background = backgroundBrush;

            playerImage = new Image();
            // Create a BitmapImage Object and set decoding
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(imageFolderPath + "1.jpg"); // Image resource path
            bitmap.CacheOption = BitmapCacheOption.OnLoad; // Optimized loading performance
            bitmap.EndInit();
            playerImage.Source = bitmap;
            playerImage0.Source = bitmap;

            playerImage.Width = 100; // Set width
            playerImage.Height = 100; // Set height

            Canvas.SetLeft(playerImage, 200); // Set in Canvas The distance to the left
            Canvas.SetTop(playerImage, 170); // Set in Canvas The upper distance

            canvas.Children.Add(playerImage);


            curGaze = new Ellipse
            {
                Width = 30,  // The width of the dot
                Height = 30, // The height of the dot
                Stroke = Brushes.Red, // Border color
                StrokeThickness = 1 // Border thickness
            };
            Canvas.SetLeft(curGaze, 200);
            Canvas.SetTop(curGaze, 100);
            canvas.Children.Add(curGaze);

            textBlock123 = new TextBlock();
            textBlock123.Text = "3";
            textBlock123.FontSize = 40;
            textBlock123.FontWeight = FontWeights.Bold;
            textBlock123.Foreground = new SolidColorBrush(Colors.Green);
            Canvas.SetLeft(textBlock123, 400);
            Canvas.SetTop(textBlock123, 150);
            canvas.Children.Add(textBlock123);


            _timerMoveImage = new DispatcherTimer();
            _timerMoveImage.Interval = TimeSpan.FromMilliseconds(moveInterval);
            _timerMoveImage.Tick += TimerMoveImage_Tick;

            _timerStart123 = new DispatcherTimer();
            _timerStart123.Interval = TimeSpan.FromSeconds(1);
            timeStart123 = 3;
            _timerStart123.Tick += TimerStart123_Tick;

            this.Loaded += Eye_movement_training_2_explanation_Loaded;
        }

        void InitializeEyeTracker()
        {
            int screenWidth = 1920;
            int screenHeight = 1080;

            if (EyeTrackerWrapper.InitEyeTracker(screenWidth, screenHeight))
            {
                Debug.Write("Eye tracker initialized successfully.\n");
            }
            else
            {
                Debug.Write("Failed to initialize eye tracker. Falling back to random coordinates.\n");
            }
        }
        private void Eye_movement_training_2_explanation_Loaded(object sender, RoutedEventArgs e)
        {

            nextButton_Click(null, null);
        }


        private DispatcherTimer _timerMoveImage;
        private const int moveInterval = 20; // Moving interval（millisecond）
        private const int MoveStep = 2; // Number of pixels per move
        private DispatcherTimer _timerStart123;  //Countdown to 123 of the start game
        int timeStart123 = 3;

        void TimerStart123_Tick(object sender, EventArgs e)
        {
            timeStart123--;

            textBlock123.Text = timeStart123.ToString();

            if (timeStart123 <= 0)
            {
                textBlock123.Text = "";
                _timerStart123?.Stop();
                _timerMoveImage?.Start();
                timeStart123 = 3;
            }

        }

        private void TimerMoveImage_Tick(object sender, EventArgs e)
        {

            double posX = Canvas.GetLeft(playerImage), posY = Canvas.GetTop(playerImage);
            Vector2 newPos = new Vector2((float)posX + MoveStep, (float)posY);

            if (newPos.X >= 800)
            {
                StopGame();
                OkButton.Visibility = Visibility.Visible;
                return;
            }

            if ((newPos.X > 0) && (newPos.X + playerImage.Width < canvas.Width) &&
                (newPos.Y > 0) && (newPos.Y + playerImage.Height < canvas.Height))
            {
                Canvas.SetLeft(playerImage, newPos.X);
                Canvas.SetTop(playerImage, newPos.Y);
            }

            //Get gaze point coordinates
            Vector2 gazePoint = GetGazeCoordinates();
            Point canvasPoint;
            if (useMouseCoordinates == true)
            {
                canvasPoint = Mouse.GetPosition(this);
            }
            else
            {
                gazePoint = GetGazeCoordinates();
                canvasPoint = canvas.PointFromScreen(new Point(gazePoint.X, gazePoint.Y));
            }

            if (PresentationSource.FromVisual(canvas) != null)
            {

                if ((canvasPoint.X - curGaze.Width / 2 > 0) && (canvasPoint.X + curGaze.Width / 2 < canvas.Width) &&
                (canvasPoint.Y - curGaze.Width / 2 > 0) && (canvasPoint.Y + curGaze.Height / 2 < canvas.Height))
                {    // Place the circle on the specified coordinates
                    Canvas.SetLeft(curGaze, canvasPoint.X - curGaze.Width / 2);
                    Canvas.SetTop(curGaze, canvasPoint.Y - curGaze.Height / 2);
                }
            }

        }
        Vector2 GetGazeCoordinates()
        {
            int x, y;

            EyeTrackerWrapper.GetGazeCoordinates(out x, out y);

            return new Vector2(x, y);
        }

        void StartGame()
        {
            Canvas.SetLeft(playerImage, 20);
            _timerStart123?.Start();
        }
        void StopGame()
        {
            _timerStart123?.Stop();
            _timerMoveImage?.Stop();
        }



        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            OnGameBegin();
        }

        int currentPage = -1;

        private void lastButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 0)
            {
                currentPage--;
            }
            PageSwitch();
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < 2)
            {
                currentPage++;
            }
            PageSwitch();
        }


        private void ignoreButton_Click(object sender, RoutedEventArgs e)
        {
            OnGameBegin();
        }

        async void PageSwitch()
        {
            switch (currentPage)
            {
                case 0:
                    {
                        page_panel.Visibility = Visibility.Visible;

                        page_0.Visibility = Visibility.Visible;
                        page_1.Visibility = Visibility.Collapsed;

                        lastButton.IsEnabled = false;
                        nextButton.Content = "Next step";

                        await OnVoicePlayAsync(page_0_message.Text);
                    }
                    break;
                case 1:
                    {
                        page_panel.Visibility = Visibility.Visible;

                        page_0.Visibility = Visibility.Collapsed;
                        page_1.Visibility = Visibility.Visible;

                        lastButton.IsEnabled = true;
                        nextButton.Content = "Trial";


                        await OnVoicePlayAsync(page_1_message.Text);
                    }
                    break;
                case 2:
                    {
                        //LJN, instructing the delegate in the explanation module
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("20241114 to be supplemented");//Add code, call function, display the text under the digital person
                        //LJN
                        StartGame();
                        //page_panel.Visibility = Visibility.Collapsed;
                    }
                    break;
            }
        }
    }
}
