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
            typeof(Space digital search),
            typeof(Alert ability),
            typeof(Select attention),
            typeof(Logical reasoning ability),
            typeof(Word memory ability),
            typeof(Broadness of memory),
            typeof(Vision)
        };

        public EvaluateTest()
        {
            InitializeComponent();
            Button_1.Content = "Space digital search";
            Button_2.Content = "Alert ability";
            Button_3.Content = "Select attention";
            Button_4.Content = "Logical reasoning ability";
            Button_5.Content = "Word memory ability";
            Button_6.Content = "Broadness of memory";
            Button_7.Content = "Vision";
            Button_8.Content = "Begin the evaluation";
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) { OpenWindow(typeof(Space digital search)); }
        private void Button_Click_2(object sender, RoutedEventArgs e) { OpenWindow(typeof(Alert ability)); }
        private void Button_Click_3(object sender, RoutedEventArgs e) { OpenWindow(typeof(Select attention)); }
        private void Button_Click_4(object sender, RoutedEventArgs e) { OpenWindow(typeof(Logical reasoning ability)); }
        private void Button_Click_5(object sender, RoutedEventArgs e) { OpenWindow(typeof(Word memory ability)); }
        private void Button_Click_6(object sender, RoutedEventArgs e) { OpenWindow(typeof(Broadness of memory)); }
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

            // Create a new window instance
            var newWindow = (Window)Activator.CreateInstance(windowType);

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
            Window window = (Window)Activator.CreateInstance(windowType);
            window.Show();
        }
    }
}