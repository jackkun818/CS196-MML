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

namespace crs.game.Games
{
    /// <summary>
    /// Logical reasoning ability report.xaml Interaction logic
    /// </summary>
    public partial class Logical reasoning ability report : Window
    {
        public Logical reasoning ability report(double average_time,int correctCount,int incorrectCount)
        {
            InitializeComponent();

            var reportData = new ReportDataLogical reasoning ability
            {
                CorrectCount=correctCount,
                IncorrectCount=incorrectCount,
                Average_time = average_time,
                Date = DateTime.Now// Set date
            };
            dataGrid.ItemsSource = new List<ReportDataLogical reasoning ability> { reportData };
        }
        public class ReportDataLogical reasoning ability
        {
            public DateTime Date { get; set; } // date
            public int CorrectCount { get; set; } 
            public int IncorrectCount { get; set; } 
            public double Average_time { get; set; } // Average reaction time
        }
    }
}
