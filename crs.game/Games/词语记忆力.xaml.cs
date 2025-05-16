using crs.core.DbModels;
using crs.core;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Runtime.Serialization;


namespace crs.game.Games
{
    /// <summary>
    /// 词语记忆力.xaml 的交互逻辑
    /// </summary>
    public partial class 词语记忆力 : BaseUserControl
    {
        /*
         * 存放游戏参数，需要coder修改，或从数据库读写
         */
        public int MemoryWordNum = 10;//要素词汇，默认5个
        public int TrainingMode = 3;//词汇难度，1简单2中等3困难4混合，混合词汇指难度等级表中的词汇难度混合出现，简单/中等/困难词汇指难度等级表中的所有词汇难度都为对应的简单/中等/困难
        public int RunDirection = 0;//词汇运动方向，1左0右
        public int RunSpeed = 1;//速度值，1-6，越大越快
        public int TreatDurations = 1;//训练时间，单位分钟
        public int IfVisionFeedBack = 1;//视觉反馈，1有0没有
        public int IfAudioFeedBack = 1;//声音反馈，1有0没有
        public int IfTextFeedBack = 0;//文字反馈，1有0没有，20241206文字反馈结合在图片反馈的png里了，不弄了
        public int SelectedDifficulty = 1;//所选择的难度等级，默认为1
        public int MemorizeTimeLimit = 1;//记忆阶段的时间限制，1限制为若干分钟，0没有，默认为1
        public int LevelUp = 3;//答对3组题才能提高等级
        public int LevelDown = 3;//答错3组题才会降低等级
        int max_hardness = 0;

        /*
         游戏逻辑处理相关函数
         */
        private void CheckIfSet()
        {//检查一下是否选中了训练设置，如果没有勾选，
            //则那些什么单词数量，单词类型都是根据难度等级列表和选中的难度等级来确定
            if (IfSet == 0)
            {//需求变更，只需要修改速度值

                //MemoryWordNum = (int)LevelTableDifficultyLevel.Rows[SelectedDifficulty - 1][1];
                //TrainingMode = (int)LevelTableDifficultyLevel.Rows[SelectedDifficulty - 1][2];
                //AllowedError = (int)LevelTableDifficultyLevel.Rows[SelectedDifficulty - 1][3];

                RunSpeed = 5;//速度值默认为5
            }
        }

        private void ShowWordsToMemorize()
        {//把需要记忆的词语显示在WordsToMemorizeTextBLock中
            string RelativePath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Words", $"{GetWordLevel(TrainingMode)}.txt");
            WordsToMemorizeList = LoadPartWords(RelativePath, MemoryWordNum);
            WordsToMemorizeTextBLock.Text = string.Join(" ", WordsToMemorizeList);
            WordsToMemorizeTextBLock.FontSize = FontSizeDict[(int)((LevelTableDifficultyLevel.Rows[SelectedDifficulty - 1])["单词数量"])];
            AllWordList = RemoveCommonElements(LoadAllWords(RelativePath), WordsToMemorizeList);
            MemorizeTipTextBlock.Visibility = Visibility.Visible;//提示可见
            GetWordsReady();//把要展示的词也准备好
            // 初始化记忆阶段的计时器
            MemorizeSeconds = MemorizeSecondsLimt;//因为是倒计时，所以需要一个初始值
            MemorizeTimer = new DispatcherTimer();
            MemorizeTimer.Interval = TimeSpan.FromSeconds(1); // 以秒为单位
            MemorizeTimer.Tick += MemorizeTimer_Tick;
            MemorizeTimer.Start();
        }

        private void GetWordsReady()
        {//用来将所需要显示出去的这组词给准备好，通过List<string> AllWordList,List<string> WordsToMemorizeList这两个列表来整理出来
            //在AllWordList中随机抽(WordsGroupNums*MemoryWordNum-MemoryWordNum)个
            List<string> OtherWords = GetRandomElements(AllWordList, WordsGroupNums * MemoryWordNum - MemoryWordNum);
            //最后把OtherWords和WordsToMemorizeList合并并打乱
            Random random = new Random();
            WordsToShow = ((OtherWords.Concat(WordsToMemorizeList).ToList()).OrderBy(x => random.Next())).ToList();
        }

        private void PositionRectangle(int direction)
        {//显示红色矩形(答题区的位置)
            if (direction == 1)
            {
                Canvas.SetLeft(TargetArea, 40); // 将Rectangle移到左侧
            }
            else
            {
                double canvasWidth = WordArea.ActualWidth;
                Canvas.SetLeft(TargetArea, canvasWidth - TargetArea.Width - 40); // 将Rectangle移到右侧
            }
            TargetArea.Visibility = Visibility.Visible;
        }

        private void CreateTextBlocksOffScreen()
        {//把几个TextBlock对象先创建出来并调整好参数，包括初始化
            double canvasHeight = WordArea.ActualHeight;
            double canvasWidth = WordArea.ActualWidth;

            // Fixed width and height for each TextBlock
            double textBlockWidth = 200;
            int NumberOfTextBlocksSet = (WordsToShow.Count < NumberOfTextBlocks) ? WordsToShow.Count : NumberOfTextBlocks;//程序开始前检查textblock数量和词汇数量的大小关系并做处理
            for (int i = 0; i < NumberOfTextBlocksSet; i++)
            {
                TextBlock textBlock = new TextBlock
                {
                    //Text = GetRandomWord(AllWordList, WordsToMemorizeList),
                    Text = WordsToShow[0],//获取第一个元素
                    Background = Brushes.Transparent, // 设置背景透明
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Height = canvasHeight,
                    Width = textBlockWidth,
                    FontFamily = new FontFamily("Times New Roman"), // 设置字体
                    FontSize = 160,// 设置字体大小
                    Visibility = Visibility.Collapsed//一开始先隐藏掉
                };
                //如果在创建NumberOfTextBlocks个text block的时候WordsToShow没那么多，那此时就在WordsToShow中随机选取即可
                if (NumberOfTextBlocksSet > WordsToShow.Count)
                {
                    WordsToShow = GetRandomElements(WordsToShow, WordsToShow.Count);//相当于把WordsToShow打乱
                }
                else
                {
                    WordsToShow.RemoveAt(0);//为了计数，每次将textblock的text赋值后都要把WordsToShow减少一个
                }
                AdjustTextBlockSize(textBlock);

                // Add the TextBlock to Canvas
                WordArea.Children.Add(textBlock);

                // Calculate vertical center position for the TextBlock
                double textBlockHeight = textBlock.Height;
                double verticalCenterPosition = (canvasHeight - textBlockHeight) / 2;

                // Set initial position off-screen 
                textBlockWidth = textBlock.Width;//调整过了所以需要更新
                double initialLeftPosition = RunDirection == 1 ? canvasWidth : -textBlockWidth;
                Canvas.SetLeft(textBlock, initialLeftPosition);
                Canvas.SetTop(textBlock, verticalCenterPosition); // 设置垂直居中位置


                // 将 TextBlock 的检测状态初始化为 false
                TextBlockDetected[textBlock] = false;

                //添加到列表里
                CreatedTextBlocks.Add(textBlock);  // 添加到列表
            }
        }

        private void AnimateTextBlocks(int direction, double speed)
        {//根据速度值设置每个textblock的动画
            double canvasWidth = WordArea.ActualWidth;
            double textBlockWidth = 200;
            double durationInSeconds = 11 - speed; // Speed 1 (slowest) -> 10 seconds, Speed 10 (fastest) -> 1 second

            // Calculate delay per TextBlock to avoid them starting at the same time
            double delayInterval = durationInSeconds / NumberOfTextBlocks;

            for (int i = 0; i < WordArea.Children.Count; i++)
            {
                if (WordArea.Children[i] is TextBlock textBlock)
                {
                    double from = direction == 1 ? canvasWidth : -textBlockWidth;
                    double to = direction == 1 ? -textBlockWidth : canvasWidth;
                    textBlock.Visibility = Visibility.Collapsed;
                    StartTextBlockAnimation(textBlock, from, to, durationInSeconds, TimeSpan.FromSeconds(i * delayInterval));
                }
            }
        }

        private void StartTextBlockAnimation(TextBlock textBlock, double from, double to, double durationInSeconds, TimeSpan beginTime)
        {//对于每个textblock，设置并开始他们的动画
            if (AnimationCount >= WordsGroupNums * MemoryWordNum)
            {//动画数量如果大于该组题的素材数，则不需要再开始动画了
                AnimationCount = 0; return;
            }
            else
            {
                // Create and configure the animation
                DoubleAnimation animation = new DoubleAnimation
                {
                    From = from,
                    To = to,
                    Duration = new Duration(TimeSpan.FromSeconds(durationInSeconds)),
                    BeginTime = beginTime
                };

                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, textBlock);
                Storyboard.SetTargetProperty(animation, new PropertyPath(Canvas.LeftProperty));
                // 每次动画结束时都更新文本内容
                storyboard.Completed += (s, e) =>
                {
                    if (!IfNextGroup)
                    {//防止出现在一组题中素材已经显示完了，但是另外多余的动画在结束后重复触发此部分逻辑代码
                        WordsAll++; // 每次有动画结束则词汇数量++
                        WordsAllTemp++;

                        if (WordsToMemorizeList.Contains(textBlock.Text) && !TextBlockDetected[textBlock])
                        { // 更新内容之前先做判断看看是否它是需要记住的词，而且没有按键按下
                            WordsIgnore++;
                            WordsIgnoreTemp++;
                        }

                        if (WordsAllTemp >= WordsGroupNums * MemoryWordNum)
                        {//说明这组题的词要显示完了，该开始下一轮了
                            UpdateGroupResult();//首先根据这组提的做题情况来增加对应的答对题组数，错误题组数
                            AdjustDifficulty();//然后再根据题组数来选择升降难度并设置对应的参数
                            BeginNextGroup();//开启下一轮的显示游玩
                            ParameterSet();
                            return;
                        }
                        //textBlock.Text = GetRandomWord(AllWordList, WordsToMemorizeList);
                        textBlock.Text = WordsToShow.Count > 0 ? WordsToShow[0] : string.Empty;//获取第一个元素
                        if (WordsToShow.Count > 0) WordsToShow.RemoveAt(0);//为了计数，每次将textblock的text赋值后都要把WordsToShow减少一个
                        AdjustTextBlockSize(textBlock); // 动态调整 TextBlock 宽度
                        StartTextBlockAnimation(textBlock, from, to, durationInSeconds, TimeSpan.Zero); // 重启动画
                    }

                };

                // 在启动动画前重置检测状态
                TextBlockDetected[textBlock] = false;
                TextBlockAnimations.Add(storyboard); // 将动画添加到列表


                // 为 Canvas 设置剪裁区域，确保 TextBlock 在 Canvas 外部部分不可见
                ApplyCanvasClip(WordArea);
                // 动画进行期间，检查位置并更新可见性
                storyboard.CurrentTimeInvalidated += (s, e) =>
                {
                    UpdateTextBlockVisibility(textBlock, WordArea);
                };
                AnimationCount++;//动画计数
                textBlock.Visibility = Visibility.Visible;//真正开始动画了才显示出来
                storyboard.Begin();
            }
        }

        // 应用剪裁区域到 Canvas
        private void ApplyCanvasClip(Canvas containerCanvas)
        {//通过裁剪，实现textblock在这个canvas中部分可见部分不可见
            // 创建一个与 Canvas 相同大小的矩形
            RectangleGeometry clipGeometry = new RectangleGeometry
            {
                Rect = new Rect(0, 0, containerCanvas.ActualWidth, containerCanvas.ActualHeight)
            };

            // 将该矩形作为 Canvas 的剪裁区域
            containerCanvas.Clip = clipGeometry;
        }

        private void UpdateTextBlockVisibility(TextBlock textBlock, Canvas containerCanvas)
        {//对于运动中的textblock检查一下他们是否该出现
            // 获取 TextBlock 的当前左边距
            double left = Canvas.GetLeft(textBlock);
            double textBlockWidth = textBlock.ActualWidth;
            // 获取 Canvas 的宽度
            double canvasLeft = 0; // 假设 Canvas 的左边界为 0
            double canvasRight = containerCanvas.ActualWidth;
            // 判断 TextBlock 是否在 Canvas 的范围内
            if (left + textBlockWidth > canvasLeft && left < canvasRight)
            {
                // 在 Canvas 范围内，设置可见
                textBlock.Visibility = Visibility.Visible;
            }
            else
            {
                // 超出 Canvas 范围，设置隐藏
                textBlock.Visibility = Visibility.Hidden;
            }
        }

        private void AdjustTextBlockWidth(TextBlock textBlock)
        {//动态调整TextBlock的宽度
            // 使用 FormattedText 来测量文本宽度
            var formattedText = new FormattedText(
                textBlock.Text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
                textBlock.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                1);

            // 动态调整 TextBlock 的宽度
            textBlock.Width = formattedText.Width;
        }

        private void AdjustTextBlockSize(TextBlock textBlock)
        {
            // 使用 FormattedText 来测量文本尺寸
            var formattedText = new FormattedText(
                textBlock.Text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
                textBlock.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                1);

            // 动态调整 TextBlock 的宽度和高度
            textBlock.Width = formattedText.Width;
            textBlock.Height = formattedText.Height;
            textBlock.Visibility = Visibility.Collapsed;

        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {//倒计时开始运作并更新
            if (CountdownSeconds > 0)
            {
                CountdownSeconds--;
                TimeStatisticsAction?.Invoke((int)CountdownSeconds, 0);
                CountdownDisplay.Text = CountdownSeconds.ToString(); // 更新界面上的倒计时显示//不显示了需求变更
            }
            else
            {
                CountdownTimer.Stop();
                // 手动触发结束按钮点击事件
                RoutedEventArgs args = new RoutedEventArgs();
                EndClick(this, args);
            }
        }

        // Timer的tick事件处理程序
        private void MemorizeTimer_Tick(object sender, EventArgs e)
        {
            //TimeStatisticsAction?.Invoke(MemorizeSeconds, MemorizeSeconds);//显示到屏幕上
            MemorizeSeconds--;
            //是否有记忆阶段限制，如果有，大于某个时间时手动按下OK按钮
            if (MemorizeSeconds <= 0)
            {//超时了才开始判断
                if (MemorizeTimeLimit == 1)
                {//有时间限制
                    MemorizeOKButtonClick(this, new RoutedEventArgs());//手动点下OK按钮
                }
            }

        }

        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {// 按键检测
            // 检查按下的键是否是你指定的键
            if (e.Key == System.Windows.Input.Key.Enter) // 假设你指定的键是回车键
            {
                CheckIntersection();//看看是否有交集
            }
        }

        private async void CheckIntersection()
        {
            // 获取 Rectangle 的边界
            double rectLeft = Canvas.GetLeft(TargetArea);
            double rectTop = Canvas.GetTop(TargetArea);
            // 检查是否为 NaN 并给予默认值
            if (double.IsNaN(rectLeft)) rectLeft = 0;
            if (double.IsNaN(rectTop)) rectTop = 0;
            // 创建 Rectangle 边界
            Rect rectangleBounds = new Rect(rectLeft, rectTop, TargetArea.Width, TargetArea.Height);
            foreach (var child in WordArea.Children)
            {
                if (child is TextBlock textBlock)
                {
                    // 如果 TextBlock 尚未被检测到
                    if (!TextBlockDetected[textBlock])
                    {
                        // 获取 TextBlock 的边界
                        double textBlockLeft = Canvas.GetLeft(textBlock);
                        double textBlockTop = Canvas.GetTop(textBlock);
                        // 检查是否为 NaN 并给予默认值
                        if (double.IsNaN(textBlockLeft)) textBlockLeft = 0;
                        if (double.IsNaN(textBlockTop)) textBlockTop = 0;
                        // 创建 TextBlock 边界
                        Rect textBlockBounds = new Rect(textBlockLeft, textBlockTop, textBlock.Width, textBlock.ActualHeight);
                        // 检查是否有重叠
                        if (rectangleBounds.IntersectsWith(textBlockBounds))
                        {
                            bool isCorrect = WordsToMemorizeList.Contains(textBlock.Text);
                            // 进行判断并更新 _ViewModel 计数器
                            if (isCorrect)
                            {
                                WordsCorrect++; // 更新正确计数
                                WordsCorrectTemp++;
                                TargetArea.Stroke = new SolidColorBrush(Colors.Green);
                                if (IfAudioFeedBack == 1) // 是否放声音
                                {
                                    PlayWav(CorrectSound);
                                }
                                if (IfVisionFeedBack == 1)
                                {
                                    ShowFeedbackImage(CorrectImage);
                                }
                                if (IfTextFeedBack == 1)
                                {
                                    ShowFeedbackTextBlock(CorrectTextBlock); // 显示正确文本反馈
                                }
                            }
                            else
                            {
                                WordsError++; // 更新错误计数
                                WordsErrorTemp++;
                                TargetArea.Stroke = new SolidColorBrush(Colors.Red);
                                if (IfAudioFeedBack == 1) // 是否放声音
                                {
                                    PlayWav(ErrorSound);
                                }
                                if (IfVisionFeedBack == 1)
                                {
                                    ShowFeedbackImage(ErrorImage);
                                }
                                if (IfTextFeedBack == 1)
                                {
                                    ShowFeedbackTextBlock(ErrorTextBlock); // 显示正确文本反馈
                                }
                            }
                            // MessageBox.Show(textBlock.Text);
                            TextBlockDetected[textBlock] = true; // 更新检测状态

                            // 停止所有动画
                            foreach (var storyboard in TextBlockAnimations)
                            {
                                storyboard.Pause();
                            }
                            // 延迟 StopDurations 毫秒
                            await Task.Delay(StopDurations);
                            TargetArea.Stroke = new SolidColorBrush(Colors.Black);//恢复颜色
                            // 重新启动所有动画
                            foreach (var storyboard in TextBlockAnimations)
                            {
                                storyboard.Resume();
                            }

                            break; // 找到一个重叠的文本后退出循环
                        }
                    }
                }
            }
        }

        /*这里是我更新难度的地方！*/
        private void AdjustDifficulty()
        {//若干组题做完了，需要来看一下是否需要调整难度等级
            if (GroupCorrect >= LevelUp)
            {//如果做对的题目组数达到一定数量后就可以升等级
                if (SelectedDifficulty < MaxLevel) SelectedDifficulty += 1; GroupResultInit();
            }
            else if (GroupError >= LevelDown)
            {//错太多要降
                if (SelectedDifficulty > MinLevel) SelectedDifficulty -= 1; GroupResultInit();
            }
            else
            {
                return;//说明对的不够多，错的不够多，不需要升降，那就继续吧
            }
            ParameterSet();// 根据难度等级来设置参数
            max_hardness = Math.Max(max_hardness, SelectedDifficulty);
        }

        private void BeginNextGroup()
        {//写这个函数是为了在不改变参数的情况下，让它回到初始状态
            InitializeComponent();
            //最开始打开窗口看到的是记忆阶段的组件
            WordsToMemorizeTextBLock.Visibility = Visibility.Visible;
            MemorizeOKButton.Visibility = Visibility.Visible;//记忆阶段两个组件显示
            PlayGrid.Visibility = Visibility.Collapsed;
            ShowWordsToMemorize();//让需要记忆的词汇显示
            IfHaveStarted = false;
            StartButton.IsEnabled = true;
            // 停止所有动画
            foreach (var storyboard in TextBlockAnimations) { storyboard.Remove(); }//直接移除动画
            // 移除创建的 TextBlock 对象
            foreach (var textBlock in CreatedTextBlocks) { WordArea.Children.Remove(textBlock); }
            CreatedTextBlocks.Clear();  // 清空列表
            // 停止计时器
            if (MemorizeTimer != null && MemorizeTimer.IsEnabled)
            {
                MemorizeTimer.Stop();
                MemorizeTimer.Tick -= MemorizeTimer_Tick;
                MemorizeTimer = null;
                //此时MemorizeSeconds就是记忆的秒数
            }
            InitTempResults();//把一些计数量也重置
            IfNextGroup = true;
        }

        private void ParameterSet()
        {//这是在参数修改之后，手动同步到游戏设置上面，确保修改生效
            switch (TrainingMode)
            {// 只有选择了混合词汇才需要根据难度表来实时更改词汇难度，其余的情况不用，固定即可
                case 1: TrainingMode = 1; break;
                case 2: TrainingMode = 2; break;
                case 3: TrainingMode = 3; break;
                case 4: TrainingMode = (int)LevelTableDifficultyLevel.Rows[SelectedDifficulty - 1][2]; break;
                default: TrainingMode = (int)LevelTableDifficultyLevel.Rows[SelectedDifficulty - 1][2]; break;
            }
            //主要设置词汇难度，需要记忆的词汇数量
            MemoryWordNum = (int)LevelTableDifficultyLevel.Rows[SelectedDifficulty - 1][1];
            AllowedError = (int)LevelTableDifficultyLevel.Rows[SelectedDifficulty - 1][3];
            //RunSpeed = 5;//速度值默认为5
        }

        private void GroupResultInit()
        {//每次升降难度后，所记录的正确题组和错误题组数都要归零更新
            GroupCorrect = 0;
            GroupError = 0;
        }

        private void UpdateGroupResult()
        {//在一组题的所有素材词汇都飘过去了，就要看它的答对数量之类的来判断这组题有没有做对
            if (WordsIgnoreTemp > 0 || WordsErrorTemp - AllowedError > 0)
            {//有词汇是需要记住的但是漏了没选中，则直接算错误，或者，没有漏选的情况下，判断是否有选中要素词汇以外的词，如果有且次数超过了对应的限制，则算错
                GroupError++;
            }
            else
            {//上面两个判断已经能够分出是否正确的情况了
                GroupCorrect++;
            }
            InitTempResults();//对这些计数值进行清空
            UpdateDisplay();//每次变更就同步显示到UI上去
            // 并做好记录
            AllLevelResult[SelectedDifficulty] = AllLevelResult.GetValueOrDefault(SelectedDifficulty) ?? new Dictionary<string, int>();
            AllLevelResult[SelectedDifficulty]["正确组题数量"] = AllLevelResult[SelectedDifficulty].GetValueOrDefault("正确组题数量") + GroupCorrect;
            AllLevelResult[SelectedDifficulty]["错误组题数量"] = AllLevelResult[SelectedDifficulty].GetValueOrDefault("错误组题数量") + GroupError;
        }

        /*
        按钮触发类函数
        */

        private void MemorizeOKButtonClick(object sender, RoutedEventArgs e)
        {//点击记忆完成后，记忆阶段的组件就应该隐藏掉
            WordsToMemorizeTextBLock.Visibility = Visibility.Collapsed;
            MemorizeOKButton.Visibility = Visibility.Collapsed;//记忆阶段两个组件消失
            PlayGrid.Visibility = Visibility.Visible;//训练阶段的组件都归到这个grid中，让他显示
            MemorizeTipTextBlock.Visibility = Visibility.Collapsed;//提示不可见
            // 停止计时器
            if (MemorizeTimer != null && MemorizeTimer.IsEnabled)
            {
                MemorizeTimer.Stop();
                MemorizeTimer.Tick -= MemorizeTimer_Tick;
                MemorizeTimer = null;
                //此时MemorizeSeconds就是记忆的秒数
            }
            IfNextGroup = false;//设置标志位
            IfHaveStarted = false;
            StartButton.IsEnabled = true;
        }

        private void StartClick(object sender, RoutedEventArgs e)
        {
            if (IfHaveStarted == false)
            {
                CreateTextBlocksOffScreen();//创建出若干个textblock
                //需求变更(20241015)，强制方向为从左向右
                RunDirection = 0;//手动赋值为向右运动
                PositionRectangle(RunDirection);
                AnimateTextBlocks(RunDirection, (double)RunSpeed);

            }
            IfHaveStarted = true;
            StartButton.IsEnabled = false;
        }

        private void EndClick(object sender, RoutedEventArgs e)
        {
            if (RunMode == 1)
            {//说明是正式训练模式，需要打开report窗口
                //ReportWindow reportWindow = new ReportWindow(SettingTable, ResultTable);//顺便把两个table传过去显示
                OnGameEnd();
            }
            else
            {//说明只是训练模式，回到最初的样子就好
                return;
            }
        }
        /*
         存放临时变量，不需要coder修改
         */
        public int RunMode = 1;//以什么模式开始，1正式训练0只是练习，20241205为无效参数了，固定为1
        public int IfSet = 0;//是否勾选了训练设置，1有0没有，只有不勾选训练设置，(词汇难度、允许错几个等)参数才能根据难度等级列表来确定，20241205为无效参数了，固定为0

        public int WordsGroupNums = 7;//一组题有(7*要素词汇)个素材词汇这样来出现，比如，目标记忆素材3个，则传送带上出现的一组素材总量为3*7=21个，其中会出现3个目标记忆素材。

        private List<string> AllWordList = new List<string>();//从文件中读取到的除需要记忆的单词以外，所有的单词列表，用来动画显示
        private List<string> WordsToMemorizeList = new List<string>();//存放需要记住的单词列表
        private List<string> WordsToShow = new List<string>();//20241205需求变更，用于将需要显示出去的词来存到一个列表里面打乱后显示出来

        private string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory; // 当前项目的绝对路径
        private string ResourcesPath = System.IO.Path.Combine("Resources", "词语记忆力");

        private bool IfHaveStarted = false;//是否已经点击过开始按钮
        private bool IfNextGroup = false;//是否已经准备显示下一题组，设置这个标志位是确保进入到下一个题组了，动画结束后的逻辑判断中不会涉及到重新开始

        private int NumberOfTextBlocks = 3;//要创建的TextBlock的数量，默认三个
        private Dictionary<TextBlock, bool> TextBlockDetected = new Dictionary<TextBlock, bool>(); // 初始化检测状态字典; // 存储每个 TextBlock 的检测状态
        private List<TextBlock> CreatedTextBlocks = new List<TextBlock>();//用来存放创建的textblock对象

        private Random RandomObject = new Random();//随机对象
        private List<Storyboard> TextBlockAnimations = new List<Storyboard>(); // 列表存储所有动画

        private double CountdownSeconds = 0;//计时器的计时数
        private DispatcherTimer CountdownTimer;//计时器对象
        private DispatcherTimer MemorizeTimer;//记忆阶段的计时器对像
        private int MemorizeSecondsLimt = 1 * 60;//限制记忆用时用多久，单位s
        private int MemorizeSeconds = 0;//记忆阶段的用时

        private SoundPlayer soundPlayer; // 用来放歌

        public string ErrorSound;//错误的声音
        public string CorrectSound;//正确的声音
        private int StopDurations = 2000; // 停止时间，ms

        private DataTable LevelTableDifficultyLevel = new DataTable();//等级列表参数

        private int TreatNum = 0;//治疗编号，说明是从第几次开始的治疗训练
        private string DateNow = DateTime.Now.ToString("yyyy/M/d"); // 获取当前日期

        public int AllowedError = -1;//允许错误的单词数量，-1为不限制错误数量

        private int MaxLevel = 30;//最高难度等级
        private int MinLevel = 1;//最低难度等级

        private Dictionary<int, Dictionary<string, int>> AllLevelResult = new Dictionary<int, Dictionary<string, int>>();//一个字典，存放各个难度等级下的游戏结果，用难度等级的int来索引游戏结果，游戏结果以字典形式储存，有单词总数，正确数等等。。。
        private int WordsAllTemp = 0;//一组题的素材中飘过去的单词总数
        private int WordsIgnoreTemp = 0;//一组题的素材中飘过去的单词中，忽略了的单词数
        private int WordsCorrectTemp = 0;//一组题的素材中飘过去的单词中，正确的单词数
        private int WordsErrorTemp = 0;//一组题的素材中飘过去的单词中，错误的单词数

        //声明存放结果的数组
        private int[] correctAnswers;//先不说长度多少
        private int[] wrongAnswers;
        private int[] igonreAnswer;

        //这一堆是整个游戏过程中的数据
        private int WordsAll = 0;//飘过去的单词总数
        private int WordsIgnore = 0;//飘过去的单词中，忽略了的单词数
        private int WordsCorrect = 0;//飘过去的单词中，正确的单词数
        private int WordsError = 0;//飘过去的单词中，错误的单词数
        private int AnimationCount = 0;// 已经开始的动画个数，用来限制重复多余开始动画

        //这一堆是后期以一组一组题为单位后的游戏过程中的数据
        private int GroupAll = 0;//一共答多少组题
        private int GroupCorrect = 0;//一共答对多少组题
        private int GroupError = 0;//一共答错多少组题

        //不同词汇数量的需要记住的词汇的大小不一样
        private Dictionary<int, int> FontSizeDict = new Dictionary<int, int>
        {//{词汇数量，字体大小}
            {1,100},{2,100},{3,100},{4,100},{5,100},{6,98},{7,80},{8,70},{9,60},{10,55}
        };

        /*
         存放功能函数，不需要coder修改
         */

        private List<string> LoadAllWords(string FileName)
        {//把本地所有单词都加载出来
            Encoding encoding = Encoding.UTF8;
            List<string> words = new List<string>();

            // 首先读取文件中的所有行
            using (StreamReader file = new StreamReader(FileName))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    words.Add(line.Trim());
                }
            }
            return words;
        }

        private List<string> LoadPartWords(string filename, int count)
        {//从本地读取部分单词出来
            Encoding encoding = Encoding.UTF8;
            List<string> words = new List<string>();

            // 首先读取文件中的所有行
            List<string> allLines = new List<string>();
            using (StreamReader file = new StreamReader(filename))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    allLines.Add(line.Trim());
                }
            }

            // 使用当前时间作为种子
            Random random = new Random();
            int linesToRead = Math.Min(count, allLines.Count);

            // 随机选择不重复的行
            for (int i = 0; i < linesToRead; i++)
            {
                int index = random.Next(allLines.Count);
                words.Add(allLines[index]);
                allLines.RemoveAt(index); // 确保不会重复选中同一行
            }
            return words;
        }

        public List<string> RemoveCommonElements(List<string> A, List<string> B)
        {//A-B
            // 使用 Except 方法获取 A 中不存在于 B 的元素
            List<string> result = A.Except(B).ToList();
            return result;
        }

        private List<string> GetRandomElements(List<string> OriginList, int Count)
        {//随机打乱列表，并取前Count个，以实现随机选取的目的
            Random random = new Random();
            return OriginList.OrderBy(x => random.Next()).Take(Count).ToList();
        }

        static private string GetWordLevel(int TrainingMode = 0)
        {//根据TrainingMode的选择返回词汇的难度字符串
            switch (TrainingMode)
            {
                case 1: return "Easy";
                case 2: return "Medium";
                case 3: return "Hard";
                case 4: return "Hybrid";
                default: return "Easy";
            }
        }

        private string GetRandomWord(List<string> AllWordList, List<string> WordsToMemorizeList)
        {//从两个列表中随机抽取出来，随机从AllWordList和WordsToMemorizeList两个list中随机挑
            // 确保两个列表都已经加载
            if ((AllWordList == null || AllWordList.Count == 0) && (WordsToMemorizeList == null || WordsToMemorizeList.Count == 0))
            {
                return "No Words Loaded";
            }
            // 随机选择列表，0 表示 WordList，1 表示 _ViewModel.WordsToMemorizeList
            int listSelector = RandomObject.Next(0, 2);
            List<string> selectedList;
            if (listSelector == 0 && AllWordList != null && AllWordList.Count > 0)
            {
                selectedList = AllWordList;
            }
            else if (WordsToMemorizeList != null && WordsToMemorizeList.Count > 0)
            {
                selectedList = WordsToMemorizeList;
            }
            else if (AllWordList != null && AllWordList.Count > 0)
            {
                // 如果上面都不符合条件，那就使用剩下的非空列表
                selectedList = AllWordList;
            }
            else
            {
                return "No Words Loaded";
            }

            // 从选择的列表中随机选取一个元素
            int index = RandomObject.Next(selectedList.Count);
            return selectedList[index];
        }

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

        private async void ShowFeedbackImage(Image image)
        {//显示反馈的图片
            image.Visibility = Visibility.Visible;

            // 延迟指定的时间（例如2秒）
            await Task.Delay(StopDurations);

            image.Visibility = Visibility.Collapsed;
        }

        private async void ShowFeedbackTextBlock(TextBlock textBlock)
        {
            textBlock.Visibility = Visibility.Visible;

            // 延迟指定的时间（例如2秒）
            await Task.Delay(StopDurations);

            textBlock.Visibility = Visibility.Collapsed;
        }

        public DataTable DifficultyLevelInit()
        {//把等级列表构造出来
            DataTable LevelTable = new DataTable();

            LevelTable.Columns.Add("等级", typeof(int));
            LevelTable.Columns.Add("单词数量", typeof(int));
            LevelTable.Columns.Add("单词类型", typeof(int));
            LevelTable.Columns.Add("允许错误数量", typeof(int));

            LevelTable.Rows.Add(1, 1, 0, 0);
            LevelTable.Rows.Add(2, 1, 1, 0);
            LevelTable.Rows.Add(3, 1, 2, 0);
            LevelTable.Rows.Add(4, 2, 0, 0);
            LevelTable.Rows.Add(5, 2, 1, 0);
            LevelTable.Rows.Add(6, 2, 2, 0);
            LevelTable.Rows.Add(7, 3, 0, 0);
            LevelTable.Rows.Add(8, 3, 1, 0);
            LevelTable.Rows.Add(9, 3, 2, 0);
            LevelTable.Rows.Add(10, 4, 0, 0);
            LevelTable.Rows.Add(11, 4, 1, 0);
            LevelTable.Rows.Add(12, 4, 2, 0);
            LevelTable.Rows.Add(13, 5, 0, 1);
            LevelTable.Rows.Add(14, 5, 1, 1);
            LevelTable.Rows.Add(15, 5, 2, 1);
            LevelTable.Rows.Add(16, 6, 0, 1);
            LevelTable.Rows.Add(17, 6, 1, 1);
            LevelTable.Rows.Add(18, 6, 2, 1);
            LevelTable.Rows.Add(19, 7, 2, 1);
            LevelTable.Rows.Add(20, 7, 1, 1);
            LevelTable.Rows.Add(21, 7, 2, 1);
            LevelTable.Rows.Add(22, 8, 0, 1);
            LevelTable.Rows.Add(23, 8, 1, 1);
            LevelTable.Rows.Add(24, 8, 2, 1);
            LevelTable.Rows.Add(25, 9, 0, 2);
            LevelTable.Rows.Add(26, 9, 1, 2);
            LevelTable.Rows.Add(27, 9, 2, 2);
            LevelTable.Rows.Add(28, 10, 0, 2);
            LevelTable.Rows.Add(29, 10, 1, 2);
            LevelTable.Rows.Add(30, 10, 2, 2);
            return LevelTable;
        }

        public string TrainingModeToString(int TrainingMode)
        {//把TrainingMode转换成字符串输入，方便填到datatable里面
            switch (TrainingMode)
            {
                case 0: return "简单词汇";
                case 1: return "中等词汇";
                case 2: return "困难词汇";
                case 3: return "混合词汇";
                default: return "简单词汇";
            }
        }

        private void UpdateDisplay()
        {//将答对答错的题组数正确显示到UI上
            RightStatisticsAction?.Invoke(GroupCorrect, LevelUp);
            WrongStatisticsAction?.Invoke(GroupError, LevelDown);
            LevelStatisticsAction?.Invoke(SelectedDifficulty, MaxLevel);
        }

        private void InitTempResults()
        {//在切换难度等级后一些量要重置
            WordsAllTemp = 0;//一组题的素材中飘过去的单词总数
            WordsIgnoreTemp = 0;//一组题的素材中飘过去的单词中，忽略了的单词数
            WordsCorrectTemp = 0;//一组题的素材中飘过去的单词中，正确的单词数
            WordsErrorTemp = 0;//一组题的素材中飘过去的单词中，错误的单词数
        }

        private void AllLevelResultToArray()
        {
            int Length = AllLevelResult.Count;//这个是指一共玩了多少个等级
            correctAnswers = new int[Length];
            wrongAnswers = new int[Length];
            igonreAnswer = new int[Length];

            for (int i = 0; i < Length; i++)//遍历所有难度等级
            {//i实际上就是难度等级-1
                Dictionary<string, int> LevelResult = AllLevelResult[i + 1];
                correctAnswers[i] = LevelResult["正确组题数量"];
                wrongAnswers[i] = LevelResult["错误组题数量"];
                //igonreAnswer[i] = LevelResult["忽略单词"];
                igonreAnswer[i] = 0;//这一项是没有统计过的
            }
        }

    }

    public partial class 词语记忆力 : BaseUserControl
    {
        public 词语记忆力()
        {
            InitializeComponent();
        }

        protected override async Task OnInitAsync()
        {
            InitializeComponent();
            ////最开始打开窗口看到的是记忆阶段的组件
            WordsToMemorizeTextBLock.Visibility = Visibility.Visible;
            MemorizeOKButton.Visibility = Visibility.Visible;//记忆阶段两个组件显示
            PlayGrid.Visibility = Visibility.Collapsed;
            LevelTableDifficultyLevel = DifficultyLevelInit();


            CorrectSound = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSound = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");
            // 为 Image 控件加载图片 Source
            CorrectImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Correct.png"), UriKind.RelativeOrAbsolute));
            ErrorImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Error.png"), UriKind.RelativeOrAbsolute));
            bool Left = false;//词汇向左运动，临时变量，用来和数据库交互，不用修改
            bool Right = false;

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
                            case 70: // 治疗时间 
                                TreatDurations = par.Value.HasValue ? (int)par.Value.Value : 60;
                                Debug.WriteLine($"TreatDurations={TreatDurations}");
                                break;
                            //case 71: // 要素词汇
                            //    MemoryWordNum = par.Value.HasValue ? (int)par.Value.Value : 10;
                            //    Debug.WriteLine($"MemoryWordNum={MemoryWordNum}");
                            //    break;
                            case 315://词汇类型
                                TrainingMode = par.Value.HasValue ? (int)par.Value.Value : 1;
                                break;
                            case 76://听觉反馈
                                IfAudioFeedBack = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"IfAudioFeedBack ={IfAudioFeedBack}");
                                break;
                            case 77://视觉反馈
                                IfVisionFeedBack = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"IfVisionFeedBack ={IfVisionFeedBack}");
                                break;
                            case 247://单词运动速度
                                RunSpeed = par.Value.HasValue ? (int)par.Value.Value : 5;
                                Debug.WriteLine($"RunSpeed ={RunSpeed}");
                                break;
                            case 158://难度等级
                                SelectedDifficulty = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"SelectedDifficulty ={SelectedDifficulty}");
                                break;
                            case 276://是否限制记忆时间
                                MemorizeTimeLimit = par.Value.HasValue ? (int)par.Value.Value : 1;
                                break;
                            case 277:// 等级提高
                                LevelUp = par.Value.HasValue ? (int)par.Value.Value : 3;
                                break;
                            case 278:// 等级降低
                                LevelDown = par.Value.HasValue ? (int)par.Value.Value : 3;
                                break;
                            // 添加其他需要处理的 ModuleParId
                            default:
                                Debug.WriteLine($"未处理的 ModuleParId: {par.ModuleParId}");
                                break;
                        }

                        //整理一下临时变量
                        if ((bool)Left == true && (bool)Right == false) { RunDirection = 1; }//词汇向左运动
                        else if ((bool)Left == true && (bool)Right == false) { RunDirection = 0; }
                        else { RunDirection = 0; }//强制向右运动，保证默认值存在
                        MemoryWordNum = (int)LevelTableDifficultyLevel.Rows[SelectedDifficulty - 1][1];
                        AllowedError = (int)LevelTableDifficultyLevel.Rows[SelectedDifficulty - 1][3];
                    }
                }
            }
            else
            {
                Debug.WriteLine("没有数据");
            }
        }

        protected override async Task OnStartAsync()
        {
            // 重新启动所有动画
            if (TextBlockAnimations != null)
            {
                foreach (var storyboard in TextBlockAnimations)
                {
                    storyboard.Resume();
                }
            }

            WordsToMemorizeTextBLock.Visibility = Visibility.Visible;
            MemorizeOKButton.Visibility = Visibility.Visible;//记忆阶段两个组件显示
            PlayGrid.Visibility = Visibility.Collapsed;
            CheckIfSet();//在显示需要记忆的单词之前就得检查一下参数设置
            ShowWordsToMemorize();//让需要记忆的词汇显示

            if (CountdownTimer != null)
            {// 停止并释放之前的计时器（如果有的话）
                CountdownTimer.Stop();
                CountdownTimer.Tick -= CountdownTimer_Tick;
                CountdownTimer = null;
            }
            // 设置倒计时并启动计时器
            CountdownSeconds = TreatDurations * 60; // 设置倒计时时间
            CountdownTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1) // 计时器每秒触发一次
            };
            CountdownTimer.Tick += CountdownTimer_Tick;
            CountdownDisplay.Text = CountdownSeconds.ToString();//20241206需求变更不显示了
            CountdownTimer.Start();

            // 调用委托
            VoiceTipAction?.Invoke("请从屏幕出现的词语中找出重复的词语。");
            SynopsisAction?.Invoke("请记住记忆阶段屏幕上所出现的词语，记忆完成后，按下键盘上OK键。随后在屏幕上会有一系列词语从左往右移动，当您看到与记忆阶段所记住的词语相匹配时并移动至方框内时，请按下键盘上的OK键。");
            RuleAction?.Invoke("请记住记忆阶段屏幕上所出现的词语，记忆完成后，按下键盘上OK键。随后在屏幕上会有一系列词语从左往右移动，当您看到与记忆阶段所记住的词语相匹配时并移动至方框内时，请按下键盘上的OK键。");//增加代码，调用函数，显示数字人下的文字
        }

        protected override async Task OnStopAsync()
        {
            CountdownTimer?.Stop();
            // 停止所有动画
            foreach (var storyboard in TextBlockAnimations)
            {
                storyboard.Stop();
            }
            TextBlockAnimations.Clear();
            //我的函数
            AllLevelResultToArray();
        }

        protected override async Task OnPauseAsync()
        {
            CountdownTimer?.Stop();
            foreach (var storyboard in TextBlockAnimations)
            {
                storyboard.Pause();
            }
        }

        protected override async Task OnNextAsync()
        {
            // 重新启动所有动画
            foreach (var storyboard in TextBlockAnimations)
            {
                storyboard.Stop();
            }
            TextBlockAnimations.Clear();
            WordsToMemorizeTextBLock.Visibility = Visibility.Visible;
            MemorizeOKButton.Visibility = Visibility.Visible;//记忆阶段两个组件显示
            PlayGrid.Visibility = Visibility.Collapsed;
            CheckIfSet();//在显示需要记忆的单词之前就得检查一下参数设置
            ShowWordsToMemorize();//让需要记忆的词汇显示
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new 词语记忆力讲解();
        }

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
            return igonreAnswer[difficultyLevel];
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
                        int ignoreCount = 0;


                        // 计算准确率
                        double accuracy = 0;

                        

                        //int correctCount = GetCorrectNum(lv);
                        //int wrongCount = GetWrongNum(lv);
                        //int ignoreCount = GetIgnoreNum(lv);

                        //if (correctCount == 0 && wrongCount == 0 && ignoreCount == 0)
                        //{
                        //    // 如果所有数据都为0，跳过此难度级别
                        //    Debug.WriteLine($"难度级别 {lv}: 没有数据，跳过.");
                        //    continue;
                        //}

                        //// 计算准确率
                        //double accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);

                        foreach (var item in AllLevelResult)
                        {
                            correctCount += GetCorrectNum(item.Key);
                            wrongCount += GetWrongNum(item.Key);
                            ignoreCount += GetIgnoreNum(item.Key);
                        }
                        accuracy = CalculateAccuracy(correctCount, wrongCount, ignoreCount);

                        // 创建 Result 记录
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "词汇记忆力",
                            Lv = max_hardness, // 当前的难度级别
                            Eval = false,
                            ScheduleId = BaseParameter.ScheduleId ?? null, // 假设的 schedule_id，可以替换为实际值

                        };
                        Debug.WriteLine($"截止");
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
                                    Order = -1,
                                    ValueName = "等级",
                                    Value = max_hardness,
                                    Maxvalue = 20,
                                    Minvalue = 1,
                                    ModuleId = BaseParameter.ModuleId //  BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "总数词汇",
                                    Value = WordsAll,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "错误",
                                    Value = WordsError,
                                    Maxvalue = WordsError,
                                    Minvalue = 0,
                                    Charttype = "柱状图" ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "忽略",
                                    Value = WordsIgnore,
                                    Maxvalue = WordsIgnore,
                                    Minvalue = 0,
                                    Charttype = "柱状图" ,
                                    ModuleId = BaseParameter.ModuleId
                                },
                                new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "记忆时间",
                                    Value = MemorizeSeconds,
                                    ModuleId = BaseParameter.ModuleId
                                }
                            };

                        // 插入 ResultDetail 数据
                        db.ResultDetails.AddRange(resultDetails);
                        await db.SaveChangesAsync();

                        // 输出每个 ResultDetail 对象的数据
                        Debug.WriteLine($"难度级别 {max_hardness}:");
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
