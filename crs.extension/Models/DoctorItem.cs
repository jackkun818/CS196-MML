using crs.core.DbModels;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace crs.extension.Models
{
    public class DoctorItem : BindableBase
    {
        private string name;
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        public User Data { get; private set; }

        public DoctorItem Update(User data)
        {
            Data = data;

            Name = data.UserName;

            return this;
        }
    }
}
