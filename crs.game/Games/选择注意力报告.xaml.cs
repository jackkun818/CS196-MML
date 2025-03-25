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
using static crs.game.Games.视野报告;

namespace crs.game.Games
{
    /// <summary>
    /// 选择注意力报告.xaml 的交互逻辑
    /// </summary>
    public partial class 选择注意力报告 : Window
    {
        public 选择注意力报告(double average_time,int correct,int wrong,int forget)
        {
            InitializeComponent();
            var reportData = new ReportData选择注意力
            {
                CorrectCount = correct,
                IncorrectCount = wrong,
                Average_time = average_time,
                ForgetCount = forget,
                Date = DateTime.Now// 设置日期
            };
            dataGrid.ItemsSource = new List<ReportData选择注意力> { reportData };
        }
        public class ReportData选择注意力
        {
            public DateTime Date { get; set; } // 日期
            public int CorrectCount { get; set; } 
            public int IncorrectCount { get; set; } 
            public int ForgetCount { get; set; }
            public double Average_time { get; set; } // 平均反应时间
        }
    }
}
