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
using static crs.game.Games.逻辑推理能力报告;

namespace crs.game.Games
{
    /// <summary>
    /// 空间数字搜索报告.xaml 的交互逻辑
    /// </summary>
    public partial class 空间数字搜索报告 : Window
    {
        public 空间数字搜索报告(int incorrectCount, double[] timesInterval)
        {
            InitializeComponent();
            var reportData = new ReportData空间数字搜索
            {
                IncorrectCount = incorrectCount,
                Date = DateTime.Now// 设置日期
            };
            dataGrid.ItemsSource = new List<ReportData空间数字搜索> { reportData };
            int itemsToDisplay = Math.Min(24, timesInterval.Length);

            for (int i = 0; i < itemsToDisplay; i++)
            {
                // 创建一个 TextBlock 显示每个时间间隔
                TextBlock textBlock = new TextBlock
                {
                    Text = timesInterval[i].ToString("F2"), // 格式化为小数点后两位
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5)
                };

                // 将 TextBlock 添加到 UniformGrid
                timesIntervalGrid.Children.Add(textBlock);
            }
        }
        public class ReportData空间数字搜索
        {
            public DateTime Date { get; set; } // 日期
            public int IncorrectCount { get; set; }
        }
    }
}
