using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class Patient
{
    public int Id { get; set; }

    public string UserId { get; set; }

    public string FullName { get; set; }

    public string PhoneNumber { get; set; }

    public string IdentifyNum { get; set; }

    public string CurrentOrganizationId { get; set; }

    public int? CurrentOrgPatientId { get; set; }

    public virtual PatientUser PatientUser { get; set; }

    public virtual PersonalUser User { get; set; }
}
