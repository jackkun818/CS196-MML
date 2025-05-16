using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class RoleClaim
{
    public int Id { get; set; }

    public string Description { get; set; }

    public string Group { get; set; }

    public string RoleId { get; set; }

    public string ClaimType { get; set; }

    public string ClaimValue { get; set; }

    public virtual Role Role { get; set; }
}
