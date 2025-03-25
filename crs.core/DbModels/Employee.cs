using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class Employee
{
    public string Id { get; set; }

    public string OrganizationId { get; set; }

    public string EmployeeCode { get; set; }

    public string Department { get; set; }

    public int? Sex { get; set; }

    public virtual User IdNavigation { get; set; }

    public virtual Organization Organization { get; set; }

    public virtual ICollection<OrganizationPatient> OrganizationPatients { get; set; } = new List<OrganizationPatient>();
}
