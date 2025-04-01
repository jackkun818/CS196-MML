using crs.core;
using crs.core.DbModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
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
using System.Windows.Forms;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using Spire.Additions.Xps.Schema;
using System.Numerics;
using SkiaSharp;
using Newtonsoft.Json;
using System.Drawing;
using Point = System.Windows.Point;

namespace crs.game.Games
{
    public partial class Kill mosquitoes : BaseUserControl, IGameBase
    {
        public Kill mosquitoes()
        {
            InitializeComponent();
        }
        public class UdpServer
        {
            private UdpClient udpClient;
            private IPEndPoint broadcastEndPoint;
            public UdpServer()
            {
                // createUdpClientInstance, listen for specified ports
                udpClient = new UdpClient(7001);
                // Setting up broadcast endpoints
                broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, 7002);
            }

            public void BroadcastMessage(string data)
            {
                // Convert string data to bytes
                byte[] bytesToSend = Encoding.ASCII.GetBytes(data);
                try
                {
                    // Broadcast data
                    udpClient.Send(bytesToSend, bytesToSend.Length, broadcastEndPoint);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error broadcasting data: " + ex.Message);
                }
            }

            public void Stop()
            {
                // closureUdpClientFree up resources
                udpClient?.Close();
            }
        }
        UdpServer udpServer;
        Random random = new Random();
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

        void InitializeEyeTracker()
        {
            int screenWidth = 1920;
            int screenHeight = 1080;

            if (EyeTrackerWrapper.InitEyeTracker(screenWidth, screenHeight))
            {
                Debug.WriteLine("Eye tracker initialized successfully.\n");
            }
            else
            {
                Debug.WriteLine("Failed to initialize eye tracker. Falling back to random coordinates.\n");
            }
        }

        /// <summary>
        /// Wait for two seconds before loadingunity
        /// </summary>
        DispatcherTimer unityLoadTimer;
        DispatcherTimer unityStartTimer;

        DispatcherTimer udpSendMessageTimer;


        void UdpSendMessageTimer_Tick(object sender, EventArgs e)
        {
            int x, y;
            EyeTrackerWrapper.GetGazeCoordinates(out x, out y);
            udpServer.BroadcastMessage(JsonConvert.SerializeObject(new
            {
                ScreenX = (float)x,
                ScreenY = (float)y,
            }));
            //udpServer.BroadcastMessage(JsonConvert.SerializeObject(new
            //{
            //    ScreenX = (float)System.Windows.Forms.Control.MousePosition.X,
            //    ScreenY = (float)System.Windows.Forms.Control.MousePosition.Y,
            //}));
        }




        override protected void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            ActivateUnityWindow();
        }
        #region Display the screen to the therapist

        private DispatcherTimer displayImageTimer;

        private void DisplayImageTimer_Tick(object sender, EventArgs e)
        {

            var bitmap = GetWindowImage(unityHWND);
            var bitmapSource = BitmapToBitmapImage(bitmap);
            ImageControl.Source = bitmapSource;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }
        }


        private static Bitmap GetWindowImage(IntPtr hwnd)
        {
            RECT rc = new RECT();
            GetWindowRect(hwnd, ref rc);

            int width = rc.Right - rc.Left;
            int height = rc.Bottom - rc.Top;

            IntPtr hdcSource = GetDC(hwnd);
            IntPtr hdcDest = CreateCompatibleDC(hdcSource);
            IntPtr hBitmap = CreateCompatibleBitmap(hdcSource, width, height);
            IntPtr hOld = SelectObject(hdcDest, hBitmap);

            // Capture the window into the bitmap.
            PrintWindow(hwnd, hdcDest, 0x00000002);

            // Restore selection.
            SelectObject(hdcDest, hOld);

            // Clean up.
            DeleteDC(hdcDest);
            ReleaseDC(hwnd, hdcSource);

            // Create a .NET bitmap from the hBitmap.
            Bitmap image = System.Drawing.Image.FromHbitmap(hBitmap);
            DeleteObject(hBitmap);

            // Return the captured image.
            return image;
        }



        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.UriSource = null;
                bitmapImage.StreamSource = memory;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // Freeze the image so it can be used safely in multiple threads
                return bitmapImage;
            }
        }

        #endregion

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
                string u3DFolderPath = System.IO.Path.Combine(currentDirectory, "Resources/BeatMosquitoes/");

                //String appStartupPath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                process.StartInfo.FileName = u3DFolderPath + @"\BeatMosquitoes.exe";
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
                u3dLeftUpPos = Panel2.TransformToAncestor(window).Transform(new Point(0, 0));
                u3dLeftUpPos = new Point(0, 0);
                DPIUtils.Init(this);
                //u3dLeftUpPos.X *= DPIUtils.DPIX;
                //u3dLeftUpPos.Y *= DPIUtils.DPIY;
                //MoveWindow(unityHWND, (int)u3dLeftUpPos.X, (int)u3dLeftUpPos.Y,
                //    (int)(Panel2.Width * DPIUtils.DPIX), (int)(Panel2.Height * DPIUtils.DPIY), false);
                MoveWindow(unityHWND, (int)u3dLeftUpPos.X, (int)u3dLeftUpPos.Y,
                    (int)(Panel2.Width), (int)(Panel2.Height), false);
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

    }
    public partial class Kill mosquitoes : BaseUserControl, IGameBase
    {


        protected override async Task OnInitAsync()
        {

            udpServer = new UdpServer();

            InitializeEyeTracker();

            udpSendMessageTimer = new DispatcherTimer();
            udpSendMessageTimer.Interval = TimeSpan.FromMilliseconds(50);
            udpSendMessageTimer.Tick += UdpSendMessageTimer_Tick;
            udpSendMessageTimer.Start();

            displayImageTimer = new DispatcherTimer();
            displayImageTimer.Interval = TimeSpan.FromMilliseconds(50);
            displayImageTimer.Tick += DisplayImageTimer_Tick;
        }

        protected override async Task OnStartAsync()
        {





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

                        displayImageTimer?.Start();
                    }
                };
                unityLoadTimer.Start(); // Start the timer



                return;
            }


        }


        protected override async Task OnStopAsync()
        {
            displayImageTimer?.Stop();

            CloseUnity();
            udpServer.Stop();
        }

        protected override async Task OnPauseAsync()
        {


        }

        protected override async Task OnNextAsync()
        {

        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        private async Task updateDataAsync()
        {

        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new Explanation of killing mosquitoes();
        }


    }
}
