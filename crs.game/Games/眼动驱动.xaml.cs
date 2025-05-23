﻿using crs.core;
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

    public partial class 眼动驱动 : BaseUserControl, IGameBase
    {
        public 眼动驱动()
        {
            InitializeComponent();
        }
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
        bool hasBGM;     //背景音
        int trainTime;      //训练时间(s)
        double averageScore;       //平均得分
        double gazeRatio;       //凝视比例
        double gazeDistance;       //凝视距离
        bool useRandomCoordinates;

        //得分...
        List<Vector2> listTargetPoint;   //储存目标点
        List<Vector2> listGazePoint;    //储存凝视点

        List<double> recent5Scores;    //最近五次得分
        List<double> averageScoreOfPerLevel;  //每个难度的平均分数,根据得分分布计算
        List<Vector3> gazeRateOfPerLevel;     //每个难度的凝视比例，X=凝视点熟练，Y=总点数，Z=凝视比例
        List<ScoreDistribution> scoreDistributions;   //每个难度的得分分布
        List<Vector2> errorPointRate;       //错误率，X：错误点的数量， Y：总数
        List<Vector2> averageTime;        //平均用时，X： 使用总时间，Y ：总训练次数

        private int windowSizeError = 0; //屏幕误差


        private DispatcherTimer _timerMoveImage;   //控制图片移动
        private const int moveInterval = 30; // 移动间隔（毫秒）
        private const int moveSpeed = 5; // 每次移动的像素数
        double maxMoveDistance = 80;   //可以移动的最大距离
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
        int trajectoryTypes;  //轨迹描述，1：平  2：斜  3：很斜 4：上下/下上  5：上下上/下上下
        int trajectoryWidthTypes;  //轨迹宽窄   1：宽  2：中  3：窄
        bool isTrajectoryPosChanged;  //轨迹位置是否变化



        Ellipse gaze;//
        Image ball;
        TextBlock textBlock123;  ///


        /// <summary>
        /// 生成轨迹相关参数
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

            //控制播放音乐
            mediaPlayer.Source = new Uri("Resources/眼动驱动/3.WAV", UriKind.RelativeOrAbsolute);


            // 创建一个 BitmapImage 对象并设置解码
            BitmapImage bitmapBG = new BitmapImage();
            bitmapBG.BeginInit();
            bitmapBG.UriSource = new Uri("Resources/眼动驱动/1.jpg", UriKind.Relative); // 图像资源路径
            bitmapBG.CacheOption = BitmapCacheOption.OnLoad; // 优化加载性能
            bitmapBG.EndInit();
            // 设置 ImageBrush 的 ImageSource 属性
            backgroundBrush.ImageSource = bitmapBG;
            // 设置 Canvas 的 Background 属性
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


            // 创建一个圆圈代表当前视线位置
            gaze = new Ellipse
            {
                Width = 30,  // 圆点的宽度
                Height = 30, // 圆点的高度
                Stroke = Brushes.Red, // 边框颜色
                StrokeThickness = 1 // 边框厚度
            };
            Canvas.SetLeft(gaze, 200);
            Canvas.SetTop(gaze, 200);
            canvas.Children.Add(gaze);

            ////
            //ball = new Ellipse
            //{
            //    Width = 40,  // 圆点的宽度
            //    Height = 40, // 圆点的高度
            //    Stroke = Brushes.Blue, // 边框颜色
            //    StrokeThickness = 1, // 边框厚度
            //    Fill = new SolidColorBrush(Colors.Blue),
            //};
            //Canvas.SetLeft(ball, startPoint_X + 5);
            //Canvas.SetTop(ball, startPoint_Y + 20);
            //canvas.Children.Add(ball);


            //////
            ball = new Image();
            // 创建一个 BitmapImage 对象并设置解码
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri("Resources/眼动驱动/2.jpg", UriKind.Relative); // 图像资源路径
            bitmap.CacheOption = BitmapCacheOption.OnLoad; // 优化加载性能
            bitmap.EndInit();
            ball.Source = bitmap;
            ball.Width = 40; // 设置宽度
            ball.Height = 40; // 设置高度
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
                tmpRectangle.Stroke = Brushes.Khaki; // 边框颜色
                tmpRectangle.StrokeThickness = 1; // 边框厚度

                // 将Rectangle放置在Canvas上
                Canvas.SetLeft(tmpRectangle, i);
                Canvas.SetTop(tmpRectangle, startPoint_Y);

                // 将Rectangle添加到Canvas的子元素集合中
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
                tmpRectangle.Stroke = Brushes.Khaki; // 边框颜色
                tmpRectangle.StrokeThickness = 1; // 边框厚度

                // 将Rectangle放置在Canvas上
                Canvas.SetLeft(tmpRectangle, i);
                Canvas.SetTop(tmpRectangle, startPoint_Y + trend_direction * (i - startPoint_X) * 0.15);

                // 将Rectangle添加到Canvas的子元素集合中
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
                tmpRectangle.Stroke = Brushes.Khaki; // 边框颜色
                tmpRectangle.StrokeThickness = 1; // 边框厚度

                // 将Rectangle放置在Canvas上
                Canvas.SetLeft(tmpRectangle, i);
                Canvas.SetTop(tmpRectangle, startPoint_Y + trend_direction * (i - startPoint_X) * 0.3);

                // 将Rectangle添加到Canvas的子元素集合中
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
                tmpRectangle.Stroke = Brushes.Khaki; // 边框颜色
                tmpRectangle.StrokeThickness = 1; // 边框厚度

                // 将Rectangle放置在Canvas上
                Canvas.SetLeft(tmpRectangle, i);
                Canvas.SetTop(tmpRectangle, startPoint_Y + trend_direction * 100 * Math.Sin(Math.PI * ((float)(i - startPoint_X) / (endPoint_X - startPoint_X))));

                // 将Rectangle添加到Canvas的子元素集合中
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
                tmpRectangle.Stroke = Brushes.Khaki; // 边框颜色
                tmpRectangle.StrokeThickness = 1; // 边框厚度

                // 将Rectangle放置在Canvas上
                Canvas.SetLeft(tmpRectangle, i);
                Canvas.SetTop(tmpRectangle, startPoint_Y + trend_direction * 100 * Math.Sin(2 * Math.PI * ((float)(i - startPoint_X) / (endPoint_X - startPoint_X))));

                // 将Rectangle添加到Canvas的子元素集合中
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

            //调整等级
            AdjustLevel();
            //重新设置参数
            SetLevelParameter(level);
            levelTrainTime = 0;
            timeStart123 = 3;

            t = 0;

            ClearCanvas();


            //加载图片
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
            errorPointRate = new List<Vector2>();    //每个难度的错误率
            for (int i = 0; i < 11; i++)
            {
                errorPointRate.Add(new Vector2(0, 0));
            }
            averageTime = new List<Vector2>();    //每个难度的平均用时
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
            //计算错误率
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

            //记录平均用时
            averageTime[level] += new Vector2((int)t / 1000, 1);

            //当前题目分数
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
                Fill = Brushes.LightSkyBlue, // 填充颜色
                Stroke = Brushes.Black, // 边框颜色
                StrokeThickness = 1 // 边框厚度
            };

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
        /// 记录当前题目已经用时
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

            //到达终点
            if (curX + 10 >= endPoint_X && isPassable(curX, curY))
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

            Vector2 targePoint = new Vector2((float)Canvas.GetLeft(ball), (float)Canvas.GetTop(ball)) + new Vector2((float)ball.Width / 2, (float)ball.Height / 2);

            listTargetPoint.Add(targePoint);
            //获取凝视点坐标
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
            //显示凝视点
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
                {    // 将圆放置在指定的坐标上
                    Canvas.SetLeft(gaze, mouse_X - gaze.Width / 2);
                    Canvas.SetTop(gaze, mouse_Y - gaze.Height / 2);
                }
                curGaze_X = mouse_X;
                curGaze_Y = mouse_Y;
            }
            else
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

                    if ((canvasPoint.X - gaze.Width / 2 > 0) && (canvasPoint.X + gaze.Width / 2 < canvas.Width) &&
                        (canvasPoint.Y - gaze.Width / 2 > 0) && (canvasPoint.Y + gaze.Height / 2 < canvas.Height))
                    {    // 将圆放置在指定的坐标上
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
    public partial class 眼动驱动 : BaseUserControl
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
            /////这里从前端获取参数
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
                            case 249: // 难度等级
                                level = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"level={level}");
                                break;
                            case 250: // 静音
                                hasBGM = par.Value.HasValue ? (bool)(par.Value.Value == 0) : false;
                                Debug.WriteLine($"hasBGM={hasBGM}");
                                break;
                            case 253: // 治疗时间 
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


            levelTrainTime = 0;
            timeStart123 = 3;
            t = 0;

            InitializeEyeTracker();

            InitializeList();

            InitializeTimer();

            InitCanvas();

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
            VoiceTipAction?.Invoke("请您通过视线的注视点牵引篮球，使篮球沿着轨迹移动。");
            SynopsisAction?.Invoke("现在您可以看到屏幕上的篮球，红色圆圈代表您当前视线的注视点，请您通过视线的注视点牵引篮球，使篮球沿着轨迹移动。");
            RuleAction?.Invoke("现在您可以看到屏幕上的篮球，红色圆圈代表您当前视线的注视点，请您通过视线的注视点牵引篮球，使篮球沿着轨迹移动。");
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

            VoiceTipAction?.Invoke("请您通过视线的注视点牵引篮球，使篮球沿着轨迹移动。");
            SynopsisAction?.Invoke("现在您可以看到屏幕上的篮球，红色圆圈代表您当前视线的注视点，请您通过视线的注视点牵引篮球，使篮球沿着轨迹移动。");
            RuleAction?.Invoke("现在您可以看到屏幕上的篮球，红色圆圈代表您当前视线的注视点，请您通过视线的注视点牵引篮球，使篮球沿着轨迹移动。");

        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }



        protected override IGameBase OnGetExplanationExample()
        {
            return new 眼动驱动讲解();
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
                            int mistakeRate = (int)(errorPointRate[lv].X / errorPointRate[lv].Y);
                            int averageTrainTime = (int)(averageTime[lv].X / averageTime[lv].Y);




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
                                    ValueName = "出错率",
                                    Value = (int)(mistakeRate) ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "平均用时",
                                    Value = (int)(averageTrainTime) ,
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

    }
}
