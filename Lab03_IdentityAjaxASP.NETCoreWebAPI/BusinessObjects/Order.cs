using System;
using System.Collections.Generic;

namespace BusinessObjects;

public partial class Order
{
    public int Id { get; set; }

    public int? AccountId { get; set; }

    public DateOnly? OrderDate { get; set; }

    public string? OrderStatus { get; set; }

    public decimal? TotalAmount { get; set; }

    public virtual Account? Account { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
