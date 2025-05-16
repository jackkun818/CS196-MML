using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class Edition
{
    public int TherapistId { get; set; }

    public int ProgramId { get; set; }

    public DateTime EditTime { get; set; }

    public virtual Program Program { get; set; }

    public virtual Therapist Therapist { get; set; }
}
