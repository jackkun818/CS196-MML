using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class Therapist
{
    public int TherapistId { get; set; }

    public virtual ICollection<Program> Programs { get; set; } = new List<Program>();

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
