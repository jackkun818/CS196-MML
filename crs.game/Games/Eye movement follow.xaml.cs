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
using System.Net.Sockets;
using System.Net;
using Newtonsoft.Json;




namespace crs.game.Games
{
    /// <summary>
    /// SAKA.xaml Interaction logic
    /// </summary>
    public partial class Eye movement follow : BaseUserControl
    {
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
        bool needToTurn;     //Do you need to turn
        bool hasBGM;     //Background sound
        int trainTime = 15;      //Training time(s)
        double averageScore;       //Average score
        double gazeRatio;       //Gaze proportions
        double gazeDistance;
        bool useRandomCoordinates;

        //Score...
        List<Vector2> listTargetPoint;   //Storage target
        List<Vector2> listGazePoint;    //Store gaze points

        List<double> recent5Scores;    //Last five scores
        List<double> averageScoreOfPerLevel;  //Average score for each difficulty,Calculate based on the score distribution
        List<Vector3> gazeRateOfPerLevel;     //The gazing ratio of each difficulty,X=Proficient in gaze point,Y=Total points,Z=Gaze proportions
        List<ScoreDistribution> scoreDistributions;   //Score distribution for each difficulty

        private int windowSizeError = 0; //Screen Error

        //Control image movement
        private DispatcherTimer _timerMoveImage;
        private const int moveInterval = 20; // Moving interval（millisecond）
        private const int MoveStep = 1; // Number of pixels per move
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
        bool isTrajectoryHorizontal;  //Straight track flat or oblique
        bool isTrajectoryBendChanges;  //Is the curved trajectory changed?
        bool isPosChanged;  //Whether the position changes
        int speedLevel;   //Fast and slow, 1\2\3
        bool isSpeedChanges;  //Does the speed change

        int trajectoryType = -1;  //-1，  1 level,  2 Lower diagonal,  3 upside down ， 4~6 curves

        double flagCurLevelSpeed = -1;

        double startPosition_Y = 0; //The level beginsycoordinate

        void SetLevelParameter(int level = 1)
        {
            switch (level)
            {
                case 1:
                    isTrajectoryCurved = false; isTrajectoryHorizontal = true; isTrajectoryBendChanges = false;
                    isPosChanged = false; speedLevel = 1; isSpeedChanges = false;
                    break;
                case 2:
                    isTrajectoryCurved = false; isTrajectoryHorizontal = false; isTrajectoryBendChanges = false;
                    isPosChanged = true; speedLevel = 1; isSpeedChanges = false;
                    break;
                case 3:
                    isTrajectoryCurved = false; isTrajectoryHorizontal = false; isTrajectoryBendChanges = false;
                    isPosChanged = false; speedLevel = 2; isSpeedChanges = false;
                    break;
                case 4:
                    isTrajectoryCurved = false; isTrajectoryHorizontal = false; isTrajectoryBendChanges = false;
                    isPosChanged = true; speedLevel = 3; isSpeedChanges = false;
                    break;
                case 5:
                    isTrajectoryCurved = true; isTrajectoryHorizontal = true; isTrajectoryBendChanges = false;
                    isPosChanged = true; speedLevel = 1; isSpeedChanges = false;
                    break;
                case 6:
                    isTrajectoryCurved = true; isTrajectoryHorizontal = true; isTrajectoryBendChanges = true;
                    isPosChanged = true; speedLevel = 1; isSpeedChanges = false;
                    break;
                case 7:
                    isTrajectoryCurved = false; isTrajectoryHorizontal = false; isTrajectoryBendChanges = false;
                    isPosChanged = true; speedLevel = 2; isSpeedChanges = true;
                    break;
                case 8:
                    isTrajectoryCurved = true; isTrajectoryHorizontal = true; isTrajectoryBendChanges = true;
                    isPosChanged = true; speedLevel = 1; isSpeedChanges = true;
                    break;
                case 9:
                    isTrajectoryCurved = true; isTrajectoryHorizontal = true; isTrajectoryBendChanges = true;
                    isPosChanged = true; speedLevel = 2; isSpeedChanges = true;
                    break;
                case 10:
                    isTrajectoryCurved = true; isTrajectoryHorizontal = true; isTrajectoryBendChanges = true;
                    isPosChanged = true; speedLevel = 3; isSpeedChanges = true;
                    break;

            }

        }



        public Eye movement follow()
        {
            InitializeComponent();
        }

        void LoadImage()
        {

            ImageBrush backgroundBrush = new ImageBrush();
            // Create a BitmapImage Object and set decoding
            backgroundBrush.ImageSource = new BitmapImage(new Uri("Resources/Eye movement follow/2.jpg", UriKind.Relative));

            // set up Canvas of Background property
            canvas.Background = backgroundBrush;

            Image image = new Image();
            // Create a BitmapImage Object and set decoding
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri("Resources/Eye movement follow/1.jpg", UriKind.Relative); // Image resource path
            bitmap.CacheOption = BitmapCacheOption.OnLoad; // Optimized loading performance
            bitmap.EndInit();
            image.Source = bitmap;
            image.Width = 100; // Set width
            image.Height = 100; // Set height

            if (!isPosChanged)
            {
                Canvas.SetLeft(image, 10); // Set in Canvas The distance to the left
                Canvas.SetTop(image, 170); // Set in Canvas The upper distance
            }
            else
            {
                int randomInt = random.Next(0, 4);
                Canvas.SetLeft(image, 10 + randomInt * 50); // Set in Canvas The distance to the left
                Canvas.SetTop(image, 100 + randomInt * 50); // Set in Canvas The upper distance
                startPosition_Y = 100 + randomInt * 50;
            }
            canvas.Children.Add(image);


            // Create a circle to represent the current line of sight position
            Ellipse curGaze = new Ellipse
            {
                Width = 30,  // The width of the dot
                Height = 30, // The height of the dot
                Stroke = Brushes.Red, // Border color
                StrokeThickness = 1 // Border thickness
            };
            Canvas.SetLeft(curGaze, 200);
            Canvas.SetTop(curGaze, 200);
            canvas.Children.Add(curGaze);


            TextBlock textBlock123 = new TextBlock();
            textBlock123.Text = "3";
            textBlock123.FontSize = 40;
            textBlock123.FontWeight = FontWeights.Bold;
            textBlock123.Foreground = new SolidColorBrush(Colors.Green);
            Canvas.SetLeft(textBlock123, 400);
            Canvas.SetTop(textBlock123, 150);
            canvas.Children.Add(textBlock123);
        }

        //Calculate the next position of the skier
        Vector2 CalNewImagePos(double x, double y)
        {
            Vector2 newPos = new Vector2((float)x, (float)y);
            if (isTrajectoryCurved)
            {
                if (trajectoryType == -1)
                {
                    if (isTrajectoryBendChanges)
                    {
                        trajectoryType = random.Next(4, 7);
                    }
                    else
                    {
                        //trajectoryType = random.Next(2,4);
                        trajectoryType = 4;
                    }
                }
                double curSpeed = CalCurLevelSpeed();

                newPos.X += (float)curSpeed * moveInterval / 1000f;
                newPos.Y = CalNewPos_Y(trajectoryType);

            }
            else
            {
                if (trajectoryType == -1)
                {
                    if (isTrajectoryHorizontal)
                    {
                        trajectoryType = 1;
                    }
                    else
                    {
                        //trajectoryType = random.Next(2,4);
                        trajectoryType = 2;
                    }
                }
                double curSpeed = CalCurLevelSpeed();

                newPos.X += (float)curSpeed * moveInterval / 1000f;

                if (trajectoryType == 2)
                {
                    newPos.Y += ((float)curSpeed * moveInterval / 1000f) / 4f;
                }
            }

            return newPos;
        }


        // Calculate the speed of the current question, and return it directly
        double CalCurLevelSpeed()
        {
            if (flagCurLevelSpeed != -1)
            {
                return flagCurLevelSpeed;
            }

            Image image = (Image)canvas.Children[0];
            double totalDistance = canvas.Width;
            double curLevelSpeed = totalDistance / (19 - 4 * speedLevel);

            //Change of speed
            //if (isSpeedChanges)
            //{
            //    int n = random.Next(0, 21);//Speed ​​change ratio
            //    curLevelSpeed *= ((double)100 - (n - 10)) / 100.0;
            //}
            //else
            //{

            //}
            flagCurLevelSpeed = curLevelSpeed;
            return curLevelSpeed;
        }

        float CalNewPos_Y(int type)
        {
            if (type == 4)
            {
                if (startPosition_Y <= 150)
                {
                    float pivot = (19 - speedLevel * 4) / 2;
                    float a = (float)3 * ((((float)t / 1000) - pivot) * (((float)t / 1000) - pivot) - pivot * pivot);
                    return (float)startPosition_Y - a;
                }
                else
                {
                    float pivot = (19 - speedLevel * 4) / 2;
                    float a = (float)3 * ((((float)t / 1000) - pivot) * (((float)t / 1000) - pivot) - pivot * pivot);
                    return (float)startPosition_Y + a;
                }
            }
            else if (type == 5)
            {
                float tmp = (19 - speedLevel * 4);
                float a = (float)Math.Sin((2 * Math.PI) * ((float)t / 1000) / tmp);
                return (float)startPosition_Y - a * 100;
            }
            else if (type == 6)
            {
                float tmp = (19 - speedLevel * 4);
                float a = (float)Math.Sin((2 * Math.PI) * ((float)t / 1000) / tmp);
                return (float)startPosition_Y + a * 100;
            }

            return (float)startPosition_Y;
        }

        void ClearCanvas()
        {
            canvas.Children.Clear();
        }

        void NextRound()
        {
            //Adjust the level
            AdjustLevel();
            //Reset parameters
            SetLevelParameter(level);
            levelTrainTime = 19 - 4 * speedLevel;
            timeStart123 = 3;
            trajectoryType = -1;
            flagCurLevelSpeed = -1;
            t = 0;

            ClearCanvas();


            //Loading pictures
            LoadImage();

            _timerStart123?.Start();
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

        void InitializeList()
        {
            if (averageScoreOfPerLevel == null)
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
            }
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

        void CalculateScore()
        {
            //Calculate the score
            for (int i = 0; i < listTargetPoint.Count; i++)
            {
                var targetPoint = listTargetPoint[i];
                var gazePoint = listGazePoint[i];
                double distance = Vector2.Distance(targetPoint, gazePoint);
                if (distance < 30)
                {
                    averageScore += 100;
                    scoreDistributions[level].A++;
                    scoreDistributions[level].Count++;
                }
                else if (distance < 45)
                {
                    averageScore += 75;
                    scoreDistributions[level].B++;
                    scoreDistributions[level].Count++;
                }
                else if (distance < 105)
                {
                    averageScore += 50;
                    scoreDistributions[level].C++;
                    scoreDistributions[level].Count++;
                }
                else if (distance < 150)
                {
                    averageScore += 25;
                    scoreDistributions[level].D++;
                    scoreDistributions[level].Count++;
                }
                else
                {
                    averageScore += 0;
                    scoreDistributions[level].E++;
                    scoreDistributions[level].Count++;
                }
            }
            averageScore /= listTargetPoint.Count;
            averageScoreOfPerLevel[level] = (scoreDistributions[level].A * 100 + scoreDistributions[level].B * 75
                + scoreDistributions[level].C * 50 + scoreDistributions[level].E * 25) / scoreDistributions[level].Count;

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
                Fill = Brushes.Blue, // Fill color
                Stroke = Brushes.Black, // Border color
                StrokeThickness = 1 // Border thickness
            };
            if (PresentationSource.FromVisual(canvas) != null)
            {
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

        void TimerTotalTrain_Tick(object sender, EventArgs e)
        {
            totalTrainTime--;
            if (totalTrainTime <= 0)
            {
                StopAllTimer();
            }
            Debug.Write(totalTrainTime.ToString());
            TimeStatisticsAction?.Invoke(totalTrainTime, levelTrainTime);
        }
        void TimerLevelTrain_Tick(object sender, EventArgs e)
        {
            levelTrainTime--;
            if (levelTrainTime <= 0)
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
            Image image = (Image)canvas.Children[0];
            Vector2 targePoint = new Vector2((float)Canvas.GetLeft(image), (float)Canvas.GetTop(image)) + new Vector2(50, 50);

            listTargetPoint.Add(targePoint);
            //Get gaze point coordinates
            Vector2 gazePoint = GetGazeCoordinates();

            if (PresentationSource.FromVisual(canvas) != null)
            {
                Point canvasPoint = canvas.PointFromScreen(new Point(gazePoint.X, gazePoint.Y));
                listGazePoint.Add(new Vector2((float)canvasPoint.X, (float)canvasPoint.Y));
            }

            //Show gaze point
            //ShowGazeDot(x, y);

        }

        private void TimerMoveImage_Tick(object sender, EventArgs e)
        {
            t += moveInterval;
            Image image = (Image)canvas.Children[0];
            double posX = Canvas.GetLeft(image), posY = Canvas.GetTop(image);
            Vector2 newPos = CalNewImagePos(posX, posY);

            if ((newPos.X > 0) && (newPos.X + image.Width < canvas.Width) &&
                (newPos.Y > 0) && (newPos.Y + image.Height < canvas.Height))
            {
                Canvas.SetLeft(image, newPos.X);
                Canvas.SetTop(image, newPos.Y);
            }

        }

        DateTime dateTime_last = DateTime.Now;
        void TimerMoveCurGaze_Tick(object sender, EventArgs e)
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
                Ellipse curGaze = (Ellipse)canvas.Children[1];
                // Circles represent the current line of sight

                if ((canvasPoint.X - curGaze.Width / 2 > 0) && (canvasPoint.X + curGaze.Width / 2 < canvas.Width) &&
                    (canvasPoint.Y - curGaze.Width / 2 > 0) && (canvasPoint.Y + curGaze.Height / 2 < canvas.Height))
                {    // Place the circle on the specified coordinates
                    Canvas.SetLeft(curGaze, canvasPoint.X - curGaze.Width / 2);
                    Canvas.SetTop(curGaze, canvasPoint.Y - curGaze.Height / 2);
                }
            }

        }

        void TimerStart123_Tick(object sender, EventArgs e)
        {
            timeStart123--;

            TextBlock textBlock123 = (TextBlock)canvas.Children[2];
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
    public partial class Eye movement follow : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
            {
                level = 1;
                needToTurn = false;
                hasBGM = false;
                trainTime = 10;
                averageScore = 0;
                gazeRatio = 0;
                gazeDistance = 50;
                listTargetPoint = new List<Vector2>();
                listGazePoint = new List<Vector2>();

                useRandomCoordinates = false;
                random = new Random();
                SetLevelParameter(level);
            }
            /////Get parameters from the front end here
            // parameter（Includes module parameter information）
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
                            case 231: // Difficulty level
                                level = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"level={level}");
                                break;
                            case 237: // Mute
                                hasBGM = par.Value.HasValue ? (bool)(par.Value.Value == 0) : false;
                                Debug.WriteLine($"hasBGM={hasBGM}");
                                break;
                            case 241: // Treatment time 
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

            totalTrainTime = 15 * 60;
            levelTrainTime = 19 - 4 * speedLevel;
            timeStart123 = 3;
            trajectoryType = -1;
            t = 0;

            InitializeEyeTracker();

            InitializeList();

            InitializeTimer();

            LoadImage();

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
            VoiceTipAction?.Invoke("Please let the gaze point of sight follow the skier.");
            SynopsisAction?.Invoke("Now you can see the skier on the screen, and the red circle represents the gaze point of your current gaze. Please let the gaze point of your gaze move with the skier.");
            RuleAction?.Invoke("Now you can see the skier on the screen, and the red circle represents the gaze point of your current gaze. Please let the gaze point of your gaze move with the skier.");
        }

        protected override async Task OnStopAsync()
        {
            StopAllTimer();

            tcpServer?.Stop();
            udpServer?.Stop();
        }

        protected override async Task OnPauseAsync()
        {
            StopAllTimer();

        }

        protected override async Task OnNextAsync()
        {
            StopLevelTimer();

            NextRound();

            VoiceTipAction?.Invoke("Please let the gaze point of sight follow the skier.");
            SynopsisAction?.Invoke("Now you can see the skier on the screen, and the red circle represents the gaze point of your current gaze. Please let the gaze point of your gaze move with the skier.");
            RuleAction?.Invoke("Now you can see the skier on the screen, and the red circle represents the gaze point of your current gaze. Please let the gaze point of your gaze move with the skier.");

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

                            int scoreRangeOfA = (int)(100 * scoreDistributions[lv].A / scoreDistributions[lv].Count);
                            int scoreRangeOfB = (int)(100 * scoreDistributions[lv].B / scoreDistributions[lv].Count);
                            int scoreRangeOfC = (int)(100 * scoreDistributions[lv].C / scoreDistributions[lv].Count);
                            int scoreRangeOfD = (int)(100 * scoreDistributions[lv].D / scoreDistributions[lv].Count);
                            int scoreRangeOfE = (int)(100 * scoreDistributions[lv].E / scoreDistributions[lv].Count);


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
                                    ValueName = "AScore ratio",
                                    Value = scoreRangeOfA ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "BScore ratio",
                                    Value = scoreRangeOfB ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "CScore ratio",
                                    Value = scoreRangeOfC ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "DScore ratio",
                                    Value = scoreRangeOfD ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "EScore ratio",
                                    Value = scoreRangeOfE ,
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

        protected override IGameBase OnGetExplanationExample()
        {
            return new Eye movement explanation();
        }
    }
}
