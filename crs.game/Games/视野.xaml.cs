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
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace crs.game.Games
{
    /// <summary>
    /// 视野.xaml 的交互逻辑
    /// </summary>
    public partial class 视野 : BaseUserControl
    {
        public 视野()
        {
            InitializeComponent();

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
                DateTime dateTime = DateTime.Now;
                if (stage == 2)
                {



                    int x = (curQuestion - 1) / 24 + 1;
                    int y = ((curQuestion - (x - 1) * 24) - 1) / 3 + 1;
                    int z = (curQuestion - 1) % 3 + 1;

                    if (curQuestion > 144)
                    {

                    }
                    else if (z == 3)
                    {
                        records[(curQuestion - 1) / 3 + 1].records.Add("F");
                        AddDataItem(x, y, z, 0);
                        allIncorrectCount[restCount]++;
                    }
                    else
                    {
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
            question_index = new List<int>();
            for (int i = 1; i <= 240; i++)
            {
                question_index.Add(i);
            }
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
            TimeStatisticsAction.Invoke(time_AllTrainTIme, time_AllTrainTIme);

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

        private void DrawNewRound()
        {
            if (question_index.Count == 0)
            {
                OnStopAsync();
                return;
            }
            int tmp = random.Next(question_index.Count);
            int newQuestion = question_index[tmp];
            curQuestion = newQuestion;
            question_index.RemoveAt(tmp);

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

    }
    public partial class 视野 : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
            InitAll();

            LevelStatisticsAction?.Invoke(0, 0);
            RightStatisticsAction?.Invoke(0, 10);
            WrongStatisticsAction?.Invoke(0, 10);

        }

        protected override async Task OnStartAsync()
        {
            _timer_AllTrainTime?.Start();
            lastTime = DateTime.Now;

            line.StrokeThickness = 0;
            grid.Children.Clear();
            stage = 1;

            VoiceTipAction?.Invoke("当出现圆心与圆形连接或者重叠时，请按下“OK”按钮。");
            SynopsisAction?.Invoke("现在您在屏幕上会先看到一个橙色圆心，当出现圆心与圆形连接或者变为圆环时，请按下“OK”按钮，其余情况请不要做出任何反应。");
            RuleAction?.Invoke("现在您在屏幕上会先看到一个橙色圆心，当出现圆心与圆形连接或者变为圆环时，请按下“OK”按钮，其余情况请不要做出任何反应。");
        }

        protected override async Task OnStopAsync()
        {
            _timer_AllTrainTime?.Stop();
        }

        protected override async Task OnPauseAsync()
        {
            _timer_AllTrainTime?.Stop();

            line.StrokeThickness = 0;
            grid.Children.Clear();
            stage = 1;
        }

        protected override async Task OnNextAsync()
        {
            line.StrokeThickness = 0;
            grid.Children.Clear();
            stage = 1;
            lastTime = DateTime.Now;
            // 调用委托
            VoiceTipAction?.Invoke("现在您在屏幕上会先看到一个橙色圆心，当出现圆心与圆形连接或者重叠时，请用鼠标点击确认按钮，其余情况请不要做出任何反应。");
            SynopsisAction?.Invoke("现在您在屏幕上会先看到一个橙色圆心，当出现圆心与圆形连接或者重叠时，请用鼠标点击确认按钮，其余情况请不要做出任何反应。");
            RuleAction?.Invoke("现在您在屏幕上会先看到一个橙色圆心，当出现圆心与圆形连接或者重叠时，请用鼠标点击确认按钮，其余情况请不要做出任何反应。");
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new 视野讲解();
        }


        private async Task updateDataAsync()
        {
            CalAllData();
            var baseParameter = BaseParameter;

            var program_id = baseParameter.ProgramId;
            Crs_Db2Context db = baseParameter.Db;

            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {

                    // 创建 Result 记录
                    var newResult = new Result
                    {
                        ProgramId = BaseParameter.ProgramId, // program_id
                        Report = "视野评估报告",
                        Eval = true,
                        Lv = null, // 当前的难度级别
                        ScheduleId = BaseParameter.ScheduleId ?? null // 假设的 Schedule_id，可以替换为实际
                    };
                    db.Results.Add(newResult);
                    await db.SaveChangesAsync();
                    // 获得 result_id
                    int result_id = newResult.ResultId;
                    // 创建 ResultDetail 对象列表
                    var resultDetails = new List<ResultDetail>();
                    resultDetails.AddRange(allDataItem.Select((x, id) => new List<ResultDetail>
                    {
                        new ResultDetail
                        {
                            Order = id + 10,
                            ResultId = result_id,
                            ValueName = x.name + ",正确率",
                            Value = Math.Round(x.correctRate, 2),
                            ModuleId = BaseParameter.ModuleId
                        },
                        new ResultDetail
                        {
                            Order = id + 20,
                            ResultId = result_id,
                            ValueName = x.name + ",中央反应时间",
                            Value = x.reactionTime,
                            ModuleId = BaseParameter.ModuleId
                        },
                        new ResultDetail
                        {
                            Order = id + 30,
                            ResultId = result_id,
                            ValueName = x.name + ",遗漏",
                            Value = x.missingCount,
                            ModuleId = BaseParameter.ModuleId
                        },
                        new ResultDetail
                        {
                            Order = id + 40,
                            ResultId = result_id,
                            ValueName = x.name + ",错误",
                            Value = x.incorrectCount,
                            ModuleId = BaseParameter.ModuleId
                        },

                    }).ToList().SelectMany(x => x).ToList());

                    resultDetails.AddRange(allReactionTime.Select((x, index) => new ResultDetail
                    {
                        ResultId = result_id,
                        ValueName = "任务顺序,反应时间[ms]",
                        Value = Math.Round(x, 2),
                        Abscissa = index + 1,
                        Charttype = "折线图",
                        ModuleId = BaseParameter.ModuleId
                    }).ToList());

                    resultDetails.AddRange(allIncorrectCount.Select((x, index) => new ResultDetail
                    {
                        ResultId = result_id,
                        ValueName = "休息,错误次数",
                        Value = x,
                        Abscissa = index + 1,
                        Charttype = "柱状图",
                        ModuleId = BaseParameter.ModuleId
                    }).ToList());

                    resultDetails.AddRange(allMissingCount.Select((x, index) => new ResultDetail
                    {
                        ResultId = result_id,
                        ValueName = "休息,遗漏次数",
                        Value = x,
                        Abscissa = index + 1,
                        Charttype = "柱状图",
                        ModuleId = BaseParameter.ModuleId
                    }).ToList());


                    // 插入 ResultDetail 数据
                    db.ResultDetails.AddRange(resultDetails);
                    await db.SaveChangesAsync();
                    // 输出每个 ResultDetail 对象的数据
                    /*Debug.WriteLine($"难度级别 {lv}:");*/
                    foreach (var detail in resultDetails)
                    {
                        Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId}");
                    }

                    // 提交事务
                    await transaction.CommitAsync();
                    Debug.WriteLine("插入成功");
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

