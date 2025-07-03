using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects;

public partial class Orchid
{
    public int OrchidId { get; set; }

    [Required(ErrorMessage = "Need to define natural or hybrid")]
    public bool? IsNatural { get; set; }

    public string? OrchidDescription { get; set; }

    [Required(ErrorMessage = "Orchid name is required")]
    public string? OrchidName { get; set; }

    public string? OrchidUrl { get; set; }

    [Required(ErrorMessage = "Price is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal? Price { get; set; }

    [Required(ErrorMessage = "Category is required")]
    public int? CategoryId { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
