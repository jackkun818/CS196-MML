using crs.core.DbModels;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using static crs.extension.Crs_Enum;

namespace crs.extension.Models
{
    public class PatientItem : BindableBase
    {
        private string name;
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        private string phoneNumber;
        public string PhoneNumber
        {
            get { return phoneNumber; }
            set { SetProperty(ref phoneNumber, value); }
        }

        private SexImgType sexImgType;
        public SexImgType SexImgType
        {
            get { return sexImgType; }
            set { SetProperty(ref sexImgType, value); }
        }

        private SexType sexType;
        public SexType SexType
        {
            get { return sexType; }
            set { SetProperty(ref sexType, value); }
        }

        private int? age;
        public int? Age
        {
            get { return age; }
            set { SetProperty(ref age, value); }
        }

        public OrganizationPatient Data { get; private set; }

        public PatientItem Update(OrganizationPatient data)
        {
            Data = data;

            Name = data.FullName;
            PhoneNumber = data.PhoneNumber;
            SexImgType = data.Sex switch
            {
                0 => SexImgType.男生头像,
                1 => SexImgType.女生头像,
                _ => SexImgType.男生头像
            };
            SexType = data.Sex switch
            {
                0 => SexType.男,
                1 => SexType.女,
                _ => SexType.男
            };
            Age = data.Age;

            return this;
        }
    }
}
