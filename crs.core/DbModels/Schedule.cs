using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class Schedule
{
    public int ScheduleId { get; set; }

    public int PatientId { get; set; }

    public int TherapistId { get; set; }

    public int? ProgramId { get; set; }

    public DateTime CreateTime { get; set; }

    public string Remark { get; set; }

    public bool? SoftDel { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string Status { get; set; }

    public virtual PatientUser Patient { get; set; }

    public virtual Program Program { get; set; }

    public virtual ICollection<Result> Results { get; set; } = new List<Result>();

    public virtual Therapist Therapist { get; set; }
}
