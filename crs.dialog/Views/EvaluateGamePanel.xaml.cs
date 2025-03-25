using crs.extension;
using System.Windows.Controls;
using crs.theme.Extensions;

namespace crs.dialog.Views
{
    /// <summary>
    /// Interaction logic for EvaluateGamePanel
    /// </summary>
    public partial class EvaluateGamePanel : UserControl
    {
        public EvaluateGamePanel()
        {
            InitializeComponent();
        }

        private async void SimplePanel_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("非交互区域，请尝试点击返回按钮");
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
