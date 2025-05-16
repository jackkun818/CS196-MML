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
using System.Text.RegularExpressions;
using System.Drawing;
using Point = System.Windows.Point;

namespace crs.game.Games
{
    public partial class 注意力分配 : BaseUserControl, IGameBase
    {
        public 注意力分配()
        {
            InitializeComponent();
        }


        //建立socket连接
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
        int max_hardness = 1;
        int _totalTrainTime;
        private int INCREASE; // 提高难度的阈值
        private int DECREASE;  // 降低难度的阈值
        int _levelTime;     //等级持续时间
        int signalDistance;       //两个信号之间的距离
        bool hasTrainNoise;
        bool hasSoundResponse;   //听觉反馈

        /// <summary>
        /// 等待两秒后加载unity
        /// </summary>
        DispatcherTimer unityLoadTimer;
        DispatcherTimer unityStartTimer;

        /// <summary>
        /// 记录结果
        /// </summary>
        List<(int, Vector3)> resultsList;  //总，（正确，错误，遗漏）
        Vector3 lastResult = new Vector3();


        /// <summary>
        /// 记录总倒计时和等级时间
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
                //交互，获取结果
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
                    max_hardness = Math.Max(max_hardness, level);
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

                string[] re = message.Split(',');

                lastResult = new Vector3(int.Parse(re[0]), int.Parse(re[1]), int.Parse(re[2]));

                resultsList[level] = (resultsList[level].Item1 + int.Parse(re[3]),
                   resultsList[level].Item2 + new Vector3(int.Parse(re[0]), int.Parse(re[1]), int.Parse(re[2])));


                if (hasReceivedMessage == true)
                {
                    hasReceivedMessage = false;

                    AdjustLevel();

                    Next();
                }


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


        #region 显示画面到治疗师端

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
                bitmapImage.Freeze(); // 冻结图像，使其可以在多个线程中安全使用
                return bitmapImage;
            }
        }

        #endregion


        #region Unity操作

        [DllImport("User32.dll")]
        static extern bool MoveWindow(IntPtr handle, int x, int y, int width, int height, bool redraw);
        internal delegate int WindowEnumProc(IntPtr hwnd, IntPtr lparam);
        //改变指定窗口的位置和尺寸，基于左上角（屏幕/父窗口）（指定窗口的句柄，窗口左位置，窗口顶位置，窗口新宽度，窗口新高度，指定是否重画窗口）

        [DllImport("user32.dll")]
        internal static extern bool EnumChildWindows(IntPtr hwnd, WindowEnumProc func, IntPtr lParam);
        //枚举一个父窗口的所有子窗口（父窗口句柄，回调函数的地址，自定义的参数）

        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        //该函数将指定的消息发送到一个或多个窗口。此函数为指定的窗口调用窗口程序，
        //直到窗口程序处理完消息再返回。


        private Process process;
        private IntPtr unityHWND = IntPtr.Zero;
        private const int WM_ACTIVATE = 0x0006;
        private readonly IntPtr WA_ACTIVE = new IntPtr(1);
        private readonly IntPtr WA_INACTIVE = new IntPtr(0);
        private bool isU3DLoaded = false;
        private System.Windows.Point u3dLeftUpPos;

        private DispatcherTimer dispatcherTimer;

        private void LoadUnity()
        {
            try
            {
                //IntPtr hwnd = ((HwndSource)PresentationSource.FromVisual(Panel1)).Handle;
                IntPtr hwnd = Panel2.Handle;
                process = new Process();

                // 获取当前工作目录
                string currentDirectory = Directory.GetCurrentDirectory();
                string u3DFolderPath = System.IO.Path.Combine(currentDirectory, "Resources/注意力分配/Unity3/");

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
                u3dLeftUpPos = Panel2.TransformToAncestor(window).Transform(new Point(0, 0));
                u3dLeftUpPos = new Point(0, 0);
                DPIUtils.Init(this);
                //u3dLeftUpPos.X *= DPIUtils.DPIX;
                //u3dLeftUpPos.Y *= DPIUtils.DPIY;
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
                    // 尝试关闭 Unity 程序
                    process.CloseMainWindow();
                    // 等待 Unity 程序关闭
                    process.WaitForExit(1000); // 

                    // 如果 Unity 程序没有关闭，则强制结束进程
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                // 处理异常，例如 Unity 程序可能已经关闭
                string error = ex.Message;
            }
            finally
            {
                // 清理资源
                if (process != null)
                {
                    process.Dispose();
                    process = null;
                }
            }
        }
        #endregion


        #region 窗体位置坐标变换
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
    public partial class 注意力分配 : BaseUserControl, IGameBase
    {


        protected override async Task OnInitAsync()
        {
            {
                level = 1;
                _totalTrainTime = 30 * 60;
                INCREASE = 95; // 提高难度的阈值
                DECREASE = 80;  // 降低难度的阈值
                _levelTime = 600;     //等级持续时间
                signalDistance = 1;       //两个信号之间的距离
                hasTrainNoise = false;
                hasSoundResponse = false;   //听觉反馈}
            }

            ///从前端获取参数
            var baseParameter = BaseParameter;
            if (baseParameter.ProgramModulePars != null && baseParameter.ProgramModulePars.Any())
            {
                Debug.WriteLine("ProgramModulePars 已加载数据：");
                // 遍历 ProgramModulePars 打印出每个参数
                foreach (var par in baseParameter.ProgramModulePars)
                {
                    //Debug.WriteLine($"ProgramId: {par.ProgramId}, ModuleParId: {par.ModuleParId}, Value: {par.Value}, TableId: {par.TableId}");
                    if (par.ModulePar.ModuleId == baseParameter.ModuleId)
                    {
                        switch (par.ModuleParId) // 完成赋值
                        {
                            case 154: // 难度等级
                                level = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"level={level}");
                                break;
                            case 28: // 治疗时间
                                _totalTrainTime = par.Value.HasValue ? (int)(par.Value.Value * 60) : 30 * 60;
                                break;
                            case 29:
                                INCREASE = par.Value.HasValue ? (int)par.Value.Value * 20 : 4 * 20;
                                break;
                            case 30:
                                DECREASE = par.Value.HasValue ? (int)par.Value.Value * 20 : 3 * 20;
                                break;
                            case 31:
                                _levelTime = par.Value.HasValue ? (int)(par.Value.Value) : 600;
                                break;
                            case 32:
                                signalDistance = par.Value.HasValue ? (int)(par.Value.Value) : 1;
                                break;
                            case 33:
                                hasSoundResponse = par.Value.HasValue ? (bool)(par.Value.Value == 1) : false;
                                break;
                            case 34:
                                hasTrainNoise = par.Value.HasValue ? (bool)(par.Value.Value == 1) : false;
                                break;



                            default:
                                Debug.WriteLine($"未处理的 ModuleParId: {par.ModuleParId}");
                                break;
                        }
                    }
                }
            }
            else
            {
                Debug.WriteLine("没有数据");
            }

            ///
            totalCountTime = _totalTrainTime;
            levelCountTime = _levelTime;
            InitTimer();
            InitResultList();

            max_hardness = Math.Max(max_hardness, level);

            displayImageTimer = new DispatcherTimer();
            displayImageTimer.Interval = TimeSpan.FromMilliseconds(50);
            displayImageTimer.Tick += DisplayImageTimer_Tick;



            LevelStatisticsAction(level, 14);
        }

        protected override async Task OnStartAsync()
        {

            VoiceTipAction?.Invoke("您需要控制使仪表盘中的速度指针保持在绿色区间内；当仪表盘中的黄色信号灯亮起时，请按下OK键；当仪表盘中的红色刹车信号灯亮起时，请按下OK键刹车；当画面中出现红绿灯时，不能在红灯时通过。");
            SynopsisAction?.Invoke("现在您可以看到屏幕上的画面，您需要控制火车的速度，使仪表盘中的速度指针保持在绿色区间内；当仪表盘中的黄色信号灯亮起时，请按下F键；当仪表盘中的红色刹车信号灯亮起时，请按下OK键刹车；当画面中出现红绿灯时，请注意不能在红灯时通过。");
            RuleAction?.Invoke("现在您可以看到屏幕上的画面，您需要控制火车的速度，使仪表盘中的速度指针保持在绿色区间内；当仪表盘中的黄色信号灯亮起时，请按下OK键；当仪表盘中的红色刹车信号灯亮起时，请按下OK键刹车；当画面中出现红绿灯时，请注意不能在红灯时通过。");


            if (isStartListen == false)
            {
                isStartListen = true;
                StartListen();
            }

            if (gamePauseState == true)
            {
                BroadcastToClient("ContinueGame");
                gamePauseState = false;
            }

            if (isU3DLoaded == false)
            {
                unityLoadTimer = new DispatcherTimer();
                unityLoadTimer.Interval = TimeSpan.FromMilliseconds(20); // 
                unityLoadTimer.Tick += (sender, e) =>
                {

                    if (PresentationSource.FromVisual(Panel2) != null)
                    {
                        unityLoadTimer.Stop(); // 停止定时器
                        unityLoadTimer.Tick -= (s, args) => { }; // 取消事件订阅，防止内存泄漏

                        LoadUnity();
                    }
                };
                unityLoadTimer.Start(); // 启动定时器


                unityStartTimer = new DispatcherTimer();
                unityStartTimer.Interval = TimeSpan.FromMilliseconds(20); // 
                unityStartTimer.Tick += (sender, e) =>
                {
                    if (streamList.Count > 0)
                    {
                        displayImageTimer?.Start();


                        unityStartTimer.Stop(); // 停止定时器
                        unityStartTimer.Tick -= (s, args) => { }; // 取消事件订阅，防止内存泄漏

                        BroadcastToClient(String.Format("StartGame,{0},{1},{2}", level, hasTrainNoise ? 1 : 0, hasSoundResponse ? 1 : 0));
                    }
                };
                unityStartTimer.Start(); // 启动定时器

                _totalCountTimer?.Start();
                _levelCountTimer?.Start();

                return;
            }

            BroadcastToClient(String.Format("StartGame,{0},{1},{2}", level, hasTrainNoise ? 1 : 0, hasSoundResponse ? 1 : 0));

            _totalCountTimer?.Start();
            _levelCountTimer?.Start();
            RuleAction?.Invoke("待补充");//增加代码，调用函数，显示数字人下的文字
        }


        protected override async Task OnStopAsync()
        {

            _totalCountTimer?.Stop();
            _levelCountTimer?.Stop();
            displayImageTimer?.Stop();

            tcpListener?.Stop();
            tcpListener?.Dispose();
            StopClient();

            CloseUnity();

            
        }

        protected override async Task OnPauseAsync()
        {

            BroadcastToClient("PauseGame");
            gamePauseState = true;

            _totalCountTimer?.Stop();
            _levelCountTimer?.Stop();

        }

        protected override async Task OnNextAsync()
        {
            /////
            BroadcastToClient(String.Format("StartGame,{0},{1},{2}", level, hasTrainNoise ? 1 : 0, hasSoundResponse ? 1 : 0));

            VoiceTipAction?.Invoke("您需要控制使仪表盘中的速度指针保持在绿色区间内；当仪表盘中的黄色信号灯亮起时，请按下F键；当仪表盘中的红色刹车信号灯亮起时，请按下空格键刹车；当画面中出现红绿灯时，不能在红灯时通过。");
            SynopsisAction?.Invoke("现在您可以看到屏幕上的画面，您需要控制火车的速度，使仪表盘中的速度指针保持在绿色区间内；当仪表盘中的黄色信号灯亮起时，请按下F键；当仪表盘中的红色刹车信号灯亮起时，请按下空格键刹车；当画面中出现红绿灯时，请注意不能在红灯时通过。");
            RuleAction?.Invoke("现在您可以看到屏幕上的画面，您需要控制火车的速度，使仪表盘中的速度指针保持在绿色区间内；当仪表盘中的黄色信号灯亮起时，请按下F键；当仪表盘中的红色刹车信号灯亮起时，请按下空格键刹车；当画面中出现红绿灯时，请注意不能在红灯时通过。");
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
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
                        int correctCount = 0;
                        int wrongCount = 0;
                        int ignoreCount = 0;
                        int totalCount = 0;

                        for (int lv = 1; lv <= max_hardness; lv++)
                        {
                            correctCount += (int)resultsList[lv].Item2.X;
                            wrongCount += (int)resultsList[lv].Item2.Y;
                            ignoreCount += (int)resultsList[lv].Item2.Z;
                        }
                        totalCount = correctCount + wrongCount + ignoreCount;
                        double accuracy = 0;
                        if (totalCount > 0)
                        {
                            accuracy = correctCount / totalCount;
                        }

                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "注意力分配",
                            Eval = false,
                            Lv = max_hardness,
                            //否则为null，且前⾯的for循环要去掉
                            ScheduleId = BaseParameter.ScheduleId ?? null // 
                        }; db.Results.Add(newResult);
                        await db.SaveChangesAsync();
                        // 获得 result_id
                        int result_id = newResult.ResultId;




                        // 创建 ResultDetail 对象列表
                        //更新不同参数请在底下更新，根据数据结构，只更改resultDetail{}内的
                        var resultDetails = new List<ResultDetail>
                            {
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "等级",
                                    Value = max_hardness ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "正确次数",
                                    Value = correctCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "错误次数",
                                    Value = wrongCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "遗漏次数",
                                    Value = ignoreCount ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "总次数",
                                    Value = totalCount ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "正确率",
                                    Value = accuracy,
                                    ModuleId = BaseParameter.ModuleId
                                },


                            };

                        // 插⼊ ResultDetail 数据
                        db.ResultDetails.AddRange(resultDetails);
                        await db.SaveChangesAsync();
                        // 输出每个 ResultDetail 对象的数据
                        Debug.WriteLine($"难度级别 {max_hardness}:");
                        foreach (var detail in resultDetails)
                        {

                            Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId}");

                        }


                        await transaction.CommitAsync();
                        Debug.WriteLine("插⼊成功");
                    });
                }
                catch (Exception ex)
                {
                    // 回滚事务
                    await transaction.RollbackAsync();
                    Debug.WriteLine(ex.ToString());
                }
            }
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new 注意力分配讲解();
        }


    }
}

