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
using System.Windows.Controls.Primitives;
using System.Text.Json;
using Newtonsoft.Json;
using System.Runtime.Intrinsics.Arm;

namespace crs.game.Games
{
    /// <summary>
    /// EXO.xaml 的交互逻辑
    /// </summary>
    public partial class 搜索能力3 : BaseUserControl
    {
        private readonly List<List<string>> imagePaths = new List<List<string>>
        {
            new List<string>
            {
                
            },

            new List<string>
            {
                "EXO/2/Rocket1.png", "EXO/2/Rocket2.png", "EXO/2/Star.png", "EXO/2/Planet1.png",
                "EXO/2/Planet3.png", "EXO/2/Sun.png", "EXO/2/Planet2.png", "EXO/2/Meteor.png",
                "EXO/2/Planet4.png", "EXO/2/Spaceship.png", "EXO/2/Background.png"
            }

        };

        /*
         需要从数据库读取，且可被用户改变的参数
         */
        private bool IfLimitTime = false;//是否限制作答时间
        private int LevelUp = 5;//等级提高需要做对的题数
        private int LevelDown = 5;//等级降低需要做对的题数
        private int max_time = 1; // 总的治疗时间
        private bool IfVisualFeedBack = true;//视觉反馈
        private bool IfAudioFeedBack = true;//声音反馈
        private int Level = 1; // 当前游戏难度等级
        /*
         游戏运行过程中的参数
         */

        //FindMode
        private int FindMode = 1;//只是前期调试时用来指代，比数字3，4直观一些
        private int FindItem = 0;//用来给imagePaths索引使用，方便调试

        private int Complexity = 1;//Find不用Count模式的难度等级列表中的复杂性，1简单，2中等，3复杂，4非常复杂
        private string ComplexityFolder;//find模式的难度等级中对应的复杂度，用来对文件夹进行索引
        private int right_picture_number = 4; // 显示的正确图片数量
        private int chose_picture_number = 6; // 显示的可选择图片总数量，包含正确的，也包含错误的

        private Dictionary<Border, bool> BorderStatusDict = new Dictionary<Border, bool>();//Find模式：用来存储Border的选中状态，来判断是否需要框框变色；

        //FindCountMode
        private int FindAndCountMode = 2;//这两个变量只是前期调试时用来指代，比数字3，4直观一些
        private int FindCountItem = 1;//用来给imagePaths索引使用，方便调试

        private int max_right_display = 2; // 最多显示的正确图片数量,即需要计数的对象总数
        private int min_right_display = 1; // 最少显示的正确图片数量
        private int mislead_picture_display_number = 4; // 干扰图片中总的显示数量

        private List<Border> CountBordersList = new List<Border>();//把下面需要计数的东西用一个List存储起来，好让患者一个接一个地知道自己要数什么图形
        private int IndexOfCount = 0;//患者现在是在对第几个东西进行计数

        private int ErrorCount = 0;
        private int ErrorLimit = 2;//患者在一道题中顶多能输入错几次

        private Dictionary<Image, Border> ImageToBordersDict = new Dictionary<Image, Border>();//通过Image来索引到上面背景板的所有对应的border，方便让border变色 

        private Dictionary<Image, int> correctImagesCount = new Dictionary<Image, int>();//正确图片以及相应的个数

        //共用的
        private bool IfFisrtSet = false;//有些组件需要在load完之后再显示内容，达到动态调整的方式

        private string BackGroundPath;//背景路径，用一个变量来代替，好修改

        private List<Image> correctImages; // 正确图片的列表
        private List<Image> selectedImages; // 用户选择的图片

        private int[] correctAnswers;
        private int[] wrongAnswers;
        private int[] ignoreAnswers;

        private int MaxLevel = 18;//所能达到的最大等级
        private int MinLevel = 1;//所能达到的最小等级

        private DispatcherTimer gameTimer; // 全局计时器
        private TimeSpan timeRemaining; // 剩余时间

        private DispatcherTimer PlayTimer;//作答时间限制，勾选了IfLimitTime才有效
        private int BaseTimeLimit = 60;//最低等级的限制时间，秒
        private int TimeLimitInterval = 5;//不同等级之间的时间限制差距，秒，等差数列
        private int PlayTime = 0;//限制的答题时间，根据等级变化来设置

        private DispatcherTimer IntervalTimer;//题与题之间的间隔计时器
        private int CorrectInterval = 3;//答对3s间隔
        private int ErrorInterval = 5;//答错5s间隔

        private bool is_gaming = false;
        private int success_time = 0;
        private int fail_time = 0;

        private bool IfLevelDown = false;//是否需要降低等级
        private bool IfLevelUp = false;//是否需要升级等级

        private string userInput; // 存储用户输入的数字

        private bool is_finish = false;

        //------------------报告参数-------------------------------------
        private int train_mode = 1; // 游戏模式，1，2，3，4	1.	模式1：找出数字范围内的缺失数字，并将它们从小到大逐个输入。这种模式通常涉及用户识别和输入缺失的数字。
                                    //模式2：识别出叠加在一起的不同形状，并将它们从屏幕下部选择出来。这种模式涉及到用户需要从叠加的形状中找到正确的形状。
                                    //模式4：数出并输入每个正确对象在图片中出现的次数。

        private int repet_time = 1;

        //----------------------------------------------------------------

        private int repet_count = 0;

        public 搜索能力3()
        {
            InitializeComponent();
            this.Cursor = Cursors.None; // 隐藏默认光标
            CustomCursor.Visibility = Visibility.Visible; // 显示自定义光标
            MouseMove += Window_MouseMove; // 订阅 MouseMove 事件
            CustomCursor.Width = 65; // 调整为你想要的宽度
            CustomCursor.Height = 65; // 调整为你想要的高度
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
        public static Window GetTopLevelWindow(UserControl userControl)
        {
            DependencyObject current = userControl;
            while (current != null && !(current is Window))
            {
                current = VisualTreeHelper.GetParent(current);
            }

            return current as Window;
        }

        /*FindMode*/
        private void SetupFindMode()
        {
            //先绘制背景
            BackGround.Loaded += BackGround_Loaded;
            //准备素材
            GetItemsReady();
            // 初始化正确图片列表
            correctImages = new List<Image>();
            selectedImages = new List<Image>();

            // 随机选择正确的图片，并将其添加到correctImages列表中
            Random rand = new Random();
            List<int> correctIndices = new List<int>();
            while (correctIndices.Count < right_picture_number)
            {
                int index = rand.Next(imagePaths[FindItem].Count);

                if (imagePaths[FindItem][index] != BackGroundPath && !correctIndices.Contains(index))
                {//选择正确图片
                    correctIndices.Add(index);
                    Image img = new Image
                    {
                        Source = new BitmapImage(new Uri(imagePaths[FindItem][index], UriKind.Relative)),
                    };
                    correctImages.Add(img);
                }
            }

            if (IfFisrtSet == false)
            {//第一次绘制内容需要等组件load完了才能绘制
                SelectionAreaGrid.Loaded += (s, e) => DisplaySelectableImages(SelectionCanvas, rand);//在SelectionCanvas显示图片,因为需要动态调整，所以在load完再显示
                TargetAreaGrid.Loaded += (s, e) => DisplayCorrectImages(TargetItemGrid);//在TargetItemGrid显示待选图片,因为需要动态调整，所以在load完再调用
                IfFisrtSet = true;
            }
            else
            {//不是第一次则是直接调用
                BackGround_Loaded(BackGround, new RoutedEventArgs());
                DisplaySelectableImages(SelectionCanvas, rand);
                DisplayCorrectImages(TargetItemGrid);
            }
        }

        private void BackGround_Loaded(object sender, RoutedEventArgs e)//在组件加载时BackGround，Load上去
        {//把背景图片尽量大的方式去
            // 创建背景图片
            BackGround.Children.Clear();
            Image backgroundImage = new Image
            {
                Source = new BitmapImage(new Uri(BackGroundPath, UriKind.Relative)),
                Stretch = Stretch.Fill,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            BackGround.Children.Add(backgroundImage);//加到BackGroundGrid中

        }

        private void DisplayCorrectImages(UniformGrid TargetItemGrid)//展示下面一行给患者看的图片
        {
            TargetItemGrid.Children.Clear();
            TargetItemGrid.Columns = correctImages.Count;//确保能一行显示完
            double Size = Math.Min(TargetItemGrid.ActualWidth / correctImages.Count-10, TargetItemGrid.ActualHeight-10);//-10是为了留点裕量，保证显示完全
            foreach (var img in correctImages)
            {
                Image correctImg = new Image
                {
                    Source = img.Source,
                    Width = Size*0.7,
                    Height = Size*0.7,//确保是正方形
                    Margin = new Thickness(10)
                };
                TargetItemGrid.Children.Add(correctImg);//加到这个uniformgrid里
            }


        }

        private void DisplaySelectableImages(Canvas selectionCanvas, Random rand)//展示上面那些让患者点击选的图片
        {
            selectionCanvas.Children.Clear();//每次调用前先清除一下
                                             // 获取背景图 BackGround
            /*
             背景图本身采用Fill填充，所以先需要计算它长宽方向的缩放比例，同步来缩放它的item的size
            然后同时测量得到的json文件里面的px距离也得缩放
            px距离缩放后还得转化成Canvas.SetLeft所利用的设备无关像素（Device-Independent Pixels, DIP）距离
            WPF中的空间大小、距离单位都是1/96英寸，[物理单位大小]=[独立设备单位大小] × [屏幕DPI点数]
            显示到屏幕上的物理大小(像素)= WPF的单位(1/96) * 屏幕设置的DPI
            由于蓝湖上用的是PX距离，WPF上用的是DIP距离
            素材背景如此处理：直接Fill到整个Gird中
            素材：首先要缩放，按 素材宽/背景宽 素材高/背景高   这个比例去缩放，对应的margin也去缩放
             */
            var backGround = (Grid)SelectionAreaGrid.FindName("BackGround");
            double bgWidthScale = backGround.ActualWidth/((imageData.BackgroundImage.Size.Width / 96) * screenDpiHorizontal);
            double bgHeightScale = backGround.ActualHeight / ((imageData.BackgroundImage.Size.Height / 96) * screenDpiVertical);
            // 获取背景图的矩形边界，TransformToAncestor 用来获取相对于父元素的位置
            var backgroundBounds = backGround.TransformToAncestor(SelectionAreaGrid)
                .TransformBounds(new Rect(0, 0, backGround.ActualWidth, backGround.ActualHeight));

            // 背景图的边界信息
            double bgLeft = backgroundBounds.Left; // 背景图相对 SelectionAreaGrid 的左边位置
            double bgTop = backgroundBounds.Top;   // 背景图相对 SelectionAreaGrid 的上边位置

            // 创建一个可选择图片的索引列表，并首先添加正确图片的索引
            List<int> selectableIndices = correctImages
                .Select(img => imagePaths[FindItem].IndexOf(((BitmapImage)img.Source).UriSource.ToString().Replace("pack://application:,,,", "")))
                .ToList();


            // 从剩余的图片中随机选择，直到达到chose_picture_number
            List<int> remainingIndices = Enumerable.Range(0, imagePaths[FindItem].Count)
                .Where(i => !selectableIndices.Contains(i) && imagePaths[FindItem][i] != BackGroundPath)
                .OrderBy(x => rand.Next())
                .Take(chose_picture_number - selectableIndices.Count)//Take只会选取前若干个，所以是不会重复的，除非个数不足
                .ToList();

            selectableIndices.AddRange(remainingIndices);

            // 随机化选择图片的顺序
            selectableIndices = selectableIndices.OrderBy(x => rand.Next()).ToList();
            foreach (int index in selectableIndices)
            {
                Image img = new Image
                {
                    Source = new BitmapImage(new Uri(imagePaths[FindItem][index], UriKind.Relative)),
                    Width = (imageData.Images[index].Size.Width / 96) * screenDpiHorizontal * bgWidthScale,
                    Height = (imageData.Images[index].Size.Height / 96) * screenDpiVertical * bgHeightScale,
                };
                Border border = new Border
                {
                    BorderThickness = new Thickness(2),
                    BorderBrush = Brushes.Transparent,
                    Child = img
                };

                //设置 Border 的位置
                // 获取图标相对背景图的 PxMargin
                double marginLeft = (imageData.Images[index].PxMargin.Left / 96) * screenDpiHorizontal * bgWidthScale;
                double marginTop = (imageData.Images[index].PxMargin.Up / 96) * screenDpiVertical * bgHeightScale;
                // 计算图标相对 Canvas 的位置
                double iconLeft = bgLeft + marginLeft; // 相对 Canvas 的左边距离
                double iconTop = bgTop + marginTop; // 相对 Canvas 的上边距离
                Canvas.SetLeft(border, iconLeft);//相对canvas的偏移
                Canvas.SetTop(border, iconTop);
                Canvas.SetZIndex(border, int.MaxValue);  // 将 border 设置为最上层

                BorderStatusDict[border] = false;//一开始都是初始化没选中
                border.MouseLeftButtonDown += Border_MouseLeftButtonDown;//实现选中后框框变色

                selectionCanvas.Children.Add(border);
                
            }
        }
        
        private void BorderColorChange()//在Find模式下，点击了确认按钮，那些选中的图片就该变色了
        {
            //处理选中的图片的框框颜色显示
            foreach (var kvp in BorderStatusDict)
            {
                Border border = kvp.Key;
                border.MouseLeftButtonDown -= Border_MouseLeftButtonDown;//已经揭晓答案了就不要有变色机制了
                Image img = border.Child as Image;
                bool status = kvp.Value;
                if (status == true)//只针对那些选中过的图片来处理框框
                {
                    if (correctImages.Any(c => ((BitmapImage)c.Source).UriSource.ToString() == ((BitmapImage)img.Source).UriSource.ToString()))
                    {//这个判断应该在揭晓答案之后才让BorderBrush变色！
                        border.BorderBrush = Brushes.Green;
                    }
                    else
                    {
                        border.BorderBrush = Brushes.Red;
                    }
                }
                else//没有选中过的照片也可能是遗漏的
                {
                    if (correctImages.Any(c => ((BitmapImage)c.Source).UriSource.ToString() == ((BitmapImage)img.Source).UriSource.ToString()))
                    {//这个判断应该在揭晓答案之后才让BorderBrush变色！
                        border.BorderBrush = Brushes.Orange;
                    }
                }
            }
        }

        private void FoundButton_Click(object sender, RoutedEventArgs e)//Find模式下的确认按钮，判断主逻辑
        {
            BorderColorChange();//先处理一下框框变色，优化空间：做一次判断，各种操作共享，而不是各种操作分开判断

            bool isCorrect = selectedImages.Count == correctImages.Count &&
                        selectedImages.All(si => correctImages.Any(ci => ((BitmapImage)si.Source).UriSource == ((BitmapImage)ci.Source).UriSource));
            if (isCorrect)
            {
                success_time++;
                correctAnswers[Level] += 1;
                IntervalTimer.Interval = TimeSpan.FromSeconds(CorrectInterval);
                if (IfAudioFeedBack) PlayWav(CorrectSoundPath);
                if (IfVisualFeedBack) ShowFeedbackImage(CorrectImage, CorrectInterval * 1000);
            }
            else
            {
                fail_time++;
                wrongAnswers[Level] += 1;
                IntervalTimer.Interval = TimeSpan.FromSeconds(ErrorInterval);
                if (IfAudioFeedBack) PlayWav(ErrorSoundPath);
                if (IfVisualFeedBack) ShowFeedbackImage(ErrorImage, ErrorInterval * 1000);
                int ignoreCount = correctImages.Count(ci => !selectedImages.Any(si => ((BitmapImage)si.Source).UriSource == ((BitmapImage)ci.Source).UriSource));
                ignoreAnswers[Level] += ignoreCount;
                is_finish = false;
            }

            ResultCheck();// 判断完了就该检查结果


            AdjustFindModeDifficulty(); //看看是否需要调整游戏难度

            //接下来等计时器触发了才能开始下一句游戏，同时在倒计时期间不能触发任何组件
            if (IfLimitTime)
            {
                PlayTime = BaseTimeLimit + (Level - MinLevel) * TimeLimitInterval;//计算好时间限制
                PlayTimer.Stop();//作答时间限制计时器停止
            }
            FoundButton.IsEnabled = false;
            IntervalTimer.Start();
        }

        int max_hardness = 1;
        private void AdjustFindModeDifficulty()//根据level值来调整对应的参数
        {
            if (IfLevelUp && Level <= MaxLevel)//如果足够升级等级
            {
                if (Level < MaxLevel) { Level += 1; }
                IfLevelUp = false; ResultInit();//每次升降等级都得清空重新计算
                max_hardness = Math.Max(max_hardness, Level);
            }
            else if (IfLevelDown && Level >= MinLevel)
            {
                if (Level > MinLevel) { Level -= 1; }
                IfLevelDown = false; ResultInit();
            }
            FindModeLevelCheck(Level);//跟据Level值来调整参数，确保参数生效
        }

        private void FindModeLevelCheck(int level)
        {
            switch (level)
            {
                case 1:
                    right_picture_number = 2;
                    chose_picture_number = 6; // 2个正确图片 + 4个干扰图片
                    Complexity = 1;
                    break;
                case 2:
                    right_picture_number = 2;
                    chose_picture_number = 7; // 2个正确图片 + 5个干扰图片
                    Complexity = 1;
                    break;
                case 3:
                    right_picture_number = 3;
                    chose_picture_number = 9; // 3个正确图片 + 6个干扰图片
                    Complexity = 1;
                    break;
                case 4:
                    right_picture_number = 3;
                    chose_picture_number = 10; // 3个正确图片 + 7个干扰图片
                    Complexity = 1;
                    break;
                case 5:
                    right_picture_number = 4;
                    chose_picture_number = 12; // 4个正确图片 + 8个干扰图片
                    Complexity = 1;
                    break;
                case 6:
                    right_picture_number = 4;
                    chose_picture_number = 10; // 4个正确图片 + 6个干扰图片
                    Complexity = 2;
                    break;
                case 7:
                    right_picture_number = 5;
                    chose_picture_number = 12; // 5个正确图片 + 7个干扰图片
                    Complexity = 2;
                    break;
                case 8:
                    right_picture_number = 5;
                    chose_picture_number = 13; // 5个正确图片 + 8个干扰图片
                    Complexity = 2;
                    break;
                case 9:
                    right_picture_number = 6;
                    chose_picture_number = 15; // 6个正确图片 + 9个干扰图片
                    Complexity = 2;
                    break;
                case 10:
                    right_picture_number = 6;
                    chose_picture_number = 16; // 6个正确图片 + 10个干扰图片
                    Complexity = 2;
                    break;
                case 11:
                    right_picture_number = 7;
                    chose_picture_number = 14; // 7个正确图片 + 7个干扰图片
                    Complexity = 3;
                    break;
                case 12:
                    right_picture_number = 7;
                    chose_picture_number = 15; // 7个正确图片 + 8个干扰图片
                    Complexity = 3;
                    break;
                case 13:
                    right_picture_number = 8;
                    chose_picture_number = 14; // 8个正确图片 + 9个干扰图片
                    Complexity = 3;
                    break;
                case 14:
                    right_picture_number = 8;
                    chose_picture_number = 18; // 8个正确图片 + 10个干扰图片
                    Complexity = 3;
                    break;
                case 15:
                    right_picture_number = 10;
                    chose_picture_number = 21; // 10个正确图片 + 11个干扰图片
                    Complexity = 3;
                    break;
                case 16:
                    right_picture_number = 10;
                    chose_picture_number = 25; // 10个正确图片 + 15个干扰图片
                    Complexity = 4;
                    break;
                case 17:
                    right_picture_number = 10;
                    chose_picture_number = 30; // 10个正确图片 + 20个干扰图片
                    Complexity = 4;
                    break;
                case 18:
                    right_picture_number = 10;
                    chose_picture_number = 35; // 10个正确图片 + 25个干扰图片
                    Complexity = 4;
                    break;
                default:
                    right_picture_number = 2;
                    chose_picture_number = 6; // 2个正确图片 + 4个干扰图片
                    Complexity = 1;
                    break;
            }
            GetComplexityFolder();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            Image img = border.Child as Image;
            if (border != null && BorderStatusDict.ContainsKey(border))
            {
                // 选框变色逻辑
                if (BorderStatusDict[border] == false) // 没有选中过
                {
                    border.BorderBrush = Brushes.Blue;
                    BorderStatusDict[border] = true;
                    selectedImages.Add(img);//加入列表
                }
                else // 取消选中
                {
                    border.BorderBrush = Brushes.Transparent;
                    BorderStatusDict[border] = false;
                    selectedImages.Remove(img);//取消选中
                }
            }
        }

        private void GetComplexityFolder()//将复杂度从int转成string
        {
            switch(Complexity)
            {
                case 1:ComplexityFolder = "Easy";break;
                case 2:ComplexityFolder = "Medium";break;
                case 3:ComplexityFolder = "Hard";break;
                case 4:ComplexityFolder = "VeryHard";break;
                default:ComplexityFolder = "VeryHard";break;
            }
        }
        /*FindCountMode*/
        private void SetupFindCountMode()
        {
            //把两张反馈图片的位置放到右手边好看一点
            CorrectImage.HorizontalAlignment = HorizontalAlignment.Right;
            ErrorImage.HorizontalAlignment = HorizontalAlignment.Right;
            //先加载背景
            BackGroundPath = "EXO/2/Background.png";
            BackGroundFindCount.Loaded += BackGroundFindCount_Loaded;

            // 初始化正确图片列表
            correctImages = new List<Image>();
            IndexOfCount = 0;

            // 随机选择正确的图片，并将其添加到correctImages列表中
            Random rand = new Random();
            List<int> correctIndices = new List<int>();

            while (correctIndices.Count < right_picture_number)
            {
                int index = rand.Next(imagePaths[FindCountItem].Count);

                if (imagePaths[FindCountItem][index] != BackGroundPath && !correctIndices.Contains(index))
                {
                    correctIndices.Add(index);

                    // 将正确图片添加到correctImages列表中
                    Image img = new Image
                    {
                        Source = new BitmapImage(new Uri(imagePaths[FindCountItem][index], UriKind.Relative)),
                    };

                    correctImages.Add(img);
                }
            }
            if (IfFisrtSet == false)
            {//第一次绘制内容需要等组件load完了才能绘制
                FindCountGrid.Loaded += (s, e) => DisplayCountableImages(FindCountCanvas, rand);//在背景板上叠一层出现的随机图片
                TargetAreaFindCount.Loaded += (s, e) => DisplayToCountImages();
                IfFisrtSet = true;

            }
            else
            {//不是第一次则是直接调用
                DisplayCountableImages(FindCountCanvas, rand);//在背景板上叠一层出现的随机图片
                DisplayToCountImages();
            }


        }

        private void BackGroundFindCount_Loaded(object sender, RoutedEventArgs e)//把BackGround加上
        {
            Image backgroundImage = new Image
            {
                Source = new BitmapImage(new Uri(BackGroundPath, UriKind.Relative)),
                //Stretch = Stretch.Uniform,
                Stretch = Stretch.Fill,
                Width = BackGroundFindCount.ActualWidth,  // 调整背景图片的宽度
                Height = BackGroundFindCount.ActualHeight,  // 调整背景图片的高度
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            BackGroundFindCount.Children.Add(backgroundImage);//存放背景板
        }

        private void DisplayCountableImages(Canvas selectionCanvas, Random rand)//展示上面那些需要患者去一个一个数的图片
        {
            selectionCanvas.Children.Clear();//每次调用前先清除一下
            //获取边界信息
            var selectionAreaGrid = (Grid)selectionCanvas.Parent; // 获取 SelectionCanvas 的父控件，即SelectionAreaGrid
            var gridBounds = selectionAreaGrid.TransformToAncestor(this)
                .TransformBounds(new Rect(0, 0, selectionAreaGrid.ActualWidth, selectionAreaGrid.ActualHeight));

            // 边界信息
            double leftBound = gridBounds.Left;//多一个Transform
            double rightBound = gridBounds.Right;
            double topBound = gridBounds.Top;
            double bottomBound = gridBounds.Bottom;

            // 初始化图片显示计数
            List<Image> imagesToDisplay = new List<Image>();

            // 添加正确图片到图片显示列表
            foreach (var correctImage in correctImages)
            {
                // 为每个正确图片随机生成显示次数
                int displayCount = rand.Next(min_right_display, max_right_display + 1);
                correctImagesCount[correctImage] = displayCount;//记录它的个数
                for (int i = 0; i < displayCount; i++)
                {
                    Image imgCopy = new Image
                    {
                        Source = correctImage.Source,
                        Width = 80,
                        Height = 80,
                        Margin = new Thickness(10)
                    };
                    imagesToDisplay.Add(imgCopy);
                }
            }

            // 添加干扰图片到图片显示列表
            List<int> remainingIndices = Enumerable.Range(0, imagePaths[FindCountItem].Count)
                .Where(i => !correctImages.Any(c => ((BitmapImage)c.Source).UriSource.ToString().EndsWith(imagePaths[FindCountItem][i]))
                             && imagePaths[FindCountItem][i] != BackGroundPath)
                .OrderBy(x => rand.Next())
                .Take(mislead_picture_display_number)
                .ToList();

            foreach (var index in remainingIndices)
            {
                Image img = new Image
                {
                    Source = new BitmapImage(new Uri(imagePaths[FindCountItem][index], UriKind.Relative)),
                    Width = 80,
                    Height = 80,
                    Margin = new Thickness(10)
                };

                imagesToDisplay.Add(img);
            }

            // 已生成的图片位置列表，用于检测碰撞
            List<Rect> existingRects = new List<Rect>();
            // 随机化图片的位置，并将图片显示在Canvas中
            foreach (var img in imagesToDisplay)
            {
                // 创建边框并设置图片位置
                Border border = new Border
                {
                    BorderThickness = new Thickness(2),
                    BorderBrush = Brushes.Transparent, // 初始时无边框
                    Child = img
                };
                double maxLeft = rightBound - img.Width;
                double maxTop = bottomBound - img.Height;

                Rect newRect;
                bool isOverlapping;
                // 随机生成位置直到没有重叠
                do
                {                // 随机生成X和Y坐标，确保图片不会超出背景图片边界
                    double left = rand.NextDouble() * (maxLeft - leftBound) + leftBound;
                    double top = rand.NextDouble() * (maxTop - topBound) + topBound;

                    newRect = new Rect(left, top, img.Width, img.Height);
                    isOverlapping = existingRects.Any(rect => rect.IntersectsWith(newRect));
                } while (isOverlapping);

                // 将新图片的位置加入到已有位置列表
                existingRects.Add(newRect);
                // 设置 Border 的位置
                Canvas.SetLeft(border, newRect.Left - leftBound);//相对canvas的偏移
                Canvas.SetTop(border, newRect.Top - topBound);

                selectionCanvas.Children.Add(border);

                // 判断 img 是否为那些下面展示的对象，而不是在背景板上面的对象
                bool isInCorrectImage = correctImages.Any(correctImage =>
                    ((BitmapImage)correctImage.Source).UriSource == ((BitmapImage)img.Source).UriSource);
                if (isInCorrectImage)
                {
                    ImageToBordersDict[img] = border;
                }
            }
        }

        private void DisplayToCountImages()//展示正确图片，让患者知道需要对什么计数
        {
            // 创建一个HashSet用于去重，确保每个正确图片只显示一次
            HashSet<string> displayedImages = new HashSet<string>();
            TargetItemGridFindCount.Columns = correctImages.Count;
            double Size = Math.Min(TargetItemGridFindCount.ActualWidth / correctImages.Count - 10, TargetItemGridFindCount.ActualHeight - 10);//-10是为了留点裕量，保证显示完全
            foreach (var img in correctImages)
            {
                string imageUri = ((BitmapImage)img.Source).UriSource.ToString();                // 获取图片的URI
                if (displayedImages.Contains(imageUri)) continue;                // 如果图片已经显示过，则跳过
                displayedImages.Add(imageUri);                // 添加到HashSet中，防止重复显示
                Image correctImg = new Image
                {                // 在下方面板中显示一次该图片
                    Source = img.Source,
                    Width = Size,
                    Height = Size,
                    Margin = new Thickness(10),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                // 创建边框，告诉患者现在是对谁在计数
                Border border = new Border
                {
                    BorderThickness = new Thickness(2),
                    BorderBrush = Brushes.Transparent, // 初始时无边框
                    Child = img,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                CountBordersList.Add(border);

                TargetItemGridFindCount.Children.Add(border);
            }
            UpdateTargetItemBorder();//并随手更新边框
        }

        private void UpdateTargetItemBorder()//根据现在的index来判断患者目前正在对第几个东西进行计数
        {
            foreach (Border border in CountBordersList)
            {
                if (IndexOfCount == CountBordersList.IndexOf(border))
                {//当前正在数的东西，边框变色
                    border.BorderBrush = Brushes.Blue;
                }
                else
                {//如果不是则保持透明
                    border.BorderBrush = Brushes.Transparent;
                }
            }
        }

        // 数字按钮按下事件处理函数
        private void OnNumberButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                int number = int.Parse(button.Content.ToString());
                userInput += button.Content.ToString();
                UpdateTextBlock();
            }
        }

        // "✔" 按钮按下事件处理函数
        private async void OnSubmitButtonClick(object sender, RoutedEventArgs e)
        {
            //先把用户输入解析成int
            int UserInputCount = -1;
            if (!string.IsNullOrEmpty(userInput))//清空显示
            {
                // 使用 TryParse 来安全地解析整数，避免异常
                if (int.TryParse(userInput, out UserInputCount))
                {
                    UserInputCount = int.Parse(userInput);
                }
                else
                {
                    // 处理解析失败的情况（例如，输入的不是有效数字）
                    UserInputCount = -1; // 或者设置一个默认值
                }
                userInput = string.Empty;
                UpdateTextBlock();
            }
            if (IndexOfCount <= CountBordersList.Count - 1 && IndexOfCount >= 0)//只有索引在这个范围内才去判断
            {
                Image CountingObject = CountBordersList[IndexOfCount].Child as Image;//找出用户在数哪个对象的数量
                bool isCorrect = (correctImagesCount[CountingObject] == UserInputCount);//判断是否数对
                if (isCorrect)
                {
                    IndexOfCount++;
                    correctAnswers[Level] += 1;
                    if (IndexOfCount >= CountBordersList.Count)
                    {//能做到这里说明整道题都算对
                        GroupResultCheck(isCorrect);
                        return;//提前return是为了防止重复反馈正确
                    }
                    myCanvas.IsEnabled = false;
                    int PauseDurations = 1000;//暂停的间隔
                    if (IfAudioFeedBack) PlayWav(CorrectSoundPath);
                    if (IfVisualFeedBack) ShowFeedbackImage(CorrectImage, PauseDurations);
                    await Task.Delay(PauseDurations);//延迟一秒奖励一下
                    myCanvas.IsEnabled = true;
                    UpdateTargetItemBorder();//更新边框，奖励完了才更新
                }
                else
                {
                    ErrorCount++;
                    if (ErrorCount >= ErrorLimit)
                    {//整道题都算错
                        GroupResultCheck(isCorrect);
                    }
                    else
                    {//只是第一次答错，可以再答一次
                        myCanvas.IsEnabled = false;
                        int PauseDurations = 1000;//暂停的间隔
                        if (IfAudioFeedBack) PlayWav(ErrorSoundPath);
                        if (IfVisualFeedBack) ShowFeedbackImage(ErrorImage, PauseDurations);
                        await Task.Delay(PauseDurations);//延迟一秒反省一下
                        myCanvas.IsEnabled = true;
                    }

                }
            }
        }

        private void GroupResultCheck(bool isCorrect)//对整道题进行判断
        {//包含数错两次一个东西，算整道题错；所有东西都数对，算整道题对
            if(isCorrect)
            {
                success_time++;
                if (IfAudioFeedBack) PlayWav(CorrectSoundPath);
                if (IfVisualFeedBack) ShowFeedbackImage(CorrectImage, CorrectInterval * 1000);
                IntervalTimer.Interval = TimeSpan.FromSeconds(CorrectInterval);
            }
            else
            {
                fail_time++;
                if (IfAudioFeedBack) PlayWav(ErrorSoundPath);
                if (IfVisualFeedBack) ShowFeedbackImage(ErrorImage, ErrorInterval * 1000);
                IntervalTimer.Interval = TimeSpan.FromSeconds(ErrorInterval);
                CountImageBorderChange();
                //int ignoreCount = correctImages.Count(ci => !selectedImages.Any(si => ((BitmapImage)si.Source).UriSource == ((BitmapImage)ci.Source).UriSource));
                //ignoreAnswers[Level] += ignoreCount;
                //不进行遗漏的计算，因为它有可能数多了也有可能数少了
                is_finish = false;
            }

            ResultCheck();
            AdjustFindCountModeDifficulty();

            //接下来等计时器触发了才能开始下一句游戏，同时在倒计时期间不能触发任何组件
            if (IfLimitTime)
            {
                PlayTime = BaseTimeLimit + (Level - MinLevel) * TimeLimitInterval;//计算好时间限制
                PlayTimer.Stop();//作答时间限制计时器停止
            }
            myCanvas.IsEnabled = false;
            IntervalTimer.Start();
        }

        private void CountImageBorderChange()//在答错后需要将那些数漏的border变黄
        {
            Image CountingObject = CountBordersList[IndexOfCount].Child as Image;//找出用户在数哪个对象的数量
            foreach(var (key,value) in ImageToBordersDict)
            {
                if(((BitmapImage)CountingObject.Source).UriSource == ((BitmapImage)key.Source).UriSource)
                {//如果他们的Source来源dou一样
                    value.BorderBrush = Brushes.Orange;
                }
            }
        }

        private void AdjustFindCountModeDifficulty()//调整FindCount模式的难度等级
        {
            if (IfLevelUp && Level <= MaxLevel)//如果足够升级等级
            {
                if (Level < MaxLevel) { Level += 1; }
                IfLevelUp = false; ResultInit();//每次升降等级都得清空重新计算
            }
            else if (IfLevelDown && Level >= MinLevel)
            {
                if (Level > MinLevel) { Level -= 1; }
                IfLevelDown = false; ResultInit();
            }
            FindCountModeLevelCheck(Level);//跟据Level值来调整参数，确保参数生效
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
        {//为什么不跟userInput绑定呢
            displayTextBlock.Text = userInput;
        }

        private void FindCountModeLevelCheck(int level)
        {
            switch (level)
            {
                case 1:
                    right_picture_number = 1; // 要计数对象的种类
                    max_right_display = 5;
                    min_right_display = 2;
                    mislead_picture_display_number = 4; // 不相关物品种类
                    break;
                case 2:
                    right_picture_number = 1;
                    max_right_display = 6;
                    min_right_display = 3;
                    mislead_picture_display_number = 5;
                    break;
                case 3:
                    right_picture_number = 1;
                    max_right_display = 7;
                    min_right_display = 4;
                    mislead_picture_display_number = 6;
                    break;
                case 4:
                    right_picture_number = 1;
                    max_right_display = 8;
                    min_right_display = 5;
                    mislead_picture_display_number = 7;
                    break;
                case 5:
                    right_picture_number = 1;
                    max_right_display = 10;
                    min_right_display = 6;
                    mislead_picture_display_number = 8;
                    break;
                case 6:
                    right_picture_number = 1;
                    max_right_display = 7;
                    min_right_display = 4;
                    mislead_picture_display_number = 6;
                    break;
                case 7:
                    right_picture_number = 2;
                    max_right_display = 8;
                    min_right_display = 5;
                    mislead_picture_display_number = 7;
                    break;
                case 8:
                    right_picture_number = 2;
                    max_right_display = 10;
                    min_right_display = 3;
                    mislead_picture_display_number = 8;
                    break;
                case 9:
                    right_picture_number = 2;
                    max_right_display = 10;
                    min_right_display = 3;
                    mislead_picture_display_number = 9;
                    break;
                case 10:
                    right_picture_number = 2;
                    max_right_display = 10;
                    min_right_display = 3;
                    mislead_picture_display_number = 10;
                    break;
                case 11:
                    right_picture_number = 3;
                    max_right_display = 10;
                    min_right_display = 3;
                    mislead_picture_display_number = 7;
                    break;
                case 12:
                    right_picture_number = 3;
                    max_right_display = 10;
                    min_right_display = 3;
                    mislead_picture_display_number = 8;
                    break;
                case 13:
                    right_picture_number = 3;
                    max_right_display = 10;
                    min_right_display = 3;
                    mislead_picture_display_number = 9;
                    break;
                case 14:
                    right_picture_number = 3;
                    max_right_display = 10;
                    min_right_display = 3;
                    mislead_picture_display_number = 10;
                    break;
                case 15:
                    right_picture_number = 3;
                    max_right_display = 10;
                    min_right_display = 3;
                    mislead_picture_display_number = 11;
                    break;
                case 16:
                    right_picture_number = 3;
                    max_right_display = 10;
                    min_right_display = 3;
                    mislead_picture_display_number = 15;
                    break;
                case 17:
                    right_picture_number = 3;
                    max_right_display = 10;
                    min_right_display = 3;
                    mislead_picture_display_number = 20;
                    break;
                case 18:
                    right_picture_number = 3;
                    max_right_display = 10;
                    min_right_display = 3;
                    mislead_picture_display_number = 25;
                    break;
                default:
                    right_picture_number = 1;
                    max_right_display = 5;
                    min_right_display = 2;
                    mislead_picture_display_number = 4;
                    break;
            }
        }

        /*共用*/
        private void GameTimer_Tick(object sender, EventArgs e)//这个定时器是用来看还剩下多少时间，整的游戏时间
        {
            // 每秒减少剩余时间
            if (timeRemaining > TimeSpan.Zero)
            {
                timeRemaining = timeRemaining.Add(TimeSpan.FromSeconds(-1));
                int remainingSeconds = (int)timeRemaining.TotalSeconds;

                TimeStatisticsAction.Invoke(remainingSeconds, PlayTime);
                // 调用委托
                LevelStatisticsAction?.Invoke(Level, MaxLevel);
                RightStatisticsAction?.Invoke(success_time, LevelUp);
                WrongStatisticsAction?.Invoke(fail_time, LevelDown);
            }
            else
            {
                gameTimer.Stop(); // 停止计时器

                OnGameEnd();
            }
        }

        private void ResultInit()//每次升降难度后都得把临时记录的结果置零
        {
            success_time = 0;
            fail_time = 0;
            if (train_mode == FindAndCountMode) ErrorCount = 0;
        }

        private void ResultCheck()//判断累计做对几道题，需不需要升降等级
        {
            if (success_time >= LevelUp) { IfLevelUp = true; }
            if (fail_time >= LevelDown) { IfLevelDown = true; }
        }

        private void ResetGameState()//重置游戏状态
        {
            if(train_mode==FindMode)
            {
                // 清除用户选择的图片
                selectedImages?.Clear();
                // 清除正确的图片列表
                correctImages?.Clear();

                SelectionCanvas.Children.Clear();//清除背景大图上所出现的一些可选项
                TargetItemGrid.Children.Clear();//清除待选择的那些图片

            }
            else if(train_mode==FindAndCountMode)
            {
                // 清除用户选择的图片
                selectedImages?.Clear();
                // 清除正确的图片列表
                correctImages?.Clear();

                CountBordersList.Clear();
                ImageToBordersDict.Clear();

                FindCountCanvas.Children.Clear();
                TargetItemGridFindCount.Children.Clear();
            }
        }

        private void InitializeGame()
        {
            // 在开始新游戏前重置状态
            if (train_mode == FindMode)
            {
                FindPatternGrid.Visibility = Visibility.Visible;//显示对应的组件
                FindAndCountPatternGrid.Visibility = Visibility.Collapsed;
                FindModeLevelCheck(Level);
                VoiceTipAction?.Invoke("请找出屏幕下部的对象在上方图片中的位置，并用鼠标点击以选择出来。");
                RuleAction?.Invoke("请找出屏幕下部的对象在上方图片中的位置，并用鼠标点击以选择出来。");
                SetupFindMode();
            }
            else if (train_mode == FindAndCountMode)
            {
                FindAndCountPatternGrid.Visibility = Visibility.Visible;//显示对应的组件
                FindPatternGrid.Visibility = Visibility.Collapsed;
                FindCountModeLevelCheck(Level);
                VoiceTipAction?.Invoke("请数出下方图片对象在上方图片中出现的次数并在右侧键盘输入");
                RuleAction?.Invoke("请数出下方图片对象在上方图片中出现的次数并在右侧键盘输入");
                SetupFindCountMode();
            }
        }

        private void PlayTimer_Tick(object sender, EventArgs e)//作答时间限制
        {
            PlayTime--;
            if (PlayTime <= 0)//时间已到
            {
                //开始下一轮答题了,本题算错
                fail_time++;
                wrongAnswers[Level] += 1;
                IntervalTimer.Interval = TimeSpan.FromSeconds(ErrorInterval);
                if (IfAudioFeedBack) PlayWav(ErrorSoundPath);
                if (IfVisualFeedBack) ShowFeedbackImage(ErrorImage, ErrorInterval * 1000);

                ResultCheck();// 判断完了就该检查结果
                if(train_mode==FindMode)
                {
                    AdjustFindModeDifficulty(); //看看是否需要调整游戏难度
                    FoundButton.IsEnabled = false;
                }
                else if(train_mode == FindAndCountMode)
                {
                    AdjustFindCountModeDifficulty();
                    myCanvas.IsEnabled = false;
                }
                //接下来等计时器触发了才能开始下一句游戏，同时在倒计时期间不能触发任何组件

                IntervalTimer.Start();
                PlayTime = BaseTimeLimit + (Level - MinLevel) * TimeLimitInterval;//计算好时间限制
                PlayTimer.Stop();//作答时间限制计时器停止
            }
        }

        private void IntervalTimer_Tick(object sender, EventArgs e)//答题间隔计时器
        {// 这个触发了才能进入到下一个题目
            if(train_mode == FindMode)
            {
                FoundButton.IsEnabled = true;
            } 
            if(train_mode == FindAndCountMode)
            {
                myCanvas.IsEnabled = true;
                ErrorCount = 0;//重置错误次数
            }
            if(IfLimitTime)
            {
                PlayTimer.Start();//开始作答限制计时器
            }

            IntervalTimer.Stop();//停止计时器

            //更新游戏状态用这个函数来更新
            ResetGameState(); // 重置游戏状态
            InitializeGame(); // 开始新游戏
        }


        /*LJN
 添加进来视觉、声音反馈的资源
 */
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

        private async void ShowFeedbackImage(Image image, int StopDurations=2000)//StopDurations单位是ms
        {//显示反馈的图片
            image.Visibility = Visibility.Visible;

            // 延迟指定的时间（例如1秒）
            await Task.Delay(StopDurations);

            image.Visibility = Visibility.Collapsed;
        }


    }
    public partial class 搜索能力3 : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
            correctAnswers = new int[MaxLevel + 1];
            wrongAnswers = new int[MaxLevel + 1];
            ignoreAnswers = new int[MaxLevel + 1];
            IfLevelDown = false; IfLevelUp = false;

            max_time = 1; // 窗口总的持续时间，单位分钟
            train_mode = 3; // 游戏模式，1、2或3
            Level = 1; // 当前游戏难度等级

        //读取数据库
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
                            case 304: // 治疗时间
                                max_time = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"max_time={max_time}");
                                break;
                            case 303: // 游戏模式
                                train_mode = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"train_mode ={train_mode}");
                                break;
                            case 310: //游戏等级
                                Level = par.Value.HasValue ? (int)par.Value.Value : 1;
                                Debug.WriteLine($"level ={Level}");
                                break;
                            case 308://听觉反馈
                                IfAudioFeedBack = par.Value == 1;
                                break;
                            case 307://视觉反馈
                                IfVisualFeedBack = par.Value == 1;
                                break;
                            case 309://限制作答时间
                                IfLimitTime = par.Value == 1;
                                break;
                            case 305://提高等级
                                LevelUp = par.Value.HasValue ? (int)par.Value.Value : 5;
                                break;
                            case 306://等级降低
                                LevelDown = par.Value.HasValue ? (int)par.Value.Value : 5;
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

            // 获取当前显示器的 DPI
            screenDpiHorizontal = GetDpiHorizontal();  // 水平方向的 DPI
            screenDpiVertical = GetDpiVertical();      // 垂直方向的 DPI
        }

        protected override async Task OnStartAsync()
        {
            /*LJN
            配置反馈资源的路径             
             */
            CorrectSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Correct.wav");
            ErrorSoundPath = System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", $"Error.wav");
            // 为 Image 控件加载图片 Source
            CorrectImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Correct.png"), UriKind.RelativeOrAbsolute));
            ErrorImage.Source = new BitmapImage(new Uri(System.IO.Path.Combine(BaseDirectory, ResourcesPath, "Effects", "Error.png"), UriKind.RelativeOrAbsolute));


            gameTimer.Start(); // 开始计时

            ResetGameState(); // 重置游戏状态
            InitializeGame(); // 开始新游戏

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
            gameTimer.Stop(); // 停止计时器
            PlayTimer.Stop();
            IntervalTimer.Stop();
            // 调整难度
            ResetGameState(); // 重置游戏状态
            InitializeGame(); // 开始新游戏
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {//因为是先把本体给init了，所以此时train_mode的值已经是读取到了，可以传递过去
            return new 搜索能力3讲解(train_mode);
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
                        int rep = repet_time;
                        int totalCount = wrongCount * (rep + 1);
                        int Count = totalCount + correctCount;
                        double accuracy = Math.Round((double)correctCount / (double)Count, 2);

                        // 创建 Result 记录
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "搜索能力3",
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
                                    ValueName = "模式",
                                    Value = mode,
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
                                  new ResultDetail
                                {
                                    ResultId = result_id,
                                    Order = 1,
                                    ValueName = "总机会数",
                                    Value = totalCount,
                                    ModuleId = BaseParameter.ModuleId
                                },
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

            //using (var transaction = await db.Database.BeginTransactionAsync())
            //{
            //    try
            //    {
            //        await Task.Run(async () =>
            //        {
            //            for (int lv = 1; lv <= Level; lv++)
            //            {
            //                // 获取当前难度级别的数据
            //                int correctCount = GetCorrectNum(lv);
            //                int wrongCount = GetWrongNum(lv);
            //                int mode = train_mode;
            //                int rep = repet_time;
            //                int totalCount = wrongCount * (rep + 1);
            //                int Count = totalCount + correctCount;
            //                if (correctCount == 0 && wrongCount == 0)
            //                {
            //                    // 如果所有数据都为0，跳过此难度级别
            //                    Debug.WriteLine($"难度级别 {lv}: 没有数据，跳过.");
            //                    continue;
            //                }
            //                // 计算准确率
            //                double accuracy = Math.Round((double)correctCount / (double)Count, 2);
            //                // 创建 Result 记录
            //                var newResult = new Result
            //                {
            //                    ProgramId = program_id, // program_id
            //                    Report = "搜索能力2",
            //                    Eval = false,
            //                    Lv = lv, // 当前的难度级别
            //                    ScheduleId = BaseParameter.ScheduleId ?? null// 假设的 Schedule_id，可以替换为实际值
            //                };
            //                db.Results.Add(newResult);
            //                await db.SaveChangesAsync(); //这里注释了
            //                // 获得 result_id
            //                int result_id = newResult.ResultId;
            //                // 创建 ResultDetail 对象列表
            //                var resultDetails = new List<ResultDetail>
            //                {
            //                   new ResultDetail
            //                    {
            //                        ResultId = result_id,
            //                        ValueName = "等级",
            //                        Value = lv,
            //                        ModuleId = BaseParameter.ModuleId
            //                    },
            //                    new ResultDetail
            //                    {
            //                        ResultId = result_id,
            //                        ValueName = "目录",
            //                        Value = mode,
            //                        ModuleId = BaseParameter.ModuleId
            //                    },
            //                     new ResultDetail
            //                    {
            //                        ResultId = result_id,
            //                        ValueName = "正确率",
            //                        Value = accuracy * 100, // 以百分比形式存储
            //                        ModuleId =  BaseParameter.ModuleId
            //                    },
            //                      new ResultDetail
            //                    {
            //                        ResultId = result_id,
            //                        ValueName = "总机会数",
            //                        Value = totalCount,
            //                        ModuleId = BaseParameter.ModuleId
            //                    },
            //                       new ResultDetail
            //                    {
            //                        ResultId = result_id,
            //                        ValueName = "已使用机会数",
            //                        Value = Count,
            //                        ModuleId = BaseParameter.ModuleId
            //                    },
            //                    new ResultDetail
            //                    {
            //                        ResultId = result_id,
            //                        ValueName = "正确次数",
            //                        Value = correctCount,
            //                        ModuleId = BaseParameter.ModuleId
            //                    },
            //                    new ResultDetail
            //                    {
            //                        ResultId = result_id,
            //                        ValueName = "错误次数",
            //                        Value = wrongCount,
            //                        ModuleId =  BaseParameter.ModuleId
            //                    }
            //                };
            //                // 插入 ResultDetail 数据
            //                db.ResultDetails.AddRange(resultDetails);
            //                await db.SaveChangesAsync();
            //                // 输出每个 ResultDetail 对象的数据
            //                Debug.WriteLine($"难度级别 {lv}:");
            //                foreach (var detail in resultDetails)
            //                {
            //                    Debug.WriteLine($"    {detail.ValueName}: {detail.Value}, ModuleId: {detail.ModuleId} ");
            //                }
            //            }
            //            // 提交事务
            //            await transaction.CommitAsync();
            //            Debug.WriteLine("插入成功");
            //        });
            //    }
            //    catch (Exception ex)
            //    {
            //        // 回滚事务
            //        await transaction.RollbackAsync();
            //        Debug.WriteLine(ex.ToString());
            //    }
            //}
        }

    }

    public partial class 搜索能力3
    {
        /*
         Json文件读取
         */
        // 定义类来对应 JSON 结构
        public class ImageData
        {
            public BackgroundImageObject BackgroundImage { get; set; }
            public List<ImagesObject> Images { get; set; }
        }

        public class BackgroundImageObject
        {
            public string Path { get; set; }
            public Position PxPosition { get; set; }
            public SizeObject Size { get; set; }
            public string Info { get; set; }
        }

        public class ImagesObject
        {
            public string Path { get; set; }
            public Position PxMargin { get; set; }
            public SizeObject Size { get; set; }
            public string Info { get; set; }
            public int Index { get; set; }
        }

        public class Position
        {//相当于margin，但是是以px为单位
            public double Left { get; set; }
            public double Up { get; set; }
            public double Right { get; set; }
            public double Down { get; set; }
        }

        public class SizeObject
        {//以px为单位
            public double Width { get; set; }
            public double Height { get; set; }

        }

        public class JsonExample
        {
            public static ImageData ReadJson(string filePath)
            {
                // 读取 JSON 文件内容
                string json = File.ReadAllText(filePath);

                // 将 JSON 字符串反序列化为 ImageData 对象
                ImageData imageData = JsonConvert.DeserializeObject<ImageData>(json);

                return imageData;
            }

            public static void WriteJson(string filePath, ImageData imageData)
            {
                // 将 ImageData 对象序列化为 JSON 字符串
                string json = JsonConvert.SerializeObject(imageData, Formatting.Indented);

                // 将 JSON 字符串写入文件
                File.WriteAllText(filePath, json);
            }
        }

        private ImageData imageData;//用来存储数据
        private double screenDpiHorizontal;  // 水平方向的 DPI
        private double screenDpiVertical;      // 垂直方向的 DPI
        public double GetDpiHorizontal()
        {
            var visual = (Visual)Application.Current.MainWindow;
            var transform = PresentationSource.FromVisual(visual).CompositionTarget.TransformToDevice;
            return transform.M11 * 96; // M11 是水平方向的缩放因子
        }

        public double GetDpiVertical()
        {
            var visual = (Visual)Application.Current.MainWindow;
            var transform = PresentationSource.FromVisual(visual).CompositionTarget.TransformToDevice;
            return transform.M22 * 96; // M22 是垂直方向的缩放因子
        }

        private void GetItemsReady()//为Find模式准备素材
        {
            //再准备题目素材
            string jsonFolderPath = @$"Games/EXO/PicMaterial/{ComplexityFolder}/Json";
            string[] filePaths = Directory.GetFiles(jsonFolderPath);
            var jsonList = filePaths.Select(filePath => System.IO.Path.GetFileName(filePath)).ToList();
            Random rand = new Random();
            string jsonName = jsonList.OrderBy(x => rand.Next()).FirstOrDefault();//随机排列并选出第一个
            // 文件路径
            string filePath = @$"Games/EXO/PicMaterial/{ComplexityFolder}/Json/{jsonName}";

            // 读取 JSON 文件
            imageData = JsonExample.ReadJson(filePath);
            imagePaths[FindItem].Clear();
            foreach (ImagesObject ImageObject in imageData.Images)
            {
                // 也可以逐个元素地添加路径到数组中的某一组
                imagePaths[FindItem].Add($"EXO/PicMaterial/{ComplexityFolder}/Pic/{System.IO.Path.GetFileNameWithoutExtension(jsonName)}/{ImageObject.Path}");  // 向第一组添加元素
            }
            BackGroundPath = $"EXO/PicMaterial/{ComplexityFolder}/Pic/{System.IO.Path.GetFileNameWithoutExtension(jsonName)}/{imageData.BackgroundImage.Path}";
            imagePaths[FindItem].Add(BackGroundPath);//把背景加上

        }
    } 
}
