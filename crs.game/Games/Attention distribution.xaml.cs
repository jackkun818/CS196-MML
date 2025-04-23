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
    public partial class Attention_distribution : BaseUserControl, IGameBase
    {
        public Attention_distribution()
        {
            InitializeComponent();
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
        int max_hardness = 1;
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
        private System.Windows.Point u3dLeftUpPos;

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
    public partial class Attention_distribution : BaseUserControl, IGameBase
    {


        protected override async Task OnInitAsync()
        {
            {
                level = 1;
                _totalTrainTime = 30 * 60;
                INCREASE = 95; // Increase the threshold for difficulty
                DECREASE = 80;  // Threshold for reducing difficulty
                _levelTime = 600;     //Level Duration
                signalDistance = 1;       //The distance between two signals
                hasTrainNoise = false;
                hasSoundResponse = false;   //Auditory feedback}
            }

            ///Get parameters from the front end
            var baseParameter = BaseParameter;
            if (baseParameter.ProgramModulePars != null && baseParameter.ProgramModulePars.Any())
            {
                Debug.WriteLine("ProgramModulePars Loaded data:");
                // Traversal ProgramModulePars Print out each parameter
                foreach (var par in baseParameter.ProgramModulePars)
                {
                    //Debug.WriteLine($"ProgramId: {par.ProgramId}, ModuleParId: {par.ModuleParId}, Value: {par.Value}, TableId: {par.TableId}");
                    if (par.ModulePar.ModuleId == baseParameter.ModuleId)
                    {
                        switch (par.ModuleParId) // Complete assignment
                        {
                            case 154: // Difficulty level
                                level = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"level={level}");
                                break;
                            case 28: // Treatment time
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
                                Debug.WriteLine($"Unprocessed ModuleParId: {par.ModuleParId}");
                                break;
                        }
                    }
                }
            }
            else
            {
                Debug.WriteLine("No data");
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

            VoiceTipAction?.Invoke("You need controls to keep the speed pointer in the dashboard within the green range；When the yellow signal light in the dashboard is on, pressOKkey；When the red brake signal light in the dashboard is on, pressOKKey brake；When a traffic light appears in the screen, it cannot pass when the red light is red.");
            SynopsisAction?.Invoke("Now you can see the screen on the screen, you need to control the speed of the train so that the speed pointer in the dashboard remains within the green range；When the yellow signal light in the dashboard is on, pressFkey；When the red brake signal light in the dashboard is on, pressOKKey brake；When a traffic light appears in the screen, please be careful not to pass when the red light is.");
            RuleAction?.Invoke("Now you can see the screen on the screen, you need to control the speed of the train so that the speed pointer in the dashboard remains within the green range；When the yellow signal light in the dashboard is on, pressOKkey；When the red brake signal light in the dashboard is on, pressOKKey brake；When a traffic light appears in the screen, please be careful not to pass when the red light is.");


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
                        displayImageTimer?.Start();


                        unityStartTimer.Stop(); // Stop the timer
                        unityStartTimer.Tick -= (s, args) => { }; // Unsubscribe from event to prevent memory leaks

                        BroadcastToClient(String.Format("StartGame,{0},{1},{2}", level, hasTrainNoise ? 1 : 0, hasSoundResponse ? 1 : 0));
                    }
                };
                unityStartTimer.Start(); // Start the timer

                _totalCountTimer?.Start();
                _levelCountTimer?.Start();

                return;
            }

            BroadcastToClient(String.Format("StartGame,{0},{1},{2}", level, hasTrainNoise ? 1 : 0, hasSoundResponse ? 1 : 0));

            _totalCountTimer?.Start();
            _levelCountTimer?.Start();
            RuleAction?.Invoke("To be supplemented");//Add code, call function, display the text under the digital person
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

            VoiceTipAction?.Invoke("You need controls to keep the speed pointer in the dashboard within the green range；When the yellow signal light in the dashboard is on, pressFkey；When the red brake signal light in the dashboard is on, press the spacebar to brake；When a traffic light appears in the screen, it cannot pass when the red light is red.");
            SynopsisAction?.Invoke("Now you can see the screen on the screen, you need to control the speed of the train so that the speed pointer in the dashboard remains within the green range；When the yellow signal light in the dashboard is on, pressFkey；When the red brake signal light in the dashboard is on, press the spacebar to brake；When a traffic light appears in the screen, please be careful not to pass when the red light is.");
            RuleAction?.Invoke("Now you can see the screen on the screen, you need to control the speed of the train so that the speed pointer in the dashboard remains within the green range；When the yellow signal light in the dashboard is on, pressFkey；When the red brake signal light in the dashboard is on, press the spacebar to brake；When a traffic light appears in the screen, please be careful not to pass when the red light is.");
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
                            Report = "Attention distribution",
                            Eval = false,
                            Lv = max_hardness,
                            //Otherwise it isnull, and before⾯offorThe loop needs to be removed
                            ScheduleId = BaseParameter.ScheduleId ?? null // 
                        }; db.Results.Add(newResult);
                        await db.SaveChangesAsync();
                        // get result_id
                        int result_id = newResult.ResultId;




                        // create ResultDetail Object List
                        //Please update different parameters below, and only change according to the data structure.resultDetail{}Internal
                        var resultDetails = new List<ResultDetail>
                            {
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "grade",
                                    Value = max_hardness ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Correct times",
                                    Value = correctCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Errors",
                                    Value = wrongCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Number of omissions",
                                    Value = ignoreCount ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Total times",
                                    Value = totalCount ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Correct rate",
                                    Value = accuracy,
                                    ModuleId = BaseParameter.ModuleId
                                },


                            };

                        // Insert⼊ ResultDetail data
                        db.ResultDetails.AddRange(resultDetails);
                        await db.SaveChangesAsync();
                        // Output each ResultDetail Object data
                        Debug.WriteLine($"Difficulty level {max_hardness}:");
                        foreach (var detail in resultDetails)
                        {

                            Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId}");

                        }


                        await transaction.CommitAsync();
                        Debug.WriteLine("Insert⼊success");
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

        protected override IGameBase OnGetExplanationExample()
        {
            return new Attention_distribution_explanation();
        }


    }
}

