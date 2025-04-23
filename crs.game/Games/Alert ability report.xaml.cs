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
    /// Alert ability report.xaml Interaction logic
    /// </summary>
    public partial class Alert_ability_report : Window
    {
        public Alert_ability_report(double average_time,bool is_beep,int forget)
        {
            InitializeComponent();
            var reportData = new ReportDataAlert_ability
            {
                Alarm = is_beep?"There is a warning sound":"No warning tone",
                Forget = forget,
                Average_time = average_time,
                Date = DateTime.Now// Set date
            };
            dataGrid.ItemsSource = new List<ReportDataAlert_ability> { reportData };
        }
    }
    public class ReportDataAlert_ability
    {
        public DateTime Date { get; set; } // date
        public string Alarm { get; set; } // situation
        public int Forget { get; set; } // Number of omissions
        public double Average_time { get; set; } // Average reaction time
    }
}
