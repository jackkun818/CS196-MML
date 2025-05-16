using crs.core;
using crs.core.DbModels;
using Microsoft.Identity.Client.NativeInterop;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace crs.game.Games
{
    public partial class 反应能力 : BaseUserControl
    {
        private readonly string[] imagePaths =
        {
            "反应能力/4.png",//下
            "反应能力/2.png",//左
            "反应能力/1.png",//右
            "反应能力/3.png",//上
            "反应能力/5.png",//叉
            "反应能力/6.png"//圆
        };
        private readonly string correct_wav = "./Resources/词语记忆力/Effects/Correct.wav";
        private readonly string wrong_wav = "./Resources/词语记忆力/Effects/Error.wav";
        private int increase;//30道中答对的
        private int decrease;//30道中答错的
        private Random random;
        private int MAX_HARDNESS = 16;
        private int MAX_REACT = 5000;
        private int counterA;//相关刺激数量
        private int counterB;//wild数量
        private int randomIndex;
        private int ignore;
        private int wrong;
        private int overtime;
        private DispatcherTimer displayTimer;
        private DispatcherTimer trainingTimer; // 新增计时器用于训练时间
        private DispatcherTimer imageDisplayTimer;//图片展示间隔
        private int STIMULI_INTERVAL = 3000; // 刺激间隔
        private int TRAIN_TIME = 60; // 训练时间60秒
        private int INCREASE = 5; // 增加难度的阈值
        private int DECREASE = 5; // 降低难度的阈值
        private int hardness = 1; // 设置初始难度
        private double remainingTime; // 当前题目剩余时间（秒）
        private int traintime; // 总剩余时间（秒）
        private bool IS_BEEP = true;
        private bool IS_SCREEN = true;
        private int delayTime;
        private int max_hardness;
        private int press;
        private double timecount;


        /// <summary>
        /// 答题所用的时间
        /// </summary>
        DateTime startTime = new DateTime();
        DateTime endTime = new DateTime();

        public 反应能力()
        {
            InitializeComponent();

        }
        private async Task ChangeBorderColor()
        {
            if (BorderElement.Background is SolidColorBrush brush && brush.Color == Colors.Red)
                return;
            // 保存原来的颜色
            var originalColor = BorderElement.Background;

            // 设置为红色
            BorderElement.Background = new SolidColorBrush(Colors.Red);

            // 等待 0.2 秒
            await Task.Delay(200);

            // 恢复原来的颜色
            BorderElement.Background = originalColor;
        }
        private void PlaySound(string filePath)
        {
            try
            {
                string absolutePath = System.IO.Path.GetFullPath(filePath);
                if (!System.IO.File.Exists(filePath))
                {
                    MessageBox.Show($"音效文件不存在：{absolutePath}");
                    return;
                }
                SoundPlayer player = new SoundPlayer(filePath);
                player.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"播放音效时出错：{ex.Message}");
            }
        }
        private void ShowRandomImage()
        {
            // 在显示新图像之前设置图像源为null
            RandomImage.Source = null;
            imageDisplayTimer?.Stop();
            imageDisplayTimer.Interval = TimeSpan.FromMilliseconds(STIMULI_INTERVAL);
            imageDisplayTimer?.Start();
        }
        private void OnImageDisplayTimerElapsed(object sender, EventArgs e)
        {
            imageDisplayTimer?.Stop();
            DisplayRandomImage(); // 显示随机图像
        }

        private void OnDisplayTimerElapsed(object sender, EventArgs e)
        {
            // 每秒减少剩余时间
            remainingTime--;
            // 如果剩余时间达到 0，停止计时器并执行结算逻辑
            if (remainingTime < 0)
            {

                endTime = DateTime.Now;
                TimeSpan duration = endTime - startTime; // 计算作答时间            
                timecount += (int)duration.TotalMilliseconds;
                press++;


                displayTimer?.Stop(); // 停止计时器
                if (randomIndex == 4 || randomIndex == 5)
                {
                    increase++;
                    counterB++;
                }
                else
                {
                    decrease++;
                    ignore++;
                    counterA++;
                }

                AdjustDifficulty();
                DisplayRandomImage();
            }
        }
        private void OnTrainingTimerElapsed(object sender, EventArgs e)
        {
            traintime--;
            // 调用委托，传递训练用时和剩余时间
            TimeStatisticsAction?.Invoke(traintime, (int)remainingTime);
            // 如果达到 TRAIN_TIME，停止所有计时器并打开报告窗口
            if (traintime < 0)
            {
                StopAllTimers();
                OnGameEnd();
            }
        }

        private void StopAllTimers()
        {
            imageDisplayTimer?.Stop();
            displayTimer?.Stop();
            trainingTimer?.Stop();
        }
        private void Window_MouseMove(object sender, MouseEventArgs e)//绑定自定义鼠标光标和默认鼠标光标
        {

            Point position = e.GetPosition(this);
            Canvas.SetLeft(CustomCursor, position.X - (CustomCursor.Width / 2));
            Canvas.SetTop(CustomCursor, position.Y - (CustomCursor.Height / 2));
        }
        private void OnGameStart()//调用自定义鼠标光标函数
        {
            this.Cursor = Cursors.None; // 隐藏默认光标
            CustomCursor.Visibility = Visibility.Visible; // 显示自定义光标
            MouseMove += Window_MouseMove; // 订阅 MouseMove 事件
            CustomCursor.Width = 65; // 调整为你想要的宽度
            CustomCursor.Height = 65; // 调整为你想要的高度
        }
        private void DisplayRandomImage()
        {
            double maxY;
            double maxX;
            double left;
            double top;
            remainingTime = MAX_REACT / 1000;
            displayTimer.Start();

            startTime = DateTime.Now;
            switch (hardness)
            {
                case 1:
                    randomIndex = random.Next(1); //forbid
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Collapsed;
                    left_arrow.Visibility = Visibility.Collapsed;
                    Right.Visibility = Visibility.Collapsed;
                    right_arrow.Visibility = Visibility.Collapsed;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 2:
                    randomIndex = random.Next(1); //forbid                    
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Collapsed;
                    left_arrow.Visibility = Visibility.Collapsed;
                    Right.Visibility = Visibility.Collapsed;
                    right_arrow.Visibility = Visibility.Collapsed;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 3:
                    randomIndex = random.Next(2);//forbid+left                    
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Collapsed;
                    right_arrow.Visibility = Visibility.Collapsed;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 4:
                    randomIndex = random.Next(2);//forbid+left                    
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Collapsed;
                    right_arrow.Visibility = Visibility.Collapsed;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 5:
                    randomIndex = random.Next(2);//forbid+left(全屏)                    
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Collapsed;
                    right_arrow.Visibility = Visibility.Collapsed;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"{imagePaths[randomIndex]}", UriKind.Relative));
                    maxX = ImageGrid.ActualWidth - RandomImage.Width;
                    maxY = ImageGrid.ActualWidth / 1.7 - RandomImage.Height;
                    left = random.NextDouble() * maxX;
                    top = random.NextDouble() * maxY;
                    RandomImage.Margin = new Thickness(left, top, 0, 0);
                    break;

                case 6:
                    randomIndex = random.Next(4);//forbid+left+wild
                    if (randomIndex == 2 || randomIndex == 3)
                    {
                        randomIndex = 4;
                    }
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Collapsed;
                    right_arrow.Visibility = Visibility.Collapsed;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 7:
                    randomIndex = random.Next(4);//forbid+left+wild
                    if (randomIndex == 2 || randomIndex == 3)
                        randomIndex = 4;
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Collapsed;
                    right_arrow.Visibility = Visibility.Collapsed;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 8:
                    randomIndex = random.Next(4);//forbid+left+wild(全屏)
                    if (randomIndex == 2 || randomIndex == 3)
                        randomIndex = 4;
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Collapsed;
                    right_arrow.Visibility = Visibility.Collapsed;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"{imagePaths[randomIndex]}", UriKind.Relative));
                    maxX = ImageGrid.ActualWidth - RandomImage.Width;
                    maxY = ImageGrid.ActualWidth / 1.7 - RandomImage.Height;
                    left = random.NextDouble() * maxX;
                    top = random.NextDouble() * maxY;
                    RandomImage.Margin = new Thickness(left, top, 0, 0);
                    break;

                case 9:
                    randomIndex = random.Next(3);//forbid+left+right                    
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Visible;
                    right_arrow.Visibility = Visibility.Visible;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 10:
                    randomIndex = random.Next(3);//forbid+left+right                    
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Visible;
                    right_arrow.Visibility = Visibility.Visible;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 11:
                    randomIndex = random.Next(3);//forbid+left+right(全屏)                    
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Visible;
                    right_arrow.Visibility = Visibility.Visible;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"{imagePaths[randomIndex]}", UriKind.Relative));
                    maxX = ImageGrid.ActualWidth - RandomImage.Width;
                    maxY = ImageGrid.ActualWidth / 1.7 - RandomImage.Height;
                    left = random.NextDouble() * maxX;
                    top = random.NextDouble() * maxY;
                    RandomImage.Margin = new Thickness(left, top, 0, 0);
                    break;

                case 12:
                    randomIndex = random.Next(6);//forbid+left+right+wild
                    if (randomIndex > 2)
                        randomIndex = 4;
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Visible;
                    right_arrow.Visibility = Visibility.Visible;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 13:
                    randomIndex = random.Next(6);//forbid+left+right+wild
                    if (randomIndex > 2)
                        randomIndex = 4;
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Visible;
                    right_arrow.Visibility = Visibility.Visible;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 14:
                    randomIndex = random.Next(6);//forbid+left+right+wild(全屏)
                    if (randomIndex > 2)
                        randomIndex = 4;
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Visible;
                    right_arrow.Visibility = Visibility.Visible;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"{imagePaths[randomIndex]}", UriKind.Relative));
                    maxX = ImageGrid.ActualWidth - RandomImage.Width;
                    maxY = ImageGrid.ActualWidth / 1.7 - RandomImage.Height;
                    left = random.NextDouble() * maxX;
                    top = random.NextDouble() * maxY;
                    RandomImage.Margin = new Thickness(left, top, 0, 0);
                    break;

                case 15:
                    randomIndex = random.Next(6);//forbid+left+right+wild*2
                    if (randomIndex == 3)
                        randomIndex = 5;
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Visible;
                    right_arrow.Visibility = Visibility.Visible;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 16:
                    randomIndex = random.Next(6);//forbid+left+right+wild*2
                    if (randomIndex == 3)
                        randomIndex = 5;
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Visible;
                    right_arrow.Visibility = Visibility.Visible;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"{imagePaths[randomIndex]}", UriKind.Relative));
                    RandomImage.Margin = new Thickness(0, 0, 0, 0);
                    break;

                case 17:
                    randomIndex = random.Next(6);//forbid+left+right+wild*2(全屏)
                    if (randomIndex == 3)
                        randomIndex = 5;
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Visible;
                    right_arrow.Visibility = Visibility.Visible;
                    forward.Visibility = Visibility.Collapsed;
                    Forward.Visibility = Visibility.Collapsed;
                    RandomImage.Source = new BitmapImage(new Uri($@"{imagePaths[randomIndex]}", UriKind.Relative));
                    maxX = ImageGrid.ActualWidth - RandomImage.Width;
                    maxY = ImageGrid.ActualWidth / 1.7 - RandomImage.Height;
                    left = random.NextDouble() * maxX;
                    top = random.NextDouble() * maxY;
                    RandomImage.Margin = new Thickness(left, top, 0, 0);
                    break;

                case 18:
                    randomIndex = random.Next(4);//forbid+left+right+forward(全屏)                    
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Visible;
                    right_arrow.Visibility = Visibility.Visible;
                    forward.Visibility = Visibility.Visible;
                    Forward.Visibility = Visibility.Visible;
                    RandomImage.Source = new BitmapImage(new Uri($@"{imagePaths[randomIndex]}", UriKind.Relative));
                    maxX = ImageGrid.ActualWidth - RandomImage.Width;
                    maxY = ImageGrid.ActualWidth / 1.7 - RandomImage.Height;
                    left = random.NextDouble() * maxX;
                    top = random.NextDouble() * maxY;
                    RandomImage.Margin = new Thickness(left, top, 0, 0);
                    break;

                case 19:
                    randomIndex = random.Next(8);//forbid+left+right+forward+wild(全屏)
                    if (randomIndex > 3)
                        randomIndex = 4;
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Visible;
                    right_arrow.Visibility = Visibility.Visible;
                    forward.Visibility = Visibility.Visible;
                    Forward.Visibility = Visibility.Visible;
                    RandomImage.Source = new BitmapImage(new Uri($@"{imagePaths[randomIndex]}", UriKind.Relative));
                    maxX = ImageGrid.ActualWidth - RandomImage.Width;
                    maxY = ImageGrid.ActualWidth / 1.7 - RandomImage.Height;
                    left = random.NextDouble() * maxX;
                    top = random.NextDouble() * maxY;
                    RandomImage.Margin = new Thickness(left, top, 0, 0);
                    break;

                case 20:
                    randomIndex = random.Next(8);//forbid+left+right+forward+wild*2(全屏)
                    if (randomIndex == 6)
                        randomIndex = 4;
                    else if (randomIndex == 7)
                        randomIndex = 5;
                    forbid.Visibility = Visibility.Visible;
                    Forbid.Visibility = Visibility.Visible;
                    Left.Visibility = Visibility.Visible;
                    left_arrow.Visibility = Visibility.Visible;
                    Right.Visibility = Visibility.Visible;
                    right_arrow.Visibility = Visibility.Visible;
                    forward.Visibility = Visibility.Visible;
                    Forward.Visibility = Visibility.Visible;
                    RandomImage.Source = new BitmapImage(new Uri($@"{imagePaths[randomIndex]}", UriKind.Relative));
                    maxX = ImageGrid.ActualWidth - RandomImage.Width;
                    maxY = ImageGrid.ActualWidth / 1.7 - RandomImage.Height;
                    left = random.NextDouble() * maxX;
                    top = random.NextDouble() * maxY;
                    RandomImage.Margin = new Thickness(left, top, 0, 0);
                    break;
                default:
                    // 默认情况，可以处理未定义的难度级别
                    break;
            }
        }

        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Down && e.Key != Key.Up && e.Key != Key.Left && e.Key != Key.Right)
                return;
            bool isCorrect = false;  // 声明并初始化 isCorrect 变量
            imageDisplayTimer?.Stop();
            displayTimer?.Stop(); // 停止计时器
            if (RandomImage.Source == null)
            {
                remainingTime = 0;
                decrease++;
                overtime++;
                if (IS_BEEP)
                    PlaySound(wrong_wav);
                if (IS_SCREEN)
                    _ = ChangeBorderColor();
                AdjustDifficulty();
                ShowRandomImage();
                return;
            }
            if (RandomImage.Source != null)
            {
                if (randomIndex != 4 && randomIndex != 5)
                {
                    counterA++;
                }
                else if (randomIndex == 4 || randomIndex == 5)
                {
                    counterB++;
                }
                if (e.Key == Key.Down && randomIndex == 0)
                {
                    isCorrect = true;  // 如果按键正确，设置 isCorrect 为 true
                }
                else if (e.Key == Key.Left && randomIndex == 1)
                {
                    isCorrect = true;  // 如果按键正确，设置 isCorrect 为 true
                }
                else if (e.Key == Key.Right && randomIndex == 2)
                {
                    isCorrect = true;  // 如果按键正确，设置 isCorrect 为 true
                }
                else if (e.Key == Key.Up && randomIndex == 3)
                {
                    isCorrect = true;  // 如果按键正确，设置 isCorrect 为 true
                }
            }
            if (!isCorrect)
            {
                wrong++;
                if (IS_BEEP)
                    PlaySound(wrong_wav);
                if (IS_SCREEN)
                    _ = ChangeBorderColor();
            }
            if (isCorrect)
                increase++;
            else
                decrease++;
            // 调整难度
            //timecount += MAX_REACT / 1000 - remainingTime;           
            //press++;

            endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime; // 计算作答时间            
            timecount += (int)duration.TotalMilliseconds;
            press++;

            remainingTime = 0;
            AdjustDifficulty();
            ShowRandomImage();
        }


        private void AdjustDifficulty()
        {
            if (increase >= 30 && hardness <= MAX_HARDNESS)
            {
                // 提高难度
                hardness++;
                increase = 0;
                decrease = 0;
            }
            else if (decrease >= 30 && hardness > 1)
            {
                // 降低难度
                increase = 0; decrease = 0;
                hardness--;
            }
            else if (hardness == 1 && decrease >= 30)
            {
                increase = 0; decrease = 0;
            }
            else if (hardness == MAX_HARDNESS && increase >= 30)
            {
                increase = 0; decrease = 0;
            }
            if (hardness > max_hardness)
                max_hardness = hardness;
            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
            RightStatisticsAction?.Invoke(increase, 30);
            WrongStatisticsAction?.Invoke(decrease, 30);
        }
    }

    public partial class 反应能力 : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {

            random = new Random();
            //---------------------------------------------------------------------------------------------
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
                            case 162:// 等级
                                hardness = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"HARDNESS: {hardness}");
                                break;
                            case 194: // 治疗时间 
                                TRAIN_TIME = par.Value.HasValue ? (int)par.Value.Value * 60 : 60;
                                Debug.WriteLine($"TRAIN_TIME={TRAIN_TIME}");
                                break;
                            case 195: // 等级提高
                                INCREASE = par.Value.HasValue ? (int)par.Value.Value : 5;
                                Debug.WriteLine($"INCREASE={INCREASE}");
                                break;
                            case 196: // 等级降低
                                DECREASE = par.Value.HasValue ? (int)par.Value.Value : 3;
                                Debug.WriteLine($"DECREASE ={DECREASE}");
                                break;
                            case 201: // 刺激间隔
                                STIMULI_INTERVAL = par.Value.HasValue ? (int)par.Value.Value : 2;
                                STIMULI_INTERVAL = STIMULI_INTERVAL * 1000;
                                Debug.WriteLine($"刺激间隔{STIMULI_INTERVAL}");
                                break;
                            case 206: // 视觉反馈
                                IS_SCREEN = par.Value == 1;
                                Debug.WriteLine($"是否听觉反馈 ={IS_BEEP}");
                                break;
                            case 205: // 听觉反馈
                                IS_BEEP = par.Value == 1;
                                Debug.WriteLine($"是否视觉反馈 ={IS_SCREEN}");
                                break;
                            case 285: // 最长反应时间
                                MAX_REACT = par.Value.HasValue ? (int)par.Value.Value : 3;
                                MAX_REACT = MAX_REACT * 1000;
                                Debug.WriteLine($"最长反应时间{MAX_REACT}");
                                break;
                            // 添加其他需要处理的 ModuleParId
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

            traintime = TRAIN_TIME;
            remainingTime = MAX_REACT / 1000;
            counterA = 0;
            counterB = 0;
            overtime = 0;
            ignore = 0;
            wrong = 0;
            forbid.Visibility = Visibility.Visible;
            Forbid.Visibility = Visibility.Visible;
            Left.Visibility = Visibility.Collapsed;
            left_arrow.Visibility = Visibility.Collapsed;
            Right.Visibility = Visibility.Collapsed;
            right_arrow.Visibility = Visibility.Collapsed;
            forward.Visibility = Visibility.Collapsed;
            Forward.Visibility = Visibility.Collapsed;

            imageDisplayTimer = new DispatcherTimer();
            imageDisplayTimer.Tick += OnImageDisplayTimerElapsed;
            imageDisplayTimer?.Stop();

            displayTimer = new DispatcherTimer(); // 初始化 wildAnimalTimer
            displayTimer.Interval = TimeSpan.FromSeconds(1);
            displayTimer.Tick += OnDisplayTimerElapsed; // 绑定 Tick 事件
            displayTimer?.Stop(); // 初始时停止计时器

            trainingTimer = new DispatcherTimer(); // 初始化 trainingTimer
            trainingTimer.Interval = TimeSpan.FromSeconds(1); // 每秒触发一次
            trainingTimer.Tick += OnTrainingTimerElapsed; // 绑定 Tick 事件

            // 调用委托
            LevelStatisticsAction?.Invoke(hardness, MAX_HARDNESS);
            RightStatisticsAction?.Invoke(0, 30);
            WrongStatisticsAction?.Invoke(0, 30);
        }

        protected override async Task OnStartAsync()
        {
            trainingTimer.Start(); // 启动训练计时器
            Forbid.Source = new BitmapImage(new Uri($@"{imagePaths[0]}", UriKind.Relative));
            Left.Source = new BitmapImage(new Uri($@"{imagePaths[1]}", UriKind.Relative));
            Right.Source = new BitmapImage(new Uri($@"{imagePaths[2]}", UriKind.Relative));
            Forward.Source = new BitmapImage(new Uri($@"{imagePaths[3]}", UriKind.Relative));
            ShowRandomImage();
            OnGameStart();
            // 调用委托
            VoiceTipAction?.Invoke("请记住并熟悉屏幕上四个标志所对应的按键，在标志出现后在键盘上按下对应的按键。");
            SynopsisAction?.Invoke("测试题目说明信息");
            RuleAction?.Invoke("请记住并熟悉屏幕上四个标志所对应的按键，在标志出现后在键盘上按下对应的按键。");//增加代码，调用函数，显示数字人下的文字

        }

        protected override async Task OnStopAsync()
        {
            StopAllTimers();
        }

        protected override async Task OnPauseAsync()
        {
            StopAllTimers();
        }

        protected override async Task OnNextAsync()
        {
            ShowRandomImage();
            // 调用委托
            VoiceTipAction?.Invoke("测试返回语音指令信息");
            SynopsisAction?.Invoke("测试题目说明信息");
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new 反应能力讲解();
        }

        private int GetCorrectNum()
        {   // Ca +CB - wrong - ignore - overtime我没找到overtime
            int correct = counterA + counterB - wrong - ignore;
            if (correct > 0)
            {
                return correct;
            }
            else
            {
                return 0;
            }
        }
        private int GetCounterA()
        {
            return counterA;
        }
        private int GetCounterB()
        {
            return counterB;
        }
        private int GetWrongNum()
        {
            return wrong + overtime;
        }
        private int GetIgnoreNum()
        {
            return ignore;
        }
        private int GetunrelatedNum()
        {
            return overtime;
        }
        private double CalculateAccuracy()
        {
            int total = counterA + counterB + overtime;
            int a = GetCorrectNum();
            Debug.WriteLine($"计算时，总数为{total}，正确数为{a}");
            double accuracy = (double)GetCorrectNum() / total;  // 转换为 double 类型
            // ----------------------------问题在于，在C# 整数相除会去除小数部分，这里得强制加double才不会转类型
            return total > 0 ? Math.Round(accuracy, 2) : 0;
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
                        //只需要最高等级的报告
                        for (int lv = max_hardness; lv <= max_hardness; lv++)
                        {
                            // 获取当前难度级别的数据
                            int counterA = GetCounterA();
                            Debug.WriteLine($"难度级别1 {counterA}");
                            int counterB = GetCounterB();
                            Debug.WriteLine($"难度级别2 {counterB}");
                            int counterAB = counterA + counterB;
                            Debug.WriteLine($"难度级别3 {counterAB}: 没有数据，跳过.");
                            if (counterAB == 0)
                            {
                                // 如果所有数据都为0，跳过此难度级别
                                Debug.WriteLine($"难度级别 {lv}: 没有数据，跳过.");
                                continue;
                            }
                            int wrongCount = GetWrongNum();
                            Debug.WriteLine($"难度级别4 {wrongCount}: 没有数据，跳过.");
                            int ignoreCount = GetIgnoreNum();
                            Debug.WriteLine($"难度级别5 {ignoreCount}: 没有数据，跳过.");
                            int unrelateCount = GetunrelatedNum();
                            Debug.WriteLine($"难度级别6 {unrelateCount}: 没有数据，跳过.");
                            int correctall = counterA + counterB - wrong - ignore - overtime;
                            int wrongall = wrongCount + ignoreCount;
                            Debug.WriteLine($"难度级别7 {wrongall}: 没有数据，跳过.");
                            int correctCount = GetCorrectNum();
                            Debug.WriteLine($"难度级别8 {correctCount}: 没有数据，跳过.");
                            double train_time = (TRAIN_TIME - traintime) / (counterAB);
                            Debug.WriteLine($"难度级别9 {train_time}: 没有数据，跳过.");
                            double accuracy = CalculateAccuracy();
                            // 等级
                            /*Debug.WriteLine($"等级{lv}的参数：" + $"刺激相关{counterA},刺激非相关{counterB}" +
                                $"对{correctCount}，" + $"错误键盘{wrongCount}，遗漏{ignoreCount}" +
                                $"正确率{accuracy}" +
                                $"CounterA{counterA} CounterB{counterB}");*/

                            // 创建 Result 记录
                            var newResult = new Result
                            {
                                ProgramId = program_id, // program_id
                                Report = "反应能力",
                                Eval = false,
                                Lv = lv, // 当前的难度级别
                                ScheduleId = BaseParameter.ScheduleId ?? null // 假设的 Schedule_id，可以替换为实际值
                            };

                            db.Results.Add(newResult);
                            await db.SaveChangesAsync();

                            // 获得 result_id
                            int result_id = newResult.ResultId;

                            // 创建 ResultDetail 对象列表
                            var resultDetails = new List<ResultDetail>
                            {
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 0,
                                    ValueName = "等级",
                                    Value = max_hardness,
                                    Maxvalue = 16,
                                    Minvalue = 1,
                                    ModuleId = BaseParameter.ModuleId //  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 6,
                                    ValueName = "刺激",
                                    Value = counterAB,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 7,
                                    ValueName = "刺激相关",
                                    Value = counterA, // 以百分比形式存储
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 8,
                                    ValueName = "刺激非相关",
                                    Value = counterB, // 以百分比形式存储
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "正确率（%）",
                                    Value = accuracy * 100, // 以百分比形式存储
                                    Maxvalue = 100,
                                    Minvalue = 0,
                                    Charttype = "柱状图" ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                 new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 2,
                                    ValueName = "正确 总数",
                                    Value = correctCount, // 以百分比形式存储
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 3,
                                    ValueName = "错误 总数",
                                    Value = wrongall, // 以百分比形式存储
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 4,
                                    ValueName = "错误 按键",
                                    Value = wrongCount, // 以百分比形式存储
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 5,
                                    ValueName = "遗漏",
                                    Value = ignoreCount, // 以百分比形式存储
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 9,
                                    ValueName = "平均反应时间（ms）",
                                    Value = Math.Round(timecount/press, 2), // 以百分比形式存储
                                    Maxvalue = 5000,
                                    Minvalue = 0,
                                    Charttype = "柱状图" ,
                                    ModuleId = BaseParameter.ModuleId
                                }
                            };

                            // 插入 ResultDetail 数据
                            db.ResultDetails.AddRange(resultDetails);
                            await db.SaveChangesAsync();

                            // 输出每个 ResultDetail 对象的数据
                            Debug.WriteLine($"难度级别 {lv}:");
                            foreach (var detail in resultDetails)
                            {
                                Debug.WriteLine($"{detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId}");
                            }
                        }

                        // 提交事务
                        await transaction.CommitAsync();
                        Debug.WriteLine("插入成功");
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
