using System;
using System.Collections.Generic;
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




namespace crs.game.Games
{

    /// <summary>
    /// 容貌记忆力讲解.xaml 的交互逻辑
    /// </summary>
    public partial class 容貌记忆力讲解 : BaseUserControl
    {
        private string[] imagePaths;
        private string selectedImagePath;
        private string displayedImagePath;
        private bool isMemoryStage = true;
        private Border selectedThumbnailBorder;
        public Action GameBeginAction { get; set; }

        public Func<string, Task> VoicePlayFunc { get; set; }


        public 容貌记忆力讲解()
        {
            InitializeComponent();
            // 获取相对路径下的图片文件夹
            string imageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources/容貌记忆力/GESI");
            imagePaths = Directory.GetFiles(imageFolder, "*.jpg");
            // 随机抽取一张图片
            Random random = new Random();
            displayedImagePath = imagePaths[random.Next(imagePaths.Length)];
            // 显示随机抽取的图片
            DisplayedImage.Source = new BitmapImage(new Uri(displayedImagePath));
            InfoText4.Text = "";
            // 隐藏button3
            button3.Visibility = Visibility.Collapsed;
            
            this.Loaded += 容貌记忆力讲解_Loaded;
        }

        protected override void OnHostWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (ThumbnailPanel.Children.Count == 0) return; // 如果没有缩略图，直接返回

            // 找到当前选中的图片路径的索引
            int currentIndex = Array.IndexOf(imagePaths, selectedImagePath);

            if (e.Key == Key.Left)
            {
                // 向左滑动
                if (currentIndex > 0)
                {
                    currentIndex--; // 选择前一个
                }
                else
                {
                    currentIndex = imagePaths.Length - 1; // 循环到最后一个
                }
                selectedImagePath = imagePaths[currentIndex]; // 更新选中的图片路径
                SelectThumbnailByImagePath(selectedImagePath); // 高亮选中的缩略图
            }
            else if (e.Key == Key.Right)
            {
                // 向右滑动
                if (currentIndex < imagePaths.Length - 1)
                {
                    currentIndex++; // 选择下一个
                }
                else
                {
                    currentIndex = 0; // 循环到第一个
                }
                selectedImagePath = imagePaths[currentIndex]; // 更新选中的图片路径
                SelectThumbnailByImagePath(selectedImagePath); // 高亮选中的缩略图
            }
            else if (e.Key == Key.Enter)
            {
                // 按下 Enter 键自动点击 button2
                if (button2.Visibility == Visibility.Visible) // 确保 button2 可见
                {
                    button2.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
            }
        }

        // 根据图片路径选择缩略图的辅助方法
        private void SelectThumbnailByImagePath(string imagePath)
        {
            // 取消之前高亮的缩略图
            if (selectedThumbnailBorder != null)
            {
                selectedThumbnailBorder.BorderBrush = Brushes.Transparent; // 恢复为透明边框
                selectedThumbnailBorder.BorderThickness = new Thickness(1); // 恢复为默认边框厚度
            }

            // 遍历缩略图并找到对应的路径进行高亮
            foreach (UIElement element in ThumbnailPanel.Children)
            {
                if (element is Border border && border.Child is Image img &&
                    img.Source is BitmapImage bitmap &&
                    bitmap.UriSource.LocalPath.Equals(new Uri(imagePath, UriKind.Absolute).LocalPath, StringComparison.OrdinalIgnoreCase))
                {
                    border.BorderBrush = Brushes.Red; // 将边框颜色改为红色
                    border.BorderThickness = new Thickness(2); // 改变边框厚度

                    selectedThumbnailBorder = border; // 记录当前高亮的缩略图

                    // 强制刷新
                    border.InvalidateVisual();
                    ThumbnailPanel.InvalidateVisual();
                    break;
                }
            }

            // 更新显示的图片
            DisplayedImage.Source = new BitmapImage(new Uri(selectedImagePath));
        }


        // 选择缩略图的辅助方法


        private void 容貌记忆力讲解_Loaded(object sender, RoutedEventArgs e)
        {
            Button_2_Click(null, null);
        }

        private void LoadImage(string imagePath)
        {
            if (File.Exists(imagePath))
            {
                // 创建 BitmapImage 对象并设置图像源
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.EndInit();

                // 设置 Image 控件的源
                CorrectOrNot.Source = bitmap;
            }

        }

        private async void PressContinue_Button(object sender, RoutedEventArgs e)
        {
            if (isMemoryStage)
            {
                // 用户完成记忆，进入选择阶段
                InfoText4.Text = "";
                CorrectOrNot.Source = null;
                DisplayThumbnails();
                DisplayedImage.Source = null;
                isMemoryStage = false;  // 切换到选择阶段
                InfoText3.Text = "请根据所记信息选择对应的人物图像";
                button2.Content = "OK";
                //InfoText2.Text = "选择记忆的图片";
            }
            else
            {
                // 检查用户选择是否正确
                if (selectedImagePath == displayedImagePath)
                {
                    //InfoText4.Text = "正确";
                    //InfoText4.Foreground = Brushes.Green;
                    string currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    string targetDirectory = Path.Combine(currentDirectory, "Resources");
                    // "容貌记忆力" 文件夹
                    string FolderPath = Path.Combine(targetDirectory, "容貌记忆力");
                    string picFolderPath = Path.Combine(FolderPath, "correct.png");
                    LoadImage(picFolderPath);

                    button2.Visibility = Visibility.Collapsed;
                    button3.Visibility = Visibility.Visible;  // 显示button3
                }
                else
                {
                    //InfoText4.Text = "错误";
                    //InfoText4.Foreground = Brushes.Red;
                    string currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    string targetDirectory = Path.Combine(currentDirectory, "Resources");
                    // "容貌记忆力" 文件夹
                    string FolderPath = Path.Combine(targetDirectory, "容貌记忆力");
                    string picFolderPath = Path.Combine(FolderPath, "error.png");
                    LoadImage(picFolderPath);
                    await Task.Delay(1500);
                    CorrectOrNot.Source = null;
                    button2.Content = "OK";
                }

            }
        }

        // 高亮选中的缩略图
        private void HighlightThumbnail(string imagePath)
        {
            // 移除先前高亮的缩略图
            if (selectedThumbnailBorder != null)
            {
                selectedThumbnailBorder.BorderBrush = Brushes.Transparent;
                selectedThumbnailBorder.BorderThickness = new Thickness(0);
                selectedThumbnailBorder = null;
            }

            foreach (UIElement element in ThumbnailPanel.Children)
            {
                if (element is Border border && border.Child is Image img &&
                    img.Source is BitmapImage bitmap &&
                    bitmap.UriSource.LocalPath.Equals(new Uri(imagePath, UriKind.Absolute).LocalPath, StringComparison.OrdinalIgnoreCase))
                {
                    border.BorderBrush = Brushes.Red;
                    border.BorderThickness = new Thickness(2);

                    selectedThumbnailBorder = border;

                    // 强制刷新
                    border.InvalidateVisual();
                    ThumbnailPanel.InvalidateVisual();
                    break;
                }
            }
        }


        private void DisplayThumbnails()
        {
            ThumbnailPanel.Children.Clear();
            ThumbnailPanel.Visibility = Visibility.Visible;

            foreach (var imagePath in imagePaths)
            {
                // 创建一个新的 Border
                Border border = new Border
                {
                    Width = 100,
                    Height = 65,
                    Margin = new Thickness(5),
                    BorderBrush = Brushes.Transparent, // 默认边框颜色
                    BorderThickness = new Thickness(1) // 默认边框厚度
                };

                // 创建缩略图
                Image thumbnail = new Image
                {
                    Source = new BitmapImage(new Uri(imagePath)),
                    Width = 100,
                    Height = 100,
                    Tag = imagePath // 将路径存储在 Tag 中
                };

                thumbnail.MouseLeftButtonUp += Thumbnail_Click; // 订阅点击事件

                // 将 Image 添加到 Border
                border.Child = thumbnail;

                // 将 Border 添加到 ThumbnailPanel
                ThumbnailPanel.Children.Add(border);
            }
        }




        //缩略图
        

        private void Thumbnail_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // 确保 sender 是一个 Border
            Border selectedBorder = (sender as FrameworkElement)?.Parent as Border;

            if (selectedBorder != null)
            {
                // 如果有先前选中的 Border，恢复其样式
                if (selectedThumbnailBorder != null)
                {
                    selectedThumbnailBorder.BorderBrush = Brushes.Transparent; // 恢复为透明边框
                    selectedThumbnailBorder.BorderThickness = new Thickness(1); // 恢复为默认边框厚度
                }

                // 设置当前选中的 Border
                selectedThumbnailBorder = selectedBorder;
                selectedThumbnailBorder.BorderBrush = Brushes.Red; // 将边框颜色改为红色
                selectedThumbnailBorder.BorderThickness = new Thickness(2); // 可选：改变边框厚度

                // 获取 Image 的路径并更新显示
                Image selectedImage = (Image)selectedBorder.Child;
                selectedImagePath = selectedImage.Tag.ToString();
                DisplayedImage.Source = new BitmapImage(new Uri(selectedImagePath));
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
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        button2.Visibility = Visibility.Collapsed;
                        DisplayedImage.Visibility = Visibility.Collapsed;
                        InfoText3.Visibility = Visibility.Collapsed;
                        //Button_1.IsEnabled = false;
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Content = "下一步";
                        Button_2.Margin = new Thickness(329, 850, 0, 0);
                        Button_3.Margin = new Thickness(770, 850, 0, 0);
                        await OnVoicePlayAsync(Text_1.Text);
                    }
                    break;
                case 1:
                    {
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Visible;
                        Image_2.Visibility = Visibility.Visible;

                        Button_1.Visibility = Visibility.Visible;
                        Button_1.IsEnabled = true;
                        Button_2.Content = "试玩";
                        Button_2.Margin = new Thickness(550, 850, 0, 0);
                        Button_3.Margin = new Thickness(911, 850, 0, 0);

                        await OnVoicePlayAsync(Text_2.Text);
                    }
                    break;
                case 2:
                    {
                        Text_1.Visibility = Visibility.Collapsed;
                        Image_1.Visibility = Visibility.Collapsed;
                        Text_2.Visibility = Visibility.Collapsed;
                        Image_2.Visibility = Visibility.Collapsed;
                        InfoText4.Visibility = Visibility.Visible;
                        InfoText3.Visibility = Visibility.Visible;
                        DisplayedImage.Visibility = Visibility.Visible;
                        button2.Visibility = Visibility.Visible;
                        Button_1.Visibility = Visibility.Collapsed;
                        Button_2.Visibility = Visibility.Collapsed;

                        Button_3.Height = 67;
                        Button_3.Width = 241;
                        Button_3.FontSize = 40;
                        Button_3.Margin = new Thickness(0, 895, 0, 0);
                        Button_3.HorizontalAlignment = HorizontalAlignment.Center;

                        //LJN，在讲解模块调用委托
                        SetTitleVisibleAction?.Invoke(true);
                        RuleAction?.Invoke("您在屏幕上会先看到一个人的容貌图像，请记住其特征，记忆完成后用鼠标点击右下角的“记忆完成”按钮。随后后出现一系列的人物容貌图像，您需要根据您所记住的特征来区分出哪个是您刚刚所记住的那个人的容貌，并用鼠标点击右下角“确认选择”按钮。");//增加代码，调用函数，显示数字人下的文字
                        //LJN

                    }
                    break;
            }
        }

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
