using crs.core.DbModels;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static crs.extension.Crs_Enum;

namespace crs.extension.Models
{
    public class TrainItem : BindableBase
    {
        private TrainType type;
        public TrainType Type
        {
            get { return type; }
            set { SetProperty(ref type, value); }
        }

        private TrainMode mode;
        public TrainMode Mode
        {
            get { return mode; }
            set { SetProperty(ref mode, value); }
        }

        private ObservableCollection<ModuleParItem> moduleParItems;
        public ObservableCollection<ModuleParItem> ModuleParItems
        {
            get { return moduleParItems; }
            set { SetProperty(ref moduleParItems, value); }
        }

        private ObservableCollection<ModuleParItem> feedbackModuleParItems;
        public ObservableCollection<ModuleParItem> FeedbackModuleParItems
        {
            get { return feedbackModuleParItems; }
            set { SetProperty(ref feedbackModuleParItems, value); }
        }

        private string messageInfo;
        public string MessageInfo
        {
            get { return messageInfo; }
            set { SetProperty(ref messageInfo, value); }
        }

        public Module Data { get; private set; }
        public ModuleEx DataEx { get; set; }

        public TrainItem Update(Module data)
        {
            Data = data;
            DataEx = data.ClassMapper<ModuleEx, Module>();

            return this;
        }

        public TrainItem UpdateMessageInfo()
        {
            MessageInfo = GetMessageInfo();
            return this;
        }

        string GetMessageInfo()
        {
            StringBuilder builder = null;
            var moduleParItems = ModuleParItems;
            if (moduleParItems != null && moduleParItems.Count > 0)
            {
                builder ??= new StringBuilder();
                foreach (var item in moduleParItems)
                {
                    builder.AppendLine($"{item.Name}：{item.DefaultValue} {item.Unit}");
                }
            }
            var feedbackModuleParItems = FeedbackModuleParItems;
            if (feedbackModuleParItems != null && feedbackModuleParItems.Count > 0)
            {
                builder ??= new StringBuilder();
                var selectedItems = feedbackModuleParItems.Where(m => m.IsChecked).ToList();
                if (selectedItems.Count > 0)
                {
                    builder.AppendLine($"反馈：{string.Join("、", selectedItems.Select(m => m.Name))}");
                }
            }

            if (builder != null)
            {
                if (builder.Length > 0)
                {
                    builder.Remove(builder.Length - 2, 2);
                }
                return builder.ToString();
            }
            return null;
        }
    }
}
