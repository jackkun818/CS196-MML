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
    /// EvaluateTest.xaml Interaction logic
    /// </summary>
    public partial class EvaluateTest : Window
    {
        private Random random = new Random();
        private bool shouldContinueAssessment = true;
        private List<Type> windows = new List<Type>
        {
            typeof(Space_digital_search),
            typeof(Alert_ability),
            typeof(Select_attention),
            typeof(Logical_reasoning_ability),
            typeof(Word_memory_ability),
            typeof(Broadness_of_memory),
            typeof(Vision)
        };

        // 显式声明按钮控件
        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
        private Button button5;
        private Button button6;
        private Button button7;
        private Button button8;

        public EvaluateTest()
        {
            InitializeComponent();
            
            // 查找并连接按钮控件
            button1 = (Button)this.FindName("Button_1");
            button2 = (Button)this.FindName("Button_2");
            button3 = (Button)this.FindName("Button_3");
            button4 = (Button)this.FindName("Button_4");
            button5 = (Button)this.FindName("Button_5");
            button6 = (Button)this.FindName("Button_6");
            button7 = (Button)this.FindName("Button_7");
            button8 = (Button)this.FindName("Button_8");

            if (button1 != null) button1.Content = "Space digital search";
            if (button2 != null) button2.Content = "Alert ability";
            if (button3 != null) button3.Content = "Select attention";
            if (button4 != null) button4.Content = "Logical reasoning ability";
            if (button5 != null) button5.Content = "Word memory ability";
            if (button6 != null) button6.Content = "Broadness of memory";
            if (button7 != null) button7.Content = "Vision";
            if (button8 != null) button8.Content = "Begin the evaluation";
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) { OpenWindow(typeof(Space_digital_search)); }
        private void Button_Click_2(object sender, RoutedEventArgs e) { OpenWindow(typeof(Alert_ability)); }
        private void Button_Click_3(object sender, RoutedEventArgs e) { OpenWindow(typeof(Select_attention)); }
        private void Button_Click_4(object sender, RoutedEventArgs e) { OpenWindow(typeof(Logical_reasoning_ability)); }
        private void Button_Click_5(object sender, RoutedEventArgs e) { OpenWindow(typeof(Word_memory_ability)); }
        private void Button_Click_6(object sender, RoutedEventArgs e) { OpenWindow(typeof(Broadness_of_memory)); }
        private void Button_Click_7(object sender, RoutedEventArgs e) { OpenWindow(typeof(Vision)); }

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
            windowTypes.RemoveAt(0); // Remove the open window type

            // 创建新窗口，注意，这里需要考虑到某些类型可能是BaseUserControl而不是Window
            Window newWindow;
            
            // 根据类型创建适当的窗口
            if (typeof(Window).IsAssignableFrom(windowType))
            {
                // 如果类型是Window或其子类
                newWindow = (Window)Activator.CreateInstance(windowType);
            }
            else
            {
                // 如果类型是其他控件，如BaseUserControl，则创建一个包含它的新窗口
                newWindow = new Window();
                var control = Activator.CreateInstance(windowType);
                newWindow.Content = control;
                newWindow.SizeToContent = SizeToContent.WidthAndHeight;
                newWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            // Handle window closing event
            newWindow.Closed += (sender, e) =>
            {
                OpenWindowsInSequence(windowTypes); // Open the next window
            };

            newWindow.Show();
        }

        public void SetContinueAssessment(bool continueAssessment)
        {
            shouldContinueAssessment = continueAssessment; // Set flags
        }

        private void OpenWindow(Type windowType)
        {
            // 根据类型创建适当的窗口
            Window window;
            
            if (typeof(Window).IsAssignableFrom(windowType))
            {
                // 如果类型是Window或其子类
                window = (Window)Activator.CreateInstance(windowType);
            }
            else
            {
                // 如果类型是其他控件，如BaseUserControl，则创建一个包含它的新窗口
                window = new Window();
                var control = Activator.CreateInstance(windowType);
                window.Content = control;
                window.SizeToContent = SizeToContent.WidthAndHeight;
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            
            window.Show();
        }
    }
}