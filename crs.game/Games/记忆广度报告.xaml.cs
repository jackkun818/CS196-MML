using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace crs.game.Games
{
    /// <summary>
    /// 记忆广度报告.xaml 的交互逻辑
    /// </summary>
    public partial class 记忆广度报告 : Window
    {
        public 记忆广度报告(Dictionary<int, int> errorCounts)
        {
            InitializeComponent();

            // 创建一个列表来存储所有的报告数据
            List<ReportData记忆广度> reportDataList = new List<ReportData记忆广度>();

            // 遍历 errorCounts 字典
            foreach (var entry in errorCounts)
            {
                int level = entry.Key; // 当前展示的方块数量
                int incorrectCount = entry.Value; // 错误次数

                // 创建一个新的 ReportData 实例
                var reportData = new ReportData记忆广度
                {
                    Level = level,
                    Score = 2 * level - incorrectCount, // 计算分数
                    IncorrectCount = incorrectCount,
                    Date = DateTime.Now // 设置日期
                };

                // 将 ReportData 添加到列表中
                reportDataList.Add(reportData);
            }

            // 将数据模型绑定到 DataGrid
            dataGrid.ItemsSource = reportDataList; // 假设 dataGrid 是你的 DataGrid 控件
        }

        public class ReportData记忆广度
        {
            public int Level { get; set; }
            public int Score { get; set; }
            public int IncorrectCount { get; set; }
            public DateTime Date { get; set; } // 新增日期属性
        }
    }
}
