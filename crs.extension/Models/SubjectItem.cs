using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Ink;
using static crs.extension.Crs_Enum;

namespace crs.extension.Models
{
    public class SubjectChildrenItem : BindableBase
    {
        public SubjectChildrenItem()
        {
            answerItems = new ObservableCollection<AnswerItem>(Enumerable.Range(0, 16).Select(m =>
            {
                var item = new AnswerItem();
                item.PropertyChanged += Item_PropertyChanged;
                return item;
            }));

            strokeItem = new StrokeCollection();
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(AnswerItem.IsRight) && e.PropertyName != nameof(AnswerItem.IsWrong) && e.PropertyName != nameof(AnswerItem.IsUse))
            {
                return;
            }

            IsUse = true;
            ChangedNum++;
        }

        private StrokeCollection strokeItem;
        public StrokeCollection StrokeItem
        {
            get { return strokeItem; }
            set { SetProperty(ref strokeItem, value); }
        }

        private ObservableCollection<AnswerItem> answerItems;
        public ObservableCollection<AnswerItem> AnswerItems
        {
            get { return answerItems; }
            set { SetProperty(ref answerItems, value); }
        }

        private byte[] audioData;
        public byte[] AudioData
        {
            get { return audioData; }
            set { SetProperty(ref audioData, value); }
        }

        private bool isUseAudio;
        public bool IsUseAudio
        {
            get { return isUseAudio; }
            set { SetProperty(ref isUseAudio, value); }
        }

        private byte[] bitmapData;
        public byte[] BitmapData
        {
            get { return bitmapData; }
            set { SetProperty(ref bitmapData, value); }
        }

        private bool isUseBitmap;
        public bool IsUseBitmap
        {
            get { return isUseBitmap; }
            set { SetProperty(ref isUseBitmap, value); }
        }

        private bool isUse;
        public bool IsUse
        {
            get { return isUse; }
            set { SetProperty(ref isUse, value); }
        }

        private int changedNum;
        public int ChangedNum
        {
            get { return changedNum; }
            set { SetProperty(ref changedNum, value); }
        }
    }

    public class SubjectItem : BindableBase
    {
        public SubjectItem()
        {
            childrenItems = new ObservableCollection<SubjectChildrenItem>(Enumerable.Range(0, 16).Select(m =>
            {
                var item = new SubjectChildrenItem();
                item.PropertyChanged += Item_PropertyChanged;
                return item;
            }));
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(SubjectChildrenItem.IsUse) && e.PropertyName != nameof(SubjectChildrenItem.ChangedNum))
            {
                return;
            }

            try
            {
                var answerItems = (from childrenItem in ChildrenItems.Where(m => m.IsUse) from answerItem in childrenItem.AnswerItems.Where(m => m.IsUse) select answerItem).ToList();

                if (StandardType == EvaluateStandardType.MoCAScale && (Name == "Question 3" || Name == "Question 10"))
                {
                    AllAnswerCount = answerItems.Count(m => !m.Ignore);
                    RightAnswerCount = answerItems.Count(m => !m.Ignore && m.IsRight);
                    WrongAnswerCount = AllAnswerCount - RightAnswerCount;
                    return;
                }

                if (StandardType == EvaluateStandardType.MoCAScale && Name == "Question 6")
                {
                    AllAnswerCount = 3;
                    RightAnswerCount = answerItems.Count(m => m.IsRight) switch
                    {
                        >= 4 => 3,
                        >= 2 => 2,
                        >= 1 => 1,
                        _ => 0
                    };
                    WrongAnswerCount = AllAnswerCount - RightAnswerCount;
                    return;
                }

                AllAnswerCount = answerItems.Count();
                RightAnswerCount = answerItems.Count(m => m.IsRight);
                WrongAnswerCount = answerItems.Count(m => m.IsWrong);
            }
            finally
            {
                if ((StandardType == EvaluateStandardType.MoCAScale && Name == "Question 3"))
                {
                    Fraction = "No score";
                }
                else
                {
                    Fraction = $"{RightAnswerCount}/{AllAnswerCount}";
                }
            }
        }

        public string TemplateName => $"{StandardType}.{Name}";

        private string name;
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        private string originName;
        public string OriginName
        {
            get { return originName; }
            set { SetProperty(ref originName, value); }
        }

        private EvaluateStandardType standardType;
        public EvaluateStandardType StandardType
        {
            get { return standardType; }
            set { SetProperty(ref standardType, value); }
        }

        private bool isFirst;
        public bool IsFirst
        {
            get { return isFirst; }
            set { SetProperty(ref isFirst, value); }
        }

        private bool isLast;
        public bool IsLast
        {
            get { return isLast; }
            set { SetProperty(ref isLast, value); }
        }

        private bool isComplete;
        public bool IsComplete
        {
            get { return isComplete; }
            set { SetProperty(ref isComplete, value); }
        }

        private int allAnswerCount;
        public int AllAnswerCount
        {
            get { return allAnswerCount; }
            set { SetProperty(ref allAnswerCount, value); }
        }

        private int rightAnswerCount;
        public int RightAnswerCount
        {
            get { return rightAnswerCount; }
            set { SetProperty(ref rightAnswerCount, value); }
        }

        private int wrongAnswerCount;
        public int WrongAnswerCount
        {
            get { return wrongAnswerCount; }
            set { SetProperty(ref wrongAnswerCount, value); }
        }

        private string fraction;
        public string Fraction
        {
            get { return fraction; }
            set { SetProperty(ref fraction, value); }
        }

        private ObservableCollection<SubjectChildrenItem> childrenItems;
        public ObservableCollection<SubjectChildrenItem> ChildrenItems
        {
            get { return childrenItems; }
            set { SetProperty(ref childrenItems, value); }
        }
    }
}
