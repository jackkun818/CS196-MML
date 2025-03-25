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
    /// 警觉能力报告.xaml 的交互逻辑
    /// </summary>
    public partial class 警觉能力报告 : Window
    {
        public 警觉能力报告(double average_time,bool is_beep,int forget)
        {
            InitializeComponent();
            var reportData = new ReportData警觉能力
            {
                Alarm = is_beep?"有警告音":"无警告音",
                Forget = forget,
                Average_time = average_time,
                Date = DateTime.Now// 设置日期
            };
            dataGrid.ItemsSource = new List<ReportData警觉能力> { reportData };
        }
    }
    public class ReportData警觉能力
    {
        public DateTime Date { get; set; } // 日期
        public string Alarm { get; set; } // 状况
        public int Forget { get; set; } // 遗漏次数
        public double Average_time { get; set; } // 平均反应时间
    }
}
