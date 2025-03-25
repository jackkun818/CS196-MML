using crs.extension.Models;
using crs.game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace crs.extension
{
    public class Crs_Interface
    {
        public interface IGameHost
        {
            bool Init(DigitalHumanItem humanItem, PatientItem patientItem, Enum modeType);
            bool ShowDemoInfo(FrameworkElement element, string message);
            bool Show(IGameBase gameContent);
            bool Remove(IGameBase gameContent = null);
            bool ShowTime(int? totalCountdownTime, int? currentCountdownTime);

            Task VoicePlayAsync(string message);

            void SetRuleContent(string content);
            //LJN
            void SetTitleVisible(bool Visiblity);
            //LJN
            void Close();
        }

        public interface IEvaluateStandardHost
        {
            void Init(object dataContext);
            void Close();
        }
    }
}
