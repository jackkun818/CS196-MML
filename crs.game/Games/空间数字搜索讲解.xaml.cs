using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace crs.game.Games
{
    /// <summary>
    /// 空间数字搜索讲解.xaml 的交互逻辑
    /// </summary>
    public partial class 空间数字搜索讲解 : BaseUserControl
    {
        private List<int> numbers;
        private int lastClickedNumber;
        private int maxConsecutiveNumber; // 记录最长连续数字串的最大值
        private Brush defaultButtonBackground; // 存储按钮的默认背景颜色
        public Action GameBeginAction { get; set; }

        public Func<string, Task> VoicePlayFunc { get; set; }
        public 空间数字搜索讲解()
        {
            InitializeComponent();

           
           
            lastClickedNumber = 0; // 初始化为0，表示未点击
            maxConsecutiveNumber = 0; // 初始化最大连续数字串为0
            defaultButtonBackground = Brushes.White; // 更改地方：---》设置默认背景颜色灰色改为白色
            InitializeNumberGrid();

            this.Loaded += 空间数字搜索讲解_Loaded;


        }

        private void 空间数字搜索讲解_Loaded(object sender, RoutedEventArgs e)
        {
            // 页面加载时确保按键和焦点行为
            Button_2_Click(null, null);
            this.Focus();  // 确保焦点在窗口上
        }

        private void InitializeNumberGrid()
        {
            numbers = Enumerable.Range(1, 25).ToList(); // 更改地方：修改为1到4--》1到25,在xaml里我调了生成数字方格的大小位置，以及给他分了五行五列给他分布
            Random rand = new Random();
            numbers = numbers.OrderBy(x => rand.Next()).ToList(); // 打乱顺序

            foreach (var number in numbers)
            {
                Button button = new Button
                {
                    BorderThickness = new Thickness(0),//更改地方：大小改为0
                    Content = (number <= 5) ? number.ToString() : "", // 更改地方---》只显示1到5的数字
                    FontWeight = FontWeights.Bold, // 设置字体加粗
                    FontSize = 32,
                    Margin = new Thickness(5),//更改地方：大小改为5
                    Style = CreateCustomButtonStyle(),//更改地方：null---》改为CreateCustomButtonStyle这个自己定义的按钮格式，增加灰边、去掉悬停反馈等，这个定义在下面
                    Background = defaultButtonBackground // 设置按钮的初始背景颜色
                };

                button.Click += NumberButton_Click;
                NumberGrid.Children.Add(button);
            }
            Run textPart1 = new Run("请找出: ")
            {
                Foreground = new SolidColorBrush(Colors.Black)
            };

            // 创建并添加橙色文本
            Run textPart2 = new Run((maxConsecutiveNumber + 1).ToString())
            {
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB346"))
            };

            // 清空当前的 Inlines
            tipblock.Inlines.Clear();

            // 将两个部分添加到 TextBlock
            tipblock.Inlines.Add(textPart1);
            tipblock.Inlines.Add(textPart2);
        }
        private void Window_MouseMove(object sender, MouseEventArgs e)//绑定自定义鼠标光标和默认鼠标光标
        {

            Point position = e.GetPosition(this);
            Canvas.SetLeft(CustomCursor, position.X - (CustomCursor.Width / 2));
            Canvas.SetTop(CustomCursor, position.Y - (CustomCursor.Height / 2));
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
        private void OnGameStart()//调用自定义鼠标光标函数
        {
            this.Cursor = Cursors.None; // 隐藏默认光标
            CustomCursor.Visibility = Visibility.Visible; // 显示自定义光标
            MouseMove += Window_MouseMove; // 订阅 MouseMove 事件
            CustomCursor.Width = 65; // 调整为你想要的宽度
            CustomCursor.Height = 65; // 调整为你想要的高度
        }
        private async void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            int clickedNumber = Convert.ToInt32(clickedButton.Content);

            if (clickedNumber < maxConsecutiveNumber + 1)
                return;
            if (maxConsecutiveNumber == 0 && clickedNumber == maxConsecutiveNumber + 1)
            {
                maxConsecutiveNumber++; 
                clickedButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB346"));
                clickedButton.Foreground = Brushes.White; // 更改地方---》设置前景色为白色
            }
            else
            {
                if (clickedNumber == maxConsecutiveNumber + 1)
                {
                    maxConsecutiveNumber++;
                    clickedButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB346"));
                    clickedButton.Foreground = Brushes.White; // 更改地方---》设置前景色为白色

                    // 检查是否所有数字都已被点击
                    if (maxConsecutiveNumber == 5) // 更改地方---》修改为展示的五个数字，4---》5
                    {
                    
                        FeedbackImage.Source = new BitmapImage(new Uri("./pic/NUQU/right.png", UriKind.Relative)); //更改地方：把答对了换成图片展示，设置正确图片,把“恭喜你答对了”替换掉
                        FeedbackImage.Visibility = Visibility.Visible;
                        OkButton.Visibility = Visibility.Visible;
                        tipblock.Visibility = Visibility.Collapsed;//更改地方---》答对了就不显示请找出数字的文字了
                    }
                }
                else
                {
                    FeedbackImage.Visibility = Visibility.Visible; //更改地方---》答对了才显示出这个答对答错的文本框，这里可以看xaml文件，我把答对答错文本框和请找出数字的文本框分开了
                    clickedButton.Background = Brushes.Black; // 设置按钮背景为黑色

                    FeedbackImage.Source = new BitmapImage(new Uri("./pic/NUQU/error.png", UriKind.Relative)); // 更改地方：把答错了换成图片展示，设置错误图片，把“很遗憾答错了”替换掉
                    FeedbackImage.Visibility = Visibility.Visible;

                    // 等待0.5秒后恢复颜色
                    await Task.Delay(500); // 等待500毫秒
                    clickedButton.Background = defaultButtonBackground; // 恢复按钮背景为默认颜色
                    await Task.Delay(500); // 等待500毫秒

                    FeedbackImage.Visibility = Visibility.Collapsed;
                    clickedButton.IsEnabled = true; // 重新启用按钮
                    tipblock.Visibility = Visibility.Visible;
                }
            }
            lastClickedNumber = clickedNumber;
            Run textPart1 = new Run("请找出: ")
            {
                Foreground = new SolidColorBrush(Colors.Black)
            };

            // 创建并添加橙色文本
            Run textPart2 = new Run((maxConsecutiveNumber + 1).ToString())
            {
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB346"))
            };

            // 清空当前的 Inlines
            tipblock.Inlines.Clear();

            // 将两个部分添加到 TextBlock
            tipblock.Inlines.Add(textPart1);
            tipblock.Inlines.Add(textPart2);
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
                        Text_1.Visibility = Visibility.Visible;
                        Image_1.Visibility = Visibility.Collapsed;
                        NumberGrid.Visibility = Visibility.Visible;
                        FeedbackImage.Visibility = Visibility.Collapsed;
                        TipBlock1.Visibility = Visibility.Collapsed;
                        tipblock.Visibility = Visibility.Collapsed;
                        TipBlock2.Visibility = Visibility.Collapsed;
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_1.IsEnabled = false;
                        Button_2.Content = "试玩";
                        Button_2.Margin = new Thickness(329, 850, 0, 0);
                        Button_3.Margin = new Thickness(770, 850, 0, 0);
                        NumberGrid.Margin = new Thickness(0, 15, 0, 285); // 调整这里的数值
                        MouseMove += Window_MouseMove; // 订阅 MouseMove 事件
                        await OnVoicePlayAsync(Text_1.Text);
                    }
                    break;
                case 1:
                    {
                        // 显示讲解的第二个界面
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        TipBlock2.Visibility = Visibility.Visible;
                        NumberGrid.Visibility = Visibility.Visible;
                        FeedbackImage.Visibility = Visibility.Visible;
                        tipblock.Visibility = Visibility.Visible;

                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;
                        Button_3.Visibility = Visibility.Collapsed;
                        NumberGrid.Margin = new Thickness(0, 74, 0, 226); // 恢复原来的 Margin

                        //LJN，在讲解模块调用委托
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("您在屏幕上会看到有5×5的方块，方块上有若干个顺序打乱的数字，请您用鼠标按照数字的顺序，依次用鼠标点击这些方块，速度越快越好！");//增加代码，调用函数，显示数字人下的文字
                        //LJN
                        OnGameStart();


                    }
                    break;

            }
        }
        //更改地方：按钮自定义的函数代码，这是开始
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
        //按钮格式定义结束，更改结束


        /// <summary>
        /// 讲解内容语音播放
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        async Task VoicePlayer(string message)
        {
            var voicePlayFunc = VoicePlayFunc;
            if (voicePlayFunc == null)
            {
                return;
            }

            await voicePlayFunc.Invoke(message);
        }
    }
}
