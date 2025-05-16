using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class OrganizationPatient
{
    public int Id { get; set; }

    public string FullName { get; set; }

    public DateOnly? BirthDate { get; set; }

    public int? Sex { get; set; }

    public string PhoneNumber { get; set; }

    public string Notes { get; set; }

    public bool? IsDischarged { get; set; }

    public int? BodyParametersId { get; set; }

    public int? LatestMedRecordId { get; set; }

    public string OrganizationId { get; set; }

    public string DoctorId { get; set; }

    public bool? SoftDeleted { get; set; }

    public string CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string LastModifiedBy { get; set; }

    public DateTime? LastModifiedOn { get; set; }

    public int? Age { get; set; }

    public string Province { get; set; }

    public string Career { get; set; }

    public virtual Employee Doctor { get; set; }

    public virtual MedicalRecord LatestMedRecord { get; set; }

    public virtual Organization Organization { get; set; }

    public virtual ICollection<Program> Programs { get; set; } = new List<Program>();
}
