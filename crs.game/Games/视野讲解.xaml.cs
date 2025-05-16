using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Windows.Forms.AxHost;

namespace crs.game.Games
{
    /// <summary>
    /// 视野讲解.xaml 的交互逻辑
    /// </summary>
    public partial class 视野讲解 : BaseUserControl
    {
        public 视野讲解()
        {
            InitializeComponent();

            InitAll();

            CorrectSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");
            // 为 Image 控件加载图片 Source
            CorrectImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Correct.png"), UriKind.RelativeOrAbsolute));
            ErrorImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Error.png"), UriKind.RelativeOrAbsolute));

            this.Loaded += 视野讲解_Loaded;


        }

        private void 视野讲解_Loaded(object sender, RoutedEventArgs e)
        {
            Button_2_Click(null, null);
        }


        class RecordItem
        {
            public List<string> records;
            public RecordItem()
            {
                records = new List<string>();
            }
        }
        List<RecordItem> records;
        class DataItem
        {
            public string name { get; set; }
            public double correctRate { get; set; }
            public int reactionTime { get; set; }

            public int totalReactionTime { get; set; }
            public int totalReactionCount { get; set; }
            public int correctCount { get; set; }
            public int missingCount { get; set; }
            public int incorrectCount { get; set; }

            public DataItem()
            {
                correctRate = 0;
                reactionTime = 0;
                totalReactionTime = 0;
                totalReactionCount = 0;
                correctCount = 0;
                missingCount = 0;
                incorrectCount = 0;

            }

        }
        DataItem left_, right_, left_Top, left_Bottom, right_Top, right_Bottom;
        List<DataItem> allDataItem;

        List<double> allReactionTime;
        //记录每次休息为间隔的错误、遗漏数
        List<int> allIncorrectCount;
        List<int> allMissingCount;

        double time_round_pre = 900;    //ms
        double time_round_mid = 1900;
        double time_rest = 10;
        int stage = 1;
        int restCount = 0;
        int roundCount = 0; //每24次休息

        int time_AllTrainTIme = 0;
        DispatcherTimer _timer_AllTrainTime;

        DateTime curTime;
        DateTime lastTime;

        List<int> question_index;
        int curQuestion = 0;
        Random random = new Random();

        void OnContinueButton_Click(object sender, RoutedEventArgs e)
        {
            stage = 1;
            continueButton.Visibility = Visibility.Hidden;
            OrangeDot.Visibility = Visibility.Visible;

            _timer_AllTrainTime?.Start();
            lastTime = DateTime.Now;

            line.StrokeThickness = 0;
            grid.Children.Clear();

        }


        override protected void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

            if (e.Key == System.Windows.Input.Key.Enter)
            {
                state = true;
                DateTime dateTime = DateTime.Now;
                if (stage == 2)
                {
                    


                    int x = (curQuestion - 1) / 24 + 1;
                    int y = ((curQuestion - (x - 1) * 24) - 1) / 3 + 1;
                    int z = (curQuestion - 1) % 3 + 1;

                    if (curQuestion > 144)
                    {
                        ShowFeedbackImage(CorrectImage);
                        PlayWav(CorrectSoundPath);
                    }
                    else if (z == 3)
                    {
                        ShowFeedbackImage(ErrorImage);
                        PlayWav(ErrorSoundPath);

                        records[(curQuestion - 1) / 3 + 1].records.Add("F");
                        AddDataItem(x, y, z, 0);
                        allIncorrectCount[restCount]++;
                    }
                    else
                    {
                        ShowFeedbackImage(CorrectImage);
                        PlayWav(CorrectSoundPath);

                        records[(curQuestion - 1) / 3 + 1].records.Add((dateTime - lastTime).TotalMilliseconds.ToString());
                        AddDataItem(x, y, z, 1);
                        allReactionTime.Add((dateTime - lastTime).TotalMilliseconds);
                    }

                    time_round_pre = 2900 - (dateTime - lastTime).TotalMilliseconds - 100;

                    lastTime = dateTime;

                    roundCount++;
                    if (roundCount >= 24)
                    {

                        roundCount = 0;
                        stage = 3;
                        restCount++;

                        continueButton.Visibility = Visibility.Visible;
                        OrangeDot.Visibility = Visibility.Hidden;

                        _timer_AllTrainTime?.Stop();

                        line.StrokeThickness = 0;
                        grid.Children.Clear();
                    }
                    else
                    {
                        stage = 1;
                    }
                    grid.Children.Clear();
                    line.StrokeThickness = 0;
                }
                else if (stage == 4)
                {
                    ShowFeedbackImage(CorrectImage);
                    PlayWav(CorrectSoundPath);

                    time_round_pre = 2900 - (dateTime - lastTime).TotalMilliseconds - 100;

                    roundCount++;
                    if (roundCount >= 24)
                    {

                        roundCount = 0;
                        stage = 3;
                        restCount++;

                        continueButton.Visibility = Visibility.Visible;
                        OrangeDot.Visibility = Visibility.Hidden;


                        line.StrokeThickness = 0;
                        grid.Children.Clear();
                    }
                    else
                    {
                        stage = 1;
                    }
                    grid.Children.Clear();
                    line.StrokeThickness = 0;
                }
            }
        }


        void InitAll()
        {
            stage = 1;

            curTime = DateTime.Now;
            lastTime = DateTime.Now;
            question_index = new List<int>([1, 120, 145]);

            InitTimer();


            records = new List<RecordItem>();
            for (int i = 0; i <= 48; i++)
            {
                records.Add(new RecordItem());
            }
            left_ = new DataItem();
            right_ = new DataItem();
            left_Top = new DataItem();
            left_Bottom = new DataItem();
            right_Top = new DataItem();
            right_Bottom = new DataItem();

            allReactionTime = new List<double>();

            allIncorrectCount = new List<int>();
            allMissingCount = new List<int>();
            for (int i = 0; i <= 20; i++)
            {
                allIncorrectCount.Add(0);
                allMissingCount.Add(0);
            }
        }

        void InitTimer()
        {
            _timer_AllTrainTime = new DispatcherTimer();
            _timer_AllTrainTime.Interval = TimeSpan.FromSeconds(1);
            _timer_AllTrainTime.Tick += _timer_AllTrainTime_Tick;

        }

        private void _timer_AllTrainTime_Tick(object sender, EventArgs e)
        {
            time_AllTrainTIme++;


            curTime = DateTime.Now;

            if (stage == 1)
            {
                if ((curTime - lastTime).TotalMilliseconds >= time_round_pre)
                {
                    time_round_pre = 900;
                    stage = 2;
                    lastTime = curTime;

                    DrawNewRound();

                }
            }
            else if (stage == 2)
            {
                if ((curTime - lastTime).TotalMilliseconds >= time_round_mid)
                {

                    lastTime = curTime;

                    int x = (curQuestion - 1) / 24 + 1;
                    int y = ((curQuestion - (x - 1) * 24) - 1) / 3 + 1;
                    int z = (curQuestion - 1) % 3 + 1;



                    if (curQuestion > 144)
                    {
                        stage = 4;
                        WhiteDot.Visibility = Visibility.Visible;
                        image.Visibility = Visibility.Visible;
                        Task.Delay(200).ContinueWith(t =>
                        {
                            WhiteDot.Visibility = Visibility.Hidden;
                            image.Visibility = Visibility.Hidden;
                        }, TaskScheduler.FromCurrentSynchronizationContext());
                        return;
                    }
                    else
                    {
                        if (z == 3)
                        {
                            records[(curQuestion - 1) / 3 + 1].records.Add("---");
                            AddDataItem(x, y, z, 1);
                            allReactionTime.Add(2000);
                        }
                        else
                        {
                            records[(curQuestion - 1) / 3 + 1].records.Add("A");
                            AddDataItem(x, y, z, 2);

                            allMissingCount[restCount]++;
                        }
                    }

                    roundCount++;
                    if (roundCount >= 24)
                    {

                        roundCount = 0;
                        stage = 3;
                        restCount++;

                        continueButton.Visibility = Visibility.Visible;
                        OrangeDot.Visibility = Visibility.Hidden;


                        line.StrokeThickness = 0;
                        grid.Children.Clear();
                        stage = 1;
                    }
                    else
                    {
                        stage = 1;
                    }
                    grid.Children.Clear();
                    line.StrokeThickness = 0;
                }
            }
            else if (stage == 4)
            {
                if ((curTime - lastTime).TotalMilliseconds >= 2900)
                {
                    lastTime = curTime;

                    WhiteDot.Visibility = Visibility.Visible;
                    image.Visibility = Visibility.Visible;
                    Task.Delay(200).ContinueWith(t =>
                    {
                        WhiteDot.Visibility = Visibility.Hidden;
                        image.Visibility = Visibility.Hidden;
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }

            }


        }


        private SoundPlayer soundPlayer; // 用来放歌
        public string ErrorSoundPath;//错误的声音文件路径，在OnStartAsync()中配置
        public string CorrectSoundPath;//正确的声音文件路径，在OnStartAsync()中配置
        private int StopDurations = 2000; // 停止时间，ms

        private string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory; // 当前项目的绝对路径
        private string ResourcesPath = System.IO.Path.Combine("Resources", "词语记忆力");//这里用的固定是词语记忆力的路径，后期可改

        private void PlayWav(string filePath)
        {//播放本地的wav文件
            if (soundPlayer != null)
            {
                soundPlayer.Stop();
                soundPlayer.Dispose();
            }

            soundPlayer = new SoundPlayer(filePath);
            soundPlayer.Load();
            soundPlayer.Play();
        }

        private void ShowFeedbackImage(Image image)
        {//显示反馈的图片
            image.Visibility = Visibility.Visible;
            int StopDurations = image.Name switch
            {
                "CorrectImage" => 1500,
                "ErrorImage" => 1500,
                _ => 3000
            };
            // 延迟指定的时间（例如1秒）
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(StopDurations);
            timer.Tick += (sender, e) =>
            {
                HiddenFeedbackImage();
                timer.Stop();
            };
            timer.Start();

        }
        private void HiddenFeedbackImage()
        {
            CorrectImage.Visibility = Visibility.Collapsed;
            ErrorImage.Visibility = Visibility.Collapsed;
        }



        bool state = false; //是否已按下
        private void DrawNewRound()
        {


            int newQuestion = curQuestion;

            int zz = (curQuestion - 1) % 3 + 1;
            if (newQuestion != 0 && zz == 3 && state == true)
            {
                newQuestion = curQuestion;

                state = false;
            }
            else if (newQuestion != 0 && zz < 3 && state == false)
            {
                newQuestion = curQuestion;
            }
            else
            {
                if (question_index.Count == 0)
                {
                    end.Visibility = Visibility.Visible;
                    return;
                }


                int tmp = random.Next(question_index.Count);
                newQuestion = question_index[tmp];
                curQuestion = newQuestion;
                question_index.RemoveAt(tmp);

                state = false;
            }

            int x = (newQuestion - 1) / 24 + 1;
            int y = ((newQuestion - (x - 1) * 24) - 1) / 3 + 1;
            int z = (newQuestion - 1) % 3 + 1;
            if (newQuestion > 144)
            {
                WhiteDot.Visibility = Visibility.Visible;
                Task.Delay(200).ContinueWith(t =>
                {
                    WhiteDot.Visibility = Visibility.Hidden;
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                switch (z)
                {
                    case 1:
                        DrawEllipse(x, y);
                        DrawLine(x, y);
                        Task.Delay(200).ContinueWith(t =>
                        {
                            grid.Children.Clear();
                            line.StrokeThickness = 0;
                        }, TaskScheduler.FromCurrentSynchronizationContext());
                        break;
                    case 2:
                        DrawEllipse(x, y);
                        DrawLine(x, y);
                        Task.Delay(200).ContinueWith(t =>
                        {
                            grid.Children.Clear();
                            line.StrokeThickness = 0;
                        }, TaskScheduler.FromCurrentSynchronizationContext());
                        break;
                    case 3:
                        DrawLine(x, y);
                        Task.Delay(200).ContinueWith(t =>
                        {
                            grid.Children.Clear();
                            line.StrokeThickness = 0;
                        }, TaskScheduler.FromCurrentSynchronizationContext());
                        break;
                }
            }


        }


        void DrawEllipse(int x, int y)
        {
            Ellipse newEllipse = new Ellipse
            {

                Width = 50,
                Height = 50,
                Fill = Brushes.Orange

            };

            // 将Ellipse添加到Grid中，并指定行和列
            Grid.SetRow(newEllipse, x - 1);
            Grid.SetColumn(newEllipse, y - 1);

            // 将Ellipse添加到Grid的Children集合中
            grid.Children.Add(newEllipse);
        }

        void DrawLine(int x, int y)
        {
            Point point = GetGridPos(x, y);
            line.X2 = point.X;
            line.Y2 = point.Y;
            line.StrokeThickness = 20;

        }

        Point GetGridPos(int x, int y)
        {
            return new Point(167.5 * (y - 0.5), 153.8333 * (x - 0.5));
        }


        void AddDataItem(int x, int y, int z, int result, int time = 0)// result 0错误  ,1正确  ，2遗漏 
        {
            switch (result)
            {
                case 0:
                    if (x <= 4)
                    {
                        if (y <= 3)
                        {
                            left_Top.incorrectCount++;
                        }
                        else
                        {
                            left_Bottom.incorrectCount++;
                        }
                        left_.incorrectCount++;
                    }
                    else
                    {
                        if (y <= 3)
                        {
                            right_Top.incorrectCount++;
                        }
                        else
                        {
                            right_Bottom.incorrectCount++;
                        }
                        right_.incorrectCount++;
                    }
                    break;
                case 1:

                    if (x <= 4)
                    {
                        if (y <= 3)
                        {
                            left_Top.correctCount++;
                            if (z != 3)
                            {
                                left_Top.totalReactionCount++;
                                left_Top.totalReactionTime += time;
                            }
                        }
                        else
                        {
                            left_Bottom.correctCount++;
                            if (z != 3)
                            {
                                left_Bottom.totalReactionCount++;
                                left_Bottom.totalReactionTime += time;
                            }
                        }
                        left_.correctCount++;
                        if (z != 3)
                        {
                            left_.totalReactionCount++;
                            left_.totalReactionTime += time;
                        }

                    }
                    else
                    {
                        if (y <= 3)
                        {
                            right_Top.correctCount++;
                            if (z != 3)
                            {
                                right_Top.totalReactionCount++;
                                right_Top.totalReactionTime += time;
                            }
                        }
                        else
                        {
                            right_Bottom.correctCount++;
                            if (z != 3)
                            {
                                right_Bottom.totalReactionCount++;
                                right_Bottom.totalReactionTime += time;
                            }
                        }
                        right_.correctCount++;
                        if (z != 3)
                        {
                            right_.totalReactionCount++;
                            right_.totalReactionTime += time;
                        }
                    }
                    break;
                case 2:
                    if (x <= 4)
                    {
                        if (y <= 3)
                        {
                            left_Top.missingCount++;
                        }
                        else
                        {
                            left_Bottom.missingCount++;
                        }
                        left_.missingCount++;
                    }
                    else
                    {
                        if (y <= 3)
                        {
                            right_Top.missingCount++;
                        }
                        else
                        {
                            right_Bottom.missingCount++;
                        }
                        right_.missingCount++;
                    }
                    break;
            }

        }

        void CalAllData()
        {
            left_Top.correctRate = (double)left_Top.correctCount / (left_Top.correctCount + left_Top.incorrectCount + left_Top.missingCount);
            left_Top.reactionTime = (int)((double)left_Top.totalReactionTime / left_Top.totalReactionCount);
            if (left_Top.totalReactionCount == 0) { left_Top.correctRate = 0; left_Top.reactionTime = 0; }
            left_Top.name = "左上";

            left_Bottom.correctRate = (double)left_Bottom.correctCount / (left_Bottom.correctCount + left_Bottom.incorrectCount + left_Bottom.missingCount);
            left_Bottom.reactionTime = (int)((double)left_Bottom.totalReactionTime / left_Bottom.totalReactionCount);
            if (left_Bottom.totalReactionCount == 0) { left_Bottom.correctRate = 0; left_Bottom.reactionTime = 0; }
            left_Bottom.name = "左下";

            left_.correctRate = (double)left_.correctCount / (left_.correctCount + left_.incorrectCount + left_.missingCount);
            left_.reactionTime = (int)((double)left_.totalReactionTime / left_.totalReactionCount);
            if (left_.totalReactionCount == 0) { left_.correctRate = 0; left_.reactionTime = 0; }
            left_.name = "左半边";

            right_Top.correctRate = (double)right_Top.correctCount / (right_Top.correctCount + right_Top.incorrectCount + right_Top.missingCount);
            right_Top.reactionTime = (int)((double)right_Top.totalReactionTime / right_Top.totalReactionCount);
            if (right_Top.totalReactionCount == 0) { right_Top.correctRate = 0; right_Top.reactionTime = 0; }
            right_Top.name = "右上";

            right_Bottom.correctRate = (double)right_Bottom.correctCount / (right_Bottom.correctCount + right_Bottom.incorrectCount + right_Bottom.missingCount);
            right_Bottom.reactionTime = (int)((double)right_Bottom.totalReactionTime / right_Bottom.totalReactionCount);
            if (right_Bottom.totalReactionCount == 0) { right_Bottom.correctRate = 0; right_Bottom.reactionTime = 0; }
            right_Bottom.name = "右下";

            right_.correctRate = (double)right_.correctCount / (right_.correctCount + right_.incorrectCount + right_.missingCount);
            right_.reactionTime = (int)((double)right_.totalReactionTime / right_.totalReactionCount);
            if (right_.totalReactionCount == 0) { right_.correctRate = 0; right_.reactionTime = 0; }
            right_.name = "右半边";

            allDataItem = new List<DataItem> { left_Top, left_Bottom, left_, right_Top, right_Bottom, right_ };

        }




        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // 开始答题的相关逻辑
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
                        // 显示讲解的第一个界面
                        Text_1.Visibility = Visibility.Visible;
                        Text_2.Visibility = Visibility.Collapsed;
                        Text_3.Visibility = Visibility.Collapsed;

                        Image_3.Visibility = Visibility.Collapsed;
                        OrangeDot1.Visibility = Visibility.Visible;
                        whiteDot.Visibility = Visibility.Collapsed;
                        Line.Visibility = Visibility.Collapsed;


                        // 隐藏试玩部分内容
                        grid_0.Visibility = Visibility.Collapsed;
                        //TipBlock.Visibility = Visibility.Collapsed;
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Content = "下一步";
                        Button_2.Margin = new Thickness(329, 850, 0, 0);
                        Button_3.Margin = new Thickness(770, 850, 0, 0);

                        await OnVoicePlayAsync(Text_1.Text);
                    }
                    break;
                case 1:
                    {
                        // 显示讲解的第二个界面
                        Text_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Visible;
                        Text_3.Visibility = Visibility.Collapsed;

                        Image_3.Visibility = Visibility.Collapsed;
                        OrangeDot1.Visibility = Visibility.Visible;
                        whiteDot.Visibility = Visibility.Collapsed;
                        Line.Visibility = Visibility.Visible;

                        // 隐藏试玩部分内容
                        grid_0.Visibility = Visibility.Collapsed;
                        //TipBlock.Visibility = Visibility.Collapsed;

                        Button_1.Visibility = Visibility.Visible;
                        Button_2.Content = "下一步";
                        Button_2.Margin = new Thickness(550, 850, 0, 0);
                        Button_3.Margin = new Thickness(911, 850, 0, 0);

                        await OnVoicePlayAsync(Text_2.Text);
                    }
                    break;
                case 2:
                    {
                        // 显示讲解的第三个界面
                        Text_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Text_3.Visibility = Visibility.Visible;

                        Image_3.Visibility = Visibility.Visible;
                        OrangeDot1.Visibility = Visibility.Visible;
                        whiteDot.Visibility = Visibility.Visible;
                        Line.Visibility = Visibility.Collapsed;


                        // 隐藏试玩部分的控件
                        grid_0.Visibility = Visibility.Collapsed;
                        //TipBlock.Visibility = Visibility.Collapsed;
                        Button_1.IsEnabled = true;
                        Button_2.Content = "试玩";

                        await OnVoicePlayAsync(Text_3.Text);
                    }
                    break;
                case 3:
                    {
                        // 进入试玩界面
                        Text_1.Visibility = Visibility.Collapsed;

                        Text_2.Visibility = Visibility.Collapsed;

                        Text_3.Visibility = Visibility.Collapsed;
                        Image_3.Visibility = Visibility.Collapsed;
                        OrangeDot1.Visibility = Visibility.Collapsed;
                        whiteDot.Visibility = Visibility.Collapsed;
                        Line.Visibility = Visibility.Collapsed;

                        // 显示试玩部分的控件
                        grid_0.Visibility = Visibility.Visible;
                        //TipBlock.Visibility = Visibility.Visible;
                        // 隐藏讲解部分的按钮
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Collapsed;


                        _timer_AllTrainTime?.Start();
                        lastTime = DateTime.Now;

                        line.StrokeThickness = 0;
                        grid.Children.Clear();
                        stage = 1;


                        // 强制焦点保持在窗口
                        this.Focus();
                        //LJN，在讲解模块调用委托
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("现在您在屏幕上会先看到一个橙色圆心，当出现圆心与圆形连接或者重叠时，请按下“OK”按钮，其余情况请不要做出任何反应。");//增加代码，调用函数，显示数字人下的文字
                        //LJN

                    }
                    break;
            }
        }


    }
}
