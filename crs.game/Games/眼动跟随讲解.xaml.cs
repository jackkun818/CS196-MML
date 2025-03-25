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
    /// 眼动跟随讲解.xaml 的交互逻辑
    /// </summary>
    public partial class 眼动跟随讲解 : BaseUserControl
    {
        public class EyeTrackerWrapper
        {
            private const string V = "Tobii_test.dll";

            // 使用 P/Invoke 导入 C++ DLL 中的函数
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
        public 眼动跟随讲解()
        {
            InitializeComponent();
            InitializeEyeTracker();

            // 获取当前工作目录
            string currentDirectory = Directory.GetCurrentDirectory();
            string imageFolderPath = System.IO.Path.Combine(currentDirectory, "Resources/眼动跟随/");
            player = new BitmapImage(new Uri(imageFolderPath + "1.jpg", UriKind.Relative));
            background = new BitmapImage(new Uri(imageFolderPath + "2.jpg", UriKind.Relative));

            ImageBrush backgroundBrush = new ImageBrush();
            backgroundBrush.ImageSource = background;
            canvas.Background = backgroundBrush;

            playerImage = new Image();
            // 创建一个 BitmapImage 对象并设置解码
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(imageFolderPath + "1.jpg"); // 图像资源路径
            bitmap.CacheOption = BitmapCacheOption.OnLoad; // 优化加载性能
            bitmap.EndInit();
            playerImage.Source = bitmap;
            playerImage0.Source = bitmap;

            playerImage.Width = 100; // 设置宽度
            playerImage.Height = 100; // 设置高度

            Canvas.SetLeft(playerImage, 200); // 设置在 Canvas 上的左边距离
            Canvas.SetTop(playerImage, 170); // 设置在 Canvas 上的上边距离

            canvas.Children.Add(playerImage);


            curGaze = new Ellipse
            {
                Width = 30,  // 圆点的宽度
                Height = 30, // 圆点的高度
                Stroke = Brushes.Red, // 边框颜色
                StrokeThickness = 1 // 边框厚度
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

            this.Loaded += 眼动训练2讲解_Loaded;
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
        private void 眼动训练2讲解_Loaded(object sender, RoutedEventArgs e)
        {

            nextButton_Click(null, null);
        }


        private DispatcherTimer _timerMoveImage;
        private const int moveInterval = 20; // 移动间隔（毫秒）
        private const int MoveStep = 2; // 每次移动的像素数
        private DispatcherTimer _timerStart123;  //开始游戏的123倒计时
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

            //获取凝视点坐标
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
                {    // 将圆放置在指定的坐标上
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
                        nextButton.Content = "下一步";

                        await OnVoicePlayAsync(page_0_message.Text);
                    }
                    break;
                case 1:
                    {
                        page_panel.Visibility = Visibility.Visible;

                        page_0.Visibility = Visibility.Collapsed;
                        page_1.Visibility = Visibility.Visible;

                        lastButton.IsEnabled = true;
                        nextButton.Content = "试玩";


                        await OnVoicePlayAsync(page_1_message.Text);
                    }
                    break;
                case 2:
                    {
                        //LJN，在讲解模块调用委托
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("20241114待补充");//增加代码，调用函数，显示数字人下的文字
                        //LJN
                        StartGame();
                        //page_panel.Visibility = Visibility.Collapsed;
                    }
                    break;
            }
        }
    }
}
