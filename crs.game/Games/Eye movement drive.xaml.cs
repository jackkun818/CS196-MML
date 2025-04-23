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
using System.Windows.Ink;
using System.Net.Sockets;
using System.Net;
using Newtonsoft.Json;

namespace crs.game.Games
{

    public partial class Eye_movement_drive : BaseUserControl, IGameBase
    {
        public Eye_movement_drive()
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
                    Console.WriteLine("Error broadcasting data: " + ex.Message);
                }
            }

            public void Stop()
            {
                // closureUdpClientFree up resources
                udpClient?.Close();
            }
        }
        public class TcpServer
        {
            private TcpListener listener;
            private bool isRunning;
            private List<TcpClient> clients;
            private Thread thread;

            public TcpServer()
            {
                listener = new TcpListener(IPAddress.Any, 7001); // 
                clients = new List<TcpClient>();
            }

            public void StartListen()
            {
                if (!isRunning)
                {
                    isRunning = true;
                    thread = new Thread(ListenForClients);
                    thread.Start();
                }
            }

            private void ListenForClients()
            {
                listener.Start();
                while (isRunning)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    lock (clients)
                    {
                        clients.Add(client);
                    }
                }
            }

            public void BroadcastMessage(string message)
            {

                lock (clients)
                {
                    foreach (TcpClient client in clients.ToArray()) // useToArray()Prevent exceptions when modifying collections
                    {
                        try
                        {
                            NetworkStream stream = client.GetStream();
                            byte[] buffer = Encoding.ASCII.GetBytes(message);
                            stream.Write(buffer, 0, buffer.Length);
                        }
                        catch
                        {
                            // If an exception occurs, remove the client
                            lock (clients)
                            {
                                clients.Remove(client);
                            }
                        }
                    }
                }

            }

            public void Stop()
            {
                isRunning = false;
                listener.Stop();
                thread.Join();
                foreach (TcpClient client in clients)
                {
                    client.Close();
                }
            }
        }

        TcpServer tcpServer;
        UdpServer udpServer;
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

        public class ScoreDistribution
        {
            public float A, B, C, D, E, Count;
            public ScoreDistribution()
            {
                A = 0;
                B = 0;
                C = 0;
                D = 0;
                E = 0;
                Count = 0;
            }
        }

        /// <summary>
        /// parameter
        /// </summary>
        int level;      //Game Level
        bool hasBGM;     //Background sound
        int trainTime;      //Training time(s)
        double averageScore;       //Average score
        double gazeRatio;       //Gaze proportions
        double gazeDistance;       //Gaze distance
        bool useRandomCoordinates;

        //Score...
        List<Vector2> listTargetPoint;   //Storage target points
        List<Vector2> listGazePoint;    //Store gaze points

        List<double> recent5Scores;    //Last five scores
        List<double> averageScoreOfPerLevel;  //Average score for each difficulty,Calculate based on the score distribution
        List<Vector3> gazeRateOfPerLevel;     //The gazing ratio of each difficulty,X=Proficient in gaze point,Y=Total points,Z=Gaze proportions
        List<ScoreDistribution> scoreDistributions;   //Score distribution for each difficulty
        List<Vector2> errorPointRate;       //Error rate,X: The number of error points, Y:total
        List<Vector2> averageTime;        //Average time,X： Total time to use,Y : Total number of trainings

        private int windowSizeError = 0; //Screen Error


        private DispatcherTimer _timerMoveImage;   //Control image movement
        private const int moveInterval = 30; // Moving interval（millisecond）
        private const int moveSpeed = 5; // Number of pixels per move
        double maxMoveDistance = 80;   //The maximum distance that can be moved
        int t = 0; // The time has passed for the current question（millisecond）

        private DispatcherTimer _timerCollectePoint;  //Collect samples
        int collectInterval = 250;    // （millisecond）

        private DispatcherTimer _timerCurGaze;  //Show current line of sight position

        private DispatcherTimer _timerStart123;  //Countdown to 123 of the start game
        int timeStart123 = 3;

        private DispatcherTimer _totalTimerTrain;  //Total treatment time
        private DispatcherTimer _levelTimerTrain;  //Current difficulty treatment time
        int totalTrainTime; //Total treatment time
        int levelTrainTime; //Current difficulty treatment time

        private DispatcherTimer _timerNextRound;  //Next question
        int timerNextRound = 5;  //Second


        Random random;



        /// <summary>
        ///  Difficulty level related parameters
        /// </summary>
        bool isTrajectoryCurved;   //Is the track curved?
        int trajectoryTypes;  //Trajectory description, 1: Flat  2: Slant  3: Very oblique 4: Up and down/Bottom top  5: Up and down/Down up and down
        int trajectoryWidthTypes;  //Trajectory width and narrow   1: Width  2: Medium  3: Narrow
        bool isTrajectoryPosChanged;  //Does the track position change



        Ellipse gaze;//
        Image ball;
        TextBlock textBlock123;  ///


        /// <summary>
        /// Generate track-related parameters
        /// </summary>
        double startPoint_X;
        double startPoint_Y;
        double endPoint_X;
        double trend_direction = 0;

        double trajectoryHeight = 140;
        double trajectoryHeightDelta = 20;


        double curGaze_X = 0, curGaze_Y = 0;

        bool useMousePos = true;
        double mouse_X = 0, mouse_Y = 0;


        void SetLevelParameter(int level = 1)
        {
            switch (level)
            {
                case 1:
                    isTrajectoryCurved = false; trajectoryTypes = 1; trajectoryWidthTypes = 1;
                    isTrajectoryPosChanged = false;
                    break;
                case 2:
                    isTrajectoryCurved = false; trajectoryTypes = 1; trajectoryWidthTypes = 1;
                    isTrajectoryPosChanged = true;
                    break;
                case 3:
                    isTrajectoryCurved = false; trajectoryTypes = 2; trajectoryWidthTypes = 1;
                    isTrajectoryPosChanged = true;
                    break;
                case 4:
                    isTrajectoryCurved = false; trajectoryTypes = 3; trajectoryWidthTypes = 2;
                    isTrajectoryPosChanged = true;
                    break;
                case 5:
                    isTrajectoryCurved = true; trajectoryTypes = 4; trajectoryWidthTypes = 1;
                    isTrajectoryPosChanged = false;
                    break;
                case 6:
                    isTrajectoryCurved = true; trajectoryTypes = 4; trajectoryWidthTypes = 1;
                    isTrajectoryPosChanged = true;
                    break;
                case 7:
                    isTrajectoryCurved = true; trajectoryTypes = 4; trajectoryWidthTypes = 2;
                    isTrajectoryPosChanged = true;
                    break;
                case 8:
                    isTrajectoryCurved = true; trajectoryTypes = 5; trajectoryWidthTypes = 1;
                    isTrajectoryPosChanged = true;
                    break;
                case 9:
                    isTrajectoryCurved = true; trajectoryTypes = 5; trajectoryWidthTypes = 2;
                    isTrajectoryPosChanged = true;
                    break;
                case 10:
                    isTrajectoryCurved = true; trajectoryTypes = 5; trajectoryWidthTypes = 3;
                    isTrajectoryPosChanged = true;
                    break;

            }

        }

        void InitCanvas()
        {



            ImageBrush backgroundBrush = new ImageBrush();

            //Control music playback
            mediaPlayer.Source = new Uri("Resources/Eye movement drive/3.WAV", UriKind.RelativeOrAbsolute);


            // Create a BitmapImage Object and set decoding
            BitmapImage bitmapBG = new BitmapImage();
            bitmapBG.BeginInit();
            bitmapBG.UriSource = new Uri("Resources/Eye movement drive/1.jpg", UriKind.Relative); // Image resource path
            bitmapBG.CacheOption = BitmapCacheOption.OnLoad; // Optimized loading performance
            bitmapBG.EndInit();
            // set up ImageBrush of ImageSource property
            backgroundBrush.ImageSource = bitmapBG;
            // set up Canvas of Background property
            canvas.Background = backgroundBrush;


            if (trajectoryTypes == 1)
            {
                DrawTrajectory1();
            }
            else if (trajectoryTypes == 2)
            {
                DrawTrajectory2();
            }
            else if (trajectoryTypes == 3)
            {
                DrawTrajectory3();
            }
            else if (trajectoryTypes == 4)
            {
                DrawTrajectory4();
            }
            else if (trajectoryTypes == 5)
            {
                DrawTrajectory5();
            }


            // Create a circle to represent the current line of sight position
            gaze = new Ellipse
            {
                Width = 30,  // The width of the dot
                Height = 30, // The height of the dot
                Stroke = Brushes.Red, // Border color
                StrokeThickness = 1 // Border thickness
            };
            Canvas.SetLeft(gaze, 200);
            Canvas.SetTop(gaze, 200);
            canvas.Children.Add(gaze);

            ////
            //ball = new Ellipse
            //{
            //    Width = 40,  // The width of the dot
            //    Height = 40, // The height of the dot
            //    Stroke = Brushes.Blue, // Border color
            //    StrokeThickness = 1, // Border thickness
            //    Fill = new SolidColorBrush(Colors.Blue),
            //};
            //Canvas.SetLeft(ball, startPoint_X + 5);
            //Canvas.SetTop(ball, startPoint_Y + 20);
            //canvas.Children.Add(ball);


            //////
            ball = new Image();
            // Create a BitmapImage Object and set decoding
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri("Resources/Eye movement drive/2.jpg", UriKind.Relative); // Image resource path
            bitmap.CacheOption = BitmapCacheOption.OnLoad; // Optimized loading performance
            bitmap.EndInit();
            ball.Source = bitmap;
            ball.Width = 40; // Set width
            ball.Height = 40; // Set height
            Canvas.SetLeft(ball, startPoint_X + 5);
            Canvas.SetTop(ball, startPoint_Y + 20);
            canvas.Children.Add(ball);



            ////
            textBlock123 = new TextBlock();
            textBlock123.Text = "3";
            textBlock123.FontSize = 40;
            textBlock123.FontWeight = FontWeights.Bold;
            textBlock123.Foreground = new SolidColorBrush(Colors.Black);
            Canvas.SetLeft(textBlock123, 400);
            Canvas.SetTop(textBlock123, 150);
            canvas.Children.Add(textBlock123);

        }

        void DrawTrajectory1()
        {
            if (isTrajectoryPosChanged)
            {
                int tmp = random.Next(0, 4);
                startPoint_X = 50 + 40 * tmp;
                tmp = random.Next(0, 4);
                startPoint_Y = 150 + 30 * tmp;
                tmp = random.Next(0, 4);
                endPoint_X = 750 - 40 * tmp;

                trend_direction = 2 * (random.Next(0, 2) - 0.5);

            }
            else
            {
                startPoint_X = 100;
                endPoint_X = 700;
                startPoint_Y = 200;
                trend_direction = 2 * (random.Next(0, 2) - 0.5);
            }
            ////
            startPoint_X = 0;
            endPoint_X = 800;
            for (double i = startPoint_X; i < endPoint_X; i++)
            {
                Rectangle tmpRectangle = new Rectangle();
                tmpRectangle.Width = 4;
                tmpRectangle.Height = trajectoryHeight - trajectoryHeightDelta * trajectoryWidthTypes;
                tmpRectangle.Fill = new SolidColorBrush(Colors.Khaki);
                tmpRectangle.Stroke = Brushes.Khaki; // Border color
                tmpRectangle.StrokeThickness = 1; // Border thickness

                // WillRectanglePlaced inCanvassuperior
                Canvas.SetLeft(tmpRectangle, i);
                Canvas.SetTop(tmpRectangle, startPoint_Y);

                // WillRectangleAdd toCanvasin the collection of child elements
                canvas.Children.Add(tmpRectangle);
            }
        }
        void DrawTrajectory2()
        {
            if (isTrajectoryPosChanged)
            {
                int tmp = random.Next(0, 4);
                startPoint_X = 50 + 40 * tmp;
                tmp = random.Next(0, 3);
                startPoint_Y = 160 + 20 * tmp;
                tmp = random.Next(0, 4);
                endPoint_X = 750 - 40 * tmp;

                trend_direction = 2 * (random.Next(0, 2) - 0.5);


            }
            else
            {
                startPoint_X = 100;
                endPoint_X = 700;
                startPoint_Y = 200;
                trend_direction = 2 * (random.Next(0, 2) - 0.5);
            }
            ////
            startPoint_X = 0;
            endPoint_X = 800;
            for (double i = startPoint_X; i < endPoint_X; i++)
            {
                Rectangle tmpRectangle = new Rectangle();
                tmpRectangle.Width = 4;
                tmpRectangle.Height = trajectoryHeight - trajectoryHeightDelta * trajectoryWidthTypes;
                tmpRectangle.Fill = new SolidColorBrush(Colors.Khaki);
                tmpRectangle.Stroke = Brushes.Khaki; // Border color
                tmpRectangle.StrokeThickness = 1; // Border thickness

                // WillRectanglePlaced inCanvassuperior
                Canvas.SetLeft(tmpRectangle, i);
                Canvas.SetTop(tmpRectangle, startPoint_Y + trend_direction * (i - startPoint_X) * 0.15);

                // WillRectangleAdd toCanvasin the collection of child elements
                canvas.Children.Add(tmpRectangle);
            }
        }
        void DrawTrajectory3()
        {
            if (isTrajectoryPosChanged)
            {
                int tmp = random.Next(0, 4);
                startPoint_X = 50 + 40 * tmp;
                tmp = random.Next(0, 4);
                startPoint_Y = 150 + 30 * tmp;
                tmp = random.Next(0, 4);
                endPoint_X = 750 - 40 * tmp;

                trend_direction = 2 * (random.Next(0, 2) - 0.5);
                if (trend_direction == 1)
                {
                    tmp = random.Next(2, 4);
                    startPoint_Y = 30 * tmp;
                }
                else
                {
                    tmp = random.Next(0, 2);
                    startPoint_Y = 450 - 120 - 30 * tmp;
                }

            }
            else
            {
                startPoint_X = 100;
                endPoint_X = 700;
                startPoint_Y = 200;
                trend_direction = 2 * (random.Next(0, 2) - 0.5);
            }
            ////
            startPoint_X = 0;
            endPoint_X = 800;

            for (double i = startPoint_X; i < endPoint_X; i++)
            {
                Rectangle tmpRectangle = new Rectangle();
                tmpRectangle.Width = 4;
                tmpRectangle.Height = trajectoryHeight - trajectoryHeightDelta * trajectoryWidthTypes;
                tmpRectangle.Fill = new SolidColorBrush(Colors.Khaki);
                tmpRectangle.Stroke = Brushes.Khaki; // Border color
                tmpRectangle.StrokeThickness = 1; // Border thickness

                // WillRectanglePlaced inCanvassuperior
                Canvas.SetLeft(tmpRectangle, i);
                Canvas.SetTop(tmpRectangle, startPoint_Y + trend_direction * (i - startPoint_X) * 0.3);

                // WillRectangleAdd toCanvasin the collection of child elements
                canvas.Children.Add(tmpRectangle);
            }
        }
        void DrawTrajectory4()
        {
            if (isTrajectoryPosChanged)
            {
                int tmp = random.Next(0, 4);
                startPoint_X = 50 + 40 * tmp;
                tmp = random.Next(0, 3);
                startPoint_Y = 150 + 30 * tmp;
                tmp = random.Next(0, 4);
                endPoint_X = 750 - 40 * tmp;

                trend_direction = 2 * (random.Next(0, 2) - 0.5);

            }
            else
            {
                startPoint_X = 100;
                endPoint_X = 700;
                startPoint_Y = 200;
                trend_direction = 2 * (random.Next(0, 2) - 0.5);


            }
            ////
            startPoint_X = 0;
            endPoint_X = 800;

            for (double i = startPoint_X; i < endPoint_X; i++)
            {
                Rectangle tmpRectangle = new Rectangle();
                tmpRectangle.Width = 4;
                tmpRectangle.Height = trajectoryHeight - trajectoryHeightDelta * trajectoryWidthTypes;
                tmpRectangle.Fill = new SolidColorBrush(Colors.Khaki);
                tmpRectangle.Stroke = Brushes.Khaki; // Border color
                tmpRectangle.StrokeThickness = 1; // Border thickness

                // WillRectanglePlaced inCanvassuperior
                Canvas.SetLeft(tmpRectangle, i);
                Canvas.SetTop(tmpRectangle, startPoint_Y + trend_direction * 100 * Math.Sin(Math.PI * ((float)(i - startPoint_X) / (endPoint_X - startPoint_X))));

                // WillRectangleAdd toCanvasin the collection of child elements
                canvas.Children.Add(tmpRectangle);
            }
        }
        void DrawTrajectory5()
        {
            if (isTrajectoryPosChanged)
            {
                int tmp = random.Next(0, 4);
                startPoint_X = 50 + 40 * tmp;
                tmp = random.Next(0, 3);
                startPoint_Y = 150 + 30 * tmp;
                tmp = random.Next(0, 4);
                endPoint_X = 750 - 40 * tmp;

                trend_direction = 2 * (random.Next(0, 2) - 0.5);



            }
            else
            {
                startPoint_X = 100;
                endPoint_X = 700;
                startPoint_Y = 200;
                trend_direction = 2 * (random.Next(0, 2) - 0.5);
            }
            ////
            startPoint_X = 0;
            endPoint_X = 800;

            for (double i = startPoint_X; i < endPoint_X; i++)
            {
                Rectangle tmpRectangle = new Rectangle();
                tmpRectangle.Width = 4;
                tmpRectangle.Height = trajectoryHeight - trajectoryHeightDelta * trajectoryWidthTypes;
                tmpRectangle.Fill = new SolidColorBrush(Colors.Khaki);
                tmpRectangle.Stroke = Brushes.Khaki; // Border color
                tmpRectangle.StrokeThickness = 1; // Border thickness

                // WillRectanglePlaced inCanvassuperior
                Canvas.SetLeft(tmpRectangle, i);
                Canvas.SetTop(tmpRectangle, startPoint_Y + trend_direction * 100 * Math.Sin(2 * Math.PI * ((float)(i - startPoint_X) / (endPoint_X - startPoint_X))));

                // WillRectangleAdd toCanvasin the collection of child elements
                canvas.Children.Add(tmpRectangle);
            }
        }


        void ClearCanvas()
        {
            canvas.Children.Clear();
        }

        void NextRound()
        {
            listGazePoint.Clear();
            listTargetPoint.Clear();

            //Adjust the level
            AdjustLevel();
            //Reset parameters
            SetLevelParameter(level);
            levelTrainTime = 0;
            timeStart123 = 3;

            t = 0;

            ClearCanvas();


            //Loading pictures
            InitCanvas();

            _timerStart123?.Start();
        }

        Vector2 GetGazeCoordinates()
        {
            int x, y;
            if (useRandomCoordinates)
            {
                EyeTrackerWrapper.GetRandomCoordinates(out x, out y);
            }
            else
            {
                EyeTrackerWrapper.GetGazeCoordinates(out x, out y);
            }
            return new Vector2(x, y);
        }
        void AdjustLevel()
        {
            //Adjust the game level
            if (recent5Scores.Count >= 5)
            {
                if (recent5Scores.Sum() / recent5Scores.Count >= 75)
                {
                    level++;
                    if (level > 10)
                    {
                        level = 10;
                    }
                    recent5Scores.Clear();
                }
                else if (recent5Scores.Sum() / recent5Scores.Count <= 25)
                {
                    level--;
                    if (level < 1)
                    {
                        level = 1;
                    }
                    recent5Scores.Clear();
                }
            }
            LevelStatisticsAction?.Invoke(level, 10);
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

        void InitializeList()
        {

            recent5Scores = new List<double>();    //Last five scores
            averageScoreOfPerLevel = new List<double>();  //Average score for each difficulty
            for (int i = 0; i < 11; i++)
            {
                averageScoreOfPerLevel.Add(0);
            }
            gazeRateOfPerLevel = new List<Vector3>();     //Gaze ratio for each difficulty
            for (int i = 0; i < 11; i++)
            {
                gazeRateOfPerLevel.Add(new Vector3(0, 0, 0));
            }
            scoreDistributions = new List<ScoreDistribution>();   //Score distribution for each difficulty
            for (int i = 0; i < 11; i++)
            {
                scoreDistributions.Add(new ScoreDistribution());
            }
            errorPointRate = new List<Vector2>();    //Error rate for each difficulty
            for (int i = 0; i < 11; i++)
            {
                errorPointRate.Add(new Vector2(0, 0));
            }
            averageTime = new List<Vector2>();    //Average time for each difficulty
            for (int i = 0; i < 11; i++)
            {
                averageTime.Add(new Vector2(0, 0));
            }


        }


        private void InitializeTimer()
        {
            //await Task.Delay(TimeSpan.FromSeconds(1));

            _timerMoveImage = new DispatcherTimer();
            _timerMoveImage.Interval = TimeSpan.FromMilliseconds(moveInterval);
            _timerMoveImage.Tick += TimerMoveImage_Tick;

            _timerCollectePoint = new DispatcherTimer();
            _timerCollectePoint.Interval = TimeSpan.FromMilliseconds(collectInterval);
            _timerCollectePoint.Tick += TimerCollect_Tick;

            _timerCurGaze = new DispatcherTimer();
            _timerCurGaze.Interval = TimeSpan.FromMilliseconds(moveInterval);
            _timerCurGaze.Tick += TimerMoveCurGaze_Tick;


            _timerStart123 = new DispatcherTimer();
            _timerStart123.Interval = TimeSpan.FromSeconds(1);
            timeStart123 = 3;
            _timerStart123.Tick += TimerStart123_Tick;

            _totalTimerTrain = new DispatcherTimer();
            _totalTimerTrain.Interval = TimeSpan.FromSeconds(1);
            _totalTimerTrain.Tick += TimerTotalTrain_Tick;

            _levelTimerTrain = new DispatcherTimer();
            _levelTimerTrain.Interval = TimeSpan.FromSeconds(1);
            _levelTimerTrain.Tick += TimerLevelTrain_Tick;

            _timerNextRound = new DispatcherTimer();
            _timerNextRound.Interval = TimeSpan.FromSeconds(1);
            _timerNextRound.Tick += _timerNextRound_Tick;
        }


        void CalculateScore()
        {
            //Calculate error rate
            int tmpSum = 0;
            for (int i = 0; i < listTargetPoint.Count; i++)
            {
                var targetPoint = listTargetPoint[i];
                var gazePoint = listGazePoint[i];
                double distance = Vector2.Distance(targetPoint, gazePoint);

                if (distance > maxMoveDistance)
                {
                    tmpSum++;
                }

            }
            errorPointRate[level] += new Vector2(tmpSum, listTargetPoint.Count);

            //Record average time
            averageTime[level] += new Vector2((int)t / 1000, 1);

            //Current question score
            if (levelTrainTime <= 15)
            {
                averageScore = 100;
                scoreDistributions[level].A++;
            }
            else if (levelTrainTime <= 30)
            {
                averageScore = 75;
                scoreDistributions[level].B++;
            }
            else if (levelTrainTime <= 45)
            {
                averageScore = 50;
                scoreDistributions[level].C++;
            }
            else if (levelTrainTime <= 60)
            {
                averageScore = 25;
                scoreDistributions[level].D++;
            }
            else
            {
                averageScore = 0;
                scoreDistributions[level].E++;
            }
            scoreDistributions[level].Count++;
            averageScoreOfPerLevel[level] = 0.5 * (scoreDistributions[level].A * 100 + scoreDistributions[level].B * 75
                + scoreDistributions[level].C * 50 + scoreDistributions[level].E * 25) / scoreDistributions[level].Count
                + 0.5 * 100 * errorPointRate[level].X / errorPointRate[level].Y;

            recent5Scores.Add(averageScore);
            if (recent5Scores.Count > 5)
            {
                recent5Scores.RemoveAt(0);
            }



            //Calculate the gaze ratio
            int flag = 0, sum = 0;
            for (int i = 1; i < listGazePoint.Count; i++)
            {
                if (Vector2.Distance(listGazePoint[i - 1], listGazePoint[i]) < gazeDistance)
                {
                    flag += 1;
                }
                else
                {
                    if (flag >= 5)
                    {
                        sum += flag;
                    }
                    flag = 0;
                }
            }
            gazeRatio = (double)sum / listGazePoint.Count;
            Vector3 tmp = gazeRateOfPerLevel[level] + new Vector3(sum, listGazePoint.Count, 0); ;
            tmp.Z = tmp.X / tmp.Y;
            gazeRateOfPerLevel[level] = tmp;



            ///
            listGazePoint.Clear();
            listTargetPoint.Clear();
        }

        void ShowAllGazePoint()
        {
            foreach (var item in listGazePoint)
            {
                ShowGazeDot(item.X, item.Y);
            }
        }

        private void ShowGazeDot(double x, double y)
        {
            // Create an ellipse that represents a small blue dot
            Ellipse blueDot = new Ellipse
            {
                Width = 10,  // The width of the dot
                Height = 10, // The height of the dot
                Fill = Brushes.LightSkyBlue, // Fill color
                Stroke = Brushes.Black, // Border color
                StrokeThickness = 1 // Border thickness
            };

            if ((x - blueDot.Width / 2 > 0) && (x + blueDot.Width / 2 < canvas.Width) &&
                (y - blueDot.Height / 2 > 0) && (y + blueDot.Height / 2 < canvas.Height))
            {
                // Place the ellipse on the specified coordinates
                Canvas.SetLeft(blueDot, x - blueDot.Width / 2);
                Canvas.SetTop(blueDot, y - blueDot.Height / 2);
                // Add small blue dots to Canvas superior
                canvas.Children.Add(blueDot);
            }
        }

        void StartLevelTimer()
        {
            _timerCollectePoint?.Start();
            _timerMoveImage?.Start();
            _timerCurGaze?.Start();

            _levelTimerTrain?.Start();
            //_totalTimerTrain?.Start();
        }

        void StartGlobalTimer()
        {
            _timerStart123?.Start();
            _timerCurGaze?.Start();
            _totalTimerTrain?.Start();
        }

        void StopAllTimer()
        {
            _timerCollectePoint?.Stop();
            _timerMoveImage?.Stop();
            _timerCurGaze?.Stop();

            _totalTimerTrain?.Stop();
            _levelTimerTrain?.Stop();
            _timerStart123?.Stop();
        }

        void StopLevelTimer()
        {
            _timerCollectePoint?.Stop();
            _timerMoveImage?.Stop();
            _timerCurGaze?.Stop();
            _levelTimerTrain?.Stop();

        }


        bool isPassable(double newX, double newY)
        {
            double tmpHeight = trajectoryHeight - trajectoryHeightDelta * trajectoryWidthTypes;
            if (newX < startPoint_X || newX > endPoint_X)
            {
                return false;
            }

            if (trajectoryTypes == 1)
            {
                if (newY > startPoint_Y && newY < startPoint_Y + tmpHeight)
                {
                    return true;
                }

            }
            else if (trajectoryTypes == 2)
            {
                if (newY > startPoint_Y + trend_direction * (newX - startPoint_X) * 0.15 &&
                    newY < startPoint_Y + trend_direction * (newX - startPoint_X) * 0.15 + tmpHeight)
                {
                    return true;
                }
            }
            else if (trajectoryTypes == 3)
            {
                if (newY > startPoint_Y + trend_direction * (newX - startPoint_X) * 0.3 &&
                    newY < startPoint_Y + trend_direction * (newX - startPoint_X) * 0.3 + tmpHeight)
                {
                    return true;
                }
            }
            else if (trajectoryTypes == 4)
            {
                if (newY > startPoint_Y + trend_direction * 100 * Math.Sin(Math.PI * ((float)(newX - startPoint_X) / (endPoint_X - startPoint_X))) &&
                    newY < startPoint_Y + trend_direction * 100 * Math.Sin(Math.PI * ((float)(newX - startPoint_X) / (endPoint_X - startPoint_X))) + tmpHeight)
                {
                    return true;
                }
            }
            else if (trajectoryTypes == 5)
            {
                if (newY > startPoint_Y + trend_direction * 100 * Math.Sin(2 * Math.PI * ((float)(newX - startPoint_X) / (endPoint_X - startPoint_X))) &&
                    newY < startPoint_Y + trend_direction * 100 * Math.Sin(2 * Math.PI * ((float)(newX - startPoint_X) / (endPoint_X - startPoint_X))) + tmpHeight)
                {
                    return true;
                }
            }
            return false;
        }


        private void _timerNextRound_Tick(object sender, EventArgs e)
        {
            timerNextRound--;
            if (timerNextRound <= 0)
            {
                _timerNextRound?.Stop();
                timerNextRound = 5;
                NextRound();
            }
        }
        void TimerTotalTrain_Tick(object sender, EventArgs e)
        {
            totalTrainTime--;
            if (totalTrainTime <= 0)
            {
                Stop();
            }

            TimeStatisticsAction?.Invoke(totalTrainTime, levelTrainTime);
        }

        /// <summary>
        /// Record the current question time
        /// </summary>
        void TimerLevelTrain_Tick(object sender, EventArgs e)
        {
            levelTrainTime++;

        }



        private void TimerMoveImage_Tick(object sender, EventArgs e)
        {
            t += moveInterval;

            double curX = Canvas.GetLeft(ball) + ball.Width / 2;
            double curY = Canvas.GetTop(ball) + ball.Height / 2;
            Vector2 curPos = new Vector2((float)curX, (float)curY);
            Vector2 newPos = new Vector2((float)curGaze_X, (float)curGaze_Y);
            if (Vector2.Distance(newPos, curPos) < moveSpeed)
            {
                return;
            }
            if (Vector2.Distance(newPos, curPos) < maxMoveDistance)
            {

                curX += moveSpeed * ((curGaze_X - curX) / Vector2.Distance(newPos, curPos));
                curY += moveSpeed * ((curGaze_Y - curY) / Vector2.Distance(newPos, curPos));

                if (isPassable(curX, curY))
                {
                    Canvas.SetLeft(ball, curX - ball.Width / 2);
                    Canvas.SetTop(ball, curY - ball.Width / 2);
                }
            }

            //Arrive at the end point
            if (curX + 10 >= endPoint_X && isPassable(curX, curY))
            {
                StopLevelTimer();
                ShowAllGazePoint();


                //Calculate the score
                CalculateScore();

                //Wait for five seconds
                _timerNextRound?.Start();
            }

        }

        void TimerCollect_Tick(object sender, EventArgs e)
        {
            //Get the current image coordinates

            Vector2 targePoint = new Vector2((float)Canvas.GetLeft(ball), (float)Canvas.GetTop(ball)) + new Vector2((float)ball.Width / 2, (float)ball.Height / 2);

            listTargetPoint.Add(targePoint);
            //Get gaze point coordinates
            if (useMousePos)
            {
                listGazePoint.Add(new Vector2((float)mouse_X, (float)mouse_Y));
            }
            else
            {
                Vector2 gazePoint = GetGazeCoordinates();
                Point canvasPoint = canvas.PointFromScreen(new Point(gazePoint.X, gazePoint.Y));
                listGazePoint.Add(new Vector2((float)canvasPoint.X, (float)canvasPoint.Y));
            }
            //Show gaze point
            //ShowGazeDot(x, y);

        }

        DateTime dateTime_last = DateTime.Now;
        void TimerMoveCurGaze_Tick(object sender, EventArgs e)
        {
            if (useMousePos)
            {
                Point mousePosition = Mouse.GetPosition(this);
                mouse_X = mousePosition.X;
                mouse_Y = mousePosition.Y;
                if ((mouse_X - gaze.Width / 2 > 0) && (mouse_X + gaze.Width / 2 < canvas.Width) &&
                    (mouse_Y - gaze.Width / 2 > 0) && (mouse_Y + gaze.Height / 2 < canvas.Height))
                {    // Place the circle on the specified coordinates
                    Canvas.SetLeft(gaze, mouse_X - gaze.Width / 2);
                    Canvas.SetTop(gaze, mouse_Y - gaze.Height / 2);
                }
                curGaze_X = mouse_X;
                curGaze_Y = mouse_Y;
            }
            else
            {
                //Get gaze point coordinates
                Vector2 gazePoint = GetGazeCoordinates();

                DateTime dateTime_now = DateTime.Now;
                if ((dateTime_now - dateTime_last).TotalMilliseconds > 50)
                {
                    dateTime_last = dateTime_now;
                    udpServer.BroadcastMessage(JsonConvert.SerializeObject(new
                    {
                        ScreenX = (float)gazePoint.X,
                        ScreenY = (float)gazePoint.Y,
                    }));
                }
                if (PresentationSource.FromVisual(canvas) != null)
                {
                    Point canvasPoint = canvas.PointFromScreen(new Point(gazePoint.X, gazePoint.Y));

                    if ((canvasPoint.X - gaze.Width / 2 > 0) && (canvasPoint.X + gaze.Width / 2 < canvas.Width) &&
                        (canvasPoint.Y - gaze.Width / 2 > 0) && (canvasPoint.Y + gaze.Height / 2 < canvas.Height))
                    {    // Place the circle on the specified coordinates
                        Canvas.SetLeft(gaze, canvasPoint.X - gaze.Width / 2);
                        Canvas.SetTop(gaze, canvasPoint.Y - gaze.Height / 2);
                    }
                    curGaze_X = canvasPoint.X;
                    curGaze_Y = canvasPoint.Y;
                }
            }
        }

        void TimerStart123_Tick(object sender, EventArgs e)
        {
            timeStart123--;


            textBlock123.Text = timeStart123.ToString();

            if (timeStart123 <= 0)
            {
                textBlock123.Text = "";
                StartLevelTimer();
                _timerStart123?.Stop();
                timeStart123 = 3;
            }

        }




    }
    public partial class Eye_movement_drive : BaseUserControl
    {



        protected override async Task OnInitAsync()
        {
            {
                level = 8;
                totalTrainTime = 15 * 60;
                hasBGM = false;
                trainTime = 10;
                averageScore = 0;
                gazeRatio = 0;
                gazeDistance = 50;
                listTargetPoint = new List<Vector2>();
                listGazePoint = new List<Vector2>();

                useRandomCoordinates = false;
                useMousePos = false;
                random = new Random();
                SetLevelParameter(level);
            }
            /////Get parameters from the front end here
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
                            case 249: // Difficulty level
                                level = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"level={level}");
                                break;
                            case 250: // Mute
                                hasBGM = par.Value.HasValue ? (bool)(par.Value.Value == 0) : false;
                                Debug.WriteLine($"hasBGM={hasBGM}");
                                break;
                            case 253: // Treatment time 
                                totalTrainTime = par.Value.HasValue ? (int)par.Value.Value * 60 : 15 * 60;
                                Debug.WriteLine($"TRAIN_TIME={totalTrainTime}");
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
            //////


            levelTrainTime = 0;
            timeStart123 = 3;
            t = 0;

            InitializeEyeTracker();

            InitializeList();

            InitializeTimer();

            InitCanvas();

            //TCPconnect
            //tcpServer = new TcpServer();
            //tcpServer.StartListen();

            //UDPconnect
            udpServer = new UdpServer();

            LevelStatisticsAction(level, 10);

        }

        protected override async Task OnStartAsync()
        {

            StartGlobalTimer();
            VoiceTipAction?.Invoke("Please pull the basketball through the gaze point of your sight and move the basketball along the track.");
            SynopsisAction?.Invoke("Now you can see the basketball on the screen. The red circle represents the gaze point of your current gaze. Please pull the basketball through the gaze point of your gaze to move the basketball along the track.");
            RuleAction?.Invoke("Now you can see the basketball on the screen. The red circle represents the gaze point of your current gaze. Please pull the basketball through the gaze point of your gaze to move the basketball along the track.");
        }

        protected override async Task OnStopAsync()
        {
            StopAllTimer();

            tcpServer?.Stop();
            udpServer?.Stop();

        }
        public void Stop(bool isReport = false)
        {
            StopAllTimer();
            mediaPlayer?.Stop();
        }

        protected override async Task OnPauseAsync()
        {
            StopAllTimer();

        }

        protected override async Task OnNextAsync()
        {
            StopLevelTimer();

            NextRound();

            VoiceTipAction?.Invoke("Please pull the basketball through the gaze point of your sight and move the basketball along the track.");
            SynopsisAction?.Invoke("Now you can see the basketball on the screen. The red circle represents the gaze point of your current gaze. Please pull the basketball through the gaze point of your gaze to move the basketball along the track.");
            RuleAction?.Invoke("Now you can see the basketball on the screen. The red circle represents the gaze point of your current gaze. Please pull the basketball through the gaze point of your gaze to move the basketball along the track.");

        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }



        protected override IGameBase OnGetExplanationExample()
        {
            return new Eye-motion drive explanation();
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
                        for (int lv = 1; lv < 11; lv++)
                        {

                            if (scoreDistributions[lv].Count == 0)
                            //this⾥Do a data test
                            {
                                // If all data is 0, skip this difficulty level
                                Debug.WriteLine($"Difficulty level {lv}: No data, skip.");
                                continue;
                            }

                            var newResult = new Result
                            {
                                ProgramId = program_id, // program_id
                                Report = "Eye movement follow",
                                Eval = false,
                                Lv = lv,
                                //Otherwise it isnull, and before⾯offorThe loop needs to be removed
                                ScheduleId = BaseParameter.ScheduleId ?? null // 
                            }; db.Results.Add(newResult);
                            await db.SaveChangesAsync();
                            // get result_id
                            int result_id = newResult.ResultId;

                            //
                            int hardness = lv;
                            int averageScore = (int)averageScoreOfPerLevel[lv];
                            int gazePointCount = (int)gazeRateOfPerLevel[lv].X;
                            int totalEyeTrackerWrapperCount = (int)gazeRateOfPerLevel[lv].Y;
                            int mistakeRate = (int)(errorPointRate[lv].X / errorPointRate[lv].Y);
                            int averageTrainTime = (int)(averageTime[lv].X / averageTime[lv].Y);




                            // create ResultDetail Object List
                            //Please update different parameters below, and only change according to the data structure.resultDetail{}Internal
                            var resultDetails = new List<ResultDetail>
                            {
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "grade",
                                    Value = hardness ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Average score",
                                    Value = averageScore ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Number of gaze points",
                                    Value = gazePointCount ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Eye movement points",
                                    Value = totalEyeTrackerWrapperCount ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Gaze proportions",
                                    Value = (int)(gazePointCount * 100f / totalEyeTrackerWrapperCount) ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Error rate",
                                    Value = (int)(mistakeRate) ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Average time",
                                    Value = (int)(averageTrainTime) ,
                                    ModuleId = BaseParameter.ModuleId
                                },

                            };

                            // Insert⼊ ResultDetail data
                            db.ResultDetails.AddRange(resultDetails);
                            await db.SaveChangesAsync();
                            // Output each ResultDetail Object data
                            Debug.WriteLine($"Difficulty level {lv}:");
                            foreach (var detail in resultDetails)
                            {

                                Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId}");

                            }
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

    }
}
