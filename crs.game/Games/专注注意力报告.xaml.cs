using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace crs.game.Games
{
    public partial class 专注注意力报告 : Window
    {
        private int[] correctAnswers;
        private int[] wrongAnswers;
        private int[] ignoreAnswer;

        // 将 AnswerData 类设置为 private
        private class AnswerData
        {
            public int Hardness { get; set; }
            public int CorrectAnswers { get; set; }
            public int WrongAnswers { get; set; }
            public int IgnoreAnswer { get; set; } // 添加 IgnoreAnswer 属性
            public double Accuracy { get; set; } // 添加 Accuracy 属性
        }

        public 专注注意力报告(double increase, double decrease, int mt, int tt, bool irt, bool ib, int[] correctAnswers, int[] wrongAnswers, int[] ignoreAnswer)
        {
            InitializeComponent();
            this.correctAnswers = correctAnswers;
            this.wrongAnswers = wrongAnswers;
            this.ignoreAnswer = ignoreAnswer;


            // 设置参数数据
            var parameterData = new List<ParameterData>
            {
                new ParameterData { Name = "日期",Value=DateTime.Now.ToString("yyyy/MM/dd")},
                new ParameterData { Name = "等级提高", Value = increase },
                new ParameterData { Name = "等级降低", Value = decrease },
                new ParameterData { Name = "答题时间", Value = irt ? "是" : "否" },
                new ParameterData { Name = "声音反馈", Value = ib ? "是" : "否" }
            };

            Parameter.ItemsSource = parameterData; // 将参数数据绑定到 Parameter DataGrid
            LoadData(); // 加载答题数据
        }

        private void LoadData()
        {
            var dataList = new List<AnswerData>();

            for (int i = 0; i < correctAnswers.Length; i++)
            {
                if (correctAnswers[i] > 0 || wrongAnswers[i] > 0 || ignoreAnswer[i] > 0)
                {
                    int totalAnswers = correctAnswers[i] + wrongAnswers[i];
                    double accuracy = totalAnswers > 0 ? (double)correctAnswers[i] / totalAnswers : 0; // 计算正确率

                    dataList.Add(new AnswerData
                    {
                        Hardness = i,
                        CorrectAnswers = correctAnswers[i],
                        WrongAnswers = wrongAnswers[i],
                        IgnoreAnswer = ignoreAnswer[i],
                        Accuracy = accuracy
                    });
                }
            }

            dataGrid.ItemsSource = dataList; // 将答题数据绑定到 dataGrid
        }

        // 数据模型类用于参数数据
        private class ParameterData
        {
            public string Name { get; set; }
            public object Value { get; set; } // 使用 object 以支持不同类型的值
        }
    }
}
