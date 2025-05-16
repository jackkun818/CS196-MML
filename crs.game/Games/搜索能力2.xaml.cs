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
using HandyControl.Controls;
using Window = System.Windows.Window;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace crs.game.Games
{
    /// <summary>
    /// EXO.xaml 的交互逻辑
    /// </summary>
    public partial class 搜索能力2 : BaseUserControl
    {
        private readonly string[][][] imagePaths = new string[][][]
        {
            new string[][]//有色图片路径
            {
                new string[]//有色，复杂图形   
                {
                    "Games/EXO/color/Figure/太阳_colored.png","Games/EXO/color/Figure/爆炸形_colored.png","Games/EXO/color/Figure/爆炸圆形_colored.png","Games/EXO/color/Figure/波浪形_colored.png","Games/EXO/color/Figure/称号形_colored.png","Games/EXO/color/Figure/肥十字_colored.png","Games/EXO/color/Figure/花型_colored.png","Games/EXO/color/Figure/禁止_colored.png","Games/EXO/color/Figure/闪电_colored.png","Games/EXO/color/Figure/四分之三圆形_colored.png","Games/EXO/color/Figure/心形_colored.png","Games/EXO/color/Figure/云形_colored.png",
                },
                new string[]//有色，几何简单
                {
                    "Games/EXO/color/Geometry/五边形_colored.png","Games/EXO/color/Geometry/五角星_colored.png","Games/EXO/color/Geometry/下箭头_colored.png","Games/EXO/color/Geometry/右箭头_colored.png","Games/EXO/color/Geometry/L形_colored.png","Games/EXO/color/Geometry/八边形_colored.png","Games/EXO/color/Geometry/六边形_colored.png","Games/EXO/color/Geometry/七边形_colored.png","Games/EXO/color/Geometry/切割圆形_colored.png","Games/EXO/color/Geometry/十边形_colored.png","Games/EXO/color/Geometry/右V箭头_colored.png","Games/EXO/color/Geometry/水滴_colored.png",
                },
                new string[]//有色，混合
                {
                    "Games/EXO/color/Figure/爆炸形_colored.png","Games/EXO/color/Figure/爆炸圆形_colored.png","Games/EXO/color/Figure/波浪形_colored.png","Games/EXO/color/Figure/称号形_colored.png","Games/EXO/color/Figure/肥十字_colored.png","Games/EXO/color/Figure/花型_colored.png","Games/EXO/color/Figure/禁止_colored.png","Games/EXO/color/Figure/闪电_colored.png","Games/EXO/color/Figure/四分之三圆形_colored.png","Games/EXO/color/Figure/心形_colored.png","Games/EXO/color/Figure/云形_colored.png",
                    "Games/EXO/color/Figure/太阳_colored.png","Games/EXO/color/Geometry/五边形_colored.png","Games/EXO/color/Geometry/五角星_colored.png","Games/EXO/color/Geometry/下箭头_colored.png","Games/EXO/color/Geometry/右箭头_colored.png","Games/EXO/color/Geometry/L形_colored.png","Games/EXO/color/Geometry/八边形_colored.png","Games/EXO/color/Geometry/六边形_colored.png","Games/EXO/color/Geometry/七边形_colored.png","Games/EXO/color/Geometry/切割圆形_colored.png","Games/EXO/color/Geometry/十边形_colored.png","Games/EXO/color/Geometry/右V箭头_colored.png","Games/EXO/color/Geometry/水滴_colored.png",
                }
            },
            new string[][]//这是黑色图片的路径
            {
                new string[]//黑色，复杂图形
                {
                    "Games/EXO/black/Figure/太阳_black.png","Games/EXO/black/Figure/爆炸形_black.png","Games/EXO/black/Figure/爆炸圆形_black.png","Games/EXO/black/Figure/波浪形_black.png","Games/EXO/black/Figure/称号形_black.png","Games/EXO/black/Figure/肥十字_black.png","Games/EXO/black/Figure/花型_black.png","Games/EXO/black/Figure/禁止_black.png","Games/EXO/black/Figure/闪电_black.png","Games/EXO/black/Figure/四分之三圆形_black.png","Games/EXO/black/Figure/心形_black.png","Games/EXO/black/Figure/云形_black.png",
                },
                new string[]//黑色，几何简单
                {
                    "Games/EXO/black/Geometry/五角星_black.png","Games/EXO/black/Geometry/下箭头_black.png","Games/EXO/black/Geometry/右箭头_black.png","Games/EXO/black/Geometry/L形_black.png","Games/EXO/black/Geometry/八边形_black.png","Games/EXO/black/Geometry/六边形_black.png","Games/EXO/black/Geometry/七边形_black.png","Games/EXO/black/Geometry/切割圆形_black.png","Games/EXO/black/Geometry/十边形_black.png","Games/EXO/black/Geometry/右V箭头_black.png","Games/EXO/black/Geometry/水滴_black.png","Games/EXO/black/Geometry/五边形_black.png",
                },
                new string[]//黑色，混合
                {
                    "Games/EXO/black/Figure/爆炸形_black.png","Games/EXO/black/Figure/爆炸圆形_black.png","Games/EXO/black/Figure/波浪形_black.png","Games/EXO/black/Figure/称号形_black.png","Games/EXO/black/Figure/肥十字_black.png","Games/EXO/black/Figure/花型_black.png","Games/EXO/black/Figure/禁止_black.png","Games/EXO/black/Figure/闪电_black.png","Games/EXO/black/Figure/四分之三圆形_black.png","Games/EXO/black/Figure/心形_black.png","Games/EXO/black/Figure/云形_black.png",
                    "Games/EXO/black/Figure/太阳_black.png","Games/EXO/black/Geometry/五角星_black.png","Games/EXO/black/Geometry/下箭头_black.png","Games/EXO/black/Geometry/右箭头_black.png","Games/EXO/black/Geometry/L形_black.png","Games/EXO/black/Geometry/八边形_black.png","Games/EXO/black/Geometry/六边形_black.png","Games/EXO/black/Geometry/七边形_black.png","Games/EXO/black/Geometry/切割圆形_black.png","Games/EXO/black/Geometry/十边形_black.png","Games/EXO/black/Geometry/右V箭头_black.png","Games/EXO/black/Geometry/水滴_black.png","Games/EXO/black/Geometry/五边形_black.png",
                }
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

        private List<Image> correctImages; // 正确图片的列表
        private List<Image> selectedImages; // 用户选择的图片
        private int right_picture_number = 4; // 显示的正确图片数量,即用来重叠的图片数量
        private int chose_picture_number = 6; // 显示的可选择图片数量

        private int success_time = 0;//答对多少题
        private int fail_time = 0;//答错多少题
        private bool IfLevelDown = false;//是否需要降等级标志位
        private bool IfLevelUp = false;//是否需要升等级标志位

        private int[] correctAnswers;//存储每个等级的结果数组
        private int[] wrongAnswers;
        private int[] ignoreAnswers;

        private int ColorMode = 0;//色彩模式，0有色，1黑色，跟上面存储的路径有关
        private int EasyOrHard = 0;//图像的简单容易程度，0复杂图形，1简单几何图形，2混合图像，跟上面存储的路径有关
        
        private Dictionary<Border, bool> BorderStatusDict = new Dictionary<Border, bool>();

        private Dictionary<int, int[]> SelectImageGridStructDict = new Dictionary<int, int[]>//用来指定下面待选框有几列几行 
        {//可选择的形状数量，(行，列)
                { 3, new int[] {1,3} },
                { 6, new int[] {2,3} },
                { 8, new int[] {2,4} },
                {4*3,new int[] {3,4} },
                {6*3,new int[] {3,6} }
        };

        private double MainGridMaxHeight = 828;//MainGrid 的目测最高是多少
        private double OverLayImageGridHeight = 0;
        private double SelectImageGridHeight = 0;//这两行的高度分别为多少，给出个指定值方便后面计算
        public 搜索能力2()
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

        private void GameTimer_Tick(object sender, EventArgs e)//这个定时器是用来看还剩下多少时间，整的游戏时间
        {
            // 每秒减少剩余时间
            if (timeRemaining > TimeSpan.Zero)
            {
                timeRemaining = timeRemaining.Add(TimeSpan.FromSeconds(-1));
                int remainingSeconds = (int)timeRemaining.TotalSeconds;

                TimeStatisticsAction.Invoke(remainingSeconds, PlayTime);
                WrongStatisticsAction?.Invoke(fail_time, LevelDown);
                RightStatisticsAction?.Invoke(success_time, LevelUp);
            }
            else
            {
                gameTimer.Stop(); // 停止计时器
                if (IfLimitTime) { PlayTimer.Stop(); }
                IntervalTimer.Stop();
                OnGameEnd();
            }
        }

        private void InitializeGame()
        {
            //ResetGameState(); // 在开始新游戏前重置状态
            VoiceTipAction?.Invoke("请识别出叠加在一起的不同形状，并将它们从屏幕下部选择出来。");
            RuleAction?.Invoke("识别出叠加在一起的不同形状，并将它们从屏幕下部选择出来。");
            SetupGameMode2();
        }

        private void SetupGameMode2()
        {
            LevelCheck();
            if (IfLimitTime)
            {//如果限制作答时间
                PlayTime = BaseTimeLimit + (Level - MinLevel) * TimeLimitInterval;//计算好时间限制
                PlayTimer.Start();
            }
            confirm.Visibility = Visibility.Visible;
            //Panel.SetZIndex(confirm, 999); // 999 是一个比较大的值，确保在最上层
            // 显示叠加的正确图片
            correctImages = new List<Image>();
            selectedImages = new List<Image>();
            AdjustMainGrid();
            DisplayOverlayImages();
            // 显示可供选择的图片
            DisplayChoiceImages();
        }

        private void AdjustMainGrid()//根据需要显示的元素数量来动态改变行高分布
        {
            switch (SelectImageGridStructDict[chose_picture_number][0])
            {//获取需要显示的行数
                case 1:
                    MainGrid.RowDefinitions[0].Height = new GridLength(6.5, GridUnitType.Star);
                    MainGrid.RowDefinitions[1].Height = new GridLength(3.5, GridUnitType.Star);
                    OverLayImageGridHeight = 0.7 * MainGridMaxHeight;
                    SelectImageGridHeight = 0.3 * MainGridMaxHeight;
                    break;
                case 2:
                    MainGrid.RowDefinitions[0].Height = new GridLength(6, GridUnitType.Star);
                    MainGrid.RowDefinitions[1].Height = new GridLength(4, GridUnitType.Star);
                    OverLayImageGridHeight = 0.6 * MainGridMaxHeight;
                    SelectImageGridHeight = 0.4 * MainGridMaxHeight;
                    break;
                case 3:
                    MainGrid.RowDefinitions[0].Height = new GridLength(5, GridUnitType.Star);
                    MainGrid.RowDefinitions[1].Height = new GridLength(5, GridUnitType.Star);
                    OverLayImageGridHeight = 0.5 * MainGridMaxHeight;
                    SelectImageGridHeight = 0.5 * MainGridMaxHeight;
                    break;
                default:
                    MainGrid.RowDefinitions[0].Height = new GridLength(7, GridUnitType.Star);
                    MainGrid.RowDefinitions[1].Height = new GridLength(3, GridUnitType.Star);
                    OverLayImageGridHeight = 0.7 * MainGridMaxHeight;
                    SelectImageGridHeight = 0.3 * MainGridMaxHeight;
                    break;
            }

        }

        private void DisplayOverlayImages()
        {
            OverLayImageGrid.Children.Clear();//显示之前一定要清空
            Canvas overlayCanvas = new Canvas
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Width = OverLayImageGridHeight-10,  // 宽度可以根据需求调整
                Height = OverLayImageGridHeight-10  // 高度可以根据需求调整
            };

            Random rand = new Random();
            List<int> indices = Enumerable.Range(0, imagePaths[ColorMode][EasyOrHard].Length).OrderBy(x => rand.Next()).Take(right_picture_number).ToList();

            foreach (int index in indices)
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePaths[ColorMode][EasyOrHard][index], UriKind.Relative);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                Image img = new Image
                {
                    Source = bitmap,
                    Width = OverLayImageGridHeight-20,
                    Height = OverLayImageGridHeight-20,
                    //Opacity = 0.5, // 设置透明度
                    RenderTransformOrigin = new Point(0.5, 0.5)
                };

                //// 随机调整图片的位置和旋转角度，使其产生重叠效果
                //double left = rand.Next(50);
                //double top = rand.Next(50);

                //Canvas.SetLeft(img, left);
                //Canvas.SetTop(img, top);
                //不提供旋转和位置移动，直接中心重叠，所以需要形状不同才能区分
                correctImages.Add(img);  // 将正确图片添加到列表中
                overlayCanvas.Children.Add(img);
            }

            Grid.SetRow(overlayCanvas, 0);
            OverLayImageGrid.Children.Add(overlayCanvas);//向Grid里面加入元素
        }

        private void DisplayChoiceImages()
        {
            SelectImageGrid.Children.Clear();//显示之前一定要清空

            Random rand = new Random();
            List<int> indices = new List<int>();

            // 首先确保所有重叠显示的正确图片被添加到选择列表中
            foreach (var correctImg in correctImages)
            {
                // 这里直接获取正确图片的索引
                int correctIndex = Array.IndexOf(imagePaths[ColorMode][EasyOrHard], ((BitmapImage)correctImg.Source).UriSource.ToString().Replace("pack://application:,,,", ""));
                indices.Add(correctIndex);
            }

            // 填充剩余的选择图片，确保总数达到 chose_picture_number
            while (indices.Count < chose_picture_number)
            {
                int index = rand.Next(imagePaths[ColorMode][EasyOrHard].Length);
                if (!indices.Contains(index))
                {
                    indices.Add(index);
                }
            }

            // 随机化选择图片的顺序
            indices = indices.OrderBy(x => rand.Next()).ToList();

            // 初始化一个Grid，用来存放这些待选的图片
            Grid ChoiceGrid = new Grid();
            int Rows = SelectImageGridStructDict[chose_picture_number][0];
            int Columns = SelectImageGridStructDict[chose_picture_number][1];

            for (int i = 0; i < Rows; i++)
            {
                ChoiceGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }
            for (int j = 0; j < Columns;  j++)
            {
                ChoiceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            int NumOfImage = 0;//创建这个是方便用来计数，确定Image放在第几行第几列
            foreach (int index in indices)
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePaths[ColorMode][EasyOrHard][index], UriKind.Relative);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                Image img = new Image
                {
                    Source = bitmap,
                    Width = (1340 - 10) /(Columns),
                    Height = (SelectImageGridHeight - 10) /(Rows)-10,
                    Margin = new Thickness(2)
                };

                Border border = new Border
                {
                    BorderThickness = new Thickness(5),
                    BorderBrush = Brushes.Transparent, // 初始时无边框
                    Child = img
                };
                BorderStatusDict[border] = false;
                border.MouseLeftButtonDown += Border_MouseLeftButtonDown;//实现选中后框框变色

                // 将 Border 添加到 Grid 中
                Grid.SetRow(border, NumOfImage/ Columns);
                Grid.SetColumn(border, NumOfImage % Columns);
                NumOfImage++;
                ChoiceGrid.Children.Add(border);
            }
            SelectImageGrid.Children.Add(ChoiceGrid);
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

        private void confirmButton_Click2(object sender, RoutedEventArgs e)//点击了按钮才判断答对答错
        {
            //处理选中的图片的框框颜色显示
            foreach( var kvp in BorderStatusDict)
            {
                Border border = kvp.Key;
                border.MouseLeftButtonDown -= Border_MouseLeftButtonDown;//已经揭晓答案了就不要有变色机制了
                Image img = border.Child as Image;
                bool status = kvp.Value;
                if(status==true)//只针对那些选中过的图片来处理框框
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

            //答对答错逻辑
            bool isCorrect = (selectedImages.Count == correctImages.Count) &&
                        selectedImages.All(si => correctImages.Any(ci => ((BitmapImage)si.Source).UriSource == ((BitmapImage)ci.Source).UriSource));

            if (isCorrect)
            {
                success_time++;
                correctAnswers[Level] += 1;
                IntervalTimer.Interval = TimeSpan.FromSeconds(CorrectInterval);
                if (IfAudioFeedBack) PlayWav(CorrectSoundPath);
                if (IfVisualFeedBack) ShowFeedbackImage(CorrectImage,CorrectInterval*1000);
            }
            else
            {
                fail_time++;
                wrongAnswers[Level] += 1;
                IntervalTimer.Interval = TimeSpan.FromSeconds(ErrorInterval);
                if (IfAudioFeedBack) PlayWav(ErrorSoundPath);
                if (IfVisualFeedBack) ShowFeedbackImage(ErrorImage,ErrorInterval*1000);
                int ignoredCount = correctImages.Count(ci => !selectedImages.Any(si => ((BitmapImage)si.Source).UriSource == ((BitmapImage)ci.Source).UriSource));
                ignoreAnswers[Level] += ignoredCount;
            }
            ResultCheck();

            //看看是否需要调整游戏难度
            AdjustDifficulty();

            //接下来等计时器触发了才能开始下一句游戏，同时在倒计时期间不能触发任何组件
            if(IfLimitTime)
            {
                PlayTime = BaseTimeLimit + (Level - MinLevel) * TimeLimitInterval;//计算好时间限制
                PlayTimer.Stop();//作答时间限制计时器停止
            }
            confirm.IsEnabled = false;//用一个蒙层覆盖掉，用来屏蔽组件
            SelectImageGrid.IsEnabled = true;
            IntervalTimer.Start();
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
        }

        private void LevelCheck()//观察是否难度等级发生变化，如果有，则同步到游戏参数变化上
        {
            switch (Level)
            {
                case 1:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    ColorMode = 0;
                    EasyOrHard = 1;
                    break;
                case 2:
                    right_picture_number = 2;
                    chose_picture_number = 6;
                    ColorMode = 0;
                    EasyOrHard = 1;
                    break;
                case 3:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    ColorMode = 0;
                    EasyOrHard = 0;
                    break;
                case 4:
                    right_picture_number = 2;
                    chose_picture_number = 6;
                    ColorMode = 0;
                    EasyOrHard = 0;
                    break;
                case 5:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    ColorMode = 1;
                    EasyOrHard = 1;
                    break;
                case 6:
                    right_picture_number = 2;
                    chose_picture_number = 6;
                    ColorMode = 1;
                    EasyOrHard = 1;
                    break;
                case 7:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    ColorMode = 1;
                    EasyOrHard = 0;
                    break;
                case 8:
                    right_picture_number = 2;
                    chose_picture_number = 6;
                    ColorMode = 1;
                    EasyOrHard = 0;
                    break;
                case 9:
                    right_picture_number = 3;
                    chose_picture_number = 6;
                    ColorMode = 0;
                    EasyOrHard = 1;
                    break;
                case 10:
                    right_picture_number = 3;
                    chose_picture_number = 8;
                    ColorMode = 0;
                    EasyOrHard = 1;
                    break;
                case 11:
                    right_picture_number = 3;
                    chose_picture_number = 6;
                    ColorMode = 0;
                    EasyOrHard = 0;
                    break;
                case 12:
                    right_picture_number = 3;
                    chose_picture_number = 8;
                    ColorMode = 0;
                    EasyOrHard = 0;
                    break;
                case 13:
                    right_picture_number = 3;
                    chose_picture_number = 6;
                    ColorMode = 1;
                    EasyOrHard = 1;
                    break;
                case 14:
                    right_picture_number = 3;
                    chose_picture_number = 8;
                    ColorMode = 1;
                    EasyOrHard = 1;
                    break;
                case 15:
                    right_picture_number = 3;
                    chose_picture_number = 6;
                    ColorMode = 1;
                    EasyOrHard = 0;
                    break;
                case 16:
                    right_picture_number = 3;
                    chose_picture_number = 8;
                    ColorMode = 1;
                    EasyOrHard = 0;
                    break;
                case 17:
                    right_picture_number = 3;
                    chose_picture_number = 4*3;
                    ColorMode = 1;
                    EasyOrHard = 2;
                    break;
                case 18:
                    right_picture_number = 3;
                    chose_picture_number = 6*3;
                    ColorMode = 1;
                    EasyOrHard = 2;
                    break;
                default:
                    right_picture_number = 2;
                    chose_picture_number = 3;
                    ColorMode = 0; 
                    break;
            }
        }
        int max_hardness = 1;
        private void AdjustDifficulty()//模式1根据level值来调整对应的参数
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
            LevelCheck();
            LevelStatisticsAction?.Invoke(Level, MaxLevel);
        }

        private void ResetGameState()//重置游戏状态
        {
            // 清除用户选择的图片
            selectedImages?.Clear();
            // 清除正确的图片列表
            correctImages?.Clear();

            OverLayImageGrid.Children.Clear();
            SelectImageGrid.Children.Clear();

            // 重置 UI 组件的可见性
            confirm.Visibility = Visibility.Collapsed;
        }

        private void EndGame()
        {
            ResetGameState(); // 重置游戏状态
            InitializeGame(); // 开始新游戏
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
                ResultCheck();
                //看看是否需要调整游戏难度
                AdjustDifficulty();
                confirm.IsEnabled = false;//用一个蒙层覆盖掉，用来屏蔽组件
                SelectImageGrid.IsEnabled = true;
                IntervalTimer.Start();//强制进入到间隔计时器，从而跳过本题

                PlayTimer.Stop();
            }
        }

        private void IntervalTimer_Tick(object sender, EventArgs e)//答题间隔计时器
        {// 这个触发了才能进入到下一个题目
            confirm.IsEnabled = true;
            SelectImageGrid.IsEnabled = true;
            PlayTimer.Start();//开始作答限制计时器
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
    public partial class 搜索能力2 : BaseUserControl
    {
        protected override async Task OnInitAsync()
        {
            correctAnswers = new int[MaxLevel + 1];
            wrongAnswers = new int[MaxLevel + 1];
            ignoreAnswers = new int[MaxLevel + 1];
            IfLevelDown = false; IfLevelUp = false;

            max_time = 30; // 窗口总的持续时间，单位分钟
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
                                case 139: // 治疗时间
                                    max_time = par.Value.HasValue ? (int)par.Value.Value : 1;
                                    Debug.WriteLine($"max_time={max_time}");
                                    break;
                                case 177: //游戏等级
                                    Level = par.Value.HasValue ? (int)par.Value.Value : 1;
                                    Debug.WriteLine($"Level ={Level}");
                                    break;
                                case 268://听觉反馈
                                    IfAudioFeedBack = par.Value == 1;
                                    break;
                                case 269://视觉反馈
                                    IfVisualFeedBack = par.Value == 1;
                                    break;
                                case 300://等级提高
                                    LevelUp = par.Value.HasValue ? (int)par.Value.Value : 5;
                                    break;
                                case 301://等级降低
                                    LevelDown = par.Value.HasValue ? (int)par.Value.Value : 5;
                                    break;
                                case 302://限制作答时间
                                    IfLimitTime = par.Value == 1;
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

            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1); // 每秒触发一次
            gameTimer.Tick += GameTimer_Tick;
            timeRemaining = TimeSpan.FromMinutes(max_time); // 设定整个窗口存在的时间
            if(IfLimitTime)
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
        }

        protected override async Task OnPauseAsync()
        {
            gameTimer.Stop(); // 停止计时器

        }

        protected override async Task OnNextAsync()
        {
            // 调整难度
            EndGame();
        }

        protected override async Task OnReportAsync()
        {
            await updateDataAsync();
        }

        protected override IGameBase OnGetExplanationExample()
        {
            return new 搜索能力2讲解();
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
                        int mode = 2;
                        int rep = 0;
                        int totalCount = wrongCount * (rep + 1);
                        int Count = totalCount + correctCount;
                        double accuracy = Math.Round((double)correctCount / (double)Count, 2);

                        // 创建 Result 记录
                        var newResult = new Result
                        {
                            ProgramId = program_id, // program_id
                            Report = "搜索能力2",
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
            //                int mode = 2;
            //                int rep = 0;
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
}
