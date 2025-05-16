using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class MedicalAdviecsOrder
{
    public int Id { get; set; }

    public int MedicalRecordId { get; set; }

    public string Organizationld { get; set; }

    public int OrgPatientld { get; set; }

    public DateTimeOffset? SubmittedOn { get; set; }

    public bool Confirmed { get; set; }

    public virtual ICollection<MedicalAdvice> MedicalAdvices { get; set; } = new List<MedicalAdvice>();

    public virtual MedicalRecord MedicalRecord { get; set; }
}
