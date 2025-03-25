using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class MedicalAdvice
{
    public int Id { get; set; }

    public bool SoftDeleted { get; set; }

    public int MedicalAdvicesOrderId { get; set; }

    public byte FrequencyType { get; set; }

    public int NumOfDates { get; set; }

    public byte DeviceType { get; set; }

    public int ExpectedScheduleCount { get; set; }

    public int FinishedScheduleCount { get; set; }

    public int Status { get; set; }

    public int Duration { get; set; }

    public string Note { get; set; }

    public string TrainingModes { get; set; }

    public string ExecuteDates { get; set; }

    public string OrganizationId { get; set; }

    public string CreatedBy { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public string LastModifiedBy { get; set; }

    public DateTimeOffset? LastModifiedOn { get; set; }

    public virtual MedicalAdviecsOrder MedicalAdvicesOrder { get; set; }
}
