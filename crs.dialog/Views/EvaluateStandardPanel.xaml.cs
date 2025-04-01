using crs.extension;
using System.Windows.Controls;
using crs.theme.Extensions;

namespace crs.dialog.Views
{
    /// <summary>
    /// Interaction logic for EvaluateStandardPanel
    /// </summary>
    public partial class EvaluateStandardPanel : UserControl
    {
        public EvaluateStandardPanel()
        {
            InitializeComponent();
            
        }

        private async void SimplePanel_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //await Crs_DialogEx.MessageBoxShow().GetMessageBoxResultAsync("Non-interactive area, please try to click the return button");
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
