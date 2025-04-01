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
    /// REVE_report.xaml Interaction logic
    /// </summary>
    public partial class Response behavior report : Window
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

        public Response behavior report(double increase,double decrease, int STIMULI_AMOUNT, int STIMULI_INTERVAL, bool IS_BEEP, bool IS_SCREEN,int[] Answers, int[] wrongAnswers)
        {
            InitializeComponent();
            this.Answers = Answers;
            this.wrongAnswers = wrongAnswers;


            // Set parameter data
            var parameterData = new List<ParameterData>
            {
                new ParameterData { Name = "date",Value=DateTime.Now.ToString("yyyy/MM/dd")},
                new ParameterData { Name = "Level improvement", Value = increase },
                new ParameterData { Name = "Level down", Value = decrease },
                new ParameterData {Name = "Stimulation interval[ms]", Value = STIMULI_INTERVAL},
                new ParameterData {Name = "Number of stimulus", Value = STIMULI_AMOUNT},
                new ParameterData { Name = "Visual feedback", Value = IS_SCREEN ? "yes" : "no" },
                new ParameterData { Name = "Auditory feedback", Value = IS_BEEP ? "yes" : "no" }
            };

            Parameter.ItemsSource = parameterData; // Bind parameter data to Parameter DataGrid
            LoadData(); // Load answer data
        }

        private void LoadData()
        {
            var dataList = new List<AnswerData>();

            for (int i = 0; i < Answers.Length; i++)
            {
                if (Answers[i] > 0)
                {
                    double accuracy = (Answers[i] - wrongAnswers[i]) > 0 ? ((double)(Answers[i] - wrongAnswers[i]) / Answers[i]) : 0; // Calculate the correct rate

                    dataList.Add(new AnswerData
                    {
                        Hardness = i,
                        CorrectAnswers = Answers[i] - wrongAnswers[i],
                        WrongAnswers = wrongAnswers[i],
                        Accuracy = accuracy
                    });
                }
            }

            dataGrid.ItemsSource = dataList; // Bind answer data to dataGrid
        }

        // Data model class is used for parameter data
        private class ParameterData
        {
            public string Name { get; set; }
            public object Value { get; set; } // use object To support different types of values
        }
    }
}
