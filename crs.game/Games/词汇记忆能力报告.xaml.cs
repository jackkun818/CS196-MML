using System;
using System.Collections.Generic;
using System.Windows;

namespace crs.game.Games
{
    public partial class 词汇记忆能力报告 : Window
    {
        public 词汇记忆能力报告(int score, int totalTests, int incorrectCount, int skippedCount)
        {
            InitializeComponent();

            // 创建数据模型实例
            var reportData = new ReportData词汇记忆能力
            {
                TotalTests = totalTests,
                Score = score,
                IncorrectCount = incorrectCount,
                SkippedCount = skippedCount,
                Date =DateTime.Now// 设置日期
            };

            // 将数据模型绑定到DataGrid
            dataGrid.ItemsSource = new List<ReportData词汇记忆能力> { reportData };
        }
    }

    public class ReportData词汇记忆能力
    {
        public int TotalTests { get; set; }
        public int Score { get; set; }
        public int IncorrectCount { get; set; }
        public int SkippedCount { get; set; }
        public DateTime Date { get; set; } // 新增日期属性
    }
}
