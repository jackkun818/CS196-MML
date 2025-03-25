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
using static crs.game.Games.逻辑推理能力报告;

namespace crs.game.Games
{
    /// <summary>
    /// 视野报告.xaml 的交互逻辑
    /// </summary>
    public partial class 视野报告 : Window
    {
        public 视野报告(double average_time,int correct,int wrong,int forget)
        {
            InitializeComponent();
            var reportData = new ReportData视野
            {
                CorrectCount = correct,
                IncorrectCount = wrong,
                Average_time = average_time,
                ForgetCount=forget,
                Date = DateTime.Now// 设置日期
            };
            dataGrid.ItemsSource = new List<ReportData视野> { reportData };
        }
        public class ReportData视野
        {
            public DateTime Date { get; set; } // 日期
            public int CorrectCount { get; set; } 
            public int IncorrectCount { get; set; } 
            public int ForgetCount {  get; set; }
            public double Average_time { get; set; } // 平均反应时间
        }
    }
}
