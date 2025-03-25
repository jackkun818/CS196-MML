using crs.core.DbModels;
using crs.extension.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static crs.extension.Crs_Enum;

namespace crs.extension
{
    public class Crs_EventAggregator
    {
        public class WindowStateChangedEvent : PubSubEvent<bool> { }

        public class PatientSelectedChangedEvent : PubSubEvent<PatientItem> { }

        public class MenuSelectedChangedEvent : PubSubEvent<(MenuType, bool)> { }
    }
}
