using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace crs.extension.Models
{
    public class MultiItem<TItem1> : BindableBase
    {
        private TItem1 item1;
        public TItem1 Item1
        {
            get { return item1; }
            set { SetProperty(ref item1, value); }
        }
    }

    public class MultiItem<TItem1, TItem2> : BindableBase
    {
        private TItem1 item1;
        public TItem1 Item1
        {
            get { return item1; }
            set { SetProperty(ref item1, value); }
        }

        private TItem2 item2;
        public TItem2 Item2
        {
            get { return item2; }
            set { SetProperty(ref item2, value); }
        }
    }

    public class MultiItem<TItem1, TItem2, TItem3> : BindableBase
    {
        private TItem1 item1;
        public TItem1 Item1
        {
            get { return item1; }
            set { SetProperty(ref item1, value); }
        }

        private TItem2 item2;
        public TItem2 Item2
        {
            get { return item2; }
            set { SetProperty(ref item2, value); }
        }

        private TItem3 item3;
        public TItem3 Item3
        {
            get { return item3; }
            set { SetProperty(ref item3, value); }
        }
    }
}
