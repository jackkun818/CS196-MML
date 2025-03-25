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
    /// 逻辑推理能力报告.xaml 的交互逻辑
    /// </summary>
    public partial class 逻辑推理能力报告 : Window
    {
        public 逻辑推理能力报告(double average_time,int correctCount,int incorrectCount)
        {
            InitializeComponent();

            var reportData = new ReportData逻辑推理能力
            {
                CorrectCount=correctCount,
                IncorrectCount=incorrectCount,
                Average_time = average_time,
                Date = DateTime.Now// 设置日期
            };
            dataGrid.ItemsSource = new List<ReportData逻辑推理能力> { reportData };
        }
        public class ReportData逻辑推理能力
        {
            public DateTime Date { get; set; } // 日期
            public int CorrectCount { get; set; } 
            public int IncorrectCount { get; set; } 
            public double Average_time { get; set; } // 平均反应时间
        }
    }
}
