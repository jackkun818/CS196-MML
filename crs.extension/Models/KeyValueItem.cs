using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace crs.extension.Models
{
    public class KeyValueItem<TKey, TValue> : BindableBase
    {
        public KeyValueItem() { }
        public KeyValueItem(TKey key, TValue value)
        {
            this.key = key;
            this._value = value;
        }

        private TKey key;
        public TKey Key
        {
            get { return key; }
            set { SetProperty(ref key, value); }
        }

        private TValue _value;
        public TValue Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }
    }

    public class KeyValueItem<TKey, TValue1, TValue2> : BindableBase
    {
        public KeyValueItem() { }
        public KeyValueItem(TKey key, TValue1 value1, TValue2 value2)
        {
            this.key = key;
            this._value1 = value1;
            this._value2 = value2;
        }

        private TKey key;
        public TKey Key
        {
            get { return key; }
            set { SetProperty(ref key, value); }
        }

        private TValue1 _value1;
        public TValue1 Value1
        {
            get { return _value1; }
            set { SetProperty(ref _value1, value); }
        }

        private TValue2 _value2;
        public TValue2 Value2
        {
            get { return _value2; }
            set { SetProperty(ref _value2, value); }
        }
    }
}
