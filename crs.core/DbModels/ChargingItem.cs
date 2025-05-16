using System;
using System.Collections.Generic;

namespace crs.core.DbModels;

public partial class ChargingItem
{
    public int Id { get; set; }

    public byte? DeviceType { get; set; }

    public string Name { get; set; }

    public decimal Price { get; set; }

    public string Description { get; set; }

    public string OrganizationId { get; set; }

    public bool SoftDeleted { get; set; }

    public string CreatedBy { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public string LastModifiedBy { get; set; }

    public DateTimeOffset LastModifiedOn { get; set; }
}
