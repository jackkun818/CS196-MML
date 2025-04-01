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
            //await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("Non-interactive area, please try to click the return button");
        }

        private void ListBox_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                // ListViewIntercept mouse wheel events
                e.Handled = true;

                // Inspire a mouse wheel event and bubble to the outer layerListViewReceived
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
