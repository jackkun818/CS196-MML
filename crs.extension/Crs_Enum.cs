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
            [Description("Users")]
            UserManagement,
            [Description("Testing")]
            EvaluateTest,
            [Description("Training")]
            Train,
            [Description("Schedule")]
            Schedule,
            [Description("Reports")]
            Report,
            [Description("AI Helper")]
            DigitalHuman,
        }

        public enum EvaluateTestMode
        {
            标准评估,
            记忆广度,
            警觉能力,
            视觉分配能力,
            词语记忆能力,
            选择注意力,
            视野,
            逻辑推理能力,
            空间数字搜索,
            平面视野
        }

        public enum TrainType
        {
            注意力训练,
            记忆能力训练,
            思维能力训练,
            知觉障碍训练,
            眼动训练,
        }

        public enum TrainMode
        {
            // 注意力训练
            专注注意力,
            注意力分配,
            注意力分配2,

            // 记忆训练
            工作记忆力,
            图形记忆力,
            词语记忆力,
            拓扑记忆力,
            容貌记忆力,
            细节记忆力,

            // 思维能力训练
            反应行为,
            警觉训练,
            反应能力,
            警惕训练,
            平面识别能力,

            // 知觉障碍训练
            视觉修复训练,
            视空间训练,
            逻辑思维能力,
            搜索能力1,
            搜索能力2,
            搜索能力3,


            //眼动训练
            眼动跟随,
            眼动驱动,
            算术游戏,
            拼图,
            打蚊子

        }

        public enum ScheduleType
        {
            今日排班,
            每日排班
        }

        public enum ProgramType
        {
            评估测试,
            康复训练
        }

        public enum ScheduleStatus
        {
            未报道,
            已报道,
            已过号,
            已完成
        }

        public enum ReportType
        {
            评估报告,
            训练报告
        }

        public enum EvaluateStandardType
        {
            MoCA量表,
            MMSE量表
        }

        public enum SexImgType
        {
            男生头像,
            女生头像
        }

        public enum SexType
        {
            男,
            女
        }
    }
}
