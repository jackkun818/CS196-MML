using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class Program
{
    public int ProgramId { get; set; }

    public int? ScheduleId { get; set; }

    public int? ProcessTherapistId { get; set; }

    public int? EquipmentId { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public DateTime? ActStartTime { get; set; }

    public DateTime? ActEndTime { get; set; }

    public string Remark { get; set; }

    public bool? SoftDel { get; set; }

    public bool? Eval { get; set; }

    public int? PatientId { get; set; }

    public DateTime? CreateTime { get; set; }

    public virtual Equipment Equipment { get; set; }

    public virtual ICollection<ModuleResult> ModuleResults { get; set; } = new List<ModuleResult>();

    public virtual OrganizationPatient Patient { get; set; }

    public virtual Therapist ProcessTherapist { get; set; }

    public virtual ICollection<ProgramModule> ProgramModules { get; set; } = new List<ProgramModule>();

    public virtual ICollection<QuestionResult> QuestionResults { get; set; } = new List<QuestionResult>();

    public virtual ICollection<Result> Results { get; set; } = new List<Result>();

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
