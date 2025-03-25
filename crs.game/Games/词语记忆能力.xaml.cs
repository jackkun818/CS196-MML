using crs.core.DbModels;
using crs.core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace crs.game.Games
{
    /// <summary>
    /// 词语记忆能力.xaml 的交互逻辑
    /// </summary>
    public partial class 词语记忆能力 : BaseUserControl
    {
        public Action StopAction { get; set; }
        public Action<object> ProgressAction { get; set; }
        private DispatcherTimer gameTimer; // 计时器
        private TimeSpan totalGameTime; // 总游戏时间

        private List<string> wordsToMemorize = new List<string> { "苹 果", "香 蕉", "橙 子", "西 瓜", "草 莓", "汽 车", "自 行 车", "摩 托 车", "飞 机", "火 车", "铅 笔", "钢 笔", "橡 皮", "尺 子", "笔 记 本" };
        private List<string> testWords = new List<string>
        { 
            // 水果类
            "梨", "桃 子", "葡 萄", "菠 萝", "柚 子",
            "樱 桃", "椰 子", "蓝 莓", "黑 莓", "柠 檬",
            "酸 橙", "番 茄", "红 枣", "荔 枝", "芒 果",
            "甜 瓜", "山 竹", "无 花 果", "橙 子", "榴 莲",
    
            // 交通工具类
            "轮 船", "电 动 车", "直 升 机", "小 型 飞 机", "脚 踏 车",
            "滑 板 车", "公 交 车", "货 车", "轿 车", "旅 游 车",
            "三 轮 车", "赛 摩 托", "轨 道 交 通", "轻 轨", "高 速 列车",
            "观 光 巴 士", "机 动 船", "皮 划 艇", "赛 艇", "自 驾 车",
    
            // 文具类
            "书", "记 号 笔", "书 写 板", "纸 张", "文 件 夹",
            "订 书 机", "便 签 纸", "笔 筒", "涂 改 液", "画 笔",
            "水 彩 笔", "画 纸", "笔 记 本 电 脑", "画 板", "计 算 器",
            "书 签", "速 写 本", "夹 子", "书 法 笔", "画 册"
        };
        private HashSet<string> memorizedWords = new HashSet<string>();
        List<string> result = new List<string>();
        private int score = 0;
        private int totalTests = 72; // 总测试次数
        private int currentTestCount = 0; // 当前测试次数
        private int incorrectCount = 0; // 错误次数
        private int skippedCount = 0; // 跳过次数

        private int[] incorrectcount;//新加的折线图数据：累计错误数

        public 词语记忆能力()
        {
            InitializeComponent();
        }
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            totalGameTime = totalGameTime.Add(TimeSpan.FromSeconds(1)); // 每次增加一秒
            // 获取总秒数
            int totalSeconds = (int)totalGameTime.TotalSeconds;

            // 调用委托
            TimeStatisticsAction?.Invoke(totalSeconds, totalSeconds);

        }

        private void StartMemorizationPhase()
        {
            Random random = new Random();

            var selectedWords = wordsToMemorize.OrderBy(x => random.Next()).Take(5).ToList();
            foreach (var word in selectedWords)
            {
                memorizedWords.Add(word); // 将每个词存入集合
            }

            var availableTestWords = new List<string>(testWords);

            for (int i = 0; i < 6; i++)
            {
                var group = new List<string>();

                group.AddRange(selectedWords);

                var randomTestWords = availableTestWords.OrderBy(x => random.Next()).Take(7).ToList();
                group.AddRange(randomTestWords);

                foreach (var word in randomTestWords)
                {
                    availableTestWords.Remove(word);
                }

                group = group.OrderBy(x => random.Next()).ToList();

                result.AddRange(group);
            }
            WordTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            EnterTestingPhase();
        }



        private void EnterTestingPhase()
        {
            if (currentTestCount >= totalTests)
            {
                ShowResults();
                return;
            }

            Random random = new Random();
            WordTextBlock.Text = result[currentTestCount]; // 使用 currentTestCount 作为索引

            RightStatisticsAction?.Invoke(score, 10);
            WrongStatisticsAction?.Invoke((incorrectCount + skippedCount), 10);
        }


        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            string currentWord = WordTextBlock.Text;
            if (memorizedWords.Contains(currentWord) && currentTestCount >= 12)
            {
                score++;
            }
            else
            {
                incorrectCount++;
                if (currentTestCount >= 0)
                {
                    for (int i = (currentTestCount) / 12; i < incorrectcount.Length; i++)
                    {
                        incorrectcount[i]++;
                    }
                }
            }
            currentTestCount++; // 增加测试次数后进入下一轮
            RightStatisticsAction?.Invoke(score, 10);
            WrongStatisticsAction?.Invoke((incorrectCount + skippedCount), 10);
            EnterTestingPhase();
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            string currentWord = WordTextBlock.Text;
            if (memorizedWords.Contains(currentWord) && currentTestCount >= 12)
            {
                skippedCount++;
                for (int i = (currentTestCount) / 12; i < incorrectcount.Length; i++)
                {
                    incorrectcount[i]++;
                }
            }
            currentTestCount++; // 增加测试次数后进入下一轮
            RightStatisticsAction?.Invoke(score, 10);
            WrongStatisticsAction?.Invoke((incorrectCount + skippedCount), 10);
            EnterTestingPhase();
        }

        private async void ShowResults()
        {
            // 触发事件
            //词汇记忆能力报告 nwd=new 词语记忆能力报告(score,totalTests,incorrectCount,skippedCount);
            //nwd.Show();
            OnGameEnd();
            Button_Click(null, null);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            gameTimer?.Stop();
        }
        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                OKButton_Click(sender, e);
            }
            if (e.Key == Key.Right)
            {
                SkipButton_Click(sender, e);
            }
        }
    }
    public partial class 词语记忆能力 : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
            currentTestCount = 0; // 当前测试次数
            incorrectCount = 0; // 错误次数
            skippedCount = 0; // 跳过次数
            RightStatisticsAction?.Invoke(0, 10);
            WrongStatisticsAction?.Invoke(0, 10);

        }

        protected override async Task OnStartAsync()
        {
            totalGameTime = TimeSpan.Zero; // 重置总时间
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1); // 每秒更新一次
            gameTimer.Tick += GameTimer_Tick; // 绑定计时器事件
            gameTimer.Start(); // 启动计时器
            incorrectcount = new int[6];
            for (int i = 0; i < incorrectcount.Length; i++)
            {
                incorrectcount[i] = 0;
            }
            StartMemorizationPhase();
            // 调用委托
            VoiceTipAction?.Invoke("请从屏幕出现的词语中找出重复的词语。");
            SynopsisAction?.Invoke("本题将有一些词重复出现,请找到重复出现的词语。 当重复的词出现时，请您点击“是”按钮，否则点击“否”按钮");
            RuleAction?.Invoke("本题将有一些词重复出现,请找到重复出现的词语。 当重复的词出现时，请您点击“是”按钮，否则点击“否”按钮");//增加代码，调用函数，显示数字人下的文字

        }

        protected override async Task OnStopAsync()
        {
            gameTimer?.Stop();
        }

        protected override async Task OnPauseAsync()
        {
            gameTimer?.Stop();
        }

        protected override async Task OnNextAsync()
        {

            // 调用委托
            VoiceTipAction?.Invoke("请从屏幕出现的词语中找出重复的词语。");
            SynopsisAction?.Invoke("本题将有一些词重复出现,请找到重复出现的词语。 当重复的词出现时，请您点击“是”按钮，否则点击“否”按钮");
            RuleAction?.Invoke("本题将有一些词重复出现,请找到重复出现的词语。 当重复的词出现时，请您点击“是”按钮，否则点击“否”按钮");//增加代码，调用函数，显示数字人下的文字
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new 词语记忆能力讲解();
        }

        private int GetCorrectNum()
        {
            return score;
        }
        private int GetWrongNum()
        {
            return incorrectCount + skippedCount;//把遗漏次数算进错误次数里
        }
        private int GetIgnoreNum()
        {
            return skippedCount;
        }
        //正确率固定以25作为分母
        private double CalculateAccuracy(int correctCount)
        {
            const int totalRequiredCorrect = 25; // 答对 25 个即为 100%
            return correctCount >= totalRequiredCorrect ? 1.0 : Math.Round((double)correctCount / totalRequiredCorrect, 2);
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
                        // 获取当前难度级别的数据
                        int correctCount = GetCorrectNum();
                        int wrongCount = GetWrongNum();
                        int ignoreCount = GetIgnoreNum();
                        int diasCount = (correctCount - wrongCount);//增加偏差参数
                        double totalMilliseconds = totalGameTime.TotalMilliseconds;
                        double time = Math.Round((double)totalMilliseconds / currentTestCount, 0);
                        // 计算准确率
                        double accuracy = CalculateAccuracy(correctCount);
                        double ZcorrectCount = Math.Round((double)(correctCount - 21.7) / 3, 2);//增加z值正确参数
                        double ZwrongCount = Math.Round((double)(wrongCount - 4) / 20.00, 2);//增加z值错误参数
                        double ZdiasCount = Math.Round((double)((correctCount - wrongCount) - 21.5) / 3.4, 2);//增加Z值语言学习能力参数

                        // 创建 Result 记录
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "词汇记忆能力评估报告",
                            Eval = true,
                            Lv = null, // 当前的难度级别
                            ScheduleId = BaseParameter.ScheduleId ?? null // 假设的 Schedule_id，可以替换为实际
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
                                    ValueName = "正确",
                                    Order = 0,
                                    Value = correctCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "错误",
                                    Order = 1,
                                    Value = wrongCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                 new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "偏差",
                                    Order = 2,
                                    Value = diasCount, // 以百分比形式存储
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "正确率",
                                    Order = 3,
                                    Value = Math.Round(accuracy * 100, 2), // 以百分比形式存储
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "平均反应时间(ms)",
                                    Order = 4,
                                    Value = Math.Round(time, 2), // 以百分比形式存储
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Z值正确",
                                    Order = 5,
                                    Value = Math.Round(ZcorrectCount, 2),
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Z值错误",
                                    Order = 6,
                                    Value = Math.Round(ZwrongCount, 2),
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    ValueName = "Z值语言学习能力",
                                    Order = 7,
                                    Value = Math.Round(ZdiasCount, 2),
                                    ModuleId = BaseParameter.ModuleId
                                }
                            };

                        resultDetails.AddRange(incorrectcount.Select((value, index) => new ResultDetail
                        {
                            ResultId = result_id,
                            ValueName = "任务顺序(组),错误数(个)",
                            Value = value,
                            Abscissa = index + 2,
                            Charttype = "折线图",
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