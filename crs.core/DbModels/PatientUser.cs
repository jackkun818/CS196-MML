using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class PatientUser
{
    public int PatientId { get; set; }

    public int? MegaHumanId { get; set; }

    public virtual MegaHuman MegaHuman { get; set; }

    public virtual Patient Patient { get; set; }

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
