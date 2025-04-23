using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace crs.game.Games
{
    public partial class Focus_on_attention_report : Window
    {
        private int[] correctAnswers;
        private int[] wrongAnswers;
        private int[] ignoreAnswer;

        // Will AnswerData Class set to private
        private class AnswerData
        {
            public int Hardness { get; set; }
            public int CorrectAnswers { get; set; }
            public int WrongAnswers { get; set; }
            public int IgnoreAnswer { get; set; } // Add to IgnoreAnswer property
            public double Accuracy { get; set; } // Add to Accuracy property
        }

        public Focus_on_attention_report(double increase, double decrease, int mt, int tt, bool irt, bool ib, int[] correctAnswers, int[] wrongAnswers, int[] ignoreAnswer)
        {
            InitializeComponent();
            this.correctAnswers = correctAnswers;
            this.wrongAnswers = wrongAnswers;
            this.ignoreAnswer = ignoreAnswer;


            // Set parameter data
            var parameterData = new List<ParameterData>
            {
                new ParameterData { Name = "date",Value=DateTime.Now.ToString("yyyy/MM/dd")},
                new ParameterData { Name = "Level improvement", Value = increase },
                new ParameterData { Name = "Level down", Value = decrease },
                new ParameterData { Name = "Answer time", Value = irt ? "yes" : "no" },
                new ParameterData { Name = "Voice feedback", Value = ib ? "yes" : "no" }
            };

            Parameter.ItemsSource = parameterData; // Bind parameter data to Parameter DataGrid
            LoadData(); // Load answer data
        }

        private void LoadData()
        {
            var dataList = new List<AnswerData>();

            for (int i = 0; i < correctAnswers.Length; i++)
            {
                if (correctAnswers[i] > 0 || wrongAnswers[i] > 0 || ignoreAnswer[i] > 0)
                {
                    int totalAnswers = correctAnswers[i] + wrongAnswers[i];
                    double accuracy = totalAnswers > 0 ? (double)correctAnswers[i] / totalAnswers : 0; // Calculate the correct rate

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
