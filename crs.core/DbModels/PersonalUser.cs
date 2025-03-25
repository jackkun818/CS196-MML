using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class PersonalUser
{
    public string Id { get; set; }

    public int? CurrentPatientId { get; set; }

    public virtual User IdNavigation { get; set; }

    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();
}
