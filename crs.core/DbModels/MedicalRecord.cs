using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class MedicalRecord
{
    public int Id { get; set; }

    public bool SoftDeleted { get; set; }

    public DateTime CheckInDate { get; set; }

    public int AdmissionState { get; set; }

    public string BedCode { get; set; }

    public byte Archived { get; set; }

    public bool AllOrdersConfirmed { get; set; }

    public string Dysfunctions { get; set; }

    public byte AffectedSide { get; set; }

    public string ChiefComplaints { get; set; }

    public string HistoriesOfPresentComplaints { get; set; }

    public string PastMedicalHistories { get; set; }

    public string PersonalMarriageFamilyHistories { get; set; }

    public string PhysicalExamination { get; set; }

    public string Auxiliarylnspection { get; set; }

    public string Diagnosis { get; set; }

    public int OrgPatientld { get; set; }

    public string Organizationld { get; set; }

    public string CreatedBy { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public string LastModifiedBy { get; set; }

    public DateTimeOffset? LastModifiedOn { get; set; }

    public virtual ICollection<MedicalAdviecsOrder> MedicalAdviecsOrders { get; set; } = new List<MedicalAdviecsOrder>();

    public virtual ICollection<OrganizationPatient> OrganizationPatients { get; set; } = new List<OrganizationPatient>();
}
