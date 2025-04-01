using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace crs.game.Games
{
    /// <summary>
    /// Memory Broadness Report.xaml Interaction logic
    /// </summary>
    public partial class Memory Broadness Report : Window
    {
        public Memory Broadness Report(Dictionary<int, int> errorCounts)
        {
            InitializeComponent();

            // Create a list to store all report data
            List<ReportDataBroadness of memory> reportDataList = new List<ReportDataBroadness of memory>();

            // Traversal errorCounts dictionary
            foreach (var entry in errorCounts)
            {
                int level = entry.Key; // The number of blocks currently displayed
                int incorrectCount = entry.Value; // Errors

                // Create a new ReportData Example
                var reportData = new ReportDataBroadness of memory
                {
                    Level = level,
                    Score = 2 * level - incorrectCount, // Calculate the score
                    IncorrectCount = incorrectCount,
                    Date = DateTime.Now // Set date
                };

                // Will ReportData Add to list
                reportDataList.Add(reportData);
            }

            // Bind the data model to DataGrid
            dataGrid.ItemsSource = reportDataList; // Assumptions dataGrid It's yours DataGrid Controls
        }

        public class ReportDataBroadness of memory
        {
            public int Level { get; set; }
            public int Score { get; set; }
            public int IncorrectCount { get; set; }
            public DateTime Date { get; set; } // Added date attribute
        }
    }
}
