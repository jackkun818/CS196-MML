using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class Role
{
    public string Id { get; set; }

    public string Description { get; set; }

    public string Name { get; set; }

    public string NormalizedName { get; set; }

    public string ConcurrencyStamp { get; set; }

    public virtual ICollection<RoleClaim> RoleClaims { get; set; } = new List<RoleClaim>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
