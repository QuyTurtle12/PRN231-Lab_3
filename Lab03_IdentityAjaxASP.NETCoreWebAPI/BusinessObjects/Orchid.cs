using System;
using System.Collections.Generic;

namespace BusinessObjects;

public partial class Orchid
{
    public int OrchidId { get; set; }

    public bool? IsNatural { get; set; }

    public string? OrchidDescription { get; set; }

    public string? OrchidName { get; set; }

    public string? OrchidUrl { get; set; }

    public decimal? Price { get; set; }

    public int? CategoryId { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
