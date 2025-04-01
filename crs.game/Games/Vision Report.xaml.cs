using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
    /// Vision Report.xaml Interaction logic
    /// </summary>
    public partial class Vision Report : Window
    {
        public Vision Report(double average_time,int correct,int wrong,int forget)
        {
            InitializeComponent();
            var reportData = new ReportDataVision
            {
                CorrectCount = correct,
                IncorrectCount = wrong,
                Average_time = average_time,
                ForgetCount=forget,
                Date = DateTime.Now// Set date
            };
            dataGrid.ItemsSource = new List<ReportDataVision> { reportData };
        }
        public class ReportDataVision
        {
            public DateTime Date { get; set; } // date
            public int CorrectCount { get; set; } 
            public int IncorrectCount { get; set; } 
            public int ForgetCount {  get; set; }
            public double Average_time { get; set; } // Average reaction time
        }
    }
}
