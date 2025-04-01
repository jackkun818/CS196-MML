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
using System.Windows.Shapes;
using static crs.game.Games.Logical reasoning ability report;

namespace crs.game.Games
{
    /// <summary>
    /// Space digital search report.xaml Interaction logic
    /// </summary>
    public partial class Space digital search report : Window
    {
        public Space digital search report(int incorrectCount, double[] timesInterval)
        {
            InitializeComponent();
            var reportData = new ReportDataSpace digital search
            {
                IncorrectCount = incorrectCount,
                Date = DateTime.Now// Set date
            };
            dataGrid.ItemsSource = new List<ReportDataSpace digital search> { reportData };
            int itemsToDisplay = Math.Min(24, timesInterval.Length);

            for (int i = 0; i < itemsToDisplay; i++)
            {
                // Create a TextBlock Show each time interval
                TextBlock textBlock = new TextBlock
                {
                    Text = timesInterval[i].ToString("F2"), // Formatted as two decimal places
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5)
                };

                // Will TextBlock Add to UniformGrid
                timesIntervalGrid.Children.Add(textBlock);
            }
        }
        public class ReportDataSpace digital search
        {
            public DateTime Date { get; set; } // date
            public int IncorrectCount { get; set; }
        }
    }
}
