using BusinessObjects;

namespace IdentityAjaxClient.Model
{
    public class CartItemViewModel
    {
        public int OrchidId { get; set; }
        public int Quantity { get; set; }
        public Orchid Product { get; set; }
    }
}
