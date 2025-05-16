using crs.extension;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using crs.theme.Extensions;

namespace crs.dialog.Views
{
    /// <summary>
    /// Interaction logic for TrainGamePanel
    /// </summary>
    public partial class TrainGamePanel : UserControl
    {
        public TrainGamePanel()
        {
            InitializeComponent();
        }

        private async void SimplePanel_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("非交互区域，请尝试点击返回按钮");
        }

        private void ListBox_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                // ListView拦截鼠标滚轮事件
                e.Handled = true;

                // 激发一个鼠标滚轮事件，冒泡给外层ListView接收到
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;

                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems?.Count == 0)
            {
                return;
            }

            var listBox = sender as ListBox;
            var lastItem = e.AddedItems[e.AddedItems.Count - 1];
            listBox?.ScrollIntoView(lastItem);
        }
    }
}
