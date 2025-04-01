using HandyControl.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;

namespace crs.extension
{
    public class Crs_Enum
    {
        public enum MenuType
        {
            [Description("User Management")]
            UserManagement,
            [Description("Evaluation test")]
            EvaluateTest,
            [Description("Rehabilitation training")]
            Train,
            [Description("Schedule inquiry")]
            Schedule,
            [Description("Data Report")]
            Report,
            [Description("Digital Man Management")]
            DigitalHuman,
        }

        public enum EvaluateTestMode
        {
            Standard evaluation,
            Broadness of memory,
            Alert ability,
            Visual distribution capability,
            Word memory ability,
            Select attention,
            Vision,
            Logical reasoning ability,
            Space digital search,
            Planar view
        }

        public enum TrainType
        {
            Attention training,
            Memory training,
            Thinking Ability Training,
            Perception impairment training,
            Eye movement training,
        }

        public enum TrainMode
        {
            // Attention training
            Focus on attention,
            Attention distribution,
            Attention Distribution 2,

            // Memory training
            Working memory,
            Graphic memory,
            Word memory,
            Topological memory,
            Appearance and memory,
            Detail memory,

            // Thinking Ability Training
            Response behavior,
            Alert training,
            Response ability,
            Beware of training,
            Plane recognition capability,

            // Perception impairment training
            Visual repair training,
            Visual space training,
            Logical thinking ability,
            Search capability 1,
            Search capability 2,
            Search capability 3,


            //Eye movement training
            Eye movement follow,
            Eye movement drive,
            Arithmetic game,
            puzzle,
            Kill mosquitoes

        }

        public enum ScheduleType
        {
            Schedule today,
            Daily shift schedule
        }

        public enum ProgramType
        {
            Evaluation test,
            Rehabilitation training
        }

        public enum ScheduleStatus
        {
            Not reported,
            Reported,
            Passed number,
            Completed
        }

        public enum ReportType
        {
            Evaluation report,
            Training Report
        }

        public enum EvaluateStandardType
        {
            MoCAScale,
            MMSEScale
        }

        public enum SexImgType
        {
            Boys avatar,
            Girls' avatar
        }

        public enum SexType
        {
            male,
            female
        }
    }
}
