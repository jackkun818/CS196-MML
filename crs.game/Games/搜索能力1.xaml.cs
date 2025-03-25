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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Media;
using System.Windows.Ink;

namespace crs.game.Games
{
    /// <summary>
    /// EXO.xaml 的交互逻辑
    /// </summary>
    public partial class 搜索能力1 : BaseUserControl
    {
        /*
         游戏参数
         */
        private int max_time = 1; // 窗口总的持续时间，单位分钟
        private int MaxLevel = 18;//最大的难度等级，可调
        private int LevelUp = 5;//等级提高需要做对的题数
        private int LevelDown = 5;//等级降低需要做对的题数
        private bool IfLimitTime = false;//是否限制作答时间
        private int train_mode = 1; // 游戏模式，1，2，3，4	1.	模式1：找出数字范围内的缺失数字，并将它们从小到大逐个输入。这种模式通常涉及用户识别和输入缺失的数字。模式2：识别出叠加在一起的不同形状，并将它们从屏幕下部选择出来。这种模式涉及到用户需要从叠加的形状中找到正确的形状。模式4：数出并输入每个正确对象在图片中出现的次数。
        private int Level = 1; // 当前游戏难度等级
        private bool IfVisualFeedBack = true;//视觉反馈
        private bool IfAudioFeedBack = true;//声音反馈
        /*
         游戏变量
         */
        private int MinLevel = 1;//最小的等级
        private List<int> missingNumbers;//缺的数字
        private List<int> userInputNumbers;//用户输入的数字
        private string userInput; // 存储用户输入的数字

        private bool is_finish = true;//是否完成

        private int repeat_time = 0;

        private DispatcherTimer gameTimer; // 全局计时器
        private TimeSpan timeRemaining; // 剩余时间

        private DispatcherTimer PlayTimer;//作答时间限制，勾选了IfLimitTime才有效
        private int BaseTimeLimit = 60;//最低等级的限制时间，秒
        private int TimeLimitInterval = 5;//不同等级之间的时间限制差距，秒，等差数列
        private int PlayTime = 0;//限制的答题时间，根据等级变化来设置

        private DispatcherTimer IntervalTimer;//题与题之间的间隔计时器
        private int CorrectInterval = 3;//答对3s间隔
        private int ErrorInterval = 5;//答错5s间隔

        private int maxnumber = 5; // 显示的最大数字
        private int minnumber = 1;//显示的最小数字
        private int miss_number = 2; // 消失的数字数量
        private List<int> AllNumbers = new List<int>();//用来存储所有数字的列表
        private int mode1_display_size = 4; // 显示框的大小：1=小，2=中，3=大，4=全屏

        private int success_time = 0;//连续答对几道题，用来显示星星
        private int fail_time = 0;//连续答错几道题，用来显示星星
        private bool IfLevelDown = false;//需不需要降低等级
        private bool IfLevelUp = false;//需不需要升等级
        private Queue<bool> ResultQueue = new Queue<bool>();//用来存储最近若干道题的结果，是当需要连续答对若干道提升降难度用的，如果是累计答对答错则不需要使用它
        private Dictionary<int, TextBlock> NumTextDict = new Dictionary<int, TextBlock>();//用来存储数字和对应的Textblock对象，后期好索引

        private int[] correctAnswers;
        private int[] wrongAnswers;
        private int[] ignoreAnswers;

        private int ErrorCount = 2;//患者在一道题中顶多能找错多少次数字
        private int ErrorLimit = 2;

        public 搜索能力1()
        {
            InitializeComponent();

            this.Cursor = Cursors.None; // 隐藏默认光标
            CustomCursor.Visibility = Visibility.Visible; // 显示自定义光标
            MouseMove += Window_MouseMove; // 订阅 MouseMove 事件
            CustomCursor.Width = 65; // 调整为你想要的宽度
            CustomCursor.Height = 65; // 调整为你想要的高度
        }

        public static Window GetTopLevelWindow(UserControl userControl)
        {
            DependencyObject current = userControl;
            while (current != null && !(current is Window))
            {
                current = VisualTreeHelper.GetParent(current);
            }

            return current as Window;
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            // 每秒减少剩余时间
            if (timeRemaining > TimeSpan.Zero)
            {
                timeRemaining = timeRemaining.Add(TimeSpan.FromSeconds(-1));//每秒同步训练计时
                int remainingSeconds = (int)timeRemaining.TotalSeconds;

                TimeStatisticsAction.Invoke(remainingSeconds, PlayTime);
                //remainingSeconds是总的，后面那个空是目前的时间，显示在患者端右侧
                WrongStatisticsAction?.Invoke(fail_time, LevelDown);
                RightStatisticsAction?.Invoke(success_time, LevelUp);
            }
            else
            {
                gameTimer.Stop(); // 停止计时器

                OnGameEnd();
            }
        }

        private void InitializeGame()
        {
            //ResetGameState(); // 在开始新游戏前重置状态

            //显示对应的规则
            VoiceTipAction?.Invoke("请找出数字范围内的缺失数字，并将它们从小到大逐个输入。");
            RuleAction?.Invoke("找出数字范围内的缺失数字，并将它们从小到大逐个输入。");//增加代码，调用函数，显示数字人下的文字
            SetupGameMode1();
            repeat_time += 1;//记录游玩次数
        }

        private void ResetGameState()
        {
            // 重置游戏状态变量
            missingNumbers = new List<int>();
            userInputNumbers = new List<int>();
            userInput = string.Empty;
            UpdateTextBlock();
            AdjustDifficulty();
            MainGrid.RowDefinitions.Clear();
            MainGrid.ColumnDefinitions.Clear();

            // 重置模式特定的变量
            is_finish = true;

            // 移除动态添加的 UI 元素，保留静态组件
            for (int i = MainGrid.Children.Count - 1; i >= 0; i--)
            {
                var child = MainGrid.Children[i];
                if (child != textBlock && child != myCanvas)
                {
                    MainGrid.Children.RemoveAt(i);
                }
            }

            // 重置 UI 组件的可见性
            textBlock.Visibility = Visibility.Collapsed;//数字键盘上面的文本框，用于显示用户输入的数字
            myCanvas.Visibility = Visibility.Collapsed;//数字键盘所在的canvas
        }

        private void SetupGameMode1()
        {
            NumTextDict.Clear();
            ErrorCount = ErrorLimit;

            if (IfLimitTime)
            {//如果限制作答时间
                PlayTime = BaseTimeLimit + (Level - MinLevel) * TimeLimitInterval;//计算好时间限制
                PlayTimer.Start();
            }

            textBlock.Visibility = Visibility.Visible;
            myCanvas.Visibility = Visibility.Visible;

            // 清除上一次游戏的内容
            MainGrid.Children.Clear();

            // 根据 mode1_display_size 设置数字显示框的大小
            double width = 1072;
            double height = 920;

            switch (mode1_display_size)
            {
                case 1:
                    width = width * 0.6;
                    height = height * 0.6;
                    break;
                case 2:
                    width = width * 0.7;
                    height = height * 0.7;
                    break;
                case 3:
                    width = width * 0.85;
                    height = height * 0.85;
                    break;
                case 4:
                    width = width * 1.0;  // 手动设置大小
                    height = height * 1.0; // 手动设置大小
                    break;
            }

            // 创建一个带有色边框的透明长方形
            Border gameAreaBorder = new Border
            {
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(5),
                Width = width,
                Height = height,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            MainGrid.Children.Add(gameAreaBorder);

            // 随机生成数字列表并移除几个数字

            List<int> numbers = Enumerable.Range(minnumber, maxnumber).ToList();
            AllNumbers = new List<int>(numbers); ;//存储所有数字,为后续选中数字出现做准备
            missingNumbers = RemoveRandomNumbers(numbers);//这个时候numbers是已经去掉missingNumbers后的列表，已经不完整了
            // 用来存储已占用的位置（使用 HashSet 提高效率）
            HashSet<Rect> usedPositions = new HashSet<Rect>();
            // 显示剩余的数字，并随机分布在显示区域中
            Canvas numbersCanvas = new Canvas();
            Random rand = new Random();

            // 假设 gameAreaBorder 已经存在，且有它的宽度和高度
            double borderLeft = 0;
            double borderTop = 0;
            double borderRight = width;
            double borderBottom = height;

            foreach (int number in AllNumbers)
            {
                TextBlock numberText = new TextBlock
                {
                    Text = number.ToString(),
                    FontSize = Math.Sqrt(width * height / (maxnumber - minnumber) * 0.5) * 0.6, // 使得数字的大小也不会太小
                    Foreground = Brushes.Black,
                    Visibility = Visibility.Visible
                };

                if (numbers.Contains(number)) { numberText.Visibility = Visibility.Visible; }
                else { numberText.Visibility = Visibility.Hidden; numberText.Foreground = Brushes.Orange; } // 如果是缺失了的数字，则它要隐藏

                NumTextDict[number] = numberText;//建立数字到textblock的索引

                // 尝试找到一个随机的不重叠的位置
                bool positionFound = false;
                double left = 0, top = 0;
                int attempts = 0;  // 限制尝试次数，防止无限循环
                while (!positionFound && attempts < 10000)
                {
                    // 随机生成一个位置
                    left = rand.Next(0, Math.Max(1, (int)(width - numberText.FontSize)));
                    top = rand.Next(0, Math.Max(1, (int)(height - numberText.FontSize)));

                    // 创建一个新的矩形，表示 numberText 的位置
                    Rect newRect = new Rect(left, top, numberText.FontSize, numberText.FontSize);

                    // 检查是否与现有的位置重叠
                    bool overlapsWithExisting = usedPositions.Any(existingRect => existingRect.IntersectsWith(newRect));

                    // 检查是否超出了 gameAreaBorder 的边界
                    bool overlapsWithBorder = newRect.Left < borderLeft - 5 || newRect.Top < borderTop - 5 ||
                                              newRect.Right > borderRight - 5 || newRect.Bottom > borderBottom - 5;//-5是想加入一个裕量，确保不接触边界

                    // 如果没有重叠且没有超出边界，则位置有效
                    if (!overlapsWithExisting && !overlapsWithBorder)
                    {
                        usedPositions.Add(newRect);  // 如果没有重叠，记录位置
                        positionFound = true;
                    }

                    attempts++;  // 增加尝试次数
                }

                // 如果找到了合适位置
                if (positionFound)
                {
                    // 设置位置
                    Canvas.SetLeft(numberText, left);
                    Canvas.SetTop(numberText, top);
                    numbersCanvas.Children.Add(numberText);
                }
            }


            gameAreaBorder.Child = numbersCanvas;
        }

        private List<int> RemoveRandomNumbers(List<int> numbers)
        {
            Random rand = new Random();
            List<int> removedNumbers = new List<int>();
            while (removedNumbers.Count < miss_number)
            {
                int index = rand.Next(numbers.Count);
                if (numbers[index] == minnumber || numbers[index] == maxnumber) { continue; }//确保不会把最小值和最大值去掉
                removedNumbers.Add(numbers[index]);
                numbers.RemoveAt(index);
            }

            return removedNumbers.OrderBy(n => n).ToList(); // 返回已移除的数字（排序后的）
        }

        // 数字按钮按下事件处理函数
        private void OnNumberButtonClick(object sender, RoutedEventArgs e)//用户输入数字，并同步显示到框框里
        {
            if (sender is Button button)
            {//那如果输入的是两位数怎么办？
                userInput += button.Content.ToString();
                UpdateTextBlock();
            }
        }

        // "✔" 按钮按下事件处理函数，即提交本次作答的答案
        private void OnSubmitButtonClick(object sender, RoutedEventArgs e)
        {//首先明确，用户一次只能提交一个答案，且只有两次答错机会
            if (!string.IsNullOrEmpty(userInput))
            {
                //先把用户输入解析成int
                int number = -1;
                // 使用 TryParse 来安全地解析整数，避免异常
                if (int.TryParse(userInput, out number))
                {
                    number = int.Parse(userInput);
                }
                else
                {
                    // 处理解析失败的情况（例如，输入的不是有效数字）
                    number = -1; // 或者设置一个默认值
                }
                userInputNumbers.Add(number);//加到结果里来

                userInput = string.Empty;
                UpdateTextBlock();
            }
            SubmitInput();//模式1提交输入
        }

        // "←" 按钮按下事件处理函数，新功能：删除上一个输入的数字
        private void OnBackButtonClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(userInput))
            {
                // 删除最后一个数字
                userInput = userInput.Substring(0, userInput.Length - 1);
                UpdateTextBlock();
            }
        }

        private void UpdateTextBlock()
        {//为什么不把displayTextBlock.Text和userInput绑定起来嘞
            displayTextBlock.Text = userInput;
        }

        private void SubmitInput()//就是在这个函数里面执行判断主逻辑
        {
            //将用户本次输入的数字与缺失的数字列表进行比较
            int InputNum = userInputNumbers.LastOrDefault(); ;//获取最后一个输入
            bool isCorrect = missingNumbers.Contains(InputNum);
            if (AllNumbers.Contains(InputNum))//用户输入的数字在范围内
            {
                if (isCorrect)
                {//如果用户本次输入的数字是缺失的数字的其中之一，则显示到右侧数字区域
                    NumTextDict[InputNum].Visibility = Visibility.Visible;//数字可见
                    missingNumbers.Remove(InputNum);//缺失数字列表中去除这个数字
                }
                else
                {
                    NumTextDict[InputNum].Foreground = Brushes.Red;//红色以警示
                    ErrorCount--;
                }
            }
            else
            {//如果不在范围内直接错
                ErrorCount--;
                isCorrect = false;
            }
            GroupResultCheck(isCorrect);//检查需不需要判断这个整道题的对错
        }

        private void EndGame()
        {
            ResetGameState(); // 重置游戏状态
            InitializeGame(); // 开始新游戏
        }

        private void GroupResultCheck(bool IsNumFeedBack)//完整的这道题的结果检查，判断整道题的对错
        {//IsNumFeedBack是用来判断需不需要给单个数字判断反馈，可以防止和整道题的反馈重叠
            bool IfChecked = false;//设置它是用来判断这整道题到底做完没有
            if (ErrorCount <= 0)
            {//在这一整道题中找错了若干个数字，不行了整道题要错了
                fail_time++; IfChecked = true;
                wrongAnswers[Level] += 1;
                IntervalTimer.Interval = TimeSpan.FromSeconds(ErrorInterval);
                if (IfAudioFeedBack) PlayWav(ErrorSoundPath);
                if (IfVisualFeedBack) ShowFeedbackImage(ErrorImage, ErrorInterval * 1000);

                int ignoreCount = missingNumbers.Count(number => !userInputNumbers.Contains(number));
                ignoreAnswers[Level] += ignoreCount;

                is_finish = false;
            }
            else if (missingNumbers.Count <= 0)
            {//说明所有的数字都找完了，相当于答对了
                success_time++; IfChecked = true;
                correctAnswers[Level] += 1;
                IntervalTimer.Interval = TimeSpan.FromSeconds(CorrectInterval);
                if (IfAudioFeedBack) PlayWav(CorrectSoundPath);
                if (IfVisualFeedBack) ShowFeedbackImage(CorrectImage, CorrectInterval * 1000);
            }

            if (IfChecked)//这整道题做完了才需要执行下面的逻辑
            {
                ResultCheck();
                //看看是否需要调整游戏难度
                AdjustDifficulty();
                //接下来等计时器触发了才能开始下一句游戏，同时在倒计时期间不能触发任何组件
                PlayTime = BaseTimeLimit + (Level - MinLevel) * TimeLimitInterval;//计算好时间限制
                PlayTimer.Stop();//作答时间限制计时器停止
                OverLayGrid.IsEnabled = false;//用一个蒙层覆盖掉，用来屏蔽组件
                IntervalTimer.Start();
            }
            else
            {//说明只是需要单独一个数字的判断反馈
                if (IsNumFeedBack)
                {
                    if (IfAudioFeedBack) PlayWav(CorrectSoundPath);
                    if (IfVisualFeedBack) ShowFeedbackImage(CorrectImage, StopDurations);
                }
                else
                {
                    if (IfAudioFeedBack) PlayWav(ErrorSoundPath);
                    if (IfVisualFeedBack) ShowFeedbackImage(ErrorImage, StopDurations);
                }
            }

        }

        int max_hardness = 1;
        private void AdjustDifficulty()//模式1根据level值来调整对应的参数
        {
            if (IfLevelUp && Level <= MaxLevel)//如果足够升级等级
            {
                if (Level < MaxLevel) { Level += 1; }
                max_hardness = Math.Max(max_hardness, Level);
                IfLevelUp = false;
                ResultQueue.Clear();
                ResultInit();//每次升降等级都得清空重新计算
            }
            else if (IfLevelDown && Level >= MinLevel)
            {
                if (Level > MinLevel) { Level -= 1; }
                IfLevelDown = false; ResultQueue.Clear(); ResultInit();
            }
            LevelCheck();
            LevelStatisticsAction?.Invoke(Level, MaxLevel);
        }

        private void LevelCheck()//观察是否难度等级发生变化，如果有，则同步到游戏参数变化上
        {
            switch (Level)
            {
                case 1:
                    maxnumber = 5;
                    miss_number = 1;
                    mode1_display_size = 1;
                    break;
                case 2:
                    maxnumber = 7;
                    miss_number = 2;
                    mode1_display_size = 1;
                    break;
                case 3:
                    maxnumber = 8;
                    miss_number = 2;
                    mode1_display_size = 1; break;
                case 4:
                    maxnumber = 9;
                    miss_number = 3;
                    mode1_display_size = 1; break;
                case 5:
                    maxnumber = 10;
                    miss_number = 3;
                    mode1_display_size = 1; break;
                case 6:
                    maxnumber = 12;
                    miss_number = 3;
                    mode1_display_size = 2; break;
                case 7:
                    maxnumber = 14;
                    miss_number = 3;
                    mode1_display_size = 2; break;
                case 8:
                    maxnumber = 16;
                    miss_number = 4;
                    mode1_display_size = 2; break;
                case 9:
                    maxnumber = 18;
                    miss_number = 4;
                    mode1_display_size = 2; break;
                case 10:
                    maxnumber = 20;
                    miss_number = 4;
                    mode1_display_size = 3; break;
                case 11:
                    maxnumber = 24;
                    miss_number = 5;
                    mode1_display_size = 3; break;
                case 12:
                    maxnumber = 28;
                    miss_number = 5;
                    mode1_display_size = 3; break;
                case 13:
                    maxnumber = 30;
                    miss_number = 5;
                    mode1_display_size = 4; break;
                case 14:
                    maxnumber = 35;
                    miss_number = 5;
                    mode1_display_size = 4; break;
                case 15:
                    maxnumber = 38;
                    miss_number = 6;
                    mode1_display_size = 4; break;
                case 16:
                    maxnumber = 40;
                    miss_number = 6;
                    mode1_display_size = 4; break;
                case 17:
                    maxnumber = 45;
                    miss_number = 7;
                    mode1_display_size = 4; break;
                case 18:
                    maxnumber = 50;
                    miss_number = 8;
                    mode1_display_size = 4; break;
                default:
                    maxnumber = 5;
                    miss_number = 1;
                    mode1_display_size = 1; break;
            }
        }

        private void ResultCheck(bool isCorrect)//判断本题的结果需不需要记录到队列里面等等
        {//判断累计答对答错多少
            ResultQueue.Enqueue(isCorrect);//入队
            if (ResultQueue.Count > Math.Max(LevelDown, LevelUp)) { ResultQueue.Dequeue(); }              //因为入了队，所以需要先把最久远的出了
            if (ResultQueue.Count >= Math.Min(LevelDown, LevelUp))
            {//说明做了一定数目的题了，需要检查一下需不需要升降等级了
                int ErrorNums = 0;
                int CorrectNums = 0;
                //不知道LevelDown和LevelUp的大小关系，需要分类讨论
                if (LevelUp - LevelDown > 0)
                {//说明答的题目数量已经达到LevelDown了，需要判断需不需要LevelDown
                    ErrorNums = ResultQueue.Take(LevelDown).Count(x => !x);
                    if (ResultQueue.Count >= LevelUp)
                    {
                        CorrectNums = ResultQueue.Take(LevelUp).Count(x => x);
                    }
                }
                else if (LevelDown - LevelUp > 0)
                {//说明答的题目数量已经达到LevelUp了，需要判断需不需要LevelUp
                    CorrectNums = ResultQueue.Take(LevelUp).Count(x => x);
                    if (ResultQueue.Count >= LevelDown)
                    {
                        ErrorNums = ResultQueue.Take(LevelDown).Count(x => !x);
                    }
                }
                else
                {//说明LevelDown=LevelUp
                    ErrorNums = ResultQueue.Take(LevelDown).Count(x => !x);
                    CorrectNums = ResultQueue.Take(LevelUp).Count(x => x);
                }
                if (CorrectNums >= LevelUp) { IfLevelUp = true; }
                if (ErrorNums >= LevelDown) { IfLevelDown = true; }
            }
        }

        private void ResultCheck()//判断累计做对几道题，需不需要升降等级
        {
            if (success_time >= LevelUp) { IfLevelUp = true; }
            if (fail_time >= LevelDown) { IfLevelDown = true; }
        }

        private void ResultInit()//每次升降难度后都得把临时记录的结果置零
        {
            success_time = 0;
            fail_time = 0;
            ResultQueue.Clear();
        }

        private void PlayTimer_Tick(object sender, EventArgs e)//作答时间限制
        {
            //将PlayTime显示上去
            TimeStatisticsAction.Invoke((int)timeRemaining.TotalSeconds, PlayTime);
            PlayTime--;
            if (PlayTime <= 0)//时间已到
            {
                //强制算错
                fail_time++;
                wrongAnswers[Level] += 1;
                IntervalTimer.Interval = TimeSpan.FromSeconds(ErrorInterval);
                if (IfAudioFeedBack) PlayWav(ErrorSoundPath);
                if (IfVisualFeedBack) ShowFeedbackImage(ErrorImage, ErrorInterval * 1000);

                int ignoreCount = missingNumbers.Count(number => !userInputNumbers.Contains(number));
                ignoreAnswers[Level] += ignoreCount;

                is_finish = false;

                ResultCheck();//看看是否需要调整游戏难度
                AdjustDifficulty();
                //接下来等计时器触发了才能开始下一句游戏，同时在倒计时期间不能触发任何组件
                PlayTime = BaseTimeLimit + (Level - MinLevel) * TimeLimitInterval;//计算好时间限制
                OverLayGrid.IsEnabled = false;//用一个蒙层覆盖掉，用来屏蔽组件
                IntervalTimer.Start();

                PlayTimer.Stop();
            }
        }

        private void IntervalTimer_Tick(object sender, EventArgs e)//答题间隔计时器
        {// 这个触发了才能进入到下一个题目
            // 重新初始化游戏内容
            InitializeGame(); // 再次重新开始游戏
            // 重置用户输入
            userInputNumbers.Clear();
            userInput = string.Empty;
            UpdateTextBlock();

            OverLayGrid.IsEnabled = true;
            if (IfLimitTime)
            {
                PlayTime = BaseTimeLimit + (Level - MinLevel) * TimeLimitInterval;//计算好时间限制
                PlayTimer.Start();//开始作答限制计时器
            }
            IntervalTimer.Stop();//停止计时器
        }



        /*LJN
    添加进来视觉、声音反馈的资源
    */
        private SoundPlayer soundPlayer; // 用来放歌
        public string ErrorSoundPath;//错误的声音文件路径，在OnStartAsync()中配置
        public string CorrectSoundPath;//正确的声音文件路径，在OnStartAsync()中配置
        private int StopDurations = 1000; // 停止时间，ms

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

        private async void ShowFeedbackImage(Image image, int StopDurations)//StopDurations单位是ms
        {//显示反馈的图片
            image.Visibility = Visibility.Visible;

            // 延迟指定的时间（例如1秒）
            await Task.Delay(StopDurations);

            image.Visibility = Visibility.Collapsed;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)//绑定自定义鼠标光标和默认鼠标光标
        {
            Point position = e.GetPosition(this);
            Canvas.SetLeft(CustomCursor, position.X);
            Canvas.SetTop(CustomCursor, position.Y);
            // 获取 Canvas 的边界
            double canvasLeft = Canvas.GetLeft(CustomCursor);
            double canvasTop = Canvas.GetTop(CustomCursor);
            double canvasWidth = this.ActualWidth;
            double canvasHeight = this.ActualHeight;

            // 获取 CustomCursor 的宽高
            double cursorWidth = CustomCursor.Width;
            double cursorHeight = CustomCursor.Height;

            // 如果 CustomCursor 超出 Canvas 的边界，进行裁剪
            if (canvasLeft + cursorWidth > canvasWidth)
            {
                Canvas.SetLeft(CustomCursor, canvasWidth - cursorWidth); // 限制在右边界
            }
            if (canvasTop + cursorHeight > canvasHeight)
            {
                Canvas.SetTop(CustomCursor, canvasHeight - cursorHeight); // 限制在下边界
            }
            if (canvasLeft < 0)
            {
                Canvas.SetLeft(CustomCursor, 0); // 限制在左边界
            }
            if (canvasTop < 0)
            {
                Canvas.SetTop(CustomCursor, 0); // 限制在上边界
            }

            // 如果 CustomCursor 的位置超出 Canvas 范围，裁剪掉超出的部分
            Rect clipRect = new Rect(0, 0, canvasWidth, canvasHeight);
            CustomCursor.Clip = new RectangleGeometry(clipRect);  // 使用矩形裁剪区域
        }
    }
    public partial class 搜索能力1 : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
            /*LJN
            配置反馈资源的路径             
             */
            CorrectSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");
            // 为 Image 控件加载图片 Source
            CorrectImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Correct.png"), UriKind.RelativeOrAbsolute));
            ErrorImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Error.png"), UriKind.RelativeOrAbsolute));

            //初始化结果数组
            correctAnswers = new int[MaxLevel + 1];
            wrongAnswers = new int[MaxLevel + 1];
            ignoreAnswers = new int[MaxLevel + 1];
            IfLevelDown = false; IfLevelUp = false;

            max_time = 1; // 窗口总的持续时间，单位分钟
            train_mode = 1; // 游戏模式，为1
            Level = 1; // 当前游戏难度等级

            ResultQueue.Clear();//清空结果
            repeat_time = 0;
            ErrorCount = ErrorLimit;
            {
                // 参数（包含模块参数信息）
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
                            switch (par.ModuleParId)
                            {
                                case 292: // 治疗时间
                                    max_time = par.Value.HasValue ? (int)par.Value.Value : 1;
                                    Debug.WriteLine($"max_time={max_time}");
                                    break;
                                case 297://听觉反馈
                                    IfAudioFeedBack = par.Value == 1;
                                    break;
                                case 298://视觉反馈
                                    IfVisualFeedBack = par.Value == 1;
                                    break;
                                case 293://等级提高
                                    LevelUp = par.Value.HasValue ? (int)par.Value.Value : 5;
                                    break;
                                case 294://等级降低
                                    LevelDown = par.Value.HasValue ? (int)par.Value.Value : 5;
                                    break;
                                case 296://是否限制作答时间
                                    IfLimitTime = par.Value == 1;
                                    break;
                                case 295://预设的等级
                                    Level = par.Value.HasValue ? (int)par.Value.Value : 1;
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


            }

            max_hardness = Math.Max(max_hardness, Level);
            // 调用委托
            LevelStatisticsAction?.Invoke(Level, MaxLevel);
            RightStatisticsAction?.Invoke(0, LevelUp);
            WrongStatisticsAction?.Invoke(0, LevelDown);

            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1); // 每秒触发一次
            gameTimer.Tick += GameTimer_Tick;

            timeRemaining = TimeSpan.FromMinutes(max_time); // 设定整个窗口存在的时间

            if (IfLimitTime)
            {
                PlayTime = BaseTimeLimit + (Level - MinLevel) * TimeLimitInterval;//计算好时间限制
                PlayTimer = new DispatcherTimer();
                PlayTimer.Interval = TimeSpan.FromSeconds(1);//设置1秒后触发一次
                PlayTimer.Tick += PlayTimer_Tick;
            }

            IntervalTimer = new DispatcherTimer();
            IntervalTimer.Tick += IntervalTimer_Tick;
        }

        protected override async Task OnStartAsync()
        {
            gameTimer.Start(); // 开始计时

            ResetGameState(); // 重置游戏状态
            InitializeGame(); // 开始新游戏//其实就是更新状态
            // 调用委托
            SynopsisAction?.Invoke("一共有四种游戏模式，其规则分别如下：\r\n 模式一：找出数字范围内的缺失数字，并将它们从小到大逐个输入。\r\n模式二：识别出叠加在一起的不同形状，并将它们从屏幕下部选择出来\r\n模式三：找出屏幕下部的对象在图片中的位置并选择出来。\r\n模式四：数出并输入每个正确对象在图片中出现的次数。");
        }

        protected override async Task OnStopAsync()
        {
            gameTimer.Stop(); // 停止计时器
            if (IfLimitTime) { PlayTimer.Stop(); }
            IntervalTimer.Stop();
        }

        protected override async Task OnPauseAsync()
        {
            gameTimer.Stop(); // 停止计时器
            PlayTimer.Stop();
            IntervalTimer.Stop();
        }

        protected override async Task OnNextAsync()
        {
            // 调整难度
            InitializeGame(); // 开始新游戏 EndGame();//其实就是更新状态
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new 搜索能力1讲解();
        }


        // 插入写法
        private int GetCorrectNum(int difficultyLevel)
        {
            return correctAnswers[difficultyLevel];
        }
        private int GetWrongNum(int difficultyLevel)
        {
            return wrongAnswers[difficultyLevel];
        }
        private int GetIgnoreNum(int difficultyLevel)
        {
            return ignoreAnswers[difficultyLevel];
        }
        private double CalculateAccuracy(int correctCount, int wrongCount, int ignoreCount)
        {
            int totalAnswers = correctCount + wrongCount + ignoreCount;
            return totalAnswers > 0 ? Math.Round((double)correctCount / totalAnswers, 2) : 0;
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

                        for (int lv = 1; lv <= max_hardness; lv++)
                        {
                            // 获取当前难度级别的数据
                            correctCount += GetCorrectNum(lv);
                            wrongCount += GetWrongNum(lv);


                        }
                        int mode = train_mode;
                        int rep = repeat_time;
                        int totalCount = wrongCount * (rep + 1);
                        int Count = totalCount + correctCount;
                        double accuracy = Math.Round((double)correctCount / (double)Count, 2);

                        // 创建 Result 记录
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "搜索能力1",
                            Eval = false,
                            Lv = max_hardness, // 当前的难度级别
                            ScheduleId = BaseParameter.ScheduleId ?? null// 假设的 Schedule_id，可以替换为实际值
                        };
                        db.Results.Add(newResult);
                        await db.SaveChangesAsync(); //这里注释了
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
                                    ModuleId = BaseParameter.ModuleId
                                },
                                 new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "正确率",
                                    Value = accuracy * 100, // 以百分比形式存储
                                    ModuleId =  BaseParameter.ModuleId
                                },
                                //  new ResultDetail
                                //{
                                //    ResultId = result_id,
                                //    Order = 1,
                                //    ValueName = "总机会数",
                                //    Value = totalCount,
                                //    ModuleId = BaseParameter.ModuleId
                                //},
                                   new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "已使用机会数",
                                    Value = Count,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "正确次数",
                                    Value = correctCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "错误次数",
                                    Value = wrongCount,
                                    ModuleId =  BaseParameter.ModuleId
                                }
                            };
                        // 插入 ResultDetail 数据
                        db.ResultDetails.AddRange(resultDetails);
                        await db.SaveChangesAsync();
                        // 输出每个 ResultDetail 对象的数据
                        Debug.WriteLine($"难度级别 {max_hardness}:");
                        foreach (var detail in resultDetails)
                        {
                            Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId} ");
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
