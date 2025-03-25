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
    /// SAKA.xaml 的交互逻辑
    /// </summary>
    public partial class 眼动跟随 : BaseUserControl
    {
        public class UdpServer
        {
            private UdpClient udpClient;
            private IPEndPoint broadcastEndPoint;
            public UdpServer()
            {
                // 创建UdpClient实例，监听指定端口
                udpClient = new UdpClient(7001);
                // 设置广播端点
                broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, 7002);
            }

            public void BroadcastMessage(string data)
            {
                // 将字符串数据转换为字节
                byte[] bytesToSend = Encoding.ASCII.GetBytes(data);
                try
                {
                    // 广播数据
                    udpClient.Send(bytesToSend, bytesToSend.Length, broadcastEndPoint);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error broadcasting data: " + ex.Message);
                }
            }

            public void Stop()
            {
                // 关闭UdpClient释放资源
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
                    foreach (TcpClient client in clients.ToArray()) // 使用ToArray()防止修改集合时的异常
                    {
                        try
                        {
                            NetworkStream stream = client.GetStream();
                            byte[] buffer = Encoding.ASCII.GetBytes(message);
                            stream.Write(buffer, 0, buffer.Length);
                        }
                        catch
                        {
                            // 如果发生异常，移除客户端
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
        /// 参数
        /// </summary>
        int level;      //游戏等级
        bool needToTurn;     //是否需要转弯
        bool hasBGM;     //背景音
        int trainTime = 15;      //训练时间(s)
        double averageScore;       //平均得分
        double gazeRatio;       //凝视比例
        double gazeDistance;
        bool useRandomCoordinates;

        //得分...
        List<Vector2> listTargetPoint;   //储存目标
        List<Vector2> listGazePoint;    //储存凝视点

        List<double> recent5Scores;    //最近五次得分
        List<double> averageScoreOfPerLevel;  //每个难度的平均分数,根据得分分布计算
        List<Vector3> gazeRateOfPerLevel;     //每个难度的凝视比例，X=凝视点熟练，Y=总点数，Z=凝视比例
        List<ScoreDistribution> scoreDistributions;   //每个难度的得分分布

        private int windowSizeError = 0; //屏幕误差

        //控制图片移动
        private DispatcherTimer _timerMoveImage;
        private const int moveInterval = 20; // 移动间隔（毫秒）
        private const int MoveStep = 1; // 每次移动的像素数
        int t = 0; // 当前题目已经经过的时间（毫秒）

        private DispatcherTimer _timerCollectePoint;  //收集样本
        int collectInterval = 250;    // （毫秒）

        private DispatcherTimer _timerCurGaze;  //显示当前视线位置

        private DispatcherTimer _timerStart123;  //开始游戏的123倒计时
        int timeStart123 = 3;

        private DispatcherTimer _totalTimerTrain;  //总治疗时间
        private DispatcherTimer _levelTimerTrain;  //当前难度治疗时间
        int totalTrainTime; //总治疗时间
        int levelTrainTime; //当前难度治疗时间

        private DispatcherTimer _timerNextRound;  //下一题
        int timerNextRound = 5;  //秒


        Random random;



        /// <summary>
        ///  难度等级有关参数
        /// </summary>
        bool isTrajectoryCurved;   //轨迹是否弯曲
        bool isTrajectoryHorizontal;  //直轨迹平或斜
        bool isTrajectoryBendChanges;  //弯轨迹是否变化
        bool isPosChanged;  //位置是否变化
        int speedLevel;   //速度快慢，1\2\3
        bool isSpeedChanges;  //速度是否变化

        int trajectoryType = -1;  //-1，  1水平，  2下斜，  3上斜 ， 4~6曲线

        double flagCurLevelSpeed = -1;

        double startPosition_Y = 0; //关卡开始的y坐标

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



        public 眼动跟随()
        {
            InitializeComponent();
        }

        void LoadImage()
        {

            ImageBrush backgroundBrush = new ImageBrush();
            // 创建一个 BitmapImage 对象并设置解码
            backgroundBrush.ImageSource = new BitmapImage(new Uri("Resources/眼动跟随/2.jpg", UriKind.Relative));

            // 设置 Canvas 的 Background 属性
            canvas.Background = backgroundBrush;

            Image image = new Image();
            // 创建一个 BitmapImage 对象并设置解码
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri("Resources/眼动跟随/1.jpg", UriKind.Relative); // 图像资源路径
            bitmap.CacheOption = BitmapCacheOption.OnLoad; // 优化加载性能
            bitmap.EndInit();
            image.Source = bitmap;
            image.Width = 100; // 设置宽度
            image.Height = 100; // 设置高度

            if (!isPosChanged)
            {
                Canvas.SetLeft(image, 10); // 设置在 Canvas 上的左边距离
                Canvas.SetTop(image, 170); // 设置在 Canvas 上的上边距离
            }
            else
            {
                int randomInt = random.Next(0, 4);
                Canvas.SetLeft(image, 10 + randomInt * 50); // 设置在 Canvas 上的左边距离
                Canvas.SetTop(image, 100 + randomInt * 50); // 设置在 Canvas 上的上边距离
                startPosition_Y = 100 + randomInt * 50;
            }
            canvas.Children.Add(image);


            // 创建一个圆圈代表当前视线位置
            Ellipse curGaze = new Ellipse
            {
                Width = 30,  // 圆点的宽度
                Height = 30, // 圆点的高度
                Stroke = Brushes.Red, // 边框颜色
                StrokeThickness = 1 // 边框厚度
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

        //计算滑雪者的下一个位置
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


        // 计算当前题目的速度，已计算速度则直接返回
        double CalCurLevelSpeed()
        {
            if (flagCurLevelSpeed != -1)
            {
                return flagCurLevelSpeed;
            }

            Image image = (Image)canvas.Children[0];
            double totalDistance = canvas.Width;
            double curLevelSpeed = totalDistance / (19 - 4 * speedLevel);

            //速度变化
            //if (isSpeedChanges)
            //{
            //    int n = random.Next(0, 21);//速度变化比例
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
            //调整等级
            AdjustLevel();
            //重新设置参数
            SetLevelParameter(level);
            levelTrainTime = 19 - 4 * speedLevel;
            timeStart123 = 3;
            trajectoryType = -1;
            flagCurLevelSpeed = -1;
            t = 0;

            ClearCanvas();


            //加载图片
            LoadImage();

            _timerStart123?.Start();
        }

        void AdjustLevel()
        {
            //调整游戏等级
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
                recent5Scores = new List<double>();    //最近五次得分
                averageScoreOfPerLevel = new List<double>();  //每个难度的平均分数
                for (int i = 0; i < 11; i++)
                {
                    averageScoreOfPerLevel.Add(0);
                }
                gazeRateOfPerLevel = new List<Vector3>();     //每个难度的凝视比例
                for (int i = 0; i < 11; i++)
                {
                    gazeRateOfPerLevel.Add(new Vector3(0, 0, 0));
                }
                scoreDistributions = new List<ScoreDistribution>();   //每个难度的得分分布
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
            //计算分数
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



            //计算凝视比例
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
            // 创建一个椭圆表示蓝色的小圆点
            Ellipse blueDot = new Ellipse
            {
                Width = 10,  // 圆点的宽度
                Height = 10, // 圆点的高度
                Fill = Brushes.Blue, // 填充颜色
                Stroke = Brushes.Black, // 边框颜色
                StrokeThickness = 1 // 边框厚度
            };
            if (PresentationSource.FromVisual(canvas) != null)
            {
                if ((x - blueDot.Width / 2 > 0) && (x + blueDot.Width / 2 < canvas.Width) &&
                (y - blueDot.Height / 2 > 0) && (y + blueDot.Height / 2 < canvas.Height))
                {
                    // 将椭圆放置在指定的坐标上
                    Canvas.SetLeft(blueDot, x - blueDot.Width / 2);
                    Canvas.SetTop(blueDot, y - blueDot.Height / 2);
                    // 将蓝色的小圆点添加到 Canvas 上
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


                //计算分数
                CalculateScore();

                //等待五秒钟
                _timerNextRound?.Start();

            }

        }

        void TimerCollect_Tick(object sender, EventArgs e)
        {
            //获取当前图片坐标
            Image image = (Image)canvas.Children[0];
            Vector2 targePoint = new Vector2((float)Canvas.GetLeft(image), (float)Canvas.GetTop(image)) + new Vector2(50, 50);

            listTargetPoint.Add(targePoint);
            //获取凝视点坐标
            Vector2 gazePoint = GetGazeCoordinates();

            if (PresentationSource.FromVisual(canvas) != null)
            {
                Point canvasPoint = canvas.PointFromScreen(new Point(gazePoint.X, gazePoint.Y));
                listGazePoint.Add(new Vector2((float)canvasPoint.X, (float)canvasPoint.Y));
            }

            //显示凝视点
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
            //获取凝视点坐标
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
                // 圆圈代表当前视线位置

                if ((canvasPoint.X - curGaze.Width / 2 > 0) && (canvasPoint.X + curGaze.Width / 2 < canvas.Width) &&
                    (canvasPoint.Y - curGaze.Width / 2 > 0) && (canvasPoint.Y + curGaze.Height / 2 < canvas.Height))
                {    // 将圆放置在指定的坐标上
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
    public partial class 眼动跟随 : BaseUserControl
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
            /////这里从前端获取参数
            // 参数（包含模块参数信息）
            var baseParameter = BaseParameter;
            if (baseParameter.ProgramModulePars != null && baseParameter.ProgramModulePars.Any())
            {
                Debug.WriteLine("ProgramModulePars 已加载数据：");

                // 遍历 ProgramModulePars 打印出每个参数
                foreach (var par in baseParameter.ProgramModulePars)
                {
                    /*Debug.WriteLine($"ProgramId: {par.ProgramId}, ModuleParId: {par.ModuleParId}, Value: {par.Value}, TableId: {par.TableId}");*/
                    if (par.ModulePar.ModuleId == baseParameter.ModuleId)
                    {
                        switch (par.ModuleParId) // 完成赋值
                        {
                            case 231: // 难度等级
                                level = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"level={level}");
                                break;
                            case 237: // 静音
                                hasBGM = par.Value.HasValue ? (bool)(par.Value.Value == 0) : false;
                                Debug.WriteLine($"hasBGM={hasBGM}");
                                break;
                            case 241: // 治疗时间 
                                totalTrainTime = par.Value.HasValue ? (int)par.Value.Value * 60 : 15 * 60;
                                Debug.WriteLine($"TRAIN_TIME={totalTrainTime}");
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

            //TCP连接
            //tcpServer = new TcpServer();
            //tcpServer.StartListen();

            //UDP连接
            udpServer = new UdpServer();


            LevelStatisticsAction(level, 10);
        }

        protected override async Task OnStartAsync()
        {
            StartGlobalTimer();
            VoiceTipAction?.Invoke("请让视线的注视点跟随滑雪者移动。");
            SynopsisAction?.Invoke("现在您可以看到屏幕上的滑雪者，红色圆圈代表您当前视线的注视点，请让视线的注视点跟随滑雪者移动。");
            RuleAction?.Invoke("现在您可以看到屏幕上的滑雪者，红色圆圈代表您当前视线的注视点，请让视线的注视点跟随滑雪者移动。");
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

            VoiceTipAction?.Invoke("请让视线的注视点跟随滑雪者移动。");
            SynopsisAction?.Invoke("现在您可以看到屏幕上的滑雪者，红色圆圈代表您当前视线的注视点，请让视线的注视点跟随滑雪者移动。");
            RuleAction?.Invoke("现在您可以看到屏幕上的滑雪者，红色圆圈代表您当前视线的注视点，请让视线的注视点跟随滑雪者移动。");

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
                            //这⾥做个数据检验
                            {
                                // 如果所有数据都为0，跳过此难度级别
                                Debug.WriteLine($"难度级别 {lv}: 没有数据，跳过.");
                                continue;
                            }

                            var newResult = new Result
                            {
                                ProgramId = program_id, // program_id
                                Report = "眼动跟随",
                                Eval = false,
                                Lv = lv,
                                //否则为null，且前⾯的for循环要去掉
                                ScheduleId = BaseParameter.ScheduleId ?? null // 
                            }; db.Results.Add(newResult);
                            await db.SaveChangesAsync();
                            // 获得 result_id
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


                            // 创建 ResultDetail 对象列表
                            //更新不同参数请在底下更新，根据数据结构，只更改resultDetail{}内的
                            var resultDetails = new List<ResultDetail>
                            {
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "等级",
                                    Value = hardness ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "平均分数",
                                    Value = averageScore ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "凝视点数",
                                    Value = gazePointCount ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "眼动点数",
                                    Value = totalEyeTrackerWrapperCount ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "凝视比例",
                                    Value = (int)(gazePointCount * 100f / totalEyeTrackerWrapperCount) ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "A得分的比例",
                                    Value = scoreRangeOfA ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "B得分的比例",
                                    Value = scoreRangeOfB ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "C得分的比例",
                                    Value = scoreRangeOfC ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "D得分的比例",
                                    Value = scoreRangeOfD ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "E得分的比例",
                                    Value = scoreRangeOfE ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                            };

                            // 插⼊ ResultDetail 数据
                            db.ResultDetails.AddRange(resultDetails);
                            await db.SaveChangesAsync();
                            // 输出每个 ResultDetail 对象的数据
                            Debug.WriteLine($"难度级别 {lv}:");
                            foreach (var detail in resultDetails)
                            {

                                Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId}");

                            }
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
            return new 眼动跟随讲解();
        }
    }
}
