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
    /// REVE_report.xaml 的交互逻辑
    /// </summary>
    public partial class 反应行为报告 : Window
    {
        private int[] Answers;
        private int[] wrongAnswers;

        private class AnswerData
        {
            public int Hardness { get; set; }
            public int CorrectAnswers { get; set; }
            public int WrongAnswers { get; set; }
            public double Accuracy { get; set; }
        }

        public 反应行为报告(double increase,double decrease, int STIMULI_AMOUNT, int STIMULI_INTERVAL, bool IS_BEEP, bool IS_SCREEN,int[] Answers, int[] wrongAnswers)
        {
            InitializeComponent();
            this.Answers = Answers;
            this.wrongAnswers = wrongAnswers;


            // 设置参数数据
            var parameterData = new List<ParameterData>
            {
                new ParameterData { Name = "日期",Value=DateTime.Now.ToString("yyyy/MM/dd")},
                new ParameterData { Name = "等级提高", Value = increase },
                new ParameterData { Name = "等级降低", Value = decrease },
                new ParameterData {Name = "刺激间隔[ms]", Value = STIMULI_INTERVAL},
                new ParameterData {Name = "刺激数量", Value = STIMULI_AMOUNT},
                new ParameterData { Name = "视觉反馈", Value = IS_SCREEN ? "是" : "否" },
                new ParameterData { Name = "听觉反馈", Value = IS_BEEP ? "是" : "否" }
            };

            Parameter.ItemsSource = parameterData; // 将参数数据绑定到 Parameter DataGrid
            LoadData(); // 加载答题数据
        }

        private void LoadData()
        {
            var dataList = new List<AnswerData>();

            for (int i = 0; i < Answers.Length; i++)
            {
                if (Answers[i] > 0)
                {
                    double accuracy = (Answers[i] - wrongAnswers[i]) > 0 ? ((double)(Answers[i] - wrongAnswers[i]) / Answers[i]) : 0; // 计算正确率

                    dataList.Add(new AnswerData
                    {
                        Hardness = i,
                        CorrectAnswers = Answers[i] - wrongAnswers[i],
                        WrongAnswers = wrongAnswers[i],
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
