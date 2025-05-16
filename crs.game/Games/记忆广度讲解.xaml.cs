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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;
namespace crs.game.Games
{
    /// <summary>
    /// 记忆广度讲解.xaml 的交互逻辑
    /// </summary>
    public partial class 记忆广度讲解 : BaseUserControl
    {
        private bool istrue;
        private const int GridSize = 5;
        private const double DELAY = 1.0; // 两相邻方块展示时间间隔
        private const int MAX_BLOCKS = 3; // 设置最大方块数量为3
        private List<Button> buttons = new List<Button>();
        private List<int> sequence = new List<int>();
        private List<int> selectedIndices = new List<int>(); // 记录玩家选中的方块索引
        private Stopwatch stopwatch = new Stopwatch();
        private DispatcherTimer countdownTimer; // 倒计时定时器
        private List<DispatcherTimer> sequenceTimers = new List<DispatcherTimer>(); // 存储展示方块的定时器
        private bool isShowingSequence; // 是否正在展示方块
        private DispatcherTimer gameTimer; // 计时器



        public 记忆广度讲解()
        {
            InitializeComponent();
            InitializeGrid();

            this.Loaded += 记忆广度讲解_Loaded;
        }

        private void 记忆广度讲解_Loaded(object sender, RoutedEventArgs e)
        {
            Button_2_Click(null, null);
        }

        private void InitializeGrid()
        {
            // 获取当前工作目录
            string currentDirectory = Directory.GetCurrentDirectory();
            string imageFolderPath = System.IO.Path.Combine(currentDirectory, "../../../crs.game/Games/pic/JIYI");
            BitmapImage image1 = new BitmapImage(new Uri(imageFolderPath + "1.jpg", UriKind.Relative));
            BitmapImage image2 = new BitmapImage(new Uri(imageFolderPath + "2.jpg", UriKind.Relative));
            BitmapImage image3 = new BitmapImage(new Uri(imageFolderPath + "3.jpg", UriKind.Relative));
            Image_1.Source = image1;
            Image_2.Source = image2;
            Image_3.Source = image3;

            GameGrid.Children.Clear();


            buttons.Clear(); // 同时清空按钮列表

            // 动态计算按钮的高度
            double rowHeight = GameGrid.Height / GameGrid.RowDefinitions.Count;

            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    Button button = new Button
                    {
                        //LJN,修改颜色
                        Background = Brushes.White, // 设置初始背景颜色为灰色
                        //LJN,修改间距2->5
                        Margin = new Thickness(5),
                        FontSize = 24, // 设置默认字体大小
                        Content = "",

                        //LJN,修改高度，让button充满那个grid单元格
                        //Width = 100,
                        Height = rowHeight,
                        //LJN,应用自定义样式
                        Style= CreateCustomButtonStyle(), // 应用自定义样式
                        //LJN,按钮创建时不可点击，直到点击了试玩按钮才可以点击
                        IsEnabled = false // 设置按钮为不可点击
                    };

                    button.Click += Button_Click;
                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);
                    GameGrid.Children.Add(button);
                    buttons.Add(button);
                }
            }
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
        private void StartGame()
        {
            StatusTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            sequence.Clear(); // 清空展示的方块序列
            selectedIndices.Clear(); // 清空玩家已选中的方块索引
            isShowingSequence = true;
            istrue = true; // 重置正确标志
            StatusTextBlock.Text = "请记住红色方块亮起的顺序并在展示完所有红色方块后按展示的顺序依次按下对应的方块。"; // 设置准备开始的文本
            StartInitialCountdown(); // 启动初始倒计时
            //LJN,用来显示图片
            CreateImage();
            OnGameStart();
        }

        private void ShowNextRound()
        {
            int numberToShow = 3; // 每轮展示的方块数
            Random rand = new Random();
            HashSet<int> shownIndices = new HashSet<int>();
            for (int i = 0; i < numberToShow; i++)
            {
                int index;
                do
                {
                    index = rand.Next(buttons.Count);
                } while (shownIndices.Contains(index));

                shownIndices.Add(index);
                sequence.Add(index);
            }
            stopwatch.Restart();
            ShowSequence(0);
        }

        private void StartInitialCountdown()
        {
            countdownTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            int countdownTime = 5; // 5秒倒计时
            countdownTimer.Tick += (s, args) =>
            {
                if (countdownTime > 0)
                {
                    countdownTime--;
                }
                else
                {
                    countdownTimer.Stop();
                    StatusTextBlock.Text = "现在开始展示方块。";
                    ShowNextRound(); // 倒计时结束后开始展示方块
                }
            };
            countdownTimer.Start();
        }

        private void ShowSequence(int index)
        {
            if (index < sequence.Count)
            {
                int buttonIndex = sequence[index];
                buttons[buttonIndex].Background = Brushes.Red;

                DispatcherTimer timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(DELAY)
                };
                timer.Tick += (s, args) =>
                {
                    //LJN,修改颜色为white
                    buttons[buttonIndex].Background = Brushes.White; // 隐藏方块
                    timer.Stop();
                    sequenceTimers.Remove(timer); // 移除定时器
                    ShowSequence(index + 1); // 展示下一个方块
                };
                timer.Start();
                sequenceTimers.Add(timer); // 添加到列表中
            }
            else
            {
                // 所有方块展示完毕，提示用户
                isShowingSequence = false;
                StatusTextBlock.Text = "现在请依次按下对应方块";

                //LJN,按钮此时才能使能，才能点击
                foreach (var button in buttons)
                {
                    button.IsEnabled = true; // 使按钮可点击
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            int clickedIndex = buttons.IndexOf(clickedButton);

            // 只有在展示完之后才处理逻辑
            if (!isShowingSequence)
            {
                // 检查玩家是否按顺序点击方块
                if (selectedIndices.Count < sequence.Count)
                {
                    if (clickedIndex != sequence[selectedIndices.Count])
                    {
                        istrue = false;
                    }

                    if (selectedIndices.Contains(clickedIndex))
                    {//如果已经选中过这个button，则把这个button恢复原样
                        selectedIndices.Remove(clickedIndex);
                        //LJN,恢复为白色
                        clickedButton.Background = Brushes.White; // 恢复为灰色
                        clickedButton.Content = ""; // 清空内容
                        istrue = true;
                    }
                    else
                    {
                        selectedIndices.Add(clickedIndex);
                        //LJN,修改点击后的颜色为橙色
                        clickedButton.Background = Brushes.Orange;
                        clickedButton.Content = (selectedIndices.Count).ToString(); // 显示当前点击的顺序
                        clickedButton.Foreground = Brushes.White; // 设置字体颜色为白色
                        clickedButton.FontSize = 36; // 增加字体大小
                    }
                }

                // 判断是否所有方块都已正确点击
                if (selectedIndices.Count == sequence.Count)
                {
                    foreach (var index in selectedIndices)
                    {
                        if (sequence.Contains(index))
                        {
                            buttons[index].Background = Brushes.Green; // 正确的选项
                            buttons[index].Content = "✔"; // 显示正确标记
                        }
                        else
                        {
                            buttons[index].Background = Brushes.Red; // 错误的选项
                            buttons[index].Content = "✖"; // 显示错误标记
                        }
                        //LJN,防止冲突，将按钮失能，在适当的时候才使其可点击
                        foreach (var button in buttons)
                        {
                            button.IsEnabled = false; // 使按钮不可点击
                        }
                    }
                    if (istrue)
                    {
                        StatusTextBlock1.FontSize = 40;
                        StatusTextBlock1.Text = "      恭喜你答对了！";
                        StatusTextBlock1.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#06CE77"));
                        isShowingSequence = true;
                        OkButton.Visibility = Visibility.Visible;
                        //LJN,显示图片,隐藏文字
                        CorrectImage.Visibility = Visibility.Visible;
                        StatusTextBlock.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        StatusTextBlock.FontSize = 40;
                        StatusTextBlock1.Text = "      很遗憾答错了！";
                        StatusTextBlock1.Foreground = new SolidColorBrush(Colors.Red);
                        OkButton.Visibility = Visibility.Collapsed;
                        //LJN,显示图片,隐藏文字
                        ErrorImage.Visibility = Visibility.Visible;
                        StatusTextBlock.Visibility = Visibility.Collapsed;

                        DispatcherTimer waitTimer = new DispatcherTimer
                        {
                            Interval = TimeSpan.FromSeconds(3) // 设置等待时间为 3 秒
                        };
                        waitTimer.Tick += (s, args) =>
                        {
                            waitTimer.Stop(); // 停止定时器
                            InitializeGrid(); // 初始化网格
                            //LJN,清空一下文字提示
                            StatusTextBlock1.Text = "";
                            //LJN,把图片反馈也给隐藏掉，提示文字显示出来
                            ErrorImage.Visibility = Visibility.Collapsed;
                            CorrectImage.Visibility = Visibility.Collapsed;
                            StatusTextBlock.Visibility = Visibility.Visible;
                            StartGame(); // 开始游戏
                        };
                        waitTimer.Start(); // 启动定时器
                    }
                }
            }
        }

        private void StopAllTimers()
        {
            // 停止倒计时定时器
            countdownTimer?.Stop();

            // 停止所有展示方块的定时器
            foreach (var timer in sequenceTimers)
            {
                timer.Stop();
            }
           
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            //添加开始答题相关逻辑
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
                        Text_1.Visibility = Visibility.Visible;
                        Image_1.Visibility = Visibility.Visible;
                        Text_2.Visibility = Visibility.Hidden;
                        Image_2.Visibility = Visibility.Hidden;
                        Text_3.Visibility = Visibility.Hidden;
                        Image_3.Visibility = Visibility.Hidden;
                       
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Content = "下一步";
                        Button_2.Margin = new Thickness(329, 850, 0, 0);
                        Button_3.Margin = new Thickness(770, 850, 0, 0);


                        await OnVoicePlayAsync(Text_1.Text);
                    }
                    break;
                case 1:
                    {
                        Text_1.Visibility = Visibility.Hidden;
                        Image_1.Visibility = Visibility.Hidden;
                        Text_2.Visibility = Visibility.Visible;
                        Image_2.Visibility = Visibility.Visible;
                        Text_3.Visibility = Visibility.Hidden;
                        Image_3.Visibility = Visibility.Hidden;
                       
                        Button_1.Visibility = Visibility.Visible;
                        Button_1.IsEnabled = true;
                        Button_2.Content = "下一步";
                        Button_2.Margin = new Thickness(550, 850, 0, 0);
                        Button_3.Margin = new Thickness(911, 850, 0, 0);

                        await OnVoicePlayAsync(Text_2.Text);
                    }
                    break;
                case 2:
                    {
                        Text_1.Visibility = Visibility.Hidden;
                        Image_1.Visibility = Visibility.Hidden;
                        Text_2.Visibility = Visibility.Hidden;
                        Image_2.Visibility = Visibility.Hidden;
                        Text_3.Visibility = Visibility.Visible;
                        Image_3.Visibility = Visibility.Visible;
                       
                        Button_2.Content = "试玩";
                        MouseMove += Window_MouseMove; // 订阅 MouseMove 事件
                        await OnVoicePlayAsync(Text_3.Text);
                    }
                    break;
                case 3:
                    {
                        Text_1.Visibility = Visibility.Hidden;
                        Image_1.Visibility = Visibility.Hidden;
                        Text_2.Visibility = Visibility.Hidden;
                        Image_2.Visibility = Visibility.Hidden;
                        Text_3.Visibility = Visibility.Hidden;
                        Image_3.Visibility = Visibility.Hidden;
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Collapsed;
                        //LJN，在讲解模块调用委托
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("现在您看到5×5的方块，然后会按照顺序显示方块，请您进行记忆，在方块隐藏后按顺序依次点击方块");//增加代码，调用函数，显示数字人下的文字

                        //LJN
                        StartGame();
                    }
                    break;
            }
        }


        //LJN,加入一些函数，样式
        private Style CreateCustomButtonStyle()
        {//包装好一个样式
            // 创建按钮样式
            Style buttonStyle = new Style(typeof(Button));

            // 设置背景为白色
            buttonStyle.Setters.Add(new Setter(Button.BackgroundProperty, Brushes.White));

            // 设置阴影效果
            ////LJN,加入阴影效果
            //Effect = new DropShadowEffect
            //{
            //    Color = Colors.Gray,      // 阴影颜色
            //    BlurRadius = 10,          // 模糊半径
            //    ShadowDepth = 5,          // 阴影深度
            //    Direction = 315,          // 阴影方向，角度
            //    Opacity = 0.5             // 阴影透明度
            //},
            buttonStyle.Setters.Add(new Setter(Button.EffectProperty, new DropShadowEffect
            {
                Color = Colors.Gray,
                BlurRadius = 10,
                ShadowDepth = 5,
                Direction = 315,
                Opacity = 0.5
            }));

            // 自定义模板，移除鼠标悬停时的默认视觉变化
            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            border.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Button.BorderBrushProperty));
            border.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Button.BorderThicknessProperty));

            ////LJN,取消长宽的设定，保持填充
            //HorizontalAlignment = HorizontalAlignment.Stretch,  // 设置水平填充
            //VerticalAlignment = VerticalAlignment.Stretch,   // 设置垂直填充
            FrameworkElementFactory contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            border.AppendChild(contentPresenter);
            template.VisualTree = border;

            ////LJN,使得鼠标移动到上面的时候不该变button的的颜色
            //FocusVisualStyle = null
            // 触发器，确保鼠标悬停时背景保持不变
            System.Windows.Trigger isMouseOverTrigger = new System.Windows.Trigger { Property = Button.IsMouseOverProperty, Value = true };
            isMouseOverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, Brushes.White));
            template.Triggers.Add(isMouseOverTrigger);

            // 将模板设置到样式
            buttonStyle.Setters.Add(new Setter(Button.TemplateProperty, template));

            return buttonStyle;
        }

        //LJN,加入一些函数，样式
        private void CreateImage()
        {//创建两张图片，从本地的JIYI文件夹读取
            //首先获取图片的完整路径
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;            // 获取当前项目的根目录(即bin目录)
            //@ 符号用于定义逐字字符串（verbatim string），使字符串中的特殊字符（如反斜杠 \）被原样解释，不再需要转义。

            // 构建图片的相对路径
            string correctRelativePath = @"Games\pic\JIYI\Correct.png";
            string errorRelativePath = @"Games\pic\JIYI\Error.png";

            // 使用 Path.Combine 来拼接绝对路径
            string correctImagePath = System.IO.Path.Combine(currentDirectory, correctRelativePath);
            string errorImagePath = System.IO.Path.Combine(currentDirectory, errorRelativePath);
            CorrectImage.Source = new BitmapImage(new Uri(correctImagePath, UriKind.RelativeOrAbsolute));
            ErrorImage.Source = new BitmapImage(new Uri(errorImagePath, UriKind.RelativeOrAbsolute));
            ErrorImage.Visibility = Visibility.Collapsed;
            CorrectImage.Visibility = Visibility.Collapsed;


            //针对试玩版本，把原先的文字提示给隐藏掉
            StatusTextBlock1.Visibility = Visibility.Collapsed;
        }
    }
}

