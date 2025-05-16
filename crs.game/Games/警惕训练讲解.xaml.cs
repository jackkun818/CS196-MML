using crs.core;
using crs.core.DbModels;
using crs.game;
using crs.game.Games;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Identity.Client.NativeInterop;
using Spire.Additions.Xps.Schema.Mc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static Azure.Core.HttpHeader;

namespace crs.game.Games
{
    /// <summary>
    /// VIG.xaml 的交互逻辑
    /// </summary>
    public partial class 警惕训练讲解 : BaseUserControl
    {
        //需要从数据库读取的游戏参数
        private int Level = 3;//当前的游戏等级，1-9
        private int MaxTime = 30;//训练时间，单位分钟
        private double RunSpeedFactor = 1;//传送速度，0.3-2.0，作为一个传送因子
        private bool IfVisionFeedBack = true;//视觉反馈，1有0没有
        private bool IfAudioFeedBack = true;//声音反馈，1有0没有
        private int ImageReality = 0;//图片类型，0抽象，1真实
        private bool IfItemEqualInterval = true;//不同物体之间的间隔是否固定
        private int LevelUp = 30;//一次任务中累计答对若干个就可以升等级
        private int LevelDown = 30;//一次任务中累计答错若干个就得降等级

        //程序过程中需要的变量

        //难度等级相关变量
        private int RefNum = 1;//引用物体数量
        private int ChoiceNum = 2;//差异数量，指的是针对引用物体，一共有多少个与之差异的素材
        private int ItemsInLevel = 10;//一个等级一共会有多少个item飘过去
        private double ErrorRate = 0.8;//指的是所有飘过去的item中有多少是错误的不该选的比例
        private int ShowTime = 8;//指的是一个item从左到右一共需要的时间，指代不同等级的速度不同，单位s
        private int ShowIntervel = 2;//指的是不同的item出现的间隔，跟是否勾选IfItemEqualInterval有关系
        private double MaxIntervel = 3;//当勾选了item间隔随机后的最大间隔
        private double MinIntervel = 1;//当勾选了item间隔随机后的最小间隔
        private double FixedIntervel = 1.5;//当勾选了item等间隔后的间隔
        private int MaxLevel = 8;//最大等级，因为素材只准备到了第8级
        private int MinLevel = 1;//最小等级
        private bool IfChange = false;//难度等级是否需要改变

        //图片素材相关变量
        private string ItemsPath = "VIG/real";
        private List<string> AllItems = new List<string>();//用来存放该等级下所有的素材，存的是文件名
        private List<string> ReferenceItems = new List<string>();//存放引用素材
        private List<string> NonReferenceItems = new List<string>();//存放那些非引用素材
        private List<string> ToShowItems = new List<string>();//最后的需要用来show的那些
        private bool IfDetailDiff = false;//难度列表里面的物体差异化类型的是否存在细节差异，颜色轮廓差异在所有难度等级中都存在

        //计时器相关变量
        private DispatcherTimer MaxTimer = new DispatcherTimer(); // 训练时间的计时器
        private DispatcherTimer StarTimer = new DispatcherTimer();// 用一个计时器来定时显示治疗师端的那些星星，保持同步
        private int MaxTimeCount = 0;//游戏时间计时

        //动画相关变量
        private List<Storyboard> StoryBoardsList = new List<Storyboard>();//用来进进出出动画对象
        private List<Storyboard> StoryBoardStartedList = new List<Storyboard>();//把已经开始的动画对象集合到一起，方便暂停
        private Dictionary<Image, bool> ItemStatusDict = new Dictionary<Image, bool>();//用来存储各个Item的状态，防止重复检测
        private System.Timers.Timer StoryBoardTimer = new System.Timers.Timer(1);//创建一个定时器，用来分时开启动画
        private int StoryBoardIndex = 0;//将要开始的动画是第几个
        private Random RandomObject = new Random();//用的随机对象
        private double RandomIntervel = 0;//记录目前的随机间隔是多少
        private int CurrentTime = 0;//当前计数到了多少了，单位为ms
        private bool IfPause = false;//目前是否处于答对答错了的暂停间隔里面，关乎到CurrentTime计数是否要继续增长
        private Dictionary<Storyboard, Image> StoryBoardToImageDict = new Dictionary<Storyboard, Image>();//存储从storyboard到img的映射

        //反馈
        private SoundPlayer soundPlayer; // 用来放歌
        public string ErrorSoundPath;//错误的声音文件路径，在OnStartAsync()中配置
        public string CorrectSoundPath;//正确的声音文件路径，在OnStartAsync()中配置
        private int StopDurations = 1000; // 停止时间，ms
        private string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory; // 当前项目的绝对路径
        private string ResourcesPath = System.IO.Path.Combine("Resources", "词语记忆力");//这里用的固定是词语记忆力的路径，后期可改

        //声明存放结果的数组
        private int[] correctAnswers;//先不说长度多少
        private int[] wrongAnswers;
        private int[] ignoreAnswers;
        private int LevelRight = 0;//在飘过去的ItemsInLevel个Item中答对了多少个，难度等级更新后清0
        private int LevelWrong = 0;//在飘过去的ItemsInLevel个Item中答错了多少个，难度等级更新后清0
        private int LevelIgnore = 0;//在飘过去的ItemsInLevel个Item中答漏了多少个，难度等级更新后清0

        //用于在讲解的时候翻页的配置变量
        private int CurrentPage = -1;
        private List<string> PicPathList = Directory.GetFiles(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Games", "VIG", "explain")).ToList();//获取指定等级的所有素材文件名;
        private string CurrentPicPath;//目前展示的图片路径

        private bool IfTryClicked = false;

        public 警惕训练讲解()
        {
            InitializeComponent();
            //首先进入的是试玩的讲解部分
            CurrentPage = 1;
            ExplainGrid.Visibility = Visibility.Visible;
            PlayGrid.Visibility = Visibility.Collapsed;
            PageSwitch();//进入第一个页面
        }

        private void LevelSet()//调整完Level后在下一轮游戏开始前手动更新参数
        {
            //试玩阶段强制Level=2
            Level = 2;
            switch (Level)
            {
                case 1:
                    RefNum = 1;
                    ChoiceNum = 2;
                    IfDetailDiff = false;
                    ErrorRate = 0.33;
                    ShowTime = 8;
                    //还有item之间的距离需要设置
                    MaxIntervel = 3;
                    MinIntervel = 1;
                    FixedIntervel = 1.5;
                    break;
                case 2:
                    RefNum = 1;
                    ChoiceNum = 3;
                    IfDetailDiff = true;
                    ErrorRate = 0.30;
                    ShowTime = 8;
                    //还有item之间的距离需要设置
                    MinIntervel = 1;
                    MaxIntervel = 3;
                    FixedIntervel = 1.5;
                    break;
                case 3:
                    RefNum = 2;
                    ChoiceNum = 4;
                    IfDetailDiff = false;
                    ErrorRate = 0.28;
                    ShowTime = 7;
                    //还有item之间的距离需要设置
                    MinIntervel = 1.5;
                    MaxIntervel = 4;
                    FixedIntervel = 2;
                    break;
                case 4:
                    RefNum = 2;
                    ChoiceNum = 6;
                    IfDetailDiff = true;
                    ErrorRate = 0.26;
                    ShowTime = 7;
                    //还有item之间的距离需要设置
                    MinIntervel = 1.5;
                    MaxIntervel = 4;
                    FixedIntervel = 2;
                    break;
                case 5:
                    RefNum = 2;
                    ChoiceNum = 4;
                    IfDetailDiff = false;
                    ErrorRate = 0.24;
                    ShowTime = 7;
                    //还有item之间的距离需要设置
                    MinIntervel = 2;
                    MaxIntervel = 5;
                    FixedIntervel = 3;
                    break;
                case 6:
                    RefNum = 2;
                    ChoiceNum = 6;
                    IfDetailDiff = true;
                    ErrorRate = 0.22;
                    ShowTime = 7;
                    //还有item之间的距离需要设置
                    MinIntervel = 2;
                    MaxIntervel = 6;
                    FixedIntervel = 3;
                    break;
                case 7:
                    RefNum = 3;
                    ChoiceNum = 6;
                    IfDetailDiff = true;
                    ErrorRate = 0.2;
                    ShowTime = 6;
                    //还有item之间的距离需要设置
                    MinIntervel = 2;
                    MaxIntervel = 8;
                    FixedIntervel = 3.5;
                    break;
                case 8:
                    RefNum = 3;
                    ChoiceNum = 9;
                    IfDetailDiff = true;
                    ErrorRate = 0.15;
                    ShowTime = 6;
                    //还有item之间的距离需要设置
                    MinIntervel = 2;
                    MaxIntervel = 9;
                    FixedIntervel = 4;
                    break;
                case 9:
                    RefNum = 3;
                    ChoiceNum = 9;
                    IfDetailDiff = true;
                    ErrorRate = 0.1;
                    ShowTime = 6;
                    //还有item之间的距离需要设置
                    MinIntervel = 2;
                    MaxIntervel = 10;
                    FixedIntervel = 4;
                    break;
                default:
                    MessageBox.Show("您的网络有问题，请联系管理员！Error:LevelSet()");
                    break;
            }
            ReadItems();//每次改变Level后就把List准备好
        }

        private void ReadItems()//将本地的item按指定等级读进内存，放在AllItems中
        {
            ItemsPath = System.IO.Path.Combine(BaseDirectory, "Games", "VIG", "real", $"{Level}");
            AllItems = Directory.GetFiles(ItemsPath).ToList();//获取指定等级的所有素材文件名
            //读进来就是完整路径了
            ReferenceItems = AllItems.Where(file => file.Contains("ref")).ToList();//把参考图片读取进来
            NonReferenceItems = AllItems.Where(file => !file.Contains("ref")).ToList();//把参考图片读取进来
        }

        private void ShowRefItem()//将得到的RefItem显示在指定组件上
        {
            ReferenceInit();//每次显示之前都要清空一下
            ReferenceItemGrid.Columns = ReferenceItems.Count;//列数=需要显示的item数目，美观一点
            foreach (var imgName in ReferenceItems)
            {
                Image correctImg = new Image
                {
                    Source = new BitmapImage(new Uri(imgName, UriKind.Absolute)),
                    Stretch = Stretch.Uniform,
                    Visibility = Visibility.Visible,
                    Tag = imgName
                };
                ReferenceItemGrid.Children.Add(correctImg);//加到这个uniformgrid里
            }
        }

        private void ConveyArea_Loaded(object sender, RoutedEventArgs e)
        {
            //手动调用ShowSelectableItem()即可
            PlayGrid.Visibility = Visibility.Visible;
            ExplainGrid.Visibility = Visibility.Collapsed;
            //ShowSelectableItem();
        }

        private List<string> GetRandomSamples(List<string> items, int count) //从LIst中随机采样
        {
            List<string> samples = new List<string>();
            for (int i = 0; i < count; i++)
            {
                int index = RandomObject.Next(items.Count);
                samples.Add(items[index]);
            }
            return samples;
        }

        private void GetItemsToShow()//判断ToShowItems的数目有没有到达规定的ItemsInLevel数目
        {
            int NonRefNum = (int)Math.Ceiling(ItemsInLevel * ErrorRate);
            int RefNum = ItemsInLevel - NonRefNum;
            ToShowItems.AddRange(GetRandomSamples(NonReferenceItems, NonRefNum));//是患者应该选出来的流水线上那些有瑕疵的
            ToShowItems.AddRange(GetRandomSamples(ReferenceItems, RefNum));
            ToShowItems = ToShowItems.OrderBy(x => RandomObject.Next()).ToList();//随机打乱
        }

        private void ShowSelectableItem()//将item弄到传送带上一个一个输送出去
        {
            ConveyAreaInit();//把传送带清空
            GetItemsToShow();
            foreach (var imgName in ToShowItems)
            {
                Image img = new Image
                {
                    Source = new BitmapImage(new Uri(imgName, UriKind.Absolute)),
                    Tag = imgName,
                    Visibility = Visibility.Collapsed,
                    //Height = ConveyArea.ActualHeight,//这个值其实是ConveyArea.Height,需要自己读取
                    Height = 285
                };

                img.Loaded += Img_Loaded;
                ConveyArea.Children.Add(img);
            }
            StartStoryBoardsTimer();
        }

        private void Img_Loaded(object sender, RoutedEventArgs e)
        {
            Image img = sender as Image;

            ItemStatusDict[img] = false;//检测状态变成否
            DoubleAnimation animation = new DoubleAnimation
            {
                From = -300,//从传送带的最左边运动到最右边
                //To = ConveyArea.ActualWidth,
                To = 1340,
                Duration = new Duration(TimeSpan.FromSeconds(ShowTime / RunSpeedFactor))
            };

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            StoryBoardToImageDict[storyboard] = img;
            storyboard.Completed += (s, e) => StoryBoardEnd(img, storyboard);//动画结束时要调用的东西

            CompositionTarget.Rendering += (sender, e) =>
            {// 注册 CompositionTarget.Rendering 事件
                double imgLeft = Canvas.GetLeft(img);                        // 获取当前 Image 的左侧位置
                if (imgLeft > Canvas.GetLeft(TargetRect) + TargetRect.ActualWidth)// 判断是否超出右侧边界
                {
                    // 执行逻辑代码块
                    CheckIfIgnore(img);
                }
            };
            Storyboard.SetTarget(animation, img);
            Storyboard.SetTargetProperty(animation, new PropertyPath(Canvas.LeftProperty));//这意味着 DoubleAnimation 动画将作用于 img 的 Canvas.Left 属性，也就是改变 img 在 Canvas 上的水平位置。
            StoryBoardsList.Add(storyboard); // 将动画添加到列表，然后再统一到某个函数里面按一定次序开始
            ApplyCanvasClip(ConveyArea);//实现裁剪，保证在canvas外的部分不可见，防止露馅
        }

        private void StoryBoardEnd(Image img, Storyboard storyboard)//每个动画结束之后要触发的
        {
            StoryBoardStartedList.Remove(storyboard);//把这个动画对象从列表中移除
            ConveyArea.Children.Remove(img);//把这个img从canvas中移除
            ItemStatusDict.Remove(img);//删除这对键值对
            if (ItemStatusDict.Count <= 0)
            {//说明所有创建过的动画都结束了
                //试玩时，应该判断患者是否曾经答对，如果答对就可以进入本体了
                if (LevelRight > 0)
                {// 可以进入本体了，不要再重复了
                    TipTextBlock.Text = "试玩结束，请点击下方按钮进入训练。";
                    StartTheGameButton.Visibility = Visibility.Visible;
                    //显示按钮，指引按下按钮
                }
                else
                {
                    Init();//资源初始化
                    LevelSet();//使得参数生效
                    ShowRefItem();//显示refItems
                    IfTryClicked = true;
                    ShowSelectableItem();

                }

            }
        }

        private void IfLevelChange()//根据作答结果判断是否等级升降
        {
            if (LevelRight >= LevelUp && Level < MaxLevel)
            {//升级优先
                Level += 1; LevelSet(); IfChange = true;
            }
            else if (LevelWrong >= LevelDown && Level > MinLevel)
            {
                Level -= 1; LevelSet(); IfChange = true;
            }
        }

        private void StartStoryBoardsTimer()//把动画列表里面的动画按一定次序开始和结束
        {
            StoryBoardTimer.Start();//开始计时，准备开始动画
        }

        private void ApplyCanvasClip(Canvas containerCanvas)//通过裁剪，实现image对象在这个canvas中部分可见部分不可见
        {
            // 创建一个与 Canvas 相同大小的矩形
            RectangleGeometry clipGeometry = new RectangleGeometry
            {
                Rect = new Rect(0, 0, containerCanvas.ActualWidth, containerCanvas.ActualHeight)
            };
            // 将该矩形作为 Canvas 的剪裁区域
            containerCanvas.Clip = clipGeometry;
        }

        private async void CheckIntersection()//判断是否交叉
        {
            // 获取 Rectangle 的边界            // 检查是否为 NaN 并给予默认值
            double rectLeft = Canvas.GetLeft(TargetRect); if (double.IsNaN(rectLeft)) rectLeft = 0;
            double rectTop = Canvas.GetTop(TargetRect); if (double.IsNaN(rectTop)) rectTop = 0;

            // 创建 Rectangle 边界
            Rect rectangleBounds = new Rect(rectLeft, rectTop, TargetRect.ActualWidth, TargetRect.ActualHeight);
            foreach (var child in ConveyArea.Children)
            {
                if (child is Image img)
                {
                    // 如果 img 尚未被检测到
                    if (ItemStatusDict[img] == false)
                    {
                        // 获取 img 的边界                        // 检查是否为 NaN 并给予默认值
                        double imgLeft = Canvas.GetLeft(img); if (double.IsNaN(imgLeft)) imgLeft = 0;
                        double imgTop = Canvas.GetTop(img); if (double.IsNaN(imgTop)) imgTop = 0;

                        Rect imgBounds = new Rect(imgLeft, imgTop, img.ActualWidth, img.ActualHeight);
                        // 检查是否有重叠
                        if (rectangleBounds.IntersectsWith(imgBounds))
                        {
                            //进行判断是否正确
                            if (ReferenceItems.Contains(img.Tag))
                            {//如果是refItem之一
                                wrongAnswers[Level] += 1;
                                LevelWrong += 1;
                                TargetRect.Stroke = new SolidColorBrush(Colors.Red);
                                if (IfAudioFeedBack == true) { PlayWav(ErrorSoundPath); }
                                if (IfVisionFeedBack == true) { ShowFeedbackImage(ErrorImage); }
                            }
                            else
                            {//说明患者成功找到那些流水线上有瑕疵的
                                correctAnswers[Level] += 1;
                                LevelRight += 1;
                                TargetRect.Stroke = new SolidColorBrush(Colors.Green);
                                if (IfAudioFeedBack == true) { PlayWav(CorrectSoundPath); }
                                if (IfVisionFeedBack == true) { ShowFeedbackImage(CorrectImage); }

                            }

                            ItemStatusDict[img] = true; // 更新检测状态

                            // 停止所有动画
                            PauseStoryBoards();
                            // 延迟 StopDurations 毫秒
                            IfPause = true;//这个标志位是会涉及到计时器的触发的，StoryBoardTimer_Tick
                            await Task.Delay(StopDurations);
                            IfPause = false;
                            //TargetRect恢复颜色
                            TargetRect.Stroke = new SolidColorBrush(Colors.Black);
                            // 重新启动所有动画
                            ResumeStoryBoards();
                            return; // 找到一个重叠的item后退出循环
                        }
                    }
                }
            }
        }

        private void CheckIfIgnore(Image img)//判断img在移动到右边后是否还存在遗漏
        {
            if (ItemStatusDict.ContainsKey(img) && ItemStatusDict[img] == false)
            {//没有被检查过
                if (!ReferenceItems.Contains(img.Tag))
                {//且不是refItem之一，说明患者还是没找到该找的瑕疵品
                    TargetRect.Stroke = new SolidColorBrush(Colors.DarkGray);//框框变色以提示
                    ignoreAnswers[Level] += 1;
                    LevelIgnore += 1;
                    ItemStatusDict[img] = true;
                }
            }
        }

        private void PauseStoryBoards()//暂停StoryBoardStartedList中的所有动画
        {
            foreach (Storyboard StartedStoryBoard in StoryBoardStartedList)
            {
                StartedStoryBoard.Pause();
            }
        }

        private void ResumeStoryBoards()//重新开始StoryBoardStartedList中的所有动画
        {
            foreach (Storyboard StartedStoryBoard in StoryBoardStartedList)
            {
                StartedStoryBoard.Resume();
            }
        }

        private void Init()//总的最开始的初始化函数
        {
            ItemsInit();
            ReferenceInit();
            ConveyAreaInit();
            TimerInit();
            FeedBackInit();
            AnswersInit();
            LevelResultInit();
            IfChange = false; IfPause = false;
            TargetRect.Stroke = new SolidColorBrush(Colors.Black);
        }

        private void ReInit()//在游戏开始后，下一轮游戏开始前做的初始化，有些变量不用再初始化了
        {

            ItemsInit();
            ReferenceInit();
            ConveyAreaReInit();
            TimerReInit();
            LevelResultInit();
            IfChange = false; IfPause = false;
            TargetRect.Stroke = new SolidColorBrush(Colors.Black);
        }

        private void StartGame()//在初始化完后游戏开始所调用的函数
        {
            ReadItems();
            ShowRefItem();//显示refItems
            ShowSelectableItem();//展示传送带上的东西
            //StoryBoardTimer.Elapsed += StoryBoardTimer_Elapsed;
        }

        private void ItemsInit()//图片素材初始化
        {
            AllItems = new List<string>();//用来存放该等级下所有的素材，存的是文件名
            ReferenceItems = new List<string>();//存放引用素材
            NonReferenceItems = new List<string>();
            ToShowItems = new List<string>();
            StoryBoardsList = new List<Storyboard>();
            StoryBoardStartedList = new List<Storyboard>();
            ItemStatusDict.Clear();
            StoryBoardIndex = 0;
            RandomIntervel = 0;
            CurrentTime = 0;
            StoryBoardToImageDict.Clear();
        }

        private void ReferenceInit()//引用的那些图片的初始化
        {//其实直接清空就好了
            ReferenceItemGrid.Children.Clear();
        }

        private void ConveyAreaInit()//清空传送带上的东西
        {
            ConveyArea.Loaded += ConveyArea_Loaded;
            ConveyArea.Children.Clear();
            StoryBoardsList.Clear();//动画也清楚一下
            StoryBoardIndex = 0;//索引置0
        }

        private void ConveyAreaReInit()//重新清空传送带上的东西
        {
            ConveyArea.Children.Clear();
            StoryBoardsList.Clear();//动画也清楚一下
            StoryBoardIndex = 0;//索引置0
        }

        private void TimerInit()//计时器相关的初始化
        {
            //训练时间计时器初始化
            MaxTimer = new DispatcherTimer();
            MaxTimer.Interval = TimeSpan.FromSeconds(1);  // 设置1s更新一次
            MaxTimer.Tick += MaxTimer_Tick;  // 绑定 Tick 事件
            MaxTimeCount = MaxTime * 60;// 转化为秒

            //治疗师端星星显示计时器初始化
            StarTimer = new DispatcherTimer();
            StarTimer.Interval = TimeSpan.FromSeconds(1);//1s更新一次
            StarTimer.Tick += StarTimer_Tick;

            //动画定时器初始化
            StoryBoardTimer = new System.Timers.Timer(1);//1ms触发一次，方便对时间间隔进行开始动画
            //用DispatcherTimer根本做不到1ms触发一次，所以用System.Timers.Timer
            StoryBoardTimer.Elapsed += StoryBoardTimer_Elapsed;
            CurrentTime = 0;//时间计数值归零
        }

        private void StarTimer_Tick(object sender, EventArgs e)
        {
            //因为只有在动画结束后才能进行升降等级
            if (LevelRight >= LevelUp) { LevelRight = LevelUp; }
            if (LevelWrong >= LevelDown) { LevelWrong = LevelDown; }
            // 调用委托
            LevelStatisticsAction?.Invoke(Level, MaxLevel);
            RightStatisticsAction?.Invoke(LevelRight, LevelUp);
            WrongStatisticsAction?.Invoke(LevelWrong, LevelDown);
        }

        private void TimerReInit()//训练事件计时器就不用再初始化了了
        {
            //动画定时器初始化
            StoryBoardTimer.Stop();

            CurrentTime = 0;//时间计数值归零
        }

        private void AnswersInit()//存放结果的数组初始化
        {
            correctAnswers = new int[MaxLevel + 1];
            wrongAnswers = new int[MaxLevel + 1];
            ignoreAnswers = new int[MaxLevel + 1];
            LevelRight = 0; LevelWrong = 0; LevelIgnore = 0;
        }

        private void LevelResultInit()//在难度等级更新后的等级结果清零
        {
            LevelRight = 0; LevelWrong = 0; LevelIgnore = 0;
        }

        private void StoryBoardTimer_Elapsed(object sender, EventArgs e)//用来定时触发动画开始
        {
            Application.Current.Dispatcher.Invoke(async () =>//这里加了个async很重要
            {//切换回主线程
                if (IfPause)
                {//如果是暂停阶段,暂停所有动画，可简化
                    PauseStoryBoards();
                }
                else
                {//如果不是则正常计数
                    CurrentTime += 1;
                    if (IfItemEqualInterval)
                    {//如果等间隔
                        if (Math.Abs(CurrentTime - FixedIntervel / RunSpeedFactor * 1000) <= 1)
                        {//计时到了固定间隔，该开始另外一个动画了
                            if (StoryBoardIndex >= StoryBoardsList.Count)
                            {                            // 检查是否所有动画都已经开始
                                StoryBoardTimer.Stop(); // 停止计时器
                                StoryBoardTimer.Elapsed -= StoryBoardTimer_Elapsed;
                                return;
                            }
                            StoryBoardToImageDict[StoryBoardsList[StoryBoardIndex]].Visibility = Visibility.Visible;
                            StoryBoardsList[StoryBoardIndex].Begin();
                            StoryBoardStartedList.Add(StoryBoardsList[StoryBoardIndex]);
                            StoryBoardIndex += 1;
                            CurrentTime = 0;//重新开始计数
                        }
                    }
                    else
                    {//如果不等间隔
                        if (Math.Abs(CurrentTime - RandomIntervel / RunSpeedFactor * 1000) <= 1)
                        {
                            if (StoryBoardIndex >= StoryBoardsList.Count)
                            {                            // 检查是否所有动画都已经开始
                                StoryBoardTimer.Stop(); // 停止计时器
                                StoryBoardTimer.Elapsed -= StoryBoardTimer_Elapsed;
                                return;
                            }
                            StoryBoardToImageDict[StoryBoardsList[StoryBoardIndex]].Visibility = Visibility.Visible;
                            StoryBoardsList[StoryBoardIndex].Begin();
                            StoryBoardStartedList.Add(StoryBoardsList[StoryBoardIndex]);
                            RandomIntervel = RandomObject.NextDouble() * (MaxIntervel - MinIntervel) + MinIntervel;//更新间隔
                            StoryBoardIndex += 1;
                            CurrentTime = 0;

                        }
                    }
                }

            });

        }

        private void MaxTimer_Tick(object sender, EventArgs e)//总的训练时间的触发事件
        {
            MaxTimeCount -= 1;
            if (MaxTimeCount >= 0)
            {   //第一个空指的是总倒计时，第二参数指的是本题倒计时
                TimeStatisticsAction?.Invoke((int)MaxTimeCount, (int)MaxTimeCount);//显示倒计时
            }
            else
            {
                //强制游戏结束相关逻辑
                OnGameEnd();
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

        /*LJN
         添加进来视觉、声音反馈的资源
         */
        private void FeedBackInit()
        {
            /*LJN
            配置反馈资源的路径             
             */
            CorrectSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");
            // 为 Image 控件加载图片 Source
            CorrectImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Correct.png"), UriKind.RelativeOrAbsolute));
            ErrorImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Error.png"), UriKind.RelativeOrAbsolute));

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

            // 延迟指定的时间（例如1秒）
            await Task.Delay(StopDurations);

            image.Visibility = Visibility.Collapsed;
        }
    }
    public partial class 警惕训练讲解 : BaseUserControl
    {
        private void LastStep_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage--;
            PageSwitch();
        }

        private void NextStep_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage++;
            PageSwitch();
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {//跳过整个试玩，直接开始本体
            OnGameBegin();
        }

        async private void Try_Click(object sender, RoutedEventArgs e)
        {
            PlayGrid.Visibility = Visibility.Visible;
            ExplainGrid.Visibility = Visibility.Collapsed;
            Init();//资源初始化
            LevelSet();//使得参数生效
            ShowRefItem();//显示refItems
            IfTryClicked = true;
            ShowSelectableItem();

            //LevelSet();
            //IfLevelChange();//先判断是否需要进行等级升降
            //                //这部分结果初始化
            //ReInit();
            ////开始新游戏
            //StartGame();


            SetTitleVisibleAction?.Invoke(true);//显示"题目规则"四个黑字
            RuleAction?.Invoke("在屏幕上的传送带上方会有若干个图像，随后在屏幕上会有一系列图像从左往右移动，当您看到与传送带上方有差异的图象时，请按下OK键");//增加代码，调用函数，显示数字人下的文字                                             
            //StartStoryBoardsTimer();
        }

        private async void PageSwitch()//根据page值来显示组件
        {
            switch (CurrentPage)
            {
                case 1://第一个页面，只有下一步和跳过
                    ButtonExplainGrid.Columns = 2;
                    LastStep.Visibility = Visibility.Collapsed;
                    NextStep.Visibility = Visibility.Visible;
                    Skip.Visibility = Visibility.Visible;
                    Try.Visibility = Visibility.Collapsed;

                    //接下来是图片展示的问题
                    PicExplain.Source = null;

                    //接下来是提示语的展示问题
                    TextExplain.Text = "现在将向您讲解本模块，请点击下方按钮进入讲解。";
                    await OnVoicePlayAsync(TextExplain.Text);
                    break;
                case 2://第二个页面，既有上一步也有下一步和跳过
                    ButtonExplainGrid.Columns = 3;
                    LastStep.Visibility = Visibility.Visible;
                    NextStep.Visibility = Visibility.Visible;
                    Skip.Visibility = Visibility.Visible;
                    Try.Visibility = Visibility.Collapsed;

                    //接下来是图片展示的问题
                    CurrentPicPath = PicPathList.FirstOrDefault(item => item.Contains("Part1.png"));
                    PicExplain.Source = new BitmapImage(new Uri(CurrentPicPath, UriKind.RelativeOrAbsolute));


                    //接下来是提示语的展示问题
                    TextExplain.Text = "首先您会在方框中看到若干参考物。";
                    await OnVoicePlayAsync(TextExplain.Text);
                    break;
                case 3://第三个页面，既有上一步，下一步，也有跳过
                    ButtonExplainGrid.Columns = 3;
                    LastStep.Visibility = Visibility.Visible;
                    NextStep.Visibility = Visibility.Visible;
                    Skip.Visibility = Visibility.Visible;
                    Try.Visibility = Visibility.Collapsed;

                    //接下来是图片展示的问题
                    CurrentPicPath = PicPathList.FirstOrDefault(item => item.Contains("Part2.png"));
                    PicExplain.Source = new BitmapImage(new Uri(CurrentPicPath, UriKind.RelativeOrAbsolute));

                    //接下来是提示语的展示问题
                    TextExplain.Text = "然后您会看到一条传送带上有若干物品从左到右移动。";
                    await OnVoicePlayAsync(TextExplain.Text);
                    break;
                case 4://第四个页面，既有上一步，下一步，也有跳过
                    ButtonExplainGrid.Columns = 3;
                    LastStep.Visibility = Visibility.Visible;
                    NextStep.Visibility = Visibility.Visible;
                    Skip.Visibility = Visibility.Visible;
                    Try.Visibility = Visibility.Collapsed;

                    //接下来是图片展示的问题
                    CurrentPicPath = PicPathList.FirstOrDefault(item => item.Contains("Part3.png"));
                    PicExplain.Source = new BitmapImage(new Uri(CurrentPicPath, UriKind.RelativeOrAbsolute));

                    //接下来是提示语的展示问题
                    TextExplain.Text = "您需要在物品移动到黑色框时判断该物品是否与上方参考物有差异，并按下OK键以确认其存在差异。";
                    await OnVoicePlayAsync(TextExplain.Text);
                    break;
                case 5://第五个页面，只有上一步，试玩和跳过
                    ButtonExplainGrid.Columns = 3;
                    LastStep.Visibility = Visibility.Visible;
                    NextStep.Visibility = Visibility.Collapsed;
                    Skip.Visibility = Visibility.Visible;
                    Try.Visibility = Visibility.Visible;

                    //接下来是图片展示的问题
                    PicExplain.Source = null;

                    //接下来是提示语的展示问题
                    TextExplain.Text = "讲解完毕，请点击试玩按钮以开始试玩，或点击跳过进入正式游戏";
                    await OnVoicePlayAsync(TextExplain.Text);
                    break;
                default:
                    System.Windows.MessageBox.Show("您的网络有问题，请联系管理员");
                    break;
            }
        }
    }
}
