using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Numerics;
using System.Runtime.InteropServices;
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

namespace crs.game.Games
{
    /// <summary>
    /// Attention distribution explanation.xaml Interaction logic
    /// </summary>
    public partial class Attention_distribution_explanation : BaseUserControl
    {
        public Attention_distribution_explanation()
        {
            InitializeComponent();

            InitImage();




            this.Loaded += Attention_distribution_explanation_Loaded;
        }

        private void Attention_distribution_explanation_Loaded(object sender, RoutedEventArgs e)
        {
            Button_2_Click(null, null);
        }

        void InitImage()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string imageFolderPath = System.IO.Path.Combine(currentDirectory, "Resources/Attention distribution/img/");
            Image_1.Source = new BitmapImage(new Uri(imageFolderPath + "1.png", UriKind.Absolute));
            Image_2.Source = new BitmapImage(new Uri(imageFolderPath + "2.png", UriKind.Absolute));
            Image_3.Source = new BitmapImage(new Uri(imageFolderPath + "3.png", UriKind.Absolute));
            Image_4.Source = new BitmapImage(new Uri(imageFolderPath + "4.png", UriKind.Absolute));
        }



        void StartGame()
        {
            if (isStartListen == false)
            {
                isStartListen = true;
                StartListen();
            }

            if (isU3DLoaded == false)
            {
                unityLoadTimer = new DispatcherTimer();
                unityLoadTimer.Interval = TimeSpan.FromMilliseconds(20); // 
                unityLoadTimer.Tick += (sender, e) =>
                {

                    if (PresentationSource.FromVisual(Panel2) != null)
                    {
                        unityLoadTimer.Stop(); // Stop the timer
                        unityLoadTimer.Tick -= (s, args) => { }; // Unsubscribe from event to prevent memory leaks

                        LoadUnity();
                    }
                };
                unityLoadTimer.Start(); // Start the timer


                unityStartTimer = new DispatcherTimer();
                unityStartTimer.Interval = TimeSpan.FromMilliseconds(20); // 
                unityStartTimer.Tick += (sender, e) =>
                {
                    if (streamList.Count > 0)
                    {
                        unityStartTimer.Stop(); // Stop the timer
                        unityStartTimer.Tick -= (s, args) => { }; // Unsubscribe from event to prevent memory leaks

                        BroadcastToClient(String.Format("StartGame,{0},{1},{2}", 15, 0, 0));
                    }
                };
                unityStartTimer.Start(); // Start the timer

                DispatcherTimer hasMessageTimer = new DispatcherTimer();
                hasMessageTimer.Interval = TimeSpan.FromMilliseconds(20);
                hasMessageTimer.Tick += (sender, e) =>
                {
                    if (hasReceivedMessage)
                    {
                        hasMessageTimer.Stop();
                        end.Visibility = Visibility.Visible;
                        Panel2.Visibility = Visibility.Collapsed;
                    }

                };
                hasMessageTimer.Start();


                return;
            }
        }


        //Establishsocketconnect
        private TcpListener tcpListener;
        List<NetworkStream> streamList = new List<NetworkStream>();
        private readonly int port = 49200;
        bool isStartListen = false;
        bool gamePauseState = false;

        /// <summary>
        /// 
        /// </summary>
        /// 
        int level = 1;
        int _totalTrainTime;
        private int INCREASE; // Increase the threshold for difficulty
        private int DECREASE;  // Threshold for reducing difficulty
        int _levelTime;     //Level Duration
        int signalDistance;       //The distance between two signals
        bool hasTrainNoise;
        bool hasSoundResponse;   //Auditory feedback

        /// <summary>
        /// Wait for two seconds before loadingunity
        /// </summary>
        DispatcherTimer unityLoadTimer;
        DispatcherTimer unityStartTimer;

        /// <summary>
        /// Record the results
        /// </summary>
        List<(int, Vector3)> resultsList;  //total,（Correct, wrong, omission）
        Vector3 lastResult = new Vector3();


        /// <summary>
        /// Record total countdown and level time
        /// </summary>
        DispatcherTimer _totalCountTimer;
        DispatcherTimer _levelCountTimer;
        int totalCountTime = 0;
        int levelCountTime = 0;

        void InitResultList()
        {
            resultsList = new List<(int, Vector3)>();
            for (int i = 0; i <= 14; i++)
            {
                resultsList.Add((0, new Vector3(0, 0, 0)));
            }
        }

        void InitTimer()
        {
            _totalCountTimer = new DispatcherTimer();
            _totalCountTimer.Interval = TimeSpan.FromSeconds(1);
            _totalCountTimer.Tick += TotalCountTimer_Tick;

            _levelCountTimer = new DispatcherTimer();
            _levelCountTimer.Interval = TimeSpan.FromSeconds(1);
            _levelCountTimer.Tick += LevelCountTimer_Tick;
        }

        private void LevelCountTimer_Tick(object sender, EventArgs e)
        {
            levelCountTime -= 1;
            if (levelCountTime < 0)
            {
                //Interaction, get results
                BroadcastToClient("GetResult");

                DispatcherTimer getResultTimer = new DispatcherTimer();
                getResultTimer.Interval = TimeSpan.FromMilliseconds(20);
                getResultTimer.Tick += (sender, sy) =>
                {
                    if (hasReceivedMessage == true)
                    {
                        hasReceivedMessage = false;

                        AdjustLevel();

                        Next();
                        getResultTimer?.Stop();
                    }
                };
                getResultTimer.Start();


            }
        }

        private void TotalCountTimer_Tick(object sender, EventArgs e)
        {
            totalCountTime -= 1;
            if (totalCountTime <= 0)
            {
                Stop();
            }
            TimeStatisticsAction?.Invoke(totalCountTime, levelCountTime);
        }

        void Stop()
        {
            tcpListener?.Stop();
            tcpListener?.Dispose();
            StopClient();

            CloseUnity();

            _totalCountTimer?.Stop();
            _levelCountTimer?.Stop();
        }

        void Next()
        {
            BroadcastToClient(String.Format("StartGame,{0},{1},{2}", level, hasTrainNoise ? 1 : 0, hasSoundResponse ? 1 : 0));
        }

        void AdjustLevel()
        {
            double tmp = 100.0 * lastResult.X / (lastResult.X + lastResult.Y + lastResult.Z);
            if (tmp >= INCREASE)
            {
                if (level < 14)
                {
                    level++;
                }
            }
            else if (tmp <= DECREASE)
            {
                if (level > 1)
                {
                    level--;
                }
            }
        }

        public void StartListen()
        {


            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            Debug.WriteLine("Server started on port " + port);

            Task.Run(AcceptClients);
        }



        private async Task AcceptClients()
        {
            while (true)
            {
                TcpClient client = await tcpListener.AcceptTcpClientAsync();
                Debug.WriteLine("Client connected");
                NetworkStream stream = client.GetStream();

                streamList.Add(stream);

                await HandleClientAsync(stream);
                client.Close();
            }
        }


        bool hasReceivedMessage = false;
        private async Task HandleClientAsync(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Debug.WriteLine("Received: " + message);

                hasReceivedMessage = true;

                tcpListener?.Stop();
                tcpListener?.Dispose();
                StopClient();



                CloseUnity();

                //end.Visibility = Visibility.Visible;
                //ALL_Canvs.Visibility = Visibility.Hidden;

            }
        }



        private async Task SendMessage(NetworkStream stream, string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        private void BroadcastToClient(string message)
        {
            foreach (var stream in streamList)
            {
                Task.Run(async () =>
                {
                    await SendMessage(stream, message);
                });
            }
        }

        void StopClient()
        {
            foreach (var stream in streamList)
            {
                stream?.Close();
            }
        }

        override protected void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            ActivateUnityWindow();
        }


        #region Unityoperate

        [DllImport("User32.dll")]
        static extern bool MoveWindow(IntPtr handle, int x, int y, int width, int height, bool redraw);
        internal delegate int WindowEnumProc(IntPtr hwnd, IntPtr lparam);
        //Change the position and size of the specified window, based on the upper left corner（Screen/Parent window）（Specify the handle of the window, the left position of the window, the top position of the window, the new width of the window, the new height of the window, and whether to re-draw the window）

        [DllImport("user32.dll")]
        internal static extern bool EnumChildWindows(IntPtr hwnd, WindowEnumProc func, IntPtr lParam);
        //Enumerate all child windows of a parent window（Parent window handle, callback function address, custom parameters）

        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        //This function sends the specified message to one or more windows. This function calls the window program for the specified window.
        //Return until the window program has finished processing the message.


        private Process process;
        private IntPtr unityHWND = IntPtr.Zero;
        private const int WM_ACTIVATE = 0x0006;
        private readonly IntPtr WA_ACTIVE = new IntPtr(1);
        private readonly IntPtr WA_INACTIVE = new IntPtr(0);
        private bool isU3DLoaded = false;
        private Point u3dLeftUpPos;

        private DispatcherTimer dispatcherTimer;

        private void LoadUnity()
        {
            try
            {
                //IntPtr hwnd = ((HwndSource)PresentationSource.FromVisual(Panel1)).Handle;
                IntPtr hwnd = Panel2.Handle;
                process = new Process();

                // Get the current working directory
                string currentDirectory = Directory.GetCurrentDirectory();
                string u3DFolderPath = System.IO.Path.Combine(currentDirectory, "Resources/Attention distribution/Unity3/");

                //String appStartupPath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                process.StartInfo.FileName = u3DFolderPath + @"\Attention Game.exe";
                process.StartInfo.Arguments = "-parentHWND " + hwnd.ToInt32() + " " + Environment.CommandLine;
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                process.WaitForInputIdle();
                isU3DLoaded = true;
                EnumChildWindows(hwnd, WindowEnum, IntPtr.Zero);

                dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Tick += new EventHandler(InitialResize);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
                dispatcherTimer.Start();
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
        }
        private void InitialResize(object sender, EventArgs e)
        {
            ResizeU3D();
            dispatcherTimer.Stop();
        }
        private int WindowEnum(IntPtr hwnd, IntPtr lparam)
        {
            unityHWND = hwnd;
            ActivateUnityWindow();
            return 0;
        }
        private void ActivateUnityWindow()
        {
            SendMessage(unityHWND, WM_ACTIVATE, WA_ACTIVE, IntPtr.Zero);
        }

        private void DeactivateUnityWindow()
        {
            SendMessage(unityHWND, WM_ACTIVATE, WA_INACTIVE, IntPtr.Zero);
        }

        private void ResizeU3D()
        {
            if (isU3DLoaded)
            {
                Window window = Window.GetWindow(this);
                u3dLeftUpPos = Panel1.TransformToAncestor(window).Transform(new Point(0, 0));
                u3dLeftUpPos = new Point(0, 0);
                DPIUtils.Init(this);
                //u3dLeftUpPos.X *= DPIUtils.DPIX;
                //u3dLeftUpPos.Y *= DPIUtils.DPIY;
                //MoveWindow(unityHWND, (int)u3dLeftUpPos.X, (int)u3dLeftUpPos.Y,
                //    (int)(Panel2.Width * DPIUtils.DPIX), (int)(Panel2.Height * DPIUtils.DPIY), false);
                MoveWindow(unityHWND, (int)u3dLeftUpPos.X, (int)u3dLeftUpPos.Y,
                    (int)(Panel2.Width * 1), (int)(Panel2.Height * 1), false);
                ActivateUnityWindow();
            }
        }

        private void CloseUnity()
        {
            try
            {
                if (process != null && !process.HasExited)
                {
                    // Try to close Unity program
                    process.CloseMainWindow();
                    // wait Unity Program Close
                    process.WaitForExit(1000); // 

                    // if Unity The process is forced to end if the program is not closed.
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                }


            }
            catch (Exception ex)
            {
                // Handle exceptions, e.g. Unity The program may have been closed
                string error = ex.Message;
            }
            finally
            {
                // Clean up resources
                if (process != null)
                {
                    process.Dispose();
                    process = null;
                }
            }
        }
        #endregion


        #region Form position coordinate transformation
        public class DPIUtils
        {
            private static double _dpiX = 1.0;
            private static double _dpiY = 1.0;
            public static double DPIX
            {
                get
                {
                    return DPIUtils._dpiX;
                }
            }
            public static double DPIY
            {
                get
                {
                    return DPIUtils._dpiY;
                }
            }
            public static void Init(System.Windows.Media.Visual visual)
            {
                Matrix transformToDevice = System.Windows.PresentationSource.FromVisual(visual).CompositionTarget.TransformToDevice;
                DPIUtils._dpiX = transformToDevice.M11;
                DPIUtils._dpiY = transformToDevice.M22;
            }
            public static Point DivideByDPI(Point p)
            {
                return new Point(p.X / DPIUtils.DPIX, p.Y / DPIUtils.DPIY);
            }
            public static Rect DivideByDPI(Rect r)
            {
                return new Rect(r.Left / DPIUtils.DPIX, r.Top / DPIUtils.DPIY, r.Width, r.Height);
            }
        }
        #endregion





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
                        Text_1.Visibility = Visibility.Visible;
                        Image_1.Visibility = Visibility.Visible;
                        Text_2.Visibility = Visibility.Hidden;
                        Image_2.Visibility = Visibility.Hidden;
                        Text_3.Visibility = Visibility.Hidden;
                        Image_3.Visibility = Visibility.Hidden;
                        Text_4.Visibility = Visibility.Hidden;
                        Image_4.Visibility = Visibility.Hidden;

                        Button_1.IsEnabled = false;
                        Button_2.Content = "Next step";

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
                        Text_4.Visibility = Visibility.Hidden;
                        Image_4.Visibility = Visibility.Hidden;

                        Button_1.IsEnabled = true;

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
                        Text_4.Visibility = Visibility.Hidden;
                        Image_4.Visibility = Visibility.Hidden;

                        Button_1.IsEnabled = true;

                        await OnVoicePlayAsync(Text_2.Text);
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
                        Text_4.Visibility = Visibility.Visible;
                        Image_4.Visibility = Visibility.Visible;

                        Button_1.IsEnabled = true;
                        Button_2.Content = "Trial";

                        await OnVoicePlayAsync(Text_2.Text);
                    }
                    break;
                case 4:
                    {
                        Text_1.Visibility = Visibility.Hidden;
                        Image_1.Visibility = Visibility.Hidden;
                        Text_2.Visibility = Visibility.Hidden;
                        Image_2.Visibility = Visibility.Hidden;
                        Text_3.Visibility = Visibility.Hidden;
                        Image_3.Visibility = Visibility.Hidden;
                        Text_4.Visibility = Visibility.Hidden;
                        Image_4.Visibility = Visibility.Hidden;

                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Collapsed;

                        ALL_Canvs.Visibility = Visibility.Visible;
                        RuleAction?.Invoke("Now you can see the screen on the screen, you need to control the speed of the train so that the speed pointer in the dashboard remains within the green range；When the yellow signal light in the dashboard is on, pressOKkey；When the red brake signal light in the dashboard is on, pressOKKey brake；When a traffic light appears in the screen, please be careful not to pass when the red light is.");
                        StartGame();
                    }
                    break;
            }
        }
    }
}
