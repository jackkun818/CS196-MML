using System.Windows.Controls;

namespace crs.window.Views
{
    /// <summary>
    /// Interaction logic for Train
    /// </summary>
    public partial class Train : UserControl
    {
        public Train()
        {
            InitializeComponent();
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
