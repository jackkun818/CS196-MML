using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class Organization
{
    public string Id { get; set; }

    public string Province { get; set; }

    public string City { get; set; }

    public string Address { get; set; }

    public string Description { get; set; }

    public bool? IsVerified { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual User IdNavigation { get; set; }

    public virtual ICollection<OrganizationPatient> OrganizationPatients { get; set; } = new List<OrganizationPatient>();
}
