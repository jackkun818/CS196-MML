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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace crs.game.Games
{
    /// <summary>
    /// EvaluateTest.xaml 的交互逻辑
    /// </summary>
    public partial class EvaluateTest : Window
    {
        private Random random = new Random();
        private bool shouldContinueAssessment = true;
        private List<Type> windows = new List<Type>
        {
            typeof(空间数字搜索),
            typeof(警觉能力),
            typeof(选择注意力),
            typeof(逻辑推理能力),
            typeof(词语记忆能力),
            typeof(记忆广度),
            typeof(视野)
        };

        public EvaluateTest()
        {
            InitializeComponent();
            Button_1.Content = "空间数字搜索";
            Button_2.Content = "警觉能力";
            Button_3.Content = "选择注意力";
            Button_4.Content = "逻辑推理能力";
            Button_5.Content = "词语记忆力能力";
            Button_6.Content = "记忆广度";
            Button_7.Content = "视野";
            Button_8.Content = "开始评估";
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) { OpenWindow(typeof(空间数字搜索)); }
        private void Button_Click_2(object sender, RoutedEventArgs e) { OpenWindow(typeof(警觉能力)); }
        private void Button_Click_3(object sender, RoutedEventArgs e) { OpenWindow(typeof(选择注意力)); }
        private void Button_Click_4(object sender, RoutedEventArgs e) { OpenWindow(typeof(逻辑推理能力)); }
        private void Button_Click_5(object sender, RoutedEventArgs e) { OpenWindow(typeof(词语记忆能力)); }
        private void Button_Click_6(object sender, RoutedEventArgs e) { OpenWindow(typeof(记忆广度)); }
        private void Button_Click_7(object sender, RoutedEventArgs e) { OpenWindow(typeof(视野)); }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            shouldContinueAssessment = true;
            List<Type> randomWindows = new List<Type>();
            foreach (var window in windows)
            {
                randomWindows.Add(window);
            }

            // Shuffle the list using Fisher-Yates algorithm
            for (int i = randomWindows.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                // Swap randomWindows[i] with the element at random index
                var temp = randomWindows[i];
                randomWindows[i] = randomWindows[j];
                randomWindows[j] = temp;
            }

            // Open each window in the shuffled list
            OpenWindowsInSequence(randomWindows);
        }


        private void OpenWindowsInSequence(List<Type> windowTypes)
        {
            if (windowTypes.Count == 0 || !shouldContinueAssessment)
            {
                return;
            }

            Type windowType = windowTypes[0];
            windowTypes.RemoveAt(0); // 移除已打开的窗口类型

            // 创建新窗口实例
            var newWindow = (Window)Activator.CreateInstance(windowType);

            // 处理窗口关闭事件
            newWindow.Closed += (sender, e) =>
            {
                OpenWindowsInSequence(windowTypes); // 打开下一个窗口
            };

            newWindow.Show();
        }
        public void SetContinueAssessment(bool continueAssessment)
        {
            shouldContinueAssessment = continueAssessment; // 设置标志
        }
        private void OpenWindow(Type windowType)
        {
            Window window = (Window)Activator.CreateInstance(windowType);
            window.Show();
        }
    }
}